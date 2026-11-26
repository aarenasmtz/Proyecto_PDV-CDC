namespace Pos.Application.Invoicing.Dtos;

public class FacturacionLogDto
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Guid? FacturaId { get; set; }
    public Guid? IntentoId { get; set; }
    public Guid? VentaId { get; set; }

    public string EmpresaId { get; set; } = string.Empty;
    public string SucursalId { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }

    public string Origen { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string MetodoHttp { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;

    public DateTime FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public long? DuracionMs { get; set; }

    public string EstadoEjecucion { get; set; } = string.Empty;
    public int? HttpStatusCode { get; set; }
    public int? ProviderHttpStatusCode { get; set; }
    public bool? Succeeded { get; set; }
    public string Resultado { get; set; } = string.Empty;

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
    public string VersionAplicacion { get; set; } = string.Empty;
}

public class FacturacionLogFiltrosDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Ambiente { get; set; }
    public string? Operacion { get; set; }
    public Guid? VentaId { get; set; }
    public Guid? FacturaId { get; set; }
    public string? UuidFiscal { get; set; }
    public string? Resultado { get; set; }
    public int? HttpStatusCode { get; set; }
    public string? CorrelationId { get; set; }
    public string? TraceIdentifier { get; set; }
    public string? Busqueda { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

