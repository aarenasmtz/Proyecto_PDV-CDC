using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionFacturaDetalle : EntidadBase
{
    public Guid FacturaId { get; set; }
    public FacturacionFactura Factura { get; set; } = null!;

    public int NumeroPartida { get; set; }
    public Guid? ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string? Sku { get; set; }
    public string ClaveProdServ { get; set; } = string.Empty;
    public string ClaveUnidad { get; set; } = string.Empty;
    public string? Unidad { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public string ObjetoImp { get; set; } = "02";

    public decimal BaseIva { get; set; }
    public decimal TasaOcuotaIva { get; set; } = 0.160000m;
    public decimal ImporteIva { get; set; }

    public ICollection<FacturacionFacturaImpuesto> Impuestos { get; set; } = new List<FacturacionFacturaImpuesto>();
}
