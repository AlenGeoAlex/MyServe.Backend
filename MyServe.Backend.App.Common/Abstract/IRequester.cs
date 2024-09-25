namespace MyServe.Backend.Common.Abstract;

public interface IRequester
{
    /**
     * Unique id of the user
     */
    Guid UserId { get; }
    
    /**
     * Is user a logged in user
     */
    bool IsLoggedIn { get; }
    
    /**
     * Email Address of the user
     */
    string EmailAddress { get; }
}