using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NUnit.Framework;

using RaptorDB;

namespace Hoot.Tests
{
	public class IndexTestsWithStop
	{
		protected HootConfig _config;

		/// <summary>
		/// Initialize the test environment
		/// </summary>
		[SetUp]
		public void Setup()
		{
			_config = new HootConfig
			{
				DocMode = true,
				IndexPath = "FullTextIndexes",
				FileName = "NunitTests",
				UseStopList = true,
				IgnoreNumerics = true
			};
		}
		/// <summary>
		/// Build a new index from Test Files
		/// </summary>
		[Test, Order(1)]
		public void BuildIndex()
		{
			if (Directory.Exists(_config.IndexPath))
				Directory.Delete(_config.IndexPath, true);

			Directory.CreateDirectory(_config.IndexPath);

			RaptorDB.Hoot _hoot = new RaptorDB.Hoot(_config);

			try
			{
				foreach (var _file in Directory.EnumerateFiles("TestFiles"))
				{
					if (!_hoot.IsIndexed(_file))
					{
						using (TextReader _tf = File.OpenText(_file))
						{
							String _text = null;

							if (_tf != null)
								_text = _tf.ReadToEnd();

							if (!String.IsNullOrEmpty(_text))
								_hoot.Index(new myDoc(new FileInfo(_file), _text), true);
						}
					}
				}
				_hoot.Save();
			}
			catch (Exception ex)
			{
				throw new AssertionException($"Exception Building Index: {ex.Message} - {ex.StackTrace}");
			}
			Assert.AreEqual(59, _hoot.DocumentCount);
			Assert.AreEqual(99697, _hoot.WordCount);
			_hoot.Shutdown();
		}
		/// <summary>
		/// Test Loading an existing index
		/// </summary>
		[Test, Order(2)]
		public void LoadIndex()
		{
			RaptorDB.Hoot _hoot = new RaptorDB.Hoot(_config);

			Assert.AreEqual(59, _hoot.DocumentCount);
			Assert.AreEqual(99697, _hoot.WordCount);
			_hoot.Shutdown();
		}
		/// <summary>
		/// Perform Query Tests
		/// </summary>
		[Test, Order(3)]
		public void QueryTests()
		{
			IEnumerable<string> _d;

			RaptorDB.Hoot _hoot = new RaptorDB.Hoot(_config);

			Assert.AreEqual(59, _hoot.DocumentCount);
			Assert.AreEqual(99697, _hoot.WordCount);

			_d = _hoot.FindDocumentFileNames("score");
			Assert.IsNotNull(_d);
			Assert.AreEqual(27, _d.Count());
			//
			//	Test And Function
			//
			_d = _hoot.FindDocumentFileNames("Peter Jesus fisherman boat");
			Assert.IsNotNull(_d);
			Assert.AreEqual(3, _d.Count());
			//
			//	Test And Function
			//
			_d = _hoot.FindDocumentFileNames("human declare Governments history Despotism wholesome");
			Assert.IsNotNull(_d);
			Assert.AreEqual(2, _d.Count());
			//
			//	Test Not Function
			//
			_d = _hoot.FindDocumentFileNames("-SIERRA -NEVADA");
			Assert.IsNotNull(_d);
			Assert.AreEqual(3, _d.Count());
			//
			//	Test And, Or Functions
			//
			_d = _hoot.FindDocumentFileNames("Peter Jesus fisherman +water");
			Assert.IsNotNull(_d);
			Assert.AreEqual(54, _d.Count());
			//
			//	Test And, Or, And Not, Functions
			//
			_d = _hoot.FindDocumentFileNames("human declare Governments +history -Despotism -wholesome");
			Assert.IsNotNull(_d);
			Assert.AreEqual(25, _d.Count());
			//
			//	Test Stop List Workds
			//
			_d = _hoot.FindDocumentFileNames("about +although +cannot +corresponding +indicates +regarding");
			Assert.IsNotNull(_d);
			Assert.AreEqual(0, _d.Count());

			_hoot.Shutdown();
		}
	}
}
