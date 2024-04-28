using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCodeEditor;
using VCodeEditor.Document;
using VCodeEditor.Gui;


namespace VCodeEditor.Actions
{
    /// <summary>
    /// 代码编辑动作
    /// </summary>
    public class EditAction
    {
        public EditAction(TextArea textArea)
        {
            TextArea = textArea;
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
            get => TextArea.Document;
        }

        #region 书签相关
        /// <summary>
        /// 切换当前行书签
        /// </summary>
        public void ToggleBookmark()
        {

            ToggleBookmark(TextArea.Caret.Line);
        }
        /// <summary>
        /// 切换当前行书签
        /// </summary>
        /// <param name="line">行号</param>
        public void ToggleBookmark(int line)
        {
            Document.BookmarkManager.ToggleMarkAt(line);
            Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, line));
            Document.CommitUpdate();
        }

        /// <summary>
        /// 转到上一个书签
        /// </summary>
        /// <param name="predicate"></param>
        public void GotoPrevBookmark(Predicate<Bookmark> predicate)
        {
            Bookmark mark = Document.BookmarkManager.GetPrevMark(
                TextArea.Caret.Line,
                predicate);
            if (mark != null)
            {
                TextArea.Caret.Line = mark.LineNumber;
                TextArea.SelectionManager.ClearSelection();
            }
        }

        /// <summary>
        /// 转到下一个书签
        /// </summary>
        /// <param name="predicate"></param>
        public void GotoNextBookmark(Predicate<Bookmark> predicate)
        {
            Bookmark mark = Document.BookmarkManager.GetNextMark(
                TextArea.Caret.Line,
                predicate);
            if (mark != null)
            {
                TextArea.Caret.Line = mark.LineNumber;
                TextArea.SelectionManager.ClearSelection();
            }
        }

        /// <summary>
        /// 清除所有书签
        /// </summary>
        public void ClearAllBookmarks(Predicate<Bookmark> predicate)
        {
            Document.BookmarkManager.RemoveMarks(predicate);
            Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
            Document.CommitUpdate();
        }
        #endregion 书签相关

        #region 基础编辑相关
        /// <summary>
        /// 剪切
        /// </summary>
        public void Cut()
        {
            if (TextArea.Document.ReadOnly)
            {
                return;
            }
            TextArea.ClipboardHandler.Cut(null, null);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Copy()
        {

            TextArea.AutoClearSelection = false;
            TextArea.ClipboardHandler.Copy(null, null);
        }
        /// <summary>
        /// 粘贴
        /// </summary>
        public void Paste()
        {
            if (TextArea.Document.ReadOnly)
            {
                return;
            }
            TextArea.ClipboardHandler.Paste(null, null);
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
            ToggleFolding(TextArea.Caret.Line);
        }

        /// <summary>
        /// 切换折叠状态
        /// </summary>
        public void ToggleFolding(int line)
        {
            List<FoldMarker> foldMarkers = TextArea.Document.FoldingManager.GetFoldingsWithStart(line);
            foreach (FoldMarker fm in foldMarkers)
            {
                fm.IsFolded = !fm.IsFolded;
            }
            TextArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);

        }
        /// <summary>
        /// 展开、折叠所有
        /// </summary>
        public void ToggleAllFoldings()
        {
            bool doFold = true;
            foreach (FoldMarker fm in TextArea.Document.FoldingManager.FoldMarker)
            {
                if (fm.IsFolded)
                {
                    doFold = false;
                    break;
                }
            }
            foreach (FoldMarker fm in TextArea.Document.FoldingManager.FoldMarker)
            {
                fm.IsFolded = doFold;
            }
            TextArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
        }
        #endregion 折叠相关

        #region
        #endregion
    }
}
