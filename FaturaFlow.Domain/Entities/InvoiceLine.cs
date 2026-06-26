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

        public InvoiceLine(Guid invoiceId, Guid productId, int quantity, Price unitPrice, VatRate vatRate, decimal priceWithVat, bool vatIncluded)
        {
            Id = Guid.NewGuid();
            InvoiceId = invoiceId;
            ProductId = productId;
            Quantity = quantity;
            VatRate = vatRate;

            decimal totalGross = quantity * priceWithVat;

            if (vatIncluded)
            {
                decimal discountFactor = 1m + (vatRate.Value / 100m);
                Subtotal = totalGross / discountFactor;
                UnitPrice = new Price(unitPrice.Value / discountFactor);
            }
            else
            {
                Subtotal = quantity * unitPrice.Value;
                UnitPrice = new Price(unitPrice.Value);
            }

            VatAmount = totalGross - Subtotal;
        }
    }
}