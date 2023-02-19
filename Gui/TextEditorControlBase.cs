using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using VCodeEditor.Actions;
using VCodeEditor.Document;

namespace VCodeEditor
{
    /// <summary>
    /// 代码编辑器主控件 基类
    /// </summary>
    [ToolboxItem(false)]
    public abstract class TextEditorControlBase : UserControl
    {

        /// <summary>
        /// 构造方法
        /// </summary>
        protected TextEditorControlBase()
        {
            GenerateDefaultActions();
            //注册高亮管理器ReloadSyntaxHighlighting事件
            HLManager.Manager.ReloadSyntaxHL += new EventHandler(ReloadHighlighting);
        }
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
        /// 文本区域控件抽象属性
        /// </summary>
        public abstract TextAreaControl ActiveTextAreaControl
        {
            get;
        }

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

        /// <summary>
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

        /// <remarks>
        /// 调用此方法以解锁文本区域。结束更新
        /// </remarks>
        public virtual void EndUpdate()
        {
            Debug.Assert(updateLevel > 0);
            updateLevel = Math.Max(0, updateLevel - 1);
        }

        /// <summary>
        /// 加载代码文件重载1
        /// </summary>
        /// <param name="fileName">代码文件名称</param>
        public void LoadFile(string fileName)
        {
           this. LoadFile(fileName, true, true);
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

        /// <summary>
        /// 抽象方法 选项改变
        /// </summary>
        public abstract void OptionsChanged();


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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                HLManager.Manager.ReloadSyntaxHL -= new EventHandler(ReloadHighlighting);
                document.HighlightStyle = null;
                document.UndoStack.TextEditorControl = null;
            }
            base.Dispose(disposing);
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

    }

}
