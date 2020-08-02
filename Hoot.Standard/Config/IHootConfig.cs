namespace RaptorDB
{
	public interface IHootConfig
	{
		bool DocMode { get; set; }
		string FileName { get; set; }
		bool IgnoreNumerics { get; set; }
		string IndexPath { get; set; }
		bool UseStopList { get; set; }
	}
}