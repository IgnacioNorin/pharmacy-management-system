# Migraciones de base de datos

`Database/PharmacyDB.sql` es el esquema completo para una **instalación nueva**
(incluye `DROP DATABASE`). Los scripts de esta carpeta actualizan una base
**ya desplegada** sin recrearla ni perder datos.

## Cómo aplicar

1. **Backup completo** de `PharmacyDB` antes de empezar.
2. Correr los scripts en orden numérico, una sola vez cada uno, con SSMS,
   Azure Data Studio o `sqlcmd`:

   ```
   sqlcmd -S <servidor> -U <usuario> -P <clave> -b -i 001_upgrade_to_1_1_0.sql
   ```

3. Cada script es idempotente: volver a correrlo no hace nada y no da error.

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
