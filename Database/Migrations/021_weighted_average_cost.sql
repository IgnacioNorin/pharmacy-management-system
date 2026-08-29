-- =============================================================================
-- Migration 021: weighted-average cost
--
--   product.average_cost   moving weighted average, recomputed on every purchase:
--                          avg' = (stock*avg + qty*buyCost) / (stock + qty)
--   sale_detail.unit_cost   the product's average cost frozen on the line at the
--                          moment of the sale, so the margin of a past sale does
--                          not change when the cost changes later.
--
-- Backfill: average_cost starts from the last purchase price (purchase_price),
-- the best seed available. It converges to the true weighted average as new
-- purchases come in.
--
-- The recompute lives in PurchaseRepository.Register; the freeze lives in
-- SaleRepository.Register. This script only adds the columns.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.product') AND name = 'average_cost')
    ALTER TABLE dbo.product ADD [average_cost] [decimal](18, 2) NULL;
GO

UPDATE dbo.product
SET average_cost = purchase_price
WHERE average_cost IS NULL AND purchase_price IS NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.sale_detail') AND name = 'unit_cost')
    ALTER TABLE dbo.sale_detail ADD [unit_cost] [decimal](18, 2) NULL;
GO

PRINT '--- Migration 021 complete (product.average_cost + sale_detail.unit_cost) ---';
GO
