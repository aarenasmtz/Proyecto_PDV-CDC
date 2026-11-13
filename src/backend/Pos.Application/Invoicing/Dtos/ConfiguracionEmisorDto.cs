using Pos.Domain.Common;

namespace Pos.Application.Invoicing.Dtos;

public class ConfiguracionEmisorDto
{
    public Guid Id { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string Sucursal { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RegimenFiscal { get; set; } = string.Empty;
    public string CodigoPostalExpedicion { get; set; } = string.Empty;
    public string Serie { get; set; } = "A";
    public long FolioSiguiente { get; set; } = 1;
    public string? FiscalapiPersonId { get; set; }
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;
    public string? CsdCertificadoNumero { get; set; }
    public bool EstaListoParaEmitir { get; set; }
    public string Notas { get; set; } = string.Empty;
    public bool FacturacionHabilitada { get; set; }
}

public class ActualizarConfiguracionEmisorDto
{
    public string Empresa { get; set; } = "WPC Bajío";
    public string Sucursal { get; set; } = "Matriz";
    public string Rfc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RegimenFiscal { get; set; } = "601";
    public string CodigoPostalExpedicion { get; set; } = string.Empty;
    public string Serie { get; set; } = "A";
    public long FolioSiguiente { get; set; } = 1;
    public string? FiscalapiPersonId { get; set; }
    public string? CsdCertificadoNumero { get; set; }
    public bool EstaListoParaEmitir { get; set; }
    public string Notas { get; set; } = string.Empty;
}

