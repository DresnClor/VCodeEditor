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
using VCodeEditor;

namespace VCodeEditor.Gui.CompletionWindow
{
	/// <summary>
	/// 自动完成数据接口
	/// </summary>
	public interface ICompletionData : IComparable
	{
		/// <summary>
		/// 图片索引
		/// </summary>
		int ImageIndex {
			get;
		}
		
		/// <summary>
		/// 文本
		/// </summary>
		string Text {
			get;
			set;
		}
		
		/// <summary>
		/// 说明
		/// </summary>
		string Description {
			get;
		}

		/// <summary>
		/// 获取完成数据项目的优先值。按其开始字符选择项目时，具有最高优先级的项首先选择。
		/// </summary>
		double Priority {
			get;
		}

        /// <summary>
        /// 将完成数据表示的元素插入文本编辑器。
        /// </summary>
        /// <param name="textArea">.文本区域插入完成数据</param>
        /// <param name="ch">完成数据后应插入的字符。
        /// \0 当没有字符应插入</param>
        /// <returns>当插入动作处理字符时返回真实
        /// <paramref name="ch"/>; 当字符未处理时为错误.</returns>
        bool InsertAction(TextArea textArea, char ch);
	}

	/// <summary>
	/// 默认自动完成数据实现
	/// </summary>
	public class DefaultCompletionData : ICompletionData
	{
		string text;
		string description;
		int imageIndex;
		
		public int ImageIndex {
			get {
				return imageIndex;
			}
		}
		
		public string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}
		
		public string Description {
			get {
				return description;
			}
		}
		
		double priority;
		
		public double Priority {
			get {
				return priority;
			}
			set {
				priority = value;
			}
		}
		
		public virtual bool InsertAction(TextArea textArea, char ch)
		{
			textArea.InsertString(text);
			return false;
		}
		
		public DefaultCompletionData(string text, string description, int imageIndex)
		{
			this.text        = text;
			this.description = description;
			this.imageIndex  = imageIndex;
		}
		
		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is DefaultCompletionData)) {
				return -1;
			}
			return text.CompareTo(((DefaultCompletionData)obj).Text);
		}
	}
}
