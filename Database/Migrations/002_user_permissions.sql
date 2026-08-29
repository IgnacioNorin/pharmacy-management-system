-- =============================================================================
-- Migration 002: 1.1.0 -> 1.2.0  (user permissions by role)
--
-- Adds the permission catalogue, the role -> permission mapping and the
-- person_type.is_system flag. Seeds the four built-in roles with exactly the
-- access they have today, so behaviour does not change until an admin edits a
-- role.
--
-- Run once against an existing PharmacyDB. Idempotent: re-running is a no-op.
-- Take a backup first.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 002 -> 1.2.0 starting ---';
GO

-- -----------------------------------------------------------------------------
-- 1. person_type.is_system
-- -----------------------------------------------------------------------------
IF COL_LENGTH('dbo.person_type', 'is_system') IS NULL
BEGIN
    ALTER TABLE dbo.person_type
        ADD is_system BIT NOT NULL CONSTRAINT DF_person_type_is_system DEFAULT (0);
END
GO
UPDATE dbo.person_type SET is_system = 1 WHERE id IN (1, 2, 3, 4) AND is_system = 0;
GO
PRINT '1. person_type.is_system';
GO

-- -----------------------------------------------------------------------------
-- 2. permission / role_permission tables
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.permission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.permission (
        id          INT IDENTITY(1,1) NOT NULL,
        code        VARCHAR(60)  NOT NULL,
        section     VARCHAR(30)  NOT NULL,
        description VARCHAR(150) NOT NULL,
        CONSTRAINT PK_permission PRIMARY KEY CLUSTERED (id ASC),
        CONSTRAINT UX_permission_code UNIQUE (code)
    );
END
GO

IF OBJECT_ID('dbo.role_permission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.role_permission (
        person_type_id INT NOT NULL,
        permission_id  INT NOT NULL,
        CONSTRAINT PK_role_permission PRIMARY KEY CLUSTERED (person_type_id ASC, permission_id ASC),
        CONSTRAINT FK_role_permission_role FOREIGN KEY (person_type_id) REFERENCES dbo.person_type (id) ON DELETE CASCADE,
        CONSTRAINT FK_role_permission_permission FOREIGN KEY (permission_id) REFERENCES dbo.permission (id) ON DELETE CASCADE
    );
END
GO
PRINT '2. permission / role_permission tables';
GO

-- -----------------------------------------------------------------------------
-- 3. Seed the permission catalogue (only rows that are missing)
-- -----------------------------------------------------------------------------
INSERT INTO dbo.permission (code, section, description)
SELECT v.code, v.section, v.description
FROM (VALUES
    ('ventas.acceso',          'ventas',      'Usar el punto de venta'),
    ('compras.acceso',         'compras',     'Registrar compras a proveedores'),
    ('clientes.acceso',        'clientes',    'Ver la seccion de clientes'),
    ('clientes.gestionar',     'clientes',    'Crear, editar y eliminar clientes'),
    ('proveedores.acceso',     'proveedores', 'Ver la seccion de proveedores'),
    ('proveedores.gestionar',  'proveedores', 'Crear, editar y eliminar proveedores'),
    ('productos.acceso',       'productos',   'Ver la seccion de productos'),
    ('productos.gestionar',    'productos',   'Crear y editar productos'),
    ('productos.editar_precios','productos',  'Modificar precios de compra y de venta'),
    ('productos.eliminar',     'productos',   'Eliminar o dar de baja productos'),
    ('categorias.acceso',      'categorias',  'Ver la seccion de categorias'),
    ('categorias.gestionar',   'categorias',  'Crear, editar y eliminar categorias'),
    ('tienda.acceso',          'tienda',      'Ver los datos de la tienda'),
    ('tienda.editar',          'tienda',      'Modificar nombre, datos fiscales y moneda'),
    ('usuarios.acceso',        'usuarios',    'Ver la seccion de usuarios'),
    ('usuarios.gestionar',     'usuarios',    'Crear, editar y eliminar usuarios'),
    ('roles.gestionar',        'usuarios',    'Administrar roles y sus permisos'),
    ('reportes.acceso',        'reportes',    'Ver reportes'),
    ('reportes.exportar',      'reportes',    'Exportar reportes a Excel'),
    ('alertas.acceso',         'alertas',     'Ver el centro de notificaciones'),
    ('alertas.reconocer',      'alertas',     'Reconocer alertas de inventario'),
    ('alertas.silenciar',      'alertas',     'Silenciar alertas puntuales'),
    ('alertas.configurar',     'alertas',     'Cambiar los umbrales de alerta')
) v(code, section, description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.permission p WHERE p.code = v.code);
GO
PRINT '3. permission catalogue seeded';
GO

-- -----------------------------------------------------------------------------
-- 4. Seed role_permission for the built-in roles.
--    Uses a per-row NOT EXISTS so a partial re-run completes without a PK
--    violation. NOTE: this is not "customisation-safe" - if an admin has
--    already removed a seeded permission from a built-in role, re-running this
--    script puts it back. Migrations are meant to run once.
-- -----------------------------------------------------------------------------
-- 1 Administrador General -> everything
INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT 1, p.id FROM dbo.permission p
WHERE NOT EXISTS (SELECT 1 FROM dbo.role_permission rp WHERE rp.person_type_id = 1 AND rp.permission_id = p.id);

-- 2 Administrador -> everything except the Tienda section and roles.gestionar
--   (only Administrador General administers roles)
INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT 2, p.id FROM dbo.permission p
WHERE p.section <> 'tienda' AND p.code <> 'roles.gestionar'
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permission rp WHERE rp.person_type_id = 2 AND rp.permission_id = p.id);

-- 3 Empleado -> Ventas, Clientes and Alertas (view + acknowledge/mute)
INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT 3, p.id FROM dbo.permission p
WHERE p.code IN ('ventas.acceso', 'clientes.acceso', 'clientes.gestionar',
                 'alertas.acceso', 'alertas.reconocer', 'alertas.silenciar')
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permission rp WHERE rp.person_type_id = 3 AND rp.permission_id = p.id);
GO
PRINT '4. role_permission seeded for built-in roles';
GO

PRINT '--- Migration 002 -> 1.2.0 complete ---';
GO
