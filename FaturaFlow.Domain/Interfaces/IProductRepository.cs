using FaturaFlow.Domain.Entities;

namespace FaturaFlow.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product?> GetByRefAsync(string Ref);
        Task<IEnumerable<Product>> GetAllAsync();
        
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeactivateAsync(Guid id);
    }
}