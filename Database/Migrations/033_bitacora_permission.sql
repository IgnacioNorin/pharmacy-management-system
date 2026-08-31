-- =============================================================================
-- Migration 033: 'bitacora.acceso' permission
--
-- Gates the new "Bitácora" screen that shows the security_event audit trail
-- (migration 032). Granted to the two built-in admin roles.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[permission] WHERE code = 'bitacora.acceso')
BEGIN
    INSERT INTO [dbo].[permission] (code, section, description, parent_code)
    VALUES ('bitacora.acceso', 'usuarios', 'Ver la bitácora de acciones', NULL);
END
GO

-- Grant it to the built-in admin roles (1 Administrador General, 2 Administrador).
INSERT INTO [dbo].[role_permission] (person_type_id, permission_id)
SELECT r.person_type_id, p.id
FROM (VALUES (1), (2)) AS r(person_type_id)
CROSS JOIN [dbo].[permission] p
WHERE p.code = 'bitacora.acceso'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[role_permission] rp
                  WHERE rp.person_type_id = r.person_type_id AND rp.permission_id = p.id);
GO

PRINT '--- Migration 033 complete (bitacora.acceso) ---';
GO
