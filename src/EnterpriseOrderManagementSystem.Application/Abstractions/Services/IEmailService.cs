namespace EnterpriseOrderManagementSystem.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}