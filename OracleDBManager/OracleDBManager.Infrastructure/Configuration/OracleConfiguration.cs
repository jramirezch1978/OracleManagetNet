namespace OracleDBManager.Infrastructure.Configuration
{
    public class OracleConfiguration
    {
        public string? ConnectionString { get; set; }
        public string? DataSource { get; set; }
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public int ConnectionTimeout { get; set; } = 30;
        public int CommandTimeout { get; set; } = 120;
        public bool UseWallet { get; set; }
        public string? WalletLocation { get; set; }
        
        public string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;
                
            if (UseWallet && !string.IsNullOrEmpty(WalletLocation))
            {
                return $"Data Source={DataSource};Wallet_Location={WalletLocation}";
            }
            
            return $"Data Source={DataSource};User Id={UserId};Password={Password};Connection Timeout={ConnectionTimeout}";
        }
    }
}
