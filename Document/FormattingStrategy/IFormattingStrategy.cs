// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Text;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 文本内容格式化策略
	/// </summary>
	public interface IFormattingStrategy
	{
		/// <summary>
		///  此功能在之后对特定行进行格式化<code>ch</code> 按下。
		/// </summary>
		/// <returns>
		/// 护理三角洲的位置照顾将移动此号码
		///字节（例如，在护理之前插入的字节数，或
		///删除，如果此数字为负数）
		/// </returns>
		int FormatLine(TextArea textArea, int line, int caretOffset, char charTyped);
		
		/// <summary>
		/// 设定指定行的缩进级别
		/// </summary>
		/// <returns>插入字符的数量</returns>
		int IndentLine(TextArea textArea, int line);

        /// <summary>
        /// 设定一系列行的缩进级别
        /// </summary>
        void IndentLines(TextArea textArea, int begin, int end);
		
		/// <summary>
		/// 搜索左括号
		/// </summary>
		/// <param name="document">要搜索的文件</param>
		/// <param name="offset">块中位置的偏移或关闭支架的偏移。</param>
		/// <param name="openBracket">开口支架的字符。</param>
		/// <param name="closingBracket">关闭支架的字符。</param>
		/// <returns>如果没有找到匹配的支架，则返回开口支架的偏移或 -1。</returns>
		int SearchBracketBackward(IDocument document, int offset, char openBracket, char closingBracket);

        /// <summary>
        /// 搜索右括号
        /// </summary>
        /// <param name="document">The document to search in.</param>
        /// <param name="offset">The offset of an position in the block or the offset of the opening bracket.</param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>Returns the offset of the closing bracket or -1 if no matching bracket was found.</returns>
        int SearchBracketForward(IDocument document, int offset, char openBracket, char closingBracket);
	}
}
