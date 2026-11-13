using Pos.Application.Common.Models;
using Pos.Application.Invoicing.Dtos;

namespace Pos.Application.Invoicing.Services;

public interface IFacturacionApplicationService
{
    Task<ConfiguracionEmisorDto> ObtenerConfiguracionEmisorAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracionEmisorDto> GuardarConfiguracionEmisorAsync(ActualizarConfiguracionEmisorDto dto, Guid usuarioId, CancellationToken cancellationToken = default);

    Task<VistaPreviaFacturaDto> GenerarVistaPreviaVentaAsync(Guid ventaId, CancellationToken cancellationToken = default);
    Task<ResultadoEmisionFacturaDto> EmitirFacturaVentaAsync(Guid ventaId, SolicitarEmisionFacturaDto dto, Guid usuarioId, CancellationToken cancellationToken = default);

    Task<PagedResult<FacturaDto>> ObtenerFacturasAsync(FacturaFiltrosDto filtros, CancellationToken cancellationToken = default);
    Task<FacturaDto?> ObtenerFacturaPorIdAsync(Guid facturaId, CancellationToken cancellationToken = default);

    Task<(byte[] Bytes, string ContentType, string NombreArchivo)> DescargarXmlAsync(Guid facturaId, CancellationToken cancellationToken = default);
    Task<(byte[] Bytes, string ContentType, string NombreArchivo)> DescargarPdfAsync(Guid facturaId, CancellationToken cancellationToken = default);

    Task<bool> EnviarPorCorreoAsync(Guid facturaId, EnviarFacturaCorreoDto dto, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<ResultadoEstadoSatDto> ConsultarEstadoSatAsync(Guid facturaId, Guid usuarioId, CancellationToken cancellationToken = default);

    Task<ResultadoCancelacionFacturaDto> SolicitarCancelacionAsync(Guid facturaId, SolicitarCancelacionFacturaDto dto, Guid usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogoSatItemDto>> ObtenerCatalogoSatAsync(string nombreCatalogo, CancellationToken cancellationToken = default);

    // Bitácora y diagnóstico técnico de FiscalAPI
    Task<PagedResult<FacturacionLogDto>> ObtenerBitacoraAsync(FacturacionLogFiltrosDto filtros, CancellationToken cancellationToken = default);
    Task<FacturacionLogDto?> ObtenerBitacoraPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FacturacionLogDto>> ObtenerLlamadasRelacionadasAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<int> PurgarBitacoraAntiguaAsync(int? diasRetencion = null, CancellationToken cancellationToken = default);
}

