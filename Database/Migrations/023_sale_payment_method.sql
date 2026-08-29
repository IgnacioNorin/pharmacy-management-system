-- =============================================================================
-- Migration 023: payment method on the sale
--
-- sale.payment_method records how the sale was collected (Efectivo / Tarjeta /
-- Transferencia). One method per sale for now. Existing rows are filled with
-- 'Efectivo' by the NOT NULL default.
--
-- The sale screen adds a combo, the ticket shows the method, and the sales
-- report gets a "Forma de Pago" column.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.sale') AND name = 'payment_method')
    ALTER TABLE dbo.sale ADD [payment_method] [varchar](20) NOT NULL
        CONSTRAINT [DF_sale_payment_method] DEFAULT ('Efectivo');
GO

PRINT '--- Migration 023 complete (sale.payment_method) ---';
GO
