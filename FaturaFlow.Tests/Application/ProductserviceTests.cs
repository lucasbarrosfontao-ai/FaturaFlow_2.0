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

namespace FaturaFlow.Tests.Application
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _productRepoMock = new Mock<IProductRepository>();
            _service = new ProductService(_productRepoMock.Object);
        }

        [Fact]
        public async Task SaveProductAsync_Deve_Adicionar_Novo_Produto_Quando_Id_For_Nulo()
        {
            // --- ARRANGE ---
            var supplierId = Guid.NewGuid();

            // --- ACT ---
            await _service.SaveProductAsync(
                null,
                supplierId,
                "Teclado Mecânico",
                "KBD-001",
                40.00m, // Purchase
                80.00m, // Sale
                23.0m,  // VAT
                50,     // Stock
                "Un"
            );

            // --- ASSERT ---
            // Verifica se o método AddAsync foi chamado uma vez
            _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task SaveProductAsync_Deve_Atualizar_Produto_Existente()
        {
            // --- ARRANGE ---
            var productId = Guid.NewGuid();
            var existingProduct = new Product(
                "Nome Antigo", "REF-0", "Un",
                new Price(10), new Price(20), new VatRate(23), 10, Guid.NewGuid()
            );

            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // --- ACT ---
            await _service.SaveProductAsync(
                productId,
                existingProduct.SupplierId,
                "Nome Novo",
                "REF-0",
                15.00m,
                30.00m,
                23.0m,
                20,
                "Un"
            );

            // --- ASSERT ---
            existingProduct.Name.Should().Be("Nome Novo");
            existingProduct.SalePrice.Value.Should().Be(30.00m);
            _productRepoMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        }

        [Fact]
        public async Task SaveProductAsync_Deve_Lancar_Excecao_Se_Dados_Invalidos()
        {
            // --- ARRANGE ---
            // Preço negativo (-10) vai causar erro no Value Object Price

            // --- ACT ---
            Func<Task> acao = async () => await _service.SaveProductAsync(
                null, Guid.NewGuid(), "Erro", "REF", -10.00m, 20.00m, 23.0m, 1, "Un"
            );

            // --- ASSERT ---
            // O teu catch no Service relança como "Erro ao salvar produto: ..."
            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao salvar produto: *");
        }

        [Fact]
        public async Task DeactivateAsync_Deve_Mudar_Status_E_Gravar_No_Repositorio()
        {
            // --- ARRANGE ---
            var productId = Guid.NewGuid();
            var product = new Product("Teste", "R1", "Un", new Price(1), new Price(2), new VatRate(23), 5, Guid.NewGuid());

            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            // --- ACT ---
            await _service.DeactivateAsync(productId);

            // --- ASSERT ---
            product.IsActive.Should().BeFalse();
            _productRepoMock.Verify(r => r.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_Deve_Filtrar_Apenas_Produtos_Ativos()
        {
            // --- ARRANGE ---
            var p1 = new Product("Ativo", "R1", "U", new Price(1), new Price(2), new VatRate(23), 1, Guid.NewGuid());
            var p2 = new Product("Inativo", "R2", "U", new Price(1), new Price(2), new VatRate(23), 1, Guid.NewGuid());
            p2.Deactivate();

            var lista = new List<Product> { p1, p2 };
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lista);

            // --- ACT ---
            var result = await _service.GetAllActiveAsync();

            // --- ASSERT ---
            result.Should().HaveCount(1);
            result.All(p => p.IsActive).Should().BeTrue();
        }
    }
}