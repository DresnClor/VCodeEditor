// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 选择接口 <see cref="VCodeEditor.Document.ISelection"/> 默认实现
    /// </summary>
    public class DefaultSelection : ISelection
    {
        IDocument document = null;
        bool isRectangularSelection = false;
        Point startPosition = new Point(-1, -1);
        Point endPosition = new Point(-1, -1);

        /// <summary>
        /// 开始点
        /// </summary>
        public Point StartPosition
        {
            get
            {
                return startPosition;
            }
            set
            {
                startPosition = value;
            }
        }

        /// <summary>
        /// 结束点
        /// </summary>
        public Point EndPosition
        {
            get
            {
                return endPosition;
            }
            set
            {
                endPosition = value;
            }
        }

        /// <summary>
        /// 偏移
        /// </summary>
        public int Offset
        {
            get
            {
                return document.PositionToOffset(startPosition);
            }
        }

        /// <summary>
        /// 结束偏移
        /// </summary>
        public int EndOffset
        {
            get
            {
                return document.PositionToOffset(endPosition);
            }
        }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length
        {
            get
            {
                return EndOffset - Offset;
            }
        }

        /// <value>
        /// 判断选择是否为空
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return startPosition == endPosition;
            }
        }

        /// <value>
        /// 如果选择是矩形的，返回true
        /// </value>
        // TODO : 使用此未使用的属性
        public bool IsRectangularSelection
        {
            get
            {
                return isRectangularSelection;
            }
            set
            {
                isRectangularSelection = value;
            }
        }

        /// <value>
        /// 获取选择的文本
        /// </value>
        public string SelectedText
        {
            get
            {
                if (document != null)
                {
                    if (Length < 0)
                    {
                        return null;
                    }
                    return document.GetText(Offset, Length);
                }
                return null;
            }
        }

        /// <summary>
        /// 创建新实例 <see cref="DefaultSelection"/>
        /// </summary>	
        public DefaultSelection(IDocument document, Point startPosition, Point endPosition)
        {
            this.document = document;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
        }

        /// <summary>
        /// 转换  <see cref="DefaultSelection"/> 实例到字符串（用于解试目的）
        /// </summary>
        public override string ToString()
        {
            return String.Format("[DefaultSelection : StartPosition={0}, EndPosition={1}]", startPosition, endPosition);
        }
        /// <summary>
        /// 是否包含指定点
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool ContainsPosition(Point position)
        {
            return startPosition.Y < position.Y && position.Y < endPosition.Y ||
                   startPosition.Y == position.Y && startPosition.X <= position.X && (startPosition.Y != endPosition.Y || position.X <= endPosition.X) ||
                   endPosition.Y == position.Y && startPosition.Y != endPosition.Y && position.X <= endPosition.X;
        }

        /// <summary>
        /// 是否包含指定偏移
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool ContainsOffset(int offset)
        {
            return Offset <= offset && offset <= EndOffset;
        }
    }
}
