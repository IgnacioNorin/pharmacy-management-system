-- =============================================================================
-- Migration 009: recipient fiscal data on a Factura
--
-- Adds sale.recipient_tax_id / _business_name / _activity / _address / _commune,
-- all nullable - only filled when the sale document type is a Factura.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO

IF COL_LENGTH('dbo.sale', 'recipient_tax_id') IS NULL
    ALTER TABLE dbo.sale ADD recipient_tax_id VARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.sale', 'recipient_business_name') IS NULL
    ALTER TABLE dbo.sale ADD recipient_business_name VARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.sale', 'recipient_activity') IS NULL
    ALTER TABLE dbo.sale ADD recipient_activity VARCHAR(80) NULL;
GO
IF COL_LENGTH('dbo.sale', 'recipient_address') IS NULL
    ALTER TABLE dbo.sale ADD recipient_address VARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.sale', 'recipient_commune') IS NULL
    ALTER TABLE dbo.sale ADD recipient_commune VARCHAR(60) NULL;
GO

PRINT '--- Migration 009 complete (factura recipient columns) ---';
GO
