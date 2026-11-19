# NEXT TASK — Siguiente Tarea Recomendada

## 📌 Estado Actual

- **Producción (En Vivo)**:
  - **Backend API**: `https://api.wpcbajio.com/api/v1` (Operativo en VPS Ubuntu `193.46.198.88`).
  - **Frontend SPA**: `https://pos-wpcbajio.aaronarenasmartinez.workers.dev` (Desplegado en Cloudflare Workers).
  - **Base de Datos VPS**: Migrada con `AddFacturacionModuleCompleto` y notas normalizadas en español.
  - **FiscalAPI**: Operando en modo QA (`https://test.fiscalapi.com`).
- **Control de Versiones**:
  - **Rama Activa**: `feature/facturacion-cfdi-y-notas-de-venta`
  - **Pull Request Activo**: [#9 en GitHub](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9) listo para merge a `main`.

## 📌 Siguiente Tarea Recomendada: Validación de Nota de Venta PDF en Producción y Aprobación de PR #9

1. **Validación de la Nota de Venta PDF**:
   - Ingresar a [https://pos-wpcbajio.aaronarenasmartinez.workers.dev](https://pos-wpcbajio.aaronarenasmartinez.workers.dev).
   - Iniciar sesión y entrar al módulo de **Notas de Venta**.
   - Descargar el PDF de la venta folio 22 (o cualquier venta previa) y verificar que en "OBSERVACIONES / CONDICIONES COMERCIALES" se lea claramente `Venta de mostrador. Cobertura estimada: 1.16 m².` sin símbolos `?`.
2. **Revisión y Aprobación del PR #9**:
   - Revisar el Pull Request #9 en GitHub e incorporar este commit adicional a la rama del PR.
3. **Cambio Futuro de Claves FiscalAPI a Producción (PR)**:
   - Cuando se cuente con las credenciales definitivas de producción de FiscalAPI, actualizar en `/var/www/pos-api/appsettings.json` en el VPS las llaves `ApiKey`, `Tenant` y `ApiUrl: "https://api.fiscalapi.com"`.

## 📌 Tareas Posteriores en Cola

1. **Commit y Push Autorizado**: Cuando el desarrollador humano lo autorice explícitamente, realizar el commit formal con la identidad autorizada (`Aaron Arenas Martinez` / `44418304+UTLCDA@users.noreply.github.com`).
2. **Despliegue a Producción (VPS)**: Sincronización y actualización de binarios en el servidor.
3. **Flujo de Complementos de Pago**: Para ventas a crédito o pagos diferidos en parcialidades (PPD).
