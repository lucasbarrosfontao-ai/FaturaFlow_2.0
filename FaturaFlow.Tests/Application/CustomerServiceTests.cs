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
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _repoMock;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _repoMock = new Mock<ICustomerRepository>();
            _service = new CustomerService(_repoMock.Object);
        }

        [Fact]
        public async Task SaveCustomerAsync_Deve_Criar_Novo_Cliente_Quando_Id_For_Nulo()
        {
            // --- ARRANGE ---
            // Não precisamos configurar o GetById porque o ID será nulo

            // --- ACT ---
            await _service.SaveCustomerAsync(
                null,
                "Lucas Fontão",
                "123456789", // NIF válido
                "912345678",
                "lucas@teste.com",
                "Rua A", "Braga", "4700-001"
            );

            // --- ASSERT ---
            // Verificamos se o método AddAsync foi chamado (criação)
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task SaveCustomerAsync_Deve_Atualizar_Cliente_Quando_Id_Existe()
        {
            // --- ARRANGE ---
            var customerId = Guid.NewGuid();
            var existingCustomer = new Customer(
                "Nome Antigo", new PersonalId("123456789"), null, null, null, null, null
            );

            _repoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(existingCustomer);

            // --- ACT ---
            await _service.SaveCustomerAsync(
                customerId,
                "Nome Novo",
                "123456789",
                "912345678",
                "lucas@teste.com",
                "Rua A", "Braga", "4700-001"
            );

            // --- ASSERT ---
            existingCustomer.Name.Should().Be("Nome Novo");
            _repoMock.Verify(r => r.UpdateAsync(existingCustomer), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_Deve_Retornar_Apenas_Clientes_Ativos()
        {
            // --- ARRANGE ---
            var c1 = new Customer("Ativo", new PersonalId("123456789"), null, null, null, null, null);
            var c2 = new Customer("Inativo", new PersonalId("501306072"), null, null, null, null, null);
            c2.Deactivate();

            var lista = new List<Customer> { c1, c2 };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lista);

            // --- ACT ---
            var result = await _service.GetAllActiveAsync();

            // --- ASSERT ---
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Ativo");
        }

        [Fact]
        public async Task SaveCustomerAsync_Deve_Lancar_Erro_Se_NIF_For_Invalido()
        {
            // --- ACT ---
            // NIF "1" é inválido, vai disparar a Exception do Value Object PersonalId
            Func<Task> acao = async () => await _service.SaveCustomerAsync(
                null, "Teste", "1", "912345678", "a@b.com", "Rua", "City", "4700-001"
            );

            // --- ASSERT ---
            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao salvar cliente: *");
        }

        [Fact]
        public async Task ActivateAsync_Deve_Mudar_Estado_E_Gravar()
        {
            // --- ARRANGE ---
            var customerId = Guid.NewGuid();
            var customer = new Customer("Teste", new PersonalId("123456789"), null, null, null, null, null);
            customer.Deactivate(); // Começa inativo

            _repoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            // --- ACT ---
            await _service.ActivateAsync(customerId);

            // --- ASSERT ---
            customer.IsActive.Should().BeTrue();
            _repoMock.Verify(r => r.UpdateAsync(customer), Times.Once);
        }
    }
}