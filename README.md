Hoot.Standard
=============

# Hoot.Standard
Hoot Full Text Engine for .Net Standard and .Net Core

#Description
Smallest full text search engine (lucene replacement) built from scratch using inverted WAH bitmap index, highly compact storage, operating in database and document modes.

## Original Project
The original project by Mehdi Gholam from (https://github.com/mgholam/hOOt) has been ported to .DotNet Standard 2.1 for use in Projects including .Net Core 3.1
See the original article here : [hOOt full text search engine](http://www.codeproject.com/Articles/224722/hOOt-full-text-search-engine)

## Enhancements

A number of enhancements have been made from the original version and are listed below:

- New configuration class has been added to support ASP.NET Core Web Applications.

- Text Filters for filtering input text can now be defined.

- A Html Text Filter has been added to remove Html markup.

- The sample project is now a .Net Core 3.1 WinForms application.

- A Stop List has been added to the ITokenizer interface.
Added new option to Filter Numeric values from word list

## Configuration File
Hoot Configuration file that can be initialized and passed to hoot Constructor

    	public class HootConfig
    	{
    		/// <summary>
    		/// Path where index files are stored
    		/// </summary>
    		public String IndexPath { get; set; }
    		/// <summary>
    		/// Filename prefix for index files. 
    		/// Defaults to indexx
    		/// </summary>
    		public String FileName { get; set; }
    		/// <summary>
    		/// Use Document Mode
    		/// </summary>
    		public bool DocMode { get; set; }
    		/// <summary>
    		/// Use Word Stop List
    		/// </summary>
    		public bool UseStopList { get; set; }
    		/// <summary>
    		/// Ignore numeric words, ie 123,555, etc
    		/// </summary>
    		public bool IgnoreNumerics { get; set; }
    	}

