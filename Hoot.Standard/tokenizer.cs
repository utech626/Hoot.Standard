using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace RaptorDB
{
	public class tokenizer : ITokenizer
	{
		internal static HashSet<String> m_stopWords = null;

		public Dictionary<string, int> GenerateWordFreq(string text, HootConfig config)
		{
			Dictionary<string, int> dic = new Dictionary<string, int>(500);

			char[] chars = text.ToCharArray();
			int index = 0;
			int look = 0;
			int count = chars.Length;
			int lastlang = langtype(chars[0]);

			while (index < count)
			{
				int lang = -1;
				while (look < count)
				{
					char c = chars[look];
					lang = langtype(c);
					if (lang == lastlang)
						look++;
					else
						break;
				}
				if (lastlang > -1)
					ParseString(dic, chars, look, index, config);

				index = look;
				lastlang = lang;
			}
			return dic;
		}
		/// <summary>
		/// Initialize the Stop List
		/// </summary>
		/// <param name="indexFolder"></param>
		public void InitializeStopList(String indexFolder)
		{
			String _stopWordFile = Path.Combine(indexFolder, "stoplist.words");

			if (tokenizer.m_stopWords == null)
				m_stopWords = new HashSet<string>();

			//
			// Check Needed for Unit Tests
			//
			if (!Directory.Exists(indexFolder))
				Directory.CreateDirectory(indexFolder);

			if (!File.Exists(_stopWordFile))
			{
				String[] _words = Properties.Resources.StopList.Split(new char[] { ',' });

				using (StreamWriter _wr = File.CreateText(_stopWordFile))
					foreach (String _wd in _words)
						_wr.WriteLine(_wd);
			}
			using (StreamReader _rdr = File.OpenText(_stopWordFile))
			{
				String _buf;

				while ((_buf = _rdr.ReadLine()) != null)
					tokenizer.m_stopWords.Add(_buf.ToLower().Trim());
			}
		}
		/// <summary>
		/// Check for language specific characters
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private static int langtype(char c)
		{
			if (char.IsDigit(c))
				return 0;

			else if (char.IsWhiteSpace(c))
				return -1;

			else if (char.IsPunctuation(c))
				return -1;

			else if (char.IsLetter(c)) // FEATURE : language checking here
				return 1;

			else
				return -1;
		}

		private static void ParseString(Dictionary<string, int> dic, char[] chars, int end, int start, HootConfig config)
		{
			// check if upper lower case mix -> extract words
			int uppers = 0;
			bool found = false;

			for (int i = start; i < end; i++)
			{
				if (char.IsUpper(chars[i]))
					uppers++;
			}
			// not all uppercase
			if (uppers != end - start - 1)
			{
				int lastUpper = start;

				string word = "";

				for (int i = start + 1; i < end; i++)
				{
					char c = chars[i];
					if (char.IsUpper(c))
					{
						found = true;
						word = new string(chars, lastUpper, i - lastUpper).ToLowerInvariant().Trim();
						
						AddDictionary(dic, word, config);
						lastUpper = i;
					}
				}

				if (lastUpper > start)
				{
					string last = new string(chars, lastUpper, end - lastUpper).ToLowerInvariant().Trim();

					if (word != last)
						AddDictionary(dic, last, config);
				}
			}
			if (found == false)
			{
				string s = new string(chars, start, end - start).ToLowerInvariant().Trim();
				AddDictionary(dic, s, config);
			}
		}
		/// <summary>
		/// Add a word to the dictionary
		/// </summary>
		/// <param name="dic"></param>
		/// <param name="word"></param>
		private static void AddDictionary(Dictionary<string, int> dic, string word, HootConfig config)
		{
			if (word == null)
				return;

			int l = word.Length;
			//
			// too long
			//
			if (l > Global.DefaultStringKeySize)
				return;
			//
			// too short
			//
			if (l < 2)
				return;

			if (config.IgnoreNumerics)
				if (wordIsNumeric(word))
					return;

			if (config.UseStopList)
				if (m_stopWords.Contains(word))
					return;

			addword(dic, word);
		}
		/// <summary>
		/// Add word to dictionary
		/// </summary>
		/// <param name="dic"></param>
		/// <param name="word"></param>
		private static void addword(Dictionary<string, int> dic, string word)
		{
			int cc = 0;

			if (dic.TryGetValue(word, out cc))
				dic[word] = ++cc;
			else
				dic.Add(word, 1);
		}
		/// <summary>
		/// Check is word is pure numeric 
		/// </summary>
		/// <param name="word"></param>
		/// <returns>True if Numeric</returns>
		private static bool wordIsNumeric(String word)
		{
			bool _isnumeric = true;

			foreach (char _ch in word.ToCharArray())
			{
				if (!Char.IsDigit(_ch))
				{
					_isnumeric = false;
					break;
				}
			}
			return (_isnumeric);
		}
	}
}
