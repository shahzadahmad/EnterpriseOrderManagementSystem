
using System.Net;
using System.Net.Mail;
using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseOrderManagementSystem.Infrastructure.Services.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    #region Constructor

    public SmtpEmailService(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;

        ValidateConfiguration();
    }

    #endregion

    #region Send Email

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        cancellationToken.ThrowIfCancellationRequested();

        using var message = new MailMessage
        {
            From = new MailAddress(
                _options.FromEmail,
                _options.FromName),

            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(to));

        using var client = new SmtpClient(
            _options.Host,
            _options.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _options.UserName,
                _options.Password),
            Timeout = 30000
        };

        try
        {
            await client.SendMailAsync(
                message,
                cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {Recipient}.",
                to);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Email delivery was cancelled for {Recipient}.",
                to);

            throw;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(
                ex,
                "SMTP delivery failed for {Recipient}. " +
                "SMTP status code: {StatusCode}.",
                to,
                ex.StatusCode);

            throw;
        }
    }

    #endregion

    #region Configuration Validation

    private void ValidateConfiguration()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            _options.Host);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            _options.UserName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            _options.Password);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            _options.FromEmail);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            _options.Port);

        if (_options.Port > 65535)
        {
            throw new ArgumentOutOfRangeException(
                nameof(_options.Port),
                "SMTP port must not exceed 65535.");
        }
    }

    #endregion
}