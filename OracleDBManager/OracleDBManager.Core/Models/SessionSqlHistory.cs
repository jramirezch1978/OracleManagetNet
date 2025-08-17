namespace OracleDBManager.Core.Models
{
    public class SessionSqlHistory
    {
        public string? SqlId { get; set; }
        public string? SqlText { get; set; }
        public string? Module { get; set; }
        public DateTime FirstLoadTime { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public decimal ElapsedTime { get; set; }
        public decimal CpuTime { get; set; }
        public decimal BufferGets { get; set; }
        public decimal DiskReads { get; set; }
        public decimal Executions { get; set; }
        public decimal AvgElapsedTime { get; set; }
    }
}
