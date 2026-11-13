import React, { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { servicioNotasVenta } from '../../services/servicioNotasVenta';
import {
  NotaVentaListItem,
  FiltrosNotasVenta
} from '../../types/tiposNotasVenta';
import './NotasVentaPage.css';

export const NotasVentaPage: React.FC = () => {
  const { t, i18n } = useTranslation();
  const isZh = i18n.language === 'zh';

  // Estados de datos
  const [ventas, setVentas] = useState<NotaVentaListItem[]>([]);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Estados de filtros
  const [search, setSearch] = useState<string>('');
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [saleStatus, setSaleStatus] = useState<string>('ALL');
  const [pageNumber, setPageNumber] = useState<number>(1);
  const pageSize = 25;

  // Estados de modales
  const [previewVentaId, setPreviewVentaId] = useState<string | null>(null);
  const [previewFolio, setPreviewFolio] = useState<string>('');
  const [previewPdfUrl, setPreviewPdfUrl] = useState<string | null>(null);
  const [loadingPreview, setLoadingPreview] = useState<boolean>(false);

  // Cargar ventas
  const cargarVentas = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const filtros: FiltrosNotasVenta = {
        search: search.trim() || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        saleStatus,
        pageNumber,
        pageSize
      };
      const res = await servicioNotasVenta.getVentas(filtros);
      setVentas(res.items);
      setTotalCount(res.totalItems ?? res.totalCount ?? 0);
    } catch (err: any) {
      setError(err?.response?.data?.message || err?.message || 'Error al consultar notas de venta.');
    } finally {
      setLoading(false);
    }
  }, [search, startDate, endDate, saleStatus, pageNumber]);

  useEffect(() => {
    cargarVentas();
  }, [cargarVentas]);

  // Manejar búsqueda y limpieza
  const handleBuscar = (e: React.FormEvent) => {
    e.preventDefault();
    setPageNumber(1);
    cargarVentas();
  };

  const handleLimpiarFiltros = () => {
    setSearch('');
    setStartDate('');
    setEndDate('');
    setSaleStatus('ALL');
    setPageNumber(1);
  };

  // Abrir vista previa PDF
  const handleVerVistaPrevia = async (venta: NotaVentaListItem) => {
    const folioStr = venta.idVenta > 0 ? venta.idVenta.toString() : venta.folio;
    setPreviewVentaId(venta.id);
    setPreviewFolio(folioStr);
    setLoadingPreview(true);
    try {
      const blob = await servicioNotasVenta.obtenerBlobPdf(venta.id);
      const url = window.URL.createObjectURL(blob);
      setPreviewPdfUrl(url);
    } catch (err: any) {
      alert(err?.response?.data?.message || 'Error al generar la vista previa del PDF.');
      setPreviewVentaId(null);
    } finally {
      setLoadingPreview(false);
    }
  };

  const handleCerrarVistaPrevia = () => {
    if (previewPdfUrl) {
      window.URL.revokeObjectURL(previewPdfUrl);
    }
    setPreviewPdfUrl(null);
    setPreviewVentaId(null);
    setPreviewFolio('');
  };

  // Descarga directa
  const handleDescargar = async (venta: NotaVentaListItem) => {
    try {
      const folioStr = venta.idVenta > 0 ? venta.idVenta.toString() : venta.folio;
      await servicioNotasVenta.descargarPdf(venta.id, folioStr);
    } catch (err: any) {
      alert('Error al descargar el PDF: ' + (err?.response?.data?.message || err?.message));
    }
  };

  // Imprimir directo
  const handleImprimir = async (venta: NotaVentaListItem) => {
    try {
      await servicioNotasVenta.imprimirPdf(venta.id);
    } catch (err: any) {
      alert('Error al imprimir el documento: ' + (err?.response?.data?.message || err?.message));
    }
  };

  // Cálculos de métricas
  const totalPaginas = Math.ceil(totalCount / pageSize) || 1;
  const totalMontoPagina = ventas.reduce((acc, v) => acc + v.total, 0);

  const renderEstadoBadge = (estado: string) => {
    const norm = (estado || '').toLowerCase();
    if (norm === 'completada' || norm === 'completed') {
      return (
        <span className="badge-estado badge-completada" title="Venta completada">
          ✔ {isZh ? '已完成' : 'Completada'}
        </span>
      );
    }
    if (norm === 'cancelada' || norm === 'cancelled') {
      return (
        <span className="badge-estado badge-cancelada" title="Venta cancelada">
          ✕ {isZh ? '已取消' : 'Cancelada'}
        </span>
      );
    }
    if (norm === 'apartadopagado' || norm === 'depositpaid') {
      return (
        <span className="badge-estado badge-apartado" title="Venta con anticipo / apartado">
          📦 {isZh ? '定金已付' : 'Con Anticipo'}
        </span>
      );
    }
    if (norm === 'pendientepago' || norm === 'pendingpayment') {
      return (
        <span className="badge-estado badge-pendiente" title="Venta pendiente de pago">
          ⏳ {isZh ? '待付款' : 'Pendiente Pago'}
        </span>
      );
    }
    if (norm === 'devolucionparcial' || norm === 'partiallyreturned') {
      return (
        <span className="badge-estado badge-devolucion" title="Devolución parcial">
          ↩ {isZh ? '部分退货' : 'Dev. Parcial'}
        </span>
      );
    }
    if (norm === 'devuelta' || norm === 'returned') {
      return (
        <span className="badge-estado badge-devolucion" title="Venta devuelta">
          ↩ {isZh ? '全部退货' : 'Devuelta'}
        </span>
      );
    }
    return <span className="badge-estado badge-general">{estado || '—'}</span>;
  };

  return (
    <div className="notas-venta-container">
      {/* Encabezado Principal */}
      <div className="notas-venta-header">
        <div className="notas-venta-title-group">
          <h2>
            <span>📋</span> {isZh ? '销售单据管理 — WPC Bajío' : 'Notas de Venta — WPC Bajío'}
          </h2>
          <p className="notas-venta-subtitle">
            {isZh
              ? '商业销售小票与出货单据生成、预览与打印（无税务效力，独立于FiscalAPI）'
              : 'Emisión, consulta, reimpresión y comprobantes comerciales sin efectos fiscales (independiente de FiscalAPI)'}
          </p>
        </div>
        <div>
          <button className="btn-secondary-wpc" onClick={cargarVentas} disabled={loading}>
            🔄 {isZh ? '刷新列表' : 'Actualizar'}
          </button>
        </div>
      </div>

      {/* Tarjetas de Métricas Rápidas */}
      <div className="notas-stats-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))' }}>
        <div className="notas-stat-card">
          <div className="notas-stat-icon">📄</div>
          <div>
            <div className="notas-stat-label">{isZh ? '总单据数' : 'Total Notas de Venta'}</div>
            <div className="notas-stat-value">{totalCount}</div>
          </div>
        </div>

        <div className="notas-stat-card">
          <div className="notas-stat-icon">📑</div>
          <div>
            <div className="notas-stat-label">{isZh ? '当前页记录' : 'Notas en Página'}</div>
            <div className="notas-stat-value" style={{ color: '#6F3215' }}>{ventas.length}</div>
          </div>
        </div>

        <div className="notas-stat-card">
          <div className="notas-stat-icon">💵</div>
          <div>
            <div className="notas-stat-label">{isZh ? '当前页总额' : 'Total en Página'}</div>
            <div className="notas-stat-value" style={{ color: '#9C4D22' }}>
              ${totalMontoPagina.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </div>
          </div>
        </div>
      </div>

      {/* Barra de Filtros */}
      <div className="notas-filters-card">
        <form onSubmit={handleBuscar} className="notas-filters-grid">
          <div className="notas-filter-group">
            <label>{isZh ? '搜索单号 / 客户' : 'Buscar por Folio, Cliente o RFC'}</label>
            <input
              type="text"
              className="notas-filter-input"
              placeholder={isZh ? '例如：25, Juan Pérez, RFC...' : 'Ej. 25, Juan Pérez, RFC...'}
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </div>

          <div className="notas-filter-group">
            <label>{isZh ? '销售状态' : 'Estado de Venta'}</label>
            <select
              className="notas-filter-select"
              value={saleStatus}
              onChange={e => setSaleStatus(e.target.value)}
            >
              <option value="ALL">{isZh ? '所有状态' : 'Todos los estados'}</option>
              <option value="Completada">{isZh ? '✔ 已完成' : '✔ Completada'}</option>
              <option value="ApartadoPagado">{isZh ? '📦 定金已付' : '📦 Con Anticipo'}</option>
              <option value="PendientePago">{isZh ? '⏳ 待付款' : '⏳ Pendiente de Pago'}</option>
              <option value="Cancelada">{isZh ? '✕ 已取消' : '✕ Cancelada'}</option>
              <option value="DevolucionParcial">{isZh ? '↩ 部分退货' : '↩ Devolución Parcial'}</option>
              <option value="Devuelta">{isZh ? '↩ 全部退货' : '↩ Devuelta'}</option>
            </select>
          </div>

          <div className="notas-filter-group">
            <label>{isZh ? '起始日期' : 'Fecha Inicial'}</label>
            <input
              type="date"
              className="notas-filter-input"
              value={startDate}
              onChange={e => setStartDate(e.target.value)}
            />
          </div>

          <div className="notas-filter-group">
            <label>{isZh ? '截止日期' : 'Fecha Final'}</label>
            <input
              type="date"
              className="notas-filter-input"
              value={endDate}
              onChange={e => setEndDate(e.target.value)}
            />
          </div>

          <div className="notas-filter-actions">
            <button type="submit" className="btn-primary-wpc">
              🔍 {isZh ? '查询' : 'Buscar'}
            </button>
            <button type="button" className="btn-secondary-wpc" onClick={handleLimpiarFiltros}>
              ✖ {isZh ? '重置' : 'Limpiar'}
            </button>
          </div>
        </form>
      </div>

      {/* Alerta de Error */}
      {error && (
        <div style={{ background: '#FFEBEE', color: '#C62828', padding: '0.85rem', borderRadius: '6px', marginBottom: '1rem', border: '1px solid #FFCDD2' }}>
          <strong>Error:</strong> {error}
        </div>
      )}

      {/* Tabla Principal */}
      <div className="notas-table-card">
        <div className="notas-table-responsive">
          <table className="notas-table">
            <thead>
              <tr>
                <th>{isZh ? '单号 (Folio)' : 'Folio'}</th>
                <th>{isZh ? '日期' : 'Fecha'}</th>
                <th>{isZh ? '客户' : 'Cliente'}</th>
                <th style={{ textAlign: 'center' }}>{isZh ? '商品件数' : 'Productos'}</th>
                <th style={{ textAlign: 'right' }}>{isZh ? '金额' : 'Total'}</th>
                <th style={{ textAlign: 'center' }}>{isZh ? '状态' : 'Estado'}</th>
                <th style={{ textAlign: 'center' }}>{isZh ? '操作' : 'Acciones'}</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={7} style={{ textAlign: 'center', padding: '2rem' }}>
                    ⏳ {isZh ? '正在加载销售单据...' : 'Cargando ventas y notas comerciales...'}
                  </td>
                </tr>
              ) : ventas.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <div className="empty-state">
                      <div className="empty-state-icon">📋</div>
                      <div>{isZh ? '未找到符合条件的销售记录' : 'No se encontraron ventas con los filtros seleccionados.'}</div>
                    </div>
                  </td>
                </tr>
              ) : (
                ventas.map(venta => (
                  <tr key={venta.id}>
                    <td className="folio-cell">#{venta.idVenta > 0 ? venta.idVenta : venta.folio}</td>
                    <td className="date-cell">
                      {new Date(venta.fechaCreacionUtc).toLocaleString('es-MX', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </td>
                    <td className="client-cell">{venta.clienteNombre}</td>
                    <td style={{ textAlign: 'center' }}>
                      <strong>{venta.cantidadProductos}</strong> {venta.cantidadProductos === 1 ? 'partida' : 'partidas'}
                    </td>
                    <td className="total-cell" style={{ textAlign: 'right' }}>
                      ${venta.total.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      {renderEstadoBadge(venta.estadoVenta)}
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      <div className="notas-actions-cell" style={{ justifyContent: 'center' }}>
                        <button
                          className="btn-action-icon"
                          title="Vista previa de Nota de Venta"
                          onClick={() => handleVerVistaPrevia(venta)}
                        >
                          👁️ {isZh ? '预览' : 'Vista previa'}
                        </button>
                        <button
                          className="btn-action-icon"
                          title="Descargar PDF comercial"
                          onClick={() => handleDescargar(venta)}
                        >
                          📥 PDF
                        </button>
                        <button
                          className="btn-action-icon"
                          title="Imprimir en tamaño carta"
                          onClick={() => handleImprimir(venta)}
                        >
                          🖨️ {isZh ? '打印' : 'Imprimir'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Paginación */}
        {totalCount > 0 && (
          <div className="notas-pagination">
            <div className="notas-pagination-info">
              {isZh
                ? `显示第 ${(pageNumber - 1) * pageSize + 1} 至 ${Math.min(pageNumber * pageSize, totalCount)} 条，共 ${totalCount} 条`
                : `Mostrando registros ${(pageNumber - 1) * pageSize + 1} al ${Math.min(pageNumber * pageSize, totalCount)} de un total de ${totalCount}`}
            </div>
            <div className="notas-pagination-controls">
              <button
                className="btn-secondary-wpc"
                disabled={pageNumber <= 1 || loading}
                onClick={() => setPageNumber(p => Math.max(1, p - 1))}
              >
                ◀ {isZh ? '上一页' : 'Anterior'}
              </button>
              <span style={{ padding: '0.4rem 0.8rem', fontWeight: 600, color: '#6F3215' }}>
                {pageNumber} / {totalPaginas}
              </span>
              <button
                className="btn-secondary-wpc"
                disabled={pageNumber >= totalPaginas || loading}
                onClick={() => setPageNumber(p => p + 1)}
              >
                {isZh ? '下一页' : 'Siguiente'} ▶
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Modal de Vista Previa PDF */}
      {previewVentaId && (
        <div className="modal-overlay" onClick={handleCerrarVistaPrevia}>
          <div className="modal-content-pdf" onClick={e => e.stopPropagation()}>
            <div className="modal-header-wpc">
              <h3>
                <span>📄</span> {isZh ? `商业销售单据预览 - #${previewFolio}` : `Vista Previa — Nota de Venta #${previewFolio}`}
              </h3>
              <button className="modal-close-btn" onClick={handleCerrarVistaPrevia}>✕</button>
            </div>
            <div className="modal-body-pdf">
              {loadingPreview ? (
                <div style={{ color: '#FFFFFF', textAlign: 'center', padding: '3rem' }}>
                  ⏳ {isZh ? '正在生成PDF文档...' : 'Generando documento PDF de alta calidad...'}
                </div>
              ) : previewPdfUrl ? (
                <iframe
                  src={previewPdfUrl}
                  title="Vista Previa Nota de Venta"
                  className="pdf-iframe"
                />
              ) : (
                <div style={{ color: '#FFFFFF', textAlign: 'center', padding: '3rem' }}>
                  {isZh ? '无法加载预览' : 'No se pudo cargar la vista previa.'}
                </div>
              )}
            </div>
            <div className="modal-footer-wpc">
              <button
                className="btn-primary-wpc"
                onClick={() => {
                  if (previewVentaId) {
                    servicioNotasVenta.descargarPdf(previewVentaId, previewFolio);
                  }
                }}
              >
                📥 {isZh ? '下载PDF' : 'Descargar PDF'}
              </button>
              <button
                className="btn-secondary-wpc"
                onClick={() => {
                  if (previewVentaId) {
                    servicioNotasVenta.imprimirPdf(previewVentaId);
                  }
                }}
              >
                🖨️ {isZh ? '打印' : 'Imprimir'}
              </button>
              <button className="btn-secondary-wpc" onClick={handleCerrarVistaPrevia}>
                {isZh ? '关闭' : 'Cerrar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
