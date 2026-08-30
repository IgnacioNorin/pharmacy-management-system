-- =============================================================================
-- Migration 031: login hardening
--
--   person.must_change_password   forces a password change on the next login
--                                 (seeded admin + every user created from the
--                                 Usuarios screen start with it set).
--   login_attempt                 one row per authentication attempt. Failed
--                                 attempts drive the lockout (5 failures for a
--                                 document in the last 15 minutes); a row with
--                                 success = 1 (a good login, an admin unlock or
--                                 an admin reset) clears the running count.
--
-- The lockout is derived from this table - there is no mutable counter column to
-- get out of sync. Automatic unlock = wait out the 15-minute window or log in
-- correctly; manual unlock = an admin inserts a success row.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF COL_LENGTH('dbo.person', 'must_change_password') IS NULL
    ALTER TABLE dbo.person ADD [must_change_password] [bit] NOT NULL
        CONSTRAINT [DF_person_must_change_password] DEFAULT ((0));
GO

IF OBJECT_ID('dbo.login_attempt') IS NULL
    CREATE TABLE [dbo].[login_attempt](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [document_number] [varchar](50) NOT NULL,
        [success] [bit] NOT NULL,
        -- 'login' | 'admin_unlock' | 'admin_reset'
        [reason] [varchar](20) NOT NULL CONSTRAINT [DF_login_attempt_reason] DEFAULT ('login'),
        [actor_id] [int] NULL,
        [station] [varchar](100) NULL,
        [at] [datetime] NOT NULL CONSTRAINT [DF_login_attempt_at] DEFAULT (getdate()),
        CONSTRAINT [PK_login_attempt] PRIMARY KEY CLUSTERED ([id] ASC)
    );
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_login_attempt_document_at' AND object_id = OBJECT_ID('dbo.login_attempt'))
    CREATE INDEX [IX_login_attempt_document_at] ON [dbo].[login_attempt] ([document_number], [at]);
GO

-- Force the change only if the default account still has the factory password in
-- plain text; if it was already changed, leave it alone.
UPDATE dbo.person
SET must_change_password = 1
WHERE document_number = '1010101010' AND password = '12345678';
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_purge_login_attempts] AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.login_attempt WHERE at < DATEADD(DAY, -90, GETDATE());
END
GO

PRINT '--- Migration 031 complete (login hardening) ---';
GO
