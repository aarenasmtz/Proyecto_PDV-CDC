using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pos.Application.Common.Models;
using Pos.Application.Invoicing.Dtos;
using Pos.Domain.Entidades;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Invoicing.Logging;

public class FacturacionLogService : IFacturacionLogService
{
    private readonly PosDbContext _dbContext;
    private readonly ILogger<FacturacionLogService> _logger;

    public FacturacionLogService(PosDbContext dbContext, ILogger<FacturacionLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> RegistrarLlamadaIniciadaAsync(
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
        CancellationToken cancellationToken = default)
    {
        var log = new FacturacionLog
        {
            CorrelationId = !string.IsNullOrWhiteSpace(correlationId) ? correlationId.Trim() : Guid.NewGuid().ToString("D"),
            Operacion = operacion,
            MetodoHttp = metodoHttp,
            Ruta = ruta,
            RequestSanitizadoJson = FacturacionSanitizer.SanitizeJson(requestJson),
            IdempotencyKey = idempotencyKey,
            FacturaId = facturaId,
            IntentoId = intentoId,
            VentaId = ventaId,
            UsuarioId = usuarioId,
            Origen = origen,
            Ambiente = ambiente,
            NumeroReintento = numeroReintento,
            EstadoEjecucion = "Iniciada",
            FechaInicioUtc = DateTime.UtcNow,
            Resultado = "Iniciado"
        };

        try
        {
            _dbContext.FacturacionLogs.Add(log);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return log.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al registrar inicio de llamada en FacturacionLog para CorrelationId {CorrelationId}", log.CorrelationId);
            return log.Id;
        }
    }

    public async Task RegistrarLlamadaFinalizadaAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = await _dbContext.FacturacionLogs.FindAsync(new object[] { logId }, cancellationToken);
            if (log == null)
            {
                _logger.LogWarning("No se encontró el registro FacturacionLog {LogId} para finalizar.", logId);
                return;
            }

            var now = DateTime.UtcNow;
            log.FechaFinUtc = now;
            log.DuracionMs = (long)(now - log.FechaInicioUtc).TotalMilliseconds;
            log.HttpStatusCode = httpStatusCode;
            log.ProviderHttpStatusCode = providerHttpStatusCode;
            log.Succeeded = succeeded;
            log.Resultado = resultado;
            log.EstadoEjecucion = resultado == "ResultadoIncierto" ? "ResultadoIncierto" : "Finalizada";
            log.ResponseSanitizadoJson = FacturacionSanitizer.SanitizeJson(responseJson);
            log.Mensaje = mensaje;
            log.DetalleError = detalleError;
            log.TraceIdentifierProveedor = traceIdentifierProveedor;
            log.IdentificadorExterno = identificadorExterno;
            log.UuidFiscal = uuidFiscal;

            if (exception != null)
            {
                log.TipoExcepcion = exception.GetType().FullName;
                log.StackTraceSanitizado = FacturacionSanitizer.SanitizeException(exception);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al registrar finalización en FacturacionLog {LogId}", logId);
        }
    }

    public async Task<PagedResult<FacturacionLogDto>> ObtenerLogsAsync(FacturacionLogFiltrosDto filtros, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FacturacionLogs.AsNoTracking().AsQueryable();

        if (filtros.FechaInicio.HasValue)
        {
            query = query.Where(l => l.FechaInicioUtc >= filtros.FechaInicio.Value);
        }

        if (filtros.FechaFin.HasValue)
        {
            query = query.Where(l => l.FechaInicioUtc <= filtros.FechaFin.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Ambiente) && filtros.Ambiente != "Todos")
        {
            query = query.Where(l => l.Ambiente == filtros.Ambiente);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Operacion))
        {
            query = query.Where(l => l.Operacion == filtros.Operacion);
        }

        if (filtros.VentaId.HasValue)
        {
            query = query.Where(l => l.VentaId == filtros.VentaId.Value);
        }

        if (filtros.FacturaId.HasValue)
        {
            query = query.Where(l => l.FacturaId == filtros.FacturaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.UuidFiscal))
        {
            var uuid = filtros.UuidFiscal.Trim();
            query = query.Where(l => l.UuidFiscal != null && l.UuidFiscal.Contains(uuid));
        }

        if (!string.IsNullOrWhiteSpace(filtros.Resultado))
        {
            query = query.Where(l => l.Resultado == filtros.Resultado);
        }

        if (filtros.HttpStatusCode.HasValue)
        {
            query = query.Where(l => l.HttpStatusCode == filtros.HttpStatusCode.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.CorrelationId))
        {
            var corr = filtros.CorrelationId.Trim();
            query = query.Where(l => l.CorrelationId.Contains(corr));
        }

        if (!string.IsNullOrWhiteSpace(filtros.TraceIdentifier))
        {
            var trace = filtros.TraceIdentifier.Trim();
            query = query.Where(l => l.TraceIdentifierProveedor != null && l.TraceIdentifierProveedor.Contains(trace));
        }

        if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
        {
            var search = filtros.Busqueda.Trim();
            query = query.Where(l =>
                l.CorrelationId.Contains(search) ||
                (l.UuidFiscal != null && l.UuidFiscal.Contains(search)) ||
                (l.TraceIdentifierProveedor != null && l.TraceIdentifierProveedor.Contains(search)) ||
                (l.Mensaje != null && l.Mensaje.Contains(search)) ||
                l.Ruta.Contains(search));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var page = filtros.Page > 0 ? filtros.Page : 1;
        var pageSize = filtros.PageSize > 0 ? filtros.PageSize : 20;

        var items = await query
            .OrderByDescending(l => l.FechaInicioUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new FacturacionLogDto
            {
                Id = l.Id,
                CorrelationId = l.CorrelationId,
                FacturaId = l.FacturaId,
                IntentoId = l.IntentoId,
                VentaId = l.VentaId,
                EmpresaId = l.EmpresaId,
                SucursalId = l.SucursalId,
                UsuarioId = l.UsuarioId,
                Origen = l.Origen,
                Ambiente = l.Ambiente,
                Proveedor = l.Proveedor,
                Operacion = l.Operacion,
                MetodoHttp = l.MetodoHttp,
                Ruta = l.Ruta,
                FechaInicioUtc = l.FechaInicioUtc,
                FechaFinUtc = l.FechaFinUtc,
                DuracionMs = l.DuracionMs,
                EstadoEjecucion = l.EstadoEjecucion,
                HttpStatusCode = l.HttpStatusCode,
                ProviderHttpStatusCode = l.ProviderHttpStatusCode,
                Succeeded = l.Succeeded,
                Resultado = l.Resultado,
                Mensaje = l.Mensaje,
                DetalleError = l.DetalleError,
                TraceIdentifierProveedor = l.TraceIdentifierProveedor,
                IdentificadorExterno = l.IdentificadorExterno,
                UuidFiscal = l.UuidFiscal,
                IdempotencyKey = l.IdempotencyKey,
                NumeroReintento = l.NumeroReintento,
                RequestSanitizadoJson = l.RequestSanitizadoJson,
                ResponseSanitizadoJson = l.ResponseSanitizadoJson,
                TipoExcepcion = l.TipoExcepcion,
                StackTraceSanitizado = l.StackTraceSanitizado,
                VersionAplicacion = l.VersionAplicacion
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FacturacionLogDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<FacturacionLogDto?> ObtenerLogPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var l = await _dbContext.FacturacionLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (l == null) return null;

        return new FacturacionLogDto
        {
            Id = l.Id,
            CorrelationId = l.CorrelationId,
            FacturaId = l.FacturaId,
            IntentoId = l.IntentoId,
            VentaId = l.VentaId,
            EmpresaId = l.EmpresaId,
            SucursalId = l.SucursalId,
            UsuarioId = l.UsuarioId,
            Origen = l.Origen,
            Ambiente = l.Ambiente,
            Proveedor = l.Proveedor,
            Operacion = l.Operacion,
            MetodoHttp = l.MetodoHttp,
            Ruta = l.Ruta,
            FechaInicioUtc = l.FechaInicioUtc,
            FechaFinUtc = l.FechaFinUtc,
            DuracionMs = l.DuracionMs,
            EstadoEjecucion = l.EstadoEjecucion,
            HttpStatusCode = l.HttpStatusCode,
            ProviderHttpStatusCode = l.ProviderHttpStatusCode,
            Succeeded = l.Succeeded,
            Resultado = l.Resultado,
            Mensaje = l.Mensaje,
            DetalleError = l.DetalleError,
            TraceIdentifierProveedor = l.TraceIdentifierProveedor,
            IdentificadorExterno = l.IdentificadorExterno,
            UuidFiscal = l.UuidFiscal,
            IdempotencyKey = l.IdempotencyKey,
            NumeroReintento = l.NumeroReintento,
            RequestSanitizadoJson = l.RequestSanitizadoJson,
            ResponseSanitizadoJson = l.ResponseSanitizadoJson,
            TipoExcepcion = l.TipoExcepcion,
            StackTraceSanitizado = l.StackTraceSanitizado,
            VersionAplicacion = l.VersionAplicacion
        };
    }

    public async Task<IReadOnlyList<FacturacionLogDto>> ObtenerLlamadasRelacionadasAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return Array.Empty<FacturacionLogDto>();

        var corr = correlationId.Trim();
        return await _dbContext.FacturacionLogs.AsNoTracking()
            .Where(l => l.CorrelationId == corr)
            .OrderBy(l => l.FechaInicioUtc)
            .Select(l => new FacturacionLogDto
            {
                Id = l.Id,
                CorrelationId = l.CorrelationId,
                FacturaId = l.FacturaId,
                IntentoId = l.IntentoId,
                VentaId = l.VentaId,
                EmpresaId = l.EmpresaId,
                SucursalId = l.SucursalId,
                UsuarioId = l.UsuarioId,
                Origen = l.Origen,
                Ambiente = l.Ambiente,
                Proveedor = l.Proveedor,
                Operacion = l.Operacion,
                MetodoHttp = l.MetodoHttp,
                Ruta = l.Ruta,
                FechaInicioUtc = l.FechaInicioUtc,
                FechaFinUtc = l.FechaFinUtc,
                DuracionMs = l.DuracionMs,
                EstadoEjecucion = l.EstadoEjecucion,
                HttpStatusCode = l.HttpStatusCode,
                ProviderHttpStatusCode = l.ProviderHttpStatusCode,
                Succeeded = l.Succeeded,
                Resultado = l.Resultado,
                Mensaje = l.Mensaje,
                DetalleError = l.DetalleError,
                TraceIdentifierProveedor = l.TraceIdentifierProveedor,
                IdentificadorExterno = l.IdentificadorExterno,
                UuidFiscal = l.UuidFiscal,
                IdempotencyKey = l.IdempotencyKey,
                NumeroReintento = l.NumeroReintento,
                RequestSanitizadoJson = l.RequestSanitizadoJson,
                ResponseSanitizadoJson = l.ResponseSanitizadoJson,
                TipoExcepcion = l.TipoExcepcion,
                StackTraceSanitizado = l.StackTraceSanitizado,
                VersionAplicacion = l.VersionAplicacion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> PurgarLogsAntiguosAsync(int diasRetencion, CancellationToken cancellationToken = default)
    {
        if (diasRetencion <= 0) diasRetencion = 90;
        var fechaLimite = DateTime.UtcNow.AddDays(-diasRetencion);

        var logsAEliminar = await _dbContext.FacturacionLogs
            .Where(l => l.FechaInicioUtc < fechaLimite)
            .ToListAsync(cancellationToken);

        if (logsAEliminar.Count > 0)
        {
            _dbContext.FacturacionLogs.RemoveRange(logsAEliminar);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return logsAEliminar.Count;
    }
}

