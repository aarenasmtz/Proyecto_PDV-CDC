import { describe, expect, it, vi } from 'vitest';
import { apiClient } from '../../services/apiClient';
import { servicioFacturacion } from '../../services/servicioFacturacion';

describe('servicioFacturacion - Bitacora y llamadas tecnicas FiscalAPI', () => {
  it('calls obtenerBitacora with encoded query parameters', async () => {
    const requestSpy = vi.spyOn(apiClient, 'request').mockResolvedValueOnce({
      items: [],
      pageNumber: 1,
      pageSize: 20,
      totalItems: 0,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false
    });

    await servicioFacturacion.obtenerBitacora({
      busqueda: 'CORR-123',
      ambiente: 'QA',
      operacion: 'Emitir',
      resultado: 'Exitoso',
      page: 2,
      pageSize: 50
    });

    expect(requestSpy).toHaveBeenCalledTimes(1);
    const calledUrl = requestSpy.mock.calls[0][0];
    expect(calledUrl).toContain('/facturacion/bitacora?');
    expect(calledUrl).toContain('busqueda=CORR-123');
    expect(calledUrl).toContain('ambiente=QA');
    expect(calledUrl).toContain('operacion=Emitir');
    expect(calledUrl).toContain('resultado=Exitoso');
    expect(calledUrl).toContain('page=2');
    expect(calledUrl).toContain('pageSize=50');
  });

  it('calls obtenerBitacoraPorId with target id', async () => {
    const fakeLogId = '11111111-2222-3333-4444-555555555555';
    const requestSpy = vi.spyOn(apiClient, 'request').mockResolvedValueOnce({
      id: fakeLogId,
      correlationId: 'CORR-TEST',
      operacion: 'Consultar',
      metodoHttp: 'GET',
      ruta: '/v1/invoices/1',
      estadoEjecucion: 'Finalizada',
      resultado: 'Exitoso'
    });

    const res = await servicioFacturacion.obtenerBitacoraPorId(fakeLogId);

    expect(requestSpy).toHaveBeenCalledWith(`/facturacion/bitacora/${fakeLogId}`);
    expect(res.id).toBe(fakeLogId);
  });

  it('calls obtenerLlamadasRelacionadas with encoded correlationId', async () => {
    const correlationId = 'CORR-VENTA#100-XYZ';
    const requestSpy = vi.spyOn(apiClient, 'request').mockResolvedValueOnce([]);

    await servicioFacturacion.obtenerLlamadasRelacionadas(correlationId);

    expect(requestSpy).toHaveBeenCalledWith(`/facturacion/bitacora/relacionadas/${encodeURIComponent(correlationId)}`);
  });

  it('calls purgarBitacora with POST method and diasRetencion query param', async () => {
    const requestSpy = vi.spyOn(apiClient, 'request').mockResolvedValueOnce({
      registrosEliminados: 14,
      mensaje: 'Purga completada'
    });

    const res = await servicioFacturacion.purgarBitacora(60);

    expect(requestSpy).toHaveBeenCalledWith('/facturacion/bitacora/purgar?diasRetencion=60', {
      method: 'POST'
    });
    expect(res.registrosEliminados).toBe(14);
  });
});

