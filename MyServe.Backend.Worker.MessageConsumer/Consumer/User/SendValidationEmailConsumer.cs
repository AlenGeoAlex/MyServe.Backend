using Flurl;
using MassTransit;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Messages.User;
using MyServe.Backend.Common;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Constants.EmailTemplate;
using MyServe.Backend.Common.Models;
using Serilog;

namespace MyServe.Backend.Worker.MessageConsumer.Consumer.User;

public class SendValidationEmailConsumer(IEmailClient emailClient, ILogger logger) : IConsumer<RequestEmailValidationCommand>
{
    public async Task Consume(ConsumeContext<RequestEmailValidationCommand> context)
    {
        if ((DateTimeOffset.UtcNow - context.Message.CreatedAt).Minutes > AppConfigurations.Auth.OtpValidityDurationInMinutes + 2)
        {
            logger.Information("Ignoring the validation message as the OTP for {User} as has been expired", context.Message.Email);
            return;
        }

        try
        {
            logger.Information("Received command to send email otp email for {User} for {Device}", context.Message.Email, context.Message.Device);
            var requestEmailValidationCommand = context.Message;
            Dictionary<string, string> subjectPlaceholders = new();
            Dictionary<string, string> bodyPlaceholders = new();

            var directLink = new Uri(context.Message.RequestOrigin)
                .AppendPathSegment("callback")
                .AppendPathSegment("otp")
                .SetQueryParam("code", requestEmailValidationCommand.Code)
                .SetQueryParam("expiry", context.Message.CreatedAt.AddMinutes(AppConfigurations.Auth.OtpValidityDurationInMinutes));

            subjectPlaceholders.Add(EmailPlaceholdersConstants.Subject.ValidateOtp.Code, requestEmailValidationCommand.Code);
        
            bodyPlaceholders.Add(EmailPlaceholdersConstants.Body.ValidateOtp.Code, requestEmailValidationCommand.Code);
            bodyPlaceholders.Add(EmailPlaceholdersConstants.Body.ValidateOtp.User, requestEmailValidationCommand.Email);
            bodyPlaceholders.Add(EmailPlaceholdersConstants.Body.ValidateOtp.Device, requestEmailValidationCommand.Device == "WebApp" ? "Browser" : "App");
            bodyPlaceholders.Add(EmailPlaceholdersConstants.Body.ValidateOtp.Url, directLink.ToString());

            var content = await EmailTemplates.ValidateOtp.RenderTemplateAsync(
                subjectPlaceholders,
                bodyPlaceholders
            );

            var emailAddressBook = new EmailAddressBook();
            emailAddressBook.To.Add(EmailAddress.Parse(requestEmailValidationCommand.Email));

            await emailClient.SendAsync(emailAddressBook, content);
        }
        catch (Exception e)
        {
            logger.Error("Failed to send email otp email for {Email}", context.Message.Email);
            logger.Error(e.Message);
        }
    }
}