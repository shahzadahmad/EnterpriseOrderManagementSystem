namespace EnterpriseOrderManagementSystem.Infrastructure.Services.Email;

public sealed class EmailOptions
{
    #region SMTP Settings

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; }

    #endregion

    #region Sender

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    #endregion
}