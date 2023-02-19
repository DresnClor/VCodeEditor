// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="none" email=""/>
//     <version>$Revision: 1118 $</version>
// </file>

using System;
using System.Drawing;
using System.Text;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 编辑器属性接口
    /// </summary>
    public interface ITextEditorProperties
    {
        /// <summary>
        /// 自动插入花括号
        /// </summary>
        bool AutoInsertCurlyBracket
        {
            get;
            set;
        }

        /// <summary>
        /// 隐藏鼠标指针
        /// </summary>
        bool HideMouseCursor
        {
            get;
            set;
        }

        /// <summary>
        /// 图标栏是否可见
        /// </summary>
        bool IsIconBarVisible
        {
            get;
            set;
        }

        /// <summary>
        /// 断点栏是否可见
        /// </summary>
        bool IsBreakpointBarVisible
        {
            get;
            set;
        }

        /// <summary>
        /// 允许插入符号超出EOL
        /// </summary>
        bool AllowCaretBeyondEOL
        {
            get;
            set;
        }

        /// <summary>
        /// 显示匹配括号
        /// </summary>
        bool ShowMatchingBracket
        {
            get;
            set;
        }

        /// <summary>
        /// 允许剪切或复制整行
        /// </summary>
        bool CutCopyWholeLine
        {
            get;
            set;
        }

        /// <summary>
        /// 使用抗锯齿字体
        /// </summary>
        bool UseAntiAliasedFont
        {
            get;
            set;
        }

        /// <summary>
        /// 允许鼠标滚轮向下滚动
        /// </summary>
        bool MouseWheelScrollDown
        {
            get;
            set;
        }

        /// <summary>
        /// 允许鼠标滚轮缩放文本
        /// </summary>
        bool MouseWheelTextZoom
        {
            get;
            set;
        }

        /// <summary>
        /// 行终止符
        /// </summary>
        string LineTerminator
        {
            get;
            set;
        }

        /// <summary>
        /// 创建备份副本
        /// </summary>
        bool CreateBackupCopy
        {
            get;
            set;
        }

        /// <summary>
        /// 行视图样式
        /// </summary>
        LineViewerStyle LineViewerStyle
        {
            get;
            set;
        }

        /// <summary>
        /// 显示无效行
        /// </summary>
        bool ShowInvalidLines
        {
            get;
            set;
        }

        /// <summary>
        /// 垂直标尺行
        /// </summary>
        int VerticalRulerRow
        {
            get;
            set;
        }

        /// <summary>
        /// 显示空白字符
        /// </summary>
        bool ShowSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// 显示tab字符
        /// </summary>
        bool ShowTabs
        {
            get;
            set;
        }

        /// <summary>
        /// 显示EOL标记
        /// </summary>
        bool ShowEOLMarker
        {
            get;
            set;
        }

        /// <summary>
        /// 转换tab为空格
        /// </summary>
        bool ConvertTabsToSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// 显示水平标尺
        /// </summary>
        bool ShowHorizontalRuler
        {
            get;
            set;
        }

        /// <summary>
        /// 显示垂直标尺
        /// </summary>
        bool ShowVerticalRuler
        {
            get;
            set;
        }

        /// <summary>
        /// 编码
        /// </summary>
        Encoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// 启用折叠
        /// </summary>
        bool EnableFolding
        {
            get;
            set;
        }

        /// <summary>
        /// 显示行号
        /// </summary>
        bool ShowLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// tab缩进宽度
        /// </summary>
        int TabIndent
        {
            get;
            set;
        }

        /// <summary>
        /// 缩进样式
        /// </summary>
        IndentStyle IndentStyle
        {
            get;
            set;
        }

        /// <summary>
        /// 文档选择模式
        /// </summary>
        DocumentSelectionMode DocumentSelectionMode
        {
            get;
            set;
        }

        /// <summary>
        /// 字体
        /// </summary>
        Font Font
        {
            get;
            set;
        }

        /// <summary>
        /// 括号匹配样式
        /// </summary>
        BracketMatchingStyle BracketMatchingStyle
        {
            get;
            set;
        }

        /// <summary>
        /// 使用自定义行
        /// </summary>
        bool UseCustomLine
        {
            get;
            set;
        }
    }
}
