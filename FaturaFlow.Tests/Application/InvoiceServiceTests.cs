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
            // Inicializamos os Mocks
            _invoiceRepoMock = new Mock<IInvoiceRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _messageServiceMock = new Mock<IMessageService>();

            // Injetamos os Mocks no serviço real
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
            // --- ARRANGE ---
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            // Simular que o cliente existe
            var customer = new Customer("Lucas", new PersonalId("123456789"), null, null, null, null, null);
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            // Simular que o produto existe
            var product = new Product("PC", "REF1", "Un", new Price(500), new Price(1000), new VatRate(23), 10, Guid.NewGuid());
            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            var items = new List<(Guid, int)> { (productId, 2) };

            // --- ACT ---
            var resultId = await _service.CreateDraftInvoiceAsync(customerId, "FAT-001", items);

            // --- ASSERT ---
            resultId.Should().NotBeEmpty();

            // Verificamos se o repositório de faturas recebeu o comando AddAsync
            _invoiceRepoMock.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
        }

        [Fact]
        public async Task EmitInvoiceAsync_Deve_Baixar_Stock_E_Enviar_Mensagem()
        {
            // --- ARRANGE ---
            var invoiceId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            // 1. Criar uma fatura fake em estado de Rascunho com uma linha
            var invoice = new Invoice(customerId, "FAT-001", Invoice.StatusDraft);
            invoice.AddLine(productId, 5, new Price(100), new VatRate(23));

            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);

            // 2. Criar o produto fake com stock inicial de 10
            var product = new Product("Mouse", "M1", "Un", new Price(10), new Price(20), new VatRate(23), 10, Guid.NewGuid());
            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            // 3. Criar o cliente para a notificação
            var customer = new Customer("Lucas", new PersonalId("123456789"), null, new EmailAddress("lucas@teste.com"), null, null, null);
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            // --- ACT ---
            await _service.EmitInvoiceAsync(invoiceId);

            // --- ASSERT ---

            // A. O stock do produto deve ter baixado (era 10, comprou 5, fica 5)
            product.StockQuantity.Should().Be(5);

            // B. O repositório de produtos deve ter sido atualizado
            _productRepoMock.Verify(r => r.UpdateAsync(product), Times.Once);

            // C. O status da fatura deve ser "Emitida"
            invoice.Status.Should().Be(Invoice.StatusIssued);

            // D. O serviço de mensagens (RabbitMQ) deve ter sido chamado
            _messageServiceMock.Verify(m => m.SendInvoiceMessageAsync(
                invoice.Id, customer.Name, customer.Email.Value), Times.Once);
        }

        [Fact]
        public async Task EmitInvoiceAsync_Deve_Lancar_Erro_Se_Fatura_Ja_Emitida()
        {
            // Arrange
            var invoiceId = Guid.NewGuid();
            var invoice = new Invoice(Guid.NewGuid(), "FAT-EXISTENTE", Invoice.StatusIssued);
            _invoiceRepoMock.Setup(r => r.GetByIdAsync(invoiceId)).ReturnsAsync(invoice);

            // Act
            Func<Task> acao = async () => await _service.EmitInvoiceAsync(invoiceId);

            // Assert
            await acao.Should().ThrowAsync<Exception>().WithMessage("Esta fatura já foi emitida.");
            _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }
    }
}