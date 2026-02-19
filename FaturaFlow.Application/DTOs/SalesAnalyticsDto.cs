namespace FaturaFlow.Application.DTOs;

public record ChartDataPoint(string Label, decimal Value, DateTime OriginalDate);

public record SalesAnalyticsDto(
    List<ChartDataPoint> Last24Hours,
    List<ChartDataPoint> Last7Days,
    List<ChartDataPoint> Last12Months
);