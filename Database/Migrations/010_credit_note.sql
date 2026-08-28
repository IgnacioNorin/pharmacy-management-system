-- =============================================================================
-- Migration 010: Nota de Credito (reverse a sale)
--
--   sale.reference_id / reference_reason   a NC points at the sale it reverses
--   seq_folio_nota_credito                 its own folio sequence
--   permission ventas.nota_credito         under ventas.acceso; granted to the
--                                          two admin roles (not Empleado)
--
-- Run once against an existing PharmacyDB. Idempotent.
-- =============================================================================

USE [PharmacyDB]
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

PRINT '--- Migration 010 (nota de credito) starting ---';
GO

IF COL_LENGTH('dbo.sale', 'reference_id') IS NULL
    ALTER TABLE dbo.sale ADD reference_id INT NULL;
GO
IF COL_LENGTH('dbo.sale', 'reference_reason') IS NULL
    ALTER TABLE dbo.sale ADD reference_reason VARCHAR(255) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_sale_reference')
    ALTER TABLE dbo.sale WITH CHECK ADD CONSTRAINT FK_sale_reference
        FOREIGN KEY (reference_id) REFERENCES dbo.sale (id);
GO

IF OBJECT_ID('dbo.seq_folio_nota_credito', 'SO') IS NULL
    CREATE SEQUENCE dbo.seq_folio_nota_credito AS INT START WITH 1 INCREMENT BY 1;
GO
PRINT '1. reference columns + sequence';
GO

INSERT INTO dbo.permission (code, section, description, parent_code)
SELECT 'ventas.nota_credito', 'ventas', 'Emitir notas de credito (anular ventas)', 'ventas.acceso'
WHERE NOT EXISTS (SELECT 1 FROM dbo.permission WHERE code = 'ventas.nota_credito');
GO

INSERT INTO dbo.role_permission (person_type_id, permission_id)
SELECT r.person_type_id, p.id
FROM (VALUES (1), (2)) r(person_type_id)
CROSS JOIN dbo.permission p
WHERE p.code = 'ventas.nota_credito'
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permission x WHERE x.person_type_id = r.person_type_id AND x.permission_id = p.id);
GO
PRINT '2. ventas.nota_credito permission';
GO

PRINT '--- Migration 010 complete ---';
GO
