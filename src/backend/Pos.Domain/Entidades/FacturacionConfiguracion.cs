using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionConfiguracion : EntidadBase
{
    public string EmpresaId { get; set; } = "WPC-BAJIO";
    public string Empresa { get; set; } = "WPC Bajío";
    public string SucursalId { get; set; } = "MATRIZ";
    public string Sucursal { get; set; } = "Matriz";
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;

    public string Rfc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RegimenFiscal { get; set; } = "601";
    public string CodigoPostalExpedicion { get; set; } = string.Empty;
    public string Serie { get; set; } = "A";
    public long FolioSiguiente { get; set; } = 1;

    public string? FiscalapiIssuerId { get; set; }
    public string? FiscalapiPersonId { get => FiscalapiIssuerId; set => FiscalapiIssuerId = value; }
    public string? CsdCertificadoNumero { get; set; }
    public bool EstaListoParaEmitir { get; set; } = false;
    public string Notas { get; set; } = string.Empty;
    public Guid? UsuarioModificoId { get; set; }
}
