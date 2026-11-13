using Pos.Domain.Common;

namespace Pos.Domain.Entidades;

public class FacturacionHistorialEstado : EntidadBase
{
    public Guid FacturaId { get; set; }
    public FacturacionFactura Factura { get; set; } = null!;

    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
}

