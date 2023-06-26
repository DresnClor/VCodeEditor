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
	/// ������ʾ�����¼�
	/// </summary>
	public class ToolTipRequestEventArgs
	{
		Point mousePosition;
		Point logicalPosition;
		bool inDocument;
		
		/// <summary>
		/// ���λ��
		/// </summary>
		public Point MousePosition {
			get {
				return mousePosition;
			}
		}
		
		/// <summary>
		/// �߼�λ��
		/// </summary>
		public Point LogicalPosition {
			get {
				return logicalPosition;
			}
		}

		/// <summary>
		/// ���ĵ���
		/// </summary>
		public bool InDocument {
			get {
				return inDocument;
			}
		}
		
		/// <summary>
		/// ���������¼���ĳ���ͻ����Ѿ���ʾ�˹�����ʾ�����ȡ��
		/// </summary>
		public bool ToolTipShown {
			get {
				return toolTipText != null;
			}
		}
		
		internal string toolTipText;
		
		/// <summary>
		/// ��ʾ������ʾ
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
