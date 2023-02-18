// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 此代表用于文档事件。
	/// </summary>
	public delegate void DocumentEventHandler(object sender, DocumentEventArgs e);
	
	/// <summary>
	/// 此类包含有关文档事件的更多信息
	/// </summary>
	public class DocumentEventArgs : EventArgs
	{
		IDocument document;
		int       offset;
		int       length;
		string    text;
		
		/// <returns>
		/// 文档
		/// </returns>
		public IDocument Document {
			get {
				return document;
			}
		}
		
		/// <returns>
		/// -1如果没有为此事件指定偏移
		/// </returns>
		public int Offset {
			get {
				return offset;
			}
		}
		
		/// <returns>
		/// 空，如果没有为此事件指定文本
		/// </returns>
		public string Text {
			get {
				return text;
			}
		}
		
		/// <returns>
		/// -1如果没有为此事件指定长度
		/// </returns>
		public int Length {
			get {
				return length;
			}
		}
		
		/// <summary>
		/// 创建新实例关闭<see cref="DocumentEventArgs"/>
		/// </summary>
		public DocumentEventArgs(IDocument document) : this(document, -1, -1, null)
		{
		}
		
		/// <summary>
		///  创建新实例关闭<see cref="DocumentEventArgs"/>
		/// </summary>
		public DocumentEventArgs(IDocument document, int offset) : this(document, offset, -1, null)
		{
		}
		
		/// <summary>
		/// Creates a new instance off <see cref="DocumentEventArgs"/>
		/// </summary>
		public DocumentEventArgs(IDocument document, int offset, int length) : this(document, offset, length, null)
		{
		}
		
		/// <summary>
		/// Creates a new instance off <see cref="DocumentEventArgs"/>
		/// </summary>
		public DocumentEventArgs(IDocument document, int offset, int length, string text)
		{
			this.document = document;
			this.offset   = offset;
			this.length   = length;
			this.text     = text;
		}
		public override string ToString()
		{
			return String.Format("[DocumentEventArgs: Document = {0}, Offset = {1}, Text = {2}, Length = {3}]",
			                     Document,
			                     Offset,
			                     Text,
			                     Length);
		}
	}
}
