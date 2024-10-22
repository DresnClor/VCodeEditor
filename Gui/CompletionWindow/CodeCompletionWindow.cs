﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1105 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

using VCodeEditor;

namespace VCodeEditor.Gui.CompletionWindow
{
	/// <summary>
	/// 代码自动补全窗口
	/// </summary>
	public class CodeCompletionWindow : AbstCompleWindow
	{
		ICompletionData[] completionData;
		CodeCompleListView   codeCompletionListView;
		VScrollBar    vScrollBar = new VScrollBar();
		ICompletionDataProvider dataProvider;
		
		int                      startOffset;
		int                      endOffset;
		DeclarationViewWindow    declarationViewWindow = null;
		Rectangle workingScreen;

		/// <summary>
		/// 显示自动补全窗口
		/// </summary>
		/// <param name="parent">父窗体</param>
		/// <param name="control">编辑器控件</param>
		/// <param name="fileName">文件名称</param>
		/// <param name="completionDataProvider">数据接口实现</param>
		/// <param name="firstChar">第一个字符</param>
		/// <returns></returns>
		public static CodeCompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar)
		{
			ICompletionData[] completionData = completionDataProvider.GenerateCompletionData(fileName, control.ActiveTextAreaControl.TextArea, firstChar);
			if (completionData == null || completionData.Length == 0) {
				return null;
			}
			CodeCompletionWindow codeCompletionWindow = new CodeCompletionWindow(completionDataProvider, completionData, parent, control, fileName);
			codeCompletionWindow.ShowCompletionWindow();
			return codeCompletionWindow;
		}
		
		CodeCompletionWindow(
			ICompletionDataProvider completionDataProvider, 
			ICompletionData[] completionData, 
			Form parentForm, 
			TextEditorControl control, 
			string fileName) : 
			base(parentForm, control, fileName)
		{
			this.dataProvider = completionDataProvider;
			this.completionData = completionData;
			
			workingScreen = Screen.GetWorkingArea(Location);
			startOffset = control.ActiveTextAreaControl.Caret.Offset + 1;
			endOffset   = startOffset;
			if (completionDataProvider.PreSelection != null) {
				startOffset -= completionDataProvider.PreSelection.Length + 1;
				endOffset--;
			}
			
			codeCompletionListView = new CodeCompleListView(completionData);
			codeCompletionListView.ImageList = completionDataProvider.ImageList;
			codeCompletionListView.Dock = DockStyle.Fill;
			codeCompletionListView.SelectedItemChanged += new EventHandler(CodeCompletionListViewSelectedItemChanged);
			codeCompletionListView.DoubleClick += new EventHandler(CodeCompletionListViewDoubleClick);
			codeCompletionListView.Click  += new EventHandler(CodeCompletionListViewClick);
			Controls.Add(codeCompletionListView);
			
			if (completionData.Length > 10) {
				vScrollBar.Dock = DockStyle.Right;
				vScrollBar.Minimum = 0;
				vScrollBar.Maximum = completionData.Length - 8;
				vScrollBar.SmallChange = 1;
				vScrollBar.LargeChange = 3;
				codeCompletionListView.FirstItemChanged += new EventHandler(CodeCompletionListViewFirstItemChanged);
				Controls.Add(vScrollBar);
			}
			
			this.drawingSize = new Size(
				codeCompletionListView.ItemHeight * 10, 
				codeCompletionListView.ItemHeight * Math.Min(10, completionData.Length));
			SetLocation();
			
			if (declarationViewWindow == null) {
				declarationViewWindow = new DeclarationViewWindow(parentForm);
			}
			SetDeclarationViewLocation();
			declarationViewWindow.ShowDeclarationViewWindow();
			control.Focus();
			CodeCompletionListViewSelectedItemChanged(this, EventArgs.Empty);
			
			if (completionDataProvider.DefaultIndex >= 0) {
				codeCompletionListView.SelectIndex(completionDataProvider.DefaultIndex);
			}
			
			if (completionDataProvider.PreSelection != null) {
				CaretOffsetChanged(this, EventArgs.Empty);
			}
			
			vScrollBar.Scroll += new ScrollEventHandler(DoScroll);
		}
		
		void CodeCompletionListViewFirstItemChanged(object sender, EventArgs e)
		{
			vScrollBar.Value = Math.Min(vScrollBar.Maximum, codeCompletionListView.FirstItem);
		}
		
		void SetDeclarationViewLocation()
		{
			//  此方法使用具有更多自由空间的一面
			int leftSpace = Bounds.Left - workingScreen.Left;
			int rightSpace = workingScreen.Right - Bounds.Right;
			Point pos;
			// 在使用时，声明视图窗口具有更好的断线效果
			//右侧，所以更喜欢右侧到左侧。
			if (rightSpace * 2 > leftSpace)
				pos = new Point(Bounds.Right, Bounds.Top);
			else
				pos = new Point(Bounds.Left - declarationViewWindow.Width, Bounds.Top);
			if (declarationViewWindow.Location != pos) {
				declarationViewWindow.Location = pos;
			}
		}
		
		protected override void SetLocation()
		{
			base.SetLocation();
			if (declarationViewWindow != null) {
				SetDeclarationViewLocation();
			}
		}
		
		public void HandleMouseWheel(MouseEventArgs e)
		{
			int MAX_DELTA  = 120; //基本上它是恒定的，但现在，但可以稍后由MS改变
			int multiplier = Math.Abs(e.Delta) / MAX_DELTA;
			
			int newValue;
			if (System.Windows.Forms.SystemInformation.MouseWheelScrollLines > 0) {
				newValue = this.vScrollBar.Value - (control.Property.MouseWheelScrollDown ? 1 : -1) * Math.Sign(e.Delta) * System.Windows.Forms.SystemInformation.MouseWheelScrollLines * vScrollBar.SmallChange * multiplier;
			} else {
				newValue = this.vScrollBar.Value - (control.Property.MouseWheelScrollDown ? 1 : -1) * Math.Sign(e.Delta) * vScrollBar.LargeChange;
			}
			vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum, newValue));
			DoScroll(this, null);
		}

		
		void CodeCompletionListViewSelectedItemChanged(object sender, EventArgs e)
		{
			ICompletionData data = codeCompletionListView.SelectedCompletionData;
			if (data != null && data.Description != null && data.Description.Length > 0) {
				declarationViewWindow.Description = data.Description;
				SetDeclarationViewLocation();
			} else {
				declarationViewWindow.Description = null;
			}
		}
		
		public override bool ProcessKeyEvent(char ch)
		{
			if (dataProvider.IsInsertionKey(ch)) {
				if (ch == ' ' && dataProvider.InsertSpace) {
					// 增量启动+结束和过程作为正常空间 increment start + end and process as normal space
					++startOffset;
				} else {
					return InsertSelectedItem(ch);
				}
			}
			dataProvider.InsertSpace = false;
			++endOffset;
			return base.ProcessKeyEvent(ch);
		}
		
		protected override void CaretOffsetChanged(object sender, EventArgs e)
		{
			int offset = control.ActiveTextAreaControl.Caret.Offset;
			if (offset == startOffset) {
				return;
			}
			if (offset < startOffset || offset > endOffset) {
				Close();
			} else {
				codeCompletionListView.SelectItemWithStart(control.Document.GetText(startOffset, offset - startOffset));
			}
		}
		
		protected void DoScroll(object sender, ScrollEventArgs sea)
		{
			codeCompletionListView.FirstItem = vScrollBar.Value;
			codeCompletionListView.Refresh();
			control.ActiveTextAreaControl.TextArea.Focus();
		}
		
		protected override bool ProcessTextAreaKey(Keys keyData)
		{
			if (!Visible) {
				return false;
			}
			
			switch (keyData) {
				case Keys.Back:
					--endOffset;
					if (endOffset < startOffset) {
						Close();
					}
					return false;
				case Keys.Delete:
					if (control.ActiveTextAreaControl.Caret.Offset <= endOffset) {
						--endOffset;
					}
					if (endOffset < startOffset) {
						Close();
					}
					return false;
				case Keys.Home:
					codeCompletionListView.SelectIndex(0);
					return true;
				case Keys.End:
					codeCompletionListView.SelectIndex(completionData.Length-1);
					return true;
				case Keys.PageDown:
					codeCompletionListView.PageDown();
					return true;
				case Keys.PageUp:
					codeCompletionListView.PageUp();
					return true;
				case Keys.Down:
				case Keys.Right:
					codeCompletionListView.SelectNextItem();
					return true;
				case Keys.Up:
				case Keys.Left:
					codeCompletionListView.SelectPrevItem();
					return true;
				case Keys.Tab:
				case Keys.Return:
					InsertSelectedItem('\0');
					return true;
			}
			return base.ProcessTextAreaKey(keyData);
		}
		
		void CodeCompletionListViewDoubleClick(object sender, EventArgs e)
		{
			InsertSelectedItem('\0');
		}
		
		void CodeCompletionListViewClick(object sender, EventArgs e)
		{
			control.ActiveTextAreaControl.TextArea.Focus();
		}
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Dispose();
			codeCompletionListView.Dispose();
			codeCompletionListView = null;
			declarationViewWindow.Dispose();
			declarationViewWindow = null;
		}
		
		bool InsertSelectedItem(char ch)
		{
			ICompletionData data = codeCompletionListView.SelectedCompletionData;
			bool result = false;
			if (data != null) {
				control.BeginUpdate();
				
				if (endOffset - startOffset > 0) {
					control.Document.Remove(startOffset, endOffset - startOffset);
				}
				if (dataProvider.InsertSpace) {
					control.Document.Insert(startOffset++, " ");
				}
				control.ActiveTextAreaControl.Caret.Position = control.Document.OffsetToPosition(startOffset);
				
				result = data.InsertAction(control.ActiveTextAreaControl.TextArea, ch);
				control.EndUpdate();
			}
			Close();
			return result;
		}
	}
}
