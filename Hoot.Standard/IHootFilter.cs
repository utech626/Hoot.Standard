using System;
using System.Collections.Generic;
using System.Text;

namespace RaptorDB
{
	public interface IHootFilter
	{
		/// <summary>
		/// Perform Initialization of Filter
		/// </summary>
		/// <param name="filterPath">Path of Filter Text to load</param>
		void InitializeFilter(String filterPath);
		/// <summary>
		/// Filter the input test
		/// </summary>
		/// <param name="input">Text to Filter</param>
		/// <returns>Filtered Text</returns>
		String FilterText(String input);
	}
}
