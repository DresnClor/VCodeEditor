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
using MeltuiCodeEditor.Document;

namespace MeltuiCodeEditor
{
	/// <summary>
	/// 侧栏鼠标事件
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="mousepos"></param>
	/// <param name="mouseButtons"></param>
	public delegate void MarginMouseEventHandler(AbstractMargin sender, Point mousepos, MouseButtons mouseButtons);

	/// <summary>
	/// 侧栏重绘事件
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="g"></param>
	/// <param name="rect"></param>
	public delegate void MarginPaintEventHandler(AbstractMargin sender, Graphics g, Rectangle rect);

	/// <summary>
	/// 侧栏 行数和折叠标记 抽象类
	/// </summary>
	public abstract class AbstractMargin
	{
		protected Rectangle drawingPosition = new Rectangle(0, 0, 0, 0);
		protected TextArea textArea;

		/// <summary>
		/// 绘图位置
		/// </summary>
		public Rectangle DrawingPosition
		{
			get
			{
				return drawingPosition;
			}
			set
			{
				drawingPosition = value;
			}
		}

		/// <summary>
		/// 文本区域
		/// </summary>
		public TextArea TextArea
		{
			get
			{
				return textArea;
			}
		}

		/// <summary>
		/// 文档
		/// </summary>
		public IDocument Document
		{
			get
			{
				return textArea.Document;
			}
		}

		/// <summary>
		/// 编辑属性
		/// </summary>
		public ITextEditorProperties TextEditorProperties
		{
			get
			{
				return textArea.Document.TextEditorProperties;
			}
		}
		/// <summary>
		/// 鼠标指针
		/// </summary>
		public virtual Cursor Cursor
		{
			get
			{
				return Cursors.Default;
			}
		}

		/// <summary>
		/// 大小
		/// </summary>
		public virtual Size Size
		{
			get
			{
				return new Size(-1, -1);
			}
		}

		/// <summary>
		/// 是否可视
		/// </summary>
		public virtual bool IsVisible
		{
			get
			{
				return true;
			}
		}

		protected AbstractMargin(TextArea textArea)
		{
			this.textArea = textArea;
		}

		public virtual void HandleMouseDown(Point mousepos, MouseButtons mouseButtons)
		{
			if (MouseDown != null)
			{
				MouseDown(this, mousepos, mouseButtons);
			}
		}
		public virtual void HandleMouseMove(Point mousepos, MouseButtons mouseButtons)
		{
			if (MouseMove != null)
			{
				MouseMove(this, mousepos, mouseButtons);
			}
		}
		public virtual void HandleMouseLeave(EventArgs e)
		{
			if (MouseLeave != null)
			{
				MouseLeave(this, e);
			}
		}

		public virtual void Paint(Graphics g, Rectangle rect)
		{
			if (Painted != null)
			{
				Painted(this, g, rect);
			}
		}

		/// <summary>
		/// 重绘
		/// </summary>
		public event MarginPaintEventHandler Painted;

		/// <summary>
		/// 鼠标按下
		/// </summary>
		public event MarginMouseEventHandler MouseDown;

		/// <summary>
		/// 鼠标移动
		/// </summary>
		public event MarginMouseEventHandler MouseMove;

		/// <summary>
		/// 鼠标离开
		/// </summary>
		public event EventHandler MouseLeave;
	}
}
