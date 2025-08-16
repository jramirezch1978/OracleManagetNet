namespace OracleDBManager.Core.Models
{
    public class BlockingChain
    {
        public SessionLock BlockingSession { get; set; } = new();
        public List<SessionLock> BlockedSessions { get; set; } = new();
        
        public int TotalBlockedSessions => BlockedSessions.Count;
        public int MaxWaitTime => BlockedSessions.Any() 
            ? BlockedSessions.Max(s => s.SecondsInWait ?? 0) 
            : 0;
    }
}
