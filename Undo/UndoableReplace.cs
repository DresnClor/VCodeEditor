// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Diagnostics;
using System.Drawing;
using VCodeEditor.Document;
using VCodeEditor.Undo;

namespace VCodeEditor.Undo
{
	/// <summary>
	/// 此类用于撤销文档插入操作
	/// </summary>
	public class UndoableReplace : IUndoableOperation
	{
		IDocument document;
//		int       oldCaretPos;
		int       offset;
		string    text;
		string    origText;
		
		/// <summary>
		/// Creates a new instance of <see cref="UndoableReplace"/>
		/// </summary>	
		public UndoableReplace(IDocument document, int offset, string origText, string text)
		{
			if (document == null) {
				throw new ArgumentNullException("document");
			}
			if (offset < 0 || offset > document.TextLength) {
				throw new ArgumentOutOfRangeException("offset");
			}
			
			Debug.Assert(text != null, "text can't be null");
//			oldCaretPos   = document.Caret.Offset;
			this.document = document;
			this.offset   = offset;
			this.text     = text;
			this.origText = origText;
		}
		
		/// <remarks>
		/// 撤消上次操作
		/// </remarks>
		public void Undo()
		{
			// 我们直接清除所有选择，因为重绘
			//每次刷新在行动结束时完成
//			document.SelectionCollection.Clear();

			document.UndoStack.AcceptChanges = false;
			document.Replace(offset, text.Length, origText);
//			document.Caret.Offset = Math.Min(document.TextLength, Math.Max(0, oldCaretPos));
			document.UndoStack.AcceptChanges = true;
		}
		
		/// <remarks>
		/// 重做上次撤消操作
		/// </remarks>
		public void Redo()
		{
			// 我们直接清除所有选择，因为重绘
			//每次刷新在行动结束时完成
//			document.SelectionCollection.Clear();

			document.UndoStack.AcceptChanges = false;
			document.Replace(offset, origText.Length, text);
//			document.Caret.Offset = Math.Min(document.TextLength, Math.Max(0, document.Caret.Offset));
			document.UndoStack.AcceptChanges = true;
		}
	}
}
