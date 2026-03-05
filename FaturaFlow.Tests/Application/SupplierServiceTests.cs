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
    public class SupplierServiceTests
    {
        private readonly Mock<ISupplierRepository> _repoMock;
        private readonly SupplierService _service;

        public SupplierServiceTests()
        {
            _repoMock = new Mock<ISupplierRepository>();
            _service = new SupplierService(_repoMock.Object);
        }

        [Fact]
        public async Task SaveSupplierAsync_Deve_Criar_Novo_Fornecedor_Quando_Id_Eh_Nulo()
        {
            string nifValido = "501306072";

            await _service.SaveSupplierAsync(
                null,
                "Fornecedor Teste",
                nifValido,
                "Representante A",
                "210000000",
                "fornecedor@teste.com",
                "Rua X", "Lisboa", "1000-001"
            );

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Supplier>()), Times.Once);
        }

        [Fact]
        public async Task SaveSupplierAsync_Deve_Atualizar_Fornecedor_Existente()
        {
            var supplierId = Guid.NewGuid();
            var existing = new Supplier(
                "Antigo", new PersonalId("501306072"), "Rep",
                new PhoneNumber("210000000"), new EmailAddress("a@a.com"),
                "Rua", "City", new PostalCode("1000-001")
            );

            _repoMock.Setup(r => r.GetByIdAsync(supplierId)).ReturnsAsync(existing);

            await _service.SaveSupplierAsync(
                supplierId,
                "Novo Nome",
                "501306072",
                "Novo Rep",
                "210000000",
                "a@a.com", "Rua", "City", "1000-001"
            );

            existing.CompanyName.Should().Be("Novo Nome");
            existing.RepresentativeName.Should().Be("Novo Rep");
            _repoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeactivateAsync_Deve_Mudar_Estado_E_Gravar()
        {
            var id = Guid.NewGuid();
            var supplier = new Supplier("Teste", new PersonalId("501306072"), "Rep", null, null, null, null, null);
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(supplier);

            await _service.DeactivateAsync(id);

            supplier.IsActive.Should().BeFalse();
            _repoMock.Verify(r => r.UpdateAsync(supplier), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_Deve_Filtrar_Apenas_Ativos()
        {
            var s1 = new Supplier("Ativo", new PersonalId("501306072"), "R1", null, null, null, null, null);
            var s2 = new Supplier("Inativo", new PersonalId("123456789"), "R2", null, null, null, null, null);
            s2.Deactivate();

            var lista = new List<Supplier> { s1, s2 };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lista);

            var result = await _service.GetAllActiveAsync();

            result.Should().HaveCount(1);
            result.First().CompanyName.Should().Be("Ativo");
        }

        [Fact]
        public async Task SaveSupplierAsync_Deve_Lancar_Excecao_Se_Dados_Invalidos()
        {
            Func<Task> acao = async () => await _service.SaveSupplierAsync(
                null, "Teste", "501306072", "Rep", "210000000", "email-errado", "Rua", "City", "1000-001"
            );

            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao salvar fornecedor: *");
        }
    }
}