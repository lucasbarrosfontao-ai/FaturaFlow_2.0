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

    public async Task SaveSupplierAsync(Guid? id, string companyName, string nipc, string representativeName, string phone, string email, string address, string city, string zipCode)
    {
        try 
        {
            var nipcVo = new PersonalId(nipc);
            var emailVo = new EmailAddress(email);
            var phoneVo = new PhoneNumber(phone);
            var postalVo = new PostalCode(zipCode);

            var existingSupplierWithNipc = await _supplierRepo.GetByNIPCAsync(nipcVo);

            if (existingSupplierWithNipc != null)
            {
                if (id.HasValue && id != Guid.Empty)
                {
                    if (existingSupplierWithNipc.Id != id.Value)
                    {
                        throw new Exception("O NIPC já existe e pertence a outro fornecedor.");
                    }
                }
                else
                {
                    throw new Exception("O NIPC já existe.");
                }
            }

            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _supplierRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Fornecedor não encontrado.");
                
                existing.UpdateDetails(companyName, nipcVo, representativeName, phoneVo, emailVo, address, city, postalVo);
                await _supplierRepo.UpdateAsync(existing); 
            }
            else
            {
                var newSupplier = new Supplier(companyName, nipcVo, representativeName, phoneVo, emailVo, address, city, postalVo);
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