using System;
using System.Collections.Generic;
using System.Text;

namespace RaptorDB.Filters
{
	public class NoFilter : IHootFilter
	{
		/// <summary>
		/// Just return the Text in default Filter
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string FilterText(string input)
		{
			return (input);
		}
		/// <summary>
		/// Do nothing in the default filter
		/// </summary>
		/// <param name="filterPath">Path to full text folder</param>
		public void InitializeFilter(string filterPath)
		{

		}
	}
}
