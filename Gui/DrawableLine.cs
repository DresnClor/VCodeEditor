// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 1242 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Drawing;
using VCodeEditor.Document;

namespace VCodeEditor
{
	/// <summary>
	/// 能够在任何控件上画出一条线的类（文本编辑器之外）
	/// </summary>
	public class DrawableLine
	{
		static StringFormat sf = (StringFormat)System.Drawing.StringFormat.GenericTypographic.Clone();
		
		List<SimpleTextWord> words = new List<SimpleTextWord>();
		SizeF spaceSize;
		Font monospacedFont;
		Font boldMonospacedFont;
		
		private class SimpleTextWord {
			internal TextWordType Type;
			internal string       Word;
			internal bool         Bold;
			internal Color        Color;
			
			public SimpleTextWord(TextWordType Type, string Word, bool Bold, Color Color)
			{
				this.Type = Type;
				this.Word = Word;
				this.Bold = Bold;
				this.Color = Color;
			}
			
			internal readonly static SimpleTextWord Space = new SimpleTextWord(TextWordType.Space, " ", false, Color.Black);
			internal readonly static SimpleTextWord Tab = new SimpleTextWord(TextWordType.Tab, "\t", false, Color.Black);
		}
		
		/// <summary>
		/// 绘制线
		/// </summary>
		/// <param name="document"></param>
		/// <param name="line"></param>
		/// <param name="monospacedFont"></param>
		/// <param name="boldMonospacedFont"></param>
		public DrawableLine(IDocument document, LineSegment line, Font monospacedFont, Font boldMonospacedFont)
		{
			this.monospacedFont = monospacedFont;
			this.boldMonospacedFont = boldMonospacedFont;
			if (line.Words != null) {
				foreach (TextWord word in line.Words) {
					if (word.Type == TextWordType.Space) {
						words.Add(SimpleTextWord.Space);
					} else if (word.Type == TextWordType.Tab) {
						words.Add(SimpleTextWord.Tab);
					} else {
						words.Add(new SimpleTextWord(TextWordType.Word, word.Word, word.Font.Bold, word.Color));
					}
				}
			} else {
				words.Add(new SimpleTextWord(TextWordType.Word, document.GetText(line), false, Color.Black));
			}
		}
		
		/// <summary>
		/// 线长度
		/// </summary>
		public int LineLength {
			get {
				int length = 0;
				foreach (SimpleTextWord word in words) {
					length += word.Word.Length;
				}
				return length;
			}
		}
		
		/// <summary>
		/// 设置粗体
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="endIndex"></param>
		/// <param name="bold"></param>
		public void SetBold(int startIndex, int endIndex, bool bold)
		{
			if (startIndex < 0)
				throw new ArgumentException("startIndex must be >= 0");
			if (startIndex > endIndex)
				throw new ArgumentException("startIndex must be <= endIndex");
			if (startIndex == endIndex) return;
			int pos = 0;
			for (int i = 0; i < words.Count; i++) {
				SimpleTextWord word = words[i];
				if (pos >= endIndex)
					break;
				int wordEnd = pos + word.Word.Length;
				// 3 种可能性:
				if (startIndex <= pos && endIndex >= wordEnd) {
					// 字是完全在地区: word is fully in region
					word.Bold = bold;
				} else if (startIndex <= pos) {
					// 字的开头是在区域
					int inRegionLength = endIndex - pos;
					SimpleTextWord newWord = new SimpleTextWord(word.Type, word.Word.Substring(inRegionLength), word.Bold, word.Color);
					words.Insert(i + 1, newWord);
					
					word.Bold = bold;
					word.Word = word.Word.Substring(0, inRegionLength);
				} else if (startIndex < wordEnd) {
					// 字的末尾在区域（或单词中间在区域）
					int notInRegionLength = startIndex - pos;
					
					SimpleTextWord newWord = new SimpleTextWord(word.Type, word.Word.Substring(notInRegionLength), word.Bold, word.Color);
					// newWord.Bold 将在下一次迭代中设置
					words.Insert(i + 1, newWord);
					
					word.Word = word.Word.Substring(0, notInRegionLength);
				}
				pos = wordEnd;
			}
		}
		
		/// <summary>
		/// 绘制文档单词
		/// </summary>
		/// <param name="g"></param>
		/// <param name="word"></param>
		/// <param name="position"></param>
		/// <param name="font"></param>
		/// <param name="foreColor"></param>
		/// <returns></returns>
		public static float DrawDocumentWord(Graphics g, string word, PointF position, Font font, Color foreColor)
		{
			if (word == null || word.Length == 0) {
				return 0f;
			}
			SizeF wordSize = g.MeasureString(word, font, 32768, sf);
			
			g.DrawString(word,
			             font,
			             BrushRegistry.GetBrush(foreColor),
			             position,
			             sf);
			return wordSize.Width;
		}
		
		/// <summary>
		/// 获取空格大小
		/// </summary>
		/// <param name="g"></param>
		/// <returns></returns>
		public SizeF GetSpaceSize(Graphics g)
		{
			if (spaceSize.IsEmpty) {
				spaceSize = g.MeasureString("-", boldMonospacedFont,  new PointF(0, 0), sf);
			}
			return spaceSize;
		}
		
		/// <summary>
		/// 绘制线
		/// </summary>
		/// <param name="g"></param>
		/// <param name="xPos"></param>
		/// <param name="xOffset"></param>
		/// <param name="yPos"></param>
		/// <param name="c"></param>
		public void DrawLine(Graphics g, ref float xPos, float xOffset, float yPos, Color c)
		{
			SizeF spaceSize = GetSpaceSize(g);
			foreach (SimpleTextWord word in words) {
				switch (word.Type) {
					case TextWordType.Space:
						xPos += spaceSize.Width;
						break;
					case TextWordType.Tab:
						float tabWidth = spaceSize.Width * 4;
						xPos += tabWidth;
						xPos = (int)((xPos + 2) / tabWidth) * tabWidth;
						break;
					case TextWordType.Word:
						xPos += DrawDocumentWord(g,
						                         word.Word,
						                         new PointF(xPos + xOffset, yPos),
						                         word.Bold ? boldMonospacedFont : monospacedFont,
						                         c == Color.Empty ? word.Color : c
						                        );
						break;
				}
			}
		}
		
		/// <summary>
		/// 绘制线
		/// </summary>
		/// <param name="g"></param>
		/// <param name="xPos"></param>
		/// <param name="xOffset"></param>
		/// <param name="yPos"></param>
		public void DrawLine(Graphics g, ref float xPos, float xOffset, float yPos)
		{
			DrawLine(g, ref xPos, xOffset, yPos, Color.Empty);
		}

		/// <summary>
		/// 测量宽度
		/// </summary>
		/// <param name="g"></param>
		/// <param name="xPos"></param>
		/// <returns></returns>
		public float MeasureWidth(Graphics g, float xPos)
		{
			SizeF spaceSize = GetSpaceSize(g);
			foreach (SimpleTextWord word in words) {
				switch (word.Type) {
					case TextWordType.Space:
						xPos += spaceSize.Width;
						break;
					case TextWordType.Tab:
						float tabWidth = spaceSize.Width * 4;
						xPos += tabWidth;
						xPos = (int)((xPos + 2) / tabWidth) * tabWidth;
						break;
					case TextWordType.Word:
						if (word.Word != null && word.Word.Length > 0) {
							xPos += g.MeasureString(word.Word, word.Bold ? boldMonospacedFont : monospacedFont, 32768, sf).Width;
						}
						break;
				}
			}
			return xPos;
		}
	}
}
