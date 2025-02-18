﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 使用背景图像扩展高亮颜色
	/// </summary>
	public class HighlightBackground : ColorStyle
	{
		Image backgroundImage;

		/// <value>
		/// 背景图像
		/// </value>
		public Image BackgroundImage {
			get {
				return backgroundImage;
			}
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="HighlightBackground"/>
		/// </summary>
		public HighlightBackground(XmlElement el) : base(el)
		{
			if (el.Attributes["image"] != null) {
				backgroundImage = new Bitmap(el.Attributes["image"].InnerText);
			}
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="HighlightBackground"/>
		/// </summary>
		public HighlightBackground(Color color, Color backgroundcolor, bool bold, bool italic) : base(color, backgroundcolor, bold, italic)
		{
		}
		
		public HighlightBackground(string systemColor, string systemBackgroundColor, bool bold, bool italic) : base(systemColor, systemBackgroundColor, bold, italic)
		{
		}
	}
}
