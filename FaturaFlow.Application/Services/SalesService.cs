using FaturaFlow.Application.DTOs;
using FaturaFlow.Domain.Interfaces;

namespace FaturaFlow.Application.Services;

public class SalesService
{
    private readonly IInvoiceRepository _invoiceRepo;

    public SalesService(IInvoiceRepository invoiceRepo)
    {
        _invoiceRepo = invoiceRepo;
    }

    public async Task<SalesAnalyticsDto> GetSalesAnalyticsAsync()
    {
        var agora = DateTime.Now;
        var limiteAno = agora.AddYears(-1);
        
        // Buscamos as faturas do repositório
        var allInvoices = await _invoiceRepo.GetAllAsync();
        var faturas = allInvoices.Where(f => f.IssueDate >= limiteAno).ToList();

        // 1. ÚLTIMAS 24 HORAS
        var limite24h = agora.AddHours(-24);
        var v24h = faturas
            .Where(f => f.IssueDate >= limite24h)
            .GroupBy(f => f.IssueDate.Hour)
            .Select(g => new ChartDataPoint($"{g.Key}h", g.Sum(f => f.TotalPayable), agora.Date.AddHours(g.Key)))
            .OrderBy(x => x.Label)
            .ToList();

        // 2. ÚLTIMOS 7 DIAS
        var limiteSemana = DateTime.Today.AddDays(-7);
        var vSemana = faturas
            .Where(f => f.IssueDate >= limiteSemana)
            .GroupBy(f => f.IssueDate.Date)
            .Select(g => new ChartDataPoint(g.Key.ToString("dd/MM"), g.Sum(f => f.TotalPayable), g.Key))
            .OrderBy(x => x.OriginalDate)
            .ToList();

        // 3. ÚLTIMOS 12 MESES
        var vAno = faturas
            .GroupBy(f => new { f.IssueDate.Year, f.IssueDate.Month })
            .Select(g => new ChartDataPoint(
                $"{g.Key.Month}/{g.Key.Year}", 
                g.Sum(f => f.TotalPayable), 
                new DateTime(g.Key.Year, g.Key.Month, 1)))
            .OrderBy(x => x.OriginalDate)
            .ToList();

        return new SalesAnalyticsDto(v24h, vSemana, vAno);
    }
}