using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public string? UnitOfMeasure { get; private set; } // Unidade de medida (Ex: Un, Kg, L)
        
        public Price PurchasePrice { get; private set; }
        public Price SalePrice { get; private set; }
        public VatRate VatRate { get; private set; }
        
        public int StockQuantity { get; private set; }
        public Guid SupplierId { get; private set; }
        public bool IsActive { get; private set; }
        #pragma warning disable CS8618 
        private Product () {}
        #pragma warning restore CS8618
        public Product(string name, string reference, string unit, Price purchasePrice, Price salePrice, VatRate vatRate, int initialStock, Guid supplierId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Nome obrigatório.");
            if (string.IsNullOrWhiteSpace(reference)) throw new Exception("Referência obrigatória.");

            Id = Guid.NewGuid();
            Name = name;
            Reference = reference;
            UnitOfMeasure = unit;
            PurchasePrice = purchasePrice;
            SalePrice = salePrice;
            VatRate = vatRate;
            StockQuantity = initialStock;
            SupplierId = supplierId;
            IsActive = true;
        }

        // Métodos de Stock (Permite negativo como pediste)
        public void AddStock(int quantity) => StockQuantity += quantity;
        public void RemoveStock(int quantity) => StockQuantity -= quantity;

        public void UpdateDetails(string name, string reference, string unit, Price purchasePrice, Price salePrice, VatRate vat, int stock, Guid supplierId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Nome obrigatório.");
            if (string.IsNullOrWhiteSpace(reference)) throw new Exception("Referência obrigatória.");

            Name = name;
            Reference = reference;
            UnitOfMeasure = unit;
            PurchasePrice = purchasePrice;
            SalePrice = salePrice;
            VatRate = vat; // Use o nome que definiu (Vat ou VatRate)
            StockQuantity = stock;
            SupplierId = supplierId;
        }

        public void UpdatePrices(Price purchase, Price sale)
        {
            PurchasePrice = purchase;
            SalePrice = sale;
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