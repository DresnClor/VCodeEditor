// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Collections.Generic;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 折叠策略
	/// </summary>
	public interface IFoldingStrategy
	{
		/// <remarks>
		/// 生成折叠标记
		/// </remarks>
		List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation);
	}
}
