using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class Supplier
    {
        // Propriedades em Inglês e PascalCase
        public Guid Id { get; private set; }
        public string CompanyName { get; private set; }
        public PersonalId? NIPC { get; private set; }
        public string? RepresentativeName { get; private set; }
        public PhoneNumber? Phone { get; private set; }
        public EmailAddress? Email { get; private set; }
        public string? Address { get; private set; }
        public string? City { get; private set; }
        public PostalCode? ZipCode { get; private set; }
        public bool IsActive { get; private set; }
        #pragma warning disable CS8618 
        private Supplier () {}
        #pragma warning restore CS8618
        public Supplier(string name, PersonalId nipc, string representativename, PhoneNumber phone, EmailAddress email, string address, string city, PostalCode zipCode)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("O nome do cliente é obrigatório.");

            Id = Guid.NewGuid();
            CompanyName = name;
            NIPC = nipc;
            RepresentativeName = representativename;
            Phone = phone;
            Email = email;
            Address = address;
            City = city;
            ZipCode = zipCode;
            IsActive = true;
        }

        public void UpdateDetails(string name, PersonalId nipc,string representativename, PhoneNumber phone, EmailAddress email, string address, string city, PostalCode zipCode)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("O nome do cliente é obrigatório.");

            CompanyName = name;
            NIPC = nipc;
            RepresentativeName = representativename;
            Phone = phone;
            Email = email;
            Address = address;
            City = city;
            ZipCode = zipCode;
        }

        // Soft Delete (Desativar)
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