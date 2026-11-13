using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Pos.Application.Commercial.NotasVenta.DTOs;
using Pos.Application.Commercial.NotasVenta.Services;
using Pos.Application.Common.Interfaces;
using Pos.Application.Common.Models;
using Pos.Domain.Common;
using Pos.Domain.Entidades;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Services;

public class NotaVentaApplicationService : INotaVentaService
{
    private readonly PosDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private static readonly CultureInfo CultureMx = new("es-MX");

    public NotaVentaApplicationService(
        PosDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<NotaVentaListItemDto>> GetVentasAsync(
        string? search,
        DateTime? startDate,
        DateTime? endDate,
        string? saleStatus,
        string? deliveryStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Include(s => s.Cliente)
            .Include(s => s.Partidas)
            .AsQueryable();

        // 1. Filtro por búsqueda de texto
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.NumeroFolio.ToLower().Contains(term) ||
                (s.Cliente != null && (
                    s.Cliente.Nombre.ToLower().Contains(term) ||
                    s.Cliente.Apellido.ToLower().Contains(term) ||
                    (s.Cliente.NombreEmpresa != null && s.Cliente.NombreEmpresa.ToLower().Contains(term)) ||
                    (s.Cliente.Rfc != null && s.Cliente.Rfc.ToLower().Contains(term))
                ))
            );
        }

        // 2. Filtro por fecha
        if (startDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(s => s.FechaCreacionUtc >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(s => s.FechaCreacionUtc <= endUtc);
        }

        // 3. Filtro por estado de venta (soporta tanto los valores reales en español como alias estándar)
        if (!string.IsNullOrWhiteSpace(saleStatus) && !saleStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var targetStatus = saleStatus.Trim();
            var aliases = new List<string> { targetStatus };

            if (targetStatus.Equals("Completada", StringComparison.OrdinalIgnoreCase) || targetStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                aliases.AddRange(new[] { SaleStatuses.Completed, "Completed" });
            }
            else if (targetStatus.Equals("Cancelada", StringComparison.OrdinalIgnoreCase) || targetStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                aliases.AddRange(new[] { SaleStatuses.Cancelled, "Cancelled" });
            }
            else if (targetStatus.Equals("PendientePago", StringComparison.OrdinalIgnoreCase) || targetStatus.Equals("PendingPayment", StringComparison.OrdinalIgnoreCase))
            {
                aliases.AddRange(new[] { SaleStatuses.PendingPayment, "PendingPayment" });
            }
            else if (targetStatus.Equals("ApartadoPagado", StringComparison.OrdinalIgnoreCase) || targetStatus.Equals("DepositPaid", StringComparison.OrdinalIgnoreCase))
            {
                aliases.AddRange(new[] { SaleStatuses.DepositPaid, "DepositPaid" });
            }
            else if (targetStatus.Equals("DevolucionParcial", StringComparison.OrdinalIgnoreCase) || targetStatus.Equals("PartiallyReturned", StringComparison.OrdinalIgnoreCase))
            {
                aliases.AddRange(new[] { SaleStatuses.PartiallyReturned, "PartiallyReturned" });
            }
            else if (targetStatus.Equals("Devuelta", StringComparison.OrdinalIgnoreCase) || targetStatus.Equals("Returned", StringComparison.OrdinalIgnoreCase))
            {
                aliases.AddRange(new[] { SaleStatuses.Returned, "Returned" });
            }

            var uniqueAliases = aliases.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            query = query.Where(s => uniqueAliases.Contains(s.Estado));
        }

        // 4. Filtro por estado de entrega
        if (!string.IsNullOrWhiteSpace(deliveryStatus) && !deliveryStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            if (deliveryStatus.Equals("ENTREGADO", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.Notas.Contains("[ESTATUS: Entregado]"));
            }
            else if (deliveryStatus.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => !s.Notas.Contains("[ESTATUS: Entregado]"));
            }
        }

        // Orden descendente por fecha
        query = query.OrderByDescending(s => s.FechaCreacionUtc);

        var totalItems = await query.CountAsync(cancellationToken);
        var effectivePage = pageNumber > 0 ? pageNumber : 1;
        var effectivePageSize = pageSize > 0 ? pageSize : 25;

        var sales = await query
            .Skip((effectivePage - 1) * effectivePageSize)
            .Take(effectivePageSize)
            .ToListAsync(cancellationToken);

        var items = sales.Select(s =>
        {
            var esEntregado = s.Notas.Contains("[ESTATUS: Entregado]");
            var estadoEntrega = esEntregado ? "ENTREGADO" : "PENDIENTE DE ENTREGA";

            string clienteNombre = "Público General";
            if (s.Cliente != null)
            {
                var fullName = $"{s.Cliente.Nombre} {s.Cliente.Apellido}".Trim();
                clienteNombre = !string.IsNullOrWhiteSpace(fullName) ? fullName : (s.Cliente.NombreEmpresa ?? "Público General");
            }

            var cantidadProductos = s.Partidas.Count;
            var folioVisible = s.IdVenta > 0 ? s.IdVenta.ToString() : (string.IsNullOrWhiteSpace(s.NumeroFolio) ? $"V-{s.IdVenta:D6}" : s.NumeroFolio);

            return new NotaVentaListItemDto(
                Id: s.Id,
                IdVenta: s.IdVenta,
                Folio: folioVisible,
                FechaCreacionUtc: s.FechaCreacionUtc,
                ClienteNombre: clienteNombre,
                CantidadProductos: cantidadProductos,
                Total: s.MontoTotal,
                EstadoVenta: s.Estado,
                EstadoEntrega: estadoEntrega,
                EsEntregado: esEntregado
            );
        }).ToList();

        return new PagedResult<NotaVentaListItemDto>(items, totalItems, effectivePage, effectivePageSize);
    }

    public async Task<NotaVentaDto?> GetNotaVentaByIdAsync(Guid ventaId, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Cliente)
            .Include(s => s.Usuario)
                .ThenInclude(u => u!.Empleado)
            .Include(s => s.Partidas)
                .ThenInclude(p => p.Producto)
            .Include(s => s.Abonos)
            .FirstOrDefaultAsync(s => s.Id == ventaId, cancellationToken);

        if (sale == null) return null;

        return MapearNotaVenta(sale);
    }

    public async Task<NotaVentaDto?> GetNotaVentaByFolioAsync(int idVenta, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Cliente)
            .Include(s => s.Usuario)
                .ThenInclude(u => u!.Empleado)
            .Include(s => s.Partidas)
                .ThenInclude(p => p.Producto)
            .Include(s => s.Abonos)
            .FirstOrDefaultAsync(s => s.IdVenta == idVenta, cancellationToken);

        if (sale == null) return null;

        return MapearNotaVenta(sale);
    }

    public async Task<NotaVentaDto> ConfirmarEntregaAsync(
        Guid ventaId,
        ConfirmarEntregaRequestDto request,
        Guid? currentUserId,
        string? currentUserName,
        string correlationId,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales
            .Include(s => s.Cliente)
            .Include(s => s.Usuario)
                .ThenInclude(u => u!.Empleado)
            .Include(s => s.Partidas)
                .ThenInclude(p => p.Producto)
            .Include(s => s.Abonos)
            .FirstOrDefaultAsync(s => s.Id == ventaId, cancellationToken);

        if (sale == null)
        {
            throw new KeyNotFoundException($"Venta con ID {ventaId} no encontrada.");
        }

        var oldNotas = sale.Notas;

        if (request.Entregado)
        {
            // Remover cualquier estatus previo de entrega
            sale.Notas = Regex.Replace(sale.Notas, @"\[ESTATUS:\s*[^\]]+\]", "").Trim();

            var stamp = $"[ESTATUS: Entregado]";
            if (!string.IsNullOrWhiteSpace(request.ObservacionesEntrega))
            {
                stamp += $" [ENTREGA_NOTAS: {request.ObservacionesEntrega.Trim()}]";
            }

            sale.Notas = string.IsNullOrWhiteSpace(sale.Notas) ? stamp : $"{sale.Notas} | {stamp}";
        }
        else
        {
            // Remover estatus de entregado
            sale.Notas = Regex.Replace(sale.Notas, @"\[ESTATUS:\s*Entregado\]", "").Trim();
            sale.Notas = Regex.Replace(sale.Notas, @"\[ENTREGA_NOTAS:\s*[^\]]+\]", "").Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Registro de Auditoría
        await _auditLogService.LogAsync(
            correlationId: correlationId,
            userId: currentUserId,
            action: request.Entregado ? "CONFIRMAR_ENTREGA" : "DESMARCAR_ENTREGA",
            entityName: "Venta",
            entityId: sale.Id.ToString(),
            oldValuesJson: System.Text.Json.JsonSerializer.Serialize(new { Notas = oldNotas }),
            newValuesJson: System.Text.Json.JsonSerializer.Serialize(new { Notas = sale.Notas, Entregado = request.Entregado }),
            ipAddress: ipAddress,
            reason: request.ObservacionesEntrega ?? (request.Entregado ? "Confirmación de entrega de mercancía" : "Mercancía devuelta a pendiente de entrega"),
            module: "NotasVenta",
            eventType: "CommercialDelivery",
            resultStatus: "Success",
            cancellationToken: cancellationToken);

        return MapearNotaVenta(sale);
    }

    private NotaVentaDto MapearNotaVenta(Venta sale)
    {
        var esEntregado = sale.Notas.Contains("[ESTATUS: Entregado]");
        var estadoEntrega = esEntregado ? "ENTREGADO" : "PENDIENTE DE ENTREGA";

        // Vendedor / Cajero
        string vendedor = "Caja Principal";
        if (sale.Usuario != null)
        {
            if (sale.Usuario.Empleado != null)
            {
                var nombreEmp = $"{sale.Usuario.Empleado.Nombre} {sale.Usuario.Empleado.Apellido}".Trim();
                vendedor = !string.IsNullOrWhiteSpace(nombreEmp) ? nombreEmp : sale.Usuario.NombreUsuario;
            }
            else if (!string.IsNullOrWhiteSpace(sale.Usuario.NombreUsuario))
            {
                vendedor = sale.Usuario.NombreUsuario;
            }
        }

        // Mapeo Cliente
        NotaVentaClienteDto clienteDto;
        if (sale.Cliente != null)
        {
            var ciudadEstadoCp = string.Join(", ", new[]
            {
                sale.Cliente.Ciudad,
                sale.Cliente.Estado,
                string.IsNullOrWhiteSpace(sale.Cliente.CodigoPostal) ? null : $"C.P. {sale.Cliente.CodigoPostal}"
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var fullName = $"{sale.Cliente.Nombre} {sale.Cliente.Apellido}".Trim();
            var nombreCliente = !string.IsNullOrWhiteSpace(fullName) ? fullName : (sale.Cliente.NombreEmpresa ?? "Público General");

            clienteDto = new NotaVentaClienteDto(
                Id: sale.Cliente.Id,
                NombreCompleto: nombreCliente,
                Telefono: sale.Cliente.Telefono,
                Direccion: sale.Cliente.Direccion,
                CiudadEstadoCp: ciudadEstadoCp,
                Email: sale.Cliente.Email,
                Rfc: sale.Cliente.Rfc ?? sale.Cliente.RazonSocialFiscal
            );
        }
        else
        {
            clienteDto = new NotaVentaClienteDto(
                Id: null,
                NombreCompleto: "Público General",
                Telefono: "Sin registrar",
                Direccion: "Venta de mostrador",
                CiudadEstadoCp: "León, Guanajuato",
                Email: "contacto@wpcbajio.com",
                Rfc: "XAXX010101000"
            );
        }

        // Mapeo Entrega
        var isPickup = sale.Notas.Contains("Recolección en Tienda", StringComparison.OrdinalIgnoreCase);
        var tipoEntrega = isPickup ? "Recolección en Mostrador WPC Bajío" : "Entrega a domicilio / Envíos";
        
        var destinatario = !string.IsNullOrWhiteSpace(clienteDto.NombreCompleto) ? clienteDto.NombreCompleto : "Mostrador WPC Bajío";
        var direccionEntrega = isPickup || string.IsNullOrWhiteSpace(clienteDto.Direccion)
            ? "Recoger en sucursal WPC Bajío: Blvd. José María Morelos #321, León, Gto."
            : $"{clienteDto.Direccion}, {clienteDto.CiudadEstadoCp}".Trim().TrimEnd(',');

        var entregaDto = new NotaVentaEntregaDto(
            Destinatario: destinatario,
            DireccionEntrega: direccionEntrega,
            TelefonoContacto: clienteDto.Telefono,
            TipoEntrega: tipoEntrega
        );

        // Mapeo Partidas con histórico real (sin recalcular)
        var partidasDto = sale.Partidas.Select(p =>
        {
            var sku = p.Producto?.Sku ?? "WPC-GEN";
            var descripcion = p.Producto?.Nombre ?? "Lambrín Decorativo";
            var unidad = p.Producto?.UnidadMedida ?? "Pza";
            var piezasPorCaja = p.Producto?.PiezasPorCaja ?? 1;

            int? totalPiezas = null;
            if (piezasPorCaja > 1 || unidad.Equals("Caja", StringComparison.OrdinalIgnoreCase))
            {
                totalPiezas = Convert.ToInt32(p.Cantidad * piezasPorCaja);
            }
            else if (unidad.Equals("Pza", StringComparison.OrdinalIgnoreCase) || unidad.Equals("Pieza", StringComparison.OrdinalIgnoreCase))
            {
                totalPiezas = Convert.ToInt32(p.Cantidad);
            }

            return new NotaVentaPartidaDto(
                PartidaId: p.Id,
                Sku: sku,
                Descripcion: descripcion,
                Cantidad: p.Cantidad,
                UnidadMedida: unidad,
                PiezasPorCaja: piezasPorCaja,
                TotalPiezas: totalPiezas,
                PrecioUnitario: p.PrecioUnitario,
                Descuento: p.MontoDescuento,
                Importe: p.PrecioTotal
            );
        }).ToList();

        // Mapeo Forma de Pago
        string metodoPago = "Efectivo";
        if (sale.MontoEfectivo > 0 && sale.MontoTarjeta == 0 && sale.MontoTransferencia == 0)
        {
            metodoPago = "Efectivo";
        }
        else if (sale.MontoTarjeta > 0 && sale.MontoEfectivo == 0 && sale.MontoTransferencia == 0)
        {
            metodoPago = "Tarjeta Débito/Crédito";
        }
        else if (sale.MontoTransferencia > 0 && sale.MontoEfectivo == 0 && sale.MontoTarjeta == 0)
        {
            metodoPago = "Transferencia Electrónica";
        }
        else if ((sale.MontoEfectivo > 0 && sale.MontoTarjeta > 0) || (sale.MontoEfectivo > 0 && sale.MontoTransferencia > 0) || (sale.MontoTarjeta > 0 && sale.MontoTransferencia > 0))
        {
            metodoPago = "Pago Mixto";
        }
        else if (!string.IsNullOrWhiteSpace(sale.TipoPago))
        {
            metodoPago = sale.TipoPago;
        }

        var pagado = Math.Max(0m, sale.MontoTotal - sale.SaldoPendiente);

        var totalesDto = new NotaVentaTotalesDto(
            Subtotal: sale.SubTotal,
            Descuento: sale.MontoDescuento,
            TieneIva: sale.MontoIva > 0,
            MontoIva: sale.MontoIva,
            Total: sale.MontoTotal,
            MetodoPago: metodoPago,
            MontoPagado: pagado,
            SaldoPendiente: sale.SaldoPendiente
        );

        // Limpieza de notas para observaciones
        var cleanNotas = Regex.Replace(sale.Notas, @"\[ESTATUS:\s*[^\]]+\]", "").Trim();
        cleanNotas = Regex.Replace(cleanNotas, @"\[GUIA:\s*[^\]]+\]", "").Trim();
        cleanNotas = Regex.Replace(cleanNotas, @"\[E-COMMERCE\]", "").Trim();
        cleanNotas = cleanNotas.Trim('|', ' ').Trim();

        var empresaDto = new EmpresaComercialConfigDto(
            NombreComercial: "WPC BAJÍO",
            RazonSocial: "WPC Bajío Acabados Arquitectónicos",
            Slogan: "Calidad y Vanguardia en Lambrín Decorativo y Acabados de WPC",
            Direccion: "Blvd. Adolfo López Mateos 2826, Col. El Rosario",
            CiudadEstadoCp: "León de los Aldama, Guanajuato, C.P. 37125",
            Telefono: "477 807 2768",
            CelularWhatsapp: "477 807 2768",
            Email: "contacto@wpcbajio.com",
            SitioWeb: "www.wpcbajio.com"
        );

        var condiciones = "1. Precios en Moneda Nacional (MXN). 2. Verifique su material antes de retirarlo. No se aceptan cambios ni devoluciones de mercancía cortada, manipulada o instalada. 3. Para cualquier reclamación conserve esta nota de venta.";

        var fechaLocal = sale.FechaCreacionUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureMx);
        var folioVisible = sale.IdVenta > 0 ? sale.IdVenta.ToString() : (string.IsNullOrWhiteSpace(sale.NumeroFolio) ? $"V-{sale.IdVenta:D6}" : sale.NumeroFolio);

        return new NotaVentaDto(
            VentaId: sale.Id,
            IdVenta: sale.IdVenta,
            Folio: folioVisible,
            FechaHoraUtc: sale.FechaCreacionUtc,
            FechaHoraLocal: fechaLocal,
            VendedorNombre: vendedor,
            EstadoVenta: sale.Estado,
            EstadoEntrega: estadoEntrega,
            EsEntregado: esEntregado,
            Cliente: clienteDto,
            Entrega: entregaDto,
            Partidas: partidasDto,
            Totales: totalesDto,
            ObservacionesComerciales: cleanNotas,
            CondicionesVenta: condiciones,
            Empresa: empresaDto
        );
    }
}
