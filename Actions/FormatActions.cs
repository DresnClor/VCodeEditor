using System;
using System.Text;
using VCodeEditor.Document;

//格式作用相关

namespace VCodeEditor.Actions
{
	/// <summary>
	/// 行格式 抽象类，继承AbstractEditAction
	/// </summary>
	public abstract class AbstractLineFormatAction : AbstractEditAction
	{
		protected TextArea textArea;
		abstract protected void Convert(IDocument document, int startLine, int endLine);
		
		public override void Execute(TextArea textArea)
		{
			this.textArea = textArea;
			textArea.BeginUpdate();
			if (textArea.SelectionManager.HasSomethingSelected) {
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					Convert(textArea.Document, selection.StartPosition.Y, selection.EndPosition.Y);
				}
			} else {
				Convert(textArea.Document, 0, textArea.Document.TotalNumberOfLines - 1);
			}
			textArea.Caret.ValidateCaretPos();
			textArea.EndUpdate();
			textArea.Refresh();
		}
	}

	/// <summary>
	/// 选择格式 抽象类，继承AbstractEditAction
	/// </summary>
	public abstract class AbstractSelectionFormatAction : AbstractEditAction
	{
		protected TextArea textArea;
		abstract protected void Convert(IDocument document, int offset, int length);
		
		public override void Execute(TextArea textArea)
		{
			this.textArea = textArea;
			textArea.BeginUpdate();
			if (textArea.SelectionManager.HasSomethingSelected) {
				foreach (ISelection selection in textArea.SelectionManager.SelectionCollection) {
					Convert(textArea.Document, selection.Offset, selection.Length);
				}
			} else {
				Convert(textArea.Document, 0, textArea.Document.TextLength);
			}
			textArea.Caret.ValidateCaretPos();
			textArea.EndUpdate();
			textArea.Refresh();
		}
	}
	
	/// <summary>
	/// 移除领先
	/// </summary>
	public class RemoveLeadingWS : AbstractLineFormatAction
	{
		protected override void Convert(IDocument document, int y1, int y2) 
		{
			int  redocounter = 0; // 必须计算删除操作发生的数量
			for (int i = y1; i < y2; ++i) {
				LineSegment line = document.GetLineSegment(i);
				int removeNumber = 0;
				for (int x = line.Offset; x < line.Offset + line.Length && Char.IsWhiteSpace(document.GetCharAt(x)); ++x) {
					++removeNumber;
				}
				if (removeNumber > 0) {
					document.Remove(line.Offset, removeNumber);
					++redocounter; // 计数删除
				}
			}
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // redo the whole operation (not the single deletes)
			}
		}
	}
	
	/// <summary>
	/// 移除尾部
	/// </summary>
	public class RemoveTrailingWS : AbstractLineFormatAction
	{
		protected override void Convert(IDocument document, int y1, int y2) 
		{
			int  redocounter = 0; // 必须计算删除操作发生的数量
			for (int i = y2 - 1; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				int removeNumber = 0;
				for (int x = line.Offset + line.Length - 1; x >= line.Offset && Char.IsWhiteSpace(document.GetCharAt(x)); --x) {
					++removeNumber;
				}
				if (removeNumber > 0) {
					document.Remove(line.Offset + line.Length - removeNumber, removeNumber);
					++redocounter;         // count deletes
				}
			}
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // redo the whole operation (not the single deletes)
			}
		}
	}
	
	/// <summary>
	/// 到大写转换
	/// </summary>
	public class ToUpperCase : AbstractSelectionFormatAction
	{
		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string what = document.GetText(startOffset, length).ToUpper();
			document.Replace(startOffset, length, what);
		}
	}

	/// <summary>
	/// 到小写转换
	/// </summary>
	public class ToLowerCase : AbstractSelectionFormatAction
	{
		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string what = document.GetText(startOffset, length).ToLower();
			document.Replace(startOffset, length, what);
		}
	}
	
	/// <summary>
	/// 转化操作
	/// </summary>
	public class InvertCaseAction : AbstractSelectionFormatAction
	{
		protected override void Convert(IDocument document, int startOffset, int length)
		{
			StringBuilder what = new StringBuilder(document.GetText(startOffset, length));
			
			for (int i = 0; i < what.Length; ++i) {
				what[i] = Char.IsUpper(what[i]) ? Char.ToLower(what[i]) : Char.ToUpper(what[i]);
			}
			
			document.Replace(startOffset, length, what.ToString());
		}
	}
	
	/// <summary>
	/// 大写
	/// </summary>
	public class CapitalizeAction : AbstractSelectionFormatAction
	{
		protected override void Convert(IDocument document, int startOffset, int length)
		{
			StringBuilder what = new StringBuilder(document.GetText(startOffset, length));
			
			for (int i = 0; i < what.Length; ++i) {
				if (!Char.IsLetter(what[i]) && i < what.Length - 1) {
					what[i + 1] = Char.ToUpper(what[i + 1]);
				}
			}
			document.Replace(startOffset, length, what.ToString());
		}
		
	}
	
	/// <summary>
	/// tab转换为空格
	/// </summary>
	public class ConvertTabsToSpaces : AbstractSelectionFormatAction
	{
		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string what = document.GetText(startOffset, length);
			string spaces = new string(' ', document.TextEditorProperties.TabIndent);
			document.Replace(startOffset, length, what.Replace("\t", spaces));
		}
	}
	
	/// <summary>
	/// 空格转换为tab
	/// </summary>
	public class ConvertSpacesToTabs : AbstractSelectionFormatAction
	{
		protected override void Convert(IDocument document, int startOffset, int length)
		{
			string what = document.GetText(startOffset, length);
			string spaces = new string(' ', document.TextEditorProperties.TabIndent);
			document.Replace(startOffset, length, what.Replace(spaces, "\t"));
		}
	}
	
	/// <summary>
	/// 首位置tab转换空格
	/// </summary>
	public class ConvertLeadingTabsToSpaces : AbstractLineFormatAction
	{
		protected override void Convert(IDocument document, int y1, int y2) 
		{
			int  redocounter = 0;
			for (int i = y2; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				
				if(line.Length > 0) {
					// 数一下开始有多少个空白字符
					int whiteSpace = 0;
					for(whiteSpace = 0; whiteSpace < line.Length && Char.IsWhiteSpace(document.GetCharAt(line.Offset + whiteSpace)); whiteSpace++) {
						// deliberately empty
					}
					if(whiteSpace > 0) {
						string newLine = document.GetText(line.Offset,whiteSpace);
						string newPrefix = newLine.Replace("\t",new string(' ', document.TextEditorProperties.TabIndent));
						document.Replace(line.Offset,whiteSpace,newPrefix);
						++redocounter;
					}
				}
			}
			
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // redo the whole operation (not the single deletes)
			}
		}
	}

	/// <summary>
	/// 首位置空格转换tab
	/// </summary>
	public class ConvertLeadingSpacesToTabs : AbstractLineFormatAction
	{
		protected override void Convert(IDocument document, int y1, int y2) 
		{
			int  redocounter = 0;
			for (int i = y2; i >= y1; --i) {
				LineSegment line = document.GetLineSegment(i);
				if(line.Length > 0) {
					// 注意：一些用户可能更喜欢更激进的转换领导空间到塔布，
					//意味着可以没有空间之前的第一个字符，即使空间
					//没有加到一个完整的标签数量
					string newLine = TextUtilities.LeadingWhiteSpaceToTabs(document.GetText(line.Offset,line.Length), document.TextEditorProperties.TabIndent);
					document.Replace(line.Offset,line.Length,newLine);
					++redocounter;
				}
			}
			
			if (redocounter > 0) {
				document.UndoStack.UndoLast(redocounter); // redo the whole operation (not the single deletes)
			}
		}
	}

	/// <summary>
	/// 格式缓冲区
	/// </summary>
	public class FormatBuffer : AbstractLineFormatAction
	{
		protected override void Convert(IDocument document, int startLine, int endLine)
		{
			document.FormattingStrategy.IndentLines(textArea, startLine, endLine);
		}
	}
}
