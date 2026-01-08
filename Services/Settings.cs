namespace PushEmailEzd.Services;

/// <summary>
/// Email IMAP connection settings
/// </summary>
public class EmailSettings
{
    public string ImapServer { get; set; } = string.Empty;
    public int Port { get; set; } = 993;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// EZD PUW API connection settings
/// </summary>
public class EzdSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public int CID { get; set; }
    public int IdPracownikaWlasciciela { get; set; }
    public int IdStanowiskaWlasciciela { get; set; }
    
    // Validation helpers
    public bool HasApiUrl => !string.IsNullOrEmpty(ApiUrl);
    public bool HasCID => CID > 0;
    public bool HasIdPracownika => IdPracownikaWlasciciela > 0;
    public bool HasIdStanowiska => IdStanowiskaWlasciciela > 0;
    public bool IsFullyConfigured => HasApiUrl && HasCID && HasIdPracownika && HasIdStanowiska;
    
    public List<string> GetMissingConfigItems()
    {
        var missing = new List<string>();
        if (!HasCID) missing.Add("CID");
        if (!HasIdPracownika) missing.Add("IdPracownikaWlasciciela");
        if (!HasIdStanowiska) missing.Add("IdStanowiskaWlasciciela");
        return missing;
    }
}

