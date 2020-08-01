using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using hOOt;

namespace SampleApp
{
	class myDoc : Document
	{
		public myDoc(FileInfo fileinfo, string text) 
			: base(fileinfo, text)
		{
			now = DateTime.Now;
		}

		// other data I want to save
		public DateTime now;
	}
}
