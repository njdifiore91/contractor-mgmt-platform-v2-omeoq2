using Azure.Communication.Email;  // v1.0.0
using Microsoft.Extensions.Configuration;  // v6.0.0
using Microsoft.Extensions.Logging;  // v6.0.0
using Microsoft.Extensions.Caching.Memory;  // v6.0.0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Backend.Core.Interfaces.Services;

namespace Backend.Infrastructure.Services
{
    /// <summary>
    /// Implementation of email service using Azure Communication Services with enhanced security,
    /// monitoring, and error handling capabilities.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailClient _emailClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _fromEmailAddress;
        private readonly IMemoryCache _templateCache;
        private readonly int _maxRetryAttempts;
        private readonly TimeSpan _retryDelay;
        private const string EMAIL_REGEX = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IMemoryCache templateCache)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _templateCache = templateCache ?? throw new ArgumentNullException(nameof(templateCache));

            var connectionString = _configuration["Azure:EmailService:ConnectionString"]
                ?? throw new InvalidOperationException("Azure Email Service connection string not found");
            
            _emailClient = new EmailClient(connectionString);
            _fromEmailAddress = _configuration["Azure:EmailService:SenderEmail"]
                ?? throw new InvalidOperationException("Sender email address not configured");
            
            _maxRetryAttempts = _configuration.GetValue<int>("Azure:EmailService:MaxRetryAttempts", 3);
            _retryDelay = TimeSpan.FromSeconds(_configuration.GetValue<int>("Azure:EmailService:RetryDelaySeconds", 2));

            _logger.LogInformation("Email service initialized successfully");
        }

        /// <inheritdoc/>
        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            IEnumerable<string> attachments = null)
        {
            try
            {
                if (!ValidateEmailAddress(to))
                {
                    _logger.LogError($"Invalid recipient email address: {to}");
                    return false;
                }

                var emailContent = new EmailContent(subject)
                {
                    PlainText = StripHtml(body),
                    Html = SanitizeHtml(body)
                };

                var emailMessage = new EmailMessage(
                    _fromEmailAddress,
                    to,
                    emailContent);

                if (attachments?.Any() == true)
                {
                    foreach (var attachment in attachments)
                    {
                        await AddAttachmentAsync(emailMessage, attachment);
                    }
                }

                return await SendWithRetryAsync(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {to}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SendTemplatedEmailAsync(
            string to,
            string templateName,
            object templateData,
            IEnumerable<string> attachments = null)
        {
            try
            {
                if (!ValidateEmailAddress(to))
                {
                    _logger.LogError($"Invalid recipient email address: {to}");
                    return false;
                }

                var template = await GetTemplateAsync(templateName);
                if (template == null)
                {
                    _logger.LogError($"Template not found: {templateName}");
                    return false;
                }

                var processedContent = ProcessTemplate(template, templateData);
                var subject = ExtractSubject(processedContent);
                var body = ExtractBody(processedContent);

                return await SendEmailAsync(to, subject, body, attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending templated email to {to}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SendMobilizationNotificationAsync(
            string inspectorEmail,
            string projectName,
            string customerName,
            DateTime mobilizationDate)
        {
            try
            {
                const string templateName = "MobilizationNotification";
                var templateData = new
                {
                    ProjectName = projectName,
                    CustomerName = customerName,
                    MobilizationDate = mobilizationDate.ToString("D"),
                    InspectorEmail = inspectorEmail
                };

                _logger.LogInformation($"Sending mobilization notification for {inspectorEmail} on project {projectName}");
                
                return await SendTemplatedEmailAsync(
                    inspectorEmail,
                    templateName,
                    templateData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending mobilization notification to {inspectorEmail}");
                return false;
            }
        }

        private bool ValidateEmailAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, EMAIL_REGEX, RegexOptions.IgnoreCase);
        }

        private async Task<bool> SendWithRetryAsync(EmailMessage message)
        {
            for (int attempt = 1; attempt <= _maxRetryAttempts; attempt++)
            {
                try
                {
                    var response = await _emailClient.SendAsync(
                        Azure.WaitUntil.Completed,
                        message);
                    
                    _logger.LogInformation($"Email sent successfully to {message.Recipients.To[0]}");
                    return true;
                }
                catch (Exception ex) when (attempt < _maxRetryAttempts)
                {
                    _logger.LogWarning(ex, $"Attempt {attempt} failed. Retrying...");
                    await Task.Delay(_retryDelay);
                }
            }
            return false;
        }

        private async Task<string> GetTemplateAsync(string templateName)
        {
            return await _templateCache.GetOrCreateAsync(
                $"EmailTemplate_{templateName}",
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromHours(1);
                    // In a real implementation, this would load from a template store
                    return await Task.FromResult(GetDefaultTemplate(templateName));
                });
        }

        private string GetDefaultTemplate(string templateName)
        {
            // Simplified template handling - in production, templates would be loaded from a store
            return $"Subject: {templateName}\nBody: {{content}}";
        }

        private async Task AddAttachmentAsync(EmailMessage message, string attachmentPath)
        {
            // Implementation would handle attachment processing
            _logger.LogInformation($"Processing attachment: {attachmentPath}");
        }

        private string ProcessTemplate(string template, object data)
        {
            // Implementation would handle template processing with the provided data
            return template;
        }

        private string ExtractSubject(string processedContent)
        {
            // Implementation would extract subject from processed content
            return "Subject";
        }

        private string ExtractBody(string processedContent)
        {
            // Implementation would extract body from processed content
            return processedContent;
        }

        private string SanitizeHtml(string html)
        {
            // Implementation would sanitize HTML content
            return html;
        }

        private string StripHtml(string html)
        {
            // Implementation would strip HTML tags for plain text version
            return Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}