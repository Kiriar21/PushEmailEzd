using PushEmailEzd.Models;

namespace PushEmailEzd.Services;

/// <summary>
/// Service for interacting with EZD PUW API
/// </summary>
public interface IEzdApiService
{
    /// <summary>
    /// Register an email as incoming document (wplyw) in EZD using form data
    /// </summary>
    /// <param name="email">Original email message</param>
    /// <param name="form">Editable registration form data</param>
    /// <returns>Registration result with RPW number</returns>
    Task<EzdRegistrationResult> RejestrujWplywAsync(EmailMessage email, EzdRegistrationForm form);
    
    /// <summary>
    /// Check if EZD API is properly configured
    /// </summary>
    bool IsConfigured { get; }
}

