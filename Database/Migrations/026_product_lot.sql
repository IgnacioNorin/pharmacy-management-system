-- =============================================================================
-- Migration 026: lot traceability (DEF-02 fase A, fase 1)
--
-- product_lot holds one row per received batch of a product: how many units of
-- that batch are still on hand, its expiry and its purchase cost. A purchase
-- creates a lot per line; a sale consumes lots first-expiry-first-out; a credit
-- note puts the returned units back as a new (undated) lot. product.stock is kept
-- as the cached sum of the lots.
--
-- Backfills one lot per product that currently has stock, from its master
-- stock / date_expired / cost, so nothing is lost.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 026 (product lot) starting ---';
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'product_lot')
BEGIN
    CREATE TABLE [dbo].[product_lot] (
        [id]                 [int] IDENTITY(1,1) NOT NULL,
        [product_id]         [int] NOT NULL,
        [purchase_detail_id] [int] NULL,
        [quantity]           [int] NOT NULL,
        [date_expired]       [datetime] NULL,
        [unit_cost]          [decimal](18, 2) NULL,
        [received_at]        [datetime] NOT NULL CONSTRAINT [DF_product_lot_received_at] DEFAULT (GETDATE()),
        CONSTRAINT [PK_product_lot] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_product_lot_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[product] ([id]),
        CONSTRAINT [FK_product_lot_purchase_detail] FOREIGN KEY ([purchase_detail_id]) REFERENCES [dbo].[purchase_detail] ([id])
    );
    CREATE INDEX [IX_product_lot_product] ON [dbo].[product_lot] ([product_id], [quantity], [date_expired]);
END
GO

-- One lot per product that still has stock and no lots yet.
INSERT INTO [dbo].[product_lot] (product_id, quantity, date_expired, unit_cost, received_at)
SELECT p.id, p.stock, p.date_expired, ISNULL(p.average_cost, p.purchase_price), ISNULL(p.date_created, GETDATE())
FROM [dbo].[product] p
WHERE ISNULL(p.stock, 0) > 0
  AND NOT EXISTS (SELECT 1 FROM [dbo].[product_lot] pl WHERE pl.product_id = p.id);
GO

PRINT '--- Migration 026 complete ---';
GO
