# Historial de cambios

El formato sigue, a grandes rasgos, [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

## [1.2.0]

### Agregado

- **Permisos por rol.** El acceso deja de estar fijo por rol: cada permiso
  (`seccion.accion`, 30 en total) se puede asignar o quitar de un rol, y se
  pueden crear roles nuevos además de los cuatro de siempre.
  - Nueva pantalla **Roles y permisos** (barra lateral → Gestión, solo la abre
    quien tiene `roles.gestionar`, que de fábrica es únicamente el
    Administrador General): árbol de permisos por rol (cada sección con su
    permiso de acceso y, debajo, sus permisos internos; tildar un permiso
    arrastra a sus padres, destildar uno limpia a sus hijos), alta / renombrar
    / eliminar de roles personalizados. Los cuatro roles del sistema se pueden
    re-permisar pero no renombrar ni eliminar; un rol con usuarios asignados
    no se puede eliminar.
  - La barra lateral y las pestañas de Gestión y de Reportes se muestran según
    los permisos del usuario. Cada acción sensible (crear/editar/eliminar en
    cada sección, editar la tienda, configurar alertas, reconocer/silenciar
    alertas) se valida en el presenter y además deshabilita el botón.
  - Los reportes se controlan **por tipo**: `reportes.acceso` abre la pantalla
    y, adentro, ver y exportar son permisos separados para ventas, compras,
    productos e historial de alertas (`reportes.ventas`,
    `reportes.ventas.exportar`, …). Cada pestaña y cada botón de exportar se
    muestra según su permiso.
  - La pantalla de Inicio oculta las tarjetas y los accesos rápidos de compras
    y de stock a los roles sin esos permisos; los handlers de navegación de la
    ventana principal validan el permiso antes de abrir la sección.
  - El combo de rol en la pantalla de Usuarios se arma desde `person_type`, así
    que muestra también los roles personalizados.
  - **Rol Administrador General protegido.** Un usuario que no es Administrador
    General no puede crear, editar, eliminar ni asignar ese rol, y nadie puede
    eliminar ni degradar al último Administrador General activo (dejaría el
    sistema sin quien administre roles y tienda). Se valida en el presenter de
    Usuarios y en `sp_delete_person` / `sp_update_person`.
  - Migraciones `Database/Migrations/002_user_permissions.sql`,
    `003_role_admin.sql`, `004_report_permissions.sql` y
    `005_protect_admin_general.sql`. Tras aplicarlas el comportamiento es
    idéntico al de la 1.1.0 hasta que un administrador edite un rol.

- **Modelo de IVA (nivel 2, fase A).** La venta ahora guarda su desglose:
  neto + IVA + exento = total. Los precios se siguen ingresando con IVA
  incluido; el neto se calcula hacia atrás para la parte afecta. La tasa vive
  en `store.default_tax_rate` (19 por defecto, pero es configurable, no una
  constante), y cada producto tiene `tax_affected` (afecto / exento). El ticket
  muestra NETO / IVA (tasa%) / EXENTO, y el reporte de ventas suma esas tres
  columnas. En Gestión de productos hay un checkbox "Afecto a IVA" y en Tienda
  un campo "Tasa IVA (%)". Migración `007_tax_model.sql` (rellena las ventas
  existentes asumiendo 100% afecto al 19%).
- **Tipo de documento boleta / factura (nivel 2, fase B).** La pantalla de venta
  ofrece Boleta y Factura (antes solo "Factura", fijo). El correlativo es
  independiente por tipo (`seq_folio_boleta` / `seq_folio_factura`, reemplazan a
  `seq_sale_folio`), y el número es único por `(tipo, número)` — boleta 000001 y
  factura 000001 pueden coexistir. El tipo por defecto se configura en Tienda
  (`store.default_document_type`). Migración `008_document_types.sql` (reinicia
  el correlativo de factura pasado el número más alto ya emitido).
- **Datos del receptor en la factura (nivel 2, fase C).** Al elegir "Factura" en
  la pantalla de venta se despliega un panel que pide RUT, razón social, giro,
  dirección y comuna del receptor. El RUT se valida con dígito verificador
  chileno (módulo 11, aislado en `ChileanRutValidator` — el resto de los
  documentos sigue con el validador neutro). Se guardan en `sale.recipient_*` y
  aparecen en el ticket bajo "RECEPTOR". Migración `009_factura_recipient.sql`.

### Interno

- Se quitó la referencia muerta a Entity Framework 6 del proyecto WinForms
  (no se usaba y hacía fallar el build en un clon sin la carpeta `packages`).
- Se eliminaron seis procedimientos almacenados de alta/edición
  (`sp_create_person` / `_product` / `_supplier`, `sp_update_product` /
  `_supplier` / `_category`): solo hacían un chequeo de duplicado con
  `IF NOT EXISTS` que ya cubren los índices únicos. Los repositorios ahora
  ejecutan el `INSERT` / `UPDATE` directo y mapean el error 2601/2627 a
  "duplicado". Migración `006_drop_redundant_sps.sql`.

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
