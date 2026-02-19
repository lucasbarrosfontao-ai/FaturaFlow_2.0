using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepo;

    public CustomerService(ICustomerRepository customerRepo)
    {
        _customerRepo = customerRepo;
    }

    public async Task<IEnumerable<Customer>> GetAllActiveAsync()
    {
        var all = await _customerRepo.GetAllAsync();
        return all.Where(c => c.IsActive);
    }

    public async Task<Customer?> GetByIdAsync(Guid id) => await _customerRepo.GetByIdAsync(id);

    public async Task SaveCustomerAsync(Guid? id, string name, string nif, string phone, string email, string address, string city, string zipCode)
    {
        try 
        {
            var personalId = new PersonalId(nif);
            var emailAddr = new EmailAddress(email);
            var phoneNum = new PhoneNumber(phone);
            var postal = new PostalCode(zipCode);

            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _customerRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Cliente não encontrado.");
                
                // ATUALIZAÇÃO: Agora chamamos o método da entidade para mudar os dados no objeto
                existing.UpdateDetails(name, personalId, phoneNum, emailAddr, address, city, postal);
                
                await _customerRepo.UpdateAsync(existing); 
            }
            else
            {
                var newCustomer = new Customer(name, personalId, phoneNum, emailAddr, address, city, postal);
                await _customerRepo.AddAsync(newCustomer);
            }
        }
        catch (Exception ex)
        {
            // Captura erros de NIF duplicado ou validações dos Value Objects
            throw new Exception($"Erro ao salvar cliente: {ex.Message}");
        }
    }
    public async Task DeleteAsync(Guid id)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer != null)
        {
            // Em DDD, geralmente fazemos Soft Delete (desativar)
            // customer.Deactivate(); 
            await _customerRepo.UpdateAsync(customer);
            // Ou delete real:
            await _customerRepo.DeleteAsync(id);
        }
    }
    // Adicione estes dois métodos dentro da classe CustomerService
    public async Task<IEnumerable<Customer>> GetInactiveAsync()
    {
        var all = await _customerRepo.GetAllAsync();
        return all.Where(c => !c.IsActive);
    }

    public async Task ActivateAsync(Guid id)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer != null)
        {
            customer.Activate();
            await _customerRepo.UpdateAsync(customer);
        }
    }
}