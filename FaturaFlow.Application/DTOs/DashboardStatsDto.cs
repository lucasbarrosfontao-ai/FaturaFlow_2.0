namespace FaturaFlow.Application.DTOs;

public record DashboardStatsDto(
    int TotalInvoices,
    int TotalCustomers,
    decimal TotalInvoicedAmount,
    int LowStockProducts
);