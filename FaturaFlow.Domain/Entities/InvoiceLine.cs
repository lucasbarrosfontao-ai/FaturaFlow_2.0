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
        
        // Em contabilidade, o Subtotal é SEMPRE o valor Líquido (Sem IVA)
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

            // 1. O Total Bruto exato desta linha (Com IVA)
            decimal totalGross = quantity * priceWithVat;

            // 2. Encontrar o Subtotal (Líquido) e o Preço Base Unitário
            if (vatIncluded)
            {
                // Removemos o IVA para encontrar a Base Limpa
                decimal discountFactor = 1m + (vatRate.Value / 100m);
                Subtotal = totalGross / discountFactor;
                UnitPrice = new Price(unitPrice.Value / discountFactor);
            }
            else
            {
                // Se o IVA não estava incluído, o unitPrice é a própria Base Limpa
                Subtotal = quantity * unitPrice.Value;
                UnitPrice = new Price(unitPrice.Value);
            }

            // 3. O valor exato do IVA é a diferença entre o Total Bruto e o Subtotal
            VatAmount = totalGross - Subtotal;
        }
    }
}