using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using Telehealth.Platform.Application.Abstractions.Notifications;

namespace Telehealth.Platform.Infrastructure.Notifications;

public class SmtpEmailSenderOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public string FromEmail { get; set; } = "noreply@telehealth.eu";
    public string FromName { get; set; } = "Telehealth Platform";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = true;
    public bool Enabled { get; set; } = false;
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailSenderOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _options = configuration.GetSection(SmtpEmailSenderOptions.SectionName)
                                .Get<SmtpEmailSenderOptions>()
                   ?? new SmtpEmailSenderOptions();
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Email disabled. Would send '{Subject}' to {ToEmail}", subject, toEmail);
            return;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = !string.IsNullOrEmpty(_options.Username)
                ? new NetworkCredential(_options.Username, _options.Password)
                : null
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail, toName));

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email '{Subject}' sent to {ToEmail}", subject, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email '{Subject}' to {ToEmail}", subject, toEmail);
            throw;
        }
    }
}
