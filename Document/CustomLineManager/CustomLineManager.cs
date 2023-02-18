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
	/// �Զ�����
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
	/// �Զ����й���
	/// </summary>
	public class CustomLineManager : ICustomLineManager
	{
		ArrayList lines = new ArrayList();
		
		/// <summary>
		/// ������ʵ��<see cref="CustomLineManager"/>
		/// </summary>
		public CustomLineManager(ILineManager lineTracker)
		{
			lineTracker.LineCountChanged += new LineManagerEventHandler(MoveIndices);
		}

		/// <value>
		/// �Զ���������
		/// </value>
		public ArrayList CustomLines {
			get {
				return lines;
			}
		}
		
		/// <remarks>
		/// ������ɫ�������<code>lineNr</code> ���Զ���bg��ɫ
		/// ���򷵻� <code>defaultColor</code>
		/// </remarks>
		public Color GetCustomColor(int lineNr, Color defaultColor)
		{
			foreach(CustomLine line in lines)
				if (line.StartLineNr <= lineNr && line.EndLineNr >= lineNr)
					return line.Color;
			return defaultColor;
		}
		
		/// <remarks>
		/// ���ض�ȡֻ������� <code>lineNr</code> ���Զ���
		/// ���򷵻�<code>default</code>
		/// </remarks>
		public bool IsReadOnly(int lineNr, bool defaultReadOnly)
		{
			foreach(CustomLine line in lines)
				if (line.StartLineNr <= lineNr && line.EndLineNr >= lineNr)
					return line.ReadOnly;
			return defaultReadOnly;
		}
		
		/// <remarks>
		/// �����棬��� <code>selection</code> ����ȡ
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
		/// ��������Զ�����
		/// </remarks>
		public void Clear()
		{
			OnBeforeChanged();
			lines.Clear();
			OnChanged();
		}
		
		/// <remarks>
		/// ����
		/// </remarks>
		public event EventHandler BeforeChanged;

		/// <remarks>
		/// ����
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
		/// �����������Զ����� <code>lineNr</code>
		/// </remarks>
		public void AddCustomLine(int lineNr, Color customColor, bool readOnly)
		{
			OnBeforeChanged();
			lines.Add(new CustomLine(lineNr, customColor, readOnly));
			OnChanged();
		}

		/// <remarks>
		/// ����������Զ�����<code>startLineNr</code> to the line <code>endLineNr</code>
		/// </remarks>
		public void AddCustomLine(int startLineNr, int endLineNr, Color customColor, bool readOnly)
		{
			OnBeforeChanged();
			lines.Add(new CustomLine(startLineNr, endLineNr, customColor, readOnly));
			OnChanged();
		}

		/// <remarks>
		/// ������ɾ���Զ����� <code>lineNr</code>
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
		/// �˷���������ָ����ָ�����ϼ������ƶ�
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
