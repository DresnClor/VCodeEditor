using System;
using System.Drawing;
using System.Text;

//杂项

using MeltuiCodeEditor.Document;

namespace MeltuiCodeEditor.Actions
{
	/// <summary>
	/// Tab键（插入tab符号）
	/// </summary>
	public class Tab : AbstractEditAction
	{
		public static string GetIndentationString(IDocument document)
		{
			return GetIndentationString(document, null);
		}
		
		public static string GetIndentationString(IDocument document, TextArea textArea)
		{
			StringBuilder indent = new StringBuilder();
			
			if (document.TextEditorProperties.ConvertTabsToSpaces) {
				int tabIndent = document.TextEditorProperties.TabIndent;
				if (textArea != null) {
					int column = textArea.TextView.GetVisualColumn(textArea.Caret.Line, textArea.Caret.Column);
					indent.Append(new String(' ', tabIndent - column % tabIndent));
				} else {
					indent.Append(new String(' ', tabIndent));
				}
			} else {
				indent.Append('\t');
			}
			return indent.ToString();
		}
		
		void InsertTabs(IDocument document, ISelection selection, int y1, int y2)
		{
			int    redocounter = 0;
			string indentationString = GetIndentationString(document);
			for (int i = y2; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				if (i == y2 && i == selection.EndPosition.Y && selection.EndPosition.X  == 0) {
					continue;
				}
				
				// 此位是可选的-但有用的，如果你使用块标签来整理
				//具有选项卡和空格混合的源文件
//				string newLine = document.GetText(line.Offset,line.Length);
//				document.Replace(line.Offset,line.Length,newLine);
//				++redocounter;
				
				document.Insert(line.Offset, indentationString);
				++redocounter;
			}
			
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // 重整操作（不是单个删除）
			}
		}
		
		void InsertTabAtCaretPosition(TextArea textArea)
		{
			switch (textArea.Caret.CaretMode) {
				case CaretMode.InsertMode:
					textArea.InsertString(GetIndentationString(textArea.Document, textArea));
					break;
				case CaretMode.OverwriteMode:
					string indentStr = GetIndentationString(textArea.Document, textArea);
					textArea.ReplaceChar(indentStr[0]);
					if (indentStr.Length > 1) {
						textArea.InsertString(indentStr.Substring(1));
					}
					break;
			}
			textArea.SetDesiredColumn();
		}
		/// <remarks>
		/// 执行此编辑操作
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/>用于回调目的 </param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			if (textArea.SelectionManager.HasSomethingSelected) {
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					int startLine = selection.StartPosition.Y;
					int endLine   = selection.EndPosition.Y;
					if (startLine != endLine) {
						textArea.BeginUpdate();
						InsertTabs(textArea.Document, selection, startLine, endLine);
						textArea.Document.UpdateQueue.Clear();
						textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, startLine, endLine));
						textArea.EndUpdate();
					} else {
						InsertTabAtCaretPosition(textArea);
						break;
					}
				}
				textArea.Document.CommitUpdate();
				textArea.AutoClearSelection = false;
			} else {
				InsertTabAtCaretPosition(textArea);
			}
		}
	}

	/// <summary>
	/// Shift + Tab（将光标移动到行首+1个tab的距离，如果tab存在的话）
	/// </summary>
	public class ShiftTab : AbstractEditAction
	{
		void RemoveTabs(IDocument document, ISelection selection, int y1, int y2)
		{
			int  redocounter = 0;
			for (int i = y2; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				if (i == y2 && line.Offset == selection.EndOffset) {
					continue;
				}
				if (line.Length > 0) {
					/**** TextPad Strategy:
					/// first convert leading whitespace to tabs (controversial! - not all editors work like this)
					string newLine = TextUtilities.LeadingWhiteSpaceToTabs(document.GetText(line.Offset,line.Length),document.Properties.Get("TabIndent", 4));
					if(newLine.Length > 0 && newLine[0] == '\t') {
						document.Replace(line.Offset,line.Length,newLine.Substring(1));
						++redocounter;
					}
					else if(newLine.Length > 0 && newLine[0] == ' ') {
						/// there were just some leading spaces but less than TabIndent of them
						int leadingSpaces = 1;
						for(leadingSpaces = 1; leadingSpaces < newLine.Length && newLine[leadingSpaces] == ' '; leadingSpaces++) {
							/// deliberately empty
						}
						document.Replace(line.Offset,line.Length,newLine.Substring(leadingSpaces));
						++redocounter;
					}
					/// else
					/// there were no leading tabs or spaces on this line so do nothing
					/// MS Visual Studio 6 strategy:
					 ****/
//					string temp = document.GetText(line.Offset,line.Length);
					if (line.Length > 0) {
						int charactersToRemove = 0;
						if(document.GetCharAt(line.Offset) == '\t') { // 第一个字符是一个选项卡-只需删除它
							charactersToRemove = 1;
						} else if(document.GetCharAt(line.Offset) == ' ') {
							int leadingSpaces = 1;
							int tabIndent = document.TextEditorProperties.TabIndent;
							for (leadingSpaces = 1; leadingSpaces < line.Length && document.GetCharAt(line.Offset + leadingSpaces) == ' '; leadingSpaces++) {
								//故意空  deliberately empty
							}
							if (leadingSpaces >= tabIndent) {
								// 只需删除选项卡
								charactersToRemove = tabIndent;
							}
							else if(line.Length > leadingSpaces && document.GetCharAt(line.Offset + leadingSpaces) == '\t') {
								// 删除领先空间和以下选项卡，因为它们加起来
								//只需一个选项卡停止
								charactersToRemove = leadingSpaces+1;
							}
							else {
								// 只需删除领先的空间
								charactersToRemove = leadingSpaces;
							}
						}
						if (charactersToRemove > 0) {
							document.Remove(line.Offset,charactersToRemove);
							++redocounter;
						}
					}
				}
			}
			
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // 重整操作（不是单个删除）
			}
		}
		
		/// <remarks>
		/// 执行此编辑操作
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/>用于回调目的 </param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.SelectionManager.HasSomethingSelected) {
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					int startLine = selection.StartPosition.Y;
					int endLine   = selection.EndPosition.Y;
					textArea.BeginUpdate();
					RemoveTabs(textArea.Document, selection, startLine, endLine);
					textArea.Document.UpdateQueue.Clear();
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, startLine, endLine));
					textArea.EndUpdate();
					
				}
				textArea.AutoClearSelection = false;
			} else {
				//按下没有选择光标的移位选项卡将移回
				//前一个选项卡停止。它将在行的开头停止。此外，所需的
				//列更新到该列。
				LineSegment line = textArea.Document.GetLineSegmentForOffset(textArea.Caret.Offset);
				string startOfLine = textArea.Document.GetText(line.Offset,textArea.Caret.Offset - line.Offset);
				int tabIndent = textArea.Document.TextEditorProperties.TabIndent;
				int currentColumn = textArea.Caret.Column;
				int remainder = currentColumn % tabIndent;
				if (remainder == 0) {
					textArea.Caret.DesiredColumn = Math.Max(0, currentColumn - tabIndent);
				} else {
					textArea.Caret.DesiredColumn = Math.Max(0, currentColumn - remainder);
				}
				textArea.SetCaretToDesiredColumn(textArea.Caret.Line);
			}
		}
	}

	/// <summary>
	/// Ctrl + / | Ctrl + ?（注释或取消注释）
	/// </summary>
	public class ToggleComment : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			
			if (textArea.Document.HighlightStyle.Properties.ContainsKey("LineComment")) {
				new ToggleLineComment().Execute(textArea);
			} else if (textArea.Document.HighlightStyle.Properties.ContainsKey("BlockCommentBegin") &&
			           textArea.Document.HighlightStyle.Properties.ContainsKey("BlockCommentBegin")) {
				new ToggleBlockComment().Execute(textArea);
			}
		}
	}
	
	/// <summary>
	/// 行注释
	/// </summary>
	public class ToggleLineComment : AbstractEditAction
	{
		int firstLine;
		int lastLine;
		
		void RemoveCommentAt(IDocument document, string comment, ISelection selection, int y1, int y2)
		{
			int  redocounter = 0;
			firstLine = y1;
			lastLine  = y2;
			
			for (int i = y2; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				if (selection != null && i == y2 && line.Offset == selection.Offset + selection.Length) {
					--lastLine;
					continue;
				}
				
				string lineText = document.GetText(line.Offset, line.Length);
				if (lineText.Trim().StartsWith(comment)) {
					document.Remove(line.Offset + lineText.IndexOf(comment), comment.Length);
					++redocounter;
				}
			}
			
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // redo the whole operation (not the single deletes)
			}
		}
		
		void SetCommentAt(IDocument document, string comment, ISelection selection, int y1, int y2)
		{
			int  redocounter = 0;
			firstLine = y1;
			lastLine  = y2;
			
			for (int i = y2; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				if (selection != null && i == y2 && line.Offset == selection.Offset + selection.Length) {
					--lastLine;
					continue;
				}
				
				string lineText = document.GetText(line.Offset, line.Length);
				document.Insert(line.Offset, comment);
				++redocounter;
			}
			
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // redo the whole operation (not the single deletes)
			}
		}
		
		bool ShouldComment(IDocument document, string comment, ISelection selection, int startLine, int endLine)
		{
			for (int i = endLine; i >= startLine; --i) {
				LineSegment line = document.GetLineSegment(i);
				if (selection != null && i == endLine && line.Offset == selection.Offset + selection.Length) {
					--lastLine;
					continue;
				}
				string lineText = document.GetText(line.Offset, line.Length);
				if (!lineText.Trim().StartsWith(comment)) {
					return true;
				}
			}
			return false;
		}
		
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			
			string comment = null;
			if (textArea.Document.HighlightStyle.Properties.ContainsKey("LineComment")) {
				comment = textArea.Document.HighlightStyle.Properties["LineComment"].ToString();
			}
			
			if (comment == null || comment.Length == 0) {
				return;
			}
			
			if (textArea.SelectionManager.HasSomethingSelected) {
				bool shouldComment = true;
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					if (!ShouldComment(textArea.Document, comment, selection, selection.StartPosition.Y, selection.EndPosition.Y)) {
						shouldComment = false;
						break;
					}
				}
				
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					textArea.BeginUpdate();
					if (shouldComment) {
						SetCommentAt(textArea.Document, comment, selection, selection.StartPosition.Y, selection.EndPosition.Y);
					} else {
						RemoveCommentAt(textArea.Document, comment, selection, selection.StartPosition.Y, selection.EndPosition.Y);
					}
					textArea.Document.UpdateQueue.Clear();
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, firstLine, lastLine));
					textArea.EndUpdate();
				}
				textArea.Document.CommitUpdate();
				textArea.AutoClearSelection = false;
			} else {
				textArea.BeginUpdate();
				int caretLine = textArea.Caret.Line;
				if (ShouldComment(textArea.Document, comment, null, caretLine, caretLine)) {
					SetCommentAt(textArea.Document, comment, null, caretLine, caretLine);
				} else {
					RemoveCommentAt(textArea.Document, comment, null, caretLine, caretLine);
				}
				textArea.Document.UpdateQueue.Clear();
				textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, caretLine));
				textArea.EndUpdate();
			}
		}
	}
	
	/// <summary>
	/// 块注释
	/// </summary>
	public class ToggleBlockComment : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			
			string commentStart = null;
			if (textArea.Document.HighlightStyle.Properties.ContainsKey("BlockCommentBegin")) {
				commentStart = textArea.Document.HighlightStyle.Properties["BlockCommentBegin"].ToString();
			}
			
			string commentEnd = null;
			if (textArea.Document.HighlightStyle.Properties.ContainsKey("BlockCommentEnd")) {
				commentEnd = textArea.Document.HighlightStyle.Properties["BlockCommentEnd"].ToString();
			}
			
			if (commentStart == null || commentStart.Length == 0 || commentEnd == null || commentEnd.Length == 0) {
				return;
			}
			
			int selectionStartOffset;
			int selectionEndOffset;
			
			if (textArea.SelectionManager.HasSomethingSelected) {
				selectionStartOffset = textArea.SelectionManager.SelectionCollection[0].Offset;
				selectionEndOffset = textArea.SelectionManager.SelectionCollection[textArea.SelectionManager.SelectionCollection.Count - 1].EndOffset;
			} else {
				selectionStartOffset = textArea.Caret.Offset;
				selectionEndOffset = selectionStartOffset;
			}
			
			BlockCommentRegion commentRegion = FindSelectedCommentRegion(textArea.Document, commentStart, commentEnd, selectionStartOffset, selectionEndOffset);
			
			if (commentRegion != null) {
				RemoveComment(textArea.Document, commentRegion);
			} else if (textArea.SelectionManager.HasSomethingSelected) {
				SetCommentAt(textArea.Document, selectionStartOffset, selectionEndOffset, commentStart, commentEnd);
			}
			
			textArea.Document.CommitUpdate();
			textArea.AutoClearSelection = false;
		}
		
		public static BlockCommentRegion FindSelectedCommentRegion(IDocument document, string commentStart, string commentEnd, int selectionStartOffset, int selectionEndOffset)
		{
			if (document.TextLength == 0) {
				return null;
			}
			
			// 在所选文本中查找注释的开头。
			
			int commentEndOffset = -1;
			string selectedText = document.GetText(selectionStartOffset, selectionEndOffset - selectionStartOffset);
			
			int commentStartOffset = selectedText.IndexOf(commentStart);
			if (commentStartOffset >= 0) {
				commentStartOffset += selectionStartOffset;
			}

			// 在选定的文本中查找评论的末尾。
			
			if (commentStartOffset >= 0) {
				commentEndOffset = selectedText.IndexOf(commentEnd, commentStartOffset + commentStart.Length - selectionStartOffset);
			} else {
				commentEndOffset = selectedText.IndexOf(commentEnd);
			}
			
			if (commentEndOffset >= 0) {
				commentEndOffset += selectionStartOffset;
			}
			
			//在选定的文本之前查找注释的开头。
			
			if (commentStartOffset == -1) {
				int offset = selectionEndOffset + commentStart.Length - 1;
				if (offset > document.TextLength) {
					offset = document.TextLength;
				}
				string text = document.GetText(0, offset);
				commentStartOffset = text.LastIndexOf(commentStart);
			}
			
			// 在选定的文本后查找评论的结尾。
			
			if (commentEndOffset == -1) {
				int offset = selectionStartOffset + 1 - commentEnd.Length;
				if (offset < 0) {
					offset = selectionStartOffset;
				}
				string text = document.GetText(offset, document.TextLength - offset);
				commentEndOffset = text.IndexOf(commentEnd);
				if (commentEndOffset >= 0) {
					commentEndOffset += offset;
				}
			}
			
			if (commentStartOffset != -1 && commentEndOffset != -1) {
				return new BlockCommentRegion(commentStart, commentEnd, commentStartOffset, commentEndOffset);
			}
			
			return null;
		}
		

		void SetCommentAt(IDocument document, int offsetStart, int offsetEnd, string commentStart, string commentEnd)
		{
			document.Insert(offsetEnd, commentEnd);
			document.Insert(offsetStart, commentStart);
			document.UndoStack.UndoLast(2);
		}
		
		void RemoveComment(IDocument document, BlockCommentRegion commentRegion)
		{
			document.Remove(commentRegion.EndOffset, commentRegion.CommentEnd.Length);
			document.Remove(commentRegion.StartOffset, commentRegion.CommentStart.Length);
			document.UndoStack.UndoLast(2);
		}
	}
	
	/// <summary>
	/// 块注释区域
	/// </summary>
	public class BlockCommentRegion
	{
		string commentStart = String.Empty;
		string commentEnd = String.Empty;
		int startOffset = -1;
		int endOffset = -1;
		
		/// <summary>
		/// 最终偏移是注释端字符串开始的偏移。
		/// </summary>
		public BlockCommentRegion(string commentStart, string commentEnd, int startOffset, int endOffset)
		{
			this.commentStart = commentStart;
			this.commentEnd = commentEnd;
			this.startOffset = startOffset;
			this.endOffset = endOffset;
		}
		
		public string CommentStart {
			get {
				return commentStart;
			}
		}
		
		public string CommentEnd {
			get {
				return commentEnd;
			}
		}
		
		public int StartOffset {
			get {
				return startOffset;
			}
		}
		
		public int EndOffset {
			get {
				return endOffset;
			}
		}
		
		public override bool Equals(object obj)
		{
			BlockCommentRegion commentRegion = obj as BlockCommentRegion;
			if (commentRegion != null) {
				if (commentRegion.commentStart == commentStart &&
				    commentRegion.commentEnd == commentEnd &&
				    commentRegion.startOffset == startOffset &&
				    commentRegion.endOffset == endOffset) {
					return true;
				}
			}
			
			return false;
		}
		
		public override int GetHashCode()
		{
			return commentStart.GetHashCode() & commentEnd.GetHashCode() & startOffset.GetHashCode() & endOffset.GetHashCode();
		}
	}
	
	/// <summary>
	/// 缩进选择
	/// </summary>
	public class IndentSelection : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			textArea.BeginUpdate();
			if (textArea.SelectionManager.HasSomethingSelected) {
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					textArea.Document.FormattingStrategy.IndentLines(textArea, selection.StartPosition.Y, selection.EndPosition.Y);
				}
			} else {
				textArea.Document.FormattingStrategy.IndentLines(textArea, 0, textArea.Document.TotalNumberOfLines - 1);
			}
			textArea.EndUpdate();
			textArea.Refresh();
		}
	}

	/// <summary>
	/// Backspace键（删除字符）
	/// </summary>
	public class Backspace : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			if (textArea.SelectionManager.HasSomethingSelected) {
				textArea.BeginUpdate();
				textArea.Caret.Position = textArea.SelectionManager.SelectionCollection[0].StartPosition;
				textArea.SelectionManager.RemoveSelectedText();
				textArea.ScrollToCaret();
				textArea.EndUpdate();
			} else {
				if (textArea.Caret.Offset > 0) {
					textArea.BeginUpdate();
					int curLineNr     = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
					int curLineOffset = textArea.Document.GetLineSegment(curLineNr).Offset;
					
					if (curLineOffset == textArea.Caret.Offset) {
						LineSegment line = textArea.Document.GetLineSegment(curLineNr - 1);
						bool lastLine = curLineNr == textArea.Document.TotalNumberOfLines;
						int lineEndOffset = line.Offset + line.Length;
						int lineLength = line.Length;
						textArea.Document.Remove(lineEndOffset, curLineOffset - lineEndOffset);
						textArea.Caret.Position = new Point(lineLength, curLineNr - 1);
						textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, curLineNr - 1)));
						textArea.EndUpdate();
					} else {
						int caretOffset = textArea.Caret.Offset - 1;
						textArea.Caret.Position = textArea.Document.OffsetToPosition(caretOffset);
						textArea.Document.Remove(caretOffset, 1);
						
						textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, new Point(textArea.Caret.Offset - textArea.Document.GetLineSegment(curLineNr).Offset, curLineNr)));
						textArea.EndUpdate();
					}
				}
			}
		}
	}

	/// <summary>
	/// Delete键（删除光标右边的一个字符）
	/// </summary>
	public class Delete : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			if (textArea.SelectionManager.HasSomethingSelected) {
				textArea.BeginUpdate();
				textArea.Caret.Position = textArea.SelectionManager.SelectionCollection[0].StartPosition;
				textArea.SelectionManager.RemoveSelectedText();
				textArea.ScrollToCaret();
				textArea.EndUpdate();
			} else {
				
				if (textArea.Caret.Offset < textArea.Document.TextLength) {
					textArea.BeginUpdate();
					int curLineNr   = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
					LineSegment curLine = textArea.Document.GetLineSegment(curLineNr);
					
					if (curLine.Offset + curLine.Length == textArea.Caret.Offset) {
						if (curLineNr + 1 < textArea.Document.TotalNumberOfLines) {
							LineSegment nextLine = textArea.Document.GetLineSegment(curLineNr + 1);
							
							textArea.Document.Remove(textArea.Caret.Offset, nextLine.Offset - textArea.Caret.Offset);
							textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, curLineNr)));
						}
					} else {
						textArea.Document.Remove(textArea.Caret.Offset, 1);
//						textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToLineEnd, new Point(textArea.Caret.Offset - textArea.Document.GetLineSegment(curLineNr).Offset, curLineNr)));
					}
					textArea.UpdateMatchingBracket();
					textArea.EndUpdate();
				}
			}
		}
	}

	/// <summary>
	/// PageDown键（以光标当前位置为起点，向下移动一页的位置）
	/// </summary>
	public class MovePageDown : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			int curLineNr           = textArea.Caret.Line;
			int requestedLineNumber = Math.Min(textArea.Document.GetNextVisibleLineAbove(curLineNr, textArea.TextView.VisibleLineCount), textArea.Document.TotalNumberOfLines - 1);
			
			if (curLineNr != requestedLineNumber) {
				textArea.Caret.Position = new Point(textArea.Caret.DesiredColumn, requestedLineNumber);
			}
		}
	}

	/// <summary>
	/// PageUp键（以光标当前位置为起点，向上移动一页的位置）
	/// </summary>
	public class MovePageUp : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			int curLineNr           = textArea.Caret.Line;
			int requestedLineNumber = Math.Max(textArea.Document.GetNextVisibleLineBelow(curLineNr, textArea.TextView.VisibleLineCount), 0);
			
			if (curLineNr != requestedLineNumber) {
				textArea.Caret.Position = new Point(textArea.Caret.DesiredColumn, requestedLineNumber);
			}
		}
	}

	/// <summary>
	/// Enter键（换行）
	/// </summary>
	public class Return : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			textArea.BeginUpdate();
			try {
				if (textArea.HandleKeyPress('\n'))
					return;
				textArea.InsertString(Environment.NewLine);
				
				int curLineNr = textArea.Caret.Line;
				textArea.Caret.Column = textArea.Document.FormattingStrategy.FormatLine(textArea, curLineNr, textArea.Caret.Offset, '\n');
				textArea.SetDesiredColumn();
				
				textArea.Document.UpdateQueue.Clear();
				textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, curLineNr - 1)));
			} finally {
				textArea.EndUpdate();
			}
		}
	}

	/// <summary>
	/// Insert键（切换编辑模式）
	/// </summary>
	public class ToggleEditMode : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			if (textArea.Document.ReadOnly) {
				return;
			}
			switch (textArea.Caret.CaretMode) {
				case CaretMode.InsertMode:
					textArea.Caret.CaretMode = CaretMode.OverwriteMode;
					break;
				case CaretMode.OverwriteMode:
					textArea.Caret.CaretMode = CaretMode.InsertMode;
					break;
			}
		}
	}

	/// <summary>
	/// Ctrl + Z（撤销）
	/// </summary>
	public class Undo : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			textArea.MotherTextEditorControl.Undo();
		}
	}

	/// <summary>
	/// Ctrl + Y（重做）
	/// </summary>
	public class Redo : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			textArea.MotherTextEditorControl.Redo();
		}
	}

	/// <summary>
	/// Ctrl + Backspace（以词组为单位，每次向左删除一个词）
	/// </summary>
	public class WordBackspace : AbstractEditAction
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			// 如果有什么选择，我们将只是先删除它
			textArea.BeginUpdate();
			if (textArea.SelectionManager.HasSomethingSelected) {
				textArea.SelectionManager.RemoveSelectedText();
				textArea.ScrollToCaret();
			}
			// 现在从照顾者删除到单词的开头
			LineSegment line =
				textArea.Document.GetLineSegmentForOffset(textArea.Caret.Offset);
			//如果我们不是在一条线的开始
			if(textArea.Caret.Offset > line.Offset) {
				int prevWordStart = TextUtilities.FindPrevWordStart(textArea.Document,
				                                                    textArea.Caret.Offset);
				if(prevWordStart < textArea.Caret.Offset) {
					textArea.Document.Remove(prevWordStart,textArea.Caret.Offset -
					                         prevWordStart);
					textArea.Caret.Position = textArea.Document.OffsetToPosition(prevWordStart);
				}
			}
			// 如果我们现在在一条线的开始
			if(textArea.Caret.Offset == line.Offset) {
				// 如果我们不在第一线
				int curLineNr =
					textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
				if(curLineNr > 0) {
					// 移动到上面行的末尾
					LineSegment lineAbove = textArea.Document.GetLineSegment(curLineNr -
					                                                         1);
					int endOfLineAbove = lineAbove.Offset + lineAbove.Length;
					int charsToDelete = textArea.Caret.Offset - endOfLineAbove;
					textArea.Document.Remove(endOfLineAbove,charsToDelete);
					textArea.Caret.Position = textArea.Document.OffsetToPosition(endOfLineAbove);
				}
			}
			textArea.SetDesiredColumn();
			textArea.EndUpdate();
			// 如果现在有更少的线，我们需要这个或有重绘问题
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset))));
			textArea.Document.CommitUpdate();
		}
	}

	/// <summary>
	/// Ctrl + Delete（以词组为单位，每次向右删除一个词）
	/// </summary>
	public class DeleteWord : Delete
	{
		/// <remarks>
		/// Executes this edit action
		/// </remarks>
		/// <param name="textArea">The <see cref="ItextArea"/> which is used for callback purposes</param>
		public override void Execute(TextArea textArea)
		{
			// 如果有什么选择，我们将只是先删除它
			textArea.BeginUpdate();
			if (textArea.SelectionManager.HasSomethingSelected) {
				textArea.SelectionManager.RemoveSelectedText();
				textArea.ScrollToCaret();
			}
			// 现在从照顾者删除到单词的开头
			LineSegment line =
				textArea.Document.GetLineSegmentForOffset(textArea.Caret.Offset);
			if(textArea.Caret.Offset == line.Offset + line.Length) {
				// 如果我们是在一条线的末端
				base.Execute(textArea);
			} else {
				int nextWordStart = TextUtilities.FindNextWordStart(textArea.Document,
				                                                    textArea.Caret.Offset);
				if(nextWordStart > textArea.Caret.Offset) {
					textArea.Document.Remove(textArea.Caret.Offset,nextWordStart -
					                         textArea.Caret.Offset);
					// 光标永远不会移动此命令
				}
			}
			textArea.UpdateMatchingBracket();
			textArea.EndUpdate();
			// 如果现在有更少的线，我们需要这个或有重绘问题
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset))));
			textArea.Document.CommitUpdate();
		}
	}

	/// <summary>
	/// Ctrl + D（删除所在行）
	/// </summary>
	public class DeleteLine : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			int lineNr = textArea.Caret.Line;
			LineSegment line = textArea.Document.GetLineSegment(lineNr);
			textArea.Document.Remove(line.Offset, line.TotalLength);
			textArea.Caret.Position = textArea.Document.OffsetToPosition(line.Offset);

			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, lineNr)));
			textArea.UpdateMatchingBracket();
			textArea.Document.CommitUpdate();
		}
	}

	/// <summary>
	/// Ctrl + Shift + D（删除光标所在行右边所有文本 ）
	/// </summary>
	public class DeleteToLineEnd : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			int lineNr = textArea.Caret.Line;
			LineSegment line = textArea.Document.GetLineSegment(lineNr);
			
			int numRemove = (line.Offset + line.Length) - textArea.Caret.Offset;
			if (numRemove > 0) {
				textArea.Document.Remove(textArea.Caret.Offset, numRemove);
				textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, new Point(0, lineNr)));
				textArea.Document.CommitUpdate();
			}
		}
	}

	/// <summary>
	/// Ctrl + B（将光标移动到下一个匹配的括号处）
	/// </summary>
	public class GotoMatchingBrace : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			if (textArea.TextView.Highlight != null) {
				Point p1 = new Point(textArea.TextView.Highlight.CloseBrace.X + 1, textArea.TextView.Highlight.CloseBrace.Y);
				Point p2 = new Point(textArea.TextView.Highlight.OpenBrace.X + 1, textArea.TextView.Highlight.OpenBrace.Y);
				if (p1 == textArea.Caret.Position) {
					if (textArea.Document.TextEditorProperties.BracketMatchingStyle == BracketMatchingStyle.After) {
						textArea.Caret.Position = p2;
					} else {
						textArea.Caret.Position = new Point(p2.X - 1, p2.Y);
					}
				} else {
					if (textArea.Document.TextEditorProperties.BracketMatchingStyle == BracketMatchingStyle.After) {
						textArea.Caret.Position = p1;
					} else {
						textArea.Caret.Position = new Point(p1.X - 1, p1.Y);
					}
				}
				textArea.SetDesiredColumn();
			}
		}
	}
}
