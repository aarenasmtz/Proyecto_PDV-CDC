import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FacturacionLogDto } from '../../types/tiposFacturacion';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import './FacturacionPage.css';

interface Props {
  log: FacturacionLogDto;
  onClose: () => void;
  onSelectRelatedLog?: (log: FacturacionLogDto) => void;
}

export const FacturacionLogDetalleModal: React.FC<Props> = ({
  log,
  onClose,
  onSelectRelatedLog
}) => {
  const { t, i18n } = useTranslation();
  const locale = i18n.language.startsWith('zh') ? 'zh-CN' : 'es-MX';

  const [activeTab, setActiveTab] = useState<'diagnostico' | 'request' | 'response' | 'relacionadas'>('diagnostico');
  const [relatedLogs, setRelatedLogs] = useState<FacturacionLogDto[]>([]);
  const [loadingRelated, setLoadingRelated] = useState(false);
  const [copiedKey, setCopiedKey] = useState<string | null>(null);

  useEffect(() => {
    if (log.correlationId) {
      setLoadingRelated(true);
      servicioFacturacion.obtenerLlamadasRelacionadas(log.correlationId)
        .then(res => setRelatedLogs(res))
        .catch(() => setRelatedLogs([]))
        .finally(() => setLoadingRelated(false));
    }
  }, [log.correlationId]);

  const copyToClipboard = (text: string | undefined, key: string) => {
    if (!text) return;
    navigator.clipboard.writeText(text);
    setCopiedKey(key);
    setTimeout(() => setCopiedKey(null), 2000);
  };

  const formatJson = (raw?: string | null) => {
    if (!raw) return '—';
    try {
      const parsed = JSON.parse(raw);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return raw;
    }
  };

  const getResultBadgeClass = (resultado: string) => {
    switch (resultado) {
      case 'Exitoso': return 'badge-success';
      case 'Rechazado': return 'badge-warning';
      case 'FalloTecnico': return 'badge-danger';
      case 'ResultadoIncierto': return 'badge-secondary';
      default: return 'badge-secondary';
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="modal modal-lg" onClick={e => e.stopPropagation()} style={{ maxWidth: '900px', maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
        <header className="modal-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <h3 style={{ margin: 0, display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
              🔍 Detalle de Llamada FiscalAPI
              <span className={`badge ${getResultBadgeClass(log.resultado)}`}>{log.resultado}</span>
              <span className={`facturacion-env-badge facturacion-env-badge--${log.ambiente.toLowerCase()}`} style={{ fontSize: '0.75rem' }}>
                {log.ambiente}
              </span>
            </h3>
            <small style={{ color: 'var(--text-secondary)', fontFamily: 'monospace' }}>
              Correlation ID: {log.correlationId}
            </small>
          </div>
          <button type="button" className="close-btn" onClick={onClose} aria-label="Cerrar">✕</button>
        </header>

        {/* Modal Tabs */}
        <div style={{ display: 'flex', gap: '0.5rem', borderBottom: '1px solid var(--border-color)', padding: '0.5rem 1.25rem 0' }}>
          <button
            type="button"
            className={`tab-btn ${activeTab === 'diagnostico' ? 'is-active' : ''}`}
            onClick={() => setActiveTab('diagnostico')}
            style={{
              padding: '0.5rem 1rem',
              border: 0,
              borderBottom: activeTab === 'diagnostico' ? '2px solid var(--accent-primary)' : '2px solid transparent',
              background: 'transparent',
              fontWeight: 600,
              cursor: 'pointer',
              color: activeTab === 'diagnostico' ? 'var(--accent-primary)' : 'var(--text-secondary)'
            }}
          >
            📋 Resumen Técnico
          </button>
          <button
            type="button"
            className={`tab-btn ${activeTab === 'request' ? 'is-active' : ''}`}
            onClick={() => setActiveTab('request')}
            style={{
              padding: '0.5rem 1rem',
              border: 0,
              borderBottom: activeTab === 'request' ? '2px solid var(--accent-primary)' : '2px solid transparent',
              background: 'transparent',
              fontWeight: 600,
              cursor: 'pointer',
              color: activeTab === 'request' ? 'var(--accent-primary)' : 'var(--text-secondary)'
            }}
          >
            📤 Request Sanitizado
          </button>
          <button
            type="button"
            className={`tab-btn ${activeTab === 'response' ? 'is-active' : ''}`}
            onClick={() => setActiveTab('response')}
            style={{
              padding: '0.5rem 1rem',
              border: 0,
              borderBottom: activeTab === 'response' ? '2px solid var(--accent-primary)' : '2px solid transparent',
              background: 'transparent',
              fontWeight: 600,
              cursor: 'pointer',
              color: activeTab === 'response' ? 'var(--accent-primary)' : 'var(--text-secondary)'
            }}
          >
            📥 Response Sanitizado
          </button>
          <button
            type="button"
            className={`tab-btn ${activeTab === 'relacionadas' ? 'is-active' : ''}`}
            onClick={() => setActiveTab('relacionadas')}
            style={{
              padding: '0.5rem 1rem',
              border: 0,
              borderBottom: activeTab === 'relacionadas' ? '2px solid var(--accent-primary)' : '2px solid transparent',
              background: 'transparent',
              fontWeight: 600,
              cursor: 'pointer',
              color: activeTab === 'relacionadas' ? 'var(--accent-primary)' : 'var(--text-secondary)'
            }}
          >
            🔗 Cadena de Llamadas ({relatedLogs.length})
          </button>
        </div>

        <div className="modal-body" style={{ overflowY: 'auto', padding: '1.25rem', flex: 1 }}>
          {activeTab === 'diagnostico' && (
            <div style={{ display: 'grid', gap: '1rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '0.75rem', background: 'var(--surface-sunken)', padding: '1rem', borderRadius: '6px' }}>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Operación / Método</span>
                  <div style={{ fontWeight: 600 }}>
                    <code style={{ background: 'rgba(0,0,0,0.06)', padding: '2px 4px', borderRadius: '3px' }}>{log.metodoHttp}</code> {log.operacion}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Ruta Relativa</span>
                  <div style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>{log.ruta}</div>
                </div>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Código HTTP Recibido</span>
                  <div>
                    {log.httpStatusCode !== null && log.httpStatusCode !== undefined ? (
                      <span className={`badge ${log.httpStatusCode >= 200 && log.httpStatusCode < 300 ? 'badge-success' : 'badge-danger'}`}>
                        HTTP {log.httpStatusCode}
                      </span>
                    ) : (
                      <span style={{ color: 'var(--text-secondary)', fontStyle: 'italic' }}>Sin respuesta HTTP (Timeout / Red)</span>
                    )}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Duración de la Llamada</span>
                  <div style={{ fontWeight: 600 }}>
                    {log.duracionMs !== null && log.duracionMs !== undefined ? `${log.duracionMs} ms` : '—'}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Fecha / Hora (Local)</span>
                  <div style={{ fontSize: '0.85rem' }}>
                    {new Date(log.fechaInicioUtc).toLocaleString(locale)}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Estado de Ejecución</span>
                  <div>
                    <span className="badge badge-secondary">{log.estadoEjecucion}</span>
                  </div>
                </div>
              </div>

              {/* Identificadores */}
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '0.5rem', border: '1px solid var(--border-color)', padding: '0.75rem', borderRadius: '6px' }}>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>Idempotency Key:</span>
                  <div style={{ fontFamily: 'monospace', fontSize: '0.8rem', wordBreak: 'break-all' }}>
                    {log.idempotencyKey || '—'}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>UUID Fiscal SAT:</span>
                  <div style={{ fontFamily: 'monospace', fontSize: '0.8rem', wordBreak: 'break-all' }}>
                    {log.uuidFiscal || '—'}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>Trace Identifier Proveedor:</span>
                  <div style={{ fontFamily: 'monospace', fontSize: '0.8rem', wordBreak: 'break-all' }}>
                    {log.traceIdentifierProveedor || '—'}
                  </div>
                </div>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>Identificador Externo:</span>
                  <div style={{ fontFamily: 'monospace', fontSize: '0.8rem', wordBreak: 'break-all' }}>
                    {log.identificadorExterno || '—'}
                  </div>
                </div>
              </div>

              {/* Mensajes y Diagnóstico */}
              {log.mensaje && (
                <div style={{ background: '#f8f9fa', borderLeft: '4px solid #17a2b8', padding: '0.75rem 1rem', borderRadius: '4px' }}>
                  <strong>Mensaje:</strong>
                  <div style={{ marginTop: '0.25rem' }}>{log.mensaje}</div>
                </div>
              )}

              {log.detalleError && (
                <div style={{ background: '#fff3cd', borderLeft: '4px solid #ffc107', padding: '0.75rem 1rem', borderRadius: '4px' }}>
                  <strong style={{ color: '#856404' }}>Detalle de Error / Validación:</strong>
                  <div style={{ marginTop: '0.25rem', fontFamily: 'monospace', fontSize: '0.85rem', whiteSpace: 'pre-wrap' }}>
                    {log.detalleError}
                  </div>
                </div>
              )}

              {log.tipoExcepcion && (
                <div style={{ background: '#f8d7da', borderLeft: '4px solid #dc3545', padding: '0.75rem 1rem', borderRadius: '4px' }}>
                  <strong style={{ color: '#721c24' }}>Excepción Técnica: {log.tipoExcepcion}</strong>
                  {log.stackTraceSanitizado && (
                    <pre style={{ margin: '0.5rem 0 0', maxHeight: '160px', overflowY: 'auto', fontSize: '0.75rem', background: '#fff', padding: '0.5rem', borderRadius: '4px', border: '1px solid #f5c6cb' }}>
                      {log.stackTraceSanitizado}
                    </pre>
                  )}
                </div>
              )}
            </div>
          )}

          {activeTab === 'request' && (
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Payload HTTP enviado (Secretos y Base64 redactados):
                </span>
                <button
                  type="button"
                  className="lang-btn"
                  onClick={() => copyToClipboard(log.requestSanitizadoJson, 'req')}
                  disabled={!log.requestSanitizadoJson}
                >
                  {copiedKey === 'req' ? '✓ Copiado' : '📋 Copiar Request'}
                </button>
              </div>
              <pre style={{ background: '#1e1e1e', color: '#d4d4d4', padding: '1rem', borderRadius: '6px', fontSize: '0.82rem', maxHeight: '450px', overflow: 'auto', margin: 0 }}>
                {formatJson(log.requestSanitizadoJson)}
              </pre>
            </div>
          )}

          {activeTab === 'response' && (
            <div>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Cuerpo HTTP de respuesta recibido de FiscalAPI:
                </span>
                <button
                  type="button"
                  className="lang-btn"
                  onClick={() => copyToClipboard(log.responseSanitizadoJson, 'res')}
                  disabled={!log.responseSanitizadoJson}
                >
                  {copiedKey === 'res' ? '✓ Copiado' : '📋 Copiar Response'}
                </button>
              </div>
              <pre style={{ background: '#1e1e1e', color: '#d4d4d4', padding: '1rem', borderRadius: '6px', fontSize: '0.82rem', maxHeight: '450px', overflow: 'auto', margin: 0 }}>
                {formatJson(log.responseSanitizadoJson)}
              </pre>
            </div>
          )}

          {activeTab === 'relacionadas' && (
            <div>
              <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.75rem' }}>
                Llamadas vinculadas bajo el mismo Correlation ID ({log.correlationId}):
              </p>
              {loadingRelated ? (
                <p>Cargando llamadas relacionadas...</p>
              ) : relatedLogs.length === 0 ? (
                <p>No se encontraron otras llamadas con este identificador de correlación.</p>
              ) : (
                <table className="facturacion-table" style={{ fontSize: '0.82rem' }}>
                  <thead>
                    <tr>
                      <th>Hora</th>
                      <th>Operación</th>
                      <th>Método / Ruta</th>
                      <th>Estado</th>
                      <th>HTTP</th>
                      <th>Duración</th>
                      <th>Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {relatedLogs.map(r => (
                      <tr key={r.id} style={{ background: r.id === log.id ? 'var(--background-selected)' : undefined }}>
                        <td>{new Date(r.fechaInicioUtc).toLocaleTimeString(locale)}</td>
                        <td><strong>{r.operacion}</strong></td>
                        <td><code>{r.metodoHttp}</code> {r.ruta}</td>
                        <td>
                          <span className={`badge ${getResultBadgeClass(r.resultado)}`} style={{ fontSize: '0.7rem' }}>
                            {r.resultado}
                          </span>
                        </td>
                        <td>{r.httpStatusCode !== null && r.httpStatusCode !== undefined ? `HTTP ${r.httpStatusCode}` : '—'}</td>
                        <td>{r.duracionMs ? `${r.duracionMs} ms` : '—'}</td>
                        <td>
                          {r.id !== log.id && onSelectRelatedLog && (
                            <button
                              type="button"
                              className="lang-btn"
                              style={{ padding: '0.2rem 0.5rem', fontSize: '0.75rem' }}
                              onClick={() => onSelectRelatedLog(r)}
                            >
                              Ver
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </div>

        <footer className="modal-footer" style={{ borderTop: '1px solid var(--border-color)', padding: '0.75rem 1.25rem', display: 'flex', justifyContent: 'flex-end' }}>
          <button type="button" className="action-btn" onClick={onClose}>
            {t('close') || 'Cerrar'}
          </button>
        </footer>
      </div>
    </div>
  );
};

