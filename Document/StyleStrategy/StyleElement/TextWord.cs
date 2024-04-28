// <file>
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
	public enum TextWordType
	{
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
		ColorStyle color;
		LineSegment line;
		IDocument document;

		int offset;
		int length;

		/// <summary>
		/// 空格单词
		/// </summary>
		public sealed class SpaceTextWord : TextWord
		{
			public SpaceTextWord()
			{
				length = 1;
			}

			public SpaceTextWord(ColorStyle color)
			{
				length = 1;
				base.SyntaxStyle = color;
			}

			public override Font Font
			{
				get
				{
					return null;
				}
			}

			public override TextWordType Type
			{
				get
				{
					return TextWordType.Space;
				}
			}
			public override bool IsWhiteSpace
			{
				get
				{
					return true;
				}
			}
		}

		/// <summary>
		/// 制表符单词
		/// </summary>
		public sealed class TabTextWord : TextWord
		{
			public TabTextWord()
			{
				length = 1;
			}
			public TabTextWord(ColorStyle color)
			{
				length = 1;
				base.SyntaxStyle = color;
			}

			public override Font Font
			{
				get
				{
					return null;
				}
			}

			public override TextWordType Type
			{
				get
				{
					return TextWordType.Tab;
				}
			}
			public override bool IsWhiteSpace
			{
				get
				{
					return true;
				}
			}
		}

		static TextWord spaceWord = new SpaceTextWord();
		static TextWord tabWord = new TabTextWord();

		public bool hasDefaultColor;

		/// <summary>
		/// 空格单词
		/// </summary>
		static public TextWord Space
		{
			get
			{
				return spaceWord;
			}
		}

		/// <summary>
		/// 制表符单词
		/// </summary>
		static public TextWord Tab
		{
			get
			{
				return tabWord;
			}
		}

		/// <summary>
		/// 偏移位置
		/// </summary>
		public int Offset
		{
			get
			{
				return offset;
			}
		}

		/// <summary>
		/// 长度
		/// </summary>
		public int Length
		{
			get
			{
				return length;
			}
		}

		public bool HasDefaultColor
		{
			get
			{
				return hasDefaultColor;
			}
		}

		/// <summary>
		/// 单词类型
		/// </summary>
		public virtual TextWordType Type
		{
			get
			{
				return TextWordType.Word;
			}
		}

		/// <summary>
		/// 单词文本内容
		/// </summary>
		public string Word
		{
			get
			{
				if (document == null)
				{
					return String.Empty;
				}
				return document.GetText(line.Offset + offset, length);
			}
		}

		/// <summary>
		/// 字体
		/// </summary>
		public virtual Font Font
		{
			get
			{
				return color.Font;
			}
		}

		/// <summary>
		/// 颜色
		/// </summary>
		public Color Color
		{
			get
			{
				return color.Color;
			}
		}

		/// <summary>
		/// 语法样式
		/// </summary>
		public ColorStyle SyntaxStyle
		{
			get
			{
				return color;
			}
			set
			{
				Debug.Assert(value != null);
				color = value;
			}
		}

		/// <summary>
		/// 是否为空白
		/// </summary>
		public virtual bool IsWhiteSpace
		{
			get
			{
				return false;
			}
		}

		protected TextWord()
		{
		}

		// TAB
		public TextWord(IDocument document, LineSegment line, int offset, int length, ColorStyle color, bool hasDefaultColor)
		{
			Debug.Assert(document != null);
			Debug.Assert(line != null);
			Debug.Assert(color != null);

			this.document = document;
			this.line = line;
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
