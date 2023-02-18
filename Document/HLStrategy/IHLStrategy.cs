// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VCodeEditor.Document
{
    /// <summary>
    /// A highlighting strategy for a buffer.
    /// 高亮显示策略接口
    /// </summary>
    public interface IHLStrategy
    {
        /// <value>
        /// 高亮显示策略的名称必须唯一
        /// </value>
        string Name
        {
            get;
        }

        /// <value>
        /// 使用此高亮显示策略的文件扩展名
        /// </value>
        string[] Extensions
        {
            set;
            get;
        }

        /// <summary>
        /// 属性表
        /// </summary>
        Dictionary<string, string> Properties
        {
            get;
        }

        /// <summary>
        /// 规则集
        /// </summary>
        List<HLRuleSet> Rules
        {
            get;
        }

        /// <summary>
        /// 默认规则
        /// </summary>
        HLRuleSet DefaultRuleSet
        {
            get;
        }

        // returns special color. (BackGround Color, Cursor Color and so on)

        /// <remarks>
        /// 获取指定名称的系统颜色
        /// </remarks>
        HighlightColor GetColorFor(string name);

        /// <summary>
        /// 获取xshd文件内部定义的颜色
        /// </summary>
        /// <param name="name">颜色名称</param>
        /// <returns></returns>
        HighlightColor GetHLColor(string name);

        /// <remarks>
        /// 取规则集，span为null返回默认规则集
        /// </remarks>
        HLRuleSet GetRuleSet(Span span);

        /// <remarks>
        /// 获取指定位置的高亮颜色
        /// </remarks>
        HighlightColor GetColor(IDocument document, LineSegment keyWord, int index, int length);

        /// <remarks>
        /// 更新高亮单词集
        /// </remarks>
        void MarkTokens(IDocument document, List<LineSegment> lines);

        /// <remarks>
        /// 更新高亮单词集
        /// </remarks>
        void MarkTokens(IDocument document);
    }
}
