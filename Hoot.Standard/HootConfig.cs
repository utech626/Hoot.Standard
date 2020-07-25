using System;
using System.Collections.Generic;
using System.Text;

namespace RaptorDB
{
	/// <summary>
	/// Hoot Configuration file that can be initialized and passed to hoot Constructor
	/// </summary>
	public class HootConfig
	{
		public String IndexPath { get; set; }
		public String FileName { get; set; }
		public bool DocMode { get; set; }
		public bool UseStopList { get; set; }
		public bool IgnoreNumerics { get; set; }

		public HootConfig()
		{
			IndexPath = String.Empty;
			FileName = "words";
			DocMode = false;
			UseStopList = true;
			IgnoreNumerics = false;
		}
	}
}
