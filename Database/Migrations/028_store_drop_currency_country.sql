-- =============================================================================
-- Migration 028: the system is CLP-only, drop the currency / country settings
--
-- store.currency_culture and store.country_code backed a configurable currency
-- and country preset. The product now operates in Chilean pesos (CLP) only, with
-- Chile-fixed rules (RUT validation, Boleta/Factura), so both columns are removed.
-- The VAT rate (store.default_tax_rate) stays configurable.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 028 (drop store currency/country) starting ---';
GO

-- Drop the default constraint on currency_culture first (its name is known from the schema).
IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_store_currency_culture')
    ALTER TABLE [dbo].[store] DROP CONSTRAINT [DF_store_currency_culture];
GO

-- Any other default bound to either column (defensive, in case a name differs by environment).
DECLARE @drop nvarchar(max) = N'';
SELECT @drop = @drop + N'ALTER TABLE [dbo].[store] DROP CONSTRAINT [' + dc.name + N'];'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.default_object_id = dc.object_id
WHERE c.object_id = OBJECT_ID('dbo.store') AND c.name IN ('currency_culture', 'country_code');
IF LEN(@drop) > 0 EXEC sp_executesql @drop;
GO

IF COL_LENGTH('dbo.store', 'currency_culture') IS NOT NULL
    ALTER TABLE [dbo].[store] DROP COLUMN [currency_culture];
GO
IF COL_LENGTH('dbo.store', 'country_code') IS NOT NULL
    ALTER TABLE [dbo].[store] DROP COLUMN [country_code];
GO

PRINT '--- Migration 028 complete ---';
GO
