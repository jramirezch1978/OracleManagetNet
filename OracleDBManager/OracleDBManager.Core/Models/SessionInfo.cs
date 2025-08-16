namespace OracleDBManager.Core.Models
{
    public class SessionInfo
    {
        public int SessionId { get; set; }
        public int SerialNumber { get; set; }
        public string? Username { get; set; }
        public string? OsUser { get; set; }
        public string? Machine { get; set; }
        public string? Terminal { get; set; }
        public string? Program { get; set; }
        public string? Module { get; set; }
        public string? ClientInfo { get; set; }
        public DateTime? LogonTime { get; set; }
        public string? Status { get; set; }
        public string? State { get; set; }
        public string? SqlId { get; set; }
        public string? Command { get; set; }
        public string? Event { get; set; }
        public int? SecondsInWait { get; set; }
        public string? WaitClass { get; set; }
        
        // Propiedades calculadas
        public string? IpAddress { get; set; }
        public TimeSpan? ConnectionDuration => LogonTime.HasValue 
            ? DateTime.Now - LogonTime.Value 
            : null;
        
        public string ConnectionDurationFormatted
        {
            get
            {
                if (!ConnectionDuration.HasValue)
                    return "N/A";
                
                var duration = ConnectionDuration.Value;
                if (duration.TotalDays >= 1)
                    return $"{(int)duration.TotalDays}d {duration.Hours}h {duration.Minutes}m";
                else if (duration.TotalHours >= 1)
                    return $"{(int)duration.TotalHours}h {duration.Minutes}m";
                else
                    return $"{duration.Minutes}m {duration.Seconds}s";
            }
        }
        
        // Para el checkbox de selección
        public bool IsSelected { get; set; }
    }
}
