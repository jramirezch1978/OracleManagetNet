namespace OracleDBManager.Core.Models
{
    public class SessionLock
    {
        public int SessionId { get; set; }
        public int SerialNumber { get; set; }
        public string? Username { get; set; }
        public string? OsUser { get; set; }
        public string? Machine { get; set; }
        public string? Program { get; set; }
        public string? SqlId { get; set; }
        public string? LockType { get; set; }
        public string? LockMode { get; set; }
        public string? RequestMode { get; set; }
        public string? ObjectType { get; set; }
        public string? ObjectOwner { get; set; }
        public string? ObjectName { get; set; }
        public string? RowId { get; set; }
        public int TimeHeldSeconds { get; set; }
        public bool IsBlocking { get; set; }
        public int? BlockingSessionId { get; set; }
        public string? ProcessId { get; set; }
        public DateTime? LockTime { get; set; }
        
        // Propiedades adicionales para la vista
        public bool IsSelected { get; set; }
        public bool IsBlocked { get; set; }
        public int? BlockedBySessionId { get; set; }
        public string? WaitClass { get; set; }
        public int? SecondsInWait { get; set; }
    }
}
