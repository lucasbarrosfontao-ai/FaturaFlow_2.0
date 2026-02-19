namespace FaturaFlow.Application.Services;

using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Application.DTOs;

public class DashboardService
{
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IProductRepository _productRepo;

    public DashboardService(
        IInvoiceRepository invoiceRepo, 
        ICustomerRepository customerRepo, 
        IProductRepository productRepo)
    {
        _invoiceRepo = invoiceRepo;
        _customerRepo = customerRepo;
        _productRepo = productRepo;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var invoices = await _invoiceRepo.GetAllAsync();
        var customers = await _customerRepo.GetAllAsync();
        var products = await _productRepo.GetAllAsync();

        return new DashboardStatsDto(
            TotalInvoices: invoices.Count(),
            TotalCustomers: customers.Count(),
            TotalInvoicedAmount: invoices.Sum(i => i.TotalPayable),
            LowStockProducts: products.Count(p => p.StockQuantity < 5)
        );
    }
}