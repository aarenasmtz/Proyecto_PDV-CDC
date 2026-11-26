using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionLog : EntidadBase
{
    public string CorrelationId { get; set; } = string.Empty;
    public Guid? FacturaId { get; set; }
    public Guid? IntentoId { get; set; }
    public Guid? VentaId { get; set; }

    public string EmpresaId { get; set; } = "WPC-BAJIO";
    public string SucursalId { get; set; } = "MATRIZ";
    public Guid? UsuarioId { get; set; }

    public string Origen { get; set; } = "Usuario"; // "Usuario", "TareaAutomatica", "Conciliacion"
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;
    public string Proveedor { get; set; } = "FiscalAPI";
    public string Operacion { get; set; } = string.Empty;
    public string MetodoHttp { get; set; } = "POST";
    public string Ruta { get; set; } = string.Empty;

    public DateTime FechaInicioUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFinUtc { get; set; }
    public long? DuracionMs { get; set; }

    public string EstadoEjecucion { get; set; } = "Iniciada"; // "Iniciada", "Finalizada", "Interrumpida", "ResultadoIncierto"
    public int? HttpStatusCode { get; set; }
    public int? ProviderHttpStatusCode { get; set; }
    public bool? Succeeded { get; set; }
    public string Resultado { get; set; } = "Iniciado"; // "Exitoso", "Rechazado", "FalloTecnico", "ResultadoIncierto"

    public string? Mensaje { get; set; }
    public string? DetalleError { get; set; }
    public string? TraceIdentifierProveedor { get; set; }
    public string? IdentificadorExterno { get; set; }
    public string? UuidFiscal { get; set; }
    public string? IdempotencyKey { get; set; }
    public int NumeroReintento { get; set; } = 1;

    public string? RequestSanitizadoJson { get; set; }
    public string? ResponseSanitizadoJson { get; set; }
    public string? TipoExcepcion { get; set; }
    public string? StackTraceSanitizado { get; set; }
    public string VersionAplicacion { get; set; } = "2.15.0";
}

