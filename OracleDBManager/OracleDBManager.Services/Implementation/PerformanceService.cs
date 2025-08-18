using OracleDBManager.Core.Interfaces;
using OracleDBManager.Core.Models.Performance;

namespace OracleDBManager.Services.Implementation;

public class PerformanceService : IPerformanceService
{
	private readonly IPerformanceRepository _repository;

	public PerformanceService(IPerformanceRepository repository)
	{
		_repository = repository;
	}

	public Task<List<CpuPoint>> GetCpuUtilizationAsync(DateTime from, DateTime to)
		=> _repository.GetCpuUtilizationAsync(from, to);

	public Task<MemoryUsage> GetMemoryUsageAsync()
		=> _repository.GetMemoryUsageAsync();

	public Task<List<TablespaceSize>> GetTablespacesAsync()
		=> _repository.GetTablespacesAsync();

	public Task<List<DatafileSize>> GetDatafilesAsync()
		=> _repository.GetDatafilesAsync();
}


