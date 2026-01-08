using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Options;
using MimeKit;
using PushEmailEzd.Models;

namespace PushEmailEzd.Services;

/// <summary>
/// Email service implementation using MailKit for IMAP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool IsConfigured => 
        !string.IsNullOrEmpty(_settings.ImapServer) && 
        !string.IsNullOrEmpty(_settings.Username) &&
        !string.IsNullOrEmpty(_settings.Password);

    public async Task<List<EmailMessage>> GetEmailsAsync(int count = 50)
    {
        var emails = new List<EmailMessage>();

        if (!IsConfigured)
        {
            _logger.LogWarning("Email service is not configured. Please set IMAP settings in appsettings.json");
            return emails;
        }

        try
        {
            using var client = new ImapClient();
            
            // Bypass SSL certificate validation (for test environments with self-signed certs)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            
            // Connect to IMAP server
            await client.ConnectAsync(_settings.ImapServer, _settings.Port, _settings.UseSsl);
            
            // Authenticate
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            
            // Open inbox
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            _logger.LogInformation("Connected to IMAP. Inbox has {Count} messages", inbox.Count);

            // Get the most recent messages
            var startIndex = Math.Max(0, inbox.Count - count);
            var uids = await inbox.SearchAsync(SearchQuery.All);
            var recentUids = uids.Skip(startIndex).Take(count).Reverse().ToList();

            foreach (var uid in recentUids)
            {
                try
                {
                    var message = await inbox.GetMessageAsync(uid);
                    var emailMessage = ConvertToEmailMessage(uid.ToString(), message);
                    
                    // Get raw message for archiving
                    using var stream = new MemoryStream();
                    await message.WriteToAsync(stream);
                    emailMessage.RawMessage = stream.ToArray();
                    
                    emails.Add(emailMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message UID {Uid}", uid);
                }
            }

            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to IMAP server");
            throw;
        }

        return emails;
    }

    public async Task<bool> DeleteEmailAsync(string uniqueId)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Email service is not configured");
            return false;
        }

        try
        {
            using var client = new ImapClient();
            
            // Bypass SSL certificate validation
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            
            await client.ConnectAsync(_settings.ImapServer, _settings.Port, _settings.UseSsl);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            if (uint.TryParse(uniqueId, out var uidValue))
            {
                var uid = new UniqueId(uidValue);
                
                // Mark as deleted
                await inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true);
                
                // Expunge to permanently delete
                await inbox.ExpungeAsync();
                
                _logger.LogInformation("Email UID {Uid} deleted successfully", uniqueId);
                
                await client.DisconnectAsync(true);
                return true;
            }
            
            await client.DisconnectAsync(true);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email UID {Uid}", uniqueId);
            return false;
        }
    }

    private EmailMessage ConvertToEmailMessage(string uid, MimeMessage message)
    {
        var from = message.From.Mailboxes.FirstOrDefault();
        
        var email = new EmailMessage
        {
            UniqueId = uid,
            FromEmail = from?.Address ?? "",
            FromName = from?.Name ?? from?.Address ?? "Unknown",
            Subject = message.Subject ?? "(no subject)",
            Date = message.Date.LocalDateTime,
            Body = message.HtmlBody ?? message.TextBody ?? ""
        };

        // Process attachments
        foreach (var attachment in message.Attachments)
        {
            if (attachment is MimePart mimePart)
            {
                using var memoryStream = new MemoryStream();
                mimePart.Content.DecodeTo(memoryStream);
                
                email.Attachments.Add(new EmailAttachment
                {
                    FileName = mimePart.FileName ?? "attachment",
                    Content = memoryStream.ToArray(),
                    ContentType = mimePart.ContentType.MimeType
                });
            }
        }

        return email;
    }
}

