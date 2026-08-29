-- =============================================================================
-- Migration 003: 1.2.0  (roles admin stored procedures)
--
-- Adds the stored procedures the roles admin screen (frmRoles) needs. No table
-- or data change - migration 002 already added permission / role_permission and
-- person_type.is_system.
--
-- Run once against an existing PharmacyDB. Idempotent (CREATE OR ALTER).
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 003 (roles admin) starting ---';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_set_role_permissions]
    @person_type_id INT,
    @permission_ids VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
        DELETE FROM role_permission WHERE person_type_id = @person_type_id;

        INSERT INTO role_permission (person_type_id, permission_id)
        SELECT DISTINCT @person_type_id, TRY_CONVERT(INT, value)
        FROM STRING_SPLIT(ISNULL(@permission_ids, ''), ',')
        WHERE TRY_CONVERT(INT, value) IN (SELECT id FROM permission);
    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_create_person_type]
    @description VARCHAR(50),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF NOT EXISTS (SELECT 1 FROM person_type WHERE UPPER(description) = UPPER(@description))
    BEGIN
        DECLARE @newId INT = (SELECT ISNULL(MAX(id), 99) + 1 FROM person_type WHERE id >= 100);
        IF @newId < 100 SET @newId = 100;
        INSERT INTO person_type (id, description, status, date_created, is_system)
        VALUES (@newId, @description, 1, GETDATE(), 0);
        SET @result = @newId;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_update_person_type]
    @id INT,
    @description VARCHAR(50),
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF EXISTS (SELECT 1 FROM person_type WHERE id = @id AND is_system = 0)
       AND NOT EXISTS (SELECT 1 FROM person_type WHERE UPPER(description) = UPPER(@description) AND id <> @id)
    BEGIN
        UPDATE person_type SET description = @description WHERE id = @id;
        SET @result = 1;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_person_type]
    @id INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF EXISTS (SELECT 1 FROM person_type WHERE id = @id AND is_system = 0)
       AND NOT EXISTS (SELECT 1 FROM person WHERE person_type_id = @id)
    BEGIN
        DELETE FROM person_type WHERE id = @id;
        SET @result = 1;
    END
END
GO

PRINT '--- Migration 003 (roles admin) complete ---';
GO
