import { apiClient, getApiBaseUrl } from './apiClient';
import {
  ConfiguracionEmisorDto,
  ActualizarConfiguracionEmisorDto,
  VistaPreviaFacturaDto,
  SolicitarEmisionFacturaDto,
  ResultadoEmisionFacturaDto,
  FacturaDto,
  FacturaFiltrosDto,
  SolicitarCancelacionFacturaDto,
  ResultadoCancelacionFacturaDto,
  ResultadoEstadoSatDto,
  CatalogoSatItemDto,
  EnviarFacturaCorreoDto,
  FacturacionLogDto,
  FacturacionLogFiltros
} from '../types/tiposFacturacion';
import { PagedResult } from '../types/pagination';

export const servicioFacturacion = {
  obtenerConfiguracion: () =>
    apiClient.request<ConfiguracionEmisorDto>('/facturacion/config'),

  guardarConfiguracion: (dto: ActualizarConfiguracionEmisorDto) =>
    apiClient.request<ConfiguracionEmisorDto>('/facturacion/config', {
      method: 'PUT',
      body: JSON.stringify(dto)
    }),

  generarVistaPrevia: (saleId: string) =>
    apiClient.request<VistaPreviaFacturaDto>(`/facturacion/sales/${saleId}/preview`),

  emitirFactura: (saleId: string, dto: SolicitarEmisionFacturaDto) =>
    apiClient.request<ResultadoEmisionFacturaDto>(`/facturacion/sales/${saleId}/issue`, {
      method: 'POST',
      body: JSON.stringify(dto)
    }),

  obtenerFacturas: (filtros: FacturaFiltrosDto = {}) => {
    const params = new URLSearchParams();
    if (filtros.busqueda) params.append('busqueda', filtros.busqueda);
    if (filtros.estado) params.append('estado', filtros.estado);
    if (filtros.ambiente) params.append('ambiente', filtros.ambiente);
    if (filtros.rfcReceptor) params.append('rfcReceptor', filtros.rfcReceptor);
    if (filtros.idVenta) params.append('idVenta', filtros.idVenta.toString());
    if (filtros.fechaInicioUtc) params.append('fechaInicioUtc', filtros.fechaInicioUtc);
    if (filtros.fechaFinUtc) params.append('fechaFinUtc', filtros.fechaFinUtc);
    if (filtros.pageNumber) params.append('pageNumber', filtros.pageNumber.toString());
    if (filtros.pageSize) params.append('pageSize', filtros.pageSize.toString());

    const query = params.toString() ? `?${params.toString()}` : '';
    return apiClient.request<PagedResult<FacturaDto>>(`/facturacion/invoices${query}`);
  },

  obtenerFacturaPorId: (facturaId: string) =>
    apiClient.request<FacturaDto>(`/facturacion/invoices/${facturaId}`),

  descargarXml: async (facturaId: string, nombreArchivo?: string): Promise<void> => {
    const token = apiClient.getToken();
    const url = `${getApiBaseUrl()}/facturacion/invoices/${facturaId}/xml`;
    const res = await fetch(url, {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: 'Error al descargar XML' }));
      throw new Error(err.message || 'Error al descargar XML');
    }

    const blob = await res.blob();
    const downloadUrl = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = downloadUrl;
    a.download = nombreArchivo || `Factura_${facturaId}.xml`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(downloadUrl);
  },

  descargarPdf: async (facturaId: string, nombreArchivo?: string): Promise<void> => {
    const token = apiClient.getToken();
    const url = `${getApiBaseUrl()}/facturacion/invoices/${facturaId}/pdf`;
    const res = await fetch(url, {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: 'Error al descargar PDF' }));
      throw new Error(err.message || 'Error al descargar PDF');
    }

    const blob = await res.blob();
    const downloadUrl = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = downloadUrl;
    a.download = nombreArchivo || `Factura_${facturaId}.pdf`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(downloadUrl);
  },

  enviarPorCorreo: (facturaId: string, dto: EnviarFacturaCorreoDto) =>
    apiClient.request<{ exitoso: boolean; message: string }>(`/facturacion/invoices/${facturaId}/send-email`, {
      method: 'POST',
      body: JSON.stringify(dto)
    }),

  consultarEstadoSat: (facturaId: string) =>
    apiClient.request<ResultadoEstadoSatDto>(`/facturacion/invoices/${facturaId}/sat-status`, {
      method: 'POST'
    }),

  solicitarCancelacion: (facturaId: string, dto: SolicitarCancelacionFacturaDto) =>
    apiClient.request<ResultadoCancelacionFacturaDto>(`/facturacion/invoices/${facturaId}/cancel`, {
      method: 'POST',
      body: JSON.stringify(dto)
    }),

  obtenerCatalogoSat: (nombreCatalogo: string) =>
    apiClient.request<CatalogoSatItemDto[]>(`/facturacion/catalogs/${nombreCatalogo}`),

  obtenerBitacora: (filtros: FacturacionLogFiltros = {}) => {
    const params = new URLSearchParams();
    if (filtros.fechaInicio) params.append('fechaInicio', filtros.fechaInicio);
    if (filtros.fechaFin) params.append('fechaFin', filtros.fechaFin);
    if (filtros.ambiente) params.append('ambiente', filtros.ambiente);
    if (filtros.operacion) params.append('operacion', filtros.operacion);
    if (filtros.ventaId) params.append('ventaId', filtros.ventaId);
    if (filtros.facturaId) params.append('facturaId', filtros.facturaId);
    if (filtros.uuidFiscal) params.append('uuidFiscal', filtros.uuidFiscal);
    if (filtros.resultado) params.append('resultado', filtros.resultado);
    if (filtros.httpStatusCode) params.append('httpStatusCode', filtros.httpStatusCode.toString());
    if (filtros.correlationId) params.append('correlationId', filtros.correlationId);
    if (filtros.traceIdentifier) params.append('traceIdentifier', filtros.traceIdentifier);
    if (filtros.busqueda) params.append('busqueda', filtros.busqueda);
    if (filtros.page) params.append('page', filtros.page.toString());
    if (filtros.pageSize) params.append('pageSize', filtros.pageSize.toString());

    const queryString = params.toString();
    const endpoint = queryString ? `/facturacion/bitacora?${queryString}` : '/facturacion/bitacora';
    return apiClient.request<PagedResult<FacturacionLogDto>>(endpoint);
  },

  obtenerBitacoraPorId: (id: string) =>
    apiClient.request<FacturacionLogDto>(`/facturacion/bitacora/${id}`),

  obtenerLlamadasRelacionadas: (correlationId: string) =>
    apiClient.request<FacturacionLogDto[]>(`/facturacion/bitacora/relacionadas/${encodeURIComponent(correlationId)}`),

  purgarBitacora: (diasRetencion: number = 90) =>
    apiClient.request<{ registrosEliminados: number; mensaje: string }>(`/facturacion/bitacora/purgar?diasRetencion=${diasRetencion}`, {
      method: 'POST'
    })
};

