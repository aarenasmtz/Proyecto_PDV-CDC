import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import { FacturaDto } from '../../types/tiposFacturacion';

interface Props {
  factura: FacturaDto;
  onClose: () => void;
  onRefresh: () => void;
  onOpenCancelar: (factura: FacturaDto) => void;
}

export const FacturaDetalleModal: React.FC<Props> = ({ factura, onClose, onRefresh, onOpenCancelar }) => {
  const { t, i18n } = useTranslation();
  const locale = i18n.language.startsWith('zh') ? 'zh-CN' : 'es-MX';
  const money = new Intl.NumberFormat(locale, { style: 'currency', currency: 'MXN' });

  const [loadingSat, setLoadingSat] = useState(false);
  const [downloadingXml, setDownloadingXml] = useState(false);
  const [downloadingPdf, setDownloadingPdf] = useState(false);
  const [sendingEmail, setSendingEmail] = useState(false);
  const [showEmailInput, setShowEmailInput] = useState(false);
  const [targetEmail, setTargetEmail] = useState(factura.emailReceptor || '');
  const [feedbackMessage, setFeedbackMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleDescargarXml = async () => {
    setDownloadingXml(true);
    setFeedbackMessage(null);
    try {
      await servicioFacturacion.descargarXml(factura.id, `Factura_${factura.serie ? `${factura.serie}_` : ''}${factura.folio}.xml`);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al descargar XML';
      setFeedbackMessage({ type: 'error', text: msg });
    } finally {
      setDownloadingXml(false);
    }
  };

  const handleDescargarPdf = async () => {
    setDownloadingPdf(true);
    setFeedbackMessage(null);
    try {
      await servicioFacturacion.descargarPdf(factura.id, `Factura_${factura.serie ? `${factura.serie}_` : ''}${factura.folio}.pdf`);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al descargar PDF';
      setFeedbackMessage({ type: 'error', text: msg });
    } finally {
      setDownloadingPdf(false);
    }
  };

  const handleConsultarSat = async () => {
    setLoadingSat(true);
    setFeedbackMessage(null);
    try {
      const res = await servicioFacturacion.consultarEstadoSat(factura.id);
      setFeedbackMessage({
        type: 'success',
        text: `Estado SAT: ${res.estado || 'Consultado'} — Cancelabilidad: ${res.esCancelable || 'N/A'}`
      });
      onRefresh();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al consultar SAT';
      setFeedbackMessage({ type: 'error', text: msg });
    } finally {
      setLoadingSat(false);
    }
  };

  const handleEnviarCorreo = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!targetEmail.trim()) return;
    setSendingEmail(true);
    setFeedbackMessage(null);
    try {
      await servicioFacturacion.enviarPorCorreo(factura.id, { emailDestino: targetEmail.trim() });
      setFeedbackMessage({ type: 'success', text: t('emailSentSuccess') });
      setShowEmailInput(false);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al enviar correo';
      setFeedbackMessage({ type: 'error', text: msg });
    } finally {
      setSendingEmail(false);
    }
  };

  const estadoBadgeClass = (estado: string) => {
    switch (estado) {
      case 'Timbrada': return 'badge-success';
      case 'EnProceso': return 'badge-info';
      case 'Cancelada': return 'badge-danger';
      case 'Rechazada': return 'badge-danger';
      case 'CancelacionSolicitada': return 'badge-warning';
      case 'ResultadoPorConfirmar': return 'badge-warning';
      default: return 'badge-secondary';
    }
  };

  return (
    <div className="pos-receipt-backdrop" onMouseDown={e => e.target === e.currentTarget && onClose()}>
      <div className="pos-customer-modal" style={{ width: 'min(800px, 98%)', maxHeight: '90vh' }} role="dialog" aria-modal="true">
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', borderBottom: '1px solid var(--border-subtle)', paddingBottom: '0.75rem', marginBottom: '1rem' }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.35rem' }}>
                🧾 Factura {factura.serie ? `${factura.serie}-` : ''}{factura.folio}
              </h2>
              <span className={`badge ${estadoBadgeClass(factura.estado)}`}>
                {factura.estado}
              </span>
              {factura.estadoSat && (
                <span className={`badge ${factura.estadoSat === 'Vigente' ? 'badge-success' : 'badge-danger'}`}>
                  SAT: {factura.estadoSat}
                </span>
              )}
            </div>
            <p style={{ margin: '0.25rem 0 0', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
              Venta #{factura.idVenta} • Fecha Emisión: {new Date(factura.fechaEmisionUtc).toLocaleString(locale)}
            </p>
          </div>
          <button type="button" className="pos-receipt-close" onClick={onClose}>✕</button>
        </div>

        {feedbackMessage && (
          <div
            className={`pos-notice ${feedbackMessage.type === 'error' ? 'pos-notice--error' : 'pos-notice--success'}`}
            style={{ marginBottom: '1rem', padding: '0.5rem 0.75rem', borderRadius: '4px' }}
          >
            {feedbackMessage.text}
          </div>
        )}

        {/* Action Toolbar */}
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem', marginBottom: '1rem', padding: '0.5rem', background: 'var(--background-container)', borderRadius: '6px' }}>
          {factura.estado === 'Timbrada' && (
            <>
              <button
                type="button"
                className="action-btn"
                style={{ fontSize: '0.82rem', padding: '0.4rem 0.75rem' }}
                onClick={handleDescargarPdf}
                disabled={downloadingPdf}
              >
                {downloadingPdf ? 'Descargando...' : '📄 Descargar PDF'}
              </button>
              <button
                type="button"
                className="action-btn"
                style={{ fontSize: '0.82rem', padding: '0.4rem 0.75rem' }}
                onClick={handleDescargarXml}
                disabled={downloadingXml}
              >
                {downloadingXml ? 'Descargando...' : '📥 Descargar XML'}
              </button>
              <button
                type="button"
                className="lang-btn"
                style={{ fontSize: '0.82rem', padding: '0.4rem 0.75rem' }}
                onClick={() => setShowEmailInput(!showEmailInput)}
              >
                ✉️ {t('sendEmail')}
              </button>
            </>
          )}

          <button
            type="button"
            className="lang-btn"
            style={{ fontSize: '0.82rem', padding: '0.4rem 0.75rem' }}
            onClick={handleConsultarSat}
            disabled={loadingSat}
          >
            {loadingSat ? 'Consultando...' : '🔄 Consultar SAT'}
          </button>

          {factura.estado === 'Timbrada' && (
            <button
              type="button"
              className="lang-btn"
              style={{ fontSize: '0.82rem', padding: '0.4rem 0.75rem', color: 'var(--danger)', marginLeft: 'auto' }}
              onClick={() => onOpenCancelar(factura)}
            >
              🚫 {t('cancelInvoice')}
            </button>
          )}
        </div>

        {/* Email prompt if open */}
        {showEmailInput && (
          <form onSubmit={handleEnviarCorreo} style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', marginBottom: '1rem', background: 'var(--background-surface)', padding: '0.6rem', border: '1px solid var(--border-input)', borderRadius: '6px' }}>
            <input
              type="email"
              required
              className="form-control"
              style={{ flex: 1 }}
              placeholder="correo@ejemplo.com"
              value={targetEmail}
              onChange={e => setTargetEmail(e.target.value)}
              disabled={sendingEmail}
            />
            <button type="submit" className="action-btn" style={{ fontSize: '0.82rem' }} disabled={sendingEmail || !targetEmail.trim()}>
              {sendingEmail ? t('sending') : t('send')}
            </button>
            <button type="button" className="lang-btn" style={{ fontSize: '0.82rem' }} onClick={() => setShowEmailInput(false)}>
              ✕
            </button>
          </form>
        )}

        {/* Fiscal Overview Grid */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
          {/* Emisor */}
          <div style={{ padding: '0.75rem', border: '1px solid var(--border-subtle)', borderRadius: '6px' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', color: 'var(--primary-main)' }}>Emisor</h4>
            <div style={{ fontSize: '0.85rem', display: 'grid', gap: '0.2rem' }}>
              <div><strong>RFC:</strong> {factura.rfcEmisor}</div>
              <div><strong>Razón Social:</strong> {factura.razonSocialEmisor}</div>
              <div><strong>Régimen Fiscal:</strong> {factura.regimenFiscalEmisor}</div>
              <div><strong>C.P. Expedición:</strong> {factura.codigoPostalExpedicion}</div>
            </div>
          </div>

          {/* Receptor */}
          <div style={{ padding: '0.75rem', border: '1px solid var(--border-subtle)', borderRadius: '6px' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', color: 'var(--primary-main)' }}>Receptor</h4>
            <div style={{ fontSize: '0.85rem', display: 'grid', gap: '0.2rem' }}>
              <div><strong>RFC:</strong> {factura.rfcReceptor}</div>
              <div><strong>Razón Social:</strong> {factura.razonSocialReceptor}</div>
              <div><strong>Régimen Fiscal:</strong> {factura.regimenFiscalReceptor}</div>
              <div><strong>Uso CFDI:</strong> {factura.usoCfdi}</div>
              <div><strong>C.P. Receptor:</strong> {factura.codigoPostalReceptor}</div>
              <div><strong>Email:</strong> {factura.emailReceptor || '—'}</div>
            </div>
          </div>
        </div>

        {/* SAT Identifiers */}
        <div style={{ padding: '0.75rem', background: 'var(--background-container)', borderRadius: '6px', marginBottom: '1rem', fontSize: '0.82rem' }}>
          <div><strong>Folio Fiscal (UUID):</strong> <span style={{ fontFamily: 'monospace' }}>{factura.uuid || 'Sin timbrar'}</span></div>
          {factura.fechaTimbradoUtc && <div><strong>Fecha Timbrado SAT:</strong> {new Date(factura.fechaTimbradoUtc).toLocaleString(locale)}</div>}
          {factura.uuidSustitucion && <div><strong>UUID Sustituto:</strong> {factura.uuidSustitucion}</div>}
        </div>

        {/* Items Table */}
        <div style={{ overflowX: 'auto', marginBottom: '1rem' }}>
          <table className="sales-history-table" style={{ fontSize: '0.8rem' }}>
            <thead>
              <tr>
                <th>Clave SAT</th>
                <th>Descripción</th>
                <th style={{ textAlign: 'right' }}>Cant.</th>
                <th style={{ textAlign: 'right' }}>P. Unit</th>
                <th style={{ textAlign: 'right' }}>Desc.</th>
                <th style={{ textAlign: 'right' }}>Base</th>
                <th style={{ textAlign: 'right' }}>IVA (16%)</th>
                <th style={{ textAlign: 'right' }}>Importe</th>
              </tr>
            </thead>
            <tbody>
              {(factura.partidas || []).map(p => (
                <tr key={p.id}>
                  <td><code style={{ fontSize: '0.75rem' }}>{p.claveProdServ}</code> ({p.claveUnidad})</td>
                  <td>{p.descripcion}</td>
                  <td style={{ textAlign: 'right' }}>{p.cantidad}</td>
                  <td style={{ textAlign: 'right' }}>{money.format(p.valorUnitario)}</td>
                  <td style={{ textAlign: 'right' }}>{p.descuento > 0 ? `-${money.format(p.descuento)}` : '—'}</td>
                  <td style={{ textAlign: 'right' }}>{money.format(p.baseIva)}</td>
                  <td style={{ textAlign: 'right' }}>{money.format(p.importeIva)}</td>
                  <td style={{ textAlign: 'right' }}><strong>{money.format(p.importe)}</strong></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Totals Summary */}
        <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: '1rem' }}>
          <div style={{ minWidth: '240px', background: 'var(--background-surface)', border: '1px solid var(--border-subtle)', borderRadius: '6px', padding: '0.75rem', fontSize: '0.85rem', display: 'grid', gap: '0.3rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>Subtotal:</span>
              <strong>{money.format(factura.subtotal)}</strong>
            </div>
            {factura.descuento > 0 && (
              <div style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--danger)' }}>
                <span>Descuento:</span>
                <strong>-{money.format(factura.descuento)}</strong>
              </div>
            )}
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span>IVA (16%):</span>
              <strong>{money.format(factura.totalImpuestosTrasladados)}</strong>
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid var(--border-subtle)', paddingTop: '0.4rem', fontSize: '1.05rem', color: 'var(--primary-main)' }}>
              <span>Total:</span>
              <strong>{money.format(factura.total)}</strong>
            </div>
          </div>
        </div>

        {/* History / Traceability */}
        {factura.historialEstados && factura.historialEstados.length > 0 && (
          <div style={{ marginTop: '1rem', borderTop: '1px solid var(--border-subtle)', paddingTop: '0.75rem' }}>
            <h4 style={{ margin: '0 0 0.5rem 0', fontSize: '0.9rem' }}>📜 Historial de Estados y Trazabilidad</h4>
            <div style={{ maxHeight: '140px', overflowY: 'auto', fontSize: '0.78rem' }}>
              {factura.historialEstados.map(h => (
                <div key={h.id} style={{ display: 'flex', gap: '0.5rem', padding: '0.25rem 0', borderBottom: '1px dotted var(--border-subtle)' }}>
                  <span style={{ color: 'var(--text-secondary)', minWidth: '130px' }}>
                    {new Date(h.fechaUtc).toLocaleString(locale)}
                  </span>
                  <span className={`badge ${estadoBadgeClass(h.estadoNuevo)}`} style={{ fontSize: '0.7rem' }}>
                    {h.estadoNuevo}
                  </span>
                  <span style={{ color: 'var(--text-secondary)' }}>{h.detalle || 'Cambio de estado'}</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default FacturaDetalleModal;
