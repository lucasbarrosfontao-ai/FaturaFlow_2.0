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
        [Fact]
        public void Deve_Somar_Totais_De_Todas_As_Linhas()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/1");

            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));

            invoice.AddLine(Guid.NewGuid(), 1, new Price(50m), new VatRate(6m));


            invoice.TotalNet.Should().Be(150.00m);     
            invoice.TotalVat.Should().Be(26.00m);     
            invoice.TotalPayable.Should().Be(176.00m); 
        }

        [Fact]
        public void Nao_Deve_Permitir_Emitir_Fatura_Sem_Linhas()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/2");

            Action acao = () => invoice.Issue();

            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Não é possível emitir uma fatura sem itens.");
        }
        [Fact]
        public void Deve_Seguir_Fluxo_De_Estados_Corretamente()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/3");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(10m), new VatRate(23m));

            invoice.Status.Should().Be(Invoice.StatusDraft);

            invoice.Issue();
            invoice.Status.Should().Be(Invoice.StatusIssued);

            invoice.MarkAsPaid();
            invoice.Status.Should().Be(Invoice.StatusPaid);
        }

        [Fact]
        public void Nao_Deve_Permitir_Adicionar_Linhas_A_Uma_Fatura_Emitida()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/4");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(10m), new VatRate(23m));
            invoice.Issue();

            Action acao = () => invoice.AddLine(Guid.NewGuid(), 1, new Price(20m), new VatRate(23m));

            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Não é possível adicionar itens a uma fatura já emitida.");
        }

        [Fact]
        public void Deve_Limpar_Linhas_E_Zerar_Totais()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/5");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));

            invoice.ClearLines();

            invoice.Lines.Should().BeEmpty();
            invoice.TotalNet.Should().Be(0);
            invoice.TotalPayable.Should().Be(0);
        }
        [Fact]
        public void Nao_Deve_Permitir_Limpar_Linhas_De_Uma_Fatura_Emitida()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/6");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));
            invoice.Issue();

            Action acao = () => invoice.ClearLines();

            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Não é possível alterar itens de uma fatura já emitida.");
        }
        [Fact]
        public void Nao_Deve_Permitir_Marcar_Como_Paga_Uma_Fatura_Nao_Emitida()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/7");

            Action acao = () => invoice.MarkAsPaid();

            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Apenas faturas emitidas podem ser marcadas como pagas.");
        }
        [Fact]
        public void Nao_Deve_Permitir_Cancelar_Uma_Fatura_Ja_Cancelada()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/8");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));
            invoice.Issue();
            invoice.Cancel();

            Action acao = () => invoice.Cancel();

            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("A fatura já está cancelada.");
        }
        [Fact]
        public void Nao_Deve_Permitir_Cancelar_Uma_Fatura_Paga()
        {
            var invoice = new Invoice(Guid.NewGuid(), "FAT/9");
            invoice.AddLine(Guid.NewGuid(), 1, new Price(100m), new VatRate(23m));
            invoice.Issue();
            invoice.MarkAsPaid();

            Action acao = () => invoice.Cancel();

            acao.Should().Throw<InvalidOperationException>()
                .WithMessage("Não é possível cancelar uma fatura paga.");
        }
    }
}