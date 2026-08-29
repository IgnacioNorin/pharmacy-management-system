-- =============================================================================
-- Least-privilege SQL Server login for the PharmacySystem application (DEF-07).
--
-- The application must NOT connect as `sa` (or any sysadmin). It only needs to
-- read and write the PharmacyDB tables, execute its stored procedures, and
-- advance the folio sequences. It never runs DDL - that is the DbMigrator's job,
-- which uses a separate, higher-privilege connection only at deploy time.
--
-- Run this ONCE against the instance, as a sysadmin, AFTER creating the database
-- (Database\PharmacyDB.sql). Change the password below before running.
--
--   App runtime  -> pharmacy_app  (this script: datareader + datawriter +
--                                  EXECUTE + UPDATE on dbo)
--   Deploy time  -> a db_owner-on-PharmacyDB account (or sa), used only to run
--                   PharmacyDB.sql and the migrations.
-- =============================================================================

SET NOCOUNT ON;
GO

-- 1. Server login ------------------------------------------------------------
IF SUSER_ID('pharmacy_app') IS NULL
BEGIN
    CREATE LOGIN [pharmacy_app]
        WITH PASSWORD = N'CHANGE_ME_StrongPassword!',
             DEFAULT_DATABASE = [PharmacyDB],
             CHECK_POLICY = ON;
END
GO

-- 2. Database user ----------------------------------------------------------
USE [PharmacyDB];
GO

IF DATABASE_PRINCIPAL_ID('pharmacy_app') IS NULL
    CREATE USER [pharmacy_app] FOR LOGIN [pharmacy_app];
GO

-- 3. Minimal privileges ---------------------------------------------------
ALTER ROLE [db_datareader] ADD MEMBER [pharmacy_app];   -- SELECT on all tables/views
ALTER ROLE [db_datawriter] ADD MEMBER [pharmacy_app];   -- INSERT / UPDATE / DELETE
GO

GRANT EXECUTE ON SCHEMA::dbo TO [pharmacy_app];         -- all sp_* the app calls
GRANT UPDATE  ON SCHEMA::dbo TO [pharmacy_app];         -- NEXT VALUE FOR seq_folio_*
GO

-- Explicitly deny anything schema-changing, in case a future role grants it.
DENY ALTER, CREATE TABLE, CREATE PROCEDURE, CREATE VIEW, REFERENCES ON SCHEMA::dbo TO [pharmacy_app];
GO

PRINT '--- pharmacy_app created with least privilege on PharmacyDB. Set its password and use it in ConnectionStrings.config. ---';
GO
