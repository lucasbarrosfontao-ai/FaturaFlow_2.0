using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.ValueObjects;
using System;

namespace FaturaFlow.Tests.Domain.ValueObjects
{
    public class PersonalIdTests
    {
        [Theory]
        [InlineData("123456789")]    
        [InlineData("501306072")]    
        [InlineData(" 256.241.210 ")] 
        public void Deve_Aceitar_NIF_Valido_E_Limpar_Formatacao(string nifInput)
        {
            // Act
            var nif = new PersonalId(nifInput);

            // Assert
            nif.Value.Length.Should().Be(9);
            nif.Value.Should().NotContain(".");
            nif.Value.Should().NotContain(" ");
            nif.Value.Should().NotContain("-");
        }

        [Fact]
        public void Deve_Lancar_Erro_Se_CheckDigit_For_Invalido()
        {
           
            Action acao = () => new PersonalId("123456780");

            acao.Should().Throw<Exception>()
                .WithMessage("Número de Identificação inválido (Erro no algoritmo de controlo).");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Deve_Lancar_Erro_Se_Estiver_Vazio(string nifVazio)
        {
            Action acao = () => new PersonalId(nifVazio);

            acao.Should().Throw<Exception>()
                .WithMessage("O Número de Identificação não pode estar vazio.");
        }
    }
}