namespace OracleDBManager.Core.Models
{
    public class SessionDetail
    {
        public int SessionId { get; set; }
        public int SerialNumber { get; set; }
        public string? Username { get; set; }
        public string? OsUser { get; set; }
        public string? Machine { get; set; }
        public string? Terminal { get; set; }
        public string? Program { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public string? ClientInfo { get; set; }
        public DateTime? LogonTime { get; set; }
        public string? Status { get; set; }
        public string? State { get; set; }
        public string? WaitClass { get; set; }
        public int? WaitTime { get; set; }
        public int? SecondsInWait { get; set; }
        public string? SqlId { get; set; }
        public int? SqlChildNumber { get; set; }
        public string? PrevSqlId { get; set; }
        public string? OsProcessId { get; set; }
        public decimal? PgaUsedMem { get; set; }
        public decimal? PgaAllocMem { get; set; }
        public string? SqlText { get; set; }
        public string? SqlFullText { get; set; }
    }
}
