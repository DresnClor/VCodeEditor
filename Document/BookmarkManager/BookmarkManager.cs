// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections.Generic;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 书签工厂
	/// </summary>
	public interface IBookmarkFactory
	{
		/// <summary>
		/// 创建书签
		/// </summary>
		/// <param name="document"></param>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		Bookmark CreateBookmark(IDocument document, int lineNumber);
	}

	/// <summary>
	/// 书签管理器
	/// </summary>
	public class BookmarkManager
	{
		IDocument      document;
		List<Bookmark> bookmark = new List<Bookmark>();
		
		/// <value>
		/// 包含所有书签作为 int 值
		/// </value>
		public List<Bookmark> Marks {
			get {
				return bookmark;
			}
		}
		
		/// <summary>
		/// 文档
		/// </summary>
		public IDocument Document {
			get {
				return document;
			}
		}
		
		/// <summary>
		/// 创建新实例
		/// <see cref="BookmarkManager"/>
		/// </summary>
		public BookmarkManager(IDocument document, ILineManager lineTracker)
		{
			this.document = document;
			lineTracker.LineCountChanged += new LineManagerEventHandler(MoveIndices);
		}
		
		IBookmarkFactory factory;
		
		/// <summary>
		/// 书签工厂
		/// </summary>
		public IBookmarkFactory Factory {
			get {
				return factory;
			}
			set {
				factory = value;
			}
		}

		/// <summary>
		/// 在行中设置标记<code>lineNr</code> 如果它没有设置，
		/// 如果行已标记 这个标记已清除。
		/// 切换书签标记
		/// </summary>
		public void ToggleMarkAt(int lineNr)
		{
			for (int i = 0; i < bookmark.Count; ++i) {
				Bookmark mark = bookmark[i];
				if (mark.LineNumber == lineNr && mark.CanToggle) {
					bookmark.RemoveAt(i);
					OnRemoved(new BookmarkEventArgs(mark));
					OnChanged(EventArgs.Empty);
					return;
				}
			}
			Bookmark newMark;
			if (factory != null)
				newMark = factory.CreateBookmark(document, lineNr);
			else
				newMark = new Bookmark(document, lineNr);
			bookmark.Add(newMark);
			OnAdded(new BookmarkEventArgs(newMark));
			OnChanged(EventArgs.Empty);
		}
		
		/// <summary>
		/// 添加书签标记
		/// </summary>
		/// <param name="mark"></param>
		public void AddMark(Bookmark mark)
		{
			bookmark.Add(mark);
			OnAdded(new BookmarkEventArgs(mark));
			OnChanged(EventArgs.Empty);
		}

		/// <summary>
		/// 移除书签标记
		/// </summary>
		/// <param name="mark"></param>
		public void RemoveMark(Bookmark mark)
		{
			bookmark.Remove(mark);
			OnRemoved(new BookmarkEventArgs(mark));
			OnChanged(EventArgs.Empty);
		}

		/// <summary>
		/// 移除书签标记列表
		/// </summary>
		/// <param name="predicate"></param>
		public void RemoveMarks(Predicate<Bookmark> predicate)
		{
			for (int i = 0; i < bookmark.Count; ++i) {
				Bookmark bm = bookmark[i];
				if (predicate(bm)) {
					bookmark.RemoveAt(i--);
					OnRemoved(new BookmarkEventArgs(bm));
				}
			}
			OnChanged(EventArgs.Empty);
		}

		/// <returns>
		/// 是否存在书签标记
		/// </returns>
		public bool IsMarked(int lineNr)
		{
			for (int i = 0; i < bookmark.Count; ++i) {
				if (bookmark[i].LineNumber == lineNr) {
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// 此方法将所有指数从指数向上计数行移动
		/// (useful for deletion/insertion of text)
		/// </summary>
		void MoveIndices(object sender,LineManagerEventArgs e)
		{
			bool changed = false;
			for (int i = 0; i < bookmark.Count; ++i) {
				Bookmark mark = bookmark[i];
				if (e.LinesMoved < 0 && mark.LineNumber == e.LineStart) {
					bookmark.RemoveAt(i);
					OnRemoved(new BookmarkEventArgs(mark));
					--i;
					changed = true;
				} else if (mark.LineNumber > e.LineStart + 1 || (e.LinesMoved < 0 && mark.LineNumber > e.LineStart))  {
					changed = true;
					int newLine = mark.LineNumber + e.LinesMoved;
					if (newLine >= 0) {
						bookmark[i].LineNumber = newLine;
					} else {
						bookmark.RemoveAt(i);
						OnRemoved(new BookmarkEventArgs(mark));
						--i;
					}
				}
			}
			
			if (changed) {
				OnChanged(EventArgs.Empty);
			}
		}
		
		/// <remarks>
		/// 清除所有书签
		/// </remarks>
		public void Clear()
		{
			foreach (Bookmark mark in bookmark) {
				OnRemoved(new BookmarkEventArgs(mark));
			}
			bookmark.Clear();
			OnChanged(EventArgs.Empty);
		}

		/// <value>
		/// 获取第一个书签标记
		/// </value>
		public Bookmark GetFirstMark(Predicate<Bookmark> predicate)
		{
			if (bookmark.Count < 1) {
				return null;
			}
			Bookmark first = null;
			for (int i = 0; i < bookmark.Count; ++i) {
				if (predicate(bookmark[i]) && bookmark[i].IsEnabled && (first == null || bookmark[i].LineNumber < first.LineNumber)) {
					first = bookmark[i];
				}
			}
			return first;
		}

		/// <value>
		/// 获取最后一个书签标记
		/// </value>
		public Bookmark GetLastMark(Predicate<Bookmark> predicate)
		{
			if (bookmark.Count < 1) {
				return null;
			}
			Bookmark last = null;
			for (int i = 0; i < bookmark.Count; ++i) {
				if (predicate(bookmark[i]) && bookmark[i].IsEnabled && (last == null || bookmark[i].LineNumber > last.LineNumber)) {
					last = bookmark[i];
				}
			}
			return last;
		}
		bool AcceptAnyMarkPredicate(Bookmark mark)
		{
			return true;
		}

		/// <summary>
		/// 获取下一个书签标记
		/// </summary>
		/// <param name="curLineNr"></param>
		/// <returns></returns>
		public Bookmark GetNextMark(int curLineNr)
		{
			return GetNextMark(curLineNr, AcceptAnyMarkPredicate);
		}
		
		/// <remarks>
		/// 返回第一标记高于 <code>lineNr</code>
		/// </remarks>
		/// <returns>
		/// 返回下一个标记>如果它不存在，它返回FirstMark()
		/// </returns>
		public Bookmark GetNextMark(int curLineNr, Predicate<Bookmark> predicate)
		{
			if (bookmark.Count == 0) {
				return null;
			}
			
			Bookmark next = GetFirstMark(predicate);
			foreach (Bookmark mark in bookmark) {
				if (predicate(mark) && mark.IsEnabled && mark.LineNumber > curLineNr) {
					if (mark.LineNumber < next.LineNumber || next.LineNumber <= curLineNr) {
						next = mark;
					}
				}
			}
			return next;
		}

		/// <summary>
		/// 获取上一个书签标记
		/// </summary>
		/// <param name="curLineNr"></param>
		/// <returns></returns>
		public Bookmark GetPrevMark(int curLineNr)
		{
			return GetPrevMark(curLineNr, AcceptAnyMarkPredicate);
		}
		
		/// <remarks>
		/// 返回第一个标记低于<code>lineNr</code>
		/// </remarks>
		/// <returns>
		/// 返回低于cur的下一个标记，如果它不存在，它返回 LastMark()
		/// </returns>
		public Bookmark GetPrevMark(int curLineNr, Predicate<Bookmark> predicate)
		{
			if (bookmark.Count == 0) {
				return null;
			}
			
			Bookmark prev = GetLastMark(predicate);
			
			foreach (Bookmark mark in bookmark) {
				if (predicate(mark) && mark.IsEnabled && mark.LineNumber < curLineNr) {
					if (mark.LineNumber > prev.LineNumber || prev.LineNumber >= curLineNr) {
						prev = mark;
					}
				}
			}
			return prev;
		}
		
		protected virtual void OnChanged(EventArgs e)
		{
			if (Changed != null) {
				Changed(this, e);
			}
		}
		
		
		protected virtual void OnRemoved(BookmarkEventArgs e)
		{
			if (Removed != null) {
				Removed(this, e);
			}
		}
		
		
		protected virtual void OnAdded(BookmarkEventArgs e)
		{
			if (Added != null) {
				Added(this, e);
			}
		}
		
		/// <summary>
		/// 移除书签触发
		/// </summary>
		public event BookmarkEventHandler Removed;

		/// <summary>
		/// 添加书签触发
		/// </summary>
		public event BookmarkEventHandler Added;
		
		/// <summary>
		/// 书签更改事件
		/// </summary>
		public event EventHandler Changed;
	}
}
