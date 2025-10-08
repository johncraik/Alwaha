using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlwahaSite.Pages;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SitemapIgnoreAttribute : Attribute { }

[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
[SitemapIgnore]
public class SiteMapModel : PageModel
{
    private readonly EndpointDataSource _endpointDataSource;

    public SiteMapModel(EndpointDataSource endpointDataSource)
    {
        _endpointDataSource = endpointDataSource;
    }
    
    public IActionResult OnGet()
    {
        Response.ContentType = "application/xml";
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var today = DateTime.UtcNow.Date;

        // Add static routes
        var pageRoutes = _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<PageActionDescriptor>() != null)
            .Where(e => e.Metadata.GetMetadata<SitemapIgnoreAttribute>() is null)
            .Where(e =>
            {
                var http = e.Metadata.GetMetadata<IHttpMethodMetadata>();
                return http == null || http.HttpMethods.Contains("GET", StringComparer.OrdinalIgnoreCase);
            })
            .Select(e => e.RoutePattern.RawText)
            .Where(path =>
                !string.IsNullOrWhiteSpace(path) &&
                !path.StartsWith("/api") &&          // skip APIs
                !path.StartsWith("/_") &&            // skip partials, etc.
                !path.Contains("{"))                 // skip parameterised routes
            .Distinct()
            .ToList();

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var xml = new XDocument(
            new XElement(ns + "urlset",
                pageRoutes.Select(path =>
                    new XElement(ns + "url",
                        new XElement(ns + "loc", $"{baseUrl}{NormalizePath(path)}"),
                        new XElement(ns + "lastmod", today.ToString("yyyy-MM-dd")),
                        new XElement(ns + "changefreq", "weekly"),
                        new XElement(ns + "priority", NormalizePath(path) == "/" ? "1.0" : "0.8")
                    )
                )
            )
        );

        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }
    
    private static string NormalizePath(string? p)
    {
        if (string.IsNullOrWhiteSpace(p)) return "/";
        if (!p.StartsWith("/")) p = "/" + p;
        if (p.Equals("/Index", StringComparison.OrdinalIgnoreCase)) return "/";
        if (p.EndsWith("/Index", StringComparison.OrdinalIgnoreCase)) return p[..^6];
        return p;
    }
}