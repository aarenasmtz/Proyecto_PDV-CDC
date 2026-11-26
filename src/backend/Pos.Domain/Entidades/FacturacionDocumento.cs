using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionDocumento : EntidadBase
{
    public Guid FacturaId { get; set; }
    public FacturacionFactura Factura { get; set; } = null!;

    public string TipoDocumento { get; set; } = string.Empty; // "XML", "PDF", "AcuseCancelacion"
    public string NombreArchivo { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string Sha256Hash { get; set; } = string.Empty;

    public string? RutaAlmacenamientoPrivada { get; set; }
    public string? ContenidoBase64 { get; set; }
}

