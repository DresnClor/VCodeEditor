// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 标记类型
	/// </summary>
	public enum TextMarkerType {
		/// <summary>
		/// 实心块
		/// </summary>
		SolidBlock, 
		/// <summary>
		/// 下划线
		/// </summary>
		Underlined,
		/// <summary>
		/// 波浪线
		/// </summary>
		WaveLine,
		//新加：
		/// <summary>
		/// TT符号 no
		/// </summary>
		TT,
		/// <summary>
		/// 文本删除线
		/// </summary>
		Strike,
		/// <summary>
		/// 边框  bug
		/// </summary>
		Box,
		/// <summary>
		/// 虚线
		/// </summary>
		Dash,
		/// <summary>
		/// 点线
		/// </summary>
		Dots,
		/// <summary>
		/// 点边框 bug
		/// </summary>
		DotBox,
		/// <summary>
		/// 上 到 下 渐变填充 bug
		/// </summary>
		Gradient,
		/// <summary>
		/// 居中渐变填充 bug
		/// </summary>
		GradientCentre,
		/// <summary>
		/// 绘制文本前景 no
		/// </summary>
		TextFore,
		/// <summary> 
		/// 三角箭头 no
		/// </summary>
		Triangle,
	}
	
	/// <summary>
	/// 文本标记
	/// </summary>
	public class TextMarker : AbstractSegment
	{
		TextMarkerType textMarkerType;
		Color          color;
		Color          foreColor;
		string         toolTip = null;
		bool           overrideForeColor = false;
		
		/// <summary>
		/// 标记类型
		/// </summary>
		public TextMarkerType TextMarkerType {
			get {
				return textMarkerType;
			}
		}
		
		/// <summary>
		/// 颜色
		/// </summary>
		public Color Color {
			get {
				return color;
			}
		}
		
		/// <summary>
		/// 前景颜色
		/// </summary>
		public Color ForeColor {
			get {
				return foreColor;
			}
		}

		/// <summary>
		/// 覆盖前景色
		/// </summary>
		public bool OverrideForeColor {
			get {
				return overrideForeColor;
			}
		}
		
		/// <summary>
		/// 提示文本
		/// </summary>
		public string ToolTip {
			get {
				return toolTip;
			}
			set {
				toolTip = value;
			}
		}
		
		public TextMarker(int offset, int length, TextMarkerType textMarkerType) : 
			this(offset, length, textMarkerType, Color.Red)
		{
		}
		
		public TextMarker(int offset, int length, TextMarkerType textMarkerType, Color color)
		{
			this.offset          = offset;
			this.length          = length;
			this.textMarkerType  = textMarkerType;
			this.color           = color;
		}
		
		public TextMarker(int offset, int length, TextMarkerType textMarkerType, Color color, Color foreColor)
		{
			this.offset          = offset;
			this.length          = length;
			this.textMarkerType  = textMarkerType;
			this.color           = color;
			this.foreColor       = foreColor;
			this.overrideForeColor = true;
		}
	}
}
