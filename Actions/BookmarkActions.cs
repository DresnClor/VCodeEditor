using System;
using VCodeEditor.Document;

//书签相关

namespace VCodeEditor.Actions
{
	/// <summary>
	/// 切换书签
	/// </summary>
	public class ToggleBookmark : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.Document.BookmarkManager.ToggleMarkAt(textArea.Caret.Line);
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, textArea.Caret.Line));
			textArea.Document.CommitUpdate();
			
		}
	}
	
	/// <summary>
	/// 转到上一个书签
	/// </summary>
	public class GotoPrevBookmark : AbstractEditAction
	{
		Predicate<Bookmark> predicate = null;
		
		public GotoPrevBookmark(Predicate<Bookmark> predicate)
		{
			this.predicate = predicate;
		}
		
		public override void Execute(TextArea textArea)
		{
			Bookmark mark = textArea.Document.BookmarkManager.GetPrevMark(textArea.Caret.Line, predicate);
			if (mark != null) {
				textArea.Caret.Line = mark.LineNumber;
				textArea.SelectionManager.ClearSelection();
			}
		}
	}
	
	/// <summary>
	/// 转到下一个书签
	/// </summary>
	public class GotoNextBookmark : AbstractEditAction
	{
		Predicate<Bookmark> predicate = null;
		
		public GotoNextBookmark(Predicate<Bookmark> predicate)
		{
			this.predicate = predicate;
		}
		
		public override void Execute(TextArea textArea)
		{
			Bookmark mark = textArea.Document.BookmarkManager.GetNextMark(textArea.Caret.Line, predicate);
			if (mark != null) {
				textArea.Caret.Line = mark.LineNumber;
				textArea.SelectionManager.ClearSelection();
			}
		}
	}
	
	/// <summary>
	/// 清除所有书签
	/// </summary>
	public class ClearAllBookmarks : AbstractEditAction
	{
		Predicate<Bookmark> predicate = null;
		
		public ClearAllBookmarks(Predicate<Bookmark> predicate)
		{
			this.predicate = predicate;
		}
		
		public override void Execute(TextArea textArea)
		{
			textArea.Document.BookmarkManager.RemoveMarks(predicate);
			textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			textArea.Document.CommitUpdate();
		}
	}
}
