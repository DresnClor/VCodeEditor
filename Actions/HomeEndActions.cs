using System.Collections.Generic;
using System.Drawing;
using VCodeEditor.Document;

//首页-结束相关

namespace VCodeEditor.Actions
{
	/// <summary>
	/// Home键（将光标定位至行开始的位置）
	/// </summary>
	public class Home : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			LineSegment curLine;
			Point       newPos = textArea.Caret.Position;
			bool        jumpedIntoFolding = false;
			do {
				curLine = textArea.Document.GetLineSegment(newPos.Y);
						
				if (TextUtilities.IsEmptyLine(textArea.Document, newPos.Y)) {
					if (newPos.X != 0) {
						newPos.X = 0;
					} else  {
						newPos.X = curLine.Length;
					}
				} else {
					int firstCharOffset = TextUtilities.GetFirstNonWSChar(textArea.Document, curLine.Offset);
					int firstCharColumn = firstCharOffset - curLine.Offset;
					
					if (newPos.X == firstCharColumn) {
						newPos.X = 0;
					} else {
						newPos.X = firstCharColumn;
					}
				}
				List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(newPos.Y, newPos.X);
				jumpedIntoFolding = false;
				foreach (FoldMarker foldMarker in foldings) {
					if (foldMarker.IsFolded) {
						newPos = new Point(foldMarker.StartColumn, foldMarker.StartLine);
						jumpedIntoFolding = true;
						break;
					}
				}
				
			} while (jumpedIntoFolding);
			
			if (newPos != textArea.Caret.Position) {
				textArea.Caret.Position = newPos;
				textArea.SetDesiredColumn();
			}
		}
	}

	/// <summary>
	/// End键（将光标定位至行结束的位置）
	/// </summary>
	public class End : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			LineSegment curLine;
			Point       newPos = textArea.Caret.Position;
			bool        jumpedIntoFolding = false;
			do {
				curLine  = textArea.Document.GetLineSegment(newPos.Y);
				newPos.X = curLine.Length;
				
				List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(newPos.Y, newPos.X);
				jumpedIntoFolding = false;
				foreach (FoldMarker foldMarker in foldings) {
					if (foldMarker.IsFolded) {
						newPos = new Point(foldMarker.EndColumn, foldMarker.EndLine);
						jumpedIntoFolding = true;
						break;
					}
				}
			} while (jumpedIntoFolding);
			
			if (newPos != textArea.Caret.Position) {
				textArea.Caret.Position = newPos;
				textArea.SetDesiredColumn();
			}
		}
	}

	/// <summary>
	/// Ctrl + Home（将光标定位至全部文本开始）
	/// </summary>
	public class MoveToStart : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			if (textArea.Caret.Line != 0 || textArea.Caret.Column != 0) {
				textArea.Caret.Position = new Point(0, 0);
				textArea.SetDesiredColumn();
			}
		}
	}

	/// <summary>
	/// Ctrl + End（将光标定位至全部文本末尾）
	/// </summary>
	public class MoveToEnd : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			Point endPos = textArea.Document.OffsetToPosition(textArea.Document.TextLength);
			if (textArea.Caret.Position != endPos) {
				textArea.Caret.Position = endPos;
				textArea.SetDesiredColumn();
			}
		}
	}
}
