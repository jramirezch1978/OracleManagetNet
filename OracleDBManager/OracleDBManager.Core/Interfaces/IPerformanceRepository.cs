using OracleDBManager.Core.Models.Performance;

namespace OracleDBManager.Core.Interfaces;

public interface IPerformanceRepository
{
	Task<List<CpuPoint>> GetCpuUtilizationAsync(DateTime from, DateTime to);
	Task<MemoryUsage> GetMemoryUsageAsync();
	Task<List<TablespaceSize>> GetTablespacesAsync();
	Task<List<DatafileSize>> GetDatafilesAsync();
}


