using Oracle.ManagedDataAccess.Client;
using OracleDBManager.Core.Interfaces;
using OracleDBManager.Core.Models;
using System.Data;

namespace OracleDBManager.Infrastructure.Repositories
{
    public class LockRepository : ILockRepository
    {
        private readonly Configuration.OracleConfiguration _config;
        
        public LockRepository(Configuration.OracleConfiguration config)
        {
            _config = config;
        }
        
        private OracleConnection GetConnection()
        {
            return new OracleConnection(_config.GetConnectionString());
        }
        
        public async Task<List<SessionLock>> GetAllLocksAsync()
        {
            var locks = new List<SessionLock>();
            
            const string query = @"
                SELECT 
                    s.sid,
                    s.serial#,
                    s.username,
                    s.osuser,
                    s.machine,
                    s.program,
                    s.sql_id,
                    l.type,
                    DECODE(l.lmode,
                        0, 'NONE',
                        1, 'NULL',
                        2, 'ROW SHARE',
                        3, 'ROW EXCLUSIVE',
                        4, 'SHARE',
                        5, 'SHARE ROW EXCLUSIVE',
                        6, 'EXCLUSIVE') AS lock_mode,
                    DECODE(l.request,
                        0, 'NONE',
                        1, 'NULL',
                        2, 'ROW SHARE',
                        3, 'ROW EXCLUSIVE',
                        4, 'SHARE',
                        5, 'SHARE ROW EXCLUSIVE',
                        6, 'EXCLUSIVE') AS request_mode,
                    o.object_type,
                    o.owner,
                    o.object_name,
                    l.id1,
                    l.id2,
                    l.block,
                    l.ctime AS time_held_seconds,
                    p.spid AS process_id,
                    s.blocking_session,
                    s.wait_class,
                    s.seconds_in_wait
                FROM 
                    v$lock l
                    JOIN v$session s ON l.sid = s.sid
                    LEFT JOIN dba_objects o ON l.id1 = o.object_id
                    LEFT JOIN v$process p ON s.paddr = p.addr
                WHERE 
                    l.type IN ('TM', 'TX')
                ORDER BY 
                    s.username, s.sid";
                    
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            using var command = new OracleCommand(query, connection);
            command.CommandTimeout = _config.CommandTimeout;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var sessionLock = new SessionLock
                {
                    SessionId = reader.GetInt32(reader.GetOrdinal("sid")),
                    SerialNumber = reader.GetInt32(reader.GetOrdinal("serial#")),
                    Username = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
                    OsUser = reader.IsDBNull(reader.GetOrdinal("osuser")) ? null : reader.GetString(reader.GetOrdinal("osuser")),
                    Machine = reader.IsDBNull(reader.GetOrdinal("machine")) ? null : reader.GetString(reader.GetOrdinal("machine")),
                    Program = reader.IsDBNull(reader.GetOrdinal("program")) ? null : reader.GetString(reader.GetOrdinal("program")),
                    SqlId = reader.IsDBNull(reader.GetOrdinal("sql_id")) ? null : reader.GetString(reader.GetOrdinal("sql_id")),
                    LockType = reader.GetString(reader.GetOrdinal("type")),
                    LockMode = reader.GetString(reader.GetOrdinal("lock_mode")),
                    RequestMode = reader.GetString(reader.GetOrdinal("request_mode")),
                    ObjectType = reader.IsDBNull(reader.GetOrdinal("object_type")) ? null : reader.GetString(reader.GetOrdinal("object_type")),
                    ObjectOwner = reader.IsDBNull(reader.GetOrdinal("owner")) ? null : reader.GetString(reader.GetOrdinal("owner")),
                    ObjectName = reader.IsDBNull(reader.GetOrdinal("object_name")) ? null : reader.GetString(reader.GetOrdinal("object_name")),
                    TimeHeldSeconds = reader.GetInt32(reader.GetOrdinal("time_held_seconds")),
                    ProcessId = reader.IsDBNull(reader.GetOrdinal("process_id")) ? null : reader.GetString(reader.GetOrdinal("process_id")),
                    IsBlocking = reader.GetInt32(reader.GetOrdinal("block")) == 1,
                    BlockedBySessionId = reader.IsDBNull(reader.GetOrdinal("blocking_session")) ? null : reader.GetInt32(reader.GetOrdinal("blocking_session")),
                    WaitClass = reader.IsDBNull(reader.GetOrdinal("wait_class")) ? null : reader.GetString(reader.GetOrdinal("wait_class")),
                    SecondsInWait = reader.IsDBNull(reader.GetOrdinal("seconds_in_wait")) ? null : reader.GetInt32(reader.GetOrdinal("seconds_in_wait")),
                    IsBlocked = !reader.IsDBNull(reader.GetOrdinal("blocking_session"))
                };
                
                locks.Add(sessionLock);
            }
            
            return locks;
        }
        
        public async Task<List<SessionLock>> GetBlockingLocksAsync()
        {
            var locks = new List<SessionLock>();
            
            const string query = @"
                SELECT DISTINCT
                    blocking_session.sid AS blocking_sid,
                    blocking_session.serial# AS blocking_serial,
                    blocking_session.username AS blocking_user,
                    blocking_session.osuser AS blocking_osuser,
                    blocking_session.machine AS blocking_machine,
                    blocking_session.program AS blocking_program,
                    blocking_session.sql_id AS blocking_sql_id,
                    blocked_session.sid AS blocked_sid,
                    blocked_session.serial# AS blocked_serial,
                    blocked_session.username AS blocked_user,
                    blocked_session.wait_class,
                    blocked_session.seconds_in_wait,
                    p.spid AS process_id
                FROM 
                    v$session blocked_session
                    JOIN v$session blocking_session 
                        ON blocked_session.blocking_session = blocking_session.sid
                    LEFT JOIN v$process p ON blocking_session.paddr = p.addr
                WHERE 
                    blocked_session.blocking_session IS NOT NULL";
                    
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            using var command = new OracleCommand(query, connection);
            command.CommandTimeout = _config.CommandTimeout;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var blockingLock = new SessionLock
                {
                    SessionId = reader.GetInt32(reader.GetOrdinal("blocking_sid")),
                    SerialNumber = reader.GetInt32(reader.GetOrdinal("blocking_serial")),
                    Username = reader.IsDBNull(reader.GetOrdinal("blocking_user")) ? null : reader.GetString(reader.GetOrdinal("blocking_user")),
                    OsUser = reader.IsDBNull(reader.GetOrdinal("blocking_osuser")) ? null : reader.GetString(reader.GetOrdinal("blocking_osuser")),
                    Machine = reader.IsDBNull(reader.GetOrdinal("blocking_machine")) ? null : reader.GetString(reader.GetOrdinal("blocking_machine")),
                    Program = reader.IsDBNull(reader.GetOrdinal("blocking_program")) ? null : reader.GetString(reader.GetOrdinal("blocking_program")),
                    SqlId = reader.IsDBNull(reader.GetOrdinal("blocking_sql_id")) ? null : reader.GetString(reader.GetOrdinal("blocking_sql_id")),
                    ProcessId = reader.IsDBNull(reader.GetOrdinal("process_id")) ? null : reader.GetString(reader.GetOrdinal("process_id")),
                    IsBlocking = true,
                    BlockingSessionId = reader.GetInt32(reader.GetOrdinal("blocked_sid"))
                };
                
                locks.Add(blockingLock);
            }
            
            return locks;
        }
        
        public async Task<List<BlockingChain>> GetBlockingChainsAsync()
        {
            var chains = new Dictionary<int, BlockingChain>();
            
            const string query = @"
                SELECT 
                    blocking_session.sid AS blocking_sid,
                    blocking_session.serial# AS blocking_serial,
                    blocking_session.username AS blocking_user,
                    blocking_session.osuser AS blocking_osuser,
                    blocking_session.machine AS blocking_machine,
                    blocking_session.program AS blocking_program,
                    blocking_session.sql_id AS blocking_sql_id,
                    blocked_session.sid AS blocked_sid,
                    blocked_session.serial# AS blocked_serial,
                    blocked_session.username AS blocked_user,
                    blocked_session.osuser AS blocked_osuser,
                    blocked_session.machine AS blocked_machine,
                    blocked_session.program AS blocked_program,
                    blocked_session.sql_id AS blocked_sql_id,
                    blocked_session.wait_class,
                    blocked_session.seconds_in_wait
                FROM 
                    v$session blocked_session
                    JOIN v$session blocking_session 
                        ON blocked_session.blocking_session = blocking_session.sid
                WHERE 
                    blocked_session.blocking_session IS NOT NULL
                ORDER BY 
                    blocking_session.sid";
                    
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            using var command = new OracleCommand(query, connection);
            command.CommandTimeout = _config.CommandTimeout;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var blockingSid = reader.GetInt32(reader.GetOrdinal("blocking_sid"));
                
                if (!chains.ContainsKey(blockingSid))
                {
                    chains[blockingSid] = new BlockingChain
                    {
                        BlockingSession = new SessionLock
                        {
                            SessionId = blockingSid,
                            SerialNumber = reader.GetInt32(reader.GetOrdinal("blocking_serial")),
                            Username = reader.IsDBNull(reader.GetOrdinal("blocking_user")) ? null : reader.GetString(reader.GetOrdinal("blocking_user")),
                            OsUser = reader.IsDBNull(reader.GetOrdinal("blocking_osuser")) ? null : reader.GetString(reader.GetOrdinal("blocking_osuser")),
                            Machine = reader.IsDBNull(reader.GetOrdinal("blocking_machine")) ? null : reader.GetString(reader.GetOrdinal("blocking_machine")),
                            Program = reader.IsDBNull(reader.GetOrdinal("blocking_program")) ? null : reader.GetString(reader.GetOrdinal("blocking_program")),
                            SqlId = reader.IsDBNull(reader.GetOrdinal("blocking_sql_id")) ? null : reader.GetString(reader.GetOrdinal("blocking_sql_id")),
                            IsBlocking = true
                        }
                    };
                }
                
                var blockedSession = new SessionLock
                {
                    SessionId = reader.GetInt32(reader.GetOrdinal("blocked_sid")),
                    SerialNumber = reader.GetInt32(reader.GetOrdinal("blocked_serial")),
                    Username = reader.IsDBNull(reader.GetOrdinal("blocked_user")) ? null : reader.GetString(reader.GetOrdinal("blocked_user")),
                    OsUser = reader.IsDBNull(reader.GetOrdinal("blocked_osuser")) ? null : reader.GetString(reader.GetOrdinal("blocked_osuser")),
                    Machine = reader.IsDBNull(reader.GetOrdinal("blocked_machine")) ? null : reader.GetString(reader.GetOrdinal("blocked_machine")),
                    Program = reader.IsDBNull(reader.GetOrdinal("blocked_program")) ? null : reader.GetString(reader.GetOrdinal("blocked_program")),
                    SqlId = reader.IsDBNull(reader.GetOrdinal("blocked_sql_id")) ? null : reader.GetString(reader.GetOrdinal("blocked_sql_id")),
                    WaitClass = reader.IsDBNull(reader.GetOrdinal("wait_class")) ? null : reader.GetString(reader.GetOrdinal("wait_class")),
                    SecondsInWait = reader.IsDBNull(reader.GetOrdinal("seconds_in_wait")) ? null : reader.GetInt32(reader.GetOrdinal("seconds_in_wait")),
                    IsBlocked = true,
                    BlockedBySessionId = blockingSid
                };
                
                chains[blockingSid].BlockedSessions.Add(blockedSession);
            }
            
            return chains.Values.ToList();
        }
        
        public async Task<SessionDetail> GetSessionDetailAsync(int sessionId)
        {
            const string query = @"
                SELECT 
                    s.sid,
                    s.serial#,
                    s.username,
                    s.osuser,
                    s.machine,
                    s.terminal,
                    s.program,
                    s.module,
                    s.action,
                    s.client_info,
                    s.logon_time,
                    s.status,
                    s.state,
                    s.wait_class,
                    s.wait_time,
                    s.seconds_in_wait,
                    s.sql_id,
                    s.sql_child_number,
                    s.prev_sql_id,
                    p.spid AS os_process_id,
                    p.pga_used_mem,
                    p.pga_alloc_mem
                FROM 
                    v$session s
                    JOIN v$process p ON s.paddr = p.addr
                WHERE 
                    s.sid = :sid";
                    
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            using var command = new OracleCommand(query, connection);
            command.Parameters.Add(new OracleParameter("sid", sessionId));
            command.CommandTimeout = _config.CommandTimeout;
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var detail = new SessionDetail
                {
                    SessionId = reader.GetInt32(reader.GetOrdinal("sid")),
                    SerialNumber = reader.GetInt32(reader.GetOrdinal("serial#")),
                    Username = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
                    OsUser = reader.IsDBNull(reader.GetOrdinal("osuser")) ? null : reader.GetString(reader.GetOrdinal("osuser")),
                    Machine = reader.IsDBNull(reader.GetOrdinal("machine")) ? null : reader.GetString(reader.GetOrdinal("machine")),
                    Terminal = reader.IsDBNull(reader.GetOrdinal("terminal")) ? null : reader.GetString(reader.GetOrdinal("terminal")),
                    Program = reader.IsDBNull(reader.GetOrdinal("program")) ? null : reader.GetString(reader.GetOrdinal("program")),
                    Module = reader.IsDBNull(reader.GetOrdinal("module")) ? null : reader.GetString(reader.GetOrdinal("module")),
                    Action = reader.IsDBNull(reader.GetOrdinal("action")) ? null : reader.GetString(reader.GetOrdinal("action")),
                    ClientInfo = reader.IsDBNull(reader.GetOrdinal("client_info")) ? null : reader.GetString(reader.GetOrdinal("client_info")),
                    LogonTime = reader.IsDBNull(reader.GetOrdinal("logon_time")) ? null : reader.GetDateTime(reader.GetOrdinal("logon_time")),
                    Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                    State = reader.IsDBNull(reader.GetOrdinal("state")) ? null : reader.GetString(reader.GetOrdinal("state")),
                    WaitClass = reader.IsDBNull(reader.GetOrdinal("wait_class")) ? null : reader.GetString(reader.GetOrdinal("wait_class")),
                    WaitTime = reader.IsDBNull(reader.GetOrdinal("wait_time")) ? null : reader.GetInt32(reader.GetOrdinal("wait_time")),
                    SecondsInWait = reader.IsDBNull(reader.GetOrdinal("seconds_in_wait")) ? null : reader.GetInt32(reader.GetOrdinal("seconds_in_wait")),
                    SqlId = reader.IsDBNull(reader.GetOrdinal("sql_id")) ? null : reader.GetString(reader.GetOrdinal("sql_id")),
                    SqlChildNumber = reader.IsDBNull(reader.GetOrdinal("sql_child_number")) ? null : reader.GetInt32(reader.GetOrdinal("sql_child_number")),
                    PrevSqlId = reader.IsDBNull(reader.GetOrdinal("prev_sql_id")) ? null : reader.GetString(reader.GetOrdinal("prev_sql_id")),
                    OsProcessId = reader.IsDBNull(reader.GetOrdinal("os_process_id")) ? null : reader.GetString(reader.GetOrdinal("os_process_id")),
                    PgaUsedMem = reader.IsDBNull(reader.GetOrdinal("pga_used_mem")) ? null : reader.GetDecimal(reader.GetOrdinal("pga_used_mem")),
                    PgaAllocMem = reader.IsDBNull(reader.GetOrdinal("pga_alloc_mem")) ? null : reader.GetDecimal(reader.GetOrdinal("pga_alloc_mem"))
                };
                
                // Obtener el SQL Text si existe
                if (!string.IsNullOrEmpty(detail.SqlId))
                {
                    detail.SqlText = await GetSessionSqlTextAsync(detail.SqlId);
                }
                
                return detail;
            }
            
            throw new Exception($"Session {sessionId} not found");
        }
        
        public async Task<string> GetSessionSqlTextAsync(string sqlId)
        {
            const string query = @"
                SELECT 
                    sql_text
                FROM 
                    v$sql
                WHERE 
                    sql_id = :sql_id
                    AND ROWNUM = 1";
                    
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            using var command = new OracleCommand(query, connection);
            command.Parameters.Add(new OracleParameter("sql_id", sqlId));
            command.CommandTimeout = _config.CommandTimeout;
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? string.Empty;
        }
        
        public async Task<bool> KillSessionAsync(int sessionId, int serialNumber)
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                
                var killCommand = $"ALTER SYSTEM KILL SESSION '{sessionId},{serialNumber}' IMMEDIATE";
                
                using var command = new OracleCommand(killCommand, connection);
                command.CommandTimeout = _config.CommandTimeout;
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                
                using var command = new OracleCommand("SELECT 1 FROM DUAL", connection);
                command.CommandTimeout = _config.CommandTimeout;
                
                await command.ExecuteScalarAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<List<SessionInfo>> GetAllSessionsAsync()
        {
            var sessions = new List<SessionInfo>();
            
            const string query = @"
                SELECT 
                    s.sid,
                    s.serial#,
                    s.username,
                    s.osuser,
                    s.machine,
                    s.terminal,
                    s.program,
                    s.module,
                    s.client_info,
                    s.logon_time,
                    s.status,
                    s.state,
                    s.sql_id,
                    s.command,
                    s.event,
                    s.seconds_in_wait,
                    s.wait_class,
                    p.spid AS process_id,
                    c.client_info AS connection_info
                FROM 
                    v$session s
                    LEFT JOIN v$process p ON s.paddr = p.addr
                    LEFT JOIN v$session_connect_info c ON s.sid = c.sid
                WHERE 
                    s.type = 'USER'
                    AND s.username IS NOT NULL
                ORDER BY 
                    s.logon_time DESC";
                    
            using var connection = GetConnection();
            await connection.OpenAsync();
            
            using var command = new OracleCommand(query, connection);
            command.CommandTimeout = _config.CommandTimeout;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var session = new SessionInfo
                {
                    SessionId = reader.GetInt32(reader.GetOrdinal("sid")),
                    SerialNumber = reader.GetInt32(reader.GetOrdinal("serial#")),
                    Username = reader.IsDBNull(reader.GetOrdinal("username")) ? null : reader.GetString(reader.GetOrdinal("username")),
                    OsUser = reader.IsDBNull(reader.GetOrdinal("osuser")) ? null : reader.GetString(reader.GetOrdinal("osuser")),
                    Machine = reader.IsDBNull(reader.GetOrdinal("machine")) ? null : reader.GetString(reader.GetOrdinal("machine")),
                    Terminal = reader.IsDBNull(reader.GetOrdinal("terminal")) ? null : reader.GetString(reader.GetOrdinal("terminal")),
                    Program = reader.IsDBNull(reader.GetOrdinal("program")) ? null : reader.GetString(reader.GetOrdinal("program")),
                    Module = reader.IsDBNull(reader.GetOrdinal("module")) ? null : reader.GetString(reader.GetOrdinal("module")),
                    ClientInfo = reader.IsDBNull(reader.GetOrdinal("client_info")) ? null : reader.GetString(reader.GetOrdinal("client_info")),
                    LogonTime = reader.IsDBNull(reader.GetOrdinal("logon_time")) ? null : reader.GetDateTime(reader.GetOrdinal("logon_time")),
                    Status = reader.IsDBNull(reader.GetOrdinal("status")) ? null : reader.GetString(reader.GetOrdinal("status")),
                    State = reader.IsDBNull(reader.GetOrdinal("state")) ? null : reader.GetString(reader.GetOrdinal("state")),
                    SqlId = reader.IsDBNull(reader.GetOrdinal("sql_id")) ? null : reader.GetString(reader.GetOrdinal("sql_id")),
                    Event = reader.IsDBNull(reader.GetOrdinal("event")) ? null : reader.GetString(reader.GetOrdinal("event")),
                    SecondsInWait = reader.IsDBNull(reader.GetOrdinal("seconds_in_wait")) ? null : reader.GetInt32(reader.GetOrdinal("seconds_in_wait")),
                    WaitClass = reader.IsDBNull(reader.GetOrdinal("wait_class")) ? null : reader.GetString(reader.GetOrdinal("wait_class"))
                };
                
                // Intentar extraer IP de la máquina si está disponible
                if (!string.IsNullOrEmpty(session.Machine))
                {
                    // Si la máquina contiene una IP entre paréntesis, extraerla
                    var ipMatch = System.Text.RegularExpressions.Regex.Match(session.Machine, @"\((\d+\.\d+\.\d+\.\d+)\)");
                    if (ipMatch.Success)
                    {
                        session.IpAddress = ipMatch.Groups[1].Value;
                    }
                    else
                    {
                        // Si no, usar el nombre de la máquina como IP
                        session.IpAddress = session.Machine;
                    }
                }
                
                sessions.Add(session);
            }
            
            return sessions;
        }
    }
}
