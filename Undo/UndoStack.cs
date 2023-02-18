// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;

namespace VCodeEditor.Undo
{
	/// <summary>
	/// 撤消堆栈
	/// </summary>
	public class UndoStack
	{
		Stack undostack = new Stack();
		Stack redostack = new Stack();
		
		public TextEditorControlBase TextEditorControl = null;
		
		/// <summary>
		/// </summary>
		public event EventHandler ActionUndone;
		/// <summary>
		/// </summary>
		public event EventHandler ActionRedone;
		
		/// <summary>
		/// </summary>
		public bool AcceptChanges = true;
		
		/// <summary>
		///
		/// </summary>
		internal Stack _UndoStack {
			get {
				return undostack;
			}
		}
		
		/// <summary>
		/// 是否允许撤销
		/// </summary>
		public bool CanUndo {
			get {
				return undostack.Count > 0;
			}
		}

		/// <summary>
		/// 是否允许重做
		/// </summary>
		public bool CanRedo {
			get {
				return redostack.Count > 0;
			}
		}
		
		/// <summary>
		/// 您调用此方法来汇集撤消堆栈中的最后 x 操作 从它做1个操作。
		/// </summary>
		public void UndoLast(int x)
		{
			undostack.Push(new UndoQueue(this, x));
		}
		
		/// <summary>
		/// 撤消
		/// </summary>
		public void Undo()
		{
			if (undostack.Count > 0) {
				IUndoableOperation uedit = (IUndoableOperation)undostack.Pop();
				redostack.Push(uedit);
				uedit.Undo();
				OnActionUndone();
			}
		}
		
		/// <summary>
		/// 重做
		/// </summary>
		public void Redo()
		{
			if (redostack.Count > 0) {
				IUndoableOperation uedit = (IUndoableOperation)redostack.Pop();
				undostack.Push(uedit);
				uedit.Redo();
				OnActionRedone();
			}
		}
		
		/// <summary>
		/// 调用此方法以推动撤消堆上的不可操作操作，重做将清除，如果你使用这种方法。
		/// </summary>
		public void Push(IUndoableOperation operation) 
		{
			if (operation == null) {
				throw new ArgumentNullException("UndoStack.Push(UndoableOperation operation) : operation can't be null");
			}
			
			if (AcceptChanges) {
				undostack.Push(operation);
				if (TextEditorControl != null) {
					undostack.Push(new UndoableSetCaretPosition(this, TextEditorControl.ActiveTextAreaControl.Caret.Position));
					UndoLast(2);
				}
				ClearRedoStack();
			}
		}
		
		/// <summary>
		/// 清除重做栈
		/// </summary>
		public void ClearRedoStack()
		{
			redostack.Clear();
		}
		
		/// <summary>
		/// 清除所有
		/// </summary>
		public void ClearAll()
		{
			undostack.Clear();
			redostack.Clear();
		}
		
		/// <summary>
		/// </summary>
		protected void OnActionUndone()
		{
			if (ActionUndone != null) {
				ActionUndone(null, null);
			}
		}
		
		/// <summary>
		/// </summary>
		protected void OnActionRedone()
		{
			if (ActionRedone != null) {
				ActionRedone(null, null);
			}
		}
		
		class UndoableSetCaretPosition : IUndoableOperation
		{
			UndoStack stack;
			Point pos;
			Point redoPos;
			
			public UndoableSetCaretPosition(UndoStack stack, Point pos)
			{
				this.stack = stack;
				this.pos = pos;
			}
			
			public void Undo()
			{
				redoPos = stack.TextEditorControl.ActiveTextAreaControl.Caret.Position;
				stack.TextEditorControl.ActiveTextAreaControl.Caret.Position = pos;
			}
			
			public void Redo()
			{
				stack.TextEditorControl.ActiveTextAreaControl.Caret.Position = redoPos;
			}
		}
	}
}
