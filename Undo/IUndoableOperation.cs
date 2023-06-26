// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

namespace MeltuiCodeEditor.Undo
{
	/// <summary>
	/// 此界面描述了基本的撤消/重做操作，所有撤消操作必须实现此接口
	/// </summary>
	public interface IUndoableOperation
	{
		/// <summary>
		/// 撤消最后一次操作
		/// </summary>
		void Undo();
		
		/// <summary>
		/// 重做了最后一次手术
		/// </summary>
		void Redo();
	}
}
