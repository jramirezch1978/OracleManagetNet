# Mejoras en el Sistema de Logging - Información de IPs

## 🔍 Información Capturada

El sistema ahora captura y registra la siguiente información del cliente:

### 1. **Usuario Windows**
- Nombre de usuario autenticado en Windows/Active Directory
- Formato: `DOMINIO\usuario` o `MÁQUINA\usuario`

### 2. **Nombre de Máquina**
- Nombre del equipo cliente desde donde se realiza la conexión

### 3. **IP Local (IPv4)**
- Dirección IP v4 de la tarjeta de red activa
- Ignora direcciones loopback (127.0.0.1)
- Muestra la IP real de la red local (ej: 192.168.1.100)

### 4. **IP Pública**
- Dirección IP pública del cliente
- Se obtiene consultando servicios externos:
  - api.ipify.org
  - icanhazip.com  
  - ifconfig.me
- Útil para auditoría y rastreo de conexiones remotas

### 5. **Dirección MAC**
- Dirección física de la tarjeta de red activa
- Formato: XX:XX:XX:XX:XX:XX
- Identificación única del dispositivo

### 6. **Interfaz de Red**
- Nombre de la interfaz de red utilizada
- Ej: "Ethernet", "Wi-Fi", etc.

## 📋 Formato de Log Mejorado

### Antes (con IPv6 localhost):
```
=== INTENTO DE CONEXIÓN A ORACLE ===
Usuario Windows: JRAMIREZ-ELITEB\jramirez
IP Cliente: ::1
```

### Ahora (con información completa):
```
=== INTENTO DE CONEXIÓN A ORACLE ===
Timestamp: 2025-08-16 01:00:00.123
Usuario Windows: JRAMIREZ-ELITEB\jramirez
Información del Cliente: Usuario: JRAMIREZ-ELITEB\jramirez, Máquina: JRAMIREZ-ELITEB, IP Local: 192.168.1.100, IP Pública: 201.123.45.67, MAC: A0:B1:C2:D3:E4:F5, Interfaz: Ethernet
--- Parámetros de Conexión ---
Host: 192.168.0.159
Puerto: 1521
Servicio/SID: hades
Usuario Oracle: jarch
Connection String: (DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.159)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=hades)))
```

## 🔧 Implementación Técnica

### Clase ClientInfo
```csharp
public class ClientInfo
{
    public string UserName { get; set; }
    public string MachineName { get; set; }
    public string LocalIpV4 { get; set; }
    public string PublicIp { get; set; }
    public string MacAddress { get; set; }
    public string NetworkInterfaceName { get; set; }
}
```

### Métodos de Obtención

1. **IP Local IPv4**:
   - Enumera todas las interfaces de red
   - Busca direcciones IPv4 no loopback
   - Prioriza interfaces activas

2. **IP Pública**:
   - Consulta asíncrona a servicios externos
   - Timeout de 5 segundos
   - Fallback a múltiples servicios

3. **MAC Address**:
   - Obtiene la dirección física de la interfaz activa
   - Formateada con separadores ":"

## 🛡️ Beneficios para Seguridad

1. **Auditoría Completa**: 
   - Rastreo exacto del origen de conexiones
   - Identificación de patrones sospechosos

2. **Detección de Anomalías**:
   - Cambios inesperados en IP/MAC
   - Conexiones desde ubicaciones no autorizadas

3. **Cumplimiento**:
   - Registro detallado para auditorías
   - Evidencia forense en caso de incidentes

## 📊 Análisis de Logs

Con esta información se puede:

1. **Identificar Patrones**:
   - Usuarios conectándose desde múltiples ubicaciones
   - Horarios inusuales de conexión
   - IPs públicas sospechosas

2. **Generar Reportes**:
   - Conexiones por IP pública/país
   - Usuarios más activos
   - Dispositivos únicos (por MAC)

3. **Alertas Automáticas**:
   - Conexiones desde IPs no conocidas
   - Múltiples intentos fallidos desde una IP
   - Cambios de MAC address para un usuario

## 🚀 Uso

La información se registra automáticamente en:
- `Logs/oracle-dbmanager-YYYYMMDD.log` (todos los eventos)
- `Logs/oracle-errors-YYYYMMDD.log` (solo errores)

Para ver los logs en tiempo real:
```powershell
Get-Content -Path "Logs\oracle-dbmanager-$(Get-Date -Format 'yyyyMMdd').log" -Tail 50 -Wait
```
