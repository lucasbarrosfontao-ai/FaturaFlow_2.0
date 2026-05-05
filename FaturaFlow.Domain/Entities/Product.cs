using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public string? UnitOfMeasure { get; private set; } 
        
        public Price PurchasePrice { get; private set; }
        public Price SalePrice { get; private set; }
        public bool VatIncluded { get; private set; }
        public Price PriceWithVat {get; private set; }
        public VatRate VatRate { get; private set; }
        
        public int StockQuantity { get; private set; }
        public Guid SupplierId { get; private set; }
        public bool IsActive { get; private set; }
        #pragma warning disable CS8618 
        private Product () {}
        #pragma warning restore CS8618
        public Product(string name, string reference, string unit, Price purchasePrice, Price salePrice,bool vatIncluded, VatRate vatRate,Price priceWithVat, int initialStock, Guid supplierId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Nome obrigatório.");
            if (string.IsNullOrWhiteSpace(reference)) throw new Exception("Referência obrigatória.");

            Id = Guid.NewGuid();
            Name = name;
            Reference = reference;
            UnitOfMeasure = unit;
            PurchasePrice = purchasePrice;
            if (vatIncluded)
            {
                PriceWithVat = salePrice;
                VatIncluded = true;
            }
            else
            {
                PriceWithVat = new Price(salePrice.Value * (1 + vatRate.Value / 100));
                VatIncluded = false;
            }
            SalePrice = salePrice;
            VatRate = vatRate;
            StockQuantity = initialStock;
            SupplierId = supplierId;
            IsActive = true;
        }

        public void AddStock(int quantity) => StockQuantity += quantity;
        public void RemoveStock(int quantity) => StockQuantity -= quantity;

        public void UpdateDetails(string name, string reference, string unit, Price purchasePrice, Price salePrice,bool vatIncluded, VatRate vat,Price pricewithvat, int stock, Guid supplierId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Nome obrigatório.");
            if (string.IsNullOrWhiteSpace(reference)) throw new Exception("Referência obrigatória.");

            Name = name;
            Reference = reference;
            UnitOfMeasure = unit;
            PurchasePrice = purchasePrice;
            SalePrice = salePrice;
            if (vatIncluded)
            {
                PriceWithVat = new Price(salePrice.Value);
                VatIncluded = true;

            }
            else
            {
                PriceWithVat = new Price(salePrice.Value * (1 + vat.Value / 100));
                VatIncluded = false;
            }
            VatRate = vat;
            StockQuantity = stock;
            SupplierId = supplierId;
        }

        public void UpdatePrices(Price purchase, Price sale, bool vatIncluded, VatRate vat)
        {
            PurchasePrice = purchase;
            if (vatIncluded)
            {
                SalePrice = sale;
                VatIncluded = true;
            }
            else
            {
                SalePrice = new Price(sale.Value * (1 + vat.Value / 100));
                VatIncluded = false;
            }
        }
        public void Deactivate()
        {
            IsActive = false;
        }
        public void Activate()
        {
            IsActive = true;
        }
    }
}