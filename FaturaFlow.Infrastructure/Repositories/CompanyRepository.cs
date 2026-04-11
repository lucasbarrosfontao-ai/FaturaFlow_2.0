using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using FaturaFlow.Infrastructure.Data;
using FaturaFlow.Domain.ValueObjects;
using MySqlConnector;
namespace FaturaFlow.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetByIdAsync(Guid id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companies.ToListAsync();
        }

        public async Task AddAsync(Company company)
        {
            try 
            {
                await _context.Companies.AddAsync(company);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        public async Task UpdateAsync(Company company)
        {
            try 
            {
                _context.Companies.Update(company);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                HandleDbException(ex);
            }
        }

        private void HandleDbException(DbUpdateException ex)
        {
            if (ex.InnerException is MySqlException mysqlEx)
            {
                switch (mysqlEx.Number)
                {
                    default:
                        throw new Exception("Ocorreu um erro ao acessar o banco de dados. Tente novamente mais tarde.");
                }
            }
            else
            {
                throw new Exception("Ocorreu um erro inesperado. Tente novamente mais tarde.");
            }
        }
    }
}