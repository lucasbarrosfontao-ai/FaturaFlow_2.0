using Xunit;
using FluentAssertions;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;

namespace FaturaFlow.Tests.Domain.Entities
{
    public class UserTests
    {
        [Fact]
        public void Deve_Criar_Utilizador_Com_Sucesso()
        {
            var username = "lucas.admin";
            var passwordHash = "AQAAAAEAACcQAAAAE..."; 
            var email = new EmailAddress("admin@faturaflow.com");

            var user = new User(username, passwordHash, email);

            user.Id.Should().NotBeEmpty();
            user.Username.Should().Be(username);
            user.Email.Value.Should().Be("admin@faturaflow.com");
        }

        [Theory]
        [InlineData("", "senha123", "O nome de utilizador é obrigatório.")]
        [InlineData("lucas", "", "A palavra-passe é obrigatória.")]
        public void Nao_Deve_Criar_User_Se_Dados_Obrigatorios_Faltarem(string user, string pass, string erroEsperado)
        {
            Action acao = () => new User(user, pass);

            acao.Should().Throw<Exception>().WithMessage(erroEsperado);
        }

        [Fact]
        public void Deve_Atualizar_Password_Com_Sucesso()
        {
            var user = new User("lucas", "senha-velha");
            var novaSenha = "senha-nova-hash";

            user.UpdatePassword(novaSenha);

            user.Password.Should().Be(novaSenha);
        }

        [Fact]
        public void Deve_Gerir_Codigo_De_Recuperacao()
        {
            var user = new User("lucas", "senha");

            user.SetRecoveryCode("123456");
            user.RecoveryCode.Should().Be("123456");

            user.ClearRecoveryCode();
            user.RecoveryCode.Should().BeNull();
        }

        [Fact]
        public void Nao_Deve_Permitir_Update_Com_Username_Vazio()
        {
            var user = new User("lucas", "senha");

            Action acao = () => user.UpdateDetails("", null);

            acao.Should().Throw<Exception>()
                .WithMessage("O nome de utilizador não pode ser vazio.");
        }
    }
}