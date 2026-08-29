-- =============================================================================
-- Migration 006: drop the redundant create/update stored procedures
--
-- sp_create_person, sp_create_product, sp_create_supplier, sp_update_product,
-- sp_update_supplier and sp_update_category did nothing but a race-prone
-- "IF NOT EXISTS" duplicate check that the UNIQUE indexes (UX_person_document,
-- UX_product_code, UX_supplier_document, UX_category_description) already
-- enforce - case-insensitively, since the database collation is
-- SQL_Latin1_General_CP1_CI_AS. The repositories now run the INSERT / UPDATE
-- directly and map SQL error 2601 / 2627 to "duplicate".
--
-- sp_create_category is kept (it also reactivates a soft-deleted row) and
-- sp_update_person is kept (it guards the last Administrador General).
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO

DROP PROCEDURE IF EXISTS [dbo].[sp_create_person];
DROP PROCEDURE IF EXISTS [dbo].[sp_create_product];
DROP PROCEDURE IF EXISTS [dbo].[sp_create_supplier];
DROP PROCEDURE IF EXISTS [dbo].[sp_update_product];
DROP PROCEDURE IF EXISTS [dbo].[sp_update_supplier];
DROP PROCEDURE IF EXISTS [dbo].[sp_update_category];
GO

PRINT '--- Migration 006 complete (redundant SPs dropped) ---';
GO
