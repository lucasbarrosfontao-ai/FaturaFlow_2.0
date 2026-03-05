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
            
            await _service.SaveCustomerAsync(
                null,
                "Lucas Fontão",
                "123456789", 
                "912345678",
                "lucas@teste.com",
                "Rua A", "Braga", "4700-001"
            );

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task SaveCustomerAsync_Deve_Atualizar_Cliente_Quando_Id_Existe()
        {
            
            var customerId = Guid.NewGuid();
            var existingCustomer = new Customer(
                "Nome Antigo", new PersonalId("123456789"), null, null, null, null, null
            );

            _repoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(existingCustomer);

            await _service.SaveCustomerAsync(
                customerId,
                "Nome Novo",
                "123456789",
                "912345678",
                "lucas@teste.com",
                "Rua A", "Braga", "4700-001"
            );

             
            existingCustomer.Name.Should().Be("Nome Novo");
            _repoMock.Verify(r => r.UpdateAsync(existingCustomer), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveAsync_Deve_Retornar_Apenas_Clientes_Ativos()
        {
            
            var c1 = new Customer("Ativo", new PersonalId("123456789"), null, null, null, null, null);
            var c2 = new Customer("Inativo", new PersonalId("501306072"), null, null, null, null, null);
            c2.Deactivate();

            var lista = new List<Customer> { c1, c2 };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(lista);

            var result = await _service.GetAllActiveAsync();

             
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Ativo");
        }

        [Fact]
        public async Task SaveCustomerAsync_Deve_Lancar_Erro_Se_NIF_For_Invalido()
        {
            Func<Task> acao = async () => await _service.SaveCustomerAsync(
                null, "Teste", "1", "912345678", "a@b.com", "Rua", "City", "4700-001"
            );

             
            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao salvar cliente: *");
        }

        [Fact]
        public async Task ActivateAsync_Deve_Mudar_Estado_E_Gravar()
        {
            
            var customerId = Guid.NewGuid();
            var customer = new Customer("Teste", new PersonalId("123456789"), null, null, null, null, null);
            customer.Deactivate();

            _repoMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            await _service.ActivateAsync(customerId);

             
            customer.IsActive.Should().BeTrue();
            _repoMock.Verify(r => r.UpdateAsync(customer), Times.Once);
        }
    }
}