using System.Net.Http.Headers;
using System.Text;
using LLMLab.Server.Data;
using Microsoft.AspNetCore.Identity;

namespace LLMLab.Server.Components.Account;
// Your ApplicationUser

public class MailgunEmailSender : IEmailSender<ApplicationUser> {
    private readonly string _apiKey;
    private readonly string _domain;
    private readonly string _fromAddress;
    private readonly HttpClient _httpClient;

    public MailgunEmailSender(string apiKey, string domain, string fromAddress)
    {
        _apiKey = apiKey;
        _domain = domain;
        _fromAddress = fromAddress;
        _httpClient = new HttpClient();
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.eu.mailgun.net/v3/{_domain}/messages");
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_apiKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        var content = new MultipartFormDataContent
        {
            { new StringContent(_fromAddress), "from" },
            { new StringContent(email), "to" },
            { new StringContent(subject), "subject" },
            { new StringContent(htmlMessage), "html" }
        };

        request.Content = content;
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
