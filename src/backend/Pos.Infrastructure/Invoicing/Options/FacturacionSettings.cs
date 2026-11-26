using Fiscalapi.Common;

namespace Pos.Infrastructure.Invoicing.Options;

public class FacturacionSettings
{
    public const string SectionName = "Facturacion";

    public string Ambiente { get; set; } = "QA"; // QA o PR
    public bool Habilitada { get; set; } = true;
    public string TimeZone { get; set; } = "America/Mexico_City";
    public int TimeoutSeconds { get; set; } = 30;
    public string? Base64Logo { get; set; }
    public string BandColor { get; set; } = "#9C4D22";
    public string FontColor { get; set; } = "#FFFFFF";
    public int BitacoraRetencionDias { get; set; } = 90;

    public (bool IsValid, string? ErrorMessage) ValidateWithFiscalapi(FiscalapiSettings fiscalapiSettings)
    {
        var ambienteNorm = (Ambiente ?? "").Trim().ToUpperInvariant();
        if (ambienteNorm != "QA" && ambienteNorm != "PR")
        {
            return (false, $"El ambiente de facturación '{Ambiente}' no es válido. Debe ser 'QA' o 'PR'.");
        }

        var apiUrl = fiscalapiSettings?.ApiUrl?.Trim().ToLowerInvariant() ?? "";
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            return (false, "La URL base de FiscalAPI (FiscalapiSettings:ApiUrl) no está configurada.");
        }

        if (ambienteNorm == "QA")
        {
            if (apiUrl.Contains("live.fiscalapi.com"))
            {
                return (false, "Inconsistencia de configuración: El ambiente está configurado como 'QA', pero la URL apunta a Producción (live.fiscalapi.com).");
            }
        }
        else if (ambienteNorm == "PR")
        {
            if (apiUrl.Contains("test.fiscalapi.com"))
            {
                return (false, "Inconsistencia de configuración: El ambiente está configurado como 'PR' (Producción), pero la URL apunta a Pruebas (test.fiscalapi.com).");
            }
        }

        return (true, null);
    }
}

