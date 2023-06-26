// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Drawing;

using MeltuiCodeEditor.Undo;

namespace MeltuiCodeEditor.Document
{
    /// <summary>
    /// 文档接口
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// 编辑框属性
        /// </summary>
        ITextEditorProperties TextEditorProperties
        {
            get;
            set;
        }

        /// <summary>
        /// 撤销、重做栈
        /// </summary>
        UndoStack UndoStack
        {
            get;
        }

        /// <value>
        /// 将文档设为只读
        /// </value>
        bool ReadOnly
        {
            get;
            set;
        }

        /// <summary>
        /// 文档格式化策略
        /// </summary>
        IFormattingStrategy FormattingStrategy
        {
            get;
            set;
        }

        /// <summary>
        /// 文本缓冲策略
        /// </summary>
        ITextBufferStrategy TextBufferStrategy
        {
            get;
        }

        /// <summary>
        /// 折叠管理器
        /// </summary>
        FoldingManager FoldingManager
        {
            get;
        }

        /// <summary>
        /// 编辑框样式
        /// </summary>
        IStyleStrategy HighlightStyle
        {
            get;
            set;
        }

        /// <summary>
        /// 书签管理器
        /// </summary>
        BookmarkManager BookmarkManager
        {
            get;
        }

        /// <summary>
        /// 自定义行管理
        /// </summary>
        ICustomLineManager CustomLineManager
        {
            get;
        }

        /// <summary>
        /// 标记管理器
        /// </summary>
        MarkerStrategy MarkerStrategy
        {
            get;
        }

        //		/// <summary>
        //		/// The <see cref="SelectionManager"/> attached to the <see cref="IDocument"/> instance
        //		/// </summary>
        //		SelectionManager SelectionManager {
        //			get;
        //		}

        #region ILineManager 行管理接口
        /// <summary>
        /// 所有行段的集合
        /// </summary>
        List<LineSegment> LineSegmentCollection
        {
            get;
        }

        /// <value>
        /// 行的总数，这可能是！=阵列列表。
        /// 如果最后一行以分界线结束。
        /// </value>
        int TotalNumberOfLines
        {
            get;
        }

        /// <remarks>
        /// 返回给定偏移的有效行号。
        /// </remarks>
        /// <param name="offset">
        /// 指向行中的字符的偏移
        ///已返回行号。
        /// </param>
        /// <returns>
        /// 值是行号的 int。
        /// </returns>
        /// <exception cref="System.ArgumentException">如果偏移指向无效位置</exception>
        int GetLineNumberForOffset(int offset);

        /// <summary>
        /// 偏移位置转为列号
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        int GetColumnNumberForOffset(int offset);

        /// <summary>
        /// 获取指定偏移的行段落
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        LineSegment GetLineSegmentForOffset(int offset);

        /// <summary>
        /// 获取指定行号的行段落
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        LineSegment GetLineSegment(int lineNumber);

        /// <remarks>
        /// 获取给定可见线的第一条逻辑线。
        /// 示例：行数==100个折叠位于线跟踪器中
        /// 0.1（2折叠，不可见线）之间此方法返回102
        /// "逻辑"行号
        /// </remarks>
        int GetFirstLogicalLine(int lineNumber);

        /// <remarks>
        /// 获取给定可见线的最后一条逻辑线。
        /// 示例：行数==100个折叠位于线跟踪器中
        /// 0.1（2折叠，不可见线）之间此方法返回102
        /// "逻辑"行号
        /// </remarks>
        int GetLastLogicalLine(int lineNumber);

        /// <remarks>
        /// 获取给定逻辑行的可见行。
        /// 示例：行数==100个折叠位于线跟踪器中
        /// 0.1（2折叠，不可见行）之间此方法返回98
        /// "可见"行号
        /// </remarks>
        int GetVisibleLine(int lineNumber);

        //		/// <remarks>
        //		/// 获取给定的逻辑行和逻辑列的可见列。
        //		/// </remarks>
        //		int GetVisibleColumn(int logicalLine, int logicalColumn);

        /// <remarks>
        /// 获取行号后的下一条可见行
        /// </remarks>
        int GetNextVisibleLineAbove(int lineNumber, int lineCount);

        /// <remarks>
        /// 获取行号下的下一条可见行
        /// </remarks>
        int GetNextVisibleLineBelow(int lineNumber, int lineCount);
        #endregion ILineManager 行管理接口

        #region ITextBufferStrategy 文本缓冲策略接口
        /// <value>
        /// 文本内容
        /// </value>
        string TextContent
        {
            get;
            set;
        }

        /// <value>
        ///可以编辑的字符序列的当前长度。
        /// </value>
        int TextLength
        {
            get;
        }

        /// <summary>
        /// 将一串字符插入序列。
        /// </summary>
        /// <param name="offset">
        /// 偏移到插入字符串的位置。
        /// </param>
        /// <param name="text">
        /// 要插入的文本。
        /// </param>
        void Insert(int offset, string text);

        /// <summary>
        /// 删除序列的某些部分。
        /// </summary>
        /// <param name="offset">
        /// 删除的偏移。
        /// </param>
        /// <param name="length">
        /// 要删除的字符数。
        /// </param>
        void Remove(int offset, int length);

        /// <summary>
        /// 替换序列的某些部分。
        /// </summary>
        /// <param name="offset">
        /// 偏移
        /// </param>
        /// <param name="length">
        /// 要替换的字符数
        /// </param>
        /// <param name="text">
        /// 要替换的文本。
        /// </param>
        void Replace(int offset, int length, string text);

        /// <summary>
        /// 返回序列的特定字符。
        /// </summary>
        /// <param name="offset">
        /// 要获取的字符的偏移。
        /// </param>
        char GetCharAt(int offset);

        /// <summary>
        /// 获取序列中包含的字符串。
        /// </summary>
        /// <param name="offset">
        /// 偏移到要取取的序列中
        /// </param>
        /// <param name="length">
        /// 要复制的字符数。
        /// </param>
        string GetText(int offset, int length);
        #endregion  ITextBufferStrategy 文本缓冲策略接口
     
        /// <summary>
        /// 获取自动行段落文本
        /// </summary>
        /// <param name="segment">行段落</param>
        /// <returns></returns>
        string GetText(ISegment segment);

        #region ITextModel interface
        /// <summary>
        /// 从偏移返回逻辑行/列位置
        /// </summary>
        Point OffsetToPosition(int offset);

        /// <summary>
        /// 从位置返回逻辑行/列偏移
        /// </summary>
        int PositionToOffset(Point p);
        #endregion
        /// <value>
        /// 存储所有文本区域更新对象的容器
        /// </value>
        List<TextAreaUpdate> UpdateQueue
        {
            get;
        }

        /// <remarks>
        /// 请求文本区域的更新
        /// </remarks>
        void RequestUpdate(TextAreaUpdate update);

        /// <remarks>
        /// 将队列中的所有更新提交到文本区域（文本区域
        /// 文本区域将被绘制）
        /// </remarks>
        void CommitUpdate();

        /// <summary>
        /// 移动、调整大小、删除插入/删除/替换事件上的段列表。
        /// </summary>
        void UpdateSegmentListOnDocumentChange<T>(List<T> list, DocumentEventArgs e) where T : ISegment;

        /// <summary>
        /// 当提交更新称为"提交更新"时触发
        /// </summary>
        event EventHandler UpdateCommited;

        /// <summary>
        /// 将要改变文档前触发
        /// </summary>
        event DocumentEventHandler DocumentAboutToBeChanged;

        /// <summary>
        /// 文档改变触发
        /// </summary>
        event DocumentEventHandler DocumentChanged;

        /// <summary>
        /// 文本内容改变触发
        /// </summary>
        event EventHandler TextContentChanged;
    }
}
