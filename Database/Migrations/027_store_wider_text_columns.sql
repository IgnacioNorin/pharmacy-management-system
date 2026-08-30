-- =============================================================================
-- Migration 027: widen the store profile text columns
--
-- store.company_name / address / email / phone / document_store were varchar(50),
-- too short for a real "razon social" or address. Saving longer text failed with
-- "String or binary data would be truncated" and the app showed a misleading
-- "no se pudo guardar los datos, revise los datos".
--
-- Run once against an existing PharmacyDB. Idempotent (checks current length).
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 027 (store wider text columns) starting ---';
GO

IF COL_LENGTH('dbo.store', 'company_name') < 150
    ALTER TABLE [dbo].[store] ALTER COLUMN [company_name] [varchar](150) NULL;
GO
IF COL_LENGTH('dbo.store', 'address') < 200
    ALTER TABLE [dbo].[store] ALTER COLUMN [address] [varchar](200) NULL;
GO
IF COL_LENGTH('dbo.store', 'email') < 120
    ALTER TABLE [dbo].[store] ALTER COLUMN [email] [varchar](120) NULL;
GO
IF COL_LENGTH('dbo.store', 'phone') < 30
    ALTER TABLE [dbo].[store] ALTER COLUMN [phone] [varchar](30) NULL;
GO
IF COL_LENGTH('dbo.store', 'document_store') < 30
    ALTER TABLE [dbo].[store] ALTER COLUMN [document_store] [varchar](30) NULL;
GO

PRINT '--- Migration 027 complete ---';
GO
