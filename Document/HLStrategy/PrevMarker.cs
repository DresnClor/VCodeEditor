// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 用于标记上一个令牌
	/// </summary>
	public class PrevMarker
	{
		string      what;
		HighlightStyle color;
		bool        markMarker = false;
		
		/// <value>
		/// String value to indicate to mark previous token
		/// </value>
		public string What {
			get {
				return what;
			}
		}
		
		/// <value>
		/// Color for marking previous token
		/// </value>
		public HighlightStyle Color {
			get {
				return color;
			}
		}
		
		/// <value>
		/// If true the indication text will be marked with the same color
		/// too
		/// </value>
		public bool MarkMarker {
			get {
				return markMarker;
			}
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="PrevMarker"/>
		/// </summary>
		public PrevMarker(XmlElement mark)
		{
			color = new HighlightStyle(mark);
			what  = mark.InnerText;
			if (mark.Attributes["markmarker"] != null) {
				markMarker = Boolean.Parse(mark.Attributes["markmarker"].InnerText);
			}
		}
	}

}
