// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;

using VCodeEditor.Document;

namespace VCodeEditor.Util
{
	/// <summary>
	/// 文本辅助类
	/// </summary>
	public class TextUtility
	{
		/// <summary>
		/// 区域匹配项
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		public static bool RegionMatches(IDocument document, int offset, int length, string word)
		{
			if (length != word.Length || document.TextLength < offset + length) {
				return false;
			}
			
			for (int i = 0; i < length; ++i) {
				if (document.GetCharAt(offset + i) != word[i]) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 区域匹配项
		/// </summary>
		/// <param name="document"></param>
		/// <param name="casesensitive"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		public static bool RegionMatches(IDocument document, bool casesensitive, int offset, int length, string word)
		{
			if (casesensitive) {
				return RegionMatches(document, offset, length, word);
			}
			
			if (length != word.Length || document.TextLength < offset + length) {
				return false;
			}
			
			for (int i = 0; i < length; ++i) {
				if (Char.ToUpper(document.GetCharAt(offset + i)) != Char.ToUpper(word[i])) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 区域匹配项
		/// </summary>
		/// <param name="document"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		public static bool RegionMatches(IDocument document, int offset, int length, char[] word)
		{
			if (length != word.Length || document.TextLength < offset + length) {
				return false;
			}
			
			for (int i = 0; i < length; ++i) {
				if (document.GetCharAt(offset + i) != word[i]) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 区域匹配项
		/// </summary>
		/// <param name="document"></param>
		/// <param name="casesensitive"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		public static bool RegionMatches(IDocument document, bool casesensitive, int offset, int length, char[] word)
		{
			if (casesensitive) {
				return RegionMatches(document, offset, length, word);
			}
			
			if (length != word.Length || document.TextLength < offset + length) {
				return false;
			}
			
			for (int i = 0; i < length; ++i) {
				if (Char.ToUpper(document.GetCharAt(offset + i)) != Char.ToUpper(word[i])) {
					return false;
				}
			}
			return true;
		}
	}
}
