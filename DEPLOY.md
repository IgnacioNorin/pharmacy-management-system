# Guía de despliegue

## Requisitos del equipo cliente

- Windows 10 / 11 (x64).
- **.NET Framework 4.8** (viene con Windows 10 1903+ y Windows 11).
- Acceso de red a una instancia de **SQL Server 2019 o superior** (puede ser
  local en el mismo equipo).

## 1. Armar el paquete (en el equipo de desarrollo)

```powershell
# Desde la raíz del repositorio, con Visual Studio 2022 instalado
powershell -ExecutionPolicy Bypass -File deploy\package.ps1
```

Genera `dist\PharmacySystem-<versión>\` y `dist\PharmacySystem-<versión>.zip`
con:

```
PharmacySystem.exe            aplicación
*.dll                         dependencias
PharmacySystem.exe.config     configuración (sin credenciales)
ConnectionStrings.config.example  plantilla de cadena de conexión
Database\PharmacyDB.sql       esquema para instalación nueva
Database\Migrations\          scripts de actualización
CHANGELOG.md
LEEME.txt                     estos pasos, resumidos
```

## 2. Base de datos

### Instalación nueva

Ejecutar `Database\PharmacyDB.sql` contra la instancia de SQL Server (SSMS,
Azure Data Studio o `sqlcmd`). Crea la base, los objetos y las filas iniciales:
los 4 roles, la cuenta `Administrador General` por defecto, y las filas de
configuración de tienda y de alertas.

### Actualización de una base existente

No volver a correr `PharmacyDB.sql` (tiene `DROP DATABASE`). Aplicar los scripts
de `Database\Migrations\` en orden — ver `Database\Migrations\README.md`.
**Hacer backup completo antes.**

### Usuario de la aplicación (privilegios mínimos)

Después de crear la base, ejecutar **una vez** `Database\create_app_login.sql`
como administrador de la instancia. Crea el login `pharmacy_app` con permisos
solo sobre `PharmacyDB` (leer, escribir, ejecutar procedimientos y avanzar los
correlativos) — **la aplicación nunca debe conectarse como `sa`**. Cambiar la
contraseña del script antes de correrlo.

El **migrador** (`PharmacySystem.DbMigrator`) sí necesita permisos de esquema
(crea tablas, índices y procedimientos): usar una cuenta `db_owner` sobre
`PharmacyDB` (o `sa`) solo en el momento del despliegue, no en la configuración
de la aplicación.

## 3. Configuración de la aplicación

1. Copiar la carpeta del paquete al equipo cliente (por ejemplo
   `C:\PharmacySystem\`).
2. Copiar `ConnectionStrings.config.example` a `ConnectionStrings.config` en la
   misma carpeta que `PharmacySystem.exe` y completar servidor, base y
   credenciales:

   ```xml
   <connectionStrings>
     <add name="connection"
          connectionString="Server=SERVIDOR;database=PharmacyDB;User Id=pharmacy_app;Password=CLAVE;"
          providerName="System.Data.SqlClient"/>
   </connectionStrings>
   ```

   Usar el login `pharmacy_app` creado con `Database\create_app_login.sql`
   (permisos solo sobre `PharmacyDB`). **Nunca `sa`.**

## 4. Primer arranque

1. Ejecutar `PharmacySystem.exe`.
2. Iniciar sesión con la cuenta por defecto:

   ```
   Documento:   1010101010
   Contraseña:  12345678
   ```

3. **Cambiar de inmediato la contraseña de esa cuenta** (queda en texto plano
   en la base hasta el primer inicio de sesión; ahí se re-hashea con PBKDF2).
   Crear las cuentas reales de los usuarios y, si esa cuenta por defecto no se
   va a usar, darla de baja desde la pantalla de Usuarios.
4. Cargar los datos de la tienda (nombre, datos fiscales, moneda) en
   Gestión → Tienda.
5. Ajustar los umbrales de alerta en el centro de notificaciones.

## 5. Operación

### Backup

Programar un backup de `PharmacyDB` con la periodicidad que exija el negocio
(diario como mínimo). Ejemplo de backup completo:

```sql
BACKUP DATABASE [PharmacyDB]
TO DISK = N'C:\Backups\PharmacyDB_full.bak'
WITH INIT, COMPRESSION, CHECKSUM;
```

Restaurar en una instancia de prueba periódicamente para verificar el backup.

### Registro de errores

La aplicación escribe `error.log` junto al ejecutable. Rota solo al superar
5 MB (guarda `error.log.1`). Revisarlo ante cualquier "no se pudo ..." que
reporte un usuario.

### Actualización a una versión nueva

1. Backup de la base.
2. Reemplazar los archivos de la aplicación por los del paquete nuevo,
   conservando `ConnectionStrings.config`.
3. Aplicar los scripts de `Database\Migrations\` que falten, en orden.
