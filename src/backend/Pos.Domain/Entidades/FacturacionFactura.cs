using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionFactura : EntidadBase
{
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;
    public string Serie { get; set; } = "A";
    public string Folio { get; set; } = string.Empty;

    // Relación directa principal con Venta para acceso rápido
    public Guid? VentaId { get; set; }
    public int? IdVenta { get; set; }
    public string? FolioVenta { get; set; }
    public Venta? Venta { get; set; }

    // Estados
    public string Estado { get; set; } = FacturaEstados.Borrador;
    public string? EstadoSat { get; set; }
    public string? EstadoCancelacionSat { get; set; }
    public string? EsCancelableSat { get; set; }

    // Identificadores externos
    public string? FiscalapiInvoiceId { get; set; }
    public string? Uuid { get; set; }

    public DateTime FechaEmisionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaTimbradoUtc { get; set; }
    public DateTime? FechaCancelacionUtc { get; set; }

    // Datos del Emisor
    public string RfcEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string RegimenFiscalEmisor { get; set; } = string.Empty;
    public string CodigoPostalExpedicion { get; set; } = string.Empty;

    // Datos del Receptor
    public string RfcReceptor { get; set; } = string.Empty;
    public string RazonSocialReceptor { get; set; } = string.Empty;
    public string CodigoPostalReceptor { get; set; } = string.Empty;
    public string RegimenFiscalReceptor { get; set; } = SatCatalogosConstantes.RegimenesFiscales.SinObligacionesFiscales;
    public string UsoCfdi { get; set; } = SatCatalogosConstantes.UsosCfdi.GastosEnGeneral;
    public string? EmailReceptor { get; set; }

    // Parámetros fiscales
    public string Moneda { get; set; } = "MXN";
    public decimal TipoCambio { get; set; } = 1.0000m;
    public string FormaPago { get; set; } = SatCatalogosConstantes.FormasPago.Efectivo;
    public string MetodoPago { get; set; } = SatCatalogosConstantes.MetodosPago.PagoEnUnaSolaExhibicion;
    public string TipoComprobante { get; set; } = "I";
    public string Exportacion { get; set; } = "01";
    public string? CondicionesDePago { get; set; }

    // Totales
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalImpuestosTrasladados { get; set; }
    public decimal TotalImpuestosRetenidos { get; set; }
    public decimal Total { get; set; }

    // Trazabilidad técnica
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? TraceIdentifier { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? DetallesRespuesta { get; set; }
    public int? CodigoEstadoHttp { get; set; }

    // Compatibilidad y acceso directo a artefactos y cancelación
    public string? XmlContenido { get; set; }
    public string? PdfRuta { get; set; }
    public string? MotivoCancelacion { get; set; }
    public string? UuidSustitucion { get; set; }
    public DateTime? FechaSolicitudCancelacionUtc { get; set; }
    public string? AcuseCancelacionXml { get; set; }

    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Relaciones hijas
    public ICollection<FacturacionFacturaDetalle> Detalles { get; set; } = new List<FacturacionFacturaDetalle>();
    public ICollection<FacturacionFacturaDetalle> Partidas => Detalles;
    public ICollection<FacturacionFacturaVenta> VentasAsociadas { get; set; } = new List<FacturacionFacturaVenta>();
    public ICollection<FacturacionDocumento> Documentos { get; set; } = new List<FacturacionDocumento>();
    public ICollection<FacturacionIntento> Intentos { get; set; } = new List<FacturacionIntento>();
    public ICollection<FacturacionCancelacion> Cancelaciones { get; set; } = new List<FacturacionCancelacion>();
    public ICollection<FacturacionHistorialEstado> HistorialEstados { get; set; } = new List<FacturacionHistorialEstado>();
}
