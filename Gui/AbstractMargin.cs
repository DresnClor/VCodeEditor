// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using VCodeEditor.Document;

namespace VCodeEditor
{
	public delegate void MarginMouseEventHandler(AbstractMargin sender, Point mousepos, MouseButtons mouseButtons);
	public delegate void MarginPaintEventHandler(AbstractMargin sender, Graphics g, Rectangle rect);
	
	/// <summary>
	/// 侧栏 行数和折叠标记 抽象类
	/// </summary>
	public abstract class AbstractMargin
	{
		protected Rectangle drawingPosition = new Rectangle(0, 0, 0, 0);
		protected TextArea  textArea;

		/// <summary>
		/// 绘图位置
		/// </summary>
		public Rectangle DrawingPosition {
			get {
				return drawingPosition;
			}
			set {
				drawingPosition = value;
			}
		}
		
		/// <summary>
		/// 文本区域
		/// </summary>
		public TextArea TextArea {
			get {
				return textArea;
			}
		}
		
		/// <summary>
		/// 文档
		/// </summary>
		public IDocument Document {
			get {
				return textArea.Document;
			}
		}
		
		/// <summary>
		/// 编辑属性
		/// </summary>
		public ITextEditorProperties TextEditorProperties {
			get {
				return textArea.Document.TextEditorProperties;
			}
		}
		/// <summary>
		/// 鼠标指针
		/// </summary>
		public virtual Cursor Cursor {
			get {
				return Cursors.Default;
			}
		}
		
		/// <summary>
		/// 大小
		/// </summary>
		public virtual Size Size {
			get {
				return new Size(-1, -1);
			}
		}
		
		/// <summary>
		/// 是否可视
		/// </summary>
		public virtual bool IsVisible {
			get {
				return true;
			}
		}
		
		protected AbstractMargin(TextArea textArea)
		{
			this.textArea = textArea;
		}
		
		public virtual void HandleMouseDown(Point mousepos, MouseButtons mouseButtons)
		{
			if (MouseDown != null) {
				MouseDown(this, mousepos, mouseButtons);
			}
		}
		public virtual void HandleMouseMove(Point mousepos, MouseButtons mouseButtons)
		{
			if (MouseMove != null) {
				MouseMove(this, mousepos, mouseButtons);
			}
		}
		public virtual void HandleMouseLeave(EventArgs e)
		{
			if (MouseLeave != null) {
				MouseLeave(this, e);
			}
		}
		
		public virtual void Paint(Graphics g, Rectangle rect)
		{
			if (Painted != null) {
				Painted(this, g, rect);
			}
		}
		
		public event MarginPaintEventHandler Painted;
		public event MarginMouseEventHandler MouseDown;
		public event MarginMouseEventHandler MouseMove;
		public event EventHandler            MouseLeave;
	}
}
