namespace PushEmailEzd.Models;

/// <summary>
/// Response from EZD after registering an incoming document (Wplyw)
/// </summary>
public class EzdRegistrationResult
{
    /// <summary>Registration was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>ZnakWplywu - the RPW number assigned by EZD</summary>
    public string? RpwNumber { get; set; }
    
    /// <summary>Internal EZD document ID</summary>
    public long? IdDokumentu { get; set; }
    
    /// <summary>Internal EZD inflow ID</summary>
    public int? IdWplywu { get; set; }
    
    /// <summary>Internal EZD folder (koszulka) ID</summary>
    public int? IdKoszulki { get; set; }
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
}
