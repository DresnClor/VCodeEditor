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
    public class TextEditorControl : UserControl
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public TextEditorControl()
        {
            GenerateDefaultActions();
            //注册高亮管理器ReloadSyntaxHighlighting事件
            HLManager.Manager.ReloadSyntaxHL += new EventHandler(ReloadHighlighting);

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
        public  TextAreaControl ActiveTextAreaControl
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
        public  void OptionsChanged()
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
                    EditorExts?.GotoDefine(this);
                }));
            Menus.Items.Add(gotoDefine);
            this.ContextMenuItems.Add("GotoDefine", gotoDefine);
            //插入代码
            ToolStripMenuItem instCode = this.CreateMenuStrip("插入代码(&I)",
                Keys.Shift | Keys.Insert, Resource.InstCode,
                new EventHandler((object sender, EventArgs e) =>
                {
                    EditorExts?.InstCode(this);
                }));
            Menus.Items.Add(instCode);
            this.ContextMenuItems.Add("InstCode", instCode);
            //重命名
            ToolStripMenuItem rename = this.CreateMenuStrip("重命名(&N)",
                Keys.Alt | Keys.Shift | Keys.N, Resource.Rename,
                new EventHandler((object sender, EventArgs e) =>
                {
                    EditorExts?.RenameIdent(this);
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
                    EditorExts?.CodeTreeView(this);
                }));
            Menus.Items.Add(codeTree);
            this.ContextMenuItems.Add("CodeTree", codeTree);

            //保存
            ToolStripMenuItem save = this.CreateMenuStrip("保存(&S)",
                Keys.Control | Keys.S, Resource.Save,
                new EventHandler((object sender, EventArgs e) =>
                {
                    EditorExts?.Save(this);
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
                HLManager.Manager.ReloadSyntaxHL -= new EventHandler(ReloadHighlighting);
                document.HighlightStyle = null;
                document.UndoStack.TextEditorControl = null;
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
        public  void EndUpdate()
        {
            Debug.Assert(updateLevel > 0);
            updateLevel = Math.Max(0, updateLevel - 1);
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


        #region Base

        /// <summary>
        /// 更新等级
        /// </summary>
        private int updateLevel = 0;

        /// <summary>
        /// 此哈希表包含所有编辑器密钥，其中
        ///关键是关键组合和价值
        ///行动。
        /// </summary>
        protected Dictionary<Keys, IEditAction> editactions =
            new Dictionary<Keys, IEditAction>();

        #region 公共属性
        /// <summary>
        /// 新加：代码编辑器扩展
        /// </summary>
        public DresnClor.IEditorExtsable EditorExts
        {
            get => this.editorExts;
            set => this.editorExts = value;
        }
        private DresnClor.IEditorExtsable editorExts = null;

        /// <summary>
        /// 文本编辑器属性
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITextEditorProperties Property
        {
            get
            {
                return document.TextEditorProperties;
            }
            set
            {
                document.TextEditorProperties = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 当前文件的字符编码
        /// </value>
        public Encoding Encoding
        {
            get
            {
                if (encoding == null)
                    return Property.Encoding;
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }
        private Encoding encoding;

        /// <value>
        /// 当前文件名称
        /// </value>
        [Browsable(false)]
        [ReadOnly(true)]
        public string FileName
        {
            get
            {
                return currentFileName;
            }
            set
            {
                if (currentFileName != value)
                {
                    currentFileName = value;
                    OnFileNameChanged(EventArgs.Empty);
                }
            }
        }
        private string currentFileName = null;


        /// <summary>
        /// 是否更新
        /// </summary>
        [Browsable(false)]
        public bool IsUpdating
        {
            get
            {
                return updateLevel > 0;
            }
        }

        /// <value>
        /// 代码文档接口
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDocument Document
        {
            get
            {
                return document;
            }
            set
            {
                document = value;
                document.UndoStack.TextEditorControl = this;
            }
        }
        private IDocument document;

        /// <summary>
        /// 文本内容
        /// </summary>
        [Browsable(true)]
        public override string Text
        {
            get
            {
                return Document.TextContent;
            }
            set
            {
                Document.TextContent = value;
            }
        }

        /// <summary>
        /// 解析字体
        /// </summary>
        /// <param name="font">字体名称</param>
        /// <returns>返回解析的字体句柄</returns>
        public static Font ParseFont(string font)
        {
            string[] descr = font.Split(new char[] { ',', '=' });
            return new Font(descr[1], Single.Parse(descr[3]));
        }

        /// <summary>
        /// 是否为只读内容
        /// </summary>
        [Browsable(false)]
        [ReadOnly(true)]
        public bool IsReadOnly
        {
            get
            {
                return Document.ReadOnly;
            }
            set
            {
                Document.ReadOnly = value;
            }
        }

        /// <summary>
        /// 是否正在更新
        /// </summary>
        [Browsable(false)]
        public bool IsInUpdate
        {
            get
            {
                return this.updateLevel > 0;
            }
        }

        /// <value>
        /// 据推测，这是根据.NET文档做到这一点的方式，
        /// 而不是设置构建器中的大小
        /// </value>
        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 100);
            }
        }

        /// <summary>
        /// 高亮策略
        /// </summary>
        //新加
        public IStyleStrategy HLStrategy
        {
            get => this.document.HighlightStyle;
            set => this.document.HighlightStyle = value;
        }

        #region Document Properties
        /// <value>
        /// 文本区域显示真实空间
        /// </value>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("如果文本区域显示真实空间")]
        public bool ShowSpaces
        {
            get
            {
                return document.TextEditorProperties.ShowSpaces;
            }
            set
            {
                document.TextEditorProperties.ShowSpaces = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果文本区域内使用真正的反分析字体
        /// </value>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("If true antialiased fonts are used inside the textarea")]
        public bool UseAntiAliasFont
        {
            get
            {
                return document.TextEditorProperties.UseAntiAliasedFont;
            }
            set
            {
                document.TextEditorProperties.UseAntiAliasedFont = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果文本区域显示真实选项卡
        /// </value>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("If true tabs are shown in the textarea")]
        public bool ShowTabs
        {
            get
            {
                return document.TextEditorProperties.ShowTabs;
            }
            set
            {
                document.TextEditorProperties.ShowTabs = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果文本区域中显示真正的 EOL 标记
        /// </value>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("If true EOL markers are shown in the textarea")]
        public bool ShowEOLMarkers
        {
            get
            {
                return document.TextEditorProperties.ShowEOLMarker;
            }
            set
            {
                document.TextEditorProperties.ShowEOLMarker = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果是真，则水平尺显示在文本区域中
        /// </value>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("If true the horizontal ruler is shown in the textarea")]
        public bool ShowHRuler
        {
            get
            {
                return document.TextEditorProperties.ShowHorizontalRuler;
            }
            set
            {
                document.TextEditorProperties.ShowHorizontalRuler = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果是真的，则垂直标尺显示在文本区域中
        /// </value>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("If true the vertical ruler is shown in the textarea")]
        public bool ShowVRuler
        {
            get
            {
                return document.TextEditorProperties.ShowVerticalRuler;
            }
            set
            {
                document.TextEditorProperties.ShowVerticalRuler = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 显示垂直尺子的行
        /// </value>
        [Category("Appearance")]
        [DefaultValue(80)]
        [Description("The row in which the vertical ruler is displayed")]
        public int VRulerRow
        {
            get
            {
                return document.TextEditorProperties.VerticalRulerRow;
            }
            set
            {
                document.TextEditorProperties.VerticalRulerRow = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果文本区域中显示真实行数
        /// </value>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("If true line numbers are shown in the textarea")]
        public bool ShowLineNumbers
        {
            get
            {
                return document.TextEditorProperties.ShowLineNumbers;
            }
            set
            {
                document.TextEditorProperties.ShowLineNumbers = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果文本区域中标记了真正的无效行
        /// </value>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("If true invalid lines are marked in the textarea")]
        public bool ShowInvalidLines
        {
            get
            {
                return document.TextEditorProperties.ShowInvalidLines;
            }
            set
            {
                document.TextEditorProperties.ShowInvalidLines = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果文本区域启用了真正的折叠
        /// </value>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("If true folding is enabled in the textarea")]
        public bool EnableFolding
        {
            get
            {
                return document.TextEditorProperties.EnableFolding;
            }
            set
            {
                document.TextEditorProperties.EnableFolding = value;
                OptionsChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("If true matching brackets are highlighted")]
        public bool ShowMatchingBracket
        {
            get
            {
                return document.TextEditorProperties.ShowMatchingBracket;
            }
            set
            {
                document.TextEditorProperties.ShowMatchingBracket = value;
                OptionsChanged();
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("If true the icon bar is displayed")]
        public bool IsIconBarVisible
        {
            get
            {
                return document.TextEditorProperties.IsIconBarVisible;
            }
            set
            {
                document.TextEditorProperties.IsIconBarVisible = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 选项卡字符空间中的宽度
        /// </value>
        [Category("Appearance")]
        [DefaultValue(4)]
        [Description("The width in spaces of a tab character")]
        public int TabIndent
        {
            get
            {
                return document.TextEditorProperties.TabIndent;
            }
            set
            {
                document.TextEditorProperties.TabIndent = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 线查看器样式
        /// </value>
        [Category("Appearance")]
        [DefaultValue(LineViewerStyle.None)]
        [Description("The line viewer style")]
        public LineViewerStyle LineViewerStyle
        {
            get
            {
                return document.TextEditorProperties.LineViewerStyle;
            }
            set
            {
                document.TextEditorProperties.LineViewerStyle = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 凹痕样式
        /// </value>
        [Category("Behavior")]
        [DefaultValue(IndentStyle.Smart)]
        [Description("The indent style")]
        public IndentStyle IndentStyle
        {
            get
            {
                return document.TextEditorProperties.IndentStyle;
            }
            set
            {
                document.TextEditorProperties.IndentStyle = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果将真正的空格转换为tab
        /// </value>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Converts tabs to spaces while typing")]
        public bool ConvertTabsToSpaces
        {
            get
            {
                return document.TextEditorProperties.ConvertTabsToSpaces;
            }
            set
            {
                document.TextEditorProperties.ConvertTabsToSpaces = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果将真正的空格转换为选项卡
        /// </value>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Creates a backup copy for overwritten files")]
        public bool CreateBackupCopy
        {
            get
            {
                return document.TextEditorProperties.CreateBackupCopy;
            }
            set
            {
                document.TextEditorProperties.CreateBackupCopy = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果将真正的空格转换为选项卡
        /// </value>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Hide the mouse cursor while typing")]
        public bool HideMouseCursor
        {
            get
            {
                return document.TextEditorProperties.HideMouseCursor;
            }
            set
            {
                document.TextEditorProperties.HideMouseCursor = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 如果将真正的空格转换为选项卡
        /// </value>
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Allows the caret to be places beyonde the end of line")]
        public bool AllowCaretBeyondEOL
        {
            get
            {
                return document.TextEditorProperties.AllowCaretBeyondEOL;
            }
            set
            {
                document.TextEditorProperties.AllowCaretBeyondEOL = value;
                OptionsChanged();
            }
        }
        /// <value>
        /// if true spaces are converted to tabs
        /// </value>
        [Category("Behavior")]
        [DefaultValue(BracketMatchingStyle.After)]
        [Description("Specifies if the bracket matching should match the bracket before or after the caret.")]
        public BracketMatchingStyle BracketMatchingStyle
        {
            get
            {
                return document.TextEditorProperties.BracketMatchingStyle;
            }
            set
            {
                document.TextEditorProperties.BracketMatchingStyle = value;
                OptionsChanged();
            }
        }

        /// <value>
        /// 文本区域的基本字体。无粗体字体或方体字体
        //可以使用，因为粗体/italic 保留用于突出显示目的。
        /// </value>
        [Browsable(true)]
        [Description("The base font of the text area. No bold or italic fonts can be used because bold/italic is reserved for highlighting purposes.")]
        public override Font Font
        {
            get
            {
                return document.TextEditorProperties.Font;
            }
            set
            {
                document.TextEditorProperties.Font = value;
                OptionsChanged();
            }
        }

        #endregion

        /// <summary>
        /// 更新代码高亮风格
        /// </summary>
        //新加
        public void UpdateHL()
        {
            this.HLStrategy.MarkTokens(this.document);
        }


        /// <summary>
        /// 文件名称改变 事件委托
        /// </summary>
        public event EventHandler FileNameChanged;

        /// <summary>
        /// 改变 事件委托
        /// </summary>
        public event EventHandler Changed;


        #endregion 公共属性

        // <summary>
        /// 重新加载高亮
        /// </summary>
        /// <param name="sender">调用者</param>
        /// <param name="e">参数</param>
        void ReloadHighlighting(object sender, EventArgs e)
        {
            if (Document.HighlightStyle != null)
            {
                Document.HighlightStyle = HighlightStrategyFactory.CreateHLStrategy(Document.HighlightStyle.Name);
                OptionsChanged();
            }
        }

        /// <summary>
        /// 获取编辑动作
        /// </summary>
        /// <param name="keyData">键代码</param>
        /// <returns>编辑快捷键</returns>
        internal IEditAction GetEditAction(Keys keyData)
        {
            if (!editactions.ContainsKey(keyData))
            {
                return null;
            }
            return editactions[keyData];
        }

        /// <summary>
        /// 注册操作快捷键
        /// </summary>
        void GenerateDefaultActions()
        {
            //方向键：←键
            this.editactions[Keys.Left] = new CaretLeft();
            //Shift + ←键
            this.editactions[Keys.Left | Keys.Shift] = new ShiftCaretLeft();
            //Ctrl + ←键
            this.editactions[Keys.Left | Keys.Control] = new WordLeft();
            //Ctrl + Shift + ←键
            this.editactions[Keys.Left | Keys.Control | Keys.Shift] = new ShiftWordLeft();
            //方向键：→键
            this.editactions[Keys.Right] = new CaretRight();
            //Shift + →键
            this.editactions[Keys.Right | Keys.Shift] = new ShiftCaretRight();
            //Ctrl + →键
            this.editactions[Keys.Right | Keys.Control] = new WordRight();
            //Ctrl + Shift + →键
            this.editactions[Keys.Right | Keys.Control | Keys.Shift] = new ShiftWordRight();
            //方向键：↑键
            this.editactions[Keys.Up] = new CaretUp();
            //Shift + ↑键
            this.editactions[Keys.Up | Keys.Shift] = new ShiftCaretUp();
            //Ctrl + ↑键
            this.editactions[Keys.Up | Keys.Control] = new ScrollLineUp();
            //方向键：↓键
            this.editactions[Keys.Down] = new CaretDown();
            //Shift + ↓键
            this.editactions[Keys.Down | Keys.Shift] = new ShiftCaretDown();
            //Ctrl + ↓键
            this.editactions[Keys.Down | Keys.Control] = new ScrollLineDown();
            //Insert键
            this.editactions[Keys.Insert] = new ToggleEditMode();
            //Ctrl + Insert
            this.editactions[Keys.Insert | Keys.Control] = new Copy();
            //Shift + Insert
            this.editactions[Keys.Insert | Keys.Shift] = new Paste();
            //Delete键
            this.editactions[Keys.Delete] = new Delete();
            //Shift + Delete
            this.editactions[Keys.Delete | Keys.Shift] = new Cut();
            //Home键
            this.editactions[Keys.Home] = new Home();
            //Shift + Home
            this.editactions[Keys.Home | Keys.Shift] = new ShiftHome();
            //Ctrl + Home
            this.editactions[Keys.Home | Keys.Control] = new MoveToStart();
            //Ctrl + Shift + Home
            this.editactions[Keys.Home | Keys.Control | Keys.Shift] = new ShiftMoveToStart();
            //End键
            this.editactions[Keys.End] = new End();
            //Shift + End
            this.editactions[Keys.End | Keys.Shift] = new ShiftEnd();
            //Ctrl + End
            this.editactions[Keys.End | Keys.Control] = new MoveToEnd();
            //Ctrl + Shift + End
            this.editactions[Keys.End | Keys.Control | Keys.Shift] = new ShiftMoveToEnd();
            //PageUp键
            this.editactions[Keys.PageUp] = new MovePageUp();
            //Shift + PageUp
            this.editactions[Keys.PageUp | Keys.Shift] = new ShiftMovePageUp();
            //PageDown键
            this.editactions[Keys.PageDown] = new MovePageDown();
            //Shift + PageDown
            this.editactions[Keys.PageDown | Keys.Shift] = new ShiftMovePageDown();

            //Enter键
            this.editactions[Keys.Return] = new Return();
            //Tab键
            this.editactions[Keys.Tab] = new Tab();
            //Shift + Tab
            this.editactions[Keys.Tab | Keys.Shift] = new ShiftTab();
            //Backspace键
            this.editactions[Keys.Back] = new Backspace();
            //Shift + Backspace
            this.editactions[Keys.Back | Keys.Shift] = new Backspace();

            //Ctrl + X
            this.editactions[Keys.X | Keys.Control] = new Cut();
            //Ctrl + C
            this.editactions[Keys.C | Keys.Control] = new Copy();
            //Ctrl + V
            this.editactions[Keys.V | Keys.Control] = new Paste();

            //Ctrl + A
            this.editactions[Keys.A | Keys.Control] = new SelectWholeDocument();
            //Esc键
            this.editactions[Keys.Escape] = new ClearAllSelections();

            //Ctrl + /
            this.editactions[Keys.Divide | Keys.Control] = new ToggleComment();
            //Ctrl + ?
            this.editactions[Keys.OemQuestion | Keys.Control] = new ToggleComment();

            //Alt + Backspace
            this.editactions[Keys.Back | Keys.Alt] = new Actions.Undo();
            //Ctrl + Z
            this.editactions[Keys.Z | Keys.Control] = new Actions.Undo();
            //Ctrl + Y
            this.editactions[Keys.Y | Keys.Control] = new Redo();

            //Ctrl + Delete
            this.editactions[Keys.Delete | Keys.Control] = new DeleteWord();
            //Ctrl + Backspace
            this.editactions[Keys.Back | Keys.Control] = new WordBackspace();
            //Ctrl + D
            this.editactions[Keys.D | Keys.Control] = new DeleteLine();
            //Ctrl + Shift + D
            this.editactions[Keys.D | Keys.Shift | Keys.Control] = new DeleteToLineEnd();
            //Ctrl + B
            this.editactions[Keys.B | Keys.Control] = new GotoMatchingBrace();
        }

        /// <remarks>
        /// 在长期更新操作之前调用此方法锁定文本区域，以便不进行屏幕更新。
        /// </remarks>
        public virtual void BeginUpdate()
        {
            ++updateLevel;
        }


        /// <summary>
        /// 加载代码文件重载1
        /// </summary>
        /// <param name="fileName">代码文件名称</param>
        public void LoadFile(string fileName)
        {
            this.LoadFile(fileName, true, true);
        }

        /// <remarks>
        /// 加载代码文件重载2
        /// </remarks>
        /// <param name="fileName">代码文件名称</param>
        /// <param name="autoLoadHighlighting">是否自动加载代码高亮</param>
        /// <param name="autodetectEncoding">是否自动检测并设置编码</param>
        public void LoadFile(string fileName, bool autoLoadHighlighting, bool autodetectEncoding)
        {
            BeginUpdate();
            document.TextContent = String.Empty;
            document.UndoStack.ClearAll();
            document.BookmarkManager.Clear();
            if (autoLoadHighlighting)
            {
                document.HighlightStyle =
                    HighlightStrategyFactory.CreateHLStrategyForFile(fileName);
            }

            if (autodetectEncoding)
            {
                Encoding encoding = this.Encoding;
                Document.TextContent = Util.FileReader.ReadFileContent(fileName, ref encoding, this.Property.Encoding);
                this.Encoding = encoding;
            }
            else
            {
                using (StreamReader reader = new StreamReader(fileName, this.Encoding))
                {
                    Document.TextContent = reader.ReadToEnd();
                }
            }

            this.FileName = fileName;

            OptionsChanged();

            Document.UpdateQueue.Clear();

            EndUpdate();
            Refresh();
        }

        /// <summary>
        /// 获取文档是否可以在不丢失数据的情况下使用当前编码保存。
        /// </summary>
        public bool CanSaveWithCurrentEncoding()
        {
            if (encoding == null || Util.FileReader.IsUnicode(encoding))
                return true;
            // 不是单码代码页
            string text = document.TextContent;

            return encoding.GetString(encoding.GetBytes(text)) == text;
        }

        /// <summary>
        /// 保存代码文件
        /// </summary>
        /// <param name="fileName">代码文件名称</param>
        public void SaveFile(string fileName)
        {
            if (document.TextEditorProperties.CreateBackupCopy)
            {//备份文件
             //MakeBackupCopy(fileName);
            }

            StreamWriter stream;
            Encoding encoding = this.Encoding;
            if (encoding == null)
            { // 默认情况下使用与BOM的UTF8
                stream = new StreamWriter(fileName, false, Encoding.UTF8);
            }
            else
            {
                stream = new StreamWriter(fileName, false, encoding);
            }

            foreach (LineSegment line in Document.LineSegmentCollection)
            {
                stream.Write(Document.GetText(line.Offset, line.Length));
                stream.Write(document.TextEditorProperties.LineTerminator);
            }

            stream.Close();

            this.FileName = fileName;
        }

        /// <summary>
        /// 创建备份原文件
        /// </summary>
        /// <param name="fileName">备份文件名称</param>
        public void MakeBackupCopy(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    string backupName = fileName + ".bak";
                    File.Copy(fileName, backupName, true);
                }
            }
            catch (Exception)
            {
                //
                //				MessageService.ShowError(e, "Can not create backup copy of " + fileName);
            }
        }


        // Localization ISSUES

        // 用于洞察窗口 used in insight window

        public virtual string GetRangeDescription(int selectedItem, int itemCount)
        {
            StringBuilder sb = new StringBuilder(selectedItem.ToString());
            sb.Append(" from ");
            sb.Append(itemCount.ToString());
            return sb.ToString();
        }

        /// <remarks>
        /// 覆盖刷新方法，如果控制处于锁定中，更新周期
        /// </remarks>
        public override void Refresh()
        {
            if (IsUpdating)
            {
                return;
            }
            base.Refresh();
        }


        protected virtual void OnFileNameChanged(EventArgs e)
        {
            if (FileNameChanged != null)
            {
                FileNameChanged(this, e);
            }
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }
        #endregion Base
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
