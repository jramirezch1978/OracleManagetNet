# Sistema de Logging de Oracle Database Manager

## 📁 Estructura de Logs

Los logs se almacenan en la carpeta `Logs/` con la siguiente estructura:

- **oracle-dbmanager-YYYYMMDD.log**: Log general con toda la actividad
- **oracle-errors-YYYYMMDD.log**: Solo errores y excepciones

## 🔍 Información Registrada

### 1. **Intento de Conexión**
```
=== INTENTO DE CONEXIÓN A ORACLE ===
Timestamp: 2025-08-16 10:30:45.123
Usuario Windows: DOMINIO\usuario
Información del Cliente: Usuario: DOMINIO\usuario, Máquina: PC-USUARIO, IP Local: 192.168.1.100, IP Pública: 201.123.45.67, MAC: A0:B1:C2:D3:E4:F5, Interfaz: Ethernet
--- Parámetros de Conexión ---
Host: 192.168.0.159
Puerto: 1521
Servicio/SID: hades
Usuario Oracle: jarch
Connection String: (DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.159)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=hades)))
```

### 2. **Conexión Exitosa**
```
=== CONEXIÓN EXITOSA ===
Timestamp: 2025-08-16 10:30:47.456
Usuario Windows: DOMINIO\usuario
Información del Cliente: Usuario: DOMINIO\usuario, Máquina: PC-USUARIO, IP Local: 192.168.1.100, IP Pública: 201.123.45.67, MAC: A0:B1:C2:D3:E4:F5, Interfaz: Ethernet
Servidor: 192.168.0.159:1521
Servicio: hades
Usuario Oracle: jarch
```

### 3. **Error de Conexión Detallado**
```
=== ERROR DE CONEXIÓN A ORACLE ===
Timestamp: 2025-08-16 10:30:47.456
Usuario Windows: DOMINIO\usuario
Información del Cliente: Usuario: DOMINIO\usuario, Máquina: PC-USUARIO, IP Local: 192.168.1.100, IP Pública: 201.123.45.67, MAC: A0:B1:C2:D3:E4:F5, Interfaz: Ethernet
--- Parámetros de Conexión ---
Host: 192.168.0.159
Puerto: 1521
Servicio/SID: hades
Usuario Oracle: jarch
Connection String: (DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.159)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=hades)))
--- Detalles del Error ---
Tipo de Error: Oracle.ManagedDataAccess.Client.OracleException
Mensaje: ORA-12541: TNS: no hay ningún listener
Oracle Error Number: 12541
Oracle Error Code: -2147467259
Oracle Source: Oracle Data Provider for .NET
--- Análisis del Error ---
DIAGNÓSTICO: El listener de Oracle no está activo o no es accesible.
POSIBLES SOLUCIONES:
1. Verificar que el servicio Oracle Listener esté ejecutándose en el servidor
2. Verificar conectividad de red al puerto 1521
3. Verificar firewall/cortafuegos
--- Stack Trace ---
[Stack trace completo...]
```

## 📊 Tipos de Errores y Diagnósticos

### ORA-12541: TNS: no hay ningún listener
- **Diagnóstico**: El listener de Oracle no está activo o no es accesible
- **Soluciones**:
  1. Verificar que el servicio Oracle Listener esté ejecutándose
  2. Verificar conectividad de red al puerto 1521
  3. Verificar firewall/cortafuegos

### ORA-12514: TNS: el listener no conoce el servicio
- **Diagnóstico**: El servicio Oracle especificado no existe o no está registrado
- **Soluciones**:
  1. Verificar el nombre del servicio/SID
  2. Ejecutar 'lsnrctl services' en el servidor Oracle

### ORA-01017: Usuario/contraseña inválidos
- **Diagnóstico**: Credenciales inválidas
- **Soluciones**:
  1. Verificar usuario y contraseña
  2. Verificar que el usuario no esté bloqueado
  3. Verificar mayúsculas/minúsculas

### ORA-28000: La cuenta está bloqueada
- **Diagnóstico**: La cuenta de usuario está bloqueada
- **Soluciones**:
  1. Desbloquear usuario con: ALTER USER usuario ACCOUNT UNLOCK
  2. Contactar al DBA

## ⚙️ Configuración de Logging

### Niveles de Log
- **Information**: Operaciones normales
- **Warning**: Situaciones anormales pero no críticas
- **Error**: Errores que requieren atención
- **Fatal**: Errores que causan que la aplicación se detenga

### Retención de Logs
- Logs generales: 30 días
- Logs de errores: 90 días
- Tamaño máximo por archivo: 50MB
- Rotación: Diaria

## 📍 Ubicación de Archivos

```
OracleDBManager/
├── OracleDBManager.Web/
│   └── Logs/
│       ├── oracle-dbmanager-20240816.log
│       ├── oracle-errors-20240816.log
│       └── ...
```

## 🔧 Uso en Desarrollo

Para ver los logs en tiempo real durante el desarrollo:

```powershell
# Ver log general
Get-Content -Path "Logs\oracle-dbmanager-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50 -Wait

# Ver solo errores
Get-Content -Path "Logs\oracle-errors-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50 -Wait
```

## 🚨 Monitoreo de Errores

Los administradores deben revisar regularmente:
1. El archivo de errores para identificar problemas recurrentes
2. Patrones de errores de conexión por usuario/IP
3. Intentos de conexión fallidos frecuentes

## 📈 Análisis de Logs

Para analizar los logs se recomienda:
1. Usar herramientas como LogParser o ELK Stack
2. Configurar alertas para errores críticos
3. Generar reportes mensuales de actividad
