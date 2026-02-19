using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;

    public InvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        // O segredo está no .Include! 
        // Ele faz o "JOIN" automático com a tabela de linhas.
        return await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.Lines)
            .ToListAsync();
    }

    public async Task AddAsync(Invoice Invoice)
    {
        await _context.Invoices.AddAsync(Invoice);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice Invoice)
    {
        _context.Invoices.Update(Invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var Invoice = await GetByIdAsync(id);
        if (Invoice != null)
        {
            _context.Invoices.Remove(Invoice);
            await _context.SaveChangesAsync();
        }
    }
}