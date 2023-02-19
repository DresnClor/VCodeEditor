﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1262 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 单词类型
	/// </summary>
	public enum TextWordType {
		/// <summary>
		/// 单词
		/// </summary>
		Word,
		/// <summary>
		/// 空格
		/// </summary>
		Space,
		/// <summary>
		/// 制表符
		/// </summary>
		Tab
	}

	/// <summary>
	/// 单个单词,此类表示具有颜色信息的单个单词，单词的两个特殊版本是空格和制表符。
	/// </summary>
	public class TextWord
	{
		HighlightStyle  color;
		LineSegment     line;
		IDocument       document;
		
		int          offset;
		int          length;
		
		public sealed class SpaceTextWord : TextWord
		{
			public SpaceTextWord()
			{
				length = 1;
			}
			
			public SpaceTextWord(HighlightStyle color)
			{
				length = 1;
				base.SyntaxColor = color;
			}
			
			public override Font Font {
				get {
					return null;
				}
			}
			
			public override TextWordType Type {
				get {
					return TextWordType.Space;
				}
			}
			public override bool IsWhiteSpace {
				get {
					return true;
				}
			}
		}
		
		public sealed class TabTextWord : TextWord
		{
			public TabTextWord()
			{
				length = 1;
			}
			public TabTextWord(HighlightStyle color)
			{
				length = 1;
				base.SyntaxColor = color;
			}
			
			public override Font Font {
				get {
					return null;
				}
			}
			
			public override TextWordType Type {
				get {
					return TextWordType.Tab;
				}
			}
			public override bool IsWhiteSpace {
				get {
					return true;
				}
			}
		}
		
		static TextWord spaceWord = new SpaceTextWord();
		static TextWord tabWord   = new TabTextWord();
		
		public bool hasDefaultColor;
		
		static public TextWord Space {
			get {
				return spaceWord;
			}
		}
		
		static public TextWord Tab {
			get {
				return tabWord;
			}
		}
		
		public int Offset {
			get {
				return offset;
			}
		}
		
		public int Length {
			get {
				return length;
			}
		}
		
		public bool HasDefaultColor {
			get {
				return hasDefaultColor;
			}
		}
		
		public virtual TextWordType Type {
			get {
				return TextWordType.Word;
			}
		}
		
		public string Word {
			get {
				if (document == null) {
					return String.Empty;
				}
				return document.GetText(line.Offset + offset, length);
			}
		}
		
		public virtual Font Font {
			get {
				return color.Font;
			}
		}
		
		public Color Color {
			get {
				return color.Color;
			}
		}
		
		public HighlightStyle SyntaxColor {
			get {
				return color;
			}
			set {
				Debug.Assert(value != null);
				color = value;
			}
		}
		
		public virtual bool IsWhiteSpace {
			get {
				return false;
			}
		}
		
		protected TextWord()
		{
		}
		
		// TAB
		public TextWord(IDocument document, LineSegment line, int offset, int length, HighlightStyle color, bool hasDefaultColor)
		{
			Debug.Assert(document != null);
			Debug.Assert(line != null);
			Debug.Assert(color != null);
			
			this.document = document;
			this.line  = line;
			this.offset = offset;
			this.length = length;
			this.color = color;
			this.hasDefaultColor = hasDefaultColor;
		}
		
		/// <summary>
		/// Converts a <see cref="TextWord"/> instance to string (for debug purposes)
		/// </summary>
		public override string ToString()
		{
			return "[TextWord: Word = " + Word + ", Font = " + Font + ", Color = " + Color + "]";
		}
	}
}
