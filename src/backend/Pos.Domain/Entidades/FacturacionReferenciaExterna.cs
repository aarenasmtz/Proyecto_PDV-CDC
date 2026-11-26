using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionReferenciaExterna : EntidadBase
{
    public string TipoEntidad { get; set; } = string.Empty; // "Emisor", "Receptor", "Producto"
    public Guid EntidadLocalId { get; set; }
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;
    public string IdentificadorExterno { get; set; } = string.Empty;
    public string? Tenant { get; set; }
    public string? DetallesJson { get; set; }
}

