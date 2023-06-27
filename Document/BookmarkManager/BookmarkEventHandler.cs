// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;

namespace VCodeEditor.Document
{
	public delegate void BookmarkEventHandler(object sender, BookmarkEventArgs e);

	/// <summary>
	/// 书签事件参数
	/// </summary>
	public class BookmarkEventArgs : EventArgs
	{
		Bookmark bookmark;
		
		/// <summary>
		/// 书签
		/// </summary>
		public Bookmark Bookmark {
			get {
				return bookmark;
			}
		}
		
		public BookmarkEventArgs(Bookmark bookmark)
		{
			this.bookmark = bookmark;
		}
	}
}
