// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 段落信息接口
	/// </summary>
	public interface ISegment
	{
		/// <value>
		/// 开始偏移
		/// </value>
		int Offset {
			get;
			set;
		}
		
		/// <value>
		/// 长度
		/// </value>
		int Length {
			get;
			set;
		}
	}
	
}
