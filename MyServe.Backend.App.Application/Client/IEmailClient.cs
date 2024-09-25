using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Models;

namespace MyServe.Backend.App.Application.Client;

public interface IEmailClient
{
    Task<bool> SendAsync(EmailAddressBook emailAddressBook, EmailContent emailContent);
}