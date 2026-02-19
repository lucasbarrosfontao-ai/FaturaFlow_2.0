using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Application.Services;

public class SupplierService
{
    private readonly ISupplierRepository _supplierRepo;

    public SupplierService(ISupplierRepository supplierRepo)
    {
        _supplierRepo = supplierRepo;
    }

    public async Task<IEnumerable<Supplier>> GetAllActiveAsync()
    {
        var all = await _supplierRepo.GetAllAsync();
        return all.Where(s => s.IsActive);
    }

    public async Task<Supplier?> GetByIdAsync(Guid id) => await _supplierRepo.GetByIdAsync(id);

    public async Task SaveSupplierAsync(Guid? id, string name, string nipc, string repName, string phone, string email, string address, string city, string zip)
    {
        try 
        {
            var nipcVo = new PersonalId(nipc);
            var phoneVo = new PhoneNumber(phone);
            var emailVo = new EmailAddress(email);
            var zipVo = new PostalCode(zip);

            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _supplierRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Fornecedor não encontrado.");

                existing.UpdateDetails(name, nipcVo, repName, phoneVo, emailVo, address, city, zipVo);
                await _supplierRepo.UpdateAsync(existing);
            }
            else
            {
                var newSupplier = new Supplier(name, nipcVo, repName, phoneVo, emailVo, address, city, zipVo);
                await _supplierRepo.AddAsync(newSupplier);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao salvar fornecedor: {ex.Message}");
        }
    }
    public async Task DeactivateAsync(Guid id)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id);
        if (supplier != null)
        {
            supplier.Deactivate();
            await _supplierRepo.UpdateAsync(supplier);
        }
    }
    // Adicione estes dois métodos dentro da classe SupplierService
    public async Task<IEnumerable<Supplier>> GetInactiveAsync()
    {
        var all = await _supplierRepo.GetAllAsync();
        return all.Where(s => !s.IsActive);
    }

    public async Task ActivateAsync(Guid id)
    {
        var supplier = await _supplierRepo.GetByIdAsync(id);
        if (supplier != null)
        {
            supplier.Activate();
            await _supplierRepo.UpdateAsync(supplier);
        }
    }
}