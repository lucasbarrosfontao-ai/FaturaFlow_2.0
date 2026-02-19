using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;
using System.Linq.Expressions;

namespace FaturaFlow.Application.Services;

using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;

public class InvoiceService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IProductRepository _productRepo;
    private readonly IMessageService _messageRepo;

    public InvoiceService(
        IInvoiceRepository invoiceRepo,
        ICustomerRepository customerRepo,
        IProductRepository productRepo,
        IMessageService messageRepo)
    {
        _invoiceRepo = invoiceRepo;
        _customerRepo = customerRepo;
        _productRepo = productRepo;
        _messageRepo = messageRepo;
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync() => await _invoiceRepo.GetAllAsync();

    public async Task<Guid> CreateInvoiceAsync(Guid customerId, string invoiceNumber, List<(Guid productId, int quantity)> items)
    {
        // 1. Buscar Cliente
        var customer = await _customerRepo.GetByIdAsync(customerId) 
            ?? throw new Exception("Cliente não encontrado.");

        // 2. Criar a Raiz do Agregado (Invoice)
        var invoice = new Invoice(customerId, invoiceNumber);

        // 3. Processar Itens
        foreach (var item in items)
        {
            var product = await _productRepo.GetByIdAsync(item.productId) 
                ?? throw new Exception($"Produto {item.productId} não encontrado.");

            // Regra de Negócio: Adicionar linha (A Invoice calcula os totais internamente)
            invoice.AddLine(product.Id, item.quantity, product.SalePrice, product.VatRate);

            // Regra de Negócio: Baixar Stock
            product.RemoveStock(item.quantity);

            // Notificamos o repositório da mudança no produto
            await _productRepo.UpdateAsync(product);
        }

        // 4. Persistir a Fatura (O EF salva as linhas automaticamente devido ao mapeamento)
        await _invoiceRepo.AddAsync(invoice);

        // 5. Enviar Mensagem (RabbitMQ)
        if (customer.Email?.Value != null)
        {
            await _messageRepo.SendInvoiceMessageAsync(invoice.Id, customer.Name, customer.Email.Value);
        }

        return invoice.Id;
    }
}