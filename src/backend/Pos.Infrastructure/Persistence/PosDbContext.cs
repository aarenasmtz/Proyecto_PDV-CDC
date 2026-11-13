using Microsoft.EntityFrameworkCore;
using Pos.Domain.Entidades;

namespace Pos.Infrastructure.Persistence;

public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options) { }

    public DbSet<Producto> Products { get; set; } = null!;
    public DbSet<Categoria> Categories { get; set; } = null!;
    public DbSet<ImagenProducto> ProductImages { get; set; } = null!;
    public DbSet<Cliente> Customers { get; set; } = null!;
    public DbSet<Usuario> Users { get; set; } = null!;
    public DbSet<Empleado> Employees { get; set; } = null!;
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<Permiso> Permissions { get; set; } = null!;
    public DbSet<UsuarioRol> UserRoles { get; set; } = null!;
    public DbSet<RolPermiso> RolePermissions { get; set; } = null!;
    public DbSet<TokenRefresco> RefreshTokens { get; set; } = null!;
    public DbSet<LogAuditoria> AuditLogs { get; set; } = null!;
    public DbSet<Existencia> Stocks { get; set; } = null!;
    public DbSet<MovimientoInventario> InventoryMovements { get; set; } = null!;
    public DbSet<Venta> Sales { get; set; } = null!;
    public DbSet<PartidaVenta> SaleItems { get; set; } = null!;
    public DbSet<Cotizacion> Quotes { get; set; } = null!;
    public DbSet<PartidaCotizacion> QuoteItems { get; set; } = null!;
    public DbSet<AbonoPago> PaymentInstallments { get; set; } = null!;
    public DbSet<DevolucionCabecera> ReturnHeaders { get; set; } = null!;
    public DbSet<DevolucionDetalle> ReturnItems { get; set; } = null!;
    public DbSet<PlantillaDocumento> DocumentTemplates { get; set; } = null!;
    public DbSet<TurnoCaja> CashShifts { get; set; } = null!;
    public DbSet<TransaccionCaja> CashTransactions { get; set; } = null!;
    public DbSet<SaludSistema> HealthStatuses { get; set; } = null!;
    public DbSet<ConfiguracionSistema> SystemSettings { get; set; } = null!;
    public DbSet<ReciboCompra> PurchaseReceipts { get; set; } = null!;

    // Módulo Oficial de Facturación Electrónica CFDI 4.0 (11 Tablas)
    public DbSet<FacturacionConfiguracion> FacturacionConfiguraciones { get; set; } = null!;
    public DbSet<FacturacionReferenciaExterna> FacturacionReferenciasExternas { get; set; } = null!;
    public DbSet<FacturacionFactura> FacturacionFacturas { get; set; } = null!;
    public DbSet<FacturacionFacturaDetalle> FacturacionFacturaDetalles { get; set; } = null!;
    public DbSet<FacturacionFacturaImpuesto> FacturacionFacturaImpuestos { get; set; } = null!;
    public DbSet<FacturacionFacturaVenta> FacturacionFacturaVentas { get; set; } = null!;
    public DbSet<FacturacionIntento> FacturacionIntentos { get; set; } = null!;
    public DbSet<FacturacionDocumento> FacturacionDocumentos { get; set; } = null!;
    public DbSet<FacturacionCancelacion> FacturacionCancelaciones { get; set; } = null!;
    public DbSet<FacturacionHistorialEstado> FacturacionHistorialEstados { get; set; } = null!;
    public DbSet<FacturacionLog> FacturacionLogs { get; set; } = null!;

    // Aliases convenientes para compatibilidad
    public DbSet<FacturacionConfiguracion> IssuerConfigs => FacturacionConfiguraciones;
    public DbSet<FacturacionFactura> Invoices => FacturacionFacturas;
    public DbSet<FacturacionFacturaDetalle> InvoiceItems => FacturacionFacturaDetalles;
    public DbSet<FacturacionHistorialEstado> InvoiceStatusHistories => FacturacionHistorialEstados;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many UserRoles
        modelBuilder.Entity<UsuarioRol>()
            .HasKey(ur => new { ur.UsuarioId, ur.RolId });

        modelBuilder.Entity<UsuarioRol>()
            .HasOne(ur => ur.Usuario)
            .WithMany(u => u.UsuarioRoles)
            .HasForeignKey(ur => ur.UsuarioId);

        modelBuilder.Entity<UsuarioRol>()
            .HasOne(ur => ur.Rol)
            .WithMany(r => r.UsuarioRoles)
            .HasForeignKey(ur => ur.RolId);

        // Many-to-Many RolePermissions
        modelBuilder.Entity<RolPermiso>()
            .HasKey(rp => new { rp.RolId, rp.PermisoId });

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(rp => rp.RolId);

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(rp => rp.PermisoId);

        modelBuilder.Entity<Venta>()
            .Property(sale => sale.NumeroFolio)
            .HasMaxLength(64);
        modelBuilder.Entity<Venta>()
            .HasIndex(sale => sale.NumeroFolio)
            .IsUnique();

        modelBuilder.Entity<TurnoCaja>()
            .Property(shift => shift.NumeroTurno)
            .HasMaxLength(64);
        modelBuilder.Entity<TurnoCaja>()
            .HasIndex(shift => shift.NumeroTurno)
            .IsUnique();

        modelBuilder.Entity<Cotizacion>()
            .Property(quote => quote.NumeroCotizacion)
            .HasMaxLength(64);
        modelBuilder.Entity<Cotizacion>()
            .HasIndex(quote => quote.NumeroCotizacion)
            .IsUnique();

        modelBuilder.Entity<AbonoPago>()
            .Property(payment => payment.NumeroRecibo)
            .HasMaxLength(64);
        modelBuilder.Entity<AbonoPago>()
            .HasIndex(payment => payment.NumeroRecibo);

        modelBuilder.Entity<DevolucionCabecera>()
            .Property(returnHeader => returnHeader.NumeroDevolucion)
            .HasMaxLength(64);
        modelBuilder.Entity<DevolucionCabecera>()
            .Property(returnHeader => returnHeader.FormaReembolso)
            .HasMaxLength(32);
        modelBuilder.Entity<DevolucionCabecera>()
            .HasIndex(returnHeader => returnHeader.NumeroDevolucion)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .Property(customer => customer.Email)
            .HasMaxLength(256);
        modelBuilder.Entity<Cliente>()
            .Property(customer => customer.Rfc)
            .HasMaxLength(13);
        modelBuilder.Entity<Cliente>()
            .HasIndex(customer => customer.Email)
            .IsUnique();
        modelBuilder.Entity<Cliente>()
            .HasIndex(customer => customer.Rfc)
            .IsUnique()
            .HasFilter("[Rfc] IS NOT NULL AND [Rfc] <> N''");

        modelBuilder.Entity<Producto>()
            .Property(product => product.Sku)
            .HasMaxLength(64);
        modelBuilder.Entity<Producto>()
            .Property(product => product.Barcode)
            .HasMaxLength(64);
        modelBuilder.Entity<Producto>()
            .Property(product => product.Nombre)
            .HasMaxLength(200);
        modelBuilder.Entity<Producto>()
            .Property(product => product.UnidadMedida)
            .HasMaxLength(16);
        modelBuilder.Entity<Producto>()
            .Property(product => product.Material)
            .HasMaxLength(100);
        modelBuilder.Entity<Producto>()
            .HasIndex(product => product.Sku)
            .IsUnique();
        modelBuilder.Entity<Producto>()
            .HasIndex(product => product.Barcode)
            .IsUnique();
        modelBuilder.Entity<Producto>()
            .Property(product => product.IdProducto)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Producto>()
            .HasIndex(product => product.IdProducto)
            .IsUnique();

        modelBuilder.Entity<Categoria>()
            .Property(category => category.Nombre)
            .HasMaxLength(120);
        modelBuilder.Entity<Categoria>()
            .Property(category => category.Slug)
            .HasMaxLength(140);
        modelBuilder.Entity<Categoria>()
            .Property(category => category.ImagenUrl)
            .HasMaxLength(500);
        modelBuilder.Entity<Categoria>()
            .HasIndex(category => category.Slug)
            .IsUnique();

        modelBuilder.Entity<Existencia>()
            .Property(stock => stock.Ubicacion)
            .HasMaxLength(200);
        modelBuilder.Entity<Existencia>()
            .HasIndex(stock => stock.ProductoId)
            .IsUnique();

        modelBuilder.Entity<LogAuditoria>()
            .Property(log => log.IdCorrelacion)
            .HasMaxLength(100);
        modelBuilder.Entity<LogAuditoria>()
            .Property(log => log.Accion)
            .HasMaxLength(100);
        modelBuilder.Entity<LogAuditoria>()
            .HasIndex(log => log.IdCorrelacion);
        modelBuilder.Entity<LogAuditoria>()
            .HasIndex(log => log.Accion);
        modelBuilder.Entity<LogAuditoria>()
            .HasIndex(log => log.FechaCreacionUtc);

        modelBuilder.Entity<Venta>()
            .Property(v => v.IdVenta)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Venta>()
            .HasIndex(v => v.IdVenta)
            .IsUnique();

        modelBuilder.Entity<PartidaVenta>()
            .HasIndex(pv => pv.IdVenta);
        modelBuilder.Entity<AbonoPago>()
            .HasIndex(ap => ap.IdVenta);
        modelBuilder.Entity<DevolucionCabecera>()
            .HasIndex(dc => dc.IdVenta);
        modelBuilder.Entity<MovimientoInventario>()
            .HasIndex(mi => mi.IdVenta);
        modelBuilder.Entity<TransaccionCaja>()
            .HasIndex(tc => tc.IdVenta);

        modelBuilder.Entity<ConfiguracionSistema>()
            .Property(cs => cs.Clave)
            .HasMaxLength(100);
        modelBuilder.Entity<ConfiguracionSistema>()
            .HasIndex(cs => cs.Clave)
            .IsUnique();

        modelBuilder.Entity<ReciboCompra>()
            .Property(r => r.Folio)
            .HasMaxLength(50);
        modelBuilder.Entity<ReciboCompra>()
            .HasIndex(r => r.Folio)
            .IsUnique();
        modelBuilder.Entity<ReciboCompra>()
            .Property(r => r.NombreProducto)
            .HasMaxLength(250);
        modelBuilder.Entity<ReciboCompra>()
            .Property(r => r.Sku)
            .HasMaxLength(100);
        modelBuilder.Entity<ReciboCompra>()
            .Property(r => r.CodigoBarras)
            .HasMaxLength(100);
        modelBuilder.Entity<ReciboCompra>()
            .Property(r => r.Notas)
            .HasMaxLength(500);
        modelBuilder.Entity<ReciboCompra>()
            .HasOne(r => r.Producto)
            .WithMany()
            .HasForeignKey(r => r.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ReciboCompra>()
            .HasOne(r => r.Usuario)
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configuración de Facturación Electrónica CFDI 4.0 (11 Tablas Oficiales)
        modelBuilder.Entity<FacturacionConfiguracion>(entity =>
        {
            entity.ToTable("FacturacionConfiguracion");
            entity.Property(c => c.EmpresaId).HasMaxLength(50);
            entity.Property(c => c.Empresa).HasMaxLength(150);
            entity.Property(c => c.SucursalId).HasMaxLength(50);
            entity.Property(c => c.Sucursal).HasMaxLength(150);
            entity.Property(c => c.Ambiente).HasMaxLength(10);
            entity.Property(c => c.Rfc).HasMaxLength(13);
            entity.Property(c => c.RazonSocial).HasMaxLength(300);
            entity.Property(c => c.RegimenFiscal).HasMaxLength(10);
            entity.Property(c => c.CodigoPostalExpedicion).HasMaxLength(10);
            entity.Property(c => c.Serie).HasMaxLength(20);
            entity.Property(c => c.FiscalapiIssuerId).HasMaxLength(100);
            entity.Property(c => c.CsdCertificadoNumero).HasMaxLength(50);
            entity.Property(c => c.Notas).HasMaxLength(500);

            entity.HasIndex(c => new { c.EmpresaId, c.SucursalId, c.Ambiente });
            entity.HasIndex(c => new { c.Rfc, c.Ambiente });
        });

        modelBuilder.Entity<FacturacionReferenciaExterna>(entity =>
        {
            entity.ToTable("FacturacionReferenciaExterna");
            entity.Property(r => r.TipoEntidad).HasMaxLength(50);
            entity.Property(r => r.Ambiente).HasMaxLength(10);
            entity.Property(r => r.IdentificadorExterno).HasMaxLength(150);
            entity.Property(r => r.Tenant).HasMaxLength(150);

            entity.HasIndex(r => new { r.TipoEntidad, r.EntidadLocalId, r.Ambiente }).IsUnique();
            entity.HasIndex(r => new { r.IdentificadorExterno, r.Ambiente });
        });

        modelBuilder.Entity<FacturacionFactura>(entity =>
        {
            entity.ToTable("FacturacionFactura");
            entity.Property(f => f.Ambiente).HasMaxLength(10);
            entity.Property(f => f.Serie).HasMaxLength(20);
            entity.Property(f => f.Folio).HasMaxLength(50);
            entity.Property(f => f.FolioVenta).HasMaxLength(50);
            entity.Property(f => f.Estado).HasMaxLength(50);
            entity.Property(f => f.EstadoSat).HasMaxLength(50);
            entity.Property(f => f.EstadoCancelacionSat).HasMaxLength(100);
            entity.Property(f => f.EsCancelableSat).HasMaxLength(50);
            entity.Property(f => f.FiscalapiInvoiceId).HasMaxLength(100);
            entity.Property(f => f.Uuid).HasMaxLength(50);
            entity.Property(f => f.RfcEmisor).HasMaxLength(13);
            entity.Property(f => f.RazonSocialEmisor).HasMaxLength(300);
            entity.Property(f => f.RegimenFiscalEmisor).HasMaxLength(10);
            entity.Property(f => f.CodigoPostalExpedicion).HasMaxLength(10);
            entity.Property(f => f.RfcReceptor).HasMaxLength(13);
            entity.Property(f => f.RazonSocialReceptor).HasMaxLength(300);
            entity.Property(f => f.CodigoPostalReceptor).HasMaxLength(10);
            entity.Property(f => f.RegimenFiscalReceptor).HasMaxLength(10);
            entity.Property(f => f.UsoCfdi).HasMaxLength(10);
            entity.Property(f => f.EmailReceptor).HasMaxLength(200);
            entity.Property(f => f.Moneda).HasMaxLength(10);
            entity.Property(f => f.FormaPago).HasMaxLength(10);
            entity.Property(f => f.MetodoPago).HasMaxLength(10);
            entity.Property(f => f.TipoComprobante).HasMaxLength(5);
            entity.Property(f => f.Exportacion).HasMaxLength(10);
            entity.Property(f => f.CondicionesDePago).HasMaxLength(200);
            entity.Property(f => f.IdempotencyKey).HasMaxLength(150);
            entity.Property(f => f.TraceIdentifier).HasMaxLength(150);
            entity.Property(f => f.MensajeRespuesta).HasMaxLength(1000);

            entity.HasIndex(f => f.Uuid);
            entity.HasIndex(f => f.IdempotencyKey);
            entity.HasIndex(f => f.Estado);
            entity.HasIndex(f => new { f.Ambiente, f.FechaEmisionUtc });
            entity.HasIndex(f => new { f.RfcReceptor, f.FechaEmisionUtc });

            entity.HasOne(f => f.Venta)
                .WithMany()
                .HasForeignKey(f => f.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FacturacionFacturaDetalle>(entity =>
        {
            entity.ToTable("FacturacionFacturaDetalle");
            entity.Property(p => p.ClaveProdServ).HasMaxLength(20);
            entity.Property(p => p.ClaveUnidad).HasMaxLength(20);
            entity.Property(p => p.Unidad).HasMaxLength(50);
            entity.Property(p => p.Descripcion).HasMaxLength(1000);
            entity.Property(p => p.Sku).HasMaxLength(100);
            entity.Property(p => p.ObjetoImp).HasMaxLength(10);

            entity.HasIndex(p => new { p.FacturaId, p.NumeroPartida });

            entity.HasOne(p => p.Factura)
                .WithMany(f => f.Detalles)
                .HasForeignKey(p => p.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Producto)
                .WithMany()
                .HasForeignKey(p => p.ProductoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FacturacionFacturaImpuesto>(entity =>
        {
            entity.ToTable("FacturacionFacturaImpuesto");
            entity.Property(i => i.Tipo).HasMaxLength(20);
            entity.Property(i => i.Impuesto).HasMaxLength(10);
            entity.Property(i => i.TipoFactor).HasMaxLength(20);

            entity.HasIndex(i => new { i.FacturaDetalleId, i.Tipo, i.Impuesto });

            entity.HasOne(i => i.FacturaDetalle)
                .WithMany(d => d.Impuestos)
                .HasForeignKey(i => i.FacturaDetalleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacturacionFacturaVenta>(entity =>
        {
            entity.ToTable("FacturacionFacturaVenta");
            entity.Property(v => v.FolioVenta).HasMaxLength(50);

            entity.HasIndex(v => new { v.FacturaId, v.VentaId });
            entity.HasIndex(v => v.VentaId);

            entity.HasOne(v => v.Factura)
                .WithMany(f => f.VentasAsociadas)
                .HasForeignKey(v => v.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Venta)
                .WithMany()
                .HasForeignKey(v => v.VentaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FacturacionIntento>(entity =>
        {
            entity.ToTable("FacturacionIntento");
            entity.Property(i => i.CorrelationId).HasMaxLength(100);
            entity.Property(i => i.Ambiente).HasMaxLength(10);
            entity.Property(i => i.IdempotencyKey).HasMaxLength(150);
            entity.Property(i => i.Estado).HasMaxLength(50);
            entity.Property(i => i.Resultado).HasMaxLength(50);
            entity.Property(i => i.Mensaje).HasMaxLength(1000);

            entity.HasIndex(i => i.IdempotencyKey);
            entity.HasIndex(i => i.CorrelationId);
            entity.HasIndex(i => new { i.FacturaId, i.FechaInicioUtc });
            entity.HasIndex(i => new { i.VentaId, i.FechaInicioUtc });

            entity.HasOne(i => i.Factura)
                .WithMany(f => f.Intentos)
                .HasForeignKey(i => i.FacturaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FacturacionDocumento>(entity =>
        {
            entity.ToTable("FacturacionDocumento");
            entity.Property(d => d.TipoDocumento).HasMaxLength(30);
            entity.Property(d => d.NombreArchivo).HasMaxLength(255);
            entity.Property(d => d.MimeType).HasMaxLength(100);
            entity.Property(d => d.Sha256Hash).HasMaxLength(64);
            entity.Property(d => d.RutaAlmacenamientoPrivada).HasMaxLength(500);

            entity.HasIndex(d => new { d.FacturaId, d.TipoDocumento });
            entity.HasIndex(d => d.Sha256Hash);

            entity.HasOne(d => d.Factura)
                .WithMany(f => f.Documentos)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacturacionCancelacion>(entity =>
        {
            entity.ToTable("FacturacionCancelacion");
            entity.Property(c => c.CorrelationId).HasMaxLength(100);
            entity.Property(c => c.MotivoCancelacion).HasMaxLength(5);
            entity.Property(c => c.UuidSustitucion).HasMaxLength(50);
            entity.Property(c => c.EstadoSat).HasMaxLength(100);
            entity.Property(c => c.EsCancelableSat).HasMaxLength(100);
            entity.Property(c => c.EstatusCancelacionSat).HasMaxLength(100);
            entity.Property(c => c.MensajeRespuesta).HasMaxLength(1000);

            entity.HasIndex(c => c.FacturaId);
            entity.HasIndex(c => c.CorrelationId);

            entity.HasOne(c => c.Factura)
                .WithMany(f => f.Cancelaciones)
                .HasForeignKey(c => c.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacturacionHistorialEstado>(entity =>
        {
            entity.ToTable("FacturacionHistorialEstado");
            entity.Property(h => h.EstadoAnterior).HasMaxLength(50);
            entity.Property(h => h.EstadoNuevo).HasMaxLength(50);
            entity.Property(h => h.Detalle).HasMaxLength(1000);

            entity.HasIndex(h => new { h.FacturaId, h.FechaCreacionUtc });

            entity.HasOne(h => h.Factura)
                .WithMany(f => f.HistorialEstados)
                .HasForeignKey(h => h.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FacturacionLog>(entity =>
        {
            entity.ToTable("FacturacionLog");
            entity.Property(l => l.CorrelationId).HasMaxLength(100);
            entity.Property(l => l.EmpresaId).HasMaxLength(50);
            entity.Property(l => l.SucursalId).HasMaxLength(50);
            entity.Property(l => l.Origen).HasMaxLength(50);
            entity.Property(l => l.Ambiente).HasMaxLength(10);
            entity.Property(l => l.Proveedor).HasMaxLength(50);
            entity.Property(l => l.Operacion).HasMaxLength(100);
            entity.Property(l => l.MetodoHttp).HasMaxLength(10);
            entity.Property(l => l.Ruta).HasMaxLength(255);
            entity.Property(l => l.EstadoEjecucion).HasMaxLength(50);
            entity.Property(l => l.Resultado).HasMaxLength(50);
            entity.Property(l => l.Mensaje).HasMaxLength(1000);
            entity.Property(l => l.TraceIdentifierProveedor).HasMaxLength(150);
            entity.Property(l => l.IdentificadorExterno).HasMaxLength(150);
            entity.Property(l => l.UuidFiscal).HasMaxLength(50);
            entity.Property(l => l.IdempotencyKey).HasMaxLength(150);
            entity.Property(l => l.TipoExcepcion).HasMaxLength(200);
            entity.Property(l => l.VersionAplicacion).HasMaxLength(50);

            entity.HasIndex(l => l.CorrelationId);
            entity.HasIndex(l => new { l.FacturaId, l.FechaInicioUtc });
            entity.HasIndex(l => l.IntentoId);
            entity.HasIndex(l => l.VentaId);
            entity.HasIndex(l => new { l.Ambiente, l.FechaInicioUtc });
            entity.HasIndex(l => new { l.Resultado, l.FechaInicioUtc });
            entity.HasIndex(l => l.TraceIdentifierProveedor);
            entity.HasIndex(l => l.UuidFiscal);
            entity.HasIndex(l => l.IdempotencyKey);
        });

        // Precision mapping for money fields (decimal(18,2))
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in properties)
            {
                if (property.DeclaringType.ClrType == typeof(FacturacionFacturaDetalle) &&
                    (property.Name == nameof(FacturacionFacturaDetalle.Cantidad) ||
                     property.Name == nameof(FacturacionFacturaDetalle.ValorUnitario) ||
                     property.Name == nameof(FacturacionFacturaDetalle.BaseIva) ||
                     property.Name == nameof(FacturacionFacturaDetalle.ImporteIva)))
                {
                    property.SetPrecision(18);
                    property.SetScale(4);
                }
                else if (property.DeclaringType.ClrType == typeof(FacturacionFacturaDetalle) &&
                         property.Name == nameof(FacturacionFacturaDetalle.TasaOcuotaIva))
                {
                    property.SetPrecision(18);
                    property.SetScale(6);
                }
                else if (property.DeclaringType.ClrType == typeof(FacturacionFacturaImpuesto) &&
                         (property.Name == nameof(FacturacionFacturaImpuesto.Base) ||
                          property.Name == nameof(FacturacionFacturaImpuesto.Importe)))
                {
                    property.SetPrecision(18);
                    property.SetScale(4);
                }
                else if (property.DeclaringType.ClrType == typeof(FacturacionFacturaImpuesto) &&
                         property.Name == nameof(FacturacionFacturaImpuesto.TasaOCuota))
                {
                    property.SetPrecision(18);
                    property.SetScale(6);
                }
                else if (property.DeclaringType.ClrType == typeof(FacturacionFactura) &&
                         property.Name == nameof(FacturacionFactura.TipoCambio))
                {
                    property.SetPrecision(18);
                    property.SetScale(4);
                }
                else
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SimulateIdentityColumnsForInMemory();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SimulateIdentityColumnsForInMemory();
        return base.SaveChanges();
    }

    private void SimulateIdentityColumnsForInMemory()
    {
        if (Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
        {
            var newProducts = ChangeTracker.Entries<Producto>()
                .Where(e => e.State == EntityState.Added && e.Entity.IdProducto <= 0)
                .Select(e => e.Entity)
                .ToList();

            if (newProducts.Count > 0)
            {
                var maxId = Products.Any() ? Products.Max(p => p.IdProducto) : 0;
                foreach (var product in newProducts)
                {
                    maxId++;
                    product.IdProducto = maxId;
                }
            }

            var newSales = ChangeTracker.Entries<Venta>()
                .Where(e => e.State == EntityState.Added && e.Entity.IdVenta <= 0)
                .Select(e => e.Entity)
                .ToList();

            if (newSales.Count > 0)
            {
                var maxSaleId = Sales.Any() ? Sales.Max(s => s.IdVenta) : 0;
                foreach (var sale in newSales)
                {
                    maxSaleId++;
                    sale.IdVenta = maxSaleId;
                }
            }
        }
    }
}
