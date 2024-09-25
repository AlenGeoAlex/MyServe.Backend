using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Models;
using Serilog;

namespace MyServe.Backend.Worker.Infrastructure.Services;

public class UnknownEmailClient(ILogger logger) : IEmailClient
{
    public Task<bool> SendAsync(EmailAddressBook emailAddressBook, EmailContent emailContent)
    {
        logger.Information("Sending email content to {@EmailAddressBook} has been ignored as no valid email client has been configured",
            emailAddressBook.To.FirstOrDefault(EmailAddress.Empty));
        return Task.FromResult(false);
    }
}