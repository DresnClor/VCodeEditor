using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeltuiCodeEditor;
using MeltuiCodeEditor.Document;
using MeltuiCodeEditor.Gui;


namespace DresnClor
{
    /// <summary>
    /// 代码编辑动作
    /// </summary>
    public class EditAction
    {
        public EditAction(TextArea textArea)
        {
            this.TextArea = textArea;
        }

        /// <summary>
        /// 文本区域
        /// </summary>
        public TextArea TextArea
        {
            get;
            private set;
        }

        protected IDocument Document
        {
            get => this.TextArea.Document;
        }

        #region 书签相关
        /// <summary>
        /// 切换当前行书签
        /// </summary>
        public void ToggleBookmark()
        {

            this.ToggleBookmark(this.TextArea.Caret.Line);
        }
        /// <summary>
        /// 切换当前行书签
        /// </summary>
        /// <param name="line">行号</param>
        public void ToggleBookmark(int line)
        {
            this.Document.BookmarkManager.ToggleMarkAt(line);
            this.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, line));
            this.Document.CommitUpdate();
        }

        /// <summary>
        /// 转到上一个书签
        /// </summary>
        /// <param name="predicate"></param>
        public void GotoPrevBookmark(Predicate<Bookmark> predicate)
        {
            Bookmark mark = this.Document.BookmarkManager.GetPrevMark(
                this.TextArea.Caret.Line,
                predicate);
            if (mark != null)
            {
                this.TextArea.Caret.Line = mark.LineNumber;
                this.TextArea.SelectionManager.ClearSelection();
            }
        }

        /// <summary>
        /// 转到下一个书签
        /// </summary>
        /// <param name="predicate"></param>
        public void GotoNextBookmark(Predicate<Bookmark> predicate)
        {
            Bookmark mark = this.Document.BookmarkManager.GetNextMark(
                this.TextArea.Caret.Line,
                predicate);
            if (mark != null)
            {
                this.TextArea.Caret.Line = mark.LineNumber;
                this.TextArea.SelectionManager.ClearSelection();
            }
        }

        /// <summary>
        /// 清除所有书签
        /// </summary>
        public void ClearAllBookmarks(Predicate<Bookmark> predicate)
        {
            this.Document.BookmarkManager.RemoveMarks(predicate);
            this.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
            this.Document.CommitUpdate();
        }
        #endregion 书签相关

        #region 基础编辑相关
        /// <summary>
        /// 剪切
        /// </summary>
        public void Cut()
        {
            if (this.TextArea.Document.ReadOnly)
            {
                return;
            }
            this.TextArea.ClipboardHandler.Cut(null, null);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Copy()
        {

            this.TextArea.AutoClearSelection = false;
            this.TextArea.ClipboardHandler.Copy(null, null);
        }
        /// <summary>
        /// 粘贴
        /// </summary>
        public void Paste()
        {
            if (this.TextArea.Document.ReadOnly)
            {
                return;
            }
            this.TextArea.ClipboardHandler.Paste(null, null);
        }
        #endregion 基础编辑相关

        #region 代码指示标记相关

        #endregion 代码指示标记相关

        #region 折叠相关
        /// <summary>
        /// 展开、折叠当前
        /// </summary>
        public void ToggleFolding()
        {
            this.ToggleFolding(this.TextArea.Caret.Line);
        }

        /// <summary>
        /// 切换折叠状态
        /// </summary>
        public void ToggleFolding(int line)
        {
            List<FoldMarker> foldMarkers = this.TextArea.Document.FoldingManager.GetFoldingsWithStart(line);
            foreach (FoldMarker fm in foldMarkers)
            {
                fm.IsFolded = !fm.IsFolded;
            }
            this.TextArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);

        }
        /// <summary>
        /// 展开、折叠所有
        /// </summary>
        public void ToggleAllFoldings()
        {
            bool doFold = true;
            foreach (FoldMarker fm in this.TextArea.Document.FoldingManager.FoldMarker)
            {
                if (fm.IsFolded)
                {
                    doFold = false;
                    break;
                }
            }
            foreach (FoldMarker fm in this.TextArea.Document.FoldingManager.FoldMarker)
            {
                fm.IsFolded = doFold;
            }
            this.TextArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
        }
        #endregion 折叠相关

        #region
        #endregion
    }
}
