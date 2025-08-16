using OracleDBManager.Core.Models;

namespace OracleDBManager.Core.Interfaces
{
    public interface ILockRepository
    {
        Task<List<SessionLock>> GetAllLocksAsync();
        Task<List<SessionLock>> GetBlockingLocksAsync();
        Task<List<BlockingChain>> GetBlockingChainsAsync();
        Task<SessionDetail> GetSessionDetailAsync(int sessionId);
        Task<string> GetSessionSqlTextAsync(string sqlId);
        Task<bool> KillSessionAsync(int sessionId, int serialNumber);
        Task<bool> TestConnectionAsync();
        Task<List<SessionInfo>> GetAllSessionsAsync();
    }
}
