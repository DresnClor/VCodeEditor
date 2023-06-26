// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Windows.Forms;
using System.Drawing;

using MeltuiCodeEditor.Document;

namespace MeltuiCodeEditor
{
	/// <summary>
	/// 文本区域更新类型
	/// </summary>
	public enum TextAreaUpdateType {
		/// <summary>
		/// 整个文本区域
		/// </summary>
		WholeTextArea,
		/// <summary>
		/// 单行
		/// </summary>
		SingleLine,
		/// <summary>
		/// 单个位置
		/// </summary>
		SinglePosition,
		/// <summary>
		/// 位置到行结束
		/// </summary>
		PositionToLineEnd,
		/// <summary>
		/// 位置到结束
		/// </summary>
		PositionToEnd,
		/// <summary>
		/// 行之间
		/// </summary>
		LinesBetween
	}

	/// <summary>
	/// 文本区域更新
	/// </summary>
	public class TextAreaUpdate
	{
		Point              position;
		TextAreaUpdateType type;
		
		/// <summary>
		/// 文本区域更新类型
		/// </summary>
		public TextAreaUpdateType TextAreaUpdateType {
			get {
				return type;
			}
		}
		
		/// <summary>
		/// 位置
		/// </summary>
		public Point Position {
			get {
				return position;
			}
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="TextAreaUpdate"/>
		/// </summary>	
		public TextAreaUpdate(TextAreaUpdateType type)
		{
			this.type = type;
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="TextAreaUpdate"/>
		/// </summary>	
		public TextAreaUpdate(TextAreaUpdateType type, Point position)
		{
			this.type     = type;
			this.position = position;
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="TextAreaUpdate"/>
		/// </summary>	
		public TextAreaUpdate(TextAreaUpdateType type, int startLine, int endLine)
		{
			this.type     = type;
			this.position = new Point(startLine, endLine);
		}
		
		/// <summary>
		/// Creates a new instance of <see cref="TextAreaUpdate"/>
		/// </summary>	
		public TextAreaUpdate(TextAreaUpdateType type, int singleLine)
		{
			this.type     = type;
			this.position = new Point(0, singleLine);
		}
		
		public override string ToString()
		{
			return String.Format("[TextAreaUpdate: Type={0}, Position={1}]", type, position);
		}
	}
}
