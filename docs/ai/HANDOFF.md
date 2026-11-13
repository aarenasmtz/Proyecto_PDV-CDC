## 📌 Hito Cumplido: Despliegue en Vivo a Producción en VPS Cloud y Cloudflare Workers (13 de Noviembre, 2026)
- **Objetivo**: Recompilar y desplegar los cambios más recientes para que se reflejen de inmediato en el servidor VPS de producción y en la CDN Edge de Cloudflare.
- **Acciones Realizadas**:
  1. **Backend API en Servidor VPS (`193.46.198.88`)**:
     - Compilado en modo Release (`dotnet publish -c Release`).
     - Transferido por SCP y desplegado en `/var/www/pos-api/`.
     - Ajustados permisos `www-data:www-data` y reiniciado el servicio `pos-api.service`.
     - Migración `AddFacturacionModuleCompleto` aplicada automáticamente por EF Core en la BD de producción.
     - 99 imágenes Base64 migradas a WebP en `/var/wpcbajio/data/products`.
     - Verificado en vivo respondiendo `200 OK` en `https://api.wpcbajio.com/api/v1/health`.
  2. **Frontend SPA en Cloudflare Workers**:
     - Compilado con Vite (`npm run build`).
     - Desplegado con Wrangler a Cloudflare Workers Assets (`pos-wpcbajio`).
     - Verificado en vivo respondiendo `200 OK` en `https://pos-wpcbajio.aaronarenasmartinez.workers.dev`.
  3. **Credenciales FiscalAPI**:
     - Mantenidas en modo QA (`https://test.fiscalapi.com`) como se solicitó expresamente.

## 📌 Hito Cumplido: Creación y Publicación del Pull Request #9 (13 de Noviembre, 2026)
- **Objetivo**: Preparar, comitear y publicar en GitHub el Pull Request oficial que integra los módulos de Facturación Electrónica CFDI 4.0 con FiscalAPI, Notas de Venta Comerciales y optimizaciones de rendimiento en PDV, manteniendo las claves de FiscalAPI en QA.
- **Acciones Realizadas**:
  1. Rama creada: `feature/facturacion-cfdi-y-notas-de-venta`.
  2. Identidad Git autorizada verificada: `Aaron Arenas Martinez` (`44418304+UTLCDA@users.noreply.github.com`). Cero mención a cuentas externas o corporativas.
  3. Commit realizado: `b5e1bad` (83 archivos, +18,611 líneas).
  4. Rama publicada en remoto y Pull Request creado con GitHub CLI:
     - **URL del PR**: [https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9)
  5. Configuración de FiscalAPI mantenida en **QA** (`appsettings.json`), dejando como tarea pendiente el cambio a credenciales de producción (PR) durante el despliegue al servidor VPS.

## 📌 Hito Cumplido: Levantamiento de Servicios y Auditoría Exhaustiva de Carga en los 17 Módulos del Sistema (13 de Noviembre, 2026)
- **Objetivo**: Levantar los servicios de Backend y Frontend tras reinicio del entorno, y auditar por qué algunos módulos reportaban no mostrar información tras la última implementación.
- **Acciones Realizadas**:
  1. **Servicios Levantados y Monitoreados**:
     - Backend API: `dotnet run --project src/backend/Pos.Api` (`http://localhost:5000`).
     - Frontend SPA: `npm --prefix src/frontend/pos-web run dev` (`http://localhost:5173`).
     - Base de Datos SQL Server `PosLambrinDb` verificada y conectada.
  2. **Auditoría Integral de Rendimiento y Endpoints**:
     - Creado script de pruebas automatizadas sobre las 42 peticiones que disparan los componentes React.
     - **42 de 42 peticiones aprobadas al 100% (cero errores HTTP, latencias <50 ms)**.
  3. **Causa Raíz de Módulos sin Información**:
     - *Historial de Ventas*, *Movimientos de Inventario* y *Reportes*: Inicializan sus filtros con la fecha de hoy (`2026-11-13`). Como no se han registrado operaciones en el día actual (las pruebas fueron generadas hasta el 09/11/2026), la pantalla muestra 0 registros por diseño del filtro. Al limpiar o mover la fecha hacia atrás, aparecen todas las ventas y movimientos al instante.
     - *Recibos de Compra*, *Cotizaciones* y *Devoluciones*: Tablas legítimamente vacías en la base de datos local (0 filas).
     - *Catálogo de Productos*: Filtra por defecto la categoría *Lambrin Interior* (29 productos); al seleccionar todas, muestra los 111 productos.
  4. **Verificación de Calidad**:
     - Backend: 122/122 pruebas unitarias e integración aprobadas al 100%.
     - Frontend: 53/53 pruebas unitarias de Vitest aprobadas.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Corrección de Retardo en Ticket de Venta y Timeout de Conexión en Base de Datos (09 de Noviembre, 2026)
- **Objetivo**: Resolver el problema donde, tras completar una venta en el Punto de Venta, tardaba mucho en mostrar el ticket, la pantalla se quedaba cargando para volver a mostrar los productos y saltaba el error de tiempo de espera agotado al conectar con el servidor (15s).
- **Acciones Realizadas**:
  1. **Visualización Inmediata del Ticket (`PaginaPuntoVenta.tsx`)**:
     - Se eliminó el desmontaje del árbol DOM provocado por `setLoading(true)` dentro de `loadData()`, el cual destruía el modal `<SaleReceiptModal>` antes de renderizarlo.
     - El ticket se despliega ahora de forma instantánea al recibir la respuesta de la venta.
     - Creado `refreshStockAndSummary()` que corre en segundo plano de manera silenciosa sin bloquear la UI ni el ticket, reduciendo las consultas posteriores de 5 endpoints a solo 2 esenciales (`getProducts` + `getSalesSummary`).
  2. **Optimización del Pool de Conexiones en SQL Server (`appsettings.json`, `appsettings.Development.json`)**:
     - Agregados los parámetros `Connect Timeout=30;Max Pool Size=200;MultipleActiveResultSets=True;` en la cadena de conexión de `PosDbContext`, evitando la saturación del pool y el bloqueo en post-login (`[Post-Login] complete=14016`).
  3. **Ajuste de Timeout del Cliente (`apiClient.ts`)**:
     - Timeout ampliado de 15s a 30s para prevenir abortos prematuros en llamadas transaccionales.
  4. **Verificación**:
     - Creación de venta en vivo completada en 938 ms con refresco silencioso en 93 ms.
     - 122/122 pruebas de backend aprobadas.
     - 53/53 pruebas de frontend aprobadas. Build de producción completado con 0 errores.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Rediseño del Control de Cantidad y Carrito en Punto de Venta (09 de Noviembre, 2026)
- **Objetivo**: Corregir el diseño del contenedor donde se agregan y disminuyen piezas (`+` y `−`) en el carrito del Punto de Venta, resolviendo el problema de baja visibilidad ("se pierde") y bordes desconectados.
- **Acciones Realizadas**:
  1. **Depuración de CSS en Cascada (`PaginaPuntoVenta.css`)**:
     - Eliminadas las reglas conflictivas heredadas en línea 944 que forzaban `display: grid` y rompían los bordes laterales del input numérico.
  2. **Cápsula Segmentada de Cantidad (`.pos-quantity-control`)**:
     - Estructura unificada en píldora con borde cálido contrastante (`#CFB9A5`, 1.5px), esquinas redondeadas (`7px`) y elevación sutil.
     - Botones `−` y `+` con fondo `#FAF5ED`, separadores nítidos e interacción hover de fondo terracota (`#9C4D22`) con icono blanco.
     - Input central en `#FFFFFF` con número en marrón oscuro `#3A2312` (`font-weight: 750`), sin flechas nativas molestas.
  3. **Jerarquía Visual del Carrito**:
     - Botón de eliminación (`×`) con forma circular suave y hover rojo elegante.
     - Subtotal de partida en negrita nítida (`#3A2312`) y metadatos de piezas/cobertura en tono secundario ordenado (`#7A6657`).
  4. **Verificación**:
     - 53/53 pruebas unitarias de Vitest aprobadas.
     - Build de producción Vite completado con cero errores y cero advertencias CSS.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Optimización de Rendimiento en Carga de Productos del PDV (09 de Noviembre, 2026)
- **Objetivo**: Resolver la lentitud crítica en la carga de la vista principal del PDV (`/api/v1/products?categoryId=...&pageSize=500`) donde la petición tardaba más de 4.56 segundos (2.27s Stalled + 2.27s TTFB).
- **Acciones Realizadas**:
  1. **Migración de Imágenes Base64 a Disco en WebP (`DbInitializer.cs`)**:
     - Causa identificada: 99 de 112 productos almacenaban imágenes Base64 completas en `Products.ImagenUrl` (~600 KB c/u, 31.2 MB en tabla, 8.28 MB en la categoría del PDV).
     - Se implementó la migración automática `MigrateBase64ImagesToDiskAsync` en el arranque: decodifica, redimensiona y almacena imágenes optimizadas en WebP (128x128, 256x256, 512x512) en `C:\WPCBajioData\Products\{productId}\`.
     - Actualizadas las rutas en base de datos a `/products/{productId}/pos.webp?v=...`, reduciendo el payload de la API de 8.5 MB a solo 15 KB (560x de reducción).
  2. **Proyección Directa a DTO en una Sola Consulta (`CatalogApplicationService.cs`)**:
     - Refactorizado `GetProductsAsync` con `.Select(...)` directo, resolviendo la categoría mediante navegación y el stock mediante subconsulta directa, evitando la carga de entidades en memoria y el uso de `OPENJSON`.
     - El tiempo de respuesta del backend .NET 9 cayó de 2,924 ms a 35 ms.
  3. **Corrección de Socket IPv6 y Stalled en Vite Dev Server (`vite.config.ts`)**:
     - Configurado `server.host: '::'` (dual-stack IPv6 e IPv4) para evitar el timeout de conexión TCP SYN de 2.27 segundos de Windows al intentar conectar a `localhost` (`::1`), y configurado el target del proxy a `http://127.0.0.1:5000`.
  4. **Resultados Finales**:
     - Tiempo de carga total: de **4,560 ms** a **26 ms** (~175x de aceleración).
  5. **Verificación**:
     - Backend: 122/122 pruebas unitarias e integración aprobadas al 100%.
     - Frontend: 53/53 pruebas unitarias aprobadas. Build de producción exitoso en 13.64s.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Corrección de Carga Lenta, Doble Fetch y Parpadeo en Módulo de Facturación (09 de Noviembre, 2026)
- **Objetivo**: Resolver el problema en el **Módulo de Facturación CFDI 4.0** donde tardaba en cargar, se quedaba la leyenda "Cargando datos..." y las facturas aparecían y desaparecían repetidamente.
- **Acciones Realizadas**:
  1. **Eliminación del Doble Fetch en Cascada (`FacturacionPage.tsx`)**:
     - Desacoplada la llamada a `obtenerConfiguracion()` fuera del ciclo de consulta de facturas (`loadInvoices`). Ahora se ejecuta una sola vez al montar el componente en su propio `useEffect`.
     - Eliminada la dependencia `config` de `useCallback`, evitando que la actualización de la configuración del emisor invalide la función y dispare una segunda petición inmediata a la API con un segundo ciclo de `setLoading(true)`.
  2. **Estabilidad del DOM de la Tabla (Cero Desmontajes y Cero Parpadeo)**:
     - El componente `<table>` y la barra de paginación `<TablePagination>` se mantienen fijos y permanentemente montados en el DOM.
     - La animación de carga `⏳ Cargando facturas emitidas...` y el estado vacío se colocaron limpiamente dentro del `<tbody>` como filas (`<tr><td colSpan={9}>`), eliminando la destrucción de la tabla que provocaba el efecto de aparecer y desaparecer.
  3. **Corrección de Filtros de Fecha Iniciales**:
     - `startDate` y `endDate` se inicializan con cadena vacía `''` en lugar de `today` (`09/11/2026`), asegurando que las facturas emitidas en fechas previas (como las del `08/11/2026`) no se filtren ni desaparezcan arbitrariamente al interactuar con el formulario.
  4. **Homologación en la Pestaña de Bitácora (`FacturacionBitacoraTab.tsx`)**:
     - Aplicadas las mismas mejoras estructurales: tabla de logs montada de forma permanente con fila de carga en `<tbody>` (`colSpan={10}`) y fechas iniciales limpias.
  5. **Verificación**:
     - Backend: 122/122 pruebas pasando al 100%. Latencia de endpoint `GET /api/v1/facturacion/invoices`: ~22ms.
     - Frontend: 53/53 pruebas de Vitest aprobadas. Build de producción completado en 18.49s con 0 errores.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Reestructuración de Filtro de Estados y Badges en Notas de Venta (08 de Noviembre, 2026)
- **Objetivo**: Corregir y reestructurar el filtro `<select className="notas-filter-select">` en el módulo de Notas de Venta para que coincida exactamente con los estados reales almacenados en la base de datos (`Sales.Estado`), e incorporar la columna visible de Estado con badges de color en la tabla.
- **Acciones Realizadas**:
  1. **Reestructuración del Selector de Estados (`NotasVentaPage.tsx`)**:
     - Sustituidas las opciones desfasadas en inglés (`Completed`, `DepositPaid`, etc.) por los estados reales del sistema con soporte bilingüe (español y chino):
       - `ALL`: Todos los estados / 所有状态
       - `Completada`: ✔ Completada / ✔ 已完成
       - `ApartadoPagado`: 📦 Con Anticipo / 📦 定金已付
       - `PendientePago`: ⏳ Pendiente de Pago / ⏳ 待付款
       - `Cancelada`: ✕ Cancelada / ✕ 已取消
       - `DevolucionParcial`: ↩ Devolución Parcial / ↩ 部分退货
       - `Devuelta`: ↩ Devuelta / ↩ 全部退货
  2. **Columna "Estado" y Badges Visuales (`NotasVentaPage.tsx` & `NotasVentaPage.css`)**:
     - Agregada la columna `Estado` / `状态` en la tabla principal con badges distinguidos por color: verde (`Completada`), rojo (`Cancelada`), azul (`Con Anticipo`), ámbar (`Pendiente Pago`), púrpura (`Devolución`).
  3. **Compatibilidad Bidireccional en Backend (`NotaVentaApplicationService.cs`)**:
     - El endpoint `GET /api/v1/notas-venta/ventas?saleStatus=...` acepta tanto los valores en español como constantes estándar o alias en inglés.
  4. **Paginación (`tiposNotasVenta.ts` & `NotasVentaPage.tsx`)**:
     - Mapeo de `res.totalItems ?? res.totalCount ?? 0` para sincronización exacta con el backend.
  5. **Verificación**:
     - Base de datos consultada: 15 ventas en total (9 Completadas, 6 Canceladas). Filtros probados y validados al 100%.
     - Backend: 122/122 pruebas pasando.
     - Frontend: 53/53 pruebas pasando y build limpio en 18.54s.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Refinamiento de Notas de Venta — Folio Identity 1-1, Esquinas Curvas y Depuración Visual (08 de Noviembre, 2026)
- **Objetivo**: Atender las observaciones del usuario sobre el módulo de Notas de Venta: sustituir el folio GUID por el `IdVenta` identity consecutivo, eliminar el estatus y flujo de entrega del módulo, mostrar exclusivamente el nombre del cliente sin duplicar la razón social, redondear las esquinas de los recuadros del PDF, actualizar los datos de contacto con los reales del e-commerce y eliminar recuadros innecesarios ("ENTREGAR A / DESTINO", "ESTADO: ENTREGADO" y label de "Saldo: $0.00 (Liquidado)").
- **Acciones Realizadas**:
  1. **Folio Consecutivo Identity 1-1**:
     - `NotaVentaApplicationService.cs` y `NotaVentaPdfService.cs`: Folio visible establecido como `IdVenta` numérico (ej. `25` o `125`), manteniendo el GUID a nivel base de datos únicamente para integridad referencial.
     - `NotasVentaPage.tsx`: Visualización de `#${idVenta}` en la tabla y título del modal.
  2. **Eliminación del Estado y Botón de Entrega en el Módulo**:
     - `NotasVentaPage.tsx`: Se retiró la columna "Estado Entrega", el botón "🚚 Entrega", el diálogo modal de entrega y el filtro de entrega.
     - Las acciones se enfocaron exclusivamente en: `👁️ Vista previa`, `📥 PDF` e `🖨️ Imprimir`.
  3. **Nombre de Cliente Limpio**:
     - Eliminada la concatenación duplicada de la razón social en el nombre del cliente (`${Cliente.Nombre} ${Cliente.Apellido}`, ej. `Aaron Arenas Martinez` o `Público General`).
  4. **Esquinas Curvas Redondeadas en PDF con Dynamic SVG Vectorial (QuestPDF)**:
     - Diseñado el helper `RoundedCard` mediante `layers.Layer().Svg(size => ...)` que genera un `<rect>` dinámico con `rx="6"` y `ry="6"`, adaptado con precisión milimétrica al tamaño de su capa de contenido.
     - Aplicado a: Recuadro NOTA DE VENTA, Tarjeta DATOS DEL CLIENTE, Recuadro OBSERVACIONES y Recuadro TOTALES.
  5. **Datos Comerciales Reales de WPC Bajío**:
     - Dirección: `Blvd. Adolfo López Mateos 2826, Col. El Rosario, C.P. 37125, León de los Aldama, Guanajuato`.
     - Tel. / WhatsApp: `477 807 2768`.
     - Correo y Web: `contacto@wpcbajio.com` | `www.wpcbajio.com`.
  6. **Depuración del Layout del PDF**:
     - Retirado el recuadro `ENTREGAR A / DESTINO`. El bloque `DATOS DEL CLIENTE` ahora ocupa el ancho completo en dos columnas limpias.
     - Retirado el recuadro/sello `ESTADO: ENTREGADO` / `PENDIENTE DE ENTREGA`.
     - Retirada la etiqueta `Saldo: $0.00 (Liquidado)`.
  7. **Verificación y Pruebas**:
     - Backend: 122/122 pruebas pasando al 100% (Domain: 19, Application: 82, Integration: 21).
     - Frontend: 53/53 pruebas de Vitest pasando al 100%. Build de Vite con 0 errores en 13.56s.
     - Servidores ejecutándose activamente: API en `http://localhost:5000` y Web en `http://localhost:5173`.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Implementación Módulo Completo "Notas de Venta — WPC Bajío" (08 de Noviembre, 2026)
- **Objetivo**: Crear un módulo independiente denominado **Notas de Venta** para emitir, consultar, visualizar, imprimir y descargar notas comerciales en PDF tamaño carta sin efectos fiscales, completamente desacoplado de FiscalAPI y sin consumir timbres ni requerir timbrado SAT.
- **Acciones Realizadas**:
  1. **Generación de PDF en .NET 9 (`QuestPDF 2024.12.3`)**:
     - `NotaVentaPdfService.cs`: Generación de documento en tamaño carta (216 × 279 mm), vertical, márgenes de 26pt.
     - Paleta corporativa: `#9C4D22`, `#833E1A`, `#6F3215`, `#FAF8F5`, `#FEF7EB`, `#F3E9DA`, `#3A2312`, `#7A6657`.
     - Logotipo WPC Bajío integrado desde `wwwroot/images/logo_wpc_bajio.jpeg` con fallback automático.
     - Datos de emisor de WPC Bajío (León, Gto.), recuadro de Nota de Venta / Sin Efectos Fiscales / Folio / Fecha y Vendedor.
     - Bloque en 2 columnas: "DATOS DEL CLIENTE" y "ENTREGAR A / DESTINO" (modalidad, destinatario, dirección y contacto).
     - Tabla dinámica con descripciones comerciales, piezas calculadas para cajas de lambrín (`Cantidad * PiezasPorCaja`), precios históricos y totales de partida.
     - Observaciones comerciales, sello oficial de entrega (`✔ ESTADO: ENTREGADO` / `⏳ ESTADO: PENDIENTE DE ENTREGA`), totales históricos (con desglose de IVA solo si fue registrado en la venta) y leyenda obligatoria: *"Documento comercial sin efectos fiscales. No sustituye un comprobante fiscal digital por Internet (CFDI)."*
     - Numeración de páginas en pie: "Página X de Y".
  2. **Backend API (`NotasVentaController.cs` & `NotaVentaApplicationService.cs`)**:
     - Endpoints REST:
       - `GET /api/v1/notas-venta/ventas`: Consulta con búsqueda de texto, rango de fechas, estado de venta, estado de entrega y paginación de 25 registros ordenados descendentemente por fecha.
       - `GET /api/v1/notas-venta/ventas/{id}`: Detalle completo de la nota.
       - `GET /api/v1/notas-venta/ventas/{id}/pdf`: Retorna el PDF para visualización y descarga directa con auditoría en `IAuditLogService`.
       - `POST /api/v1/notas-venta/ventas/{id}/confirmar-entrega`: Alterna o confirma el estatus de entrega de mercancía persistiendo en `Notas` de la venta (`[ESTATUS: Entregado]`) con registro de auditoría.
  3. **Frontend SPA (`NotasVentaPage.tsx` & `.css`, `servicioNotasVenta.ts`)**:
     - Navegación independiente en navbar: `📋 Notas de Venta` (bilingüe: español / chino simplificado `📋 销售单据`).
     - Tarjetas de métricas: Total ventas, Entregadas en página, Pendientes de entrega y Monto acumulado en página.
     - Tabla interactiva con columnas Folio, Fecha, Cliente, Productos, Total, Estado Entrega y Acciones:
       - `👁️ Vista previa`: Modal con visor de PDF integrado para previsualizar antes de imprimir.
       - `📥 PDF`: Descarga del archivo PDF oficial.
       - `🖨️ Imprimir`: Disparo de impresión directo en tamaño carta.
       - `🚚 Entrega`: Diálogo modal para marcar como entregado o revertir a pendiente con notas de entrega.
     - Enlace directo en `SalesHistoryPage.tsx`: Botón `📋 Nota` para descarga en 1 clic.
  4. **Verificación y Pruebas**:
     - Backend: 122/122 pruebas unitarias e integración aprobadas (100% de éxito).
     - Frontend: 53/53 pruebas de Vitest aprobadas. Build de producción exitoso en 18.30s.
     - Ambos servidores ejecutándose de forma reactiva en `http://localhost:5000` (API) y `http://localhost:5173` (Vite).
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Combo Buscable Estilizado de Clientes en Punto de Venta (08 de Noviembre, 2026)
- **Problema Reportado**: El selector de clientes en el Punto de Venta utilizaba el `<select>` nativo del navegador, desplegando una ventana emergente gris sin diseño ni formateo visual para los datos fiscales.
- **Acciones Realizadas**:
  1. `PaginaPuntoVenta.tsx`: Reemplazo del elemento `<select>` nativo por un selector customizado de clientes con trigger y panel flotante.
  2. Búsqueda y Filtrado: Barra de búsqueda superior en vivo con `🔍` que filtra por nombre, empresa, RFC, teléfono y código postal.
  3. Diseño Visual y Badges:
     - Badges de tipo de cliente (`Mayorista` en verde, `Arquitecto` en morado, `Particular` en gris).
     - Chips fiscales destacados: RFC en color azul con tipografía monoespaciada (`🏷️ RFC: ...`) y Código Postal (`📍 CP: ...`).
     - Checkmark de selección activa y botón directo de deselección.
     - Cierre automático al hacer clic fuera o presionar la tecla `Escape`.
  4. `PaginaPuntoVenta.css`: Clases CSS dedicadas (`.pos-customer-select-trigger`, `.pos-customer-dropdown-menu`, `.pos-customer-option-item`) con sombras de elevación, transiciones y animación `posFadeInDown`.
  5. Verificación: Build de Vite exitoso en 16.58s y 51/51 pruebas de Vitest aprobadas.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Alta de Clientes con RFC y CP en Punto de Venta e Integración de IVA para Facturación (08 de Noviembre, 2026)
- **Problema / Solicitud**: En la pantalla de Punto de Venta (PDV), al hacer clic en `+ Cliente`, faltaban los campos de **RFC** y **Código Postal (CP)** para registrar clientes fiscales y completar el flujo de venta con factura, donde era importante reflejar el 16% de IVA correctamente en los montos.
- **Acciones Realizadas**:
  1. `PaginaPuntoVenta.tsx`:
     - Modal `+ Cliente`: Incorporados campos de `RFC / Tax ID` (con validación de formato SAT y auto-mayúsculas) y `Código Postal Fiscal` (5 dígitos numéricos con auto-resolución de ciudad y estado).
     - Selector de Clientes: Ahora muestra `DisplayName · RFC: ... · CP: ...` y una barra informativa con los datos fiscales del cliente seleccionado.
     - Checkbox `¿Desea facturar? (Aplicar 16% IVA)`: Muestra banner contextual indicando el receptor del CFDI 4.0 y refleja la adición del 16% de IVA en la liquidación del carrito.
  2. `CatalogApplicationService.cs`:
     - Sincroniza `CodigoPostalFiscal = values.PostalCode` tanto en alta como en edición de clientes para que el módulo de facturación lo detecte sin requerir capturas adicionales.
  3. `FacturacionApplicationService.cs`:
     - Prorrateo automático y exacto a nivel centavos de descuentos de venta (`sale.MontoDescuento`) entre partidas para que los importes de los conceptos del CFDI 4.0 cuadren con las reglas matemáticas del SAT.
  4. Verificación Automatizada:
     - Backend: 119 pruebas unitarias aprobadas. Compilación limpia con 0 errores.
     - Frontend: 51/51 pruebas de Vitest aprobadas. Build de producción con Vite exitoso en 17.86s.
- **Condición Git**:
  - Sin commits ni empujes a repositorios remotos, conforme a la instrucción expresa del usuario.

## 📌 Hito Cumplido: Consulta de Estado SAT ante FiscalAPI Resuelta (08 de Noviembre, 2026)
- **Problema Reportado**: Error al ejecutar `ConsultarEstadoSat`: `"Debe proporcionar solo el 'Id' o el resto de los campos, pero no ambos."`
- **Causa Raíz**: FiscalAPI exige que en `InvoiceStatusRequest` se mande únicamente `Id` (si fue emitida en su plataforma) O los datos fiscales (`InvoiceUuid`, `IssuerTin`, `RecipientTin`, `InvoiceTotal`), pero nunca ambos a la vez.
- **Acciones Realizadas**:
  1. `FacturacionApplicationService.cs`:
     - Se aplicó bifurcación excluyente en `ConsultarEstadoSatAsync` (solo `req.Id` si existe `FiscalapiInvoiceId`).
     - Se aplicó la misma regla preventiva en `SolicitarCancelacionAsync` (`CancelInvoiceRequest`).
  2. Verificación Exitosa:
     - Consulta ejecutada para Folio A-2: Respuesta SAT `Vigente`, `Cancelable sin aceptación`, validación EFOS `200` y código HTTP 200 en 1106 ms.
     - Registro auditado en `FacturacionLog`.
- **Condición Git**:
  - Sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Estilos y Modal Flotante Centrado en Bitácora Técnica FiscalAPI (08 de Noviembre, 2026)
- **Problema Reportado**: Al dar clic en ver detalles de un registro en la pestaña de Bitácora Técnica de FiscalAPI, la vista enviaba hasta abajo de la página en lugar de abrir un modal sobre la tabla.
- **Acciones Realizadas**:
  1. `FacturacionPage.css`: Definición completa de `.modal-backdrop`, `.modal`, `.modal-lg`, `.modal-header`, `.modal-body` y `.modal-footer` con posicionamiento fijo (`z-index: 1200`), fondo oscuro blur y animación centrada.
  2. `FacturacionBitacoraTab.tsx` y `FacturacionLogDetalleModal.tsx`: Importación directa de los estilos de facturación.
  3. Verificación de build Vite: Exitoso en 11.83s sin errores.
- **Condición Git**:
  - Sin commits ni pushes conforme a la regla del usuario.

## 📌 Hito Cumplido: Unicidad y Filtrado de Facturas Emitidas (Solo Timbradas) (08 de Noviembre, 2026)
- **Problema Reportado**: La pestaña "Facturas Emitidas" mostraba 17 registros (incluyendo intentos técnicos rechazados repetidos de Folio A-1 y A-2), comportándose como un log en vez de un catálogo de facturas timbradas.
- **Acciones Realizadas**:
  1. `FacturacionApplicationService.cs`:
     - Se ajustó `ObtenerFacturasAsync` para excluir por defecto estados que no representen facturas fiscales emitidas (`Rechazada`, `Borrador`).
     - Se actualizó `EmitirFacturaVentaAsync` para reutilizar el registro existente de una venta ante reintentos de timbrado, evitando duplicados en `FacturacionFactura`.
  2. `FacturacionPage.tsx`:
     - Filtro de estado fijado por defecto en `Timbrada`.
     - Depuración del selector para no incluir opciones de fallas técnicas.
     - Métricas ajustadas para reflejar el conteo real de comprobantes timbrados.
  3. Base de Datos Local:
     - Depurados los 15 registros temporales de `Rechazada` en `FacturacionFactura`.
     - Conservados íntegramente los 17 registros de auditoría técnica en `FacturacionLog`.
  4. Verificación:
     - `GET /api/v1/facturacion/invoices` retorna exactamente 2 facturas timbradas (`Folio A-1` y `Folio A-2`).
     - Build de Vite exitoso en 11.74s.
- **Condición Git**:
  - Trabajo local sin commits conforme a la regla del usuario.

## 📌 Hito Cumplido: Primer Timbrado Oficial CFDI 4.0 con FiscalAPI Exitoso en Localhost (08 de Noviembre, 2026)
- **Venta Timbrada**: Venta #23 (`FC3D7188-9A04-4E4E-8378-F235B651CB8E`)
- **Datos Fiscales Generados**:
  - UUID SAT: `4cbb6b1e-a5c9-484b-9bf7-a37bc3b075df`
  - Serie / Folio: `A-1`
  - Total: `$183.28 MXN`
  - Emisor: `EKU9003173C9` (*ESCUELA KEMPER URGATE*, Régimen 601, CP 42501, PersonId `980ec84f-90b4-4a8d-a2e7-eb19d5c60f52`)
  - Receptor: `AEMA960428ME6` (*AARON ARENAS MARTINEZ*, Régimen 612, CP 37545, Uso CFDI `G03`)
- **Artefactos y Auditoría**:
  - XML CFDI 4.0: Persistido en `FacturacionFactura.XmlContenido` y `FacturacionDocumento` (`4,861 bytes`).
  - Representación PDF: Descargada y almacenada en `Storage/Invoices/Pdf/` (`137,330 bytes`).
  - Bitácora Técnica `FacturacionLog`: Registrada con HTTP 200 OK y 372 ms de latencia.
- **Correcciones Clave Realizadas**:
  1. `TaxFlagCode = "T"` en `FacturacionApplicationService.cs` (sustituyendo string `"Traslado"`).
  2. Asignación de `Issuer.Id = emisorConfig.FiscalapiPersonId` para timbrado por referencia CSD.
  3. Identificación y aclaración de saldo de créditos: distinción entre créditos de tickets/validaciones y timbres CFDI.
- **Condición Git**:
  - Working tree local intacto, sin commits ni empujes a repositorios remotos.

## 📌 Hito Cumplido: Configuración de Credenciales Sandbox FiscalAPI y Validación Exitosa (08 de Noviembre, 2026)
- **Credenciales Provistas**:
  - `ApiKey`: `sk_test_e91be921_0861_4d83_b62e_676c6441a586`
  - `Tenant`: `a97a851a-45bf-4be0-a48e-7c9bf4b18c30`
- **Acciones Realizadas**:
  1. Configuración de llaves en `appsettings.Development.json` y `appsettings.json` apuntando al ambiente `QA` (`https://test.fiscalapi.com`).
  2. Reinicio en caliente del backend .NET 9 en `http://localhost:5000`.
  3. Comprobación de configuración del emisor y generación de vista previa de CFDI 4.0 para venta completada con elegibilidad al 100% (`esElegibleParaFacturar: true`, desglose exacto de IVA 16%).
  4. Mantenimiento del working tree local sin realizar commits en Git.

## 📌 Hito Cumplido: Corrección de Validación de Tenant en DI y Puesta a Punto en Localhost (08 de Noviembre, 2026)
- **Problema Reportado**: Excepción al navegar al módulo `/facturacion`: `System.ArgumentException: Tenant is required (Parameter 'Tenant') at Fiscalapi.Services.FiscalApiClient.ValidateSettings`.
- **Causa Raíz**: El SDK oficial `FiscalApiClient.Create(FiscalapiSettings)` valida obligatoriamente que `Tenant` y `ApiKey` no sean nulos ni vacíos. Al no existir credenciales configuradas en los archivos `appsettings.json` de desarrollo, el contenedor DI de ASP.NET Core fallaba al instanciar `FacturacionController`.
- **Acciones Realizadas**:
  1. Configuración de fallbacks en `Pos.Infrastructure/DependencyInjection.cs`:
     - Se asigna `"NOT_CONFIGURED"` por defecto si `fSettings.Tenant` o `fSettings.ApiKey` están en blanco.
     - Se asigna `"America/Mexico_City"` por defecto si `fSettings.TimeZone` no fue provisto.
  2. Adición de credenciales de desarrollo en `appsettings.json` y `appsettings.Development.json` (`sandbox_dev_placeholder_tenant`, `sandbox_dev_placeholder_key`).
  3. Recompilación limpia del backend (0 errores, 79/79 pruebas de aplicación superadas).
  4. Servicios levantados en localhost:
     - Backend: `http://localhost:5000` (PID activo, endpoints `GET /api/v1/facturacion/config`, `GET /api/v1/facturacion/invoices` y `GET /api/v1/facturacion/bitacora` respondiendo `200 OK`).
     - Frontend: `http://localhost:5173` (Vite dev server activo y sirviendo).
  5. Cero commits realizados (respetando la indicación explícita del usuario).

## 📌 Hito Cumplido: Base de Datos Relacional y Bitácora Técnica de Facturación FiscalAPI (08 de Noviembre, 2026)
- **Objetivo**: Desarrollar e integrar la persistencia completa y el subsistema de auditoría técnica profunda para el módulo de facturación con el SDK oficial de FiscalAPI .NET, cubriendo 11 tablas relacionales en SQL Server, registro detallado de cada llamada HTTP (`FacturacionLog`), sanitización estricta de secretos y payloads voluminosos, endpoints REST de diagnóstico y consulta, y panel web con visor de trazas e historial de correlación.
- **Acciones Realizadas**:
  1. **11 Tablas Relacionales e Índices (EF Core 9)**:
     - `FacturacionConfiguracion`, `FacturacionReferenciaExterna`, `FacturacionFactura`, `FacturacionFacturaDetalle`, `FacturacionFacturaImpuesto`, `FacturacionFacturaVenta`, `FacturacionIntento`, `FacturacionDocumento`, `FacturacionCancelacion`, `FacturacionHistorialEstado`, `FacturacionLog`.
     - Claves foráneas, índices no agrupados optimizados y tipos monetarios en `decimal(18,2)`, `(18,4)` y `(18,6)` según la precisión oficial requerida por el SAT.
     - Migración oficial generada: `20261108052821_AddFacturacionModuleCompleto.cs`.
  2. **Servicio de Bitácora Técnica (`FacturacionLogService` & `IFacturacionLogService`)**:
     - Registro previo a la llamada como `Iniciada` con `CorrelationId`, `IdempotencyKey`, usuario, sucursal, ambiente y request serializado.
     - Registro post-llamada como `Finalizada`, calculando latencia en ms (`DuracionMs`), código HTTP real recibido (`HttpStatusCode`) y resultado de negocio (`Exitoso`, `Rechazado`, `FalloTecnico`, `ResultadoIncierto`).
     - **Regla Estricta**: Si ocurre una desconexión o timeout de red, `HttpStatusCode` permanece estrictamente en `NULL` (sin inventar códigos 408 o 504 sintéticos).
     - Manejo de timeouts: emisión pasa a `ResultadoPorConfirmar` en la base de datos sin arrojar excepción 500 no controlada, permitiendo reconciliación directa ante el SAT.
  3. **Motor de Sanitización Criptográfica (`FacturacionSanitizer`)**:
     - Enmascaramiento recursivo (`***REDACTED***`) de API keys, tenant keys, contraseñas, bearer tokens y certificados en JSON y cadenas de texto.
     - Detección y exclusión de cadenas Base64 voluminosas (`[BASE64_DOCUMENT_DATA_EXCLUDED: length=X]`).
     - Tope de seguridad de 16 KB en payloads JSON y stack traces para evitar denegación de servicio por almacenamiento en SQL Server.
  4. **Controlador REST y Endpoints de Bitácora (`FacturacionController`)**:
     - `GET /api/v1/facturacion/bitacora`: Paginación y filtrado por fechas, ambiente, operación, resultado, correlationId y HTTP status.
     - `GET /api/v1/facturacion/bitacora/{id}`: Detalle de registro individual.
     - `GET /api/v1/facturacion/bitacora/relacionadas/{correlationId}`: Cadena completa de llamadas asociadas.
     - `POST /api/v1/facturacion/bitacora/purgar`: Purga técnica de registros antiguos con validación de retención mínima legal de 30 días, protegida con permiso `facturacion:bitacora_purgar`.
  5. **Frontend Diagnóstico en `pos-web`**:
     - Sistema de pestañas en `FacturacionPage.tsx` (`Facturas Emitidas` / `Bitácora Técnica FiscalAPI`).
     - `FacturacionBitacoraTab.tsx`: Filtros dinámicos, métricas en tiempo real (llamadas registradas, tasa de éxito, rechazos 4xx, latencia promedio), tabla con colores por rendimiento y modal de confirmación de purga.
     - `FacturacionLogDetalleModal.tsx`: Visualización de metadatos técnicos, visor de Request y Response formateados con botón de copiado al portapapeles, traza de error y tabla de llamadas vinculadas por `CorrelationId`.
     - Soporte i18n completo en español y chino simplificado (`zh-CN`).
  6. **Pruebas y Verificación**:
     - Backend: **119/119 pruebas unitarias e integración pasando al 100%** (19 Domain, 79 Application, 21 Api.IntegrationTests).
     - Frontend: **51/51 pruebas de Vitest pasando al 100%** (8 suites). Build de producción completado en 11.42s con 0 errores.
  7. **Condición de Trabajo**:
     - **Sin Git Commit**: Todo el trabajo se encuentra en el working tree local listo para pruebas en localhost por el desarrollador.

## 📌 Hito Cumplido: Implementación Integral del Módulo de Facturación Electrónica CFDI 4.0 con FiscalAPI .NET SDK (08 de Noviembre, 2026)
- **Objetivo**: Desarrollar el módulo de Facturación Electrónica para PDV Lambrín / WPC Bajío con el SDK oficial `Fiscalapi` NuGet v4.0.469, abarcando backend (.NET 9 C#), base de datos (SQL Server con EF Core 9 Migrations), frontend (React 18 + TypeScript + Vite) con soporte bilingüe (ES / ZH), auditoría completa y pruebas unitarias/integración al 100%.
- **Acciones Realizadas**:
  1. **SDK e Integración Desacoplada**:
     - Paquete oficial `Fiscalapi` v4.0.469 instalado en `Pos.Infrastructure`.
     - Adaptador desacoplado `IFiscalApiClientAdapter` y `FiscalApiClientAdapter` consumiendo `IFiscalApiClient`.
     - Opciones tipadas `FacturacionSettings` con validación estricta de coherencia QA (`test.fiscalapi.com`) vs PR (`live.fiscalapi.com`). Degradación elegante ante falta de credenciales.
  2. **Modelo de Datos y Persistencia**:
     - Nuevas entidades: `ConfiguracionEmisor`, `Factura`, `FacturaPartida`, `FacturaHistorialEstado`.
     - Campos fiscales en `Cliente` (`Rfc`, `RazonSocialFiscal`, `CodigoPostalFiscal`, `RegimenFiscal`, `UsoCfdi`, `EmailFacturacion`) y en `Producto` (`ClaveProdServSat`, `ClaveUnidadSat`, `ObjetoImpuestoSat`).
     - Migración EF Core `20261108041333_AddFacturacionModule` generada y probada.
  3. **Lógica de Negocio y Reglas SAT**:
     - Bloqueo estricto de facturación PUE para ventas con saldo pendiente de pago (`SaldoPendiente > 0`) o apartados en proceso.
     - Desglose y conciliación impositiva: cálculo exacto de IVA trasladado (16%) y base imponible para productos con IVA incluido sin doble imposición, respetando precios y descuentos originales de la venta.
     - Idempotencia y reserva atómica: llave de idempotencia persistida y estado `EnProceso` guardado antes del llamado HTTP para evitar timbrado duplicado por peticiones concurrentes o doble clic.
     - Manejo de timeouts: si FiscalAPI o el SAT tardan en responder, el estado se fija en `ResultadoPorConfirmar` en lugar de falso rechazo.
     - Cancelación con motivos SAT 01 a 04, validación de UUID sustituto para motivo 01 y almacenamiento del acuse de cancelación.
  4. **Frontend y Experiencia de Usuario**:
     - Página principal `FacturacionPage.tsx` con listado de facturas, tarjetas de resumen, filtros avanzados y botones de acción.
     - Modales especializados: `FacturaPreviewModal` (vista previa pre-timbrado con edición de receptor), `FacturaDetalleModal` (detalle completo, visor y descarga de XML/PDF, reenvío de correo y consulta SAT), `FacturaCancelarModal` (motivos 01-04 y UUID de sustitución) y `ConfiguracionEmisorModal` (gestión de emisor y folios).
     - Botón de acción `Facturar` integrado en `SalesHistoryPage.tsx`.
     - Soporte i18n bilingüe completo (Español y Chino Simplificado) en `i18n/index.ts`.
  5. **Pruebas y Verificación**:
     - Backend: 114/114 pruebas superadas al 100% (19 Domain, 74 Application incluyendo `FacturacionApplicationTests`, 21 Api.IntegrationTests).
     - Frontend: 47/47 pruebas Vitest superadas al 100%. Build de producción con Vite completado sin errores.
     - Documentación exhaustiva en `docs/FACTURACION_FISCALAPI.md`.

## 📌 Hito Cumplido: Corrección de Bloqueo CORS en Producción para Cloudflare Workers (07 de Octubre, 2026)
- **Problema Reportado**:
  - Al abrir el frontend del Punto de Venta (PDV) alojado en Cloudflare Workers (`https://pos-wpcbajio.aaronarenasmartinez.workers.dev`), las solicitudes a la API de producción `https://api.wpcbajio.com/api/v1/products?...` fallaban por error de CORS:
    `Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource.`
- **Causa Raíz**:
  - La remediación de seguridad previa eliminó comodines globales (`*.workers.dev`), pero no incluyó explícitamente el origen específico y oficial del trabajador de Cloudflare del PDV (`pos-wpcbajio.aaronarenasmartinez.workers.dev`) ni configuró la sección `Cors:AllowedOrigins` en `appsettings.json`.
- **Acciones Realizadas**:
  1. `src/backend/Pos.Api/Program.cs`: `SetIsOriginAllowed` actualizado para permitir de manera segura `pos-wpcbajio.aaronarenasmartinez.workers.dev`, así como subdominios autorizados del usuario (`*.aaronarenasmartinez.workers.dev` y `*.aaronarenasmartinez.pages.dev`), además de normalización `TrimEnd('/')` en orígenes configurados dinámicamente.
  2. `src/backend/Pos.Api/appsettings.json` y `appsettings.Development.json`: Registrada la sección `Cors:AllowedOrigins` con los orígenes oficiales.
  3. `tests/backend/Pos.Api.IntegrationTests/CorsTests.cs`: Nueva suite con 7 pruebas de integración para validar CORS preflight (OPTIONS) y peticiones regulares en orígenes válidos y el bloqueo estricto ante orígenes no autorizados.
  4. Pruebas superadas al 100%: 104 pruebas del backend y 47 del frontend.
  5. Despliegue en VPS (`193.46.198.88`): Binarios `Pos.Api` actualizados, `appsettings.json` con CORS, llave SSH agregada a `authorized_keys`, `pos-api.service` reiniciado y verificado en vivo con curl (responde con cabeceras CORS válidas: `Access-Control-Allow-Origin: https://pos-wpcbajio.aaronarenasmartinez.workers.dev` y `Access-Control-Allow-Credentials: true`).

## 📌 Hito Cumplido: Remediación Integral de Vulnerabilidades Críticas y Blindaje de Seguridad (05 de Octubre, 2026 - Madrugada)
- **Objetivo**: Auditoría técnica y corrección a nivel extremo de todas las vulnerabilidades detectadas en el backend (.NET 9) y tienda en línea (Next.js), garantizando cero exposición de vectores de ataque en producción.
- **Acciones Realizadas**:
  1. **Neutralización de `simulate-webhook`**: Endpoint restringido a `Development` (`404 Not Found` en producción) y protegido con política `ventas:procesar`.
  2. **Criptografía de Contraseñas y Eliminación de Backdoor**: Reemplazada la contraseña maestra `"WPC123"` por autenticación formal con `IPasswordHasherService` (PBKDF2 HMAC-SHA256, 100,000 iteraciones); migración inline transparente para cuentas antiguas.
  3. **Mitigación de Account Takeover (ATO)**: Reseteo de contraseñas (`customers/reset-password`) ahora exige validación estricta de factor secundario de identidad (teléfono registrado del cliente).
  4. **Blindaje de Tracking de Envíos**: Eliminado `[AllowAnonymous]` global en `StripePaymentsController`; `PUT web-orders/{id}/tracking` ahora protegido con `ventas:procesar` (`401 Unauthorized` a anónimos).
  5. **Protección de Datos Personales (PII) en Contacto**: `GET /api/v1/store/contact-messages` protegido con `clientes:ver` (`401 Unauthorized` público).
  6. **Restricción Estricta de CORS**: Eliminados comodines permisivos (`.workers.dev`, `.pages.dev`); admitidos únicamente `localhost`, `127.0.0.1` y dominios oficiales de `wpcbajio.com`.
  7. **Defensa contra Ataques de Fuerza Bruta y DoS**: Implementado ASP.NET Core Sliding Window Rate Limiting (`auth`: 15 req/min, `payments`: 20 req/min) por IP.
  8. **Protección de Credenciales de Logs Serilog**: Eliminadas credenciales estáticas de código; leídas de configuración con comparación en tiempo constante (`CryptographicOperations.FixedTimeEquals`) contra ataques de temporización.
  9. **Protección de Margen de Ganancia**: `PUT system-settings/ecommerce-pricing` protegido con `catalogo:productos_editar`.
- **Despliegue y Validación en VPS (`193.46.198.88`)**:
  - Binarios `Pos.Api` desplegados en `/var/www/pos-api`; servicio `pos-api.service` activo.
  - Frontend Next.js desplegado en `/var/www/ecommerce`; PM2 `wpc-ecommerce` activo.
  - Verificación en vivo con curl: endpoints protegidos retornan 401/404 según lo diseñado.

## 📌 Hito Cumplido: Protección Crítica de Privacidad (PII) y Verificación en Rastreo de Pedidos (05 de Octubre, 2026 - Madrugada)
- **Problema Reportado**: Al pegar un enlace `/pedido/{folio}` en ventana privada/incógnito sin iniciar sesión, se mostraban los datos del pedido y datos personales del comprador a cualquier visitante no autenticado.
- **Archivos Modificados**:
  - `src/backend/Pos.Api/Controllers/v1/StripePaymentsController.cs`:
    - Endpoint `GET /api/v1/payments/stripe/orders/{folio}` ahora recibe `[FromQuery] string? email`.
    - Validación estricta: si el correo del comprador no coincide exactamente con el registrado en `sale.Cliente.Email`, el API deniega la entrega de datos devolviendo `403 Forbidden` (`requiresVerification: true`).
  - `d:\WPC-BajioEcommerce`:
    - `services/api/checkout.ts`: Incorpora `getOrderFromApiWithVerification` para gestionar respuestas 403 y estados de verificación.
    - `services/index.ts`: Eliminado el mock estático residual que devolvía datos ficticios ante fallos de búsqueda; ahora enlaza de forma segura con el servicio de API.
    - `components/order/order-status-client.tsx`: Nueva pantalla de autenticación y verificación de seguridad con icono de escudo (`ShieldCheck`); exige al visitante el correo de compra para desbloquear los datos del pedido si no tiene sesión activa.
- **Despliegue y Pruebas en Vivo**:
  - Binarios Release de `Pos.Api` desplegados en `/var/www/pos-api/` y servicio `pos-api.service` reiniciado en VPS.
  - Build de Next.js (`npm run build`) ejecutado en `/var/www/ecommerce` y proceso PM2 `wpc-ecommerce` recargado.
  - Petición sin correo o correo erróneo: `HTTP 403 Forbidden` (Zero PII leak).
  - Petición con correo legítimo del cliente: `HTTP 200 OK` (desbloqueo seguro).

## 📌 Hito Cumplido: Estandarización de Órdenes Web con IdVenta, Canal WEB y Corrección de Visualización (05 de Octubre, 2026 - Madrugada)
- **Archivos Modificados**:
  - `src/backend/Pos.Api/Controllers/v1/StripePaymentsController.cs`:
    - Creación de órdenes con folio `WEB-{IdVenta}` en lugar de UUIDs largos.
    - Metadatos Stripe actualizados con `IdVenta`, `Channel: "WEB"` y `Source: "WEB"`.
    - `GetOrderByFolio` permite consultar por `IdVenta`, `#IdVenta`, `WEB-IdVenta` o UUID antiguo.
    - Respuestas de órdenes enriquecidas con `idVenta`, `channel: "WEB"`, `createdAt`, `createdAtUtc`, `pricePerUnit`, `unitPrice`, `lineTotal`, `imageUrl` e `image`.
  - `src/frontend/pos-web/src/pages/WebOrders/WebOrdersPage.tsx`:
    - Estandarizado para mostrar `Venta #{order.idVenta}` con badge `WEB`.
  - `d:\WPC-BajioEcommerce`:
    - `types/index.ts`: Modelos `Order` y `CartItem` actualizados con `idVenta`, `channel`, `unitPrice`, `lineTotal`, `imageUrl`, etc.
    - `services/api/checkout.ts`: Normalizador centralizado de órdenes (`normalizeOrder`) que garantiza coherencia de campos y previene `NaN` e `Invalid Date`.
    - `app/cuenta/page.tsx`: Folios como `Venta #{idVenta}` + badge `WEB`, formateador de fechas seguro y miniaturas de partidas con fallback `<Package />` e importes exactos.
    - `components/order/order-status-client.tsx`: Folio con `Venta #{order.idVenta}` + badge `WEB`, fechas seguras, desglose monetario sin `NaN` y contenedor de imagen con fallback si `src` está vacío.
    - `app/pago-exitoso/page.tsx`: Folio mostrado como `Venta #{idVenta}` con badge `WEB`.
- **Validaciones**:
  - 97 pruebas backend superadas con 0 errores (`dotnet test`).
  - Build de producción Vite en `pos-web` exitoso.
  - Build de producción Next.js en `WPC-BajioEcommerce` exitoso (25 rutas compiladas).

## 📌 Hito Cumplido: Reactivación y Desbloqueo de Pagos Stripe en Producción (PR) (04 de Octubre, 2026 - Tarde / Noche)
- **Archivos Modificados**:
  - `src/backend/Pos.Api/Controllers/v1/StripePaymentsController.cs`:
    - Desbloqueo de modo de mantenimiento (`MaintenanceMode: false` por defecto en `CreatePaymentIntent` y `CreateCheckoutSession`).
    - Flexibilización de validación de monto mínimo a `$1.00 MXN` (`amountCents >= 100`).
    - Manejo de excepciones `StripeException` para retransmitir diagnósticos claros de la pasarela Stripe sin enmascarar con 502.
  - `src/backend/Pos.Api/appsettings.json` y `/var/www/pos-api/appsettings.json` en VPS:
    - Agregado `"MaintenanceMode": false` bajo `"StripeSettings"`.
  - Base de Datos SQL Server en VPS (`PosLambrinDb`):
    - Configurado `PrecioOnlineManual = 1.00` para el producto de prueba ID 106 (`7E6E4E93-02BB-478C-872F-A4659806B027`, SKU `ZWEB`) para asegurar cobro de `$1.00 MXN` exacto online sin alterar el catálogo de mostrador. Stock verificado: 10 unidades.
  - `D:\WPC-BajioEcommerce\lib\maintenance.ts` y `/var/www/ecommerce/lib/maintenance.ts` en VPS:
    - Desactivado modo de mantenimiento (`enabled: false`).
    - Eliminado el banner visible de cabecera y el cuadro de mantenimiento del carrito de compras.
- **Validaciones**:
  - 97 pruebas backend superadas (0 fallos).
  - Next.js build (`npm run build`) en VPS ejecutado exitosamente (122 rutas optimizadas).
  - `POST /api/v1/payments/stripe/create-payment-intent` verificado en vivo retornando `clientSecret` y `isLive: true`.
  - Webhooks verificados en `POST /api/v1/payments/stripe/webhook` con firma criptográfica activa.
  - Restricción de Stripe documentada: la pasarela Stripe en México exige un monto mínimo de `$10.00 MXN` por transacción; por ello, la prueba con el producto de $1.00 MXN se completa agregando 10 piezas o ajustando el umbral en Stripe.

## 📌 Hito Cumplido: Corrección de Búsqueda Predictiva de SKU (`LAM-15`), Despliegue de Catálogo y Consulta de Stock en Vivo (24 de Septiembre, 2026 - Tarde / Noche)
- **Archivos Modificados**:
  - `src/backend/Pos.Infrastructure/Services/CatalogApplicationService.cs`: Búsqueda precisa multivariante en `GetProductsAsync` y `GetProductByCodeAsync` que contempla guiones ASCII (`-`), guiones no separables Unicode (`\u2011`), variantes ortográficas y códigos compactos sin guiones (`LAM15` -> `LAM-15`). Normalización en `NormalizeSku`.
  - `src/frontend/pos-web/src/services/inventoryService.ts`: `getStockByProductId` corregido para invocar directamente el endpoint REST `GET /api/v1/inventory/product/{productId}` en vez de la búsqueda textual por GUID que arrojaba 0 resultados.
  - `src/frontend/pos-web/src/pages/Inventory/MovementCaptureModal.tsx`:
    - Identificador de entrada actualizado a `id="inventory-product-search"`.
    - Dropdown ampliado a al menos 30-50 elementos visibles (`pageSize: 100`, `maxHeight: 320px` con scroll).
    - Despliegue automático de productos al enfocar/hacer clic en el campo.
    - Eliminado el destello a 0 piezas al seleccionar un producto (`availableQuantity` como base inmediata y persistente).
  - `src/frontend/pos-web/src/pages/Receipts/PurchaseReceiptsPage.tsx`: Actualizado con `pageSize: 100` y persistencia de stock idéntica.
- **Validaciones**: 97 pruebas de backend superadas (0 fallos), 47 pruebas de frontend superadas (0 fallos), `npm run build` ejecutado exitosamente con 0 errores. Pruebas realizadas exclusivamente en DEV local.

## 📌 Hito Cumplido: Trazabilidad de Cantidad Anterior y Resultante en Movimientos y Ajustes de Inventario (24 de Septiembre, 2026 - Tarde)
- **Archivos Modificados / Creados**:
  - `src/frontend/pos-web/src/pages/Inventory/MovementCaptureModal.tsx`: Nuevo componente reutilizable para la captura de movimientos y ajustes con cálculo en tiempo real (`Stock Anterior`, `Ajuste/Movimiento` con badge de diferencia neta, y `Stock Resultante`).
  - `src/frontend/pos-web/src/pages/Inventory/InventoryMovementsPage.tsx`: Incorporación de las columnas `Stock Previo / 原库存` y `Stock Final / 最终库存`, ordenamiento por defecto por fecha descendente (`createdAtUtc desc`), ordenamiento dinámico por `previousQuantity` y `newQuantity`, exportación a Excel/PDF y botón superior `➕ Capturar Movimiento`.
  - `src/frontend/pos-web/src/pages/Inventory/InventoryListPage.tsx`: Sustitución del modal duplicado por `MovementCaptureModal.tsx`.
  - `src/frontend/pos-web/src/pages/Inventory/InventoryListPage.css`: Nuevos estilos para la tarjeta de cálculo predictivo de stock (`.inventory-stock-calculation-box`, `.inventory-stock-calculation-grid`, `.inventory-stock-diff-badge`).
  - `src/frontend/pos-web/src/i18n/index.ts`: Añadidas llaves de traducción para encabezados y previsualización de inventario en español y chino (`previousStockHeader`, `resultStockHeader`, `movementPreview`, etc.).
  - `src/backend/Pos.Infrastructure/Services/InventoryApplicationService.cs`: Añadido ordenamiento por `previousquantity`, `newquantity` y ordenamiento predeterminado por fecha más reciente descendente.
  - `tests/backend/Pos.Application.Tests/Inventory/InventoryApplicationTests.cs`: Pruebas unitarias para validar el registro de ajustes con cantidades anterior/nueva y ordenamiento por fecha descendente por defecto.
- **Validaciones**: 97 pruebas de backend superadas (0 fallos), 47 pruebas de frontend superadas (0 fallos), `npm run build` ejecutado exitosamente con 0 errores.

## 📌 Hito Cumplido: Internacionalización y Traducción Completa al Chino Simplificado (24 de Septiembre, 2026)
- **Archivos Modificados**:
  - `src/frontend/pos-web/src/i18n/index.ts`: Añadidos diccionarios completos en español (`es`) y chino simplificado (`zh`) para pedidos web, catálogo de categorías, control de inventario y desglose de corte Z de caja.
  - `src/frontend/pos-web/src/App.tsx`: Localizada la pestaña `📦 Pedidos Web (CDC)` / `📦 网店订单 (CDC)` y traducción dinámica del rol del usuario (Administrador / 管理员).
  - `src/frontend/pos-web/src/pages/WebOrders/WebOrdersPage.tsx`: Integración total con `useTranslation` (título, subtítulo, botón de actualización, tarjetas métricas, filtros, tabla con formato dual bilingüe y modal de asignación de guía de rastreo y despacho).
  - `src/frontend/pos-web/src/pages/Inventory/InventoryListPage.tsx`: Encabezados principales del módulo (`WPC Bajío 库存管理`) y campos del modal de movimientos traducidos con `t()`.
  - `src/frontend/pos-web/src/pages/Categories/CategoryListPage.tsx`: Títulos, filtros, tabla, modal de creación/edición de categorías traducidos al chino.
  - `src/frontend/pos-web/src/pages/CashShift/CashShiftPage.tsx`: Localización de métricas de cierre y corte Z (`Esperado en Caja`, `Fondo`, `Ingresos`, `Ventas/Abonos Efectivo`, `Retiros`) y advertencias de contingencia.
- **Validaciones**: 95 pruebas de backend superadas (0 fallos), 47 pruebas de frontend superadas (0 fallos), `npm run build` ejecutado exitosamente con 0 errores.

## 📌 Hito Cumplido: Observaciones y Mejoras del 18 de Septiembre (Rama `mantenimiento-observaciones-18-septiembre`)
- **Rama Git**: `mantenimiento-observaciones-18-septiembre` (sin commits realizados todavía, respetando la instrucción de esperar el VoBo del usuario tras su validación en `localhost`).
- **Resumen de Cambios**:
  1. **🛒 Punto de Venta (PDV)**:
     - Ajuste de zoom al 80% y 60%: `.main-content` a `max-width: 100%` con layout elástico para que las tarjetas de catálogo y cobro no se encojan.
     - Búsqueda en catálogo con normalización de guiones Mac Unicode (`\u2010-\u2015`, `\u2212`, etc.), case-insensitive y búsqueda estricta por SKU / código de barras con y sin guiones.
     - Escáner superior con búsqueda estricta por código de barras.
     - Carga de categoría inicial con `pageSize: 500` (desplegando los 104+ productos) y animación spinner de carga al cambiar de categoría.
     - Campo obligatorio de Referencia Bancaria (hasta 50 caracteres) en pagos con Tarjeta o Mixtos; modal de alerta ante omisión y anexado auditado en notas.
     - Regla de mayoreo para *Lambrin Interior 格栅板* (2+ cajas todo a mayoreo; 1 caja a mayoreo y sueltas a menudeo; <1 caja menudeo) reflejada en el cálculo del carrito y del backend, con badge visual en fila completa sin desbordar.
     - Calculadora de m² dual (por dimensiones `Alto × Ancho` en metros o directo en m²) con modal ampliado y responsivo (`min(780px, 96vw)`), botones estilizados con color corporativo activo, 100% traducida a Chino y Español, buscador predictivo con despliegue de los 23+ productos completos (LAM-01 a LAM-23 y catálogo remoto hasta 200 items), con `overscroll-behavior: contain` para scroll independiente y fluido sin desbordes.
     - Descuento manual modificado a porcentaje (ej. 15 = 15%) y recálculo exacto de IVA sobre la base gravable (`subtotal - descuento`).
     - Comprobante térmico de venta fijado en español (sin chino) y retiro del badge `PRÓXIMAMENTE` junto a `www.wpcbajio.com`.
     - Soporte multilínea para nombres largos de productos y cuadrícula de 3 columnas para mayor legibilidad.
  2. **🔐 Login**: Campos de correo y contraseña limpios por defecto (`''`).
  3. **🏷️ Navbar y Footer**: Eliminado botón de modo claro; pie de página bilingüe oficial.
  4. **📄 Contratos A4**: Generación simultánea de plantilla en Chino Simplificado al guardar plantilla en español.
  5. **🏭 Inventario (Capturar Movimiento)**: Buscador predictivo por SKU/código de barras en el modal con existencias en tiempo real.
  6. **📦 Módulo de Recibos de Compra**: Módulo completo end-to-end (entidad `ReciboCompra`, servicio, controlador REST, migración EF Core, pruebas en backend y página React con modal de captura, ajuste dinámico de stock y voucher térmico de 80mm). Columnas de la tabla y exportación en formato simultáneo bilingüe Español / Chino (`Folio / 单号`, `Fecha / 日期`, `ID Prod. / 产品编号`, `Producto / 产品说明`, `SKU / Código / 货号`, `Precio Costo / 成本价`, `Precio Venta / 零售价`, `Stock Previo / 原库存`, `Cantidad / 进货数量`, `Stock Resultante / 最终库存`, `Operador / 操作员`, `Acciones / 操作`).
- **Validaciones**: 95 pruebas de backend superadas (0 fallos), 47 pruebas de frontend superadas (0 fallos), build de producción exitoso.

## 📌 Hito Cumplido: Script Maestro en Python para Inicio Simultáneo de Servicios (`iniciar_servicios.py`)
- **Archivos Creados**:
  - `D:\Proyecto_PDV-CDC\iniciar_servicios.py`: Script maestro en Python 3 que inicia y monitorea concurrentemente los 5 servicios (Backend .NET, Visor de Etiquetas, PDV, Media Studio y E-commerce). Soporta resolución dual IPv4/IPv6 (`socket.create_connection`), chequeo de puertos ocupados, health checks HTTP en vivo y terminación en árbol (`taskkill /F /T`) al presionar `Ctrl+C`.
  - `D:\Proyecto_PDV-CDC\iniciar_servicios.bat`: Acceso directo para ejecutar el iniciador con un solo doble clic desde el Explorador de Windows.
- **Uso**: `python iniciar_servicios.py` (o doble clic al `.bat`) para encender todo; `python iniciar_servicios.py --status` para ver el estado de los puertos al instante.

## 📌 Hito Cumplido: Corrección de Bucle Infinito en Catálogo de Productos (`Cargando datos...`)
- **Archivos Modificados**:
  - `src/frontend/pos-web/src/pages/Products/PaginaCatalogoProductos.tsx`
  - `src/frontend/pos-web/src/hooks/usePagination.ts`
- **Descripción**: Se erradicó el bucle infinito de re-renders que mantenía la vista del Catálogo en `Cargando datos...` y saturaba el API del VPS. Se desacopló la referencia mutable del objeto `pagination` de las dependencias de `cargarDatos` (ahora solo depende de las primitivas `pageNumber` y `pageSize`), utilizando `useRef` para actualizar los metadatos de paginación sin invalidar la función. Además, se memoizó el objeto de retorno en `usePagination.ts` y se aplicó verificación de cambios (`prev !== next`) en `setPaginationFromResult`.
- **Resultados**: La vista de Catálogo carga de inmediato en una única petición sin bucles, sin bloqueos y con consumo mínimo en servidor.
- **Pruebas**: 47/47 pruebas de Vitest superadas (100%), build exitoso (11.99s). Commits sincronizados en `main`, `version-final-de-PR` y `feature/ecommerce-fase-2`. Servicio `pos-api` reiniciado en VPS.

## 📌 Hito Cumplido: Propuesta Final de Fondo de Arte Dorado / Montañas en Etiquetas Térmicas (100mm × 60mm)
- **Archivos/Scripts Involucrados**:
  - `D:\Visozr Etiquetas\src\assets\label-bg-art.png` y `public/label-bg-art.png`: Obra artística de ondas y cordilleras doradas fluidas con destellos provista por el usuario.
  - `scripts/development/apply_golden_mountains_background.js`: Ajusta el fondo en `ThermalLabel.tsx` con encuadre horizontal centrado (42% vertical), marco perimetral dorado fino (`rgba(205, 165, 75, 0.55)`) y sombra interior cálida.
  - `D:\Visozr Etiquetas\src\components\ThermalLabel.tsx`: Contraste tipográfico perfeccionado en café tostado (`#241408`), marco de miniatura de producto con ribete dorado e integración fluida con la descarga de PDF e impresión física.
- **Resultados**:
  - Propuesta estética de lujo premium acorde a la identidad oriental / WPC de la marca.
  - Textos, códigos de barras y precios 100% legibles y listos para mostrar al cliente.
  - Build de Vite completado exitosamente (15.31s, 0 errores).

## 📌 Hito Cumplido: Sincronización de Catálogo PR a Local, Corrección UTF-8 y Planillas Tamaño Carta (8 por hoja)
- **Archivos/Scripts Involucrados**:
  - `scripts/development/Sync-PrToLocal.ps1`: Sincronización directa y segura desde SQL Server VPS (`193.46.198.88:1433`) a local `localhost` (`PosLambrinDb`).
  - `scripts/development/Generate-EtiquetasPresets.ps1`, `fix_etiquetas.js` y `apply_letter_sheet_mode.js`: Extracción y normalización de presets con UTF-8 puro, separador `x` en dimensiones y módulo de impresión/PDF en hoja Carta.
  - `D:\Visozr Etiquetas\src\data\presets.ts`: Catálogo completo de 104 productos activos preconfigurados para etiquetas térmicas.
  - `D:\Visozr Etiquetas\src\components\ThermalLabel.tsx`: Eliminada la clase `truncate` y `uppercase`, ajuste dinámico de tamaño de fuente (8pt - 11pt) y renderizado multilínea completo para evitar recortes en nombres de colores bilingües largos (ej. `Madera rojiza‑marrón —— 红棕木色`). Limpieza de mojibake `Ã—` a `x`.
  - `D:\Visozr Etiquetas\src\components\BatchQueue.tsx`: Botones destacados **`📄 Descargar PDF Carta (13 Hojas)`** e **`🖨️ Imprimir en Carta`** (8 etiquetas por hoja de 100mm × 60mm), junto con soporte para rollo térmico continuo.
  - `D:\Visozr Etiquetas\src\components\LabelPreviewCard.tsx`: Selector de vista previa dual `100×60mm` vs `Hoja Carta (8x)`.
  - `D:\Visozr Etiquetas\src\components\PrintContainer.tsx` & `src/index.css`: Reglas `@media print` de alta precisión para hoja Carta (215.9mm × 279.4mm) con retícula de 2 columnas × 4 filas y guías de corte punteadas.
  - `D:\Visozr Etiquetas\src\utils\pdfExport.ts`: Nueva función `exportBatchQueueToLetterPdf` para exportar el catálogo completo en un PDF de 13 páginas tamaño Carta listo para imprimir en cualquier impresora de oficina.
- **Resultados**:
  - Base de datos local actualizada con los 105 productos reales capturados en producción (13 categorías y 105 stocks).
  - Sistema de etiquetas listo para imprimir o generar en PDF las 104 etiquetas de los productos en 13 hojas tamaño Carta con 1 solo clic.
  - Build de `D:\Visozr Etiquetas` verificado con 0 errores y probado visualmente con navegador.

## 📌 Hito Cumplido: Categorías en PDV y Catálogo (Lambrín Interior por Defecto y Libre Manejo)
- **Archivos Modificados**: `PaginaPuntoVenta.tsx`, `PaginaCatalogoProductos.tsx`.
- **Descripción**: Tanto el Punto de Venta como el Catálogo cargan inicialmente filtrados por la categoría principal `Lambrin Interior 格栅板` (`7938934b-d6cb-44fd-98de-b0645c66017d`), mostrando únicamente sus productos en el render inicial. El combo `<select>` carga todas las categorías activas de la base de datos más la opción de "Todas las Categorías", permitiendo al usuario cambiar libremente a cualquier categoría con actualización inmediata de productos.
- **Pruebas**: 47/47 pruebas de Vitest aprobadas al 100%, compilación `tsc && vite build` exitosa.

## 📌 Hito Cumplido: Optimización de Almacenamiento y Entrega de Imágenes de Productos (Desplegado a Producción VPS y Main)
- **Rama Git**: `main` (desplegada y sincronizada con `origin/main` en commit `3551058`).
- **Descripción**: Migración integral de la gestión de imágenes de productos desde cadenas Base64 embebidas en SQL Server (`nvarchar(max)`) y JSON a almacenamiento físico en disco en formato WebP optimizado con 3 variantes (`thumbnail.webp`, `pos.webp`, `preview.webp`), servido mediante ASP.NET Core Static Files con caché HTTP (`Cache-Control: public, max-age=604800, must-revalidate`), carga ágil en Punto de Venta (`pageSize: 40`), escaneo resiliente por código de barras hacia la API y migración automática de imágenes legadas.
- **Entregables Realizados y Desplegados en VPS (`193.46.198.88`)**:
  1. **Servicio de Almacenamiento WebP (`LocalProductImageStorageService`)**:
     - Motor `SixLabors.ImageSharp` 4.1.1 con preservación de aspect ratio, eliminación de metadatos EXIF y compresión WebP por variante: `thumbnail.webp` (128x128, q75), `pos.webp` (256x256, q80), `preview.webp` (512x512, q85).
     - Almacenamiento atómico por carpeta `{productId}/` fuera de `wwwroot`.
     - Carpeta de almacenamiento configurada y creada en producción: `/var/wpcbajio/data/products` (propiedad de `www-data:www-data`).
     - Protección contra path-traversal y nombres fijos para prevenir archivos huérfanos.
  2. **Entrega de Archivos Estáticos y Caché**:
     - Configuración en `Program.cs` mediante `PhysicalFileProvider` sobre `/var/wpcbajio/data/products` expuesto en `/products`.
     - Encabezado `Cache-Control: public, max-age=604800, must-revalidate` (7 días).
  3. **Endpoints REST de Productos**:
     - `POST /api/v1/products/{id}/image`: Carga multipart (`IFormFile`) con validación de tipo MIME y tamaño máx 10 MB.
     - `DELETE /api/v1/products/{id}/image`: Eliminación física de archivos y reseteo de `ImagenUrl`.
     - `POST /api/v1/products/migrate-base64-images`: Migración masiva ejecutada en producción con respuesta 200 OK.
  4. **Optimización de Memoria y Rendimiento SQL**:
     - Removido `.Include(p => p.Imagenes)` en `GetProductsAsync`. Las consultas de catálogo ahora retornan DTOs esbeltos sin sobrecarga de memoria.
  5. **Frontend React + Vite**:
     - `PaginaCatalogoProductos.tsx`: Reemplazada la conversión Base64 en frontend por vista previa con `URL.createObjectURL` y subida multipart vía `uploadProductImage`. `loading="lazy"` y `decoding="async"`. Incorporado botón flotante `✕` en la esquina superior de la miniatura para remover la foto actual o descartar una selección por error de forma minimalista sin afectar ni reiniciar ningún otro dato del producto.
     - `PaginaPuntoVenta.tsx`: Carga inicial reducida a `pageSize: 40`. Si un código de barras escaneado no está en memoria, `findAndAddProduct` consulta `GET /api/v1/products/code/{code}` para agregarlo al carrito al instante.
     - `apiClient.ts`: Soporte nativo de `FormData` sin forzar `application/json`.
  6. **Pruebas y Verificación**:
     - Backend: 84/84 pruebas unitarias y de integración superadas (100%).
     - Frontend: 47/47 pruebas Vitest aprobadas (100%) y build exitoso (11.69s, 0 errores).
- **Estado del Despliegue en VPS (Producción)**:
  - Carpeta física creada en VPS: `/var/wpcbajio/data/products` con permisos `www-data:www-data`.
  - Configuración `/var/www/pos-api/appsettings.json` actualizada con `"ProductImagesPath": "/var/wpcbajio/data/products"`.
  - Compilación `dotnet publish -c Release` ejecutada exitosamente en el servidor.
  - `pos-api.service` reiniciado y respondiendo 200 OK en `https://api.wpcbajio.com/api/v1/health`.
  - Endpoint `POST /api/v1/products/migrate-base64-images` ejecutado con token admin (0 errores).

## 📌 Hito Cumplido: Unificación Visual de Columnas de Precio en Catálogo de Productos
- **Rama Git**: `mantenimiento/unificar-columna-precios-catalogo`
- **Descripción**: Integración de las columnas "Precio Menudeo / 零售价" y "Precio Mayoreo / 批发价" en una sola columna visual en la tabla del catálogo para compactar el espacio horizontal, preservando la exportación a Excel y PDF con ambas columnas por separado.
- **Entregables Realizados**:
  1. **Columna Combinada en Catálogo**: Encabezado unificado `Precios (Men. / May.) / 价格 (零售 / 批发)` con ordenamiento interactivo por precio base. Celdas estilizadas con badges compactos `Men` y `May` con sus montos y umbrales mínimos.
  2. **Exportación Normal Preservada**: `exportConfig` mantiene las columnas independientes `unitPrice` y `wholesalePrice`, asegurando que las descargas en Excel y PDF no se vean alteradas.
  3. **Diccionario i18n Bilingüe**: Añadida clave `pricesCombined` para español y chino simplificado.
  4. **Pruebas y Verificación**: 47/47 pruebas Vitest y 77/77 pruebas de backend superadas con build limpio.

## 📌 Hito Cumplido: Optimización de Espacios en Punto de Venta (Ampliación de Catálogo y Compactación de Checkout)
- **Rama Git**: `mantenimiento-ajuste-pdv`
- **Descripción**: Reducción de ancho y compactación de espaciados de la columna de checkout (`.pos-checkout`) para permitir que la vista de catálogo (`.pos-card`) gane amplitud y muestre más tarjetas de productos en pantalla aprovechando al máximo la resolución horizontal.
- **Entregables Realizados**:
  1. **Redistribución de Columnas**: `.pos-layout` ahora asigna `minmax(0, 1fr)` al catálogo y fija/clampa el checkout a `clamp(295px, 24vw, 340px)`.
  2. **Grid de Productos Expandido**: `.pos-products` con rejilla auto-fill `minmax(235px, 1fr)` y gap `0.75rem`.
  3. **Compactación de Checkout**: Margen, relleno, tipografía y elementos de cliente, lista de carrito, facturación, totales, modalidad de pago y botón de cobro optimizados con dimensiones más esbeltas y sin espacios en blanco excesivos.
  4. **Pruebas y Verificación**: 47/47 pruebas Vitest aprobadas, build de producción 100% limpio y 77/77 pruebas de backend operativas.

## 📌 Hito Cumplido: Sincronización de Base de Datos Producción (VPS) a Entorno Local DEV y Respaldo Semanal
- **Descripción**: Respaldo completo de la base de datos de producción (`193.46.198.88`) extraído desde el contenedor `mssql-server` y restaurado localmente en `PosLambrinDb` y `PosLambrinDb_Dev`, permitiendo al desarrollador trabajar con los datos reales que los usuarios han insertado en producción (74 productos, 13 categorías, 74 existencias en stock, clientes y auditoría), además de la automatización de respaldos periódicos semanales en el VPS.
- **Entregables Realizados**:
  1. **Respaldo Automatizado en VPS**: Script `/usr/local/bin/backup-pos-db.sh` con política de retención para 8 semanas, registro en `/var/log/pos-db-backup.log` y archivo `/var/backups/pos-database/PosLambrinDb_latest.bak`.
  2. **Tarea Programada (Cron)**: Configurada en `crontab` de root para ejecutarse cada domingo a las 03:00 UTC (21:00 MX).
  3. **Restauración en SQL Server Local**: Restauración con reubicación física de archivos (`WITH MOVE`) en `PosLambrinDb` (base principal de desarrollo) y `PosLambrinDb_Dev` (réplica limpia de apoyo).
  4. **Script PowerShell Local**: [scripts/development/sync-db-from-vps.ps1](file:///d:/Proyecto_PDV-CDC/scripts/development/sync-db-from-vps.ps1) para sincronizar en cualquier momento con un solo comando.
  5. **Pruebas y Verificación**: 77/77 pruebas unitarias y de integración de backend pasando al 100%.

## 📌 Hito Cumplido: Estandarización Bilingüe Universal de Columnas y Alertas Reactivas
- **Rama Git**: `main`
- **Descripción**: Estandarización al 100% en formato `"Nombre Español / Nombre Chino"` de todas las cabeceras de columnas en todos los módulos de la aplicación, y reactividad dinámica total para alertas críticas del sistema (como la alerta de turno de caja sin aperturar y notificaciones de ventas) para que conmuten instantáneamente de idioma al cambiar a Chino Simplificado (`zh`).
- **Entregables Realizados**:
  1. **Alertas Reactivas del Punto de Venta**:
     - `noOpenShiftBanner` (*"⚠️ Atención: La caja no ha sido aperturada. Debes realizar la apertura de caja para poder procesar ventas."* / *"⚠️ 注意：钱箱尚未开扎。必须先开扎钱箱才能进行销售。"*) ahora es renderizado de forma reactiva con `t('noOpenShiftBanner')`. Al hacer clic en el botón de idioma del encabezado, el mensaje se traduce al instante en pantalla.
     - El manejador de notificaciones `notice` en `PaginaPuntoVenta.tsx` admite claves reactivas `key` con parámetros, traduciendo de inmediato validaciones de carrito vacío, montos de anticipo, descuentos excedentes, etc.
  2. **Estandarización de Encabezados de Tablas (Bilingüe Dual)**:
     - Formato `"Nombre Español / Nombre Chino"` establecido en `es` y `zh` para todas las columnas en:
       - Catálogo de Productos
       - Categorías
       - Clientes y modal de historial de compras
       - Inventario (existencias y umbrales)
       - Movimientos de Inventario y Evidencia Física
       - Cotizaciones y modal de desglose de partidas
       - Histórico de Ventas
       - Operaciones Comerciales (Transacciones, Abonos y Devoluciones)
       - Turno de Caja (Arqueos, Movimientos y Cortes)
       - Usuarios y Roles
       - Explorador de Bitácora de Auditoría
       - Reportes Ejecutivos
  3. **Verificación y Pruebas**:
     - `npm run test` (Vitest): 47/47 pruebas superadas al 100%.
     - `npm run build` (`tsc && vite build`): completado con éxito en 11.13s con 0 errores y 0 advertencias.

## 📌 Hito Cumplido: Mejoras al Módulo de Productos "Catálogo" (ABC, IdProducto e Inventario Actual)
- **Rama Git**: `main` (commit `33eee52`, fusionado desde PR #4 `mantenimiento/mejoras-catalogo-productos`).
- **Pull Request**: [PR #4 Merged](https://github.com/UTLCDA/Proyecto_PDV-CDC/pull/4)
- **Descripción**: Mejoras al módulo de productos "catálogo" solicitadas por el cliente.
- **Entregables Realizados**:
  1. **Identificador Secuencial `IdProducto` (Identity 1-1)**:
     - Añadido a `Producto` / `Product` y a todos los DTOs de backend y modelos de frontend.
     - Mapeado con EF Core `ValueGeneratedOnAdd()` y columna SQL Server `IDENTITY(1,1)` con índice único `IX_Products_IdProducto`. El GUID original `Id` sigue operando como PK/FK en todo el sistema.
     - Creada la migración `20260909231411_AddIdProductoIdentityToProducts`.
     - Implementado soporte simulador en `PosDbContext.cs` para pruebas unitarias con proveedor `InMemory`.
     - Soporte de búsqueda por texto extendido para coincidencia numérica exacta contra `IdProducto`.
     - Mostrado en la tabla del catálogo como primera columna "ID" (`#{p.idProducto}`) con capacidad de ordenamiento interactivo.
  2. **Columna "Inventario Actual" en Tabla de Catálogo**:
     - Columna visible con piezas totales en inventario (`p.availableQuantity`).
     - Insignia de estado (suficiente, bajo stock, agotado).
     - Soporte de ordenamiento por existencias (`stock`) en backend y frontend.
     - Textos internacionalizados en español y chino simplificado.
  3. **Acción de Eliminar Producto (Baja Lógica / ABC Completo)**:
     - Endpoint `DELETE /api/v1/products/{id}` en `ProductsController.cs` con política `Catalog.ProductsEdit`.
     - Desactivación lógica `EstaActivo = false` y registro en bitácora de auditoría (`PRODUCT_DELETED`).
     - Parámetro de filtrado `includeInactive` (por defecto `false`), con selector en frontend para alternar entre "Activos", "Inactivos" y "Todos".
     - Botón "🗑️ Eliminar" en acciones con modal bilingüe de confirmación y advertencia.
- **Pruebas y Verificación**:
  - Backend: 77/77 pruebas superadas al 100% (`dotnet test src/backend/Pos.sln`).
  - Frontend: 47/47 pruebas superadas al 100% (`npm run test`).
  - Build Frontend: `npm run build` completado exitosamente sin errores ni advertencias en 12.00s.

## 📌 Hito Cumplido: Diagnóstico y Corrección de Visibilidad de Paginación en Frontend
- **Rama Git**: `mantenimiento/mejoras-v2`
- **Problema Reportado**: "no veo la numeracion en el front".
- **Causa Raíz Resuelta**:
  1. `PagedResult<T>` en C# implementaba `IReadOnlyList<T>` / `IEnumerable`. System.Text.Json serializaba el resultado como un arreglo JSON plano `[...]` perdiendo las propiedades de metadatos (`totalItems`, `totalPages`, etc.).
  2. En el cliente, al no recibir `totalItems`, `pagination.totalItems` quedaba en `0`.
  3. `TablePagination.tsx` hacía un `return null` temprano cuando `totalItems === 0`.
- **Solución y Estado**:
  1. Se desacopló `PagedResult<T>` de `IEnumerable` en el Backend; ahora serializa un objeto JSON completo `{ items, pageNumber, pageSize, totalItems, totalPages }`.
  2. Se ajustaron los controladores y suites de integración para acceder a `Items`.
  3. Se robusteció `usePagination.ts` (`setPaginationFromResult`) para tolerar camelCase y PascalCase.
  4. Se actualizó `TablePagination.tsx` para mostrar siempre la barra de paginación incluso con 0 registros ("Sin registros" / "暂无数据" con botones deshabilitados) y mostrar siempre los números `[ 1 ]` cuando hay registros.
- **Validación**:
  - Backend: 73/73 pruebas xUnit superadas al 100%.
  - Frontend: 47/47 pruebas Vitest superadas al 100%.
  - Build Frontend: `npm run build` completado exitosamente en 10.72s.

## 📌 Hito Cumplido: Implementación del Componente de Paginación Numerada Reutilizable (`TablePagination`) en Todas las Tablas
- **Rama Git**: `mantenimiento/mejoras-v2`
- **Alcance Entregado**:
  1. **Componente Reutilizable (`TablePagination.tsx`)**:
     - Paginador numerado con lógica de elipses de 7 botones (`getPageNumbers`).
     - Botones `«`, `‹`, `›`, `»` y botones numéricos con estado activo e indicador accesible `aria-current="page"`.
     - Selector de tamaño de página (25, 50, 100).
     - Resumen dinámico bilingüe (Español / Chino Simplificado).
     - Handlers seguros `handlePageChange` y `handlePageSizeChange` que validan límites y estado `disabled`.
  2. **Diseño Visual (`TablePagination.css`)**:
     - Integración con los tokens oficiales de diseño de WPC Bajío (`--primary-main`, `--border-subtle`, `--background-surface`, `--text-main`, etc.).
     - Diseño responsive para pantallas móviles (< 640px).
  3. **Despliegue en el 100% de las Tablas Paginadas**:
     - `SalesHistoryPage.tsx`, `PaginaCatalogoProductos.tsx`, `CategoryListPage.tsx`, `CustomerListPage.tsx`, `InventoryListPage.tsx`, `InventoryMovementsPage.tsx`, `QuoteListPage.tsx`, `CommercialOpsPage.tsx` (Transacciones, Abonos, Devoluciones), `CashShiftPage.tsx` (Movimientos e Historial), `PaginaUsuarios.tsx`, `AuditLogPage.tsx`.
- **Pruebas y Verificación**:
  - Frontend: `npm run build` (`tsc && vite build`) completado con 0 errores; pruebas unitarias para `getPageNumbers` integradas en `usePagination.test.ts`.
  - Backend: 72/72 pruebas pasadas (100% xUnit).

## 📌 Hito Cumplido: Extensión de Paginación de Alto Rendimiento y Desacoplamiento de Filtros en Todo el Sistema
- **Rama Git**: `mantenimiento/mejoras-v2`
- **Alcance Completado**:
  - Tras el éxito de la optimización en el módulo de Ventas, se aplicó la misma arquitectura a **todos los demás módulos paginados del sistema**:
    1. **Backend**: Conteo limpio sin colecciones dependientes `CountAsync()`, selección paginada de IDs con `Skip().Take().Select(x => x.Id)`, y consulta dividida `.AsSplitQuery()` para hidratar entidades completas preservando el orden en:
       - `CommercialOperationsService.cs` (Cotizaciones, Devoluciones, Abonos, Transacciones).
       - `CatalogApplicationService.cs` (Productos, Categorías, Clientes).
       - `InventoryApplicationService.cs` (Existencias, Movimientos de Inventario).
       - `CashShiftApplicationService.cs` (Historial de Turnos).
       - `UserApplicationService.cs` (Usuarios y Roles).
       - `ReportingApplicationService.cs` (Bitácora de Auditoría).
    2. **Frontend**:
       - Desacoplamiento de efectos de carga respecto a los estados de los campos de entrada (`appliedFilters` triggered solo por Submit o Limpiar filtros).
       - Caching de catálogos y opciones estáticas para no re-consultar en cada cambio de página.
       - Desacoplamiento de llamadas en `CommercialOpsPage` (carga estática separada de la paginación de transacciones, devoluciones y abonos).
       - Inclusión uniforme del botón "Limpiar filtros" en todos los formularios de filtrado.
- **Validación**:
  - Pruebas Backend: 72/72 pasadas (100% xUnit: Domain, Application e IntegrationTests).
  - Build Frontend: `tsc && vite build` completado exitosamente (código 0).

## 📌 HotFix de Rendimiento: Corrección de Timeout (15s) en Filtro de Fechas de Ventas
- **Rama Git**: `mantenimiento/mejoras-v2`
- **Problema Resuelto**: Al filtrar por fechas en el Histórico de Ventas, la petición tardaba más de 27 segundos en responder, lo que disparaba la cancelación por timeout (15s) en el cliente React ("Tiempo de espera agotado al conectar con el servidor (15s). Verifica la conexión.").
- **Cambios Realizados**:
  - **Backend**:
    - `SaleApplicationService.cs`: Optimización de `GetSalesAsync` separando el conteo `totalItems` a una consulta limpia sin `Include` (< 5 ms) y aplicando paginación en dos fases (recuperación de IDs paginados primero, y posterior hidratación con `.AsSplitQuery()` para evitar explosión cartesiana). El tiempo de base de datos disminuyó de 25,007 ms a ~41 ms.
    - Limpieza de inclusiones redundantes en `GetSalesSummaryAsync`.
    - Inclusión preventiva de `.AsSplitQuery()` en `BuildSaleQuery()`, `BuildQuoteQuery()` y `BuildReturnQuery()`.
  - **Frontend**:
    - `SalesHistoryPage.tsx`: Desacoplamiento de `loadSales` para que opere exclusivamente sobre `appliedFilters` y no sobre las variables de estado reactivas del formulario. Las peticiones ahora se despachan únicamente al hacer clic en "Buscar" / enviar formulario, al limpiar filtros o al cambiar de página / ordenamiento.
- **Validación**:
  - Pruebas automatizadas: Vitest 42/42 pasadas (100%), xUnit 72/72 pasadas (100%).
  - Prueba en navegador real con subagente: Consulta del rango `01/09/2026` a `09/09/2026` respondiendo de inmediato con 17 registros y métricas sin errores.

## 📌 Hito Cumplido: Paginación Server-Side Universal en Todo el PDV (Mejoras v2)
- **Rama Git**: `mantenimiento/mejoras-v2`
- **Implementación Full-Stack**:
  - Backend: Modelo unificado `PagedResult<T>`, clase `PagedRequest.cs` y helper `QueryPaging.cs` aplicando `.Skip((pageNumber - 1) * pageSize).Take(pageSize)` en SQL Server con Entity Framework Core 9.
  - Frontend: Hook reutilizable `usePagination.ts` (con pruebas unitarias), componente de paginación accesible `TablePagination.tsx` y utilidad `loadAllPagesForExport` para exportaciones completas en PDF y Excel.
- **Módulos Integrados (11/11)**:
  1. 🧾 **Histórico de Ventas**: Paginación con métricas globales preservadas.
  2. 💵 **Corte de Turno y Caja**: Paginación dual para historial y movimientos.
  3. 📑 **Cotizaciones**: Paginación server-side con filtros de fecha y estado.
  4. 💳 **Operaciones Comerciales**: Paginación dual para transacciones/abonos y devoluciones.
  5. 📦 **Catálogo de Productos WPC Bajío**: Paginación server-side con filtros por categoría y ordenamiento.
  6. 📁 **Catálogo de Categorías WPC Bajío**: Paginación server-side y búsqueda.
  7. 👥 **Directorio de Clientes**: Paginación server-side con debounce de búsqueda.
  8. 🏭 **Control de Inventarios WPC Bajío**: Paginación server-side con escáner de código de barras.
  9. 📋 **Movimientos de Inventario**: Paginación server-side por fechas y tipo de movimiento.
  10. 👤 **Gestión de Usuarios y Roles**: Paginación server-side de usuarios y gestión de roles.
  11. 🔍 **Bitácora Central de Auditoría**: Paginación server-side con filtros por usuario, módulo y resultado.
- **Suite de Pruebas y Compilación**:
  - Frontend: Vitest 42/42 pasadas (100%), build de producción `tsc && vite build` completado sin errores.
  - Backend: xUnit 72/72 pasadas (100%), `dotnet build` con 0 errores y 0 advertencias.
- **Validación Local**: Sistema levantado y listo para validación por parte del desarrollador humano antes de commit y push.

## 📌 Hito Cumplido: Ordenamiento de Tablas (Table Sorting de 3 Estados)
- **Implementación**: Sistema unificado y tipado con `useTableSort.ts` y `SortableTh.tsx`.
- **Comportamiento**: 1er clic ascendente (↑), 2do clic descendente (↓), 3er clic restablece (⇅).
- **Módulos Integrados**: Histórico de Ventas, Transacciones y Abonos, Catálogo de Productos, Catálogo de Categorías, Control de Inventarios y Movimientos de Inventario.

## 📌 Hito Cumplido: Producción 100% Operativa en la Nube
- **Frontend SPA**: Desplegado en Cloudflare (`https://pos-wpcbajio.aaronarenasmartinez.workers.dev` / `https://pos.wpcbajio.com`) con Vite 6.4.3, React 18, y enrutamiento SPA mediante Wrangler Assets.
- **Backend .NET 9**: Operando en VPS Ubuntu 26.04 (`193.46.198.88`) bajo Nginx y `systemd` (`pos-api.service`) accesible vía `https://api.wpcbajio.com/api/v1`.
- **SQL Server 2022 Express**: Corriendo en contenedor Docker (`mssql-server`) con persistencia en `/var/opt/mssql`, base unificada `PosLambrinDb` y 26 tablas físicas autoritativas.
- **Validación**: Autenticación JWT y acceso al panel administrativo validados con éxito en tiempo real.

## 📌 HotFix — Carga de Imágenes HEIC, Compresión Canvas y Edición de Productos
1. **Soporte de Imágenes HEIC / HEIF**: Integrada librería `heic2any` con importación dinámica para convertir fotos de iPhone/iPad a JPEG automáticamente sin bloquear la carga.
2. **Compresión Canvas y Eliminación del Límite de 2 MB**: Utilidad reutilizable `imageProcessor.ts` (máx. 1200px, JPEG 82%) optimiza fotos a ~100–250 KB Base64.
3. **Edición y Reemplazo de Imágenes en Productos**: Modal de edición refleja foto actual, permite reemplazarla o eliminarla ("✕ Quitar foto").
4. **Evidencia Física en Inventarios**: Soporte HEIC y auto-compresión para fotos de evidencia.

## 📌 Feature — Conversión SKU a Guiones, Campo Color y Ficha Técnica PDF
- **Estandarización de SKU**: Al ingresar espacios en el SKU, se convierten automáticamente en guiones `-`.
- **Campo de Color y Persistencia BD**: Campo Color en formulario, dominio C# y columna `Color` en SQL Server `PosLambrinDb`.
- **Descarga de Ficha Técnica PDF (`technicalSheetGenerator.tsx`)**: Acción `📄 Ficha Técnica` en tabla con desglose de precios y regla comercial.

## 📌 Feature — SKU Libre Captura y Código de Barras Dinámico en Base64
- **SKU de Libre Captura**: Sin prefijo forzado `WPC-`.
- **Generador de Código de Barras Code 128 (`barcodeGenerator.ts`)**: Generación en tiempo real en Canvas, renderizado visual, guardado en Base64 y descarga de etiqueta PNG.

## 📌 Control de Acceso y Roles
- **Transacciones restringidas a Cajero**: Pestaña `💳 Transacciones` y endpoint `/api/v1/payments/transactions` requieren `reportes:ver_ventas`.
- **Codificación de Acentos y Edición de Roles**: Unicode NVARCHAR verificado en `PosLambrinDb` y gestión de permisos/roles.

## Despliegue y Compilación en VPS de Producción
1. **Instalación de .NET 9 SDK**:
   - Instalado SDK versión `9.0.317 linux-x64` en el servidor VPS (`193.46.198.88`) en `/usr/share/dotnet`.
2. **Clonado del Repositorio**:
   - Repositorio clonado en `/root/Proyecto_PDV-CDC` sobre la rama `version-final-de-PR` (commit `26fc950`).
3. **Compilación y Publicación**:
   - Ejecutado `dotnet publish /root/Proyecto_PDV-CDC/src/backend/Pos.Api/Pos.Api.csproj -c Release -o /var/www/pos-api/`.
4. **Protección de Configuración**:
   - `appsettings.json` preservado intacto con credenciales de SQL Server 2022 y JWT de producción.
5. **Nginx Reverse Proxy**:
   - Añadido `client_max_body_size 50M;` a `/etc/nginx/sites-available/pos-api` y recargado Nginx para evitar el error `413 Request Entity Too Large`.
6. **Estado del Servicio**:
   - `pos-api.service` reiniciado y respondiendo 200 OK en `https://api.wpcbajio.com/api/v1/health`.

## Resumen del HotFix Realizado
1. **Soporte de Imágenes HEIC / HEIF**:
   - Integrada librería `heic2any` con importación dinámica para convertir fotos tomadas en iPhones/iPads a JPEG automáticamente en el cliente sin bloquear la carga.
2. **Compresión Canvas y Eliminación del Límite de 2 MB**:
   - Creación de utilidad reutilizable [`imageProcessor.ts`](file:///d:/Proyecto_PDV-CDC/src/frontend/pos-web/src/utils/imageProcessor.ts) que redimensiona fotos a máximo 1200px y las comprime a JPEG calidad 82%.
   - Fotos pesadas de 5 MB a 20 MB se comprimen a ~100–250 KB en Base64, evitando errores de memoria, límites de red y saturación de la base de datos.
3. **Edición y Reemplazo de Imágenes en Productos**:
   - En [`PaginaCatalogoProductos.tsx`](file:///d:/Proyecto_PDV-CDC/src/frontend/pos-web/src/pages/Products/PaginaCatalogoProductos.tsx), el modal de edición ahora refleja correctamente la foto actual del producto, permite cambiarla por una nueva (con cualquier formato/peso) o eliminarla con el botón "✕ Quitar foto".
4. **Evidencia Física en Inventarios**:
   - Actualizado [`InventoryListPage.tsx`](file:///d:/Proyecto_PDV-CDC/src/frontend/pos-web/src/pages/Inventory/InventoryListPage.tsx) para usar el mismo procesador con soporte HEIC y auto-compresión.
5. **Suite de Pruebas**:
   - Vitest: 27/27 pasadas (100%).
   - Build Vite: Exitoso con `heic2any` dividido en chunk independiente.

## Resumen de Cambios Previos en PR
>>>>>>> version-final-de-PR
1. **Punto de Venta (PDV)**:
   - Botones rápidos renombrados a `Pieza +` y `Caja +`.
   - Filtro de búsqueda rápida integrado arriba de `📦 Catálogo rápido`.
   - Desglose detallado de piezas, cajas y m² en el carrito (`23 Pzas (2 Cjas + 3 Pzas) · $290.00 · 10.01 m²`).
   - Cobertura corregida para multiplicar la cantidad por la cobertura por pieza (`coveragePerUnitSqM`).
   - Incorporación de **Calculadora de m² de Lambrín** en la tarjeta de cobro (`pos-checkout`) con cálculo automático de piezas/cajas y adición en 1 clic.
   - Modal rápido para alta de clientes directamente desde caja por cajeros.
   - Incrementadas imágenes de tarjetas de 62px a 92px.
2. **Catálogo de Productos y Costo Neto**:
   - Inclusión del campo `Costo Neto / Inicial ($ MXN)` en el modal de alta/edición de productos.
   - Persistencia de `CostoUnitario` que alimenta Costo Neto (COGS) y Ganancia en los Movimientos de Inventario.
   - Encabezados bilingües en la tabla del catálogo (`Precio Menudeo / 零售价` y `Precio Mayoreo / 批发价`).
3. **Control de Clientes y Límites Diarios**:
   - Permiso `clientes:limite_diario` y campo `LimiteCajasDiarias` en entidad `Cliente`.
   - Validación autoritativa en `SaleApplicationService` que impide rebasar el límite diario de cajas.
   - Modal de Historial de Compras de Cliente para consulta de cajeros y administradores.
4. **Movimientos de Inventario, Turnos de Caja y Reportería Bilingüe**:
   - Folio limpio `Venta #X` en la columna Motivo de Movimientos de Inventario y folios secuenciales `CAJA-YYYYMMDD-1` en la tarjeta de caja (`cash-card`), turnos de caja, listado de historial y bitácora de auditoría.
   - Cancelación de Ventas restringida a Administradores (`ventas:cancelar`), con reintegración automática de existencias a `Stocks` y sincronización en tiempo real con el esperado del Corte de Caja.
   - Impuesto ajustado a `$0.00` para ventas no facturadas.
   - Exportaciones PDF y Excel con 100% de encabezados bilingües en Español y Chino Simplificado, con registro singleton de fuentes CJK para exportaciones de alto rendimiento en milisegundos.

## Resumen Ejecutivo de la Iteración

Se ha configurado la base de datos limpia de SQL Server con autenticación de usuario (`wpcadminaam`), se montó el esquema autoritativo de 26 tablas e insertaron las semillas de inicio. Adicionalmente, el Backend API y el Frontend SPA se compilaron en modo producción y se publicaron en los directorios de IIS (`C:\inetpub\wwwroot\pos-api` y `C:\inetpub\wwwroot\pos-web`).

### Configuración de Base de Datos SQL Server (SSMS 20):
- **Server name**: `.` (o `localhost`)
- **Authentication**: `SQL Server Authentication`
- **Login**: `wpcadminaam`
- **Password**: `Aaron2804#`
- **Database**: `PosLambrinDb` (26 tablas creadas)

### Estado del Despliegue IIS:
- **Frontend SPA (Punto de Venta)**: `http://localhost` (`C:\inetpub\wwwroot\pos-web`)
- **Backend API (.NET 9)**: `http://localhost:5000` (`C:\inetpub\wwwroot\pos-api`)

## Estado de la Suite de Pruebas
- Frontend Vitest: **24/24** pasadas (100%).
- Backend xUnit: **68/68** pasadas (100%).
- Pruebas Totales: **92/92** pasadas al 100%.

## Servidores de Desarrollo Activos
- Backend API (.NET 9): `http://localhost:5000`
- Frontend Web SPA (React + Vite): `http://localhost:5173`
