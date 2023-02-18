// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Text;
using System.Diagnostics;

using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 文本辅助类
	/// </summary>
	public sealed class TextUtilities
	{
		/// <remarks>
		/// 此功能需要一个字符串并转换前面的空白
		/// 它的标签。如果字符串开头的白空间长度
		/// 不是一个完整的标签，然后仍然会有一些空间只是
		/// 在文本开始之前。
		/// 输出字符串将的形式：
		/// 1.零或更多选项卡
		/// 2.零或更多空间（小于选项卡）
		/// 3.行的其余部分
		/// </remarks>
		public static string LeadingWhiteSpaceToTabs(string line, int tabIndent) {
			StringBuilder sb = new StringBuilder(line.Length);
			int consecutiveSpaces = 0;
			int i = 0;
			for(i = 0; i < line.Length; i++) {
				if(line[i] == ' ') {
					consecutiveSpaces++;
					if(consecutiveSpaces == tabIndent) {
						sb.Append('\t');
						consecutiveSpaces = 0;
					}
				}
				else if(line[i] == '\t') {
					sb.Append('\t');
					//如果我们说3个空格，然后一个选项卡和选项卡凹痕是4，然后
					//我们想简单地用1个选项卡替换所有这些
					
					consecutiveSpaces = 0;
				}
				else {
					break;
				}
			}
			
			if(i < line.Length) {
				sb.Append(line.Substring(i-consecutiveSpaces));
			}
			return sb.ToString();
		}

		/// <summary>
		/// 是否为字母数字或下划线
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool IsLetterDigitOrUnderscore(char c)
		{
			if(!Char.IsLetterOrDigit(c)) {
				return c == '_';
			}
			return true;
		}
		
		/// <summary>
		/// 字符类型
		/// </summary>
		public enum CharacterType {
			/// <summary>
			/// 字母数字或下划线
			/// </summary>
			LetterDigitOrUnderscore,
			/// <summary>
			/// 空白字符
			/// </summary>
			WhiteSpace,
			/// <summary>
			/// 其它
			/// </summary>
			Other
		}
		
		/// <remarks>
		/// 此方法在指定的偏移之前返回表达式。
		/// 该方法用于代码完成以确定给出的表达式
		/// 到解析器的类型解析。		
		/// </remarks>
		public static string GetExpressionBeforeOffset(TextArea textArea, int initialOffset)
		{
			IDocument document = textArea.Document;
			int offset = initialOffset;
			while (offset - 1 > 0) {
				switch (document.GetCharAt(offset - 1)) {
					case '\n':
					case '\r':
					case '}':
						goto done;
//						offset = SearchBracketBackward(document, offset - 2, '{','}');
//						break;
					case ']':
						offset = SearchBracketBackward(document, offset - 2, '[',']');
						break;
					case ')':
						offset = SearchBracketBackward(document, offset - 2, '(',')');
						break;
					case '.':
						--offset;
						break;
					case '"':
						if (offset < initialOffset - 1) {
							return null;
						}
						return "\"\"";
					case '\'':
						if (offset < initialOffset - 1) {
							return null;
						}
						return "'a'";
					case '>':
						if (document.GetCharAt(offset - 2) == '-') {
							offset -= 2;
							break;
						}
						goto done;
					default:
						if (Char.IsWhiteSpace(document.GetCharAt(offset - 1))) {
							--offset;
							break;
						}
						int start = offset - 1;
						if (!IsLetterDigitOrUnderscore(document.GetCharAt(start))) {
							goto done;
						}
						
						while (start > 0 && IsLetterDigitOrUnderscore(document.GetCharAt(start - 1))) {
							--start;
						}
						string word = document.GetText(start, offset - start).Trim();
						switch (word) {
							case "ref":
							case "out":
							case "in":
							case "return":
							case "throw":
							case "case":
								goto done;
						}
						
						if (word.Length > 0 && !IsLetterDigitOrUnderscore(word[0])) {
							goto done;
						}
						offset = start;
						break;
				}
			}
		done:
			//// 简单的退出失败时：是内部评论行或任何其他字符
			//// 我们必须检查，如果我们有几个ID在由此产生的行，这通常发生在
			//// id.在评论一后键入下一行
			//// 如果莱克瑟能正确解析这样的表达方式，那就更好了。然而，这将导致
			//// 在这方面的修改太 - 获得完整的评论行，并删除它之后

			string resText =document.GetText(offset, textArea.Caret.Offset - offset ).Trim();
			int pos=resText.LastIndexOf('\n');
			if (pos>=0) {
				offset+=pos+1;
				//// 空白空间和选项卡，这可能是里面，将跳过修剪下面
			}
			string expression = document.GetText(offset, textArea.Caret.Offset - offset ).Trim();
			return expression;
		}
		
		
		/// <summary>
		/// 获取字符类型
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static CharacterType GetCharacterType(char c)
		{
			if(IsLetterDigitOrUnderscore(c))
				return CharacterType.LetterDigitOrUnderscore;
			if(Char.IsWhiteSpace(c))
				return CharacterType.WhiteSpace;
			return CharacterType.Other;
		}
		
		/// <summary>
		/// /获取第一个宽字符串字符
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static int GetFirstNonWSChar(IDocument document, int offset)
		{
			while (offset < document.TextLength && Char.IsWhiteSpace(document.GetCharAt(offset))) {
				++offset;
			}
			return offset;
		}
		
		/// <summary>
		/// 查找单词结束
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static int FindWordEnd(IDocument document, int offset)
		{
			LineSegment line   = document.GetLineSegmentForOffset(offset);
			int     endPos = line.Offset + line.Length;
			while (offset < endPos && IsLetterDigitOrUnderscore(document.GetCharAt(offset))) {
				++offset;
			}
			
			return offset;
		}

		/// <summary>
		/// 查找单词开始
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static int FindWordStart(IDocument document, int offset)
		{
			LineSegment line = document.GetLineSegmentForOffset(offset);
			
			while (offset > line.Offset && !IsLetterDigitOrUnderscore(document.GetCharAt(offset - 1))) {
				--offset;
			}
			
			return offset;
		}

		/// <summary>
		/// 前进到下一个字的开头
		/// 如果光标是在单词的开头或中间，我们移动到单词的末尾
		/// 然后过去任何白色的空间，遵循它
		/// 如果光标是在开始或在一些空白的中间，我们移动到开始
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// 下一个字<returns></returns>
		public static int FindNextWordStart(IDocument document, int offset)
		{
			int originalOffset = offset;
			LineSegment line   = document.GetLineSegmentForOffset(offset);
			int     endPos = line.Offset + line.Length;
			// 让我们去这个词的结尾，空白或运营商
			CharacterType t = GetCharacterType(document.GetCharAt(offset));
			while (offset < endPos && GetCharacterType(document.GetCharAt(offset)) == t) {
				++offset;
			}
			
			// 现在我们在单词的末尾，让我们通过跳过白空间找到下一个的开始
			while (offset < endPos && GetCharacterType(document.GetCharAt(offset)) == CharacterType.WhiteSpace) {
				++offset;
			}

			return offset;
		}

		/// <summary>
		/// 回到我们所在单词的开头
		/// 如果我们已经在一个字的开头，或者如果我们是在白色的空间，然后回去
		/// 到前一个单词的开头
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static int FindPrevWordStart(IDocument document, int offset)
		{
			int originalOffset = offset;
			if (offset > 0) {
				LineSegment line = document.GetLineSegmentForOffset(offset);
				CharacterType t = GetCharacterType(document.GetCharAt(offset - 1));
				while (offset > line.Offset && GetCharacterType(document.GetCharAt(offset - 1)) == t) {
					--offset;
				}
				
				// 如果在空白，或者在末尾，回到开始
				if(t == CharacterType.WhiteSpace && offset > line.Offset) {
					t = GetCharacterType(document.GetCharAt(offset - 1));
					while (offset > line.Offset && GetCharacterType(document.GetCharAt(offset - 1)) == t) {
						--offset;
					}
				}
			}
			
			return offset;
		}
		
		/// <summary>
		/// 获取指定行字符串
		/// </summary>
		/// <param name="document"></param>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public static string GetLineAsString(IDocument document, int lineNumber)
		{
			LineSegment line = document.GetLineSegment(lineNumber);
			return document.GetText(line.Offset, line.Length);
		}
		
		/// <summary>
		/// 查找下一个括号
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <param name="openBracket"></param>
		/// <param name="closingBracket"></param>
		/// <returns></returns>
		public static int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			return document.FormattingStrategy.SearchBracketBackward(document, offset, openBracket, closingBracket);
		}
		
		/// <summary>
		/// 查找上一个括号
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <param name="openBracket"></param>
		/// <param name="closingBracket"></param>
		/// <returns></returns>
		public static int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket)
		{
			return document.FormattingStrategy.SearchBracketForward(document, offset, openBracket, closingBracket);
		}
		
		/// <remarks>
		/// 是否为空白行
		/// </remarks>
		public static bool IsEmptyLine(IDocument document, int lineNumber)
		{
			return IsEmptyLine(document, document.GetLineSegment(lineNumber));
		}

		/// <remarks>
		/// 返回真，如果线号是空的或充满空白。
		/// </remarks>
		public static bool IsEmptyLine(IDocument document, LineSegment line)
		{
			for (int i = line.Offset; i < line.Offset + line.Length; ++i) {
				char ch = document.GetCharAt(i);
				if (!Char.IsWhiteSpace(ch)) {
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// 字符是否为单词部分
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		static bool IsWordPart(char ch)
		{
			return IsLetterDigitOrUnderscore(ch) || ch == '.';
		}
		
		/// <summary>
		/// 获取指定偏移 指定长度字符串
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public static string GetWordAt(IDocument document, int offset)
		{
			if (offset < 0 || offset >= document.TextLength - 1 || !IsWordPart(document.GetCharAt(offset))) {
				return String.Empty;
			}
			int startOffset = offset;
			int endOffset   = offset;
			while (startOffset > 0 && IsWordPart(document.GetCharAt(startOffset - 1))) {
				--startOffset;
			}
			
			while (endOffset < document.TextLength - 1 && IsWordPart(document.GetCharAt(endOffset + 1))) {
				++endOffset;
			}
			
			Debug.Assert(endOffset >= startOffset);
			return document.GetText(startOffset, endOffset - startOffset + 1);
		}
	}
}
