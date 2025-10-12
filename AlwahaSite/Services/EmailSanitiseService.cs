using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace AlwahaSite.Services;

public class EmailSanitiseService
{
    private readonly HtmlSanitizer _sanitiser;
    
    public EmailSanitiseService()
    {
        _sanitiser = new HtmlSanitizer();
        _sanitiser.AllowedSchemes.Clear();
        _sanitiser.AllowedSchemes.Add("http");
        _sanitiser.AllowedSchemes.Add("https");
        _sanitiser.AllowedSchemes.Add("mailto");
        _sanitiser.KeepChildNodes = true;
    }

    public SanitiseMessage SanitiseMessage(string message)
    {
        var norm = Normalise(message);
        var plain = Sanitise(norm);
        
        if(string.IsNullOrEmpty(plain))
            return new SanitiseMessage()
            {
                ErrorMessage = "Message is empty or contains only whitespace"
            };
        
        var redacted = RedactSensitive(plain);
        return new SanitiseMessage(redacted);
    }


    private static string Normalise(string? s)
    {
        s ??= string.Empty;
        s = s.Replace("\r\n", "\n").Replace("\r", "\n");
        s = s.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "");
        s = s.Trim();
        if(s.Length > 20000) s = s[..20000] + "\n[...truncated...]";
        return s;
    }

    private string Sanitise(string input) => _sanitiser.Sanitize(input);
    
    public string HtmlToPlain(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        //Extract content inside <body> if present
        var bodyMatch = Regex.Match(html, @"<body[^>]*>(.*?)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var inner = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;

        //Remove all remaining tags (keep text only)
        var text = Regex.Replace(inner, "<.*?>", string.Empty, RegexOptions.Singleline);

        //Decode HTML entities (&amp; → &, etc.)
        text = WebUtility.HtmlDecode(text);

        //Normalise whitespace
        text = Regex.Replace(text, @"[ \t]+", " ");
        text = Regex.Replace(text, @"\r\n|\n\r|\n|\r", "\r\n");
        text = Regex.Replace(text, @"\r\n{3,}", "\r\n\r\n"); // collapse large gaps

        text = text.Replace("<br>", "\r\n"); // replace <br> with \r\n (if not already present)
        text = text.Replace("Date", "\r\nDate:\r\n");
        text = text.Replace("Party Size", "\r\nParty Size:\r\n");
        text = text.Replace("Message", "\r\nMessage:\r\n");
        
        //Trim
        text = text.Trim();

        return text;
    }
        
    private static string RedactSensitive(string input)
    {
        var s = input;

        // Credit cards (13–19 digits, allowing spaces/dashes)
        s = Regex.Replace(s, @"\b(?:\d[ -]*?){13,19}\b", "[REDACTED CARD]");

        // Password-like disclosures: "password: ..." / "pwd=..."
        s = Regex.Replace(s, @"(?i)\b(pass(word)?|pwd)\s*[:=]\s*\S+", "[REDACTED PASSWORD]");

        // Long tokens / keys (very rough, catches JWT-like strings)
        s = Regex.Replace(s, @"\b[A-Za-z0-9-_]{48,}\b", "[REDACTED TOKEN]");

        // (Optional) UK NI number pattern — enable if relevant:
        s = Regex.Replace(s, @"\b(?!BG|GB|NK|KN|TN|NT|ZZ)[A-CEGHJ-PR-TW-Z]{2}\d{6}[A-D]\b", "[REDACTED NI]",
            RegexOptions.IgnoreCase);

        return s;
    }
}

public class SanitiseMessage
{
    public string? Message { get; }
    
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }

    public SanitiseMessage()
    {
    }
    
    public SanitiseMessage(string message)
    {
        Message = message;
    }
}