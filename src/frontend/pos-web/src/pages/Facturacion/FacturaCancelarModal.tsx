import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import { FacturaDto } from '../../types/tiposFacturacion';

interface Props {
  factura: FacturaDto;
  onClose: () => void;
  onCancelled: () => void;
}

export const FacturaCancelarModal: React.FC<Props> = ({ factura, onClose, onCancelled }) => {
  const { t } = useTranslation();
  const [motivo, setMotivo] = useState('02');
  const [folioFiscalSustitucion, setFolioFiscalSustitucion] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (motivo === '01' && !folioFiscalSustitucion.trim()) {
      setError('El motivo 01 requiere capturar el UUID del comprobante que lo sustituye.');
      return;
    }

    if (motivo === '01') {
      const uuidRegex = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
      if (!uuidRegex.test(folioFiscalSustitucion.trim())) {
        setError('El UUID de sustitución no tiene un formato válido (XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX).');
        return;
      }
    }

    setSubmitting(true);
    try {
      const res = await servicioFacturacion.solicitarCancelacion(factura.id, {
        motivoCancelacion: motivo,
        uuidSustitucion: motivo === '01' ? folioFiscalSustitucion.trim() : undefined
      });

      if (res.exitoso) {
        onCancelled();
        onClose();
      } else {
        setError(res.mensaje || 'No se pudo cancelar el comprobante.');
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al solicitar la cancelación.';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="pos-receipt-backdrop" onMouseDown={e => e.target === e.currentTarget && !submitting && onClose()}>
      <div className="pos-customer-modal" style={{ width: 'min(550px, 100%)' }} role="dialog" aria-modal="true">
        <h2 style={{ color: 'var(--danger)', margin: '0 0 0.5rem 0' }}>
          🚫 {t('cancelInvoice')}
        </h2>
        <p style={{ margin: '0 0 1rem 0' }}>
          Factura <strong>{factura.serie ? `${factura.serie}-` : ''}{factura.folio}</strong> (UUID: {factura.uuid || 'Sin timbrar'})
        </p>

        {error && <div className="pos-notice pos-notice--error" style={{ marginBottom: '1rem' }}>{error}</div>}

        <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '1rem' }}>
          <label className="pos-field">
            {t('cancellationReason')} *
            <select
              className="form-control"
              value={motivo}
              onChange={e => setMotivo(e.target.value)}
              disabled={submitting}
            >
              <option value="02">{t('cancellationReason02')}</option>
              <option value="03">{t('cancellationReason03')}</option>
              <option value="01">{t('cancellationReason01')}</option>
              <option value="04">{t('cancellationReason04')}</option>
            </select>
          </label>

          {motivo === '01' && (
            <label className="pos-field">
              {t('replacementUuid')} *
              <input
                type="text"
                className="form-control"
                required
                value={folioFiscalSustitucion}
                onChange={e => setFolioFiscalSustitucion(e.target.value.trim())}
                placeholder="Ej. 12345678-ABCD-1234-ABCD-1234567890AB"
                disabled={submitting}
              />
              <small style={{ color: 'var(--text-secondary)' }}>
                Ingrese el UUID del nuevo CFDI previamente emitido que sustituye a esta factura.
              </small>
            </label>
          )}

          <div style={{
            fontSize: '0.8rem',
            color: 'var(--text-secondary)',
            background: 'var(--background-container)',
            padding: '0.65rem 0.85rem',
            borderRadius: '4px'
          }}>
            ⚠️ La solicitud de cancelación se enviará al SAT a través de FiscalAPI. Dependiendo del monto y el tiempo transcurrido desde la emisión, podría requerir la aceptación del receptor en el portal del SAT.
          </div>

          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
            <button
              type="button"
              className="pos-receipt-close"
              onClick={onClose}
              disabled={submitting}
            >
              {t('cancel')}
            </button>
            <button
              type="submit"
              className="action-btn"
              style={{ background: 'var(--danger)', borderColor: 'var(--danger)' }}
              disabled={submitting}
            >
              {submitting ? t('cancellingInvoice') : 'Confirmar Cancelación'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default FacturaCancelarModal;
