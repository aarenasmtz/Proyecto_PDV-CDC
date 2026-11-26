using Fiscalapi.Abstractions;
using Fiscalapi.Common;
using Fiscalapi.Models;
using Microsoft.Extensions.Logging;

namespace Pos.Infrastructure.Invoicing.Clients;

public class FiscalApiClientAdapter : IFiscalApiClientAdapter
{
    private readonly IFiscalApiClient _fiscalApiClient;
    private readonly ILogger<FiscalApiClientAdapter> _logger;

    public FiscalApiClientAdapter(IFiscalApiClient fiscalApiClient, ILogger<FiscalApiClientAdapter> logger)
    {
        _fiscalApiClient = fiscalApiClient;
        _logger = logger;
    }

    public async Task<ApiResponse<Invoice>> CreateInvoiceAsync(Invoice requestModel, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enviando solicitud de timbrado a FiscalAPI con IdempotencyKey: {IdempotencyKey}", idempotencyKey);
        return await _fiscalApiClient.Invoices.CreateAsync(requestModel, idempotencyKey);
    }

    public async Task<ApiResponse<CancelInvoiceResponse>> CancelInvoiceAsync(CancelInvoiceRequest requestModel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enviando solicitud de cancelación a FiscalAPI para UUID: {Uuid}, Motivo: {Motivo}", requestModel.InvoiceUuid, requestModel.CancellationReasonCode);
        return await _fiscalApiClient.Invoices.CancelAsync(requestModel);
    }

    public async Task<ApiResponse<FileResponse>> GetPdfAsync(CreatePdfRequest requestModel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solicitando PDF de factura a FiscalAPI para InvoiceId: {InvoiceId}", requestModel.InvoiceId);
        return await _fiscalApiClient.Invoices.GetPdfAsync(requestModel);
    }

    public async Task<ApiResponse<FileResponse>> GetXmlAsync(string invoiceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solicitando XML de factura a FiscalAPI para InvoiceId: {InvoiceId}", invoiceId);
        return await _fiscalApiClient.Invoices.GetXmlAsync(invoiceId);
    }

    public async Task<ApiResponse<bool>> SendInvoiceEmailAsync(SendInvoiceRequest requestModel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solicitando envío de correo a FiscalAPI para InvoiceId: {InvoiceId}, Destino: {Email}", requestModel.InvoiceId, requestModel.ToEmail);
        return await _fiscalApiClient.Invoices.SendAsync(requestModel);
    }

    public async Task<ApiResponse<InvoiceStatusResponse>> GetInvoiceStatusAsync(InvoiceStatusRequest requestModel, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando estado ante el SAT en FiscalAPI para UUID: {Uuid}", requestModel.InvoiceUuid);
        return await _fiscalApiClient.Invoices.GetStatusAsync(requestModel);
    }
}

