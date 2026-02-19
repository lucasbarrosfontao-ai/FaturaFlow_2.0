using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class Invoice
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string InvoiceNumber { get; private set; }
        public DateTime IssueDate { get; private set; }
        public string Status { get; private set; } // Pendente, Emitida

        // Totais (Calculados automaticamente)
        public decimal TotalNet { get; private set; }
        public decimal TotalVat { get; private set; }
        public decimal TotalPayable { get; private set; }

        // Lista de Linhas (Repare o 'ReadOnly' para ninguém mexer de fora)
        
        private readonly List<InvoiceLine> _lines = new();
        public IReadOnlyCollection<InvoiceLine> Lines => _lines;
        #pragma warning disable CS8618 
        private Invoice () {}
        #pragma warning restore CS8618
        public Invoice(Guid customerId, string invoiceNumber)
        {
            Id = Guid.NewGuid(); // ID gerado no código!
            CustomerId = customerId;
            InvoiceNumber = invoiceNumber;
            IssueDate = DateTime.Now;
            Status = "Emitida";
        }

        // O "Cérebro" da Fatura: Adicionar uma linha e recalcular tudo
        public void AddLine(Guid productId, int quantity, Price unitPrice, VatRate vatRate)
        {
            var line = new InvoiceLine(Id, productId, quantity, unitPrice, vatRate);
            _lines.Add(line);

            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            TotalNet = _lines.Sum(l => l.Subtotal);
            TotalVat = _lines.Sum(l => l.VatAmount);
            TotalPayable = TotalNet + TotalVat;
        }
    }
}