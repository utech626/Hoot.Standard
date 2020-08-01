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
		/// <summary>
		/// Path where index files are stored
		/// </summary>
		public String IndexPath { get; set; }
		/// <summary>
		/// Filename prefix for index files. 
		/// Defaults to indexx
		/// </summary>
		public String FileName { get; set; }
		/// <summary>
		/// Use Document Mode
		/// </summary>
		public bool DocMode { get; set; }
		/// <summary>
		/// Use Word Stop List
		/// </summary>
		public bool UseStopList { get; set; }
		/// <summary>
		/// Ignore numeric words, ie 123,555, etc
		/// </summary>
		public bool IgnoreNumerics { get; set; }

		public HootConfig()
		{
			IndexPath = String.Empty;
			FileName = "index";
			DocMode = false;
			UseStopList = true;
			IgnoreNumerics = false;
		}
	}
}
