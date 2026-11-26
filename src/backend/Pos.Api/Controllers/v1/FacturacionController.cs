using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Common.Models;
using Pos.Application.Common.Security;
using Pos.Application.Invoicing.Dtos;
using Pos.Application.Invoicing.Services;

namespace Pos.Api.Controllers.v1;

[ApiController]
[Route("api/v1/facturacion")]
[Route("api/v1/invoicing")]
[Authorize]
public class FacturacionController : ControllerBase
{
    private readonly IFacturacionApplicationService _facturacionService;

    public FacturacionController(IFacturacionApplicationService facturacionService)
    {
        _facturacionService = facturacionService;
    }

    [HttpGet("config")]
    [Authorize(Policy = PermissionCodes.Invoicing.View)]
    public async Task<ActionResult<ConfiguracionEmisorDto>> ObtenerConfiguracion(CancellationToken cancellationToken)
    {
        var config = await _facturacionService.ObtenerConfiguracionEmisorAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut("config")]
    [Authorize(Policy = PermissionCodes.Invoicing.AdminConfig)]
    public async Task<ActionResult<ConfiguracionEmisorDto>> GuardarConfiguracion(
        [FromBody] ActualizarConfiguracionEmisorDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = GetCurrentUserId() ?? Guid.Empty;
        try
        {
            var config = await _facturacionService.GuardarConfiguracionEmisorAsync(dto, usuarioId, cancellationToken);
            return Ok(config);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("sales/{saleId:guid}/preview")]
    [Authorize(Policy = PermissionCodes.Invoicing.View)]
    public async Task<ActionResult<VistaPreviaFacturaDto>> GenerarVistaPrevia(Guid saleId, CancellationToken cancellationToken)
    {
        try
        {
            var vistaPrevia = await _facturacionService.GenerarVistaPreviaVentaAsync(saleId, cancellationToken);
            return Ok(vistaPrevia);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sales/{saleId:guid}/issue")]
    [Authorize(Policy = PermissionCodes.Invoicing.Issue)]
    public async Task<ActionResult<ResultadoEmisionFacturaDto>> EmitirFactura(
        Guid saleId,
        [FromBody] SolicitarEmisionFacturaDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = GetCurrentUserId() ?? Guid.Empty;
        try
        {
            var resultado = await _facturacionService.EmitirFacturaVentaAsync(saleId, dto, usuarioId, cancellationToken);
            if (!resultado.Exitoso)
            {
                if (resultado.RequiereConfirmacionManual)
                {
                    return StatusCode(202, resultado); // Accepted with pending state
                }
                return BadRequest(resultado);
            }
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("invoices")]
    [Authorize(Policy = PermissionCodes.Invoicing.View)]
    public async Task<ActionResult<PagedResult<FacturaDto>>> ObtenerFacturas(
        [FromQuery] FacturaFiltrosDto filtros,
        CancellationToken cancellationToken)
    {
        var result = await _facturacionService.ObtenerFacturasAsync(filtros, cancellationToken);
        return Ok(result);
    }

    [HttpGet("invoices/{id:guid}")]
    [Authorize(Policy = PermissionCodes.Invoicing.View)]
    public async Task<ActionResult<FacturaDto>> ObtenerFacturaPorId(Guid id, CancellationToken cancellationToken)
    {
        var factura = await _facturacionService.ObtenerFacturaPorIdAsync(id, cancellationToken);
        if (factura == null)
        {
            return NotFound(new { message = "Factura no encontrada." });
        }
        return Ok(factura);
    }

    [HttpGet("invoices/{id:guid}/xml")]
    [Authorize(Policy = PermissionCodes.Invoicing.Download)]
    public async Task<IActionResult> DescargarXml(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var (bytes, contentType, nombreArchivo) = await _facturacionService.DescargarXmlAsync(id, cancellationToken);
            return File(bytes, contentType, nombreArchivo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("invoices/{id:guid}/pdf")]
    [Authorize(Policy = PermissionCodes.Invoicing.Download)]
    public async Task<IActionResult> DescargarPdf(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var (bytes, contentType, nombreArchivo) = await _facturacionService.DescargarPdfAsync(id, cancellationToken);
            return File(bytes, contentType, nombreArchivo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("invoices/{id:guid}/send-email")]
    [Authorize(Policy = PermissionCodes.Invoicing.SendEmail)]
    public async Task<IActionResult> EnviarPorCorreo(
        Guid id,
        [FromBody] EnviarFacturaCorreoDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = GetCurrentUserId() ?? Guid.Empty;
        try
        {
            var exitoso = await _facturacionService.EnviarPorCorreoAsync(id, dto, usuarioId, cancellationToken);
            return Ok(new { exitoso, message = "Factura enviada exitosamente por correo electrónico." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("invoices/{id:guid}/sat-status")]
    [Authorize(Policy = PermissionCodes.Invoicing.View)]
    public async Task<ActionResult<ResultadoEstadoSatDto>> ConsultarEstadoSat(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = GetCurrentUserId() ?? Guid.Empty;
        try
        {
            var res = await _facturacionService.ConsultarEstadoSatAsync(id, usuarioId, cancellationToken);
            return Ok(res);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("invoices/{id:guid}/cancel")]
    [Authorize(Policy = PermissionCodes.Invoicing.Cancel)]
    public async Task<ActionResult<ResultadoCancelacionFacturaDto>> CancelarFactura(
        Guid id,
        [FromBody] SolicitarCancelacionFacturaDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = GetCurrentUserId() ?? Guid.Empty;
        try
        {
            var res = await _facturacionService.SolicitarCancelacionAsync(id, dto, usuarioId, cancellationToken);
            return Ok(res);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("catalogs/{catalogName}")]
    [Authorize(Policy = PermissionCodes.Invoicing.View)]
    public async Task<ActionResult<IReadOnlyList<CatalogoSatItemDto>>> ObtenerCatalogoSat(string catalogName, CancellationToken cancellationToken)
    {
        var items = await _facturacionService.ObtenerCatalogoSatAsync(catalogName, cancellationToken);
        return Ok(items);
    }

    [HttpGet("bitacora")]
    [Authorize(Policy = PermissionCodes.Invoicing.ViewLogs)]
    public async Task<ActionResult<PagedResult<FacturacionLogDto>>> ObtenerBitacora(
        [FromQuery] FacturacionLogFiltrosDto filtros,
        CancellationToken cancellationToken)
    {
        var result = await _facturacionService.ObtenerBitacoraAsync(filtros, cancellationToken);
        return Ok(result);
    }

    [HttpGet("bitacora/{id:guid}")]
    [Authorize(Policy = PermissionCodes.Invoicing.ViewLogs)]
    public async Task<ActionResult<FacturacionLogDto>> ObtenerBitacoraPorId(Guid id, CancellationToken cancellationToken)
    {
        var log = await _facturacionService.ObtenerBitacoraPorIdAsync(id, cancellationToken);
        if (log == null)
            return NotFound(new { message = $"No se encontró el registro de bitácora con ID {id}" });

        return Ok(log);
    }

    [HttpGet("bitacora/relacionadas/{correlationId}")]
    [Authorize(Policy = PermissionCodes.Invoicing.ViewLogs)]
    public async Task<ActionResult<IReadOnlyList<FacturacionLogDto>>> ObtenerLlamadasRelacionadas(
        string correlationId,
        CancellationToken cancellationToken)
    {
        var items = await _facturacionService.ObtenerLlamadasRelacionadasAsync(correlationId, cancellationToken);
        return Ok(items);
    }

    [HttpPost("bitacora/purgar")]
    [Authorize(Policy = PermissionCodes.Invoicing.PurgeLogs)]
    public async Task<ActionResult> PurgarBitacora(
        [FromQuery] int? diasRetencion,
        CancellationToken cancellationToken)
    {
        var eliminados = await _facturacionService.PurgarBitacoraAntiguaAsync(diasRetencion, cancellationToken);
        return Ok(new { eliminados, mensaje = $"Se purgaron {eliminados} registros antiguos de la bitácora técnica." });
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

