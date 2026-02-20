using MedicineReminder.Application.Common.Interfaces;
using System.Text;
using System.Text.Json;
using MedicineReminder.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MedicineReminder.Infrastructure.Services;

public class EmailBackgroundWorker : BackgroundService
{
    private readonly ILogger<EmailBackgroundWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ISmtpEmailSender _emailSender;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _queueName;

    public EmailBackgroundWorker(
        ILogger<EmailBackgroundWorker> logger,
        IConfiguration configuration,
        ISmtpEmailSender emailSender)
    {
        _logger = logger;
        _configuration = configuration;
        _emailSender = emailSender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitConnectionString = _configuration.GetConnectionString("RabbitMQ") ?? "amqp://localhost";
        var factory = new ConnectionFactory
        {
            Uri = new Uri(rabbitConnectionString)
        };

        _queueName = "email_queue";

        // Retry connection to RabbitMQ if it's not ready
        var connectionPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => _logger.LogWarning(ex, "Could not connect to RabbitMQ. Retrying in {Time}...", time));

        await connectionPolicy.ExecuteAsync(async () =>
        {
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null,
                                 cancellationToken: stoppingToken);

            _logger.LogInformation("Connected to RabbitMQ and listening on queue: {QueueName}", _queueName);
        });

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var emailRequest = JsonSerializer.Deserialize<EmailQueueMessage>(message);

            if (emailRequest != null)
            {
                await ProcessEmailWithRetry(emailRequest, ea.DeliveryTag);
            }
        };

        await _channel!.BasicConsumeAsync(queue: _queueName,
                             autoAck: false,
                             consumer: consumer,
                             cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessEmailWithRetry(EmailQueueMessage emailRequest, ulong deliveryTag)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time, retryCount, context) =>
                {
                    _logger.LogWarning(ex, "Failed to send email to {To}. Attempt {RetryCount}. Retrying in {Time}...",
                        emailRequest.To, retryCount, time);
                });

        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                await _emailSender.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);
            });

            await _channel!.BasicAckAsync(deliveryTag, multiple: false);
            _logger.LogInformation("Successfully processed and sent email to {To}.", emailRequest.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exhausted retries for sending email to {To}. Dead lettering or dropping message.", emailRequest.To);
            await _channel!.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null && _channel.IsOpen) await _channel.CloseAsync(cancellationToken: cancellationToken);
        if (_connection != null && _connection.IsOpen) await _connection.CloseAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
