// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;

using MeltuiCodeEditor.Document;
using MeltuiCodeEditor.Util;
using MeltuiCodeEditor;

namespace MeltuiCodeEditor.Gui.CompletionWindow
{
	/// <summary>
	/// 说明视图窗口接口
	/// </summary>
	public interface IDeclarationViewWindow
	{
		/// <summary>
		/// 说明信息
		/// </summary>
		string Description {
			get;
			set;
		}
		/// <summary>
		/// 显示说明窗口
		/// </summary>
		void ShowDeclarationViewWindow();
		/// <summary>
		/// 关闭说明窗口
		/// </summary>
		void CloseDeclarationViewWindow();
	}

	/// <summary>
	/// 说明视图窗口实现
	/// </summary>
	public class DeclarationViewWindow : Form, IDeclarationViewWindow
	{
		string description = String.Empty;
		
		public string Description {
			get {
				return description;
			}
			set {
				description = value;
				if (value == null && Visible) {
					Visible = false;
				} else if (value != null) {
					if (!Visible) ShowDeclarationViewWindow();
					Refresh();
				}
			}
		}
		
		public bool HideOnClick;
		
		public DeclarationViewWindow(Form parent)
		{
			SetStyle(ControlStyles.Selectable, false);
			StartPosition   = FormStartPosition.Manual;
			FormBorderStyle = FormBorderStyle.None;
			Owner           = parent;
			ShowInTaskbar   = false;
			Size            = new Size(0, 0);
			base.CreateHandle();
		}
		
		protected override CreateParams CreateParams {
			get {
				CreateParams p = base.CreateParams;
				AbstCompleWindow.AddShadowToWindow(p);
				return p;
			}
		}
		
		protected override bool ShowWithoutActivation {
			get {
				return true;
			}
		}
		
		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (HideOnClick) Hide();
		}
		
		public void ShowDeclarationViewWindow()
		{
			Show();
		}
		
		public void CloseDeclarationViewWindow()
		{
			Close();
			Dispose();
		}
		
		protected override void OnPaint(PaintEventArgs pe)
		{
			if (description != null && description.Length > 0) {
				TipPainterTools.DrawHelpTipFromCombinedDescription(this, pe.Graphics, Font, null, description);
			}
		}
		
		protected override void OnPaintBackground(PaintEventArgs pe)
		{
			pe.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0XE5,0XE5,0XE6)), pe.ClipRectangle);
		}
	}
}
