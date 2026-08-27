# Historial de cambios

El formato sigue, a grandes rasgos, [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

## [1.2.0]

### Agregado

- **Permisos por rol.** El acceso deja de estar fijo por rol: cada permiso
  (`seccion.accion`, 23 en total) se puede asignar o quitar de un rol, y se
  pueden crear roles nuevos además de los cuatro de siempre.
  - Nueva pantalla **Roles y permisos** (barra lateral → Gestión, solo la abre
    quien tiene `roles.gestionar`, que de fábrica es únicamente el
    Administrador General): checklist de permisos por rol, alta / renombrar /
    eliminar de roles personalizados. Los cuatro roles del sistema se pueden
    re-permisar pero no renombrar ni eliminar; un rol con usuarios asignados
    no se puede eliminar.
  - La barra lateral y las pestañas de Gestión se muestran según los permisos
    del usuario. Cada acción sensible (crear/editar/eliminar en cada sección,
    editar la tienda, configurar alertas, exportar reportes, reconocer/silenciar
    alertas) se valida en el presenter y además deshabilita el botón.
  - El combo de rol en la pantalla de Usuarios se arma desde `person_type`, así
    que muestra también los roles personalizados.
  - Migraciones `Database/Migrations/002_user_permissions.sql` y
    `003_role_admin.sql`. Tras aplicarlas el comportamiento es idéntico al de
    la 1.1.0 hasta que un administrador edite un rol.

## [1.1.0]

### Corregido

- **Venta e inventario:** el descuento de stock se ejecuta dentro de la misma
  transacción que registra la venta y su detalle, con guardia
  `stock >= cantidad` por línea. Si una línea no tiene stock suficiente se
  revierte toda la venta; ya no queda stock descontado sin venta registrada ni
  descuentos parciales sin *rollback*.
- **Número de comprobante de venta:** se genera con una secuencia
  (`dbo.seq_sale_folio`) dentro de la transacción, con índice único. Antes se
  calculaba con `COUNT(*) + 1`, que asignaba el mismo número a ventas
  simultáneas y se repetía tras un borrado.
- **Reporte de compras:** el total de compras dejaba de ser correcto cuando una
  compra tenía más de una línea de detalle (sumaba el monto de la cabecera una
  vez por línea).
- **Instalación nueva:** una base recién creada no podía guardar el perfil de la
  tienda ni los umbrales de alerta. Ahora el esquema siembra ambas filas de
  configuración y los procedimientos y repositorios las crean si faltan.
- **Baja de productos:** un producto que solo había disparado alertas (sin
  compras ni ventas) no se podía eliminar. `sp_delete_product` ahora contempla
  `product_alert_history`.
- **Edición de usuarios:** editar un usuario sin tocar la contraseña ya no la
  reescribe; dejar el campo en blanco conserva la actual.
- **Reportes por fecha:** los filtros de fecha usan `DateTime` tipado y un rango
  *sargable* en lugar de texto sensible a la configuración regional.
- Truncado silencioso de `name`/`description` de producto en
  `sp_create_product` / `sp_update_product` (parámetros alineados al ancho real
  de las columnas).

### Agregado

- Baja lógica de usuarios (`sp_delete_person`): una persona referenciada por
  ventas, compras o alertas reconocidas se desactiva (`status = 0`) en vez de
  fallar el borrado. El inicio de sesión rechaza usuarios con `status = 0`.
- Índices únicos en documento de persona/proveedor, código de producto y
  descripción de categoría; índices en las columnas de clave foránea.
- Rotación de `error.log` al superar 5 MB y escritura segura entre procesos.
- `Database/Migrations/001_upgrade_to_1_1_0.sql` para actualizar una base ya
  desplegada sin recrearla.
- `DEPLOY.md` y `deploy/package.ps1` para armar un paquete de distribución.

### Cambiado

- Todas las columnas de importes pasan de `decimal(10,2)` a `decimal(18,2)`
  para admitir monedas de bajo valor sin desbordar.
- Opciones de base `ANSI_NULLS`, `ANSI_PADDING`, `ANSI_WARNINGS`, `ARITHABORT`,
  `QUOTED_IDENTIFIER` y `CONCAT_NULL_YIELDS_NULL` en `ON`.

## [1.0.0]

- Primera versión: punto de venta, compras, inventario con alertas de stock y
  vencimiento, clientes, proveedores, usuarios con roles y reportes.
