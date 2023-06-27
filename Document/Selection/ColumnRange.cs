// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Collections;
using System.Text;
using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 列范围
	/// </summary>
	public class ColumnRange 
	{
        public ColumnRange(int startColumn, int endColumn)
        {
            this.startColumn = startColumn;
            this.endColumn = endColumn;

        }

        public static readonly ColumnRange NoColumn    = new ColumnRange(-2, -2);
		public static readonly ColumnRange WholeColumn = new ColumnRange(-1, -1);
		
		int startColumn;
		int endColumn;
		
		/// <summary>
		/// 开始列号
		/// </summary>
		public int StartColumn {
			get {
				return startColumn;
			}
			set {
				startColumn = value;
			}
		}
		
		/// <summary>
		/// 结束列号
		/// </summary>
		public int EndColumn {
			get {
				return endColumn;
			}
			set {
				endColumn = value;
			}
		}
	
		public override int GetHashCode()
		{
			return startColumn + (endColumn << 16);
		}
		
		public override bool Equals(object obj)
		{
			if (obj is ColumnRange) {
				return ((ColumnRange)obj).startColumn == startColumn &&
				       ((ColumnRange)obj).endColumn == endColumn;
				
			}
			return false;
		}
		
		public override string ToString()
		{
			return String.Format("[ColumnRange: StartColumn={0}, EndColumn={1}]", startColumn, endColumn);
		}
		
	}
}
