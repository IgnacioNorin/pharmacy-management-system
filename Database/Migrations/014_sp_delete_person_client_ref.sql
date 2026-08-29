-- =============================================================================
-- Migration 014: sp_delete_person treats sale.client_id as a reference
--
-- Migration 012 linked a sale to its client (sale.client_id -> person, FK
-- FK_sale_client). sp_delete_person decides hard-delete vs soft-delete by
-- scanning the tables that reference a person, but it was never taught about
-- client_id, so deleting a client who appears on any sale ran DELETE FROM
-- person, hit the FK and failed ("No se pudo eliminar") instead of
-- deactivating the client the way a referenced person is meant to be handled.
--
-- Run once against an existing PharmacyDB. Idempotent (CREATE OR ALTER).
-- =============================================================================

USE [PharmacyDB]
GO
-- Baked into the procedure metadata at CREATE time; person has a filtered unique
-- index and a plain sqlcmd session defaults QUOTED_IDENTIFIER OFF.
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
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

    -- Same soft-delete pattern as products/categories: a person referenced by a sale (as the
    -- seller or the client), a purchase or an acknowledged alert cannot be physically removed
    -- (FK), so deactivate them instead. LoginPresenter must reject status = 0 so a former
    -- employee cannot sign in.
    IF NOT EXISTS (SELECT 1 FROM sale WHERE user_id = @id_person)
       AND NOT EXISTS (SELECT 1 FROM sale WHERE client_id = @id_person)
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

PRINT '--- Migration 014 complete (sp_delete_person honours sale.client_id) ---';
GO
