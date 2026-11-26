# PROJECT CONTEXT — WPC Bajío POS, Inventario & E-Commerce

## 1. Visión General del Negocio y Dominio

**WPC BAJÍO Acabados Arquitectónicos** es una empresa mexicana con sede en León, Guanajuato, especializada en la importación, distribución y comercialización de **Lambrín Decorativo** y acabados compuestos de madera y polímero (Wood Plastic Composite - WPC) para diseño interior y exterior.

### Particularidades del Producto y Comercialización:
1. **Unidades de Medida y Venta**:
   - Los productos principales (Lambrín interior, exterior, vigas y paneles UV) se venden por **Pieza (Pza)**, por **Caja** (con factor fijo de piezas por caja, ej. 14 piezas) o calculados por **Metro Cuadrado ($\text{m}^2$)**.
   - Cada modelo cuenta con dimensiones físicas exactas en milímetros y centímetros (Largo, Ancho, Espesor) y una tasa de cobertura por pieza y por caja.
2. **Esquema de Precios Dinámico**:
   - **Precio de Mostrador (Menudeo)**: Precio estándar para clientes de tienda física.
   - **Precio de Mayoreo**: Precio preferencial automático aplicado a clientes mayoristas o cuando la cantidad supera el umbral mínimo configurado (`CantidadMinimaMayoreo`).
   - **Precio de Tienda Online (E-Commerce)**: Precio diferenciado con margen dinámico para cubrir la comisión de pasarela Stripe (ej. 3.6% + $3.00 MXN + IVA), redondeado hacia arriba a múltiplos de $0.50 MXN sin centavos extraños.
3. **Público y Operación Bilingüe (Español / Chino Simplificado)**:
   - El sistema opera 100% bilingüe (Español `es` y Chino Simplificado `zh-CN`) mediante `i18next` en frontend y normalización en backend para la coexistencia de socios e inversionistas de habla china y cajeros/personal operativo local mexicano.
4. **Trazabilidad y Cumplimiento Fiscal**:
   - Trazabilidad del 100% de las acciones mediante bitácora inmutable de auditoría (`AuditLogs`).
   - Timbrado fiscal electrónico CFDI 4.0 mediante **FiscalAPI** (.NET SDK).
   - Generación de Notas de Venta comerciales en PDF tamaño Carta (QuestPDF) para entrega en almacén.
   - Tickets térmicos de 80 mm para mostrador.

---

## 2. Arquitectura de Software y Stack Tecnológico

El sistema sigue los principios de **Clean Architecture Monolítica Modular**, garantizando separación estricta de responsabilidades, alta cohesión y bajo acoplamiento:

```
                        ┌─────────────────────────────────────────┐
                        │   pos-web (SPA React 18 + Vite + TS)    │
                        │   WPC-Ecommerce (Next.js / Storefront)  │
                        └────────────────────┬────────────────────┘
                                             │ HTTP / REST / JSON
                        ┌────────────────────▼────────────────────┐
                        │                Pos.Api                  │
                        │ (Controllers REST, JWT, Swagger, CORS)  │
                        └────────────────────┬────────────────────┘
                                             │ Depende de
                        ┌────────────────────▼────────────────────┐
                        │            Pos.Application              │
                        │ (Use Cases, DTOs, FluentValidation,     │
                        │          Service Interfaces)            │
                        └─────────┬─────────────────────┬─────────┘
                                  │                     │
            Depende de (Dominio)  │                     │ Implementa
                                  ▼                     ▼
                        ┌──────────────────┐  ┌───────────────────┐
                        │    Pos.Domain    │  │Pos.Infrastructure │
                        │(Entities, Enums, │  │(EF Core 9, Dapper,│
                        │ Value Objects)   │  │QuestPDF, FiscalAPI│
                        └──────────────────┘  │Stripe, Serilog)   │
                                              └─────────┬─────────┘
                                                        │
                                              ┌─────────▼─────────┐
                                              │SQL Server 2022 DB │
                                              │  (PosLambrinDb)   │
                                              └───────────────────┘
```

### Tecnologías Principales:
- **Backend**:
  - .NET 9 C# (ASP.NET Core Web API).
  - Entity Framework Core 9 (Code-First con Migraciones Versionadas).
  - Dapper (para consultas de alta velocidad en reportes y bitácora).
  - ASP.NET Core Identity con JWT Bearer Tokens y Refresh Tokens rotativos.
  - Serilog (logging dual: archivo diario rotativo con retención y consola estructurada).
  - QuestPDF 2024.12 (motor de generación de documentos PDF comerciales y técnicos).
  - FiscalAPI .NET SDK v4.0 (timbrado CFDI 4.0 ante el SAT).
  - Stripe.net SDK (pasarela de pagos online).
  - SixLabors.ImageSharp (procesamiento, compresión y conversión física a WebP).
  - xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing (pruebas unitarias y de integración).
- **Frontend (`pos-web`)**:
  - React 18, TypeScript, Vite 6.
  - Vanilla CSS Design Token System (paleta corporativa WPC Bajío, sin frameworks CSS pesados que rompan la personalización).
  - TanStack Query (gestión de estado del servidor y caché reactivo).
  - i18next / react-i18next (soporte bilingüe español/chino).
  - ExcelJS (generación y exportación de reportes XLSX en cliente).
  - @react-pdf/renderer (previsualización de documentos).
  - Vitest, Playwright (pruebas unitarias y E2E).
- **Base de Datos**:
  - Microsoft SQL Server 2022 (`PosLambrinDb`).
  - Tipos monetarios estrictos `decimal(18,2)` y `decimal(18,4)`. Cero uso de `float` o `double`.
  - Fechas almacenadas siempre en UTC (`DateTime2(7)`), renderizadas en cliente en huso horario local (León / Tiempo del Centro UTC-6).

---

## 3. Topología de Infraestructura y Despliegue

El sistema opera en una arquitectura híbrida de alta disponibilidad:

1. **Servidor Cloud VPS (Backend API & Base de Datos)**:
   - **Proveedor / Host**: VPS Ubuntu 24.04 LTS (`193.46.198.88`).
   - **Servicios en VPS**:
     - `MSSQLSERVER` (SQL Server 2022 Linux en puerto 1433 local, base `PosLambrinDb`).
     - `pos-api.service` (Demonio systemd ejecutando `/var/www/pos-api/Pos.Api.dll` en Kestrel `http://127.0.0.1:5000`).
     - `wpc-ecommerce` (Instancia Next.js gestionada con PM2 para la tienda online).
     - Almacenamiento físico de imágenes WebP en `/var/wpcbajio/data/products/`.
     - Nginx (Proxy inverso con terminación SSL y cabeceras de seguridad).
2. **Cloudflare Edge Network & CDN**:
   - **Frontend PDV (`pos-web`)**: Desplegado en Cloudflare Workers / Pages Assets:
     - **URL Oficial**: `https://pos-wpcbajio.aaronarenasmartinez.workers.dev`
   - **API Pública**: `https://api.wpcbajio.com/api/v1` (enrutada vía Cloudflare DNS / Tunnel hacia el VPS).
   - **Tienda Virtual**: `https://wpcbajio.com`
3. **Ambiente Local de Desarrollo**:
   - Backend API: `http://localhost:5000`
   - Frontend SPA: `http://localhost:5173`
   - Base de Datos: `localhost` (instancia SQL Server local `PosLambrinDb`).

---

## 4. Los 17 Módulos del Sistema en Detalle

### Módulo 1: Autenticación, Seguridad y RBAC (`Auth & Users`)
- Login con credenciales seguras, hashing PBKDF2/Argon2 vía ASP.NET Identity.
- Emisión de JWT Bearer con expiración corta y Refresh Tokens rotativos almacenados en base de datos.
- Permisos granulares (`PermissionCodes`): `ventas:crear`, `ventas:historial`, `ventas:cancelar`, `inventario:ver`, `inventario:ajustar`, `clientes:administrar`, `productos:administrar`, `facturacion:emitir`, `facturacion:cancelar`, `caja:corte`, etc.
- Asignación de roles predeterminados: Administrador, Cajero, Encargado de Almacén.
- Botón de visibilidad de contraseña (ojo interactivo) en formulario de ingreso.

### Módulo 2: Punto de Venta (PDV / POS)
- Vista principal de cobro rápido optimizada para pantallas táctiles y teclado.
- Integración nativa con lectores de código de barras USB (captura automática del evento `Enter`).
- **Calculadora Bidireccional de $\text{m}^2$**: Conversión instantánea de área a piezas y cajas considerando el desperdicio comercial (10% sugerido).
- **Selector de Cliente con Datos Fiscales**: Despliegue de RFC y Código Postal fiscal en la lista desplegable y banner de confirmación. Botón rápido `+ Cliente` con validación SAT.
- **Modalidades de Pago**:
  - Pago total en efectivo (con cálculo interactivo de cambio).
  - Tarjeta de débito/crédito.
  - Transferencia bancaria (con captura opcional de referencia bancaria).
  - Pago Mixto (combinación de efectivo, tarjeta y transferencia cuadrando al centavo).
  - Anticipo / Apartado (pago parcial con saldo pendiente).
- Checkbox `¿Desea facturar? (Aplicar 16% IVA)` que agrega la línea formal del impuesto y asigna el emisor para timbrado posterior.
- Generación instantánea de ticket térmico en 80 mm (`SaleReceiptModal`) sin bloqueos de interfaz ni recargas del DOM.

### Módulo 3: Catálogo de Productos y Categorías
- CRUD completo con categorización jerárquica (Lambrín Interior, Exterior, Vigas, Paneles UV, Accesorios).
- Campos de especificación técnica: SKU normalizado (inmune a variantes de guion `LAM-15`, `LAM‑15`, `LAM 15`), código de barras, nombre, descripción, color, material, largo, ancho, espesor, piezas por caja, $\text{m}^2$ por pieza y caja.
- Parámetros fiscales SAT: Clave de Producto/Servicio SAT (`ClaveProdServSat`), Clave de Unidad SAT (`ClaveUnidadSat`) y Objeto de Impuesto SAT (`ObjetoImpuestoSat`).
- **Almacenamiento Físico de Imágenes en WebP**:
  - Conversión automática en subida y migración de Base64 a archivos WebP multi-resolución en disco (`C:\WPCBajioData\Products\` / `/var/wpcbajio/data/products/`).
  - Versionado automático mediante querystring (`?v=timestamp`) para invalidación forzada de caché en CDN y navegador.
  - Reducción del payload de red de 8.5 MB a ~15 KB por consulta.

### Módulo 4: Control de Inventario y Movimientos
- Control de existencias por producto en tiempo real (`Stocks`).
- Registro auditado de movimientos (`InventoryMovements`):
  - Entradas por compra, Salidas por merma/ajuste, Devoluciones, Ajustes físicos y Ventas.
  - **Trazabilidad de Stock Previo y Stock Resultante**: Toda transacción guarda `previousQuantity` y `newQuantity` para auditoría total.
  - Captura opcional de evidencia fotográfica comprimida en cliente antes de la carga.
  - Modal desacoplado `MovementCaptureModal` con previsualización en vivo del saldo resultante y advertencias visuales de stock negativo.

### Módulo 5: Recibos de Compra a Proveedores
- Registro formal de órdenes de compra a proveedores.
- Captura de proveedor, factura comercial externa, costo unitario por producto, cantidad recibida y precio de venta actualizado.
- Impacto directo e inmediato en el stock disponible con registro de auditoría.
- Tabla con filtros por folio, SKU o producto, y exportación a PDF y Excel.

### Módulo 6: Historial de Ventas y Arqueo Operativo
- Listado general de transacciones con folio operativo consecutivo entero (`IdVenta`) y folio estructurado (`VENTA-YYYYMMDD-XXXX`).
- Filtros dinámicos por rango de fechas, cliente, estado de venta (`Completada`, `Con Anticipo`, `Pendiente de Pago`, `Cancelada`, `Devuelta`), y método de pago.
- Cancelación controlada de ventas: reingreso automático al stock (únicamente si la venta había descontado inventario; ventas pendientes de pago no reingresan stock fantasma).
- Acceso directo a reimpresión de tickets, abono a saldos y emisión de notas de venta.

### Módulo 7: Notas de Venta Comerciales Oficiales (PDF Tamaño Carta)
- Módulo independiente diseñado para el control de entrega física de mercancía en mostrador y almacén.
- Documento formal generado en .NET 9 con QuestPDF en orientación vertical carta:
  - Logotipo oficial WPC Bajío y datos comerciales corporativos.
  - Folio de venta y fecha/hora local.
  - Recuadro en dos columnas: Datos del Cliente (con RFC y teléfono) y Datos de Entrega (modalidad en mostrador o envío a domicilio).
  - Tabla de partidas detallada con cálculo de piezas totales contenidas en cajas de lambrín.
  - Sello de entrega visual: `✔ ENTREGADO` o `⏳ PENDIENTE DE ENTREGA`.
  - Recuadro inferior: **Observaciones Comerciales** sanitizadas y libres de glifos `?`, condiciones de venta de la empresa y leyenda obligatoria de documento sin efectos fiscales.
  - Totales históricos exactos (subtotal, descuento, IVA si aplica, total, pagado y saldo pendiente).
- Endpoints en backend para confirmación o alternancia de entrega (`POST /confirmar-entrega`) con registro en bitácora.

### Módulo 8: Facturación Electrónica CFDI 4.0 (FiscalAPI)
- Integración directa con el PAC FiscalAPI mediante su SDK oficial v4.0.469.
- Estructura relacional de 11 tablas dedicadas (`Facturacion*`):
  - `FacturacionFactura`, `FacturacionFacturaDetalle`, `FacturacionFacturaImpuesto`, `FacturacionFacturaVenta`, `FacturacionConfiguracion`, `FacturacionLog`, `FacturacionDocumento`, `FacturacionCancelacion`, etc.
- **Reglas Fiscales y de Negocio**:
  - Validación de emisor, régimen fiscal (ej. 601 General de Ley Personas Morales), lugar de expedición (C.P. 37125 León, Gto.).
  - Validación de receptor: RFC válido (12 o 13 caracteres), Razón Social oficial en mayúsculas sin régimen societario, Código Postal fiscal del receptor, Régimen Fiscal SAT y Uso de CFDI.
  - Modo Público en General con RFC genérico `XAXX010101000` y régimen `616 Sin obligaciones fiscales`.
  - Prorrateo exacto a nivel centavos de los descuentos comerciales de la venta entre las partidas facturables.
  - Bloqueo estricto de timbrado PUE para ventas con saldo pendiente de pago.
  - Idempotencia determinista: bloqueo de doble timbrado simultáneo.
  - Cancelación de comprobantes ante el SAT con motivos 01 a 04, asignación de UUID sustituto y trazabilidad del acuse XML.
  - Descarga directa de archivos XML y PDF oficiales timbrados.
  - **Bitácora Técnica Exclusiva (`FacturacionLog`)**: Registro de pre-llamada y post-llamada con latencia en milisegundos, enmascaramiento de secretos y tokens, y sin códigos HTTP inventados en caso de corte de red.

### Módulo 9: Turnos y Cortes de Caja (Cash Shifts)
- Apertura de turno con monto de fondo fijo inicial.
- Registro transaccional de ingresos y egresos de caja con motivo auditado.
- Cierre y Arqueo Ciego: el cajero declara el efectivo físico contado sin conocer el monto teórico del sistema; el sistema calcula las diferencias (faltante/sobrante) y emite el comprobante de corte de caja en 80 mm.
- Bloqueo de operaciones de cobro si no existe un turno abierto para el cajero o estación.

### Módulo 10: Directorio y Gestión de Clientes
- Alta, edición y consulta de clientes con separación de personas físicas y morales.
- Almacenamiento de datos fiscales SAT: RFC, Razón Social, Régimen Fiscal, Código Postal y Correo para envío automático de CFDI.
- Soporte de clientes mayoristas con precios preferenciales automáticos.
- Control de saldo acumulado de apartados y anticipos.

### Módulo 11: Cotizaciones Comerciales
- Generación de cotizaciones rápidas con vigencia configurable (ej. 15 días).
- Aplicación de descuentos y cálculo de coberturas en $\text{m}^2$.
- Conversión a venta en 1 solo clic desde el Punto de Venta importando partidas, cliente y precios pactados.
- Exportación formal a PDF para envío por WhatsApp o correo.

### Módulo 12: Devoluciones y Operaciones Comerciales
- Registro de devoluciones totales o parciales sobre ventas liquidadas.
- Reingreso selectivo de mercancía al stock disponible.
- Generación de nota de crédito o comprobante de devolución.
- Protección estricta: prohibida la cancelación o alteración sobre ventas con devoluciones registradas.

### Módulo 13: Plataforma E-Commerce (Fase 2)
- Tienda pública online en Next.js conectada a los endpoints de `StoreController` y `StripePaymentsController`.
- Carrito de compras en línea, cálculo automático de gastos de envío / recolección en sucursal.
- Inicialización segura de Stripe PaymentIntent (`POST /api/v1/stripe/create-payment-intent`) con Stripe Elements.
- Webhook de Stripe idempotente (`POST /api/v1/stripe/webhook`) que valida la firma criptográfica (`Stripe-Signature`), acredita la venta, descuenta stock y asigna folio web `WEB-{IdVenta}`.
- Modo de mantenimiento activable/desactivable de forma remota sin requerir nuevo despliegue.

### Módulo 14: Rastreo Público de Pedidos (Zero Trust)
- Consulta pública del estado de pedidos web en la tienda online.
- **Blindaje de Seguridad Zero Trust**: Para prevenir la enumeración de folios o la extracción de datos personales, el sistema exige ingresar obligatoriamente el Folio de la Orden y el Correo Electrónico del comprador registrado.

### Módulo 15: Bitácora de Auditoría Inmutable (Audit Trail)
- Sistema integral de auditoría que registra el 100% de las mutaciones en el sistema:
  - IdCorrelacion (GUID para traza de petición HTTP completa).
  - UsuarioId, Dirección IP, Entidad, EntidadId, Acción (`CREAR`, `MODIFICAR`, `CANCELAR`, `TIMBRAR`, etc.).
  - Valores anteriores (`ValoresAnterioresJson`) y valores nuevos (`ValoresNuevosJson`).
  - Motivo de la acción y resultado (`Success` / `Failure`).
- Regla inquebrantable de la base de datos: registros inmutables (sin `UPDATE` ni `DELETE`).

### Módulo 16: Reportes, Indicadores y Exportación
- Dashboard de métricas: ventas del día, ventas por período, margen bruto, productos más vendidos, stock bajo.
- Motor de exportación universal:
  - Exportación a Excel (`.xlsx`) mediante `ExcelJS` con celdas tipadas, filtros, cabeceras congeladas y formato de moneda.
  - Exportación a PDF mediante QuestPDF (backend) y `@react-pdf/renderer` (frontend).

### Módulo 17: Generador de Etiquetas de Código de Barras
- Impresión de etiquetas para exhibición y control de almacén:
  - Formato multi-etiqueta en hoja tamaño Carta (8 etiquetas por página con guías de corte).
  - Formato continuo para impresoras térmicas de etiquetas (50x30 mm o similar).
- Incluye: Logotipo, nombre comercial, SKU, código de barras Code128, precio por pieza, precio por $\text{m}^2$, y fondo decorativo de mármol blanco o montañas doradas.

---

## 5. Reglas de Negocio Fundamentales

1. **Moneda y Valores Monetarios**:
   - Todo monto financiero se almacena y calcula estrictamente en `decimal(18,2)` o `decimal(18,4)`.
   - Está prohibido el uso de `float` o `double` para dinero.
2. **Fechas y Husos Horarios**:
   - En base de datos y comunicaciones API, las fechas se transmiten y guardan siempre en **UTC** (`DateTime.UtcNow`).
   - La interfaz de usuario las transforma a la hora local de México (`America/Mexico_City`, UTC-6 / CST).
3. **Cálculo de IVA (16%)**:
   - Subtotal = Suma de importes antes de impuesto menos descuentos.
   - IVA = Subtotal base imponible $\times 0.16$.
   - Total = Subtotal + IVA.
4. **Ventas a Crédito / Apartados vs Facturación**:
   - La ley fiscal mexicana (SAT) exige que para emitir un CFDI en modalidad Pago en una Sola Exhibición (PUE), la venta debe estar 100% liquidada al momento de la emisión.
   - El sistema bloquea explícitamente el timbrado PUE de cualquier venta con `SaldoPendiente > 0`.
5. **Normalización Lingüística**:
   - Los registros permanentes de base de datos (como notas de venta, conceptos de partidas y folios) deben quedar en formato estándar en español para validez legal y contable en México, incluso si el operador físico opera el software en Chino Simplificado.

---

## 6. Evolución Histórica de Versiones y Commits

| Versión | Fecha | Hito Principal / Cambios |
| :--- | :--- | :--- |
| **v1.0.0** | 2026-08-04 | Release Candidate Fase 1: Módulos ABC base, ASP.NET Core Web API, EF Core y Serilog dual. |
| **v1.1.0** | 2026-08-04 | Catálogo de productos: badges de cobertura $\text{m}^2$, alineación de formularios y validaciones. |
| **v1.2.0** | 2026-08-05 | Control de inventario, trazabilidad de existencias y captura de evidencia fotográfica. |
| **v1.3.0** | 2026-08-06 | Finalización oficial de Fase 1 e incorporación del folio consecutivo operativo `IdVenta` (INT IDENTITY). |
| **v2.0.0** | 2026-08-08 | IVA 16% transparente, horario local UTC-6, amortización de abonos y comprobantes históricos. |
| **v2.1.0** | 2026-08-11 | Sistema de bitácora de auditoría inmutable (`AuditLogs`), middleware de rastreo y visor en frontend. |
| **v2.2.0** | 2026-08-14 | Script de instalación automática en IIS local (`install_local_iis.py`) y hosting bundle .NET 9. |
| **v2.3.0** | 2026-08-25 | Cancelación controlada de ventas, calculadora de $\text{m}^2$ y soporte inicial de fuentes CJK. |
| **v2.4.0** | 2026-08-28 | Estandarización de SKUs, fichas técnicas en PDF y configuración de túneles Cloudflare. |
| **v2.5.0** | 2026-09-02 | Migración cloud a VPS Ubuntu (`193.46.198.88`), SQL Server Linux y Nginx. |
| **v2.6.0** | 2026-09-05 | Generador de etiquetas con código de barras en tamaño Carta 8x y rollos térmicos. |
| **v2.7.0** | 2026-09-11 | Inicio de Fase 2 (E-Commerce): `StoreController`, carrito web y pasarela Stripe. |
| **v2.8.0** | 2026-09-14 | Esquema de precios diferenciado PDV vs Tienda Online con margen dinámico y redondeo a $0.50. |
| **v2.9.0** | 2026-09-17 | Almacenamiento físico de imágenes en WebP multi-tamaño en disco (`C:\WPCBajioData\Products\`). |
| **v2.10.0** | 2026-09-18 | Módulo de Recibos de Compra a Proveedores, mejoras al PDV e internacionalización bilingüe. |
| **v2.11.0** | 2026-09-24 | Traducción completa al Chino Simplificado (`zh-CN`) en todos los módulos de frontend. |
| **v2.12.0** | 2026-09-24 | Trazabilidad de Stock Previo y Stock Resultante en movimientos y ajustes de inventario. |
| **v2.13.0** | 2026-10-04 | Reactivación de pasarela Stripe en producción, montos mínimos y folios web `WEB-{IdVenta}`. |
| **v2.14.0** | 2026-10-07 | Corrección de política CORS para Cloudflare Workers en producción (`pos-wpcbajio`). |
| **v2.15.0** | 2026-11-08 | Módulo de Facturación CFDI 4.0 con FiscalAPI .NET SDK v4.0, emisión, cancelación y XML/PDF. |
| **v2.16.0** | 2026-11-08 | Bitácora técnica exclusiva de llamadas FiscalAPI (`FacturacionLog`), latencias y sanitización. |
| **v2.17.0** | 2026-11-08 | Selector de cliente con datos fiscales en caja, prorrateo de descuentos e IVA sincronizado. |
| **v2.18.0** | 2026-11-08 | Módulo independiente de Notas de Venta comerciales en PDF tamaño Carta (QuestPDF). |
| **v2.19.0** | 2026-11-09 | Optimización de rendimiento PDV: ticket post-venta inmediato, conexión SQL ampliada y carga de catálogo en 26 ms. |
| **v2.20.0** | 2026-11-19 | Corrección de glifos `?` en Observaciones de Notas de Venta PDF, normalización bilingüe y despliegue a producción. |

---

## 7. Glosario Trilingüe Técnico y Comercial

| Término en Español | Término Técnico en Inglés | Chino Simplificado (中文) | Descripción Funcional |
| :--- | :--- | :--- | :--- |
| **Lambrín** | Wall Panel / Lambrin | 装饰墙板 | Producto insignia: panel de recubrimiento arquitectónico en WPC |
| **Punto de Venta (PDV)** | Point of Sale (POS) | 销售点 / POS系统 | Módulo de caja y cobro de mostrador |
| **Caja / Turno** | Cash Drawer / Cash Shift | 收银班次 | Período de operación de cobro de un cajero |
| **Corte de Caja** | Shift Cut / Cash Count | 轧帐 / 交班结帐 | Conciliación y arqueo de efectivo al terminar turno |
| **Anticipo** | Advance Payment | 预付款 | Pago parcial inicial para apartar producto |
| **Apartado** | Layaway / Reserve | 预订 / 留货 | Reserva física de mercancía con pago parcial |
| **Abono** | Payment Installment | 分期付款 / 缴款 | Pago periódico para liquidar saldo pendiente |
| **Nota de Venta** | Sales Receipt / Commercial Note | 销售单据 | Comprobante comercial de almacén sin efectos fiscales |
| **Factura CFDI 4.0** | Electronic Tax Invoice | 电子发票 (CFDI) | Comprobante fiscal digital oficial ante el SAT mexicano |
| **Bitácora** | Audit Log / Audit Trail | 审计日志 / 操作日志 | Historial inmutable y fechado de operaciones del sistema |
| **Cotización** | Quote / Price Quotation | 报价单 | Propuesta económica sin compromiso transaccional |
| **Devolución** | Return / Refund | 退货 / 退款 | Devolución de producto al stock y ajuste de cobro |
| **Cliente Mayorista** | Wholesale Customer | 批发客户 | Cliente con tarifas de descuento por volumen |
| **Recibo de Compra** | Purchase Receipt | 采购进货单 | Documento de ingreso de stock desde proveedor |
| **Cobertura $\text{m}^2$** | Area Coverage ($\text{m}^2$) | 覆盖面积 (平方米) | Área de pared cubierta por pieza o caja |
| **Venta de Mostrador** | Over-the-counter Sale | 柜台销售 | Venta directa realizada en tienda física |

---

## 8. Guía de Ejecución y Desarrollo Local en Laptop

Al clonar o abrir el proyecto en una nueva laptop o estación de trabajo:

### Requisitos Previos:
- .NET 9.0 SDK (`dotnet --version` $\ge 9.0.100$).
- Node.js 20+ y npm 10+ (`node -v`, `npm -v`).
- SQL Server 2022 (o SQL Server Express / LocalDB).

### Pasos de Arranque:
1. **Clonar Repositorio**:
   ```bash
   git clone https://github.com/aarenasmtz/Proyecto_PDV-CDC.git
   cd Proyecto_PDV-CDC
   ```
2. **Restaurar y Compilar Backend**:
   ```bash
   dotnet build src/backend/Pos.sln
   ```
3. **Instalar Dependencias de Frontend**:
   ```bash
   npm --prefix src/frontend/pos-web install
   ```
4. **Ejecutar Pruebas Automatizadas**:
   ```bash
   dotnet test src/backend/Pos.sln
   npm --prefix src/frontend/pos-web run test
   ```
5. **Iniciar Servicios en Desarrollo**:
   - Backend API:
     ```bash
     dotnet run --project src/backend/Pos.Api
     # Escuchando en http://localhost:5000 (Swagger: http://localhost:5000/swagger)
     ```
   - Frontend SPA:
     ```bash
     npm --prefix src/frontend/pos-web run dev
     # Escuchando en http://localhost:5173
     ```
6. **Credenciales Predeterminadas de Administrador**:
   - **Usuario**: `admin`
   - **Contraseña**: `Admin123!`
