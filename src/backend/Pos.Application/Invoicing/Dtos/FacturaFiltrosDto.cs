using Pos.Application.Common.Models;

namespace Pos.Application.Invoicing.Dtos;

public class FacturaFiltrosDto : PagedRequest
{
    public string? Busqueda { get; set; }
    public string? Estado { get; set; }
    public string? Ambiente { get; set; }
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public string? RfcReceptor { get; set; }
    public int? IdVenta { get; set; }
}

