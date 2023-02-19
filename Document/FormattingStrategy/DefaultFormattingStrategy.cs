// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Text;


namespace VCodeEditor.Document
{
	/// <summary>
	/// 默认格式化策略
	/// </summary>
	public class DefaultFormattingStrategy : IFormattingStrategy
	{
		/// <summary>
		/// Creates a new instance off <see cref="DefaultFormattingStrategy"/>
		/// </summary>
		public DefaultFormattingStrategy()
		{
		}
		
		/// <summary>
		/// 返回行中非空白字符之前的空白作为一个字符串。
		/// </summary>
		protected string GetIndentation(TextArea textArea, int lineNumber)
		{
			if (lineNumber < 0 || lineNumber > textArea.Document.TotalNumberOfLines) {
				throw new ArgumentOutOfRangeException("lineNumber");
			}
			
			string lineText = TextUtilities.GetLineAsString(textArea.Document, lineNumber);
			StringBuilder whitespaces = new StringBuilder();
			
			foreach (char ch in lineText) {
				if (Char.IsWhiteSpace(ch)) {
					whitespaces.Append(ch);
				} else {
					break;
				}
			}
			return whitespaces.ToString();
		}

		/// <summary>
		/// 自动缩进行
		/// </summary>
		protected virtual int AutoIndentLine(TextArea textArea, int lineNumber)
		{
			string indentation = lineNumber != 0 ? GetIndentation(textArea, lineNumber - 1) : "";
			if(indentation.Length > 0) {
				string newLineText = indentation + TextUtilities.GetLineAsString(textArea.Document, lineNumber).Trim();
				LineSegment oldLine  = textArea.Document.GetLineSegment(lineNumber);
				textArea.Document.Replace(oldLine.Offset, oldLine.Length, newLineText);
			}
			return indentation.Length;
		}
		
		/// <summary>
		/// 可以覆盖以定义更复杂的缩进。
		/// </summary>
		protected virtual int SmartIndentLine(TextArea textArea, int line)
		{
			return AutoIndentLine(textArea, line); // smart = autoindent in normal texts
		}
		
		/// <summary>
		///  此功能在之后对特定行进行格式化<code>ch</code> is pressed.
		/// </summary>
		/// <returns>
		/// 护理三角洲的位置照顾将移动此号码
		///字节（例如，在护理之前插入的字节数，或
		///删除，如果此数字为负数）
		///the caret delta position the caret will be moved this number
		/// of bytes (e.g. the number of bytes inserted before the caret, or
		/// removed, if this number is negative)
		/// </returns>
		public virtual int FormatLine(TextArea textArea, int line, int cursorOffset, char ch)
		{
			if (ch == '\n') {
				return IndentLine(textArea, line);
			}
			return 0;
		}

		/// <summary>
		/// 缩进行
		/// </summary>
		/// <returns>
		/// 插入字符的数量。
		/// </returns>
		public int IndentLine(TextArea textArea, int line)
		{
			switch (textArea.Document.TextEditorProperties.IndentStyle) {
				case IndentStyle.None:
					break;
				case IndentStyle.Auto:
					return AutoIndentLine(textArea, line);
				case IndentStyle.Smart:
					return SmartIndentLine(textArea, line);
			}
			return 0;
		}

		/// <summary>
		/// 缩进行
		/// </summary>
		public virtual void IndentLines(TextArea textArea, int begin, int end)
		{
			int redocounter = 0;
			for (int i = begin; i <= end; ++i) {
				if (IndentLine(textArea, i) > 0) {
					++redocounter;
				}
			}
			if (redocounter > 0) {
				textArea.Document.UndoStack.UndoLast(redocounter);
			}
		}

		/// <summary>
		/// 向后查找括号
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <param name="openBracket"></param>
		/// <param name="closingBracket"></param>
		/// <returns></returns>
		public virtual int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			int brackets = -1;
			// 首先尝试"快速查找"-找到匹配的支架，如果没有字符串/注释的方式
			for (int i = offset; i > 0; --i) {
				char ch = document.GetCharAt(i);
				if (ch == openBracket) {
					++brackets;
					if (brackets == 0) return i;
				} else if (ch == closingBracket) {
					--brackets;
				} else if (ch == '"') {
					break;
				} else if (ch == '\'') {
					break;
				} else if (ch == '/' && i > 0) {
					if (document.GetCharAt(i - 1) == '/') break;
					if (document.GetCharAt(i - 1) == '*') break;
				}
			}
			return -1;
		}
		
		/// <summary>
		/// 向前查找括号
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <param name="openBracket"></param>
		/// <param name="closingBracket"></param>
		/// <returns></returns>
		public virtual int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			int brackets = 1;
			// 尝试"快速查找"-找到匹配的支架，如果没有字符串/评论的方式
			for (int i = offset; i < document.TextLength; ++i) {
				char ch = document.GetCharAt(i);
				if (ch == openBracket) {
					++brackets;
				} else if (ch == closingBracket) {
					--brackets;
					if (brackets == 0) return i;
				} else if (ch == '"') {
					break;
				} else if (ch == '\'') {
					break;
				} else if (ch == '/' && i > 0) {
					if (document.GetCharAt(i - 1) == '/') break;
				} else if (ch == '*' && i > 0) {
					if (document.GetCharAt(i - 1) == '/') break;
				}
			}
			return -1;
		}
	}
}
