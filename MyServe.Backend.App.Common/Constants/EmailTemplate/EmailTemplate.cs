using MyServe.Backend.Common.Models;

namespace MyServe.Backend.Common.Constants.EmailTemplate;

public class EmailTemplate(string templateKey, string subject)
{
    
    private static readonly string EmailTemplatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Static" , "EmailTemplates");
    
    private string? _body;
    private string _templateKey = templateKey;

    public async Task<EmailContent> RenderTemplateAsync(
        IReadOnlyDictionary<string, string> subjectPlaceholder,
        IReadOnlyDictionary<string, string> bodyPlaceholder
        )
    {
        if (string.IsNullOrWhiteSpace(_body))
            _body = await GetTemplateAsync();

        var tempBody = _body;
        var tempSubject = subject;
        
        foreach (var (placeholder, replacement) in bodyPlaceholder)
        {
            var tempPlaceholderKey = placeholder;
            if(!tempPlaceholderKey.StartsWith("{{"))
                tempPlaceholderKey = "{{" + placeholder + "}}";
            
            tempBody = tempBody.Replace(tempPlaceholderKey, replacement);
        }
        
        foreach (var (placeholder, replacement) in subjectPlaceholder)
        {
            var tempPlaceholderKey = placeholder;
            if(!tempPlaceholderKey.StartsWith("{{"))
                tempPlaceholderKey = "{{" + placeholder + "}}";
            
            tempSubject = tempSubject.Replace(tempPlaceholderKey, replacement);
        }

        return new EmailContent()
        {
            HtmlBody = tempBody,
            Subject = subject,
        };
    }

    private async Task<string> GetTemplateAsync()
    {
        if(!_templateKey.EndsWith(".html"))
            _templateKey += ".html";
        
        var filePath = Path.Combine(EmailTemplatesPath, _templateKey);
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Template not found", filePath);

        var body = await File.ReadAllTextAsync(filePath);
        if(string.IsNullOrWhiteSpace(body))
            throw new FileNotFoundException("Template body not found", filePath);

        _body = body;
        return _body;
    }
}