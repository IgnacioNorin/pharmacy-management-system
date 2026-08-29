-- =============================================================================
-- Migration 001: base 1.0 -> 1.1.0
--
-- Run this ONCE against an existing PharmacyDB (a database created from an older
-- Database/PharmacyDB.sql). A brand-new install does NOT need it - the current
-- Database/PharmacyDB.sql already contains everything below.
--
-- The script is idempotent: running it again is a no-op and does not error.
--
-- Before running, take a full backup. Two steps can fail on pre-existing data;
-- both print a clear message and are safe to fix and re-run:
--   * UX_person_document / UX_supplier_document / UX_product_code /
--     UX_category_description / UX_sale_document_number fail if duplicate values
--     already exist. Detection queries are in Database/Migrations/README.md.
--   * CK_store_singleton / CK_notification_settings_singleton fail if a row with
--     id <> 1 exists in those tables.
-- =============================================================================

USE [PharmacyDB]
GO

-- Required ON so the filtered indexes and the recreated procedures below compile
-- the same way regardless of the client's own session defaults.
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 001 -> 1.1.0 starting ---';
GO

-- -----------------------------------------------------------------------------
-- 1. Database-level SET options (deprecated OFF values -> ON)
-- -----------------------------------------------------------------------------
ALTER DATABASE [PharmacyDB] SET ANSI_NULLS ON;
ALTER DATABASE [PharmacyDB] SET ANSI_PADDING ON;
ALTER DATABASE [PharmacyDB] SET ANSI_WARNINGS ON;
ALTER DATABASE [PharmacyDB] SET ARITHABORT ON;
ALTER DATABASE [PharmacyDB] SET QUOTED_IDENTIFIER ON;
ALTER DATABASE [PharmacyDB] SET CONCAT_NULL_YIELDS_NULL ON;
GO
PRINT '1. Database SET options -> ON';
GO

-- -----------------------------------------------------------------------------
-- 2. Money columns decimal(10,2) -> decimal(18,2) (widening, no data loss)
-- -----------------------------------------------------------------------------
ALTER TABLE dbo.product              ALTER COLUMN purchase_price  decimal(18,2) NULL;
ALTER TABLE dbo.product              ALTER COLUMN sale_price      decimal(18,2) NULL;
ALTER TABLE dbo.purchase             ALTER COLUMN total_amount    decimal(18,2) NULL;
ALTER TABLE dbo.purchase_detail      ALTER COLUMN purchase_price  decimal(18,2) NULL;
ALTER TABLE dbo.purchase_detail      ALTER COLUMN sale_price      decimal(18,2) NULL;
ALTER TABLE dbo.purchase_detail      ALTER COLUMN total_amount    decimal(18,2) NULL;
ALTER TABLE dbo.sale                 ALTER COLUMN total_amount    decimal(18,2) NULL;
ALTER TABLE dbo.sale                 ALTER COLUMN amount_received decimal(18,2) NOT NULL;
ALTER TABLE dbo.sale                 ALTER COLUMN change_amount   decimal(18,2) NULL;
ALTER TABLE dbo.sale_detail          ALTER COLUMN sale_price      decimal(18,2) NULL;
ALTER TABLE dbo.sale_detail          ALTER COLUMN subtotal        decimal(18,2) NULL;
ALTER TABLE dbo.product_alert_history ALTER COLUMN trigger_value  decimal(18,2) NULL;
GO
PRINT '2. Money columns -> decimal(18,2)';
GO

-- -----------------------------------------------------------------------------
-- 3. Single-row config tables: seed rows + CHECK (id = 1)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.store WHERE id = 1)
    INSERT INTO dbo.store (id, document_store, company_name, email, phone, address, currency_culture)
    VALUES (1, '', 'Mi Farmacia', '', '', '', 'es-EC');

IF NOT EXISTS (SELECT 1 FROM dbo.notification_settings WHERE id = 1)
    INSERT INTO dbo.notification_settings (id, critical_stock, notify_day) VALUES (1, 10, 30);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_store_singleton')
    ALTER TABLE dbo.store ADD CONSTRAINT CK_store_singleton CHECK (id = 1);

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_notification_settings_singleton')
    ALTER TABLE dbo.notification_settings ADD CONSTRAINT CK_notification_settings_singleton CHECK (id = 1);
GO
PRINT '3. store / notification_settings rows + singleton checks';
GO

-- -----------------------------------------------------------------------------
-- 4. Natural-key UNIQUE filtered indexes
--    (fail here => resolve duplicates first, see README, then re-run)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_person_document' AND object_id = OBJECT_ID('dbo.person'))
    CREATE UNIQUE INDEX [UX_person_document] ON dbo.person ([document_number]) WHERE [document_number] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_supplier_document' AND object_id = OBJECT_ID('dbo.supplier'))
    CREATE UNIQUE INDEX [UX_supplier_document] ON dbo.supplier ([document_number]) WHERE [document_number] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_product_code' AND object_id = OBJECT_ID('dbo.product'))
    CREATE UNIQUE INDEX [UX_product_code] ON dbo.product ([code]) WHERE [code] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_category_description' AND object_id = OBJECT_ID('dbo.category'))
    CREATE UNIQUE INDEX [UX_category_description] ON dbo.category ([description]) WHERE [description] IS NOT NULL;
GO
PRINT '4. Natural-key unique indexes';
GO

-- -----------------------------------------------------------------------------
-- 5. Foreign-key column indexes (non-unique, always safe)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_product_category' AND object_id = OBJECT_ID('dbo.product'))
    CREATE INDEX [IX_product_category] ON dbo.product ([category_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_sale_user' AND object_id = OBJECT_ID('dbo.sale'))
    CREATE INDEX [IX_sale_user] ON dbo.sale ([user_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_sale_date_registered' AND object_id = OBJECT_ID('dbo.sale'))
    CREATE INDEX [IX_sale_date_registered] ON dbo.sale ([date_registered]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_sale_detail_sale' AND object_id = OBJECT_ID('dbo.sale_detail'))
    CREATE INDEX [IX_sale_detail_sale] ON dbo.sale_detail ([sale_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_sale_detail_product' AND object_id = OBJECT_ID('dbo.sale_detail'))
    CREATE INDEX [IX_sale_detail_product] ON dbo.sale_detail ([product_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_purchase_person' AND object_id = OBJECT_ID('dbo.purchase'))
    CREATE INDEX [IX_purchase_person] ON dbo.purchase ([person_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_purchase_supplier' AND object_id = OBJECT_ID('dbo.purchase'))
    CREATE INDEX [IX_purchase_supplier] ON dbo.purchase ([supplier_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_purchase_date_registered' AND object_id = OBJECT_ID('dbo.purchase'))
    CREATE INDEX [IX_purchase_date_registered] ON dbo.purchase ([date_registered]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_purchase_detail_purchase' AND object_id = OBJECT_ID('dbo.purchase_detail'))
    CREATE INDEX [IX_purchase_detail_purchase] ON dbo.purchase_detail ([purchase_id]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_purchase_detail_product' AND object_id = OBJECT_ID('dbo.purchase_detail'))
    CREATE INDEX [IX_purchase_detail_product] ON dbo.purchase_detail ([product_id]);
GO
PRINT '5. Foreign-key column indexes';
GO

-- -----------------------------------------------------------------------------
-- 6. Sale receipt number: sequence + unique filtered index
--    The sequence starts past the highest existing numeric document_number so it
--    never collides with historic rows.
--    (unique index fails here => resolve duplicate sale.document_number first)
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.sequences WHERE name = 'seq_sale_folio')
BEGIN
    DECLARE @start int = (SELECT ISNULL(MAX(TRY_CONVERT(int, document_number)), 0) + 1 FROM dbo.sale);
    DECLARE @sql nvarchar(300) =
        N'CREATE SEQUENCE dbo.seq_sale_folio AS INT START WITH ' + CAST(@start AS nvarchar(20)) + N' INCREMENT BY 1';
    EXEC sp_executesql @sql;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_sale_document_number' AND object_id = OBJECT_ID('dbo.sale'))
    CREATE UNIQUE INDEX [UX_sale_document_number] ON dbo.sale ([document_number]) WHERE [document_number] IS NOT NULL;
GO
PRINT '6. seq_sale_folio + UX_sale_document_number';
GO

-- -----------------------------------------------------------------------------
-- 7. Stored procedures (CREATE OR ALTER - idempotent)
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROC [dbo].[sp_create_product](
@code VARCHAR(50),
@name VARCHAR(50),
@description VARCHAR(500),
@category_id INT,
@result INT OUTPUT
) AS
BEGIN
    SET @result = 0
    IF NOT EXISTS (SELECT * FROM product WHERE code = @code)
    BEGIN
        INSERT INTO product(code,name,description,category_id) VALUES (@code,@name,@description,@category_id)
        SET @result = SCOPE_IDENTITY()
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_update_product](
@id_product INT,
@code VARCHAR(50),
@name VARCHAR(50),
@description VARCHAR(500),
@category_id INT,
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM product WHERE code = @code AND id != @id_product)
        UPDATE product SET
            code = @code,
            name = @name,
            description = @description,
            category_id = @category_id
        WHERE id = @id_product
    ELSE
        SET @result = 0
END
GO

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
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM person WHERE document_number = @document AND id != @id_person)
        -- A NULL @password means "keep the current one".
        UPDATE person SET
            document_number = @document,
            name = @name,
            address = @address,
            phone = @phone,
            password = ISNULL(@password, password),
            person_type_id = @person_type_id
        WHERE id = @id_person
    ELSE
        SET @result = 0
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_update_category](
@category_id INT,
@description VARCHAR(50),
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM category WHERE UPPER(description) = UPPER(@description) AND id != @category_id)
        UPDATE category SET description = @description WHERE id = @category_id
    ELSE
        SET @result = 0
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_update_notificacion_settings](
@critical_stock INT,
@notify_day INT,
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF EXISTS (SELECT * FROM notification_settings WHERE id = 1)
        UPDATE notification_settings SET critical_stock = @critical_stock, notify_day = @notify_day WHERE id = 1
    ELSE
        INSERT INTO notification_settings (id, critical_stock, notify_day) VALUES (1, @critical_stock, @notify_day)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_product]
    @id_product INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF NOT EXISTS (SELECT 1 FROM purchase_detail WHERE product_id = @id_product)
       AND NOT EXISTS (SELECT 1 FROM sale_detail WHERE product_id = @id_product)
       AND NOT EXISTS (SELECT 1 FROM product_alert_history WHERE product_id = @id_product)
    BEGIN
        DELETE FROM product WHERE id = @id_product;
        SET @result = 1;
    END
    ELSE
    BEGIN
        UPDATE product SET status = 0 WHERE id = @id_product;
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
PRINT '7. Stored procedures updated';
GO

PRINT '--- Migration 001 -> 1.1.0 complete ---';
GO
