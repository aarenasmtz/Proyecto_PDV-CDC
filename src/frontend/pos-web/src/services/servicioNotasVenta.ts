import apiClient, { getApiBaseUrl } from './apiClient';
import {
  FiltrosNotasVenta,
  NotaVentaDetalle,
  PagedNotasVentaResult
} from '../types/tiposNotasVenta';

export const servicioNotasVenta = {
  async getVentas(filtros: FiltrosNotasVenta): Promise<PagedNotasVentaResult> {
    const params = new URLSearchParams();
    if (filtros.search) params.append('search', filtros.search);
    if (filtros.startDate) params.append('startDate', filtros.startDate);
    if (filtros.endDate) params.append('endDate', filtros.endDate);
    if (filtros.saleStatus && filtros.saleStatus !== 'ALL') params.append('saleStatus', filtros.saleStatus);
    if (filtros.deliveryStatus && filtros.deliveryStatus !== 'ALL') params.append('deliveryStatus', filtros.deliveryStatus);
    params.append('pageNumber', filtros.pageNumber.toString());
    params.append('pageSize', filtros.pageSize.toString());

    const response = await apiClient.get<PagedNotasVentaResult>(`/notas-venta/ventas?${params.toString()}`);
    return response.data;
  },

  async getNotaVenta(ventaId: string): Promise<NotaVentaDetalle> {
    const response = await apiClient.get<NotaVentaDetalle>(`/notas-venta/ventas/${ventaId}`);
    return response.data;
  },

  async obtenerBlobPdf(ventaId: string): Promise<Blob> {
    const token = apiClient.getToken();
    const url = `${getApiBaseUrl()}/notas-venta/ventas/${ventaId}/pdf`;
    const res = await fetch(url, {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      }
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({ message: 'Error al obtener PDF de nota de venta.' }));
      throw new Error(err.message || 'Error al obtener PDF de nota de venta.');
    }

    return await res.blob();
  },

  async descargarPdf(ventaId: string, folio: string): Promise<void> {
    const blob = await this.obtenerBlobPdf(ventaId);
    const sanitizedFolio = (folio || 'Nota').replace(/[\/\\]/g, '_');
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `NotaVenta_${sanitizedFolio}.pdf`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  },

  async imprimirPdf(ventaId: string): Promise<void> {
    const blob = await this.obtenerBlobPdf(ventaId);
    const url = window.URL.createObjectURL(blob);
    const iframe = document.createElement('iframe');
    iframe.style.position = 'fixed';
    iframe.style.right = '0';
    iframe.style.bottom = '0';
    iframe.style.width = '0';
    iframe.style.height = '0';
    iframe.style.border = '0';
    iframe.src = url;

    document.body.appendChild(iframe);
    iframe.onload = () => {
      setTimeout(() => {
        iframe.contentWindow?.focus();
        iframe.contentWindow?.print();
        setTimeout(() => {
          document.body.removeChild(iframe);
          window.URL.revokeObjectURL(url);
        }, 1000);
      }, 250);
    };
  },

  async confirmarEntrega(ventaId: string, entregado: boolean, observaciones?: string): Promise<NotaVentaDetalle> {
    const response = await apiClient.post<NotaVentaDetalle>(`/notas-venta/ventas/${ventaId}/confirmar-entrega`, {
      entregado,
      observacionesEntrega: observaciones
    });
    return response.data;
  }
};
