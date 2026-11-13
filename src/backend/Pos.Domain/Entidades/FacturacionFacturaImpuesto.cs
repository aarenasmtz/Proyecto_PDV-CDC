using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionFacturaImpuesto : EntidadBase
{
    public Guid FacturaDetalleId { get; set; }
    public FacturacionFacturaDetalle FacturaDetalle { get; set; } = null!;

    public string Tipo { get; set; } = "Traslado"; // "Traslado", "Retencion"
    public string Impuesto { get; set; } = "002"; // "002" para IVA
    public string TipoFactor { get; set; } = "Tasa";
    public decimal Base { get; set; }
    public decimal TasaOCuota { get; set; }
    public decimal Importe { get; set; }
}

