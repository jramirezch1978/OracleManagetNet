namespace OracleDBManager.Core.Models.Performance;

public class TablespaceSize
{
	public string? Name { get; set; }
	public decimal TotalMb { get; set; }
	public decimal UsedMb { get; set; }
}


