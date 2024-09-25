using MyServe.Backend.Common.Abstract;

namespace MyServe.Backend.Common.Impl;

public class UserRequester(Guid userId, string emailAddress) : IRequester
{
    public Guid UserId => userId;
    public bool IsLoggedIn => true;
    public string EmailAddress => emailAddress;
}