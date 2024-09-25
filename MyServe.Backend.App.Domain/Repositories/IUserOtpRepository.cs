using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Models.User;

namespace MyServe.Backend.App.Domain.Repositories;

public interface IUserOtpRepository : IAppRepository<UserOtp>
{
    Task<bool> ValidateOtp(string otp, string emailAddress, string device);

    Task<(int, bool, Guid)> GetOtpCreationCriteria(string emailAddress);
}