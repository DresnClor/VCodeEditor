// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;

namespace MeltuiCodeEditor
{
	public delegate void ToolTipRequestEventHandler(object sender, ToolTipRequestEventArgs e);
	
	/// <summary>
	/// 工具提示请求事件
	/// </summary>
	public class ToolTipRequestEventArgs
	{
		Point mousePosition;
		Point logicalPosition;
		bool inDocument;
		
		/// <summary>
		/// 鼠标位置
		/// </summary>
		public Point MousePosition {
			get {
				return mousePosition;
			}
		}
		
		/// <summary>
		/// 逻辑位置
		/// </summary>
		public Point LogicalPosition {
			get {
				return logicalPosition;
			}
		}

		/// <summary>
		/// 在文档中
		/// </summary>
		public bool InDocument {
			get {
				return inDocument;
			}
		}
		
		/// <summary>
		/// 如果处理该事件的某个客户端已经显示了工具提示，则获取。
		/// </summary>
		public bool ToolTipShown {
			get {
				return toolTipText != null;
			}
		}
		
		internal string toolTipText;
		
		/// <summary>
		/// 显示工具提示
		/// </summary>
		/// <param name="text"></param>
		public void ShowToolTip(string text)
		{
			toolTipText = text;
		}
		
		public ToolTipRequestEventArgs(Point mousePosition, Point logicalPosition, bool inDocument)
		{
			this.mousePosition = mousePosition;
			this.logicalPosition = logicalPosition;
			this.inDocument = inDocument;
		}
	}
}
