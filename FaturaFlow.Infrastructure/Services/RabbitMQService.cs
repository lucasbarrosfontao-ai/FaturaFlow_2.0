using System.Text;
using System.Text.Json;
using FaturaFlow.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace FaturaFlow.Infrastructure.Services;

public class RabbitMQService : IMessageService
{
    private readonly IConfiguration _configuration;

    public RabbitMQService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendInvoiceMessageAsync(Guid invoiceId, string customerName, string customerEmail)
    {
        var factory = new ConnectionFactory() 
        { 
            HostName = _configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = _configuration["RABBITMQ_USER"] ?? "guest",
            Password = _configuration["RABBITMQ_PASSWORD"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "faturas_queue", durable: true, exclusive: false, autoDelete: false);

        var message = new { Id_Fatura = invoiceId, NomeCliente = customerName, EmailCliente = customerEmail };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "faturas_queue", body: body);
    }

    public async Task SendPasswordRecoveryAsync(string email, string recoveryCode)
    {
        var factory = new ConnectionFactory() 
        { 
            HostName = _configuration["RABBITMQ_HOST"] ?? "localhost",
            UserName = _configuration["RABBITMQ_USER"] ?? "guest",
            Password = _configuration["RABBITMQ_PASSWORD"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: "recuperacao_queue", durable: true, exclusive: false, autoDelete: false);

        var message = new { Email = email, Codigo = recoveryCode };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "recuperacao_queue", body: body);
    }
}