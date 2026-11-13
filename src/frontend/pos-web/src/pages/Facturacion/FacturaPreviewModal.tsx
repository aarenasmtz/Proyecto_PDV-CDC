import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import {
  VistaPreviaFacturaDto,
  SolicitarEmisionFacturaDto,
  ResultadoEmisionFacturaDto
} from '../../types/tiposFacturacion';

interface Props {
  saleId: string;
  idVenta?: number;
  onClose: () => void;
  onSuccess: (resultado: ResultadoEmisionFacturaDto) => void;
}

export const FacturaPreviewModal: React.FC<Props> = ({ saleId, idVenta, onClose, onSuccess }) => {
  const { t, i18n } = useTranslation();
  const locale = i18n.language.startsWith('zh') ? 'zh-CN' : 'es-MX';
  const money = new Intl.NumberFormat(locale, { style: 'currency', currency: 'MXN' });

  const [loadingPreview, setLoadingPreview] = useState(true);
  const [preview, setPreview] = useState<VistaPreviaFacturaDto | null>(null);
  const [errorPreview, setErrorPreview] = useState('');

  // Receptor Form State
  const [rfcReceptor, setRfcReceptor] = useState('');
  const [razonSocialReceptor, setRazonSocialReceptor] = useState('');
  const [regimenFiscalReceptor, setRegimenFiscalReceptor] = useState('616');
  const [usoCfdi, setUsoCfdi] = useState('G03');
  const [codigoPostalReceptor, setCodigoPostalReceptor] = useState('');
  const [emailEnvio, setEmailEnvio] = useState('');
  const [actualizarDatosCliente, setActualizarDatosCliente] = useState(true);

  // Stamping State
  const [stamping, setStamping] = useState(false);
  const [stampedResult, setStampedResult] = useState<ResultadoEmisionFacturaDto | null>(null);
  const [stampError, setStampError] = useState<{ message: string; traceId?: string } | null>(null);

  useEffect(() => {
    cargarVistaPrevia();
  }, [saleId]);

  const cargarVistaPrevia = async () => {
    setLoadingPreview(true);
    setErrorPreview('');
    try {
      const data = await servicioFacturacion.generarVistaPrevia(saleId);
      setPreview(data);
      setRfcReceptor(data.rfcReceptor || 'XAXX010101000');
      setRazonSocialReceptor(data.razonSocialReceptor || 'PÚBLICO EN GENERAL');
      setRegimenFiscalReceptor(data.regimenFiscalReceptor || '616');
      setUsoCfdi(data.usoCfdi || 'S01');
      setCodigoPostalReceptor(data.codigoPostalReceptor || data.codigoPostalExpedicion || '37000');
      setEmailEnvio(data.emailReceptor || '');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al generar la vista previa de la factura.';
      setErrorPreview(msg);
    } finally {
      setLoadingPreview(false);
    }
  };

  const handleEmitirFactura = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!preview || !preview.esElegibleParaFacturar) return;

    setStampError(null);

    // Validations
    const cleanRfc = rfcReceptor.trim().toUpperCase();
    if (!cleanRfc || (cleanRfc.length !== 12 && cleanRfc.length !== 13)) {
      setStampError({ message: 'El RFC del receptor debe tener 12 (moral) o 13 (física) caracteres.' });
      return;
    }
    if (!razonSocialReceptor.trim()) {
      setStampError({ message: 'La Razón Social o Nombre fiscal del receptor es requerida.' });
      return;
    }
    if (!/^\d{5}$/.test(codigoPostalReceptor.trim())) {
      setStampError({ message: 'El Código Postal fiscal del receptor debe ser de 5 dígitos numéricos.' });
      return;
    }

    const payload: SolicitarEmisionFacturaDto = {
      rfcReceptor: cleanRfc,
      razonSocialReceptor: razonSocialReceptor.trim().toUpperCase(),
      regimenFiscalReceptor: regimenFiscalReceptor.trim(),
      usoCfdi: usoCfdi.trim(),
      codigoPostalReceptor: codigoPostalReceptor.trim(),
      emailReceptor: emailEnvio.trim() || '',
      metodoPago: preview.metodoPago,
      formaPago: preview.formaPago,
      guardarDatosEnCliente: actualizarDatosCliente
    };

    setStamping(true);
    try {
      const res = await servicioFacturacion.emitirFactura(saleId, payload);
      if (res.exitoso) {
        setStampedResult(res);
        onSuccess(res);
      } else {
        setStampError({
          message: res.mensaje || 'La emisión fue rechazada por FiscalAPI.',
          traceId: res.traceIdentifier
        });
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error inesperado durante la emisión.';
      setStampError({ message: msg });
    } finally {
      setStamping(false);
    }
  };

  const handleDescargarXml = async () => {
    if (!stampedResult?.facturaId) return;
    await servicioFacturacion.descargarXml(stampedResult.facturaId);
  };

  const handleDescargarPdf = async () => {
    if (!stampedResult?.facturaId) return;
    await servicioFacturacion.descargarPdf(stampedResult.facturaId);
  };

  return (
    <div className="pos-receipt-backdrop" onMouseDown={e => e.target === e.currentTarget && !stamping && onClose()}>
      <div className="pos-customer-modal" style={{ width: 'min(760px, 98%)', maxHeight: '92vh' }} role="dialog" aria-modal="true">
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '1px solid var(--border-subtle)', paddingBottom: '0.75rem', marginBottom: '1rem' }}>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.25rem' }}>
              🧾 {t('previewInvoice')} — Venta #{idVenta || preview?.idVenta || ''}
            </h2>
            <small style={{ color: 'var(--text-secondary)' }}>
              Generación de CFDI 4.0 de Ingreso
            </small>
          </div>
          <button type="button" className="pos-receipt-close" onClick={onClose} disabled={stamping}>✕</button>
        </div>

        {loadingPreview ? (
          <p>{t('loading')}</p>
        ) : errorPreview ? (
          <div className="pos-notice pos-notice--error">{errorPreview}</div>
        ) : preview ? (
          <>
            {/* If Stamped Successfully */}
            {stampedResult ? (
              <div style={{ textAlign: 'center', padding: '1.5rem 0' }}>
                <div style={{ fontSize: '3rem', marginBottom: '0.5rem' }}>🎉</div>
                <h3 style={{ color: '#28a745', margin: '0 0 0.5rem 0' }}>
                  {t('invoiceStampedSuccess')}
                </h3>
                <p style={{ margin: '0 0 1rem 0' }}>
                  Folio: <strong>{stampedResult.serie ? `${stampedResult.serie}-` : ''}{stampedResult.folio}</strong>
                  <br />
                  UUID: <code style={{ fontSize: '1rem' }}>{stampedResult.uuid}</code>
                </p>
                <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'center' }}>
                  <button type="button" className="action-btn" onClick={handleDescargarPdf}>
                    📄 {t('downloadPdf')}
                  </button>
                  <button type="button" className="action-btn" onClick={handleDescargarXml}>
                    📥 {t('downloadXml')}
                  </button>
                  <button type="button" className="lang-btn" onClick={onClose}>
                    Cerrar
                  </button>
                </div>
              </div>
            ) : (
              <form onSubmit={handleEmitirFactura} style={{ display: 'grid', gap: '1rem' }}>
                {/* Blocking Warnings */}
                {!preview.esElegibleParaFacturar && (
                  <div className="pos-notice pos-notice--error" style={{ display: 'grid', gap: '0.4rem' }}>
                    <strong>🚫 {t('blockedFromInvoicing')}:</strong>
                    <ul style={{ margin: 0, paddingLeft: '1.2rem' }}>
                      {preview.bloqueos.map((b, idx) => (
                        <li key={idx}>{b}</li>
                      ))}
                    </ul>
                  </div>
                )}

                {/* Stamp Error */}
                {stampError && (
                  <div className="pos-notice pos-notice--error">
                    <div>{stampError.message}</div>
                    {stampError.traceId && (
                      <small style={{ display: 'block', marginTop: '0.25rem', fontFamily: 'monospace' }}>
                        Trace Identifier: {stampError.traceId}
                      </small>
                    )}
                  </div>
                )}

                {/* Fiscal Data Receptor Grid */}
                <fieldset style={{ border: '1px solid var(--border-subtle)', borderRadius: '6px', padding: '0.85rem' }}>
                  <legend style={{ fontWeight: 600, color: 'var(--primary-main)', padding: '0 0.4rem' }}>
                    Datos Fiscales del Receptor
                  </legend>

                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '0.75rem', marginBottom: '0.75rem' }}>
                    <label className="pos-field">
                      {t('receiverRfc')} *
                      <input
                        type="text"
                        className="form-control"
                        required
                        maxLength={13}
                        value={rfcReceptor}
                        onChange={e => setRfcReceptor(e.target.value.toUpperCase())}
                        style={{ textTransform: 'uppercase' }}
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      />
                    </label>

                    <label className="pos-field">
                      {t('receiverLegalName')} *
                      <input
                        type="text"
                        className="form-control"
                        required
                        value={razonSocialReceptor}
                        onChange={e => setRazonSocialReceptor(e.target.value.toUpperCase())}
                        style={{ textTransform: 'uppercase' }}
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      />
                    </label>
                  </div>

                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '0.75rem', marginBottom: '0.75rem' }}>
                    <label className="pos-field">
                      {t('taxRegime')} *
                      <select
                        className="form-control"
                        value={regimenFiscalReceptor}
                        onChange={e => setRegimenFiscalReceptor(e.target.value)}
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      >
                        <option value="601">601 - General Personas Morales</option>
                        <option value="612">612 - Personas Físicas Empresariales</option>
                        <option value="626">626 - RESICO</option>
                        <option value="616">616 - Sin obligaciones fiscales</option>
                        <option value="605">605 - Sueldos y Salarios</option>
                      </select>
                    </label>

                    <label className="pos-field">
                      {t('cfdiUse')} *
                      <select
                        className="form-control"
                        value={usoCfdi}
                        onChange={e => setUsoCfdi(e.target.value)}
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      >
                        <option value="G01">G01 - Adquisición de mercancías</option>
                        <option value="G03">G03 - Gastos en general</option>
                        <option value="S01">S01 - Sin efectos fiscales</option>
                        <option value="CP01">CP01 - Pagos</option>
                      </select>
                    </label>

                    <label className="pos-field">
                      {t('postalCode')} *
                      <input
                        type="text"
                        className="form-control"
                        required
                        maxLength={5}
                        value={codigoPostalReceptor}
                        onChange={e => setCodigoPostalReceptor(e.target.value)}
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      />
                    </label>
                  </div>

                  <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: '0.75rem' }}>
                    <label className="pos-field">
                      {t('billingEmail')}
                      <input
                        type="email"
                        className="form-control"
                        value={emailEnvio}
                        onChange={e => setEmailEnvio(e.target.value)}
                        placeholder="facturacion@cliente.com"
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      />
                    </label>
                  </div>

                  {preview.clienteId && (
                    <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginTop: '0.6rem', fontSize: '0.85rem', cursor: 'pointer' }}>
                      <input
                        type="checkbox"
                        checked={actualizarDatosCliente}
                        onChange={e => setActualizarDatosCliente(e.target.checked)}
                        disabled={!preview.esElegibleParaFacturar || stamping}
                      />
                      {t('updateCustomerFiscalData')}
                    </label>
                  )}
                </fieldset>

                {/* Items & Calculations Preview */}
                <div>
                  <h4 style={{ margin: '0 0 0.5rem 0', fontSize: '0.9rem' }}>Desglose de Conceptos e Impuestos</h4>
                  <div style={{ overflowX: 'auto', border: '1px solid var(--border-subtle)', borderRadius: '6px' }}>
                    <table className="sales-history-table" style={{ fontSize: '0.8rem', margin: 0 }}>
                      <thead>
                        <tr>
                          <th>Concepto</th>
                          <th style={{ textAlign: 'center' }}>Claves SAT</th>
                          <th style={{ textAlign: 'right' }}>Cant.</th>
                          <th style={{ textAlign: 'right' }}>P. Unit</th>
                          <th style={{ textAlign: 'right' }}>Desc.</th>
                          <th style={{ textAlign: 'right' }}>IVA (16%)</th>
                          <th style={{ textAlign: 'right' }}>Total</th>
                        </tr>
                      </thead>
                      <tbody>
                        {preview.conceptos.map((item, idx) => (
                          <tr key={idx}>
                            <td>{item.descripcion}</td>
                            <td style={{ textAlign: 'center' }}>
                              <span style={{ fontFamily: 'monospace' }}>{item.claveProdServSat}</span> / {item.claveUnidadSat}
                            </td>
                            <td style={{ textAlign: 'right' }}>{item.cantidad}</td>
                            <td style={{ textAlign: 'right' }}>{money.format(item.valorUnitario)}</td>
                            <td style={{ textAlign: 'right' }}>{item.descuento > 0 ? `-${money.format(item.descuento)}` : '—'}</td>
                            <td style={{ textAlign: 'right' }}>{money.format(item.importeIva)}</td>
                            <td style={{ textAlign: 'right' }}><strong>{money.format(item.importe)}</strong></td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>

                {/* Payment & Totals Bar */}
                <div style={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-between', alignItems: 'center', gap: '1rem', background: 'var(--background-container)', padding: '0.75rem 1rem', borderRadius: '6px' }}>
                  <div style={{ fontSize: '0.85rem' }}>
                    <div><strong>Método de Pago:</strong> {preview.metodoPago} (Una sola exhibición)</div>
                    <div><strong>Forma de Pago SAT:</strong> Clave {preview.formaPago}</div>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                      Subtotal: {money.format(preview.subtotal)} | IVA: {money.format(preview.impuestoTrasladadoIva)}
                    </div>
                    <div style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--primary-main)' }}>
                      Total: {money.format(preview.total)}
                    </div>
                  </div>
                </div>

                {/* Footer buttons */}
                <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
                  <button type="button" className="pos-receipt-close" onClick={onClose} disabled={stamping}>
                    {t('cancel')}
                  </button>
                  <button
                    type="submit"
                    className="action-btn"
                    disabled={!preview.esElegibleParaFacturar || stamping}
                  >
                    {stamping ? t('stampingInProgress') : `⚡ ${t('stampInvoice')}`}
                  </button>
                </div>
              </form>
            )}
          </>
        ) : null}
      </div>
    </div>
  );
};

export default FacturaPreviewModal;
