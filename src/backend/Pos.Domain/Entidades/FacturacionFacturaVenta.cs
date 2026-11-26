using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionFacturaVenta : EntidadBase
{
    public Guid FacturaId { get; set; }
    public FacturacionFactura Factura { get; set; } = null!;

    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public string? FolioVenta { get; set; }
    public int? IdVenta { get; set; }
    public decimal MontoAsociado { get; set; }
    public DateTime FechaAsociacionUtc { get; set; } = DateTime.UtcNow;
}

