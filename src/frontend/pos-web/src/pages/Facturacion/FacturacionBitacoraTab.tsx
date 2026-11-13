import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import { FacturacionLogDto, FacturacionLogFiltros } from '../../types/tiposFacturacion';
import { usePagination } from '../../hooks/usePagination';
import TablePagination from '../../components/common/TablePagination';
import { getOperationalDateInputValue, toOperationalUtcBoundary } from '../../utils/operationalDate';
import { FacturacionLogDetalleModal } from './FacturacionLogDetalleModal';
import './FacturacionPage.css';

const today = getOperationalDateInputValue;

export const FacturacionBitacoraTab: React.FC = () => {
  const { t, i18n } = useTranslation();
  const { hasPermission } = useAuth();
  const canPurgeLogs = hasPermission('facturacion', 'bitacora_purgar') || hasPermission('facturacion', 'configurar');

  const locale = i18n.language.startsWith('zh') ? 'zh-CN' : 'es-MX';

  // Data state
  const [logs, setLogs] = useState<FacturacionLogDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [feedback, setFeedback] = useState('');

  // Filters state
  const [busqueda, setBusqueda] = useState('');
  const [ambiente, setAmbiente] = useState('');
  const [operacion, setOperacion] = useState('');
  const [resultado, setResultado] = useState('');
  const [fechaInicio, setFechaInicio] = useState('');
  const [fechaFin, setFechaFin] = useState('');

  const [appliedFilters, setAppliedFilters] = useState({
    busqueda: '',
    ambiente: '',
    operacion: '',
    resultado: '',
    fechaInicio: '',
    fechaFin: ''
  });

  // Modals state
  const [selectedLog, setSelectedLog] = useState<FacturacionLogDto | null>(null);
  const [showPurgeModal, setShowPurgeModal] = useState(false);
  const [diasRetencion, setDiasRetencion] = useState(90);
  const [purging, setPurging] = useState(false);

  const pagination = usePagination({ initialPageSize: 20 });

  const loadLogs = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const filtros: FacturacionLogFiltros = {
        busqueda: appliedFilters.busqueda || undefined,
        ambiente: appliedFilters.ambiente || undefined,
        operacion: appliedFilters.operacion || undefined,
        resultado: appliedFilters.resultado || undefined,
        fechaInicio: appliedFilters.fechaInicio ? toOperationalUtcBoundary(appliedFilters.fechaInicio) : undefined,
        fechaFin: appliedFilters.fechaFin ? toOperationalUtcBoundary(appliedFilters.fechaFin, true) : undefined,
        page: pagination.pageNumber,
        pageSize: pagination.pageSize
      };

      const res = await servicioFacturacion.obtenerBitacora(filtros);
      setLogs(res.items);
      pagination.setPaginationFromResult(res);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al consultar bitácora de facturación';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [appliedFilters, pagination.pageNumber, pagination.pageSize]);

  useEffect(() => {
    loadLogs();
  }, [loadLogs]);

  const handleApplyFilters = (e: React.FormEvent) => {
    e.preventDefault();
    pagination.setPageNumber(1);
    setAppliedFilters({
      busqueda,
      ambiente,
      operacion,
      resultado,
      fechaInicio,
      fechaFin
    });
  };

  const handleClearFilters = () => {
    setBusqueda('');
    setAmbiente('');
    setOperacion('');
    setResultado('');
    setFechaInicio('');
    setFechaFin('');
    pagination.setPageNumber(1);
    setAppliedFilters({
      busqueda: '',
      ambiente: '',
      operacion: '',
      resultado: '',
      fechaInicio: '',
      fechaFin: ''
    });
  };

  const handlePurge = async () => {
    if (diasRetencion < 30) {
      alert('Por seguridad y trazabilidad legal del SAT, el período mínimo de retención es de 30 días.');
      return;
    }
    setPurging(true);
    setError('');
    try {
      const res = await servicioFacturacion.purgarBitacora(diasRetencion);
      setFeedback(`Purga completada: ${res.registrosEliminados} registros técnicos antiguos eliminados.`);
      setShowPurgeModal(false);
      loadLogs();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al purgar bitácora';
      setError(msg);
    } finally {
      setPurging(false);
    }
  };

  // Metrics calculated from current view
  const metrics = useMemo(() => {
    const total = logs.length;
    const exitosas = logs.filter(l => l.resultado === 'Exitoso').length;
    const rechazadas = logs.filter(l => l.resultado === 'Rechazado').length;
    const fallos = logs.filter(l => l.resultado === 'FalloTecnico' || l.resultado === 'ResultadoIncierto').length;
    const duraciones = logs.filter(l => l.duracionMs !== null && l.duracionMs !== undefined).map(l => l.duracionMs!);
    const latenciaPromedio = duraciones.length > 0 ? Math.round(duraciones.reduce((a, b) => a + b, 0) / duraciones.length) : 0;
    return { total, exitosas, rechazadas, fallos, latenciaPromedio };
  }, [logs]);

  const getResultBadgeClass = (res: string) => {
    switch (res) {
      case 'Exitoso': return 'badge-success';
      case 'Rechazado': return 'badge-warning';
      case 'FalloTecnico': return 'badge-danger';
      case 'ResultadoIncierto': return 'badge-secondary';
      default: return 'badge-secondary';
    }
  };

  const getDurationColor = (ms?: number) => {
    if (!ms) return 'var(--text-secondary)';
    if (ms < 1000) return '#28a745'; // fast
    if (ms < 3000) return '#b27b00'; // moderate
    return '#dc3545'; // slow
  };

  return (
    <div style={{ display: 'grid', gap: '1.25rem' }}>
      {/* Header & Filter Card */}
      <article className="card facturacion-header">
        <div className="facturacion-header__top">
          <div>
            <h3 style={{ margin: 0 }}>📡 Bitácora Técnica de FiscalAPI</h3>
            <p>
              Trazabilidad técnica obligatoria y auditoría profunda de cada llamada realizada al PAC FiscalAPI (QA y PR).
            </p>
          </div>
          <div style={{ display: 'flex', gap: '0.6rem', alignItems: 'center' }}>
            <button
              type="button"
              className="lang-btn"
              onClick={loadLogs}
              title="Recargar registros"
            >
              🔄 Refrescar
            </button>
            {canPurgeLogs && (
              <button
                type="button"
                className="lang-btn"
                onClick={() => setShowPurgeModal(true)}
                style={{ color: 'var(--danger)', borderColor: 'var(--danger)' }}
                title="Depurar registros históricos antiguos"
              >
                🗑️ Purgar Bitácora
              </button>
            )}
          </div>
        </div>

        {/* Filters */}
        <form className="facturacion-filter-form" onSubmit={handleApplyFilters}>
          <input
            type="text"
            className="form-control"
            placeholder="🔎 CorrelationId, UUID, Venta #, o Ruta..."
            value={busqueda}
            onChange={e => setBusqueda(e.target.value)}
          />

          <select
            className="form-control"
            value={operacion}
            onChange={e => setOperacion(e.target.value)}
          >
            <option value="">Todas las Operaciones</option>
            <option value="Emitir">Emitir Factura</option>
            <option value="Consultar">Consultar Comprobante</option>
            <option value="ConsultarEstadoSat">Estado ante el SAT</option>
            <option value="ObtenerXml">Descarga XML</option>
            <option value="GenerarPdf">Generación PDF</option>
            <option value="EnviarCorreo">Envío de Correo</option>
            <option value="Cancelar">Cancelación SAT</option>
            <option value="SincronizarCliente">Sincronizar Receptor</option>
            <option value="ConfigurarEmisor">Configurar Emisor</option>
          </select>

          <select
            className="form-control"
            value={ambiente}
            onChange={e => setAmbiente(e.target.value)}
          >
            <option value="">Todos los Ambientes</option>
            <option value="QA">Ambiente QA (Pruebas)</option>
            <option value="PR">Ambiente PR (Producción)</option>
          </select>

          <select
            className="form-control"
            value={resultado}
            onChange={e => setResultado(e.target.value)}
          >
            <option value="">Todos los Resultados</option>
            <option value="Exitoso">Exitoso</option>
            <option value="Rechazado">Rechazado (Validación/4xx)</option>
            <option value="FalloTecnico">Fallo Técnico (5xx)</option>
            <option value="ResultadoIncierto">Resultado Incierto (Timeout)</option>
          </select>

          <label className="facturacion-date-field">
            <span>Fecha Inicio</span>
            <input
              type="date"
              className="form-control"
              value={fechaInicio}
              onChange={e => setFechaInicio(e.target.value)}
            />
          </label>

          <label className="facturacion-date-field">
            <span>Fecha Fin</span>
            <input
              type="date"
              className="form-control"
              value={fechaFin}
              onChange={e => setFechaFin(e.target.value)}
            />
          </label>

          <button type="submit" className="action-btn">
            🔎 Filtrar
          </button>
          <button type="button" className="lang-btn" onClick={handleClearFilters}>
            Limpiar
          </button>
        </form>
      </article>

      {error && <div className="pos-notice pos-notice--error">{error}</div>}
      {feedback && <div className="pos-notice pos-notice--success" style={{ background: '#d4edda', color: '#155724', padding: '0.6rem 1rem', borderRadius: '4px' }}>{feedback}</div>}

      {/* Metrics */}
      <div className="facturacion-metrics">
        <article className="card">
          <span>📡 Llamadas Registradas</span>
          <strong>{pagination.totalItems || metrics.total}</strong>
        </article>
        <article className="card">
          <span>✅ Respuestas Exitosas</span>
          <strong style={{ color: '#28a745' }}>{metrics.exitosas} (en pág)</strong>
        </article>
        <article className="card">
          <span>⚠️ Rechazos de Proveedor (4xx)</span>
          <strong style={{ color: '#b27b00' }}>{metrics.rechazadas} (en pág)</strong>
        </article>
        <article className="card">
          <span>⏱️ Latencia Promedio</span>
          <strong style={{ color: getDurationColor(metrics.latenciaPromedio) }}>
            {metrics.latenciaPromedio} ms
          </strong>
        </article>
      </div>

      {/* Logs Table */}
      <article className="card facturacion-table-wrap">
        <table className="facturacion-table" style={{ fontSize: '0.84rem' }}>
              <thead>
                <tr>
                  <th>Fecha/Hora (Local)</th>
                  <th>Operación</th>
                  <th>Método / Ruta</th>
                  <th>Amb.</th>
                  <th>Correlation ID</th>
                  <th>Estado</th>
                  <th>Resultado</th>
                  <th>HTTP SAT</th>
                  <th style={{ textAlign: 'right' }}>Latencia</th>
                  <th style={{ textAlign: 'center' }}>Diagnóstico</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr>
                    <td colSpan={10} style={{ textAlign: 'center', padding: '2.5rem' }}>
                      <div style={{ display: 'inline-flex', alignItems: 'center', gap: '0.6rem', color: 'var(--text-secondary)' }}>
                        <span>⏳</span>
                        <span>{t('loading') || 'Cargando registros de bitácora...'}</span>
                      </div>
                    </td>
                  </tr>
                ) : logs.length === 0 ? (
                  <tr>
                    <td colSpan={10} style={{ textAlign: 'center', padding: '2.5rem' }}>
                      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '0.5rem', color: 'var(--text-secondary)' }}>
                        <span style={{ fontSize: '2rem' }}>📡</span>
                        <span>No se encontraron registros de llamadas que coincidan con los filtros.</span>
                      </div>
                    </td>
                  </tr>
                ) : (
                  logs.map(log => (
                    <tr key={log.id}>
                      <td>
                        <span style={{ fontSize: '0.8rem', whiteSpace: 'nowrap' }}>
                          {new Date(log.fechaInicioUtc).toLocaleDateString(locale)} {new Date(log.fechaInicioUtc).toLocaleTimeString(locale, { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                        </span>
                      </td>
                      <td>
                        <strong>{log.operacion}</strong>
                      </td>
                      <td>
                        <code style={{ fontSize: '0.78rem', background: 'rgba(0,0,0,0.05)', padding: '2px 4px', borderRadius: '3px' }}>
                          {log.metodoHttp}
                        </code>{' '}
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{log.ruta}</span>
                      </td>
                      <td>
                        <span className={`facturacion-env-badge facturacion-env-badge--${log.ambiente.toLowerCase()}`} style={{ fontSize: '0.7rem', padding: '0.15rem 0.4rem' }}>
                          {log.ambiente}
                        </span>
                      </td>
                      <td>
                        <span style={{ fontFamily: 'monospace', fontSize: '0.78rem' }} title={log.correlationId}>
                          {log.correlationId ? `${log.correlationId.substring(0, 12)}...` : '—'}
                        </span>
                      </td>
                      <td>
                        <span className="badge badge-secondary" style={{ fontSize: '0.72rem' }}>
                          {log.estadoEjecucion}
                        </span>
                      </td>
                      <td>
                        <span className={`badge ${getResultBadgeClass(log.resultado)}`} style={{ fontSize: '0.72rem' }}>
                          {log.resultado}
                        </span>
                      </td>
                      <td>
                        {log.httpStatusCode !== null && log.httpStatusCode !== undefined ? (
                          <span
                            className={`badge ${log.httpStatusCode >= 200 && log.httpStatusCode < 300 ? 'badge-success' : (log.httpStatusCode >= 400 && log.httpStatusCode < 500 ? 'badge-warning' : 'badge-danger')}`}
                            style={{ fontSize: '0.72rem' }}
                          >
                            {log.httpStatusCode}
                          </span>
                        ) : (
                          <span style={{ color: 'var(--text-secondary)', fontSize: '0.75rem' }} title="Sin respuesta HTTP real (Timeout o Red)">
                            —
                          </span>
                        )}
                      </td>
                      <td style={{ textAlign: 'right', fontWeight: 600, color: getDurationColor(log.duracionMs) }}>
                        {log.duracionMs !== null && log.duracionMs !== undefined ? `${log.duracionMs} ms` : '—'}
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <button
                          type="button"
                          className="facturacion-btn-icon"
                          onClick={() => setSelectedLog(log)}
                          title="Inspeccionar request/response sanitizados y diagnóstico"
                        >
                          🔍
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            <TablePagination
              pageNumber={pagination.pageNumber}
              totalPages={pagination.totalPages}
              totalItems={pagination.totalItems}
              pageSize={pagination.pageSize}
              pageSizeOptions={[10, 20, 50, 100]}
              onPageChange={pagination.setPageNumber}
              onPageSizeChange={pagination.setPageSize}
              disabled={loading}
            />
      </article>

      {/* Detail Modal */}
      {selectedLog && (
        <FacturacionLogDetalleModal
          log={selectedLog}
          onClose={() => setSelectedLog(null)}
          onSelectRelatedLog={log => setSelectedLog(log)}
        />
      )}

      {/* Purge Modal */}
      {showPurgeModal && (
        <div className="modal-backdrop" onClick={() => setShowPurgeModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()} style={{ maxWidth: '480px' }}>
            <header className="modal-header">
              <h3 style={{ margin: 0 }}>🗑️ Purgar Bitácora Técnica Antigua</h3>
              <button type="button" className="close-btn" onClick={() => setShowPurgeModal(false)}>✕</button>
            </header>
            <div className="modal-body" style={{ padding: '1.25rem' }}>
              <p>
                Esta acción eliminará de la base de datos los registros de llamadas a FiscalAPI anteriores al período especificado.
              </p>
              <div style={{ background: '#fff3cd', borderLeft: '4px solid #ffc107', padding: '0.75rem', borderRadius: '4px', fontSize: '0.85rem', marginBottom: '1rem' }}>
                ⚠️ <strong>Aviso legal:</strong> Los comprobantes y facturas emitidas (UUID, XML, PDF) NO se borran. Únicamente se eliminan las trazas de comunicación HTTP técnica para liberar almacenamiento.
              </div>
              <label style={{ display: 'grid', gap: '0.4rem', fontWeight: 600 }}>
                <span>Retener registros de los últimos (días):</span>
                <input
                  type="number"
                  className="form-control"
                  min={30}
                  max={365}
                  value={diasRetencion}
                  onChange={e => setDiasRetencion(parseInt(e.target.value) || 30)}
                />
              </label>
            </div>
            <footer className="modal-footer" style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', padding: '0.75rem 1.25rem' }}>
              <button type="button" className="lang-btn" onClick={() => setShowPurgeModal(false)} disabled={purging}>
                Cancelar
              </button>
              <button type="button" className="action-btn" onClick={handlePurge} disabled={purging} style={{ background: 'var(--danger)', borderColor: 'var(--danger)' }}>
                {purging ? 'Purgando...' : 'Confirmar Purga'}
              </button>
            </footer>
          </div>
        </div>
      )}
    </div>
  );
};
