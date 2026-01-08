using PushEmailEzd.Models;

namespace PushEmailEzd.Services;

/// <summary>
/// Service for retrieving emails from IMAP mailbox
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Fetch recent emails from the inbox
    /// </summary>
    /// <param name="count">Maximum number of emails to fetch</param>
    /// <returns>List of email messages</returns>
    Task<List<EmailMessage>> GetEmailsAsync(int count = 50);
    
    /// <summary>
    /// Delete an email from the mailbox
    /// </summary>
    /// <param name="uniqueId">Email unique ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteEmailAsync(string uniqueId);
    
    /// <summary>
    /// Check if IMAP connection is properly configured
    /// </summary>
    bool IsConfigured { get; }
}

