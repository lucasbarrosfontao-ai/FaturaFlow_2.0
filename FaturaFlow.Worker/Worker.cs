using System.Text;
using System.Text.Json;
using FaturaFlow.Domain.Entities;
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
        Console.WriteLine("Worker iniciado, aguardando mensagens...");
        var factory = new ConnectionFactory() { 
            HostName = _configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = _configuration["RABBITMQ_USER"] ?? "guest",
            Password = _configuration["RABBITMQ_PASSWORD"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // --- FILA DE FATURAS/RECIBOS ---
        await channel.QueueDeclareAsync("faturas_queue", durable: true, exclusive: false, autoDelete: false);
        var faturaConsumer = new AsyncEventingBasicConsumer(channel);
        faturaConsumer.ReceivedAsync += async (model, ea) =>
        {
            _logger.LogInformation("Mensagem recebida na faturas_queue, processando...");
            
            try 
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<FaturaMsg>(message);

                if (data == null)
                {
                    _logger.LogWarning("Mensagem inválida recebida (corpo nulo).");
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                    var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
                    var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var invoice = await invoiceRepo.GetByIdAsync(data.Id_Fatura);
                    var emailVo = new EmailAddress(data.EmailCliente);
                    var customer = await customerRepo.GetByEmailAsync(emailVo); 

                    if (invoice == null || customer == null)
                    {
                        _logger.LogWarning("Fatura ({idF}) ou Cliente ({email}) não encontrados. Removendo da fila.", data.Id_Fatura, data.EmailCliente);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    byte[] pdfBytes;
                    string tipoDoc;

                    if (invoice.Status == "Emitida")
                    {
                        pdfBytes = await pdfService.GerarFaturaPdfAsync(invoice, customer);
                        tipoDoc = "Fatura";
                    }
                    else if (invoice.Status == "Paga")
                    {
                        pdfBytes = await pdfService.GerarReciboPdfAsync(invoice, customer);
                        tipoDoc = "Recibo";
                    }
                    else
                    {
                        _logger.LogWarning("Fatura {num} com status '{status}' ignorada.", invoice.InvoiceNumber, invoice.Status);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    await emailSender.SendInvoiceEmailAsync(emailVo.Value!, customer.Name, pdfBytes, invoice.InvoiceNumber, tipoDoc);
                    
                    _logger.LogInformation("{tipo} {num} enviada com sucesso para {email}.", tipoDoc, invoice.InvoiceNumber, emailVo.Value);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Erro crítico ao processar faturas_queue. A mensagem voltará para a fila: erro :{message}", ex.Message);
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        await channel.BasicConsumeAsync("faturas_queue", autoAck: false, consumer: faturaConsumer);


        // --- FILA DE RECUPERAÇÃO ---
        await channel.QueueDeclareAsync("recuperacao_queue", durable: true, exclusive: false, autoDelete: false);
        var recConsumer = new AsyncEventingBasicConsumer(channel);
        recConsumer.ReceivedAsync += async (model, ea) =>
        {
            Console.WriteLine("Mensagem recebida na fila de recuperacao de palavra-passe, processando...");
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

                _logger.LogInformation("Worker escutando RabbitMQ");
        

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

public record FaturaMsg(Guid Id_Fatura, string NomeCliente, string EmailCliente);
public record RecuperacaoMsg(string Email, string Codigo);