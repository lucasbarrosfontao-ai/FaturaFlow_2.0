using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class InvoiceLine
    {
        public Guid Id { get; private set; }
        public Guid InvoiceId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public Price UnitPrice { get; private set; }
        public VatRate VatRate { get; private set; }
        public decimal Subtotal { get; private set; }
        public decimal VatAmount { get; private set; }
        #pragma warning disable CS8618 
        private InvoiceLine() {}
        #pragma warning restore CS8618
        public InvoiceLine(Guid invoiceId, Guid productId, int quantity, Price unitPrice, VatRate vatRate)
        {
            Id = Guid.NewGuid();
            InvoiceId = invoiceId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            VatRate = vatRate;

            Subtotal = quantity * unitPrice.Value;
            VatAmount = Subtotal * (vatRate.Value / 100);
        }
    }
}