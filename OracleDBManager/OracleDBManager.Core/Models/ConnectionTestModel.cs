namespace OracleDBManager.Core.Models
{
    public class ConnectionTestModel
    {
        public string? Host { get; set; }
        public string? Port { get; set; }
        public string? ServiceName { get; set; }
        public string? UserId { get; set; }
        public string? Password { get; set; }
        
        public string GetConnectionString()
        {
            return $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port}))(CONNECT_DATA=(SERVICE_NAME={ServiceName})))";
        }
        
        public string GetFullConnectionString()
        {
            return $"Data Source={GetConnectionString()};User Id={UserId};Password={Password}";
        }
    }
}
