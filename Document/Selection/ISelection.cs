// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System.Drawing;
using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 文本内容选择接口
    /// </summary>
    public interface ISelection
    {
        /// <summary>
        /// 开始位置
        /// </summary>
        Point StartPosition
        {
            get;
            set;
        }

        /// <summary>
        /// 结束位置
        /// </summary>
        Point EndPosition
        {
            get;
            set;
        }

        /// <summary>
        /// 偏移
        /// </summary>
        int Offset
        {
            get;
        }

        /// <summary>
        /// 结束偏移
        /// </summary>
        int EndOffset
        {
            get;
        }

        /// <summary>
        /// 长度
        /// </summary>
        int Length
        {
            get;
        }

        /// <value>
        /// 如果选择是矩形的，返回true
        /// </value>
        bool IsRectangularSelection
        {
            get;
        }

        /// <value>
        /// 选择是否为空
        /// </value>
        bool IsEmpty
        {
            get;
        }

        /// <value>
        /// 选择的文本
        /// </value>
        string SelectedText
        {
            get;
        }

        /// <summary>
        /// 是否包含指定偏移
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        bool ContainsOffset(int offset);

        /// <summary>
        /// 是否包含指定点
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool ContainsPosition(Point position);
    }
}
