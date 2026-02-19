using System.Text;
using System.Text.Json;
using FaturaFlow.Domain.Interfaces;
using FaturaFlow.Domain.ValueObjects;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FaturaFlow.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { 
            HostName = _configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = _configuration["RABBITMQ_USER"] ?? "guest",
            Password = _configuration["RABBITMQ_PASSWORD"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // --- FILA DE FATURAS ---
        await channel.QueueDeclareAsync("faturas_queue", durable: true, exclusive: false, autoDelete: false);
        var faturaConsumer = new AsyncEventingBasicConsumer(channel);
        faturaConsumer.ReceivedAsync += async (model, ea) =>
        {
            try {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<FaturaMsg>(message);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                    var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
                    var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var invoice = await invoiceRepo.GetByIdAsync(data!.Id_Fatura);
                    
                    // Converte string para o Value Object esperado pelo Repository eddd
                    var emailVo = new EmailAddress(data.EmailCliente);
                    var customer = await customerRepo.GetByEmailAsync(emailVo); 

                    if (invoice != null && customer != null)
                    {
                        var pdf = pdfService.GerarFaturaPdf(invoice, customer);
                        
                        // Usa o .Value (ou a propriedade que retorna a string) para o envio do email
                        await emailSender.SendInvoiceEmailAsync(emailVo.Value!, customer.Name, pdf, invoice.InvoiceNumber);
                        
                        _logger.LogInformation("Fatura {num} processada e enviada para {email}.", invoice.InvoiceNumber, emailVo.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Fatura ou Cliente não encontrado para ID: {id}", data.Id_Fatura);
                    }
                }
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Erro ao processar faturas_queue");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        await channel.BasicConsumeAsync("faturas_queue", autoAck: false, consumer: faturaConsumer);


        // --- FILA DE RECUPERAÇÃO ---
        await channel.QueueDeclareAsync("recuperacao_queue", durable: true, exclusive: false, autoDelete: false);
        var recConsumer = new AsyncEventingBasicConsumer(channel);
        recConsumer.ReceivedAsync += async (model, ea) =>
        {
            try {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<RecuperacaoMsg>(message);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                    await emailSender.SendCodePassEmailAsync(data!.Email, data.Codigo);
                    _logger.LogInformation("Código de recuperação enviado para {email}", data.Email);
                }
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Erro ao processar recuperacao_queue");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        await channel.BasicConsumeAsync("recuperacao_queue", autoAck: false, consumer: recConsumer);

        _logger.LogInformation("Worker escutando RabbitMQ...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

// Classes de mapeamento (devem ter o mesmo nome das propriedades que o RabbitMQService envia)
public record FaturaMsg(Guid Id_Fatura, string NomeCliente, string EmailCliente);
public record RecuperacaoMsg(string Email, string Codigo);