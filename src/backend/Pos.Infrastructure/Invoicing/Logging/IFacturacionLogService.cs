using Pos.Application.Common.Models;
using Pos.Application.Invoicing.Dtos;

namespace Pos.Infrastructure.Invoicing.Logging;

public interface IFacturacionLogService
{
    Task<Guid> RegistrarLlamadaIniciadaAsync(
        string correlationId,
        string operacion,
        string metodoHttp,
        string ruta,
        string? requestJson,
        string? idempotencyKey = null,
        Guid? facturaId = null,
        Guid? intentoId = null,
        Guid? ventaId = null,
        Guid? usuarioId = null,
        string origen = "Usuario",
        string ambiente = "QA",
        int numeroReintento = 1,
        CancellationToken cancellationToken = default);

    Task RegistrarLlamadaFinalizadaAsync(
        Guid logId,
        int? httpStatusCode,
        int? providerHttpStatusCode,
        bool? succeeded,
        string resultado,
        string? responseJson,
        string? mensaje,
        string? detalleError,
        string? traceIdentifierProveedor,
        string? identificadorExterno,
        string? uuidFiscal,
        Exception? exception = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<FacturacionLogDto>> ObtenerLogsAsync(FacturacionLogFiltrosDto filtros, CancellationToken cancellationToken = default);
    Task<FacturacionLogDto?> ObtenerLogPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FacturacionLogDto>> ObtenerLlamadasRelacionadasAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<int> PurgarLogsAntiguosAsync(int diasRetencion, CancellationToken cancellationToken = default);
}

