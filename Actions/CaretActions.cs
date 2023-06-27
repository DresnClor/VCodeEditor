using System;
using System.Collections.Generic;
using System.Drawing;
using VCodeEditor.Document;

//方向键操作相关

namespace VCodeEditor.Actions
{
	/// <summary>
	/// 方向键： 左键，操作类（向左移动光标）
	/// </summary>
	public class CaretLeft : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			Point position = textArea.Caret.Position;
			List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldedFoldingsWithEnd(position.Y);
			FoldMarker justBeforeCaret = null;
			foreach (FoldMarker fm in foldings) {
				if (fm.EndColumn == position.X) {
					justBeforeCaret = fm;
					break; // 发现的第一个折叠是具有最小起始位置的折叠
				}
			}
			
			if (justBeforeCaret != null) {
				position.Y = justBeforeCaret.StartLine;
				position.X = justBeforeCaret.StartColumn;
			} else {
				if (position.X > 0) {
					--position.X;
				} else if (position.Y  > 0) {
					LineSegment lineAbove = textArea.Document.GetLineSegment(position.Y - 1);
					position = new Point(lineAbove.Length, position.Y - 1);
				}
			}
//			ArrayList foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(position.Y, position.X);
//			foreach (FoldMarker foldMarker in foldings) {
//				if (foldMarker.IsFolded) {
//					if (foldMarker.StartLine < position.Y || foldMarker.StartLine == position.Y && foldMarker.StartColumn < position.X) {
//						position = new Point(foldMarker.StartColumn, foldMarker.StartLine);
//					}
//				}
//			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}

	/// <summary>
	/// 方向键： 右键，操作类（向右移动光标）
	/// </summary>
	public class CaretRight : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			LineSegment curLine = textArea.Document.GetLineSegment(textArea.Caret.Line);
			Point position = textArea.Caret.Position;
			List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(position.Y);
			FoldMarker justBehindCaret = null;
			foreach (FoldMarker fm in foldings) {
				if (fm.StartColumn == position.X) {
					justBehindCaret = fm;
					break;
				}
			}
			if (justBehindCaret != null) {
				position.Y = justBehindCaret.EndLine;
				position.X = justBehindCaret.EndColumn;
			} else { // 没有折叠是有趣的
				if (position.X < curLine.Length || textArea.TextEditorProperties.AllowCaretBeyondEOL) {
					++position.X;
				} else if (position.Y + 1 < textArea.Document.TotalNumberOfLines) {
					++position.Y;
					position.X = 0;
				}
			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}

	/// <summary>
	/// 方向键： 上键，操作类（向上移动光标）
	/// </summary>
	public class CaretUp : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			Point position = textArea.Caret.Position;
			int lineNr = position.Y;
			int visualLine = textArea.Document.GetVisibleLine(lineNr);
			if (visualLine > 0) {
				int xpos = textArea.TextView.GetDrawingXPos(lineNr, position.X);
				Point pos = new Point(xpos,
				                      textArea.TextView.DrawingPosition.Y + (visualLine - 1) * textArea.TextView.FontHeight - textArea.TextView.TextArea.VirtualTop.Y);
				textArea.Caret.Position = textArea.TextView.GetLogicalPosition(pos.X, pos.Y);
				textArea.SetCaretToDesiredColumn(textArea.Caret.Position.Y);
			}
//			if (textArea.Caret.Line  > 0) {
//				textArea.SetCaretToDesiredColumn(textArea.Caret.Line - 1);
//			}
		}
	}

	/// <summary>
	/// 方向键： 下键，操作类（向下移动光标）
	/// </summary>
	public class CaretDown : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			Point position = textArea.Caret.Position;
			int lineNr = position.Y;
			int visualLine = textArea.Document.GetVisibleLine(lineNr);
			if (visualLine < textArea.Document.GetVisibleLine(textArea.Document.TotalNumberOfLines)) {
				int xpos = textArea.TextView.GetDrawingXPos(lineNr, position.X);
				Point pos = new Point(xpos,
				                      textArea.TextView.DrawingPosition.Y + (visualLine + 1) * textArea.TextView.FontHeight - textArea.TextView.TextArea.VirtualTop.Y);
				textArea.Caret.Position = textArea.TextView.GetLogicalPosition(pos.X, pos.Y);
				textArea.SetCaretToDesiredColumn(textArea.Caret.Position.Y);
			}
//			if (textArea.Caret.Line + 1 < textArea.Document.TotalNumberOfLines) {
//				textArea.SetCaretToDesiredColumn(textArea.Caret.Line + 1);
//			}
		}
	}

	/// <summary>
	/// Ctrl键 + 方向键 右键，操作类（按照词组为单位 左右移动光标）
	/// </summary>
	public class WordRight : CaretRight
	{
		public override void Execute(TextArea textArea)
		{
			LineSegment line   = textArea.Document.GetLineSegment(textArea.Caret.Position.Y);
			Point oldPos = textArea.Caret.Position;
			Point newPos;
			if (textArea.Caret.Column >= line.Length) {
				newPos = new Point(0, textArea.Caret.Line + 1);
			} else {
				int nextWordStart = TextUtilities.FindNextWordStart(textArea.Document, textArea.Caret.Offset);
				newPos = textArea.Document.OffsetToPosition(nextWordStart);
			}
			
			//手柄折叠标记
			List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(newPos.Y, newPos.X);
			foreach (FoldMarker marker in foldings) {
				if (marker.IsFolded) {
					if (oldPos.X == marker.StartColumn && oldPos.Y == marker.StartLine) {
						newPos = new Point(marker.EndColumn, marker.EndLine);
					} else {
						newPos = new Point(marker.StartColumn, marker.StartLine);
					}
					break;
				}
			}
			
			textArea.Caret.Position = newPos;
			textArea.SetDesiredColumn();
		}
	}

	/// <summary>
	/// Ctrl键 + 方向键 左键，操作类（按照词组为单位 左右移动光标）
	/// </summary>
	public class WordLeft : CaretLeft
	{
		public override void Execute(TextArea textArea)
		{
			Point oldPos = textArea.Caret.Position;
			if (textArea.Caret.Column == 0) {
				base.Execute(textArea);
			} else {
				LineSegment line   = textArea.Document.GetLineSegment(textArea.Caret.Position.Y);
				
				int prevWordStart = TextUtilities.FindPrevWordStart(textArea.Document, textArea.Caret.Offset);
				
				Point newPos = textArea.Document.OffsetToPosition(prevWordStart);
				
				// handle fold markers手柄折叠标记
				List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(newPos.Y, newPos.X);
				foreach (FoldMarker marker in foldings) {
					if (marker.IsFolded) {
						if (oldPos.X == marker.EndColumn && oldPos.Y == marker.EndLine) {
							newPos = new Point(marker.StartColumn, marker.StartLine);
						} else {
							newPos = new Point(marker.EndColumn, marker.EndLine);
						}
						break;
					}
				}
				textArea.Caret.Position = newPos;
				textArea.SetDesiredColumn();
			}
			
			
		}
	}

	/// <summary>
	/// Ctrl + ↑键（向上移动滚动条）
	/// </summary>
	public class ScrollLineUp : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			
			textArea.MotherTextAreaControl.VScrollBar.Value = Math.Max(textArea.MotherTextAreaControl.VScrollBar.Minimum, textArea.VirtualTop.Y - textArea.TextView.FontHeight);
		}
	}

	/// <summary>
	/// Ctrl + ↓键（向下移动滚动条）
	/// </summary>
	public class ScrollLineDown : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			textArea.MotherTextAreaControl.VScrollBar.Value = Math.Min(textArea.MotherTextAreaControl.VScrollBar.Maximum, textArea.VirtualTop.Y + textArea.TextView.FontHeight);
		}
	}
}
