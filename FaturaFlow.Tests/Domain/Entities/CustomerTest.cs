using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;

namespace FaturaFlow.Tests.Domain.Entities
{
    public class CustomerTests
    {
        [Fact]
        public void Deve_Criar_Cliente_Com_Dados_Validos()
        {
            var name = "Lucas Fontão";
            var nif = new PersonalId("123456789");
            var phone = new PhoneNumber("912345678");
            var email = new EmailAddress("lucas@exemplo.com");
            var address = "Rua do ISEP, 123";
            var city = "Porto";
            var zipCode = new PostalCode("4000-001");

            var customer = new Customer(name, nif, phone, email, address, city, zipCode);

            customer.Id.Should().NotBeEmpty();
            customer.Name.Should().Be(name);
            customer.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deve_Lancar_Erro_Se_Nome_For_Vazio()
        {
            var nif = new PersonalId("123456789");

            Action acao = () => new Customer(
                "", 
                nif,
                new PhoneNumber("912345678"),
                new EmailAddress("a@b.com"),
                "Rua", "Cidade", new PostalCode("1111-111")
            );

            acao.Should().Throw<Exception>().WithMessage("O nome do cliente é obrigatório.");
        }

        [Fact]
        public void Deve_Poder_Desativar_Cliente()
        {
            var customer = new Customer("Teste", new PersonalId("123456789"), null, null, null, null, null);

            customer.Deactivate();

            customer.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Deve_Atualizar_Detalhes_Com_Sucesso()
        {
            var customer = new Customer("Nome Velho", new PersonalId("123456789"), null, null, null, null, null);
            var novoNome = "Nome Novo";

            customer.UpdateDetails(
                novoNome,
                customer.NIF,
                customer.Phone,
                customer.Email,
                customer.Address,
                customer.City,
                customer.ZipCode
            );

            customer.Name.Should().Be(novoNome);
        }
    }
}