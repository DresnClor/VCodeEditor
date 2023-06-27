// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1118 $</version>
// </file>

using System;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Xml;
using System.Text;

using VCodeEditor.Document;
using VCodeEditor.Undo;
using VCodeEditor.Util;

namespace VCodeEditor
{
	/// <summary>
	/// 剪贴板处理
	/// </summary>
	public class TextAreaClipboardHandler
	{
		TextArea textArea;

		/// <summary>
		/// 是否允许剪切
		/// </summary>
		public bool EnableCut {
			get {
				return textArea.EnableCutOrPaste; //textArea.SelectionManager.HasSomethingSelected;
			}
		}

		/// <summary>
		/// 是否允许复制
		/// </summary>
		public bool EnableCopy {
			get {
				return true; //textArea.SelectionManager.HasSomethingSelected;
			}
		}
		
		/// <summary>
		/// 是否允许粘贴
		/// </summary>
		public bool EnablePaste {
			get {
				try {
					return Clipboard.ContainsText();
				} catch (ExternalException) {
					return false;
				}
			}
		}

		/// <summary>
		/// 是否允许删除
		/// </summary>
		public bool EnableDelete {
			get {
				return textArea.SelectionManager.HasSomethingSelected && textArea.EnableCutOrPaste;
			}
		}

		/// <summary>
		/// 是否允许全选
		/// </summary>
		public bool EnableSelectAll {
			get {
				return true;
			}
		}
		
		public TextAreaClipboardHandler(TextArea textArea)
		{
			this.textArea = textArea;
			textArea.SelectionManager.SelectionChanged += new EventHandler(DocumentSelectionChanged);
		}
		
		void DocumentSelectionChanged(object sender, EventArgs e)
		{
//			((DefaultWorkbench)WorkbenchSingleton.Workbench).UpdateToolbars();
		}

		string LineSelectedType
		{
			get {
				return "MSDEVLineSelect";  //这是类型VS 2003和2005用于标记整行副本 
			}
		}
		
		bool CopyTextToClipboard(string stringToCopy, bool asLine)
		{
			if (stringToCopy.Length > 0) {
				DataObject dataObject = new DataObject();
				dataObject.SetData(DataFormats.UnicodeText, true, stringToCopy);
				if (asLine) {
					MemoryStream lineSelected = new MemoryStream(1);
					lineSelected.WriteByte(1);
					dataObject.SetData(LineSelectedType, false, lineSelected);
				}
				// 默认值没有突出显示，因此我们不需要RTF输出
				if (textArea.Document.HighlightStyle.Name != "Default") {
					dataObject.SetData(DataFormats.Rtf, RtfWriter.GenerateRtf(textArea));
				}
				OnCopyText(new CopyTextEventArgs(stringToCopy));
				
				// 围绕外部例外错误工作。（SD2-426）
				//在虚拟PC内最适合复制。
				try {
					Clipboard.SetDataObject(dataObject, true, 10, 50);
				} catch (ExternalException) {
					Application.DoEvents();
					try {
						Clipboard.SetDataObject(dataObject, true, 10, 50);
					} catch (ExternalException) {}
				}
				return true;
			} else {
				return false;
			}
		}

		bool CopyTextToClipboard(string stringToCopy)
		{
			return CopyTextToClipboard(stringToCopy, false);
		}
		
		public void Cut(object sender, EventArgs e)
		{
			if (CopyTextToClipboard(textArea.SelectionManager.SelectedText)) {
				//删除文本
				textArea.BeginUpdate();
				textArea.Caret.Position = textArea.SelectionManager.SelectionCollection[0].StartPosition;
				textArea.SelectionManager.RemoveSelectedText();
				textArea.EndUpdate();
			} else if (textArea.Document.TextEditorProperties.CutCopyWholeLine){
				// 未选择文本、选择和剪切整行
				int curLineNr = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
				LineSegment lineWhereCaretIs = textArea.Document.GetLineSegment(curLineNr);
				string caretLineText = textArea.Document.GetText(lineWhereCaretIs.Offset, lineWhereCaretIs.TotalLength);
				textArea.SelectionManager.SetSelection(textArea.Document.OffsetToPosition(lineWhereCaretIs.Offset), textArea.Document.OffsetToPosition(lineWhereCaretIs.Offset + lineWhereCaretIs.TotalLength));
				if (CopyTextToClipboard(caretLineText, true)) {
					// remove line
					textArea.BeginUpdate();
					textArea.Caret.Position = textArea.Document.OffsetToPosition(lineWhereCaretIs.Offset);
					textArea.SelectionManager.RemoveSelectedText();
					textArea.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.PositionToEnd, new Point(0, curLineNr)));
					textArea.EndUpdate();
				}
			}
		}
		
		public void Copy(object sender, EventArgs e)
		{
			if (!CopyTextToClipboard(textArea.SelectionManager.SelectedText) && textArea.Document.TextEditorProperties.CutCopyWholeLine) {
				// 未选择文本，选择整行，复制，然后取消选择
				int curLineNr = textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset);
				LineSegment lineWhereCaretIs = textArea.Document.GetLineSegment(curLineNr);
				string caretLineText = textArea.Document.GetText(lineWhereCaretIs.Offset, lineWhereCaretIs.TotalLength);
				CopyTextToClipboard(caretLineText, true);
			}
		}
		
		public void Paste(object sender, EventArgs e)
		{
			// 剪贴板。获取数据对象可能会抛出一个例外。。。
			for (int i = 0;; i++) {
				try {
					IDataObject data = Clipboard.GetDataObject();
					bool fullLine = data.GetDataPresent(LineSelectedType);
					if (data.GetDataPresent(DataFormats.UnicodeText)) {
						string text = (string)data.GetData(DataFormats.UnicodeText);
						if (text.Length > 0) {
							int redocounter = 0;
							if (textArea.SelectionManager.HasSomethingSelected) {
								Delete(sender, e);
								redocounter++;
							}
							if (fullLine) {
								int col = textArea.Caret.Column;
								textArea.Caret.Column = 0;
								textArea.InsertString(text);
								textArea.Caret.Column = col;
							}
							else {
								textArea.InsertString(text);
							}
							if (redocounter > 0) {
								textArea.Document.UndoStack.UndoLast(redocounter + 1); // redo the whole operation
							}
						}
					}
					return;
				} catch (ExternalException) {
					// 获取数据对象不提供重试时间参数
					if (i > 5) throw;
				}
			}
		}
		
		public void Delete(object sender, EventArgs e)
		{
			new VCodeEditor.Actions.Delete().Execute(textArea);
		}
		
		public void SelectAll(object sender, EventArgs e)
		{
			new VCodeEditor.Actions.SelectWholeDocument().Execute(textArea);
		}
		
		protected virtual void OnCopyText(CopyTextEventArgs e)
		{
			if (CopyText != null) {
				CopyText(this, e);
			}
		}
		/// <summary>
		/// 复制文本事件
		/// </summary>
		public event CopyTextEventHandler CopyText;
	}
	
	public delegate void CopyTextEventHandler(object sender, CopyTextEventArgs e);
	/// <summary>
	/// 复制事件参数
	/// </summary>
	public class CopyTextEventArgs : EventArgs
	{
		string text;
		
		/// <summary>
		/// 文本
		/// </summary>
		public string Text {
			get {
				return text;
			}
		}
		
		public CopyTextEventArgs(string text)
		{
			this.text = text;
		}
	}
}
