import { describe, it, expect, vi, beforeEach } from 'vitest';
import { servicioNotasVenta } from '../../services/servicioNotasVenta';
import apiClient from '../../services/apiClient';

describe('servicioNotasVenta', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('getVentas calls apiClient with correct query params', async () => {
    const mockResult = {
      items: [
        {
          id: 'v-1',
          idVenta: 101,
          folio: 'V-000101',
          fechaCreacionUtc: '2026-11-08T10:00:00Z',
          clienteNombre: 'Juan Pérez',
          cantidadProductos: 2,
          total: 3500,
          estadoVenta: 'Completed',
          estadoEntrega: 'ENTREGADO',
          esEntregado: true
        }
      ],
      totalCount: 1,
      pageNumber: 1,
      pageSize: 25
    };

    const getSpy = vi.spyOn(apiClient, 'get').mockResolvedValueOnce({ data: mockResult });

    const result = await servicioNotasVenta.getVentas({
      search: 'Juan',
      saleStatus: 'Completed',
      deliveryStatus: 'ENTREGADO',
      pageNumber: 1,
      pageSize: 25
    });

    expect(getSpy).toHaveBeenCalledWith('/notas-venta/ventas?search=Juan&saleStatus=Completed&deliveryStatus=ENTREGADO&pageNumber=1&pageSize=25');
    expect(result.items.length).toBe(1);
    expect(result.items[0].folio).toBe('V-000101');
    expect(result.items[0].esEntregado).toBe(true);
  });

  it('confirmarEntrega sends POST with correct body', async () => {
    const mockDetalle = {
      ventaId: 'v-1',
      idVenta: 101,
      folio: 'V-000101',
      fechaHoraUtc: '2026-11-08T10:00:00Z',
      fechaHoraLocal: '08/11/2026 04:00',
      vendedorNombre: 'Admin',
      estadoVenta: 'Completed',
      estadoEntrega: 'ENTREGADO' as const,
      esEntregado: true,
      cliente: {
        nombreCompleto: 'Juan Pérez',
        telefono: '4771234567',
        direccion: 'Calle 1',
        ciudadEstadoCp: 'León',
        email: 'juan@test.com'
      },
      entrega: {
        destinatario: 'Juan Pérez',
        direccionEntrega: 'Calle 1, León',
        telefonoContacto: '4771234567',
        tipoEntrega: 'Entrega a domicilio'
      },
      partidas: [],
      totales: {
        subtotal: 1000,
        descuento: 0,
        tieneIva: false,
        montoIva: 0,
        total: 1000,
        metodoPago: 'Efectivo',
        montoPagado: 1000,
        saldoPendiente: 0
      },
      observacionesComerciales: '',
      condicionesVenta: '',
      empresa: {
        nombreComercial: 'WPC Bajío',
        razonSocial: 'WPC',
        slogan: '',
        direccion: '',
        ciudadEstadoCp: '',
        telefono: '',
        celularWhatsapp: '',
        email: '',
        sitioWeb: ''
      }
    };

    const postSpy = vi.spyOn(apiClient, 'post').mockResolvedValueOnce({ data: mockDetalle });

    const result = await servicioNotasVenta.confirmarEntrega('v-1', true, 'Recibido conforme');

    expect(postSpy).toHaveBeenCalledWith('/notas-venta/ventas/v-1/confirmar-entrega', {
      entregado: true,
      observacionesEntrega: 'Recibido conforme'
    });
    expect(result.esEntregado).toBe(true);
  });
});

