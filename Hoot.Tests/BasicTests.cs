using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;
using RaptorDB;

namespace Hoot.Tests
{
	public class BasicTests
	{
		private HootConfig _config;
		private String s;

		[SetUp]
		public void Setup()
		{
			String _curPath = Environment.CurrentDirectory;

			_config = new HootConfig
			{
				IndexPath = Path.Combine(_curPath, "Indexer"),
				DocMode = false,
				UseStopList = false,
				IgnoreNumerics = false
			};

			s = @"134,909.09090
.......................................
abcdefg..--=-
B.A.T. 
a.k.a. 
1111111111 111.2342314 ---------------- brain-dead c:\dir1\dir2
http://www.google.com/path1/path2
hoot.property
camelCase field
PascalCase property/    @test;pppp=1
.aaaaa  ..bbbbb        com.ionic.framework  bob@gmail.com filename.docx filename.pdf
";
		}

		[Test]
		public void Words()
		{
			var d = new RaptorDB.tokenizer().GenerateWordFreq(s, _config);

			Assert.AreEqual(32, d.Count);
		}
		[Test]
		public void NumericWords()
		{
			_config.IgnoreNumerics = true;

			var c = new RaptorDB.tokenizer().GenerateWordFreq(s, _config);
			Assert.AreEqual(26, c.Count);
		}

		[Test]
		public void TestStopList()
		{
			_config.UseStopList = true;
			var t = new RaptorDB.tokenizer();

			t.InitializeStopList(_config.IndexPath);

			var b = t.GenerateWordFreq(s, _config);

			Assert.AreEqual(31, b.Count);
		}

	}
}
