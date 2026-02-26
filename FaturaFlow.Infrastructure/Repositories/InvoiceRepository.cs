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
            // MAGIA 1: AsNoTracking()
            // Faz com que o Service receba uma cópia "desconectada".
            // Isto impede que o EF Core misture as linhas antigas com as novas na memória.
            return await _context.Invoices
                .AsNoTracking() 
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == id);
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
            // 1. Busca a fatura real que está na Base de Dados neste momento (Tracked)
            var existingInvoice = await _context.Invoices
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (existingInvoice == null) throw new Exception("Fatura não encontrada na BD.");

            // 2. Atualiza apenas os campos básicos (Status, Cliente, Totais, etc.)
            _context.Entry(existingInvoice).CurrentValues.SetValues(invoice);

            // 3. MAGIA 2: Comparação inteligente de IDs para as Linhas
            var dbLineIds = existingInvoice.Lines.Select(l => l.Id).ToList();
            var newLineIds = invoice.Lines.Select(l => l.Id).ToList();

            // 3.1. Apaga APENAS as linhas que estão na BD mas que foram removidas no Service
            var linesToRemove = existingInvoice.Lines.Where(l => !newLineIds.Contains(l.Id)).ToList();
            if (linesToRemove.Any())
            {
                _context.InvoiceLines.RemoveRange(linesToRemove);
            }

            // 3.2. Insere APENAS as linhas que são totalmente novas
            // Como a sua colecção é IReadOnlyCollection, adicionamos diretamente ao DbContext!
            var linesToAdd = invoice.Lines.Where(l => !dbLineIds.Contains(l.Id)).ToList();
            foreach (var line in linesToAdd)
            {
                _context.InvoiceLines.Add(line);
            }

            // 4. Salva as alterações!
            // O EF Core fará os DELETEs certos para as velhas, e INSERTs certos para as novas.
            await _context.SaveChangesAsync();
        }
    }
}