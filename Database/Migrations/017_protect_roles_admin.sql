-- =============================================================================
-- Migration 017: protect the roles admin screen from locking itself out
--
-- sp_set_role_permissions now refuses to save a permission set that would strip
-- 'roles.gestionar' from the last role that still holds it, and sp_delete_person_type
-- refuses to delete that last holder. Either one, done from inside frmRoles, would
-- leave nobody able to reopen the screen, recoverable only from the database.
-- This is the roles-admin analogue of the last-Administrador-General guard in
-- migration 005.
--
-- sp_set_role_permissions gains an @result BIT OUTPUT (0 = refused or error,
-- 1 = saved), matching the other role procedures.
--
-- Run once against an existing PharmacyDB. Idempotent (CREATE OR ALTER).
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 017 (protect roles admin) starting ---';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_set_role_permissions]
    @person_type_id INT,
    @permission_ids VARCHAR(MAX),   -- comma-separated permission ids, may be empty
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;

    DECLARE @role_admin_id INT = (SELECT id FROM permission WHERE code = 'roles.gestionar');

    -- The valid, de-duplicated ids being requested.
    DECLARE @incoming TABLE (id INT PRIMARY KEY);
    INSERT INTO @incoming (id)
    SELECT DISTINCT TRY_CONVERT(INT, value)
    FROM STRING_SPLIT(ISNULL(@permission_ids, ''), ',')
    WHERE TRY_CONVERT(INT, value) IN (SELECT id FROM permission);

    -- Refuse to drop 'roles.gestionar' from the only role that still grants it: from the next
    -- sign-in nobody could reopen frmRoles, with no way back from inside the app.
    IF @role_admin_id IS NOT NULL
       AND EXISTS (SELECT 1 FROM role_permission
                   WHERE person_type_id = @person_type_id AND permission_id = @role_admin_id)
       AND NOT EXISTS (SELECT 1 FROM @incoming WHERE id = @role_admin_id)
       AND (SELECT COUNT(DISTINCT person_type_id)
            FROM role_permission WHERE permission_id = @role_admin_id) <= 1
    BEGIN
        RETURN;   -- @result stays 0
    END

    BEGIN TRANSACTION;
        DELETE FROM role_permission WHERE person_type_id = @person_type_id;

        INSERT INTO role_permission (person_type_id, permission_id)
        SELECT @person_type_id, id FROM @incoming;
    COMMIT TRANSACTION;

    SET @result = 1;
END
GO
PRINT '1. sp_set_role_permissions';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_person_type]
    @id INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;

    DECLARE @role_admin_id INT = (SELECT id FROM permission WHERE code = 'roles.gestionar');

    -- Deleting a role cascades its role_permission rows. If this is the last role that grants
    -- 'roles.gestionar', that would lock everyone out of frmRoles (same reason as above).
    IF @role_admin_id IS NOT NULL
       AND EXISTS (SELECT 1 FROM role_permission
                   WHERE person_type_id = @id AND permission_id = @role_admin_id)
       AND (SELECT COUNT(DISTINCT person_type_id)
            FROM role_permission WHERE permission_id = @role_admin_id) <= 1
    BEGIN
        RETURN;   -- @result stays 0
    END

    IF EXISTS (SELECT 1 FROM person_type WHERE id = @id AND is_system = 0)
       AND NOT EXISTS (SELECT 1 FROM person WHERE person_type_id = @id)
    BEGIN
        DELETE FROM person_type WHERE id = @id;   -- role_permission rows cascade
        SET @result = 1;
    END
END
GO
PRINT '2. sp_delete_person_type';
GO

PRINT '--- Migration 017 complete ---';
GO
