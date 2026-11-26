using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionCancelacion : EntidadBase
{
    public Guid FacturaId { get; set; }
    public FacturacionFactura Factura { get; set; } = null!;

    public string CorrelationId { get; set; } = string.Empty;
    public string MotivoCancelacion { get; set; } = "02"; // "01", "02", "03", "04"
    public string? UuidSustitucion { get; set; }

    public DateTime FechaSolicitudUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEfectivaCancelacionUtc { get; set; }

    public string? EstadoSat { get; set; }
    public string? EsCancelableSat { get; set; }
    public string? EstatusCancelacionSat { get; set; }
    public string? AcuseXml { get; set; }

    public Guid? UsuarioId { get; set; }
    public bool Exitoso { get; set; }
    public int? CodigoEstadoHttp { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? DetallesRespuesta { get; set; }
}

