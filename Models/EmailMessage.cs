namespace PushEmailEzd.Models;

/// <summary>
/// Represents an email message retrieved from the mailbox
/// </summary>
public class EmailMessage
{
    /// <summary>Unique identifier from the mail server</summary>
    public string UniqueId { get; set; } = string.Empty;
    
    /// <summary>Sender email address</summary>
    public string FromEmail { get; set; } = string.Empty;
    
    /// <summary>Sender display name</summary>
    public string FromName { get; set; } = string.Empty;
    
    /// <summary>Email subject - maps to EZD Tytul</summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>Date received - maps to EZD DataWplywu</summary>
    public DateTime Date { get; set; }
    
    /// <summary>Email body (HTML or plain text)</summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>Raw email content as .eml for archiving</summary>
    public byte[]? RawMessage { get; set; }
    
    /// <summary>List of attachments</summary>
    public List<EmailAttachment> Attachments { get; set; } = new();
    
    /// <summary>Selected for registration in EZD</summary>
    public bool IsSelected { get; set; }
    
    /// <summary>RPW number after successful registration (ZnakWplywu)</summary>
    public string? RpwNumber { get; set; }
    
    /// <summary>Has been registered in EZD</summary>
    public bool IsRegistered { get; set; }
    
    /// <summary>Error message if registration failed</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Email attachment
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}
