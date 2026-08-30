# Migraciones de base de datos

`Database/PharmacyDB.sql` es el esquema completo para una **instalación nueva**
(incluye `DROP DATABASE`). Los scripts de esta carpeta actualizan una base
**ya desplegada** sin recrearla ni perder datos.

## Cómo aplicar

Usar **`PharmacySystem.DbMigrator`** (proyecto de consola en la solución). Corre
las migraciones pendientes en orden, una transacción por script, y anota lo que
aplicó en `dbo.SchemaVersions`. Los scripts van embebidos en el ejecutable.

1. **Backup completo** de `PharmacyDB` antes de empezar.
2. Ejecutar el migrador pasándole la cadena de conexión:

   ```
   PharmacySystem.DbMigrator.exe "Server=<servidor>;Database=PharmacyDB;User Id=<u>;Password=<c>;"
   ```

   La cadena también se puede tomar de la variable de entorno
   `PHARMACY_DB_CONNECTION` o de un `ConnectionStrings.config` junto al `.exe`
   (entrada `connection`). Código de salida `0` = OK, `1` = falló, `2` = sin
   cadena de conexión.

**Bootstrap:** si la base ya existe (tiene `dbo.person`) pero no tiene
`dbo.SchemaVersions` —una instalación previa a esta herramienta, o una base
recién creada con `PharmacyDB.sql`, que ya trae el efecto de todas las
migraciones— el migrador registra los scripts actuales como "ya aplicados" sin
re-ejecutarlos, y de ahí en adelante solo corre los nuevos.

Cada script se sigue escribiendo idempotente (guardas `IF ... IS NULL`,
`CREATE OR ALTER`) por prudencia, pero el migrador ya garantiza que cada uno
corre una sola vez.

### A mano (alternativa)

```
sqlcmd -S <servidor> -U <usuario> -P <clave> -b -i 001_upgrade_to_1_1_0.sql
```

Si se aplican a mano, respetar el orden numérico y correr cada script con
`QUOTED_IDENTIFIER` / `ANSI_NULLS` en `ON` (los que crean procedimientos ya lo
fijan al inicio; `sqlcmd` por defecto los deja en `OFF`).

| Script | Lleva de | a |
|---|---|---|
| `001_upgrade_to_1_1_0.sql` | 1.0 | 1.1.0 |
| `002_user_permissions.sql` | 1.1.0 | 1.2.0 (tablas de permisos + roles) |
| `003_role_admin.sql` | 1.2.0 | 1.2.0 (procedimientos de la pantalla de roles) |
| `004_report_permissions.sql` | 1.2.0 | 1.2.0 (`permission.parent_code` + reportes por tipo: acceso / ver / exportar) |
| `005_protect_admin_general.sql` | 1.2.0 | 1.2.0 (no se puede eliminar ni degradar al último Administrador General) |
| `006_drop_redundant_sps.sql` | 1.2.0 | 1.2.0 (elimina 6 SPs de alta/edición redundantes con los índices únicos) |
| `007_tax_model.sql` | 1.2.0 | 1.2.0 (modelo de IVA: neto/iva/exento en venta, `product.tax_affected`, `store.default_tax_rate`) |
| `008_document_types.sql` | 1.2.0 | 1.2.0 (tipo de documento boleta/factura: correlativo por tipo, `store.default_document_type`) |
| `009_factura_recipient.sql` | 1.2.0 | 1.2.0 (datos fiscales del receptor en la factura: `sale.recipient_*`) |
| `010_credit_note.sql` | 1.2.0 | 1.2.0 (nota de crédito: `sale.reference_id`, `seq_folio_nota_credito`, permiso `ventas.nota_credito`) |
| `011_fiscal_document_hook.sql` | 1.2.0 | 1.2.0 (enganche de emisión fiscal: `sale.fiscal_status` / `fiscal_track_id` / `fiscal_barcode`) |
| `012_client_fiscal_profile.sql` | 1.2.0 | 1.2.0 (ficha fiscal del cliente: `person.business_name` / `activity` / `commune` / `email` / `is_company`; vínculo `sale.client_id`) |
| `013_person_update_fiscal_profile.sql` | 1.2.0 | 1.2.0 (`sp_update_person` escribe también los campos fiscales del cliente) |
| `014_sp_delete_person_client_ref.sql` | 1.2.0 | 1.2.0 (`sp_delete_person` cuenta `sale.client_id` como referencia: da de baja lógica en vez de fallar) |
| `015_store_country_preset.sql` | 1.2.0 | 1.2.0 (`store.country_code`: preset de país; backfill a `CL` si la fila tiene la huella chilena) |
| `016_neutral_default_currency.sql` | 1.2.0 | 1.2.0 (default de `store.currency_culture`: `es-EC` → `en-US`) |
| `017_protect_roles_admin.sql` | 1.2.0 | 1.2.0 (no se puede quitar `roles.gestionar` del último rol que lo tiene, ni por `sp_set_role_permissions` ni al borrar el rol; `sp_set_role_permissions` gana `@result BIT OUTPUT`) |
| `018_purchase_detail_expiry.sql` | 1.2.0 | 1.2.0 (`purchase_detail.date_expired`: guarda el vencimiento de cada lote comprado; el maestro `product.date_expired` solo se adelanta, nunca se atrasa, en la compra) |
| `019_price_management.sql` | 1.2.0 | 1.2.0 (`product.is_released`: estado de comercialización, backfill `= 1` si `sale_price > 0`; tabla `product_price_history` con costo, usuario y motivo por cada cambio de precio) |
| `020_purchase_invoice_unique.sql` | 1.2.0 | 1.2.0 (índice único `UX_purchase_supplier_document` sobre `purchase(supplier_id, document_type, document_number)`: no se puede registrar dos veces la misma factura de un proveedor) |
| `021_weighted_average_cost.sql` | 1.2.0 | 1.2.0 (`product.average_cost`: costo promedio ponderado recalculado en cada compra, backfill desde `purchase_price`; `sale_detail.unit_cost`: costo del producto congelado por línea al vender) |
| `022_soft_delete_consistency.sql` | 1.2.0 | 1.2.0 (`sp_delete_supplier`: baja lógica del proveedor referenciado por compras, igual que productos/personas/categorías; backfill `status = 1` en `person`/`supplier`) |
| `023_sale_payment_method.sql` | 1.2.0 | 1.2.0 (`sale.payment_method`: forma de cobro de la venta — Efectivo/Tarjeta/Transferencia; las filas existentes quedan en `Efectivo`) |
| `024_cash_count.sql` | 1.2.0 | 1.2.0 (arqueo de caja: tablas `cash_count` / `cash_count_line` con esperado vs. contado por forma de pago; permiso `caja.acceso` para los roles Administrador General y Administrador) |
| `025_sale_payment.sql` | 1.2.0 | 1.2.0 (pago mixto: tabla `sale_payment` con una fila por forma de pago de la venta; `sale.payment_method` pasa a ser el método "principal"; backfill de una fila por venta/NC existente) |
| `026_product_lot.sql` | 1.2.0 | 1.2.0 (trazabilidad por lote — DEF-02 fase A: tabla `product_lot` con cantidad, vencimiento y costo por lote; la compra crea un lote por línea, la venta consume FEFO, la NC devuelve un lote sin fecha; `product.stock` = suma de lotes; backfill de un lote por producto con stock) |
| `027_store_wider_text_columns.sql` | 1.2.0 | 1.2.0 (ensancha `store.company_name` a 150, `address` a 200, `email` a 120; una razón social / dirección larga ya no falla al guardar los datos de la tienda) |
| `028_store_drop_currency_country.sql` | 1.2.0 | 1.2.0 (el sistema es CLP puro: elimina `store.currency_culture` y `store.country_code` y su constraint por defecto; la tasa de IVA `store.default_tax_rate` sigue configurable) |

**No es una migración**, pero se ejecuta una vez después de crear la base:
`Database\create_app_login.sql` crea el login `pharmacy_app` con privilegios
mínimos sobre `PharmacyDB` para que la aplicación no se conecte como `sa` (DEF-07).

## Si un paso falla por datos preexistentes

`001` puede fallar en dos puntos, ambos por datos que violan una restricción
nueva. Corregir los datos y volver a correr el script.

### Índices únicos (documento, código, descripción, número de venta)

Detectar duplicados antes de reintentar:

```sql
-- Documentos de persona repetidos
SELECT document_number, COUNT(*) FROM person
WHERE document_number IS NOT NULL
GROUP BY document_number HAVING COUNT(*) > 1;

-- Documentos de proveedor repetidos
SELECT document_number, COUNT(*) FROM supplier
WHERE document_number IS NOT NULL
GROUP BY document_number HAVING COUNT(*) > 1;

-- Códigos de producto repetidos
SELECT code, COUNT(*) FROM product
WHERE code IS NOT NULL
GROUP BY code HAVING COUNT(*) > 1;

-- Descripciones de categoría repetidas
SELECT description, COUNT(*) FROM category
WHERE description IS NOT NULL
GROUP BY description HAVING COUNT(*) > 1;

-- Números de comprobante de venta repetidos (posibles por el esquema viejo)
SELECT document_number, COUNT(*) FROM sale
WHERE document_number IS NOT NULL
GROUP BY document_number HAVING COUNT(*) > 1;
```

Unificar o renumerar las filas en conflicto y volver a correr `001`.

### CHECK `id = 1` en `store` / `notification_settings`

Si `ALTER TABLE ... ADD CONSTRAINT CK_..._singleton` falla, existe una fila con
`id <> 1` en esa tabla. La aplicación siempre usa `id = 1`; consolidar en esa
fila y volver a correr `001`.
