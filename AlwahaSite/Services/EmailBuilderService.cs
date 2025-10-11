using System.Net;
using System.Text;
using AlwahaLibrary.Services;
using AlwahaSite.Models;
using Ganss.Xss;

namespace AlwahaSite.Services;

public class EmailBuilderService
{
    private readonly EmailSanitiseService _sanitiseService;
    private readonly SettingsService _settings;

    public EmailBuilderService(EmailSanitiseService sanitiseService,
        SettingsService settings)
    {
        _sanitiseService = sanitiseService;
        _settings = settings;
    }

    public async Task<BuildResponse> Build(ContactForm form, bool isEvent = false)
    {
        var s = _sanitiseService.SanitiseMessage(form.Message);
        if(!s.IsSuccess) 
            return new BuildResponse
            {
                ErrorMessage = s.ErrorMessage
            };
        
        var subject = string.IsNullOrEmpty(form.Subject) ? form is EventForm e ? $"Event | {e.PartySize} people | {e.Date.ToLocalTime():d}" : "(No Subject)" : form.Subject.Trim();
        
        var html = new StringBuilder()
            .AppendLine("<!doctype html><html><head><meta charset='utf-8'>")
            .AppendLine(
                "<style>body{font-family:Segoe UI,Roboto,Arial,sans-serif;background:#f6f7fb;color:#111;padding:20px}")
            .AppendLine(
                ".wrap{max-width:700px;margin:auto;background:#fff;border:1px solid #e5e7eb;border-radius:12px;overflow:hidden}")
            .AppendLine(".hdr{background:#0f172a;color:#fff;padding:16px 20px;font-weight:600}")
            .AppendLine(
                ".sec{padding:20px}.row{margin-bottom:12px}.label{color:#6b7280;font-size:12px;text-transform:uppercase}</style>")
            .AppendLine("</head><body><div class='wrap'>")
            .AppendLine($"<div class='hdr'>{WebUtility.HtmlEncode(subject)}</div>")
            .AppendLine("<div class='sec'>");
        
        // From row with replyable link
        html.AppendLine("<div class='row'><div class='label'>From</div><div>");
        html.AppendLine($"<a href=\"[MAIL__TO]\">{WebUtility.HtmlEncode(form.Email)}</a>");
        html.AppendLine("</div></div>");

        if (form is EventForm ef && isEvent)
        {
            html.AppendLine($"<div class='row'><div class='label'>Party Size</div><div>{ef.PartySize}</div></div>")
                .AppendLine($"<div class='row'><div class='label'>Date</div><div>{ef.Date:yyyy-MM-dd}</div></div>");
        }

        html.AppendLine($"<div class='row'><div class='label'>Message</div><div>{WebUtility.HtmlEncode(form.Message).Replace("\n", "<br/>")}</div></div>")
            .AppendLine("</div></div></body></html>");

        var mailto = BuildMailto(form.Email, subject, html.ToString());
        html.Replace("[MAIL__TO]", mailto);
        
        var sendTo = await _settings.GetSettingAsync<string>(isEvent ? "EventsMailbox" : "InfoMailbox");
        if (string.IsNullOrEmpty(sendTo) || !sendTo.Contains('@'))
            return new BuildResponse
            {
                ErrorMessage = "No email address configured for this type of message"
            };
        
        return new BuildResponse(sendTo, subject, html.ToString());
    }
    
    private string BuildMailto(string toEmail, string originalSubject, string message)
    {
        var subj = $"Re: {originalSubject}";
        var msg = _sanitiseService.HtmlToPlain(message);
        
        var youWrote = $"\r\n\r\nYou wrote:\r\n\r\n{(msg.Replace("href=\"[MAIL__TO]\"", ""))}";

        return "mailto:" + Uri.EscapeDataString(toEmail)
                         + "?subject=" + Uri.EscapeDataString(subj)
                         + "&body=" + Uri.EscapeDataString(youWrote);
    }
}

public class BuildResponse
{
    public string? SendTo { get; }
    public string? Subject { get; }
    public string? Body { get; }

    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }

    public BuildResponse()
    {
    }

    public BuildResponse(string sendTo, string subject, string body)
    {
        SendTo = sendTo;
        Subject = subject;
        Body = body;
    }
}