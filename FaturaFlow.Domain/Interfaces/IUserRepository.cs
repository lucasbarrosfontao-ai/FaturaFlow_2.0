using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(EmailAddress emailAddress);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}