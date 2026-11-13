using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Fiscalapi.Common;
using Fiscalapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pos.Application.Common.Interfaces;
using Pos.Application.Common.Models;
using Pos.Application.Invoicing.Dtos;
using Pos.Application.Invoicing.Services;
using Pos.Domain.Common;
using Pos.Domain.Entidades;
using Pos.Infrastructure.Invoicing.Clients;
using Pos.Infrastructure.Invoicing.Logging;
using Pos.Infrastructure.Invoicing.Options;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Invoicing.Services;

public class FacturacionApplicationService : IFacturacionApplicationService
{
    private readonly PosDbContext _dbContext;
    private readonly IFiscalApiClientAdapter _fiscalClient;
    private readonly FacturacionSettings _facturacionSettings;
    private readonly FiscalapiSettings _fiscalapiSettings;
    private readonly IAuditLogService _auditLogService;
    private readonly IFacturacionLogService _logService;
    private readonly ILogger<FacturacionApplicationService> _logger;

    private static readonly Regex RfcFisicaRegex = new(@"^[A-ZÑ&]{4}\d{6}[A-Z0-9]{3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex RfcMoralRegex = new(@"^[A-ZÑ&]{3}\d{6}[A-Z0-9]{3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public FacturacionApplicationService(
        PosDbContext dbContext,
        IFiscalApiClientAdapter fiscalClient,
        IOptions<FacturacionSettings> facturacionOptions,
        IOptions<FiscalapiSettings> fiscalapiOptions,
        IAuditLogService auditLogService,
        IFacturacionLogService logService,
        ILogger<FacturacionApplicationService> logger)
    {
        _dbContext = dbContext;
        _fiscalClient = fiscalClient;
        _facturacionSettings = facturacionOptions.Value;
        _fiscalapiSettings = fiscalapiOptions.Value;
        _auditLogService = auditLogService;
        _logService = logService;
        _logger = logger;
    }

    public async Task<ConfiguracionEmisorDto> ObtenerConfiguracionEmisorAsync(CancellationToken cancellationToken = default)
    {
        var config = await _dbContext.IssuerConfigs
            .Where(c => c.EstaActivo && c.Ambiente == _facturacionSettings.Ambiente)
            .OrderByDescending(c => c.FechaCreacionUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (config == null)
        {
            return new ConfiguracionEmisorDto
            {
                Empresa = "WPC Bajío",
                Sucursal = "Matriz",
                Rfc = string.Empty,
                RazonSocial = "WPC BAJIO SA DE CV",
                RegimenFiscal = SatCatalogosConstantes.RegimenesFiscales.GeneralLeyPersonasMorales,
                CodigoPostalExpedicion = "37000",
                Serie = "A",
                FolioSiguiente = 1,
                Ambiente = _facturacionSettings.Ambiente,
                EstaListoParaEmitir = false,
                Notas = "Configuración fiscal inicial pendiente de CSD y datos definitivos",
                FacturacionHabilitada = _facturacionSettings.Habilitada
            };
        }

        return new ConfiguracionEmisorDto
        {
            Id = config.Id,
            Empresa = config.Empresa,
            Sucursal = config.Sucursal,
            Rfc = config.Rfc,
            RazonSocial = config.RazonSocial,
            RegimenFiscal = config.RegimenFiscal,
            CodigoPostalExpedicion = config.CodigoPostalExpedicion,
            Serie = config.Serie,
            FolioSiguiente = config.FolioSiguiente,
            FiscalapiPersonId = config.FiscalapiPersonId,
            Ambiente = config.Ambiente,
            CsdCertificadoNumero = config.CsdCertificadoNumero,
            EstaListoParaEmitir = config.EstaListoParaEmitir,
            Notas = config.Notas,
            FacturacionHabilitada = _facturacionSettings.Habilitada
        };
    }

    public async Task<ConfiguracionEmisorDto> GuardarConfiguracionEmisorAsync(
        ActualizarConfiguracionEmisorDto dto,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var config = await _dbContext.IssuerConfigs
            .Where(c => c.EstaActivo && c.Ambiente == _facturacionSettings.Ambiente)
            .OrderByDescending(c => c.FechaCreacionUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var rfcNormalizado = (dto.Rfc ?? "").Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(rfcNormalizado) && !EsRfcValido(rfcNormalizado))
        {
            throw new InvalidOperationException($"El RFC del emisor '{dto.Rfc}' no tiene un formato válido del SAT.");
        }

        if (config == null)
        {
            config = new FacturacionConfiguracion
            {
                Empresa = dto.Empresa,
                Sucursal = dto.Sucursal,
                Rfc = rfcNormalizado,
                RazonSocial = dto.RazonSocial.Trim().ToUpperInvariant(),
                RegimenFiscal = dto.RegimenFiscal.Trim(),
                CodigoPostalExpedicion = dto.CodigoPostalExpedicion.Trim(),
                Serie = dto.Serie.Trim().ToUpperInvariant(),
                FolioSiguiente = Math.Max(1, dto.FolioSiguiente),
                FiscalapiPersonId = dto.FiscalapiPersonId?.Trim(),
                Ambiente = _facturacionSettings.Ambiente,
                CsdCertificadoNumero = dto.CsdCertificadoNumero?.Trim(),
                EstaListoParaEmitir = dto.EstaListoParaEmitir,
                Notas = dto.Notas,
                FechaCreacionUtc = DateTime.UtcNow
            };
            _dbContext.IssuerConfigs.Add(config);
        }
        else
        {
            config.Empresa = dto.Empresa;
            config.Sucursal = dto.Sucursal;
            config.Rfc = rfcNormalizado;
            config.RazonSocial = dto.RazonSocial.Trim().ToUpperInvariant();
            config.RegimenFiscal = dto.RegimenFiscal.Trim();
            config.CodigoPostalExpedicion = dto.CodigoPostalExpedicion.Trim();
            config.Serie = dto.Serie.Trim().ToUpperInvariant();
            config.FolioSiguiente = Math.Max(1, dto.FolioSiguiente);
            config.FiscalapiPersonId = dto.FiscalapiPersonId?.Trim();
            config.CsdCertificadoNumero = dto.CsdCertificadoNumero?.Trim();
            config.EstaListoParaEmitir = dto.EstaListoParaEmitir;
            config.Notas = dto.Notas;
            config.FechaActualizacionUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            "FACTURA_CONFIGURACION_MODIFICADA",
            "ConfiguracionEmisor",
            config.Id,
            usuarioId,
            $"Configuración fiscal del emisor actualizada para ambiente {_facturacionSettings.Ambiente} (RFC: {config.Rfc}).",
            cancellationToken);

        return await ObtenerConfiguracionEmisorAsync(cancellationToken);
    }

    public async Task<VistaPreviaFacturaDto> GenerarVistaPreviaVentaAsync(Guid ventaId, CancellationToken cancellationToken = default)
    {
        var sale = await _dbContext.Sales
            .Include(s => s.Cliente)
            .Include(s => s.Partidas)
                .ThenInclude(p => p.Producto)
            .FirstOrDefaultAsync(s => s.Id == ventaId, cancellationToken);

        if (sale == null)
        {
            throw new KeyNotFoundException($"No se encontró la venta con identificador {ventaId}.");
        }

        var emisorConfig = await ObtenerConfiguracionEmisorAsync(cancellationToken);
        var vistaPrevia = new VistaPreviaFacturaDto
        {
            VentaId = sale.Id,
            IdVenta = sale.IdVenta,
            FolioVenta = sale.NumeroFolio,
            FechaVentaUtc = sale.FechaCreacionUtc,
            RfcEmisor = emisorConfig.Rfc,
            RazonSocialEmisor = emisorConfig.RazonSocial,
            RegimenFiscalEmisor = emisorConfig.RegimenFiscal,
            CodigoPostalExpedicion = emisorConfig.CodigoPostalExpedicion,
            Serie = emisorConfig.Serie,
            FolioPropuesto = emisorConfig.FolioSiguiente.ToString(),
            ClienteId = sale.ClienteId,
            Moneda = "MXN",
            TipoCambio = 1.0000m,
            MetodoPago = SatCatalogosConstantes.MetodosPago.PagoEnUnaSolaExhibicion
        };

        // Regla de negocio: Bloqueo explícito de ventas a crédito / apartado con saldo pendiente
        if (sale.SaldoPendiente > 0 || sale.TipoPago == SalePaymentTypes.AdvanceDeposit || sale.Estado == SaleStatuses.DepositPaid)
        {
            vistaPrevia.Bloqueos.Add($"La venta #{sale.IdVenta} cuenta con saldo pendiente (${sale.SaldoPendiente:N2} MXN) o es de tipo Apartado/Crédito. Debe liquidarse al 100% antes de emitir CFDI de ingreso PUE (los complementos de pago en parcialidades no están habilitados en esta fase).");
        }

        if (sale.Estado == SaleStatuses.Cancelled)
        {
            vistaPrevia.Bloqueos.Add($"La venta #{sale.IdVenta} está cancelada y no es elegible para facturación.");
        }

        // Verificar si ya existe factura activa para esta venta
        var facturaExistente = await _dbContext.Invoices
            .Where(f => f.VentaId == sale.Id && (f.Estado == FacturaEstados.Timbrada || f.Estado == FacturaEstados.EnProceso || f.Estado == FacturaEstados.CancelacionSolicitada))
            .FirstOrDefaultAsync(cancellationToken);

        if (facturaExistente != null)
        {
            if (facturaExistente.Estado == FacturaEstados.Timbrada)
            {
                vistaPrevia.Bloqueos.Add($"La venta #{sale.IdVenta} ya cuenta con una factura timbrada con UUID {facturaExistente.Uuid} (Folio: {facturaExistente.Serie}-{facturaExistente.Folio}).");
            }
            else if (facturaExistente.Estado == FacturaEstados.EnProceso)
            {
                vistaPrevia.Bloqueos.Add($"La venta #{sale.IdVenta} tiene actualmente un proceso de emisión en curso iniciado el {facturaExistente.FechaEmisionUtc:g} UTC.");
            }
            else if (facturaExistente.Estado == FacturaEstados.CancelacionSolicitada)
            {
                vistaPrevia.Bloqueos.Add($"La factura previa ({facturaExistente.Uuid}) de esta venta tiene una solicitud de cancelación en trámite ante el SAT. No se puede emitir otra hasta su resolución.");
            }
        }

        if (!_facturacionSettings.Habilitada)
        {
            vistaPrevia.Bloqueos.Add("El módulo de facturación se encuentra temporalmente deshabilitado en la configuración del sistema.");
        }

        if (string.IsNullOrWhiteSpace(emisorConfig.Rfc) || string.IsNullOrWhiteSpace(emisorConfig.CodigoPostalExpedicion))
        {
            vistaPrevia.Bloqueos.Add("La configuración fiscal del emisor está incompleta (falta RFC o Código Postal de expedición).");
        }

        // Datos del receptor (pre-cargar de Cliente si existe, o defaults de Público en General)
        if (sale.Cliente != null)
        {
            vistaPrevia.RfcReceptor = !string.IsNullOrWhiteSpace(sale.Cliente.Rfc) ? sale.Cliente.Rfc.Trim().ToUpperInvariant() : SatCatalogosConstantes.RfcGenericoNacional;
            vistaPrevia.RazonSocialReceptor = !string.IsNullOrWhiteSpace(sale.Cliente.RazonSocialFiscal)
                ? sale.Cliente.RazonSocialFiscal.Trim().ToUpperInvariant()
                : (!string.IsNullOrWhiteSpace(sale.Cliente.NombreEmpresa) ? sale.Cliente.NombreEmpresa.Trim().ToUpperInvariant() : sale.Cliente.NombreMostrar.Trim().ToUpperInvariant());
            vistaPrevia.CodigoPostalReceptor = !string.IsNullOrWhiteSpace(sale.Cliente.CodigoPostalFiscal) ? sale.Cliente.CodigoPostalFiscal.Trim() : (!string.IsNullOrWhiteSpace(sale.Cliente.CodigoPostal) ? sale.Cliente.CodigoPostal.Trim() : emisorConfig.CodigoPostalExpedicion);
            vistaPrevia.RegimenFiscalReceptor = !string.IsNullOrWhiteSpace(sale.Cliente.RegimenFiscal) ? sale.Cliente.RegimenFiscal.Trim() : (vistaPrevia.RfcReceptor == SatCatalogosConstantes.RfcGenericoNacional ? SatCatalogosConstantes.RegimenesFiscales.SinObligacionesFiscales : SatCatalogosConstantes.RegimenesFiscales.GeneralLeyPersonasMorales);
            vistaPrevia.UsoCfdi = !string.IsNullOrWhiteSpace(sale.Cliente.UsoCfdi) ? sale.Cliente.UsoCfdi.Trim() : (vistaPrevia.RfcReceptor == SatCatalogosConstantes.RfcGenericoNacional ? SatCatalogosConstantes.UsosCfdi.SinEfectosFiscales : SatCatalogosConstantes.UsosCfdi.GastosEnGeneral);
            vistaPrevia.EmailReceptor = !string.IsNullOrWhiteSpace(sale.Cliente.EmailFacturacion) ? sale.Cliente.EmailFacturacion.Trim() : sale.Cliente.Email.Trim();
        }
        else
        {
            vistaPrevia.RfcReceptor = SatCatalogosConstantes.RfcGenericoNacional;
            vistaPrevia.RazonSocialReceptor = "PUBLICO EN GENERAL";
            vistaPrevia.CodigoPostalReceptor = emisorConfig.CodigoPostalExpedicion;
            vistaPrevia.RegimenFiscalReceptor = SatCatalogosConstantes.RegimenesFiscales.SinObligacionesFiscales;
            vistaPrevia.UsoCfdi = SatCatalogosConstantes.UsosCfdi.SinEfectosFiscales;
            vistaPrevia.EmailReceptor = string.Empty;
        }

        // Determinar Forma de Pago real a partir de los montos cobrados en la venta
        vistaPrevia.FormaPago = DeterminarFormaPagoSat(sale);
        vistaPrevia.DescripcionFormaPago = ObtenerDescripcionFormaPagoSat(vistaPrevia.FormaPago);

        // Desglose de partidas conservando importes y reglas de IVA del PDV
        // Si la venta guardó MontoIva > 0, los precios ya estaban antes de IVA y se sumó el 16%.
        // Si la venta guardó MontoIva == 0, los precios de venta son totales con IVA incluido (mostrador).
        var preciosIncluyenIva = sale.MontoIva == 0m && sale.MontoTotal > 0m;
        decimal acumuladoSubtotal = 0m;
        decimal acumuladoIva = 0m;
        decimal acumuladoDescuento = 0m;

        var sumaDescuentosPartidas = sale.Partidas.Sum(p => p.MontoDescuento);
        var requiereProrrateoDescuento = sale.MontoDescuento > 0m && sumaDescuentosPartidas == 0m;
        var subtotalPartidas = sale.Partidas.Sum(p => p.PrecioTotal);
        decimal descuentoProrrateadoAcumulado = 0m;
        var partidasList = sale.Partidas.ToList();

        for (int i = 0; i < partidasList.Count; i++)
        {
            var p = partidasList[i];
            var claveProdServ = !string.IsNullOrWhiteSpace(p.Producto?.ClaveProdServSat) ? p.Producto.ClaveProdServSat : SatCatalogosConstantes.ClaveProdServLambrinDefault;
            var claveUnidad = !string.IsNullOrWhiteSpace(p.Producto?.ClaveUnidadSat) ? p.Producto.ClaveUnidadSat : MapearClaveUnidadSat(p.Producto?.UnidadMedida);
            var unidadMedida = !string.IsNullOrWhiteSpace(p.Producto?.UnidadMedida) ? p.Producto.UnidadMedida : "Pieza";
            var descripcion = !string.IsNullOrWhiteSpace(p.Producto?.Nombre) ? p.Producto.Nombre : "Material decorativo Lambrín";
            var sku = p.Producto?.Sku ?? string.Empty;

            decimal valorUnitario;
            decimal basePartida;
            decimal ivaPartida;
            decimal importePartida;
            decimal descuentoPartida = p.MontoDescuento;

            if (requiereProrrateoDescuento && subtotalPartidas > 0m)
            {
                if (i == partidasList.Count - 1)
                {
                    descuentoPartida = Math.Max(0m, sale.MontoDescuento - descuentoProrrateadoAcumulado);
                }
                else
                {
                    descuentoPartida = Math.Round(sale.MontoDescuento * (p.PrecioTotal / subtotalPartidas), 2);
                    descuentoProrrateadoAcumulado += descuentoPartida;
                }
            }

            if (preciosIncluyenIva)
            {
                // Desglosar IVA del total cobrado: Total = Base + 16% IVA
                // Base = Total / 1.16
                var totalRenglón = Math.Max(0m, p.PrecioTotal - descuentoPartida);
                var baseCalculada = Math.Round(totalRenglón / 1.16m, 2);
                ivaPartida = totalRenglón - baseCalculada;
                basePartida = baseCalculada;
                descuentoPartida = Math.Round(descuentoPartida / 1.16m, 2);
                importePartida = baseCalculada + descuentoPartida;
                valorUnitario = p.Cantidad > 0 ? Math.Round(importePartida / p.Cantidad, 4) : 0m;
            }
            else
            {
                importePartida = Math.Round(p.Cantidad * p.PrecioUnitario, 2);
                basePartida = Math.Max(0m, importePartida - descuentoPartida);
                ivaPartida = Math.Round(basePartida * 0.16m, 2);
                valorUnitario = p.PrecioUnitario;
            }

            acumuladoSubtotal += importePartida;
            acumuladoDescuento += descuentoPartida;
            acumuladoIva += ivaPartida;

            vistaPrevia.Conceptos.Add(new PartidaVistaPreviaDto
            {
                ProductoId = p.ProductoId,
                Sku = sku,
                Descripcion = descripcion,
                ClaveProdServSat = claveProdServ,
                ClaveUnidadSat = claveUnidad,
                UnidadMedida = unidadMedida,
                Cantidad = p.Cantidad,
                ValorUnitario = valorUnitario,
                Importe = importePartida,
                Descuento = descuentoPartida,
                ObjetoImpuestoSat = SatCatalogosConstantes.ObjetoImpuestoSiAplica,
                TasaIva = 0.1600m,
                BaseIva = basePartida,
                ImporteIva = ivaPartida
            });
        }

        vistaPrevia.Subtotal = acumuladoSubtotal;
        vistaPrevia.Descuento = acumuladoDescuento;
        vistaPrevia.ImpuestoTrasladadoIva = acumuladoIva;
        vistaPrevia.Total = Math.Round(acumuladoSubtotal - acumuladoDescuento + acumuladoIva, 2);

        // Conciliar diferencia de centavos si hubiese contra sale.MontoTotal
        var diff = sale.MontoTotal - vistaPrevia.Total;
        if (Math.Abs(diff) > 0m && Math.Abs(diff) <= 0.05m && vistaPrevia.Conceptos.Count > 0)
        {
            vistaPrevia.Total = sale.MontoTotal;
            vistaPrevia.ImpuestoTrasladadoIva += diff;
            vistaPrevia.Conceptos[0].ImporteIva += diff;
        }

        // Validaciones fiscales del receptor
        if (!EsRfcValido(vistaPrevia.RfcReceptor))
        {
            vistaPrevia.Advertencias.Add($"El RFC '{vistaPrevia.RfcReceptor}' no parece tener un formato válido según el SAT.");
        }

        if (vistaPrevia.RfcReceptor == SatCatalogosConstantes.RfcGenericoNacional && vistaPrevia.UsoCfdi != SatCatalogosConstantes.UsosCfdi.SinEfectosFiscales)
        {
            vistaPrevia.Advertencias.Add("Para Público en General (RFC XAXX010101000), el SAT requiere Uso CFDI 'S01' (Sin efectos fiscales).");
        }

        if (string.IsNullOrWhiteSpace(vistaPrevia.CodigoPostalReceptor) || vistaPrevia.CodigoPostalReceptor.Length != 5)
        {
            vistaPrevia.Advertencias.Add("El código postal fiscal del receptor debe ser de 5 dígitos numéricos.");
        }

        vistaPrevia.EsElegibleParaFacturar = vistaPrevia.Bloqueos.Count == 0;
        return vistaPrevia;
    }

    public async Task<ResultadoEmisionFacturaDto> EmitirFacturaVentaAsync(
        Guid ventaId,
        SolicitarEmisionFacturaDto dto,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        // 1. Obtener vista previa y verificar elegibilidad
        var vistaPrevia = await GenerarVistaPreviaVentaAsync(ventaId, cancellationToken);
        if (!vistaPrevia.EsElegibleParaFacturar)
        {
            return new ResultadoEmisionFacturaDto
            {
                Exitoso = false,
                Mensaje = string.Join(" ", vistaPrevia.Bloqueos)
            };
        }

        var emisorConfig = await _dbContext.IssuerConfigs
            .Where(c => c.EstaActivo && c.Ambiente == _facturacionSettings.Ambiente)
            .OrderByDescending(c => c.FechaCreacionUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (emisorConfig == null || string.IsNullOrWhiteSpace(emisorConfig.Rfc))
        {
            return new ResultadoEmisionFacturaDto
            {
                Exitoso = false,
                Mensaje = "No hay una configuración fiscal activa para el emisor en el ambiente actual."
            };
        }

        var validUsuarioId = ObtenerUsuarioIdValido(usuarioId);

        // 2. Normalizar y validar datos del receptor ingresados o confirmados
        var rfcReceptor = (dto.RfcReceptor ?? vistaPrevia.RfcReceptor).Trim().ToUpperInvariant();
        var razonSocialReceptor = (dto.RazonSocialReceptor ?? vistaPrevia.RazonSocialReceptor).Trim().ToUpperInvariant();
        var cpReceptor = (dto.CodigoPostalReceptor ?? vistaPrevia.CodigoPostalReceptor).Trim();
        var regimenReceptor = (dto.RegimenFiscalReceptor ?? vistaPrevia.RegimenFiscalReceptor).Trim();
        var usoCfdi = (dto.UsoCfdi ?? vistaPrevia.UsoCfdi).Trim().ToUpperInvariant();
        var emailReceptor = (dto.EmailReceptor ?? vistaPrevia.EmailReceptor).Trim();
        var formaPago = !string.IsNullOrWhiteSpace(dto.FormaPago) ? dto.FormaPago.Trim() : vistaPrevia.FormaPago;
        var metodoPago = !string.IsNullOrWhiteSpace(dto.MetodoPago) ? dto.MetodoPago.Trim() : vistaPrevia.MetodoPago;

        if (string.IsNullOrWhiteSpace(rfcReceptor) || !EsRfcValido(rfcReceptor))
        {
            return new ResultadoEmisionFacturaDto { Exitoso = false, Mensaje = $"El RFC '{rfcReceptor}' no tiene un formato válido ante el SAT." };
        }
        if (string.IsNullOrWhiteSpace(razonSocialReceptor))
        {
            return new ResultadoEmisionFacturaDto { Exitoso = false, Mensaje = "El nombre o razón social fiscal del receptor es requerido." };
        }
        if (string.IsNullOrWhiteSpace(cpReceptor) || cpReceptor.Length != 5)
        {
            return new ResultadoEmisionFacturaDto { Exitoso = false, Mensaje = "El código postal fiscal del receptor debe contener exactamente 5 dígitos." };
        }
        if (string.IsNullOrWhiteSpace(regimenReceptor))
        {
            return new ResultadoEmisionFacturaDto { Exitoso = false, Mensaje = "El régimen fiscal del receptor es requerido." };
        }
        if (string.IsNullOrWhiteSpace(usoCfdi))
        {
            return new ResultadoEmisionFacturaDto { Exitoso = false, Mensaje = "El uso del CFDI es requerido." };
        }

        // Reglas SAT v4.0 para Público en General
        if (rfcReceptor == SatCatalogosConstantes.RfcGenericoNacional)
        {
            razonSocialReceptor = "PUBLICO EN GENERAL";
            regimenReceptor = SatCatalogosConstantes.RegimenesFiscales.SinObligacionesFiscales;
            usoCfdi = SatCatalogosConstantes.UsosCfdi.SinEfectosFiscales;
        }

        // 3. Reserva atómica local antes del llamado HTTP a FiscalAPI
        var idempotencyKey = !string.IsNullOrWhiteSpace(dto.IdempotencyKey)
            ? dto.IdempotencyKey.Trim()
            : $"INV-{vistaPrevia.IdVenta}-{Guid.NewGuid():N}";

        // Verificar si existe intento previo para esta venta (excluyendo únicamente canceladas)
        var factura = await _dbContext.Invoices
            .Include(f => f.Partidas)
            .Include(f => f.HistorialEstados)
            .FirstOrDefaultAsync(f => f.VentaId == ventaId && f.Estado != FacturaEstados.Cancelada, cancellationToken);

        if (factura != null && factura.Estado == FacturaEstados.Timbrada)
        {
            return new ResultadoEmisionFacturaDto
            {
                Exitoso = true,
                FacturaId = factura.Id,
                Uuid = factura.Uuid,
                Serie = factura.Serie,
                Folio = factura.Folio,
                Estado = factura.Estado,
                Mensaje = "La venta ya se encontraba timbrada previamente."
            };
        }

        if (factura != null && factura.Estado == FacturaEstados.EnProceso)
        {
            var minutosEnProceso = (DateTime.UtcNow - factura.FechaEmisionUtc).TotalMinutes;
            if (minutosEnProceso < 3)
            {
                return new ResultadoEmisionFacturaDto
                {
                    Exitoso = false,
                    Mensaje = "Esta venta ya tiene un proceso de timbrado en curso. Espere un momento a que concluya o consulte su estado."
                };
            }
        }

        var folioNumero = emisorConfig.FolioSiguiente.ToString();
        var correlationId = $"CORR-{vistaPrevia.IdVenta}-{Guid.NewGuid():N}";

        if (factura == null)
        {
            factura = new FacturacionFactura
            {
                VentaId = ventaId,
                IdVenta = vistaPrevia.IdVenta,
                FolioVenta = vistaPrevia.FolioVenta,
                Serie = emisorConfig.Serie,
                Folio = folioNumero,
                Ambiente = _facturacionSettings.Ambiente,
                Estado = FacturaEstados.EnProceso,
                RfcEmisor = emisorConfig.Rfc,
                RazonSocialEmisor = emisorConfig.RazonSocial,
                RegimenFiscalEmisor = emisorConfig.RegimenFiscal,
                CodigoPostalExpedicion = emisorConfig.CodigoPostalExpedicion,
                RfcReceptor = rfcReceptor,
                RazonSocialReceptor = razonSocialReceptor,
                CodigoPostalReceptor = cpReceptor,
                RegimenFiscalReceptor = regimenReceptor,
                UsoCfdi = usoCfdi,
                EmailReceptor = emailReceptor,
                Moneda = vistaPrevia.Moneda,
                TipoCambio = vistaPrevia.TipoCambio,
                FormaPago = formaPago,
                MetodoPago = metodoPago,
                TipoComprobante = "I",
                Exportacion = "01",
                Subtotal = vistaPrevia.Subtotal,
                Descuento = vistaPrevia.Descuento,
                Total = vistaPrevia.Total,
                TotalImpuestosTrasladados = vistaPrevia.ImpuestoTrasladadoIva,
                TotalImpuestosRetenidos = 0m,
                IdempotencyKey = idempotencyKey,
                UsuarioId = validUsuarioId,
                FechaEmisionUtc = DateTime.UtcNow
            };

            var numeroPartida = 1;
            foreach (var c in vistaPrevia.Conceptos)
            {
                factura.Detalles.Add(new FacturacionFacturaDetalle
                {
                    NumeroPartida = numeroPartida++,
                    ProductoId = c.ProductoId,
                    ClaveProdServ = c.ClaveProdServSat,
                    ClaveUnidad = c.ClaveUnidadSat,
                    Unidad = c.UnidadMedida,
                    Descripcion = c.Descripcion,
                    Sku = c.Sku,
                    Cantidad = c.Cantidad,
                    ValorUnitario = c.ValorUnitario,
                    Importe = c.Importe,
                    Descuento = c.Descuento,
                    ObjetoImp = c.ObjetoImpuestoSat,
                    TasaOcuotaIva = c.TasaIva,
                    BaseIva = c.BaseIva,
                    ImporteIva = c.ImporteIva
                });
            }

            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = FacturaEstados.Borrador,
                EstadoNuevo = FacturaEstados.EnProceso,
                Detalle = $"Inicio de solicitud de timbrado para Venta #{vistaPrevia.IdVenta} con llave de idempotencia {idempotencyKey}.",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });

            _dbContext.Invoices.Add(factura);
        }
        else
        {
            factura.Serie = emisorConfig.Serie;
            factura.Folio = folioNumero;
            factura.Estado = FacturaEstados.EnProceso;
            factura.RfcReceptor = rfcReceptor;
            factura.RazonSocialReceptor = razonSocialReceptor;
            factura.CodigoPostalReceptor = cpReceptor;
            factura.RegimenFiscalReceptor = regimenReceptor;
            factura.UsoCfdi = usoCfdi;
            factura.EmailReceptor = emailReceptor;
            factura.FormaPago = formaPago;
            factura.MetodoPago = metodoPago;
            factura.IdempotencyKey = idempotencyKey;
            factura.FechaEmisionUtc = DateTime.UtcNow;
            factura.MensajeRespuesta = null;
            factura.DetallesRespuesta = null;

            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = factura.Estado,
                EstadoNuevo = FacturaEstados.EnProceso,
                Detalle = $"Reintento de timbrado con nueva llave de idempotencia {idempotencyKey}.",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });
        }

        // 1. Guardar el intento de negocio y la llave de idempotencia antes de emitir
        var intento = new FacturacionIntento
        {
            FacturaId = factura.Id,
            VentaId = ventaId,
            CorrelationId = correlationId,
            Ambiente = _facturacionSettings.Ambiente,
            NumeroRevision = 1,
            NumeroEnvio = 1,
            IdempotencyKey = idempotencyKey,
            Estado = "Iniciado",
            FechaInicioUtc = DateTime.UtcNow,
            UsuarioId = validUsuarioId
        };
        _dbContext.FacturacionIntentos.Add(intento);

        // GUARDAR RESERVA LOCAL ANTES DEL LLAMADO HTTP
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 4. Construir modelo de factura oficial del SDK FiscalAPI
        var invoiceModel = new Invoice
        {
            VersionCode = "4.0",
            Series = factura.Serie,
            Number = factura.Folio,
            Date = DateTime.Now,
            PaymentFormCode = factura.FormaPago,
            CurrencyCode = factura.Moneda,
            TypeCode = "I",
            ExpeditionZipCode = factura.CodigoPostalExpedicion,
            ExportCode = "01",
            PaymentMethodCode = factura.MetodoPago,
            Subtotal = factura.Subtotal,
            Discount = factura.Descuento,
            Total = factura.Total,
            Issuer = new InvoiceIssuer
            {
                Id = emisorConfig.FiscalapiPersonId,
                Tin = factura.RfcEmisor,
                LegalName = factura.RazonSocialEmisor,
                TaxRegimeCode = factura.RegimenFiscalEmisor
            },
            Recipient = new InvoiceRecipient
            {
                Tin = factura.RfcReceptor,
                LegalName = factura.RazonSocialReceptor,
                ZipCode = factura.CodigoPostalReceptor,
                TaxRegimeCode = factura.RegimenFiscalReceptor,
                CfdiUseCode = factura.UsoCfdi,
                Email = factura.EmailReceptor
            },
            Items = factura.Detalles.Select(p => new InvoiceItem
            {
                ItemCode = p.ClaveProdServ,
                Quantity = p.Cantidad,
                UnitOfMeasurementCode = p.ClaveUnidad,
                UnitOfMeasurement = p.Unidad,
                Description = p.Descripcion,
                UnitPrice = p.ValorUnitario,
                Discount = p.Descuento,
                TaxObjectCode = p.ObjetoImp,
                ItemSku = p.Sku,
                ItemTaxes = new List<InvoiceItemTax>
                {
                    new()
                    {
                        TaxCode = "002", // IVA
                        TaxTypeCode = "Tasa",
                        TaxRate = p.TasaOcuotaIva,
                        TaxFlagCode = "T"
                    }
                }
            }).ToList()
        };

        var requestJson = System.Text.Json.JsonSerializer.Serialize(invoiceModel);
        intento.RequestSanitizadoJson = FacturacionSanitizer.SanitizeJson(requestJson);

        // 2. Crea el registro FacturacionLog como Iniciada
        var logId = await _logService.RegistrarLlamadaIniciadaAsync(
            correlationId,
            operacion: "Emitir",
            metodoHttp: "POST",
            ruta: "/v1/invoices",
            requestJson: requestJson,
            idempotencyKey: idempotencyKey,
            facturaId: factura.Id,
            intentoId: intento.Id,
            ventaId: ventaId,
            usuarioId: validUsuarioId,
            origen: "Usuario",
            ambiente: _facturacionSettings.Ambiente,
            numeroReintento: 1,
            cancellationToken: cancellationToken);

        // 3. Enviar a FiscalAPI (sin mantener transacción SQL abierta)
        ApiResponse<Invoice>? apiResponse = null;
        try
        {
            apiResponse = await _fiscalClient.CreateInvoiceAsync(invoiceModel, idempotencyKey, cancellationToken);
        }
        catch (Exception ex) when (ex is TaskCanceledException or TimeoutException or HttpRequestException)
        {
            _logger.LogError(ex, "Timeout o error de red comunicando con FiscalAPI para factura Venta #{IdVenta}", vistaPrevia.IdVenta);

            factura.Estado = FacturaEstados.ResultadoPorConfirmar;
            factura.MensajeRespuesta = "Tiempo de espera agotado al comunicar con FiscalAPI. El resultado se encuentra por confirmar ante el SAT.";
            factura.DetallesRespuesta = ex.Message;
            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = FacturaEstados.EnProceso,
                EstadoNuevo = FacturaEstados.ResultadoPorConfirmar,
                Detalle = $"Excepción de red o timeout: {ex.Message}. Requiere conciliación de estado.",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });

            intento.Estado = "ResultadoIncierto";
            intento.Resultado = "ResultadoIncierto";
            intento.FechaFinUtc = DateTime.UtcNow;
            intento.Mensaje = factura.MensajeRespuesta;
            intento.DetalleError = ex.Message;

            await _logService.RegistrarLlamadaFinalizadaAsync(
                logId,
                httpStatusCode: null,
                providerHttpStatusCode: null,
                succeeded: null,
                resultado: "ResultadoIncierto",
                responseJson: null,
                mensaje: "Tiempo de espera agotado al comunicar con FiscalAPI. El resultado se encuentra por confirmar ante el SAT.",
                detalleError: ex.Message,
                traceIdentifierProveedor: null,
                identificadorExterno: null,
                uuidFiscal: null,
                exception: ex,
                cancellationToken: cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RegistrarAuditoriaAsync(
                "FACTURA_RESULTADO_POR_CONFIRMAR",
                "Factura",
                factura.Id,
                usuarioId,
                $"Timeout al comunicar con FiscalAPI para venta #{vistaPrevia.IdVenta}. Estado fijado en ResultadoPorConfirmar (CorrelationId: {correlationId}).",
                cancellationToken);

            return new ResultadoEmisionFacturaDto
            {
                Exitoso = false,
                FacturaId = factura.Id,
                Estado = factura.Estado,
                Mensaje = "Tiempo de espera agotado al conectar con el proveedor fiscal. Se guardó el registro como 'Resultado por Confirmar' para verificar si el SAT timbró el comprobante sin duplicarlo.",
                RequiereConfirmacionManual = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al emitir factura para venta #{IdVenta}", vistaPrevia.IdVenta);
            factura.Estado = FacturaEstados.Rechazada;
            factura.MensajeRespuesta = "Error inesperado durante la emisión: " + ex.Message;
            factura.DetallesRespuesta = ex.ToString();
            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = FacturaEstados.EnProceso,
                EstadoNuevo = FacturaEstados.Rechazada,
                Detalle = $"Error no controlado: {ex.Message}",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });

            intento.Estado = "Fallido";
            intento.Resultado = "FalloTecnico";
            intento.FechaFinUtc = DateTime.UtcNow;
            intento.Mensaje = factura.MensajeRespuesta;
            intento.DetalleError = ex.ToString();

            await _logService.RegistrarLlamadaFinalizadaAsync(
                logId,
                httpStatusCode: null,
                providerHttpStatusCode: null,
                succeeded: false,
                resultado: "FalloTecnico",
                responseJson: null,
                mensaje: "Error inesperado durante la emisión: " + ex.Message,
                detalleError: ex.ToString(),
                traceIdentifierProveedor: null,
                identificadorExterno: null,
                uuidFiscal: null,
                exception: ex,
                cancellationToken: cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ResultadoEmisionFacturaDto
            {
                Exitoso = false,
                FacturaId = factura.Id,
                Estado = factura.Estado,
                Mensaje = "Ocurrió un error al procesar la factura: " + ex.Message
            };
        }

        // 6. Procesar respuesta de negocio de FiscalAPI
        factura.CodigoEstadoHttp = apiResponse.HttpStatusCode;
        factura.TraceIdentifier = apiResponse.TraceIdentifier;
        factura.MensajeRespuesta = apiResponse.Message;
        factura.DetallesRespuesta = apiResponse.Details;

        if (apiResponse.Succeeded && apiResponse.Data != null)
        {
            var invoiceData = apiResponse.Data;
            factura.FiscalapiInvoiceId = invoiceData.Id;
            factura.Uuid = invoiceData.Uuid;

            if (string.IsNullOrWhiteSpace(factura.Uuid) && invoiceData.Responses != null && invoiceData.Responses.Count > 0)
            {
                factura.Uuid = invoiceData.Responses[0].InvoiceUuid;
            }

            factura.Estado = FacturaEstados.Timbrada;
            factura.FechaTimbradoUtc = DateTime.UtcNow;

            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = FacturaEstados.EnProceso,
                EstadoNuevo = FacturaEstados.Timbrada,
                Detalle = $"Factura timbrada exitosamente con UUID {factura.Uuid}. ID externo FiscalAPI: {factura.FiscalapiInvoiceId}",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });

            intento.Estado = "Completado";
            intento.Resultado = "Exitoso";
            intento.HttpStatusCode = apiResponse.HttpStatusCode;
            intento.FechaFinUtc = DateTime.UtcNow;
            intento.Mensaje = "Factura timbrada con éxito.";

            // 4. Actualizar registro FacturacionLog con el resultado exitoso
            await _logService.RegistrarLlamadaFinalizadaAsync(
                logId,
                httpStatusCode: apiResponse.HttpStatusCode,
                providerHttpStatusCode: apiResponse.HttpStatusCode,
                succeeded: true,
                resultado: "Exitoso",
                responseJson: System.Text.Json.JsonSerializer.Serialize(apiResponse),
                mensaje: apiResponse.Message,
                detalleError: apiResponse.Details,
                traceIdentifierProveedor: apiResponse.TraceIdentifier,
                identificadorExterno: factura.FiscalapiInvoiceId,
                uuidFiscal: factura.Uuid,
                exception: null,
                cancellationToken: cancellationToken);

            // Relación FacturacionFacturaVenta
            _dbContext.FacturacionFacturaVentas.Add(new FacturacionFacturaVenta
            {
                FacturaId = factura.Id,
                VentaId = ventaId,
                FolioVenta = vistaPrevia.FolioVenta,
                IdVenta = vistaPrevia.IdVenta,
                MontoAsociado = factura.Total,
                FechaAsociacionUtc = DateTime.UtcNow
            });

            // Impuestos desglosados en FacturacionFacturaImpuesto
            foreach (var part in factura.Detalles)
            {
                _dbContext.FacturacionFacturaImpuestos.Add(new FacturacionFacturaImpuesto
                {
                    FacturaDetalleId = part.Id,
                    Tipo = "Traslado",
                    Impuesto = "002",
                    TipoFactor = "Tasa",
                    Base = part.BaseIva,
                    TasaOCuota = part.TasaOcuotaIva,
                    Importe = part.ImporteIva
                });
            }

            // Incrementar folio consecutivo en la configuración del emisor
            emisorConfig.FolioSiguiente++;

            // Actualizar datos fiscales del cliente si se solicitó
            if (dto.GuardarDatosEnCliente && vistaPrevia.ClienteId.HasValue)
            {
                var cliente = await _dbContext.Customers.FindAsync(new object[] { vistaPrevia.ClienteId.Value }, cancellationToken);
                if (cliente != null)
                {
                    cliente.Rfc = rfcReceptor;
                    cliente.RazonSocialFiscal = razonSocialReceptor;
                    cliente.CodigoPostalFiscal = cpReceptor;
                    cliente.RegimenFiscal = regimenReceptor;
                    cliente.UsoCfdi = usoCfdi;
                    if (!string.IsNullOrWhiteSpace(emailReceptor))
                    {
                        cliente.EmailFacturacion = emailReceptor;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 7. Descargar y almacenar documentos XML y PDF en FacturacionDocumento
            try
            {
                await DescargarYGuardarDocumentosAsync(factura.Id, factura.FiscalapiInvoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron precargar los archivos XML/PDF inmediatamente tras el timbrado para factura {FacturaId}", factura.Id);
            }

            await RegistrarAuditoriaAsync(
                "FACTURA_EMITIDA",
                "Factura",
                factura.Id,
                usuarioId,
                $"Factura timbrada exitosamente para Venta #{vistaPrevia.IdVenta} con UUID {factura.Uuid} (Total: ${factura.Total:N2} MXN, CorrelationId: {correlationId}).",
                cancellationToken);

            return new ResultadoEmisionFacturaDto
            {
                Exitoso = true,
                FacturaId = factura.Id,
                Uuid = factura.Uuid,
                Serie = factura.Serie,
                Folio = factura.Folio,
                Estado = factura.Estado,
                Mensaje = $"Factura timbrada con éxito. UUID: {factura.Uuid}",
                TraceIdentifier = apiResponse.TraceIdentifier
            };
        }
        else
        {
            // Rechazo de negocio por FiscalAPI / SAT
            factura.Estado = FacturaEstados.Rechazada;
            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = FacturaEstados.EnProceso,
                EstadoNuevo = FacturaEstados.Rechazada,
                Detalle = $"Rechazo de FiscalAPI/SAT: {apiResponse.Message}. Detalles: {apiResponse.Details}",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });

            intento.Estado = "Rechazado";
            intento.Resultado = "Rechazado";
            intento.HttpStatusCode = apiResponse.HttpStatusCode;
            intento.FechaFinUtc = DateTime.UtcNow;
            intento.Mensaje = apiResponse.Message;
            intento.DetalleError = apiResponse.Details;

            await _logService.RegistrarLlamadaFinalizadaAsync(
                logId,
                httpStatusCode: apiResponse.HttpStatusCode,
                providerHttpStatusCode: apiResponse.HttpStatusCode,
                succeeded: false,
                resultado: "Rechazado",
                responseJson: System.Text.Json.JsonSerializer.Serialize(apiResponse),
                mensaje: apiResponse.Message,
                detalleError: apiResponse.Details,
                traceIdentifierProveedor: apiResponse.TraceIdentifier,
                identificadorExterno: null,
                uuidFiscal: null,
                exception: null,
                cancellationToken: cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RegistrarAuditoriaAsync(
                "FACTURA_RECHAZADA",
                "Factura",
                factura.Id,
                usuarioId,
                $"Rechazo al timbrar factura para venta #{vistaPrevia.IdVenta}: {apiResponse.Message} (CorrelationId: {correlationId}).",
                cancellationToken);

            return new ResultadoEmisionFacturaDto
            {
                Exitoso = false,
                FacturaId = factura.Id,
                Estado = factura.Estado,
                Mensaje = !string.IsNullOrWhiteSpace(apiResponse.Message) ? apiResponse.Message : "La emisión fue rechazada por el proveedor fiscal o el SAT.",
                DetalleTecnico = apiResponse.Details,
                TraceIdentifier = apiResponse.TraceIdentifier
            };
        }
    }

    public async Task<PagedResult<FacturaDto>> ObtenerFacturasAsync(FacturaFiltrosDto filtros, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Invoices
            .Include(f => f.Partidas)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtros.Ambiente))
        {
            query = query.Where(f => f.Ambiente == filtros.Ambiente);
        }
        else
        {
            query = query.Where(f => f.Ambiente == _facturacionSettings.Ambiente);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Estado))
        {
            query = query.Where(f => f.Estado == filtros.Estado);
        }
        else
        {
            // En "Facturas Emitidas", por defecto solo mostrar facturas reales timbradas / emitidas (y canceladas en su caso),
            // excluyendo intentos técnicos rechazados o borradores.
            query = query.Where(f => f.Estado == FacturaEstados.Timbrada ||
                                     f.Estado == FacturaEstados.Cancelada ||
                                     f.Estado == FacturaEstados.CancelacionSolicitada ||
                                     f.Estado == FacturaEstados.ResultadoPorConfirmar);
        }

        if (!string.IsNullOrWhiteSpace(filtros.RfcReceptor))
        {
            var rfc = filtros.RfcReceptor.Trim().ToUpperInvariant();
            query = query.Where(f => f.RfcReceptor.Contains(rfc));
        }

        if (filtros.IdVenta.HasValue)
        {
            query = query.Where(f => f.IdVenta == filtros.IdVenta.Value);
        }

        if (filtros.FechaInicioUtc.HasValue)
        {
            query = query.Where(f => f.FechaEmisionUtc >= filtros.FechaInicioUtc.Value);
        }

        if (filtros.FechaFinUtc.HasValue)
        {
            query = query.Where(f => f.FechaEmisionUtc <= filtros.FechaFinUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
        {
            var search = filtros.Busqueda.Trim().ToLowerInvariant();
            query = query.Where(f =>
                f.FolioVenta.ToLower().Contains(search) ||
                (f.Uuid != null && f.Uuid.ToLower().Contains(search)) ||
                f.Folio.ToLower().Contains(search) ||
                f.RfcReceptor.ToLower().Contains(search) ||
                f.RazonSocialReceptor.ToLower().Contains(search));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(f => f.FechaEmisionUtc)
            .Skip((filtros.PageNumber - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .Select(f => new FacturaDto
            {
                Id = f.Id,
                VentaId = f.VentaId,
                IdVenta = f.IdVenta,
                FolioVenta = f.FolioVenta,
                Serie = f.Serie,
                Folio = f.Folio,
                Ambiente = f.Ambiente,
                Estado = f.Estado,
                EstadoSat = f.EstadoSat,
                EstadoCancelacionSat = f.EstadoCancelacionSat,
                FiscalapiInvoiceId = f.FiscalapiInvoiceId,
                Uuid = f.Uuid,
                FechaEmisionUtc = f.FechaEmisionUtc,
                FechaTimbradoUtc = f.FechaTimbradoUtc,
                RfcEmisor = f.RfcEmisor,
                RazonSocialEmisor = f.RazonSocialEmisor,
                RegimenFiscalEmisor = f.RegimenFiscalEmisor,
                CodigoPostalExpedicion = f.CodigoPostalExpedicion,
                RfcReceptor = f.RfcReceptor,
                RazonSocialReceptor = f.RazonSocialReceptor,
                UsoCfdi = f.UsoCfdi,
                RegimenFiscalReceptor = f.RegimenFiscalReceptor,
                CodigoPostalReceptor = f.CodigoPostalReceptor,
                EmailReceptor = f.EmailReceptor,
                Moneda = f.Moneda,
                TipoCambio = f.TipoCambio,
                FormaPago = f.FormaPago,
                MetodoPago = f.MetodoPago,
                TipoComprobante = f.TipoComprobante,
                Exportacion = f.Exportacion,
                Subtotal = f.Subtotal,
                Descuento = f.Descuento,
                Total = f.Total,
                TotalImpuestosTrasladados = f.TotalImpuestosTrasladados,
                TotalImpuestosRetenidos = f.TotalImpuestosRetenidos,
                IdempotencyKey = f.IdempotencyKey,
                TraceIdentifier = f.TraceIdentifier,
                MensajeRespuesta = f.MensajeRespuesta,
                DetallesRespuesta = f.DetallesRespuesta,
                CodigoEstadoHttp = f.CodigoEstadoHttp,
                TieneXml = !string.IsNullOrEmpty(f.XmlContenido),
                TienePdf = !string.IsNullOrEmpty(f.PdfRuta),
                TieneAcuseCancelacion = !string.IsNullOrEmpty(f.AcuseCancelacionXml),
                MotivoCancelacion = f.MotivoCancelacion,
                UuidSustitucion = f.UuidSustitucion,
                FechaSolicitudCancelacionUtc = f.FechaSolicitudCancelacionUtc,
                FechaCancelacionUtc = f.FechaCancelacionUtc
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FacturaDto>(items, totalItems, filtros.PageNumber, filtros.PageSize);
    }

    public async Task<FacturaDto?> ObtenerFacturaPorIdAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        var f = await _dbContext.Invoices
            .Include(x => x.Partidas)
            .Include(x => x.HistorialEstados)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == facturaId, cancellationToken);

        if (f == null) return null;

        return new FacturaDto
        {
            Id = f.Id,
            VentaId = f.VentaId,
            IdVenta = f.IdVenta,
            FolioVenta = f.FolioVenta,
            Serie = f.Serie,
            Folio = f.Folio,
            Ambiente = f.Ambiente,
            Estado = f.Estado,
            EstadoSat = f.EstadoSat,
            EstadoCancelacionSat = f.EstadoCancelacionSat,
            FiscalapiInvoiceId = f.FiscalapiInvoiceId,
            Uuid = f.Uuid,
            FechaEmisionUtc = f.FechaEmisionUtc,
            FechaTimbradoUtc = f.FechaTimbradoUtc,
            RfcEmisor = f.RfcEmisor,
            RazonSocialEmisor = f.RazonSocialEmisor,
            RegimenFiscalEmisor = f.RegimenFiscalEmisor,
            CodigoPostalExpedicion = f.CodigoPostalExpedicion,
            RfcReceptor = f.RfcReceptor,
            RazonSocialReceptor = f.RazonSocialReceptor,
            UsoCfdi = f.UsoCfdi,
            RegimenFiscalReceptor = f.RegimenFiscalReceptor,
            CodigoPostalReceptor = f.CodigoPostalReceptor,
            EmailReceptor = f.EmailReceptor,
            Moneda = f.Moneda,
            TipoCambio = f.TipoCambio,
            FormaPago = f.FormaPago,
            MetodoPago = f.MetodoPago,
            TipoComprobante = f.TipoComprobante,
            Exportacion = f.Exportacion,
            Subtotal = f.Subtotal,
            Descuento = f.Descuento,
            Total = f.Total,
            TotalImpuestosTrasladados = f.TotalImpuestosTrasladados,
            TotalImpuestosRetenidos = f.TotalImpuestosRetenidos,
            IdempotencyKey = f.IdempotencyKey,
            TraceIdentifier = f.TraceIdentifier,
            MensajeRespuesta = f.MensajeRespuesta,
            DetallesRespuesta = f.DetallesRespuesta,
            CodigoEstadoHttp = f.CodigoEstadoHttp,
            TieneXml = !string.IsNullOrEmpty(f.XmlContenido),
            TienePdf = !string.IsNullOrEmpty(f.PdfRuta),
            TieneAcuseCancelacion = !string.IsNullOrEmpty(f.AcuseCancelacionXml),
            MotivoCancelacion = f.MotivoCancelacion,
            UuidSustitucion = f.UuidSustitucion,
            FechaSolicitudCancelacionUtc = f.FechaSolicitudCancelacionUtc,
            FechaCancelacionUtc = f.FechaCancelacionUtc,
            Partidas = f.Partidas.Select(p => new FacturaPartidaDto
            {
                Id = p.Id,
                ProductoId = p.ProductoId,
                ClaveProdServ = p.ClaveProdServ,
                ClaveUnidad = p.ClaveUnidad,
                Unidad = p.Unidad,
                Descripcion = p.Descripcion,
                Sku = p.Sku,
                Cantidad = p.Cantidad,
                ValorUnitario = p.ValorUnitario,
                Importe = p.Importe,
                Descuento = p.Descuento,
                ObjetoImp = p.ObjetoImp,
                TasaOcuotaIva = p.TasaOcuotaIva,
                BaseIva = p.BaseIva,
                ImporteIva = p.ImporteIva
            }).ToList(),
            HistorialEstados = f.HistorialEstados.OrderBy(h => h.FechaCreacionUtc).Select(h => new FacturaHistorialEstadoDto
            {
                Id = h.Id,
                EstadoAnterior = h.EstadoAnterior,
                EstadoNuevo = h.EstadoNuevo,
                Detalle = h.Detalle,
                FechaUtc = h.FechaCreacionUtc
            }).ToList()
        };
    }

    public async Task<(byte[] Bytes, string ContentType, string NombreArchivo)> DescargarXmlAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        var factura = await _dbContext.Invoices.FindAsync(new object[] { facturaId }, cancellationToken);
        if (factura == null)
        {
            throw new KeyNotFoundException($"No se encontró la factura con identificador {facturaId}.");
        }

        // 1. Revisar si ya existe en FacturacionDocumentos
        var docXml = await _dbContext.FacturacionDocumentos
            .FirstOrDefaultAsync(d => d.FacturaId == facturaId && d.TipoDocumento == "XML", cancellationToken);

        if (docXml != null && !string.IsNullOrWhiteSpace(docXml.ContenidoBase64))
        {
            return (Convert.FromBase64String(docXml.ContenidoBase64), docXml.MimeType, docXml.NombreArchivo);
        }

        string xmlString;
        if (!string.IsNullOrWhiteSpace(factura.XmlContenido))
        {
            xmlString = factura.XmlContenido;
            var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlString);
            _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
            {
                FacturaId = factura.Id,
                TipoDocumento = "XML",
                NombreArchivo = $"CFDI_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.xml",
                MimeType = "application/xml",
                TamanoBytes = xmlBytes.Length,
                Sha256Hash = ComputeSha256(xmlBytes),
                ContenidoBase64 = Convert.ToBase64String(xmlBytes),
                FechaCreacionUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(factura.FiscalapiInvoiceId))
        {
            var corrId = $"DOC-XML-{factura.Id}-{Guid.NewGuid():N}";
            var logId = await _logService.RegistrarLlamadaIniciadaAsync(
                corrId,
                "ObtenerXml",
                "GET",
                $"/v1/invoices/{factura.FiscalapiInvoiceId}/xml",
                requestJson: null,
                facturaId: factura.Id,
                ambiente: _facturacionSettings.Ambiente,
                cancellationToken: cancellationToken);

            var res = await _fiscalClient.GetXmlAsync(factura.FiscalapiInvoiceId, cancellationToken);

            await _logService.RegistrarLlamadaFinalizadaAsync(
                logId,
                res.HttpStatusCode,
                res.HttpStatusCode,
                res.Succeeded,
                res.Succeeded ? "Exitoso" : "Rechazado",
                responseJson: res.Data != null ? FacturacionSanitizer.SanitizeObject(res.Data) : null,
                res.Message,
                res.Details,
                res.TraceIdentifier,
                factura.FiscalapiInvoiceId,
                factura.Uuid,
                exception: null,
                cancellationToken: cancellationToken);

            if (res.Succeeded && res.Data != null && !string.IsNullOrWhiteSpace(res.Data.Base64File))
            {
                var bytes = Convert.FromBase64String(res.Data.Base64File);
                xmlString = System.Text.Encoding.UTF8.GetString(bytes);
                factura.XmlContenido = xmlString;

                _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
                {
                    FacturaId = factura.Id,
                    TipoDocumento = "XML",
                    NombreArchivo = $"CFDI_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.xml",
                    MimeType = "application/xml",
                    TamanoBytes = bytes.Length,
                    Sha256Hash = ComputeSha256(bytes),
                    ContenidoBase64 = res.Data.Base64File,
                    FechaCreacionUtc = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"No se pudo recuperar el XML desde FiscalAPI: {res.Message}");
            }
        }
        else
        {
            throw new InvalidOperationException("La factura aún no cuenta con archivo XML generado.");
        }

        var nombre = $"CFDI_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.xml";
        return (System.Text.Encoding.UTF8.GetBytes(xmlString), "application/xml", nombre);
    }

    public async Task<(byte[] Bytes, string ContentType, string NombreArchivo)> DescargarPdfAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        var factura = await _dbContext.Invoices.FindAsync(new object[] { facturaId }, cancellationToken);
        if (factura == null)
        {
            throw new KeyNotFoundException($"No se encontró la factura con identificador {facturaId}.");
        }

        // 1. Revisar si ya existe en FacturacionDocumentos
        var docPdf = await _dbContext.FacturacionDocumentos
            .FirstOrDefaultAsync(d => d.FacturaId == facturaId && d.TipoDocumento == "PDF", cancellationToken);

        if (docPdf != null && !string.IsNullOrWhiteSpace(docPdf.ContenidoBase64))
        {
            return (Convert.FromBase64String(docPdf.ContenidoBase64), docPdf.MimeType, docPdf.NombreArchivo);
        }

        byte[] pdfBytes;
        if (!string.IsNullOrWhiteSpace(factura.PdfRuta) && File.Exists(factura.PdfRuta))
        {
            pdfBytes = await File.ReadAllBytesAsync(factura.PdfRuta, cancellationToken);
            _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
            {
                FacturaId = factura.Id,
                TipoDocumento = "PDF",
                NombreArchivo = $"Factura_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.pdf",
                MimeType = "application/pdf",
                TamanoBytes = pdfBytes.Length,
                Sha256Hash = ComputeSha256(pdfBytes),
                RutaAlmacenamientoPrivada = factura.PdfRuta,
                ContenidoBase64 = Convert.ToBase64String(pdfBytes),
                FechaCreacionUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(factura.FiscalapiInvoiceId))
        {
            var pdfReq = new CreatePdfRequest
            {
                InvoiceId = factura.FiscalapiInvoiceId,
                Base64Logo = _facturacionSettings.Base64Logo,
                BandColor = _facturacionSettings.BandColor,
                FontColor = _facturacionSettings.FontColor
            };

            var corrId = $"DOC-PDF-{factura.Id}-{Guid.NewGuid():N}";
            var logId = await _logService.RegistrarLlamadaIniciadaAsync(
                corrId,
                "GenerarPdf",
                "POST",
                "/v1/invoices/pdf",
                requestJson: System.Text.Json.JsonSerializer.Serialize(pdfReq),
                facturaId: factura.Id,
                ambiente: _facturacionSettings.Ambiente,
                cancellationToken: cancellationToken);

            var res = await _fiscalClient.GetPdfAsync(pdfReq, cancellationToken);

            await _logService.RegistrarLlamadaFinalizadaAsync(
                logId,
                res.HttpStatusCode,
                res.HttpStatusCode,
                res.Succeeded,
                res.Succeeded ? "Exitoso" : "Rechazado",
                responseJson: res.Data != null ? FacturacionSanitizer.SanitizeObject(res.Data) : null,
                res.Message,
                res.Details,
                res.TraceIdentifier,
                factura.FiscalapiInvoiceId,
                factura.Uuid,
                exception: null,
                cancellationToken: cancellationToken);

            if (res.Succeeded && res.Data != null && !string.IsNullOrWhiteSpace(res.Data.Base64File))
            {
                pdfBytes = Convert.FromBase64String(res.Data.Base64File);

                var storageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Invoices", "Pdf");
                Directory.CreateDirectory(storageDir);
                var localPath = Path.Combine(storageDir, $"{factura.Id}.pdf");
                await File.WriteAllBytesAsync(localPath, pdfBytes, cancellationToken);
                factura.PdfRuta = localPath;

                _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
                {
                    FacturaId = factura.Id,
                    TipoDocumento = "PDF",
                    NombreArchivo = $"Factura_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.pdf",
                    MimeType = "application/pdf",
                    TamanoBytes = pdfBytes.Length,
                    Sha256Hash = ComputeSha256(pdfBytes),
                    RutaAlmacenamientoPrivada = localPath,
                    ContenidoBase64 = res.Data.Base64File,
                    FechaCreacionUtc = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"No se pudo generar el PDF desde FiscalAPI: {res.Message}");
            }
        }
        else
        {
            throw new InvalidOperationException("La factura aún no cuenta con representación impresa PDF disponible.");
        }

        var nombre = $"Factura_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.pdf";
        return (pdfBytes, "application/pdf", nombre);
    }

    public async Task<bool> EnviarPorCorreoAsync(Guid facturaId, EnviarFacturaCorreoDto dto, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var factura = await _dbContext.Invoices.FindAsync(new object[] { facturaId }, cancellationToken);
        if (factura == null)
        {
            throw new KeyNotFoundException($"No se encontró la factura {facturaId}.");
        }

        if (string.IsNullOrWhiteSpace(factura.FiscalapiInvoiceId))
        {
            throw new InvalidOperationException("La factura no tiene un ID de comprobante registrado en FiscalAPI.");
        }

        var toEmail = !string.IsNullOrWhiteSpace(dto.EmailDestino) ? dto.EmailDestino.Trim() : factura.EmailReceptor;
        if (string.IsNullOrWhiteSpace(toEmail) || !toEmail.Contains('@'))
        {
            throw new InvalidOperationException("El correo destinatario es inválido.");
        }

        var req = new SendInvoiceRequest
        {
            InvoiceId = factura.FiscalapiInvoiceId,
            ToEmail = toEmail,
            Base64Logo = _facturacionSettings.Base64Logo,
            BandColor = _facturacionSettings.BandColor,
            FontColor = _facturacionSettings.FontColor
        };

        var validUsuarioId = ObtenerUsuarioIdValido(usuarioId);
        var correlationId = $"CORR-EMAIL-{factura.Id}-{Guid.NewGuid():N}";
        var logId = await _logService.RegistrarLlamadaIniciadaAsync(
            correlationId,
            operacion: "EnviarCorreo",
            metodoHttp: "POST",
            ruta: "/v1/invoices/email",
            requestJson: System.Text.Json.JsonSerializer.Serialize(req),
            facturaId: factura.Id,
            usuarioId: validUsuarioId,
            ambiente: _facturacionSettings.Ambiente,
            cancellationToken: cancellationToken);

        var res = await _fiscalClient.SendInvoiceEmailAsync(req, cancellationToken);

        await _logService.RegistrarLlamadaFinalizadaAsync(
            logId,
            res.HttpStatusCode,
            res.HttpStatusCode,
            res.Succeeded,
            res.Succeeded ? "Exitoso" : "Rechazado",
            responseJson: res.Data != null ? FacturacionSanitizer.SanitizeObject(res.Data) : null,
            res.Message,
            res.Details,
            res.TraceIdentifier,
            factura.FiscalapiInvoiceId,
            factura.Uuid,
            exception: null,
            cancellationToken: cancellationToken);

        if (!res.Succeeded)
        {
            throw new InvalidOperationException($"Error al enviar correo mediante FiscalAPI: {res.Message}");
        }

        _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
        {
            FacturaId = factura.Id,
            EstadoAnterior = factura.Estado,
            EstadoNuevo = factura.Estado,
            Detalle = $"Factura enviada por correo electrónico a {toEmail}.",
            UsuarioId = validUsuarioId,
            FechaCreacionUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            "FACTURA_ENVIADA_CORREO",
            "Factura",
            factura.Id,
            usuarioId,
            $"Factura UUID {factura.Uuid} enviada por correo a {toEmail} (CorrelationId: {correlationId}).",
            cancellationToken);

        return true;
    }

    public async Task<ResultadoEstadoSatDto> ConsultarEstadoSatAsync(Guid facturaId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var factura = await _dbContext.Invoices
            .Include(f => f.HistorialEstados)
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null)
        {
            throw new KeyNotFoundException($"No se encontró la factura {facturaId}.");
        }

        if (string.IsNullOrWhiteSpace(factura.Uuid))
        {
            throw new InvalidOperationException("La factura no cuenta con UUID registrado para consulta ante el SAT.");
        }

        var req = new InvoiceStatusRequest();
        if (!string.IsNullOrWhiteSpace(factura.FiscalapiInvoiceId))
        {
            req.Id = factura.FiscalapiInvoiceId;
        }
        else
        {
            req.InvoiceUuid = factura.Uuid;
            req.IssuerTin = factura.RfcEmisor;
            req.RecipientTin = factura.RfcReceptor;
            req.InvoiceTotal = factura.Total;
        }

        var validUsuarioId = ObtenerUsuarioIdValido(usuarioId);
        var correlationId = $"CORR-SAT-{factura.Id}-{Guid.NewGuid():N}";
        var logId = await _logService.RegistrarLlamadaIniciadaAsync(
            correlationId,
            operacion: "ConsultarEstadoSat",
            metodoHttp: "POST",
            ruta: "/v1/invoices/status",
            requestJson: System.Text.Json.JsonSerializer.Serialize(req),
            facturaId: factura.Id,
            usuarioId: validUsuarioId,
            ambiente: _facturacionSettings.Ambiente,
            cancellationToken: cancellationToken);

        var res = await _fiscalClient.GetInvoiceStatusAsync(req, cancellationToken);

        await _logService.RegistrarLlamadaFinalizadaAsync(
            logId,
            res.HttpStatusCode,
            res.HttpStatusCode,
            res.Succeeded,
            res.Succeeded ? "Exitoso" : "Rechazado",
            responseJson: res.Data != null ? FacturacionSanitizer.SanitizeObject(res.Data) : null,
            res.Message,
            res.Details,
            res.TraceIdentifier,
            factura.FiscalapiInvoiceId,
            factura.Uuid,
            exception: null,
            cancellationToken: cancellationToken);

        if (!res.Succeeded || res.Data == null)
        {
            throw new InvalidOperationException($"No se pudo obtener el estado del SAT desde FiscalAPI: {res.Message}");
        }

        var statusData = res.Data;
        factura.EstadoSat = statusData.Status; // Vigente, Cancelado, No Encontrado
        factura.EstadoCancelacionSat = statusData.CancellationStatus ?? statusData.CancelableStatus;

        var estadoAnterior = factura.Estado;
        if (string.Equals(statusData.Status, "Cancelado", StringComparison.OrdinalIgnoreCase))
        {
            factura.Estado = FacturaEstados.Cancelada;
            factura.FechaCancelacionUtc ??= DateTime.UtcNow;
        }

        _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
        {
            FacturaId = factura.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = factura.Estado,
            Detalle = $"Consulta de estado ante el SAT: Estado={statusData.Status}, Cancelable={statusData.CancelableStatus}, EstatusCancelación={statusData.CancellationStatus}, EFOS={statusData.EfosValidation}.",
            UsuarioId = validUsuarioId,
            FechaCreacionUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoEstadoSatDto
        {
            CodigoEstatus = statusData.StatusCode ?? string.Empty,
            Estado = statusData.Status ?? string.Empty,
            EsCancelable = statusData.CancelableStatus ?? string.Empty,
            EstatusCancelacion = statusData.CancellationStatus ?? string.Empty,
            ValidacionEfos = statusData.EfosValidation ?? string.Empty,
            EstadoLocalActualizado = factura.Estado,
            FechaConsultaUtc = DateTime.UtcNow
        };
    }

    public async Task<ResultadoCancelacionFacturaDto> SolicitarCancelacionAsync(
        Guid facturaId,
        SolicitarCancelacionFacturaDto dto,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var factura = await _dbContext.Invoices
            .Include(f => f.HistorialEstados)
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null)
        {
            throw new KeyNotFoundException($"No se encontró la factura {facturaId}.");
        }

        if (factura.Estado == FacturaEstados.Cancelada)
        {
            return new ResultadoCancelacionFacturaDto
            {
                Exitoso = true,
                EstadoLocal = factura.Estado,
                EstadoSat = factura.EstadoSat,
                Mensaje = "La factura ya se encuentra cancelada."
            };
        }

        if (factura.Estado != FacturaEstados.Timbrada && factura.Estado != FacturaEstados.CancelacionSolicitada)
        {
            throw new InvalidOperationException($"No se puede cancelar una factura en estado '{factura.Estado}'. Solo facturas Timbradas pueden ser canceladas.");
        }

        var motivo = (dto.MotivoCancelacion ?? "02").Trim();
        if (motivo != "01" && motivo != "02" && motivo != "03" && motivo != "04")
        {
            throw new InvalidOperationException("El motivo de cancelación del SAT debe ser 01, 02, 03 o 04.");
        }

        var uuidSustitucion = dto.UuidSustitucion?.Trim();
        if (motivo == "01" && string.IsNullOrWhiteSpace(uuidSustitucion))
        {
            throw new InvalidOperationException("El motivo de cancelación '01' requiere especificar el UUID del comprobante sustituto.");
        }

        var correlationId = $"CORR-CANCEL-{factura.Id}-{Guid.NewGuid():N}";
        var cancelReq = new CancelInvoiceRequest
        {
            CancellationReasonCode = motivo,
            ReplacementUuid = uuidSustitucion
        };
        if (!string.IsNullOrWhiteSpace(factura.FiscalapiInvoiceId))
        {
            cancelReq.Id = factura.FiscalapiInvoiceId;
        }
        else
        {
            cancelReq.InvoiceUuid = factura.Uuid;
            cancelReq.Rfc = factura.RfcEmisor;
        }

        var validUsuarioId = ObtenerUsuarioIdValido(usuarioId);
        var logId = await _logService.RegistrarLlamadaIniciadaAsync(
            correlationId,
            operacion: "Cancelar",
            metodoHttp: "POST",
            ruta: "/v1/invoices/cancel",
            requestJson: System.Text.Json.JsonSerializer.Serialize(cancelReq),
            facturaId: factura.Id,
            usuarioId: validUsuarioId,
            ambiente: _facturacionSettings.Ambiente,
            cancellationToken: cancellationToken);

        var res = await _fiscalClient.CancelInvoiceAsync(cancelReq, cancellationToken);

        await _logService.RegistrarLlamadaFinalizadaAsync(
            logId,
            res.HttpStatusCode,
            res.HttpStatusCode,
            res.Succeeded,
            res.Succeeded ? "Exitoso" : "Rechazado",
            responseJson: res.Data != null ? FacturacionSanitizer.SanitizeObject(res.Data) : null,
            res.Message,
            res.Details,
            res.TraceIdentifier,
            factura.FiscalapiInvoiceId,
            factura.Uuid,
            exception: null,
            cancellationToken: cancellationToken);

        var cancelacion = new FacturacionCancelacion
        {
            FacturaId = factura.Id,
            CorrelationId = correlationId,
            MotivoCancelacion = motivo,
            UuidSustitucion = uuidSustitucion,
            FechaSolicitudUtc = DateTime.UtcNow,
            UsuarioId = validUsuarioId,
            Exitoso = res.Succeeded,
            CodigoEstadoHttp = res.HttpStatusCode,
            MensajeRespuesta = res.Message,
            DetallesRespuesta = res.Details
        };

        if (!res.Succeeded || res.Data == null)
        {
            _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
            {
                FacturaId = factura.Id,
                EstadoAnterior = factura.Estado,
                EstadoNuevo = factura.Estado,
                Detalle = $"Fallo al solicitar cancelación ante FiscalAPI: {res.Message}. Detalles: {res.Details}",
                UsuarioId = validUsuarioId,
                FechaCreacionUtc = DateTime.UtcNow
            });
            _dbContext.FacturacionCancelaciones.Add(cancelacion);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ResultadoCancelacionFacturaDto
            {
                Exitoso = false,
                Mensaje = !string.IsNullOrWhiteSpace(res.Message) ? res.Message : "No se pudo procesar la cancelación ante el proveedor fiscal."
            };
        }

        var cancelData = res.Data;
        factura.MotivoCancelacion = motivo;
        factura.UuidSustitucion = uuidSustitucion;
        factura.FechaSolicitudCancelacionUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(cancelData.Base64CancellationAcknowledgement))
        {
            factura.AcuseCancelacionXml = cancelData.Base64CancellationAcknowledgement;
            cancelacion.AcuseXml = cancelData.Base64CancellationAcknowledgement;

            var bytesAcuse = Convert.FromBase64String(cancelData.Base64CancellationAcknowledgement);
            _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
            {
                FacturaId = factura.Id,
                TipoDocumento = "AcuseCancelacion",
                NombreArchivo = $"Acuse_Cancelacion_{factura.Serie}_{factura.Folio}_{factura.Uuid}.xml",
                MimeType = "application/xml",
                TamanoBytes = bytesAcuse.Length,
                Sha256Hash = ComputeSha256(bytesAcuse),
                ContenidoBase64 = cancelData.Base64CancellationAcknowledgement,
                FechaCreacionUtc = DateTime.UtcNow
            });
        }

        // Evaluar resultado reportado en el diccionario de respuestas
        var estadoSat = "Cancelado";
        if (cancelData.InvoiceUuids != null && factura.Uuid != null && cancelData.InvoiceUuids.TryGetValue(factura.Uuid, out var statusSat))
        {
            estadoSat = statusSat;
        }

        cancelacion.EstadoSat = estadoSat;

        // No marcar inmediatamente como Cancelada si requiere aceptación del receptor
        if (estadoSat.Contains("Plazo vencido", StringComparison.OrdinalIgnoreCase) ||
            estadoSat.Contains("Cancelado", StringComparison.OrdinalIgnoreCase) ||
            estadoSat == "201" || estadoSat == "202")
        {
            factura.Estado = FacturaEstados.Cancelada;
            factura.EstadoSat = "Cancelado";
            factura.FechaCancelacionUtc = DateTime.UtcNow;
            cancelacion.FechaEfectivaCancelacionUtc = DateTime.UtcNow;
        }
        else
        {
            factura.Estado = FacturaEstados.CancelacionSolicitada;
            factura.EstadoSat = estadoSat;
        }

        _dbContext.FacturacionCancelaciones.Add(cancelacion);

        _dbContext.InvoiceStatusHistories.Add(new FacturacionHistorialEstado
        {
            FacturaId = factura.Id,
            EstadoAnterior = FacturaEstados.Timbrada,
            EstadoNuevo = factura.Estado,
            Detalle = $"Solicitud de cancelación procesada con motivo '{motivo}'. Respuesta SAT: {estadoSat}.",
            UsuarioId = validUsuarioId,
            FechaCreacionUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            "FACTURA_CANCELACION_SOLICITADA",
            "Factura",
            factura.Id,
            usuarioId,
            $"Solicitud de cancelación enviada para Factura UUID {factura.Uuid} (Motivo: {motivo}, Estado resultante: {factura.Estado}, CorrelationId: {correlationId}).",
            cancellationToken);

        return new ResultadoCancelacionFacturaDto
        {
            Exitoso = true,
            EstadoLocal = factura.Estado,
            EstadoSat = factura.EstadoSat,
            EstadoCancelacionSat = estadoSat,
            TieneAcuseXml = !string.IsNullOrWhiteSpace(factura.AcuseCancelacionXml),
            Mensaje = factura.Estado == FacturaEstados.Cancelada
                ? "La factura fue cancelada exitosamente ante el SAT."
                : "La solicitud de cancelación fue registrada ante el SAT y queda en espera de aceptación o validación."
        };
    }

    public Task<IReadOnlyList<CatalogoSatItemDto>> ObtenerCatalogoSatAsync(string nombreCatalogo, CancellationToken cancellationToken = default)
    {
        var norm = (nombreCatalogo ?? "").Trim().ToLowerInvariant();
        IReadOnlyList<CatalogoSatItemDto> items = norm switch
        {
            "regimenes" or "regimenes-fiscales" => new List<CatalogoSatItemDto>
            {
                new() { Clave = "601", Descripcion = "General de Ley Personas Morales" },
                new() { Clave = "603", Descripcion = "Personas Morales con Fines no Lucrativos" },
                new() { Clave = "605", Descripcion = "Sueldos y Salarios e Ingresos Asimilados a Salarios" },
                new() { Clave = "606", Descripcion = "Arrendamiento" },
                new() { Clave = "612", Descripcion = "Personas Físicas con Actividades Empresariales y Profesionales" },
                new() { Clave = "616", Descripcion = "Sin obligaciones fiscales" },
                new() { Clave = "621", Descripcion = "Incorporación Fiscal" },
                new() { Clave = "625", Descripcion = "Régimen de las Actividades Empresariales con ingresos a través de Plataformas Tecnológicas" },
                new() { Clave = "626", Descripcion = "Régimen Simplificado de Confianza (RESICO)" }
            },
            "usos" or "usos-cfdi" => new List<CatalogoSatItemDto>
            {
                new() { Clave = "G01", Descripcion = "Adquisición de mercancías" },
                new() { Clave = "G02", Descripcion = "Devoluciones, descuentos o bonificaciones" },
                new() { Clave = "G03", Descripcion = "Gastos en general" },
                new() { Clave = "I01", Descripcion = "Construcciones" },
                new() { Clave = "I02", Descripcion = "Mobiliario y equipo de oficina por inversiones" },
                new() { Clave = "I04", Descripcion = "Equipo de cómputo y accesorios" },
                new() { Clave = "S01", Descripcion = "Sin efectos fiscales" },
                new() { Clave = "CP01", Descripcion = "Pagos" }
            },
            "formas-pago" => new List<CatalogoSatItemDto>
            {
                new() { Clave = "01", Descripcion = "Efectivo" },
                new() { Clave = "02", Descripcion = "Cheque nominativo" },
                new() { Clave = "03", Descripcion = "Transferencia electrónica de fondos" },
                new() { Clave = "04", Descripcion = "Tarjeta de crédito" },
                new() { Clave = "28", Descripcion = "Tarjeta de débito" },
                new() { Clave = "29", Descripcion = "Tarjeta de servicios" },
                new() { Clave = "99", Descripcion = "Por definir" }
            },
            "motivos-cancelacion" => new List<CatalogoSatItemDto>
            {
                new() { Clave = "01", Descripcion = "Comprobante emitido con errores con relación" },
                new() { Clave = "02", Descripcion = "Comprobante emitido con errores sin relación" },
                new() { Clave = "03", Descripcion = "No se llevó a cabo la operación" },
                new() { Clave = "04", Descripcion = "Operación nominativa relacionada en una factura global" }
            },
            _ => new List<CatalogoSatItemDto>()
        };

        return Task.FromResult(items);
    }

    // Bitácora técnica y diagnóstico FiscalAPI
    public Task<PagedResult<FacturacionLogDto>> ObtenerBitacoraAsync(FacturacionLogFiltrosDto filtros, CancellationToken cancellationToken = default)
        => _logService.ObtenerLogsAsync(filtros, cancellationToken);

    public Task<FacturacionLogDto?> ObtenerBitacoraPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _logService.ObtenerLogPorIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<FacturacionLogDto>> ObtenerLlamadasRelacionadasAsync(string correlationId, CancellationToken cancellationToken = default)
        => _logService.ObtenerLlamadasRelacionadasAsync(correlationId, cancellationToken);

    public Task<int> PurgarBitacoraAntiguaAsync(int? diasRetencion = null, CancellationToken cancellationToken = default)
        => _logService.PurgarLogsAntiguosAsync(diasRetencion ?? _facturacionSettings.BitacoraRetencionDias, cancellationToken);

    private async Task DescargarYGuardarDocumentosAsync(Guid facturaId, string? fiscalapiInvoiceId)
    {
        if (string.IsNullOrWhiteSpace(fiscalapiInvoiceId)) return;

        var factura = await _dbContext.Invoices.FindAsync(facturaId);
        if (factura == null) return;

        // XML
        var docXml = await _dbContext.FacturacionDocumentos.FirstOrDefaultAsync(d => d.FacturaId == facturaId && d.TipoDocumento == "XML");
        if (docXml == null)
        {
            var resXml = await _fiscalClient.GetXmlAsync(fiscalapiInvoiceId);
            if (resXml.Succeeded && resXml.Data != null && !string.IsNullOrWhiteSpace(resXml.Data.Base64File))
            {
                var bytes = Convert.FromBase64String(resXml.Data.Base64File);
                factura.XmlContenido = System.Text.Encoding.UTF8.GetString(bytes);

                _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
                {
                    FacturaId = factura.Id,
                    TipoDocumento = "XML",
                    NombreArchivo = $"CFDI_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.xml",
                    MimeType = "application/xml",
                    TamanoBytes = bytes.Length,
                    Sha256Hash = ComputeSha256(bytes),
                    ContenidoBase64 = resXml.Data.Base64File,
                    FechaCreacionUtc = DateTime.UtcNow
                });
            }
        }

        // PDF
        var docPdf = await _dbContext.FacturacionDocumentos.FirstOrDefaultAsync(d => d.FacturaId == facturaId && d.TipoDocumento == "PDF");
        if (docPdf == null)
        {
            var pdfReq = new CreatePdfRequest
            {
                InvoiceId = fiscalapiInvoiceId,
                Base64Logo = _facturacionSettings.Base64Logo,
                BandColor = _facturacionSettings.BandColor,
                FontColor = _facturacionSettings.FontColor
            };
            var resPdf = await _fiscalClient.GetPdfAsync(pdfReq);
            if (resPdf.Succeeded && resPdf.Data != null && !string.IsNullOrWhiteSpace(resPdf.Data.Base64File))
            {
                var bytes = Convert.FromBase64String(resPdf.Data.Base64File);
                var storageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Invoices", "Pdf");
                Directory.CreateDirectory(storageDir);
                var localPath = Path.Combine(storageDir, $"{factura.Id}.pdf");
                await File.WriteAllBytesAsync(localPath, bytes);
                factura.PdfRuta = localPath;

                _dbContext.FacturacionDocumentos.Add(new FacturacionDocumento
                {
                    FacturaId = factura.Id,
                    TipoDocumento = "PDF",
                    NombreArchivo = $"Factura_{factura.Serie}_{factura.Folio}_{factura.Uuid ?? factura.IdVenta.ToString()}.pdf",
                    MimeType = "application/pdf",
                    TamanoBytes = bytes.Length,
                    Sha256Hash = ComputeSha256(bytes),
                    RutaAlmacenamientoPrivada = localPath,
                    ContenidoBase64 = resPdf.Data.Base64File,
                    FechaCreacionUtc = DateTime.UtcNow
                });
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private static string ComputeSha256(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string DeterminarFormaPagoSat(Venta sale)
    {
        if (sale.MontoTarjeta > 0 && sale.MontoEfectivo == 0 && sale.MontoTransferencia == 0)
        {
            return SatCatalogosConstantes.FormasPago.TarjetaCredito; // O 28 Débito
        }
        if (sale.MontoTransferencia > 0 && sale.MontoEfectivo == 0 && sale.MontoTarjeta == 0)
        {
            return SatCatalogosConstantes.FormasPago.TransferenciaElectronica;
        }
        if (sale.MontoEfectivo > 0 && sale.MontoTarjeta == 0 && sale.MontoTransferencia == 0)
        {
            return SatCatalogosConstantes.FormasPago.Efectivo;
        }

        // Si es pago mixto
        if (sale.MontoTarjeta >= sale.MontoEfectivo && sale.MontoTarjeta >= sale.MontoTransferencia && sale.MontoTarjeta > 0)
        {
            return SatCatalogosConstantes.FormasPago.TarjetaCredito;
        }
        if (sale.MontoTransferencia >= sale.MontoEfectivo && sale.MontoTransferencia >= sale.MontoTarjeta && sale.MontoTransferencia > 0)
        {
            return SatCatalogosConstantes.FormasPago.TransferenciaElectronica;
        }

        return SatCatalogosConstantes.FormasPago.Efectivo;
    }

    private static string ObtenerDescripcionFormaPagoSat(string claveFormaPago) => claveFormaPago switch
    {
        "01" => "01 - Efectivo",
        "02" => "02 - Cheque nominativo",
        "03" => "03 - Transferencia electrónica de fondos",
        "04" => "04 - Tarjeta de crédito",
        "28" => "28 - Tarjeta de débito",
        "29" => "29 - Tarjeta de servicios",
        "99" => "99 - Por definir",
        _ => claveFormaPago
    };

    private static string MapearClaveUnidadSat(string? unidadMedida)
    {
        var norm = (unidadMedida ?? "").Trim().ToLowerInvariant();
        return norm switch
        {
            "caja" or "cjas" or "caj" => SatCatalogosConstantes.ClaveUnidadCajaDefault,
            "m2" or "mtk" or "metro cuadrado" => SatCatalogosConstantes.ClaveUnidadMetroCuadradoDefault,
            _ => SatCatalogosConstantes.ClaveUnidadPiezaDefault // H87 Pieza
        };
    }

    private static bool EsRfcValido(string rfc)
    {
        if (string.IsNullOrWhiteSpace(rfc)) return false;
        var r = rfc.Trim().ToUpperInvariant();
        if (r == SatCatalogosConstantes.RfcGenericoNacional || r == SatCatalogosConstantes.RfcGenericoExtranjero)
            return true;
        return RfcFisicaRegex.IsMatch(r) || RfcMoralRegex.IsMatch(r);
    }

    private Task RegistrarAuditoriaAsync(string action, string entityName, Guid entityId, Guid? usuarioId, string detalle, CancellationToken ct)
    {
        return _auditLogService.LogAsync(
            correlationId: Guid.NewGuid().ToString(),
            userId: usuarioId,
            action: action,
            entityName: entityName,
            entityId: entityId.ToString(),
            oldValuesJson: null,
            newValuesJson: null,
            ipAddress: "127.0.0.1",
            reason: detalle,
            module: "Facturacion",
            eventType: action,
            resultStatus: "Success",
            cancellationToken: ct);
    }

    private Guid? ObtenerUsuarioIdValido(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty) return null;
        return _dbContext.Users.Any(u => u.Id == usuarioId) ? usuarioId : null;
    }
}
