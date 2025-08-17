using OracleDBManager.Core.Interfaces;
using OracleDBManager.Core.Models;
using Microsoft.Extensions.Logging;

namespace OracleDBManager.Services.Implementation
{
    public class LockService : ILockService
    {
        private readonly ILockRepository _lockRepository;
        private readonly ILogger<LockService> _logger;
        
        public LockService(ILockRepository lockRepository, ILogger<LockService> logger)
        {
            _lockRepository = lockRepository;
            _logger = logger;
        }
        
        public async Task<List<SessionLock>> GetAllLocksAsync(bool includeMRLocks = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los bloqueos de la base de datos");
                var locks = await _lockRepository.GetAllLocksAsync();
                
                if (!includeMRLocks)
                {
                    // Filtrar bloqueos MR (Media Recovery) si no se solicitan
                    locks = locks.Where(l => l.LockType != "MR").ToList();
                }
                
                _logger.LogInformation($"Se encontraron {locks.Count} bloqueos");
                return locks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los bloqueos");
                throw;
            }
        }
        
        public async Task<List<SessionLock>> GetBlockingLocksAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo bloqueos bloqueantes");
                return await _lockRepository.GetBlockingLocksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener bloqueos bloqueantes");
                throw;
            }
        }
        
        public async Task<List<BlockingChain>> GetBlockingChainsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo cadenas de bloqueo");
                return await _lockRepository.GetBlockingChainsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cadenas de bloqueo");
                throw;
            }
        }
        
        public async Task<SessionDetail> GetSessionDetailAsync(int sessionId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo detalles de la sesión {sessionId}");
                return await _lockRepository.GetSessionDetailAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalles de la sesión {sessionId}");
                throw;
            }
        }
        
        public async Task<(bool success, string message)> KillSessionAsync(int sessionId, int serialNumber, string username)
        {
            try
            {
                _logger.LogWarning($"Usuario {username} está intentando matar la sesión {sessionId},{serialNumber}");
                
                // Verificar que la sesión existe antes de intentar matarla
                var sessionDetail = await _lockRepository.GetSessionDetailAsync(sessionId);
                if (sessionDetail == null)
                {
                    return (false, "La sesión no existe o ya fue terminada");
                }
                
                var result = await _lockRepository.KillSessionAsync(sessionId, serialNumber);
                
                if (result)
                {
                    _logger.LogInformation($"Sesión {sessionId},{serialNumber} terminada exitosamente por {username}");
                    return (true, "Sesión terminada exitosamente");
                }
                else
                {
                    _logger.LogError($"No se pudo terminar la sesión {sessionId},{serialNumber}");
                    return (false, "No se pudo terminar la sesión. Verifique los permisos.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al intentar matar la sesión {sessionId},{serialNumber}");
                return (false, $"Error: {ex.Message}");
            }
        }
        
        public async Task<bool> TestDatabaseConnectionAsync()
        {
            _logger.LogInformation("Probando conexión a la base de datos");
            try
            {
                return await _lockRepository.TestConnectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar la conexión a la base de datos");
                throw; // Propagar la excepción para que podamos ver el error real
            }
        }
        
        public async Task<List<SessionInfo>> GetAllSessionsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las sesiones activas");
                var sessions = await _lockRepository.GetAllSessionsAsync();
                _logger.LogInformation($"Se encontraron {sessions.Count} sesiones activas");
                return sessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las sesiones");
                throw;
            }
        }
        
        public async Task<List<SessionSqlHistory>> GetSessionSqlHistoryAsync(int sessionId, string? username)
        {
            try
            {
                _logger.LogInformation($"Obteniendo historial SQL para sesión {sessionId}, usuario: {username}");
                return await _lockRepository.GetSessionSqlHistoryAsync(sessionId, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener historial SQL para sesión {sessionId}");
                throw;
            }
        }
        
        public async Task<List<SessionEventHistory>> GetSessionEventHistoryAsync(int sessionId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo historial de eventos para sesión {sessionId}");
                return await _lockRepository.GetSessionEventHistoryAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener historial de eventos para sesión {sessionId}");
                throw;
            }
        }
    }
}
