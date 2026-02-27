using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Tests.Domain.ValueObjects
{
    public class EmailAddressTests
    {
        [Theory]
        [InlineData("lucas@exemplo.com")]
        [InlineData("TESTE@DOMINIO.PT")]
        [InlineData("  espacos@site.com  ")]
        public void Deve_Aceitar_Email_Valido(string emailInput)
        {
            var email = new EmailAddress(emailInput);
            email.Value.Should().Be(emailInput.Trim().ToLower());
        }

        [Fact]
        public void Deve_Aceitar_Nulo_Ou_Vazio()
        {
            var email = new EmailAddress(null);
            email.Value.Should().BeNull();
        }

        [Theory]
        [InlineData("email-sem-arroba")]
        [InlineData("email@sem-ponto")]
        [InlineData("@sem-usuario.com")]
        public void Deve_Lancar_Erro_Para_Email_Invalido(string emailInvalido)
        {
            Action acao = () => new EmailAddress(emailInvalido);
            acao.Should().Throw<Exception>().WithMessage("O formato do e-mail é inválido.");
        }
    }
}
