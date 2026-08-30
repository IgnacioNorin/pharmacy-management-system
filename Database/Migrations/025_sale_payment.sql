-- =============================================================================
-- Migration 025: mixed payment on a sale
--
-- A sale can now be split across payment methods (e.g. part cash, part card).
-- The real breakdown lives in sale_payment (one row per method); sale.payment_method
-- is kept as the "primary" method (the one with the largest amount) for the report
-- column and cheap filtering.
--
-- Backfills one sale_payment row per existing sale and credit note, from its single
-- payment_method and total_amount (negative for credit notes, which they already are).
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 025 (sale payment) starting ---';
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'sale_payment')
BEGIN
    CREATE TABLE [dbo].[sale_payment] (
        [id]             [int] IDENTITY(1,1) NOT NULL,
        [sale_id]        [int] NOT NULL,
        [payment_method] [varchar](20) NOT NULL,
        [amount]         [decimal](18, 2) NOT NULL,
        CONSTRAINT [PK_sale_payment] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_sale_payment_sale] FOREIGN KEY ([sale_id]) REFERENCES [dbo].[sale] ([id])
    );
    CREATE INDEX [IX_sale_payment_sale] ON [dbo].[sale_payment] ([sale_id]);
END
GO

-- One row per sale/credit note that does not have a breakdown yet.
INSERT INTO [dbo].[sale_payment] (sale_id, payment_method, amount)
SELECT s.id, ISNULL(NULLIF(s.payment_method, ''), 'Efectivo'), ISNULL(s.total_amount, 0)
FROM [dbo].[sale] s
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[sale_payment] sp WHERE sp.sale_id = s.id);
GO

PRINT '--- Migration 025 complete ---';
GO
