-- =============================================================================
-- Migration 019: price management and commercialization state
--
-- A product now has an explicit "released for sale" flag, separate from its
-- soft-delete status. The flow is: create the product (not released), buy stock
-- from a supplier (stock arrives, still not released), then release it from the
-- Prices screen - which sets the first sale price and flips the flag. Sales only
-- offer released products.
--
--   product.is_released      0 = in stock but not for sale, 1 = sellable
--   product_price_history     one row per deliberate price change (release,
--                             re-price), with the cost at that moment, the user
--                             and a free-text reason.
--
-- Backfill: every product that already has a sale price was being sold under the
-- old model, so it is marked released - the current catalogue does not disappear.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.product') AND name = 'is_released')
BEGIN
    ALTER TABLE dbo.product ADD [is_released] [bit] NOT NULL
        CONSTRAINT [DF_product_is_released] DEFAULT ((0));
END
GO

-- Backfill: anything with a real sale price was already being sold.
UPDATE dbo.product
SET is_released = 1
WHERE is_released = 0 AND sale_price IS NOT NULL AND sale_price > 0;
GO

IF OBJECT_ID('dbo.product_price_history', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[product_price_history](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [product_id] [int] NOT NULL,
        [event_type] [varchar](20) NOT NULL CONSTRAINT [DF_product_price_history_event] DEFAULT ('cambio'),
        [sale_price] [decimal](18, 2) NOT NULL,
        [cost] [decimal](18, 2) NULL,
        [changed_at] [datetime] NOT NULL CONSTRAINT [DF_product_price_history_changed_at] DEFAULT (getdate()),
        [user_id] [int] NULL,
        [reason] [nvarchar](255) NULL,
        CONSTRAINT [PK_product_price_history] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_product_price_history_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[product] ([id])
    );

    CREATE INDEX [IX_product_price_history_product] ON [dbo].[product_price_history] ([product_id], [changed_at]);
END
GO

-- event_type added after the table's first version.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.product_price_history') AND name = 'event_type')
    ALTER TABLE dbo.product_price_history ADD [event_type] [varchar](20) NOT NULL
        CONSTRAINT [DF_product_price_history_event] DEFAULT ('cambio');
GO

PRINT '--- Migration 019 complete (product.is_released + product_price_history) ---';
GO
