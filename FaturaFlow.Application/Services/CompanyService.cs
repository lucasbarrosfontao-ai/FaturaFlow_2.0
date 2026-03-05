using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Application.Services;

public class CompanyService
{
    private readonly ICompanyRepository _companyRepo;

    public CompanyService(ICompanyRepository companyRepo)
    {
        _companyRepo = companyRepo;
    }

    public async Task<IEnumerable<Company>> GetAllAsync() => await _companyRepo.GetAllAsync();

    public async Task<Company?> GetByIdAsync(Guid id) => await _companyRepo.GetByIdAsync(id);


    public async Task SaveCompanyAsync(Guid? id, string name, string nif, string phone, string email, string address, string city, string zipCode)
    {
        try 
        {
            var personalId = new PersonalId(nif); 
            
            var phoneNum = string.IsNullOrWhiteSpace(phone) ? null : new PhoneNumber(phone);
            var emailAddr = string.IsNullOrWhiteSpace(email) ? null : new EmailAddress(email);
            var postal = string.IsNullOrWhiteSpace(zipCode) ? null : new PostalCode(zipCode);

            if (id.HasValue && id != Guid.Empty)
            {
                var existing = await _companyRepo.GetByIdAsync(id.Value) 
                    ?? throw new Exception("Empresa não encontrada.");
                
                existing.UpdateDetails(name, personalId, phoneNum, emailAddr, address, city, postal);
                
                await _companyRepo.UpdateAsync(existing); 
            }
            else
            {
                var newCompany = new Company(name, personalId, phoneNum, emailAddr, address, city, postal);
                await _companyRepo.AddAsync(newCompany);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message); 
        }
    }
}