-- =============================================================================
-- Emergency reset of an Administrador General password.
--
-- Use this ONLY when the last active Administrador General cannot log in (forgot
-- the password) and there is no other account that can reset it from the Usuarios
-- screen. Run it against PharmacyDB as a sysadmin / db_owner.
--
-- It sets a temporary plain-text password and turns on must_change_password: the
-- value is re-hashed on the first login (the legacy migration path) and the user
-- is forced to choose a new one immediately. It also drops a login_attempt row so
-- the reset is on the record.
--
-- Set the two variables below before running.
-- =============================================================================

USE [PharmacyDB];
GO
SET NOCOUNT ON;
GO

DECLARE @document VARCHAR(50) = '1010101010';        -- document of the Administrador General
DECLARE @temp_password VARCHAR(255) = 'cambiar123';  -- temporary password to hand over

IF NOT EXISTS (SELECT 1 FROM dbo.person
               WHERE document_number = @document AND person_type_id = 1 AND status = 1)
BEGIN
    RAISERROR('No active Administrador General with that document.', 16, 1);
    RETURN;
END

UPDATE dbo.person
SET password = @temp_password,
    must_change_password = 1
WHERE document_number = @document AND person_type_id = 1 AND status = 1;

INSERT INTO dbo.login_attempt (document_number, success, reason, actor_id, station)
VALUES (@document, 1, 'admin_reset', NULL, 'reset_admin_password.sql');

PRINT '--- Password reset. Log in with the temporary password; a change is forced. ---';
GO
