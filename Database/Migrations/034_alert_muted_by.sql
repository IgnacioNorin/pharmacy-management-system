-- =============================================================================
-- Migration 034: record who muted an alert
--
--   product_alert_history.muted_by   the person who muted the alert, mirroring
--                                    acknowledged_by. Cleared on unmute. NULL for
--                                    every row muted before this migration and
--                                    for alerts that were never muted (DEF-37).
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF COL_LENGTH('dbo.product_alert_history', 'muted_by') IS NULL
    ALTER TABLE dbo.product_alert_history ADD [muted_by] [int] NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_product_alert_history_muted_by')
    ALTER TABLE dbo.product_alert_history WITH NOCHECK
        ADD CONSTRAINT [FK_product_alert_history_muted_by]
        FOREIGN KEY ([muted_by]) REFERENCES [dbo].[person] ([id]);
GO
