/**Este script genera la tabla Clientes.
**No es necesario usarlo si se utiliza la migración de Entity Framework dentro del proyecto
**Si se desea agregar la tabla con EF, utilizar el siguiente comando (siendo MainMigration el nombre de la migración),
**dentro de la carpeta del proyecto:
**
**dotnet ef database update MainMigration
**
**Si se desea generar una migración nueva, utilizar:
**
**dotnet ef migrations add MyNewMigration
**
**Tener en cuenta en editar la connectionString (o utilizar una nueva) dentro de appsettings.json
**/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Clientes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [nvarchar](max) NOT NULL,
	[Apellidos] [nvarchar](max) NOT NULL,
	[FechaNacimiento] [date] NOT NULL,
	[CUIT] [nvarchar](max) NOT NULL,
	[Domicilio] [nvarchar](max) NULL,
	[Celular] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NOT NULL,
	[Deshabilitado] [bit] NOT NULL,
 CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Clientes] ADD  DEFAULT (CONVERT([bit],(0))) FOR [Deshabilitado]
GO


