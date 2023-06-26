// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;

using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 文本标记管理策略
	/// </summary>
	public class MarkerStrategy
	{
		/// <summary>
		/// 标记列表
		/// </summary>
		List<TextMarker> textMarker = new List<TextMarker>();
		IDocument document;
		
		/// <summary>
		/// 文档
		/// </summary>
		public IDocument Document {
			get {
				return document;
			}
		}
		
		/// <summary>
		/// 文本标记列表
		/// </summary>
		public IEnumerable<TextMarker> TextMarker {
			get {
				return textMarker;
			}
		}
		
		/// <summary>
		/// 添加标记
		/// </summary>
		/// <param name="item"></param>
		public void AddMarker(TextMarker item)
		{
			markersTable.Clear();
			textMarker.Add(item);
		}
		
		/// <summary>
		/// 插入标记
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public void InsertMarker(int index, TextMarker item)
		{
			markersTable.Clear();
			textMarker.Insert(index, item);
		}
		
		/// <summary>
		/// 移除标记
		/// </summary>
		/// <param name="item"></param>
		public void RemoveMarker(TextMarker item)
		{
			markersTable.Clear();
			textMarker.Remove(item);
		}

		/// <summary>
		/// 移除全部标记
		/// </summary>
		/// <param name="match"></param>
		public void RemoveAll(Predicate<TextMarker> match)
		{
			markersTable.Clear();
			textMarker.RemoveAll(match);
		}
		
		public MarkerStrategy(IDocument document)
		{
			this.document = document;
			document.DocumentChanged += new DocumentEventHandler(DocumentChanged);
		}
		
		Dictionary<int, List<TextMarker>> markersTable = new Dictionary<int, List<TextMarker>>();
		
		/// <summary>
		/// 获取指定位置的标记列表
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public List<TextMarker> GetMarkers(int offset)
		{
			if (!markersTable.ContainsKey(offset)) {
				List<TextMarker> markers = new List<TextMarker>();
				for (int i = 0; i < textMarker.Count; ++i) {
					TextMarker marker = (TextMarker)textMarker[i];
					if (marker.Offset <= offset && offset <= marker.Offset + marker.Length) {
						markers.Add(marker);
					}
				}
				markersTable[offset] = markers;
			}
			return markersTable[offset];
		}

		/// <summary>
		/// 获取指定位置 length 长度的标记列表
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public List<TextMarker> GetMarkers(int offset, int length)
		{
			List<TextMarker> markers = new List<TextMarker>();
			for (int i = 0; i < textMarker.Count; ++i) {
				TextMarker marker = (TextMarker)textMarker[i];
				if (marker.Offset <= offset && offset <= marker.Offset + marker.Length ||
				    marker.Offset <= offset + length && offset + length <= marker.Offset + marker.Length ||
				    offset <= marker.Offset && marker.Offset <= offset + length ||
				    offset <= marker.Offset + marker.Length && marker.Offset + marker.Length <= offset + length
				   ) {
					markers.Add(marker);
				}
			}
			return markers;
		}
		
		/// <summary>
		/// 获取指定点的标记列表
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public List<TextMarker> GetMarkers(Point position)
		{
			if (position.Y >= document.TotalNumberOfLines || position.Y < 0) {
				return new List<TextMarker>();
			}
			LineSegment segment = document.GetLineSegment(position.Y);
			return GetMarkers(segment.Offset + position.X);
		}
		
		void DocumentChanged(object sender, DocumentEventArgs e)
		{
			// 重置标记表
			markersTable.Clear();
			document.UpdateSegmentListOnDocumentChange(textMarker, e);
		}
	}
}
