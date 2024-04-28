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
	/// 规则范围
	/// </summary>
	public class Span
	{
		bool stopEOL;
		ColorStyle color;
		ColorStyle beginColor = null;
		ColorStyle endColor = null;
		char[] begin = null;
		char[] end = null;
		string name = null;
		string rule = null;
		HighlightRuleSet ruleSet = null;
		bool noEscapeSequences = false;

		internal HighlightRuleSet RuleSet
		{
			get
			{
				return ruleSet;
			}
			set
			{
				ruleSet = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool StopEOL
		{
			get
			{
				return stopEOL;
			}
		}

		/// <summary>
		/// 高亮颜色
		/// </summary>
		public ColorStyle Color
		{
			get
			{
				return color;
			}
		}

		/// <summary>
		/// 高亮开始颜色
		/// </summary>
		public ColorStyle BeginColor
		{
			get
			{
				if (beginColor != null)
				{
					return beginColor;
				}
				else
				{
					return color;
				}
			}
		}

		/// <summary>
		/// 高亮结束颜色
		/// </summary>
		public ColorStyle EndColor
		{
			get
			{
				return endColor != null ? endColor : color;
			}
		}

		/// <summary>
		/// 开始符号
		/// </summary>
		public char[] Begin
		{
			get
			{
				return begin;
			}
		}

		/// <summary>
		/// 结束符号
		/// </summary>
		public char[] End
		{
			get
			{
				return end;
			}
		}

		/// <summary>
		/// 范围名称
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
		}

		/// <summary>
		/// 指向目标的规则名称
		/// </summary>
		public string Rule
		{
			get
			{
				return rule;
			}
		}

		/// <summary>
		/// 无转义序列
		/// </summary>
		public bool NoEscapeSequences
		{
			get
			{
				return noEscapeSequences;
			}
		}

		public Span(XmlElement span)
		{
			if (span.HasAttribute("ref:color"))
			{
				string n = span.GetAttribute("ref:color");
				this.color = StyleManager.GlobalStyle.GetColorStyle(n);
			}
			else
				color = new ColorStyle(span);

			if (span.Attributes["rule"] != null)
			{
				rule = span.Attributes["rule"].InnerText;
			}

			if (span.Attributes["noescapesequences"] != null)
			{
				noEscapeSequences = Boolean.Parse(span.Attributes["noescapesequences"].InnerText);
			}

			name = span.Attributes["name"].InnerText;
			stopEOL = Boolean.Parse(span.Attributes["stopateol"].InnerText);
			begin = span["Begin"].InnerText.ToCharArray();
			if (span.HasAttribute("ref:color"))
			{
				string n = span.GetAttribute("ref:color");
				this.beginColor = StyleManager.GlobalStyle.GetColorStyle(n);
			}
			else
				beginColor = new ColorStyle(span["Begin"], color);

			if (span["End"] != null)
			{
				end = span["End"].InnerText.ToCharArray();
				if (span.HasAttribute("ref:color"))
				{
					string n = span.GetAttribute("ref:color");
					this.endColor = StyleManager.GlobalStyle.GetColorStyle(n);
				}
				else
					endColor = new ColorStyle(span["End"], color);
			}
		}
	}
}
