﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using VCodeEditor.Actions;
using VCodeEditor.Document;
using VCodeEditor.Gui.CompletionWindow;

namespace VCodeEditor
{
    /// <summary>
    /// 按键事件
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    public delegate bool KeyEventHandler(char ch);

    /// <summary>
    /// 对话框按键处理
    /// </summary>
    /// <param name="keyData"></param>
    /// <returns></returns>
    public delegate bool DialogKeyProcessor(Keys keyData);

    /// <summary>
    /// 文本区域
    /// </summary>
    [ToolboxItem(false)]
    public class TextArea : Control
    {
        public static bool HiddenMouseCursor = false;

        Point virtualTop = new Point(0, 0);
        TextAreaControl motherTextAreaControl;
        TextEditorControl motherTextEditorControl;

        List<BracketHighlightingSheme> bracketshemes = new List<BracketHighlightingSheme>();
        TextAreaClipboardHandler textAreaClipboardHandler;
        bool autoClearSelection = false;

        List<AbstractMargin> leftMargins = new List<AbstractMargin>();

        TextView textView;
        GutterMargin gutterMargin;
        FoldMargin foldMargin;
        IconBarMargin iconBarMargin;

        BreakpointBarMargin breakpointBarMargin;

        SelectionManager selectionManager;
        Caret caret;

        bool disposed;

        [Browsable(false)]
        public IList<AbstractMargin> LeftMargins
        {
            get
            {
                return leftMargins.AsReadOnly();
            }
        }
        /// <summary>
        /// 插入左边边距符号
        /// </summary>
        /// <param name="index"></param>
        /// <param name="margin"></param>
        public void InsertLeftMargin(int index, AbstractMargin margin)
        {
            leftMargins.Insert(index, margin);
            Refresh();
        }

        public TextEditorControl MotherTextEditorControl
        {
            get
            {
                return motherTextEditorControl;
            }
        }

        public TextAreaControl MotherTextAreaControl
        {
            get
            {
                return motherTextAreaControl;
            }
        }

        /// <summary>
        /// 选择管理
        /// </summary>
        public SelectionManager SelectionManager
        {
            get
            {
                return selectionManager;
            }
        }

        /// <summary>
        /// 插入点
        /// </summary>
        public Caret Caret
        {
            get
            {
                return caret;
            }
        }

        /// <summary>
        /// 文本视图
        /// </summary>
        public TextView TextView
        {
            get
            {
                return textView;
            }
        }

        /// <summary>
        /// 边距
        /// </summary>
        public GutterMargin GutterMargin
        {
            get
            {
                return gutterMargin;
            }
        }

        /// <summary>
        /// 折叠边距
        /// </summary>
        public FoldMargin FoldMargin
        {
            get
            {
                return foldMargin;
            }
        }

        /// <summary>
        /// 图标条边距
        /// </summary>
        public IconBarMargin IconBarMargin
        {
            get
            {
                return iconBarMargin;
            }
        }

        /// <summary>
        /// 断点栏
        /// </summary>
        public BreakpointBarMargin BreakpointBarMargin
        {
            get=>this.breakpointBarMargin;
        }


        /// <summary>
        /// 编码
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                return motherTextEditorControl.Encoding;
            }
        }
        /// <summary>
        /// 垂直滚动条最大值
        /// </summary>
        public int MaxVScrollValue
        {
            get
            {
                return (Document.GetVisibleLine(Document.TotalNumberOfLines - 1) + 1 + TextView.VisibleLineCount * 2 / 3) * TextView.FontHeight;
            }
        }
        /// <summary>
        /// 虚拟顶部
        /// </summary>
        public Point VirtualTop
        {
            get
            {
                return virtualTop;
            }
            set
            {
                Point newVirtualTop = new Point(value.X, Math.Min(MaxVScrollValue, Math.Max(0, value.Y)));
                if (virtualTop != newVirtualTop)
                {
                    virtualTop = newVirtualTop;
                    motherTextAreaControl.VScrollBar.Value = virtualTop.Y;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 自动清除选择
        /// </summary>
        public bool AutoClearSelection
        {
            get
            {
                return autoClearSelection;
            }
            set
            {
                autoClearSelection = value;
            }
        }

        [Browsable(false)]
        public IDocument Document
        {
            get
            {
                return motherTextEditorControl.Document;
            }
        }

        public TextAreaClipboardHandler ClipboardHandler
        {
            get
            {
                return textAreaClipboardHandler;
            }
        }


        public ITextEditorProperties TextEditorProperties
        {
            get
            {
                return motherTextEditorControl.Property;
            }
        }

        public TextArea(TextEditorControl motherTextEditorControl, TextAreaControl motherTextAreaControl)
        {
            this.motherTextAreaControl = motherTextAreaControl;
            this.motherTextEditorControl = motherTextEditorControl;

            caret = new Caret(this);
            selectionManager = new SelectionManager(Document);

            this.textAreaClipboardHandler = new TextAreaClipboardHandler(this);

            ResizeRedraw = true;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //			SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);

            textView = new TextView(this);

            gutterMargin = new GutterMargin(this);
            foldMargin = new FoldMargin(this);
            iconBarMargin = new IconBarMargin(this);
            breakpointBarMargin=new BreakpointBarMargin(this);
            leftMargins.AddRange(new AbstractMargin[] { breakpointBarMargin, iconBarMargin, gutterMargin, foldMargin });
            OptionsChanged();


            new TextAreaMouseHandler(this).Attach();
            new TextAreaDragDropHandler().Attach(this);

            bracketshemes.Add(new BracketHighlightingSheme('{', '}'));
            bracketshemes.Add(new BracketHighlightingSheme('(', ')'));
            bracketshemes.Add(new BracketHighlightingSheme('[', ']'));


            caret.PositionChanged += new EventHandler(SearchMatchingBracket);
            Document.TextContentChanged += new EventHandler(TextContentChanged);
            Document.FoldingManager.FoldingsChanged += new EventHandler(DocumentFoldingsChanged);
        }

        /// <summary>
        /// 更新匹配括号
        /// </summary>
        public void UpdateMatchingBracket()
        {
            SearchMatchingBracket(null, null);
        }

        void TextContentChanged(object sender, EventArgs e)
        {
            Caret.Position = new Point(0, 0);
            SelectionManager.SelectionCollection.Clear();
        }
        /// <summary>
        /// /搜索匹配括号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SearchMatchingBracket(object sender, EventArgs e)
        {
            if (!TextEditorProperties.ShowMatchingBracket)
            {
                textView.Highlight = null;
                return;
            }
            bool changed = false;
            if (caret.Offset == 0)
            {
                if (textView.Highlight != null)
                {
                    int line = textView.Highlight.OpenBrace.Y;
                    int line2 = textView.Highlight.CloseBrace.Y;
                    textView.Highlight = null;
                    UpdateLine(line);
                    UpdateLine(line2);
                }
                return;
            }
            foreach (BracketHighlightingSheme bracketsheme in bracketshemes)
            {
                //				if (bracketsheme.IsInside(textareapainter.Document, textareapainter.Document.Caret.Offset)) {
                HighlightBracket highlight = bracketsheme.GetHighlight(Document, Caret.Offset - 1);
                if (textView.Highlight != null && textView.Highlight.OpenBrace.Y >= 0 && textView.Highlight.OpenBrace.Y < Document.TotalNumberOfLines)
                {
                    UpdateLine(textView.Highlight.OpenBrace.Y);
                }
                if (textView.Highlight != null && textView.Highlight.CloseBrace.Y >= 0 && textView.Highlight.CloseBrace.Y < Document.TotalNumberOfLines)
                {
                    UpdateLine(textView.Highlight.CloseBrace.Y);
                }
                textView.Highlight = highlight;
                if (highlight != null)
                {
                    changed = true;
                    break;
                }
                //				}
            }
            if (changed || textView.Highlight != null)
            {
                int line = textView.Highlight.OpenBrace.Y;
                int line2 = textView.Highlight.CloseBrace.Y;
                if (!changed)
                {
                    textView.Highlight = null;
                }
                UpdateLine(line);
                UpdateLine(line2);
            }
        }

        public void SetDesiredColumn()
        {
            Caret.DesiredColumn = TextView.GetDrawingXPos(Caret.Line, Caret.Column) + (int)(VirtualTop.X * textView.WideSpaceWidth);
        }

        public void SetCaretToDesiredColumn(int caretLine)
        {
            Caret.Position = textView.GetLogicalColumn(Caret.Line, Caret.DesiredColumn + (int)(VirtualTop.X * textView.WideSpaceWidth));
        }

        /// <summary>
        /// /选项改变
        /// </summary>
        public void OptionsChanged()
        {
            UpdateMatchingBracket();
            textView.OptionsChanged();
            caret.RecreateCaret();
            caret.UpdateCaretPosition();
            Refresh();
        }

        AbstractMargin lastMouseInMargin;

        //鼠标离开
        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            this.Cursor = Cursors.Default;
            if (lastMouseInMargin != null)
            {
                lastMouseInMargin.HandleMouseLeave(EventArgs.Empty);
                lastMouseInMargin = null;
            }
            CloseToolTip();
        }

        //鼠标按下
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            CloseToolTip();

            foreach (AbstractMargin margin in leftMargins)
            {
                if (margin.DrawingPosition.Contains(e.X, e.Y))
                {
                    margin.HandleMouseDown(new Point(e.X, e.Y), e.Button);
                }
            }
        }


        //静态，因为鼠标只能在一个文本区域，我们不希望有
        //从不活跃的标签上漂浮的文本区域的工具尖。
        static DeclarationViewWindow toolTip;
        static string oldToolTip;

        /// <summary>
        /// 设置工具提示
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lineNumber"></param>
        void SetToolTip(string text, int lineNumber)
        {
            if (toolTip == null || toolTip.IsDisposed)
                toolTip = new DeclarationViewWindow(this.FindForm());
            if (oldToolTip == text)
                return;
            if (text == null)
            {
                toolTip.Hide();
            }
            else
            {
                Point p = Control.MousePosition;
                Point cp = PointToClient(p);
                if (lineNumber >= 0)
                {
                    lineNumber = this.Document.GetVisibleLine(lineNumber);
                    p.Y = (p.Y - cp.Y) + (lineNumber * this.TextView.FontHeight) - this.virtualTop.Y;
                }
                p.Offset(3, 3);
                toolTip.Location = p;
                toolTip.Description = text;
                toolTip.HideOnClick = true;
                toolTip.Show();
            }
            oldToolTip = text;
        }

        /// <summary>
        /// 工具提示请求事件
        /// </summary>
        public event ToolTipRequestEventHandler ToolTipRequest;

        protected virtual void OnToolTipRequest(ToolTipRequestEventArgs e)
        {
            if (ToolTipRequest != null)
            {
                ToolTipRequest(this, e);
            }
        }

        bool toolTipActive;
        /// <summary>
        /// 导致当前工具提示的文本区域中的矩形。
        ///防止 Tooltip 因单击或键盘而关闭时重新显示
        ///输入和鼠标未使用。
        /// </summary>
        Rectangle toolTipRectangle;

        void CloseToolTip()
        {
            if (toolTipActive)
            {
                //Console.WriteLine("Closing tooltip");
                toolTipActive = false;
                SetToolTip(null, -1);
            }
            ResetMouseEventArgs();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            //Console.WriteLine("Hover raised at " + PointToClient(Control.MousePosition));
            if (MouseButtons == MouseButtons.None)
            {
                RequestToolTip(PointToClient(Control.MousePosition));
            }
            else
            {
                CloseToolTip();
            }
        }

        protected void RequestToolTip(Point mousePos)
        {
            if (toolTipRectangle.Contains(mousePos))
            {
                if (!toolTipActive)
                    ResetMouseEventArgs();
                return;
            }

            //Console.WriteLine("Request tooltip for " + mousePos);

            toolTipRectangle = new Rectangle(mousePos.X - 4, mousePos.Y - 4, 8, 8);

            Point logicPos = textView.GetLogicalPosition(mousePos.X - textView.DrawingPosition.Left,
                                                         mousePos.Y - textView.DrawingPosition.Top);
            bool inDocument = textView.DrawingPosition.Contains(mousePos)
                && logicPos.Y >= 0 && logicPos.Y < Document.TotalNumberOfLines;
            ToolTipRequestEventArgs args = new ToolTipRequestEventArgs(mousePos, logicPos, inDocument);
            OnToolTipRequest(args);
            if (args.ToolTipShown)
            {
                //Console.WriteLine("Set tooltip to " + args.toolTipText);
                toolTipActive = true;
                SetToolTip(args.toolTipText, inDocument ? logicPos.Y + 1 : -1);
            }
            else
            {
                CloseToolTip();
            }
        }

        //鼠标移动
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!toolTipRectangle.Contains(e.Location))
            {
                toolTipRectangle = Rectangle.Empty;
                if (toolTipActive)
                    RequestToolTip(e.Location);
            }
            foreach (AbstractMargin margin in leftMargins)
            {
                if (margin.DrawingPosition.Contains(e.X, e.Y))
                {
                    this.Cursor = margin.Cursor;
                    margin.HandleMouseMove(new Point(e.X, e.Y), e.Button);
                    if (lastMouseInMargin != margin)
                    {
                        if (lastMouseInMargin != null)
                        {
                            lastMouseInMargin.HandleMouseLeave(EventArgs.Empty);
                        }
                        lastMouseInMargin = margin;
                    }
                    return;
                }
            }
            if (lastMouseInMargin != null)
            {
                lastMouseInMargin.HandleMouseLeave(EventArgs.Empty);
                lastMouseInMargin = null;
            }
            if (textView.DrawingPosition.Contains(e.X, e.Y))
            {
                this.Cursor = textView.Cursor;
                return;
            }
            this.Cursor = Cursors.Default;
        }
        AbstractMargin updateMargin = null;

        //刷新
        public void Refresh(AbstractMargin margin)
        {
            updateMargin = margin;
            Invalidate(updateMargin.DrawingPosition);
            Update();
            updateMargin = null;
        }
        //重绘背景
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        {
        }

        //重绘
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            int currentXPos = 0;
            int currentYPos = 0;
            bool adjustScrollBars = false;
            Graphics g = e.Graphics;
            Rectangle clipRectangle = e.ClipRectangle;


            if (updateMargin != null)
            {
                updateMargin.Paint(g, updateMargin.DrawingPosition);
                //				clipRectangle.Intersect(updateMargin.DrawingPosition);
            }

            if (clipRectangle.Width <= 0 || clipRectangle.Height <= 0)
            {
                return;
            }

            if (this.TextEditorProperties.UseAntiAliasedFont)
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }
            else
            {
                g.TextRenderingHint = TextRenderingHint.SystemDefault;
            }

            foreach (AbstractMargin margin in leftMargins)
            {
                if (margin.IsVisible)
                {
                    Rectangle marginRectangle = new Rectangle(currentXPos, currentYPos, margin.Size.Width, Height - currentYPos);
                    if (marginRectangle != margin.DrawingPosition)
                    {
                        adjustScrollBars = true;
                        margin.DrawingPosition = marginRectangle;
                    }
                    currentXPos += margin.DrawingPosition.Width;
                    if (clipRectangle.IntersectsWith(marginRectangle))
                    {
                        marginRectangle.Intersect(clipRectangle);
                        if (!marginRectangle.IsEmpty)
                        {
                            margin.Paint(g, marginRectangle);
                        }
                    }
                }
            }

            Rectangle textViewArea = new Rectangle(currentXPos, currentYPos, Width - currentXPos, Height - currentYPos);
            if (textViewArea != textView.DrawingPosition)
            {
                adjustScrollBars = true;
                textView.DrawingPosition = textViewArea;
            }
            if (clipRectangle.IntersectsWith(textViewArea))
            {
                textViewArea.Intersect(clipRectangle);
                if (!textViewArea.IsEmpty)
                {
                    textView.Paint(g, textViewArea);
                }
            }

            if (adjustScrollBars)
            {
                this.motherTextAreaControl.AdjustScrollBars(null, null);
            }

            Caret.UpdateCaretPosition();

            base.OnPaint(e);
        }
        void DocumentFoldingsChanged(object sender, EventArgs e)
        {
            Invalidate();
            this.motherTextAreaControl.AdjustScrollBars(null, null);
        }

        #region 键盘处理方法

        /// <summary>
        /// 此方法在每个按键上调用
        /// </summary>
        /// <returns>
        /// 诚然，如果key是由这种方法处理，不应该插入文本区域。
        /// </returns>
        protected internal virtual bool HandleKeyPress(char ch)
        {
            if (KeyEventHandler != null)
            {
                return KeyEventHandler(ch);
            }
            return false;
        }

        public void SimulateKeyPress(char ch)
        {
            if (Document.ReadOnly)
            {
                return;
            }

            if (TextEditorProperties.UseCustomLine == true)
            {
                if (SelectionManager.HasSomethingSelected)
                {
                    if (Document.CustomLineManager.IsReadOnly(SelectionManager.SelectionCollection[0], false))
                        return;
                }
                else if (Document.CustomLineManager.IsReadOnly(Caret.Line, false) == true)
                    return;
            }

            if (ch < ' ')
            {
                return;
            }

            if (!HiddenMouseCursor && TextEditorProperties.HideMouseCursor)
            {
                HiddenMouseCursor = true;
                Cursor.Hide();
            }
            CloseToolTip();

            motherTextEditorControl.BeginUpdate();
            // INSERT char
            if (!HandleKeyPress(ch))
            {
                switch (Caret.CaretMode)
                {
                    case CaretMode.InsertMode:
                        InsertChar(ch);
                        break;
                    case CaretMode.OverwriteMode:
                        ReplaceChar(ch);
                        break;
                    default:
                        Debug.Assert(false, "Unknown caret mode " + Caret.CaretMode);
                        break;
                }
            }

            int currentLineNr = Caret.Line;
            int delta = Document.FormattingStrategy.FormatLine(this, currentLineNr, Document.PositionToOffset(Caret.Position), ch);

            motherTextEditorControl.EndUpdate();
            if (delta != 0)
            {
                //				this.motherTextEditorControl.UpdateLines(currentLineNr, currentLineNr);
            }

        }


        /// <summary>
        /// 输入字符事件
        /// </summary>
        public event EventHandler<TextInputCharArgs> InputChar;

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            SimulateKeyPress(e.KeyChar);
            e.Handled = true;
           //输入符号捕获时机有问题
            this.InputChar?.Invoke(this, new TextInputCharArgs(caret.Offset, e.KeyChar));

        }

        /// <summary>
        /// 此方法执行对话框按键
        /// </summary>
        public bool ExecuteDialogKey(Keys keyData)
        {
            // 尝试，如果对话密钥处理器设置为使用此
            if (DoProcessDialogKey != null && DoProcessDialogKey(keyData))
            {
                return true;
            }

            if (keyData == Keys.Back || keyData == Keys.Delete || keyData == Keys.Enter)
            {
                if (TextEditorProperties.UseCustomLine == true)
                {
                    if (SelectionManager.HasSomethingSelected)
                    {
                        if (Document.CustomLineManager.IsReadOnly(SelectionManager.SelectionCollection[0], false))
                            return true;
                    }
                    else
                    {
                        int curLineNr = Document.GetLineNumberForOffset(Caret.Offset);
                        if (Document.CustomLineManager.IsReadOnly(curLineNr, false) == true)
                            return true;
                        if ((Caret.Column == 0) && (curLineNr - 1 >= 0) && keyData == Keys.Back &&
                            Document.CustomLineManager.IsReadOnly(curLineNr - 1, false) == true)
                            return true;
                        if (keyData == Keys.Delete)
                        {
                            LineSegment curLine = Document.GetLineSegment(curLineNr);
                            if (curLine.Offset + curLine.Length == Caret.Offset &&
                                Document.CustomLineManager.IsReadOnly(curLineNr + 1, false) == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            // 如果没有（或过程为"静音"，请使用标准编辑操作
            IEditAction action = motherTextEditorControl.GetEditAction(keyData);
            AutoClearSelection = true;
            if (action != null)
            {
                motherTextEditorControl.BeginUpdate();
                try
                {
                    lock (Document)
                    {
                        action.Execute(this);
                        if (SelectionManager.HasSomethingSelected && AutoClearSelection /*&& caretchanged*/)
                        {
                            if (Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal)
                            {
                                SelectionManager.ClearSelection();
                            }
                        }
                    }
                }
                finally
                {
                    motherTextEditorControl.EndUpdate();
                    Caret.UpdateCaretPosition();
                }
                return true;
            }
            return false;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            return ExecuteDialogKey(keyData) || base.ProcessDialogKey(keyData);
        }
        #endregion
        /// <summary>
        /// 滚动到插入点
        /// </summary>
        public void ScrollToCaret()
        {
            motherTextAreaControl.ScrollToCaret();
        }

        /// <summary>
        /// 滚动到行
        /// </summary>
        /// <param name="line"></param>
        public void ScrollTo(int line)
        {
            motherTextAreaControl.ScrollTo(line);
        }

        /// <summary>
        /// 开始更新
        /// </summary>
        public void BeginUpdate()
        {
            motherTextEditorControl.BeginUpdate();
        }

        /// <summary>
        /// 结束更新
        /// </summary>
        public void EndUpdate()
        {
            motherTextEditorControl.EndUpdate();
        }

        /// <summary>
        /// 启用剪切或粘贴
        /// </summary>
        public bool EnableCutOrPaste
        {
            get
            {
                if (motherTextAreaControl == null)
                    return false;
                if (TextEditorProperties.UseCustomLine == true)
                {
                    if (SelectionManager.HasSomethingSelected == true)
                    {
                        if (Document.CustomLineManager.IsReadOnly(SelectionManager.SelectionCollection[0], false))
                            return false;
                    }
                    if (Document.CustomLineManager.IsReadOnly(Caret.Line, false) == true)
                        return false;
                }
                return true;

            }
        }

        /// <summary>
        /// 生成空白字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        string GenerateWhitespaceString(int length)
        {
            return new string(' ', length);
        }
        /// <remarks>
        /// 在插入点插入单个字符
        /// </remarks>
        public void InsertChar(char ch)
        {
            bool updating = motherTextEditorControl.IsUpdating;
            if (!updating)
            {
                BeginUpdate();
            }

            // 过滤掉伪造的空白字符，代之以标准空间（ASCII 32）
            if (char.IsWhiteSpace(ch) && ch != '\t' && ch != '\n')
            {
                ch = ' ';
            }
            bool removedText = false;
            if (Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal &&
                SelectionManager.SelectionCollection.Count > 0)
            {
                Caret.Position = SelectionManager.SelectionCollection[0].StartPosition;
                SelectionManager.RemoveSelectedText();
                removedText = true;
            }
            LineSegment caretLine = Document.GetLineSegment(Caret.Line);
            int offset = Caret.Offset;
            // 使用所需的柱子生成白空间 use desired column for generated whitespaces
            int dc = Math.Min(Caret.Column, Caret.DesiredColumn);
            if (caretLine.Length < dc && ch != '\n')
            {
                Document.Insert(offset, GenerateWhitespaceString(dc - caretLine.Length) + ch);
            }
            else
            {
                Document.Insert(offset, ch.ToString());
            }
            ++Caret.Column;

            if (removedText)
            {
                Document.UndoStack.UndoLast(2);
            }

            if (!updating)
            {
                EndUpdate();
                UpdateLineToEnd(Caret.Line, Caret.Column);
            }

            // 我更喜欢设置不标准列，如果你键入的东西
            //			++Caret.DesiredColumn;
        }

        /// <remarks>
        /// 在插入点插入字符串
        /// </remarks>
        public void InsertString(string str)
        {
            bool updating = motherTextEditorControl.IsUpdating;
            if (!updating)
            {
                BeginUpdate();
            }
            try
            {
                bool removedText = false;
                if (Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal &&
                    SelectionManager.SelectionCollection.Count > 0)
                {
                    Caret.Position = SelectionManager.SelectionCollection[0].StartPosition;
                    SelectionManager.RemoveSelectedText();
                    removedText = true;
                }

                int oldOffset = Document.PositionToOffset(Caret.Position);
                int oldLine = Caret.Line;
                LineSegment caretLine = Document.GetLineSegment(Caret.Line);
                if (caretLine.Length < Caret.Column)
                {
                    int whiteSpaceLength = Caret.Column - caretLine.Length;
                    Document.Insert(oldOffset, GenerateWhitespaceString(whiteSpaceLength) + str);
                    Caret.Position = Document.OffsetToPosition(oldOffset + str.Length + whiteSpaceLength);
                }
                else
                {
                    Document.Insert(oldOffset, str);
                    Caret.Position = Document.OffsetToPosition(oldOffset + str.Length);
                }
                if (removedText)
                {
                    Document.UndoStack.UndoLast(2);
                }
                if (oldLine != Caret.Line)
                {
                    UpdateToEnd(oldLine);
                }
                else
                {
                    UpdateLineToEnd(Caret.Line, Caret.Column);
                }
            }
            finally
            {
                if (!updating)
                {
                    EndUpdate();
                }
            }
        }

        /// <remarks>
        /// 在插入点替换字符
        /// </remarks>
        public void ReplaceChar(char ch)
        {
            bool updating = motherTextEditorControl.IsUpdating;
            if (!updating)
            {
                BeginUpdate();
            }
            if (Document.TextEditorProperties.DocumentSelectionMode == DocumentSelectionMode.Normal && SelectionManager.SelectionCollection.Count > 0)
            {
                Caret.Position = SelectionManager.SelectionCollection[0].StartPosition;
                SelectionManager.RemoveSelectedText();
            }

            int lineNr = Caret.Line;
            LineSegment line = Document.GetLineSegment(lineNr);
            int offset = Document.PositionToOffset(Caret.Position);
            if (offset < line.Offset + line.Length)
            {
                Document.Replace(offset, 1, ch.ToString());
            }
            else
            {
                Document.Insert(offset, ch.ToString());
            }
            if (!updating)
            {
                EndUpdate();
                UpdateLineToEnd(lineNr, Caret.Column);
            }
            ++Caret.Column;
            //			++Caret.DesiredColumn;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    if (caret != null)
                    {
                        caret.PositionChanged -= new EventHandler(SearchMatchingBracket);
                        caret.Dispose();
                    }
                    if (selectionManager != null)
                    {
                        selectionManager.Dispose();
                    }
                    Document.TextContentChanged -= new EventHandler(TextContentChanged);
                    Document.FoldingManager.FoldingsChanged -= new EventHandler(DocumentFoldingsChanged);
                    motherTextAreaControl = null;
                    motherTextEditorControl = null;
                    foreach (AbstractMargin margin in leftMargins)
                    {
                        if (margin is IDisposable)
                            (margin as IDisposable).Dispose();
                    }
                    textView.Dispose();
                }
            }
        }

        #region UPDATE Commands
        internal void UpdateLine(int line)
        {
            UpdateLines(0, line, line);
        }

        internal void UpdateLines(int lineBegin, int lineEnd)
        {
            UpdateLines(0, lineBegin, lineEnd);
        }

        internal void UpdateToEnd(int lineBegin)
        {
            //			if (lineBegin > FirstPhysicalLine + textView.VisibleLineCount) {
            //				return;
            //			}

            lineBegin = Math.Min(lineBegin, FirstPhysicalLine);
            int y = Math.Max(0, (int)(lineBegin * textView.FontHeight));
            y = Math.Max(0, y - this.virtualTop.Y);
            Rectangle r = new Rectangle(0,
                                        y,
                                        Width,
                                        Height - y);
            Invalidate(r);
        }

        internal void UpdateLineToEnd(int lineNr, int xStart)
        {
            UpdateLines(xStart, lineNr, lineNr);
        }

        internal void UpdateLine(int line, int begin, int end)
        {
            UpdateLines(line, line);
        }
        int FirstPhysicalLine
        {
            get
            {
                return VirtualTop.Y / textView.FontHeight;
            }
        }
        internal void UpdateLines(int xPos, int lineBegin, int lineEnd)
        {
            //			if (lineEnd < FirstPhysicalLine || lineBegin > FirstPhysicalLine + textView.VisibleLineCount) {
            //				return;
            //			}

            InvalidateLines((int)(xPos * this.TextView.WideSpaceWidth), lineBegin, lineEnd);
        }

        void InvalidateLines(int xPos, int lineBegin, int lineEnd)
        {
            lineBegin = Math.Max(Document.GetVisibleLine(lineBegin), FirstPhysicalLine);
            lineEnd = Math.Min(Document.GetVisibleLine(lineEnd), FirstPhysicalLine + textView.VisibleLineCount);
            int y = Math.Max(0, (int)(lineBegin * textView.FontHeight));
            int height = Math.Min(textView.DrawingPosition.Height, (int)((1 + lineEnd - lineBegin) * (textView.FontHeight + 1)));

            Rectangle r = new Rectangle(0,
                                        y - 1 - this.virtualTop.Y,
                                        Width,
                                        height + 3);

            Invalidate(r);
        }
        #endregion
        /// <summary>
        /// 按键事件
        /// </summary>
        public event KeyEventHandler KeyEventHandler;
        /// <summary>
        /// 处理对话框按键
        /// </summary>
        public event DialogKeyProcessor DoProcessDialogKey;

        //internal void
    }
}
