// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections.Generic;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 计算折叠级别的简单折叠策略 行缩进级别。
	/// </summary>
	public class IndentFoldingStrategy : IFoldingStrategy
	{
		public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
		{
			List<FoldMarker> l = new List<FoldMarker>();
			Stack<int> offsetStack = new Stack<int>();
			Stack<string> textStack = new Stack<string>();
			//int level = 0;
			//foreach (LineSegment segment in document.LineSegmentCollection) {
			//	
			//}
			return l;
		}
		
		int GetLevel(IDocument document, int offset)
		{
			int level = 0;
			int spaces = 0;
			for (int i = offset; i < document.TextLength; ++i) {
				char c = document.GetCharAt(i);
				if (c == '\t' || (c == ' ' && ++spaces == 4)) {
					spaces = 0;
					++level;
				} else {
					break;
				}
			}
			return level;
		}
	}
}
