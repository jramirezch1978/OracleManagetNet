using Microsoft.Extensions.Logging;
using OracleDBManager.Core.Interfaces;
using OracleDBManager.Core.Models;
using System.Text;

namespace OracleDBManager.Services.Implementation
{
    public class ConnectionLogService : IConnectionLogService
    {
        private readonly ILogger<ConnectionLogService> _logger;
        
        public ConnectionLogService(ILogger<ConnectionLogService> logger)
        {
            _logger = logger;
        }
        
        public Task LogConnectionAttemptAsync(ConnectionTestModel connectionModel, string userName, string clientIp)
        {
            var logMessage = new StringBuilder();
            logMessage.AppendLine("=== INTENTO DE CONEXIÓN A ORACLE ===");
            logMessage.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            logMessage.AppendLine($"Usuario Windows: {userName}");
            logMessage.AppendLine($"Información del Cliente: {clientIp}");
            logMessage.AppendLine("--- Parámetros de Conexión ---");
            logMessage.AppendLine($"Host: {connectionModel.Host}");
            logMessage.AppendLine($"Puerto: {connectionModel.Port}");
            logMessage.AppendLine($"Servicio/SID: {connectionModel.ServiceName}");
            logMessage.AppendLine($"Usuario Oracle: {connectionModel.UserId}");
            logMessage.AppendLine($"Connection String: {connectionModel.GetConnectionString()}");
            
            _logger.LogInformation(logMessage.ToString());
            
            return Task.CompletedTask;
        }
        
        public Task LogConnectionSuccessAsync(ConnectionTestModel connectionModel, string userName, string clientIp)
        {
            var logMessage = new StringBuilder();
            logMessage.AppendLine("=== CONEXIÓN EXITOSA ===");
            logMessage.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            logMessage.AppendLine($"Usuario Windows: {userName}");
            logMessage.AppendLine($"Información del Cliente: {clientIp}");
            logMessage.AppendLine($"Servidor: {connectionModel.Host}:{connectionModel.Port}");
            logMessage.AppendLine($"Servicio: {connectionModel.ServiceName}");
            logMessage.AppendLine($"Usuario Oracle: {connectionModel.UserId}");
            
            _logger.LogInformation(logMessage.ToString());
            
            return Task.CompletedTask;
        }
        
        public Task LogConnectionErrorAsync(ConnectionTestModel connectionModel, string userName, string clientIp, Exception exception)
        {
            var logMessage = new StringBuilder();
            logMessage.AppendLine("=== ERROR DE CONEXIÓN A ORACLE ===");
            logMessage.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            logMessage.AppendLine($"Usuario Windows: {userName}");
            logMessage.AppendLine($"Información del Cliente: {clientIp}");
            logMessage.AppendLine("--- Parámetros de Conexión ---");
            logMessage.AppendLine($"Host: {connectionModel.Host}");
            logMessage.AppendLine($"Puerto: {connectionModel.Port}");
            logMessage.AppendLine($"Servicio/SID: {connectionModel.ServiceName}");
            logMessage.AppendLine($"Usuario Oracle: {connectionModel.UserId}");
            logMessage.AppendLine($"Connection String: {connectionModel.GetConnectionString()}");
            
            logMessage.AppendLine("--- Detalles del Error ---");
            logMessage.AppendLine($"Tipo de Error: {exception.GetType().FullName}");
            logMessage.AppendLine($"Mensaje: {exception.Message}");
            
            if (exception is Oracle.ManagedDataAccess.Client.OracleException oracleEx)
            {
                logMessage.AppendLine($"Oracle Error Number: {oracleEx.Number}");
                logMessage.AppendLine($"Oracle Error Code: {oracleEx.ErrorCode}");
                logMessage.AppendLine($"Oracle Source: {oracleEx.Source}");
                logMessage.AppendLine($"Oracle Procedure: {oracleEx.Procedure}");
                
                // Análisis del error
                logMessage.AppendLine("--- Análisis del Error ---");
                
                if (oracleEx.Message.Contains("ORA-12541"))
                {
                    logMessage.AppendLine("DIAGNÓSTICO: El listener de Oracle no está activo o no es accesible.");
                    logMessage.AppendLine("POSIBLES SOLUCIONES:");
                    logMessage.AppendLine("1. Verificar que el servicio Oracle Listener esté ejecutándose en el servidor");
                    logMessage.AppendLine("2. Verificar conectividad de red al puerto 1521");
                    logMessage.AppendLine("3. Verificar firewall/cortafuegos");
                }
                else if (oracleEx.Message.Contains("ORA-12514"))
                {
                    logMessage.AppendLine("DIAGNÓSTICO: El servicio Oracle especificado no existe o no está registrado.");
                    logMessage.AppendLine("POSIBLES SOLUCIONES:");
                    logMessage.AppendLine("1. Verificar el nombre del servicio/SID");
                    logMessage.AppendLine("2. Ejecutar 'lsnrctl services' en el servidor Oracle");
                }
                else if (oracleEx.Message.Contains("ORA-01017"))
                {
                    logMessage.AppendLine("DIAGNÓSTICO: Credenciales inválidas.");
                    logMessage.AppendLine("POSIBLES SOLUCIONES:");
                    logMessage.AppendLine("1. Verificar usuario y contraseña");
                    logMessage.AppendLine("2. Verificar que el usuario no esté bloqueado");
                    logMessage.AppendLine("3. Verificar mayúsculas/minúsculas");
                }
                else if (oracleEx.Message.Contains("ORA-28000"))
                {
                    logMessage.AppendLine("DIAGNÓSTICO: La cuenta de usuario está bloqueada.");
                    logMessage.AppendLine("POSIBLES SOLUCIONES:");
                    logMessage.AppendLine("1. Desbloquear usuario con: ALTER USER usuario ACCOUNT UNLOCK");
                    logMessage.AppendLine("2. Contactar al DBA");
                }
            }
            
            if (exception.InnerException != null)
            {
                logMessage.AppendLine("--- Inner Exception ---");
                logMessage.AppendLine($"Tipo: {exception.InnerException.GetType().FullName}");
                logMessage.AppendLine($"Mensaje: {exception.InnerException.Message}");
                
                if (exception.InnerException.InnerException != null)
                {
                    logMessage.AppendLine("--- Inner Inner Exception ---");
                    logMessage.AppendLine($"Tipo: {exception.InnerException.InnerException.GetType().FullName}");
                    logMessage.AppendLine($"Mensaje: {exception.InnerException.InnerException.Message}");
                }
            }
            
            logMessage.AppendLine("--- Stack Trace ---");
            logMessage.AppendLine(exception.StackTrace);
            
            _logger.LogError(exception, logMessage.ToString());
            
            return Task.CompletedTask;
        }
    }
}
