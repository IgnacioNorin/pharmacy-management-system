-- =============================================================================
-- Migration 024: cash-drawer reconciliation ("arqueo de caja")
--
-- Adds cash_count / cash_count_line so a shift close can be recorded: expected
-- vs counted amount per payment method for the period since the previous close.
-- Sales are never touched. Also adds the 'caja.acceso' permission (section
-- 'caja') and grants it to the two built-in admin roles.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 024 (cash count) starting ---';
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'cash_count')
BEGIN
    CREATE TABLE [dbo].[cash_count] (
        [id]           [int] IDENTITY(1,1) NOT NULL,
        [period_start] [datetime] NOT NULL,
        [period_end]   [datetime] NOT NULL,
        [user_id]      [int] NULL,
        [notes]        [nvarchar](500) NULL,
        [created_at]   [datetime] NOT NULL CONSTRAINT [DF_cash_count_created_at] DEFAULT (GETDATE()),
        CONSTRAINT [PK_cash_count] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_cash_count_user] FOREIGN KEY ([user_id]) REFERENCES [dbo].[person] ([id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'cash_count_line')
BEGIN
    CREATE TABLE [dbo].[cash_count_line] (
        [id]              [int] IDENTITY(1,1) NOT NULL,
        [cash_count_id]   [int] NOT NULL,
        [payment_method]  [varchar](20) NOT NULL,
        [expected_amount] [decimal](18, 2) NOT NULL,
        [counted_amount]  [decimal](18, 2) NOT NULL,
        CONSTRAINT [PK_cash_count_line] PRIMARY KEY CLUSTERED ([id] ASC),
        CONSTRAINT [FK_cash_count_line_header] FOREIGN KEY ([cash_count_id])
            REFERENCES [dbo].[cash_count] ([id])
    );
    CREATE INDEX [IX_cash_count_line_header] ON [dbo].[cash_count_line] ([cash_count_id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[permission] WHERE code = 'caja.acceso')
BEGIN
    INSERT INTO [dbo].[permission] (code, section, description, parent_code)
    VALUES ('caja.acceso', 'caja', 'Registrar el arqueo de caja', NULL);
END
GO

-- Grant it to the built-in admin roles (1 Administrador General, 2 Administrador).
INSERT INTO [dbo].[role_permission] (person_type_id, permission_id)
SELECT r.person_type_id, p.id
FROM (VALUES (1), (2)) AS r(person_type_id)
CROSS JOIN [dbo].[permission] p
WHERE p.code = 'caja.acceso'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[role_permission] rp
                  WHERE rp.person_type_id = r.person_type_id AND rp.permission_id = p.id);
GO

PRINT '--- Migration 024 complete ---';
GO
