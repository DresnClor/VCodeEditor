using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCodeEditor.Document;

namespace VCodeEditor.FoldStrategies
{
	/// <summary>
	/// C#折叠策略
	/// </summary>
	public class CSharpFoldStrategy : IFoldingStrategy
	{
		/// <summary>
		/// 生成代码折叠,对花括号{}，#region等的处理
		/// </summary>
		/// <param name="document">当前文档</param>
		/// <param name="fileName">文档名</param>
		/// <param name="parseInformation">Extra parse information, not used in this sample.</param>
		/// <returns>A list of FoldMarkers.</returns>
		public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
		{
			List<FoldMarker> list = new List<FoldMarker>();
			// Stack<FuncName> funName = new Stack<FuncName>();//函数名称
			Stack<int> startLines = new Stack<int>();
			for (int i = 0; i < document.TotalNumberOfLines; i++)
			{
				// 当前行文本
				string text = document.GetText(document.GetLineSegment(i));
				if (text.Trim().StartsWith("#region"))
				{
					startLines.Push(i);
				}
				if (text.Trim().StartsWith("#endregion"))
				{
					int start = startLines.Pop();
					list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, 57, FoldType.Region, "..."));
				}
				//支持嵌套 {}
				if (text.Trim().IndexOf("{") != -1)
				{
					startLines.Push(i);
				}
				if (text.Trim().StartsWith("}"))
				{
					if (startLines.Count > 0)
					{
						int start = startLines.Pop();
						list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, 57, FoldType.TypeBody, "...}"));
					}
				}

				// /// <summary>
				if (text.Trim().StartsWith("/// <summary>")) // Look for method starts
				{
					startLines.Push(i);
				}
				if (text.Trim().StartsWith("/// <returns>")) // Look for method endings
				{
					int start = startLines.Pop();
					//获取注释文本（包括空格）
					string display = document.GetText(document.GetLineSegment(start + 1).Offset, document.GetLineSegment(start + 1).Length);
					//remove ///
					display = display.Trim().TrimStart('/');
					list.Add(new FoldMarker(document, start, document.GetLineSegment(start).Length, i, 57, FoldType.TypeBody, display));
				}
			}
			return list;
		}
	}
}
