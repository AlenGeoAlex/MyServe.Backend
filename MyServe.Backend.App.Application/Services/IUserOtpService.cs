using MyServe.Backend.App.Application.Dto.Otp;
using MyServe.Backend.App.Domain.Models.User;

namespace MyServe.Backend.App.Application.Services;

public interface IUserOtpService
{
    Task<OtpDto> CreateUserOtpAsync(string emailAddress, string requestOrigin, string? device = null, TimeSpan? expiry = null);
    
    Task<User?> ValidateUserOtpAsync(string emailAddress, string code, string? device = null);
}