using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace FaturaFlow.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            try 
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        public async Task UpdateAsync(Product product)
        {
            try 
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        private void HandleDbException(DbUpdateException ex)
        {
            if (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                if (mysqlEx.Message.Contains("Reference"))
                    throw new Exception("Já existe um produto registado com esta referência.");

                throw new Exception("Não foi possível gravar o produto devido a dados duplicados.");
            }

            throw ex;
        }

        public async Task DeactivateAsync(Guid id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                product.Deactivate();
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Product?> GetByRefAsync(string refecence)
        {
            if (refecence == null) return null;
            return await _context.Products.FirstOrDefaultAsync(p => p.Reference == refecence);
        }
    }
}
