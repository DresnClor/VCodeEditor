using System;
using System.Collections.Generic;
using VCodeEditor.Document;

//代码折叠相关

namespace VCodeEditor.Actions
{
	/// <summary>
	/// 展开、折叠当前
	/// </summary>
	public class ToggleFolding : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			List<FoldMarker> foldMarkers = textArea.Document.FoldingManager.GetFoldingsWithStart(textArea.Caret.Line);
			foreach (FoldMarker fm in foldMarkers) {
				fm.IsFolded = !fm.IsFolded;
			}
			textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}
	}

	/// <summary>
	/// 展开、折叠所有
	/// </summary>
	public class ToggleAllFoldings : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			bool doFold = true;
			foreach (FoldMarker fm in  textArea.Document.FoldingManager.FoldMarker) {
				if (fm.IsFolded) {
					doFold = false;
					break;
				}
			}
			foreach (FoldMarker fm in  textArea.Document.FoldingManager.FoldMarker) {
				fm.IsFolded = doFold;
			}
			textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}
	}

	/// <summary>
	/// 仅显示定义
	/// </summary>
	public class ShowDefinitionsOnly : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			foreach (FoldMarker fm in  textArea.Document.FoldingManager.FoldMarker) {
				fm.IsFolded = fm.FoldType == FoldType.MemberBody || fm.FoldType == FoldType.Region;
			}
			textArea.Document.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
		}
	}
}
