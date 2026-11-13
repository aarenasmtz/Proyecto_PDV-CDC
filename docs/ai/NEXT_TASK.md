# NEXT TASK — Siguiente Tarea Recomendada

## 📌 Estado Actual

- **Rama Git**: `feature/facturacion-cfdi-y-notas-de-venta`
- **Pull Request Activo**: [#9 en GitHub](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9)
- **Hito Reciente**: PR publicado formalmente conteniendo:
  - Facturación Electrónica CFDI 4.0 con FiscalAPI (QA).
  - Notas de Venta comerciales en PDF tamaño Carta (QuestPDF).
  - Optimización de rendimiento en PDV (4.56s a 26ms).
  - Apertura instantánea de ticket post-venta (<1s) y stepper segmentado en carrito.
- **Servicios Activos**:
  - Backend API: `http://localhost:5000` (Activo y saludable)
  - Frontend SPA: `http://localhost:5173` (Activo y saludable)
  - Base de Datos: `PosLambrinDb` (Local sincronizado)

## 📌 Siguiente Tarea Recomendada: Revisión y Fusión del PR #9 y Despliegue a Producción (VPS)

1. **Revisión y Aprobación del PR #9**:
   - Validar en GitHub: [https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9](https://github.com/aarenasmtz/Proyecto_PDV-CDC/pull/9).
   - Realizar la fusión (merge) hacia la rama `main`.
2. **Despliegue a Producción (VPS)**:
   - Sincronizar la rama `main` en el servidor VPS de Hostinger.
   - Aplicar la migración de Entity Framework Core `AddFacturacionModuleCompleto` en la base de datos de producción.
   - **Cambio de Credenciales FiscalAPI**: Sustituir las llaves de QA por las llaves de Producción (PR) en `appsettings.Production.json`:
     - `ApiUrl: "https://api.fiscalapi.com"`
     - `ApiKey: <clave_produccion_fiscalapi>`
     - `Tenant: <tenant_produccion_fiscalapi>`
     - `Facturacion.Ambiente: "Produccion"`
   - Compilar y reiniciar los servicios `kestrel-pos` y `pos-web` en producción.

## 📌 Tareas Posteriores en Cola

1. **Commit y Push Autorizado**: Cuando el desarrollador humano lo autorice explícitamente, realizar el commit formal con la identidad autorizada (`Aaron Arenas Martinez` / `44418304+UTLCDA@users.noreply.github.com`).
2. **Despliegue a Producción (VPS)**: Sincronización y actualización de binarios en el servidor.
3. **Flujo de Complementos de Pago**: Para ventas a crédito o pagos diferidos en parcialidades (PPD).
