// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="none" email=""/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;

namespace MeltuiCodeEditor.Util
{
	/// <summary>
	/// 提示绘制工具
	/// </summary>
	class TipPainterTools
	{
		const int SpacerSize = 4;
		
		private TipPainterTools()
		{
			
		}
		public static Size GetDrawingSizeHelpTipFromCombinedDescription(Control control,
		                                                      Graphics graphics,
		                                                      Font font,
		                                                      string countMessage,
		                                                      string description)
		{
			string basicDescription = null;
			string documentation = null;

			if (IsVisibleText(description)) {
	     		string[] splitDescription = description.Split(new char[] { '\n' }, 2);
						
				if (splitDescription.Length > 0) {
					basicDescription = splitDescription[0];
					
					if (splitDescription.Length > 1) {
						documentation = splitDescription[1].Trim();
					}
				}
			}
			
			return GetDrawingSizeDrawHelpTip(control, graphics, font, countMessage, basicDescription, documentation);
		}
		
		public static Size DrawHelpTipFromCombinedDescription(Control control,
		                                                      Graphics graphics,
		                                                      Font font,
		                                                      string countMessage,
		                                                      string description)
		{
			string basicDescription = null;
			string documentation = null;

			if (IsVisibleText(description)) {
	     		string[] splitDescription = description.Split
	     			(new char[] { '\n' }, 2);
						
				if (splitDescription.Length > 0) {
					basicDescription = splitDescription[0];
					
					if (splitDescription.Length > 1) {
						documentation = splitDescription[1].Trim();
					}
				}
			}
			
			return DrawHelpTip(control, graphics, font, countMessage,
			            basicDescription, documentation);
 		}
		
		// btw. I know it's ugly.顺便 说 一 句。我知道它很丑
		public static Rectangle DrawingRectangle1;
		public static Rectangle DrawingRectangle2;
		
		public static Size GetDrawingSizeDrawHelpTip(Control control,
		                               Graphics graphics, Font font,
		                               string countMessage,
		                               string basicDescription,
		                               string documentation)
		{
			if (IsVisibleText(countMessage)     ||
			    IsVisibleText(basicDescription) ||
			    IsVisibleText(documentation)) {
				// 创建所有提示选件对象。
				CountTipText countMessageTip = new CountTipText(graphics, font, countMessage);
				
				TipSpacer countSpacer = new TipSpacer(graphics, new SizeF(IsVisibleText(countMessage) ? 4 : 0, 0));
				
				TipText descriptionTip = new TipText(graphics, font, basicDescription);
				
				TipSpacer docSpacer = new TipSpacer(graphics, new SizeF(0, IsVisibleText(documentation) ? 4 : 0));
				
				TipText docTip = new TipText(graphics, font, documentation);
				
				// 现在把它们放在一起。
				TipSplitter descSplitter = new TipSplitter(graphics, false,
				                                           descriptionTip,
				                                           docSpacer
				                                           );
				
				TipSplitter mainSplitter = new TipSplitter(graphics, true,
				                                           countMessageTip,
				                                           countSpacer,
				                                           descSplitter);
				
				TipSplitter mainSplitter2 = new TipSplitter(graphics, false,
				                                           mainSplitter,
				                                           docTip);
				
				// 显示它。
				Size size = TipPainter.GetTipSize(control, graphics, mainSplitter2);
				DrawingRectangle1 = countMessageTip.DrawingRectangle1;
				DrawingRectangle2 = countMessageTip.DrawingRectangle2;
				return size;
			}
			return Size.Empty;
		}
		public static Size DrawHelpTip(Control control,
		                               Graphics graphics, Font font,
		                               string countMessage,
		                               string basicDescription,
		                               string documentation)
		{
			if (IsVisibleText(countMessage)     ||
			    IsVisibleText(basicDescription) ||
			    IsVisibleText(documentation)) {
				// 创建所有提示选件对象。
				CountTipText countMessageTip = new CountTipText(graphics, font, countMessage);
				
				TipSpacer countSpacer = new TipSpacer(graphics, new SizeF(IsVisibleText(countMessage) ? 4 : 0, 0));
				
				TipText descriptionTip = new TipText(graphics, font, basicDescription);
				
				TipSpacer docSpacer = new TipSpacer(graphics, new SizeF(0, IsVisibleText(documentation) ? 4 : 0));
				
				TipText docTip = new TipText(graphics, font, documentation);
				
				// 现在把它们放在一起。
				TipSplitter descSplitter = new TipSplitter(graphics, false,
				                                           descriptionTip,
				                                           docSpacer
				                                           );
				
				TipSplitter mainSplitter = new TipSplitter(graphics, true,
				                                           countMessageTip,
				                                           countSpacer,
				                                           descSplitter);
				
				TipSplitter mainSplitter2 = new TipSplitter(graphics, false,
				                                           mainSplitter,
				                                           docTip);
				
				// Show it.
				Size size = TipPainter.DrawTip(control, graphics, mainSplitter2);
				DrawingRectangle1 = countMessageTip.DrawingRectangle1;
				DrawingRectangle2 = countMessageTip.DrawingRectangle2;
				return size;
			}
			return Size.Empty;
		}
		
		static bool IsVisibleText(string text)
		{
			return text != null && text.Length > 0;
		}
	}
}
