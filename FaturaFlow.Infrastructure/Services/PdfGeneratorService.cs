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

    private readonly ICompanyRepository _companyRepository;
    private readonly IProductRepository _productRepository;

    static PdfGeneratorService() => QuestPDF.Settings.License = LicenseType.Community;

    public PdfGeneratorService(ICompanyRepository companyRepository, IProductRepository productRepository)
    {
        _companyRepository = companyRepository;
        _productRepository = productRepository; 
    }

    public async Task<byte[]> GerarFaturaPdfAsync(Invoice invoice, Customer customer)
    {
        var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var company = await _companyRepository.GetByIdAsync(companyId);
        
        var nomesProdutos = new Dictionary<Guid, string>();
        foreach (var linha in invoice.Lines)
        {
            if (!nomesProdutos.ContainsKey(linha.ProductId))
            {
                var produto = await _productRepository.GetByIdAsync(linha.ProductId);
                nomesProdutos[linha.ProductId] = produto?.Name ?? "Produto não encontrado";
            }
        }

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
                            string nomeDoProduto = nomesProdutos.ContainsKey(linha.ProductId) 
                                                   ? nomesProdutos[linha.ProductId] 
                                                   : linha.ProductId.ToString();

                            table.Cell().Element(RowStyle).Text(nomeDoProduto);
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

    public async Task<byte[]> GerarReciboPdfAsync(Invoice invoice, Customer customer)
    {
        var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var company = await _companyRepository.GetByIdAsync(companyId);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                // --- CABEÇALHO  ---
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(company?.Name ?? "FaturaFlow").FontSize(24).SemiBold().FontColor(Colors.Green.Medium);
                        col.Item().Text($"NIF: {company?.NIF?.Value ?? "N/A"}").FontSize(14);
                        col.Item().Text($"{company?.Address}, {company?.City}").FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("RECIBO").FontSize(24).ExtraBold().FontColor(Colors.Green.Medium);
                        col.Item().Text($"Nº: RC-{invoice.InvoiceNumber}").FontSize(12).SemiBold();
                        col.Item().Text($"Data de Emissão: {DateTime.Now.ToString("dd/MM/yyyy", _culture)}");
                    });
                });

                // --- CONTEÚDO ---
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Background(Colors.Green.Lighten5).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("CONFIRMAÇÃO DE RECEBIMENTO").FontSize(8).SemiBold().FontColor(Colors.Green.Darken3);
                            c.Item().PaddingTop(5).Text(t =>
                            {
                                t.Span("Confirmamos que recebemos de ");
                                t.Span(customer.Name).SemiBold();
                                t.Span(" a quantia total de ");
                                t.Span(invoice.TotalPayable.ToString("C", _culture)).SemiBold().FontColor(Colors.Green.Medium);
                                t.Span(" para liquidação do documento abaixo referido.");
                            });
                        });
                    });

                    col.Item().PaddingVertical(15);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(); 
                            columns.RelativeColumn();  
                            columns.RelativeColumn();  
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Documento de Origem");
                            header.Cell().Element(HeaderStyle).Text("Data Fatura");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Valor Inc.");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Valor Pago");

                            static IContainer HeaderStyle(IContainer container) => 
                                container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        table.Cell().Element(RowStyle).Text($"Fatura nº {invoice.InvoiceNumber}");
                        table.Cell().Element(RowStyle).Text(invoice.IssueDate.ToString("dd/MM/yyyy", _culture));
                        table.Cell().Element(RowStyle).AlignRight().Text(invoice.TotalPayable.ToString("C", _culture));
                        table.Cell().Element(RowStyle).AlignRight().Text(invoice.TotalPayable.ToString("C", _culture));

                        static IContainer RowStyle(IContainer container) => 
                            container.PaddingVertical(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                    });

                    col.Item().PaddingVertical(20);

                    col.Item().Row(row => 
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("OBSERVAÇÕES").FontSize(8).SemiBold();
                            c.Item().Text("Este recibo serve como prova de pagamento integral da fatura mencionada.").FontSize(9).Italic();
                            c.Item().PaddingTop(10).Text("Meio de Pagamento: Transferência Bancária / Numerário").FontSize(9);
                        });

                        row.ConstantItem(150).Column(c => {
                            c.Item().PaddingTop(10).BorderTop(0.5f).AlignCenter().Text("Assinatura / Carimbo").FontSize(8);
                        });
                    });
                });
              
                // --- RODAPÉ ---
                page.Footer().AlignCenter().Column(c => {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    c.Item().PaddingTop(5).Text(x =>
                    {
                        x.Span("Documento gerado por ");
                        x.Span(company?.Name ?? "FaturaFlow").SemiBold();
                        x.Span(" | Processado por Computador");
                    });
                });
            });
        }).GeneratePdf();
    }
}