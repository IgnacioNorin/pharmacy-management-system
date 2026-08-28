-- =============================================================================
-- Migration 013: sp_update_person also writes the client fiscal profile
--
-- Migration 012 added person.business_name / activity / commune / email /
-- is_company. Client edits go through sp_update_person, so it gains five
-- parameters and writes those columns too. The last-Administrador-General guard
-- and the duplicate-document check are unchanged.
--
-- Run once against an existing PharmacyDB. Idempotent (CREATE OR ALTER).
-- =============================================================================

USE [PharmacyDB]
GO
-- Baked into the procedure metadata at CREATE time; required because person has a
-- filtered unique index, and a plain sqlcmd session defaults QUOTED_IDENTIFIER OFF.
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_update_person]
    @id_person INT,
    @document VARCHAR(50),
    @name VARCHAR(50),
    @address VARCHAR(50),
    @phone VARCHAR(50),
    @password VARCHAR(255),
    @person_type_id INT,
    @business_name VARCHAR(120) = NULL,
    @activity VARCHAR(80) = NULL,
    @commune VARCHAR(60) = NULL,
    @email VARCHAR(120) = NULL,
    @is_company BIT = 0,
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
            person_type_id = @person_type_id,
            business_name = @business_name,
            activity = @activity,
            commune = @commune,
            email = @email,
            is_company = @is_company
        WHERE id = @id_person;
        SET @result = 1;
    END
END
GO

PRINT '--- Migration 013 complete (sp_update_person writes the fiscal profile) ---';
GO
