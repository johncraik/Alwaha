using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace AlwahaLibrary.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if(string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_config["Email:FromName"], _config["Email:FromAddress"]));
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
            await client.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
            
            if(!string.IsNullOrEmpty(_config["Email:Username"]) && !string.IsNullOrEmpty(_config["Email:Password"]))
                await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
            
        }
        catch (Exception ex)
        {
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