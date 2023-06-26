// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 折叠管理器
	/// </summary>
	public class FoldingManager
	{
		List<FoldMarker>    foldMarker      = new List<FoldMarker>();
		IFoldingStrategy    foldingStrategy = null;
		IDocument document;
		
		/// <summary>
		/// 折叠标记列表
		/// </summary>
		public List<FoldMarker> FoldMarker {
			get {
				return foldMarker;
			}
		}
		
		/// <summary>
		/// 折叠策略
		/// </summary>
		public IFoldingStrategy FoldingStrategy {
			get {
				return foldingStrategy;
			}
			set {
				foldingStrategy = value;
			}
		}
		
		public FoldingManager(IDocument document, ILineManager lineTracker)
		{
			this.document = document;
			document.DocumentChanged += new DocumentEventHandler(DocumentChanged);
			
//			lineTracker.LineCountChanged  += new LineManagerEventHandler(LineManagerLineCountChanged);
//			lineTracker.LineLengthChanged += new LineLengthEventHandler(LineManagerLineLengthChanged);
//			foldMarker.Add(new FoldMarker(0, 5, 3, 5));
//			
//			foldMarker.Add(new FoldMarker(5, 5, 10, 3));
//			foldMarker.Add(new FoldMarker(6, 0, 8, 2));
//			
//			FoldMarker fm1 = new FoldMarker(10, 4, 10, 7);
//			FoldMarker fm2 = new FoldMarker(10, 10, 10, 14);
//			
//			fm1.IsFolded = true;
//			fm2.IsFolded = true;
//			
//			foldMarker.Add(fm1);
//			foldMarker.Add(fm2);
//			foldMarker.Sort();
		}
		
		void DocumentChanged(object sender, DocumentEventArgs e)
		{
			int oldCount = foldMarker.Count;
			document.UpdateSegmentListOnDocumentChange(foldMarker, e);
			if (oldCount != foldMarker.Count) {
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			}
		}

		/// <summary>
		/// 从指定位置获取折叠列表
		/// </summary>
		/// <param name="line">行号</param>
		/// <param name="column">列号</param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldingsFromPosition(int line, int column)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				for (int i = 0; i < foldMarker.Count; ++i) {
					FoldMarker fm = foldMarker[i];
					if ((fm.StartLine == line && column > fm.StartColumn && !(fm.EndLine == line && column >= fm.EndColumn)) ||
					    (fm.EndLine == line && column < fm.EndColumn && !(fm.StartLine == line && column <= fm.StartColumn)) ||
					    (line > fm.StartLine && line < fm.EndLine)) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}

		/// <summary>
		/// 从开始位置获取折叠列表
		/// </summary>
		/// <param name="lineNumber">行号</param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldingsWithStart(int lineNumber)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.StartLine == lineNumber) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}

		/// <summary>
		/// 从开始位置获取已折叠的折叠列表
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldedFoldingsWithStart(int lineNumber)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.IsFolded && fm.StartLine == lineNumber) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}

		/// <summary>
		/// 从开始位置获取已折叠的折叠列表 在列之后？
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldedFoldingsWithStartAfterColumn(int lineNumber, int column)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.IsFolded && fm.StartLine == lineNumber && fm.StartColumn > column) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}
		
		/// <summary>
		/// 获取结束位置的折叠列表
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldingsWithEnd(int lineNumber)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.EndLine == lineNumber) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}

		/// <summary>
		/// 从结束位置获取已折叠的折叠列表
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldedFoldingsWithEnd(int lineNumber)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.IsFolded && fm.EndLine == lineNumber) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}
		
		/// <summary>
		/// 获取指定行是否为折叠开始
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public bool IsFoldStart(int lineNumber)
		{
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.StartLine == lineNumber) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// 获取指定行是否为折叠结束
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public bool IsFoldEnd(int lineNumber)
		{
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.EndLine == lineNumber) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// 获取包含行号的折叠
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public List<FoldMarker> GetFoldingsContainsLineNumber(int lineNumber)
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.StartLine < lineNumber && lineNumber < fm.EndLine) {
						foldings.Add(fm);
					}
				}
			}
			return foldings;
		}

		/// <summary>
		/// 指定行是否在折叠之间
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public bool IsBetweenFolding(int lineNumber)
		{
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.StartLine < lineNumber && lineNumber < fm.EndLine) {
						return true;
					}
				}
			}
			return false;
		}
		
		/// <summary>
		/// 指定行是否可视
		/// </summary>
		/// <param name="lineNumber"></param>
		/// <returns></returns>
		public bool IsLineVisible(int lineNumber)
		{
			if (foldMarker != null) {
				foreach (FoldMarker fm in foldMarker) {
					if (fm.IsFolded && fm.StartLine < lineNumber && lineNumber < fm.EndLine) {
						return false;
					}
				}
			}
			return true;
		}
		
		/// <summary>
		/// 获取顶级已折叠的折叠标记列表
		/// </summary>
		/// <returns></returns>
		public List<FoldMarker> GetTopLevelFoldedFoldings()
		{
			List<FoldMarker> foldings = new List<FoldMarker>();
			if (foldMarker != null) {
				Point end = new Point(0, 0);
				foreach (FoldMarker fm in foldMarker) {
					if (fm.IsFolded && (fm.StartLine > end.Y || fm.StartLine == end.Y && fm.StartColumn >= end.X)) {
						foldings.Add(fm);
						end = new Point(fm.EndColumn, fm.EndLine);
					}
				}
			}
			return foldings;
		}
		
		/// <summary>
		/// 更新折叠
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="parseInfo"></param>
		public void UpdateFoldings(string fileName, object parseInfo)
		{
			int oldFoldingsCount = foldMarker.Count;
			lock (this) {
				List<FoldMarker> newFoldings = foldingStrategy.GenerateFoldMarkers(document, fileName, parseInfo);
				if (newFoldings != null && newFoldings.Count != 0) {
					newFoldings.Sort();
					if (foldMarker.Count == newFoldings.Count) {
						for (int i = 0; i < foldMarker.Count; ++i) {
							newFoldings[i].IsFolded = foldMarker[i].IsFolded;
						}
						foldMarker = newFoldings;
					} else {
						for (int i = 0, j = 0; i < foldMarker.Count && j < newFoldings.Count;) {
							int n = newFoldings[j].CompareTo(foldMarker[i]);
							if (n > 0) {
								++i;
							} else {
								if (n == 0) {
									newFoldings[j].IsFolded = foldMarker[i].IsFolded;
								}
								++j;
							}
						}
					}
				}
				if (newFoldings != null) {
					foldMarker = newFoldings;
				} else {
					foldMarker.Clear();
				}
			}
			if (oldFoldingsCount != foldMarker.Count) {
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
				document.CommitUpdate();
			}
		}
		
		/// <summary>
		/// 序列化为字符串
		/// </summary>
		/// <returns></returns>
		public string SerializeToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (FoldMarker marker in this.foldMarker) {
				sb.Append(marker.Offset);sb.Append("\n");
				sb.Append(marker.Length);sb.Append("\n");
				sb.Append(marker.FoldText);sb.Append("\n");
				sb.Append(marker.IsFolded);sb.Append("\n");
			}
			return sb.ToString();
		}
		
		/// <summary>
		/// 反序列化字符串
		/// </summary>
		/// <param name="str"></param>
		public void DeserializeFromString(string str)
		{
			try {
				string[] lines = str.Split('\n');
				for (int i = 0; i < lines.Length && lines[i].Length > 0; i += 4) {
					int    offset = Int32.Parse(lines[i]);
					int    length = Int32.Parse(lines[i + 1]);
					string text   = lines[i + 2];
					bool isFolded = Boolean.Parse(lines[i + 3]);
					bool found    = false;
					foreach (FoldMarker marker in foldMarker) {
						if (marker.Offset == offset && marker.Length == length) {
							marker.IsFolded = isFolded;
							found = true;
							break;
						}
					}
					if (!found) {
						foldMarker.Add(new FoldMarker(document, offset, length, text, isFolded));
					}
				}
				if (lines.Length > 0) {
					NotifyFoldingsChanged(EventArgs.Empty);
				}
			} catch (Exception) {
			}
		}
		/// <summary>
		/// 折叠改变事件
		/// </summary>
		/// <param name="e"></param>
		public void NotifyFoldingsChanged(EventArgs e)
		{
			if (FoldingsChanged != null) {
				FoldingsChanged(this, e);
			}
		}
		
		
		public event EventHandler FoldingsChanged;
	}
}
