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
    /// 样式接口
    /// </summary>
    public interface IStyleStrategy
    {
        /// <value>
        /// 样式名称
        /// </value>
        string Name { get; }

        /// <summary>
        /// 编程语言名称
        /// </summary>
        string Language { get; }

        /// <value>
        /// 文件扩展名列表
        /// </value>
        string[] Extensions { set; get; }

        
 



		/// <summary>
		/// 配置属性表
		/// </summary>
		Dictionary<string, string> Properties { get; }











        /// <summary>
        /// 断点栏样式
        /// </summary>
        BreakpointStyle BreakpointStyle { get; }

        /// <summary>
        /// 规则集
        /// </summary>
        List<HighlightRuleSet> Rules { get; }

        /// <summary>
        /// 默认规则
        /// </summary>
        HighlightRuleSet DefaultRuleSet { get; }

        /// <summary>
        /// 获取图片资源
        /// </summary>
        /// <param name="name">图片名称</param>
        /// <returns>如果不存在，返回null</returns>
        Image GetImage(string name);

        /// <remarks>
        /// 获取指定名称的系统颜色
        /// </remarks>
        ColorStyle GetStyleFor(string name);

        /// <summary>
        /// 获取内部定义的颜色
        /// </summary>
        /// <param name="name">颜色名称</param>
        /// <returns></returns>
        ColorStyle GetHighlightStyle(string name);

        /// <remarks>
        /// 取规则集，span为null返回默认规则集
        /// </remarks>
        HighlightRuleSet GetRuleSet(Span span);

        /// <remarks>
        /// 获取指定位置的高亮颜色
        /// </remarks>
        ColorStyle GetStyle(IDocument document, LineSegment keyWord, int index, int length);

        /// <remarks>
        /// 更新高亮单词集
        /// </remarks>
        void MarkTokens(IDocument document, List<LineSegment> lines);

        /// <remarks>
        /// 更新高亮单词集
        /// </remarks>
        void MarkTokens(IDocument document);

    }

    /// <summary>
    /// 断点样式
    /// </summary>
    public class BreakpointStyle
    {
        public BreakpointStyle()
        {
        }

        /// <summary>
        /// 断点正常图片
        /// </summary>
        public Image Normal { get; set; }

        /// <summary>
        /// 断点禁用图片
        /// </summary>
        public Image Disable { get; set; }

        /// <summary>
        /// 断点未命中图片
        /// </summary>
        public Image UnableToHit { get; set; }
    }
}
