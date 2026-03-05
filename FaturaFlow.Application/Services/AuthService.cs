namespace FaturaFlow.Application.Services;

using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;

public class AuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMessageService _messageService;

    public AuthService(
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        IMessageService messageService)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _messageService = messageService;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _userRepo.GetByUsernameAsync(username);

        if (user == null) return null;

        if (_passwordHasher.VerifyPassword(password, user.Password))
        {
            return user;
        }

        return null;
    }

    public async Task<User> RegisterUserAsync(string username, string rawPassword, string email)
    {
        var existingUser = await _userRepo.GetByUsernameAsync(username);
        if (existingUser != null)
            throw new Exception("Este nome de utilizador já está em uso.");

        string hashedPassword = _passwordHasher.HashPassword(rawPassword);

        var newUser = new User(username, hashedPassword, new EmailAddress(email));

        await _userRepo.AddAsync(newUser);

        return newUser;
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var emailVo = new EmailAddress(email);
        var user = await _userRepo.GetByEmailAsync(emailVo);

        if (user == null)
            throw new Exception("Utilizador não encontrado com este e-mail.");

        string code = new Random().Next(100000, 999999).ToString();
        user.SetRecoveryCode(code);

        await _userRepo.UpdateAsync(user);

        await _messageService.SendPasswordRecoveryAsync(email, code);
    }
    public async Task<IEnumerable<User>> GetAllUsersAsync() => await _userRepo.GetAllAsync();

    public async Task<User?> GetUserByIdAsync(Guid id) => await _userRepo.GetByIdAsync(id);

    public async Task DeleteUserAsync(Guid id) => await _userRepo.DeleteAsync(id);

    public async Task ResetPasswordWithCodeAsync(string email, string code, string newPassword)
    {
        var user = await _userRepo.GetByEmailAsync(new EmailAddress(email)) 
            ?? throw new Exception("Utilizador não encontrado.");

        if (user.RecoveryCode != code) throw new Exception("Código de verificação inválido.");

        string newHash = _passwordHasher.HashPassword(newPassword);
        user.UpdatePassword(newHash);
        user.ClearRecoveryCode();

        await _userRepo.UpdateAsync(user);
    }

    public async Task UpdateUserAsync(Guid userId, string oldPassword, string newUsername, string newEmail, string? newPassword)
    {
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new Exception("Utilizador não encontrado.");

        if (!_passwordHasher.VerifyPassword(oldPassword, user.Password))
            throw new Exception("A palavra-passe atual está incorreta.");

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            string newHash = _passwordHasher.HashPassword(newPassword);
            user.UpdatePassword(newHash);
        }

        user.UpdateDetails(newUsername, new EmailAddress(newEmail));
        
        await _userRepo.UpdateAsync(user);
    }
        
}
