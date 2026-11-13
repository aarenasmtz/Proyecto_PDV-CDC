# CURRENT STATE — Estado Real del Sistema WPC Bajío

## 🟢 ESTADO ACTUAL (13 de Noviembre, 2026 - Despliegue Exitoso a Producción: Backend en VPS y Frontend en Cloudflare Workers)

- **Despliegue a Producción (VPS + Cloudflare Edge CDN) 100% OPERATIVO**:
  - **Backend API (.NET 9 en VPS Ubuntu 193.46.198.88)**:
    - Compilación Release publicada a `/var/www/pos-api/`.
    - Servicio `pos-api.service` reiniciado y operando activamente.
    - Migración de base de datos EF Core `20261108052821_AddFacturacionModuleCompleto` aplicada automáticamente con éxito en `PosLambrinDb` del VPS.
    - Migración automática de 99 imágenes Base64 a WebP completada en `/var/wpcbajio/data/products/`.
    - API pública verificada respondiendo `200 OK` en `https://api.wpcbajio.com/api/v1/health` bajo proxy Cloudflare.
    - Configuración de FiscalAPI mantenida en ambiente QA (`https://test.fiscalapi.com`).
  - **Frontend SPA (Cloudflare Workers / CDN Edge Global)**:
    - Compilado con Vite en 11.60s (`dist/assets/index-m54uzgNJ.js`, `index-URv5IIDf.css`).
    - Desplegado exitosamente con Wrangler a Cloudflare Workers Assets.
    - **URL Pública Oficial**: [https://pos-wpcbajio.aaronarenasmartinez.workers.dev](https://pos-wpcbajio.aaronarenasmartinez.workers.dev) (Verificada `HTTP 200 OK`).
    - Incluye: Módulo de Facturación CFDI 4.0, Módulo de Notas de Venta comerciales, optimización de velocidad de carga y rediseño del carrito.
- **Pull Request Oficial**:
  - [#9 feat: Facturación Electrónica CFDI 4.0 con FiscalAPI, Notas de Venta y Optimización de Rendimiento en PDV](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9).
- **Servicios Locales de Desarrollo**:
  - Backend API: `http://localhost:5000` (Activo en daemon)
  - Frontend SPA: `http://localhost:5173` (Activo en daemon)
  - Base de Datos: `PosLambrinDb` (Local sincronizado)

## 🟢 ESTADO PREVIO (13 de Noviembre, 2026 - Pull Request #9 Creado y Servicios Activos)

- **Creación y Publicación del Pull Request Oficial**:
  - **Pull Request**: [#9 feat: Facturación Electrónica CFDI 4.0 con FiscalAPI, Notas de Venta y Optimización de Rendimiento en PDV](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9)
  - **Rama Origen**: `feature/facturacion-cfdi-y-notas-de-venta`
  - **Rama Destino**: `main`
  - **Autor del Commit y PR**: `Aaron Arenas Martinez` (`44418304+UTLCDA@users.noreply.github.com`)
  - **Commit**: `b5e1bad` (83 archivos modificados/creados, 18,611 inserciones).
  - **Credenciales FiscalAPI**: Configuradas en ambiente QA conforme a instrucción (`https://test.fiscalapi.com`). Queda pendiente actualizar las claves a PR de producción al realizar el despliegue en el VPS.
- **Servicios Locales Activos**:
  - Backend API: `http://localhost:5000` (Activo en daemon)
  - Frontend SPA: `http://localhost:5173` (Activo en daemon)
  - Base de Datos: `PosLambrinDb` (Local sincronizado)

## 🟢 ESTADO PREVIO (13 de Noviembre, 2026 - Levantamiento de Servicios y Auditoría Exhaustiva de Carga en los 17 Módulos del Sistema)

- **Levantamiento y Verificación de Servicios Activos**:
  - **Backend API (`Pos.Api`)**: Activo y escuchando en `http://localhost:5000` (Proceso .NET 9).
  - **Frontend SPA (`pos-web`)**: Activo y escuchando en `http://localhost:5173` (Vite 6 / React 18 / Node.js).
  - **Base de Datos Local (`PosLambrinDb`)**: Activa en `MSSQLSERVER` local.
- **Auditoría Integral de Rendimiento y Carga de Datos en 42 Peticiones y 17 Módulos**:
  - Se desarrolló y ejecutó una suite de validación automatizada sobre el proxy de Vite (`http://127.0.0.1:5173/api/v1/...`) con autenticación como Administrador, simulando exactamente las consultas que realizan los componentes React al montarse:
    - **Total pruebas**: 42 | **Aprobadas**: 42 (100%) | **Fallidas**: 0.
    - **Latencias de respuesta**: Entre **6 ms y 153 ms** en todos los módulos (óptimo).
- **Diagnóstico Preciso de los Módulos que Parecen "No Cargar Información"**:
  1. **Módulos con Filtro de Fecha Inicial en "Hoy" (`today()` / 13 de Noviembre)**:
     - **Historial de Ventas (`SalesHistoryPage.tsx`)**: Inicia con `startDate: today()`, `endDate: today()`. Como las 19 ventas de prueba fueron generadas hasta el 09 de noviembre de 2026, al abrir el módulo muestra 0 registros ("No se encontraron ventas registradas"). Al limpiar las fechas o ampliarlas, aparecen de inmediato las **19 ventas** en **18 ms**.
     - **Movimientos de Inventario (`InventoryMovementsPage.tsx`)**: Inicia con `startDate: today()`, `endDate: today()`. Al no haber movimientos hoy, muestra 0 registros. Al limpiar el filtro de fecha, aparecen instantáneamente los **56 movimientos** en **13 ms**.
     - **Reportes y Métricas (`ReportsDashboardPage.tsx`)**: Inicializa con fecha del día en curso. Muestra $0.00 y 0 productos vendidos hoy porque hoy no ha habido transacciones. Al cambiar el rango de fechas al periodo de ventas (ej. del 01/11/2026 al 10/11/2026), se calculan y visualizan todos los indicadores, desgloses y gráficas.
  2. **Módulos con Tablas Vacías en Base de Datos Local (`Count: 0`)**:
     - **Recibos de Compra (`PurchaseReceiptsPage.tsx`)**: La tabla `PurchaseReceipts` tiene **0 registros** en la BD local. Por ello la tabla muestra legítimamente "No se encontraron recibos de compra registrados / Sin registros" (confirmado con la captura del usuario).
     - **Cotizaciones (`QuoteListPage.tsx`)**: La tabla `Quotes` tiene **0 cotizaciones** en la BD local.
     - **Devoluciones (`CommercialOpsPage.tsx?mode=returns`)**: La tabla `ReturnHeaders` tiene **0 devoluciones** registradas.
  3. **Módulo con Filtro de Categoría por Defecto**:
     - **Catálogo de Productos (`PaginaCatalogoProductos.tsx`)**: Inicia con `categoriaFiltro: DEFAULT_CATEGORY_ID` (*Lambrin Interior*), cargando 29 productos en **11 ms**. Al seleccionar "Todas las categorías", muestra los **111 productos** del catálogo completo.
  4. **Módulos con Carga Inmediata y Completa de Datos**:
     - **🛒 Punto de Venta (POS)**: Carga 29 productos de la categoría activa, 7 clientes, 13 categorías, resumen del día y estado de turno en ~30 ms.
     - **📋 Notas de Venta**: Muestra las 19 ventas en 12 ms (con filtros limpios y estado ALL).
     - **📦 Pedidos Web**: Muestra los 5 pedidos de Stripe en 12 ms.
     - **💵 Turnos de Caja**: Muestra el turno actual abierto, 1 corte histórico y 21 movimientos de caja en ~30 ms.
     - **🧾 Facturación CFDI 4.0**: Muestra las 2 facturas timbradas en 50 ms, configuración fiscal en 11 ms y bitácora en 11 ms.
     - **👥 Directorio de Clientes**: Carga los 7 clientes registrados en 7 ms.
     - **📁 Categorías**: Carga las 13 categorías en 18 ms.
     - **🏭 Inventario / Existencias**: Carga el stock de los 111 productos en 61 ms.
     - **🛡️ Gestión de Usuarios**: Carga usuarios, roles y 35 permisos en 10 ms.
     - **💰 Crédito y Cobranza**: Carga 4 ventas pendientes de pago, 20 transacciones de pago, 1 abono y 2 plantillas de contrato.
     - **🔍 Bitácora de Auditoría**: Carga 33 eventos de hoy en 20 ms (y 6,939 totales).
- **Pruebas y Verificación Automatizada**:
  - Backend: 122/122 pruebas unitarias e integración aprobadas al 100% (`dotnet test src/backend/Pos.sln`).
  - Frontend: 53/53 pruebas unitarias de Vitest aprobadas (`npm run test`).
  - Servidores activos: Backend API (`http://localhost:5000`) y Frontend SPA (`http://localhost:5173`).
- **Estado Git**: Working tree local sin commits ni pushes conforme a directiva del proyecto.

## 🟢 ESTADO PREVIO (09 de Noviembre, 2026 - Corrección de Retardo en Ticket de Venta, Desmontaje de Pantalla y Timeout de Conexión en Base de Datos)

- **Corrección de Flujo Post-Venta y Rendimiento en Base de Datos (`PaginaPuntoVenta.tsx`, `apiClient.ts`, `appsettings.json`, `appsettings.Development.json`)**:
  - **Problema Reportado**: Tras crear una venta, tardaba mucho tiempo en mostrar el ticket, la pantalla se quedaba cargando para volver a mostrar los productos y aparecía el error `"Tiempo de espera agotado al conectar con el servidor (15s). Verifica la conexión."`.
  - **Diagnóstico y Causas Raíz Identificadas**:
    1. **Bloqueo del Ticket por Desmontaje del DOM (`PaginaPuntoVenta.tsx`)**:
       - Al completar la venta, se ejecutaba `setReceipt(sale)` e inmediatamente `await loadData()`.
       - `loadData()` llamaba de inmediato a `setLoading(true)`.
       - Al activarse `loading: true`, la condición temprana `if (loading) return <div className="pos-loading">...</div>` desmontaba todo el árbol de componentes del POS, **destruyendo el modal del ticket (`SaleReceiptModal`)** del DOM antes de que pudiera renderizarse. El cajero veía una pantalla en blanco con "Cargando..." en vez de su ticket.
    2. **Sobrecarga de 5 Consultas Concurrentes Innecesarias tras una Venta**:
       - `loadData()` disparaba concurrentemente: `getProducts` (500 ítems), `getCustomers` (500 ítems), `getCategories` (500 ítems), `getSalesSummary` y `getCurrentShift`.
       - Ni los clientes, ni las categorías, ni el estado del turno cambian al registrar una venta; únicamente cambian las existencias de productos y el acumulado de ventas del día.
    3. **Agotamiento de Conexiones en SQL Server y Timeout Agresivo del Cliente**:
       - SQL Server local opera bajo protocolo Shared Memory con TCP deshabilitado. Al recibir 5 peticiones simultáneas justo después de la transacción de venta, el pool de conexiones de EF Core alcanzaba el tiempo límite en la fase post-login (`[Post-Login] complete=14016`).
       - En `apiClient.ts`, `setTimeout(() => controller.abort(), 15000)` abortaba prematuramente a los 15s antes de que la transacción terminara de responder, mostrando el banner de error al usuario.
  - **Soluciones Implementadas**:
    1. **Apertura Inmediata del Ticket sin Bloqueo de UI (`PaginaPuntoVenta.tsx`)**:
       - `setReceipt(sale)` abre el ticket de inmediato (<1 segundo) para que el cajero imprima o entregue el comprobante sin esperas.
       - Implementado `refreshStockAndSummary()` que corre en segundo plano de forma silenciosa (sin activar `setLoading(true)`, sin desmontar la interfaz y sin retrasar el ticket).
       - Se redujo el refresco post-venta de 5 peticiones pesadas a solo 2 ligeras (`getProducts` de la categoría actual para existencias + `getSalesSummary` para totales del día), completando en **~93 ms**.
    2. **Optimización del Pool de Conexiones de SQL Server (`appsettings.json`, `appsettings.Development.json`)**:
       - Cadena de conexión actualizada con `Connect Timeout=30;Max Pool Size=200;MultipleActiveResultSets=True;`, permitiendo que múltiples consultas asíncronas reutilicen conexiones activas sin agotar el pool ni bloquearse en post-login.
    3. **Alineación de Timeout en `apiClient.ts`**:
       - Aumentado el límite de cancelación de `15000` ms (15s) a `30000` ms (30s), alineado con el estándar de APIs transaccionales y evitando abortos en falso del navegador.
  - **Pruebas y Verificación**:
    - Prueba de venta en vivo: Creación de venta completada en **938 ms** (Folio generado `#27`), refresco de existencias en segundo plano en **93 ms**, visualización instantánea del ticket.
    - Backend: 122/122 pruebas unitarias e integración aprobadas al 100% (`dotnet test src/backend/Pos.sln`).
    - Frontend: 53/53 pruebas unitarias de Vitest aprobadas (`npm run test`).
    - Build de producción Vite completado limpiamente en 15.46s con 0 advertencias y 0 errores.
    - Servidores activos: Backend API (`http://localhost:5000`) y Frontend SPA (`http://localhost:5173`).
  - **Estado Git**: Working tree local sin commits ni pushes conforme a directiva del proyecto.

## 🟢 ESTADO PREVIO (09 de Noviembre, 2026 - Rediseño del Control de Cantidad y Carrito en Punto de Venta)

- **Rediseño Visual y Ergonómico del Stepper (+ / −) en Carrito del PDV (`PaginaPuntoVenta.css`, `PaginaPuntoVenta.tsx`)**:
  - **Problema Reportado**: En el panel lateral del carrito de ventas, el contenedor donde se modifican las piezas (`+` y `−`) "se perdía" visualmente debido a bordes débiles, falta de contraste, líneas rotas en el campo de texto y botones poco diferenciados.
  - **Causa Raíz Identificada**:
    - Conflicto por reglas duplicadas en `PaginaPuntoVenta.css`: existía una definición antigua en cuadrícula (`display: grid`) en la línea 944 que sobrescribía parcialmente los estilos de la línea 497.
    - El `input` tenía `border-left: 0; border-right: 0;`, pero el contenedor conservaba un `gap`, lo que dejaba el número flotando con líneas superiores/inferiores desconectadas de los botones.
    - Colores de borde y fondo muy claros (`--border-input` / `#E6D8C5`) sobre fondo blanco, provocando la pérdida de contraste ("se pierde").
  - **Mejoras Implementadas**:
    1. **Eliminación de Reglas CSS Duplicadas**: Retiradas por completo las reglas redundantes en `PaginaPuntoVenta.css`.
    2. **Control de Cantidad Unificado en Cápsula Segmentada**:
       - Contenedor con borde definido (`1.5px solid #CFB9A5`), esquinas redondeadas (`7px`), sombra sutil (`box-shadow: 0 1px 2px rgba(58, 35, 18, 0.08)`) y altura ergonómica de `30px`.
       - Efecto focus-within que resalta el borde en color terracota corporativo (`#9C4D22`) con anillo suave al editar.
    3. **Botones `−` y `+` Táctiles y de Alto Contraste**:
       - Botones con superficie de fondo cálida (`#FAF5ED`), tipografía bold (`1.15rem`, `font-weight: 800`), separadores verticales nítidos (`1px solid #E4D5C5`).
       - Microinteracción hover de alto contraste: fondo terracota (`#9C4D22`) con icono blanco puro (`#FFFFFF`).
    4. **Input Numérico Central Integrado**:
       - Tipografía en marrón oscuro de alto contraste (`#3A2312`, `font-weight: 750`, `0.9rem`), centrado perfecto sin bordes flotantes rotos, y supresión de selectores/flechas nativas del navegador (spin-buttons).
    5. **Botón Quitar (`×`) Refinado**:
       - Convertido en botón circular suave con transición a rojo (`#FEE2E2` / `#DC2626`) en hover.
    6. **Jerarquía Visual de Totales por Partida**:
       - Subtotal en negrita prominente (`#3A2312`, `0.92rem`, `font-weight: 800`) y desglose de piezas/m² en color secundario balanceado (`#7A6657`).
  - **Pruebas y Verificación**:
    - Frontend: `53/53` pruebas unitarias aprobadas. Build de producción Vite completado limpiamente con 0 advertencias y 0 errores.
    - Servidores activos: Backend API (`http://localhost:5000`) y Frontend SPA (`http://localhost:5173`).
  - **Estado Git**: Working tree local sin commits ni pushes conforme a directiva del proyecto.

## 🟢 ESTADO PREVIO (09 de Noviembre, 2026 - Optimización de Rendimiento en Carga de Productos del PDV: de 4.56s a 26ms)

- **Optimización Integral de Carga de Productos en el Punto de Venta (`PaginaPuntoVenta.tsx`, `CatalogApplicationService.cs`, `DbInitializer.cs`, `vite.config.ts`)**:
  - **Problema Reportado**: La vista principal del PDV tardaba más de 4.5 segundos en cargar los productos (`http://localhost:5173/api/v1/products?categoryId=7938934b-d6cb-44fd-98de-b0645c66017d&page=1&pageNumber=1&pageSize=500`). En el desglose de Network de Chrome DevTools se observaba:
    - `Stalled`: 2.27 s
    - `Waiting for server response (TTFB)`: 2.27 s
    - Duración total: 4.56 s
  - **Diagnóstico y Causas Raíz Identificadas**:
    1. **Imágenes Base64 Gigantescas en Base de Datos (LOBs en SQL Server)**:
       - 99 de 112 productos tenían imágenes completas codificadas en cadenas Base64 (`data:image/jpeg;base64,...`) de entre 500 KB y 600 KB cada una almacenadas en la columna `ImagenUrl` de la tabla `Products`.
       - Esto sumaba **31.2 MB** de datos binarios en la tabla y **8.28 MB** de payload JSON para los 29 productos de la categoría por defecto del PDV.
       - La transferencia masiva de megabytes colapsaba el pipeline de serialización de EF Core y el proxy HTTP de Vite en Node.js.
    2. **Consulta en Dos Fases con Extracción de IDs en Memoria y `OPENJSON`**:
       - `CatalogApplicationService.GetProductsAsync` primero extraía los IDs a una lista C# y luego ejecutaba `_dbContext.Products.Where(p => pagedProductIds.Contains(p.Id)).Include(p => p.Categoria)` seguido de subconsultas separadas con `OPENJSON` para calcular el stock.
       - En backend puro (`http://localhost:5000`), esta operación tomaba **2,924 ms**.
    3. **Bloqueo de Socket IPv6 en Windows por `host: '0.0.0.0'` en Vite**:
       - `vite.config.ts` tenía configurado `server.host: '0.0.0.0'`, lo que forzaba a Node.js a escuchar exclusivamente en IPv4 (`0.0.0.0:5173`) sin abrir el socket IPv6 `[::]:5173`.
       - En Windows, al navegar o consultar `http://localhost:5173`, el resolver de DNS intenta primero conectarse a `::1:5173`. Al no haber nada escuchando en IPv6, el socket TCP se quedaba esperando el timeout de retransmisión SYN de Windows (~2,270 ms) antes de caer a IPv4 `127.0.0.1`. Esto explicaba con exactitud los **2.27 s de Stalled**.
       - Además, el proxy de Vite apuntaba a `target: 'http://localhost:5000'`, sufriendo idéntico retardo de 2 segundos en el primer reenvío interno a Kestrel.
  - **Soluciones Implementadas**:
    1. **Migración Automática de Imágenes Base64 a WebP en Disco (`DbInitializer.cs`)**:
       - Implementado `MigrateBase64ImagesToDiskAsync` en el arranque de la API: detecta automáticamente cualquier producto con `ImagenUrl` que empiece con `data:image%`.
       - Decodifica la imagen y genera mediante `IProductImageStorageService` variantes optimizadas en formato WebP (thumbnail 128x128, POS 256x256, preview 512x512) guardadas en `C:\WPCBajioData\Products\{productId}\`.
       - Actualiza la base de datos reemplazando la cadena Base64 por la URL relativa liviana `/products/{productId}/pos.webp?v=...`.
       - Los 99 productos con Base64 fueron convertidos con éxito a WebP en disco. El tamaño de la respuesta JSON pasó de **8.5 MB** a solo **~15 KB** (reducción de 560x).
    2. **Proyección Directa a DTO en una Sola Consulta SQL (`CatalogApplicationService.cs`)**:
       - Refactorizado `GetProductsAsync` para utilizar proyección directa con `.Select(...)`: recupera las columnas escalares necesarias, el nombre de categoría vía navegación `p.Categoria.Nombre` y el cálculo de stock disponible en una única consulta SQL optimizada con `TOP 500`.
       - El tiempo de respuesta directo del backend (.NET 9) bajó de **2,924 ms** a **35 ms** (~83x de mejora).
    3. **Eliminación del Retardo IPv6 y Optimización del Proxy (`vite.config.ts`)**:
       - Cambiado `server.host` a `'::'` (dual-stack IPv6 e IPv4), habilitando escucha simultánea en `[::]:5173` y `0.0.0.0:5173` sin perder el acceso por red local (`192.168.100.2`).
       - Actualizados los targets del proxy (`/api`, `/products`, `/categories`) de `http://localhost:5000` a `http://127.0.0.1:5000` para comunicación directa IPv4 sin esperas de resolución DNS.
       - Esto eliminó por completo los 2.27 segundos de espera de socket.
  - **Métricas Finales Verificadas**:
    - **Antes**: 4,560 ms (2.27s Stalled + 2.27s TTFB, payload 8.5 MB).
    - **Después**: **26 ms** (0s Stalled + 26ms TTFB, payload 15 KB).
    - **Aceleración total**: **~175x más rápido**, carga prácticamente instantánea en el Punto de Venta.
  - **Pruebas y Verificación**:
    - Backend: 122/122 pruebas unitarias e integración aprobadas al 100% (`dotnet test src/backend/Pos.sln`).
    - Frontend: 53/53 pruebas unitarias de Vitest aprobadas (`npm run test`).
    - Build de producción: `npm run build` completado en 13.64s con 0 errores TypeScript.
    - Servidores activos: Backend API en `http://localhost:5000` y Frontend SPA en `http://localhost:5173`.
  - **Estado Git**:
    - Working tree local sin commits ni empujes remotos conforme a las directivas del proyecto.

## 🟢 ESTADO PREVIO (09 de Noviembre, 2026 - Módulo de Facturación CFDI 4.0: Corrección de Carga Lenta, Doble Fetch y Parpadeo de "Cargando datos...")

- **Optimización y Estabilización del Módulo de Facturación (`FacturacionPage.tsx`, `FacturacionBitacoraTab.tsx`)**:
  - **Problema Reportado**: Al abrir el módulo de facturación, se quedaba la leyenda "Cargando datos..." y las facturas aparecían y desaparecían repetidamente (parpadeo visual).
  - **Diagnóstico y Causas Raíz**:
    1. **Doble Fetch en Cascada por Dependencia de `config`**: En `loadInvoices`, se llamaba concurrentemente a `obtenerFacturas` y `obtenerConfiguracion()`. Al resolver la configuración, se ejecutaba `setConfig(configData)`. Al estar `config` en el arreglo de dependencias de `useCallback`, la función `loadInvoices` se recreaba inmediatamente, disparando un segundo `useEffect` consecutivo con `setLoading(true)`, lo que provocaba que la tabla se montara, se destruyera y se volviera a montar en fracciones de segundo.
    2. **Destrucción Completa del DOM de la Tabla en `loading: true`**: La condición `{loading ? <p>Cargando datos...</p> : <table>...}` reemplazaba todo el bloque de la tabla y paginación por un elemento `<p>`, causando saltos bruscos de maquetación (Layout Shift) en cada actualización.
    3. **Desincronización de Fechas por Defecto**: Los campos de entrada `startDate` y `endDate` se inicializaban con `today` (`09/11/2026`), mientras que `appliedFilters` tenía cadenas vacías `''`. Cualquier interacción o búsqueda aplicaba la fecha de hoy, ocultando todas las facturas emitidas en fechas previas (como las del `08/11/2026`), dando la impresión de que las facturas desaparecían.
  - **Soluciones Implementadas**:
    1. **Desacoplamiento de Carga de Configuración**: La configuración fiscal del emisor ahora se consulta una sola vez al montar el componente en su propio `useEffect` dedicado, eliminando por completo `config` de las dependencias de `loadInvoices`.
    2. **Estructura de Tabla Permanentemente Montada**: El elemento `<table>` y el componente `<TablePagination>` se mantienen fijos y montados en el DOM en todo momento. La animación de carga `⏳ Cargando facturas emitidas...` y el estado vacío se renderizan limpiamente como filas `<tr><td colSpan={9}>` dentro de `<tbody>`, eliminando cualquier parpadeo o colapso visual.
    3. **Sincronización de Filtros de Fecha**: Inicializados `startDate` y `endDate` como cadenas vacías `''` de forma consistente, permitiendo visualizar todas las facturas emitidas sin cortes arbitrarios por día, salvo que el usuario seleccione explícitamente un rango.
    4. **Extensión a la Bitácora Técnica (`FacturacionBitacoraTab.tsx`)**: Se aplicó la misma arquitectura estable (tabla fija en DOM con spinner en `<tbody>` y fechas limpias) en la pestaña de bitácora técnica de FiscalAPI.
  - **Pruebas y Verificación**:
    - Backend: 122/122 pruebas unitarias e integración aprobadas al 100%. API respondiendo en ~22ms.
    - Frontend: 53/53 pruebas unitarias de Vitest aprobadas. Build de producción Vite completado limpiamente en 18.49s con 0 errores TypeScript.
    - Servidores activos: Backend API en `http://localhost:5000` y Frontend SPA en `http://localhost:5173`.
  - **Estado Git**:
    - Working tree local sin commits ni empujes remotos conforme a las directivas del proyecto.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo Notas de Venta: Reestructuración de Filtro de Estados de Venta y Columna de Estado con Badges)

- **Reestructuración del Filtro de Estados de Venta (`NotasVentaPage.tsx`, `NotaVentaApplicationService.cs`, `tiposNotasVenta.ts`)**:
  - **Problema Reportado**: El selector `<select className="notas-filter-select">` contenía opciones en inglés desalineadas con la base de datos (`Completed`, `DepositPaid`, `PendingPayment`, `Cancelled`), por lo que al seleccionar "Completada" no devolvía resultados debido a que en SQL Server el campo `Sales.Estado` almacena valores en español (`Completada`, `Cancelada`, etc.).
  - **Ajustes y Mejoras Implementadas**:
    1. **Opciones del Selector Reestructuradas con Estados Reales del Dominio**:
       - `ALL`: Todos los estados / 所有状态
       - `Completada`: ✔ Completada / ✔ 已完成
       - `ApartadoPagado`: 📦 Con Anticipo / 📦 定金已付
       - `PendientePago`: ⏳ Pendiente de Pago / ⏳ 待付款
       - `Cancelada`: ✕ Cancelada / ✕ 已取消
       - `DevolucionParcial`: ↩ Devolución Parcial / ↩ 部分退货
       - `Devuelta`: ↩ Devuelta / ↩ 全部退货
    2. **Mapeo Bidireccional Robusto en Backend (`NotaVentaApplicationService.cs`)**:
       - Soporta de forma transparente tanto los valores reales en español almacenados en base de datos como sus alias o constantes del dominio (`SaleStatuses.Completed`, `SaleStatuses.Cancelled`, `SaleStatuses.DepositPaid`, etc.).
       - Probado con la base de datos real: 15 ventas totales -> Filtro `Completada` retorna 9 registros; Filtro `Cancelada` retorna 6 registros.
    3. **Nueva Columna "Estado" en la Tabla de Notas de Venta**:
       - Agregada la columna `Estado` / `状态` en el encabezado `<thead>` y celdas `<td>` con badges estilizados según el estado:
         - `✔ Completada` (Verde: `.badge-completada`)
         - `✕ Cancelada` (Rojo: `.badge-cancelada`)
         - `📦 Con Anticipo` (Azul: `.badge-apartado`)
         - `⏳ Pendiente Pago` (Ámbar: `.badge-pendiente`)
         - `↩ Dev. Parcial` / `↩ Devuelta` (Púrpura: `.badge-devolucion`)
    4. **Corrección de Mapeo de Paginación (`totalItems`)**:
       - En `tiposNotasVenta.ts` y `NotasVentaPage.tsx`, se mapea `res.totalItems ?? res.totalCount ?? 0` para asegurar que el contador de registros y cálculo de páginas refleje con exactitud la respuesta del backend.
  - **Pruebas y Verificación**:
    - Backend: `dotnet build src/backend/Pos.sln` exitoso con 0 errores. 122/122 pruebas unitarias e integración aprobadas.
    - Frontend: `npm run build` en `pos-web` exitoso en 18.54s con 0 errores. 53/53 pruebas unitarias de Vitest aprobadas.
    - Servidores activos: Backend API en `http://localhost:5000` y Frontend SPA en `http://localhost:5173`.
  - **Estado Git**:
    - Working tree local sin commits ni empujes remotos conforme a las directivas del proyecto.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo Notas de Venta: Folio Consecutivo Identity 1-1, Esquinas Curvas, Datos Reales E-commerce y Depuración Visual)

- **Ajustes y Refinamiento del Módulo de Notas de Venta (`NotasVentaPage.tsx`, `NotaVentaPdfService.cs`, `NotaVentaApplicationService.cs`)**:
  - **Solicitudes de Usuario Implementadas al 100%**:
    1. **Folio Consecutivo Identity 1-1**:
       - Sustituido el identificador GUID interno largo (ej. `VENTA-20261108-656C4F9108ED4`) por el `IdVenta` numérico secuencial (ej. `#25`).
       - Visible tanto en la tabla frontend (`#25`), búsqueda, como en el recuadro destacado superior derecho del documento PDF (`Folio: 25`). El GUID se conserva internamente en la base de datos para integridad referencial sin ser expuesto al usuario.
    2. **Eliminación del Flujo y Estado de Entrega**:
       - La entrega física es un proceso operativo posterior independiente; se retiró la columna "Estado Entrega", el botón de acción "🚚 Entrega", el diálogo modal de confirmación y el filtro de entrega del frontend.
       - Acciones limpias y enfocadas exclusivamente en: `👁️ Vista previa`, `📥 PDF` e `🖨️ Imprimir`.
       - Métricas rápidas consolidadas en 3 tarjetas: Total Notas de Venta, Notas en Página y Total Comercial ($).
    3. **Nombre del Cliente Limpio (Sin duplicación de Razón Social)**:
       - La columna y bloque de cliente muestran exclusivamente el nombre del titular (`${Cliente.Nombre} ${Cliente.Apellido}`, ej. `Aaron Arenas Martinez` o `Público General`) sin duplicar entre paréntesis la razón social ni concatenar textos redundantes.
    4. **Bordes Curvos Redondeados en PDF (QuestPDF con Dynamic SVG Vectorial)**:
       - Implementado helper `RoundedCard` con `layers.Layer().Svg(size => ...)` dinámico que calcula milimétricamente el espacio natural con `rx="6"` y `ry="6"`, bordes suaves y fondos correspondientes (`#FAF8F5`, `#FEF7EB`).
       - Aplicado a todos los recuadros del documento: Recuadro NOTA DE VENTA, Tarjeta DATOS DEL CLIENTE, Recuadro OBSERVACIONES y Recuadro TOTALES.
    5. **Datos Comerciales Reales de WPC Bajío (Alineados al E-commerce)**:
       - Dirección: `Blvd. Adolfo López Mateos 2826, Col. El Rosario`
       - Ciudad / C.P.: `León de los Aldama, Guanajuato, C.P. 37125`
       - Tel. / WhatsApp: `477 807 2768`
       - Correo y Sitio: `contacto@wpcbajio.com` | `www.wpcbajio.com`
    6. **Depuración del Layout del PDF**:
       - Eliminado completamente el recuadro `ENTREGAR A / DESTINO`.
       - Eliminado completamente el recuadro/sello `ESTADO: ENTREGADO` / `PENDIENTE DE ENTREGA`.
       - Eliminada la etiqueta `Saldo: $0.00 (Liquidado)` (únicamente se muestra saldo si existe monto pendiente > 0).
       - El recuadro `DATOS DEL CLIENTE` ahora abarca el ancho completo con distribución balanceada en 2 columnas internas (Nombre, RFC y Teléfono a la izquierda; Email, Dirección y Ciudad/CP a la derecha).
  - **Pruebas y Verificación**:
    - Backend: `dotnet build src/backend/Pos.sln` exitoso con 0 errores. 122/122 pruebas unitarias e integración aprobadas (`Pos.Domain.Tests`: 19, `Pos.Application.Tests`: 82, `Pos.Api.IntegrationTests`: 21).
    - Frontend: `npm run build` en `pos-web` exitoso en 13.56s con 0 errores TypeScript. 53/53 pruebas unitarias de Vitest aprobadas.
    - Generación visual verificada mediante renderizado a imagen (`nota_venta_25_preview.png`) confirmando bordes curvos, folio `#25`, datos reales y eliminación de recuadros redundantes.
    - Servidores activos: Backend API en `http://localhost:5000` y Frontend SPA en `http://localhost:5173`.
  - **Estado Git**:
    - Working tree local sin commits ni empujes remotos.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo Standalone: Notas de Venta Comerciales — WPC Bajío)

- **Módulo Standalone de Notas de Venta Comerciales (`NotasVentaPage.tsx`, `NotaVentaPdfService.cs`, `NotasVentaController.cs`)**:
  - **Objetivo**: Proporcionar generación, consulta, reimpresión y descarga de Notas de Venta en formato PDF tamaño carta profesional, con identidad corporativa de WPC Bajío, completamente desacoplado de FiscalAPI y sin efectos fiscales ni consumo de timbres.
  - **Backend (.NET 9 ASP.NET Core & QuestPDF 2024.12)**:
    1. **Servicio PDF (`NotaVentaPdfService.cs`)**:
       - Diseñado en tamaño Carta (216 × 279 mm), vertical, márgenes para impresión doméstica y paleta de colores corporativa oficial (`#9C4D22`, `#833E1A`, `#6F3215`, `#FAF8F5`, `#FEF7EB`, `#F3E9DA`, `#3A2312`, `#7A6657`).
       - Encabezado con logotipo oficial WPC Bajío (`logo_wpc_bajio.jpeg`), datos de contacto de la empresa (León, Gto.), recuadro destacado de "NOTA DE VENTA / SIN EFECTOS FISCALES" con folio histórico y fecha/hora.
       - Sección de 2 columnas: "DATOS DEL CLIENTE" y "ENTREGAR A / DESTINO" (modalidad de entrega, dirección y contacto).
       - Tabla dinámica con desglose de productos (descripción comercial, SKU, cantidad, unidad, cálculo automático de piezas totales para cajas de lambrín, precio unitario histórico, descuentos e importe).
       - Sección inferior con observaciones, sello oficial de entrega (`✔ ESTADO: ENTREGADO` en verde o `⏳ ESTADO: PENDIENTE DE ENTREGA` en ámbar), totales históricos (subtotal, descuento, IVA únicamente si está registrado en la venta, total y estado de pago/saldo).
       - Leyenda legal obligatoria: *"Documento comercial sin efectos fiscales. No sustituye un comprobante fiscal digital por Internet (CFDI)."*
       - Pie de página con numeración dinámica "Página X de Y".
    2. **Capa de Aplicación y Controlador REST (`NotasVentaController.cs` & `NotaVentaApplicationService.cs`)**:
       - `GET /api/v1/notas-venta/ventas`: Consulta paginada (25 ítems) con filtros por folio, cliente, RFC, rango de fechas, estado de venta y estado de entrega.
       - `GET /api/v1/notas-venta/ventas/{ventaId}`: Obtiene el DTO completo estructurado.
       - `GET /api/v1/notas-venta/ventas/{ventaId}/pdf`: Genera y retorna el documento en formato `application/pdf` con auditoría automática en bitácora.
       - `POST /api/v1/notas-venta/ventas/{ventaId}/confirmar-entrega`: Permite alternar o confirmar la entrega física del material, persistiendo en `Notas` de la venta y registrando en `IAuditLogService`.
  - **Frontend SPA (React 18, TypeScript & Vite)**:
    1. **Navegación e i18n**: Opción de menú independiente `📋 Notas de Venta` / `📋 销售单据` agregada en `App.tsx` y `accessControl.ts` para cajeros y administradores.
    2. **Pantalla Principal (`NotasVentaPage.tsx` & `.css`)**:
       - 4 tarjetas de métricas en vivo: Total Ventas, Entregadas, Pendientes de Entrega y Total comercial en página.
       - Barra de filtros avanzados (búsqueda rápida, estado de venta, estado de entrega, fecha inicio, fecha fin y botón de limpieza).
       - Tabla con 25 registros por página, badges visuales de entrega (`ENTREGADO` / `PENDIENTE`) y botones de acción rápida:
         - `👁️ Vista previa`: Diálogo modal con visor de PDF integrado para revisar antes de imprimir.
         - `📥 PDF`: Descarga directa con nombre sanitizado `NotaVenta_{folio}.pdf`.
         - `🖨️ Imprimir`: Impresión directa en tamaño carta usando iframe del navegador.
         - `🚚 Entrega`: Diálogo modal para marcar como entregado o revertir a pendiente, con captura opcional de observaciones de recepción.
    3. **Integración en Historial de Ventas (`SalesHistoryPage.tsx`)**:
       - Botón directo `📋 Nota` en la columna de acciones para descarga inmediata en 1 solo clic.
  - **Pruebas y Verificación**:
    - Backend: Compilación limpia sin errores. 122/122 pruebas unitarias e integración aprobadas (incluyendo 3 nuevas pruebas unitarias para `NotaVentaTests`: consulta paginada, alternancia de entrega auditada y generación de PDF).
    - Frontend: Build de producción con Vite exitoso en 18.30s (0 errores de TypeScript) y 53/53 pruebas de Vitest aprobadas (incluyendo pruebas unitarias para `servicioNotasVenta`).
    - Servidores activos: Backend API en `http://localhost:5000` y Frontend SPA en `http://localhost:5173`.
  - **Estado Git**:
    - Working tree local intacto, sin commits ni empujes a repositorios remotos según las reglas del proyecto.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Punto de Venta: Combo Personalizado Buscable de Clientes con Chips Fiscales y Diseño Moderno)

- **Sustitución de `<select>` Nativo por Componente de Combo Buscable (`PaginaPuntoVenta.tsx` & `.css`)**:
  - **Problema Reportado**: El combo desplegable de clientes en el Punto de Venta se mostraba con el menú gris sin estilos nativo del navegador, acumulando cadenas de texto largas y sin jerarquía visual.
  - **Solución Implementada**:
    1. **Botón Disparador Estilizado (`.pos-customer-select-trigger`)**:
       - Muestra avatar/ícono (`👥` Público General / `👤` Cliente específico), nombre en negrita, badge por tipo (`Mayorista` en verde, `Arquitecto` en morado, `Particular` en gris) y chips secundarios con RFC (`🏷️ RFC: ...`), CP (`📍 CP: ...`) y teléfono.
       - Botón directo `✕` para deseleccionar rápidamente a Público General.
       - Indicador de flecha con rotación animada de 180° al abrir/cerrar.
    2. **Menú Flotante con Búsqueda en Vivo (`.pos-customer-dropdown-menu`)**:
       - Elevación con sombra suave (`0 14px 32px rgba(0,0,0,0.2)`), bordes redondeados (10px) y animación fluida `posFadeInDown`.
       - Barra de búsqueda superior con icono `🔍`, auto-focus inmediato al desplegar y filtrado en tiempo real por nombre, razón social, RFC, teléfono o código postal.
       - Tarjetas individuales de clientes con hover interactivo y borde de acento en oro cálido (`var(--primary-main)`).
       - Opción predeterminada `Público General` resaltada con checkmark de selección.
       - Estado vacío con botón rápido para registrar a un cliente nuevo si no coincide con la búsqueda.
       - Cierre automático al hacer clic fuera del componente o presionar la tecla `Escape`.
  - **Pruebas y Verificación**:
    - Frontend: Build de producción con Vite exitoso en 16.58s con 0 errores y 51/51 pruebas de Vitest aprobadas.
    - Servidores activos: Frontend SPA en `http://localhost:5173` y Backend API en `http://localhost:5000`.
  - **Estado Git**:
    - Working tree local intacto, sin commits ni empujes a repositorios remotos.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Punto de Venta: Alta de Clientes con RFC y CP e Integración de IVA para CFDI 4.0)

- **Captura Fiscal en Modal "+ Cliente" del Punto de Venta (`PaginaPuntoVenta.tsx`)**:
  - **Requerimiento**: El cajero requería registrar clientes con su RFC y Código Postal (CP) directamente desde la pantalla de cobro del PDV sin tener que navegar al módulo administrativo de Clientes, asegurando que cuando el cliente solicite factura, los datos fiscales y el IVA del 16% viajen completos y exactos.
  - **Ajustes Realizados en Frontend (`pos-web`)**:
    1. **Campos Fiscales Agregados**:
       - `taxId` (RFC / Tax ID): Máximo 13 caracteres, conversión automática a mayúsculas, validación de formato SAT mediante regex (`^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$`).
       - `postalCode` (Código Postal Fiscal): Máximo 5 dígitos numéricos, con auto-resolución geográfica (ciudad/estado) mediante `lookupPostalCode` para verificar que pertenezca a un código postal mexicano válido (por defecto `37000 León, Gto`).
    2. **Experiencia de Usuario en Caja**:
       - Diseño responsive en doble columna dentro del diálogo modal con cierre rápido (botón `✕` y tecla `Escape`).
       - El selector de clientes en la barra lateral ahora despliega el RFC y el CP de cada cliente disponible (`DisplayName · RFC: ... · CP: ...`).
       - Al seleccionar un cliente, se muestra una barra de información fiscal confirmando su RFC y CP.
       - Al activar `¿Desea facturar? (Aplicar 16% IVA)`, se muestra una alerta contextual con el destino fiscal del CFDI (RFC del cliente o RFC genérico para público en general) y se desglosa el concepto `IVA (16% IVA)` sumado al total.
  - **Ajustes Realizados en Backend (`Pos.Infrastructure` y `Pos.Domain`)**:
    1. **Sincronización en `CatalogApplicationService.cs`**:
       - Al crear o actualizar un cliente con RFC y código postal, se sincroniza tanto `CodigoPostal` como `CodigoPostalFiscal = values.PostalCode`, permitiendo que la vista previa y el timbrado en `FacturacionApplicationService` identifiquen el domicilio fiscal sin advertencias.
    2. **Prorrateo de Descuento en `FacturacionApplicationService.cs`**:
       - Cuando una venta tiene descuento global aplicado en caja (`sale.MontoDescuento > 0`), se prorratea equitativamente a nivel centavos entre cada partida para que los conceptos del CFDI 4.0 (`Concepto.Descuento`) y el total coincidan 100% con las reglas matemáticas del SAT.
  - **Pruebas y Verificación**:
    - Backend: Compilación limpia con 0 errores y 119/119 pruebas unitarias aprobadas.
    - Frontend: Build de producción con Vite exitoso en 17.86s y 51/51 pruebas de Vitest aprobadas al 100%.
    - Servidores activos en localhost: Backend API en `http://localhost:5000` y Frontend SPA en `http://localhost:5173`.
  - **Estado Git**:
    - Working tree local intacto, sin commits ni empujes a repositorios remotos.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación: Consulta de Estado SAT Verificada al 100%)

- **Corrección de Parámetros Excluyentes en `ConsultarEstadoSat` y `SolicitarCancelacion`**:
  - **Problema Detectado**: Al consultar el estado del SAT de una factura desde el modal de detalle, FiscalAPI rechazaba la solicitud con el mensaje: `Debe proporcionar solo el 'Id' o el resto de los campos, pero no ambos.`
  - **Causa Raíz**: El SDK de FiscalAPI maneja identificación mutuamente excluyente: si se provee el `Id` interno de FiscalAPI (`FiscalapiInvoiceId`), no deben enviarse los campos fiscales `InvoiceUuid`, `IssuerTin`, `RecipientTin` ni `InvoiceTotal` (o viceversa). Nuestro código estaba enviando ambos conjuntos simultáneamente en `InvoiceStatusRequest` y `CancelInvoiceRequest`.
  - **Solución Implementada**:
    - `Pos.Infrastructure/Invoicing/Services/FacturacionApplicationService.cs`:
      - `ConsultarEstadoSatAsync`: Si `FiscalapiInvoiceId` existe, se envía exclusivamente `req.Id = factura.FiscalapiInvoiceId`. Si está ausente, se envían los campos del comprobante SAT (`InvoiceUuid`, `IssuerTin`, `RecipientTin`, `InvoiceTotal`).
      - `SolicitarCancelacionAsync`: Misma lógica excluyente aplicada a `CancelInvoiceRequest` para prevenir el mismo error al cancelar.
  - **Pruebas y Verificación**:
    - Backend recompilado limpiamente con 0 errores (`Pos.sln`).
    - Prueba real ejecutada para la factura `630237C5-A3EB-4236-9AFC-BCE4D2E43B58` (UUID `d0d67121-9578-44f6-904c-1df085cc3641`, Folio A-2):
      - `HTTP 200 OK` en 1106 ms.
      - Estado SAT devuelto por el PAC: `Vigente`.
      - Cancelabilidad: `Cancelable sin aceptación`.
      - Validación EFOS: `200` (limpia, no listada en EFOS del SAT).
      - `FacturacionFactura.EstadoSat` actualizado a `Vigente` en base de datos.
  - **Estado Git**:
    - Working tree local intacto, sin commits ni empujes a repositorios remotos.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación: Modal Flotante Centrado en Bitácora Técnica de FiscalAPI)

- **Corrección de Modal de Diagnóstico en Bitácora Técnica (`FacturacionLogDetalleModal`)**:
  - **Problema Detectado**: Al hacer clic en el botón de inspección (🔍) de un registro en la pestaña **Bitácora Técnica FiscalAPI**, el detalle de la llamada se renderizaba como bloque estático al fondo de la página por falta de clases CSS de overlay (`.modal-backdrop` y `.modal`), forzando un scroll hacia abajo en vez de abrir un diálogo superpuesto.
  - **Ajustes Implementados**:
    1. **Estilos de Modal en `FacturacionPage.css`**:
       - `.modal-backdrop` y `.pos-receipt-backdrop`: Posicionamiento fijo `position: fixed; inset: 0; z-index: 1200;` con fondo translúcido oscuro `rgba(15, 23, 42, 0.72)` y desenfoque `backdrop-filter: blur(4px)`.
       - `.modal` y `.pos-customer-modal`: Diálogo centrado, bordes redondeados (12px), sombra de elevación (25px), ancho máximo dinámico (`950px` para `modal-lg`) y animación fluida de entrada.
       - Estilos consistentes para `.modal-header`, `.modal-body` (scrollable), `.modal-footer` y botón de cierre `✕`.
    2. **Imports en Componentes**:
       - `FacturacionBitacoraTab.tsx` y `FacturacionLogDetalleModal.tsx` importan explícitamente `FacturacionPage.css`.
  - **Pruebas y Verificación**:
    - Build de frontend con Vite completado en 11.83s con 0 errores de TypeScript y empaquetado CSS exitoso.
    - El modal ahora se abre inmediatamente centrado sobre la tabla con visor de request/response sanitizados, JSON formateado con botones de copiado y cadena de llamadas vinculadas por Correlation ID.
  - **Estado Git**:
    - Working tree local intacto, sin commits ni empujes a repositorios remotos conforme a la regla del usuario.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación: Depuración de Facturas Emitidas a 1 Registro por Comprobante Timbrado)

- **Corrección de Listado en "Facturas Emitidas" (Unicidad y Exclusión de Rechazos Técnicos)**:
  - **Problema Detectado**: Cada intento fallido de timbrado previo (falta de tenant, sin saldo de timbres, discrepancia de RFC) generaba un registro con estado `Rechazada` en la tabla `FacturacionFactura`, provocando que la pestaña "Facturas Emitidas" mostrara 17 filas (15 rechazos técnicos repetidos como si fuera un log) para únicamente 2 facturas reales.
  - **Ajustes Implementados**:
    1. **Backend (`FacturacionApplicationService.cs`)**:
       - `ObtenerFacturasAsync`: Se ajustó el filtro base para que, cuando no se especifique estado, se listen únicamente comprobantes fiscales válidos (`Timbrada`, `Cancelada`, `CancelacionSolicitada`, `ResultadoPorConfirmar`), excluyendo estrictamente `Rechazada` y `Borrador`.
       - `EmitirFacturaVentaAsync`: Al reintentar el timbrado de una venta con un intento previo rechazado, se reutiliza y actualiza la entidad existente en lugar de insertar una fila duplicada.
    2. **Frontend (`FacturacionPage.tsx`)**:
       - El selector de estado tiene como valor predeterminado `Timbrada`, garantizando que al ingresar a la pestaña solo se visualicen las facturas timbradas vigentes.
       - Se removieron las opciones de rechazo técnico (`Rechazada`, `EnProceso`) del selector de comprobantes emitidos (los rechazos técnicos permanecen auditados en la pestaña "Bitácora Técnica FiscalAPI").
       - Las tarjetas de métricas muestran el total exacto de facturas timbradas (`Total Facturas: 2`, `Facturas Timbradas: 2`, `Monto Facturado: $366.56 MXN`).
    3. **Depuración de Base de Datos Local (`PosLambrinDb`)**:
       - Se depuraron los 15 registros huérfanos con estado `Rechazada` de `FacturacionFactura`, manteniendo exactamente los 2 registros timbrados oficiales (`A-1` y `A-2`).
       - Los 17 registros completos de auditoría HTTP y trazabilidad con FiscalAPI permanecen íntegros en `FacturacionLog`.
  - **Pruebas y Verificación**:
    - Backend: Compilación limpia sin errores con `dotnet build src/backend/Pos.sln`.
    - Endpoint `GET /api/v1/facturacion/invoices`: Retorna exactamente 2 elementos (`A-1` y `A-2`) en estado `Timbrada`.
    - Frontend: Build de Vite completado en 11.74s con 0 errores.
  - **Estado Git**:
    - Working tree local intacto, sin commits ni empujes a repositorios remotos.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación: Primer CFDI 4.0 Timbrado Exitosamente con FiscalAPI)

- **Primer Timbrado Oficial de CFDI 4.0 Completado con Éxito (Venta #23)**:
  - **UUID SAT**: `4cbb6b1e-a5c9-484b-9bf7-a37bc3b075df`
  - **Serie y Folio**: `A-1`
  - **Emisor SAT**: `EKU9003173C9` (*ESCUELA KEMPER URGATE*, Régimen `601`, CP `42501`) con sellos digitales CSD oficiales de prueba vinculados y activos en FiscalAPI.
  - **Receptor Fiscal**: `AEMA960428ME6` (*AARON ARENAS MARTINEZ*, Régimen `612`, CP `37545`, Uso `G03`).
  - **Artefactos Fiscales Generados y Persistidos**:
    - **XML CFDI 4.0**: Sellado digitalmente por el PAC y almacenado en base de datos (`4,861 bytes`).
    - **Representación Gráfica PDF**: Generada automáticamente, descargada y vinculada en almacenamiento local (`137,330 bytes`).
  - **Bitácora Técnica Auditada (`FacturacionLog`)**:
    - Operación: `Emitir` (POST)
    - Código HTTP real de FiscalAPI: `200 OK`
    - Latencia de timbrado: `372 ms`
    - Estado: `Succeeded = 1`, `EstadoEjecucion = Finalizada`
  - **Ajustes de Integración FiscalAPI Resueltos**:
    - Corrección en `FacturacionApplicationService.cs`: Mapeo de `TaxFlagCode = "T"` (en lugar del texto `"Traslado"`) conforme a las especificaciones del SDK oficial.
    - Asignación de `Issuer.Id` vinculado a la entidad pre-registrada en FiscalAPI (`980ec84f-90b4-4a8d-a2e7-eb19d5c60f52`).
    - Verificación del saldo de timbres: confirmada la necesidad de contar con créditos en la columna de *Créditos de Timbres* (CFDI) para el emisor.
  - **Estado Git**:
    - Trabajo íntegro preservado en el working tree local sin commits ni empujes remotos conforme a la regla del usuario.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación: Credenciales QA FiscalAPI Integradas y Verificación Exitosa)

- **Integración de Credenciales Sandbox FiscalAPI**:
  - `ApiKey`: `sk_test_e91be921_0861_4d83_b62e_676c6441a586` configurada en `appsettings.Development.json` y `appsettings.json`.
  - `Tenant`: `a97a851a-45bf-4be0-a48e-7c9bf4b18c30` configurado en `appsettings.Development.json` y `appsettings.json`.
  - Backend reiniciado exitosamente en `http://localhost:5000` con `IFiscalApiClient` instanciado contra el sandbox oficial (`https://test.fiscalapi.com`).
  - **Prueba Funcional de Extremo a Extremo en Localhost**:
    - `PUT /api/v1/facturacion/config`: Emisor de pruebas configurado con éxito (`EKU9003173C9`, `ESCUELA KEMPER URGATE`, CP `37000`, Régimen `601`).
    - `GET /api/v1/facturacion/sales/{saleId}/preview`: Vista previa de comprobante fiscal generada con éxito para la venta #22 (`VENTA-20261022-95FF80BE96504`, Total: $316.00 MXN, Subtotal: $272.42 MXN, IVA 16%: $43.58 MXN).
    - Estado de elegibilidad: `esElegibleParaFacturar = true`, `bloqueos = []`, validación SAT v4.0 completa.

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación: Corrección de Inicialización DI y Soporte Localhost)

- **Corrección de Inicialización del Cliente FiscalAPI (`Tenant is required`)**:
  - **Causa Raíz**: Al acceder a la ruta `/facturacion` en el navegador, el `FacturacionController` resuelve a través del contenedor DI a `IFiscalApiClient`. El SDK oficial `FiscalApiClient.Create(settings)` ejecuta internamente `ValidateSettings`, el cual lanza de manera estricta `ArgumentException: Tenant is required (Parameter 'Tenant')` cuando `settings.Tenant` es nulo o vacío. Dado que las credenciales no estaban configuradas en `appsettings.Development.json` ni tenían un valor por defecto en `DependencyInjection.cs`, la fábrica de controladores de ASP.NET Core fallaba arrojando error 500 antes de poder acceder incluso a la configuración.
  - **Solución Implementada**:
    - `Pos.Infrastructure/DependencyInjection.cs`: Se agregaron valores por defecto seguros (`"NOT_CONFIGURED"`) tanto para `Tenant` como para `ApiKey` y `"America/Mexico_City"` para `TimeZone` en la resolución de `IFiscalApiClient`, garantizando una degradación elegante sin romper la inicialización del controlador.
    - `Pos.Api/appsettings.json` y `appsettings.Development.json`: Se configuraron placeholders predeterminados de desarrollo (`sandbox_dev_placeholder_tenant`, `sandbox_dev_placeholder_key`).
    - Base de datos local: Restaurada exitosamente desde el respaldo de VPS (`PosLambrinDb_Prod.bak`) con la migración `20261108052821_AddFacturacionModuleCompleto` y permisos `facturacion:*` sembrados.
    - Servicios en ejecución: Backend en `http://localhost:5000` y Frontend en `http://localhost:5173`. Verificados endpoints `GET /api/v1/facturacion/config` (200 OK), `GET /api/v1/facturacion/invoices` (200 OK) y `GET /api/v1/facturacion/bitacora` (200 OK).

## 🟢 ESTADO PREVIO (08 de Noviembre, 2026 - Módulo de Facturación CFDI 4.0: Base de Datos y Bitácora Técnica de FiscalAPI)

- **Implementación Completa de Base de Datos Relacional y Bitácora Técnica de Facturación (`FacturacionLog`)**:
  - **11 Tablas Relacionales e Índices en SQL Server (EF Core 9)**:
    1. `FacturacionConfiguracion`: Parámetros fiscales y operativos por empresa, sucursal y ambiente (`QA`/`PR`).
    2. `FacturacionReferenciaExterna`: Mapeo de IDs locales con identificadores externos en FiscalAPI (`PersonId`, `ProductId`, `TaxFileId`).
    3. `FacturacionFactura`: Encabezado fiscal CFDI 4.0, UUID, importes monetarios en `decimal(18,2)`, RFC emisor/receptor, método/forma de pago y estados.
    4. `FacturacionFacturaDetalle`: Conceptos con claves SAT, unidad, descripción, SKU, cantidades y precios en `decimal(18,4)`.
    5. `FacturacionFacturaImpuesto`: Desglose pormenorizado de impuestos (Base `decimal(18,2)`, Tasa `decimal(18,6)`, Importe `decimal(18,2)`).
    6. `FacturacionFacturaVenta`: Relación asociativa N:M entre ventas locales y comprobantes fiscales.
    7. `FacturacionIntento`: Historial de solicitudes de emisión/revisiones con llaves de idempotencia únicas y resultado.
    8. `FacturacionDocumento`: Almacenamiento seguro y cacheo de artefactos fiscales (XML timbrado, representación gráfica PDF, Acuses) con hash SHA256 para evitar consumo redundante de cuota del PAC.
    9. `FacturacionCancelacion`: Solicitudes ante el SAT (motivos `01`-`04`, UUID de sustitución, fecha y acuse XML).
    10. `FacturacionHistorialEstado`: Trazabilidad funcional inmutable de transiciones de estado con usuario y timestamp UTC.
    11. `FacturacionLog`: **Bitácora técnica exclusiva** de cada interacción HTTP con FiscalAPI.
    - Migración EF Core versionada: `20261108052821_AddFacturacionModuleCompleto.cs` aplicada al snapshot.
  - **Bitácora Técnica Profunda (`FacturacionLog` & `IFacturacionLogService`)**:
    - Ciclo de vida en dos fases: Pre-llamada (`Iniciada`) antes del envío HTTP y Post-llamada (`Finalizada` / `Interrumpida` / `ResultadoIncierto`).
    - Medición exacta de latencia en milisegundos (`DuracionMs`).
    - Registro del código HTTP real recibido (`HttpStatusCode`) del proveedor.
    - **Regla Estricta de Cero Códigos Sintéticos**: Ante caídas de red, DNS o timeouts, `HttpStatusCode` permanece en `NULL`, con `EstadoEjecucion = "ResultadoIncierto"` o `"Interrumpida"`, evitando inventar respuestas HTTP inexistentes.
    - Manejo resiliente de timeouts en emisión: la factura local pasa a `ResultadoPorConfirmar` sin arrojar 500 al cajero, permitiendo conciliación SAT sin duplicar timbres.
  - **Sanitización Estricta de Seguridad (`FacturacionSanitizer`)**:
    - Enmascaramiento recursivo (`***REDACTED***`) de API keys, tenant keys, passwords, bearer tokens y certificados.
    - Exclusión de payloads voluminosos en Base64 (`[BASE64_DOCUMENT_DATA_EXCLUDED: length=X]`).
    - Truncamiento de seguridad con límite de 16 KB para prevenir ataques de saturación en base de datos.
  - **Endpoints REST en `FacturacionController`**:
    - `GET /api/v1/facturacion/bitacora`: Paginación y filtrado multicriterio (fechas, ambiente, operación, resultado, correlationId, UUID, venta, HTTP status).
    - `GET /api/v1/facturacion/bitacora/{id}`: Detalle completo de una llamada individual.
    - `GET /api/v1/facturacion/bitacora/relacionadas/{correlationId}`: Rastreo de la cadena completa de llamadas de una operación.
    - `POST /api/v1/facturacion/bitacora/purgar`: Purga controlada de logs técnicos por días de retención (mínimo 30 días, default 90), restringida a `facturacion:bitacora_purgar`.
  - **Frontend Diagnóstico (`pos-web`)**:
    - Pestaña de navegación en `FacturacionPage.tsx`: `🧾 Facturas Emitidas` y `📡 Bitácora Técnica FiscalAPI`.
    - Componente `FacturacionBitacoraTab.tsx`: Filtros de búsqueda, métricas de rendimiento (llamadas totales, tasa de éxito, errores 4xx, latencia promedio), tabla interactiva y modal de confirmación de purga.
    - Modal `FacturacionLogDetalleModal.tsx`: Resumen técnico, inspector de Request/Response sanitizados en formato JSON con botones de copiado rápido, traza de excepción y visor de cadena de llamadas correlacionadas.
    - Soporte bilingüe en `i18n/index.ts` (Español y Chino Simplificado `zh-CN`).
  - **Pruebas y Verificación**:
    - Backend: **119/119 pruebas unitarias e integración pasando al 100%** (19 Domain, 79 Application, 21 Api.IntegrationTests).
    - Frontend: **51/51 pruebas de Vitest pasando al 100%** (8 suites de prueba). Build de producción Vite completado exitosamente en 11.42s con 0 errores.
  - **Estado Git**:
    - En apego estricto a la instrucción del usuario (*"no, no hacer commit . necesito hacer pruebas de manera localhost en mi equipo"*), todos los cambios se conservan en el árbol de trabajo local sin realizar commits ni empujes remotos.

## 🟢 ESTADO PREVIO (07 de Octubre, 2026 - Corrección CORS para Cloudflare Workers en Producción)

- **Corrección de Bloqueo de CORS para el Frontend PDV en Cloudflare Workers (`pos-wpcbajio.aaronarenasmartinez.workers.dev`)**:
  - **Causa Raíz**: Durante la remediación de seguridad previa, la eliminación de comodines permisivos (`*.workers.dev`) dejó fuera el origen de despliegue legítimo del PDV Cloudflare Worker (`https://pos-wpcbajio.aaronarenasmartinez.workers.dev`). Al realizar peticiones preflight OPTIONS a `https://api.wpcbajio.com/api/v1/...`, el backend ASP.NET Core respondía sin la cabecera `Access-Control-Allow-Origin`, provocando el bloqueo del navegador.
  - **Ajustes en Backend (`Program.cs` y `appsettings.json`)**:
    - `Program.cs`: `SetIsOriginAllowed` actualizado para permitir de forma segura el host de producción `pos-wpcbajio.aaronarenasmartinez.workers.dev`, así como cualquier subdominio del tenant oficial autorizado (`*.aaronarenasmartinez.workers.dev` y `*.aaronarenasmartinez.pages.dev`), manteniendo el bloqueo estricto contra orígenes no autorizados o comodines globales.
    - Soporte dinámico para `Cors:AllowedOrigins` con normalización `TrimEnd('/')`.
    - `appsettings.json` y `appsettings.Development.json`: Añadida la sección `Cors:AllowedOrigins` con los dominios oficiales de producción y desarrollo.
  - **Pruebas Automatizadas**:
    - Nueva suite de pruebas de integración `CorsTests.cs` (7 casos de prueba con `[Theory]` e `[InlineData]`), validando que los orígenes autorizados reciben `Access-Control-Allow-Origin` y `Access-Control-Allow-Credentials: true`, mientras que los dominios maliciosos/no autorizados son rechazados.
    - 104/104 pruebas del backend pasando al 100% (19 Domain, 64 Application, 21 Api.IntegrationTests).
    - 47/47 pruebas de Vitest del frontend pasando al 100%.
  - **Despliegue y Validación en Vivo en VPS (`193.46.198.88`)**:
    - Llave SSH (`id_ed25519.pub`) agregada y autorizada permanentemente en `/root/.ssh/authorized_keys`.
    - `/var/www/pos-api/appsettings.json` actualizado con los orígenes CORS permitidos.
    - Binarios Release de `Pos.Api` (`Pos.*.dll` y `Pos.*.pdb`) sincronizados en `/var/www/pos-api`.
    - Servicio `pos-api.service` reiniciado y verificado en estado `active (running)`.
    - Verificación exitosa en vivo con curl hacia `https://api.wpcbajio.com/api/v1/auth/login` y `/api/v1/products`: las peticiones preflight OPTIONS retornan `HTTP 204 No Content`, `Access-Control-Allow-Origin: https://pos-wpcbajio.aaronarenasmartinez.workers.dev` y `Access-Control-Allow-Credentials: true`.
    - Los registros del servicio en el VPS confirman: `CORS policy execution successful`.

## 🟢 ESTADO PREVIO (05 de Octubre, 2026 - Madrugada / Auditoría y Remediación de Seguridad Extrema)

- **Remediación Exhaustiva de Vulnerabilidades en Backend (.NET 9) y Frontend (Next.js)**:
  - **Vulnerabilidad 1 (Crítica) — `simulate-webhook`**:
    - Se eliminó el vector de evasión y simulación de pagos fraudulentos.
    - Se restringió a entornos locales de desarrollo (`!_env.IsDevelopment() => 404 Not Found`).
    - Se requirió autorización interna estricta: `[Authorize(Policy = PermissionCodes.Sales.Process)]`.
    - Verificado en producción con curl: responde `HTTP 404 Not Found`.
  - **Vulnerabilidad 2 (Crítica) — Contraseñas de Clientes Web y Backdoor "WPC123"**:
    - Se eliminó la puerta trasera `"WPC123"` en `StoreController.cs`.
    - Se integró `IPasswordHasherService` (PBKDF2 HMAC-SHA256, 100,000 iteraciones + salt aleatorio de 16 bytes).
    - Cuentas existentes con hash legado/texto plano se migran de forma automática y transparente a hash criptográfico en su primer login.
  - **Vulnerabilidad 3 (Alta) — Restablecimiento de Contraseñas / Account Takeover (ATO)**:
    - Se introdujo validación de factor secundario de identidad: teléfono registrado del cliente.
    - `POST /api/v1/store/customers/reset-password` exige coincidencia del teléfono antes de actualizar el hash de contraseña.
    - Frontend (`/cuenta`) actualizado para solicitar y validar el teléfono antes de enviar la solicitud.
  - **Vulnerabilidad 4 (Alta) — Manipulación Anónima de Tracking de Pedidos**:
    - Se eliminó el atributo `[AllowAnonymous]` a nivel de clase en `StripePaymentsController`.
    - `PUT /api/v1/payments/stripe/web-orders/{id}/tracking` ahora exige permiso estricto `[Authorize(Policy = PermissionCodes.Sales.Process)]`.
    - Verificado en producción con curl: peticiones anónimas reciben `HTTP 401 Unauthorized`.
  - **Vulnerabilidad 5 (Media) — Fuga de Mensajes de Contacto y Datos Personales (PII)**:
    - Se eliminó el acceso público anónimo a `GET /api/v1/store/contact-messages`.
    - Ahora exige autenticación y permiso de visualización de clientes: `[Authorize(Policy = PermissionCodes.Customers.View)]`.
    - Verificado en producción con curl: responde `HTTP 401 Unauthorized`.
  - **Vulnerabilidad 6 (Alta) — CORS Permisivo y Comodines de Terceros**:
    - Se eliminaron comodines `.workers.dev` y `.pages.dev` en `Program.cs`.
    - Orígenes autorizados restringidos estrictamente a `localhost`, `127.0.0.1`, `wpcbajio.com`, `admin.wpcbajio.com`, `api.wpcbajio.com` y dominios oficiales configurados.
  - **Vulnerabilidad 7 (Media/Alta) — Ataques de Fuerza Bruta y Denial of Service (DoS)**:
    - Se implementó ASP.NET Core Sliding Window Rate Limiting en `Program.cs` (`app.UseRateLimiter()`).
    - Políticas `auth` (15 req/min) y `payments` (20 req/min) aplicadas por IP en endpoints de login, registro, reseteo de contraseña y creación de pasarelas Stripe.
  - **Vulnerabilidad 8 (Media) — Credenciales Estáticas en Código para Serilog UI**:
    - Se eliminaron credenciales fijas en código fuente en `SerilogAuthMiddleware.cs`.
    - Ahora se leen dinámicamente de `IConfiguration` / variables de entorno.
    - Se implementó validación criptográfica en tiempo constante (`CryptographicOperations.FixedTimeEquals`) contra ataques de canal lateral (timing attacks).
  - **Vulnerabilidad 9 (Media) — Margen E-Commerce sin Validación de Rol**:
    - `PUT /api/v1/system-settings/ecommerce-pricing` blindado con `[Authorize(Policy = PermissionCodes.Catalog.ProductsEdit)]`.
  - **Despliegue y Validación en VPS (`193.46.198.88`)**:
    - Backend: publicado en Release, sincronizado a `/var/www/pos-api`, `pos-api.service` reiniciado y validado en tiempo real.
    - Frontend: cambios sincronizados a `/var/www/ecommerce`, `npm run build` completado exitosamente y `pm2 reload wpc-ecommerce` verificado.

## 🟢 ESTADO PREVIO (05 de Octubre, 2026 - Madrugada / Privacidad de Pedidos)

- **Protección de Privacidad de Datos (PII) y Blindaje contra IDOR en Consulta de Pedidos (`/pedido/[folio]`)**:
  - **Detección del Problema**: Al pegar el enlace `/pedido/{folio}` en modo incógnito/privado sin iniciar sesión, un tercero no autenticado podía visualizar el estado completo de la orden, los artículos comprados y los datos sensibles del comprador (Nombre, Apellidos, Teléfono, Correo y Dirección de Entrega).
  - **Blindaje Backend (`StripePaymentsController.cs`)**:
    - Se agregó el parámetro `[FromQuery] string? email` al endpoint público `GET /api/v1/payments/stripe/orders/{folio}`.
    - Se implementó validación estricta de identidad: si el correo electrónico no se proporciona o no coincide exactamente con el cliente registrado en la venta (`sale.Cliente.Email`), el backend responde con **HTTP 403 Forbidden** (`requiresVerification: true`).
    - Ningún dato de cliente, dirección, partidas o montos se entrega a peticiones sin validación.
  - **Blindaje Frontend (`order-status-client.tsx`, `checkout.ts`, `services/index.ts`)**:
    - Nueva pantalla de seguridad con diseño profesional y escudo protector (`ShieldCheck`): solicita el correo del comprador antes de revelar el pedido en ventanas privadas o anónimas.
    - Si el comprador ya inició sesión en `/cuenta` o acaba de finalizar el pago en su navegador, el email se valida transparentemente sin fricción.
    - Eliminado el mock estático residual en `services/index.ts` que mostraba datos ficticios de prueba.
  - **Despliegue en Producción (VPS)**:
    - Backend compilado y transferido a `/var/www/pos-api`; servicio `pos-api.service` reiniciado y verificado.
    - Frontend compilado con `npm run build` en `/var/www/ecommerce` y proceso PM2 `wpc-ecommerce` recargado al 100%.
    - Verificado con curl: peticiones anónimas o con correo erróneo reciben `HTTP 403 Forbidden`, mientras que el correo legítimo del comprador desbloquea `HTTP 200 OK`.

## 🟢 ESTADO PREVIO (05 de Octubre, 2026 - Madrugada)

- **Estandarización de Órdenes Web por Folio Operativo (`IdVenta`), Canal `WEB` y Corrección de Visualización**:
  - **Identificador de Pedido (`IdVenta` + Badge `WEB`)**:
    - Backend (`StripePaymentsController.cs`): Al registrar la venta en SQL Server, `sale.NumeroFolio` ahora adopta el identificador limpio consecutivo `WEB-{sale.IdVenta}` y registra los metadatos `IdVenta` y `Channel: "WEB"`.
    - Endpoint público `GET /api/v1/payments/stripe/orders/{folio}` ahora permite consultar órdenes no solo por UUID o folio antiguo, sino también por `IdVenta`, `#IdVenta` o `WEB-IdVenta` sujeto a validación de correo.
    - Frontend E-Commerce (`app/cuenta/page.tsx`, `components/order/order-status-client.tsx`, `app/pago-exitoso/page.tsx`): Los pedidos se visualizan como `Venta #{idVenta}` acompañados de la etiqueta distintiva `WEB` (y conservan su folio de referencia en caso necesario).
    - Frontend PDV (`WebOrdersPage.tsx`): Tabla de órdenes web estandarizada para mostrar `Venta #{idVenta}` con badge `WEB`.
  - **Corrección de Errores de Mapeo y Valores `NaN` e `Invalid Date`**:
    - Backend: Expone propiedades normalizadas duales en las respuestas de órdenes: `createdAt` / `createdAtUtc`, `unitPrice` / `pricePerUnit`, `lineTotal`, y `imageUrl` / `image`.
    - Frontend E-Commerce: Capa de normalización centralizada en `services/api/checkout.ts` (`normalizeOrder`) y parseo seguro de fechas (`formatOrderDate`), eliminando por completo los textos `$NaN`, `$NaN c/u` y `Registrado el Invalid Date`.
    - Fallback de imagen de producto: Cuando un producto en BD no cuenta con foto o `imageUrl` es cadena vacía, se renderiza un thumbnail con ícono representativo `<Package />` evitando renderizados rotos `<img src="" />`.
  - **Compilación y Pruebas**:
    - Backend .NET: 97/97 pruebas xUnit aprobadas con 0 errores.
    - Frontend PDV (`pos-web`): Compilación de producción exitosa con Vite.
    - Frontend E-Commerce (`WPCBajio-Ecommerce`): Build de producción de Next.js exitoso (122 rutas optimizadas).

## 🟢 ESTADO PREVIO (04 de Octubre, 2026 - Tarde / Noche)

- **Reactivación y Desbloqueo de Pagos Stripe para E-Commerce en Producción (PR)**:
  - **Desbloqueo de Modo Mantenimiento (`StripePaymentsController.cs` y `appsettings.json`)**:
    - Se eliminó el bloqueo preventivo temporal que retornaba HTTP 503 por validación de titular de cuenta hasta el 25 de septiembre.
    - Se configuró `"MaintenanceMode": false` por defecto y explícitamente en el entorno de producción (`/var/www/pos-api/appsettings.json`).
    - Se ajustó el filtro de validación de monto mínimo en el backend a `$1.00 MXN` (`amountCents >= 100`).
    - Se introdujo un bloque especializado `catch (StripeException sEx)` que expone directamente los mensajes nativos de la API de Stripe en lugar de devolver errores genéricos HTTP 502.
  - **Sincronización de Catálogo e Inventario para Producto de Prueba (ID 106)**:
    - Producto autorizado: ID 106 / UUID `7E6E4E93-02BB-478C-872F-A4659806B027`, SKU `ZWEB`, Nombre `PRUEBA`.
    - Se configuró `PrecioOnlineManual = 1.00` en SQL Server para neutralizar el redondeo del markup online (4.88% + múltiplos de 0.50), garantizando que el precio web coincida con `$1.00 MXN` sin alterar el precio de catálogo de mostrador (`PrecioUnitario = 1.00`).
    - Inventario confirmado en SQL Server: 10 unidades disponibles (`CantidadDisponible = 10.00`).
  - **Reactivación de Interfaz de Usuario E-Commerce (`WPCBajio-Ecommerce` / `wpcbajio.com`)**:
    - Se desactivó el modo de mantenimiento (`MAINTENANCE_CONFIG.enabled: false` en `lib/maintenance.ts`).
    - Eliminado el banner visible de cabecera y el cuadro de mantenimiento del carrito de compras.
    - Reactivado el botón de compra a estado primario sólido (`Continuar compra`), enlazando fluidamente con la carga de Stripe Payment Element y la confirmación de pago.
    - El build de Next.js (`npm run build`) se ejecutó exitosamente (122 rutas optimizadas) y el servicio PM2 `wpc-ecommerce` fue reiniciado operando al 100%.
  - **Verificación de Conectividad con Stripe**:
    - Backend responde 200 OK con `clientSecret` y `paymentIntentId` en `POST /api/v1/payments/stripe/create-payment-intent`.
    - Endpoint público de webhooks `POST /api/v1/payments/stripe/webhook` verificado y operativo con validación de firma criptográfica.
    - Pruebas unitarias backend: 97/97 pruebas superadas al 100% (0 errores).

## 🟢 ESTADO PREVIO (24 de Septiembre, 2026 - Tarde / Noche)

- **Corrección de Búsqueda Predictiva de SKU (`LAM-15`), Despliegue de Catálogo y Consulta de Stock en Vivo**:
  - **Filtro de Producto en Movimientos (`inventory-product-search` / `MovementCaptureModal.tsx`)**:
    - **Capacidad ampliada (al menos 30 a 50 elementos)**: Se incrementó la consulta remota a `pageSize: 100` y el contenedor desplegable a `maxHeight: 320px` con scroll independiente.
    - **Despliegue al enfocar**: Al dar clic o enfocar el input sin texto previo, despliega las opciones activas del catálogo para selección ágil sin forzar la escritura.
    - **Búsqueda Precisa e Inmune a Variantes de Guiones (`LAM-15`, `LAM‑15`, `LAM15`, `LAM 15`)**:
      - Backend (`CatalogApplicationService.cs`): `GetProductsAsync` y `GetProductByCodeAsync` ahora normalizan y comparan el término de búsqueda contra múltiples variantes ortográficas simultáneas: guion ASCII 45 (`-`), guion de no separación Unicode 8209 (`\u2011`), variantes de guion tipográfico (`\u2013`, `\u2014`) y forma compacta sin guiones (`Replace("-", "")`).
      - Esto resuelve de raíz la discordancia en la base de datos DEV donde existían SKUs importados con código Unicode 8209.
      - Frontend: Filtro cliente robusto que evalúa coincidencias exactas, códigos normalizados y nombres de producto.
  - **Corrección del Error de Existencias a 0 ("Existencia Actual: 0 Piezas")**:
    - `inventoryService.getStockByProductId`: Ahora consume directamente el endpoint REST dedicado `GET /api/v1/inventory/product/{productId}` en lugar de invocar `/inventory?search={guid}` (el cual fallaba en SQL Server al comparar un GUID contra columnas de texto, devolviendo 0 resultados y forzando la existencia a 0).
    - Mecanismo de persistencia y salvaguarda: Se mantiene `availableQuantity` del producto de catálogo como valor base inmediato y de fallback, garantizando que nunca se produzca un destello a 0 piezas ni reseteo erróneo de la existencia.
    - Actualizado análogamente en `PurchaseReceiptsPage.tsx`.
  - **Pruebas y Verificación**:
    - Backend: 97 pruebas de xUnit superadas al 100% (0 errores).
    - Frontend: 47 pruebas de Vitest superadas al 100% (0 errores).
    - Compilación de producción: `npm run build` ejecutada exitosamente con 0 errores.
    - Cumplimiento de regla de entorno: Pruebas ejecutadas estrictamente en entorno local DEV.

## 🟢 ESTADO PREVIO (24 de Septiembre, 2026 - Tarde)

- **Trazabilidad de Cantidad Anterior y Resultante en Movimientos y Ajustes de Inventario**:
  - **Módulo de Movimientos de Inventario (`InventoryMovementsPage.tsx`)**:
    - Incorporación de las columnas `Stock Previo / 原库存` (`previousQuantity`) y `Stock Final / 最终库存` (`newQuantity`) junto a `Cantidad / 数量`.
    - Cobertura integral y auditada para: **Ventas**, **Devoluciones**, **Ajustes de Inventario Físico**, **Entradas** y **Salidas**.
    - Ordenamiento dinámico bidireccional por `previousQuantity` y `newQuantity` tanto en el cliente como en el backend (`InventoryApplicationService.cs`).
    - Ordenamiento por defecto por **Fecha más reciente primero (`createdAtUtc` descendente)**: la vista despliega de forma predeterminada los últimos movimientos en la cima de la tabla (con flecha ▼ activa en `Fecha / 日期`), evitando que los ajustes o ventas recientes queden al final.
    - Inclusión de ambas columnas en las plantillas de exportación a Excel y PDF (`exportConfig`).
    - Botón de acceso directo `➕ Capturar Movimiento` en la barra superior del historial de movimientos.
  - **Componente Reutilizable `MovementCaptureModal.tsx`**:
    - Centraliza la captura de movimientos para `InventoryListPage.tsx` e `InventoryMovementsPage.tsx`.
    - Cálculo reactivo en vivo: muestra en una tarjeta dedicada el **Stock Anterior**, la **Variación / Ajuste Físico** con indicador visual de diferencia neta (`+X dif` / `-X dif`) y el **Stock Final** resultante.
    - Prevención y alerta si una salida excede las existencias disponibles.
  - **Pruebas y Verificación**:
    - Backend: 97 pruebas de xUnit superadas al 100%.
    - Frontend: 47 pruebas de Vitest superadas al 100%.
    - Build de producción: `npm run build` ejecutado exitosamente con 0 errores y 0 advertencias de TypeScript.

## 🟢 ESTADO PREVIO (24 de Septiembre, 2026 - Mañana)

- **Corrección Exhaustiva de Internacionalización en Chino Simplificado (zh-CN) y Español (es)**:
  - **Módulo de Pedidos Web (`WebOrdersPage.tsx`)**:
    - Pestaña de navegación en navbar traducida (`navWebOrders`: `Pedidos Web (CDC)` / `网店订单 (CDC)`).
    - Título, subtítulo, botón de refresco y tarjetas de métricas traducidas al chino (`商城订单总数`, `待打包 / 待发货`, `运输中 / 已发货`, `已成功签收`).
    - Buscador, selector de estatus, resumen de conteo, badges de modalidad (recogida en tienda / envío a domicilio), estados de entrega y columnas de tabla normalizadas.
    - Modal de asignación de guía de paquetería y detalle de pedido 100% traducidos (datos de cliente, destino, partidas, unidades, desglose financiero, transportista y estados de despacho).
  - **Módulo de Inventario (`InventoryListPage.tsx`)**:
    - Encabezados principales traducidos reactivamente al chino (`WPC Bajío 库存管理` y `仓库现货管理、补货预警及库存出入库变动登记`).
    - Buscador predictivo en el modal de movimientos con etiquetas bilingües y placeholder traducido.
  - **Módulo de Categorías (`CategoryListPage.tsx`)**:
    - Título y subtítulo traducidos (`WPC Bajío 分类目录` y descripción funcional).
    - Filtros de estado (todas, activas, inactivas), buscador, botón de limpiar filtros y nuevo registro internacionalizados.
    - Modal de alta y edición de categorías con todos los campos traducidos al chino.
  - **Corte de Caja y Turnos (`CashShiftPage.tsx`)**:
    - Resumen y tarjeta de Corte Z: *Esperado en Caja ($)* (`钱箱预期金额 ($)`), *Fondo* (`开班底金`), *Ingresos* (`存入现金`), *Ventas/Abonos Efec.* (`现金销售/还款`), *Retiros* (`现金支出`) y *Ventas/Abonos Efectivo* traducidos con precisión técnica en chino.
    - Mensajes y botones de contingencia para forzar Corte Z internacionalizados.
  - **Barra de Navegación y Perfil de Usuario (`App.tsx`)**:
    - Localización dinámica de los roles de usuario (ej. Administrador -> 管理员).
  - **Pruebas y Verificación**:
    - Backend: 95 pruebas unitarias e integración superadas (100% exitoso).
    - Frontend: 47 pruebas unitarias de Vitest superadas (100% exitoso).
    - Compilación de producción: `npm run build` completada con éxito (0 errores).

## 🟢 ESTADO PREVIO (18 de Septiembre, 2026)

- **Rama Activa de Mantenimiento (`mantenimiento-observaciones-18-septiembre`)**:
  - **Objetivo**: Implementación completa de las observaciones solicitadas el 18 de septiembre para PDV, Login, Contratos, Inventario y el nuevo Módulo de Recibos de Compra.
  - **Estado**: Código 100% implementado y validado en local. Sin commits realizados aún, en espera del VoBo del usuario tras su validación en `localhost`.
  - **Ajustes Realizados**:
    1. **🛒 Punto de Venta (PDV)**:
       - Zoom 80% y 60% corregido (el contenedor ya no se encoge; `.main-content` max-width 100% y layout con clamp elástico).
       - Búsqueda en catálogo con normalización de guiones Unicode de Mac (`\u2010` a `\u2015`, `\u2212`, etc.), case-insensitive y búsqueda estricta por SKU / código de barras con y sin guiones.
       - Escáner superior estricto por código de barras.
       - Carga de categoría por defecto con `pageSize: 500` (desplegando los 104+ productos) con indicador spinner visual al cambiar de categoría.
       - Referencia bancaria obligatoria (50 caracteres) para pagos con Tarjeta o Mixtos (tarjeta/transferencia); bloqueo y modal de alerta si no se captura.
       - Regla de mayoreo para *Lambrin Interior 格栅板*: $\ge 2$ cajas todas a mayoreo; 1 caja a mayoreo y sueltas a menudeo; $<1$ caja a menudeo. Desglose detallado visual en carrito acomodado en fila propia para evitar desbordes.
       - Calculadora m² con modal ampliado y responsivo (`min(780px, 96vw)`), botones estilizados con color corporativo activo, 100% traducida en Chino y Español, buscador predictivo flotante sin límite de corte (desplegando los 23+ productos completos de LAM-01 a LAM-23 y catálogo remoto hasta 200 items), con `overscroll-behavior: contain` para evitar pérdidas de scroll.
       - Descuento manual transformado a porcentaje (ej. 15 = 15%) con recálculo exacto de IVA sobre base gravable (`subtotal - descuento`).
       - Comprobante térmico de venta fijado en español (sin chino) y eliminación del badge `PRÓXIMAMENTE` junto a `www.wpcbajio.com`.
       - Tarjeta de producto con soporte multilínea para nombres largos (ej. *Cierre de encaje / Clips 扣*) y 3 columnas desktop para mejor legibilidad.
    2. **🔐 Login**:
       - Campos de correo y contraseña limpios por defecto (`''`), sin credenciales prellenadas.
    3. **🏷️ Navbar y Footer**:
       - Eliminado el botón/indicador de modo claro.
       - Pie de página actualizado: `WPC Bajío — Punto de Venta e Inventario Lambrín Decorativo / 销售点与装饰墙板库存管理系统`.
    4. **📄 Contratos A4**:
       - Generación automática y simultánea de la versión en Chino Simplificado (`[ZH] / [中文]`) al guardar una plantilla en español.
    5. **🏭 Inventario (Capturar Movimiento)**:
       - Reemplazado el selector estático de 25 productos por un buscador en vivo por SKU / código de barras sobre todo el catálogo con existencias en tiempo real.
    6. **📦 Módulo de Recibos de Compra**:
       - Nueva entidad `ReciboCompra`, DTOs, servicio, controlador REST y migración EF Core (`AddPurchaseReceiptsModule`) aplicada en BD local.
       - Interfaz completa en React `PurchaseReceiptsPage.tsx` con alta/edición, ajuste de stock dinámico, actualización de costo y voucher térmico de 80mm.
       - Columnas de la tabla y reportes de exportación en formato simultáneo bilingüe Español / Chino (`Folio / 单号`, `Fecha / 日期`, `ID Prod. / 产品编号`, `Producto / 产品说明`, `SKU / Código / 货号`, `Precio Costo / 成本价`, `Precio Venta / 零售价`, `Stock Previo / 原库存`, `Cantidad / 进货数量`, `Stock Resultante / 最终库存`, `Operador / 操作员`, `Acciones / 操作`), además de traducción total al chino y español de modales, botones y filtros.
  - **Pruebas y Verificación**:
    - Backend: 95 pruebas unitarias e integración superadas (100% exitoso).
    - Frontend: 47 pruebas unitarias de Vitest superadas (100% exitoso).
    - Compilación de producción: `npm run build` completada con éxito.

- **Script Maestro de Inicio Simultáneo de Servicios (`iniciar_servicios.py` y `iniciar_servicios.bat`)**:
  - **Objetivo**: Levantar y supervisar en una sola consola todos los subsistemas del ecosistema WPC Bajío sin requerir 5 terminales manuales ni lidiar con puertos bloqueados.
  - **Subsistemas Integrados**:
    1. `Pos.Api` (.NET 9 Web API): Puerto 5000 (`http://localhost:5000/swagger`)
    2. `Visor de Etiquetas` (React Vite): Puerto 5173 (`http://localhost:5173/`)
    3. `pos-web` (PDV React Vite): Puerto 5174 (`http://localhost:5174/`)
    4. `media-studio` (Catálogos React Vite): Puerto 5175 (`http://localhost:5175/`)
    5. `WPC-BajioEcommerce` (Next.js): Puerto 3000 (`http://localhost:3000/`)
  - **Características y Resiliencia**:
    - Detección de puertos ocupados mediante `socket.create_connection` (soporta IPv4 e IPv6 `::1` de Node/Vite) para no generar colisiones de procesos.
    - Soporte UTF-8 nativo en consola de Windows para evitar errores con caracteres especiales.
    - Modo chequeo rápido de estado con bandera `--status` o `-s`.
    - Apagado limpio y coordinado (`Ctrl+C`) que termina el árbol de subprocesos completo vía `taskkill /F /T`.
    - Lanzador `.bat` de acceso directo para doble clic desde el Explorador de Windows.

- **Corrección de Bucle Infinito de Renderizado en Catálogo y Carga Continua (`Cargando datos...`)**:
  - **Problema Detectado en PR**: Al ingresar al Catálogo de Productos (`PaginaCatalogoProductos.tsx`), la vista se quedaba permanentemente en `Cargando datos...` sin mostrar productos. Inspeccionando el servidor VPS, el proceso API recibía peticiones recurrentes ininterrumpidas cada ~400ms acumulando más de 12 minutos de CPU en 20 minutos.
  - **Causa Raíz**: En `PaginaCatalogoProductos.tsx`, la función `cargarDatos` tenía como dependencia en `useCallback` al objeto completo `pagination` (`[filtrosAplicados, pagination, sortDirection, sortKey, t]`). Cuando la API retornaba datos, se invocaba `pagination.setPaginationFromResult(prodsData)`, lo que actualizaba el estado en `usePagination`. Como el hook `usePagination` no memoizaba su objeto retornado, devolvía una nueva referencia en cada render. Esto recreaba `cargarDatos`, lo que a su vez disparaba el `useEffect([cargarDatos])`, volviendo a ejecutar la consulta, llamando a `setCargando(true)` y repitiendo el ciclo indefinidamente.
  - **Solución Implementada**:
    - En `PaginaCatalogoProductos.tsx`, se desacopló `pagination` de las dependencias de `useCallback` usando `useRef(pagination)` para `paginationRef.current.setPaginationFromResult(prodsData)`. La función ahora depende únicamente de valores primitivos: `[filtrosAplicados, pagination.pageNumber, pagination.pageSize, sortDirection, sortKey, t]`.
    - En `usePagination.ts`, se envolvió el objeto de retorno en `useMemo` y se optimizó `setPaginationFromResult` para verificar igualdad con el estado previo (`prev !== newValue ? newValue : prev`), evitando re-renders redundantes.
    - Se reinició el servicio `pos-api` en el VPS para liberar los hilos saturados por el bucle.
  - **Pruebas y Verificación**: 47/47 pruebas unitarias de Vitest superadas (100%), compilación `tsc && vite build` completada con éxito en 11.99s. Cambios enviados a `main`, `version-final-de-PR` y `feature/ecommerce-fase-2`.

  - **Sincronización Local vs PR**:
    - Conexión remota exitosa a SQL Server en VPS (`193.46.198.88:1433`).
    - Base de datos local `PosLambrinDb` sincronizada al 100% con los datos capturados en Producción: **105 productos**, **13 categorías** y sus correspondientes registros de inventario en `Stocks`.
    - Script automatizado reusable: [`scripts/development/Sync-PrToLocal.ps1`](file:///d:/Proyecto_PDV-CDC/scripts/development/Sync-PrToLocal.ps1).
  - **Integración con Sistema de Etiquetas Térmicas (`D:\Visozr Etiquetas`)**:
    - Se extrajeron todos los 104 productos activos directamente desde `PosLambrinDb` para alimentar el archivo de presets [`D:\Visozr Etiquetas\src\data\presets.ts`](file:///D:/Visozr%20Etiquetas/src/data/presets.ts) con sus SKUs, códigos de barras, nombres bilingües, colores, dimensiones exactas (`Largo × Ancho × Alto`) y precios p/pza con IVA.
    - **Recuadro con Imagen de Producto y Propuesta de Fondo de Arte Dorado / Montañas Orientales (100mm × 60mm)**:
      - Para los productos que ya tienen imagen cargada en la base de datos (31 productos: series `LAM-01` a `LAM-23` y `MOS-01` a `MOS-15`), se exportaron las fotos a `D:\Visozr Etiquetas\public\product-images/`.
      - Se añadió un recuadro de miniatura de 18mm × 18mm con bordes redondeados y marco dorado suave en el cuerpo central de la etiqueta térmica (`ThermalLabel.tsx`), a la derecha de la información técnica (Color, Dimensiones, SKU).
      - **Propuesta Final de Fondo (Montañas y Olas en Oro Fluido con Niebla Marfil)**: Se implementó la nueva obra gráfica solicitada por el cliente (`label-bg-art.png`), centrada horizontalmente para destacar los riscos dorados y ondas satinadas, con un velo translúcido y marco perimetral dorado (`rgba(205, 165, 75, 0.55)`), garantizando un diseño de máxima elegancia visual y contraste nítido con el texto café oscuro (`#241408`), código de barras y precio con IVA.
      - **Filtro Rápido y Subida Manual**: En `LabelForm.tsx` se incorporó el botón de filtro `📷 Con foto en BD (31)` y el botón `📁 Subir / Cambiar Foto...` para que cualquier modelo sin foto pueda tener su imagen cargada al instante desde la computadora.
      - Para los productos que aún no cuentan con foto cargada (o si se elimina la imagen en el formulario con `✕ Quitar foto`), el contenedor de la foto se oculta limpiamente y el bloque de texto toma el 100% del ancho sin dejar huecos ni deformar el diseño.
      - Se actualizaron las interfaces de TypeScript (`PresetProduct` y `LabelData`), el módulo de exportación PDF (`pdfExport.ts`) y la sincronización en cola/lotes en `App.tsx`.
    - **Corrección de Codificación (Mojibake) y Formato de Dimensiones**: Se erradicó el caracter `Ã—` reemplazándolo por el separador estándar comercial limpio `x` (`290 cm x 20 cm x 2.25 cm`). Se reescribieron los componentes con UTF-8 puro sin BOM, corrigiendo acentos y caracteres chinos.
    - **Visualización Completa del Color Bilingüe**: Se eliminó la clase `truncate` y `uppercase`, incorporando escalado tipográfico inteligente (8pt - 11pt) y soporte multilínea (`break-words line-clamp-2`), permitiendo mostrar nombres bilingües largos (ej. `Madera rojiza‑marrón —— 红棕木色`) de forma completa sin recortes.
    - **Formato Planilla Tamaño Carta (8 etiquetas por hoja - 215.9mm × 279.4mm)**:
      - Geometría adaptada para papel oficina/láser: 2 columnas × 4 filas (exactamente 8 etiquetas de 100mm × 60mm por hoja).
      - Para los 104 productos activos del catálogo, se generan automáticamente **13 hojas tamaño Carta**.
      - Incorpora guías sutiles de corte punteadas (`1.5mm / 1.5mm`) para facilitar el recorte con guillotina o tijeras.
      - Soporte dual: botón **`📄 Descargar PDF Carta (13 Hojas)`** (PDF multipágina Letter en jsPDF) y **`🖨️ Imprimir en Carta`** (vía `@media print` con retícula CSS grid exacta).
      - Vista previa interactiva de la hoja Carta completa directamente en la UI.
    - **Lote Automatizado**: Botón de 1-clic **"⚡ Cargar Catálogo Completo (104 Etiquetas)"** en la cola por lotes (`BatchQueue.tsx`), con exportación tanto en Planilla Carta como en Rollo Térmico continuo. Compilación limpia verificada con `npm run build` y prueba visual en navegador.

- **Categorías en PDV y Catálogo (Selección Predeterminada Lambrín Interior con Libre Manejo)**:
  - **Objetivo**: El Punto de Venta (`PaginaPuntoVenta.tsx`) y el Catálogo (`PaginaCatalogoProductos.tsx`) cargan inicialmente filtrados por la categoría principal `Lambrin Interior 格栅板` (`7938934b-d6cb-44fd-98de-b0645c66017d`), mostrando únicamente sus productos en el primer render para no sobrecargar la vista ni el API.
  - **Manejo Libre de Categorías**: El combo `<select>` carga e incluye todas las categorías de la base de datos más la opción "Todas las Categorías", permitiendo al usuario cambiar y consultar cualquier otra categoría de forma fluida e interactiva.

- **Optimización de Almacenamiento y Entrega de Imágenes de Productos (Desplegado en Producción VPS y Main)**:
  - **Rama Git**: `main` (desplegada y sincronizada con `origin/main` en commit `3551058`).
  - **Estado en VPS (`193.46.198.88`)**:
    - Directorio físico `/var/wpcbajio/data/products` activo con permisos para `www-data`.
    - Servicio `pos-api.service` reiniciado y respondiendo 200 OK (`https://api.wpcbajio.com/api/v1/health`).
    - Compilación Release completada excluyendo build tasks de SixLabors.
    - Endpoint de migración de Base64 ejecutado con éxito.
  - **Objetivo**: Migrar el almacenamiento de imágenes de productos desde cadenas Base64 embebidas en base de datos (`nvarchar(max)`) y payloads JSON masivos hacia archivos físicos WebP organizados por producto fuera de `wwwroot`, servidos mediante el middleware de archivos estáticos de ASP.NET Core con cabeceras de caché HTTP (`Cache-Control: public, max-age=604800, must-revalidate`), aligerar la carga del Punto de Venta desacoplando la consulta inicial (`pageSize: 40`) e implementando búsqueda directa en servidor para el escáner de códigos de barras USB.
  - **Arquitectura de Almacenamiento de Imágenes**:
    - **Servicio de Dominio e Infraestructura**: `IProductImageStorageService` implementado por `LocalProductImageStorageService` utilizando `SixLabors.ImageSharp` (4.1.1).
    - **Variantes WebP Generadas de Forma Atómica**:
      - `thumbnail.webp`: 128x128 px, calidad WebP 75 (para tablas y listas compactas).
      - `pos.webp`: 256x256 px, calidad WebP 80 (para cuadrícula de tarjetas de productos en el Punto de Venta y carrito).
      - `preview.webp`: 512x512 px, calidad WebP 85 (para modal de detalles e inspección visual).
    - **Preservación de Relación de Aspecto y Limpieza**: Redimensionamiento automático manteniendo proporción original, eliminación completa de metadatos EXIF sensibles para reducir tamaño y mitigar riesgos de privacidad.
    - **Aislamiento Fuera de `wwwroot` y Configuración**:
      - Ruta física configurable en `appsettings.json` (`Storage:ProductImagesPath`), por defecto `C:\WPCBajioData\Products` en desarrollo/Windows y `/var/wpcbajio/data/products` en producción Linux/VPS.
      - Prefijo URL público configurable (`Storage:ProductImagesRequestPath`), por defecto `/products`.
      - Mapeo organizado por carpeta de producto: `{productId}/{variant}.webp`.
      - Nombres fijos por variante que reemplazan archivos previos atómicamente evitando proliferación de archivos huérfanos.
      - Validación estricta contra ataques de Directory Traversal (`Path.GetFullPath` validando prefijo base).
    - **Entrega de Archivos Estáticos**:
      - `app.UseStaticFiles()` configurado con `PhysicalFileProvider` apuntando al directorio seguro externo.
      - Encabezados de caché HTTP configurados a 7 días (`max-age=604800, public, must-revalidate`) para optimizar el ancho de banda y carga en cliente.
    - **Endpoints REST**:
      - `POST /api/v1/products/{id}/image`: Recepción multipart (`IFormFile`), validación de tipo MIME (`image/jpeg`, `image/png`, `image/webp`) y límite de 10 MB. Devuelve URLs relativas de las 3 variantes y actualiza `ImagenUrl` en BD.
      - `DELETE /api/v1/products/{id}/image`: Remoción física de archivos y reseteo de `ImagenUrl` a `null`.
      - `POST /api/v1/products/migrate-base64-images`: Migración por lotes de imágenes legadas Base64 a archivos físicos WebP.
    - **Optimización de Consultas SQL y Rendimiento**:
      - Se retiró `.Include(p => p.Imagenes)` en `GetProductsAsync`, erradicando el cuello de botella masivo de memoria que provocaba la serialización de registros de imagen huérfanos o pesados.
      - Compatibilidad hacia atrás: El frontend y backend detectan si la imagen inicia con `/` (ruta estática) o `data:image/` (Base64 heredado), funcionando de manera fluida en cualquier estado de migración.
    - **Carga Ágil en Punto de Venta y Escaneo Resiliente**:
      - El POS carga inicialmente `pageSize: 40` (reducción de más del 90% en la transferencia inicial respecto a los 500 productos anteriores).
      - En el escaneo de código de barras USB (`findAndAddProduct`), si el producto no se encuentra en el caché en memoria de 40 productos, se realiza un fallback instantáneo al endpoint `GET /api/v1/products/code/{code}` para añadirlo al carrito sin interrupciones ni recargar la página.
      - Inclusión de atributos `loading="lazy"` y `decoding="async"` en todas las imágenes de productos.
    - **Modal de Edición/Creación de Producto con Eliminación de Imagen Segura**:
      - Incorporado botón flotante rápido `✕` en la esquina superior derecha de la miniatura de imagen en el modal de catálogo (`PaginaCatalogoProductos.tsx`).
      - Permite revertir una selección de archivo o eliminar la imagen existente de forma limpia y minimalista sin modificar ni reiniciar ninguno de los demás campos del formulario (precios, códigos, dimensiones, categoría, etc.).
    - **Auditoría Limpia**:
      - Eventos `PRODUCT_IMAGE_UPLOADED`, `PRODUCT_IMAGE_REMOVED` y `PRODUCT_IMAGES_MIGRATED` registrados en `AuditLogs` con URLs y metadatos, sin almacenar cadenas Base64.
  - **Pruebas y Verificación**:
    - Backend: 84/84 pruebas xUnit superadas al 100% (`dotnet test src/backend/Pos.sln`), incluyendo 6 pruebas unitarias específicas para `LocalProductImageStorageService` y prueba de integración de subida de imágenes con validación de static files.
    - Frontend: 47/47 pruebas unitarias de Vitest superadas (100%).
    - Build: `tsc && vite build` completado exitosamente en 11.69s con 0 errores y 0 advertencias.

  - **Rama Git**: `mantenimiento/unificar-columna-precios-catalogo`
  - **Objetivo**: Combinar las columnas de "Precio Menudeo / 零售价" y "Precio Mayoreo / 批发价" en una sola columna visual en la tabla del catálogo para ahorrar espacio horizontal en pantalla, manteniendo la exportación a Excel y PDF intacta con ambas columnas separadas.
  - **Cambios Realizados**:
    - `PaginaCatalogoProductos.tsx`:
      - Encabezado `<th>`: Unificado mediante `t('pricesCombined')` (`Precios (Men. / May.) / 价格 (零售 / 批发)`) con ordenamiento interactivo por precio base (`unitPrice`).
      - Celdas `<td>`: Ambas tarifas integradas en una estructura limpia con badges compactos de distinción (`Men` en dorado/ámbar con monto en MXN y `May` en verde con umbral de mayoreo `Min X Pzas`).
      - `colSpan` de empty state ajustado de 10 a 9 columnas.
      - **Exportación intacta**: La configuración `exportConfig` mantiene las columnas independientes `unitPrice` y `wholesalePrice`, garantizando que las descargas de Excel (XLSX) y reportes PDF conserven ambas columnas separadas.
    - `i18n/index.ts`: Añadida la clave bilingüe dual `pricesCombined: 'Precios (Men. / May.) / 价格 (零售 / 批发)'` en los diccionarios español y chino simplificado.
  - **Pruebas y Verificación**:
    - Frontend: 47/47 pruebas unitarias de Vitest superadas (100%).
    - Build: `tsc && vite build` completado exitosamente en 10.98s con 0 errores y 0 advertencias.


- **Optimización de Espacios en Punto de Venta (Ampliación de Catálogo y Compactación de Checkout)**:
  - **Rama Git**: `mantenimiento-ajuste-pdv`
  - **Objetivo**: Reducir el ancho y los espaciados del panel lateral de carrito/checkout (`.pos-checkout`) para permitir que el área de tarjetas de productos (`.pos-card`) se expanda significativamente en pantalla, y rediseñar los elementos del carrito (`.pos-cart-item`) en una estructura de 2 niveles espaciosos para evitar que se amontonen los botones de cantidad, detalles y precios.
  - **Cambios Realizados**:
    - `.pos-layout`: Reconfigurado a `grid-template-columns: minmax(0, 1fr) clamp(310px, 24vw, 355px);` con `align-items: start`. El catálogo de productos ahora ocupa más del 75% del ancho de pantalla.
    - `.pos-products`: Rejilla ajustada a `minmax(235px, 1fr)` con `gap: 0.75rem`, permitiendo una distribución fluida de 4 a 6 columnas en pantallas panorámicas.
    - `.pos-cart-item` (Estructura de 2 Filas Espaciosas):
      - **Fila Superior**: Miniatura del producto + SKU destacado + Nombre completo con el ancho completo disponible + Botón de eliminar `×` a la derecha.
      - **Fila Inferior**: Controles de cantidad `[-] [ 1 ] [+]` a la izquierda, información unitaria y cobertura en el centro, y subtotal en negrita a la derecha.
      - **Corrección de texto redundante**: Eliminada la duplicación como `1 Pzas (1 Pzas)`. Ahora muestra limpiamente `1 Pza`, `2 Pzas`, o el desglose por cajas solo cuando aplica (ej. `14 Pzas (1 Cja + 4 Pzas)`).
    - Checkout compacto: gaps y paddings reducidos en cliente, notas, modalidad de pago, totales y botón procesar.
  - **Pruebas y Verificación**:
    - Frontend: 47/47 pruebas unitarias de Vitest superadas (100%).
    - Build: `tsc && vite build` completado exitosamente en 10.53s con 0 errores y 0 advertencias.
    - Backend: 77/77 pruebas superadas al 100%.


- **Sincronización de Base de Datos Producción (VPS) a Entorno DEV Local y Respaldo Semanal**:
  - **Fecha**: 10 de Septiembre, 2026
  - **Objetivo**: Descargar el respaldo completo de la base de datos de producción (`193.46.198.88`) y restaurarlo en el motor SQL Server local del desarrollador (`PosLambrinDb` y `PosLambrinDb_Dev`), e implementar un cron job de respaldo automatizado semanal en el VPS.
  - **Implementación en VPS**:
    - Script bash de respaldo automatizado: `/usr/local/bin/backup-pos-db.sh` (replicado en repositorio como `scripts/deployment/backup-pos-db.sh`).
    - Almacenamiento seguro en host: `/var/backups/pos-database/` con nombres fechados (`PosLambrinDb_YYYYMMDD_HHMMSS.bak`) y copia fija `PosLambrinDb_latest.bak`.
    - Política de retención: Consolidación y conservación de los últimos 8 respaldos semanales (eliminación automática de los más antiguos).
    - Bitácora de ejecución de respaldos: `/var/log/pos-db-backup.log`.
    - Tarea programada (Cron): `0 3 * * 0 /usr/local/bin/backup-pos-db.sh >> /var/log/pos-db-backup.log 2>&1` (ejecución automática todos los domingos a las 03:00 UTC / 21:00 MX).
  - **Implementación en Local**:
    - Script PowerShell `scripts/development/sync-db-from-vps.ps1` que invoca el respaldo en el VPS, descarga el archivo por SCP y restaura automáticamente `PosLambrinDb` y `PosLambrinDb_Dev`.
    - `.bak` y carpeta `backups/` excluidos en `.gitignore`.
  - **Pruebas y Verificación**:
    - Backend: 77/77 pruebas superadas al 100% (Domain, Application, IntegrationTests).
    - Cron y script ejecutados y verificados con éxito en VPS. Conteos sincronizados: 74 productos, 13 categorías, 74 existencias en stock, 2002 logs de auditoría.


- **Estandarización Bilingüe Universal de Columnas y Alertas Reactivas i18n**:
  - **Rama Git**: `main`
  - **Objetivo**:
    1. **Alertas y Avisos Reactivos Dinámicos**:
       - La alerta de turno de caja cerrado (`noOpenShiftBanner`: *"⚠️ Atención: La caja no ha sido aperturada. Debes realizar la apertura de caja para poder procesar ventas."* / *"⚠️ 注意：钱箱尚未开扎。必须先开扎钱箱才能进行销售。"*) ahora se renderiza dinámicamente mediante `{t('noOpenShiftBanner')}`, conmutando de idioma de inmediato al alternar entre Español y Chino Simplificado sin requerir recargar datos.
       - En `PaginaPuntoVenta.tsx`, el estado `notice` fue refactorizado para admitir claves de traducción reactivas (`key` y `params`), permitiendo que todos los avisos de error y éxito de la venta se traduzcan reactivamente.
    2. **Estandarización de Encabezados de Tabla en Todos los Módulos (`Nombre Español / Nombre Chino`)**:
       - En ambos diccionarios (`es` y `zh`), los encabezados de columnas de todas las tablas se estandarizaron al formato bilingüe dual `"Nombre Español / Nombre Chino"`:
         - **Catálogo de Productos**: `ID / 编号`, `Imagen / 图片`, `SKU / Código / 编码`, `Categoría / 分类`, `Precio Menudeo / 零售价`, `Precio Mayoreo / 批发价`, `Inventario Actual / 当前库存`, `Piezas/Caja / 每箱片数`, `Cobertura (m²) / 覆盖面积`, `Acciones / 操作`.
         - **Categorías**: `Categoría / 分类`, `Clave / 标识符`, `Descripción / 描述`, `Categoría Padre / 父级分类`, `Estado / 状态`, `Acciones / 操作`.
         - **Clientes**: `Cliente / Empresa / 客户 / 公司`, `RFC / 税号`, `Contacto / 联系方式`, `Ubicación / 地址`, `Tipo / 类型`, `Descuento / 折扣`, `Límite Cajas/Día / 每日箱数上限`, `Estado / 状态`, `Acciones / 操作`, y en el historial de compras del modal: `Folio / Fecha / 单号 / 日期`, `Modalidad de Pago / 付款方式`, `Partidas / Cantidad / 明细 / 数量`, `Total / 合计`, `Estado / 状态`.
         - **Inventario**: `Imagen / 图片`, `SKU / Código / 编码`, `Ubicación / 仓库位置`, `Piezas / 件数`, `Mínimo / 最低预警`, `Estado / 状态`.
         - **Movimientos de Inventario**: `Fecha / 日期`, `Producto / 产品`, `Tipo / 类型`, `Cantidad / 数量`, `Costo Actual / 成本单价`, `Precio Venta / 销售单价`, `Monto Total / 总付款`, `Impuesto / 税额`, `Costo Neto / 净成本`, `Ganancia / 利润`, `Evidencia / 实物凭证`, `Motivo / 原因`, `Referencia / 参考`, `Usuario / 操作员`.
         - **Cotizaciones**: `Folio / 单号`, `Cliente / 客户`, `Productos Cotizados / 报价产品`, `Fecha Inicio / 起始日期`, `Vencimiento / 有效期至`, `Total / 合计`, `Estado / 状态`, `Acciones / 操作`, y en el modal de detalle de cotización: `SKU / Código / 编码`, `Producto / 产品`, `Cantidad / 数量`, `Precio / 单价`, `Total / 合计`.
         - **Histórico de Ventas**: `Folio / 单号`, `Fecha / 日期`, `Cliente / 客户`, `Modalidad de Pago / 付款方式`, `Estado / 状态`, `Total / 合计`, `Saldo Pendiente / 余款`, `Acciones / 操作`.
         - **Operaciones Comerciales (Abonos, Transacciones y Devoluciones)**: `Folio / 单号`, `Recibo / Referencia / 收据编号 / 参考号`, `Recibo / 收据编号`, `N° Devolución / 退货单号`, `Fecha / 日期`, `Fecha de Devolución / 退货日期`, `Movimiento / 资金变动`, `Modalidad de Pago / 付款方式`, `Monto Abonado / 实付金额`, `Saldo Pendiente / 余款`, `Cliente / 客户`, `Usuario / 操作员`, `Acciones / 操作`, `Método Reembolso / 退款方式`, `Monto Reembolsado / 退款金额`.
         - **Turno de Caja**: `Folio Turno / 班次单号`, `Usuario / 操作员`, `Apertura / 开班时间`, `Cierre / 关班时间`, `Esperado / 预期金额`, `Real / 实际结扎`, `Diferencia / 差额`, `Estado / 状态`, `Tipo / 类型`, `Motivo / 原因`, `Monto / 金额`, `Categoría / 分类`, `Tipo Movimiento / 变动类型`, `Descripción / 描述`.
         - **Usuarios y Roles**: `Usuario / Empleado / 用户 / 员工`, `Correo Electrónico / 电子邮箱`, `Puesto / 职务`, `Rol en Sistema / 系统角色`, `Estado / 状态`, `Acciones / 操作`.
         - **Bitácora y Auditoría**: `Fecha / 日期`, `Usuario / 操作员`, `Módulo / 模块`, `Actividad / 活动`, `Resultado / 结果`, `Detalles / 详情`.
         - **Reportes Ejecutivos**: `SKU / Código / 编码`, `Categoría / 分类`, `Cantidad Vendida / 销售数量`, `Cantidad Devuelta / 退货数量`, `Cantidad Neta / 净销售数量`, `Venta Neta / 净销售额`, `Piezas / 件数`, `Mínimo / 最低预警`, `Unidades Sugeridas / 建议补货`, `Estado / 状态`.
  - **Pruebas y Verificación**:
    - Frontend Build: `tsc && vite build` completado exitosamente en 11s con 0 errores y 0 advertencias.
    - Frontend Tests: Suite completa Vitest (47/47 pruebas pasando al 100%).

- **Mantenimiento Catálogo de Productos (Fusión a `main` via PR #4)**:
  - **Rama Git**: `main` (commit `33eee52`, fusionado desde PR #4 `mantenimiento/mejoras-catalogo-productos`).
  - **Objetivo**: Mejoras al módulo de productos "catálogo" solicitadas por el cliente:
    1. **Identificador Secuencial `IdProducto` (Identity 1-1)**:
       - Incorporado campo `int IdProducto` en entidades de Dominio (`Producto.cs` y `Product.cs`) y DTOs (`ProductDto.cs`, `Producto.ts`, `Product.ts`).
       - Configurado en `PosDbContext.cs` como `ValueGeneratedOnAdd()`, mapeado en SQL Server como columna `IDENTITY(1,1)` con índice único `IX_Products_IdProducto`. El GUID original `Id` permanece intacto como PK/FK.
       - Simulación de identidad automática añadida en `PosDbContext.cs` para el proveedor `InMemory` en suites de pruebas unitarias.
       - Migración EF Core generada: `20260909231411_AddIdProductoIdentityToProducts`.
       - Búsqueda por texto en API ampliada para coincidencia exacta con `IdProducto` si la consulta es numérica.
       - Columna "ID" agregada con ordenamiento interactivo y formato `#{p.idProducto}` en la tabla del catálogo.
    2. **Columna "Inventario Actual"**:
       - Añadida columna "Inventario Actual" en la tabla mostrando las piezas actuales en existencia (`p.availableQuantity`).
       - Insignias visuales de nivel de existencias (suficiente, bajo stock, agotado).
       - Ordenamiento interactivo por existencias (`stock`) soportado tanto en backend como frontend.
    3. **Baja Lógica de Producto (ABC / CRUD Completo)**:
       - Endpoint `DELETE /api/v1/products/{id}` en `ProductsController.cs` protegido por política `Catalog.ProductsEdit`.
       - Lógica de desactivación `EstaActivo = false`, auditoría `PRODUCT_DELETED` en bitácora central y validación de existencia.
       - Filtro `includeInactive` añadido a `GetProductsAsync` (por defecto `false`), permitiendo conmutar entre Activos, Inactivos y Todos en el catálogo.
       - Botón "🗑️ Eliminar" en frontend con diálogo de confirmación y advertencia bilingüe.
    4. **Refinamiento de UI en Catálogo de Productos**:
       - Botones compactos de iconos (32x32px) para `📄 Ficha Técnica`, `✏️ Editar` y `🗑️ Eliminar`, reduciendo el ancho de la celda de acciones de más de 320px a ~110px.
       - Corrección de duplicación de números en inventario: ahora se muestra exclusivamente la cifra de piezas numéricas en el badge (`{p.availableQuantity}`).
    5. **Reordenamiento y Formato Bilingüe Universal de Columnas**:
       - Columna `ID` (`#{p.idProducto}`) reubicada en primera posición, seguida de la columna de `Imagen` (2ª) y `SKU / Código` (3ª).
       - En la columna `SKU / Código`, el SKU se destaca en tipografía grande monospace (`1.15rem`, negrita 800) y el código de barras debajo (`Cod: ...`).
       - Formato bilingüe dual (`Español / 中文`) aplicado al 100% de los encabezados de tabla.
  - **Pruebas y Verificación**:
    - Backend: 77/77 pruebas superadas al 100% (xUnit: Domain, Application, IntegrationTests).
    - Frontend: 47/47 pruebas unitarias de Vitest superadas (100%).
    - Build Frontend: `npm run build` (`tsc && vite build`) completado con 0 errores y 0 advertencias.

- **Despliegue a Producción (Main y VPS Cloud)**:
  - **Rama Git**: `main` (commit `69c9e7b`, sincronizado con `mantenimiento/mejoras-v2`).
  - **Backend en VPS**: Código actualizado desde `main`, compilado en Release y publicado a `/var/www/pos-api/` con `systemctl restart pos-api` (servicio activo y respondiendo 200 OK en `https://api.wpcbajio.com/api/v1/health`).
  - **Frontend en Cloudflare Pages**: Código integrado a `main` para despliegue automático de CDN Edge en `https://pos.wpcbajio.com`.

- **Resolución de Visibilidad de Paginación en Frontend ("No veo la numeración en el front")**:
  - **Rama Git**: `main` / `mantenimiento/mejoras-v2`
  - **Problema Reportado**: Tras la implementación de la paginación numerada, el usuario no visualizaba los números ni la barra de paginación en el frontend.
  - **Causa Raíz Identificada**:
    1. *Serialización en `System.Text.Json` (`PagedResult<T>`)*: La clase `PagedResult<T>` implementaba `IReadOnlyList<T>` e `IEnumerable`. En ASP.NET Core, `System.Text.Json` trata a cualquier tipo que implemente `IEnumerable` como una colección JSON pura (`[...]`), descartando completamente todas las propiedades de objeto (`totalItems`, `totalPages`, `pageNumber`, `pageSize`).
    2. *Recepción en Frontend*: Al recibir un array JSON plano, los componentes web identificaban `Array.isArray(data)` como `true` o no encontraban `totalItems` (era `undefined`), impidiendo que `pagination.setPaginationFromResult` asignara `totalItems`, por lo que `pagination.totalItems` permanecía en `0`.
    3. *Retorno Temprano en `TablePagination.tsx`*: Con `totalItems === 0`, el componente ejecutaba `if (totalItems === 0) return null;`, lo que hacía que el componente completo se desmontara y fuera invisible en la interfaz.
  - **Solución Implementada**:
    1. *Backend (`PagedResult.cs`)*: Se removió la interfaz `IReadOnlyList<T>` e `IEnumerable` de `PagedResult<T>`, manteniendo las propiedades de conveniencia (`Count`, `this[index]`). De este modo, `System.Text.Json` serializa el DTO como un objeto JSON estándar `{ items, pageNumber, pageSize, totalItems, totalPages, hasPreviousPage, hasNextPage }`.
    2. *Controladores y Pruebas Backend*: Se adaptaron los llamados de filtrado en controladores (`QuotesController`, `PaymentsController`, `ReturnsController`) e integración para acceder a `result.Items`, validando que todas las 73 pruebas xUnit pasen al 100%.
    3. *Frontend Resiliente (`usePagination.ts`)*: Se actualizó `setPaginationFromResult` para tolerar tanto camelCase como PascalCase (`totalItems` / `TotalItems`, `totalPages` / `TotalPages`), calculando `totalPages` como respaldo si fuera omitido.
    4. *Visibilidad Permanente (`TablePagination.tsx`)*: Se eliminó el `return null` cuando `totalItems === 0`. Ahora el componente siempre es visible, mostrando "Sin registros" / "暂无数据", el selector de registros por página y los botones de página numerados `[ 1 ]` deshabilitados, garantizando consistencia visual incondicional.
  - **Verificación**:
    - Backend: 73/73 pruebas unitarias y de integración superadas (100% éxito).
    - Frontend: 47/47 pruebas unitarias de Vitest superadas (100% éxito).
    - Build de Producción Frontend: `npm run build` ejecutado en 10.72s con 0 errores y 0 advertencias.

- **Implementación Universal del Componente de Paginación Numerada Reutilizable (`TablePagination`)**:
  - **Rama Git Activa**: `mantenimiento/mejoras-v2`
  - **Objetivo Cumplido**: Estandarización y despliegue del componente reutilizable `TablePagination` en el 100% de las tablas con paginación server-side del sistema.
  - **Funcionalidad y Características**:
    - Paginación numerada inteligente con elipses adaptativas (`1, 2, ..., N` con ventana de 7 botones centrada).
    - Botones de navegación accesibles: Primera página (`«`), Anterior (`‹`), números activos destacados en paleta corporativa WPC Bajío (`#9C4D22`), Siguiente (`›`) y Última página (`»`).
    - Selector integrado de tamaño de página (25, 50, 100 registros por página).
    - Resumen dinámico bilingüe ("Mostrando X a Y de Z registros" / "显示第 X 至 Y 条，共 Z 条").
    - Manejo seguro de handlers `handlePageChange` y `handlePageSizeChange` con validación de límites y deshabilitación durante carga (`disabled={loading}`).
    - Estilos completamente integrados con los tokens CSS del sistema (`--primary-main`, `--border-subtle`, `--background-surface`, `--text-main`, etc.) y responsive design para móviles (< 640px).
  - **Tablas Integradas**:
    1. 🧾 Histórico de Ventas (`SalesHistoryPage.tsx`)
    2. 💳 Operaciones Comerciales: Transacciones, Abonos y Devoluciones (`CommercialOpsPage.tsx`) — Se corrigió la ausencia del paginador en la vista de transacciones.
    3. 💵 Corte de Turno y Caja: Movimientos de Caja e Historial de Turnos (`CashShiftPage.tsx`)
    4. 📦 Catálogo de Productos (`PaginaCatalogoProductos.tsx`)
    5. 📁 Catálogo de Categorías (`CategoryListPage.tsx`)
    6. 👥 Directorio de Clientes (`CustomerListPage.tsx`)
    7. 🏭 Control de Inventarios (`InventoryListPage.tsx`)
    8. 📋 Movimientos de Inventario (`InventoryMovementsPage.tsx`)
    9. 📑 Cotizaciones (`QuoteListPage.tsx`)
    10. 👤 Gestión de Usuarios (`PaginaUsuarios.tsx`)
    11. 🔍 Bitácora Central de Auditoría (`AuditLogPage.tsx`)
  - **Pruebas y Compilación**:
    - Backend: 72/72 pruebas pasadas (100% xUnit: Domain, Application, IntegrationTests).
    - Frontend: Compilación de producción `npm run build` (`tsc && vite build`) completada con 0 errores.
    - Pruebas Unitarias Frontend: Suite para `getPageNumbers` añadida en `usePagination.test.ts`.

- **Optimización Integral de Paginación y Filtros en Todos los Módulos Paginados (Mejoras v2 - Fase 2)**:
  - **Rama Git Activa**: `mantenimiento/mejoras-v2`
  - **Objetivo Cumplido**: Extensión universal de la arquitectura de alto rendimiento (conteo puro, paginación de dos fases por IDs con `.AsSplitQuery()` y desacoplamiento de filtros con `appliedFilters`) a TODOS los módulos del sistema PDV.
  - **Módulos Optimizados en Backend y Frontend**:
    1. 📑 **Cotizaciones** (`CommercialOperationsService.cs`, `QuoteListPage.tsx`): Paginación en dos fases, `.AsSplitQuery()`, filtros desacoplados con botón de limpiar y caching de opciones.
    2. 💳 **Operaciones Comerciales** (`CommercialOperationsService.cs`, `CommercialOpsPage.tsx`): Desacoplamiento de carga estática de dropdowns respecto a la paginación, filtros de transacciones/abonos independientes, dos fases con `.AsSplitQuery()` en devoluciones y abonos.
    3. 📦 **Catálogo de Productos** (`CatalogApplicationService.cs`, `PaginaCatalogoProductos.tsx`): Paginación de IDs para evitar carga de imágenes en `Skip/Take`, `.AsSplitQuery()` de imágenes, filtros desacoplados y botón de limpiar filtros.
    4. 📁 **Catálogo de Categorías** (`CatalogApplicationService.cs`, `CategoryListPage.tsx`): Conteo limpio sin subcategorías, selección de IDs, hidratación con `.AsSplitQuery()`, eliminación de filtro redundante en cliente.
    5. 👥 **Directorio de Clientes** (`CatalogApplicationService.cs`, `CustomerListPage.tsx`): `AsNoTracking()`, soporte de `includeInactive`, eliminación del temporizador de 250ms por tecla a favor de `appliedFilters` con botón de búsqueda y limpiar.
    6. 🏭 **Control de Inventarios** (`InventoryApplicationService.cs`, `InventoryListPage.tsx`): Conteo desacoplado de productos/categorías, paginación por ID con `.AsSplitQuery()`.
    7. 📋 **Movimientos de Inventario** (`InventoryApplicationService.cs`, `InventoryMovementsPage.tsx`): Paginación de IDs sin join inicial de productos, `.AsSplitQuery()`, filtros desacoplados de fecha y tipo.
    8. 💵 **Corte de Turno y Caja** (`CashShiftApplicationService.cs`, `CashShiftPage.tsx`): Conteo limpio sin transacciones anidadas, selección en 2 fases con `.AsSplitQuery()`.
    9. 👤 **Usuarios y Roles** (`UserApplicationService.cs`, `PaginaUsuarios.tsx`): Conteo sin roles en la fase 1, dos fases con `.AsSplitQuery()`.
    10. 🔍 **Bitácora de Auditoría** (`ReportingApplicationService.cs`, `AuditLogPage.tsx`): Conteo limpio sin usuario, paginación de IDs con `.AsSplitQuery()`, filtros desacoplados con botón de limpiar.
  - **Estado de Pruebas**:
    - Backend: 72/72 pruebas pasadas (100% xUnit: Domain, Application e IntegrationTests), compilación `dotnet build` sin errores ni advertencias.
    - Frontend: Compilación de producción `npm run build` (`tsc && vite build`) completada con éxito (código de salida 0).

- **HotFix de Rendimiento — Corrección de Timeout (15s) en Rango de Fechas de Ventas**:
  - **Rama Git Activa**: `mantenimiento/mejoras-v2`
  - **Problema Reportado**: Al consultar un rango de fechas en el módulo de ventas (ej. `2026-09-01` a `2026-09-09`), el cliente web abortaba con `"Tiempo de espera agotado al conectar con el servidor (15s). Verifica la conexión."` debido a que la petición tardaba más de 27 segundos en responder.
  - **Causa Raíz Diagnosticada**:
    1. *Explosión Cartesiana en EF Core*: `GetSalesAsync` llamaba a `query.CountAsync()` sobre `BuildSaleQuery()` que incluía múltiples colecciones (`Abonos` y `Partidas`). Al paginar sin `.AsSplitQuery()`, EF Core generaba un SQL masivo con 4 subconsultas `LEFT JOIN` hacia tablas hijas, forzando a SQL Server a computar un producto cartesiano que demoraba 25,007 ms (25 segundos) en ejecutarse.
    2. *Sobredisparo en Frontend*: En `SalesHistoryPage.tsx`, el hook `loadSales` dependía directamente de las variables de estado de los inputs (`startDate`, `endDate`, `search`, `status`), disparando múltiples peticiones concurrentes a la API en cada cambio intermedio del selector de fecha.
  - **Solución Implementada**:
    1. *Backend (`SaleApplicationService.cs`)*:
       - Cálculo de `totalItems` con `CountAsync()` sobre la consulta base pura sin `.Include()` (< 5 ms).
       - Paginación en dos etapas de alto rendimiento: primero se seleccionan únicamente los `Id`s paginados con `Skip(skip).Take(take).Select(s => s.Id)` (< 5 ms), y luego se obtienen las entidades completas con `.AsSplitQuery()` (< 40 ms), eliminando el producto cartesiano. El tiempo de base de datos disminuyó de 25,007 ms a ~41 ms (reducción de más del 99.8%).
       - Optimización de `GetSalesSummaryAsync` eliminando inclusiones redundantes.
       - Adición de `.AsSplitQuery()` a `BuildSaleQuery()`, `BuildQuoteQuery()` y `BuildReturnQuery()` para proteger consultas individuales y listados comerciales.
    2. *Frontend (`SalesHistoryPage.tsx`)*:
       - Desacoplamiento de `loadSales` de los inputs crudos: ahora consume el estado `appliedFilters`, activándose únicamente al enviar el formulario (Buscar), al pulsar Limpiar filtros o al navegar de página/ordenar columnas.
  - **Validación y Pruebas**:
    - Backend: 72/72 pruebas pasadas (100% xUnit: Domain, Application e IntegrationTests), 0 errores y 0 advertencias de compilación.
    - Frontend: 42/42 pruebas pasadas (100% Vitest), compilación de producción `tsc && vite build` completada sin errores.
    - Prueba E2E en Navegador: El agente de navegador consultó el rango exacto `01/09/2026` a `09/09/2026` en `http://localhost:5173`, obteniendo las 17 transacciones y métricas en pantalla de forma inmediata sin errores.

- **Mantenimiento y Mejora — Paginación Server-Side Universal en Todo el PDV (Mejoras v2)**:
  - **Rama Git Activa**: `mantenimiento/mejoras-v2`
  - **Objetivo Cumplido**: Implementación universal de paginación server-side (25 por página por defecto, opciones de 25, 50 y 100) en todos los módulos con tablas o listados para evitar consultas masivas a la API y sobrecarga en SQL Server.
  - **Módulos Integrados**:
    1. 🧾 **Histórico de Ventas** (`SalesHistoryPage.tsx`): Paginación server-side con preservación de métricas globales (`getSalesSummary`).
    2. 💵 **Corte de Turno y Caja** (`CashShiftPage.tsx`): Paginación para historial de turnos y movimientos de caja.
    3. 📑 **Cotizaciones** (`QuoteListPage.tsx`): Paginación server-side con filtros de fecha y estado.
    4. 💳 **Operaciones Comerciales** (`CommercialOpsPage.tsx`): Paginación independiente para transacciones, abonos y devoluciones.
    5. 📦 **Catálogo de Productos WPC Bajío** (`PaginaCatalogoProductos.tsx`): Paginación server-side, búsqueda, filtro por categoría y ordenamiento.
    6. 📁 **Catálogo de Categorías WPC Bajío** (`CategoryListPage.tsx`): Paginación server-side con búsqueda y estado.
    7. 👥 **Directorio de Clientes** (`CustomerListPage.tsx`): Paginación server-side con debounce en búsqueda y filtros.
    8. 🏭 **Control de Inventarios WPC Bajío** (`InventoryListPage.tsx`): Paginación server-side, filtro de existencias bajas y escáner de código de barras.
    9. 📋 **Movimientos de Inventario** (`InventoryMovementsPage.tsx`): Paginación server-side por tipo de movimiento y rango de fechas.
    10. 👤 **Gestión de Usuarios y Roles** (`PaginaUsuarios.tsx`): Paginación server-side de usuarios y gestión de roles/permisos.
    11. 🔍 **Bitácora Central de Auditoría** (`AuditLogPage.tsx`): Paginación server-side con filtros de usuario, módulo y resultado.
  - **Exportación Completa**: Función `loadAllPagesForExport` descarga todas las páginas filtradas para PDF y Excel sin truncar los reportes al tamaño de la página activa.
  - **Suite de Pruebas y Compilación**:
    - Frontend: Vitest 42/42 pasadas (100%), Build de producción `tsc && vite build` completado sin errores.
    - Backend: xUnit 72/72 pasadas (100%), Build `dotnet build` con 0 errores y 0 advertencias.

- **Mantenimiento y Mejora — Ordenamiento de Columnas en Tablas (Table Sorting de 3 Estados)**:
  - **Módulos Integrados**: Histórico de Ventas, Histórico de Transacciones, Catálogo de Productos, Catálogo de Categorías, Control de Inventarios y Movimientos de Inventario.
  - **Comportamiento Operativo**: Clic 1 = Ascendente (`↑`), Clic 2 = Descendente (`↓`), Clic 3 = Sin orden / Restablecer (`⇅`).
  - **Infraestructura**: Hook reutilizable `useTableSort.ts`, componente accesible `SortableTh.tsx`.

- **Despliegue Global Cloudflare & VPS Cloud**: **100% OPERATIVO EN PRODUCCIÓN**
  - **Frontend SPA (Cloudflare Workers/Pages CDN)**:
    - **URL Producción**: `https://pos-wpcbajio.aaronarenasmartinez.workers.dev` / `https://pos.wpcbajio.com`
    - **Tecnología**: React 18, Vite 6.4.3, TypeScript, Wrangler Assets con enrutamiento nativo SPA.
    - **Validación**: Inicio de sesión autenticado, emisión y renovación de tokens JWT, navegación fluida sin recargas.
  - **Backend API (.NET 9 Web API en VPS Ubuntu 26.04 - 193.46.198.88)**:
    - **URL Producción**: `https://api.wpcbajio.com/api/v1`
    - **Servicio Systemd**: `pos-api.service` activo en segundo plano (`active (running)`).
    - **Proxy Inverso**: Nginx 1.28 con reenvío de encabezados para Cloudflare y límite de carga de 50MB.
    - **Seguridad**: Firewall UFW activo (puertos 22, 80, 443).
  - **Base de Datos Central Unificada (SQL Server 2022 Express en Docker)**:
    - **Motor**: Contenedor oficial `mcr.microsoft.com/mssql/server:2022-latest` con persistencia en `/var/opt/mssql`.
    - **Base de Datos**: `PosLambrinDb` con 26 tablas físicas autoritativas, roles (`Administrador`, `Cajero`), 27 permisos y semillas de inicio.
    - **Credencial**: `wpcadminaam` conectando internamente a `localhost:1433`.

- **HotFix Integrado — Carga de Imágenes HEIC, Compresión Canvas y Edición de Productos**:
  - **Soporte de Formato HEIC/HEIF de iPhone**: Integración de conversión bajo demanda con `heic2any` (lazy-loading por importación dinámica) que transforma automáticamente archivos `.heic` a `.jpeg` de forma transparente.
  - **Eliminación del Bloqueo Estricto de 2 MB**: Sustituido el límite estricto de 2 MB por compresión y redimensionamiento automático en cliente mediante `HTMLCanvasElement` (máx. 1200px, calidad 82%). Fotos pesadas de 5 MB a 20 MB se optimizan instantáneamente a un Base64 de ~100–250 KB, evitando sobrecargar la memoria y la base de datos.
  - **Edición y Reemplazo de Imagen en Productos Existentes**: En `PaginaCatalogoProductos.tsx`, habilitada la visualización clara de la foto actual al editar, botón para cambiar imagen (soporta cualquier formato/peso) y botón de eliminación ("✕ Quitar foto").
  - **Optimización en Módulo de Inventarios**: En `InventoryListPage.tsx`, se aplicó el mismo motor de compresión y soporte HEIC para la fotografía de evidencia física en movimientos de inventario.
  - **Suite de Pruebas**: Backend xUnit 56/56, Frontend Vitest 27/27 pasadas.

## Iteración Final aprobada — Release PR "version-final-de-PR" (2026-08-28)

- **Rama Git Activa**: `version-final-de-PR` (creada a petición explícita del cliente para entregables finales).
- **Módulo Punto de Venta (PDV)**:
  - Botones de selección rápida renombrados a `Pieza +` (1 pieza) y `Caja +` (1 caja = `piecesPerBox` piezas).
  - Buscador en tiempo real de tarjetas de catálogo arriba de `📦 Catálogo rápido`.
  - Habilitado rol Cajero (`rolCajero`) para registrar clientes nuevos desde caja (`clientes:crear`) con restricción estricta de no edición (`clientes:editar`).
  - Desglose visual de piezas y cajas en el carrito de compras (ej. `23 Pzas (2 Cjas + 3 Pzas) · $290.00 · 10.01 m²`).
  - Corrección del cálculo de Cobertura en carrito y totales para que multiplique por la cobertura individual por pieza (`coveragePerUnitSqM`).
  - Integración de **📐 Calculadora Modal de m² de Lambrín** mediante el botón `📐 Calculadora m²` en la sección de cobro (`pos-checkout`), con ventana modal flotante, desglose de piezas necesarias/cajas equivalentes y adición al carrito en 1 clic.
  - Imágenes de productos en catálogo rápido ampliadas a 92px.
- **Módulo Catálogo de Productos y Movimientos**:
  - Inclusión del campo `Costo Neto / Inicial ($ MXN)` (`unitCost`) en la Sección 3 del modal de Alta/Edición de Productos.
  - Persistencia full-stack de `CostoUnitario` en SQL Server y APIs del backend.
  - Alimentación directa de las columnas de Costo Neto (COGS) y Ganancia en la tabla de Movimientos de Inventario.
  - Cabeceras bilingües en la tabla del catálogo (`Precio Menudeo / 零售价` y `Precio Mayoreo / 批发价`) e insignias visuales de cobertura (`📐 X.XXX m²/pza` y `📦 Caja: X.XX m²`).
- **Módulo Clientes y Permisos de Usuarios**:
  - Permiso `clientes:limite_diario` para configurar un límite máximo diario de cajas vendidas por cliente.
  - Modales de cliente en PDV y Directorio de Clientes adaptados con campo de Límite Diario de Cajas.
  - Validación autoritativa en `SaleApplicationService` que verifica el límite acumulado de ventas del día antes de autorizar la transacción.
  - Modal de **Historial de Compras de Cliente** disponible en la lista de clientes.
- **Módulo de Movimientos de Inventario y Reportería**:
  - Cancelación de Ventas exclusiva para Administradores (`ventas:cancelar`) desde el Historial de Ventas con restitución automática de existencias, bitácora de auditoría y recalculo del esperado en el Corte de Caja.
  - Formato limpio de folios operacionales 1 a 1: Columna Motivo en Movimientos muestra `Venta #X` en lugar de GUID. Folios de turno de caja muestran `CAJA-YYYYMMDD-1` (secuencial incremental diario) en la tarjeta principal (`cash-card`), tabla de historial y bitácora de auditoría.
  - Columnas financieras en Movimientos de Inventario: Costo Actual (`unitCost`), Precio Actual de Venta (`unitPrice`), Monto de Pago (`totalAmount`), Impuesto (`taxAmount`), Costo Neto / COGS (`netCost`) y Ganancia (`profit`).
  - Corrección del fallback de IVA: ventas no facturadas muestran `$0.00` de impuesto.
  - Exportaciones PDF y Excel con 100% de encabezados bilingües en **Español y Chino Simplificado** con registro singleton de fuentes CJK (`Noto Sans SC`) para generación ultrarrápida y sin caracteres garabateados.
- **Suite de Pruebas y Compilación**:
  - Backend (xUnit): **67/67 pruebas autoritativas pasadas al 100%**.
  - Frontend (Vitest & Vite): **24/24 pruebas pasadas al 100%**, build de producción `tsc && vite build` ejecutado exitosamente en `dist/`.
  - Migración EF Core versionada: `20260828020937_AddCustomerDailyLimitAndProductCost`.

## Iteración aprobada — Pie de Comprobante WPC Bajío, Corrección de CI y Sincronización de Puertos (2026-08-18)

- **Diseño e Integración del Pie de Comprobante (Ticket / Recibo)**:
  - Estructuración pulida del pie de comprobante en `SaleReceiptModal.tsx` con la identidad institucional WPC BAJÍO.
  - Inclusión de mensaje de agradecimiento (*¡GRACIAS POR SU COMPRA!*), atención a clientes (Tel/WhatsApp: *477 807 2768*), dirección física (*Blvd. Adolfo López Mateos 2826, El Rosario, C.P. 37125, León de los Aldama, Gto.*) e insignia `PRÓXIMAMENTE` (*www.wpcbajio.com*).
  - Reglas de impresión `@media print` en `PaginaPuntoVenta.css` optimizadas para rollo continuo de 80mm/58mm en impresoras térmicas y PDF sin recortes de altura (`@page { size: auto; margin: 0; }` y `page-break-inside: auto !important`).

- **Estabilización de Pipeline CI (GitHub Actions Linux) y Alineación de Puertos**:
  - Creación de `src/backend/Pos.sln` con rutas relativas multiplataforma (`/`) compatibles con Linux (`ubuntu-latest`).
  - Implementación de `CustomWebApplicationFactory` para pruebas de integración 100% in-memory sin dependencia de servidor SQL local en la nube.
  - Alineación de puertos de desarrollo en `launchSettings.json` y `vite.config.ts` (`http://localhost:60931` / `http://localhost:5000`).
  - Suite de pruebas 100% verde: **67/67 pruebas backend** (xUnit) y **24/24 pruebas frontend** (Vitest).

## Iteración aprobada — Bitácora Central de Auditoría y Base de Datos Limpia de Producción (2026-08-18)

- **Rediseño del Módulo de Auditoría / Bitácora Central del Sistema**:
  - Transformación integral del módulo de auditoría de una vista técnica hacia un **Historial de Actividades Ejecutivas / Auditoría Central** (QUIÉN + QUÉ HIZO + DÓNDE + CUÁNDO + SOBRE QUÉ + RESULTADO).
  - Enriquecimiento JSON estructurado (`schemaVersion: 1`, `module`, `eventType`, `resultStatus`) y desinfección en backend de campos sensibles (`password`, `token`, `secret`, `cvv`) mediante expresiones regulares.
  - Filtro preventivo en `AuditMiddleware`: se omiten registros masivos HTTP `GET 200 OK` en la tabla `AuditLogs`, reservando la tabla de auditoría para eventos de dominio y errores HTTP (`StatusCode >= 400`).
  - Interfaz ejecutiva con íconos por módulo (🛒 Ventas, 📦 Productos, 👥 Clientes, 🏭 Inventario, 💵 Caja, 💰 Pagos, 📑 Cotizaciones, ↩️ Devoluciones, 👤 Usuarios, 🛡️ Roles, 🔒 Seguridad, ⚙️ Sistema), badges de resultado (Correcto, Advertencia, Error) y modal/drawer con acordeón técnico contraído por defecto.
  - Eliminación del botón redundante `📜 Bitácora del Sistema` del navbar en `App.tsx` y desactivación del sink `InMemory` de Serilog.
  - Pruebas unitarias de frontend (`auditMapper.test.ts`) agregadas y 100% aprobadas en Vitest (24/24).

- **Generación de Base de Datos Limpia para Cierre de Fase 1 (`PosLambrinDb`)**:
  - Recreación estandarizada y limpia de la base de datos `PosLambrinDb` en SQL Server `AAM`.
  - Aplicación completa del DDL con 26 tablas, claves primarias, claves foráneas e índices mediante `clean_init.sql`.
  - Registro de las 10 migraciones de EF Core en la tabla `__EFMigrationsHistory`.
  - Población automática autoritativa mediante `DbInitializer`: 2 Roles (`Administrador`, `Cajero`), 27 Permisos, 30 Asignaciones `RolPermiso`, 1 Empleado y Usuario `admin` (`admin@lambrin.com` / `Admin123!`), y 1 Cliente (`Público en General`).
  - Todas las tablas transaccionales y de catálogo (`Sales`, `Products`, `Stocks`, `AuditLogs`, `CashShifts`, `CashTransactions`, `Quotes`, `ReturnHeaders`, `PaymentInstallments`) quedan limpias en 0 registros.

## Iteración aprobada — Exportación administrativa PDF/Excel e Integración a Main (2026-08-17)

- **Ajustes visuales posteriores a validación humana**:
  - Histórico de Ventas separa encabezado/exportación de la fila de filtros y adapta sus controles sin compresión.
  - Histórico de Transacciones elimina el número aislado de registros que resultaba ambiguo junto a las exportaciones.
  - Directorio de Clientes utiliza todo el ancho disponible y distribuye buscador, estado, alta y exportación mediante grid responsive.
  - Alta/edición de Usuarios muestra errores dentro del modal y una lista dinámica de requisitos de contraseña; cada requisito cambia a verde al cumplirse y el guardado permanece deshabilitado mientras falte alguno.
  - Movimientos de Inventario agrupa las fechas bajo “Periodo del movimiento” y muestra etiquetas visibles para Fecha inicial/Fecha final, con disposición responsive.
- **Ramas Git & Publicación en GitHub**:
  - `origin/main`, `origin/fase-1.1` y `origin/codex/exportacion-pdf-excel` integradas en `3fbc641`.
  - Nueva rama activa de trabajo creada desde `main`: `codex/2.1.0-fix-interaccion-bitacora` (publicada en `origin/codex/2.1.0-fix-interaccion-bitacora`).
- **Infraestructura reutilizable**: `ExportButtons`, generador PDF con `@react-pdf/renderer`, generador XLSX con `ExcelJS`, contrato tipado de columnas/filtros y carga paginada común.
- **Cobertura funcional**: ventas, clientes, productos, existencias, movimientos de inventario, cotizaciones, abonos, transacciones/pagos, devoluciones, caja, reportes, usuarios, roles y auditoría.
- **Archivos profesionales**: logo oficial, identidad WPC Bajío, filtros realmente aplicados, tabla multipágina en PDF, valores tipados/autofiltro/freeze en Excel y nombres derivados del periodo activo.
- **Conjunto completo**: los listados paginados se recuperan en lotes de 500; no se limita la exportación a la página visible. Los límites preventivos son 10,000 filas para PDF y 50,000 para Excel, con solicitud explícita de acotar filtros al excederlos.
- **Compatibilidad**: los parámetros API `page` y `pageSize` son opcionales; no se cambiaron rutas, permisos, esquema SQL, PK/FK, GUID técnicos ni la generación de `IdVenta`.
- **Fecha operativa**: los filtros de periodo inician con el día actual de `America/Mexico_City` y los reportes usan los últimos filtros consultados.
- **Aprobación humana y Fusión a Main**: los cambios fueron validados y fusionados directamente a `origin/main` y `origin/fase-1.1`.
- **Validación automatizada final**: backend **67/67**, frontend **21/21**, build Release .NET con **0 advertencias / 0 errores**, build Vite exitoso y auditoría de dependencias de producción con **0 vulnerabilidades**.
- **Documentación técnica**: `docs/ai/EXPORTACION_PDF_EXCEL.md` y borrador de PR `docs/ai/PR_EXPORTACION_PDF_EXCEL.md`.

## Liberación Oficial — Versión 2.0.0 (Fase 1 Comprobada y Validada)

- **Versión**: `2.0.0`
- **Línea Base Git**: `main`, `fase-1.1` y `codex/exportacion-pdf-excel` alineadas en `726faae`.
- **Cálculo Transparente de IVA**: El IVA del 16% en ventas y cotizaciones se calcula sobre el Subtotal base de productos sin ser reducido por descuentos de venta/cliente (`MontoIva = SubTotal * 0.16`). El descuento se aplica directamente al Total a Pagar.
- **Hora de Inserción y Zona Horaria Local**: Las propiedades `DateTime` se expresan en ISO 8601 UTC con sufijo `'Z'` (`UtcDateTimeJsonConverter`) y la aplicación frontend convierte automáticamente la fecha/hora a la hora local del usuario en Guadalajara (−06:00 CT, ej. 02:35 AM), sin alterar registros existentes en la BD.
- **Tabla de Amortización de Abonos e Histórico Transaccional (`commercial-global-history`)**:
  - `GetInstallmentHistoryAsync`, `GetInstallmentsBySaleReferenceAsync` y `GetPaymentTransactionsAsync` incluyen `.Include(item => item.Abonos)` en backend para descontar de forma autoritativa los abonos registrados y calcular el valor exacto del Anticipo Inicial ($6.00).
  - Venta #47 en la tabla de abonos refleja la amortización exacta:
    - **Registro 1 (Anticipo Inicial)**: Monto Abonado **$6.00**, Saldo Pendiente resultante **$400.00** (`$406.00 - $6.00`).
    - **Registro 2 (Abono a Saldo)**: Monto Abonado **$99.00**, Saldo Pendiente resultante **$301.00** (`$400.00 - $99.00`).
- **Regla `<HistoricoAbonosCorrectoComprobante>` por Auditoría**:
  - `SaleReceiptModal` realiza un filtrado por `targetPaymentId` y slicing de arreglo (`sorted.slice(0, targetIdx + 1)`).
  - Al abrir "👁️ Comprobante" en la fila del 1° abono (Anticipo Inicial), el recibo muestra únicamente los **$6.00** del anticipo inicial y saldo pendiente de **$400.00**.
  - Al abrir "👁️ Comprobante" en la fila del 2° abono ($99.00), muestra el desglose del Anticipo ($6.00) + Abono ($99.00) y saldo pendiente de **$301.00**.
  - La tabla dedicada de **Histórico de Transacciones y Movimientos de Pago** propaga ahora el identificador y la fecha del movimiento seleccionado mediante `paymentReceiptArguments`; la Venta #49 a las 02:14 muestra sólo el anticipo de **$42.83** y saldo histórico de **$1,000.01**, mientras que la fila de las 02:42 incluye además el abono posterior de **$10.00** y saldo de **$990.01**.
- **Directorio de Clientes**:
  - Campo de teléfono validado estrictamente a sólo números.
  - Formulario de dirección reordenado solicitando primero el CP y autocompletando Ciudad y Estado mediante `servicioCodigoPostal.ts` (catálogo estandarizado de municipios de Guanajuato y México).
- **Validación actual (2026-08-17)**: backend **67/67** (Domain 19, Application 36, Integration 12); build Release .NET **0 advertencias / 0 errores**; frontend build Vite exitoso; Vitest **21/21**.

## Estado de la aplicación

- **Línea base Git**: rama `fase-1.1`, con la corrección del comprobante histórico integrada sobre la base previa `6c68549`.
- **Módulos operativos independientes en navegación principal**:
  - 🛒 **Punto de Venta (PDV)** (validación estricta de caja aperturada antes de procesar ventas, selector de cliente obligatorio en apartado/anticipo, botones `+`/`-` con incremento entero).
  - 🧾 **Histórico de Ventas** (búsqueda, filtros por fecha/estado/cliente y reimpresión de comprobante).
  - 💵 **Control de Caja** (apertura, sangrías, ingreso de dinero/ajuste de cambio, Corte X, Corte Z, resumen de movimientos generales e íconos representativos).
  - 📈 **Reportes Ejecutivos** (indicadores reales con fechas por defecto del día actual, íconos por método de cobro y tabla detallada de productos con stock bajo).
  - 📑 **Cotizaciones** (omisión de Público General, miniatura de producto e incremento `+`/`-`).
  - 💰 **Abonos y Saldos Pendientes** (histórico global de abonos desglosado con anticipo inicial).
  - 💳 **Transacciones** (tabla dedicada de cobros, abonos e historial transaccional).
  - ↩️ **Devoluciones** (módulo posventa independiente con restitución transaccional de stock).
  - 📄 **Plantillas de Contratos A4** (gestor de plantillas contractuales e impresión).
  - 📦 **Catálogo de Productos** (SKU `WPC-`, mayoreo y existencias en tiempo real).
  - 🏭 **Inventario y Existencias** (stock dinámico actual en captura de movimientos, ubicación fija `Bodega Adolfo Lopez Mateos`).
  - 📋 **Movimientos de Inventario** (historial trazable con evidencia física e imágenes).
  - 👥 **Directorio de Clientes** (estatus activo/inactivo, filtros y checkbox administrado por rol Admin).
  - 🛡️ **Usuarios y Roles** (matriz de 24 permisos).
  - 🔍 **Auditoría / Bitácora** (explorador independiente de eventos auditados).
- **Rol Cajero**: conserva únicamente `ventas:procesar`, `catalogo:productos_ver` y `clientes:ver`; las dos lecturas son dependencias del PDV y no muestran módulos restringidos en el menú.
- **Autorización**: menú y acciones se derivan de permisos vigentes; API y frontend aplican permisos granulares.

## Identidad visual WPC Bajío

- El frontend utiliza un tema claro único basado en la paleta oficial: fondo `#FAF8F5`, superficies blancas, contenedores crema, primario terracota `#9C4D22`, texto café y bordes arena.
- Los tokens de color, estados, sombras, radios y alias de compatibilidad están centralizados en `src/frontend/pos-web/src/index.css`.
- El logo de `public/logo_wpc_bajio.jpeg` aparece en inicio de sesión y encabezado.
- Se optimizaron las reglas `@media print` en `PaginaPuntoVenta.css` para tickets térmicos de 80mm eliminando páginas en blanco mediante `page-break-inside: avoid`.

## Operación comercial

- **Ventas**: validación de caja aperturada requerida antes de completar ventas; anticipo exige cliente real; folios transaccionales únicos. El guardado relacional inserta la venta una sola vez, recupera desde SQL Server el `IdVenta` generado por el GUID de la venta y lo propaga a partidas y movimientos mediante actualizaciones directas dentro de la misma transacción reintentable.
- **Caja**: módulo con métricas ilustradas (💵 Efectivo, 💳 Tarjeta, 🏦 SPEI, 💸 Sangrías), modal de confirmación en sangrías que superan el saldo en caja, y función de ingreso de dinero para ajuste de cambio.
- **Clientes**: estatus visible, filtros de bajas y checkbox administrado exclusivamente por rol Admin.
- **Cotizaciones**: cliente obligatorio (omite público general), cantidades enteras con `+`/`-` y miniatura del producto.
- **Inventario**: stock actual mostrado dinámicamente al seleccionar producto en captura de movimientos; ubicación fija read-only.

## Base de datos

- **Motor**: SQL Server `AAM`, base `PosLambrinDb`, Windows Authentication.
- **Migraciones registradas y físicamente verificadas en la base operativa**:
  - `20260804135557_AddInventoryMovementEvidenceImage`
  - `20260805071520_RestrictCashierToPointOfSale`
  - `20260805085801_AddUniqueOperationalFolios`
  - `20260805092442_CompleteCommercialOperations`
  - `20260805095319_CompleteReportsAndCatalog`
  - `20260805095914_NormalizeDefaultWarehouseLocation`
  - `20260809223936_AddCashShiftTotalEntradas`
  - `20260810111538_AddIdVentaOperationalFolio`
  - `20260810123707_BackfillOperationalSaleReferences`
  - `20260810131157_StandardizeReceiptReferencesByIdVenta`, aplicada por EF Core con 11 recibos y 8 movimientos de caja normalizados.
- `__EFMigrationsHistory` está alineado hasta `20260810131157` en `AAM/PosLambrinDb`. Sigue pendiente una migración inicial completa para construir el esquema desde una base totalmente vacía.

## Validación ejecutada (2026-08-10 - Rama `fase-1.1`)

- **Backend xUnit**: **65/65** aprobadas al 100% (18 dominio, 35 aplicación, 12 integración).
- **Backend build Release**: solución `src/backend/Pos.slnx`, **0 advertencias / 0 errores**.
- **Frontend Vitest**: **10/10** aprobadas al 100%.
- **Frontend producción**: `tsc && vite build`, exitoso sin errores (88 módulos transformados).
- **🆔 Folio Operativo `IdVenta`**: Incorporación aditiva y retrocompatible del identificador consecutivo numérico `IdVenta` (`INT IDENTITY(1,1)` en `Sales` e `INT NULL` en `SaleItems`, `PaymentInstallments`, `ReturnHeaders`, `InventoryMovements`, `CashTransactions`).
  - Mantenidos al 100% los identificadores GUID `Id`, PKs y FKs sin reemplazos ni refactorizaciones masivas.
  - Concurrencia segura gestionada autoritativamente por SQL Server IDENTITY.
  - Generación de nuevo endpoint `GET /api/v1/sales/folio/{idVenta:int}` manteniendo intacto `GET /api/v1/sales/{id:guid}`.
  - Visualización uniforme en historial y comprobante de venta (`Venta #157`).
  - Corregido el doble `SaveChangesAsync(false)` que provocaba reinserciones con GUID duplicados al propagar el folio operativo.
  - Corregida la lectura prematura de `sale.IdVenta`: con `SaveChangesAsync(false)` el valor generado puede permanecer diferido; ahora se consulta en SQL Server dentro de la transacción antes de propagarlo.
  - MARS deshabilitado en la conexión operativa para conservar los savepoints de EF Core.
  - Prueba automatizada confirma que consultar una venta por GUID y por `IdVenta` devuelve la misma operación.

## Validación humana de la corrección transaccional

- La corrección de persistencia fue validada por el desarrollador humano antes de esta iteración: venta creada correctamente, sin reinserciones, sin registros parciales y con `IdVenta` generado por SQL Server.

## Pendientes reales de Fase 1

- Incorporar una **migración inicial/baseline reproducible** para construir `PosLambrinDb` desde una base SQL Server completamente vacía.
- Diseñar e implementar **Promociones** con reglas de acumulación/prioridad.
- Diseñar **Entregas/Envíos** y su flujo de estados.
