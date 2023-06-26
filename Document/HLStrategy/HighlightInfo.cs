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

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 高亮信息
	/// </summary>
	public class HighlightInfo
	{
        /// <summary>
        /// 块范围打开
        /// </summary>
        public bool BlockSpanOn = false;
		/// <summary>
		/// 范围
		/// </summary>
		public bool Span        = false;
		/// <summary>
		/// 当前范围
		/// </summary>
		public Span CurSpan     = null;
		
		public HighlightInfo(Span curSpan, bool span, bool blockSpanOn)
		{
			this.CurSpan     = curSpan;
			this.Span        = span;
			this.BlockSpanOn = blockSpanOn;
		}
	}
}
