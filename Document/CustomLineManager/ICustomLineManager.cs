// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Ivo Kovacka" email="ivok@internet.sk"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Drawing;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// 自定义行管理器
	/// </summary>
	public interface ICustomLineManager
	{
		/// <value>
		/// 包含所有自定义行
		/// </value>
		ArrayList CustomLines {
			get;
		}
		
		/// <remarks>
		/// 返回颜色，如果<code>lineNr</code> 有自定义bg颜色
		/// 否则返回 <code>defaultColor</code>
		/// </remarks>
		Color GetCustomColor(int lineNr, Color defaultColor);
		
		/// <remarks>
		/// 返回真，如果行 <code>lineNr</code> 仅读取
		/// </remarks>
		bool IsReadOnly(int lineNr, bool defaultReadOnly);

		/// <remarks>
		/// 返回真，如果 <code>selection</code> 仅读取
		/// </remarks>
		bool IsReadOnly(ISelection selection, bool defaultReadOnly);

		/// <remarks>
		/// 在行中添加自定义行<code>lineNr</code>
		/// </remarks>
		void AddCustomLine(int lineNr, Color customColor, bool readOnly);
		
		/// <remarks>
		/// 从行中添加自定义行<code>startLineNr</code> 到 <code>endLineNr</code>
		/// </remarks>
		void AddCustomLine(int startLineNr,  int endLineNr, Color customColor, bool readOnly);
		
		/// <remarks>
		/// 在行中删除自定义行 <code>lineNr</code>
		/// </remarks>
		void RemoveCustomLine(int lineNr);
		
		/// <remarks>
		/// 清除所有自定义色线
		/// </remarks>
		void Clear();
		
		/// <remarks>
		/// 更改
		/// </remarks>
		event EventHandler BeforeChanged;

		/// <remarks>
		/// 更改
		/// </remarks>
		event EventHandler Changed;
	}
}
