-- =============================================================================
-- Migration 032: security_event audit trail (fase 1)
--
-- One row per sensitive administrative action: who did it, to what, and a
-- human-readable summary. Fase 1 covers roles/permissions, users, the store
-- profile and the alert configuration; products/categories/clients/suppliers
-- come later. The auth trail (login_attempt), price history
-- (product_price_history) and alert history (product_alert_history) stay as
-- their own dedicated logs.
--
--   actor_id   person.id of who performed it (NULL = system / not signed in)
--   action     e.g. 'role.permissions', 'user.create', 'store.update'
--   entity     table the action targeted ('person_type', 'person', 'store', ...)
--   entity_id  its id, when there is one
--   summary    short readable description, e.g.
--              "rol Empleado: +ventas.acceso, -reportes.acceso"
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF OBJECT_ID('dbo.security_event') IS NULL
    CREATE TABLE [dbo].[security_event](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [at] [datetime] NOT NULL CONSTRAINT [DF_security_event_at] DEFAULT (getdate()),
        [actor_id] [int] NULL,
        [action] [varchar](40) NOT NULL,
        [entity] [varchar](40) NULL,
        [entity_id] [int] NULL,
        [summary] [varchar](400) NULL,
        [station] [varchar](100) NULL,
        CONSTRAINT [PK_security_event] PRIMARY KEY CLUSTERED ([id] ASC)
    );
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_security_event_at' AND object_id = OBJECT_ID('dbo.security_event'))
    CREATE INDEX [IX_security_event_at] ON [dbo].[security_event] ([at]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_security_event_entity' AND object_id = OBJECT_ID('dbo.security_event'))
    CREATE INDEX [IX_security_event_entity] ON [dbo].[security_event] ([entity], [entity_id]);
GO

-- Kept long: this is an audit log. Trims rows older than two years.
CREATE OR ALTER PROCEDURE [dbo].[sp_purge_security_event] AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.security_event WHERE at < DATEADD(YEAR, -2, GETDATE());
END
GO

PRINT '--- Migration 032 complete (security_event) ---';
GO
