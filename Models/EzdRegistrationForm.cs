namespace PushEmailEzd.Models;

/// <summary>
/// Form data for EZD registration - editable by user before submission
/// </summary>
public class EzdRegistrationForm
{
    // Sender information
    public string AdresatNazwa { get; set; } = string.Empty;
    public string AdresatImie { get; set; } = string.Empty;
    public string AdresatNazwisko { get; set; } = string.Empty;
    public string AdresatEmail { get; set; } = string.Empty;
    public string AdresatTelefon { get; set; } = string.Empty;
    
    // Document information
    public string Tytul { get; set; } = string.Empty;
    public string Uwagi { get; set; } = string.Empty;
    public DateTime DataPisma { get; set; } = DateTime.Now;
    public DateTime DataWplywu { get; set; } = DateTime.Now;
    
    // Delivery method
    public string SposobDostarczenia { get; set; } = "Email";
    public bool CzyDokumentElektroniczny { get; set; } = true;
    
    // Attachments to include
    public List<AttachmentSelection> Attachments { get; set; } = new();
    
    // Original email reference
    public string EmailUniqueId { get; set; } = string.Empty;
    public byte[]? RawEmailContent { get; set; }
    
    /// <summary>
    /// Create form from email message with auto-filled values
    /// </summary>
    public static EzdRegistrationForm FromEmail(EmailMessage email)
    {
        // Try to parse name into first/last name
        var nameParts = email.FromName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        
        var form = new EzdRegistrationForm
        {
            AdresatNazwa = email.FromName,
            AdresatImie = nameParts.Length > 0 ? nameParts[0] : "",
            AdresatNazwisko = nameParts.Length > 1 ? nameParts[1] : "",
            AdresatEmail = email.FromEmail,
            Tytul = email.Subject,
            DataPisma = email.Date,
            DataWplywu = DateTime.Now,
            EmailUniqueId = email.UniqueId,
            RawEmailContent = email.RawMessage
        };
        
        // Add attachments with selection state
        foreach (var att in email.Attachments)
        {
            form.Attachments.Add(new AttachmentSelection
            {
                FileName = att.FileName,
                Content = att.Content,
                ContentType = att.ContentType,
                IsSelected = true // Selected by default
            });
        }
        
        return form;
    }
}

/// <summary>
/// Attachment with selection state for the form
/// </summary>
public class AttachmentSelection
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = true;
    
    public string SizeFormatted => Content.Length switch
    {
        < 1024 => $"{Content.Length} B",
        < 1024 * 1024 => $"{Content.Length / 1024} KB",
        _ => $"{Content.Length / (1024 * 1024)} MB"
    };
}
