using FaturaFlow.Domain.Entities;

namespace FaturaFlow.Domain.Interfaces
{
    public interface ISupplierRepository
    {
        Task<Supplier?> GetByIdAsync(Guid id);
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeactivateAsync(Guid id);
    }
}