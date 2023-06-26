// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Diagnostics;
using System.Collections;

namespace MeltuiCodeEditor.Undo
{
	/// <summary>
	/// 此类堆栈从撤消堆栈的最后 x 操作，使一个撤消/重做操作从它。
	/// </summary>
	public class UndoQueue : IUndoableOperation
	{
		ArrayList undolist = new ArrayList();
		
		/// <summary>
		/// </summary>
		public UndoQueue(UndoStack stack, int numops)
		{
			if (stack == null)  {
				throw new ArgumentNullException("stack");
			}
			
			Debug.Assert(numops > 0 , "VCodeEditor.Undo.UndoQueue : numops should be > 0");
			
			for (int i = 0; i < numops; ++i) {
				if (stack._UndoStack.Count > 0) {
					undolist.Add(stack._UndoStack.Pop());
				}
			}
		}
		public void Undo()
		{
			for (int i = 0; i < undolist.Count; ++i) {
				((IUndoableOperation)undolist[i]).Undo();
			}
		}
		
		public void Redo()
		{
			for (int i = undolist.Count - 1 ; i >= 0 ; --i) {
				((IUndoableOperation)undolist[i]).Redo();
			}
		}		
	}
}
