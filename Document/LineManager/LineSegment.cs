// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 行段落
	/// </summary>
	public sealed class LineSegment : AbstractSegment
	{
		int delimiterLength;
		
		List<TextWord> words              = null;
		Stack<Span>    highlightSpanStack = null;
		
		/// <summary>
		/// 取指定列的单词
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		public TextWord GetWord(int column)
		{
			int curColumn = 0;
			foreach (TextWord word in words) {
				if (column < curColumn + word.Length) {
					return word;
				}
				curColumn += word.Length;
			}
			return null;
		}
		
		/// <summary>
		/// 长度
		/// </summary>
		public override int Length {
			get	{
				return length - delimiterLength;
			}
			set {
				throw new System.NotSupportedException();
			}
		}
		
		/// <summary>
		/// 总长度
		/// </summary>
		public int TotalLength {
			get {
				return length;
			}
			
			set {
				length = value;
			}
		}

		/// <summary>
		/// 分隔符长度
		/// </summary>
		public int DelimiterLength {
			get {
				return delimiterLength;
			}
			set {
				delimiterLength = value;
			}
		}
		
		// 突出显示信息
		/// <summary>
		/// 单词列表
		/// </summary>
		public List<TextWord> Words {
			get {
				return words;
			}
			set {
				words = value;
			}
		}
		
		/// <summary>
		/// 获取指定位置的高亮颜色
		/// </summary>
		/// <param name="x">列号，从开始，行开始位置计算</param>
		/// <returns></returns>
		public ColorStyle GetColorForPosition(int x)
		{
			if (Words != null) {
				int xPos = 0;
				foreach (TextWord word in Words) {
					if (x < xPos + word.Length) {
						return word.SyntaxStyle;
					}
					xPos += word.Length;
				}
			}
			return new ColorStyle(Color.Black, false, false);
		}
		
		/// <summary>
		/// 规则范围栈
		/// </summary>
		public Stack<Span> HighlightSpanStack {
			get {
				return highlightSpanStack;
			}
			set {
				highlightSpanStack = value;
			}
		}
		
		public LineSegment(int offset, int end, int delimiterLength)
		{
			this.offset          = offset;
			this.delimiterLength = delimiterLength;
		
			this.TotalLength     = end - offset + 1;
		}
		
		public LineSegment(int offset, int length)
		{
			this.offset          = offset;
			this.length          = length;
			this.delimiterLength = 0;
		}
		
		/// <summary>
		/// 转换 <see cref="LineSegment"/> instance to string (for debug purposes)
		/// </summary>
		public override string ToString()
		{
			return "[LineSegment: Offset = "+ offset +", Length = " + Length + ", TotalLength = " + TotalLength + ", DelimiterLength = " + delimiterLength + "]";
		}
		
		// Svante Lidman: 重新考虑是否是正确的去切除移动这些甲虫在这里。
		
		/// <summary>
		/// 得到的字符串，这匹配的常规表达expr
		/// 在字符串s2在索引
		/// </summary>
		internal string GetRegString(char[] expr, int index, IDocument document)
		{
			int j = 0;
			StringBuilder regexpr = new StringBuilder();;
			
			for (int i = 0; i < expr.Length; ++i, ++j) {
				if (index + j >= this.Length) 
					break;
				
				switch (expr[i]) {
					case '@': // "特殊"含义
						++i;
						switch (expr[i]) {
							case '!': // 与以下表达方式不匹配
								StringBuilder whatmatch = new StringBuilder();
								++i;
								while (i < expr.Length && expr[i] != '@') {
									whatmatch.Append(expr[i++]);
								}
								break;
							case '@': // @
								regexpr.Append(document.GetCharAt(this.Offset + index + j));
								break;
						}
						break;
					default:
						if (expr[i] != document.GetCharAt(this.Offset + index + j)) {
							return regexpr.ToString();
						}
					regexpr.Append(document.GetCharAt(this.Offset + index + j));
					break;
				}
			}
			return regexpr.ToString();
		}
		
		/// <summary>
		/// 返回真实，如果得到字符串s2在索引匹配的表达expr
		/// </summary>
		internal bool MatchExpr(char[] expr, int index, IDocument document)
		{
			for (int i = 0, j = 0; i < expr.Length; ++i, ++j) {
				switch (expr[i]) {
					case '@': // "special" meaning
						++i;
						if (i < expr.Length) {
							switch (expr[i]) {
								case '!': // 与以下表达方式不匹配
								{
									StringBuilder whatmatch = new StringBuilder();
									++i;
									while (i < expr.Length && expr[i] != '@') {
										whatmatch.Append(expr[i++]);
									}
									if (this.Offset + index + j + whatmatch.Length < document.TextLength) {
										int k = 0;
										for (; k < whatmatch.Length; ++k) {
											if (document.GetCharAt(this.Offset + index + j + k) != whatmatch[k]) {
												break;
											}
										}
										if (k >= whatmatch.Length) {
											return false;
										}
									}
//									--j;
									break;
								}
								case '-': // 与之前的表达方式不匹配
								{
									StringBuilder whatmatch = new StringBuilder();
									++i;
									while (i < expr.Length && expr[i] != '@') {
										whatmatch.Append(expr[i++]);
									}
									if (index - whatmatch.Length >= 0) {
										int k = 0;
										for (; k < whatmatch.Length; ++k)
											if (document.GetCharAt(this.Offset + index - whatmatch.Length + k) != whatmatch[k])
												break;
										if (k >= whatmatch.Length) {
											return false;
										}
									}
//									--j;
									break;
								}
								case '@': // matches @
									if (index + j >= this.Length || '@' != document.GetCharAt(this.Offset + index + j)) {
										return false;
									}
									break;
							}
						}
						break;
					default:
						if (index + j >= this.Length || expr[i] != document.GetCharAt(this.Offset + index + j)) {
							return false;
						}
						break;
				}
			}
			return true;
		}
	}
}
