// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;

using VCodeEditor.Document;
using VCodeEditor.Util;
using VCodeEditor;

namespace VCodeEditor.Gui.CompletionWindow
{
	/// <summary>
	/// 抽象自动完成窗口
	/// </summary>
	public abstract class AbstCompleWindow : System.Windows.Forms.Form
	{
		protected TextEditorControl control;
		protected string            fileName;
		protected Size              drawingSize;
		Rectangle workingScreen;
		Form parentForm;
		
		protected AbstCompleWindow(Form parentForm, TextEditorControl control, string fileName)
		{
			workingScreen = Screen.GetWorkingArea(parentForm);
//			SetStyle(ControlStyles.Selectable, false);
			this.parentForm = parentForm;
			this.control  = control;
			this.fileName = fileName;
			
			SetLocation();
			StartPosition   = FormStartPosition.Manual;
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar   = false;
			Size            = new Size(1, 1);
		}
		
		protected virtual void SetLocation()
		{
			TextArea textArea = control.ActiveTextAreaControl.TextArea;
			Point caretPos  = textArea.Caret.Position;
			
			int xpos = textArea.TextView.GetDrawingXPos(caretPos.Y, caretPos.X);
			int rulerHeight = textArea.TextEditorProperties.ShowHorizontalRuler ? textArea.TextView.FontHeight : 0;
			Point pos = new Point(textArea.TextView.DrawingPosition.X + xpos,
			                      textArea.TextView.DrawingPosition.Y + (textArea.Document.GetVisibleLine(caretPos.Y)) * textArea.TextView.FontHeight - textArea.TextView.TextArea.VirtualTop.Y + textArea.TextView.FontHeight + rulerHeight);
			
			Point location = control.ActiveTextAreaControl.PointToScreen(pos);
			
			// 设置界限
			Rectangle bounds = new Rectangle(location, drawingSize);
			
			if (!workingScreen.Contains(bounds)) {
				if (bounds.Right > workingScreen.Right) {
					bounds.X = workingScreen.Right - bounds.Width;
				}
				if (bounds.Left < workingScreen.Left) {
					bounds.X = workingScreen.Left;
				}
				if (bounds.Top < workingScreen.Top) {
					bounds.Y = workingScreen.Top;
				}
				if (bounds.Bottom > workingScreen.Bottom) {
					bounds.Y = bounds.Y - bounds.Height - control.ActiveTextAreaControl.TextArea.TextView.FontHeight;
					if (bounds.Bottom > workingScreen.Bottom) {
						bounds.Y = workingScreen.Bottom - bounds.Height;
					}
				}
			}
			Bounds = bounds;
		}
		
		protected override CreateParams CreateParams {
			get {
				CreateParams p = base.CreateParams;
				AddShadowToWindow(p);
				return p;
			}
		}
		
		static int shadowStatus;
		
		/// <summary>
		/// 如果由操作系统支持，则向创建参数添加阴影。
		/// </summary>
		public static void AddShadowToWindow(CreateParams createParams)
		{
			if (shadowStatus == 0) {
				// Test OS version
				shadowStatus = -1; // 不支持阴影
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					Version ver = Environment.OSVersion.Version;
					if (ver.Major > 5 || ver.Major == 5 && ver.Minor >= 1) {
						shadowStatus = 1;
					}
				}
			}
			if (shadowStatus == 1) {
				createParams.ClassStyle |= 0x00020000; // set CS_DROPSHADOW
			}
		}
		
		protected override bool ShowWithoutActivation {
			get {
				return true;
			}
		}
		
		protected void ShowCompletionWindow()
		{
			Owner = parentForm;
			Enabled = true;
			this.Show();
			
			control.Focus();
			
			if (parentForm != null) {
				parentForm.LocationChanged += new EventHandler(this.ParentFormLocationChanged);
			}
			
			control.ActiveTextAreaControl.VScrollBar.ValueChanged     += new EventHandler(ParentFormLocationChanged);
			control.ActiveTextAreaControl.HScrollBar.ValueChanged     += new EventHandler(ParentFormLocationChanged);
			control.ActiveTextAreaControl.TextArea.DoProcessDialogKey += new DialogKeyProcessor(ProcessTextAreaKey);
			control.ActiveTextAreaControl.Caret.PositionChanged       += new EventHandler(CaretOffsetChanged);
			control.ActiveTextAreaControl.TextArea.LostFocus          += new EventHandler(this.TextEditorLostFocus);
			control.Resize += new EventHandler(ParentFormLocationChanged);
		}
		
		void ParentFormLocationChanged(object sender, EventArgs e)
		{
			SetLocation();
		}
		
		public virtual bool ProcessKeyEvent(char ch)
		{
			return false;
		}
		
		protected virtual bool ProcessTextAreaKey(Keys keyData)
		{
			if (!Visible) {
				return false;
			}
			switch (keyData) {
				case Keys.Escape:
					Close();
					return true;
			}
			return false;
		}
		
		protected virtual void CaretOffsetChanged(object sender, EventArgs e)
		{
		}
		
		protected void TextEditorLostFocus(object sender, EventArgs e)
		{
			if (!control.ActiveTextAreaControl.TextArea.Focused && !this.ContainsFocus) {
				//Close();
			}
		}
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			
			// 取出插入的方法
			parentForm.LocationChanged -= new EventHandler(ParentFormLocationChanged);
			
			control.ActiveTextAreaControl.VScrollBar.ValueChanged     -= new EventHandler(ParentFormLocationChanged);
			control.ActiveTextAreaControl.HScrollBar.ValueChanged     -= new EventHandler(ParentFormLocationChanged);
			
			control.ActiveTextAreaControl.TextArea.LostFocus          -= new EventHandler(this.TextEditorLostFocus);
			control.ActiveTextAreaControl.Caret.PositionChanged       -= new EventHandler(CaretOffsetChanged);
			control.ActiveTextAreaControl.TextArea.DoProcessDialogKey -= new DialogKeyProcessor(ProcessTextAreaKey);
			control.Resize -= new EventHandler(ParentFormLocationChanged);
			Dispose();
		}
	}
}
