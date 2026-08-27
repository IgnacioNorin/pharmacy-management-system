-- =============================================================================
-- Migration 004: report permissions per type + permission.parent_code
--
-- 1. Adds permission.parent_code (two-level shape for the roles admin screen).
-- 2. Keeps reportes.acceso as the section gate and adds one "view" + one
--    "export" permission per report type (ventas, compras, productos, alertas).
--    reportes.exportar is removed (replaced by the four "<tipo>.exportar").
-- 3. Every role that could view reports keeps viewing every report; every role
--    that could export keeps exporting every report - behaviour only changes
--    once an admin narrows a role.
--
-- Run once against an existing PharmacyDB (>= migration 002). Idempotent:
-- re-running is a no-op. Take a backup first.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 004 (report permissions per type) starting ---';
GO

-- -----------------------------------------------------------------------------
-- 1. permission.parent_code
-- -----------------------------------------------------------------------------
IF COL_LENGTH('dbo.permission', 'parent_code') IS NULL
BEGIN
    ALTER TABLE dbo.permission ADD parent_code VARCHAR(60) NULL;
END
GO
PRINT '1. permission.parent_code';
GO

-- -----------------------------------------------------------------------------
-- 2. Add the eight granular report permissions (only the ones still missing)
-- -----------------------------------------------------------------------------
INSERT INTO dbo.permission (code, section, description, parent_code)
SELECT v.code, 'reportes', v.description, v.parent_code
FROM (VALUES
    ('reportes.ventas',            'Ver el reporte de ventas',       'reportes.acceso'),
    ('reportes.ventas.exportar',   'Exportar el reporte de ventas',  'reportes.ventas'),
    ('reportes.compras',           'Ver el reporte de compras',      'reportes.acceso'),
    ('reportes.compras.exportar',  'Exportar el reporte de compras', 'reportes.compras'),
    ('reportes.productos',         'Ver el reporte de productos',    'reportes.acceso'),
    ('reportes.productos.exportar','Exportar el reporte de productos','reportes.productos'),
    ('reportes.alertas',           'Ver el historial de alertas',    'reportes.acceso'),
    ('reportes.alertas.exportar',  'Exportar el historial de alertas','reportes.alertas')
) v(code, description, parent_code)
WHERE NOT EXISTS (SELECT 1 FROM dbo.permission p WHERE p.code = v.code);
GO
PRINT '2. granular report permissions added';
GO

-- -----------------------------------------------------------------------------
-- 3. Backfill parent_code for the whole catalogue
-- -----------------------------------------------------------------------------
UPDATE p SET parent_code =
    CASE
        WHEN p.code LIKE '%.acceso'            THEN NULL
        WHEN p.code = 'roles.gestionar'        THEN 'usuarios.acceso'
        WHEN p.code = 'reportes.ventas.exportar'    THEN 'reportes.ventas'
        WHEN p.code = 'reportes.compras.exportar'   THEN 'reportes.compras'
        WHEN p.code = 'reportes.productos.exportar' THEN 'reportes.productos'
        WHEN p.code = 'reportes.alertas.exportar'   THEN 'reportes.alertas'
        ELSE p.section + '.acceso'
    END
FROM dbo.permission p;
GO
PRINT '3. parent_code backfilled';
GO

-- -----------------------------------------------------------------------------
-- 4. Every role that had reportes.acceso gets the four "view" permissions
-- -----------------------------------------------------------------------------
INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT rp.person_type_id, np.id
FROM dbo.role_permission rp
JOIN dbo.permission oldp ON oldp.id = rp.permission_id AND oldp.code = 'reportes.acceso'
JOIN dbo.permission np   ON np.code IN ('reportes.ventas', 'reportes.compras',
                                       'reportes.productos', 'reportes.alertas')
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.role_permission x
    WHERE x.person_type_id = rp.person_type_id AND x.permission_id = np.id);
GO

-- -----------------------------------------------------------------------------
-- 5. Every role that had reportes.exportar gets the four "export" permissions
-- -----------------------------------------------------------------------------
INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT rp.person_type_id, np.id
FROM dbo.role_permission rp
JOIN dbo.permission oldp ON oldp.id = rp.permission_id AND oldp.code = 'reportes.exportar'
JOIN dbo.permission np   ON np.code IN ('reportes.ventas.exportar', 'reportes.compras.exportar',
                                       'reportes.productos.exportar', 'reportes.alertas.exportar')
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.role_permission x
    WHERE x.person_type_id = rp.person_type_id AND x.permission_id = np.id);
GO

-- -----------------------------------------------------------------------------
-- 6. Safety net: any role holding a granular reportes.* permission but missing
--    the reportes.acceso gate gets it back (covers a partial / earlier run).
-- -----------------------------------------------------------------------------
INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT DISTINCT rp.person_type_id, gate.id
FROM dbo.role_permission rp
JOIN dbo.permission g    ON g.id = rp.permission_id AND g.section = 'reportes' AND g.code <> 'reportes.acceso'
JOIN dbo.permission gate ON gate.code = 'reportes.acceso'
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.role_permission x
    WHERE x.person_type_id = rp.person_type_id AND x.permission_id = gate.id);
GO
PRINT '4/5/6. existing roles remapped to the granular permissions';
GO

-- -----------------------------------------------------------------------------
-- 7. Drop the obsolete reportes.exportar. role_permission rows referencing it
--    disappear via FK_role_permission_permission ON DELETE CASCADE.
-- -----------------------------------------------------------------------------
DELETE FROM dbo.permission WHERE code = 'reportes.exportar';
GO
PRINT '7. reportes.exportar removed';
GO

PRINT '--- Migration 004 complete ---';
GO
