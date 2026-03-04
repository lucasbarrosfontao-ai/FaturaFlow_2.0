using System.Net;
using System.Net.Mail;
using FaturaFlow.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FaturaFlow.Infrastructure.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    public EmailSender(IConfiguration config) => _config = config;

   public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _config["Email:Host"];
        var port = _config["Email:Port"];
        var user = _config["Email:User"]; 
        var pass = _config["Email:Pass"];

        // No Mailtrap, o remetente pode ser qualquer e-mail que você inventar para teste
        string remetente = "no-reply@faturaflow.com"; 

        using var client = new SmtpClient(host, int.Parse(port ?? "2525"))
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(remetente, to, subject, body) { IsBodyHtml = true };
        
        await client.SendMailAsync(mailMessage);
    }

    public async Task SendInvoiceEmailAsync(string email, string nome, byte[] pdf, string numero, string tipo)
    {
        string subject;
        string body;
        string remetente;
        string nomeArquivo;

        if (tipo == "Fatura")
        {
            subject = $"Fatura Flow - Sua Fatura #{numero}";
            body = $"Olá {nome}, segue em anexo a sua fatura.";
            remetente = "faturas@faturaflow.com";
            nomeArquivo = $"fatura_{numero}.pdf";
        }
        else if (tipo == "Recibo")
        {
            subject = $"Fatura Flow - Seu Recibo #{numero}";
            body = $"Olá {nome}, segue em anexo o seu recibo.";
            remetente = "recibos@faturaflow.com";
            nomeArquivo = $"recibo_{numero}.pdf";
        }
        else
        {
            throw new ArgumentException("Tipo de documento inválido. Use 'Fatura' ou 'Recibo'.");
        }

        // 2. Configuração do Cliente SMTP (Só fazemos uma vez)
        using var client = new SmtpClient(_config["Email:Host"], int.Parse(_config["Email:Port"] ?? "2525"))
        {
            Credentials = new NetworkCredential(_config["Email:User"], _config["Email:Pass"]),
            EnableSsl = true
        };

        // 3. Criação da Mensagem
        using var mailMessage = new MailMessage(remetente, email, subject, body)
        {
            IsBodyHtml = true 
        };

        // 4. Anexar o PDF
        using var ms = new MemoryStream(pdf);
        mailMessage.Attachments.Add(new Attachment(ms, nomeArquivo, "application/pdf"));

        // 5. Enviar
        await client.SendMailAsync(mailMessage);
    }

    public async Task SendCodePassEmailAsync(string email, string token)
    {
        var subject = "FaturaFlow - Recuperação de Senha";
        var body = $@"
            <h2>Recuperação de Senha</h2>
            <p>Você solicitou a alteração da sua senha.</p>
            <p>Seu código de verificação é: <strong>{token}</strong></p>
            <p>Se não foi você, ignore este e-mail.</p>";

        await SendEmailAsync(email, subject, body);
    }
}
