using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Tests.Domain.ValueObjects
{
    public class PostalCodeTests
    {
        [Theory]
        [InlineData("4700-123")]
        [InlineData("1000-001")]
        public void Deve_Aceitar_Codigo_Postal_Valido(string cpInput)
        {
            var cp = new PostalCode(cpInput);
            cp.Value.Should().Be(cpInput);
        }

        [Theory]
        [InlineData("4700123")] // Sem hifen
        [InlineData("470-123")]  // Falta digito
        [InlineData("AAAA-BBB")] // Letras
        public void Deve_Lancar_Erro_Para_Formato_Invalido(string cpErrado)
        {
            Action acao = () => new PostalCode(cpErrado);
            acao.Should().Throw<Exception>().WithMessage("*XXXX-XXX*");
        }
    }
}