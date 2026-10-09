using System.Net;
using System.Net.Mail;
using EnterpriseOrderManagementSystem.Application.Abstractions.Services;
using Microsoft.Extensions.Options;

namespace EnterpriseOrderManagementSystem.Infrastructure.Services.Email;

public sealed class SmtpEmailService
    : IEmailService
{
    private readonly EmailOptions _options;

    public SmtpEmailService(
        IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

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

        using var message = new MailMessage();

        message.From = new MailAddress(
            _options.FromEmail,
            _options.FromName);

        message.To.Add(to);

        message.Subject = subject;

        message.Body = body;

        message.IsBodyHtml = true;

        using var client = new SmtpClient(
            _options.Host,
            _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(
                _options.UserName,
                _options.Password)
        };

        cancellationToken.ThrowIfCancellationRequested();

        await client.SendMailAsync(message);
    }

    #endregion
}