namespace OracleDBManager.Core.Models
{
    public class SessionEventHistory
    {
        public string? EventName { get; set; }
        public string? WaitClass { get; set; }
        public long TotalWaits { get; set; }
        public decimal TimeWaitedSeconds { get; set; }
        public decimal AverageWaitMs { get; set; }
        public decimal MaxWaitMs { get; set; }
        public DateTime? LastWaitTime { get; set; }
    }
}
