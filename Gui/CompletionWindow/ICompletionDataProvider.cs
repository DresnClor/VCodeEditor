// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1105 $</version>
// </file>

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

using MeltuiCodeEditor.Document;

namespace MeltuiCodeEditor.Gui.CompletionWindow
{
	/// <summary>
	/// 自动完成数据提供者
	/// </summary>
	public interface ICompletionDataProvider
	{
		/// <summary>
		/// 图片列表
		/// </summary>
		ImageList ImageList {
			get;
		}
		/// <summary>
		/// 
		/// </summary>
		string PreSelection {
			get;
		}
		/// <summary>
		/// 获取默认选择的列表中的元素索引。
		/// </summary>
		int DefaultIndex {
			get;
		}
		/// <summary>
		/// 如果空间应插入完成的表达式前面，则获取/设置。
		/// </summary>
		bool InsertSpace {
			get;
			set;
		}
		/// <summary>
		/// 如果按下"键"应触发当前选定元素的插入，则获取。
		/// </summary>
		bool IsInsertionKey(char key);
		
		/// <summary>
		/// 生成完成数据。此方法由文本编辑器控制调用。
		/// </summary>
		ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped);
	}

	/// <summary>
	/// 默认自动完成数据提供者接口实现
	/// </summary>
    public class CompletionDataProvider : ICompletionDataProvider
    {
		/// <param name="completionDatas">补全项数据</param>
		public CompletionDataProvider(ICompletionData[] completionDatas)
		{
			ImageList = new ImageList();
			this.CompletionDatas = completionDatas;
		}
		public ImageList ImageList
        {
            get;
        } 
		public string PreSelection
        {
            get;
        } = "";
		public int DefaultIndex
		{
			get;
		} = 0;
        public bool InsertSpace
        {
            get;
            set;
        } = false;

		public ICompletionData[] CompletionDatas
		{
			get;
		}

		public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
		{
			return this.CompletionDatas;
		}
		public bool IsInsertionKey(char key)

		{
			return false;
		}
    }
}
