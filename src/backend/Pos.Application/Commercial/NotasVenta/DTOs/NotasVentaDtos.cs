namespace Pos.Application.Commercial.NotasVenta.DTOs;

public record NotaVentaListItemDto(
    Guid Id,
    int IdVenta,
    string Folio,
    DateTime FechaCreacionUtc,
    string ClienteNombre,
    int CantidadProductos,
    decimal Total,
    string EstadoVenta,
    string EstadoEntrega,
    bool EsEntregado
);

public record NotaVentaPartidaDto(
    Guid PartidaId,
    string Sku,
    string Descripcion,
    decimal Cantidad,
    string UnidadMedida,
    int PiezasPorCaja,
    int? TotalPiezas,
    decimal PrecioUnitario,
    decimal Descuento,
    decimal Importe
);

public record NotaVentaClienteDto(
    Guid? Id,
    string NombreCompleto,
    string Telefono,
    string Direccion,
    string CiudadEstadoCp,
    string Email,
    string? Rfc
);

public record NotaVentaEntregaDto(
    string Destinatario,
    string DireccionEntrega,
    string TelefonoContacto,
    string TipoEntrega
);

public record NotaVentaTotalesDto(
    decimal Subtotal,
    decimal Descuento,
    bool TieneIva,
    decimal MontoIva,
    decimal Total,
    string MetodoPago,
    decimal MontoPagado,
    decimal SaldoPendiente
);

public record EmpresaComercialConfigDto(
    string NombreComercial,
    string RazonSocial,
    string Slogan,
    string Direccion,
    string CiudadEstadoCp,
    string Telefono,
    string CelularWhatsapp,
    string Email,
    string SitioWeb
);

public record NotaVentaDto(
    Guid VentaId,
    int IdVenta,
    string Folio,
    DateTime FechaHoraUtc,
    string FechaHoraLocal,
    string VendedorNombre,
    string EstadoVenta,
    string EstadoEntrega,
    bool EsEntregado,
    NotaVentaClienteDto Cliente,
    NotaVentaEntregaDto Entrega,
    List<NotaVentaPartidaDto> Partidas,
    NotaVentaTotalesDto Totales,
    string ObservacionesComerciales,
    string CondicionesVenta,
    EmpresaComercialConfigDto Empresa
);

public record ConfirmarEntregaRequestDto(
    bool Entregado = true,
    string? ObservacionesEntrega = null
);

