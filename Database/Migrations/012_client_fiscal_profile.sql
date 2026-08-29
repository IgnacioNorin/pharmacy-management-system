-- =============================================================================
-- Migration 012: client fiscal profile + sale -> client link
--
-- Clients are dbo.person rows (person_type_id = Cliente). A Factura recipient
-- needs more than name + document, so person gains an optional fiscal profile:
--   business_name / activity / commune / email / is_company
-- (all NULL for users, suppliers and boleta-only clients).
--
-- A sale can now point at the client it was made to: sale.client_id, NULL for a
-- walk-in / consumidor final. Historical sales stay NULL - their client data
-- only ever existed as the free text in document_client / name_client.
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO

IF COL_LENGTH('dbo.person', 'business_name') IS NULL
    ALTER TABLE dbo.person ADD business_name VARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.person', 'activity') IS NULL
    ALTER TABLE dbo.person ADD activity VARCHAR(80) NULL;
GO
IF COL_LENGTH('dbo.person', 'commune') IS NULL
    ALTER TABLE dbo.person ADD commune VARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.person', 'email') IS NULL
    ALTER TABLE dbo.person ADD email VARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.person', 'is_company') IS NULL
    ALTER TABLE dbo.person ADD is_company BIT NOT NULL
        CONSTRAINT DF_person_is_company DEFAULT (0);
GO

IF COL_LENGTH('dbo.sale', 'client_id') IS NULL
    ALTER TABLE dbo.sale ADD client_id INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_sale_client')
    ALTER TABLE dbo.sale WITH CHECK ADD CONSTRAINT FK_sale_client
        FOREIGN KEY (client_id) REFERENCES dbo.person (id);
GO

PRINT '--- Migration 012 complete (client fiscal profile + sale.client_id) ---';
GO
