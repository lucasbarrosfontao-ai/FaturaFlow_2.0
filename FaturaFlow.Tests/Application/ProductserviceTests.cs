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
            var supplierId = Guid.NewGuid();

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

            _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task SaveProductAsync_Deve_Atualizar_Produto_Existente()
        {
            var productId = Guid.NewGuid();
            var existingProduct = new Product(
                "Nome Antigo", "REF-0", "Un",
                new Price(10), new Price(20), new VatRate(23), 10, Guid.NewGuid()
            );

            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

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

            existingProduct.Name.Should().Be("Nome Novo");
            existingProduct.SalePrice.Value.Should().Be(30.00m);
            _productRepoMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        }

        [Fact]
        public async Task SaveProductAsync_Deve_Lancar_Excecao_Se_Dados_Invalidos()
        {

            Func<Task> acao = async () => await _service.SaveProductAsync(
                null, Guid.NewGuid(), "Erro", "REF", -10.00m, 20.00m, 23.0m, 1, "Un"
            );

            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao salvar produto: *");
        }

        [Fact]
        public async Task DeactivateAsync_Deve_Mudar_Status_E_Gravar_No_Repositorio()
        {
            var productId = Guid.NewGuid();
            var product = new Product("Teste", "R1", "Un", new Price(1), new Price(2), new VatRate(23), 5, Guid.NewGuid());

            _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            await _service.DeactivateAsync(productId);

            product.IsActive.Should().BeFalse();
            _productRepoMock.Verify(r => r.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_Deve_Filtrar_Apenas_Produtos_Ativos()
        {
            var p1 = new Product("Ativo", "R1", "U", new Price(1), new Price(2), new VatRate(23), 1, Guid.NewGuid());
            var p2 = new Product("Inativo", "R2", "U", new Price(1), new Price(2), new VatRate(23), 1, Guid.NewGuid());
            p2.Deactivate();

            var lista = new List<Product> { p1, p2 };
            _productRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lista);

            var result = await _service.GetAllActiveAsync();

            result.Should().HaveCount(1);
            result.All(p => p.IsActive).Should().BeTrue();
        }
    }
}