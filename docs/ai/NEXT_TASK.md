# NEXT TASK — Siguiente Tarea Recomendada

## 📌 Estado Actual

- **Producción (En Vivo)**:
  - **Backend API**: `https://api.wpcbajio.com/api/v1` (Operativo en VPS Ubuntu `193.46.198.88`).
  - **Frontend SPA**: `https://pos-wpcbajio.aaronarenasmartinez.workers.dev` (Desplegado en Cloudflare Workers).
  - **Base de Datos VPS**: Migrada con `AddFacturacionModuleCompleto` y 99 imágenes en WebP.
  - **FiscalAPI**: Operando en modo QA (`https://test.fiscalapi.com`).
- **Control de Versiones**:
  - **Pull Request Activo**: [#9 en GitHub](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9) en rama `feature/facturacion-cfdi-y-notas-de-venta` listo para merge a `main`.

## 📌 Siguiente Tarea Recomendada: Validación en Producción por el Usuario y Fusión del PR #9

1. **Pruebas en Vivo en Cloudflare**:
   - Ingresar a [https://pos-wpcbajio.aaronarenasmartinez.workers.dev](https://pos-wpcbajio.aaronarenasmartinez.workers.dev).
   - Iniciar sesión como `admin` (`Admin123!`).
   - Validar el nuevo módulo de **Notas de Venta** (generar y descargar una nota de venta en PDF).
   - Validar el nuevo módulo de **Facturación CFDI 4.0** (vista previa y emisión de prueba con FiscalAPI QA).
   - Probar la fluidez del catálogo en el POS y el nuevo diseño del carrito.
2. **Fusión (Merge) del PR #9**:
   - Aprobar y fusionar el PR #9 hacia `main` en GitHub.
3. **Cambio Futuro de Claves FiscalAPI a Producción (PR)**:
   - Cuando se cuente con las credenciales definitivas de producción de FiscalAPI, actualizar en `/var/www/pos-api/appsettings.json` en el VPS las llaves `ApiKey`, `Tenant` y `ApiUrl: "https://api.fiscalapi.com"`.

## 📌 Tareas Posteriores en Cola

1. **Commit y Push Autorizado**: Cuando el desarrollador humano lo autorice explícitamente, realizar el commit formal con la identidad autorizada (`Aaron Arenas Martinez` / `44418304+UTLCDA@users.noreply.github.com`).
2. **Despliegue a Producción (VPS)**: Sincronización y actualización de binarios en el servidor.
3. **Flujo de Complementos de Pago**: Para ventas a crédito o pagos diferidos en parcialidades (PPD).
