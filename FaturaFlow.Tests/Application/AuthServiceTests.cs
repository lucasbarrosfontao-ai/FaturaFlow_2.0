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
            // --- ARRANGE ---
            var user = new User("lucas", "hash_antigo");
            _userRepoMock.Setup(r => r.GetByUsernameAsync("lucas")).ReturnsAsync(user);

            // Simulamos que o hasher confirma que a senha "123" corresponde ao "hash_antigo"
            _hasherMock.Setup(h => h.VerifyPassword("123", "hash_antigo")).Returns(true);

            // --- ACT ---
            var result = await _service.LoginAsync("lucas", "123");

            // --- ASSERT ---
            result.Should().NotBeNull();
            result.Username.Should().Be("lucas");
        }

        [Fact]
        public async Task RegisterUserAsync_Deve_Lancar_Erro_Se_Utilizador_Ja_Existir()
        {
            // --- ARRANGE ---
            var existingUser = new User("lucas", "hash");
            _userRepoMock.Setup(r => r.GetByUsernameAsync("lucas")).ReturnsAsync(existingUser);

            // --- ACT ---
            Func<Task> acao = async () => await _service.RegisterUserAsync("lucas", "123", "lucas@teste.com");

            // --- ASSERT ---
            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("Este nome de utilizador já está em uso.");

            // Garante que o método AddAsync nunca foi chamado
            _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_Deve_Gerar_Codigo_E_Enviar_Email()
        {
            // --- ARRANGE ---
            var emailStr = "teste@email.com";
            var user = new User("lucas", "hash", new EmailAddress(emailStr));

            _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<EmailAddress>())).ReturnsAsync(user);

            // --- ACT ---
            await _service.RequestPasswordResetAsync(emailStr);

            // --- ASSERT ---
            // 1. Verificamos se um código foi gerado no objeto user
            user.RecoveryCode.Should().NotBeNull();
            user.RecoveryCode.Length.Should().Be(6); // Definiste um Random de 6 dígitos

            // 2. Verificamos se o banco de dados foi atualizado com o código
            _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);

            // 3. Verificamos se o RabbitMQ foi avisado para enviar o código
            _messageServiceMock.Verify(m => m.SendPasswordRecoveryAsync(emailStr, user.RecoveryCode), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_Deve_Lancar_Erro_Se_Senha_Atual_Incorreta()
        {
            // --- ARRANGE ---
            var userId = Guid.NewGuid();
            var user = new User("lucas", "hash_real");
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Simulamos que a verificação da senha atual FALHOU
            _hasherMock.Setup(h => h.VerifyPassword("senha_errada", "hash_real")).Returns(false);

            // --- ACT ---
            Func<Task> acao = async () =>
                await _service.UpdateUserAsync(userId, "senha_errada", "novo_lucas", "email@novo.com", null);

            // --- ASSERT ---
            await acao.Should().ThrowAsync<Exception>()
                .WithMessage("A palavra-passe atual está incorreta.");
        }
    }
}