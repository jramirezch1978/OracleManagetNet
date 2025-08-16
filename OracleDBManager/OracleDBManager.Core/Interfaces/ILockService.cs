using OracleDBManager.Core.Models;

namespace OracleDBManager.Core.Interfaces
{
    public interface ILockService
    {
        Task<List<SessionLock>> GetAllLocksAsync(bool includeMRLocks = false);
        Task<List<SessionLock>> GetBlockingLocksAsync();
        Task<List<BlockingChain>> GetBlockingChainsAsync();
        Task<SessionDetail> GetSessionDetailAsync(int sessionId);
        Task<(bool success, string message)> KillSessionAsync(int sessionId, int serialNumber, string username);
        Task<bool> TestDatabaseConnectionAsync();
        Task<List<SessionInfo>> GetAllSessionsAsync();
    }
}
