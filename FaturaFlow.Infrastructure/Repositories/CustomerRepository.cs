using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using FaturaFlow.Infrastructure.Data;
using FaturaFlow.Domain.ValueObjects;
using MySqlConnector;
namespace FaturaFlow.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetByEmailAsync(EmailAddress email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task AddAsync(Customer customer)
        {
            try 
            {
                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        public async Task UpdateAsync(Customer customer)
        {
            try 
            {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        // Mapeamento de erro técnico para erro amigável
        private void HandleDbException(DbUpdateException ex)
        {
            if (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062)
            {
                // Aqui você traduz o erro do MySQL para algo que o seu Service entenda
                if (mysqlEx.Message.Contains("Email"))
                    throw new Exception("Este e-mail já está em uso por outro cliente.");
                
                if (mysqlEx.Message.Contains("NIF") || mysqlEx.Message.Contains("PersonalId"))
                    throw new Exception("Já existe um cliente registado com este NIF.");

                throw new Exception("Existem dados duplicados que não podem ser guardados.");
            }

            throw ex; // Se não for erro de duplicidade, lança o erro original
        }
        public async Task DeleteAsync(Guid id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}