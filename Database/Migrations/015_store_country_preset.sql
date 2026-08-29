-- =============================================================================
-- Migration 015: store country preset
--
-- A "preset" is a named bundle of country defaults (VAT rate, currency culture,
-- recipient-document validation scheme). It lives in code (CountryPresets); the
-- store only remembers which one is active.
--
--   store.country_code   ISO 3166-1 alpha-2, or NULL for the generic preset.
--
-- Existing single-row installs that carry the Chilean fingerprint (VAT 19 and a
-- default document type of 'Boleta') are backfilled to 'CL'; anything else stays
-- NULL and reads as generic until an admin picks a preset.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

IF COL_LENGTH('dbo.store', 'country_code') IS NULL
    ALTER TABLE dbo.store ADD country_code VARCHAR(8) NULL;
GO

UPDATE dbo.store
SET country_code = 'CL'
WHERE country_code IS NULL
  AND default_tax_rate = 19
  AND default_document_type = 'Boleta';
GO

PRINT '--- Migration 015 complete (store.country_code) ---';
GO
