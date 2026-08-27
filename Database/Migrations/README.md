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
