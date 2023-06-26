// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

using MeltuiCodeEditor.Document;

namespace MeltuiCodeEditor
{
	/// <summary>
	/// 高亮括号
	/// </summary>
	public class HighlightBracket
	{
		Point openBrace;
		Point closeBrace;
		
		/// <summary>
		/// 打开括号
		/// </summary>
		public Point OpenBrace {
			get {
				return openBrace;
			}
			set {
				openBrace = value;
			}
		}

		/// <summary>
		/// 关闭括号
		/// </summary>
		public Point CloseBrace {
			get {
				return closeBrace;
			}
			set {
				closeBrace = value;
			}
		}
		
		public HighlightBracket(Point openBrace, Point closeBrace)
		{
			this.openBrace = openBrace;
			this.closeBrace = closeBrace;
		}
	}
	
	/// <summary>
	/// 括号高亮类型
	/// </summary>
	public class BracketHighlightingSheme
	{
		char opentag;
		char closingtag;
		
		/// <summary>
		/// 打开标签
		/// </summary>
		public char OpenTag {
			get {
				return opentag;
			}
			set {
				opentag = value;
			}
		}
		
		/// <summary>
		/// 关闭标签
		/// </summary>
		public char ClosingTag {
			get {
				return closingtag;
			}
			set {
				closingtag = value;
			}
		}
		
		public BracketHighlightingSheme(char opentag, char closingtag)
		{
			this.opentag    = opentag;
			this.closingtag = closingtag;
		}
		
		/// <summary>
		/// 获取括号高亮
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public HighlightBracket GetHighlight(IDocument document, int offset)
		{
			int searchOffset;
			if (document.TextEditorProperties.BracketMatchingStyle == BracketMatchingStyle.After) {
				searchOffset = offset;
			} else {
				searchOffset = offset + 1;
			}
			char word = document.GetCharAt(Math.Max(0, Math.Min(document.TextLength - 1, searchOffset)));
			
			Point endP = document.OffsetToPosition(offset);
			if (word == opentag) {
				if (offset < document.TextLength) {
					int bracketOffset = TextUtilities.SearchBracketForward(document, searchOffset + 1, opentag, closingtag);
					if (bracketOffset >= 0) {
						Point p = document.OffsetToPosition(bracketOffset);
						return new HighlightBracket(p, endP);
					}
				}
			} else if (word == closingtag) {
				if (offset > 0) {
					int bracketOffset = TextUtilities.SearchBracketBackward(document, searchOffset - 1, opentag, closingtag);
					if (bracketOffset >= 0) {
						Point p = document.OffsetToPosition(bracketOffset);
						return new HighlightBracket(p, endP);
					}
				}
			}
			return null;
		}
	}
}
