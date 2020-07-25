using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using HtmlAgilityPack;

namespace RaptorDB.Filters
{
	public class HtmlFilter : IHootFilter
	{
		private HtmlDocument _doc;
		/// <summary>
		/// Filter Html Text
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string FilterText(string input)
		{
			_doc = new HtmlDocument();

			_doc.LoadHtml(input);
			return(ConvertDoc(_doc));
		}
		/// <summary>
		/// Initialize the filter, Not used
		/// </summary>
		/// <param name="filterPath"></param>
		public void InitializeFilter(string filterPath = null)
		{

		}
		/// <summary>
		/// Use the HtmlAgilityPack to Convert to Plain Text
		/// </summary>
		/// <param name="doc"></param>
		/// <returns></returns>
		private String ConvertDoc(HtmlDocument doc)
		{
			using (StringWriter _sw = new StringWriter())
			{
				ConvertTo(doc.DocumentNode, _sw);
				_sw.Flush();
				return(_sw.ToString());
			}
		}
		/// <summary>
		/// Do Convertion here
		/// </summary>
		/// <param name="documentNode"></param>
		/// <param name="sw"></param>
		private void ConvertTo(HtmlNode node, StringWriter outText)
		{
			foreach (HtmlNode subnode in node.ChildNodes)
				ConvertLineTo(subnode, outText);
		}
		/// <summary>
		/// Convert a Single Line to Text
		/// </summary>
		/// <param name="subnode"></param>
		/// <param name="outText"></param>
		private void ConvertLineTo(HtmlNode node, StringWriter outText)
		{
			string html;

			switch (node.NodeType)
			{
				//
				//	Don't convert comments
				//
				case HtmlNodeType.Comment:
					break;
				case HtmlNodeType.Document:
					ConvertTo(node, outText);
					break;

				case HtmlNodeType.Text:
					// script and style must not be output
					string parentName = node.ParentNode.Name;

					if ((parentName == "script") || (parentName == "style"))
						break;

					// get text
					html = ((HtmlTextNode)node).Text;

					// is it in fact a special closing node output as text?
					if (HtmlNode.IsOverlappedClosingElement(html))
						break;

					// check the text is meaningful and not a bunch of whitespaces
					if (html.Trim().Length > 0)
					{
						outText.Write(HtmlEntity.DeEntitize(html));
					}
					break;

				case HtmlNodeType.Element:
					switch (node.Name)
					{
						case "p":
							// treat paragraphs as crlf
							outText.Write("\r\n");
							break;
						case "br":
							outText.Write("\r\n");
							break;
					}

					if (node.HasChildNodes)
					{
						ConvertTo(node, outText);
					}
					break;
			}
		}
	}
}
