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
	/// 此类用于撤销文档删除操作
	/// </summary>
	public class UndoableDelete : IUndoableOperation
	{
		IDocument document;
//		int      oldCaretPos;
		int      offset;
		string   text;
		
		/// <summary>
		/// Creates a new instance of <see cref="UndoableDelete"/>
		/// </summary>	
		public UndoableDelete(IDocument document, int offset, string text)
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
		}
		
		/// <remarks>
		/// 撤消上次操作
		/// </remarks>
		public void Undo()
		{
			// we clear all selection direct, because the redraw
			// is done per refresh at the end of the action
//			textArea.SelectionManager.SelectionCollection.Clear();
			document.UndoStack.AcceptChanges = false;
			document.Insert(offset, text);
//			document.Caret.Offset = Math.Min(document.TextLength, Math.Max(0, oldCaretPos));
			document.UndoStack.AcceptChanges = true;
		}
		
		/// <remarks>
		///重做上次撤消操作
		/// </remarks>
		public void Redo()
		{
			// we clear all selection direct, because the redraw
			// is done per refresh at the end of the action
//			textArea.SelectionManager.SelectionCollection.Clear();

			document.UndoStack.AcceptChanges = false;
			document.Remove(offset, text.Length);
//			document.Caret.Offset = Math.Min(document.TextLength, Math.Max(0, document.Caret.Offset));
			document.UndoStack.AcceptChanges = true;
		}
	}
}
