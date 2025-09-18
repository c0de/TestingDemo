// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Mail;

namespace TestingDemo.Api;

/// <summary>
/// Interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send Email Message
    /// </summary>
    /// <param name="mailMessage"></param>
    /// <param name="cancellationToken"></param>
    Task SendEmailAsync(MailMessage mailMessage, CancellationToken cancellationToken);
}
