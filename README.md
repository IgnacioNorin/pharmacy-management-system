# 💊 Pharmacy Management System

Sistema de gestión integral para farmacias que incluye punto de venta (POS), control de inventario, gestión de clientes, proveedores y reportes.

## 🚀 Características

- **Punto de Venta (POS):** Registro rápido de ventas y gestión de transacciones
- **Inventario:** Control de stock, alertas de productos bajos
- **Clientes:** Base de datos de clientes y historial de compras
- **Proveedores:** Gestión de proveedores y órdenes de compra
- **Reportes:** Reportes de ventas, inventario y análisis
- **Usuarios:** Sistema de autenticación y roles

## 🛠️ Tecnologías

- **Framework:** .NET Framework 4.8
- **Lenguaje:** C#
- **UI:** Windows Forms
- **Base de datos:** SQL Server 2019

## 📋 Requisitos

- Visual Studio 2019 o superior
- .NET Framework 4.8
- SQL Server 2019

## 🔧 Instalación

1. Clona el repositorio:
```bash
git clone https://github.com/[tu-usuario]/pharmacy-management-system.git
```

2. Ejecuta el script de inicialización ubicado en /Database/Schema-Pharmacy.sql en SQL Server

3. Abre la solución en Visual Studio:

PharmacySystem.sln

4. Restaura los paquetes NuGet

5. Configura la cadena de conexión en App.config:

	<connectionStrings>
		<add name="connection" 
			 connectionString="Server=#HereYourServer#;database=PharmacyDB;integrated security=true;" 
			 providerName="System.Data.SqlClient"/>
	</connectionStrings>

6. Ejecuta el proyecto (F5)

🔐 Credenciales por Defecto
El sistema incluye un usuario de prueba que puedes modificar o eliminar:

	Número: 10101010
	Contraseña: 123
⚠️ Importante: Cambia estas credenciales antes de usar en producción

📄 Licencia
Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE para más detalles.
👤 Autor
Ignacio Norín

GitHub: @IgnacioNorin