using Microsoft.EntityFrameworkCore;
using Pos.Application.Commercial.NotasVenta.DTOs;
using Pos.Application.Common.Interfaces;
using Pos.Domain.Common;
using Pos.Domain.Entidades;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Services;
using QuestPDF.Fluent;
using Xunit;

namespace Pos.Application.Tests.Commercial;

public class NotaVentaTests
{
    private class FakeAuditLogService : IAuditLogService
    {
        public List<(string action, string entityId, string? reason)> Logs { get; } = new();

        public Task LogAsync(
            string correlationId,
            Guid? userId,
            string action,
            string entityName,
            string? entityId,
            string? oldValuesJson,
            string? newValuesJson,
            string ipAddress,
            string? reason = null,
            string? module = null,
            string? eventType = null,
            string? resultStatus = null,
            CancellationToken cancellationToken = default)
        {
            Logs.Add((action, entityId ?? "", reason));
            return Task.CompletedTask;
        }
    }

    private PosDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PosDbContext(options);
    }

    [Fact]
    public async Task GetVentasAsync_ReturnsPagedSales()
    {
        using var context = CreateInMemoryContext();
        var audit = new FakeAuditLogService();
        var service = new NotaVentaApplicationService(context, audit);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nombre = "Juan",
            Apellido = "Pérez",
            Telefono = "4771112233",
            Direccion = "Av. Siempre Viva 123",
            Ciudad = "León",
            Estado = "Guanajuato",
            CodigoPostal = "37000"
        };
        context.Customers.Add(cliente);

        var venta1 = new Venta
        {
            Id = Guid.NewGuid(),
            IdVenta = 101,
            NumeroFolio = "V-000101",
            ClienteId = cliente.Id,
            Cliente = cliente,
            SubTotal = 3000m,
            MontoIva = 480m,
            MontoTotal = 3480m,
            Estado = SaleStatuses.Completed,
            Notas = "Entrega estándar",
            FechaCreacionUtc = DateTime.UtcNow.AddHours(-2)
        };

        var venta2 = new Venta
        {
            Id = Guid.NewGuid(),
            IdVenta = 102,
            NumeroFolio = "V-000102",
            ClienteId = cliente.Id,
            Cliente = cliente,
            SubTotal = 1500m,
            MontoTotal = 1500m,
            Estado = SaleStatuses.Completed,
            Notas = "Venta rápida",
            FechaCreacionUtc = DateTime.UtcNow.AddHours(-1)
        };

        context.Sales.AddRange(venta1, venta2);
        await context.SaveChangesAsync();

        var result = await service.GetVentasAsync(null, null, null, "ALL", "ALL", 1, 25);

        Assert.Equal(2, result.TotalItems);
        var item102 = result.Items.First(i => i.IdVenta == 102);
        var item101 = result.Items.First(i => i.IdVenta == 101);

        Assert.Equal("Juan Pérez", item102.ClienteNombre);
        Assert.Equal("102", item102.Folio);
        Assert.Equal("101", item101.Folio);
    }

    [Fact]
    public void TestDynamicSvgRoundedCard()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(20);
                page.Content().Column(col =>
                {
                    col.Item().Layers(layers =>
                    {
                        layers.Layer().Svg(size => $@"<svg width=""{size.Width.ToString(System.Globalization.CultureInfo.InvariantCulture)}"" height=""{size.Height.ToString(System.Globalization.CultureInfo.InvariantCulture)}"" viewBox=""0 0 {size.Width.ToString(System.Globalization.CultureInfo.InvariantCulture)} {size.Height.ToString(System.Globalization.CultureInfo.InvariantCulture)}""><rect x=""1"" y=""1"" width=""{(size.Width - 2).ToString(System.Globalization.CultureInfo.InvariantCulture)}"" height=""{(size.Height - 2).ToString(System.Globalization.CultureInfo.InvariantCulture)}"" rx=""8"" ry=""8"" fill=""#FEF7EB"" stroke=""#9C4D22"" stroke-width=""1.5"" /></svg>");
                        layers.PrimaryLayer().Padding(10).Column(c =>
                        {
                            c.Item().Text("Card Title").Bold().FontSize(12).FontColor("#9C4D22");
                            c.Item().Text("This is content inside the rounded card.");
                        });
                    });
                });
            });
        });

        var bytes = doc.GeneratePdf();
        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void GenerarPdf_CreatesValidPdfDocument()
    {
        var pdfService = new NotaVentaPdfService(null);

        var nota = new NotaVentaDto(
            VentaId: Guid.NewGuid(),
            IdVenta: 125,
            Folio: "125",
            FechaHoraUtc: DateTime.UtcNow,
            FechaHoraLocal: "08/11/2026 10:30",
            VendedorNombre: "Vendedor Test",
            EstadoVenta: "Completada",
            EstadoEntrega: "ENTREGADO",
            EsEntregado: true,
            Cliente: new NotaVentaClienteDto(
                Id: Guid.NewGuid(),
                NombreCompleto: "Juan Pérez",
                Telefono: "477 807 2768",
                Direccion: "Av. Principal 456",
                CiudadEstadoCp: "León, Guanajuato C.P. 37000",
                Email: "juan@example.com",
                Rfc: "PERJ850101ABC"),
            Entrega: new NotaVentaEntregaDto(
                Destinatario: "Juan Pérez",
                DireccionEntrega: "Av. Principal 456, León",
                TelefonoContacto: "477 807 2768",
                TipoEntrega: "Recolección en Mostrador"),
            Partidas: new List<NotaVentaPartidaDto>
            {
                new(
                    PartidaId: Guid.NewGuid(),
                    Sku: "LAM-ROB-01",
                    Descripcion: "Lambrín WPC Roble Claro 2.90m",
                    Cantidad: 10,
                    UnidadMedida: "Caja",
                    PiezasPorCaja: 14,
                    TotalPiezas: 140,
                    PrecioUnitario: 350m,
                    Descuento: 0m,
                    Importe: 3500m
                )
            },
            Totales: new NotaVentaTotalesDto(
                Subtotal: 3500m,
                Descuento: 0m,
                TieneIva: true,
                MontoIva: 560m,
                Total: 4060m,
                MetodoPago: "Efectivo",
                MontoPagado: 4060m,
                SaldoPendiente: 0m),
            ObservacionesComerciales: "Entregar en segundo piso",
            CondicionesVenta: "Sin devoluciones en material cortado",
            Empresa: new EmpresaComercialConfigDto(
                NombreComercial: "WPC BAJÍO",
                RazonSocial: "WPC Bajío Acabados Arquitectónicos",
                Slogan: "Calidad y Vanguardia en Lambrín Decorativo y Acabados de WPC",
                Direccion: "Blvd. Adolfo López Mateos 2826, Col. El Rosario",
                CiudadEstadoCp: "León, Guanajuato, C.P. 37125",
                Telefono: "477 807 2768",
                CelularWhatsapp: "477 807 2768",
                Email: "contacto@wpcbajio.com",
                SitioWeb: "www.wpcbajio.com")
        );

        var bytes = pdfService.GenerarPdf(nota);

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
        var header = System.Text.Encoding.ASCII.GetString(bytes.Take(4).ToArray());
        Assert.Equal("%PDF", header);
    }
}
