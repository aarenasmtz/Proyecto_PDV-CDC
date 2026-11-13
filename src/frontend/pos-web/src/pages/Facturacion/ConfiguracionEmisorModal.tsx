import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import { ConfiguracionEmisorDto } from '../../types/tiposFacturacion';

interface Props {
  onClose: () => void;
  onSaved?: () => void;
}

export const ConfiguracionEmisorModal: React.FC<Props> = ({ onClose, onSaved }) => {
  const { t } = useTranslation();
  const [config, setConfig] = useState<ConfiguracionEmisorDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Form fields
  const [rfc, setRfc] = useState('');
  const [razonSocial, setRazonSocial] = useState('');
  const [regimenFiscal, setRegimenFiscal] = useState('601');
  const [codigoPostalExpedicion, setCodigoPostalExpedicion] = useState('');
  const [serie, setSerie] = useState('F');
  const [folioSiguiente, setFolioSiguiente] = useState(1);
  const [fiscalapiPersonId, setFiscalapiPersonId] = useState('');

  useEffect(() => {
    cargarConfiguracion();
  }, []);

  const cargarConfiguracion = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await servicioFacturacion.obtenerConfiguracion();
      setConfig(data);
      setRfc(data.rfc || '');
      setRazonSocial(data.razonSocial || '');
      setRegimenFiscal(data.regimenFiscal || '601');
      setCodigoPostalExpedicion(data.codigoPostalExpedicion || '');
      setSerie(data.serie || 'F');
      setFolioSiguiente(data.folioSiguiente || 1);
      setFiscalapiPersonId(data.fiscalapiPersonId || '');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al cargar configuración';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    // Validations
    const cleanRfc = rfc.trim().toUpperCase();
    if (!cleanRfc || (cleanRfc.length !== 12 && cleanRfc.length !== 13)) {
      setError('El RFC del emisor debe tener 12 (moral) o 13 (física) caracteres.');
      return;
    }
    if (!razonSocial.trim()) {
      setError('La Razón Social o Nombre fiscal es requerido.');
      return;
    }
    if (!/^\d{5}$/.test(codigoPostalExpedicion.trim())) {
      setError('El Código Postal de expedición debe ser de 5 dígitos numéricos.');
      return;
    }

    setSaving(true);
    try {
      await servicioFacturacion.guardarConfiguracion({
        empresa: config?.empresa || 'WPC Bajío',
        sucursal: config?.sucursal || 'Matriz',
        rfc: cleanRfc,
        razonSocial: razonSocial.trim(),
        regimenFiscal: regimenFiscal.trim(),
        codigoPostalExpedicion: codigoPostalExpedicion.trim(),
        serie: serie.trim().toUpperCase(),
        folioSiguiente: Math.max(1, Number(folioSiguiente)),
        fiscalapiPersonId: fiscalapiPersonId.trim() || undefined,
        estaListoParaEmitir: config?.estaListoParaEmitir ?? false,
        notas: config?.notas || ''
      });
      setSuccess('Configuración fiscal actualizada correctamente.');
      if (onSaved) onSaved();
      setTimeout(() => {
        onClose();
      }, 1000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al guardar configuración fiscal.';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="pos-receipt-backdrop" onMouseDown={e => e.target === e.currentTarget && onClose()}>
      <div className="pos-customer-modal" style={{ width: 'min(640px, 100%)' }} role="dialog" aria-modal="true">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h2 style={{ margin: 0, fontSize: '1.25rem' }}>⚙️ {t('issuerConfig')}</h2>
          <button type="button" className="pos-receipt-close" onClick={onClose}>✕</button>
        </div>

        {loading ? (
          <p>{t('loading')}</p>
        ) : (
          <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '0.9rem' }}>
            {config && (
              <div style={{
                display: 'flex',
                gap: '0.75rem',
                alignItems: 'center',
                padding: '0.6rem 0.85rem',
                background: config.ambiente === 'PR' ? 'rgba(40, 167, 69, 0.1)' : 'rgba(255, 193, 7, 0.15)',
                border: `1px solid ${config.ambiente === 'PR' ? '#28a745' : '#ffc107'}`,
                borderRadius: '6px'
              }}>
                <span style={{ fontWeight: 700, fontSize: '0.9rem' }}>
                  {config.ambiente === 'PR' ? t('envBadgePr') : t('envBadgeQa')}
                </span>
                <span className={`badge ${config.estaListoParaEmitir ? 'badge-success' : 'badge-warning'}`}>
                  {config.estaListoParaEmitir ? '✅ Listo para emitir' : '⚠️ Pendiente de CSD en FiscalAPI'}
                </span>
              </div>
            )}

            {error && <div className="pos-notice pos-notice--error">{error}</div>}
            {success && <div className="pos-notice pos-notice--success" style={{ background: '#d4edda', color: '#155724', padding: '0.5rem', borderRadius: '4px' }}>{success}</div>}

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '0.85rem' }}>
              <label className="pos-field">
                RFC del Emisor *
                <input
                  type="text"
                  className="form-control"
                  required
                  maxLength={13}
                  value={rfc}
                  onChange={e => setRfc(e.target.value.toUpperCase())}
                  placeholder="XAXX010101000"
                  style={{ textTransform: 'uppercase' }}
                />
              </label>

              <label className="pos-field">
                Razón Social / Nombre Fiscal *
                <input
                  type="text"
                  className="form-control"
                  required
                  value={razonSocial}
                  onChange={e => setRazonSocial(e.target.value)}
                  placeholder="WPC BAJIO SA DE CV"
                />
              </label>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '0.85rem' }}>
              <label className="pos-field">
                Régimen Fiscal (SAT) *
                <select
                  className="form-control"
                  value={regimenFiscal}
                  onChange={e => setRegimenFiscal(e.target.value)}
                >
                  <option value="601">601 - General de Ley Personas Morales</option>
                  <option value="612">612 - Personas Físicas con Actividades Empresariales</option>
                  <option value="626">626 - Régimen Simplificado de Confianza (RESICO)</option>
                  <option value="603">603 - Personas Morales con Fines no Lucrativos</option>
                  <option value="621">621 - Incorporación Fiscal</option>
                </select>
              </label>

              <label className="pos-field">
                C.P. de Expedición *
                <input
                  type="text"
                  className="form-control"
                  required
                  maxLength={5}
                  value={codigoPostalExpedicion}
                  onChange={e => setCodigoPostalExpedicion(e.target.value)}
                  placeholder="37000"
                />
              </label>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.85rem' }}>
              <label className="pos-field">
                Serie CFDI
                <input
                  type="text"
                  className="form-control"
                  maxLength={10}
                  value={serie}
                  onChange={e => setSerie(e.target.value.toUpperCase())}
                  placeholder="F"
                />
              </label>

              <label className="pos-field">
                Folio Siguiente
                <input
                  type="number"
                  className="form-control"
                  min={1}
                  value={folioSiguiente}
                  onChange={e => setFolioSiguiente(Number(e.target.value))}
                />
              </label>
            </div>

            <label className="pos-field">
              ID del Emisor en FiscalAPI (Opcional si se vincula directo por RFC)
              <input
                type="text"
                className="form-control"
                value={fiscalapiPersonId}
                onChange={e => setFiscalapiPersonId(e.target.value)}
                placeholder="Identificador asignado por FiscalAPI"
              />
            </label>

            <div style={{
              fontSize: '0.8rem',
              color: 'var(--text-secondary)',
              background: 'var(--background-container)',
              padding: '0.65rem 0.85rem',
              borderRadius: '4px'
            }}>
              ℹ️ Los Certificados de Sello Digital (CSD .cer y .key) y su contraseña deben cargarse directamente en la consola de FiscalAPI según el ambiente (QA: test.fiscalapi.com / PR: live.fiscalapi.com) para garantizar máxima seguridad y evitar almacenar llaves privadas en el PDV.
            </div>

            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end', marginTop: '0.5rem' }}>
              <button type="button" className="pos-receipt-close" onClick={onClose} disabled={saving}>
                {t('cancel')}
              </button>
              <button type="submit" className="action-btn" disabled={saving}>
                {saving ? 'Guardando...' : '💾 Guardar Configuración'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};

export default ConfiguracionEmisorModal;
