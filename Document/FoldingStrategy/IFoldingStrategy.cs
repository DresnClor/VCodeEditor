// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Collections.Generic;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 此界面用于折叠功能 文本区域。
	/// </summary>
	public interface IFoldingStrategy
	{
		/// <remarks>
		/// 计算特定行的折叠级别。
		/// </remarks>
		List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation);
	}
}
