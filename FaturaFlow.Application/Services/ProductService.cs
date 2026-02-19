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

    public async Task SaveProductAsync(Guid? id, Guid supplierId, string name, string reference, decimal purchasePrice, decimal salePrice, decimal vatRate, int stock, string unit)
    {
        try 
        {
            var pPrice = new Price(purchasePrice);
            var sPrice = new Price(salePrice);
            var vat = new VatRate(vatRate);

            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _productRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Produto não encontrado.");
                
                existing.UpdateDetails(name, reference, unit, pPrice, sPrice, vat, stock, supplierId);
                await _productRepo.UpdateAsync(existing);
            }
            else
            {
                var newProduct = new Product(name, reference, unit, pPrice, sPrice, vat, stock, supplierId);
                await _productRepo.AddAsync(newProduct);
            }
        }
        catch (Exception ex)
        {
            // Se a referência for única no banco, o erro de duplicidade cairá aqui
            throw new Exception($"Erro ao salvar produto: {ex.Message}");
        }
    }

    public async Task DeactivateAsync(Guid id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product != null)
        {
            product.Deactivate(); // Método que você deve ter na Entidade Product
            await _productRepo.UpdateAsync(product);
        }
    }
    // Adicione estes dois métodos dentro da classe ProductService
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
            product.Activate(); // Método que adicionamos no passo 1
            await _productRepo.UpdateAsync(product);
        }
    }
}