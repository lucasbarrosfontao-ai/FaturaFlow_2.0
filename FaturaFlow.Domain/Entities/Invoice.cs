using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class Invoice
    {
        // Constantes para evitar Strings "mágicas" no código
        public const string StatusDraft = "Rascunho";
        public const string StatusIssued = "Emitida";
        public const string StatusPaid = "Paga";
        public const string StatusCancelled = "Cancelada";

        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string InvoiceNumber { get; private set; }
        public DateTime IssueDate { get; private set; }
        public string Status { get; private set; }

        public decimal TotalNet { get; private set; }
        public decimal TotalVat { get; private set; }
        public decimal TotalPayable { get; private set; }

        private readonly List<InvoiceLine> _lines = new();
        public IReadOnlyCollection<InvoiceLine> Lines => _lines;

        #pragma warning disable CS8618 
        private Invoice () {}
        #pragma warning restore CS8618

        public Invoice(Guid customerId, string invoiceNumber,DateTime date, string status = StatusDraft)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            InvoiceNumber = invoiceNumber;
            IssueDate = date;
            Status = status;
        }

        // Compatibilidade: construtor sem data (usado nos testes e locais que não fornecem a data)
        public Invoice(Guid customerId, string invoiceNumber, string status = StatusDraft)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            InvoiceNumber = invoiceNumber;
            IssueDate = DateTime.Now;
            Status = status;
        }

        public void UpdateDetails(Guid customerId, string invoiceNumber, DateTime date)
        {
            if (Status != StatusDraft)
                throw new InvalidOperationException("Apenas rascunhos podem ser editados.");

            CustomerId = customerId;
            InvoiceNumber = invoiceNumber;
            IssueDate = date;
        }

        // Overload sem data para compatibilidade
        public void UpdateDetails(Guid customerId, string invoiceNumber)
        {
            if (Status != StatusDraft)
                throw new InvalidOperationException("Apenas rascunhos podem ser editados.");

            CustomerId = customerId;
            InvoiceNumber = invoiceNumber;
        }
        public void ClearLines()
        {
            if (Status != StatusDraft)
                throw new InvalidOperationException("Não é possível alterar itens de uma fatura já emitida.");
            
            _lines.Clear();
            RecalculateTotals();
        }

        public void AddLine(Guid productId, int quantity, Price unitPrice, VatRate vatRate)
        {
            if (Status != StatusDraft)
                throw new InvalidOperationException("Não é possível adicionar itens a uma fatura já emitida.");

            var line = new InvoiceLine(Id, productId, quantity, unitPrice, vatRate);
            _lines.Add(line);
            RecalculateTotals();
        }

        public void Issue()
        {
            if (Status != StatusDraft)
                throw new InvalidOperationException("A fatura já foi emitida ou cancelada.");
            
            if (!_lines.Any())
                throw new InvalidOperationException("Não é possível emitir uma fatura sem itens.");

            Status = StatusIssued;
            IssueDate = DateTime.Now; // Data oficial de emissão
        }

        public void MarkAsPaid()
        {
            if (Status != StatusIssued)
                throw new InvalidOperationException("Apenas faturas emitidas podem ser marcadas como pagas.");
            Status = StatusPaid;
        }

        private void RecalculateTotals()
        {
            TotalNet = _lines.Sum(l => l.Subtotal);
            TotalVat = _lines.Sum(l => l.VatAmount);
            TotalPayable = TotalNet + TotalVat;
        }
    }
}