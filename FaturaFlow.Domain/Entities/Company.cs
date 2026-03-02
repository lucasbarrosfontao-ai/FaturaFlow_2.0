using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class Company
    {
        public Guid Id { get; private set; }
        
        public string Name { get; private set; }
        public PersonalId NIF { get; private set; }
        public PhoneNumber? Phone { get; private set; }
        public EmailAddress? Email { get; private set; }
        public string? Address { get; private set; }
        public string? City { get; private set; }
        public PostalCode? ZipCode { get; private set; }

        #pragma warning disable CS8618 
        private Company() { }
        #pragma warning restore CS8618

        public Company(string name, PersonalId nif, PhoneNumber? phone, EmailAddress? email, string? address, string? city, PostalCode? zipCode)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("O nome da empresa é obrigatório.");

            Id = Guid.NewGuid();
            Name = name;
            NIF = nif;
            Phone = phone;
            Email = email;
            Address = address;
            City = city;
            ZipCode = zipCode;
        }

        public void UpdateDetails(string name, PersonalId nif, PhoneNumber? phone, EmailAddress? email, string? address, string? city, PostalCode? zipCode)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("O nome da empresa é obrigatório.");

            Name = name;
            NIF = nif;
            Phone = phone;
            Email = email;
            Address = address;
            City = city;
            ZipCode = zipCode;
        }
    }
}