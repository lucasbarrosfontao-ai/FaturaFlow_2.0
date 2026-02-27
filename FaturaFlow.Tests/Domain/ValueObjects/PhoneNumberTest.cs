using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Tests.Domain.ValueObjects
{
    public class PhoneNumberTests
    {
        [Theory]
        [InlineData("+351 912 345 678")]
        [InlineData("912345678")]
        [InlineData("(22) 123 4567")]
        public void Deve_Aceitar_Telefone_Valido_E_Limpar_Caracteres(string foneInput)
        {
            var fone = new PhoneNumber(foneInput);
            fone.Value.Should().NotContain(" ");
            fone.Value.Should().NotContain("(");
        }

        [Fact]
        public void Deve_Lancar_Erro_Para_Telefone_Muito_Curto()
        {
            Action acao = () => new PhoneNumber("12345");
            acao.Should().Throw<Exception>().WithMessage("*7 a 15 dígitos*");
        }
    }
}