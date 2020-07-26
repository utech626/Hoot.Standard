using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using hOOt;

using RaptorDB.Common;
using RaptorDB.Filters;


namespace RaptorDB
{
	public class Hoot
	{
		#region Public Properties

		/// <summary>
		/// Use Doc Mode
		/// </summary>
		public bool DocMode => HootConfOptions.DocMode;

		/// <summary>
		/// Get Document Count
		/// </summary>
		public int DocumentCount
		{
			get { checkloaded(); return _lastDocNum - (int)_deleted.GetBits().CountOnes(); }
		}

		/// <summary>
		/// Words file name
		/// </summary>
		public String FileName => HootConfOptions.FileName;

		/// <summary>
		/// Configuration file
		/// </summary>
		public HootConfig HootConfOptions { get; set; }

		/// <summary>
		/// Ignore all numeric words
		/// Example: 0000000
		/// </summary>
		public bool IgnoreNumerics => HootConfOptions.IgnoreNumerics;

		/// <summary>
		/// Path to indexes
		/// </summary>
		public String IndexPath => HootConfOptions.IndexPath;

		/// <summary>
		/// Use Stop List while Indexing
		/// </summary>
		public bool UseStopList => HootConfOptions.UseStopList;

		/// <summary>
		/// Get Word Count
		/// </summary>
		public int WordCount
		{
			get { checkloaded(); return _words.Count(); }
		}

		/// <summary>
		/// Get List of Words
		/// </summary>
		public string[] Words
		{
			get { checkloaded(); return _words.Keys(); }
		}

		#endregion Public Properties

		#region Public Constructors

		/// <summary>
		/// Construct a new Hoot Index
		/// </summary>
		/// <param name="IndexPath">
		/// Path to Index File
		/// </param>
		/// <param name="FileName">
		/// File Name
		/// </param>
		/// <param name="DocMode">
		/// Document Mode
		/// </param>
		public Hoot(string IndexPath, string FileName, bool DocMode)
			: this(IndexPath, FileName, DocMode, new tokenizer())
		{

		}

		/// <summary>
		/// Construct a new Hoot Index
		/// </summary>
		/// <param name="IndexPath">
		/// </param>
		/// <param name="FileName">
		/// </param>
		/// <param name="DocMode">
		/// </param>
		/// <param name="tokenizer">
		/// </param>
		public Hoot(string indexPath, string fileName, bool docMode, ITokenizer tokenizer)
			: this(new HootConfig { IndexPath = indexPath, FileName = fileName, DocMode = docMode }, tokenizer)
		{
		}

		/// <summary>
		/// Initialize with the Configuration file
		/// </summary>
		/// <param name="config">
		/// </param>
		public Hoot(HootConfig config)
			: this(config, new tokenizer())
		{
		}

		/// <summary>
		/// Construct a new object using configuration file and custom tokenizer
		/// </summary>
		/// <param name="config">
		/// </param>
		/// <param name="tokenizer">
		/// </param>
		public Hoot(HootConfig config, ITokenizer tokenizer)
		{
			HootConfOptions = config;

			_tokenizer = (tokenizer != null) ? tokenizer : new tokenizer();
			_tokenizer.InitializeStopList(IndexPath);

			if (!Directory.Exists(IndexPath))
				Directory.CreateDirectory(IndexPath);

			_log.Debug("Starting hOOt....");
			_log.Debug($"Storage Folder = {IndexPath}");

			if (DocMode)
			{
				_docs = new KeyStoreString(Path.Combine(IndexPath, "files.docs"), false);
				//
				// read deleted
				//
				_deleted = new BoolIndex(IndexPath, "_deleted", ".hoot");

				_lastDocNum = (int)_docs.Count();
			}
			_bitmaps = new BitmapIndex(IndexPath, FileName + "_hoot.bmp");
			//
			// read words
			//
			LoadWords();
		}
		#endregion Public Constructors

		#region Public Methods

		/// <summary>
		/// Fetch a Document
		/// </summary>
		/// <typeparam name="T">
		/// Type of Document
		/// </typeparam>
		/// <param name="docnum">
		/// Document Number
		/// </param>
		/// <returns>
		/// </returns>
		public T Fetch<T>(int docnum)
		{
			string b = _docs.ReadData(docnum);
			return fastJSON.JSON.ToObject<T>(b);
		}

		/// <summary>
		/// Find Documents File Names
		/// </summary>
		/// <param name="filter">
		/// </param>
		/// <returns>
		/// </returns>
		public IEnumerable<string> FindDocumentFileNames(string filter)
		{
			checkloaded();
			MGRB bits = ExecutionPlan(filter, _docs.RecordCount());
			// enumerate documents
			foreach (int i in bits.GetBitIndexes())
			{
				if (i > _lastDocNum - 1)
					break;
				string b = _docs.ReadData(i);
				var d = (Dictionary<string, object>)fastJSON.JSON.Parse(b);

				yield return d["FileName"].ToString();
			}
		}

		/// <summary>
		/// Find Documents
		/// </summary>
		/// <typeparam name="T">
		/// </typeparam>
		/// <param name="filter">
		/// </param>
		/// <returns>
		/// </returns>
		public IEnumerable<T> FindDocuments<T>(string filter)
		{
			checkloaded();
			MGRB bits = ExecutionPlan(filter, _docs.RecordCount());
			// enumerate documents
			foreach (int i in bits.GetBitIndexes())
			{
				if (i > _lastDocNum - 1)
					break;
				string b = _docs.ReadData(i);
				T d = fastJSON.JSON.ToObject<T>(b, new fastJSON.JSONParameters { ParametricConstructorOverride = true });

				yield return d;
			}
		}

		/// <summary>
		/// Find Rows
		/// </summary>
		/// <param name="filter">
		/// </param>
		/// <returns>
		/// </returns>
		public IEnumerable<int> FindRows(string filter)
		{
			checkloaded();
			MGRB bits = ExecutionPlan(filter, _docs.RecordCount());
			// enumerate records
			return bits.GetBitIndexes();
		}

		/// <summary>
		/// Free Memory
		/// </summary>
		public void FreeMemory()
		{
			lock (_lock)
			{
				InternalSave();

				if (_deleted != null)
					_deleted.FreeMemory();

				if (_bitmaps != null)
					_bitmaps.FreeMemory();

				if (_docs != null)
					_docs.FreeMemory();

				//_words = null;// new SafeSortedList<string, int>();
				//_loaded = false;
			}
		}

		/// <summary>
		/// Index a text String
		/// </summary>
		/// <param name="recordnumber">
		/// </param>
		/// <param name="text">
		/// </param>
		public void Index(int recordnumber, string text)
		{
			Index(recordnumber, text, new NoFilter());
		}

		/// <summary>
		/// Index a Text String using a filter
		/// </summary>
		/// <param name="recordnumber">
		/// </param>
		/// <param name="text">
		/// </param>
		/// <param name="filter">
		/// </param>
		public void Index(int recordnumber, string text, IHootFilter filter)
		{
			checkloaded();
			filter.InitializeFilter(IndexPath);
			AddtoIndex(recordnumber, filter.FilterText(text));
		}

		/// <summary>
		/// Index a Document
		/// </summary>
		/// <param name="doc">
		/// </param>
		/// <param name="deleteold">
		/// </param>
		/// <returns>
		/// </returns>
		public int Index(Document doc, bool deleteold)
		{
			return (Index(doc, deleteold, new NoFilter()));
		}

		/// <summary>
		/// Index a Document
		/// </summary>
		/// <param name="doc">
		/// </param>
		/// <param name="deleteold">
		/// </param>
		/// <returns>
		/// </returns>
		public int Index(Document doc, bool deleteold, IHootFilter filter)
		{
			checkloaded();
			filter.InitializeFilter(IndexPath);

			_log.Info("Indexing Doc : " + doc.FileName);

			DateTime dt = FastDateTime.Now;

			if (deleteold && doc.DocNumber > -1)
				_deleted.Set(true, doc.DocNumber);

			if (deleteold == true || doc.DocNumber == -1)
				doc.DocNumber = _lastDocNum++;

			// save doc to disk
			string dstr = fastJSON.JSON.ToJSON(doc, new fastJSON.JSONParameters { UseExtensions = false });

			_docs.Set(doc.FileName.ToLower(), fastJSON.Reflection.UnicodeGetBytes(dstr));

			_log.Info("writing doc to disk (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);

			dt = FastDateTime.Now;

			// index doc
			AddtoIndex(doc.DocNumber, filter.FilterText(doc.Text));
			_log.Info("indexing time (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);

			return _lastDocNum;
		}

		/// <summary>
		/// Check if a File is Indexed
		/// </summary>
		/// <param name="filename">
		/// </param>
		/// <returns>
		/// </returns>
		public bool IsIndexed(string filename)
		{
			byte[] b;
			return _docs.Get(filename.ToLower(), out b);
		}

		/// <summary>
		/// Optimize the Index
		/// </summary>
		public void OptimizeIndex()
		{
			lock (_lock)
			{
				InternalSave();
				//_bitmaps.Commit(false);
				_bitmaps.Optimize();
			}
		}

		/// <summary>
		/// Query the Index
		/// </summary>
		/// <param name="filter">
		/// </param>
		/// <param name="maxsize">
		/// </param>
		/// <returns>
		/// </returns>
		public MGRB Query(string filter, int maxsize)
		{
			checkloaded();
			return ExecutionPlan(filter, maxsize);
		}

		/// <summary>
		/// Remove a Document
		/// </summary>
		/// <param name="number">
		/// </param>
		public void RemoveDocument(int number)
		{
			// add number to deleted bitmap
			_deleted.Set(true, number);
		}

		/// <summary>
		/// Remove a Document by File Name
		/// </summary>
		/// <param name="filename">
		/// </param>
		/// <returns>
		/// </returns>
		public bool RemoveDocument(string filename)
		{
			// remove doc based on filename
			byte[] b;
			if (_docs.Get(filename.ToLower(), out b))
			{
				Document d = fastJSON.JSON.ToObject<Document>(fastJSON.Reflection.UnicodeGetString(b));
				RemoveDocument(d.DocNumber);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Save the Index
		/// </summary>
		public void Save()
		{
			lock (_lock)
				InternalSave();
		}

		/// <summary>
		/// Shutdown the Process
		/// </summary>
		public void Shutdown()
		{
			lock (_lock)
			{
				if (_shutdowndone == true)
					return;

				InternalSave();
				if (_deleted != null)
				{
					_deleted.SaveIndex();
					_deleted.Shutdown();
					_deleted = null;
				}

				if (_bitmaps != null)
				{
					_bitmaps.Commit(Global.FreeBitmapMemoryOnSave);
					_bitmaps.Shutdown();
					_bitmaps = null;
				}

				if (DocMode)
					_docs.Shutdown();

				_shutdowndone = true;
			}
		}
		#endregion Public Methods

		#region Private Fields

		//private SafeSortedList<string, int> _words = new SafeSortedList<string, int>();
		private BitmapIndex _bitmaps;

		private BoolIndex _deleted;
		private KeyStoreString _docs;
		private int _lastDocNum = 0;
		private object _lock = new object();
		private ILog _log = LogManager.GetLogger(typeof(Hoot));

		private bool _shutdowndone = false;
		private ITokenizer _tokenizer;

		private SafeDictionary<string, int> _words = new SafeDictionary<string, int>();
		private bool _wordschanged = true;
		#endregion Private Fields

		#region [  P R I V A T E   M E T H O D S  ]

		/// <summary>
		/// Perform Bit Operations
		/// </summary>
		/// <param name="bits">
		/// </param>
		/// <param name="c">
		/// </param>
		/// <param name="op">
		/// </param>
		/// <param name="maxsize">
		/// </param>
		/// <returns>
		/// </returns>
		private static MGRB DoBitOperation(MGRB bits, MGRB c, OPERATION op, int maxsize)
		{
			if (bits != null)
			{
				switch (op)
				{
					case OPERATION.AND:
						bits = bits.And(c);
						break;
					case OPERATION.OR:
						bits = bits.Or(c);
						break;
					case OPERATION.ANDNOT:
						bits = bits.And(c.Not(maxsize));
						break;
				}
			}
			else
				bits = c;
			return bits;
		}

		/// <summary>
		/// Add Text to the index
		/// </summary>
		/// <param name="recnum">
		/// </param>
		/// <param name="text">
		/// </param>
		private void AddtoIndex(int recnum, string text)
		{
			if (text == "" || text == null)
				return;

			text = text.ToLowerInvariant(); // lowercase index
			string[] keys;
			if (DocMode)
			{
				//_log.Debug("text size = " + text.Length);
				Dictionary<string, int> wordfreq = _tokenizer.GenerateWordFreq(text, HootConfOptions);
				//_log.Debug("word count = " + wordfreq.Count);
				var kk = wordfreq.Keys;
				keys = new string[kk.Count];
				kk.CopyTo(keys, 0);
			}
			else
			{
				keys = text.Split(' ');
			}

			foreach (string key in keys)
			{
				if (key == "")
					continue;

				int bmp;
				if (_words.TryGetValue(key, out bmp))
				{
					_bitmaps.GetBitmap(bmp).Set(recnum, true);
				}
				else
				{
					bmp = _bitmaps.GetFreeRecordNumber();
					_bitmaps.SetDuplicate(bmp, recnum);
					_words.Add(key, bmp);
				}
			}
			_wordschanged = true;
		}

		/// <summary>
		/// Check if the Index is loaded
		/// </summary>
		private void checkloaded()
		{
			if (_wordschanged == false)
				LoadWords();
		}

		/// <summary>
		/// Generate an Execution Plan
		/// </summary>
		/// <param name="filter">
		/// </param>
		/// <param name="maxsize">
		/// </param>
		/// <returns>
		/// </returns>
		private MGRB ExecutionPlan(string filter, int maxsize)
		{
			//_log.Debug("query : " + filter);
			DateTime dt = FastDateTime.Now;
			// query indexes
			string[] words = filter.Split(' ');

			//bool defaulttoand = true;
			//if (filter.IndexOfAny(new char[] { '+', '-' }, 0) > 0)
			//    defaulttoand = false;

			MGRB found = null;// MGRB.Fill(maxsize);

			foreach (string s in words)
			{
				int c;
				bool not = false;
				string word = s;

				if (!String.IsNullOrEmpty(s))
				{
					OPERATION op = OPERATION.AND;
					//
					//	Test for OR operator
					//
					if (word.StartsWith("+"))
					{
						op = OPERATION.OR;
						word = s.Replace("+", "");
					}
					//
					// Test for AND Not operator
					//
					if (word.StartsWith("-"))
					{
						op = OPERATION.ANDNOT;
						word = s.Replace("-", "");
						not = true;

						if (found == null)      // leading with - -> "-oak hill"
							found = MGRB.Fill(maxsize);
					}
					//
					//	Test Wild Cards
					//
					if (word.Contains("*") || word.Contains("?"))
					{
						MGRB wildbits = new MGRB();
						Regex reg = new Regex("^" + word.Replace("*", ".*").Replace("?", ".") + "$", RegexOptions.IgnoreCase);

						foreach (string key in _words.Keys())
						{
							if (reg.IsMatch(key))
							{
								_words.TryGetValue(key, out c);
								MGRB ba = _bitmaps.GetBitmap(c);

								wildbits = DoBitOperation(wildbits, ba, OPERATION.OR, maxsize);
							}
						}
						if (found == null)
							found = wildbits;
						else
						{
							if (not) // "-oak -*l"
								found = found.AndNot(wildbits);
							else if (op == OPERATION.AND)
								found = found.And(wildbits);
							else
								found = found.Or(wildbits);
						}
					}
					else if (_words.TryGetValue(word.ToLowerInvariant(), out c))
					{
						// bits logic
						MGRB ba = _bitmaps.GetBitmap(c);

						found = DoBitOperation(found, ba, op, maxsize);
					}
					else if (op == OPERATION.AND)
						found = new MGRB();
				}
			}
			if (found == null)
				return new MGRB();
			//
			// remove deleted docs
			//
			MGRB ret;

			if (DocMode)
				ret = found.AndNot(_deleted.GetBits());
			else
				ret = found;
			//_log.Debug("query time (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);
			return ret;
		}

		/// <summary>
		/// Save the Index
		/// </summary>
		private void InternalSave()
		{
			_log.Info("saving index...");
			DateTime dt = FastDateTime.Now;
			// save deleted
			if (_deleted != null)
				_deleted.SaveIndex();

			// save docs
			if (DocMode)
				_docs.SaveIndex();

			if (_bitmaps != null)
				_bitmaps.Commit(true);

			if (_words != null && _wordschanged == true)
			{
				// save words and bitmaps
				using (FileStream words = new FileStream(Path.Combine(IndexPath, $"{FileName}.words"), FileMode.Create))
				{
					using (BinaryWriter bw = new BinaryWriter(words, Encoding.UTF8))
					{
						foreach (string key in _words.Keys())
						{
							bw.Write(key);
							bw.Write(_words[key]);
						}
					}
				}
				_wordschanged = false;
			}
			_log.Info("save time (ms) = " + FastDateTime.Now.Subtract(dt).TotalMilliseconds);
		}

		/// <summary>
		/// Load Words
		/// </summary>
		private void LoadWords()
		{
			lock (_lock)
			{
				if (_words == null)
					_words = new SafeDictionary<string, int>();

				// new SafeSortedList<string, int>();

				if (File.Exists(Path.Combine(IndexPath, $"{FileName}.words")) == false)
					return;

				// load words
				using (FileStream words = new FileStream(Path.Combine(IndexPath, $"{FileName}.words"), FileMode.Open))
				{
					if (words.Length == 0)
						return;

					using (BinaryReader br = new BinaryReader(words, Encoding.UTF8))
					{
						string s = br.ReadString();

						while (s != "")
						{
							int off = br.ReadInt32();
							_words.Add(s, off);
							try
							{
								s = br.ReadString();
							}
							catch { s = ""; }
						}
					}
				}
				//byte[] b = File.ReadAllBytes(_Path + _FileName + ".words");
				//if (b.Length == 0)
				//    return;
				//MemoryStream ms = new MemoryStream(b);
				//BinaryReader br = new BinaryReader(ms, Encoding.UTF8);
				//string s = br.ReadString();
				//while (s != "")
				//{
				//    int off = br.ReadInt32();
				//    _words.Add(s, off);
				//    try
				//    {
				//        s = br.ReadString();
				//    }
				//    catch { s = ""; }
				//}
				_log.Debug("Word Count = " + _words.Count());
				_wordschanged = true;
			}
		}
		#endregion [  P R I V A T E   M E T H O D S  ]
	}
}