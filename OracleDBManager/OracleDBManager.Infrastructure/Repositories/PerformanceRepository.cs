using Oracle.ManagedDataAccess.Client;
using OracleDBManager.Core.Interfaces;
using OracleDBManager.Core.Models.Performance;
using OracleDBManager.Infrastructure.Configuration;

namespace OracleDBManager.Infrastructure.Repositories;

public class PerformanceRepository : IPerformanceRepository
{
	private readonly OracleDBManager.Infrastructure.Configuration.OracleConfiguration _config;

	public PerformanceRepository(OracleDBManager.Infrastructure.Configuration.OracleConfiguration config)
	{
		_config = config;
	}

	private OracleConnection GetConnection() => new OracleConnection(_config.GetConnectionString());

	public async Task<List<CpuPoint>> GetCpuUtilizationAsync(DateTime from, DateTime to)
	{
		var points = new List<CpuPoint>();
		
		// Consulta real para DBA - usar v$sysmetric_history para datos históricos reales
		const string query = @"
			SELECT 
				begin_time,
				NVL(value, 0) AS cpu_utilization
			FROM (
				SELECT begin_time, value
				FROM v$sysmetric_history 
				WHERE metric_name = 'Host CPU Utilization (%)'
				AND begin_time >= SYSDATE - INTERVAL '2' HOUR
				ORDER BY begin_time DESC
			)
			WHERE ROWNUM <= 12
			ORDER BY begin_time";

		using var conn = GetConnection();
		await conn.OpenAsync();
		using var cmd = new OracleCommand(query, conn);
		cmd.CommandTimeout = _config.CommandTimeout;
		
		using var reader = await cmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			var p = new CpuPoint
			{
				Timestamp = reader.GetDateTime(0),
				UtilizationPercent = Convert.ToDecimal(reader.GetValue(1))
			};
			points.Add(p);
		}
		
		// Si no hay datos, agregar al menos un punto con valor cero
		if (!points.Any())
		{
			points.Add(new CpuPoint
			{
				Timestamp = DateTime.Now,
				UtilizationPercent = 0
			});
		}
		
		return points;
	}

	public async Task<MemoryUsage> GetMemoryUsageAsync()
	{
		var usage = new MemoryUsage();
		
		// Consultas reales para DBA
		const string sgaQuery = @"SELECT NVL(SUM(value)/1024/1024,0) FROM v$sga";
		const string pgaQuery = @"SELECT NVL(value/1024/1024,0) FROM v$pgastat WHERE name = 'total PGA allocated'";
		const string sgaUsedQuery = @"SELECT NVL(SUM(bytes)/1024/1024,0) FROM v$sgastat WHERE name != 'free memory'";

		using var conn = GetConnection();
		await conn.OpenAsync();
		
		// SGA Total
		using (var cmd = new OracleCommand(sgaQuery, conn))
		{
			cmd.CommandTimeout = _config.CommandTimeout;
			var res = await cmd.ExecuteScalarAsync();
			usage.SgaTotalMb = Convert.ToDecimal(res ?? 0);
		}

		// SGA Usado (aproximación)
		using (var cmd = new OracleCommand(sgaUsedQuery, conn))
		{
			cmd.CommandTimeout = _config.CommandTimeout;
			var res = await cmd.ExecuteScalarAsync();
			usage.SgaUsedMb = Convert.ToDecimal(res ?? 0);
			if (usage.SgaUsedMb == 0) usage.SgaUsedMb = usage.SgaTotalMb * 0.8m;
		}

		// PGA Total
		using (var cmd = new OracleCommand(pgaQuery, conn))
		{
			cmd.CommandTimeout = _config.CommandTimeout;
			var res = await cmd.ExecuteScalarAsync();
			usage.PgaTotalMb = Convert.ToDecimal(res ?? 0);
			usage.PgaUsedMb = usage.PgaTotalMb; // PGA allocated es lo que está en uso
		}
		
		// Memoria OS (aproximación basada en SGA + PGA)
		usage.OsMemoryTotalMb = (usage.SgaTotalMb + usage.PgaTotalMb) * 2; // Estimación
		usage.OsMemoryUsedMb = usage.SgaUsedMb + usage.PgaUsedMb;
		
		return usage;
	}

	public async Task<List<TablespaceSize>> GetTablespacesAsync()
	{
		var list = new List<TablespaceSize>();
		
		// Consulta real para DBA - usar dba_data_files y dba_free_space
		const string query = @"
			SELECT
				df.tablespace_name,
				ROUND(SUM(df.bytes)/1024/1024, 2) AS total_mb,
				ROUND(SUM(df.bytes - NVL(f.free_bytes,0))/1024/1024, 2) AS used_mb
			FROM (
				SELECT tablespace_name, bytes FROM dba_data_files
				UNION ALL
				SELECT tablespace_name, bytes FROM dba_temp_files
			) df
			LEFT JOIN (
				SELECT tablespace_name, SUM(bytes) AS free_bytes
				FROM dba_free_space
				GROUP BY tablespace_name
			) f ON df.tablespace_name = f.tablespace_name
			GROUP BY df.tablespace_name
			ORDER BY df.tablespace_name";

		using var conn = GetConnection();
		await conn.OpenAsync();
		using var cmd = new OracleCommand(query, conn);
		cmd.CommandTimeout = _config.CommandTimeout;
		
		using var reader = await cmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			list.Add(new TablespaceSize
			{
				Name = reader.IsDBNull(0) ? null : reader.GetString(0),
				TotalMb = Convert.ToDecimal(reader.GetValue(1)),
				UsedMb = Convert.ToDecimal(reader.GetValue(2))
			});
		}
		
		// Si no hay datos, agregar al menos un elemento con valor cero
		if (!list.Any())
		{
			list.Add(new TablespaceSize
			{
				Name = "Sin datos",
				TotalMb = 0,
				UsedMb = 0
			});
		}
		
		return list;
	}

	public async Task<List<DatafileSize>> GetDatafilesAsync()
	{
		var list = new List<DatafileSize>();
		
		// Consulta real para DBA - usar dba_data_files
		const string query = @"
			SELECT file_name, tablespace_name, ROUND(bytes/1024/1024,2) AS mb
			FROM dba_data_files
			ORDER BY tablespace_name, file_name";

		using var conn = GetConnection();
		await conn.OpenAsync();
		using var cmd = new OracleCommand(query, conn);
		cmd.CommandTimeout = _config.CommandTimeout;
		
		using var reader = await cmd.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			list.Add(new DatafileSize
			{
				FileName = reader.IsDBNull(0) ? null : reader.GetString(0),
				TablespaceName = reader.IsDBNull(1) ? null : reader.GetString(1),
				SizeMb = Convert.ToDecimal(reader.GetValue(2))
			});
		}
		
		// Si no hay datos, agregar al menos un elemento con valor cero
		if (!list.Any())
		{
			list.Add(new DatafileSize
			{
				FileName = "Sin datos",
				TablespaceName = "Sin datos",
				SizeMb = 0
			});
		}
		
		return list;
	}
}


