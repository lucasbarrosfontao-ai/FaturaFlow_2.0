using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;
namespace FaturaFlow.Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepo;

    public ProductService(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public async Task<IEnumerable<Product>> GetAllActiveAsync()
    {
        var all = await _productRepo.GetAllAsync();
        return all.Where(p => p.IsActive);
    }

    public async Task<Product?> GetByIdAsync(Guid id) => await _productRepo.GetByIdAsync(id);

    public async Task SaveProductAsync(Guid? id, Guid supplierId, string name, string reference, decimal purchasePrice, decimal salePrice,bool vatIncluded,decimal pricewithVat, decimal vatRate, int stock, string unit)
    {
        try 
        {
            var pPrice = new Price(purchasePrice);
            var sPrice = new Price(salePrice);
            var vat = new VatRate(vatRate);
            var pricewithvat = new Price(pricewithVat);
            
            var productwithsameref = await _productRepo.GetByRefAsync(reference);
            if (productwithsameref != null)
            {
               
                if (id.HasValue && id != Guid.Empty)
                {
                    if (productwithsameref.Id != id.Value)
                    {
                        throw new Exception("A Referencia já existe e pertence a outro produto.");
                    }
                }
                else
                {
                    throw new Exception("A Referencia já existe.");
                }
            }
            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _productRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Produto não encontrado.");
                
                existing.UpdateDetails(name, reference,unit,pPrice,sPrice,vatIncluded,vat,pricewithvat,stock,supplierId);
                await _productRepo.UpdateAsync(existing); 
            }
            else
            {
                var newSupplier = new Product(name, reference,unit,pPrice,sPrice,vatIncluded,vat,pricewithvat,stock,supplierId);
                await _productRepo.AddAsync(newSupplier);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao salvar produto: {ex.Message}");
        }
    }

    public async Task DeactivateAsync(Guid id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product != null)
        {
            product.Deactivate();
            await _productRepo.UpdateAsync(product);
        }
    }
    public async Task<IEnumerable<Product>> GetInactiveAsync()
    {
        var all = await _productRepo.GetAllAsync();
        return all.Where(p => !p.IsActive);
    }

    public async Task ActivateAsync(Guid id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product != null)
        {
            product.Activate(); 
            await _productRepo.UpdateAsync(product);
        }
    }
}