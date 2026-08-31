# Pharmacy Management System

Sistema de gestión integral para farmacias que incluye punto de venta (POS), control de inventario con alertas de stock y vencimiento, gestión de clientes, proveedores, usuarios con roles, y reportes.

## Características

- **Punto de Venta (POS):** registro de ventas, carrito de compra y emisión de comprobante.
- **Compras:** registro de compras a proveedores, actualiza stock automáticamente.
- **Inventario:** control de stock, categorías de producto y fechas de vencimiento.
- **Alertas de inventario:** detección de stock crítico/bajo y productos vencidos o por vencer, con severidad, centro de notificaciones, historial con trazabilidad (quién y cuándo se reconoció cada alerta) y opción de silenciar una alerta puntual.
- **Clientes:** base de datos de clientes.
- **Proveedores:** gestión de proveedores.
- **Usuarios y roles:** roles con permisos configurables por sección y acción, roles personalizados, y una pantalla de administración de roles (ver [Roles y usuario por defecto](#roles-y-usuario-por-defecto)).
- **Reportes:** ventas, compras, inventario e historial de alertas, con exportación a Excel.
- **Pantalla de inicio:** tablero con ventas del día, alertas abiertas y accesos rápidos.

## Arquitectura

El proyecto sigue una arquitectura MVP (Model-View-Presenter) organizada en capas, cada una en su propio proyecto de la solución:

| Proyecto | Responsabilidad |
|---|---|
| `PharmacySystem.Domain` | Entidades, enums, DTOs y helpers estáticos sin dependencias externas. |
| `PharmacySystem.Data` | Acceso a datos: repositorios (Dapper), fábrica de conexión y logging. |
| `PharmacySystem.Business` | Servicios con la lógica de negocio, sobre las interfaces de `Data`. |
| `PharmacySystem.Presentation` | Presenters, interfaces `IView` y DTOs de presentación — testable sin UI. |
| `PharmacySystem.Wpf` | Ventanas WPF que implementan las interfaces `IView` (pantallas, shell, login, impresión de ticket). |
| `PharmacySystem` | Ejecutable delgado: `Program.cs` (arranque login → shell) y `CompositionRoot.cs` con el cableado manual de dependencias. |
| `PharmacySystem.Tests` | Pruebas unitarias (Presenters/Business con fakes) y de integración (repositorios contra base real). |
| `PharmacySystem.UiTests` | Pruebas "smoke" de helpers de UI (`ViewParse`, `StartupError`, `HomeAccess`). |

Dentro de `Data`, `Business` y `Presentation`, los archivos están organizados en carpetas por responsabilidad (por ejemplo `Data/Repositories/` y `Data/Repositories/Interfaces/`), no por feature.

## Tecnologías

- **Framework:** .NET 10 (`net10.0` / `net10.0-windows`)
- **Lenguaje:** C#
- **UI:** WPF
- **Base de datos:** SQL Server (probado con SQL Server 2019+)
- **Acceso a datos:** Dapper + `Microsoft.Data.SqlClient`
- **Pruebas:** xUnit
- **Exportación:** ClosedXML (Excel), PDFsharp/MigraDoc (PDF)

## Requisitos previos

- **SDK de .NET 10** (`dotnet --version` ≥ 10.0). Visual Studio 2022 17.14+ / VS 2026 opcional para el diseñador de WPF.
- **SQL Server** (local o accesible en red) con permisos para crear la base de datos y sus objetos.
- El SQL Server Client SDK / `sqlcmd`, si se prefiere ejecutar el script de base de datos desde la línea de comandos en vez de SSMS.

La solución compila y corre pruebas con el CLI de `dotnet` (`dotnet build` / `dotnet test`) o desde Visual Studio.

## Instalación

1. Clonar el repositorio:

   ```bash
   git clone https://github.com/IgnacioNorin/pharmacy-management-system.git
   ```

2. Crear la base de datos ejecutando **`Database/PharmacyDB.sql`** contra la instancia de SQL Server (desde SSMS, Azure Data Studio, o `sqlcmd`). El script crea las tablas, índices, procedimientos almacenados, y siembra los datos iniciales: los 4 roles, una cuenta `Administrador General` por defecto (ver más abajo) y las filas de configuración de tienda y de alertas.

   > Si ya hay una base `PharmacyDB` desplegada de una versión anterior, **no** volver a correr este script (contiene `DROP DATABASE`). Aplicar en su lugar los scripts incrementales de **`Database/Migrations/`** en orden — ver `Database/Migrations/README.md`.

3. Restaurar los paquetes:

   ```bash
   dotnet restore PharmacySystem.sln
   ```

4. Configurar la cadena de conexión. El proyecto no usa `App.config` directamente — usa un archivo `ConnectionStrings.config` separado (ignorado por git) en cada uno de estos tres proyectos:

   - `PharmacySystem/ConnectionStrings.config`
   - `PharmacySystem.Tests/ConnectionStrings.config`
   - `PharmacySystem.UiTests/ConnectionStrings.config`

   Cada carpeta tiene un archivo `ConnectionStrings.config.example` de plantilla. Copiar cada uno a `ConnectionStrings.config` (mismo directorio, sin el `.example`) y completar las credenciales correspondientes:

   ```bash
   cp PharmacySystem/ConnectionStrings.config.example PharmacySystem/ConnectionStrings.config
   cp PharmacySystem.Tests/ConnectionStrings.config.example PharmacySystem.Tests/ConnectionStrings.config
   cp PharmacySystem.UiTests/ConnectionStrings.config.example PharmacySystem.UiTests/ConnectionStrings.config
   ```

   ```xml
   <connectionStrings>
     <add name="connection"
          connectionString="Server=#HereYourServer#;database=PharmacyDB;User Id=#HereYourUser#;Password=#HereYourPassword#;TrustServerCertificate=True;"
          providerName="Microsoft.Data.SqlClient"/>
   </connectionStrings>
   ```

   `Microsoft.Data.SqlClient` cifra la conexión por defecto; `TrustServerCertificate=True` alcanza para un SQL Server local con certificado autofirmado (en producción, usar un certificado válido en vez de esa opción).

   `PharmacySystem.Tests` corre pruebas de integración reales contra esta base (limpian sus propias filas al terminar), y `PharmacySystem.UiTests` solo necesita que el archivo exista y esté bien formado — no ejecuta consultas reales.

5. Compilar y ejecutar:

   ```bash
   dotnet run --project PharmacySystem
   ```

## Ejecutar las pruebas

```bash
dotnet build PharmacySystem.sln

# Todas menos las de integración (que necesitan una base con el esquema aplicado):
dotnet test PharmacySystem.sln --no-build --filter "FullyQualifiedName!~PharmacySystem.Tests.Integration"

# Solo las de integración (contra la base configurada):
dotnet test PharmacySystem.Tests --no-build --filter "FullyQualifiedName~PharmacySystem.Tests.Integration"
```

Desde Visual Studio: *Test → Test Explorer → Run All*.

## Roles y usuario por defecto

| Rol | `person_type_id` | Acceso inicial |
|---|---|---|
| Administrador General | 1 | Acceso total, incluida la pestaña Tienda (nombre, datos fiscales y moneda). |
| Administrador | 2 | Todo el sistema, excepto la pestaña Tienda. |
| Empleado | 3 | Solo Clientes, Ventas y Alertas. |
| Cliente | 4 | No puede iniciar sesión en la aplicación (rol de datos únicamente). |

Los permisos de cada rol son **configurables**. Desde la pantalla
**Roles y permisos** (barra lateral → Gestión, requiere el permiso
`roles.gestionar`) se ven como un árbol: cada sección tiene su permiso de
acceso y, debajo, los permisos internos (crear/editar/eliminar, editar la
tienda, configurar alertas, etc.). Tildar un permiso interno marca también el
acceso de su sección; destildar el acceso limpia los de adentro. Ahí también
se crean roles nuevos. Los reportes van más fino: `reportes.acceso` abre la
pantalla y adentro hay un permiso de ver y otro de exportar **por tipo**
(ventas, compras, productos, historial de alertas), así un rol de reposición
puede ver compras y productos sin ver ventas. Los cuatro roles de la
tabla se pueden re-permisar pero no renombrar ni eliminar. La tabla de arriba es
el acceso con el que quedan sembrados; se puede cambiar sin tocar código.

Al eliminar un usuario que ya tiene ventas, compras o alertas reconocidas, se lo
desactiva (`status = 0`) en vez de borrarlo; un usuario desactivado no puede
iniciar sesión. Un rol con usuarios asignados tampoco se puede eliminar.

El script de base de datos siembra una cuenta `Administrador General` por defecto:

```
Documento:   1010101010
Contraseña:  12345678
```

⚠️ **Cambiar esta contraseña antes de usar el sistema en producción.** La contraseña se almacena en texto plano solo hasta el primer inicio de sesión; en ese momento se re-hashea automáticamente (PBKDF2) y queda protegida.

## Despliegue en un cliente

`deploy/package.ps1` compila en Release y arma un paquete de distribución en
`dist/` (binarios + scripts de base + plantilla de configuración). El
procedimiento completo de instalación, actualización, backup y primer arranque
está en **[DEPLOY.md](DEPLOY.md)**.

El historial de cambios por versión está en **[CHANGELOG.md](CHANGELOG.md)**.

## Licencia

Este proyecto está bajo la Licencia MIT — ver el archivo [LICENSE](LICENSE) para más detalles.

## Autor

Ignacio Norín

GitHub: [@IgnacioNorin](https://github.com/IgnacioNorin)
