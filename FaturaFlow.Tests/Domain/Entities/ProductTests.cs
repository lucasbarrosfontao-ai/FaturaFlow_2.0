using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;

namespace FaturaFlow.Tests.Domain.Entities
{
    public class ProductTests
    {
        private Product CriarProdutoPadrao(int stockInicial = 10)
        {
            return new Product(
                "Portátil Gaming",
                "REF-001",
                "Un",
                new Price(800.00m),  
                new Price(1200.00m), 
                new VatRate(23.0m),
                stockInicial,
                Guid.NewGuid()
            );
        }

        [Fact]
        public void Deve_Criar_Produto_Com_Sucesso()
        {
            var produto = CriarProdutoPadrao();

            produto.Id.Should().NotBeEmpty();
            produto.IsActive.Should().BeTrue();
            produto.StockQuantity.Should().Be(10);
        }

        [Theory]
        [InlineData("", "REF-1")]
        [InlineData("Nome", "")]
        public void Nao_Deve_Criar_Produto_Sem_Nome_Ou_Referencia(string nome, string refInput)
        {
            Action acao = () => new Product(
                nome, refInput, "Un",
                new Price(10), new Price(20),
                new VatRate(23), 0, Guid.NewGuid()
            );

            acao.Should().Throw<Exception>();
        }

        [Fact]
        public void Deve_Adicionar_Stock_Corretamente()
        {
            var produto = CriarProdutoPadrao(stockInicial: 10);

            produto.AddStock(5);

            produto.StockQuantity.Should().Be(15);
        }

        [Fact]
        public void Deve_Remover_Stock_E_Permitir_Valor_Negativo()
        {
            var produto = CriarProdutoPadrao(stockInicial: 5);

            produto.RemoveStock(10); 

            produto.StockQuantity.Should().Be(-5);
        }

        [Fact]
        public void Deve_Atualizar_Precos_Com_Sucesso()
        {
            var produto = CriarProdutoPadrao();
            var novoPrecoCompra = new Price(900.00m);
            var novoPrecoVenda = new Price(1350.00m);

            produto.UpdatePrices(novoPrecoCompra, novoPrecoVenda);

            produto.PurchasePrice.Value.Should().Be(900.00m);
            produto.SalePrice.Value.Should().Be(1350.00m);
        }

        [Fact]
        public void Deve_Desativar_Produto()
        {
            var produto = CriarProdutoPadrao();

            produto.Deactivate();

            produto.IsActive.Should().BeFalse();
        }
    }
}