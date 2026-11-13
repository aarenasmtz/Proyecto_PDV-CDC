using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturacionModuleCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaveProdServSat",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaveUnidadSat",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FiscalapiProductId",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjetoImpuestoSat",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TasaIva",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostalFiscal",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailFacturacion",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiscalapiPersonId",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocialFiscal",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegimenFiscal",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsoCfdi",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacturacionConfiguracion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Empresa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SucursalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sucursal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Rfc = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RegimenFiscal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CodigoPostalExpedicion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FolioSiguiente = table.Column<long>(type: "bigint", nullable: false),
                    FiscalapiIssuerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FiscalapiPersonId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CsdCertificadoNumero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstaListoParaEmitir = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UsuarioModificoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionConfiguracion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionFactura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    FolioVenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstadoSat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstadoCancelacionSat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsCancelableSat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FiscalapiInvoiceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Uuid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaEmisionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaTimbradoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCancelacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RfcEmisor = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RazonSocialEmisor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RegimenFiscalEmisor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CodigoPostalExpedicion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RfcReceptor = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RazonSocialReceptor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CodigoPostalReceptor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RegimenFiscalReceptor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UsoCfdi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EmailReceptor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TipoComprobante = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Exportacion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CondicionesDePago = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalImpuestosTrasladados = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalImpuestosRetenidos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TraceIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MensajeRespuesta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetallesRespuesta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoEstadoHttp = table.Column<int>(type: "int", nullable: true),
                    XmlContenido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PdfRuta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UuidSustitucion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaSolicitudCancelacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcuseCancelacionXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionFactura_Sales_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FacturacionFactura_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IntentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmpresaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SucursalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Operacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetodoHttp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FechaInicioUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DuracionMs = table.Column<long>(type: "bigint", nullable: true),
                    EstadoEjecucion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ProviderHttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    Succeeded = table.Column<bool>(type: "bit", nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetalleError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceIdentifierProveedor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IdentificadorExterno = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UuidFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NumeroReintento = table.Column<int>(type: "int", nullable: false),
                    RequestSanitizadoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseSanitizadoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoExcepcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StackTraceSanitizado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionAplicacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionReferenciaExterna",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoEntidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntidadLocalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IdentificadorExterno = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tenant = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DetallesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionReferenciaExterna", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionCancelacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    UuidSustitucion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaSolicitudUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEfectivaCancelacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstadoSat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsCancelableSat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstatusCancelacionSat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcuseXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Exitoso = table.Column<bool>(type: "bit", nullable: false),
                    CodigoEstadoHttp = table.Column<int>(type: "int", nullable: true),
                    MensajeRespuesta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetallesRespuesta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionCancelacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionCancelacion_FacturacionFactura_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256Hash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RutaAlmacenamientoPrivada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContenidoBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionDocumento_FacturacionFactura_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionFacturaDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPartida = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClaveProdServ = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClaveUnidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ObjetoImp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BaseIva = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TasaOcuotaIva = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ImporteIva = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FacturacionFacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionFacturaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionFacturaDetalle_FacturacionFactura_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturacionFacturaDetalle_FacturacionFactura_FacturacionFacturaId",
                        column: x => x.FacturacionFacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FacturacionFacturaDetalle_Products_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionFacturaVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FolioVenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdVenta = table.Column<int>(type: "int", nullable: true),
                    MontoAsociado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaAsociacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionFacturaVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionFacturaVenta_FacturacionFactura_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FacturacionFacturaVenta_Sales_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionHistorialEstado",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionHistorialEstado", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionHistorialEstado_FacturacionFactura_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionIntento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ambiente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NumeroRevision = table.Column<int>(type: "int", nullable: false),
                    NumeroEnvio = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    RequestSanitizadoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseSanitizadoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DetalleError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaInicioUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionIntento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionIntento_FacturacionFactura_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturacionFactura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FacturacionFacturaImpuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacturaDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Impuesto = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TipoFactor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Base = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TasaOCuota = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturacionFacturaImpuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturacionFacturaImpuesto_FacturacionFacturaDetalle_FacturaDetalleId",
                        column: x => x.FacturaDetalleId,
                        principalTable: "FacturacionFacturaDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionCancelacion_CorrelationId",
                table: "FacturacionCancelacion",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionCancelacion_FacturaId",
                table: "FacturacionCancelacion",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionConfiguracion_EmpresaId_SucursalId_Ambiente",
                table: "FacturacionConfiguracion",
                columns: new[] { "EmpresaId", "SucursalId", "Ambiente" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionConfiguracion_Rfc_Ambiente",
                table: "FacturacionConfiguracion",
                columns: new[] { "Rfc", "Ambiente" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionDocumento_FacturaId_TipoDocumento",
                table: "FacturacionDocumento",
                columns: new[] { "FacturaId", "TipoDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionDocumento_Sha256Hash",
                table: "FacturacionDocumento",
                column: "Sha256Hash");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_Ambiente_FechaEmisionUtc",
                table: "FacturacionFactura",
                columns: new[] { "Ambiente", "FechaEmisionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_Estado",
                table: "FacturacionFactura",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_IdempotencyKey",
                table: "FacturacionFactura",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_RfcReceptor_FechaEmisionUtc",
                table: "FacturacionFactura",
                columns: new[] { "RfcReceptor", "FechaEmisionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_UsuarioId",
                table: "FacturacionFactura",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_Uuid",
                table: "FacturacionFactura",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFactura_VentaId",
                table: "FacturacionFactura",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFacturaDetalle_FacturacionFacturaId",
                table: "FacturacionFacturaDetalle",
                column: "FacturacionFacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFacturaDetalle_FacturaId_NumeroPartida",
                table: "FacturacionFacturaDetalle",
                columns: new[] { "FacturaId", "NumeroPartida" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFacturaDetalle_ProductoId",
                table: "FacturacionFacturaDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFacturaImpuesto_FacturaDetalleId_Tipo_Impuesto",
                table: "FacturacionFacturaImpuesto",
                columns: new[] { "FacturaDetalleId", "Tipo", "Impuesto" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFacturaVenta_FacturaId_VentaId",
                table: "FacturacionFacturaVenta",
                columns: new[] { "FacturaId", "VentaId" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionFacturaVenta_VentaId",
                table: "FacturacionFacturaVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionHistorialEstado_FacturaId_FechaCreacionUtc",
                table: "FacturacionHistorialEstado",
                columns: new[] { "FacturaId", "FechaCreacionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionIntento_CorrelationId",
                table: "FacturacionIntento",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionIntento_FacturaId_FechaInicioUtc",
                table: "FacturacionIntento",
                columns: new[] { "FacturaId", "FechaInicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionIntento_IdempotencyKey",
                table: "FacturacionIntento",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionIntento_VentaId_FechaInicioUtc",
                table: "FacturacionIntento",
                columns: new[] { "VentaId", "FechaInicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_Ambiente_FechaInicioUtc",
                table: "FacturacionLog",
                columns: new[] { "Ambiente", "FechaInicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_CorrelationId",
                table: "FacturacionLog",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_FacturaId_FechaInicioUtc",
                table: "FacturacionLog",
                columns: new[] { "FacturaId", "FechaInicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_IdempotencyKey",
                table: "FacturacionLog",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_IntentoId",
                table: "FacturacionLog",
                column: "IntentoId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_Resultado_FechaInicioUtc",
                table: "FacturacionLog",
                columns: new[] { "Resultado", "FechaInicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_TraceIdentifierProveedor",
                table: "FacturacionLog",
                column: "TraceIdentifierProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_UuidFiscal",
                table: "FacturacionLog",
                column: "UuidFiscal");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionLog_VentaId",
                table: "FacturacionLog",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionReferenciaExterna_IdentificadorExterno_Ambiente",
                table: "FacturacionReferenciaExterna",
                columns: new[] { "IdentificadorExterno", "Ambiente" });

            migrationBuilder.CreateIndex(
                name: "IX_FacturacionReferenciaExterna_TipoEntidad_EntidadLocalId_Ambiente",
                table: "FacturacionReferenciaExterna",
                columns: new[] { "TipoEntidad", "EntidadLocalId", "Ambiente" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturacionCancelacion");

            migrationBuilder.DropTable(
                name: "FacturacionConfiguracion");

            migrationBuilder.DropTable(
                name: "FacturacionDocumento");

            migrationBuilder.DropTable(
                name: "FacturacionFacturaImpuesto");

            migrationBuilder.DropTable(
                name: "FacturacionFacturaVenta");

            migrationBuilder.DropTable(
                name: "FacturacionHistorialEstado");

            migrationBuilder.DropTable(
                name: "FacturacionIntento");

            migrationBuilder.DropTable(
                name: "FacturacionLog");

            migrationBuilder.DropTable(
                name: "FacturacionReferenciaExterna");

            migrationBuilder.DropTable(
                name: "FacturacionFacturaDetalle");

            migrationBuilder.DropTable(
                name: "FacturacionFactura");

            migrationBuilder.DropColumn(
                name: "ClaveProdServSat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ClaveUnidadSat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FiscalapiProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ObjetoImpuestoSat",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TasaIva",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CodigoPostalFiscal",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "EmailFacturacion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FiscalapiPersonId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RazonSocialFiscal",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RegimenFiscal",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UsoCfdi",
                table: "Customers");
        }
    }
}
