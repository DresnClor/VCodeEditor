 using System.Drawing;
using VCodeEditor.Document;

//代码选择相关

namespace VCodeEditor.Actions
{
	/// <summary>
	/// Shift + →键（以光标为起点向右选择字符，每次选这一个字符）
	/// </summary>
	public class ShiftCaretRight : CaretRight
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + ←键（以光标为起点向左选择字符，每次选这一个字符）
	/// </summary>
	public class ShiftCaretLeft : CaretLeft
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + ↑键（以当前光标为起点，向上选择字符，每次向上移动一行）
	/// </summary>
	public class ShiftCaretUp : CaretUp
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + ↓键（以当前光标为起点，向下选择字符，每次向下移动一行）
	/// </summary>
	public class ShiftCaretDown : CaretDown
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Ctrl + Shift + →键（以当前光标为起点，按照词组为单位向右选择文本，每次选择一组文本）
	/// </summary>
	public class ShiftWordRight : WordRight
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Ctrl + Shift + ←键（以当前光标为起点，按照词组为单位向左选择文本，每次选择一组文本）
	/// </summary>
	public class ShiftWordLeft : WordLeft
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + Home（以当前光标为起点，全部选中当前行光标左边的字符）
	/// </summary>
	public class ShiftHome : Home
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + End（以当前光标为起点，全部选中当前行光标右边的字符）
	/// </summary>
	public class ShiftEnd : End
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Ctrl + Shift + Home（以当前光标为起点，选中光标前面的所有文本）
	/// </summary>
	public class ShiftMoveToStart : MoveToStart
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Ctrl + Shift + End（以当前光标为起点，选中光标后面的所有文本）
	/// </summary>
	public class ShiftMoveToEnd : MoveToEnd
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + PageUp（以当前光标为起点，选择光标前面的所有字符）
	/// </summary>
	public class ShiftMovePageUp : MovePageUp
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Shift + PageDown（以当前光标为起点，选择光标后面的所有字符）
	/// </summary>
	public class ShiftMovePageDown : MovePageDown
	{
		public override void Execute(TextArea textArea)
		{
			Point oldCaretPos  = textArea.Caret.Position;
			base.Execute(textArea);
			textArea.AutoClearSelection = false;
			textArea.SelectionManager.ExtendSelection(oldCaretPos, textArea.Caret.Position);
		}
	}

	/// <summary>
	/// Ctrl + A（全选文本）
	/// </summary>
	public class SelectWholeDocument : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			Point startPoint = new Point(0, 0);
			Point endPoint   = textArea.Document.OffsetToPosition(textArea.Document.TextLength);
			if (textArea.SelectionManager.HasSomethingSelected) {
				if (textArea.SelectionManager.SelectionCollection[0].StartPosition == startPoint &&
				    textArea.SelectionManager.SelectionCollection[0].EndPosition   == endPoint) {
					return;
				}
			}
			textArea.SelectionManager.SetSelection(new DefaultSelection(textArea.Document, startPoint, endPoint));
		}
	}

	/// <summary>
	/// Esc键（取消选择的文本）
	/// </summary>
	public class ClearAllSelections : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.SelectionManager.ClearSelection();
		}
	}
}
