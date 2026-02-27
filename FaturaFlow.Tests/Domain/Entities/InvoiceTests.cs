using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;
using System.Linq;

namespace FaturaFlow.Tests.Domain.Entities
{
    public class InvoiceTests
    {
        // Teste de Cálculos Agregados
        [Fact]
        public void Deve_Somar_Totais_De_Todas_As_Linhas()
        {
            // Arrange
            var invoice = new Invoice(Guid.NewGuid(), "FAT/1");

            // Linha 1: 100€ + 23% IVA (23€)
            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));

            // Linha 2: 50€ + 6% IVA (3€)
            invoice.AddLine(Guid.NewGuid(), 1, new Price(50m), new VatRate(6m));

            // Assert
            invoice.TotalNet.Should().Be(150.00m);     // 100 + 50
            invoice.TotalVat.Should().Be(26.00m);      // 23 + 3
            invoice.TotalPayable.Should().Be(176.00m); // 150 + 26
        }

        // Teste de Regra: Não emitir sem itens
        [Fact]
        public void Nao_Deve_Permitir_Emitir_Fatura_Sem_Linhas()
        {
            // Arrange
            var invoice = new Invoice(Guid.NewGuid(), "FAT/2");

            // Act
            Action acao = () => invoice.Issue();

            // Assert
            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Não é possível emitir uma fatura sem itens.");
        }

        // Teste de Fluxo: Rascunho -> Emitida -> Paga
        [Fact]
        public void Deve_Seguir_Fluxo_De_Estados_Corretamente()
        {
            // Arrange
            var invoice = new Invoice(Guid.NewGuid(), "FAT/3");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(10m), new VatRate(23m));

            // 1. Verificar estado inicial
            invoice.Status.Should().Be(Invoice.StatusDraft);

            // 2. Emitir
            invoice.Issue();
            invoice.Status.Should().Be(Invoice.StatusIssued);

            // 3. Pagar
            invoice.MarkAsPaid();
            invoice.Status.Should().Be(Invoice.StatusPaid);
        }

        // Teste de Segurança: Bloquear edição após emissão
        [Fact]
        public void Nao_Deve_Permitir_Adicionar_Linhas_A_Uma_Fatura_Emitida()
        {
            // Arrange
            var invoice = new Invoice(Guid.NewGuid(), "FAT/4");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(10m), new VatRate(23m));
            invoice.Issue();

            // Act
            Action acao = () => invoice.AddLine(Guid.NewGuid(), 1, new Price(20m), new VatRate(23m));

            // Assert
            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Não é possível adicionar itens a uma fatura já emitida.");
        }

        [Fact]
        public void Deve_Limpar_Linhas_E_Zerar_Totais()
        {
            // Arrange
            var invoice = new Invoice(Guid.NewGuid(), "FAT/5");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));

            // Act
            invoice.ClearLines();

            // Assert
            invoice.Lines.Should().BeEmpty();
            invoice.TotalNet.Should().Be(0);
            invoice.TotalPayable.Should().Be(0);
        }
    }
}