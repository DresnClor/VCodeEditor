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
		string what;
		ColorStyle color;
		bool markMarker = false;

		/// <value>
		/// 指示标记上一个标记的字符串值
		/// </value>
		public string What
		{
			get
			{
				return what;
			}
		}

		/// <value>
		/// 高亮样式
		/// </value>
		public ColorStyle Color
		{
			get
			{
				return color;
			}
		}

		/// <value>
		/// 如果指示文本将用相同的颜色标记，返回true
		/// </value>
		public bool MarkMarker
		{
			get
			{
				return markMarker;
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="PrevMarker"/>
		/// </summary>
		public PrevMarker(XmlElement mark)
		{
			if (mark.HasAttribute("ref:color"))
			{
				string n = mark.GetAttribute("ref:color");
				this.color = StyleManager.GlobalStyle.GetColorStyle(n);
			}
			else
				color = new ColorStyle(mark);
			what = mark.InnerText;
			if (mark.Attributes["markmarker"] != null)
			{
				markMarker = Boolean.Parse(mark.Attributes["markmarker"].InnerText);
			}
		}
	}

}
