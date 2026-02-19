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
    var user = _config["Email:User"]; // Este é o ID do Mailtrap (0a526...)
    var pass = _config["Email:Pass"];

    // No Mailtrap, o remetente pode ser qualquer e-mail que você inventar para teste
    string remetente = "no-reply@faturaflow.com"; 

    using var client = new SmtpClient(host, int.Parse(port ?? "2525"))
    {
        // Aqui você usa o ID e Senha do Mailtrap para autenticar
        Credentials = new NetworkCredential(user, pass),
        EnableSsl = true
    };

    // AQUI ESTAVA O ERRO: Use a variável 'remetente' em vez de 'user'
    var mailMessage = new MailMessage(remetente, to, subject, body) { IsBodyHtml = true };
    
    await client.SendMailAsync(mailMessage);
}

public async Task SendInvoiceEmailAsync(string email, string nome, byte[] pdf, string numero)
{
    var subject = $"Fatura Flow - Sua Fatura #{numero}";
    var body = $"Olá {nome}, segue em anexo a sua fatura.";
    string remetente = "faturas@faturaflow.com"; // E-mail fictício

    using var client = new SmtpClient(_config["Email:Host"], int.Parse(_config["Email:Port"] ?? "2525"))
    {
        Credentials = new NetworkCredential(_config["Email:User"], _config["Email:Pass"]),
        EnableSsl = true
    };

    var mailMessage = new MailMessage(remetente, email, subject, body);
    
    using var ms = new MemoryStream(pdf);
    mailMessage.Attachments.Add(new Attachment(ms, $"fatura_{numero}.pdf", "application/pdf"));

    await client.SendMailAsync(mailMessage);
}

    // IMPLEMENTAÇÃO: Envio de Código de Recuperação
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
