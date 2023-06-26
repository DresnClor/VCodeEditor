// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System.Collections.Generic;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 行管理接口
	/// </summary>
	public interface ILineManager
	{
		/// <value>
		/// 全部行集合
		/// </value>
		List<LineSegment> LineSegmentCollection {
			get;
		}
		
		/// <value>
		/// 行总数
		/// </value>
		int TotalNumberOfLines {
			get;
		}
		
		/// <value>
		/// 编辑框样式
		/// </value>
		IStyleStrategy HighlightStyle {
			get;
			set;
		}
		
		/// <summary>
		/// 获取给定偏移的有效行号。
		/// </summary>
		/// <param name="offset">指向行中的字符的偏移</param>
		/// <exception cref="System.ArgumentException">如果偏移指向无效位置</exception>
		int GetLineNumberForOffset(int offset);

        /// <summary>
        /// 获取取给定偏移的有效行段落
        /// </summary>
        /// <param name="offset">指向行中的字符的偏移</param>
        /// <exception cref="System.ArgumentException">如果偏移指向无效位置</exception>
        LineSegment GetLineSegmentForOffset(int offset);

        /// <summary>
        /// 获取给定行号的有效行段落</summary>
        /// <param name="lineNumber">行号</param>
        /// <exception cref="System.ArgumentException">如果偏移指向无效位置</exception>
        LineSegment GetLineSegment(int lineNumber);
		
		/// <summary>
		/// 内部使用
		/// 插入文本
		/// </summary>
		void Insert(int offset, string text);

        /// <summary>
        /// 内部使用
		/// 移除文本
        /// </summary>
        void Remove(int offset, int length);

        /// <summary>
        /// 内部使用
		/// 替换指定位置文本
        /// </summary>
        void Replace(int offset, int length, string text);
		
		/// <summary>
		/// 设置行管理器的内容
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
		/// 获取行号后的下一条可见行
		/// </summary>
		int GetNextVisibleLineAbove(int lineNumber, int lineCount);
		
		/// <summary>
		///获取行号下的下一条可见行
		/// </summary>
		int GetNextVisibleLineBelow(int lineNumber, int lineCount);
		
		/// <summary>
		/// 插入或删除行时被激发
		/// </summary>
		event LineManagerEventHandler LineCountChanged;
		
		/// <summary>
		/// 行长度被改变时触发
		/// </summary>
		event LineLengthEventHandler LineLengthChanged;
	}
}
