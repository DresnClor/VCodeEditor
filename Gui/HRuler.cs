﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VCodeEditor
{
	/// <summary>
	/// 横向标尺控件
	/// </summary>
	[ToolboxItem(false)]
	public class HRuler : UserControl
	{
		TextArea   textArea;
		
		public HRuler(TextArea textArea)
		{
			this.textArea = textArea;
		}
		
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			int num = 0;
			for (float x = textArea.TextView.DrawingPosition.Left; x < textArea.TextView.DrawingPosition.Right; x += textArea.TextView.WideSpaceWidth) {
				int offset = (Height * 2) / 3;
				if (num % 5 == 0) {
					offset = (Height * 4) / 5;
				}
				
				if (num % 10 == 0) {
					offset = 1;
				}
				++num;
				g.DrawLine(Pens.Black,
				           (int)x, offset, (int)x, Height - offset);
			}
		}
		
		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
		{
			e.Graphics.FillRectangle(Brushes.White,
			                         new Rectangle(0,
			                                       0,
			                                       Width,
			                                       Height));
			
		}
		
		
		
	}
}
