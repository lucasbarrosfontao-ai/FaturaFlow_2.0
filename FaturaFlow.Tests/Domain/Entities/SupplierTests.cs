using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;

namespace FaturaFlow.Tests.Domain.Entities
{
    public class SupplierTests
    {
        private Supplier CriarFornecedorPadrao()
        {
            return new Supplier(
                "Software House Lda",
                new PersonalId("501306072"),
                "Carlos Representante",
                new PhoneNumber("210000000"),
                new EmailAddress("contacto@software.com"),
                "Avenida Central",
                "Braga",
                new PostalCode("4700-001")
            );
        }

        [Fact]
        public void Deve_Criar_Fornecedor_Com_Sucesso()
        {
            var supplier = CriarFornecedorPadrao();

            supplier.Id.Should().NotBeEmpty();
            supplier.CompanyName.Should().Be("Software House Lda");
            supplier.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Nao_Deve_Permitir_Fornecedor_Sem_Nome()
        {
            Action acao = () => new Supplier(
                "",
                new PersonalId("501306072"), null, null, null, null, null, null
            );

            acao.Should().Throw<Exception>()
                .WithMessage("O nome do cliente é obrigatório.");
        }

        [Fact]
        public void Deve_Atualizar_Dados_Do_Fornecedor()
        {
            var supplier = CriarFornecedorPadrao();
            var novoRepresentante = "João Silva";

            supplier.UpdateDetails(
                supplier.CompanyName,
                supplier.NIPC,
                novoRepresentante,
                supplier.Phone,
                supplier.Email,
                supplier.Address,
                supplier.City,
                supplier.ZipCode
            );

            supplier.RepresentativeName.Should().Be("João Silva");
        }

        [Fact]
        public void Deve_Alterar_Estado_Ativo_Inativo()
        {
            var supplier = CriarFornecedorPadrao();

            supplier.Deactivate();
            supplier.IsActive.Should().BeFalse();

            supplier.Activate();
            supplier.IsActive.Should().BeTrue();
        }
    }
}