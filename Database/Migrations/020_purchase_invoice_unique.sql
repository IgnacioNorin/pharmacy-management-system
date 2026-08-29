-- =============================================================================
-- Migration 020: one purchase per supplier invoice
--
-- Registering the same supplier invoice twice used to add the stock a second
-- time (no check on the document number). This adds a unique index over
-- (supplier_id, document_type, document_number) so the second INSERT fails at
-- the database; PurchaseRepository catches the violation and the user is told
-- the invoice was already recorded.
--
-- Filtered on document_number IS NOT NULL so historical rows with a null number
-- do not collide.
--
-- If CREATE UNIQUE INDEX fails, there are already duplicate invoices. Find them
-- with:
--   SELECT supplier_id, document_type, document_number, COUNT(*)
--   FROM purchase WHERE document_number IS NOT NULL
--   GROUP BY supplier_id, document_type, document_number HAVING COUNT(*) > 1;
-- and consolidate (or renumber) before re-running.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_purchase_supplier_document' AND object_id = OBJECT_ID('dbo.purchase'))
    CREATE UNIQUE INDEX [UX_purchase_supplier_document]
        ON [dbo].[purchase] ([supplier_id], [document_type], [document_number])
        WHERE [document_number] IS NOT NULL;
GO

PRINT '--- Migration 020 complete (UX_purchase_supplier_document) ---';
GO
