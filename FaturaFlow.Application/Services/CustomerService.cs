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
            // 1. Criar os Value Objects (Valida formatos antes de ir ao banco)
            var personalId = new PersonalId(nif);
            var emailAddr = new EmailAddress(email);
            var phoneNum = new PhoneNumber(phone);
            var postal = new PostalCode(zipCode);

            // 2. VERIFICAÇÃO DE DUPLICIDADE (A Correção)
            // Busca no banco se já existe alguém com este NIF
            var customerWithSameNif = await _customerRepo.GetByNifAsync(personalId); 

            if (customerWithSameNif != null)
            {
                // Se estamos editando (tem ID), verificamos se o NIF encontrado pertence a OUTRO cliente
                if (id.HasValue && id != Guid.Empty)
                {
                    if (customerWithSameNif.Id != id.Value)
                    {
                        throw new Exception("O NIPC já existe e pertence a outro cliente.");
                    }
                    // Se o ID for igual, significa que é o próprio cliente mantendo o NIF, então segue o baile.
                }
                else
                {
                    // Se é um cadastro novo e achou o NIF, bloqueia.
                    throw new Exception("O NIPC já existe.");
                }
            }

            // 3. Fluxo de Salvamento Normal
            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _customerRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Cliente não encontrado para edição.");
                
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
            // Se a exceção for a nossa de "NIPC já existe", ela cairá aqui e será repassada
            // Isso impede o congelamento, pois agora o erro é controlado e retorna uma mensagem clara.
            throw new Exception($"Erro ao salvar cliente: {ex.Message}");
        }
    }
    public async Task Deactivate(Guid id)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer != null)
        {
            // Em DDD, geralmente fazemos Soft Delete (desativar)
            customer.Deactivate(); 
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