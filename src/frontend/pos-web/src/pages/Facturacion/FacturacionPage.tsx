import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import { servicioFacturacion } from '../../services/servicioFacturacion';
import {
  FacturaDto,
  FacturaFiltrosDto,
  ConfiguracionEmisorDto
} from '../../types/tiposFacturacion';
import { usePagination } from '../../hooks/usePagination';
import TablePagination from '../../components/common/TablePagination';
import { getOperationalDateInputValue, toOperationalUtcBoundary } from '../../utils/operationalDate';
import ConfiguracionEmisorModal from './ConfiguracionEmisorModal';
import FacturaDetalleModal from './FacturaDetalleModal';
import FacturaCancelarModal from './FacturaCancelarModal';
import FacturaPreviewModal from './FacturaPreviewModal';
import { FacturacionBitacoraTab } from './FacturacionBitacoraTab';
import './FacturacionPage.css';

const today = getOperationalDateInputValue;

export const FacturacionPage: React.FC = () => {
  const { t, i18n } = useTranslation();
  const { hasPermission } = useAuth();
  const canAdminConfig = hasPermission('facturacion', 'configurar');
  const canCancel = hasPermission('facturacion', 'cancelar');
  const canViewLogs = hasPermission('facturacion', 'bitacora_ver') || canAdminConfig;

  const locale = i18n.language.startsWith('zh') ? 'zh-CN' : 'es-MX';
  const money = useMemo(() => new Intl.NumberFormat(locale, { style: 'currency', currency: 'MXN' }), [locale]);

  // Tab State
  const [currentTab, setCurrentTab] = useState<'facturas' | 'bitacora'>('facturas');

  // Data & State
  const [invoices, setInvoices] = useState<FacturaDto[]>([]);
  const [config, setConfig] = useState<ConfiguracionEmisorDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  // Filter state
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('Timbrada');
  const [rfcReceptor, setRfcReceptor] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [appliedFilters, setAppliedFilters] = useState({
    search: '',
    status: 'Timbrada',
    rfcReceptor: '',
    startDate: '',
    endDate: ''
  });

  // Modals state
  const [showConfigModal, setShowConfigModal] = useState(false);
  const [detalleFactura, setDetalleFactura] = useState<FacturaDto | null>(null);
  const [facturaACancelar, setFacturaACancelar] = useState<FacturaDto | null>(null);
  const [previewSaleId, setPreviewSaleId] = useState<string | null>(null);

  const pagination = usePagination({ initialPageSize: 20 });

  // Cargar configuración de emisor una sola vez al montar
  useEffect(() => {
    let mounted = true;
    servicioFacturacion.obtenerConfiguracion()
      .then(configData => {
        if (mounted && configData) setConfig(configData);
      })
      .catch(() => {});
    return () => { mounted = false; };
  }, []);

  const loadInvoices = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const filtros: FacturaFiltrosDto = {
        busqueda: appliedFilters.search || undefined,
        estado: appliedFilters.status || undefined,
        rfcReceptor: appliedFilters.rfcReceptor || undefined,
        fechaInicioUtc: appliedFilters.startDate ? toOperationalUtcBoundary(appliedFilters.startDate) : undefined,
        fechaFinUtc: appliedFilters.endDate ? toOperationalUtcBoundary(appliedFilters.endDate, true) : undefined,
        pageNumber: pagination.pageNumber,
        pageSize: pagination.pageSize
      };

      const res = await servicioFacturacion.obtenerFacturas(filtros);
      setInvoices(res.items);
      pagination.setPaginationFromResult(res);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al cargar facturas';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [appliedFilters, pagination.pageNumber, pagination.pageSize]);

  useEffect(() => {
    loadInvoices();
  }, [loadInvoices]);

  const handleApplyFilters = (e: React.FormEvent) => {
    e.preventDefault();
    pagination.setPageNumber(1);
    setAppliedFilters({
      search,
      status,
      rfcReceptor,
      startDate,
      endDate
    });
  };

  const handleClearFilters = () => {
    setSearch('');
    setStatus('Timbrada');
    setRfcReceptor('');
    setStartDate('');
    setEndDate('');
    pagination.setPageNumber(1);
    setAppliedFilters({
      search: '',
      status: 'Timbrada',
      rfcReceptor: '',
      startDate: '',
      endDate: ''
    });
  };

  const handleDescargarXml = async (factura: FacturaDto) => {
    try {
      await servicioFacturacion.descargarXml(factura.id, `Factura_${factura.serie ? `${factura.serie}_` : ''}${factura.folio}.xml`);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al descargar XML';
      setError(msg);
    }
  };

  const handleDescargarPdf = async (factura: FacturaDto) => {
    try {
      await servicioFacturacion.descargarPdf(factura.id, `Factura_${factura.serie ? `${factura.serie}_` : ''}${factura.folio}.pdf`);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al descargar PDF';
      setError(msg);
    }
  };

  const handleConsultarSat = async (factura: FacturaDto) => {
    try {
      const res = await servicioFacturacion.consultarEstadoSat(factura.id);
      setSuccessMessage(`SAT: ${res.estado || 'Consultado'} — Factura ${factura.folio}`);
      loadInvoices();
      setTimeout(() => setSuccessMessage(''), 4000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al consultar SAT';
      setError(msg);
    }
  };

  // Metrics calculation
  const metrics = useMemo(() => {
    const totalCount = pagination.totalItems;
    const stampedInvoices = invoices.filter(f => f.estado === 'Timbrada');
    const cancelledInvoices = invoices.filter(f => f.estado === 'Cancelada');
    const totalStamped = stampedInvoices.reduce((sum, f) => sum + f.total, 0);
    const totalCancelled = cancelledInvoices.reduce((sum, f) => sum + f.total, 0);

    return {
      totalCount,
      stampedCount: stampedInvoices.length,
      cancelledCount: cancelledInvoices.length,
      totalStamped,
      totalCancelled
    };
  }, [invoices, pagination.totalItems]);

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
    <section className="facturacion-page">
      <div className="facturacion-tabs">
        <button
          type="button"
          className={currentTab === 'facturas' ? 'is-active' : ''}
          onClick={() => setCurrentTab('facturas')}
        >
          🧾 {t('invoices') || 'Facturas Emitidas'}
        </button>
        {canViewLogs && (
          <button
            type="button"
            className={currentTab === 'bitacora' ? 'is-active' : ''}
            onClick={() => setCurrentTab('bitacora')}
          >
            📡 {t('invoicingLogs') || 'Bitácora Técnica FiscalAPI'}
          </button>
        )}
      </div>

      {currentTab === 'bitacora' ? (
        <FacturacionBitacoraTab />
      ) : (
        <>
          <article className="card facturacion-header">
        <div className="facturacion-header__top">
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap' }}>
              <h2>🧾 {t('invoicingTitle')}</h2>
              {config && (
                <span className={`facturacion-env-badge ${config.ambiente === 'PR' ? 'facturacion-env-badge--pr' : 'facturacion-env-badge--qa'}`}>
                  {config.ambiente === 'PR' ? t('envBadgePr') : t('envBadgeQa')}
                </span>
              )}
            </div>
            <p>{t('invoicingSubtitle')}</p>
          </div>

          <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
            <button
              type="button"
              className="lang-btn"
              onClick={() => loadInvoices()}
              disabled={loading}
            >
              🔄 {t('refresh') || 'Actualizar'}
            </button>
            {canAdminConfig && (
              <button
                type="button"
                className="action-btn"
                onClick={() => setShowConfigModal(true)}
              >
                ⚙️ {t('issuerConfig')}
              </button>
            )}
          </div>
        </div>

        {/* Filter bar */}
        <form className="facturacion-filter-form" onSubmit={handleApplyFilters}>
          <input
            className="form-control"
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder={t('searchInvoicePlaceholder')}
          />
          <select
            className="form-control"
            value={status}
            onChange={e => setStatus(e.target.value)}
          >
            <option value="Timbrada">✅ {t('statusStamped') || 'Timbradas'}</option>
            <option value="Cancelada">🚫 {t('statusCancelled') || 'Canceladas'}</option>
            <option value="CancelacionSolicitada">⏳ {t('statusCancelRequested') || 'Cancelación Solicitada'}</option>
            <option value="ResultadoPorConfirmar">⚠️ {t('statusConfirmPending') || 'Por Confirmar'}</option>
            <option value="">{t('allInvoiceStatuses') || 'Todas las Emitidas'}</option>
          </select>
          <input
            className="form-control"
            style={{ flex: '1 1 150px' }}
            value={rfcReceptor}
            onChange={e => setRfcReceptor(e.target.value.toUpperCase())}
            placeholder="RFC Receptor..."
          />
          <label className="facturacion-date-field">
            <span>{t('startDate')}</span>
            <input
              type="date"
              className="form-control"
              value={startDate}
              onChange={e => setStartDate(e.target.value)}
            />
          </label>
          <label className="facturacion-date-field">
            <span>{t('endDate')}</span>
            <input
              type="date"
              className="form-control"
              value={endDate}
              onChange={e => setEndDate(e.target.value)}
            />
          </label>
          <button type="submit" className="action-btn">
            🔎 {t('search')}
          </button>
          <button type="button" className="lang-btn" onClick={handleClearFilters}>
            {t('clearFilters')}
          </button>
        </form>
      </article>

      {error && <div className="pos-notice pos-notice--error">{error}</div>}
      {successMessage && <div className="pos-notice pos-notice--success" style={{ background: '#d4edda', color: '#155724', padding: '0.6rem 1rem', borderRadius: '4px' }}>{successMessage}</div>}

      {/* Metrics Cards */}
      <div className="facturacion-metrics">
        <article className="card">
          <span>🧾 {t('totalInvoices')}</span>
          <strong>{metrics.totalCount}</strong>
        </article>
        <article className="card">
          <span>✅ Facturas Timbradas</span>
          <strong style={{ color: '#28a745' }}>{metrics.stampedCount}</strong>
        </article>
        <article className="card">
          <span>💰 {t('stampedTotal')} (Pág)</span>
          <strong>{money.format(metrics.totalStamped)}</strong>
        </article>
        <article className="card">
          <span>🚫 {t('cancelledTotal')} (Pág)</span>
          <strong style={{ color: 'var(--danger)' }}>{money.format(metrics.totalCancelled)}</strong>
        </article>
      </div>

      {/* Invoices Table */}
      <article className="card facturacion-table-wrap">
        <table className="facturacion-table">
          <thead>
            <tr>
              <th>{t('invoiceFolio')}</th>
              <th>{t('uuidSat')}</th>
              <th>{t('saleNumberRef')}</th>
              <th>{t('emissionDate')}</th>
              <th>{t('customer')} (RFC / Razón Social)</th>
              <th style={{ textAlign: 'right' }}>{t('total')}</th>
              <th>{t('status')}</th>
              <th>SAT</th>
              <th style={{ textAlign: 'center' }}>{t('actions')}</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={9} style={{ textAlign: 'center', padding: '2.5rem' }}>
                  <div style={{ display: 'inline-flex', alignItems: 'center', gap: '0.6rem', color: 'var(--text-secondary)' }}>
                    <span>⏳</span>
                    <span>{t('loading') || 'Cargando facturas emitidas...'}</span>
                  </div>
                </td>
              </tr>
            ) : invoices.length === 0 ? (
              <tr>
                <td colSpan={9} style={{ textAlign: 'center', padding: '2.5rem' }}>
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '0.5rem', color: 'var(--text-secondary)' }}>
                    <span style={{ fontSize: '2rem' }}>🧾</span>
                    <span>No se encontraron facturas registradas.</span>
                  </div>
                </td>
              </tr>
            ) : (
              invoices.map(f => (
                <tr key={f.id}>
                  <td>
                    <strong>{f.serie ? `${f.serie}-` : ''}{f.folio}</strong>
                  </td>
                  <td>
                    {f.uuid ? (
                      <span style={{ fontFamily: 'monospace', fontSize: '0.8rem' }} title={f.uuid}>
                        {f.uuid.substring(0, 8)}...{f.uuid.substring(f.uuid.length - 4)}
                      </span>
                    ) : (
                      <span style={{ color: 'var(--text-secondary)' }}>—</span>
                    )}
                  </td>
                  <td>
                    <span className="badge badge-secondary" style={{ fontSize: '0.75rem' }}>
                      #{f.idVenta}
                    </span>
                  </td>
                  <td>{new Date(f.fechaEmisionUtc).toLocaleDateString(locale)}</td>
                  <td>
                    <div><strong>{f.rfcReceptor}</strong></div>
                    <small style={{ color: 'var(--text-secondary)' }}>{f.razonSocialReceptor}</small>
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <strong>{money.format(f.total)}</strong>
                  </td>
                  <td>
                    <span className={`badge ${estadoBadgeClass(f.estado)}`}>
                      {f.estado}
                    </span>
                  </td>
                  <td>
                    {f.estadoSat ? (
                      <span className={`badge ${f.estadoSat === 'Vigente' ? 'badge-success' : 'badge-danger'}`} style={{ fontSize: '0.7rem' }}>
                        {f.estadoSat}
                      </span>
                    ) : (
                      <span style={{ color: 'var(--text-secondary)', fontSize: '0.75rem' }}>—</span>
                    )}
                  </td>
                  <td>
                    <div className="facturacion-actions-cell" style={{ justifyContent: 'center' }}>
                      <button
                        type="button"
                        className="facturacion-btn-icon"
                        onClick={() => setDetalleFactura(f)}
                        title={t('viewInvoiceDetail')}
                      >
                        👁️
                      </button>
                      {f.estado === 'Timbrada' && (
                        <>
                          <button
                            type="button"
                            className="facturacion-btn-icon"
                            onClick={() => handleDescargarPdf(f)}
                            title={t('downloadPdf')}
                          >
                            📄
                          </button>
                          <button
                            type="button"
                            className="facturacion-btn-icon"
                            onClick={() => handleDescargarXml(f)}
                            title={t('downloadXml')}
                          >
                            📥
                          </button>
                        </>
                      )}
                      <button
                        type="button"
                        className="facturacion-btn-icon"
                        onClick={() => handleConsultarSat(f)}
                        title={t('satStatusQuery')}
                      >
                        🔄
                      </button>
                      {canCancel && f.estado === 'Timbrada' && (
                        <button
                          type="button"
                          className="facturacion-btn-icon"
                          style={{ color: 'var(--danger)' }}
                          onClick={() => setFacturaACancelar(f)}
                          title={t('cancelInvoice')}
                        >
                          🚫
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        <TablePagination
          pageNumber={pagination.pageNumber}
          pageSize={pagination.pageSize}
          totalItems={pagination.totalItems}
          totalPages={pagination.totalPages}
          onPageChange={pagination.setPageNumber}
          onPageSizeChange={pagination.setPageSize}
          disabled={loading}
        />
      </article>
      </>
      )}

      {/* Modals */}
      {showConfigModal && (
        <ConfiguracionEmisorModal
          onClose={() => setShowConfigModal(false)}
          onSaved={() => {
            servicioFacturacion.obtenerConfiguracion().then(c => setConfig(c)).catch(() => {});
          }}
        />
      )}

      {detalleFactura && (
        <FacturaDetalleModal
          factura={detalleFactura}
          onClose={() => setDetalleFactura(null)}
          onRefresh={() => {
            servicioFacturacion.obtenerFacturaPorId(detalleFactura.id).then(f => setDetalleFactura(f)).catch(() => {});
            loadInvoices();
          }}
          onOpenCancelar={f => {
            setDetalleFactura(null);
            setFacturaACancelar(f);
          }}
        />
      )}

      {facturaACancelar && (
        <FacturaCancelarModal
          factura={facturaACancelar}
          onClose={() => setFacturaACancelar(null)}
          onCancelled={() => {
            setSuccessMessage(`Factura ${facturaACancelar.folio} cancelada exitosamente.`);
            loadInvoices();
            setTimeout(() => setSuccessMessage(''), 4000);
          }}
        />
      )}

      {previewSaleId && (
        <FacturaPreviewModal
          saleId={previewSaleId}
          onClose={() => setPreviewSaleId(null)}
          onSuccess={() => {
            setSuccessMessage('Factura timbrada exitosamente.');
            loadInvoices();
            setTimeout(() => setSuccessMessage(''), 4000);
          }}
        />
      )}
    </section>
  );
};

export default FacturacionPage;
