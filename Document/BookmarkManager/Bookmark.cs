// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1085 $</version>
// </file>

using System;
using System.Drawing;
using SWF = System.Windows.Forms;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 书签
	/// </summary>
	public class Bookmark
	{
		IDocument document;
		int       lineNumber;
		bool      isEnabled = true;
		
		/// <summary>
		/// 文档
		/// </summary>
		public IDocument Document {
			get {
				return document;
			}
			set {
				if (document != value) {
					document = value;
					OnDocumentChanged(EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler DocumentChanged;
		
		protected virtual void OnDocumentChanged(EventArgs e)
		{
			if (DocumentChanged != null) {
				DocumentChanged(this, e);
			}
		}
		
		/// <summary>
		/// 是否启用
		/// </summary>
		public bool IsEnabled {
			get {
				return isEnabled;
			}
			set {
				if (isEnabled != value) {
					isEnabled = value;
					if (document != null) {
						document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, lineNumber));
						document.CommitUpdate();
					}
					OnIsEnabledChanged(EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler IsEnabledChanged;
		
		protected virtual void OnIsEnabledChanged(EventArgs e)
		{
			if (IsEnabledChanged != null) {
				IsEnabledChanged(this, e);
			}
		}
		
		/// <summary>
		/// 行号
		/// </summary>
		public int LineNumber {
			get {
				return lineNumber;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException("value", value, "line number must be >= 0");
				if (lineNumber != value) {
					lineNumber = value;
					OnLineNumberChanged(EventArgs.Empty);
				}
			}
		}
		
		public event EventHandler LineNumberChanged;
		
		protected virtual void OnLineNumberChanged(EventArgs e)
		{
			if (LineNumberChanged != null) {
				LineNumberChanged(this, e);
			}
		}

		/// <summary>
		/// 获取是否可以使用“set/unset bookmark”命令关闭书签
		/// </summary>

		public virtual bool CanToggle {
			get {
				return true;
			}
		}
		
		public Bookmark(IDocument document, int lineNumber) : 
			this(document, lineNumber, true)
		{
		}
		
		public Bookmark(IDocument document, int lineNumber, bool isEnabled)
		{
			if (lineNumber < 0)
				throw new ArgumentOutOfRangeException("lineNumber", lineNumber, "line number must be >= 0");
			this.document   = document;
			this.lineNumber = lineNumber;
			this.isEnabled  = isEnabled;
		}
		
		public virtual void Click(SWF.Control parent, SWF.MouseEventArgs e)
		{
			if (e.Button == SWF.MouseButtons.Left && CanToggle) {
				document.BookmarkManager.RemoveMark(this);
			}
		}
		
		/// <summary>
		/// 汉字图标条侧栏
		/// </summary>
		/// <param name="margin"></param>
		/// <param name="g"></param>
		/// <param name="p"></param>
		public virtual void Draw(IconBarMargin margin, Graphics g, Point p)
		{
			margin.DrawBookmark(g, p.Y, isEnabled);
		}
	}
}
