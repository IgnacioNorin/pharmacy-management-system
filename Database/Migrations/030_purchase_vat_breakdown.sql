-- =============================================================================
-- Migration 030: VAT breakdown on purchases
--
--   purchase.net_amount      taxable base of the invoice (net of VAT)
--   purchase.tax_amount      VAT charged by the supplier (the "credito fiscal")
--   purchase.exempt_amount   part of the invoice not subject to VAT
--   purchase.tax_rate        VAT rate captured at purchase time (so a later rate
--                            change does not rewrite past invoices)
--
-- Line purchase prices are entered VAT-included (same convention as the sale
-- screen); the net is backed out: net = round(gross / (1 + rate/100)). The
-- breakdown is computed in PurchasePresenter and stored on the header here.
--
-- Backfill: historical purchases are assumed fully taxable at 19%, the only
-- rate the system has used. total_amount stays unchanged.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF COL_LENGTH('dbo.purchase', 'net_amount') IS NULL
    ALTER TABLE dbo.purchase ADD
        [net_amount] [decimal](18, 2) NOT NULL CONSTRAINT [DF_purchase_net_amount] DEFAULT ((0)),
        [tax_amount] [decimal](18, 2) NOT NULL CONSTRAINT [DF_purchase_tax_amount] DEFAULT ((0)),
        [exempt_amount] [decimal](18, 2) NOT NULL CONSTRAINT [DF_purchase_exempt_amount] DEFAULT ((0)),
        [tax_rate] [decimal](5, 2) NOT NULL CONSTRAINT [DF_purchase_tax_rate] DEFAULT ((19));
GO

-- Seed the breakdown for invoices registered before this migration. The guard
-- (net_amount = 0 AND tax_amount = 0) keeps it from running twice.
UPDATE dbo.purchase
SET net_amount = ROUND(total_amount / 1.19, 0),
    tax_amount = total_amount - ROUND(total_amount / 1.19, 0),
    exempt_amount = 0,
    tax_rate = 19
WHERE total_amount IS NOT NULL AND total_amount > 0
  AND net_amount = 0 AND tax_amount = 0;
GO

PRINT '--- Migration 030 complete (purchase VAT breakdown) ---';
GO
