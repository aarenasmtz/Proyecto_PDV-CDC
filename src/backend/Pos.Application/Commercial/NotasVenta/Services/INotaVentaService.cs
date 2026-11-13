using Pos.Application.Commercial.NotasVenta.DTOs;
using Pos.Application.Common.Models;

namespace Pos.Application.Commercial.NotasVenta.Services;

public interface INotaVentaService
{
    Task<PagedResult<NotaVentaListItemDto>> GetVentasAsync(
        string? search,
        DateTime? startDate,
        DateTime? endDate,
        string? saleStatus,
        string? deliveryStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<NotaVentaDto?> GetNotaVentaByIdAsync(Guid ventaId, CancellationToken cancellationToken = default);

    Task<NotaVentaDto?> GetNotaVentaByFolioAsync(int idVenta, CancellationToken cancellationToken = default);

    Task<NotaVentaDto> ConfirmarEntregaAsync(
        Guid ventaId,
        ConfirmarEntregaRequestDto request,
        Guid? currentUserId,
        string? currentUserName,
        string correlationId,
        string ipAddress,
        CancellationToken cancellationToken = default);
}

