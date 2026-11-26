using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionIntento : EntidadBase
{
    public Guid? FacturaId { get; set; }
    public FacturacionFactura? Factura { get; set; }

    public Guid? VentaId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;

    public int NumeroRevision { get; set; } = 1;
    public int NumeroEnvio { get; set; } = 1;
    public string IdempotencyKey { get; set; } = string.Empty;

    public string Estado { get; set; } = "Iniciado"; // "Iniciado", "Completado", "Rechazado", "ResultadoIncierto", "Interrumpido"
    public string? Resultado { get; set; }
    public int? HttpStatusCode { get; set; }

    public string? RequestSanitizadoJson { get; set; }
    public string? ResponseSanitizadoJson { get; set; }
    public string? Mensaje { get; set; }
    public string? DetalleError { get; set; }

    public DateTime FechaInicioUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFinUtc { get; set; }
    public Guid? UsuarioId { get; set; }
}

