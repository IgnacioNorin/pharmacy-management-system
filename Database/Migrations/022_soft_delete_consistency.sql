-- =============================================================================
-- Migration 022: consistent soft-delete
--
-- sp_delete_supplier: same pattern as sp_delete_person / sp_delete_product - a
-- supplier referenced by a purchase cannot be physically removed (FK), so
-- deactivate it (status = 0); otherwise DELETE. SupplierRepository stops doing a
-- raw DELETE, which failed with a misleading "revise los datos".
--
-- person.status / supplier.status already default to 1; any leftover NULL is
-- backfilled to 1 so "active" is unambiguous. The application side then filters
-- status = 0 out of the operational lists (supplier list, client picker, client
-- screen, report client filter).
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

UPDATE dbo.person   SET status = 1 WHERE status IS NULL;
UPDATE dbo.supplier SET status = 1 WHERE status IS NULL;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_delete_supplier]
    @id_supplier INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;

    -- A supplier referenced by a purchase cannot be physically removed (FK), so
    -- deactivate instead. Same soft-delete pattern as products/persons/categories.
    IF NOT EXISTS (SELECT 1 FROM purchase WHERE supplier_id = @id_supplier)
    BEGIN
        DELETE FROM supplier WHERE id = @id_supplier;
        SET @result = 1;
    END
    ELSE
    BEGIN
        UPDATE supplier SET status = 0 WHERE id = @id_supplier;
        SET @result = 1;
    END
END
GO

PRINT '--- Migration 022 complete (sp_delete_supplier + status backfill) ---';
GO
