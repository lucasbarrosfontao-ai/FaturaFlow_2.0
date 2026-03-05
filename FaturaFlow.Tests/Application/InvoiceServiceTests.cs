using Xunit;
using Moq;
using FluentAssertions;
using FaturaFlow.Application.Services;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaturaFlow.Tests.Application
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IMessageService> _messageServiceMock;
        private readonly InvoiceService _service;

        public InvoiceServiceTests()
        {
            _invoiceRepoMock = new Mock<IInvoiceRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _messageServiceMock = new Mock<IMessageService>();

            _service = new InvoiceService(
                _invoiceRepoMock.Object,
                _customerRepoMock.Object,
                _productRepoMock.Object,
                _messageServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateDraftInvoiceAsync_Deve_Gravar_Fatura_Quando_Dados_Sao_Validos()
        {
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var customer = new Customer("Lucas", new PersonalId("123456789"), null, null, null, null, null);
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            var product = new Product("PC", "REF1", "Un", new Price(500), new Price(1000), new VatRate(23), 10, Guid.NewGuid());
            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            var items = new List<(Guid, int)> { (productId, 2) };

            var resultId = await _service.CreateDraftInvoiceAsync(customerId, "FAT-001",DateTime.Now, items); 

            resultId.Should().NotBeEmpty();

            _invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
        }

        [Fact]
        public async Task EmitInvoiceAsync_Deve_Baixar_Stock_E_Enviar_Mensagem()
        {
            var invoiceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var invoice = new Invoice(customerId, "FAT-001", Invoice.StatusDraft);
            invoice.AddLine(productId, 5, new Price(100), new VatRate(23));

            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);

            var product = new Product("Mouse", "M1", "Un", new Price(10), new Price(20), new VatRate(23), 10, Guid.NewGuid());
            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            var customer = new Customer("Lucas", new PersonalId("123456789"), null, new EmailAddress("lucas@teste.com"), null, null, null);
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            await _service.EmitInvoiceAsync(invoiceId);


            product.StockQuantity.Should().Be(5);

            _productRepoMock.Verify(r => r.UpdateAsync(product), Times.Once);

            invoice.Status.Should().Be(Invoice.StatusIssued);

            _messageServiceMock.Verify(m => m.SendInvoiceMessageAsync(
                invoice.Id, customer.Name, customer.Email.Value), Times.Once);
        }

        [Fact]
        public async Task EmitInvoiceAsync_Deve_Lancar_Erro_Se_Fatura_Ja_Emitida()
        {
            var invoiceId = Guid.NewGuid();
            var invoice = new Invoice(Guid.NewGuid(), "FAT-EXISTENTE", Invoice.StatusIssued);
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);

            Func<Task> acao = async () => await _service.EmitInvoiceAsync(invoiceId);

            await acao.Should().ThrowAsync<Exception>().WithMessage("Esta fatura já foi emitida.");
            _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }
    }
}