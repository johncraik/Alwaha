using System.Net.Mail;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MimeKit;

namespace AlwahaLibrary.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;
    
    private readonly IConfidentialClientApplication _msalClient;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
        
        _msalClient = ConfidentialClientApplicationBuilder
            .Create(_config["Email:ClientId"])
            .WithClientSecret(_config["Email:ClientSecret"])
            .WithTenantId(_config["Email:TenantId"])
            .Build();
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if(string.IsNullOrWhiteSpace(email))
        {
            _logger.LogError("Attempted to send email with null or empty recipient address");
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        try
        {
            var from = _config["Email:FromAddress"];
            
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(from));
            msg.To.Add(MailboxAddress.Parse(email));
            msg.Subject = string.IsNullOrEmpty(subject) ? "(No Subject)" : subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage ?? string.Empty,
                TextBody = string.IsNullOrEmpty(htmlMessage)
                    ? string.Empty
                    : HtmlToPlain(htmlMessage)
            };
            msg.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Timeout = 30_000;
            
            await client.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]!),
                SecureSocketOptions.StartTls);
            
            var tokenResult = await _msalClient
                .AcquireTokenForClient(["https://outlook.office365.com/.default"])
                .ExecuteAsync();

            var oauth2 = new SaslMechanismOAuth2(from, tokenResult.AccessToken);
            
            await client.AuthenticateAsync(oauth2);

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject: {Subject}", email, subject);
            throw new Exception("Error sending email", ex);
        }
    }
    
    private static string HtmlToPlain(string html)
    {
        // ultra-simple HTML → text: strip tags & decode entities
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(text).Trim();
    }
}