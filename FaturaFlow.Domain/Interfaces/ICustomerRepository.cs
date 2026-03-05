using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task<Customer?> GetByEmailAsync(EmailAddress email);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeactivateAsync(Guid id);
        Task ActiveAsync(Guid id);
        Task<Customer?> GetByNifAsync(PersonalId nif); 
    }
}