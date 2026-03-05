using System.Runtime.CompilerServices;
using System.Text;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Application.Services;

public class InvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IProductRepository _productRepo;
    private readonly IMessageService _messageService;

    public InvoiceService(
        IInvoiceRepository invoiceRepo,
        ICustomerRepository customerRepo,
        IProductRepository productRepo,
        IMessageService messageService)
    {
        _invoiceRepo = invoiceRepo;
        _customerRepo = customerRepo;
        _productRepo = productRepo;
        _messageService = messageService;
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync() => await _invoiceRepo.GetAllAsync();

    public async Task<Invoice?> GetInvoiceByIdAsync(Guid id) => await _invoiceRepo.GetByIdAsync(id);

    public async Task<Guid> CreateDraftInvoiceAsync(Guid customerId, string invoiceNumber, DateTime invoiceDate, List<(Guid productId, int quantity)> items)
    {
        try
        {
            if (invoiceDate > DateTime.Now)
                throw new Exception("A data da fatura não pode ser Futura.");

            var existingInvoice = await _invoiceRepo.GetByInvoiceNumberAsync(invoiceNumber);
            if (existingInvoice != null)
            {
                throw new Exception($"O número da fatura '{invoiceNumber}' já existe.");
            }

            var customer = await _customerRepo.GetByIdAsync(customerId)
                ?? throw new Exception("Cliente não encontrado.");

            var invoice = new Invoice(customerId, invoiceNumber, invoiceDate, Invoice.StatusDraft);
            
            foreach (var item in items)
            {
                var product = await _productRepo.GetByIdAsync(item.productId)
                    ?? throw new Exception($"Produto {item.productId} não encontrado.");
                
                invoice.AddLine(product.Id, item.quantity, product.SalePrice, product.VatRate);
            }

            await _invoiceRepo.AddAsync(invoice);
            return invoice.Id;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao criar rascunho: {ex.Message}");
        }
    }

    public async Task UpdateDraftInvoiceAsync(Guid invoiceId, Guid customerId, string invoiceNumber, DateTime invoiceDate, List<(Guid productId, int quantity)> items)
    {
        try
        {
            if (invoiceDate > DateTime.Now)
                throw new Exception("A data da fatura não pode ser Futura.");

            var existingWithNumber = await _invoiceRepo.GetByInvoiceNumberAsync(invoiceNumber);
            
            if (existingWithNumber != null)
            {
                if (existingWithNumber.Id != invoiceId)
                {
                    throw new Exception($"O número da fatura '{invoiceNumber}' já está a ser usado em outra fatura.");
                }
            }

            var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
                ?? throw new Exception("Fatura não encontrada.");

            invoice.UpdateDetails(customerId, invoiceNumber, invoiceDate); 
            invoice.ClearLines(); 

            foreach (var item in items)
            {
                var product = await _productRepo.GetByIdAsync(item.productId)
                    ?? throw new Exception($"Produto {item.productId} não encontrado.");
                
                invoice.AddLine(product.Id, item.quantity, product.SalePrice, product.VatRate);
            }

            await _invoiceRepo.UpdateAsync(invoice);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao atualizar rascunho: {ex.Message}");
        }
    }

    public async Task EmitInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
            ?? throw new Exception("Fatura não encontrada.");

        if (invoice.Status != "Rascunho")
            throw new Exception("Esta fatura já foi emitida.");
        if (invoice.IssueDate > DateTime.Now)
            throw new Exception("A data da fatura não pode ser Futura.");
        foreach (var line in invoice.Lines) 
        {
            var product = await _productRepo.GetByIdAsync(line.ProductId)
                ?? throw new Exception("Produto não encontrado.");
            
            product.RemoveStock(line.Quantity);
            await _productRepo.UpdateAsync(product);
        }

        invoice.Issue(); 
        
        await _invoiceRepo.UpdateAsync(invoice);

        var customer = await _customerRepo.GetByIdAsync(invoice.CustomerId);
        if (customer?.Email?.Value != null)
        {
            await _messageService.SendInvoiceMessageAsync(invoice.Id, customer.Name, customer.Email.Value);
        }
    }

    public async Task MarkAsPaidAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
            ?? throw new Exception("Fatura não encontrada.");

        invoice.MarkAsPaid();
        await _invoiceRepo.UpdateAsync(invoice);
    }
    public async Task<Invoice> GetInvoiceForEditAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
            ?? throw new Exception("Fatura não encontrada.");

        if (invoice.Status != Invoice.StatusDraft)
        {
            throw new Exception("Somente faturas em rascunho podem ser editadas.");
        }

        return invoice;
    }
    public async Task SendEmailAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(invoiceId)
            ?? throw new Exception("Fatura não encontrada.");

        if (invoice.Status == Invoice.StatusDraft)
        {
            throw new Exception("Não é possível enviar um rascunho por email. Por favor, emita a fatura primeiro.");
        }

        var customer = await _customerRepo.GetByIdAsync(invoice.CustomerId);
        if (customer?.Email?.Value == null)
        {
            throw new Exception("O cliente associado a esta fatura não tem um email válido. Por favor, atualize os dados do cliente antes de enviar.");
        }

        await _messageService.SendInvoiceMessageAsync(invoice.Id, customer.Name, customer.Email.Value);
    }
}