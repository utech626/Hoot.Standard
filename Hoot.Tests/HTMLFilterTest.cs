using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using NUnit.Framework;

using RaptorDB;
using RaptorDB.Filters;

namespace Hoot.Tests
{
	public class HTMLFilterTest
	{
		private String _htmlSource;
		private String _htmlResult;

		[SetUp]
		public void Setup()
		{
			_htmlSource = HttpUtility.HtmlDecode(Properties.Resources.ResourceManager.GetString("HtmlSource"));
			_htmlResult = Properties.Resources.ResourceManager.GetString("HtmlResult");
		}

		/// <summary>
		/// Test the HTML Filter
		/// </summary>
		[Test]
		public void HtmlFilter()
		{
			IHootFilter _filter = new HtmlFilter();
			String _result;

			_filter.InitializeFilter(null);
			_result = _filter.FilterText(_htmlSource);
			Assert.AreEqual(_result, _htmlResult);
		}
	}

}
