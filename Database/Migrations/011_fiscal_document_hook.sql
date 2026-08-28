-- =============================================================================
-- Migration 011: fiscal document issuance hook
--
-- Adds sale.fiscal_status / fiscal_track_id / fiscal_barcode. These back the
-- IFiscalDocumentIssuer seam: today every sale stays 'interno' (numbered by the
-- local sequence, no tax authority contacted); a DTE-provider issuer would fill
-- the tracking id / barcode and move the status to pendiente / aceptado / rechazado.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO

IF COL_LENGTH('dbo.sale', 'fiscal_status') IS NULL
    ALTER TABLE dbo.sale ADD fiscal_status VARCHAR(20) NOT NULL
        CONSTRAINT DF_sale_fiscal_status DEFAULT ('interno');
GO
IF COL_LENGTH('dbo.sale', 'fiscal_track_id') IS NULL
    ALTER TABLE dbo.sale ADD fiscal_track_id VARCHAR(64) NULL;
GO
IF COL_LENGTH('dbo.sale', 'fiscal_barcode') IS NULL
    ALTER TABLE dbo.sale ADD fiscal_barcode VARCHAR(512) NULL;
GO

PRINT '--- Migration 011 complete (fiscal document hook columns) ---';
GO
