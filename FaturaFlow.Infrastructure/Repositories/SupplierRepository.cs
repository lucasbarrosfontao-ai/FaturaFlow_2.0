using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace FaturaFlow.Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Supplier?> GetByIdAsync(Guid id)
        {
            return await _context.Suppliers.FindAsync(id);
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task AddAsync(Supplier supplier)
        {
            try 
            {
                await _context.Suppliers.AddAsync(supplier);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            try 
            {
                _context.Suppliers.Update(supplier);
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
                // Verifica o campo duplicado no Fornecedor
                if (mysqlEx.Message.Contains("Email"))
                    throw new Exception("Este e-mail já está associado a outro fornecedor.");
                
                if (mysqlEx.Message.Contains("NIF") || mysqlEx.Message.Contains("NIPC") || mysqlEx.Message.Contains("PersonalId"))
                    throw new Exception("Já existe um fornecedor registado com este NIF/NIPC.");

                throw new Exception("Dados duplicados detetados ao gravar o fornecedor.");
            }

            throw ex;
        }

        public async Task DeactivateAsync(Guid id)
        {
            var supplier = await GetByIdAsync(id);
            if (supplier != null)
            {
                supplier.Deactivate();
                await _context.SaveChangesAsync();
            }
        }
    }
}
