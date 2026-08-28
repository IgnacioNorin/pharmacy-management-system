-- =============================================================================
-- Migration 016: neutral default currency
--
-- The fallback currency (used only when the store has none set / no preset
-- picked) moves from es-EC (Ecuador, no longer part of the project) to en-US.
-- Only the column default constraint is changed; a store that already chose a
-- currency keeps it. The app already treats a leftover 'es-EC' as the default
-- at runtime (es-EC was dropped from CultureInfoHelper.SupportedCurrencies).
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__STORE__CurrencyC')
    ALTER TABLE dbo.store DROP CONSTRAINT DF__STORE__CurrencyC;
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_store_currency_culture')
    ALTER TABLE dbo.store ADD CONSTRAINT DF_store_currency_culture DEFAULT ('en-US') FOR currency_culture;
GO

PRINT '--- Migration 016 complete (default currency is en-US) ---';
GO
