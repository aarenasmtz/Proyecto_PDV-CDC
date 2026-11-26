export interface NotaVentaListItem {
  id: string;
  idVenta: number;
  folio: string;
  fechaCreacionUtc: string;
  clienteNombre: string;
  cantidadProductos: number;
  total: number;
  estadoVenta: string;
  estadoEntrega: 'ENTREGADO' | 'PENDIENTE DE ENTREGA';
  esEntregado: boolean;
}

export interface NotaVentaPartida {
  partidaId: string;
  sku: string;
  descripcion: string;
  cantidad: number;
  unidadMedida: string;
  piezasPorCaja: number;
  totalPiezas: number | null;
  precioUnitario: number;
  descuento: number;
  importe: number;
}

export interface NotaVentaCliente {
  id?: string;
  nombreCompleto: string;
  telefono: string;
  direccion: string;
  ciudadEstadoCp: string;
  email: string;
  rfc?: string;
}

export interface NotaVentaEntrega {
  destinatario: string;
  direccionEntrega: string;
  telefonoContacto: string;
  tipoEntrega: string;
}

export interface NotaVentaTotales {
  subtotal: number;
  descuento: number;
  tieneIva: boolean;
  montoIva: number;
  total: number;
  metodoPago: string;
  montoPagado: number;
  saldoPendiente: number;
}

export interface EmpresaComercialConfig {
  nombreComercial: string;
  razonSocial: string;
  slogan: string;
  direccion: string;
  ciudadEstadoCp: string;
  telefono: string;
  celularWhatsapp: string;
  email: string;
  sitioWeb: string;
}

export interface NotaVentaDetalle {
  ventaId: string;
  idVenta: number;
  folio: string;
  fechaHoraUtc: string;
  fechaHoraLocal: string;
  vendedorNombre: string;
  estadoVenta: string;
  estadoEntrega: 'ENTREGADO' | 'PENDIENTE DE ENTREGA';
  esEntregado: boolean;
  cliente: NotaVentaCliente;
  entrega: NotaVentaEntrega;
  partidas: NotaVentaPartida[];
  totales: NotaVentaTotales;
  observacionesComerciales: string;
  condicionesVenta: string;
  empresa: EmpresaComercialConfig;
}

export interface PagedNotasVentaResult {
  items: NotaVentaListItem[];
  totalItems: number;
  totalCount?: number;
  pageNumber: number;
  pageSize: number;
  totalPages?: number;
}

export interface FiltrosNotasVenta {
  search?: string;
  startDate?: string;
  endDate?: string;
  saleStatus?: string;
  deliveryStatus?: string;
  pageNumber: number;
  pageSize: number;
}

