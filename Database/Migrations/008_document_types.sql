-- =============================================================================
-- Migration 008: document types (Boleta / Factura)
--
--   store.default_document_type   type pre-selected on the sale screen
--   seq_folio_boleta / seq_folio_factura   one folio sequence per type
--       (replaces the single seq_sale_folio)
--   UX_sale_document_number   now unique per (document_type, document_number)
--
-- Existing sales were all written as "Factura"; seq_folio_factura is restarted
-- past their highest number so numbering continues without a collision.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 008 (document types) starting ---';
GO

-- 1. store.default_document_type
IF COL_LENGTH('dbo.store', 'default_document_type') IS NULL
    ALTER TABLE dbo.store ADD default_document_type VARCHAR(20) NOT NULL
        CONSTRAINT DF_store_default_document_type DEFAULT ('Boleta');
GO
PRINT '1. store.default_document_type';
GO

-- 2. Per-type folio sequences
IF OBJECT_ID('dbo.seq_folio_boleta', 'SO') IS NULL
    CREATE SEQUENCE dbo.seq_folio_boleta AS INT START WITH 1 INCREMENT BY 1;
GO
IF OBJECT_ID('dbo.seq_folio_factura', 'SO') IS NULL
    CREATE SEQUENCE dbo.seq_folio_factura AS INT START WITH 1 INCREMENT BY 1;
GO

DECLARE @nextFactura INT =
    ISNULL((SELECT MAX(TRY_CONVERT(INT, document_number)) FROM dbo.sale WHERE document_type = 'Factura'), 0) + 1;
IF @nextFactura > 1
BEGIN
    DECLARE @sql NVARCHAR(200) = N'ALTER SEQUENCE dbo.seq_folio_factura RESTART WITH ' + CAST(@nextFactura AS NVARCHAR(20));
    EXEC sys.sp_executesql @sql;
END
GO

IF OBJECT_ID('dbo.seq_sale_folio', 'SO') IS NOT NULL
    DROP SEQUENCE dbo.seq_sale_folio;
GO
PRINT '2. per-type folio sequences';
GO

-- 3. Unique per (document_type, document_number)
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_sale_document_number' AND object_id = OBJECT_ID('dbo.sale'))
    DROP INDEX UX_sale_document_number ON dbo.sale;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_sale_document_number' AND object_id = OBJECT_ID('dbo.sale'))
    CREATE UNIQUE INDEX UX_sale_document_number ON dbo.sale (document_type, document_number) WHERE document_number IS NOT NULL;
GO
PRINT '3. UX_sale_document_number is now per document type';
GO

PRINT '--- Migration 008 complete ---';
GO
