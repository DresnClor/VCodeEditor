// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;

namespace MeltuiCodeEditor.Document
{
	public delegate void LineManagerEventHandler(object sender, LineManagerEventArgs e);

	public delegate void LineLengthEventHandler(object sender, LineLengthEventArgs e);
	
	/// <summary>
	/// 行管理事件参数
	/// </summary>
	public class LineManagerEventArgs : EventArgs
	{
		IDocument document;
		int       start;
		int       moved;
		
		/// <returns>
		/// 始终是与事件相关的有效文档
		/// </returns>
		public IDocument Document {
			get {
				return document;
			}
		}
		
		/// <returns>
		/// -1，如果没有为此事件指定偏移
		/// </returns>
		public int LineStart {
			get {
				return start;
			}
		}

		/// <returns>
		/// -1，如果没有为此事件指定偏移
		/// </returns>
		public int LinesMoved {
			get {
				return moved;
			}
		}
		
		public LineManagerEventArgs(IDocument document, int lineStart, int linesMoved)
		{
			this.document = document;
			this.start    = lineStart;
			this.moved    = linesMoved;
		}
	}
	
	/// <summary>
	/// 行长度事件参数
	/// </summary>
	public class LineLengthEventArgs : EventArgs
	{
		IDocument document;
		int       lineNumber;
		int       lineOffset;
		int       moved;
		
		public IDocument Document {
			get {
				return document;
			}
		}
		
		public int LineNumber {
			get {
				return lineNumber;
			}
		}
		
		public int LineOffset {
			get {
				return lineOffset;
			}
		}
		
		public int Moved {
			get {
				return moved;
			}
		}
		
		public LineLengthEventArgs(IDocument document, int lineNumber, int lineOffset, int moved)
		{
			this.document = document;
			this.lineNumber = lineNumber;
			this.lineOffset = lineOffset;
			this.moved = moved;
		}
		
		public override string ToString()
		{
			return String.Format("[LineLengthEventArgs: Document = {0}, LineNumber = {1}, LineOffset = {2}, Moved = {3}]",
			                     Document,
			                     LineNumber,
			                     LineOffset,
			                     Moved);
		}
		
	}
}
