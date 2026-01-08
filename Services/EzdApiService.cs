using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PushEmailEzd.Models;

namespace PushEmailEzd.Services;

/// <summary>
/// EZD API service implementation using HTTP/REST (simplified approach)
/// Note: Full implementation would use WCF/SOAP based on the XSD schema
/// </summary>
public class EzdApiService : IEzdApiService
{
    private readonly EzdSettings _settings;
    private readonly ILogger<EzdApiService> _logger;
    private readonly HttpClient _httpClient;

    public EzdApiService(IOptions<EzdSettings> settings, ILogger<EzdApiService> logger, IHttpClientFactory? httpClientFactory = null)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClientFactory?.CreateClient() ?? new HttpClient();
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_settings.ApiUrl);

    public async Task<EzdRegistrationResult> RejestrujWplywAsync(EmailMessage email, EzdRegistrationForm form)
    {
        if (!IsConfigured)
        {
            return new EzdRegistrationResult
            {
                Success = false,
                ErrorMessage = "EZD API is not configured. Please set ApiUrl in appsettings.json"
            };
        }

        try
        {
            _logger.LogInformation("Registering email '{Subject}' in EZD", form.Tytul);

            // Build SOAP request using form data
            var soapRequest = BuildRejestrujWplywSoapRequest(form);
            
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://ezd.gov.pl/IIntegracja/RejestrujWplyw");

            var response = await _httpClient.PostAsync(_settings.ApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Parse SOAP response to extract ZnakWplywu (RPW number)
                var result = ParseRejestrujWplywResponse(responseContent);
                
                if (result.Success)
                {
                    _logger.LogInformation("Email registered successfully. RPW: {RpwNumber}", result.RpwNumber);
                }
                
                return result;
            }
            else
            {
                _logger.LogError("EZD API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return new EzdRegistrationResult
                {
                    Success = false,
                    ErrorMessage = $"API Error: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling EZD API");
            return new EzdRegistrationResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Build SOAP envelope for RejestrujWplyw request from form data
    /// </summary>
    private string BuildRejestrujWplywSoapRequest(EzdRegistrationForm form)
    {
        var dataPisma = form.DataPisma.ToString("yyyy-MM-dd");
        var dataWplywu = form.DataWplywu.ToString("yyyy-MM-ddTHH:mm:ss");
        
        // Escape XML special characters
        var tytul = System.Security.SecurityElement.Escape(form.Tytul);
        var adresatNazwa = System.Security.SecurityElement.Escape(form.AdresatNazwa);
        var adresatImie = System.Security.SecurityElement.Escape(form.AdresatImie);
        var adresatNazwisko = System.Security.SecurityElement.Escape(form.AdresatNazwisko);
        var adresatEmail = System.Security.SecurityElement.Escape(form.AdresatEmail);
        var adresatTelefon = System.Security.SecurityElement.Escape(form.AdresatTelefon);
        var uwagi = System.Security.SecurityElement.Escape(form.Uwagi);
        var sposobDostarczenia = System.Security.SecurityElement.Escape(form.SposobDostarczenia);
        
        // Count selected attachments
        var liczbaZalacznikow = form.Attachments.Count(a => a.IsSelected);

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:ezd=""http://ezd.gov.pl/"">
    <soap:Body>
        <ezd:RejestrujWplyw>
            <ezd:CID>{_settings.CID}</ezd:CID>
            <ezd:IdPracownikaWlasciciela>{_settings.IdPracownikaWlasciciela}</ezd:IdPracownikaWlasciciela>
            <ezd:IdStanowiskaWlasciciela>{_settings.IdStanowiskaWlasciciela}</ezd:IdStanowiskaWlasciciela>
            <ezd:AdresatNazwa>{adresatNazwa}</ezd:AdresatNazwa>
            <ezd:AdresatImie>{adresatImie}</ezd:AdresatImie>
            <ezd:AdresatNazwisko>{adresatNazwisko}</ezd:AdresatNazwisko>
            <ezd:AdresatEmail>{adresatEmail}</ezd:AdresatEmail>
            <ezd:AdresatTelefon>{adresatTelefon}</ezd:AdresatTelefon>
            <ezd:AdresatTyp>O</ezd:AdresatTyp>
            <ezd:CzyDokumentElektroniczny>{form.CzyDokumentElektroniczny.ToString().ToLower()}</ezd:CzyDokumentElektroniczny>
            <ezd:DataPisma>{dataPisma}</ezd:DataPisma>
            <ezd:DataWplywu>{dataWplywu}</ezd:DataWplywu>
            <ezd:SposobDostarczenia>{sposobDostarczenia}</ezd:SposobDostarczenia>
            <ezd:Tytul>{tytul}</ezd:Tytul>
            <ezd:Uwagi>{uwagi}</ezd:Uwagi>
            <ezd:LiczbaZalacznikow>{liczbaZalacznikow}</ezd:LiczbaZalacznikow>
        </ezd:RejestrujWplyw>
    </soap:Body>
</soap:Envelope>";
    }


    /// <summary>
    /// Parse SOAP response to extract ZnakWplywu (RPW number)
    /// </summary>
    private EzdRegistrationResult ParseRejestrujWplywResponse(string soapResponse)
    {
        try
        {
            // Simple XML parsing for ZnakWplywu
            // In production, use proper XmlDocument or XDocument
            
            if (soapResponse.Contains("<ZnakWplywu>"))
            {
                var startTag = "<ZnakWplywu>";
                var endTag = "</ZnakWplywu>";
                var startIndex = soapResponse.IndexOf(startTag) + startTag.Length;
                var endIndex = soapResponse.IndexOf(endTag);
                var znakWplywu = soapResponse.Substring(startIndex, endIndex - startIndex);

                // Parse other fields if present
                long? idDokumentu = null;
                if (soapResponse.Contains("<IdDokumentu>"))
                {
                    var idStart = soapResponse.IndexOf("<IdDokumentu>") + "<IdDokumentu>".Length;
                    var idEnd = soapResponse.IndexOf("</IdDokumentu>");
                    if (long.TryParse(soapResponse.Substring(idStart, idEnd - idStart), out var id))
                        idDokumentu = id;
                }

                int? idWplywu = null;
                if (soapResponse.Contains("<IdWplywu>"))
                {
                    var idStart = soapResponse.IndexOf("<IdWplywu>") + "<IdWplywu>".Length;
                    var idEnd = soapResponse.IndexOf("</IdWplywu>");
                    if (int.TryParse(soapResponse.Substring(idStart, idEnd - idStart), out var id))
                        idWplywu = id;
                }

                int? idKoszulki = null;
                if (soapResponse.Contains("<IdKoszulki>"))
                {
                    var idStart = soapResponse.IndexOf("<IdKoszulki>") + "<IdKoszulki>".Length;
                    var idEnd = soapResponse.IndexOf("</IdKoszulki>");
                    if (int.TryParse(soapResponse.Substring(idStart, idEnd - idStart), out var id))
                        idKoszulki = id;
                }

                return new EzdRegistrationResult
                {
                    Success = true,
                    RpwNumber = znakWplywu,
                    IdDokumentu = idDokumentu,
                    IdWplywu = idWplywu,
                    IdKoszulki = idKoszulki
                };
            }

            // Check for error message
            if (soapResponse.Contains("<ResultMessage>"))
            {
                var startTag = "<ResultMessage>";
                var endTag = "</ResultMessage>";
                var startIndex = soapResponse.IndexOf(startTag) + startTag.Length;
                var endIndex = soapResponse.IndexOf(endTag);
                var errorMessage = soapResponse.Substring(startIndex, endIndex - startIndex);

                return new EzdRegistrationResult
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }

            return new EzdRegistrationResult
            {
                Success = false,
                ErrorMessage = "Failed to parse response from EZD API"
            };
        }
        catch (Exception ex)
        {
            return new EzdRegistrationResult
            {
                Success = false,
                ErrorMessage = $"Error parsing response: {ex.Message}"
            };
        }
    }
}
