-- =============================================================================
-- Migration 018: expiry date per purchase line
--
-- Adds purchase_detail.date_expired so the expiry entered for each incoming lot
-- is kept on the line that recorded it, instead of only overwriting the single
-- product.date_expired field.
--
-- The matching behaviour change lives in the application (PurchaseRepository):
-- the product master's date_expired now only moves EARLIER on a purchase, so a
-- newer lot with a later expiry can no longer switch off the expiry alert for
-- older stock still on the shelf. This script only adds the column.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.purchase_detail') AND name = 'date_expired')
    ALTER TABLE dbo.purchase_detail ADD [date_expired] [datetime] NULL;
GO

PRINT '--- Migration 018 complete (purchase_detail.date_expired) ---';
GO
