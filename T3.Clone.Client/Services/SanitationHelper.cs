using System.Web;
using Ganss.Xss;

namespace T3.Clone.Client.Services;

public class SanitationHelper
{
    public static string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sanitizer = new HtmlSanitizer();
        
        // Configure the sanitizer to allow only safe HTML tags and attributes used by highlight.js
        sanitizer.AllowedTags.Add("span");
        sanitizer.AllowedTags.Add("code");
        sanitizer.AllowedTags.Add("pre");
        sanitizer.AllowedTags.Add("div");
        sanitizer.AllowedTags.Add("ul");
        sanitizer.AllowedTags.Add("li");
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("style");
        sanitizer.AllowedAttributes.Add("lang");
        sanitizer.AllowedAttributes.Add("data-language");
        sanitizer.AllowedTags.Add("a");
        
        

        // Use a simple regex to remove HTML tags
        return sanitizer.Sanitize(input);
    }
}