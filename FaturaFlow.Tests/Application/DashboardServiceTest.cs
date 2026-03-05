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
using FaturaFlow.Tests.Domain.Entities;

namespace FaturaFlow.Tests.Application
{
    public class DashboardServiceTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly DashboardService _service;

        public DashboardServiceTests()
        {
            _invoiceRepoMock = new Mock<IInvoiceRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _productRepoMock = new Mock<IProductRepository>();

            _service = new DashboardService(
                _invoiceRepoMock.Object,
                _customerRepoMock.Object,
                _productRepoMock.Object
            );
        }

        [Fact]
        public async Task GetStatsAsync_Deve_Calcular_Estatisticas_Corretamente()
        {
            var customers = new List<Customer> {
                new Customer("C1", new PersonalId("123456789"), null, null, null, null, null),
                new Customer("C2", new PersonalId("501306072"), null, null, null, null, null)
            };
            _customerRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            var inv1 = new Invoice(Guid.NewGuid(), "FAT1");
            inv1.AddLine(Guid.NewGuid(), 1, new Price(100), new VatRate(0));

            var inv2 = new Invoice(Guid.NewGuid(), "FAT2");
            inv2.AddLine(Guid.NewGuid(), 1, new Price(50), new VatRate(0));  

            _invoiceRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Invoice> { inv1, inv2 });

            var p1 = new Product("P1", "R1", "Un", new Price(10), new Price(20), new VatRate(23), 2, Guid.NewGuid());
            var p2 = new Product("P2", "R2", "Un", new Price(10), new Price(20), new VatRate(23), 10, Guid.NewGuid()); 

            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> { p1, p2 });

            var stats = await _service.GetStatsAsync();

            stats.TotalCustomers.Should().Be(2);
            stats.TotalInvoices.Should().Be(2);
            stats.TotalInvoicedAmount.Should().Be(150.00m);
            stats.LowStockProducts.Should().Be(1); 
        }
    }
}