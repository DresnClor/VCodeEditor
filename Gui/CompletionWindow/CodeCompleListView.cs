// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;

namespace VCodeEditor.Gui.CompletionWindow
{
	/// <summary>
	/// 代码自动完成列表视图
	/// </summary>
	public class CodeCompleListView : System.Windows.Forms.UserControl
	{
		ICompletionData[] completionData;
		int               firstItem    = 0;
		int               selectedItem = -1;
		ImageList         imageList;

        public CodeCompleListView(ICompletionData[] completionData)
        {
            Array.Sort(completionData);
            this.completionData = completionData;

            //			this.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKey);
            //			SetStyle(ControlStyles.Selectable, false);
            //			SetStyle(ControlStyles.UserPaint, true);
            //			SetStyle(ControlStyles.DoubleBuffer, false);
        }


        /// <summary>
        /// 图片列表
        /// </summary>
        public ImageList ImageList {
			get {
				return imageList;
			}
			set {
				imageList = value;
			}
		}
		
		/// <summary>
		/// 第一项
		/// </summary>
		public int FirstItem {
			get {
				return firstItem;
			}
			set {
				firstItem = value;
				OnFirstItemChanged(EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// 选择自动完成数据
		/// </summary>
		public ICompletionData SelectedCompletionData {
			get {
				if (selectedItem < 0) {
					return null;
				}
				return completionData[selectedItem];
			}
		}
		
		/// <summary>
		/// 项高度
		/// </summary>
		public int ItemHeight {
			get {
				return Math.Max(imageList.ImageSize.Height, (int)(Font.Height * 1.25));
			}
		}
		
		/// <summary>
		/// 最大可视项
		/// </summary>
		public int MaxVisibleItem {
			get {
				return Height / ItemHeight;
			}
		}
		/// <summary>
		/// 关闭
		/// </summary>
		public void Close()
		{
			if (completionData != null) {
				Array.Clear(completionData, 0, completionData.Length);
			}
			base.Dispose();
		}
		
		/// <summary>
		/// 选择索引
		/// </summary>
		/// <param name="index"></param>
		public void SelectIndex(int index)
		{
			int oldSelectedItem = selectedItem;
			int oldFirstItem    = firstItem;
			
			index = Math.Max(0, index);
			selectedItem = Math.Max(0, Math.Min(completionData.Length - 1, index));
			if (selectedItem < firstItem) {
				FirstItem = selectedItem;
			}
			if (firstItem + MaxVisibleItem <= selectedItem) {
				FirstItem = selectedItem - MaxVisibleItem + 1;
			}
			if (oldSelectedItem != selectedItem) {
				if (firstItem != oldFirstItem) {
					Invalidate();
				} else {
					int min = Math.Min(selectedItem, oldSelectedItem) - firstItem;
					int max = Math.Max(selectedItem, oldSelectedItem) - firstItem;
					Invalidate(new Rectangle(0, 1 + min * ItemHeight, Width, (max - min + 1) * ItemHeight));
				}
				Update();
				OnSelectedItemChanged(EventArgs.Empty);
			}
		}
		
		/// <summary>
		/// 清除选择
		/// </summary>
		public void ClearSelection()
		{
			if (selectedItem < 0)
				return;
			int itemNum = selectedItem - firstItem;
			selectedItem = -1;
			Invalidate(new Rectangle(0, itemNum * ItemHeight, Width, (itemNum + 1) * ItemHeight + 1));
			Update();
			OnSelectedItemChanged(EventArgs.Empty);
		}
		
		/// <summary>
		/// 页面按下
		/// </summary>
		public void PageDown()
		{
			SelectIndex(selectedItem + MaxVisibleItem);
		}
		
		/// <summary>
		/// 页面弹起
		/// </summary>
		public void PageUp()
		{
			SelectIndex(selectedItem - MaxVisibleItem);
		}
		
		/// <summary>
		/// 选择下一项
		/// </summary>
		public void SelectNextItem()
		{
			SelectIndex(selectedItem + 1);
		}
		
		/// <summary>
		/// 选择上一项
		/// </summary>
		public void SelectPrevItem()
		{
			SelectIndex(selectedItem - 1);
		}

		/// <summary>
		/// 选择开始的项
		/// </summary>
		/// <param name="startText"></param>
		public void SelectItemWithStart(string startText)
		{
			if (startText == null || startText.Length == 0) return;
			string originalStartText = startText;
			startText = startText.ToLower();
			int bestIndex = -1;
			int bestQuality = -1;
			// Qualities: 0 = 比赛开始 match start
			//            1 = match start case sensitive
			//            2 = full match
			//            3 = full match case sensitive
			double bestPriority = 0;
			for (int i = 0; i < completionData.Length; ++i) {
				string itemText = completionData[i].Text;
				string lowerText = itemText.ToLower();
				if (lowerText.StartsWith(startText)) {
					double priority = completionData[i].Priority;
					int quality;
					if (lowerText == startText) {
						if (itemText == originalStartText)
							quality = 3;
						else
							quality = 2;
					} else if (itemText.StartsWith(originalStartText)) {
						quality = 1;
					} else {
						quality = 0;
					}
					bool useThisItem;
					if (bestQuality < quality) {
						useThisItem = true;
					} else {
						if (bestIndex == selectedItem) {
							useThisItem = false;
						} else if (i == selectedItem) {
							useThisItem = bestQuality == quality;
						} else {
							useThisItem = bestQuality == quality && bestPriority < priority;
						}
					}
					if (useThisItem) {
						bestIndex = i;
						bestPriority = priority;
						bestQuality = quality;
					}
				}
			}
			if (bestIndex < 0)
				ClearSelection();
			else
				SelectIndex(bestIndex);
		}
		
		protected override void OnPaint(PaintEventArgs pe)
		{
			float yPos       = 1;
			float itemHeight = ItemHeight;
			// 保持纵横比
			int imageWidth = (int)(itemHeight * imageList.ImageSize.Width / imageList.ImageSize.Height);
			
			int curItem = firstItem;
			Graphics g  = pe.Graphics;
			while (curItem < completionData.Length && yPos < Height) {
				RectangleF drawingBackground = new RectangleF(1, yPos, Width - 2, itemHeight);
				if (drawingBackground.IntersectsWith(pe.ClipRectangle)) {
					//绘制背景
					if (curItem == selectedItem) {
						g.FillRectangle(SystemBrushes.Highlight, drawingBackground);
					} else {
						g.FillRectangle(SystemBrushes.Window, drawingBackground);
					}
					
					// 绘制图标
					int   xPos   = 0;
					if (imageList != null && completionData[curItem].ImageIndex < imageList.Images.Count) {
						g.DrawImage(imageList.Images[completionData[curItem].ImageIndex], new RectangleF(1, yPos, imageWidth, itemHeight));
						xPos = imageWidth;
					}
					
					// 绘制文本
					if (curItem == selectedItem) {
						g.DrawString(completionData[curItem].Text, Font, SystemBrushes.HighlightText, xPos, yPos);
					} else {
						g.DrawString(completionData[curItem].Text, Font, SystemBrushes.WindowText, xPos, yPos);
					}
				}
				
				yPos += itemHeight;
				++curItem;
			}
			g.DrawRectangle(SystemPens.Control, new Rectangle(0, 0, Width - 1, Height - 1));
		}
		
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
		{
			float yPos       = 1;
			int curItem = firstItem;
			float itemHeight = ItemHeight;
			
			while (curItem < completionData.Length && yPos < Height) {
				RectangleF drawingBackground = new RectangleF(1, yPos, Width - 2, itemHeight);
				if (drawingBackground.Contains(e.X, e.Y)) {
					SelectIndex(curItem);
					break;
				}
				yPos += itemHeight;
				++curItem;
			}
		}
		
		protected override void OnMouseWheel(MouseEventArgs mea)
		{
			int numberOfLines = mea.Delta * SystemInformation.MouseWheelScrollLines / 120;
			//BeginUpdate();
			while (numberOfLines>0) {
				SelectPrevItem();
				numberOfLines--;
			}
			while (numberOfLines<0) {
				SelectNextItem();
				numberOfLines++;
			}
			//EndUpdate();
		}
		
		protected override void OnPaintBackground(PaintEventArgs pe)
		{
		}
		
		protected virtual void OnSelectedItemChanged(EventArgs e)
		{
			if (SelectedItemChanged != null) {
				SelectedItemChanged(this, e);
			}
		}
		
		protected virtual void OnFirstItemChanged(EventArgs e)
		{
			if (FirstItemChanged != null) {
				FirstItemChanged(this, e);
			}
		}
		
		public event EventHandler SelectedItemChanged;
		public event EventHandler FirstItemChanged;
	}
}
