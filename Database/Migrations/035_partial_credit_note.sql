-- =============================================================================
-- Migration 035: partial credit notes
--
--   sale_detail.source_detail_id   on a Nota de Credito line, the sale_detail.id
--                                  of the original sale line it credits. NULL for
--                                  every sale line and for credit-note lines
--                                  written before this migration (those reversed
--                                  the whole sale).
--
-- With this link a sale can be credited line by line and unit by unit across
-- several notes: "credited so far for line L" = SUM(stock) of the credit-note
-- lines whose source_detail_id = L, and the remaining creditable quantity is the
-- line's sold quantity minus that.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF COL_LENGTH('dbo.sale_detail', 'source_detail_id') IS NULL
    ALTER TABLE dbo.sale_detail ADD [source_detail_id] [int] NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_sale_detail_source_detail')
    ALTER TABLE dbo.sale_detail WITH NOCHECK
        ADD CONSTRAINT [FK_sale_detail_source_detail]
        FOREIGN KEY ([source_detail_id]) REFERENCES [dbo].[sale_detail] ([id]);
GO

-- QUOTED_IDENTIFIER must be ON at CREATE time for a filtered index.
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ix_sale_detail_source_detail')
    CREATE INDEX [ix_sale_detail_source_detail] ON [dbo].[sale_detail] ([source_detail_id])
        WHERE [source_detail_id] IS NOT NULL;
GO
