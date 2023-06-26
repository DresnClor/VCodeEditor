// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using MeltuiCodeEditor.Undo;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 文本缓冲策略接口
	/// </summary>
	public interface ITextBufferStrategy
	{
		/// <value>
		/// 可以编辑的字符序列的当前长度。
		/// </value>
		int Length {
			get;
		}
		
		/// <summary>
		/// 将一串字符插入序列。
		/// </summary>
		/// <param name="offset">
		///偏移到插入字符串的位置。
		/// </param>
		/// <param name="text">
		/// 要插入的文本。
		/// </param>
		void Insert(int offset, string text);
		
		/// <summary>
		/// 删除序列的某些部分。
		/// </summary>
		/// <param name="offset">
		/// 删除的偏移。
		/// </param>
		/// <param name="length">
		/// 要删除的字符数。
		/// </param>
		void Remove(int offset, int length);
		
		/// <summary>
		/// 替换序列的某些部分
		/// </summary>
		/// <param name="offset">
		/// offset.
		/// </param>
		/// <param name="length">
		/// 要替换的字符数。
		/// </param>
		/// <param name="text">
		/// 要替换的文本。
		/// </param>
		void Replace(int offset, int length, string text);
		
		/// <summary>
		/// 获取序列中包含的字符串。
		/// </summary>
		/// <param name="offset">
		/// 偏移到要取取的序列中
		/// </param>
		/// <param name="length">
		/// 要复制的字符数。
		/// </param>
		string GetText(int offset, int length);
		
		/// <summary>
		/// 返回序列的特定字符。
		/// </summary>
		/// <param name="offset">
		/// 要获取的字符的偏移。
		/// </param>
		char GetCharAt(int offset);
		
		/// <summary>
		/// 此方法设置存储的内容。
		/// </summary>
		/// <param name="text">
		/// 表示字符序列的字符。
		/// </param>
		void SetContent(string text);
	}
}
