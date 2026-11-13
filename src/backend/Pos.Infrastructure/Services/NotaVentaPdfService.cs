using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Pos.Application.Commercial.NotasVenta.DTOs;
using Pos.Application.Commercial.NotasVenta.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pos.Infrastructure.Services;

public class NotaVentaPdfService : INotaVentaPdfService
{
    private readonly IWebHostEnvironment? _environment;

    private static readonly CultureInfo CultureMx = new("es-MX");

    // Paleta de colores oficial WPC Bajío
    private const string ColorPrincipal = "#9C4D22";
    private const string ColorSecundario = "#833E1A";
    private const string ColorCafeOscuro = "#6F3215";
    private const string ColorFondo = "#FAF8F5";
    private const string ColorFondoAlt = "#FEF7EB";
    private const string ColorBordes = "#F3E9DA";
    private const string ColorTextoPrincipal = "#3A2312";
    private const string ColorTextoSecundario = "#7A6657";
    private const string ColorEntregado = "#2E7D32";
    private const string ColorFondoEntregado = "#E8F5E9";
    private const string ColorPendiente = "#C25E00";
    private const string ColorFondoPendiente = "#FFF3E0";

    static NotaVentaPdfService()
    {
        // Licencia comunitaria de QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public NotaVentaPdfService(IWebHostEnvironment? environment = null)
    {
        _environment = environment;
    }

    private static void RoundedCard(IContainer container, string bgColor, string borderColor, float borderWidth, float radius, Action<IContainer> content)
    {
        container.Layers(layers =>
        {
            layers.Layer().Svg(size =>
            {
                var w = size.Width.ToString(CultureInfo.InvariantCulture);
                var h = size.Height.ToString(CultureInfo.InvariantCulture);
                var rectW = Math.Max(0, size.Width - borderWidth).ToString(CultureInfo.InvariantCulture);
                var rectH = Math.Max(0, size.Height - borderWidth).ToString(CultureInfo.InvariantCulture);
                var halfBorder = (borderWidth / 2f).ToString(CultureInfo.InvariantCulture);
                var r = radius.ToString(CultureInfo.InvariantCulture);
                var bw = borderWidth.ToString(CultureInfo.InvariantCulture);

                return $@"<svg width=""{w}"" height=""{h}"" viewBox=""0 0 {w} {h}""><rect x=""{halfBorder}"" y=""{halfBorder}"" width=""{rectW}"" height=""{rectH}"" rx=""{r}"" ry=""{r}"" fill=""{bgColor}"" stroke=""{borderColor}"" stroke-width=""{bw}"" /></svg>";
            });
            layers.PrimaryLayer().Element(content);
        });
    }

    public byte[] GenerarPdf(NotaVentaDto nota)
    {
        var logoBytes = ObtenerLogoBytes();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(26, Unit.Point);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(8.5f).FontColor(ColorTextoPrincipal));

                page.Header().Element(c => ComponerEncabezado(c, nota, logoBytes));
                page.Content().Element(c => ComponerCuerpo(c, nota));
                page.Footer().Element(c => ComponerPiePagina(c, nota));
            });
        });

        return document.GeneratePdf();
    }

    private void ComponerEncabezado(IContainer container, NotaVentaDto nota, byte[]? logoBytes)
    {
        container.PaddingBottom(8).Row(row =>
        {
            // Columna 1: Logotipo oficial WPC Bajío
            row.ConstantItem(100).AlignMiddle().Element(imgContainer =>
            {
                if (logoBytes != null && logoBytes.Length > 0)
                {
                    imgContainer.Width(95).Image(logoBytes);
                }
                else
                {
                    RoundedCard(imgContainer, ColorFondoAlt, ColorBordes, 1f, 6f, c =>
                    {
                        c.Width(95).Height(45).AlignCenter().AlignMiddle()
                            .Text("WPC BAJÍO").Bold().FontSize(11).FontColor(ColorPrincipal);
                    });
                }
            });

            // Columna 2: Datos Comerciales de la Empresa
            row.RelativeItem(2.2f).PaddingLeft(10).AlignMiddle().Column(col =>
            {
                col.Item().Text(nota.Empresa.NombreComercial.ToUpperInvariant())
                    .Bold().FontSize(13).FontColor(ColorPrincipal);

                if (!string.IsNullOrWhiteSpace(nota.Empresa.Slogan))
                {
                    col.Item().Text(nota.Empresa.Slogan)
                        .Italic().FontSize(7.5f).FontColor(ColorTextoSecundario);
                }

                col.Item().PaddingTop(2).Text(nota.Empresa.Direccion).FontSize(7.5f).FontColor(ColorTextoPrincipal);
                col.Item().Text(nota.Empresa.CiudadEstadoCp).FontSize(7.5f).FontColor(ColorTextoPrincipal);

                col.Item().Text($"Tel. / WhatsApp: {nota.Empresa.Telefono}").FontSize(7.5f).FontColor(ColorTextoSecundario);

                col.Item().Text($"{nota.Empresa.Email} | {nota.Empresa.SitioWeb}")
                    .FontSize(7.5f).FontColor(ColorTextoSecundario);
            });

            // Columna 3: Recuadro NOTA DE VENTA / FOLIO / FECHA (con bordes redondeados)
            var folioDisplay = nota.IdVenta > 0 ? nota.IdVenta.ToString() : nota.Folio;
            RoundedCard(row.RelativeItem(1.4f).AlignMiddle(), ColorFondoAlt, ColorPrincipal, 1.2f, 6f, card =>
            {
                card.Padding(6).Column(col =>
                {
                    col.Item().AlignCenter().Text("NOTA DE VENTA")
                        .Bold().FontSize(11).FontColor(ColorPrincipal);

                    col.Item().AlignCenter().Text("SIN EFECTOS FISCALES")
                        .Bold().FontSize(7f).FontColor(ColorCafeOscuro);

                    col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(ColorBordes);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Folio:").Bold().FontSize(8.5f).FontColor(ColorCafeOscuro);
                        r.RelativeItem().AlignRight().Text(folioDisplay).Bold().FontSize(10f).FontColor(ColorPrincipal);
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Fecha:").FontSize(7.5f).FontColor(ColorTextoSecundario);
                        r.RelativeItem().AlignRight().Text(nota.FechaHoraLocal).FontSize(7.5f).FontColor(ColorTextoPrincipal);
                    });

                    if (!string.IsNullOrWhiteSpace(nota.VendedorNombre))
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Vendedor:").FontSize(7.5f).FontColor(ColorTextoSecundario);
                            r.RelativeItem().AlignRight().Text(nota.VendedorNombre).FontSize(7.5f).FontColor(ColorTextoPrincipal);
                        });
                    }
                });
            });
        });
    }

    private void ComponerCuerpo(IContainer container, NotaVentaDto nota)
    {
        container.Column(col =>
        {
            // 1. Bloque de Datos del Cliente (Recuadro completo con esquinas redondeadas)
            col.Item().Element(block =>
            {
                RoundedCard(block, ColorFondo, ColorBordes, 0.8f, 6f, card =>
                {
                    card.Padding(8).Column(c =>
                    {
                        c.Item().Text("DATOS DEL CLIENTE").Bold().FontSize(8f).FontColor(ColorSecundario);
                        c.Item().PaddingTop(3).Row(row =>
                        {
                            // Columna izquierda cliente
                            row.RelativeItem(1.2f).Column(det =>
                            {
                                var clienteNombre = string.IsNullOrWhiteSpace(nota.Cliente.NombreCompleto) ? "Público General" : nota.Cliente.NombreCompleto;
                                det.Item().Text(clienteNombre).Bold().FontSize(8.5f).FontColor(ColorTextoPrincipal);

                                if (!string.IsNullOrWhiteSpace(nota.Cliente.Rfc))
                                {
                                    det.Item().Text($"RFC: {nota.Cliente.Rfc}").FontSize(7.5f).FontColor(ColorTextoSecundario);
                                }

                                if (!string.IsNullOrWhiteSpace(nota.Cliente.Telefono))
                                {
                                    det.Item().Text($"Tel: {nota.Cliente.Telefono}").FontSize(7.5f).FontColor(ColorTextoPrincipal);
                                }
                            });

                            // Columna derecha contacto / dirección
                            row.RelativeItem(1.4f).Column(det =>
                            {
                                if (!string.IsNullOrWhiteSpace(nota.Cliente.Email))
                                {
                                    det.Item().Text($"Email: {nota.Cliente.Email}").FontSize(7.5f).FontColor(ColorTextoSecundario);
                                }

                                if (!string.IsNullOrWhiteSpace(nota.Cliente.Direccion))
                                {
                                    det.Item().Text($"Dirección: {nota.Cliente.Direccion}").FontSize(7.5f).FontColor(ColorTextoPrincipal);
                                }

                                if (!string.IsNullOrWhiteSpace(nota.Cliente.CiudadEstadoCp))
                                {
                                    det.Item().Text(nota.Cliente.CiudadEstadoCp).FontSize(7.5f).FontColor(ColorTextoSecundario);
                                }
                            });
                        });
                    });
                });
            });

            col.Item().PaddingVertical(6);

            // 2. Tabla de Detalle de Productos
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3.5f); // Descripción
                    columns.RelativeColumn(0.9f); // Cantidad
                    columns.RelativeColumn(0.9f); // Unidad
                    columns.RelativeColumn(1.0f); // Piezas
                    columns.RelativeColumn(1.2f); // P. Unitario
                    columns.RelativeColumn(1.0f); // Descuento
                    columns.RelativeColumn(1.3f); // Importe
                });

                table.Header(header =>
                {
                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(5)
                        .Text("Descripción comercial").Bold().FontSize(8f).FontColor(Colors.White);

                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                        .Text("Cantidad").Bold().FontSize(8f).FontColor(Colors.White);

                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                        .Text("Unidad").Bold().FontSize(8f).FontColor(Colors.White);

                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(4).AlignCenter()
                        .Text("Piezas").Bold().FontSize(8f).FontColor(Colors.White);

                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(5).AlignRight()
                        .Text("P. Unit.").Bold().FontSize(8f).FontColor(Colors.White);

                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(4).AlignRight()
                        .Text("Descto.").Bold().FontSize(8f).FontColor(Colors.White);

                    header.Cell().Background(ColorPrincipal).PaddingVertical(4).PaddingHorizontal(5).AlignRight()
                        .Text("Importe").Bold().FontSize(8f).FontColor(Colors.White);
                });

                int index = 0;
                foreach (var partida in nota.Partidas)
                {
                    var bg = (index % 2 == 0) ? "#FFFFFF" : ColorFondoAlt;

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(5).Column(p =>
                    {
                        p.Item().Text(partida.Descripcion).Bold().FontSize(8f).FontColor(ColorTextoPrincipal);
                        if (!string.IsNullOrWhiteSpace(partida.Sku))
                        {
                            p.Item().Text($"SKU: {partida.Sku}").FontSize(6.8f).FontColor(ColorTextoSecundario);
                        }
                    });

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(4).AlignCenter().AlignMiddle()
                        .Text(partida.Cantidad.ToString("0.##", CultureMx)).FontSize(8f);

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(4).AlignCenter().AlignMiddle()
                        .Text(partida.UnidadMedida).FontSize(8f);

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(4).AlignCenter().AlignMiddle()
                        .Text(partida.TotalPiezas.HasValue ? $"{partida.TotalPiezas.Value} pz" : "-").FontSize(7.5f).FontColor(ColorTextoSecundario);

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(5).AlignRight().AlignMiddle()
                        .Text(partida.PrecioUnitario.ToString("C2", CultureMx)).FontSize(8f);

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(4).AlignRight().AlignMiddle()
                        .Text(partida.Descuento > 0 ? $"-{partida.Descuento.ToString("C2", CultureMx)}" : "-").FontSize(7.5f).FontColor(ColorTextoSecundario);

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(ColorBordes).PaddingVertical(3).PaddingHorizontal(5).AlignRight().AlignMiddle()
                        .Text(partida.Importe.ToString("C2", CultureMx)).Bold().FontSize(8f).FontColor(ColorTextoPrincipal);

                    index++;
                }
            });

            col.Item().PaddingVertical(8);

            // 3. Sección Inferior: Observaciones, Leyenda CFDI y Totales
            col.Item().Row(row =>
            {
                // Lado Izquierdo: Observaciones y Leyenda (60% ancho)
                row.RelativeItem(1.4f).Column(izq =>
                {
                    // Cuadro de Observaciones / Condiciones (esquinas redondeadas)
                    RoundedCard(izq.Item(), ColorFondo, ColorBordes, 0.8f, 6f, card =>
                    {
                        card.Padding(6).Column(obs =>
                        {
                            obs.Item().Text("OBSERVACIONES / CONDICIONES COMERCIALES:")
                                .Bold().FontSize(7.5f).FontColor(ColorCafeOscuro);

                            if (!string.IsNullOrWhiteSpace(nota.ObservacionesComerciales))
                            {
                                obs.Item().PaddingTop(2).Text(nota.ObservacionesComerciales)
                                    .FontSize(7.5f).FontColor(ColorTextoPrincipal);
                            }

                            if (!string.IsNullOrWhiteSpace(nota.CondicionesVenta))
                            {
                                obs.Item().PaddingTop(2).Text(nota.CondicionesVenta)
                                    .FontSize(6.8f).FontColor(ColorTextoSecundario);
                            }
                        });
                    });

                    izq.Item().PaddingTop(8);

                    // Leyenda Obligatoria
                    izq.Item().Text("Documento comercial sin efectos fiscales. No sustituye un comprobante fiscal digital por Internet (CFDI).")
                        .Italic().FontSize(6.8f).FontColor(ColorTextoSecundario);
                });

                row.ConstantItem(10);

                // Lado Derecho: Totales de la Venta (40% ancho, esquinas redondeadas)
                RoundedCard(row.RelativeItem(1.0f), ColorFondo, ColorPrincipal, 1.2f, 6f, card =>
                {
                    card.Padding(7).Column(der =>
                    {
                        der.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(8f).FontColor(ColorTextoSecundario);
                            r.RelativeItem().AlignRight().Text(nota.Totales.Subtotal.ToString("C2", CultureMx)).FontSize(8f);
                        });

                        if (nota.Totales.Descuento > 0)
                        {
                            der.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text("Descuento:").FontSize(8f).FontColor(ColorTextoSecundario);
                                r.RelativeItem().AlignRight().Text($"-{nota.Totales.Descuento.ToString("C2", CultureMx)}")
                                    .FontSize(8f).FontColor(ColorCafeOscuro);
                            });
                        }

                        if (nota.Totales.TieneIva && nota.Totales.MontoIva > 0)
                        {
                            der.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text("IVA (16%):").FontSize(8f).FontColor(ColorTextoSecundario);
                                r.RelativeItem().AlignRight().Text(nota.Totales.MontoIva.ToString("C2", CultureMx)).FontSize(8f);
                            });
                        }

                        der.Item().PaddingVertical(3).LineHorizontal(1f).LineColor(ColorPrincipal);

                        der.Item().Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").Bold().FontSize(12f).FontColor(ColorPrincipal);
                            r.RelativeItem().AlignRight().Text(nota.Totales.Total.ToString("C2", CultureMx))
                                .Bold().FontSize(12f).FontColor(ColorPrincipal);
                        });

                        der.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(ColorBordes);

                        der.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Forma de pago:").FontSize(7.5f).FontColor(ColorTextoSecundario);
                            r.RelativeItem().AlignRight().Text(nota.Totales.MetodoPago).Bold().FontSize(7.5f).FontColor(ColorTextoPrincipal);
                        });

                        der.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Pagado:").FontSize(7.5f).FontColor(ColorTextoSecundario);
                            r.RelativeItem().AlignRight().Text(nota.Totales.MontoPagado.ToString("C2", CultureMx)).FontSize(7.5f);
                        });

                        if (nota.Totales.SaldoPendiente > 0)
                        {
                            der.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Saldo Pendiente:").Bold().FontSize(8f).FontColor(ColorPendiente);
                                r.RelativeItem().AlignRight().Text(nota.Totales.SaldoPendiente.ToString("C2", CultureMx))
                                    .Bold().FontSize(8f).FontColor(ColorPendiente);
                            });
                        }
                    });
                });
            });
        });
    }

    private void ComponerPiePagina(IContainer container, NotaVentaDto nota)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(ColorBordes);
            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem().Text("WPC Bajío — Calidad y elegancia en Lambrín y acabados para interiores y exteriores.")
                    .Italic().FontSize(7f).FontColor(ColorTextoSecundario);

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(7f).FontColor(ColorTextoSecundario);
                    text.CurrentPageNumber().FontSize(7f).FontColor(ColorTextoSecundario);
                    text.Span(" de ").FontSize(7f).FontColor(ColorTextoSecundario);
                    text.TotalPages().FontSize(7f).FontColor(ColorTextoSecundario);
                });
            });
        });
    }

    private byte[]? ObtenerLogoBytes()
    {
        try
        {
            var rutasPosibles = new List<string>();

            if (_environment != null && !string.IsNullOrWhiteSpace(_environment.WebRootPath))
            {
                rutasPosibles.Add(Path.Combine(_environment.WebRootPath, "images", "logo_wpc_bajio.jpeg"));
                rutasPosibles.Add(Path.Combine(_environment.WebRootPath, "logo_wpc_bajio.jpeg"));
            }

            rutasPosibles.Add(Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "logo_wpc_bajio.jpeg"));
            rutasPosibles.Add(Path.Combine(AppContext.BaseDirectory, "wwwroot", "logo_wpc_bajio.jpeg"));
            rutasPosibles.Add(Path.GetFullPath("src/backend/Pos.Api/wwwroot/images/logo_wpc_bajio.jpeg"));
            rutasPosibles.Add(Path.GetFullPath("src/frontend/pos-web/public/logo_wpc_bajio.jpeg"));

            foreach (var ruta in rutasPosibles)
            {
                if (File.Exists(ruta))
                {
                    return File.ReadAllBytes(ruta);
                }
            }
        }
        catch
        {
            // Fallback silencioso si el archivo de imagen no está disponible
        }

        return null;
    }
}
