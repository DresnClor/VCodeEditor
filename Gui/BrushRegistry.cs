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
	/// <summary>
	/// 画刷注册
	/// </summary>
	public class BrushRegistry
	{
        /// <summary>
        /// 画刷哈希表
        /// </summary>
        static Hashtable brushes = new Hashtable();
		/// <summary>
		/// 笔哈希表
		/// </summary>
		static Hashtable pens    = new Hashtable();
		/// <summary>
		/// 点笔哈希表
		/// </summary>
		static Hashtable dotPens = new Hashtable();
		
		/// <summary>
		/// 获取画刷
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static Brush GetBrush(Color color)
		{
			if (!brushes.Contains(color)) {
				Brush newBrush = new SolidBrush(color);
				brushes.Add(color, newBrush);
				return newBrush;
			}
			return brushes[color] as Brush;
		}
		
		/// <summary>
		/// 获取笔
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static Pen GetPen(Color color)
		{
			if (!pens.Contains(color)) {
				Pen newPen = new Pen(color);
				pens.Add(color, newPen);
				return newPen;
			}
			return pens[color] as Pen;
		}
		
		/// <summary>
		/// 获取点笔
		/// </summary>
		/// <param name="bgColor"></param>
		/// <param name="fgColor"></param>
		/// <returns></returns>
		public static Pen GetDotPen(Color bgColor, Color fgColor)
		{
			bool containsBgColor = dotPens.Contains(bgColor);
			if (!containsBgColor || !((Hashtable)dotPens[bgColor]).Contains(fgColor)) {
				if (!containsBgColor) {
					dotPens[bgColor] = new Hashtable();
				}
				
				HatchBrush hb = new HatchBrush(HatchStyle.Percent50, bgColor, fgColor);
				Pen newPen = new Pen(hb);
				((Hashtable)dotPens[bgColor])[fgColor] = newPen;
				return newPen;
			}
			return ((Hashtable)dotPens[bgColor])[fgColor] as Pen;
		}
	}
}
