-- =============================================================================
-- Migration 029: split retail clients out of `person` into their own `client` table
--
-- `person` did three jobs: application user (password + role), retail client, and
-- Factura recipient (business_name / activity / commune / email / is_company).
-- Clients now live in `client` - no password, no role. `person` keeps only users.
--
-- Steps:
--   1. Create `client` (+ unique document index).
--   2. Move the person_type_id = 4 rows into `client`, keeping their ids so the
--      existing sale.client_id values stay valid.
--   3. Repoint FK_sale_client from person to client.
--   4. Delete the moved rows from `person`; drop the 5 fiscal columns.
--   5. Drop the `Cliente` role (person_type id 4) and its role_permission rows.
--   6. Rebuild sp_update_person / sp_delete_person without the client bits; add
--      sp_delete_client (same soft-delete pattern as sp_delete_supplier).
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 029 (split client from person) starting ---';
GO

-- 1. client table -------------------------------------------------------------
IF OBJECT_ID('dbo.client') IS NULL
BEGIN
    CREATE TABLE [dbo].[client](
        [id]              [int] IDENTITY(1,1) NOT NULL,
        [document_number] [varchar](50) NULL,
        [name]            [varchar](50) NULL,
        [address]         [varchar](50) NULL,
        [phone]           [varchar](50) NULL,
        [business_name]   [varchar](120) NULL,
        [activity]        [varchar](80) NULL,
        [commune]         [varchar](60) NULL,
        [email]           [varchar](120) NULL,
        [is_company]      [bit] NOT NULL CONSTRAINT [DF_client_is_company] DEFAULT ((0)),
        [status]          [bit] NULL CONSTRAINT [DF_client_status] DEFAULT ((1)),
        [date_created]    [datetime] NULL CONSTRAINT [DF_client_date_created] DEFAULT (getdate()),
        CONSTRAINT [PK_client] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_client_document' AND object_id = OBJECT_ID('dbo.client'))
    CREATE UNIQUE INDEX [UX_client_document] ON [dbo].[client] ([document_number]) WHERE [document_number] IS NOT NULL;
GO

-- 2-4. one-time data move: only while person still carries the fiscal columns ---
IF COL_LENGTH('dbo.person', 'business_name') IS NOT NULL
BEGIN
    -- Guard: a client row must not be referenced anywhere a user is expected.
    IF EXISTS (
        SELECT 1 FROM dbo.person p
        WHERE p.person_type_id = 4 AND (
            EXISTS (SELECT 1 FROM dbo.sale s WHERE s.user_id = p.id) OR
            EXISTS (SELECT 1 FROM dbo.purchase pu WHERE pu.person_id = p.id) OR
            EXISTS (SELECT 1 FROM dbo.product_alert_history h WHERE h.acknowledged_by = p.id) OR
            EXISTS (SELECT 1 FROM dbo.cash_count c WHERE c.user_id = p.id)))
    BEGIN
        RAISERROR('Migration 029: a person_type_id = 4 row is referenced as a user (sale.user_id / purchase.person_id / product_alert_history.acknowledged_by / cash_count.user_id). Clean that up before running this migration.', 16, 1);
        RETURN;
    END

    SET IDENTITY_INSERT dbo.client ON;
    INSERT INTO dbo.client (id, document_number, name, address, phone, business_name, activity, commune, email, is_company, status, date_created)
    SELECT id, document_number, name, address, phone, business_name, activity, commune, email,
           ISNULL(is_company, 0), ISNULL(status, 1), ISNULL(date_created, GETDATE())
    FROM dbo.person
    WHERE person_type_id = 4;
    SET IDENTITY_INSERT dbo.client OFF;

    -- Repoint the sale -> client link (values are unchanged: ids were preserved).
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_sale_client')
        ALTER TABLE dbo.sale DROP CONSTRAINT FK_sale_client;
    ALTER TABLE dbo.sale WITH CHECK ADD CONSTRAINT FK_sale_client
        FOREIGN KEY (client_id) REFERENCES dbo.client (id);

    DELETE FROM dbo.person WHERE person_type_id = 4;

    -- Drop the fiscal columns (and the is_company default that blocks the drop).
    DECLARE @df sysname = (
        SELECT dc.name FROM sys.default_constraints dc
        JOIN sys.columns c ON c.default_object_id = dc.object_id
        WHERE c.object_id = OBJECT_ID('dbo.person') AND c.name = 'is_company');
    IF @df IS NOT NULL EXEC('ALTER TABLE dbo.person DROP CONSTRAINT [' + @df + ']');

    ALTER TABLE dbo.person DROP COLUMN business_name, activity, commune, email, is_company;
END
GO

-- 5. retire the Cliente role -------------------------------------------------
DELETE FROM dbo.role_permission WHERE person_type_id = 4;
DELETE FROM dbo.person_type WHERE id = 4;
GO

-- 6. stored procedures ------------------------------------------------------
CREATE OR ALTER PROCEDURE [dbo].[sp_update_person](
    @id_person INT,
    @document VARCHAR(50),
    @name VARCHAR(50),
    @address VARCHAR(50),
    @phone VARCHAR(50),
    @password VARCHAR(255),
    @person_type_id INT,
    @result BIT OUTPUT
) AS
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

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_person]
    @id_person INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;

    -- The last active Administrador General (role 1) cannot be removed.
    IF EXISTS (SELECT 1 FROM person WHERE id = @id_person AND person_type_id = 1 AND status = 1)
       AND (SELECT COUNT(*) FROM person WHERE person_type_id = 1 AND status = 1) <= 1
    BEGIN
        RETURN;
    END

    -- A user referenced by a sale (seller), a purchase or an acknowledged alert cannot be
    -- physically removed (FK), so deactivate instead.
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

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_client]
    @id_client INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;

    -- A client referenced by a sale cannot be physically removed (FK), so deactivate instead.
    -- Same soft-delete pattern as sp_delete_supplier.
    IF NOT EXISTS (SELECT 1 FROM sale WHERE client_id = @id_client)
    BEGIN
        DELETE FROM client WHERE id = @id_client;
        SET @result = 1;
    END
    ELSE
    BEGIN
        UPDATE client SET status = 0 WHERE id = @id_client;
        SET @result = 1;
    END
END
GO

PRINT '--- Migration 029 complete ---';
GO
