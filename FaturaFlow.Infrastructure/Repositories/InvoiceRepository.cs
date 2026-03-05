using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FaturaFlow.Infrastructure.Repositories 
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .AsNoTracking() 
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber)) 
                return null;

            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }
        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Lines)
                .ToListAsync();
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Invoice invoice)
        {
            var existingInvoice = await _context.Invoices
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (existingInvoice == null) throw new Exception("Fatura não encontrada na BD.");

            _context.Entry(existingInvoice).CurrentValues.SetValues(invoice);

            var dbLineIds = existingInvoice.Lines.Select(l => l.Id).ToList();
            var newLineIds = invoice.Lines.Select(l => l.Id).ToList();

            var linesToRemove = existingInvoice.Lines.Where(l => !newLineIds.Contains(l.Id)).ToList();
            if (linesToRemove.Any())
            {
                _context.InvoiceLines.RemoveRange(linesToRemove);
            }

            var linesToAdd = invoice.Lines.Where(l => !dbLineIds.Contains(l.Id)).ToList();
            foreach (var line in linesToAdd)
            {
                _context.InvoiceLines.Add(line);
            }

 
            await _context.SaveChangesAsync();
        }
    }
}