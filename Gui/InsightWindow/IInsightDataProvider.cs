// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

using VCodeEditor.Document;

namespace VCodeEditor.Gui.InsightWindow
{
	/// <summary>
	/// 洞察数据提供者
	/// </summary>
	public interface IInsightDataProvider
	{
		/// <summary>
		/// 设置数据提供程序
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="textArea"></param>
		void SetupDataProvider(string fileName, TextArea textArea);
		
		/// <summary>
		/// 插入点偏移改变
		/// </summary>
		/// <returns></returns>
		bool CaretOffsetChanged();

		/// <summary>
		/// 字符类型
		/// </summary>
		/// <returns></returns>
		bool CharTyped();
		
		/// <summary>
		/// 获取洞察数据
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		string GetInsightData(int number);

		/// <summary>
		/// 洞察数据数量
		/// </summary>
		int InsightDataCount {
			get;
		}
		
		/// <summary>
		/// 默认索引
		/// </summary>
		int DefaultIndex {
			get;
		}
	}
}
