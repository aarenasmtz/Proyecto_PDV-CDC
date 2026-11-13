using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Auth.Services;
using Pos.Application.Catalog.Services;
using Pos.Application.CashShift.Services;
using Pos.Application.Commercial.NotasVenta.Services;
using Pos.Application.Commercial.Services;
using Pos.Application.Common.Interfaces;
using Pos.Application.Inventory.Services;
using Pos.Application.Reporting.Services;
using Pos.Application.Sales.Services;
using Pos.Application.Services;
using Pos.Application.Users.Services;
using Pos.Application.Invoicing.Services;
using Pos.Infrastructure.Identity;
using Pos.Infrastructure.Invoicing.Clients;
using Pos.Infrastructure.Invoicing.Options;
using Pos.Infrastructure.Invoicing.Services;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Services;
using Fiscalapi.Abstractions;
using Fiscalapi.Common;
using Fiscalapi.Services;
using Microsoft.Extensions.Options;

namespace Pos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PosDbContext>(options =>
        {
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("InMemory"))
            {
                options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
            }
            else
            {
                options.UseInMemoryDatabase("PosLambrinInMemoryDb");
            }
        });

        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthApplicationService, AuthApplicationService>();
        services.AddScoped<ICatalogApplicationService, CatalogApplicationService>();
        services.AddScoped<IInventoryApplicationService, InventoryApplicationService>();
        services.AddScoped<IPurchaseReceiptApplicationService, PurchaseReceiptApplicationService>();
        services.AddScoped<ISaleApplicationService, SaleApplicationService>();
        services.AddScoped<ICommercialOperationsService, CommercialOperationsService>();
        services.AddScoped<ICashShiftApplicationService, CashShiftApplicationService>();
        services.AddScoped<IReportingApplicationService, ReportingApplicationService>();
        services.AddScoped<IUserApplicationService, UserApplicationService>();
        services.AddScoped<IRoleApplicationService, RoleApplicationService>();
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        services.AddScoped<IProductImageStorageService, LocalProductImageStorageService>();
        services.AddScoped<ICategoryImageStorageService, LocalCategoryImageStorageService>();
        services.AddScoped<ICatalogImageStorageService, CatalogImageStorageService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<INotaVentaPdfService, NotaVentaPdfService>();
        services.AddScoped<INotaVentaService, NotaVentaApplicationService>();

        // Facturación Electrónica FiscalAPI
        services.Configure<FiscalapiSettings>(configuration.GetSection("FiscalapiSettings"));
        services.Configure<FacturacionSettings>(configuration.GetSection(FacturacionSettings.SectionName));

        var fiscalapiSettings = configuration.GetSection("FiscalapiSettings").Get<FiscalapiSettings>() ?? new FiscalapiSettings();
        var facturacionSettings = configuration.GetSection(FacturacionSettings.SectionName).Get<FacturacionSettings>() ?? new FacturacionSettings();

        // Validación de consistencia QA / PR al inicio
        var (isValid, errorMessage) = facturacionSettings.ValidateWithFiscalapi(fiscalapiSettings);
        if (!isValid && errorMessage != null && errorMessage.StartsWith("Inconsistencia", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(errorMessage);
        }

        services.AddSingleton<IFiscalApiClient>(sp =>
        {
            var fSettings = sp.GetRequiredService<IOptions<FiscalapiSettings>>().Value;
            var facSettings = sp.GetRequiredService<IOptions<FacturacionSettings>>().Value;
            if (string.IsNullOrWhiteSpace(fSettings.ApiUrl))
            {
                fSettings.ApiUrl = string.Equals(facSettings.Ambiente, "PR", StringComparison.OrdinalIgnoreCase)
                    ? "https://live.fiscalapi.com"
                    : "https://test.fiscalapi.com";
            }
            if (string.IsNullOrWhiteSpace(fSettings.ApiKey))
            {
                fSettings.ApiKey = "NOT_CONFIGURED";
            }
            if (string.IsNullOrWhiteSpace(fSettings.Tenant))
            {
                fSettings.Tenant = "NOT_CONFIGURED";
            }
            if (string.IsNullOrWhiteSpace(fSettings.TimeZone))
            {
                fSettings.TimeZone = "America/Mexico_City";
            }
            return FiscalApiClient.Create(fSettings);
        });

        services.AddScoped<IFiscalApiClientAdapter, FiscalApiClientAdapter>();
        services.AddScoped<Pos.Infrastructure.Invoicing.Logging.IFacturacionLogService, Pos.Infrastructure.Invoicing.Logging.FacturacionLogService>();
        services.AddScoped<IFacturacionApplicationService, FacturacionApplicationService>();

        return services;
    }
}
