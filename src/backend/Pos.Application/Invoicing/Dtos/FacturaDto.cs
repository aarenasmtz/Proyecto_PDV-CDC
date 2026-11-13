using Pos.Domain.Common;

namespace Pos.Application.Invoicing.Dtos;

public class FacturaDto
{
    public Guid Id { get; set; }
    public Guid? VentaId { get; set; }
    public int? IdVenta { get; set; }
    public string? FolioVenta { get; set; } = string.Empty;

    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public string FolioCompleto => string.IsNullOrEmpty(Serie) ? Folio : $"{Serie}-{Folio}";
    public string Ambiente { get; set; } = FacturacionAmbientes.QA;

    public string Estado { get; set; } = FacturaEstados.Borrador;
    public string? EstadoSat { get; set; }
    public string? EstadoCancelacionSat { get; set; }

    public string? FiscalapiInvoiceId { get; set; }
    public string? Uuid { get; set; }

    public DateTime FechaEmisionUtc { get; set; }
    public DateTime? FechaTimbradoUtc { get; set; }

    public string RfcEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string RegimenFiscalEmisor { get; set; } = string.Empty;
    public string CodigoPostalExpedicion { get; set; } = string.Empty;

    public string RfcReceptor { get; set; } = string.Empty;
    public string RazonSocialReceptor { get; set; } = string.Empty;
    public string UsoCfdi { get; set; } = string.Empty;
    public string RegimenFiscalReceptor { get; set; } = string.Empty;
    public string CodigoPostalReceptor { get; set; } = string.Empty;
    public string EmailReceptor { get; set; } = string.Empty;

    public string Moneda { get; set; } = "MXN";
    public decimal TipoCambio { get; set; } = 1.0000m;
    public string FormaPago { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = "I";
    public string Exportacion { get; set; } = "01";

    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal TotalImpuestosTrasladados { get; set; }
    public decimal TotalImpuestosRetenidos { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;
    public string? TraceIdentifier { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? DetallesRespuesta { get; set; }
    public int? CodigoEstadoHttp { get; set; }

    public bool TieneXml { get; set; }
    public bool TienePdf { get; set; }
    public bool TieneAcuseCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }
    public string? UuidSustitucion { get; set; }
    public DateTime? FechaSolicitudCancelacionUtc { get; set; }
    public DateTime? FechaCancelacionUtc { get; set; }

    public List<FacturaPartidaDto> Partidas { get; set; } = new();
    public List<FacturaHistorialEstadoDto> HistorialEstados { get; set; } = new();
}

public class FacturaPartidaDto
{
    public Guid Id { get; set; }
    public Guid? ProductoId { get; set; }
    public string ClaveProdServ { get; set; } = string.Empty;
    public string ClaveUnidad { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Importe { get; set; }
    public decimal Descuento { get; set; }
    public string ObjetoImp { get; set; } = "02";
    public decimal TasaOcuotaIva { get; set; } = 0.1600m;
    public decimal BaseIva { get; set; }
    public decimal ImporteIva { get; set; }
}

public class FacturaHistorialEstadoDto
{
    public Guid Id { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
}

