using System.Text;
using System.Text.Json;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MedicineReminder.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var rabbitConnectionString = _configuration.GetConnectionString("RabbitMQ") ?? "amqp://localhost";
            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitConnectionString)
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var queueName = "email_queue";
            await channel.QueueDeclareAsync(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = new EmailQueueMessage
            {
                To = to,
                Subject = subject,
                Body = body
            };

            var json = JsonSerializer.Serialize(message);
            var bodyBytes = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(exchange: string.Empty,
                                 routingKey: queueName,
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: bodyBytes);

            _logger.LogInformation("Email message for {To} published to RabbitMQ queue {QueueName}.", to, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish email message to RabbitMQ for {To}.", to);
            // In a real application, you might want to fallback to immediate sending or store in DB
            throw;
        }
    }
}
