namespace OracleDBManager.Core.Models.Performance;

public class DatafileSize
{
	public string? FileName { get; set; }
	public string? TablespaceName { get; set; }
	public decimal SizeMb { get; set; }
}


