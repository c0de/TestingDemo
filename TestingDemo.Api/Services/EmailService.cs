using System.Net.Mail;

namespace TestingDemo.Api.Services;

/// <summary>
/// Service for sending emails.
/// </summary>
public class EmailService : IEmailService
{
    /// <summary>
    /// Send Email Message.
    /// </summary>
    /// <param name="mailMessage"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken)
    {
        // TODO: Implement email sending logic here.
        await Task.Delay(1000, cancellationToken);
    }
}
