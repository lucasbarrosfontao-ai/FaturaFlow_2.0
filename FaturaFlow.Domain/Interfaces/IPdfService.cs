using FaturaFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaturaFlow.Domain.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GerarFaturaPdfAsync(Invoice invoice, Customer customer);
        Task<byte[]> GerarReciboPdfAsync(Invoice invoice, Customer customer);
    }
}
