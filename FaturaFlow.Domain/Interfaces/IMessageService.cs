using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaturaFlow.Domain.Interfaces
{
    public interface IMessageService
    {
        Task SendInvoiceMessageAsync(Guid invoiceId, string customerName, string customerEmail);
        Task SendPasswordRecoveryAsync(string email, string recoveryCode);
    }
}
