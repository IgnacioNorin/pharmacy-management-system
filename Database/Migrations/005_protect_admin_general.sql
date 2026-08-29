-- =============================================================================
-- Migration 005: protect the Administrador General role
--
-- sp_delete_person and sp_update_person now refuse to remove or demote the last
-- active Administrador General (person_type_id = 1, status = 1). Without this a
-- regular Administrador could delete the top account and lock everyone out of
-- role / store administration, recoverable only from the database.
--
-- Run once against an existing PharmacyDB. Idempotent (CREATE OR ALTER).
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 005 (protect Administrador General) starting ---';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_person]
    @id_person INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;

    -- The last active Administrador General (role 1) cannot be removed: that would leave nobody
    -- able to administer roles or the store profile, with no way back from inside the app.
    IF EXISTS (SELECT 1 FROM person WHERE id = @id_person AND person_type_id = 1 AND status = 1)
       AND (SELECT COUNT(*) FROM person WHERE person_type_id = 1 AND status = 1) <= 1
    BEGIN
        RETURN;   -- @result stays 0
    END

    -- Same soft-delete pattern as products/categories: a person referenced by a sale, a
    -- purchase or an acknowledged alert cannot be physically removed (FK), so deactivate them
    -- instead. LoginPresenter must reject status = 0 so a former employee cannot sign in.
    IF NOT EXISTS (SELECT 1 FROM sale WHERE user_id = @id_person)
       AND NOT EXISTS (SELECT 1 FROM purchase WHERE person_id = @id_person)
       AND NOT EXISTS (SELECT 1 FROM product_alert_history WHERE acknowledged_by = @id_person)
    BEGIN
        DELETE FROM person WHERE id = @id_person;
        SET @result = 1;
    END
    ELSE
    BEGIN
        UPDATE person SET status = 0 WHERE id = @id_person;
        SET @result = 1;
    END
END
GO
PRINT '1. sp_delete_person';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_update_person]
    @id_person INT,
    @document VARCHAR(50),
    @name VARCHAR(50),
    @address VARCHAR(50),
    @phone VARCHAR(50),
    @password VARCHAR(255),
    @person_type_id INT,
    @result BIT OUTPUT
AS
BEGIN
    SET @result = 0;

    -- Cannot move the last active Administrador General (role 1) off that role.
    IF @person_type_id <> 1
       AND EXISTS (SELECT 1 FROM person WHERE id = @id_person AND person_type_id = 1 AND status = 1)
       AND (SELECT COUNT(*) FROM person WHERE person_type_id = 1 AND status = 1) <= 1
        RETURN;

    IF NOT EXISTS (SELECT * FROM person WHERE document_number = @document AND id != @id_person)
    BEGIN
        -- A NULL @password means "keep the current one".
        UPDATE person SET
            document_number = @document,
            name = @name,
            address = @address,
            phone = @phone,
            password = ISNULL(@password, password),
            person_type_id = @person_type_id
        WHERE id = @id_person;
        SET @result = 1;
    END
END
GO
PRINT '2. sp_update_person';
GO

PRINT '--- Migration 005 complete ---';
GO
