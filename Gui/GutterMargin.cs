// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using VCodeEditor.Document;

namespace VCodeEditor
{
	/// <summary>
	/// 装订线边栏
	/// </summary>
	public class GutterMargin : AbstractMargin, IDisposable
	{
		StringFormat numberStringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
		
		public static Cursor RightLeftCursor;
		
		static GutterMargin()
		{
			Stream cursorStream = Assembly.GetCallingAssembly().GetManifestResourceStream("VCodeEditor.Resources.RightArrow.cur");
			RightLeftCursor = new Cursor(cursorStream);
			cursorStream.Close();
		}
		
		public void Dispose()
		{
			numberStringFormat.Dispose();
		}
		
		public override Cursor Cursor {
			get {
				return RightLeftCursor;
			}
		}
		
		public override Size Size {
			get {
				return new Size((int)(textArea.TextView.WideSpaceWidth
				                      * Math.Max(3, (int)Math.Log10(textArea.Document.TotalNumberOfLines) + 1)),
				                -1);
			}
		}
		
		public override bool IsVisible {
			get {
				return textArea.TextEditorProperties.ShowLineNumbers;
			}
		}
		
		public GutterMargin(TextArea textArea) : base(textArea)
		{
			numberStringFormat.LineAlignment = StringAlignment.Far;
			numberStringFormat.FormatFlags   = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.FitBlackBox |
				StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
		}
		
		public override void Paint(Graphics g, Rectangle rect)
		{
			if (rect.Width <= 0 || rect.Height <= 0) {
				return;
			}
			HighlightStyle lineNumberPainterColor = textArea.Document.HighlightingStrategy.GetStyleFor("LineNumbers");
			int fontHeight = textArea.TextView.FontHeight;
			Brush fillBrush = textArea.Enabled ? BrushRegistry.GetBrush(lineNumberPainterColor.BackgroundColor) : SystemBrushes.InactiveBorder;
			Brush drawBrush = BrushRegistry.GetBrush(lineNumberPainterColor.Color);
			for (int y = 0; y < (DrawingPosition.Height + textArea.TextView.VisibleLineDrawingRemainder) / fontHeight + 1; ++y) {
				int ypos = drawingPosition.Y + fontHeight * y  - textArea.TextView.VisibleLineDrawingRemainder;
				Rectangle backgroundRectangle = new Rectangle(drawingPosition.X, ypos, drawingPosition.Width, fontHeight);
				if (rect.IntersectsWith(backgroundRectangle)) {
					g.FillRectangle(fillBrush, backgroundRectangle);
					int curLine = textArea.Document.GetFirstLogicalLine(textArea.Document.GetVisibleLine(textArea.TextView.FirstVisibleLine) + y);
					
					if (curLine < textArea.Document.TotalNumberOfLines) {
						g.DrawString((curLine + 1).ToString(),
						             lineNumberPainterColor.Font,
						             drawBrush,
						             backgroundRectangle,
						             numberStringFormat);
					}
				}
			}
		}
		
		Point selectionStartPos;
		bool selectionComeFromGutter = false;
		bool selectionGutterDirectionDown = false; // 排水沟选择的方向会影响选择是从行的开始还是从线的末端开始
		public override void HandleMouseDown(Point mousepos, MouseButtons mouseButtons)
		{
			selectionComeFromGutter = true;
			int realline = textArea.TextView.GetLogicalLine(mousepos);
			if (realline >= 0 && realline < textArea.Document.TotalNumberOfLines) {
				if((Control.ModifierKeys & Keys.Shift) != 0 && textArea.SelectionManager.HasSomethingSelected) {
					// 让鼠标移动在排水沟中处理换档单击
					HandleMouseMove(mousepos, mouseButtons);
				} else {
					selectionGutterDirectionDown = false; // 重置用于鼠标移动中的处理标志
					selectionStartPos = new Point(0, realline);
					textArea.SelectionManager.ClearSelection();
					textArea.SelectionManager.SetSelection(new DefaultSelection(textArea.Document, selectionStartPos, new Point(textArea.Document.GetLineSegment(realline).Length + 1, realline)));
					textArea.Caret.Position = selectionStartPos;
				}
			}
		}
		
		public override void HandleMouseLeave(EventArgs e)
		{
			selectionComeFromGutter = false;
		}
		
		public override void HandleMouseMove(Point mousepos, MouseButtons mouseButtons)
		{
			if (mouseButtons == MouseButtons.Left) {
				if (selectionComeFromGutter) {
					//TODO: 修复鼠标移到排水沟左侧的处理，然后向下移动选择。 在选择线条时鼠标移动排水沟左侧后，选择行为发生变化
					int realline       = textArea.TextView.GetLogicalLine(mousepos);
					Point realmousepos = new Point(0, realline);
					if (realmousepos.Y < textArea.Document.TotalNumberOfLines) {
						if (selectionStartPos.Y == realmousepos.Y) {
							// 此设置为向上移动选择的设置选择默认值
							textArea.SelectionManager.SetSelection(new DefaultSelection(textArea.Document, realmousepos, new Point(0, realmousepos.Y + 1)));
							selectionGutterDirectionDown = false;
						} else if (selectionStartPos.Y < realmousepos.Y && textArea.SelectionManager.HasSomethingSelected) {
							// 这修复了向下移动选择的选择
							if (! selectionGutterDirectionDown) { //realmousepos.Y - selectionStartPos.Y == 1) {
								selectionGutterDirectionDown = true;
								textArea.SelectionManager.SetSelection(new DefaultSelection(textArea.Document, selectionStartPos, new Point(0, selectionStartPos.Y)));
								//这强制执行屏幕区域更新
								textArea.SelectionManager.ExtendSelection(textArea.SelectionManager.SelectionCollection[0].EndPosition, new Point(0, realmousepos.Y + 1));
							} else {
								// 选择延长到当前行的末尾
								textArea.SelectionManager.ExtendSelection(textArea.SelectionManager.SelectionCollection[0].EndPosition, new Point(0, realmousepos.Y + 1));
							}
						} else {
							if(textArea.SelectionManager.HasSomethingSelected) {
								// 这修复了向上移动选择的选择
								if (selectionGutterDirectionDown) { // selectionStartPos.Y - realmousepos.Y == 1) { // only fix for first line movement
									selectionGutterDirectionDown = false;
									textArea.SelectionManager.SetSelection(new DefaultSelection(textArea.Document, selectionStartPos, new Point(0, realmousepos.Y + 1)));
									// 将扩展选择移到此处以修复文本区域更新问题
									textArea.SelectionManager.ExtendSelection(selectionStartPos, realmousepos);
								} else {
									textArea.SelectionManager.ExtendSelection(textArea.Caret.Position, realmousepos);
								}
							}
						}
						
						textArea.Caret.Position = realmousepos;
					}
				} else {
					if (textArea.SelectionManager.HasSomethingSelected) {
						selectionStartPos  = textArea.Document.OffsetToPosition(textArea.SelectionManager.SelectionCollection[0].Offset);
						int realline       = textArea.TextView.GetLogicalLine(mousepos);
						Point realmousepos = new Point(0, realline);
						if (realmousepos.Y < textArea.Document.TotalNumberOfLines) {
							textArea.SelectionManager.ExtendSelection(textArea.Caret.Position, realmousepos);
						}
						textArea.Caret.Position = realmousepos;
					}
				}
			}
		}
	}
}
