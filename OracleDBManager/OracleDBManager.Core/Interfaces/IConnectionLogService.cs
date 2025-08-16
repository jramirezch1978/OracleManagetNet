using OracleDBManager.Core.Models;

namespace OracleDBManager.Core.Interfaces
{
    public interface IConnectionLogService
    {
        Task LogConnectionAttemptAsync(ConnectionTestModel connectionModel, string userName, string clientIp);
        Task LogConnectionSuccessAsync(ConnectionTestModel connectionModel, string userName, string clientIp);
        Task LogConnectionErrorAsync(ConnectionTestModel connectionModel, string userName, string clientIp, Exception exception);
    }
}
