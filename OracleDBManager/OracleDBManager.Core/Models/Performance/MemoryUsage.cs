namespace OracleDBManager.Core.Models.Performance;

public class MemoryUsage
{
	public decimal SgaTotalMb { get; set; }
	public decimal PgaTotalMb { get; set; }
	public decimal SgaUsedMb { get; set; }
	public decimal PgaUsedMb { get; set; }
	public decimal OsMemoryTotalMb { get; set; }
	public decimal OsMemoryUsedMb { get; set; }
}


