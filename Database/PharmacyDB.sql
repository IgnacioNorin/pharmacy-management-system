USE [master]
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PharmacyDB')
    DROP DATABASE [PharmacyDB]
GO

CREATE DATABASE [PharmacyDB]
GO

ALTER DATABASE [PharmacyDB] SET COMPATIBILITY_LEVEL = 150
GO
ALTER DATABASE [PharmacyDB] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [PharmacyDB] SET ANSI_NULLS OFF
GO
ALTER DATABASE [PharmacyDB] SET ANSI_PADDING OFF
GO
ALTER DATABASE [PharmacyDB] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [PharmacyDB] SET ARITHABORT OFF
GO
ALTER DATABASE [PharmacyDB] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [PharmacyDB] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [PharmacyDB] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [PharmacyDB] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [PharmacyDB] SET CURSOR_DEFAULT GLOBAL
GO
ALTER DATABASE [PharmacyDB] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [PharmacyDB] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [PharmacyDB] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [PharmacyDB] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [PharmacyDB] SET DISABLE_BROKER
GO
ALTER DATABASE [PharmacyDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [PharmacyDB] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [PharmacyDB] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [PharmacyDB] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [PharmacyDB] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [PharmacyDB] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [PharmacyDB] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [PharmacyDB] SET RECOVERY SIMPLE
GO
ALTER DATABASE [PharmacyDB] SET MULTI_USER
GO
ALTER DATABASE [PharmacyDB] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [PharmacyDB] SET DB_CHAINING OFF
GO
ALTER DATABASE [PharmacyDB] SET FILESTREAM(NON_TRANSACTED_ACCESS = OFF)
GO
ALTER DATABASE [PharmacyDB] SET TARGET_RECOVERY_TIME = 60 SECONDS
GO
ALTER DATABASE [PharmacyDB] SET DELAYED_DURABILITY = DISABLED
GO
ALTER DATABASE [PharmacyDB] SET QUERY_STORE = OFF
GO

USE [PharmacyDB]
GO

CREATE TABLE [dbo].[category](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [description] [varchar](50) NULL,
    [status] [bit] NULL,
    [date_created] [datetime] NULL,
    CONSTRAINT [PK__CATEGORI__A3C02A1063820E8B] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[notification_settings](
    [id] [int] NOT NULL,
    [critical_stock] [int] NULL,
    [notify_day] [int] NULL,
    CONSTRAINT [PK_ConfigNotificacion] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[person_type](
    [id] [int] NOT NULL,
    [description] [varchar](50) NULL,
    [status] [bit] NULL,
    [date_created] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[person](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [document_number] [varchar](50) NULL,
    [name] [varchar](50) NULL,
    [address] [varchar](50) NULL,
    [phone] [varchar](50) NULL,
    [password] [varchar](255) NULL,
    [person_type_id] [int] NULL,
    [status] [bit] NULL,
    [date_created] [datetime] NULL,
    CONSTRAINT [PK__PERSONA__2EC8D2AC47F385CC] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[state_product](
    [id] [int] NOT NULL,
    [name] [varchar](50) NULL,
    [description] [nvarchar](255) NULL,
    CONSTRAINT [PK__StatePro__5F9A52DBD73F55F6] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[supplier](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [document_number] [varchar](50) NULL,
    [company_name] [varchar](50) NULL,
    [email] [varchar](50) NULL,
    [phone] [varchar](50) NULL,
    [status] [bit] NULL,
    [date_created] [datetime] NULL,
    CONSTRAINT [PK__PROVEEDO__E8B631AF38D9D70E] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[product](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [code] [varchar](50) NULL,
    [name] [varchar](50) NULL,
    [description] [varchar](500) NULL,
    [category_id] [int] NULL,
    [stock] [int] NULL,
    [purchase_price] [decimal](10, 2) NULL,
    [sale_price] [decimal](10, 2) NULL,
    [status] [int] NULL,
    [date_created] [datetime] NULL,
    [date_expired] [datetime] NULL,
    [delisted_product] [nvarchar](255) NULL,
    [status_change_date] [datetime] NULL,
    CONSTRAINT [PK__PRODUCTO__098892105C6ABBAB] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[store](
    [id] [int] NOT NULL,
    [document_store] [varchar](50) NULL,
    [company_name] [varchar](50) NULL,
    [email] [varchar](50) NULL,
    [phone] [varchar](50) NULL,
    [address] [varchar](50) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[purchase](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [person_id] [int] NULL,
    [supplier_id] [int] NULL,
    [total_amount] [decimal](10, 2) NULL,
    [document_type] [varchar](50) NULL,
    [document_number] [varchar](50) NULL,
    [date_registered] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[purchase_detail](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [purchase_id] [int] NULL,
    [product_id] [int] NULL,
    [stock] [int] NULL,
    [purchase_price] [decimal](10, 2) NULL,
    [sale_price] [decimal](10, 2) NULL,
    [total_amount] [decimal](10, 2) NULL,
    [date_registered] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[sale](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [document_type] [varchar](50) NULL,
    [document_number] [varchar](20) NULL,
    [user_id] [int] NULL,
    [document_client] [varchar](50) NULL,
    [name_client] [varchar](50) NULL,
    [total_amount] [decimal](10, 2) NULL,
    [amount_received] [decimal](10, 2) NOT NULL,
    [change_amount] [decimal](10, 2) NULL,
    [date_registered] [datetime] NULL,
    CONSTRAINT [PK__VENTA__BC1240BD8994C395] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[sale_detail](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [sale_id] [int] NULL,
    [product_id] [int] NULL,
    [stock] [int] NULL,
    [sale_price] [decimal](10, 2) NULL,
    [subtotal] [decimal](10, 2) NULL,
    [date_registered] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- DEFAULTS
ALTER TABLE [dbo].[category] ADD CONSTRAINT [DF__CATEGORIA__Estad__440B1D61] DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[category] ADD CONSTRAINT [DF__CATEGORIA__Fecha__44FF419A] DEFAULT (getdate()) FOR [date_created]
GO
ALTER TABLE [dbo].[person] ADD CONSTRAINT [DF__PERSONA__Estado__36B12243] DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[person] ADD CONSTRAINT [DF__PERSONA__FechaCr__37A5467C] DEFAULT (getdate()) FOR [date_created]
GO
ALTER TABLE [dbo].[person_type] ADD DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[person_type] ADD DEFAULT (getdate()) FOR [date_created]
GO
ALTER TABLE [dbo].[product] ADD CONSTRAINT [DF__PRODUCTO__Stock__21B6055D] DEFAULT ((0)) FOR [stock]
GO
ALTER TABLE [dbo].[product] ADD CONSTRAINT [DF__PRODUCTO__Precio__22AA2996] DEFAULT ((0)) FOR [purchase_price]
GO
ALTER TABLE [dbo].[product] ADD CONSTRAINT [DF__PRODUCTO__Precio__239E4DCF] DEFAULT ((0)) FOR [sale_price]
GO
ALTER TABLE [dbo].[product] ADD CONSTRAINT [DF__PRODUCTO__Estado__24927208] DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[product] ADD CONSTRAINT [DF__PRODUCTO__FechaC__25869641] DEFAULT (getdate()) FOR [date_created]
GO
ALTER TABLE [dbo].[purchase] ADD DEFAULT ((0)) FOR [total_amount]
GO
ALTER TABLE [dbo].[purchase] ADD DEFAULT ('Boleta') FOR [document_type]
GO
ALTER TABLE [dbo].[purchase] ADD DEFAULT (getdate()) FOR [date_registered]
GO
ALTER TABLE [dbo].[purchase_detail] ADD DEFAULT (getdate()) FOR [date_registered]
GO
ALTER TABLE [dbo].[sale] ADD CONSTRAINT [DF__VENTA__FechaRegi__5165187F] DEFAULT (getdate()) FOR [date_registered]
GO
ALTER TABLE [dbo].[sale_detail] ADD DEFAULT (getdate()) FOR [date_registered]
GO
ALTER TABLE [dbo].[supplier] ADD CONSTRAINT [DF__PROVEEDOR__Estad__4F7CD00D] DEFAULT ((1)) FOR [status]
GO
ALTER TABLE [dbo].[supplier] ADD CONSTRAINT [DF__PROVEEDOR__Fecha__5070F446] DEFAULT (getdate()) FOR [date_created]
GO

-- FOREIGN KEYS
ALTER TABLE [dbo].[person] WITH CHECK ADD CONSTRAINT [FK__PERSONA__IdTipoP__5812160E]
    FOREIGN KEY([person_type_id]) REFERENCES [dbo].[person_type] ([id])
GO
ALTER TABLE [dbo].[person] CHECK CONSTRAINT [FK__PERSONA__IdTipoP__5812160E]
GO
ALTER TABLE [dbo].[product] WITH CHECK ADD CONSTRAINT [FK__PRODUCTO__IdCate__20C1E124]
    FOREIGN KEY([category_id]) REFERENCES [dbo].[category] ([id])
GO
ALTER TABLE [dbo].[product] CHECK CONSTRAINT [FK__PRODUCTO__IdCate__20C1E124]
GO
ALTER TABLE [dbo].[product] WITH CHECK ADD CONSTRAINT [FK_PRODUCTO_StateProduct]
    FOREIGN KEY([status]) REFERENCES [dbo].[state_product] ([id])
GO
ALTER TABLE [dbo].[product] CHECK CONSTRAINT [FK_PRODUCTO_StateProduct]
GO
ALTER TABLE [dbo].[purchase] WITH CHECK ADD CONSTRAINT [FK__COMPRA__IdPerson__52593CB8]
    FOREIGN KEY([person_id]) REFERENCES [dbo].[person] ([id])
GO
ALTER TABLE [dbo].[purchase] CHECK CONSTRAINT [FK__COMPRA__IdPerson__52593CB8]
GO
ALTER TABLE [dbo].[purchase] WITH CHECK ADD CONSTRAINT [FK__COMPRA__IdProvee__534D60F1]
    FOREIGN KEY([supplier_id]) REFERENCES [dbo].[supplier] ([id])
GO
ALTER TABLE [dbo].[purchase] CHECK CONSTRAINT [FK__COMPRA__IdProvee__534D60F1]
GO
ALTER TABLE [dbo].[purchase_detail] WITH CHECK ADD
    FOREIGN KEY([purchase_id]) REFERENCES [dbo].[purchase] ([id])
GO
ALTER TABLE [dbo].[purchase_detail] WITH CHECK ADD CONSTRAINT [FK__DETALLE_C__IdPro__31EC6D26]
    FOREIGN KEY([product_id]) REFERENCES [dbo].[product] ([id])
GO
ALTER TABLE [dbo].[purchase_detail] CHECK CONSTRAINT [FK__DETALLE_C__IdPro__31EC6D26]
GO
ALTER TABLE [dbo].[sale] WITH CHECK ADD CONSTRAINT [FK__VENTA__IdUsuario__59FA5E80]
    FOREIGN KEY([user_id]) REFERENCES [dbo].[person] ([id])
GO
ALTER TABLE [dbo].[sale] CHECK CONSTRAINT [FK__VENTA__IdUsuario__59FA5E80]
GO
ALTER TABLE [dbo].[sale_detail] WITH CHECK ADD CONSTRAINT [FK__DETALLE_V__IdPro__3A81B327]
    FOREIGN KEY([product_id]) REFERENCES [dbo].[product] ([id])
GO
ALTER TABLE [dbo].[sale_detail] CHECK CONSTRAINT [FK__DETALLE_V__IdPro__3A81B327]
GO
ALTER TABLE [dbo].[sale_detail] WITH CHECK ADD CONSTRAINT [FK__DETALLE_V__IdVen__571DF1D5]
    FOREIGN KEY([sale_id]) REFERENCES [dbo].[sale] ([id])
GO
ALTER TABLE [dbo].[sale_detail] CHECK CONSTRAINT [FK__DETALLE_V__IdVen__571DF1D5]
GO

-- SEED DATA
INSERT INTO [dbo].[state_product] (id, name, description) VALUES (1, 'Activo', 'Producto disponible para la venta')
GO
INSERT INTO [dbo].[state_product] (id, name, description) VALUES (0, 'Inactivo', 'Producto dado de baja')
GO

-- STORED PROCEDURES
CREATE PROC [dbo].[sp_create_category]
@description VARCHAR(50),
@result INT OUTPUT
AS
BEGIN
    SET @result = 0
    IF NOT EXISTS (SELECT * FROM category WHERE UPPER(description) = UPPER(@description))
    BEGIN
        INSERT INTO category(description) VALUES (@description)
        SET @result = SCOPE_IDENTITY();
    END
    ELSE IF EXISTS (SELECT * FROM category WHERE UPPER(description) = UPPER(@description) AND status = 0)
    BEGIN
        UPDATE category SET status = 1 WHERE UPPER(description) = UPPER(@description);
        SELECT TOP 1 @result = id FROM category WHERE UPPER(description) = UPPER(@description);
    END
    ELSE
        SET @result = 0;
END
GO

CREATE PROC [dbo].[sp_create_person](
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
    IF NOT EXISTS (SELECT * FROM person WHERE document_number = @document)
    BEGIN
        INSERT INTO person(document_number,name,address,phone,password,person_type_id)
        VALUES (@document,@name,@address,@phone,@password,@person_type_id)
        SET @result = SCOPE_IDENTITY()
    END
END
GO

CREATE PROC [dbo].[sp_create_product](
@code VARCHAR(20),
@name VARCHAR(30),
@description VARCHAR(30),
@category_id INT,
@result INT OUTPUT
) AS
BEGIN
    SET @result = 0
    IF NOT EXISTS (SELECT * FROM product WHERE code = @code)
    BEGIN
        INSERT INTO product(code,name,description,category_id) VALUES (@code,@name,@description,@category_id)
        SET @result = SCOPE_IDENTITY()
    END
END
GO

CREATE PROCEDURE [dbo].[sp_create_supplier](
@document VARCHAR(50),
@company_name VARCHAR(50),
@email VARCHAR(50),
@phone VARCHAR(50),
@result INT OUTPUT
) AS
BEGIN
    SET @result = 0
    IF NOT EXISTS (SELECT * FROM supplier WHERE document_number = @document)
    BEGIN
        INSERT INTO supplier(document_number, company_name, email, phone)
        VALUES (@document,@company_name,@email,@phone)
        SET @result = SCOPE_IDENTITY()
    END
END
GO

CREATE PROCEDURE [dbo].[sp_delete_category]
    @category_id INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF NOT EXISTS (SELECT 1 FROM product WHERE category_id = @category_id)
    BEGIN
        DELETE FROM category WHERE id = @category_id;
        SET @result = 1;
    END
    ELSE IF EXISTS (SELECT 1 FROM product WHERE category_id = @category_id)
    BEGIN
        UPDATE category SET status = 0 WHERE id = @category_id;
        SET @result = 1;
    END
    ELSE
        SET @result = 0;
END
GO

CREATE PROCEDURE [dbo].[sp_delete_product]
    @id_product INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF NOT EXISTS (SELECT 1 FROM purchase_detail WHERE product_id = @id_product)
       AND NOT EXISTS (SELECT 1 FROM sale_detail WHERE product_id = @id_product)
    BEGIN
        DELETE FROM product WHERE id = @id_product;
        SET @result = 1;
    END
    ELSE
    BEGIN
        UPDATE product SET status = 0 WHERE id = @id_product;
        SET @result = 1;
    END
END
GO

CREATE PROCEDURE [dbo].[sp_update_category](
@category_id INT,
@description VARCHAR(50),
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM category WHERE description = @description AND id != @category_id)
        UPDATE category SET description = @description WHERE id = @category_id
    ELSE
        SET @result = 0
END
GO

CREATE PROCEDURE [dbo].[sp_update_notificacion_settings](
@critical_stock INT,
@notify_day INT,
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF EXISTS (SELECT * FROM notification_settings WHERE id = 1)
        UPDATE notification_settings SET critical_stock = @critical_stock, notify_day = @notify_day WHERE id = 1
    ELSE
        SET @result = 0
END
GO

CREATE PROCEDURE [dbo].[sp_update_person](
@id_person INT,
@document VARCHAR(50),
@name VARCHAR(50),
@address VARCHAR(50),
@phone VARCHAR(50),
@password VARCHAR(255),
@person_type_id INT,
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM person WHERE document_number = @document AND id != @id_person)
        UPDATE person SET
            document_number = @document,
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

CREATE PROCEDURE [dbo].[sp_update_product](
@id_product INT,
@code VARCHAR(20),
@name VARCHAR(30),
@description VARCHAR(30),
@category_id INT,
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM product WHERE code = @code AND id != @id_product)
        UPDATE product SET
            code = @code,
            name = @name,
            description = @description,
            category_id = @category_id
        WHERE id = @id_product
    ELSE
        SET @result = 0
END
GO

CREATE PROCEDURE [dbo].[sp_update_supplier](
@id_supplier INT,
@document VARCHAR(50),
@company_name VARCHAR(50),
@email VARCHAR(50),
@phone VARCHAR(50),
@result BIT OUTPUT
) AS
BEGIN
    SET @result = 1
    IF NOT EXISTS (SELECT * FROM supplier WHERE document_number = @document AND id != @id_supplier)
        UPDATE supplier SET
            document_number = @document,
            company_name = @company_name,
            email = @email,
            phone = @phone
        WHERE id = @id_supplier
    ELSE
        SET @result = 0
END
GO

USE [master]
GO
ALTER DATABASE [PharmacyDB] SET READ_WRITE
GO
