// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Ivo Kovacka" email="ivok@internet.sk"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Drawing;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 自定义行
	/// </summary>
	public class CustomLine
	{
		public int    StartLineNr;
		public int    EndLineNr;
		public Color  Color;
		public bool   ReadOnly;

		public CustomLine(int lineNr, Color customColor, bool readOnly)
		{
			this.StartLineNr = this.EndLineNr = lineNr;
			this.Color  = customColor;
			this.ReadOnly = readOnly;
		}
		
		public CustomLine(int startLineNr, int endLineNr, Color customColor, bool readOnly)
		{
			this.StartLineNr = startLineNr;
			this.EndLineNr = endLineNr;
			this.Color  = customColor;
			this.ReadOnly = readOnly;
		}
	}
		
	/// <summary>
	/// 自定义行管理
	/// </summary>
	public class CustomLineManager : ICustomLineManager
	{
		ArrayList lines = new ArrayList();
		
		/// <summary>
		/// 创建新实例<see cref="CustomLineManager"/>
		/// </summary>
		public CustomLineManager(ILineManager lineTracker)
		{
			lineTracker.LineCountChanged += new LineManagerEventHandler(MoveIndices);
		}

		/// <value>
		/// 自定义行数组
		/// </value>
		public ArrayList CustomLines {
			get {
				return lines;
			}
		}
		
		/// <remarks>
		/// 返回颜色，如果行<code>lineNr</code> 有自定义bg颜色
		/// 否则返回 <code>defaultColor</code>
		/// </remarks>
		public Color GetCustomColor(int lineNr, Color defaultColor)
		{
			foreach(CustomLine line in lines)
				if (line.StartLineNr <= lineNr && line.EndLineNr >= lineNr)
					return line.Color;
			return defaultColor;
		}
		
		/// <remarks>
		/// 返回读取只有如果行 <code>lineNr</code> 是自定义
		/// 否则返回<code>default</code>
		/// </remarks>
		public bool IsReadOnly(int lineNr, bool defaultReadOnly)
		{
			foreach(CustomLine line in lines)
				if (line.StartLineNr <= lineNr && line.EndLineNr >= lineNr)
					return line.ReadOnly;
			return defaultReadOnly;
		}
		
		/// <remarks>
		/// 返回真，如果 <code>selection</code> 仅读取
		/// </remarks>
		public bool IsReadOnly(ISelection selection, bool defaultReadOnly)
		{
			int startLine = selection.StartPosition.Y;
			int endLine = selection.EndPosition.Y;
			foreach (CustomLine customLine in lines) {
				if (customLine.ReadOnly == false)
					continue;
				if (startLine < customLine.StartLineNr && endLine < customLine.StartLineNr)
					continue;
				if (startLine > customLine.EndLineNr && endLine > customLine.EndLineNr)
					continue;
				return true;
			}
			return defaultReadOnly;
		}
		
		/// <remarks>
		/// 清除所有自定义行
		/// </remarks>
		public void Clear()
		{
			OnBeforeChanged();
			lines.Clear();
			OnChanged();
		}
		
		/// <remarks>
		/// 更改
		/// </remarks>
		public event EventHandler BeforeChanged;

		/// <remarks>
		/// 更改
		/// </remarks>
		public event EventHandler Changed;
		
	
		
		void OnChanged() 
		{
			if (Changed != null) {
				Changed(this, null);
			}
		}
		void OnBeforeChanged() 
		{
			if (BeforeChanged != null) {
				BeforeChanged(this, null);
			}
		}
			
		/// <remarks>
		/// 在行中设置自定义行 <code>lineNr</code>
		/// </remarks>
		public void AddCustomLine(int lineNr, Color customColor, bool readOnly)
		{
			OnBeforeChanged();
			lines.Add(new CustomLine(lineNr, customColor, readOnly));
			OnChanged();
		}

		/// <remarks>
		/// 从行中添加自定义行<code>startLineNr</code> to the line <code>endLineNr</code>
		/// </remarks>
		public void AddCustomLine(int startLineNr, int endLineNr, Color customColor, bool readOnly)
		{
			OnBeforeChanged();
			lines.Add(new CustomLine(startLineNr, endLineNr, customColor, readOnly));
			OnChanged();
		}

		/// <remarks>
		/// 在行中删除自定义行 <code>lineNr</code>
		/// </remarks>
		public void RemoveCustomLine(int lineNr)
		{
			for (int i = 0; i < lines.Count; ++i) {
				if (((CustomLine)lines[i]).StartLineNr <= lineNr && ((CustomLine)lines[i]).EndLineNr >= lineNr) {
					OnBeforeChanged();
					lines.RemoveAt(i);
					OnChanged();
					return;
				}
			}
		}
		
		/// <summary>
		/// 此方法将所有指数从指数向上计数行移动
		/// (useful for deletion/insertion of text)
		/// </summary>
		void MoveIndices(object sender,LineManagerEventArgs e)
		{
			bool changed = false;
			OnBeforeChanged();
			for (int i = 0; i < lines.Count; ++i) {
				int startLineNr = ((CustomLine)lines[i]).StartLineNr;
				int endLineNr = ((CustomLine)lines[i]).EndLineNr;
				if (e.LineStart >= startLineNr && e.LineStart < endLineNr) {
					changed = true;
					((CustomLine)lines[i]).EndLineNr += e.LinesMoved;
				} 
				else if (e.LineStart < startLineNr) {
					((CustomLine)lines[i]).StartLineNr += e.LinesMoved;
					((CustomLine)lines[i]).EndLineNr += e.LinesMoved;
				} 
				else {
				}
/*
				if (e.LinesMoved < 0 && lineNr == e.LineStart) {
					lines.RemoveAt(i);
					--i;
					changed = true;
				} else if (lineNr > e.LineStart + 1 || (e.LinesMoved < 0 && lineNr > e.LineStart))  {
					changed = true;
					((CustomLine)lines[i]).StartLineNr += e.LinesMoved;
					((CustomLine)lines[i]).EndLineNr += e.LinesMoved;
				}
*/
			}
			
			if (changed) {
				OnChanged();
			}
		}
		
	}
}
