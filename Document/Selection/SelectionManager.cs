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
using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 文本内容选择管理器
	/// </summary>
	public class SelectionManager : IDisposable
	{
        /// <summary>
        /// 创建新实例<see cref="SelectionManager"/>
        /// </summary>
        public SelectionManager(IDocument document)
        {
            this.document = document;
            document.DocumentChanged += new DocumentEventHandler(DocumentChanged);
        }

        IDocument document;
		List<ISelection> selectionCollection = new List<ISelection>();
		
		/// <value>
		/// 选择集合
		/// </value>
		public List<ISelection> SelectionCollection {
			get {
				return selectionCollection;
			}
		}
		
		/// <value>
		///  如果<see cref="SelectionCollection"/> 不为空，返回true
		/// </value>
		public bool HasSomethingSelected {
			get {
				return selectionCollection.Count > 0;
			}
		}
		
		/// <value>
		/// 当前选择的文本
		/// </value>
		public string SelectedText {
			get {
				StringBuilder builder = new StringBuilder();
				
//				PriorityQueue queue = new PriorityQueue();
				
				foreach (ISelection s in selectionCollection) {
					builder.Append(s.SelectedText);
//					queue.Insert(-s.Offset, s);
				}
				
//				while (queue.Count > 0) {
//					ISelection s = ((ISelection)queue.Remove());
//					builder.Append(s.SelectedText);
//				}
				
				return builder.ToString();
			}
		}
		
		public void Dispose()
		{
			if (this.document != null) {
				document.DocumentChanged -= new DocumentEventHandler(DocumentChanged);
				this.document = null;
			}
		}
		
		void DocumentChanged(object sender, DocumentEventArgs e)
		{
			if (e.Text == null) {
				Remove(e.Offset, e.Length);
			} else {
				if (e.Length < 0) {
					Insert(e.Offset, e.Text);
				} else {
					Replace(e.Offset, e.Length, e.Text);
				}
			}
		}
		
		/// <remarks>
		/// 清除选择并设置新的选择
		/// 使用给定的<see cref="ISelection"/> object.
		/// </remarks>
		public void SetSelection(ISelection selection)
		{
//			autoClearSelection = false;
			if (selection != null) {
				if (SelectionCollection.Count == 1 &&
				    selection.StartPosition == SelectionCollection[0].StartPosition &&
				    selection.EndPosition == SelectionCollection[0].EndPosition ) {
					return;
				}
				ClearWithoutUpdate();
				selectionCollection.Add(selection);
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, selection.StartPosition.Y, selection.EndPosition.Y));
				document.CommitUpdate();
				OnSelectionChanged(EventArgs.Empty);
			} else {
				ClearSelection();
			}
		}
		
		/// <summary>
		/// 选择指定位置
		/// </summary>
		/// <param name="startPosition"></param>
		/// <param name="endPosition"></param>
		public void SetSelection(Point startPosition, Point endPosition)
		{
			SetSelection(new DefaultSelection(document, startPosition, endPosition));
		}
		
		public bool GreaterEqPos(Point p1, Point p2)
		{
			return p1.Y > p2.Y || p1.Y == p2.Y && p1.X >= p2.X;
		}
		
		/// <summary>
		/// 扩展选择
		/// </summary>
		/// <param name="oldPosition"></param>
		/// <param name="newPosition"></param>
		public void ExtendSelection(Point oldPosition, Point newPosition)
		{
			if (oldPosition == newPosition) {
				return;
			}
			Point min;
			Point max;
			bool  oldIsGreater = GreaterEqPos(oldPosition, newPosition);
			if (oldIsGreater) {
				min = newPosition;
				max = oldPosition;
			} else {
				min = oldPosition;
				max = newPosition;
			}
			if (!HasSomethingSelected) {
				SetSelection(new DefaultSelection(document, min, max));
				return;
			}
			ISelection selection = this.selectionCollection[0];
			bool changed = false;
			if (selection.ContainsPosition(newPosition)) {
				if (oldIsGreater) {
					if (selection.EndPosition != newPosition) {
						selection.EndPosition = newPosition;
						changed = true;
					}
				} else {
					if (selection.StartPosition != newPosition) {
						selection.StartPosition = newPosition;
						changed = true;
					}
				}
			} else {
				if (oldPosition == selection.StartPosition) {
					if (GreaterEqPos(newPosition, selection.EndPosition)) {
						if (selection.StartPosition != selection.EndPosition ||
						    selection.EndPosition   != newPosition) {
							selection.StartPosition = selection.EndPosition;
							selection.EndPosition   = newPosition;
							changed = true;
						}
					} else {
						if (selection.StartPosition != newPosition) {
							selection.StartPosition = newPosition;
							changed = true;
						}
					}
				} else {
					if (GreaterEqPos(selection.StartPosition, newPosition)) {
						if (selection.EndPosition != selection.StartPosition ||
						    selection.StartPosition   != newPosition) {
							changed = true;
						}
						selection.EndPosition     = selection.StartPosition;
						selection.StartPosition   = newPosition;
						changed = true;
					} else {
						if (selection.EndPosition != newPosition) {
							selection.EndPosition = newPosition;
							changed = true;
						}
					}
				}
			}
			
//			if (GreaterEqPos(selection.StartPosition, min) && GreaterEqPos(selection.EndPosition, max)) {
//				if (oldIsGreater) {
//					selection.StartPosition = min;
//				} else {
//					selection.StartPosition = max;
//				}
//			} else {
//				if (oldIsGreater) {
//					selection.EndPosition = min;
//				} else {
//					selection.EndPosition = max;
//				}
//			}
			if (changed) {
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, min.Y, max.Y));
				document.CommitUpdate();
				OnSelectionChanged(EventArgs.Empty);
			}
		}
		void ClearWithoutUpdate()
		{
			while (selectionCollection.Count > 0) {
				ISelection selection = selectionCollection[selectionCollection.Count - 1];
				selectionCollection.RemoveAt(selectionCollection.Count - 1);
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.LinesBetween, selection.StartPosition.Y, selection.EndPosition.Y));
				OnSelectionChanged(EventArgs.Empty);
			}
		}
		/// <remarks>
		/// 清除选择
		/// </remarks>
		public void ClearSelection()
		{
			ClearWithoutUpdate();
			document.CommitUpdate();
		}
		
		/// <remarks>
		/// 移除选择文本
		/// </remarks>
		public void RemoveSelectedText()
		{
			List<int> lines = new List<int>();
			int offset = -1;
			bool oneLine = true;
//			PriorityQueue queue = new PriorityQueue();
			foreach (ISelection s in selectionCollection) {
//				ISelection s = ((ISelection)queue.Remove());
				if (oneLine) {
					int lineBegin = s.StartPosition.Y;
					if (lineBegin != s.EndPosition.Y) {
						oneLine = false;
					} else {
						lines.Add(lineBegin);
					}
				}
				offset = s.Offset;
				document.Remove(s.Offset, s.Length);

//				queue.Insert(-s.Offset, s);
			}
			ClearSelection();
			if (offset >= 0) {
				//             TODO:
//				document.Caret.Offset = offset;
			}
			if (offset != -1) {
				if (oneLine) {
					foreach (int i in lines) {
						document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, i));
					}
				} else {
					document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
				}
				document.CommitUpdate();
			}
		}
		
		
		bool SelectionsOverlap(ISelection s1, ISelection s2)
		{
			return (s1.Offset <= s2.Offset && s2.Offset <= s1.Offset + s1.Length)                         ||
				(s1.Offset <= s2.Offset + s2.Length && s2.Offset + s2.Length <= s1.Offset + s1.Length) ||
				(s1.Offset >= s2.Offset && s1.Offset + s1.Length <= s2.Offset + s2.Length);
		}
		
		/// <remarks>
		/// 获取指定位置是否选择
		/// </remarks>
		public bool IsSelected(int offset)
		{
			return GetSelectionAt(offset) != null;
		}

		/// <remarks>
		/// 获取指定位置的 <see cref="ISelection"/> 对象
		/// </remarks>
		/// <returns>
		/// 如果偏移不指向选择，返回null
		/// </returns>
		public ISelection GetSelectionAt(int offset)
		{
			foreach (ISelection s in selectionCollection) {
				if (s.ContainsOffset(offset)) {
					return s;
				}
			}
			return null;
		}
		
		/// <remarks>
		/// 内部使用，不调用。
		/// </remarks>
		public void Insert(int offset, string text)
		{
//			foreach (ISelection selection in SelectionCollection) {
//				if (selection.Offset > offset) {
//					selection.Offset += text.Length;
//				} else if (selection.Offset + selection.Length > offset) {
//					selection.Length += text.Length;
//				}
//			}
		}

		/// <remarks>
		/// 内部使用，不调用。
		/// </remarks>
		public void Remove(int offset, int length)
		{
//			foreach (ISelection selection in selectionCollection) {
//				if (selection.Offset > offset) {
//					selection.Offset -= length;
//				} else if (selection.Offset + selection.Length > offset) {
//					selection.Length -= length;
//				}
//			}
		}

		/// <remarks>
		/// 内部使用，不调用。
		/// </remarks>
		public void Replace(int offset, int length, string text)
		{
//			foreach (ISelection selection in selectionCollection) {
//				if (selection.Offset > offset) {
//					selection.Offset = selection.Offset - length + text.Length;
//				} else if (selection.Offset + selection.Length > offset) {
//					selection.Length = selection.Length - length + text.Length;
//				}
//			}
		}
		
		/// <summary>
		/// 获取选择行列范围
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public ColumnRange GetSelectionAtLine(int lineNumber)
		{
			foreach (ISelection selection in selectionCollection) {
				int startLine = selection.StartPosition.Y;
				int endLine   = selection.EndPosition.Y;
				if (startLine < lineNumber && lineNumber < endLine) {
					return ColumnRange.WholeColumn;
				}
				
				if (startLine == lineNumber) {
					LineSegment line = document.GetLineSegment(startLine);
					int startColumn = selection.StartPosition.X;
					int endColumn   = endLine == lineNumber ? selection.EndPosition.X : line.Length + 1;
					return new ColumnRange(startColumn, endColumn);
				}
				
				if (endLine == lineNumber) {
					int endColumn   = selection.EndPosition.X;
					return new ColumnRange(0, endColumn);
				}
			}
			
			return ColumnRange.NoColumn;
		}
		
		public void FireSelectionChanged()
		{
			OnSelectionChanged(EventArgs.Empty);
		}
		protected virtual void OnSelectionChanged(EventArgs e)
		{
			if (SelectionChanged != null) {
				SelectionChanged(this, e);
			}
		}
		
		/// <summary>
		/// 选择改变
		/// </summary>
		public event EventHandler SelectionChanged;
	}
}
