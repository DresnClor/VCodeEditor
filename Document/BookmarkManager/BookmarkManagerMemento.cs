// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Xml;

namespace VCodeEditor.Document
{
	/// <summary>
	///  此类用于存储书签管理器的状态
	/// </summary>
	public class BookmarkManagerMemento
	{
		List<int> bookmarks = new List<int>();
		
		/// <value>
		/// 书签列表
		/// </value>
		public List<int> Bookmarks {
			get {
				return bookmarks;
			}
			set {
				bookmarks = value;
			}
		}
		
		/// <summary>
		/// 如果所有书签都在文档范围内，则验证它们。
		/// (removing all bookmarks &lt; 0 and bookmarks &gt; max. line number
		/// </summary>
		public void CheckMemento(IDocument document)
		{
			for (int i = 0; i < bookmarks.Count; ++i) {
				int mark = (int)bookmarks[i];
				if (mark < 0 || mark >= document.TotalNumberOfLines) {
					bookmarks.RemoveAt(i);
					--i;
				}
			}
		}
		
		/// <summary>
		///  创建新实例<see cref="BookmarkManagerMemento"/>
		/// </summary>
		public BookmarkManagerMemento()
		{
		}
		
		/// <summary>
		/// 创建新实例 <see cref="BookmarkManagerMemento"/>
		/// </summary>
		public BookmarkManagerMemento(XmlElement element)
		{
			foreach (XmlElement el in element.ChildNodes) {
				bookmarks.Add(Int32.Parse(el.Attributes["line"].InnerText));
			}
		}
		
		/// <summary>
		/// 创建新实例<see cref="BookmarkManagerMemento"/>
		/// </summary>
		public BookmarkManagerMemento(List<int> bookmarks)
		{
			this.bookmarks = bookmarks;
		}
		
		/// <summary>
		/// 将 xml 元素转换为 <see cref="BookmarkManagerMemento"/> 对象
		/// </summary>
		public object FromXmlElement(XmlElement element)
		{
			return new BookmarkManagerMemento(element);
		}
		
		/// <summary>
		/// 转换 <see cref="BookmarkManagerMemento"/> 为xml元素
		/// </summary>
		public XmlElement ToXmlElement(XmlDocument doc)
		{
			XmlElement bookmarknode  = doc.CreateElement("Bookmarks");
			
			foreach (int line in bookmarks) {
				XmlElement markNode = doc.CreateElement("Mark");
				
				XmlAttribute lineAttr = doc.CreateAttribute("line");
				lineAttr.InnerText = line.ToString();
				markNode.Attributes.Append(lineAttr);
						
				bookmarknode.AppendChild(markNode);
			}
			
			return bookmarknode;
		}
	}
}
