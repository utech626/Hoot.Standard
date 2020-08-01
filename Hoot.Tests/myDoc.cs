using hOOt;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hoot.Tests
{
	public class myDoc : Document
	{
		public DateTime DocTime { get; set; }

		public myDoc(FileInfo fileInfo, String text)
			: base(fileInfo, text)
		{
			DocTime = DateTime.Now;
		}
	}
}
