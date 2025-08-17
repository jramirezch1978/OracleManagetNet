# Oracle Database Manager

Una aplicación web similar a Oracle Enterprise Manager para gestionar y monitorear bases de datos Oracle, desarrollada en .NET 9.0.

## 🚀 Características

- **Gestión de Bloqueos**: Visualización y administración de bloqueos de base de datos
- **Monitoreo en Tiempo Real**: Actualización automática cada 15 segundos
- **Terminación de Sesiones**: Capacidad para matar sesiones problemáticas
- **Autenticación Windows**: Integración con Active Directory
- **Interfaz Familiar**: Diseño similar a Oracle Enterprise Manager

## 📋 Requisitos Previos

- Windows Server 2016 o superior
- IIS 10 o superior
- .NET 9.0 Runtime
- ASP.NET Core Hosting Bundle
- Oracle Client o Oracle Instant Client
- Acceso a base de datos Oracle con privilegios DBA

## 🚀 Desarrollo

### Ejecutar en Modo Desarrollo

```powershell
# Navegar al proyecto
cd OracleDBManager

# Compilar la solución
dotnet build

# Ejecutar la aplicación
cd OracleDBManager.Web
dotnet run --urls "http://localhost:5220"
```

La aplicación estará disponible en: http://localhost:5220

## 🔧 Instalación en Producción

### 1. Preparar el Servidor

```powershell
# Instalar IIS y características necesarias
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpRedirect, IIS-ApplicationDevelopment, IIS-NetFxExtensibility45, IIS-HealthAndDiagnostics, IIS-HttpLogging, IIS-Security, IIS-RequestFiltering, IIS-WindowsAuthentication, IIS-Performance, IIS-WebServerManagementTools, IIS-IIS6ManagementCompatibility, IIS-Metabase -All

# Instalar ASP.NET Core Hosting Bundle
# Descargar desde: https://dotnet.microsoft.com/download/dotnet/9.0
```

### 2. Configurar Oracle Client

1. Instalar Oracle Instant Client o Oracle Client completo
2. Configurar las variables de entorno:
   - `ORACLE_HOME`: Ruta al cliente Oracle
   - `TNS_ADMIN`: Ruta al archivo tnsnames.ora
   - Agregar `%ORACLE_HOME%\bin` al PATH

### 3. Publicar la Aplicación

```powershell
# Ejecutar como administrador
cd OracleDBManager
.\publish-to-iis.ps1
```

### 4. Configuración Manual en IIS (si es necesario)

1. Crear un nuevo Application Pool:
   - Nombre: `OracleDBManager`
   - .NET CLR Version: `No Managed Code`
   - Managed Pipeline Mode: `Integrated`
   - Identity: Cuenta con permisos Oracle

2. Crear el sitio web:
   - Nombre: `OracleDBManager`
   - Puerto: `8080` (o el deseado)
   - Application Pool: `OracleDBManager`
   - Physical Path: `C:\inetpub\wwwroot\OracleDBManager`

3. Configurar autenticación:
   - Deshabilitar autenticación anónima
   - Habilitar autenticación Windows

## ⚙️ Configuración

### Conexión a Base de Datos

Editar `appsettings.json`:

```json
{
  "OracleConfiguration": {
    "DataSource": "(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.159)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=hades)))",
    "UserId": "jarch",
    "Password": "jarch",
    "ConnectionTimeout": 30,
    "CommandTimeout": 120
  }
}
```

### Permisos de Oracle

El usuario de base de datos necesita los siguientes permisos:

```sql
-- Permisos mínimos requeridos
GRANT SELECT ON v$lock TO jarch;
GRANT SELECT ON v$session TO jarch;
GRANT SELECT ON v$process TO jarch;
GRANT SELECT ON v$sql TO jarch;
GRANT SELECT ON dba_objects TO jarch;
GRANT ALTER SYSTEM TO jarch;  -- Para poder matar sesiones
```

## 🖥️ Uso

1. Acceder a la aplicación: `http://servidor:8080`
2. Autenticarse con credenciales de Windows
3. Navegar a la sección "Bloqueos" para ver los bloqueos activos
4. Utilizar las opciones disponibles:
   - **Actualizar**: Refrescar manualmente la lista
   - **Matar Sesión**: Terminar sesiones seleccionadas
   - **Detalles**: Ver información detallada de una sesión

## 🔍 Solución de Problemas

### La aplicación no inicia

1. Verificar que el Application Pool esté iniciado
2. Revisar los logs en `C:\inetpub\wwwroot\OracleDBManager\logs`
3. Verificar Event Viewer de Windows

### Error de conexión a Oracle

1. Verificar conectividad: `tnsping hades`
2. Probar conexión: `sqlplus jarch/jarch@hades`
3. Verificar que el usuario del Application Pool tenga acceso al Oracle Client

### Error de autenticación

1. Verificar que Windows Authentication esté habilitado en IIS
2. Verificar que el usuario tenga permisos en la aplicación
3. Revisar la configuración de Active Directory

## 📂 Estructura del Proyecto

```
OracleDBManager/
├── OracleDBManager.Core/          # Modelos e interfaces
├── OracleDBManager.Infrastructure/# Acceso a datos
├── OracleDBManager.Services/      # Lógica de negocio
├── OracleDBManager.Web/           # Aplicación Blazor
└── OracleDBManager.Tests/         # Pruebas unitarias
```

## 🛠️ Desarrollo

### Requisitos de Desarrollo

- Visual Studio 2022 o VS Code
- .NET 9.0 SDK
- Oracle Developer Tools (opcional)

### Ejecutar en Desarrollo

```bash
cd OracleDBManager
dotnet restore
dotnet build
cd OracleDBManager.Web
dotnet run
```

La aplicación estará disponible en `https://localhost:5001`

## 📝 Licencia

Este proyecto es de uso interno.

## 👥 Soporte

Para soporte o preguntas, contactar al equipo de desarrollo.
