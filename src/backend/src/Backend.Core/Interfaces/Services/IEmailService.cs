using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Core.Interfaces.Services
{
    /// <summary>
    /// Defines the contract for email service operations including general emails, 
    /// templated messages, and specialized notifications.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously with support for multiple attachments and rich HTML content.
        /// </summary>
        /// <param name="to">The recipient's email address</param>
        /// <param name="subject">The email subject line</param>
        /// <param name="body">The HTML-formatted email body content</param>
        /// <param name="attachments">Optional collection of file paths for attachments</param>
        /// <returns>True if the email was sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            IEnumerable<string> attachments = null);

        /// <summary>
        /// Sends an email using a predefined template with dynamic data substitution and attachment support.
        /// </summary>
        /// <param name="to">The recipient's email address</param>
        /// <param name="templateName">The name of the email template to use</param>
        /// <param name="templateData">Object containing data to populate the template</param>
        /// <param name="attachments">Optional collection of file paths for attachments</param>
        /// <returns>True if the templated email was sent successfully, false otherwise</returns>
        Task<bool> SendTemplatedEmailAsync(
            string to,
            string templateName,
            object templateData,
            IEnumerable<string> attachments = null);

        /// <summary>
        /// Sends a specialized notification email when an inspector is mobilized for a project.
        /// </summary>
        /// <param name="inspectorEmail">The inspector's email address</param>
        /// <param name="projectName">The name of the project the inspector is mobilized for</param>
        /// <param name="customerName">The name of the customer associated with the project</param>
        /// <param name="mobilizationDate">The date when the inspector will be mobilized</param>
        /// <returns>True if the mobilization notification was sent successfully, false otherwise</returns>
        Task<bool> SendMobilizationNotificationAsync(
            string inspectorEmail,
            string projectName,
            string customerName,
            DateTime mobilizationDate);
    }
}