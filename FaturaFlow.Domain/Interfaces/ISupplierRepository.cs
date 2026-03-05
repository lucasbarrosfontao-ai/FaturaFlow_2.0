using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Interfaces
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(Guid id);
        Task<Supplier?> GetByNIPCAsync(PersonalId nipc);
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeactivateAsync(Guid id);
    }
}