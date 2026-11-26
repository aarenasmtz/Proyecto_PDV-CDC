export type FacturaEstado =
  | 'Borrador'
  | 'EnProceso'
  | 'Timbrada'
  | 'Rechazada'
  | 'ResultadoPorConfirmar'
  | 'CancelacionSolicitada'
  | 'Cancelada';

export interface FacturaPartidaDto {
  id: string;
  productoId?: string;
  claveProdServ: string;
  claveUnidad: string;
  unidad: string;
  descripcion: string;
  sku?: string;
  cantidad: number;
  valorUnitario: number;
  importe: number;
  descuento: number;
  objetoImp: string;
  tasaOcuotaIva: number;
  baseIva: number;
  importeIva: number;
}

export interface FacturaHistorialEstadoDto {
  id: string;
  estadoAnterior: string;
  estadoNuevo: string;
  detalle: string;
  fechaUtc: string;
}

export interface FacturaDto {
  id: string;
  ventaId: string;
  idVenta: number;
  folioVenta: string;
  serie: string;
  folio: string;
  folioCompleto: string;
  ambiente: 'QA' | 'PR';
  estado: FacturaEstado;
  estadoSat?: string;
  estadoCancelacionSat?: string;
  fiscalapiInvoiceId?: string;
  uuid?: string;
  fechaEmisionUtc: string;
  fechaTimbradoUtc?: string;
  rfcEmisor: string;
  razonSocialEmisor: string;
  regimenFiscalEmisor: string;
  codigoPostalExpedicion: string;
  rfcReceptor: string;
  razonSocialReceptor: string;
  usoCfdi: string;
  regimenFiscalReceptor: string;
  codigoPostalReceptor: string;
  emailReceptor: string;
  moneda: string;
  tipoCambio: number;
  formaPago: string;
  metodoPago: string;
  tipoComprobante: string;
  exportacion: string;
  subtotal: number;
  descuento: number;
  total: number;
  totalImpuestosTrasladados: number;
  totalImpuestosRetenidos: number;
  idempotencyKey: string;
  traceIdentifier?: string;
  mensajeRespuesta?: string;
  detallesRespuesta?: string;
  codigoEstadoHttp?: number;
  tieneXml: boolean;
  tienePdf: boolean;
  tieneAcuseCancelacion: boolean;
  motivoCancelacion?: string;
  uuidSustitucion?: string;
  fechaSolicitudCancelacionUtc?: string;
  fechaCancelacionUtc?: string;
  partidas?: FacturaPartidaDto[];
  historialEstados?: FacturaHistorialEstadoDto[];
}

export interface ConfiguracionEmisorDto {
  id?: string;
  empresa: string;
  sucursal: string;
  rfc: string;
  razonSocial: string;
  regimenFiscal: string;
  codigoPostalExpedicion: string;
  serie: string;
  folioSiguiente: number;
  fiscalapiPersonId?: string;
  ambiente: 'QA' | 'PR';
  csdCertificadoNumero?: string;
  estaListoParaEmitir: boolean;
  notas: string;
  facturacionHabilitada: boolean;
}

export interface ActualizarConfiguracionEmisorDto {
  empresa: string;
  sucursal: string;
  rfc: string;
  razonSocial: string;
  regimenFiscal: string;
  codigoPostalExpedicion: string;
  serie: string;
  folioSiguiente: number;
  fiscalapiPersonId?: string;
  csdCertificadoNumero?: string;
  estaListoParaEmitir: boolean;
  notas: string;
}

export interface PartidaVistaPreviaDto {
  productoId?: string;
  sku: string;
  descripcion: string;
  claveProdServSat: string;
  claveUnidadSat: string;
  unidadMedida: string;
  cantidad: number;
  valorUnitario: number;
  importe: number;
  descuento: number;
  objetoImpuestoSat: string;
  tasaIva: number;
  baseIva: number;
  importeIva: number;
}

export interface VistaPreviaFacturaDto {
  ventaId: string;
  idVenta: number;
  folioVenta: string;
  fechaVentaUtc: string;
  rfcEmisor: string;
  razonSocialEmisor: string;
  regimenFiscalEmisor: string;
  codigoPostalExpedicion: string;
  serie: string;
  folioPropuesto: string;
  clienteId?: string;
  rfcReceptor: string;
  razonSocialReceptor: string;
  codigoPostalReceptor: string;
  regimenFiscalReceptor: string;
  usoCfdi: string;
  emailReceptor: string;
  formaPago: string;
  descripcionFormaPago: string;
  metodoPago: string;
  moneda: string;
  tipoCambio: number;
  subtotal: number;
  descuento: number;
  impuestoTrasladadoIva: number;
  total: number;
  conceptos: PartidaVistaPreviaDto[];
  esElegibleParaFacturar: boolean;
  advertencias: string[];
  bloqueos: string[];
}

export interface SolicitarEmisionFacturaDto {
  rfcReceptor: string;
  razonSocialReceptor: string;
  codigoPostalReceptor: string;
  regimenFiscalReceptor: string;
  usoCfdi: string;
  emailReceptor: string;
  guardarDatosEnCliente: boolean;
  formaPago?: string;
  metodoPago?: string;
  idempotencyKey?: string;
}

export interface ResultadoEmisionFacturaDto {
  exitoso: boolean;
  facturaId?: string;
  uuid?: string;
  serie?: string;
  folio?: string;
  estado?: string;
  mensaje: string;
  detalleTecnico?: string;
  traceIdentifier?: string;
  requiereConfirmacionManual: boolean;
}

export interface SolicitarCancelacionFacturaDto {
  motivoCancelacion: string;
  uuidSustitucion?: string;
  motivoDescripcion?: string;
}

export interface ResultadoCancelacionFacturaDto {
  exitoso: boolean;
  estadoSat?: string;
  estadoCancelacionSat?: string;
  estadoLocal?: string;
  mensaje: string;
  tieneAcuseXml: boolean;
}

export interface EnviarFacturaCorreoDto {
  emailDestino: string;
  mensajePersonalizado?: string;
}

export interface ResultadoEstadoSatDto {
  codigoEstatus: string;
  estado: string;
  esCancelable: string;
  estatusCancelacion: string;
  validacionEfos: string;
  estadoLocalActualizado: string;
  fechaConsultaUtc: string;
}

export interface CatalogoSatItemDto {
  clave: string;
  descripcion: string;
}

export interface FacturaFiltrosDto {
  busqueda?: string;
  estado?: string;
  ambiente?: string;
  fechaInicioUtc?: string;
  fechaFinUtc?: string;
  rfcReceptor?: string;
  idVenta?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface FacturacionLogDto {
  id: string;
  correlationId: string;
  facturaId?: string;
  intentoId?: string;
  ventaId?: string;
  empresaId: string;
  sucursalId: string;
  usuarioId?: string;
  origen: string;
  ambiente: 'QA' | 'PR' | string;
  proveedor: string;
  operacion: string;
  metodoHttp: string;
  ruta: string;
  fechaInicioUtc: string;
  fechaFinUtc?: string;
  duracionMs?: number;
  estadoEjecucion: 'Iniciada' | 'Finalizada' | 'Interrumpida' | 'ResultadoIncierto' | string;
  httpStatusCode?: number;
  providerHttpStatusCode?: number;
  succeeded?: boolean;
  resultado: 'Exitoso' | 'Rechazado' | 'FalloTecnico' | 'ResultadoIncierto' | string;
  mensaje?: string;
  detalleError?: string;
  traceIdentifierProveedor?: string;
  identificadorExterno?: string;
  uuidFiscal?: string;
  idempotencyKey?: string;
  numeroReintento: number;
  requestSanitizadoJson?: string;
  responseSanitizadoJson?: string;
  tipoExcepcion?: string;
  stackTraceSanitizado?: string;
  versionAplicacion: string;
}

export interface FacturacionLogFiltros {
  fechaInicio?: string;
  fechaFin?: string;
  ambiente?: string;
  operacion?: string;
  ventaId?: string;
  facturaId?: string;
  uuidFiscal?: string;
  resultado?: string;
  httpStatusCode?: number;
  correlationId?: string;
  traceIdentifier?: string;
  busqueda?: string;
  page?: number;
  pageSize?: number;
}


