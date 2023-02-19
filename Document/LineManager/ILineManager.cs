// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System.Collections.Generic;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 行管理接口
	/// </summary>
	public interface ILineManager
	{
		/// <value>
		/// 所有行段的集合
		/// </value>
		List<LineSegment> LineSegmentCollection {
			get;
		}
		
		/// <value>
		/// 行的总数，这可能是！=阵列列表。
		/// 如果最后一行以分界线结束。
		/// </value>
		int TotalNumberOfLines {
			get;
		}
		
		/// <value>
		/// The current <see cref="IStyleStrategy"/>连接到此直线经理
		/// </value>
		IStyleStrategy HighlightingStrategy {
			get;
			set;
		}
		
		/// <summary>
		/// 返回给定偏移的有效行号。
		/// </summary>
		/// <param name="offset">
		/// 指向行中的字符的偏移
		/// 已返回行号。
		/// </param>
		/// <returns>
		/// 值是行号的 int。
		/// </returns>
		/// <exception cref="System.ArgumentException">如果偏移指向无效位置</exception>
		int GetLineNumberForOffset(int offset);
		
		/// <summary>
		/// Returns a <see cref="LineSegment"/> 给定偏移。
		/// </summary>
		/// <param name="offset">
		/// 指向行中的字符的偏移
		/// is returned.
		/// </param>
		/// <returns>
		/// A <see cref="LineSegment"/> object.
		/// </returns>
		/// <exception cref="System.ArgumentException">If offset points not to a valid position</exception>
		LineSegment GetLineSegmentForOffset(int offset);
		
		/// <summary>
		/// Returns a <see cref="LineSegment"/> 给定行号。
		/// 此功能应用于获取行，而不是获取
		/// line using the <see cref="ArrayList"/>.
		/// </summary>
		/// <param name="lineNumber">
		/// 请求的行号。
		/// </param>
		/// <returns>
		/// A <see cref="LineSegment"/> object.
		/// </returns>
		/// <exception cref="System.ArgumentException">If offset points not to a valid position</exception>
		LineSegment GetLineSegment(int lineNumber);
		
		/// <summary>
		/// 在内部使用，不要称自己为"
		/// </summary>
		void Insert(int offset, string text);
		
		/// <summary>
		/// Used internally, do not call yourself.
		/// </summary>
		void Remove(int offset, int length);
		
		/// <summary>
		/// Used internally, do not call yourself.
		/// </summary>
		void Replace(int offset, int length, string text);
		
		/// <summary>
		/// 设置此行管理器的内容=中断文本 成行。
		/// </summary>
		void SetContent(string text);
		
		/// <summary>
		/// 获取给定可见线的第一条逻辑线。获取给定可见线的第一条逻辑线。
		/// example : 行数==100折叠在线跟踪器中
		/// between 0..1 (2 folded, invisible lines) 此方法返回 102
		/// "逻辑"行号
		/// </summary>
		int GetFirstLogicalLine(int lineNumber);

		/// <summary>
		/// 获取给定可见线的最后一条逻辑线。
		/// example : 行数==100折叠在线跟踪器中
		/// between 0..1 (2 folded, invisible lines) 此方法返回 102
		/// the 'logical' line number
		/// </summary>
		int GetLastLogicalLine(int lineNumber);

		/// <summary>
		/// 获取给定逻辑线的可见线。
		/// example : lineNumber == 100 foldings are in the linetracker
		/// between 0..1 (2 folded, invisible lines) 此方法返回 98
		/// the 'visible' line number
		/// </summary>
		int GetVisibleLine(int lineNumber);
		
//		/// <summary>
//		///获取给定的逻辑行和逻辑列的可见列。
//		/// </summary>
//		int GetVisibleColumn(int logicalLine, int logicalColumn);
		
		/// <summary>
		/// 获取行号后的下一条可见线
		/// </summary>
		int GetNextVisibleLineAbove(int lineNumber, int lineCount);
		
		/// <summary>
		///获取行号下的下一条可见线
		/// </summary>
		int GetNextVisibleLineBelow(int lineNumber, int lineCount);
		
		/// <summary>
		/// 插入或删除线条时被激发
		/// </summary>
		event LineManagerEventHandler LineCountChanged;
		
		event LineLengthEventHandler LineLengthChanged;
	}
}
