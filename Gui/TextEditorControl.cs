// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Xml;
using System.Text;

using VCodeEditor.Document;
using VCodeEditor.Actions;
using System.Collections.Generic;

namespace VCodeEditor
{
    /// <summary>
    /// 代码编辑器主控件
    /// </summary>
    [ToolboxBitmap("VCodeEditor.Resources.TextEditorControl.bmp")]
    [ToolboxItem(true)]
    public class TextEditorControl : TextEditorControlBase
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public TextEditorControl()
        {
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.Selectable, true);

            textAreaPanel.Dock = DockStyle.Fill;
            Document = (new DocumentFactory()).CreateDocument();
            Document.HighlightStyle = HighlightStrategyFactory.CreateHLStrategy();
            primaryTextArea = new TextAreaControl(this);
            primaryTextArea.Dock = DockStyle.Fill;
            textAreaPanel.Controls.Add(primaryTextArea);
            InitializeTextAreaControl(primaryTextArea);
            Controls.Add(textAreaPanel);
            ResizeRedraw = true;
            this.ContextMenuItems = new Dictionary<string, ToolStripMenuItem>();
            this.EditAction = new DresnClor.EditAction(this.TextArea);
            this.Caret.PositionChanged += this.Caret_PositionChanged;
            Document.UpdateCommited += new EventHandler(CommitUpdateRequested);
            OptionsChanged();
        }

        //插入符号 位置改变
        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            this.CaretChanged?.Invoke(
                this,
                new CaretChangedArgs(
                    this.Caret.Offset, 
                    this.Caret.Line, 
                    this.Caret.Column, 
                    this.Caret.Position));
        }

        //文档改变事件
        private void Document_DocumentChanged(object sender, DocumentEventArgs e)
        {
            /*if (e.Text != null && e.Text.Length == 1)
            {
                this.InputChar?.Invoke(
                    this, 
                    new TextInputCharArgs(e.Offset, e.Text[0]));
            }*/
        }

        /// <summary>
        /// 输入字符事件
        /// </summary>
        public event EventHandler<TextInputCharArgs> InputChar
        {
            add
            {
                this.TextArea.InputChar += value;
            }
            remove
            {
                this.TextArea.InputChar -= value;
            }
        }

        /// <summary>
        /// 插入符号位置改变
        /// </summary>

        public event EventHandler<CaretChangedArgs> CaretChanged;

        /// <summary>
        /// 编辑动作
        /// </summary>
        public DresnClor.EditAction EditAction
        {
            get;
            private set;
        }

        /// <summary>
        /// 文本区域面板
        /// </summary>
        protected Panel textAreaPanel = new Panel();

        /// <summary>
        /// 主要文本区域控件
        /// </summary>
        private TextAreaControl primaryTextArea;

        /// <summary>
        /// 文本区域拆分器
        /// </summary>
        private Splitter textAreaSplitter = null;

        /// <summary>
        /// 次要文本区域控件
        /// </summary>
        private TextAreaControl secondaryTextArea = null;

        /// <summary>
        /// 打印文档
        /// </summary>
        private PrintDocument printDocument = null;

        /// <summary>
        /// 插入点
        /// </summary>
        //新加
        public Caret Caret
        {
            get => this.TextArea.Caret;
        }

        /// <summary>
        /// 获取打印文档对象
        /// </summary>
        public PrintDocument PrintDocument
        {
            get
            {
                if (printDocument == null)
                {
                    printDocument = new PrintDocument();
                    printDocument.BeginPrint += new PrintEventHandler(this.BeginPrint);
                    printDocument.PrintPage += new PrintPageEventHandler(this.PrintPage);
                }
                return printDocument;
            }
        }

        /// <summary>
        /// 新加：获取选中的文本 实际位置this.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText
        /// </summary>
        public string SelectText
        {
            get => this.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
        }

        /// <summary>
        /// 新加：获取文本区域控件句柄（执行绘制、编辑等操作）
        /// </summary>
        public TextArea TextArea
        {
            get => this.ActiveTextAreaControl.TextArea;
        }


        /// <summary>
        /// 文本区域控件
        /// </summary>
        public override TextAreaControl ActiveTextAreaControl
        {
            get
            {
                return primaryTextArea;
            }
        }

        
        protected virtual void InitializeTextAreaControl(TextAreaControl newControl)
        {

        }

        /// <summary>
        /// 选项改变
        /// </summary>
        public override void OptionsChanged()
        {
            primaryTextArea.OptionsChanged();
            if (secondaryTextArea != null)
            {
                secondaryTextArea.OptionsChanged();
            }
        }


        /// <summary>
        /// 分割文本区域
        /// </summary>
        public void Split()
        {
            if (secondaryTextArea == null)
            {
                secondaryTextArea = new TextAreaControl(this);
                secondaryTextArea.Dock = DockStyle.Bottom;
                secondaryTextArea.Height = Height / 2;
                textAreaSplitter = new Splitter();
                textAreaSplitter.BorderStyle = BorderStyle.FixedSingle;
                textAreaSplitter.Height = 8;
                textAreaSplitter.Dock = DockStyle.Bottom;
                textAreaPanel.Controls.Add(textAreaSplitter);
                textAreaPanel.Controls.Add(secondaryTextArea);
                InitializeTextAreaControl(secondaryTextArea);
                secondaryTextArea.OptionsChanged();
            }
            else
            {
                textAreaPanel.Controls.Remove(secondaryTextArea);
                textAreaPanel.Controls.Remove(textAreaSplitter);

                secondaryTextArea.Dispose();
                textAreaSplitter.Dispose();
                secondaryTextArea = null;
                textAreaSplitter = null;
            }
        }

        /// <summary>
        /// Undo状态
        /// </summary>
        public bool EnableUndo
        {
            get
            {
                return Document.UndoStack.CanUndo;
            }
        }

        /// <summary>
        /// Redo状态
        /// </summary>
        public bool EnableRedo
        {
            get
            {
                return Document.UndoStack.CanRedo;
            }
        }

        /// <summary>
        /// 撤销操作
        /// </summary>
        public void Undo()
        {
            if (Document.ReadOnly)
            {
                return;
            }
            if (Document.UndoStack.CanUndo)
            {
                BeginUpdate();
                Document.UndoStack.Undo();

                Document.UpdateQueue.Clear();
                Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                this.primaryTextArea.TextArea.UpdateMatchingBracket();
                if (secondaryTextArea != null)
                {
                    this.secondaryTextArea.TextArea.UpdateMatchingBracket();
                }
                EndUpdate();
            }
        }

        /// <summary>
        /// 重做操作
        /// </summary>
        public void Redo()
        {
            if (Document.ReadOnly)
            {
                return;
            }
            if (Document.UndoStack.CanRedo)
            {
                BeginUpdate();
                Document.UndoStack.Redo();

                Document.UpdateQueue.Clear();
                Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                this.primaryTextArea.TextArea.UpdateMatchingBracket();
                if (secondaryTextArea != null)
                {
                    this.secondaryTextArea.TextArea.UpdateMatchingBracket();
                }
                EndUpdate();
            }
        }

        /// <summary>
        /// 初始化样式
        /// </summary>
        public void InitStyle()
        {
            this.Property.ShowTabs = false;
            this.Property.ShowSpaces = false;
            this.Property.ShowEOLMarker = false;
            this.Property.UseAntiAliasedFont = true;
            this.OptionsChanged();
        }

        /// <summary>
        /// 初始化样式
        /// </summary>
        /// <param name="filePath">高亮文件路径</param>
        public void InitStyle(string filePath)
        {
           /* this.HLStrategy = new FileHLStrategy(
                Path.GetExtension(filePath),
                filePath);
            this.InitStyle();*/
        }

        /// <summary>
        /// 设置高亮方式
        /// </summary>
        /// <param name="name">风格名称</param>
        public void SetHighlighting(string name)
        {
            Document.HighlightStyle =
                HighlightStrategyFactory.CreateHLStrategy(name);
        }

        /// <summary>
        /// 新加：创建菜单项
        /// </summary>
        /// <param name="text">菜单文本</param>
        /// <param name="keys">菜单项快捷键</param>
        /// <param name="image">菜单图标</param>
        /// <param name="clickEvent">菜单单击事件</param>
        /// <returns>返回创建的菜单句柄</returns>
        ToolStripMenuItem CreateMenuStrip(string text, Keys keys, Image image, EventHandler clickEvent)
        {
            ToolStripMenuItem Menus = new ToolStripMenuItem();
            Menus.Text = text;
            if (keys != Keys.None)
                Menus.ShortcutKeys = keys;
            if (image != null)
                Menus.Image = image;
            if (clickEvent != null)
                Menus.Click += clickEvent;

            return Menus;
        }

        /// <summary>
        /// 右键菜单项列表
        /// </summary>
        public Dictionary<string, ToolStripMenuItem> ContextMenuItems
        {
            get;
            private set;
        }

        /// <summary>
        /// 设置默认弹出菜单
        /// </summary>
        public void SetDefaultPopupMenu()
        {
            //绑定右键菜单
            this.ContextMenuStrip = this.CreatRightMenu();
        }

        /// <summary>
        /// 新加：右键菜单
        /// </summary>
        /// <returns>右键菜单句柄</returns>
        internal ContextMenuStrip CreatRightMenu()
        {
            ContextMenuStrip Menus = new ContextMenuStrip();
            //转到定义
            ToolStripMenuItem gotoDefine = this.CreateMenuStrip("转到定义(&G)",
                Keys.Control | Keys.F12, Resource.GotoDefine,
                new EventHandler((object sender, EventArgs e) =>
                {
                    base.EditorExts?.GotoDefine(this);
                }));
            Menus.Items.Add(gotoDefine);
            this.ContextMenuItems.Add("GotoDefine", gotoDefine);
            //插入代码
            ToolStripMenuItem instCode = this.CreateMenuStrip("插入代码(&I)",
                Keys.Shift | Keys.Insert, Resource.InstCode,
                new EventHandler((object sender, EventArgs e) =>
                {
                    base.EditorExts?.InstCode(this);
                }));
            Menus.Items.Add(instCode);
            this.ContextMenuItems.Add("InstCode", instCode);
            //重命名
            ToolStripMenuItem rename = this.CreateMenuStrip("重命名(&N)",
                Keys.Alt | Keys.Shift | Keys.N, Resource.Rename,
                new EventHandler((object sender, EventArgs e) =>
                {
                    base.EditorExts?.RenameIdent(this);
                }));

            Menus.Items.Add(rename);
            this.ContextMenuItems.Add("Rename", rename);
            //-
            Menus.Items.Add(new ToolStripSeparator());
            //撤销
            ToolStripMenuItem undo = this.CreateMenuStrip("撤销(&U)",
                Keys.Control | Keys.Z, Resource.Undo,
                new EventHandler((object sender, EventArgs e) =>
                {
                    this.Undo();
                }));
            Menus.Items.Add(undo);
            this.ContextMenuItems.Add("Undo", undo);
            //重做
            ToolStripMenuItem redo = this.CreateMenuStrip("重做(&R)",
                Keys.Control | Keys.Y, Resource.Redo,
                new EventHandler((object sender, EventArgs e) =>
                {
                    this.Redo();
                }));
            Menus.Items.Add(redo);
            this.ContextMenuItems.Add("Redo", redo);
            //-
            Menus.Items.Add(new ToolStripSeparator());
            //复制
            ToolStripMenuItem copy = this.CreateMenuStrip("复制(&C)",
                Keys.Control | Keys.C, Resource.Copy,
                new EventHandler((object sender, EventArgs e) =>
                {
                    this.EditAction.Copy();
                }));
            Menus.Items.Add(copy);
            this.ContextMenuItems.Add("Copy", copy);
            //剪切
            ToolStripMenuItem cut = this.CreateMenuStrip("剪切(&T)",
                Keys.Control | Keys.X, Resource.Cut,
                new EventHandler((object sender, EventArgs e) =>
                {
                    this.EditAction.Cut();
                }));
            Menus.Items.Add(cut);
            this.ContextMenuItems.Add("Cut", cut);
            //粘贴
            ToolStripMenuItem paste = this.CreateMenuStrip("粘贴(&P)",
                Keys.Control | Keys.V, Resource.Paste,
                new EventHandler((object sender, EventArgs e) =>
                {
                    this.EditAction.Paste();
                }));
            Menus.Items.Add(paste);
            this.ContextMenuItems.Add("Paste", paste);
            //-
            Menus.Items.Add(new ToolStripSeparator());
            //删除
            ToolStripMenuItem delete = this.CreateMenuStrip("删除(&D)",
                Keys.Delete, Resource.Delete,
                new EventHandler((object sender, EventArgs e) =>
                {
                    new Delete().Execute(this.ActiveTextAreaControl.TextArea);
                }));
            Menus.Items.Add(delete);
            this.ContextMenuItems.Add("Delete", delete);
            //-
            Menus.Items.Add(new ToolStripSeparator());
            //全选
            ToolStripMenuItem selectAll = this.CreateMenuStrip("全选(&A)",
                Keys.Control | Keys.A, null,
                new EventHandler((object sender, EventArgs e) =>
                {
                    new SelectWholeDocument().Execute(this.ActiveTextAreaControl.TextArea);
                }));
            Menus.Items.Add(selectAll);
            this.ContextMenuItems.Add("SelectAll", selectAll);
            //-
            Menus.Items.Add(new ToolStripSeparator());
            //查看代码树
            ToolStripMenuItem codeTree = this.CreateMenuStrip("查看代码树(&E)",
                Keys.None, Resource.CodeTree,
                new EventHandler((object sender, EventArgs e) =>
                {
                    base.EditorExts?.CodeTreeView(this);
                }));
            Menus.Items.Add(codeTree);
            this.ContextMenuItems.Add("CodeTree", codeTree);

            //保存
            ToolStripMenuItem save = this.CreateMenuStrip("保存(&S)",
                Keys.Control | Keys.S, Resource.Save,
                new EventHandler((object sender, EventArgs e) =>
                {
                    base.EditorExts?.Save(this);
                }));
            Menus.Items.Add(save);
            this.ContextMenuItems.Add("Save", save);
            //控件即将显示事件
            Menus.Opening += new CancelEventHandler((object sender, CancelEventArgs e) =>
            {
                if (this.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText.Length == 0)
                {//选中文本为0
                    gotoDefine.Enabled = false;
                    //rename.Enabled = false;
                }
                else
                {
                    gotoDefine.Enabled = true;
                    //rename.Enabled = true;
                }
                undo.Enabled = this.EnableUndo;
                redo.Enabled = this.EnableRedo;

                //copy
                copy.Enabled = this.TextArea.ClipboardHandler.EnableCopy;
                //cut
                cut.Enabled = this.TextArea.ClipboardHandler.EnableCopy;
                //paste
                paste.Enabled = this.TextArea.ClipboardHandler.EnableCopy;
                //selectAll
                selectAll.Enabled = this.TextArea.ClipboardHandler.EnableSelectAll;
                //delete
                delete.Enabled = this.TextArea.ClipboardHandler.EnableDelete;

            });

            return Menus;
        }

        /// <summary>
        /// 销毁控件
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (printDocument != null)
                {
                    printDocument.BeginPrint -= new PrintEventHandler(this.BeginPrint);
                    printDocument.PrintPage -= new PrintPageEventHandler(this.PrintPage);
                    printDocument = null;
                }
                Document.UndoStack.ClearAll();
                Document.UpdateCommited -= new EventHandler(CommitUpdateRequested);
                if (textAreaPanel != null)
                {
                    if (secondaryTextArea != null)
                    {
                        secondaryTextArea.Dispose();
                        textAreaSplitter.Dispose();
                        secondaryTextArea = null;
                        textAreaSplitter = null;
                    }
                    if (primaryTextArea != null)
                    {
                        primaryTextArea.Dispose();
                    }
                    textAreaPanel.Dispose();
                    textAreaPanel = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Update Methods
        /// <summary>
        /// 结束更新
        /// </summary>
        public override void EndUpdate()
        {
            base.EndUpdate();
            Document.CommitUpdate();
        }

        void CommitUpdateRequested(object sender, EventArgs e)
        {
            if (IsUpdating)
            {
                return;
            }
            foreach (TextAreaUpdate update in Document.UpdateQueue)
            {
                switch (update.TextAreaUpdateType)
                {
                    case TextAreaUpdateType.PositionToEnd:
                        this.primaryTextArea.TextArea.UpdateToEnd(update.Position.Y);
                        if (this.secondaryTextArea != null)
                        {
                            this.secondaryTextArea.TextArea.UpdateToEnd(update.Position.Y);
                        }
                        break;
                    case TextAreaUpdateType.PositionToLineEnd:
                    case TextAreaUpdateType.SingleLine:
                        this.primaryTextArea.TextArea.UpdateLine(update.Position.Y);
                        if (this.secondaryTextArea != null)
                        {
                            this.secondaryTextArea.TextArea.UpdateLine(update.Position.Y);
                        }
                        break;
                    case TextAreaUpdateType.SinglePosition:
                        this.primaryTextArea.TextArea.UpdateLine(update.Position.Y, update.Position.X, update.Position.X);
                        if (this.secondaryTextArea != null)
                        {
                            this.secondaryTextArea.TextArea.UpdateLine(update.Position.Y, update.Position.X, update.Position.X);
                        }
                        break;
                    case TextAreaUpdateType.LinesBetween:
                        this.primaryTextArea.TextArea.UpdateLines(update.Position.X, update.Position.Y);
                        if (this.secondaryTextArea != null)
                        {
                            this.secondaryTextArea.TextArea.UpdateLines(update.Position.X, update.Position.Y);
                        }
                        break;
                    case TextAreaUpdateType.WholeTextArea:
                        this.primaryTextArea.TextArea.Invalidate();
                        if (this.secondaryTextArea != null)
                        {
                            this.secondaryTextArea.TextArea.Invalidate();
                        }
                        break;
                }
            }
            Document.UpdateQueue.Clear();
            this.primaryTextArea.TextArea.Update();
            if (this.secondaryTextArea != null)
            {
                this.secondaryTextArea.TextArea.Update();
            }
        }
        #endregion

        #region Printing routines
        int curLineNr = 0;
        float curTabIndent = 0;
        StringFormat printingStringFormat;

        void BeginPrint(object sender, PrintEventArgs ev)
        {
            curLineNr = 0;
            printingStringFormat = (StringFormat)System.Drawing.StringFormat.GenericTypographic.Clone();

            // 100 应该足够每个人...犯 错？
            float[] tabStops = new float[100];
            for (int i = 0; i < tabStops.Length; ++i)
            {
                tabStops[i] = TabIndent * primaryTextArea.TextArea.TextView.WideSpaceWidth;
            }

            printingStringFormat.SetTabStops(0, tabStops);
        }

        void Advance(ref float x, ref float y, float maxWidth, float size, float fontHeight)
        {
            if (x + size < maxWidth)
            {
                x += size;
            }
            else
            {
                x = curTabIndent;
                y += fontHeight;
            }
        }
        //顺便 说 一 句。我讨厌源代码复制...但这次我不在乎!!!!
        // btw. I hate source code duplication ... but this time I don't care !!!!
        float MeasurePrintingHeight(Graphics g, LineSegment line, float maxWidth)
        {
            float xPos = 0;
            float yPos = 0;
            float fontHeight = Font.GetHeight(g);
            //			bool  gotNonWhitespace = false;
            curTabIndent = 0;
            foreach (TextWord word in line.Words)
            {
                switch (word.Type)
                {
                    case TextWordType.Space:
                        Advance(ref xPos, ref yPos, maxWidth, primaryTextArea.TextArea.TextView.SpaceWidth, fontHeight);
                        //						if (!gotNonWhitespace) {
                        //							curTabIndent = xPos;
                        //						}
                        break;
                    case TextWordType.Tab:
                        Advance(ref xPos, ref yPos, maxWidth, TabIndent * primaryTextArea.TextArea.TextView.WideSpaceWidth, fontHeight);
                        //						if (!gotNonWhitespace) {
                        //							curTabIndent = xPos;
                        //						}
                        break;
                    case TextWordType.Word:
                        //						if (!gotNonWhitespace) {
                        //							gotNonWhitespace = true;
                        //							curTabIndent    += TabIndent * primaryTextArea.TextArea.TextView.GetWidth(' ');
                        //						}
                        SizeF drawingSize = g.MeasureString(word.Word, word.Font, new SizeF(maxWidth, fontHeight * 100), printingStringFormat);
                        Advance(ref xPos, ref yPos, maxWidth, drawingSize.Width, fontHeight);
                        break;
                }
            }
            return yPos + fontHeight;
        }

        /// <summary>
        /// 绘制行
        /// </summary>
        /// <param name="g">GDI对象</param>
        /// <param name="line">行所在段</param>
        /// <param name="yPos"></param>
        /// <param name="margin"></param>
        void DrawLine(Graphics g, LineSegment line, float yPos, RectangleF margin)
        {
            float xPos = 0;
            float fontHeight = Font.GetHeight(g);
            //			bool  gotNonWhitespace = false;
            curTabIndent = 0;

            foreach (TextWord word in line.Words)
            {
                switch (word.Type)
                {
                    case TextWordType.Space:
                        Advance(ref xPos, ref yPos, margin.Width, primaryTextArea.TextArea.TextView.SpaceWidth, fontHeight);
                        //						if (!gotNonWhitespace) {
                        //							curTabIndent = xPos;
                        //						}
                        break;
                    case TextWordType.Tab:
                        Advance(ref xPos, ref yPos, margin.Width, TabIndent * primaryTextArea.TextArea.TextView.WideSpaceWidth, fontHeight);
                        //						if (!gotNonWhitespace) {
                        //							curTabIndent = xPos;
                        //						}
                        break;
                    case TextWordType.Word:
                        //						if (!gotNonWhitespace) {
                        //							gotNonWhitespace = true;
                        //							curTabIndent    += TabIndent * primaryTextArea.TextArea.TextView.GetWidth(' ');
                        //						}
                        g.DrawString(word.Word, word.Font, BrushRegistry.GetBrush(word.Color), xPos + margin.X, yPos);
                        SizeF drawingSize = g.MeasureString(word.Word, word.Font, new SizeF(margin.Width, fontHeight * 100), printingStringFormat);
                        Advance(ref xPos, ref yPos, margin.Width, drawingSize.Width, fontHeight);
                        break;
                }
            }
        }

        /// <summary>
        /// 打印页
        /// </summary>
        /// <param name="sender">调用者</param>
        /// <param name="ev">参数</param>
        void PrintPage(object sender, PrintPageEventArgs ev)
        {
            Graphics g = ev.Graphics;
            float yPos = ev.MarginBounds.Top;

            while (curLineNr < Document.TotalNumberOfLines)
            {
                LineSegment curLine = Document.GetLineSegment(curLineNr);
                if (curLine.Words != null)
                {
                    float drawingHeight = MeasurePrintingHeight(g, curLine, ev.MarginBounds.Width);
                    if (drawingHeight + yPos > ev.MarginBounds.Bottom)
                    {
                        break;
                    }

                    DrawLine(g, curLine, yPos, ev.MarginBounds);
                    yPos += drawingHeight;
                }
                ++curLineNr;
            }
            
            // 如果存在更多行，则打印另一页。
            ev.HasMorePages = curLineNr < Document.TotalNumberOfLines;
        }
        #endregion
    }


    public class CaretChangedArgs : EventArgs
    {
        public CaretChangedArgs(int offset, int line,int column, Point position)
        {
            this.Offset = offset;
            this.LineNumber=line;
            this.ColumnNumber = column;
            this.Position = position;
        }

        /// <summary>
        /// 位置坐标
        /// </summary>
        public Point Position
        {
            get;
            private set;
        }

        /// <summary>
        /// 偏移位置
        /// </summary>
        public int Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// 行号
        /// </summary>
        public int LineNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 列号
        /// </summary>
        public int ColumnNumber
        {
            get;
            private set;
        }
    }

    public class TextInputCharArgs : EventArgs
    {
        public TextInputCharArgs(int offset, char c)
        {
            this.Offset = offset;
            this.Character = c;
        }

        /// <summary>
        /// 输入的字符
        /// </summary>
        public char Character
        {
            get;
            private set;
        }

        /// <summary>
        /// 偏移位置
        /// </summary>
        public int Offset
        {
            get;
            private set;
        }
    }
}
