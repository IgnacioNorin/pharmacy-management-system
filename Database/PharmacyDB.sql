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
ALTER DATABASE [PharmacyDB] SET ANSI_NULLS ON
GO
ALTER DATABASE [PharmacyDB] SET ANSI_PADDING ON
GO
ALTER DATABASE [PharmacyDB] SET ANSI_WARNINGS ON
GO
ALTER DATABASE [PharmacyDB] SET ARITHABORT ON
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
ALTER DATABASE [PharmacyDB] SET CONCAT_NULL_YIELDS_NULL ON
GO
ALTER DATABASE [PharmacyDB] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [PharmacyDB] SET QUOTED_IDENTIFIER ON
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

-- Required ON for the filtered indexes below and baked into every procedure created here.
-- Set once for the whole session so the script does not depend on the client's own defaults
-- (SSMS connects with these ON, the legacy sqlcmd ODBC driver connects with them OFF).
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
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
    CONSTRAINT [PK_ConfigNotificacion] PRIMARY KEY CLUSTERED ([id] ASC),
    -- Single-row configuration table: the app always reads/writes id = 1.
    CONSTRAINT [CK_notification_settings_singleton] CHECK ([id] = 1)
)
GO

CREATE TABLE [dbo].[person_type](
    [id] [int] NOT NULL,
    [description] [varchar](50) NULL,
    [status] [bit] NULL,
    [date_created] [datetime] NULL,
    -- 1 for the four built-in roles: the roles admin screen must not let them be renamed or
    -- deleted. Custom roles (is_system = 0) use ids >= 100.
    [is_system] [bit] NOT NULL CONSTRAINT [DF_person_type_is_system] DEFAULT ((0)),
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- Catalogue of permissions, seeded by the app and not edited by users. parent_code gives the
-- two-level shape the roles admin screen renders: a section root (parent_code IS NULL, code
-- '<section>.acceso') with its inner permissions underneath, and the report "exportar" leaves
-- hanging off their "ver" permission.
CREATE TABLE [dbo].[permission](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [code] [varchar](60) NOT NULL,
    [section] [varchar](30) NOT NULL,
    [description] [varchar](150) NOT NULL,
    [parent_code] [varchar](60) NULL,
    CONSTRAINT [PK_permission] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [UX_permission_code] UNIQUE ([code])
)
GO

-- Which permissions each role grants. A user's effective permissions = the rows here for their
-- person_type_id.
CREATE TABLE [dbo].[role_permission](
    [person_type_id] [int] NOT NULL,
    [permission_id] [int] NOT NULL,
    CONSTRAINT [PK_role_permission] PRIMARY KEY CLUSTERED ([person_type_id] ASC, [permission_id] ASC),
    CONSTRAINT [FK_role_permission_role] FOREIGN KEY ([person_type_id]) REFERENCES [dbo].[person_type] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_role_permission_permission] FOREIGN KEY ([permission_id]) REFERENCES [dbo].[permission] ([id]) ON DELETE CASCADE
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
    [purchase_price] [decimal](18, 2) NULL,
    [sale_price] [decimal](18, 2) NULL,
    [status] [int] NULL,
    [date_created] [datetime] NULL,
    [date_expired] [datetime] NULL,
    [delisted_product] [nvarchar](255) NULL,
    [status_change_date] [datetime] NULL,
    CONSTRAINT [PK__PRODUCTO__098892105C6ABBAB] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- Backs the stock-critical and expiring-soon alert queries (NotificationConfigRepository), which
-- filter by status plus one of these columns and used to do a full table scan of every active
-- product on every poll.
CREATE INDEX [ix_product_status_stock] ON [dbo].[product] ([status], [stock]) INCLUDE ([code], [name])
GO

CREATE INDEX [ix_product_status_expired] ON [dbo].[product] ([status], [date_expired]) INCLUDE ([code], [name])
    WHERE [date_expired] IS NOT NULL
GO

CREATE TABLE [dbo].[store](
    [id] [int] NOT NULL,
    [document_store] [varchar](50) NULL,
    [company_name] [varchar](50) NULL,
    [email] [varchar](50) NULL,
    [phone] [varchar](50) NULL,
    [address] [varchar](50) NULL,
    [currency_culture] [varchar](10) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    -- Single-row store profile: the app always reads/writes id = 1.
    CONSTRAINT [CK_store_singleton] CHECK ([id] = 1)
)
GO

CREATE TABLE [dbo].[purchase](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [person_id] [int] NULL,
    [supplier_id] [int] NULL,
    [total_amount] [decimal](18, 2) NULL,
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
    [purchase_price] [decimal](18, 2) NULL,
    [sale_price] [decimal](18, 2) NULL,
    [total_amount] [decimal](18, 2) NULL,
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
    [total_amount] [decimal](18, 2) NULL,
    [amount_received] [decimal](18, 2) NOT NULL,
    [change_amount] [decimal](18, 2) NULL,
    [date_registered] [datetime] NULL,
    CONSTRAINT [PK__VENTA__BC1240BD8994C395] PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

CREATE TABLE [dbo].[sale_detail](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [sale_id] [int] NULL,
    [product_id] [int] NULL,
    [stock] [int] NULL,
    [sale_price] [decimal](18, 2) NULL,
    [subtotal] [decimal](18, 2) NULL,
    [date_registered] [datetime] NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
)
GO

-- Fase 4 of the alerts rework (traceability): one row per open-or-resolved stock/expiration
-- alert on a product. Written only on a state transition (a new alert appears, its severity
-- changes, or it clears) - not on every poll - so this grows with real inventory activity, not
-- with the passage of time. alert_type: 1 = stock, 2 = expiration. severity: 1 = low/expiring
-- soon, 2 = critical/expired (mirrors PharmacySystem.Model.AlertType/AlertSeverity).
CREATE TABLE [dbo].[product_alert_history](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [product_id] [int] NOT NULL,
    [alert_type] [tinyint] NOT NULL,
    [severity] [tinyint] NOT NULL,
    [trigger_value] [decimal](18, 2) NULL,
    [detected_at] [datetime] NOT NULL,
    [resolved_at] [datetime] NULL,
    [acknowledged_by] [int] NULL,
    [acknowledged_at] [datetime] NULL,
    [muted_at] [datetime] NULL,
    CONSTRAINT [PK_product_alert_history] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_product_alert_history_product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[product] ([id]),
    CONSTRAINT [FK_product_alert_history_person] FOREIGN KEY ([acknowledged_by]) REFERENCES [dbo].[person] ([id])
)
GO

-- QUOTED_IDENTIFIER must be ON at CREATE time for a filtered index, same reason as
-- ix_product_status_expired above.
SET QUOTED_IDENTIFIER ON
GO

-- Backs both the "is this product's alert already open" lookup (product_id + alert_type, one
-- open row at a time) and the general history browse.
CREATE INDEX [ix_product_alert_history_open] ON [dbo].[product_alert_history] ([product_id], [alert_type])
    WHERE [resolved_at] IS NULL
GO

CREATE INDEX [ix_product_alert_history_detected] ON [dbo].[product_alert_history] ([detected_at])
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
ALTER TABLE [dbo].[store] ADD CONSTRAINT [DF__STORE__CurrencyC] DEFAULT ('es-EC') FOR [currency_culture]
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

-- UNIQUE KEYS AND SUPPORTING INDEXES
-- Filtered UNIQUE indexes need ANSI_NULLS / QUOTED_IDENTIFIER ON at CREATE time; modern clients
-- already connect that way, but set it explicitly so the script is not client-dependent.
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Natural keys the stored procedures only guarded with a race-prone NOT EXISTS check.
CREATE UNIQUE INDEX [UX_person_document] ON [dbo].[person] ([document_number]) WHERE [document_number] IS NOT NULL
GO
CREATE UNIQUE INDEX [UX_supplier_document] ON [dbo].[supplier] ([document_number]) WHERE [document_number] IS NOT NULL
GO
CREATE UNIQUE INDEX [UX_product_code] ON [dbo].[product] ([code]) WHERE [code] IS NOT NULL
GO
CREATE UNIQUE INDEX [UX_category_description] ON [dbo].[category] ([description]) WHERE [description] IS NOT NULL
GO
-- Sale receipt number: monotonic and concurrency-safe (was RIGHT(..., COUNT(*) + 1), which
-- collided under concurrent sales and repeated after a delete). Gaps are acceptable.
CREATE SEQUENCE [dbo].[seq_sale_folio] AS INT START WITH 1 INCREMENT BY 1
GO
CREATE UNIQUE INDEX [UX_sale_document_number] ON [dbo].[sale] ([document_number]) WHERE [document_number] IS NOT NULL
GO

-- Foreign-key columns joined/filtered by reports and detail lookups (all unindexed before).
CREATE INDEX [IX_product_category] ON [dbo].[product] ([category_id])
GO
CREATE INDEX [IX_sale_user] ON [dbo].[sale] ([user_id])
GO
CREATE INDEX [IX_sale_date_registered] ON [dbo].[sale] ([date_registered])
GO
CREATE INDEX [IX_sale_detail_sale] ON [dbo].[sale_detail] ([sale_id])
GO
CREATE INDEX [IX_sale_detail_product] ON [dbo].[sale_detail] ([product_id])
GO
CREATE INDEX [IX_purchase_person] ON [dbo].[purchase] ([person_id])
GO
CREATE INDEX [IX_purchase_supplier] ON [dbo].[purchase] ([supplier_id])
GO
CREATE INDEX [IX_purchase_date_registered] ON [dbo].[purchase] ([date_registered])
GO
CREATE INDEX [IX_purchase_detail_purchase] ON [dbo].[purchase_detail] ([purchase_id])
GO
CREATE INDEX [IX_purchase_detail_product] ON [dbo].[purchase_detail] ([product_id])
GO

-- SEED DATA
INSERT INTO [dbo].[state_product] (id, name, description) VALUES (1, 'Activo', 'Producto disponible para la venta')
GO
INSERT INTO [dbo].[state_product] (id, name, description) VALUES (0, 'Inactivo', 'Producto dado de baja')
GO

-- Administrador General (1) is the only role that can see/edit the Tienda tab in frmManagement
-- (name, tax data, currency) - see frmManagement.frmManagement_Load. Administrador (2) is the
-- day-to-day admin role - full access except that tab.
INSERT INTO [dbo].[person_type] (id, description, status, date_created, is_system) VALUES (1, 'Administrador General', 1, GETDATE(), 1)
GO
INSERT INTO [dbo].[person_type] (id, description, status, date_created, is_system) VALUES (2, 'Administrador', 1, GETDATE(), 1)
GO
INSERT INTO [dbo].[person_type] (id, description, status, date_created, is_system) VALUES (3, 'Empleado', 1, GETDATE(), 1)
GO
INSERT INTO [dbo].[person_type] (id, description, status, date_created, is_system) VALUES (4, 'Cliente', 1, GETDATE(), 1)
GO

-- Permission catalogue (section.action). Each section has a '<section>.acceso' root
-- (parent_code IS NULL); the rest hang off it, and each report "exportar" hangs off its "ver".
INSERT INTO [dbo].[permission] (code, section, description, parent_code) VALUES
    ('ventas.acceso',           'ventas',      'Usar el punto de venta',                   NULL),
    ('compras.acceso',          'compras',     'Registrar compras a proveedores',          NULL),
    ('clientes.acceso',         'clientes',    'Ver la seccion de clientes',               NULL),
    ('clientes.gestionar',      'clientes',    'Crear, editar y eliminar clientes',        'clientes.acceso'),
    ('proveedores.acceso',      'proveedores', 'Ver la seccion de proveedores',            NULL),
    ('proveedores.gestionar',   'proveedores', 'Crear, editar y eliminar proveedores',     'proveedores.acceso'),
    ('productos.acceso',        'productos',   'Ver la seccion de productos',              NULL),
    ('productos.gestionar',     'productos',   'Crear y editar productos',                 'productos.acceso'),
    ('productos.editar_precios','productos',   'Modificar precios de compra y de venta',   'productos.acceso'),
    ('productos.eliminar',      'productos',   'Eliminar o dar de baja productos',         'productos.acceso'),
    ('categorias.acceso',       'categorias',  'Ver la seccion de categorias',            NULL),
    ('categorias.gestionar',    'categorias',  'Crear, editar y eliminar categorias',      'categorias.acceso'),
    ('tienda.acceso',           'tienda',      'Ver los datos de la tienda',               NULL),
    ('tienda.editar',           'tienda',      'Modificar nombre, datos fiscales y moneda','tienda.acceso'),
    ('usuarios.acceso',         'usuarios',    'Ver la seccion de usuarios',               NULL),
    ('usuarios.gestionar',      'usuarios',    'Crear, editar y eliminar usuarios',        'usuarios.acceso'),
    ('roles.gestionar',         'usuarios',    'Administrar roles y sus permisos',         'usuarios.acceso'),
    ('reportes.acceso',            'reportes', 'Abrir la seccion de reportes',             NULL),
    ('reportes.ventas',            'reportes', 'Ver el reporte de ventas',                 'reportes.acceso'),
    ('reportes.ventas.exportar',   'reportes', 'Exportar el reporte de ventas',            'reportes.ventas'),
    ('reportes.compras',           'reportes', 'Ver el reporte de compras',                'reportes.acceso'),
    ('reportes.compras.exportar',  'reportes', 'Exportar el reporte de compras',           'reportes.compras'),
    ('reportes.productos',         'reportes', 'Ver el reporte de productos',              'reportes.acceso'),
    ('reportes.productos.exportar','reportes', 'Exportar el reporte de productos',         'reportes.productos'),
    ('reportes.alertas',           'reportes', 'Ver el historial de alertas',              'reportes.acceso'),
    ('reportes.alertas.exportar',  'reportes', 'Exportar el historial de alertas',         'reportes.alertas'),
    ('alertas.acceso',          'alertas',     'Ver el centro de notificaciones',          NULL),
    ('alertas.reconocer',       'alertas',     'Reconocer alertas de inventario',          'alertas.acceso'),
    ('alertas.silenciar',       'alertas',     'Silenciar alertas puntuales',              'alertas.acceso'),
    ('alertas.configurar',      'alertas',     'Cambiar los umbrales de alerta',           'alertas.acceso')
GO

-- Seed role_permission so the four built-in roles behave exactly as before this feature:
--   1 Administrador General -> everything
--   2 Administrador         -> everything except the Tienda section and roles.gestionar
--                              (only the Administrador General administers roles, so a regular
--                              Administrador cannot re-permission its own role past the Tienda
--                              boundary)
--   3 Empleado              -> Ventas, Clientes and Alertas (view + acknowledge/mute)
--   4 Cliente               -> nothing (cannot sign in)
INSERT INTO [dbo].[role_permission] (person_type_id, permission_id)
    SELECT 1, id FROM [dbo].[permission]
GO
INSERT INTO [dbo].[role_permission] (person_type_id, permission_id)
    SELECT 2, id FROM [dbo].[permission] WHERE section <> 'tienda' AND code <> 'roles.gestionar'
GO
INSERT INTO [dbo].[role_permission] (person_type_id, permission_id)
    SELECT 3, id FROM [dbo].[permission]
    WHERE code IN ('ventas.acceso', 'clientes.acceso', 'clientes.gestionar',
                   'alertas.acceso', 'alertas.reconocer', 'alertas.silenciar')
GO

-- Default Administrador General account so a fresh database has someone who can log in and
-- reach the Tienda tab (person_type 1) right away. Plain-text password on purpose: LoginPresenter
-- (VerifyPassword) accepts a plain-text match on first login and rewrites it as a hash
-- immediately after, the same legacy migration path every pre-existing account went through.
IF NOT EXISTS (SELECT 1 FROM [dbo].[person] WHERE document_number = '1010101010')
BEGIN
    INSERT INTO [dbo].[person] (document_number, name, address, phone, password, person_type_id, status, date_created)
    VALUES ('1010101010', 'Administrador General', 'N/A', 'N/A', '12345678', 1, 1, GETDATE())
END
GO

-- Single-row config tables. Without these rows a fresh database can never persist the store
-- profile (UpdateStoreRow only UPDATEs id = 1) nor the alert thresholds
-- (sp_update_notificacion_settings only UPDATEs id = 1), and the alert queries read 0/0.
IF NOT EXISTS (SELECT 1 FROM [dbo].[store] WHERE id = 1)
BEGIN
    INSERT INTO [dbo].[store] (id, document_store, company_name, email, phone, address, currency_culture)
    VALUES (1, '', 'Mi Farmacia', '', '', '', 'es-EC')
END
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[notification_settings] WHERE id = 1)
BEGIN
    INSERT INTO [dbo].[notification_settings] (id, critical_stock, notify_day)
    VALUES (1, 10, 30)
END
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
@code VARCHAR(50),
@name VARCHAR(50),
@description VARCHAR(500),
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

-- QUOTED_IDENTIFIER must be ON at CREATE time for any procedure that modifies a table backed by
-- a filtered index (see ix_product_status_expired above) - SQL Server bakes that setting into the
-- compiled procedure, so it does not matter what the caller's session has at execution time.
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_delete_product]
    @id_product INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    -- product_alert_history also has an FK to product (added in the alerts rework): a product
    -- that only ever triggered an alert would fail the physical DELETE without this check.
    IF NOT EXISTS (SELECT 1 FROM purchase_detail WHERE product_id = @id_product)
       AND NOT EXISTS (SELECT 1 FROM sale_detail WHERE product_id = @id_product)
       AND NOT EXISTS (SELECT 1 FROM product_alert_history WHERE product_id = @id_product)
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

CREATE PROCEDURE [dbo].[sp_delete_person]
    @id_person INT,
    @result BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    -- Same soft-delete pattern as products/categories: a person referenced by a sale, a
    -- purchase or an acknowledged alert cannot be physically removed (FK), so deactivate them
    -- instead. LoginPresenter must reject status = 0 so a former employee cannot sign in.
    IF NOT EXISTS (SELECT 1 FROM sale WHERE user_id = @id_person)
       AND NOT EXISTS (SELECT 1 FROM purchase WHERE person_id = @id_person)
       AND NOT EXISTS (SELECT 1 FROM product_alert_history WHERE acknowledged_by = @id_person)
    BEGIN
        DELETE FROM person WHERE id = @id_person;
        SET @result = 1;
    END
    ELSE
    BEGIN
        UPDATE person SET status = 0 WHERE id = @id_person;
        SET @result = 1;
    END
END
GO

-- Roles admin (frmRoles). Custom roles use ids >= 100; the four built-ins (is_system = 1)
-- can have their permission set edited but cannot be renamed or deleted.

CREATE PROCEDURE [dbo].[sp_set_role_permissions]
    @person_type_id INT,
    @permission_ids VARCHAR(MAX)   -- comma-separated permission ids, may be empty
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
        DELETE FROM role_permission WHERE person_type_id = @person_type_id;

        INSERT INTO role_permission (person_type_id, permission_id)
        SELECT DISTINCT @person_type_id, TRY_CONVERT(INT, value)
        FROM STRING_SPLIT(ISNULL(@permission_ids, ''), ',')
        WHERE TRY_CONVERT(INT, value) IN (SELECT id FROM permission);
    COMMIT TRANSACTION;
END
GO

CREATE PROCEDURE [dbo].[sp_create_person_type]
    @description VARCHAR(50),
    @result INT OUTPUT            -- new id, or 0 if the description already exists
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF NOT EXISTS (SELECT 1 FROM person_type WHERE UPPER(description) = UPPER(@description))
    BEGIN
        DECLARE @newId INT = (SELECT ISNULL(MAX(id), 99) + 1 FROM person_type WHERE id >= 100);
        IF @newId < 100 SET @newId = 100;
        INSERT INTO person_type (id, description, status, date_created, is_system)
        VALUES (@newId, @description, 1, GETDATE(), 0);
        SET @result = @newId;
    END
END
GO

CREATE PROCEDURE [dbo].[sp_update_person_type]
    @id INT,
    @description VARCHAR(50),
    @result BIT OUTPUT            -- 0 if it is a system role or the name is taken
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF EXISTS (SELECT 1 FROM person_type WHERE id = @id AND is_system = 0)
       AND NOT EXISTS (SELECT 1 FROM person_type WHERE UPPER(description) = UPPER(@description) AND id <> @id)
    BEGIN
        UPDATE person_type SET description = @description WHERE id = @id;
        SET @result = 1;
    END
END
GO

CREATE PROCEDURE [dbo].[sp_delete_person_type]
    @id INT,
    @result BIT OUTPUT            -- 0 if it is a system role or still has users assigned
AS
BEGIN
    SET NOCOUNT ON;
    SET @result = 0;
    IF EXISTS (SELECT 1 FROM person_type WHERE id = @id AND is_system = 0)
       AND NOT EXISTS (SELECT 1 FROM person WHERE person_type_id = @id)
    BEGIN
        DELETE FROM person_type WHERE id = @id;   -- role_permission rows cascade
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
    -- Case-insensitive duplicate check, same criterion sp_create_category uses.
    IF NOT EXISTS (SELECT * FROM category WHERE UPPER(description) = UPPER(@description) AND id != @category_id)
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
        INSERT INTO notification_settings (id, critical_stock, notify_day) VALUES (1, @critical_stock, @notify_day)
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
        -- A NULL @password means "keep the current one" - the caller sends NULL when the edit
        -- form leaves the password field blank, so an unrelated edit never rehashes/clears it.
        UPDATE person SET
            document_number = @document,
            name = @name,
            address = @address,
            phone = @phone,
            password = ISNULL(@password, password),
            person_type_id = @person_type_id
        WHERE id = @id_person
    ELSE
        SET @result = 0
END
GO

CREATE PROCEDURE [dbo].[sp_update_product](
@id_product INT,
@code VARCHAR(50),
@name VARCHAR(50),
@description VARCHAR(500),
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
