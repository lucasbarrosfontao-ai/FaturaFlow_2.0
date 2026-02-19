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
        byte[] GerarFaturaPdf(Invoice invoice, Customer customer);
    }
}
