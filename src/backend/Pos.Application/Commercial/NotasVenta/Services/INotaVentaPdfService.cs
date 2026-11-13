using Pos.Application.Commercial.NotasVenta.DTOs;

namespace Pos.Application.Commercial.NotasVenta.Services;

public interface INotaVentaPdfService
{
    byte[] GenerarPdf(NotaVentaDto nota);
}

