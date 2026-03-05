using Xunit;
using Moq;
using FluentAssertions;
using FaturaFlow.Application.Services;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace FaturaFlow.Tests.Application
{
    public class SalesServiceTests
    {
        private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
        private readonly SalesService _service;

        public SalesServiceTests()
        {
            _invoiceRepoMock = new Mock<IInvoiceRepository>();
            _service = new SalesService(_invoiceRepoMock.Object);
        }

        private Invoice CriarFaturaComData(DateTime data, decimal valor)
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT-" + Guid.NewGuid().ToString().Substring(0, 5));
            invoice.AddLine(Guid.NewGuid(), 1, new Price(valor), new VatRate(0));

            var property = typeof(Invoice).GetProperty("IssueDate");
            property.SetValue(invoice, data);

            return invoice;
        }

        [Fact]
        public async Task GetSalesAnalyticsAsync_Deve_Agrupar_Vendas_Por_Periodos_Corretamente()
        {
            var agora = DateTime.Now;

            var faturas = new List<Invoice>
            {
                CriarFaturaComData(agora.AddHours(-2), 100m),

                CriarFaturaComData(agora.AddDays(-3), 200m),

                CriarFaturaComData(agora.AddMonths(-2), 300m),
                
                CriarFaturaComData(agora.AddYears(-2), 500m)
            };

            _invoiceRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(faturas);

            var analytics = await _service.GetSalesAnalyticsAsync();


            analytics.Last24Hours.Should().HaveCount(1);
            analytics.Last24Hours.Sum(x => x.Value).Should().Be(100m);

            analytics.Last7Days.Should().HaveCount(2);
            analytics.Last7Days.Sum(x => x.Value).Should().Be(300m); 

            analytics.Last12Months.Sum(x => x.Value).Should().Be(600m); 
        }

        [Fact]
        public async Task GetSalesAnalyticsAsync_Deve_Retornar_Listas_Vazias_Se_Nao_Houver_Vendas()
        {
            _invoiceRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Invoice>());

            var analytics = await _service.GetSalesAnalyticsAsync();

            analytics.Last24Hours.Should().BeEmpty();
            analytics.Last7Days.Should().BeEmpty();
            analytics.Last12Months.Should().BeEmpty();
        }
    }
}