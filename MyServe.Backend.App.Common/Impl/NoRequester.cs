using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Exceptions;

namespace MyServe.Backend.Common.Impl;

public class NoRequester : IRequester
{
    public Guid UserId => throw new AnonymousUserException();
    public bool IsLoggedIn => false;
    public string EmailAddress => throw new AnonymousUserException();
}