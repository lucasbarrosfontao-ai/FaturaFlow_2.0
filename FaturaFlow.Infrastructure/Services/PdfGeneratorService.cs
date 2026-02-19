using System.Globalization;
using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FaturaFlow.Infrastructure.Services;

public class PdfGeneratorService : IPdfService
{
    private readonly CultureInfo _culture = new CultureInfo("pt-PT");

    static PdfGeneratorService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] GerarFaturaPdf(Invoice invoice, Customer customer)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                // --- CABEÇALHO ---
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FaturaFlow").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("O fluxo inteligente da sua gestão").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(5).Text("NIF: XXXXXXXXXXX");
                        col.Item().Text("Localidade: XXXXXXXXX XXXXXXX XXXXXXXXX");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("FATURA").FontSize(20).ExtraBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Nº: {invoice.InvoiceNumber}").FontSize(12).SemiBold();
                        col.Item().Text($"Data: {invoice.IssueDate.ToString("dd/MM/yyyy", _culture)}");
                        col.Item().Text($"Estado: {invoice.Status}").FontSize(9);
                    });
                });

                // --- CONTEÚDO ---
                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Seção do Cliente
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DADOS DO CLIENTE").FontSize(8).SemiBold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(customer.Name).FontSize(11).SemiBold();
                            c.Item().Text($"NIF: {customer.NIF.Value}");
                            
                            // Endereço (usando os novos campos do seu DDD)
                            var endereco = $"{customer.Address}, {customer.City}";
                            if (!string.IsNullOrEmpty(customer.ZipCode?.Value))
                                endereco += $" ({customer.ZipCode.Value})";
                                
                            c.Item().Text(endereco);
                        });
                    });

                    col.Item().PaddingVertical(15);

                    // TABELA DE ITENS
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Produto/Descrição
                            columns.RelativeColumn();  // Quantidade
                            columns.RelativeColumn();  // Preço Unit.
                            columns.RelativeColumn();  // IVA %
                            columns.RelativeColumn();  // Total
                        });

                        // Cabeçalho da Tabela
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Produto / ID");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Qtd");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Preço Unit.");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("IVA");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Total");

                            static IContainer HeaderStyle(IContainer container) => 
                                container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        // Linhas da Tabela
                        foreach (var linha in invoice.Lines)
                        {
                            table.Cell().Element(RowStyle).Text($"Produto ID: {linha.ProductId}");
                            table.Cell().Element(RowStyle).AlignRight().Text(linha.Quantity.ToString());
                            table.Cell().Element(RowStyle).AlignRight().Text(linha.UnitPrice.Value.ToString("C", _culture));
                            table.Cell().Element(RowStyle).AlignRight().Text($"{linha.VatRate.Value}%");
                            table.Cell().Element(RowStyle).AlignRight().Text(linha.Subtotal.ToString("C", _culture));

                            static IContainer RowStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        }
                    });

                    // RESUMO DE TOTAIS
                    col.Item().AlignRight().PaddingTop(15).Column(c =>
                    {
                        c.Item().Text(t => {
                            t.Span("Total Líquido: ").FontSize(10);
                            t.Span(invoice.TotalNet.ToString("C", _culture));
                        });
                        
                        c.Item().Text(t => {
                            t.Span("Total IVA: ").FontSize(10);
                            t.Span(invoice.TotalVat.ToString("C", _culture));
                        });

                        c.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("TOTAL A PAGAR: ").FontSize(16).ExtraBold();
                            text.Span(invoice.TotalPayable.ToString("C", _culture)).FontSize(16).ExtraBold().FontColor(Colors.Blue.Medium);
                        });
                    });
                });

                // --- RODAPÉ ---
                page.Footer().AlignCenter().Column(c => {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    c.Item().PaddingTop(5).Text(x =>
                    {
                        x.Span("Fatura gerada automaticamente por FaturaFlow | Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }
}