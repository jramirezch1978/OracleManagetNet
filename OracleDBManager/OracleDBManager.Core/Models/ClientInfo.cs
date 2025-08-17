using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Http;

namespace OracleDBManager.Core.Models
{
    public class ClientInfo
    {
        public string UserName { get; set; } = "Usuario desconocido";
        public string MachineName { get; set; } = "Máquina desconocida";
        public string LocalIpV4 { get; set; } = "IP local desconocida";
        public string PublicIp { get; set; } = "IP pública desconocida";
        public string MacAddress { get; set; } = "MAC desconocida";
        public string NetworkInterfaceName { get; set; } = "Interfaz desconocida";
        
        public static async Task<ClientInfo> GetClientInfoAsync(HttpContext? httpContext)
        {
            var clientInfo = new ClientInfo();
            
            try
            {
                // Información del usuario
                clientInfo.UserName = httpContext?.User?.Identity?.Name ?? Environment.UserName;
                clientInfo.MachineName = Environment.MachineName;
                
                // Obtener IP local IPv4
                clientInfo.LocalIpV4 = GetLocalIPv4();
                
                // Obtener información de la interfaz de red
                var (interfaceName, macAddress) = GetNetworkInterfaceInfo();
                clientInfo.NetworkInterfaceName = interfaceName;
                clientInfo.MacAddress = macAddress;
                
                // Obtener IP pública (async)
                clientInfo.PublicIp = await GetPublicIPAsync();
            }
            catch (Exception ex)
            {
                // Log del error pero continuar
                Console.WriteLine($"Error obteniendo información del cliente: {ex.Message}");
            }
            
            return clientInfo;
        }
        
        private static string GetLocalIPv4()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        // Ignorar direcciones loopback
                        if (!IPAddress.IsLoopback(ip))
                        {
                            return ip.ToString();
                        }
                    }
                }
                
                // Si no encontramos una IP externa, devolver la loopback IPv4
                return "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
        
        private static (string interfaceName, string macAddress) GetNetworkInterfaceInfo()
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (var ni in networkInterfaces)
                {
                    // Buscar interfaces activas y con IPv4
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipProps = ni.GetIPProperties();
                        foreach (var addr in ipProps.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                !IPAddress.IsLoopback(addr.Address))
                            {
                                var macAddress = string.Join(":", ni.GetPhysicalAddress()
                                    .GetAddressBytes()
                                    .Select(b => b.ToString("X2")));
                                    
                                return (ni.Name, macAddress);
                            }
                        }
                    }
                }
                
                return ("No encontrada", "00:00:00:00:00:00");
            }
            catch
            {
                return ("Error", "00:00:00:00:00:00");
            }
        }
        
        private static async Task<string> GetPublicIPAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                // Usar varios servicios por si uno falla
                var services = new[]
                {
                    "https://api.ipify.org",
                    "https://icanhazip.com",
                    "https://ifconfig.me/ip"
                };
                
                foreach (var service in services)
                {
                    try
                    {
                        var response = await httpClient.GetStringAsync(service);
                        var ip = response.Trim();
                        
                        // Validar que es una IP válida
                        if (IPAddress.TryParse(ip, out _))
                        {
                            return ip;
                        }
                    }
                    catch
                    {
                        // Intentar con el siguiente servicio
                        continue;
                    }
                }
                
                return "No disponible";
            }
            catch
            {
                return "Error al obtener";
            }
        }
    }
}
