-- Run this once against an existing PharmacyDB database to support hashed passwords.
-- Widens the password column and the create/update stored procedure parameters so
-- they can hold a PBKDF2 hash (~80 chars) instead of only a plain-text password.
-- Existing plain-text passwords are left untouched; the application hashes each
-- account's password automatically the next time that user logs in successfully.

USE [PharmacyDB]
GO

ALTER TABLE [dbo].[person] ALTER COLUMN [password] VARCHAR(255) NULL
GO

ALTER PROCEDURE [dbo].[sp_create_person] (
	@document VARCHAR(50),
	@name VARCHAR(50),
	@address VARCHAR(50),
	@phone VARCHAR(50),
	@password VARCHAR(255),
	@person_type_id INT,
	@result INT OUTPUT
) AS
BEGIN
	SET @result = 0
	DECLARE @person_id INT
	IF NOT EXISTS (SELECT * FROM person WHERE document_number = @document)
	BEGIN
		INSERT INTO person(document_number,name,address,phone,password,person_type_id) VALUES (
		@document,@name,@address,@phone,@password,@person_type_id)

		SET @result = SCOPE_IDENTITY()
	END
END
GO

ALTER PROCEDURE [dbo].[sp_update_person] (
	@id_person INT,
	@document VARCHAR(50),
	@name VARCHAR(50),
	@address VARCHAR(50),
	@phone VARCHAR(50),
	@password VARCHAR(255),
	@person_type_id INT,
	@result BIT OUTPUT
)
AS
BEGIN
	SET @result = 1
	IF NOT EXISTS (SELECT * FROM person WHERE document_number = @document and id != @id_person)

		UPDATE person
		SET document_number = @document,
			name = @name,
			address = @address,
			phone = @phone,
			password = @password,
			person_type_id = @person_type_id
		WHERE id = @id_person
	ELSE
		SET @result = 0

END
GO
