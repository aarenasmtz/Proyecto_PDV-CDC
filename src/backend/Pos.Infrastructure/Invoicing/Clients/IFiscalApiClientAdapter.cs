using Fiscalapi.Common;
using Fiscalapi.Models;

namespace Pos.Infrastructure.Invoicing.Clients;

public interface IFiscalApiClientAdapter
{
    Task<ApiResponse<Invoice>> CreateInvoiceAsync(Invoice requestModel, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<ApiResponse<CancelInvoiceResponse>> CancelInvoiceAsync(CancelInvoiceRequest requestModel, CancellationToken cancellationToken = default);
    Task<ApiResponse<FileResponse>> GetPdfAsync(CreatePdfRequest requestModel, CancellationToken cancellationToken = default);
    Task<ApiResponse<FileResponse>> GetXmlAsync(string invoiceId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SendInvoiceEmailAsync(SendInvoiceRequest requestModel, CancellationToken cancellationToken = default);
    Task<ApiResponse<InvoiceStatusResponse>> GetInvoiceStatusAsync(InvoiceStatusRequest requestModel, CancellationToken cancellationToken = default);
}

