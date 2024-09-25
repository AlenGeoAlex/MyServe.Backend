namespace MyServe.Backend.Common.Models;

public class EmailAddressBook
{
    private readonly List<EmailAddress> _to = new();
    
    private readonly List<EmailAddress> _cc = new();
    
    private readonly List<EmailAddress> _bcc = new();
    
    private readonly List<EmailAddress> _replyTo = new();

    public EmailAddressBook()
    {
    }

    public List<EmailAddress> To => _to;
    public List<EmailAddress> Cc => _cc;
    public List<EmailAddress> Bcc => _bcc;
    public List<EmailAddress> ReplyTo => _replyTo;
}