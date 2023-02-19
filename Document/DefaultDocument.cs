// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Drawing;

using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 线条查看器样式
    /// </summary>
    public enum LineViewerStyle
    {
        /// <summary>
        /// 将不显示线查看器
        /// </summary>
        None,
        /// <summary>
        /// 全部行
        /// </summary>
        FullRow
    }

    /// <summary>
    /// 缩进样式
    /// </summary>
    public enum IndentStyle
    {
        /// <summary>
        /// 不缩进
        /// </summary>
        None,
        /// <summary>
        /// 继承上一行的缩进
        /// </summary>
        Auto,
        /// <summary>
        /// 上下文缩进
        /// </summary>
        Smart
    }

    /// <summary>
    /// 括号高亮样式
    /// </summary>
    public enum BracketHighlightingStyle
    {

        /// <summary>
        /// 括号不会被突出显示
        /// </summary>
        None,
        /// <summary>
        /// 括号将突出显示，如果插入点是在括号上
        /// </summary>
        OnBracket,
        /// <summary>
        /// 括号将突出显示，如果改变插入点
        /// </summary>
        AfterBracket
    }

    /// <summary>
    ///文档选择模式
    /// </summary>
    public enum DocumentSelectionMode
    {
        /// <summary>
        /// 正常模式
        /// </summary>
        Normal,
        /// <summary>
        /// 多选模式
        /// </summary>
        Additive
    }

    /// <summary>
    /// 默认 <see cref="IDocument"/> 实现
    /// </summary>
    internal class DefaultDocument : IDocument
    {
        bool readOnly = false;

        ILineManager lineTrackingStrategy = null;
        ICustomLineManager customLineManager = null;
        BookmarkManager bookmarkManager = null;
        ITextBufferStrategy textBufferStrategy = null;
        IFormattingStrategy formattingStrategy = null;
        FoldingManager foldingManager = null;
        UndoStack undoStack = new UndoStack();
        ITextEditorProperties textEditorProperties = new DefaultTextEditorProperties();
        MarkerStrategy markerStrategy = null;

        /// <summary>
        /// 标记策略
        /// </summary>
        public MarkerStrategy MarkerStrategy
        {
            get
            {
                return markerStrategy;
            }
            set
            {
                markerStrategy = value;
            }
        }

        /// <summary>
        /// 编辑器属性
        /// </summary>
        public ITextEditorProperties TextEditorProperties
        {
            get
            {
                return textEditorProperties;
            }
            set
            {
                textEditorProperties = value;
            }
        }

        /// <summary>
        /// 撤销栈
        /// </summary>
        public UndoStack UndoStack
        {
            get
            {
                return undoStack;
            }
        }

        /// <summary>
        /// 行段落集合
        /// </summary>
        public List<LineSegment> LineSegmentCollection
        {
            get
            {
                return lineTrackingStrategy.LineSegmentCollection;
            }
        }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                readOnly = value;
            }
        }

        /// <summary>
        /// 行管理器
        /// </summary>
        public ILineManager LineManager
        {
            get
            {
                return lineTrackingStrategy;
            }
            set
            {
                lineTrackingStrategy = value;
            }
        }

        /// <summary>
        /// 文本缓冲策略
        /// </summary>
        public ITextBufferStrategy TextBufferStrategy
        {
            get
            {
                return textBufferStrategy;
            }
            set
            {
                textBufferStrategy = value;
            }
        }

        /// <summary>
        /// 格式化策略
        /// </summary>
        public IFormattingStrategy FormattingStrategy
        {
            get
            {
                return formattingStrategy;
            }
            set
            {
                formattingStrategy = value;
            }
        }

        /// <summary>
        /// 折叠管理
        /// </summary>
        public FoldingManager FoldingManager
        {
            get
            {
                return foldingManager;
            }
            set
            {
                foldingManager = value;
            }
        }

        /// <summary>
        /// 高亮策略
        /// </summary>
        public IStyleStrategy HighlightingStrategy
        {
            get
            {
                return lineTrackingStrategy.HighlightingStrategy;
            }
            set
            {
                lineTrackingStrategy.HighlightingStrategy = value;
            }
        }

        /// <summary>
        /// 文本长度
        /// </summary>
        public int TextLength
        {
            get
            {
                return textBufferStrategy.Length;
            }
        }

        /// <summary>
        /// 书签管理
        /// </summary>
        public BookmarkManager BookmarkManager
        {
            get
            {
                return bookmarkManager;
            }
            set
            {
                bookmarkManager = value;
            }
        }

        /// <summary>
        /// 自定义行管理
        /// </summary>
        public ICustomLineManager CustomLineManager
        {
            get
            {
                return customLineManager;
            }
            set
            {
                customLineManager = value;
            }
        }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string TextContent
        {
            get
            {
                return GetText(0, textBufferStrategy.Length);
            }
            set
            {
                Debug.Assert(textBufferStrategy != null);
                Debug.Assert(lineTrackingStrategy != null);
                OnDocumentAboutToBeChanged(new DocumentEventArgs(this, 0, 0, value));
                textBufferStrategy.SetContent(value);
                lineTrackingStrategy.SetContent(value);

                OnDocumentChanged(new DocumentEventArgs(this, 0, 0, value));
                OnTextContentChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// 在指定偏移插入文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="text"></param>
        public void Insert(int offset, string text)
        {
            if (readOnly)
            {
                return;
            }
            OnDocumentAboutToBeChanged(new DocumentEventArgs(this, offset, -1, text));
            DateTime time = DateTime.Now;
            textBufferStrategy.Insert(offset, text);

            time = DateTime.Now;
            lineTrackingStrategy.Insert(offset, text);

            time = DateTime.Now;

            undoStack.Push(new UndoableInsert(this, offset, text));

            time = DateTime.Now;
            OnDocumentChanged(new DocumentEventArgs(this, offset, -1, text));
        }

        /// <summary>
        /// 在指定偏移移除指定长度文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Remove(int offset, int length)
        {
            if (readOnly)
            {
                return;
            }
            OnDocumentAboutToBeChanged(new DocumentEventArgs(this, offset, length));
            undoStack.Push(new UndoableDelete(this, offset, GetText(offset, length)));

            textBufferStrategy.Remove(offset, length);
            lineTrackingStrategy.Remove(offset, length);

            OnDocumentChanged(new DocumentEventArgs(this, offset, length));
        }

        /// <summary>
        /// 在指定偏移替换指定长度文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="text"></param>
        public void Replace(int offset, int length, string text)
        {
            if (readOnly)
            {
                return;
            }
            OnDocumentAboutToBeChanged(new DocumentEventArgs(this, offset, length, text));
            undoStack.Push(new UndoableReplace(this, offset, GetText(offset, length), text));

            textBufferStrategy.Replace(offset, length, text);
            lineTrackingStrategy.Replace(offset, length, text);

            OnDocumentChanged(new DocumentEventArgs(this, offset, length, text));
        }

        /// <summary>
        /// 获取指定偏移字符
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char GetCharAt(int offset)
        {
            return textBufferStrategy.GetCharAt(offset);
        }

        /// <summary>
        /// 获取指定偏移 指定长度文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetText(int offset, int length)
        {
#if DEBUG
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "length < 0");
#endif
            return textBufferStrategy.GetText(offset, length);
        }

        /// <summary>
        /// 获取段落文本
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public string GetText(ISegment segment)
        {
            return GetText(segment.Offset, segment.Length);
        }

        /// <summary>
        /// 总行数
        /// </summary>
        public int TotalNumberOfLines
        {
            get
            {
                return lineTrackingStrategy.TotalNumberOfLines;
            }
        }

        /// <summary>
        /// 偏移位置转为行号
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int GetLineNumberForOffset(int offset)
        {
            return lineTrackingStrategy.GetLineNumberForOffset(offset);
        }
        /// <summary>
        /// 偏移位置转为列号
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int GetColumnNumberForOffset(int offset)
        {
            LineSegment line = lineTrackingStrategy.GetLineSegmentForOffset(offset);
            return offset - line.Offset;
        }

        /// <summary>
        /// 偏移位置转为行段落
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public LineSegment GetLineSegmentForOffset(int offset)
        {
            return lineTrackingStrategy.GetLineSegmentForOffset(offset);
        }

        /// <summary>
        /// 获取指定行段落
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public LineSegment GetLineSegment(int line)
        {
            return lineTrackingStrategy.GetLineSegment(line);
        }

        /// <summary>
        /// 获取第一个逻辑行
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public int GetFirstLogicalLine(int lineNumber)
        {
            return lineTrackingStrategy.GetFirstLogicalLine(lineNumber);
        }

        /// <summary>
        /// 获取最后一个逻辑行
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public int GetLastLogicalLine(int lineNumber)
        {
            return lineTrackingStrategy.GetLastLogicalLine(lineNumber);
        }

        /// <summary>
        /// 获取可视行
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public int GetVisibleLine(int lineNumber)
        {
            return lineTrackingStrategy.GetVisibleLine(lineNumber);
        }

        //		public int GetVisibleColumn(int logicalLine, int logicalColumn)
        //		{
        //			return lineTrackingStrategy.GetVisibleColumn(logicalLine, logicalColumn);
        //		}
        //		
        public int GetNextVisibleLineAbove(int lineNumber, int lineCount)
        {
            return lineTrackingStrategy.GetNextVisibleLineAbove(lineNumber, lineCount);
        }

        public int GetNextVisibleLineBelow(int lineNumber, int lineCount)
        {
            return lineTrackingStrategy.GetNextVisibleLineBelow(lineNumber, lineCount);
        }

        /// <summary>
        /// 偏移转为点
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Point OffsetToPosition(int offset)
        {
            int lineNr = GetLineNumberForOffset(offset);
            LineSegment line = GetLineSegment(lineNr);
            return new Point(offset - line.Offset, lineNr);
        }

        /// <summary>
        /// 点转为偏移
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int PositionToOffset(Point p)
        {
            if (p.Y >= this.TotalNumberOfLines)
            {
                return 0;
            }
            LineSegment line = GetLineSegment(p.Y);
            return Math.Min(this.TextLength, line.Offset + Math.Min(line.Length, p.X));
        }

        public void UpdateSegmentListOnDocumentChange<T>(List<T> list, DocumentEventArgs e) where T : ISegment
        {
            for (int i = 0; i < list.Count; ++i)
            {
                ISegment fm = list[i];

                if (e.Offset <= fm.Offset && fm.Offset <= e.Offset + e.Length ||
                    e.Offset <= fm.Offset + fm.Length && fm.Offset + fm.Length <= e.Offset + e.Length)
                {
                    list.RemoveAt(i);
                    --i;
                    continue;
                }

                if (fm.Offset <= e.Offset && e.Offset <= fm.Offset + fm.Length)
                {
                    if (e.Text != null)
                    {
                        fm.Length += e.Text.Length;
                    }
                    if (e.Length > 0)
                    {
                        fm.Length -= e.Length;
                    }
                    continue;
                }

                if (fm.Offset >= e.Offset)
                {
                    if (e.Text != null)
                    {
                        fm.Offset += e.Text.Length;
                    }
                    if (e.Length > 0)
                    {
                        fm.Offset -= e.Length;
                    }
                }
            }
        }

        protected void OnDocumentAboutToBeChanged(DocumentEventArgs e)
        {
            if (DocumentAboutToBeChanged != null)
            {
                DocumentAboutToBeChanged(this, e);
            }
        }

        protected void OnDocumentChanged(DocumentEventArgs e)
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(this, e);
            }
        }

        public event DocumentEventHandler DocumentAboutToBeChanged;
        public event DocumentEventHandler DocumentChanged;

        // UPDATE STUFF 更新内容
        List<TextAreaUpdate> updateQueue = new List<TextAreaUpdate>();

        public List<TextAreaUpdate> UpdateQueue
        {
            get
            {
                return updateQueue;
            }
        }

        public void RequestUpdate(TextAreaUpdate update)
        {
            updateQueue.Add(update);
        }

        public void CommitUpdate()
        {
            if (UpdateCommited != null)
            {
                UpdateCommited(this, EventArgs.Empty);
            }
        }

        protected virtual void OnTextContentChanged(EventArgs e)
        {
            if (TextContentChanged != null)
            {
                TextContentChanged(this, e);
            }
        }

        /// <summary>
        /// 更新提交
        /// </summary>
        public event EventHandler UpdateCommited;

        /// <summary>
        /// 文本内容改变
        /// </summary>
        public event EventHandler TextContentChanged;
    }
}
