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

        // Helper para criar faturas com datas retroativas (usando Reflection para contornar o private set)
        private Invoice CriarFaturaComData(DateTime data, decimal valor)
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT-" + Guid.NewGuid().ToString().Substring(0, 5));
            invoice.AddLine(Guid.NewGuid(), 1, new Price(valor), new VatRate(0));

            // Truque de Reflection para alterar a IssueDate que é private set
            var property = typeof(Invoice).GetProperty("IssueDate");
            property.SetValue(invoice, data);

            return invoice;
        }

        [Fact]
        public async Task GetSalesAnalyticsAsync_Deve_Agrupar_Vendas_Por_Periodos_Corretamente()
        {
            // --- ARRANGE ---
            var agora = DateTime.Now;

            var faturas = new List<Invoice>
            {
                // 1. Fatura de há 2 horas (deve aparecer nas 24h, na Semana e no Ano)
                CriarFaturaComData(agora.AddHours(-2), 100m),

                // 2. Fatura de há 3 dias (deve aparecer na Semana e no Ano, mas NÃO nas 24h)
                CriarFaturaComData(agora.AddDays(-3), 200m),

                // 3. Fatura de há 2 meses (deve aparecer apenas no Ano)
                CriarFaturaComData(agora.AddMonths(-2), 300m),
                
                // 4. Fatura de há 2 anos (NÃO deve aparecer em lado nenhum - limite é 1 ano)
                CriarFaturaComData(agora.AddYears(-2), 500m)
            };

            _invoiceRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(faturas);

            // --- ACT ---
            var analytics = await _service.GetSalesAnalyticsAsync();

            // --- ASSERT ---

            // Verificação das Últimas 24h
            analytics.Last24Hours.Should().HaveCount(1);
            analytics.Last24Hours.Sum(x => x.Value).Should().Be(100m);

            // Verificação dos Últimos 7 Dias (Hoje + 3 dias atrás)
            analytics.Last7Days.Should().HaveCount(2);
            analytics.Last7Days.Sum(x => x.Value).Should().Be(300m); // 100 + 200

            // Verificação dos Últimos 12 Meses (Hoje + 3 dias + 2 meses)
            // Nota: Podem ser 2 ou 3 pontos dependendo se os dias caem no mesmo mês
            analytics.Last12Months.Sum(x => x.Value).Should().Be(600m); // 100 + 200 + 300
        }

        [Fact]
        public async Task GetSalesAnalyticsAsync_Deve_Retornar_Listas_Vazias_Se_Nao_Houver_Vendas()
        {
            // --- ARRANGE ---
            _invoiceRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Invoice>());

            // --- ACT ---
            var analytics = await _service.GetSalesAnalyticsAsync();

            // --- ASSERT ---
            analytics.Last24Hours.Should().BeEmpty();
            analytics.Last7Days.Should().BeEmpty();
            analytics.Last12Months.Should().BeEmpty();
        }
    }
}