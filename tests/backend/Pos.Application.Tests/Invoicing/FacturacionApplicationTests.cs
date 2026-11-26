using Fiscalapi.Common;
using Fiscalapi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pos.Application.Invoicing.Dtos;
using Pos.Domain.Common;
using Pos.Domain.Entidades;
using Pos.Infrastructure.Identity;
using Pos.Infrastructure.Invoicing.Clients;
using Pos.Infrastructure.Invoicing.Logging;
using Pos.Infrastructure.Invoicing.Options;
using Pos.Infrastructure.Invoicing.Services;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Services;
using Xunit;

namespace Pos.Application.Tests.Invoicing;

public class FacturacionApplicationTests
{
    private PosDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new PosDbContext(options);
    }

    private class FakeFiscalApiClientAdapter : IFiscalApiClientAdapter
    {
        public Func<Invoice, string, Task<ApiResponse<Invoice>>>? OnCreateInvoice { get; set; }
        public Func<CancelInvoiceRequest, Task<ApiResponse<CancelInvoiceResponse>>>? OnCancelInvoice { get; set; }
        public Func<CreatePdfRequest, Task<ApiResponse<FileResponse>>>? OnGetPdf { get; set; }
        public Func<string, Task<ApiResponse<FileResponse>>>? OnGetXml { get; set; }
        public Func<SendInvoiceRequest, Task<ApiResponse<bool>>>? OnSendEmail { get; set; }
        public Func<InvoiceStatusRequest, Task<ApiResponse<InvoiceStatusResponse>>>? OnGetStatus { get; set; }

        public Task<ApiResponse<Invoice>> CreateInvoiceAsync(Invoice requestModel, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            if (OnCreateInvoice != null) return OnCreateInvoice(requestModel, idempotencyKey);
            return Task.FromResult(new ApiResponse<Invoice>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = new Invoice
                {
                    Id = Guid.NewGuid().ToString(),
                    Uuid = Guid.NewGuid().ToString().ToUpper(),
                    Date = DateTime.UtcNow
                },
                TraceIdentifier = "fake-trace-id"
            });
        }

        public Task<ApiResponse<CancelInvoiceResponse>> CancelInvoiceAsync(CancelInvoiceRequest requestModel, CancellationToken cancellationToken = default)
        {
            if (OnCancelInvoice != null) return OnCancelInvoice(requestModel);
            var uuid = requestModel.InvoiceUuid ?? "fake-uuid";
            return Task.FromResult(new ApiResponse<CancelInvoiceResponse>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = new CancelInvoiceResponse
                {
                    Base64CancellationAcknowledgement = "PD94bWwgdmVyc2lvbj0iMS4wIj8+PGFjdXNlPjwvYWN1c2U+",
                    InvoiceUuids = new Dictionary<string, string> { { uuid, "Cancelado" } }
                }
            });
        }

        public Task<ApiResponse<FileResponse>> GetPdfAsync(CreatePdfRequest requestModel, CancellationToken cancellationToken = default)
        {
            if (OnGetPdf != null) return OnGetPdf(requestModel);
            return Task.FromResult(new ApiResponse<FileResponse>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = new FileResponse
                {
                    Base64File = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }),
                    FileName = "factura.pdf",
                    FileExtension = ".pdf"
                }
            });
        }

        public Task<ApiResponse<FileResponse>> GetXmlAsync(string invoiceId, CancellationToken cancellationToken = default)
        {
            if (OnGetXml != null) return OnGetXml(invoiceId);
            return Task.FromResult(new ApiResponse<FileResponse>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = new FileResponse
                {
                    Base64File = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<cfdi:Comprobante/>")),
                    FileName = "factura.xml",
                    FileExtension = ".xml"
                }
            });
        }

        public Task<ApiResponse<bool>> SendInvoiceEmailAsync(SendInvoiceRequest requestModel, CancellationToken cancellationToken = default)
        {
            if (OnSendEmail != null) return OnSendEmail(requestModel);
            return Task.FromResult(new ApiResponse<bool>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = true
            });
        }

        public Task<ApiResponse<InvoiceStatusResponse>> GetInvoiceStatusAsync(InvoiceStatusRequest requestModel, CancellationToken cancellationToken = default)
        {
            if (OnGetStatus != null) return OnGetStatus(requestModel);
            return Task.FromResult(new ApiResponse<InvoiceStatusResponse>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = new InvoiceStatusResponse
                {
                    Status = "Vigente",
                    CancelableStatus = "Cancelable sin aceptación",
                    CancellationStatus = "No cancelado"
                }
            });
        }
    }

    private (FacturacionApplicationService service, PosDbContext context, FakeFiscalApiClientAdapter adapter) CreateService(
        PosDbContext? context = null,
        string ambiente = FacturacionAmbientes.QA)
    {
        var db = context ?? GetInMemoryDbContext();
        var audit = new AuditLogService(db, NullLogger<AuditLogService>.Instance);
        var settings = new FacturacionSettings
        {
            Ambiente = ambiente,
            Habilitada = true,
            TimeZone = "America/Mexico_City"
        };
        var fiscalapiSettings = new FiscalapiSettings
        {
            ApiUrl = ambiente == FacturacionAmbientes.PR ? "https://live.fiscalapi.com" : "https://test.fiscalapi.com",
            ApiKey = "fake_key_123",
            Tenant = "fake_tenant_123"
        };
        var adapter = new FakeFiscalApiClientAdapter();
        var logService = new FacturacionLogService(db, NullLogger<FacturacionLogService>.Instance);
        var service = new FacturacionApplicationService(
            db,
            adapter,
            Options.Create(settings),
            Options.Create(fiscalapiSettings),
            audit,
            logService,
            NullLogger<FacturacionApplicationService>.Instance);

        return (service, db, adapter);
    }

    private static async Task<Usuario> SeedIssuerConfigAsync(PosDbContext context, string ambiente = FacturacionAmbientes.QA)
    {
        var testUser = new Usuario
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            NombreUsuario = "adminfiscal",
            Email = "admin@wpc.mx"
        };
        if (!context.Users.Any(u => u.Id == testUser.Id))
        {
            context.Users.Add(testUser);
        }

        context.IssuerConfigs.Add(new FacturacionConfiguracion
        {
            Empresa = "WPC Bajío",
            Sucursal = "Matriz",
            Rfc = "WPC120101ABC",
            RazonSocial = "WPC BAJIO SA DE CV",
            RegimenFiscal = "601",
            CodigoPostalExpedicion = "37000",
            Serie = "F",
            FolioSiguiente = 100,
            Ambiente = ambiente,
            EstaListoParaEmitir = true,
            EstaActivo = true
        });
        await context.SaveChangesAsync();
        return testUser;
    }

    private static async Task<Venta> SeedSaleAsync(
        PosDbContext context,
        decimal total = 1160m,
        decimal saldoPendiente = 0m,
        decimal montoIva = 160m,
        string status = SaleStatuses.Completed)
    {
        var product = new Producto
        {
            Sku = "LAM-TEST-01",
            Nombre = "Lambrín WPC Roble",
            PrecioUnitario = 500m,
            ClaveProdServSat = "30161500",
            ClaveUnidadSat = "H87",
            ObjetoImpuestoSat = "02",
            TasaIva = 0.16m
        };
        context.Products.Add(product);

        var subTotalPartida = montoIva > 0 ? (total - montoIva) : total;
        var sale = new Venta
        {
            NumeroFolio = $"VENTA-{Guid.NewGuid():N}",
            IdVenta = 101,
            SubTotal = total - montoIva,
            MontoIva = montoIva,
            MontoTotal = total,
            SaldoPendiente = saldoPendiente,
            Estado = status,
            TipoPago = saldoPendiente > 0 ? SalePaymentTypes.AdvanceDeposit : SalePaymentTypes.FullPayment,
            Partidas = new List<PartidaVenta>
            {
                new PartidaVenta
                {
                    Producto = product,
                    Cantidad = 2,
                    PrecioUnitario = subTotalPartida / 2m,
                    PrecioTotal = subTotalPartida
                }
            }
        };
        context.Sales.Add(sale);
        await context.SaveChangesAsync();
        return sale;
    }

    [Fact]
    public async Task GenerarVistaPreviaVentaAsync_ShouldCalculateTaxesCorrectly_WhenSaleHasExplicitIva()
    {
        // Arrange
        var (service, context, _) = CreateService();
        await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context, total: 1160m, montoIva: 160m);

        // Act
        var preview = await service.GenerarVistaPreviaVentaAsync(sale.Id);

        // Assert
        Assert.NotNull(preview);
        Assert.True(preview.EsElegibleParaFacturar);
        Assert.Equal(1000m, preview.Subtotal);
        Assert.Equal(160m, preview.ImpuestoTrasladadoIva);
        Assert.Equal(1160m, preview.Total);
        Assert.Single(preview.Conceptos);
        Assert.Equal("30161500", preview.Conceptos[0].ClaveProdServSat);
    }

    [Fact]
    public async Task GenerarVistaPreviaVentaAsync_ShouldBackOutIva_WhenSalePricesIncludeIva()
    {
        // Arrange (Sale where MontoIva was 0 at POS, so price was VAT-inclusive)
        var (service, context, _) = CreateService();
        await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context, total: 1160m, montoIva: 0m);

        // Act
        var preview = await service.GenerarVistaPreviaVentaAsync(sale.Id);

        // Assert
        Assert.NotNull(preview);
        Assert.True(preview.EsElegibleParaFacturar);
        Assert.Equal(1000m, preview.Subtotal);
        Assert.Equal(160m, preview.ImpuestoTrasladadoIva);
        Assert.Equal(1160m, preview.Total);
    }

    [Fact]
    public async Task GenerarVistaPreviaVentaAsync_ShouldBlockCreditSaleWithPendingBalance()
    {
        // Arrange
        var (service, context, _) = CreateService();
        await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context, total: 1160m, saldoPendiente: 660m, status: SaleStatuses.DepositPaid);

        // Act
        var preview = await service.GenerarVistaPreviaVentaAsync(sale.Id);

        // Assert
        Assert.False(preview.EsElegibleParaFacturar);
        Assert.NotEmpty(preview.Bloqueos);
        Assert.Contains("cuenta con saldo pendiente", preview.Bloqueos[0]);
    }

    [Fact]
    public async Task EmitirFacturaVentaAsync_ShouldBlockCreditSaleWithPendingBalance()
    {
        // Arrange
        var (service, context, _) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context, total: 1160m, saldoPendiente: 500m);

        var dto = new SolicitarEmisionFacturaDto
        {
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            RegimenFiscalReceptor = "616",
            UsoCfdi = "S01",
            CodigoPostalReceptor = "37000"
        };

        // Act
        var result = await service.EmitirFacturaVentaAsync(sale.Id, dto, user.Id);

        // Assert
        Assert.False(result.Exitoso);
        Assert.Contains("saldo pendiente", result.Mensaje);
    }

    [Fact]
    public async Task EmitirFacturaVentaAsync_ShouldStampSuccessfully_WhenDataIsValid()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context, total: 1160m);

        var expectedUuid = Guid.NewGuid().ToString().ToUpper();
        adapter.OnCreateInvoice = (invoice, key) =>
        {
            return Task.FromResult(new ApiResponse<Invoice>
            {
                Succeeded = true,
                HttpStatusCode = 200,
                Data = new Invoice
                {
                    Id = "fiscalapi-inv-123",
                    Uuid = expectedUuid,
                    Date = DateTime.UtcNow
                },
                TraceIdentifier = "trace-test-123"
            });
        };

        var dto = new SolicitarEmisionFacturaDto
        {
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            RegimenFiscalReceptor = "616",
            UsoCfdi = "S01",
            CodigoPostalReceptor = "37000"
        };

        // Act
        var result = await service.EmitirFacturaVentaAsync(sale.Id, dto, user.Id);

        // Assert
        Assert.True(result.Exitoso);
        Assert.Equal(expectedUuid, result.Uuid);
        Assert.Equal(FacturaEstados.Timbrada, result.Estado);

        // Verify database state
        var facturaDb = await context.Invoices
            .Include(f => f.Partidas)
            .Include(f => f.HistorialEstados)
            .FirstOrDefaultAsync(f => f.VentaId == sale.Id);

        Assert.NotNull(facturaDb);
        Assert.Equal(FacturaEstados.Timbrada, facturaDb.Estado);
        Assert.Equal(expectedUuid, facturaDb.Uuid);
        Assert.Equal(1160m, facturaDb.Total);
        Assert.Single(facturaDb.Partidas);
        Assert.NotNull(facturaDb.XmlContenido);
        Assert.NotNull(facturaDb.PdfRuta);
        Assert.NotEmpty(facturaDb.HistorialEstados);

        // Verify Issuer Folio incremented
        var emisor = await context.IssuerConfigs.FirstAsync();
        Assert.Equal(101, emisor.FolioSiguiente);
    }

    [Fact]
    public async Task EmitirFacturaVentaAsync_ShouldHandleRejection_AndMarkAsRechazada()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context, total: 1160m);

        adapter.OnCreateInvoice = (invoice, key) =>
        {
            return Task.FromResult(new ApiResponse<Invoice>
            {
                Succeeded = false,
                HttpStatusCode = 400,
                Data = null!,
                Message = "CFDI40145 - El campo Rfc del receptor no es válido según la lista de RFC inscritos no cancelados.",
                Details = "Error SAT LCO",
                TraceIdentifier = "trace-rejected-999"
            });
        };

        var dto = new SolicitarEmisionFacturaDto
        {
            RfcReceptor = "AAA010101AAA",
            RazonSocialReceptor = "EMPRESA FALSA SA DE CV",
            RegimenFiscalReceptor = "601",
            UsoCfdi = "G03",
            CodigoPostalReceptor = "37000"
        };

        // Act
        var result = await service.EmitirFacturaVentaAsync(sale.Id, dto, user.Id);

        // Assert
        Assert.False(result.Exitoso);
        Assert.Equal(FacturaEstados.Rechazada, result.Estado);
        Assert.Contains("CFDI40145", result.Mensaje);

        // Verify in DB that it is marked as Rechazada
        var facturaDb = await context.Invoices.FirstOrDefaultAsync(f => f.VentaId == sale.Id);
        Assert.NotNull(facturaDb);
        Assert.Equal(FacturaEstados.Rechazada, facturaDb.Estado);
    }

    [Fact]
    public async Task CancelarFacturaAsync_ShouldRecordTraceabilityAndSubstitution_WhenReasonIs01()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context);

        var factura = new FacturacionFactura
        {
            VentaId = sale.Id,
            IdVenta = sale.IdVenta,
            FolioVenta = sale.NumeroFolio,
            Serie = "F",
            Folio = "100",
            Ambiente = FacturacionAmbientes.QA,
            Estado = FacturaEstados.Timbrada,
            Uuid = Guid.NewGuid().ToString().ToUpper(),
            RfcEmisor = "WPC120101ABC",
            RazonSocialEmisor = "WPC BAJIO",
            RegimenFiscalEmisor = "601",
            CodigoPostalExpedicion = "37000",
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            UsoCfdi = "S01",
            RegimenFiscalReceptor = "616",
            CodigoPostalReceptor = "37000",
            Total = 1160m,
            Subtotal = 1000m,
            IdempotencyKey = Guid.NewGuid().ToString(),
            UsuarioId = user.Id
        };
        context.Invoices.Add(factura);
        await context.SaveChangesAsync();

        var substitutionUuid = Guid.NewGuid().ToString().ToUpper();
        var cancelDto = new SolicitarCancelacionFacturaDto
        {
            MotivoCancelacion = SatCatalogosConstantes.MotivosCancelacion.ComprobanteConErroresConRelacion,
            UuidSustitucion = substitutionUuid,
            MotivoDescripcion = "Error en RFC del receptor"
        };

        // Act
        var result = await service.SolicitarCancelacionAsync(factura.Id, cancelDto, user.Id);

        // Assert
        Assert.True(result.Exitoso);
        var facturaDb = await context.Invoices.FindAsync(factura.Id);
        Assert.NotNull(facturaDb);
        Assert.Equal(FacturaEstados.Cancelada, facturaDb.Estado);
        Assert.Equal(substitutionUuid, facturaDb.UuidSustitucion);
        Assert.Equal(SatCatalogosConstantes.MotivosCancelacion.ComprobanteConErroresConRelacion, facturaDb.MotivoCancelacion);
    }

    [Fact]
    public void FacturacionSettings_Validation_ShouldFail_WhenQaUrlPointsToLiveFiscalapi()
    {
        // Arrange
        var settings = new FacturacionSettings
        {
            Ambiente = FacturacionAmbientes.QA
        };

        var fiscalapiSettings = new Fiscalapi.Common.FiscalapiSettings
        {
            ApiUrl = "https://live.fiscalapi.com", // Mismatched URL for QA!
            ApiKey = "key_123",
            Tenant = "tenant_123"
        };

        // Act
        var (isValid, errorMessage) = settings.ValidateWithFiscalapi(fiscalapiSettings);

        // Assert
        Assert.False(isValid);
        Assert.Contains("live.fiscalapi.com", errorMessage);
    }

    [Fact]
    public void FacturacionSettings_Validation_ShouldFail_WhenPrUrlPointsToTestFiscalapi()
    {
        // Arrange
        var settings = new FacturacionSettings
        {
            Ambiente = FacturacionAmbientes.PR
        };

        var fiscalapiSettings = new Fiscalapi.Common.FiscalapiSettings
        {
            ApiUrl = "https://test.fiscalapi.com", // Mismatched URL for PR!
            ApiKey = "key_123",
            Tenant = "tenant_123"
        };

        // Act
        var (isValid, errorMessage) = settings.ValidateWithFiscalapi(fiscalapiSettings);

        // Assert
        Assert.False(isValid);
        Assert.Contains("test.fiscalapi.com", errorMessage);
    }

    [Fact]
    public async Task ConsultarEstadoSatAsync_ShouldUpdateStatusInDatabase()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context);

        var factura = new FacturacionFactura
        {
            VentaId = sale.Id,
            IdVenta = sale.IdVenta,
            FolioVenta = sale.NumeroFolio,
            Serie = "F",
            Folio = "102",
            Ambiente = FacturacionAmbientes.QA,
            Estado = FacturaEstados.Timbrada,
            Uuid = Guid.NewGuid().ToString().ToUpper(),
            RfcEmisor = "WPC120101ABC",
            RazonSocialEmisor = "WPC BAJIO",
            RegimenFiscalEmisor = "601",
            CodigoPostalExpedicion = "37000",
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            UsoCfdi = "S01",
            RegimenFiscalReceptor = "616",
            CodigoPostalReceptor = "37000",
            Total = 1160m,
            Subtotal = 1000m,
            IdempotencyKey = Guid.NewGuid().ToString(),
            UsuarioId = user.Id
        };
        context.Invoices.Add(factura);
        await context.SaveChangesAsync();

        adapter.OnGetStatus = (req) => Task.FromResult(new ApiResponse<InvoiceStatusResponse>
        {
            Succeeded = true,
            HttpStatusCode = 200,
            Data = new InvoiceStatusResponse
            {
                Status = "Vigente",
                CancelableStatus = "Cancelable con aceptación",
                CancellationStatus = "En proceso"
            }
        });

        // Act
        var res = await service.ConsultarEstadoSatAsync(factura.Id, user.Id);

        // Assert
        Assert.Equal("Vigente", res.Estado);
        Assert.Equal("Cancelable con aceptación", res.EsCancelable);

        var facturaDb = await context.Invoices.FindAsync(factura.Id);
        Assert.NotNull(facturaDb);
        Assert.Equal("Vigente", facturaDb.EstadoSat);
    }

    [Fact]
    public async Task EmitirFacturaVentaAsync_ShouldLogTechnicalAttemptAndDocuments_WhenSuccessful()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context);

        var emitDto = new SolicitarEmisionFacturaDto
        {
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            CodigoPostalReceptor = "37000",
            RegimenFiscalReceptor = "616",
            UsoCfdi = "S01",
            FormaPago = "01",
            MetodoPago = "PUE"
        };

        // Act
        var result = await service.EmitirFacturaVentaAsync(sale.Id, emitDto, user.Id);

        // Assert
        Assert.True(result.Exitoso);
        Assert.NotNull(result.Uuid);

        // 1. Verificar FacturacionLog registrado y finalizado
        var logs = await context.FacturacionLogs.ToListAsync();
        Assert.NotEmpty(logs);
        var emisionLog = logs.FirstOrDefault(l => l.Operacion == "Emitir");
        Assert.NotNull(emisionLog);
        Assert.Equal("Finalizada", emisionLog.EstadoEjecucion);
        Assert.Equal("Exitoso", emisionLog.Resultado);
        Assert.Equal(200, emisionLog.HttpStatusCode);
        Assert.Equal(sale.Id, emisionLog.VentaId);
        Assert.NotNull(emisionLog.FacturaId);
        Assert.NotNull(emisionLog.IntentoId);
        Assert.False(string.IsNullOrWhiteSpace(emisionLog.CorrelationId));
        Assert.False(string.IsNullOrWhiteSpace(emisionLog.IdempotencyKey));
        // No debe contener secretos
        Assert.DoesNotContain("fake_key", emisionLog.RequestSanitizadoJson ?? "");

        // 2. Verificar FacturacionIntento
        var intentos = await context.FacturacionIntentos.ToListAsync();
        Assert.NotEmpty(intentos);
        var intento = intentos.First();
        Assert.Equal(result.FacturaId, intento.FacturaId);
        Assert.Equal(1, intento.NumeroEnvio);
        Assert.Equal("Completado", intento.Estado);
        Assert.Equal("Exitoso", intento.Resultado);

        // 3. Verificar FacturacionDocumento (XML y PDF cacheados)
        var documentos = await context.FacturacionDocumentos.Where(d => d.FacturaId == result.FacturaId).ToListAsync();
        Assert.NotEmpty(documentos);
        Assert.Contains(documentos, d => d.TipoDocumento == "XML" && !string.IsNullOrEmpty(d.Sha256Hash));
        Assert.Contains(documentos, d => d.TipoDocumento == "PDF" && !string.IsNullOrEmpty(d.Sha256Hash));

        // 4. Verificar FacturacionFacturaVenta relación
        var relVentas = await context.FacturacionFacturaVentas.Where(r => r.FacturaId == result.FacturaId).ToListAsync();
        Assert.Single(relVentas);
        Assert.Equal(sale.Id, relVentas[0].VentaId);
    }

    [Fact]
    public async Task EmitirFacturaVentaAsync_ShouldLogTechnicalRejection_WhenProviderRejects()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context);

        adapter.OnCreateInvoice = (req, idem) => Task.FromResult(new ApiResponse<Invoice>
        {
            Succeeded = false,
            HttpStatusCode = 400,
            Message = "El RFC del receptor no es válido ante la lista 69-B del SAT.",
            Details = "Error CFDI40141"
        });

        var emitDto = new SolicitarEmisionFacturaDto
        {
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            CodigoPostalReceptor = "37000",
            RegimenFiscalReceptor = "616",
            UsoCfdi = "S01",
            FormaPago = "01",
            MetodoPago = "PUE"
        };

        // Act
        var result = await service.EmitirFacturaVentaAsync(sale.Id, emitDto, user.Id);

        // Assert
        Assert.False(result.Exitoso);

        // Verificar FacturacionLog registrado como Rechazado
        var log = await context.FacturacionLogs.FirstOrDefaultAsync(l => l.Operacion == "Emitir");
        Assert.NotNull(log);
        Assert.Equal("Finalizada", log.EstadoEjecucion);
        Assert.Equal("Rechazado", log.Resultado);
        Assert.Equal(400, log.HttpStatusCode);
        Assert.Contains("CFDI40141", log.DetalleError ?? "");
    }

    [Fact]
    public async Task EmitirFacturaVentaAsync_ShouldLogFalloTecnico_WithoutSyntheticHttpCodes_WhenExceptionThrown()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);
        var sale = await SeedSaleAsync(context);

        adapter.OnCreateInvoice = (req, idem) => throw new HttpRequestException("Gateway timeout to FiscalAPI test endpoint");

        var emitDto = new SolicitarEmisionFacturaDto
        {
            RfcReceptor = "XAXX010101000",
            RazonSocialReceptor = "PUBLICO EN GENERAL",
            CodigoPostalReceptor = "37000",
            RegimenFiscalReceptor = "616",
            UsoCfdi = "S01",
            FormaPago = "01",
            MetodoPago = "PUE"
        };

        // Act
        var result = await service.EmitirFacturaVentaAsync(sale.Id, emitDto, user.Id);

        // Assert
        Assert.False(result.Exitoso);
        Assert.True(result.RequiereConfirmacionManual);

        // Verificar FacturacionLog marcado como ResultadoIncierto sin código HTTP inventado
        var log = await context.FacturacionLogs.FirstOrDefaultAsync(l => l.Operacion == "Emitir");
        Assert.NotNull(log);
        Assert.Equal("ResultadoIncierto", log.EstadoEjecucion);
        Assert.Equal("ResultadoIncierto", log.Resultado);
        Assert.Null(log.HttpStatusCode); // REGLA: Nullable si no hubo respuesta HTTP real, sin inventar códigos sintéticos
        Assert.Contains("HttpRequestException", log.TipoExcepcion ?? "");
    }

    [Fact]
    public void FacturacionSanitizer_ShouldMaskSecretsAndExcludeLargePayloads()
    {
        // Arrange
        var rawJson = """
        {
            "apiKey": "super_secret_api_key_12345",
            "x-tenant-key": "secret_tenant_999",
            "password": "Password123!",
            "normalField": "ValorPermitido",
            "base64Logo": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=",
            "xmlContenido": "PGNmZGk6Q29tcHJvYmFudGU+ZGF0YTwvY2ZkaTpDb21wcm9iYW50ZT4="
        }
        """;

        // Act
        var sanitized = FacturacionSanitizer.SanitizeJson(rawJson);

        // Assert
        Assert.NotNull(sanitized);
        Assert.DoesNotContain("super_secret_api_key_12345", sanitized);
        Assert.DoesNotContain("secret_tenant_999", sanitized);
        Assert.DoesNotContain("Password123!", sanitized);
        Assert.Contains("***REDACTED***", sanitized);
        Assert.Contains("ValorPermitido", sanitized);
        Assert.Contains("BASE64_DOCUMENT_DATA_EXCLUDED", sanitized);
    }

    [Fact]
    public async Task PurgarBitacoraAntiguaAsync_ShouldDeleteOnlyLogsOlderThanRetention()
    {
        // Arrange
        var (service, context, adapter) = CreateService();
        var user = await SeedIssuerConfigAsync(context);

        // Registro viejo (120 días atrás)
        context.FacturacionLogs.Add(new FacturacionLog
        {
            CorrelationId = "OLD-LOG-1",
            Operacion = "Consultar",
            MetodoHttp = "GET",
            Ruta = "/v1/invoices/1",
            FechaInicioUtc = DateTime.UtcNow.AddDays(-120),
            FechaFinUtc = DateTime.UtcNow.AddDays(-120),
            EstadoEjecucion = "Finalizada",
            Resultado = "Exitoso"
        });

        // Registro reciente (10 días atrás)
        context.FacturacionLogs.Add(new FacturacionLog
        {
            CorrelationId = "RECENT-LOG-1",
            Operacion = "Consultar",
            MetodoHttp = "GET",
            Ruta = "/v1/invoices/2",
            FechaInicioUtc = DateTime.UtcNow.AddDays(-10),
            FechaFinUtc = DateTime.UtcNow.AddDays(-10),
            EstadoEjecucion = "Finalizada",
            Resultado = "Exitoso"
        });

        await context.SaveChangesAsync();

        // Act - Purgar con retención de 90 días
        var eliminados = await service.PurgarBitacoraAntiguaAsync(90);

        // Assert
        Assert.Equal(1, eliminados);
        var restantes = await context.FacturacionLogs.ToListAsync();
        Assert.Single(restantes);
        Assert.Equal("RECENT-LOG-1", restantes[0].CorrelationId);
    }
}
