using MedicineReminder.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;

namespace MedicineReminder.Infrastructure.Services;

public interface ISmtpEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class SmtpEmailSender : ISmtpEmailSender
{
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(ILogger<SmtpEmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var senderName = smtpSettings.GetValue<string>("SenderName") ?? "MedicineReminder";
        var senderEmail = smtpSettings.GetValue<string>("SenderEmail") ?? "noreply@medicinereminder.com";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();

        var server = smtpSettings.GetValue<string>("Server") ?? throw new InvalidOperationException("SMTP Server not configured.");
        var port = smtpSettings.GetValue<int>("Port");
        var username = smtpSettings.GetValue<string>("Username") ?? throw new InvalidOperationException("SMTP Username not configured.");
        var password = smtpSettings.GetValue<string>("Password") ?? throw new InvalidOperationException("SMTP Password not configured.");

        await client.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
        _logger.LogInformation("Email sent to {To} successfully.", to);
    }
}
