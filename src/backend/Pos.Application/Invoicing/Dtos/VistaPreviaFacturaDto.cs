namespace Pos.Application.Invoicing.Dtos;

public class VistaPreviaFacturaDto
{
    public Guid VentaId { get; set; }
    public int IdVenta { get; set; }
    public string FolioVenta { get; set; } = string.Empty;
    public DateTime FechaVentaUtc { get; set; }

    // Emisor
    public string RfcEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string RegimenFiscalEmisor { get; set; } = string.Empty;
    public string CodigoPostalExpedicion { get; set; } = string.Empty;
    public string Serie { get; set; } = "A";
    public string FolioPropuesto { get; set; } = string.Empty;

    // Receptor pre-cargado
    public Guid? ClienteId { get; set; }
    public string RfcReceptor { get; set; } = string.Empty;
    public string RazonSocialReceptor { get; set; } = string.Empty;
    public string CodigoPostalReceptor { get; set; } = string.Empty;
    public string RegimenFiscalReceptor { get; set; } = string.Empty;
    public string UsoCfdi { get; set; } = string.Empty;
    public string EmailReceptor { get; set; } = string.Empty;

    // Pago SAT
    public string FormaPago { get; set; } = string.Empty;
    public string DescripcionFormaPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = "PUE";
    public string Moneda { get; set; } = "MXN";
    public decimal TipoCambio { get; set; } = 1.0000m;

    // Importes
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal ImpuestoTrasladadoIva { get; set; }
    public decimal Total { get; set; }

    // Conceptos
    public List<PartidaVistaPreviaDto> Conceptos { get; set; } = new();

    // Validaciones
    public bool EsElegibleParaFacturar { get; set; }
    public List<string> Advertencias { get; set; } = new();
    public List<string> Bloqueos { get; set; } = new();
}

public class PartidaVistaPreviaDto
{
    public Guid? ProductoId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ClaveProdServSat { get; set; } = string.Empty;
    public string ClaveUnidadSat { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public string ObjetoImpuestoSat { get; set; } = "02";
    public decimal TasaIva { get; set; } = 0.1600m;
    public decimal BaseIva { get; set; }
    public decimal ImporteIva { get; set; }
}

