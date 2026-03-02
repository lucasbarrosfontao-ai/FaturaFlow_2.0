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
    // 1. Adicionamos o repositório aqui
    private readonly ICompanyRepository _companyRepository;

    static PdfGeneratorService() => QuestPDF.Settings.License = LicenseType.Community;

    // 2. O construtor recebe o repositório via Injeção de Dependência
    public PdfGeneratorService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }
    
    // 3. O método agora é async Task<byte[]> para poder dar "await" no banco
    public async Task<byte[]> GerarFaturaPdfAsync(Invoice invoice, Customer customer)
    {
        // 4. Buscamos a empresa pelo ID fixo diretamente aqui dentro
        var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var company = await _companyRepository.GetByIdAsync(companyId);

        // O resto do código do PDF permanece igual, 
        // mas agora ele usa a variável 'company' que acabou de ser buscada
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
                        // Verificamos se o objeto 'company' que veio por parâmetro existe
                        if (company != null) 
                        {
                            col.Item().Text(company.Name).FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"NIF: {company.NIF?.Value ?? "N/A"}").FontSize(16);
                            
                            var endereco = $"{company.Address}, {company.City}";
                            if (!string.IsNullOrEmpty(company.ZipCode?.Value))
                                endereco += $" ({company.ZipCode.Value})";
                                
                            col.Item().Text(endereco).FontSize(12).FontColor(Colors.Grey.Darken2);
                        }
                        else
                        {
                            // Fallback caso a empresa não seja encontrada no banco
                            col.Item().Text("FaturaFlow").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("O fluxo inteligente da sua gestão").FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        }
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
                            c.Item().Text($"NIF: {customer.NIF?.Value ?? "Consumidor Final"}");
                            
                            var enderecoCli = $"{customer.Address} {customer.City}";
                            if (!string.IsNullOrEmpty(customer.ZipCode?.Value))
                                enderecoCli += $" ({customer.ZipCode.Value})";
                                
                            c.Item().Text(enderecoCli);
                        });
                    });

                    col.Item().PaddingVertical(15);

                    // TABELA DE ITENS
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn();  
                            columns.RelativeColumn();  
                            columns.RelativeColumn();  
                            columns.RelativeColumn();  
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Produto / Descrição");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Qtd");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Preço Unit.");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("IVA");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Total");

                            static IContainer HeaderStyle(IContainer container) => 
                                container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        foreach (var linha in invoice.Lines)
                        {
                            table.Cell().Element(RowStyle).Text(linha.ProductId.ToString());
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
                        x.Span("Fatura gerada por ");
                        x.Span(company?.Name ?? "FaturaFlow").SemiBold();
                        x.Span(" | Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }
}