using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Models;
using Serilog;

namespace MyServe.Backend.Worker.Infrastructure.Services;

public class SmtpEmailClient(ISecretClient secretClient, ILogger logger, IConfiguration configuration) : IEmailClient
{

    private static string? _hostAddress;
    private static int? _port;
    private static string? _username;
    private static string? _password;
    
    public async Task<bool> SendAsync(EmailAddressBook emailAddressBook, EmailContent emailContent)
    {
        MimeMessage message = new();

        foreach (var (email, name) in emailAddressBook.To)
        {
            message.To.Add(new MailboxAddress(name ?? string.Empty, email));
        }
        
        foreach (var (email, name) in emailAddressBook.Cc)
        {
            message.Cc.Add(new MailboxAddress(name ?? string.Empty, email));
        }
        
        foreach (var (email, name) in emailAddressBook.Bcc)
        {
            message.Bcc.Add(new MailboxAddress(name ?? string.Empty, email));
        }
        
        var (replyToEmailAddress, replyToName) = emailAddressBook.ReplyTo.FirstOrDefault(EmailAddress.Empty);
        if(!string.IsNullOrWhiteSpace(replyToEmailAddress))
            message.ReplyTo.Add(new MailboxAddress(replyToName ?? string.Empty, replyToEmailAddress));

        var fromName = configuration["Smtp:From:Name"] ?? string.Empty;
        var fromAddress = configuration["Smtp:From:Address"] ?? $"{MyServConstants.Project.ProjectName}@test.com";

        message.Body = new BodyBuilder
        {
            HtmlBody = emailContent.HtmlBody
        }.ToMessageBody();
        
        message.From.Add(new MailboxAddress(fromName, fromAddress)); 
        message.Subject = emailContent.Subject;


        if (string.IsNullOrWhiteSpace(_hostAddress) || !_port.HasValue)
        {
            List<Task<string>> tasks =
            [
                secretClient.GetSecretAsync(VaultConstants.Smtp.Host),
                secretClient.GetSecretAsync(VaultConstants.Smtp.Port),
                secretClient.GetSecretAsync(VaultConstants.Smtp.Username),
                secretClient.GetSecretAsync(VaultConstants.Smtp.Password),
            ];

            var secrets = await Task.WhenAll(tasks);
            _hostAddress = secrets[0];
            var portString = secrets[1];
            _username = secrets[2];
            _password = secrets[3];

            if (!int.TryParse(portString, out var port))
            {
                port = 587;
            }
            
            _port = port;
        }
        
        using var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync(_hostAddress, _port.Value, SecureSocketOptions.Auto);
            logger.Information($"Connected to smtp://{_hostAddress}:{_port.Value}");
            if (!string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password))
            {
                logger.Information($"Trying authentication smtp:/{_hostAddress}:{_port.Value}");
                await smtpClient.AuthenticateAsync(_username, _password);
                logger.Information($"Authentication Successful");
            }
            
            await smtpClient.SendAsync(message);
            logger.Information($"Sending email completed");
            return true;
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to send email");
            return false;
        };
    }
    
}