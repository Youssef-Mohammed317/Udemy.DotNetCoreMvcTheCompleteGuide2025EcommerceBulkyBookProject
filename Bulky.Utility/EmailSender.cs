using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BulkyBook.Utility
{
    public class BrevoEmailSender : IEmailSender
    {
        private readonly BrevoSettings _settings;
        private readonly HttpClient _httpClient;

        public BrevoEmailSender(IOptions<BrevoSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_settings.BaseAddress);
            _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var body = new
            {
                sender = new
                {
                    email = _settings.SenderEmail,
                    name = _settings.SenderName
                },
                to = new[]
            {
                new { email }
            },
                subject,
                htmlContent = htmlMessage
            };
            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            await _httpClient.PostAsync("", content);
        }
    }
}
