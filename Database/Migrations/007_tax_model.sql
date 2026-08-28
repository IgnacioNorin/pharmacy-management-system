-- =============================================================================
-- Migration 007: VAT model
--
-- Adds a VAT breakdown to sales. Prices stay VAT-included; the net is backed out
-- for tax-affected items. Country-neutral: the rate lives in store.default_tax_rate
-- (19 by default = Chile, but it is a setting).
--
--   product.tax_affected        1 = affected by VAT (default), 0 = exempt
--   sale_detail.tax_affected    snapshot of the product flag at sale time
--   sale.net_amount / tax_amount / exempt_amount   (net + tax + exempt = total_amount)
--   store.default_tax_rate      percentage, default 19
--
-- Existing sales are backfilled assuming everything was affected at 19%.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 007 (VAT model) starting ---';
GO

IF COL_LENGTH('dbo.product', 'tax_affected') IS NULL
    ALTER TABLE dbo.product ADD tax_affected BIT NOT NULL CONSTRAINT DF_product_tax_affected DEFAULT (1);
GO

IF COL_LENGTH('dbo.sale_detail', 'tax_affected') IS NULL
    ALTER TABLE dbo.sale_detail ADD tax_affected BIT NOT NULL CONSTRAINT DF_sale_detail_tax_affected DEFAULT (1);
GO

IF COL_LENGTH('dbo.store', 'default_tax_rate') IS NULL
    ALTER TABLE dbo.store ADD default_tax_rate DECIMAL(5,2) NOT NULL CONSTRAINT DF_store_default_tax_rate DEFAULT (19);
GO

IF COL_LENGTH('dbo.sale', 'net_amount') IS NULL
    ALTER TABLE dbo.sale ADD net_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_sale_net_amount DEFAULT (0);
GO
IF COL_LENGTH('dbo.sale', 'tax_amount') IS NULL
    ALTER TABLE dbo.sale ADD tax_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_sale_tax_amount DEFAULT (0);
GO
IF COL_LENGTH('dbo.sale', 'exempt_amount') IS NULL
    ALTER TABLE dbo.sale ADD exempt_amount DECIMAL(18,2) NOT NULL CONSTRAINT DF_sale_exempt_amount DEFAULT (0);
GO
PRINT '1. columns added';
GO

-- Backfill existing sales: assume 100% affected at 19%.
UPDATE dbo.sale
SET net_amount = ROUND(total_amount / 1.19, 0),
    tax_amount = total_amount - ROUND(total_amount / 1.19, 0),
    exempt_amount = 0
WHERE total_amount > 0 AND net_amount = 0 AND tax_amount = 0;
GO
PRINT '2. existing sales backfilled';
GO

PRINT '--- Migration 007 complete ---';
GO
