using Xunit;
using Moq;
using FluentAssertions;
using FaturaFlow.Application.Services;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using System;
using System.Threading.Tasks;

namespace FaturaFlow.Tests.Application
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _hasherMock;
        private readonly Mock<IMessageService> _messageServiceMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _hasherMock = new Mock<IPasswordHasher>();
            _messageServiceMock = new Mock<IMessageService>();

            _service = new AuthService(
                _userRepoMock.Object,
                _hasherMock.Object,
                _messageServiceMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_Deve_Retornar_Utilizador_Quando_Senha_Esta_Correta()
        {
            var user = new User("lucas", "hash_antigo");
            _userRepoMock.Setup(r => r.GetByUsernameAsync("lucas")).ReturnsAsync(user);

            _hasherMock.Setup(h => h.VerifyPassword("123", "hash_antigo")).Returns(true);

            var result = await _service.LoginAsync("lucas", "123");

            result.Should().NotBeNull();
            result.Username.Should().Be("lucas");
        }

        [Fact]
        public async Task RegisterUserAsync_Deve_Lancar_Erro_Se_Utilizador_Ja_Existir()
        {
            var existingUser = new User("lucas", "hash");
            _userRepoMock.Setup(r => r.GetByUsernameAsync("lucas")).ReturnsAsync(existingUser);

            Func<Task> acao = async () => await _service.RegisterUserAsync("lucas", "123", "lucas@teste.com");

            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Este nome de utilizador já está em uso.");

            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_Deve_Gerar_Codigo_E_Enviar_Email()
        {
            var emailStr = "teste@email.com";
            var user = new User("lucas", "hash", new EmailAddress(emailStr));

            _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<EmailAddress>())).ReturnsAsync(user);

            await _service.RequestPasswordResetAsync(emailStr);

            user.RecoveryCode.Should().NotBeNull();
            user.RecoveryCode.Length.Should().Be(6); 

            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);

            _messageServiceMock.Verify(m => m.SendPasswordRecoveryAsync(emailStr, user.RecoveryCode), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_Deve_Lancar_Erro_Se_Senha_Atual_Incorreta()
        {
            var userId = Guid.NewGuid();
            var user = new User("lucas", "hash_real");
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            _hasherMock.Setup(h => h.VerifyPassword("senha_errada", "hash_real")).Returns(false);

            Func<Task> acao = async () =>
                await _service.UpdateUserAsync(userId, "senha_errada", "novo_lucas", "email@novo.com", null);

            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("A palavra-passe atual está incorreta.");
        }
    }
}