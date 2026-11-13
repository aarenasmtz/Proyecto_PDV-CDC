using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Commercial.NotasVenta.DTOs;
using Pos.Application.Commercial.NotasVenta.Services;
using Pos.Application.Common.Interfaces;
using Pos.Application.Common.Models;
using Pos.Application.Common.Security;

namespace Pos.Api.Controllers.v1;

[ApiController]
[Route("api/v1/notas-venta")]
[Authorize]
public class NotasVentaController : ControllerBase
{
    private readonly INotaVentaService _notaVentaService;
    private readonly INotaVentaPdfService _pdfService;
    private readonly IAuditLogService _auditLogService;

    public NotasVentaController(
        INotaVentaService notaVentaService,
        INotaVentaPdfService pdfService,
        IAuditLogService auditLogService)
    {
        _notaVentaService = notaVentaService;
        _pdfService = pdfService;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Consulta ventas registradas para generar o reimprimir notas de venta comerciales.
    /// </summary>
    [HttpGet("ventas")]
    [Authorize(Policy = PermissionCodes.Sales.History)]
    public async Task<ActionResult<PagedResult<NotaVentaListItemDto>>> GetVentas(
        [FromQuery] string? search,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? saleStatus,
        [FromQuery] string? deliveryStatus,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var result = await _notaVentaService.GetVentasAsync(
            search, startDate, endDate, saleStatus, deliveryStatus, pageNumber, pageSize, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene toda la información detallada de una venta para su nota comercial.
    /// </summary>
    [HttpGet("ventas/{ventaId:guid}")]
    [Authorize(Policy = PermissionCodes.Sales.History)]
    public async Task<ActionResult<NotaVentaDto>> GetNotaVentaById(Guid ventaId, CancellationToken cancellationToken)
    {
        var nota = await _notaVentaService.GetNotaVentaByIdAsync(ventaId, cancellationToken);
        if (nota == null)
        {
            return NotFound(new { message = "Venta no encontrada." });
        }

        return Ok(nota);
    }

    /// <summary>
    /// Obtiene información de la nota comercial por folio numérico operacional.
    /// </summary>
    [HttpGet("ventas/folio/{folio:int}")]
    [Authorize(Policy = PermissionCodes.Sales.History)]
    public async Task<ActionResult<NotaVentaDto>> GetNotaVentaByFolio(int folio, CancellationToken cancellationToken)
    {
        var nota = await _notaVentaService.GetNotaVentaByFolioAsync(folio, cancellationToken);
        if (nota == null)
        {
            return NotFound(new { message = "Venta no encontrada." });
        }

        return Ok(nota);
    }

    /// <summary>
    /// Genera el documento PDF de la nota de venta comercial (tamaño carta, diseño WPC Bajío).
    /// </summary>
    [HttpGet("ventas/{ventaId:guid}/pdf")]
    [Authorize(Policy = PermissionCodes.Sales.History)]
    public async Task<IActionResult> GenerarPdf(Guid ventaId, CancellationToken cancellationToken)
    {
        var nota = await _notaVentaService.GetNotaVentaByIdAsync(ventaId, cancellationToken);
        if (nota == null)
        {
            return NotFound(new { message = "Venta no encontrada." });
        }

        var pdfBytes = _pdfService.GenerarPdf(nota);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? currentUserId = Guid.TryParse(userIdClaim, out var uId) ? uId : null;
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        // Registrar en bitácora auditoría
        await _auditLogService.LogAsync(
            correlationId: correlationId,
            userId: currentUserId,
            action: "GENERAR_NOTA_VENTA_PDF",
            entityName: "Venta",
            entityId: ventaId.ToString(),
            oldValuesJson: null,
            newValuesJson: System.Text.Json.JsonSerializer.Serialize(new { Folio = nota.Folio, BytesLength = pdfBytes.Length }),
            ipAddress: ipAddress,
            reason: $"Generación / Reimpresión de Nota de Venta comercial Folio: {nota.Folio}",
            module: "NotasVenta",
            eventType: "CommercialNotePdf",
            resultStatus: "Success",
            cancellationToken: cancellationToken);

        var sanitizedFolio = nota.Folio.Replace("/", "_").Replace("\\", "_");
        return File(pdfBytes, "application/pdf", $"NotaVenta_{sanitizedFolio}.pdf");
    }

    /// <summary>
    /// Actualiza el estado de entrega del pedido/venta (ENTREGADO vs PENDIENTE).
    /// </summary>
    [HttpPost("ventas/{ventaId:guid}/confirmar-entrega")]
    [Authorize(Policy = PermissionCodes.Sales.Process)]
    public async Task<ActionResult<NotaVentaDto>> ConfirmarEntrega(
        Guid ventaId,
        [FromBody] ConfirmarEntregaRequestDto request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? currentUserId = Guid.TryParse(userIdClaim, out var uId) ? uId : null;
        var currentUserName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("name")?.Value ?? "Usuario";
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        try
        {
            var notaActualizada = await _notaVentaService.ConfirmarEntregaAsync(
                ventaId, request, currentUserId, currentUserName, correlationId, ipAddress, cancellationToken);

            return Ok(notaActualizada);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

