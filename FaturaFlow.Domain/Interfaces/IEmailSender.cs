using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaturaFlow.Domain.Interfaces
{
    public interface IEmailSender
    {
        Task SendInvoiceEmailAsync(string email, string nome, byte[] pdf, string numeroFatura, string tipo);
        Task SendCodePassEmailAsync(string email, string codigo);
    }
}
