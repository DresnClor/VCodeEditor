// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1262 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Text;
using System.Xml;

using VCodeEditor.Util;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 高亮显示规则集
    /// </summary>
    public class HighlightRuleSet
    {
        /// <summary>
        /// 直接创建
        /// </summary>
        public HighlightRuleSet()
        {
            this.KwCategorys = new Dictionary<string, KeywordCategory>();
            keyWords = new LookupTable(false);
            prevMarkers = new LookupTable(false);
            nextMarkers = new LookupTable(false);
        }

        /// <summary>
        /// 从xml元素处创建
        /// </summary>
        /// <param name="el">xml元素</param>
        public HighlightRuleSet(XmlElement el)
        {
            this.KwCategorys = new Dictionary<string, KeywordCategory>();
            XmlNodeList nodes;

            if (el.Attributes["name"] != null)
            {//名称
                Name = el.Attributes["name"].InnerText;
            }
            /*else
            {
                Name = "Default";
            }*/

            if (el.Attributes["noescapesequences"] != null)
            {//无转义序列
                noEscapeSequences = Boolean.Parse(el.Attributes["noescapesequences"].InnerText);
            }

            if (el.Attributes["reference"] != null)
            {//参考
                reference = el.Attributes["reference"].InnerText;
            }

            if (el.Attributes["ignorecase"] != null)
            {//忽略大小写
                ignoreCase = Boolean.Parse(el.Attributes["ignorecase"].InnerText);
            }

            for (int i = 0; i < Delimiters.Length; ++i)
            {
                delimiters[i] = false;
            }

            if (el["Delimiters"] != null)
            {//分隔符
                string delimiterString = el["Delimiters"].InnerText;
                foreach (char ch in delimiterString)
                {
                    delimiters[(int)ch] = true;
                }
            }

            //			Spans       = new LookupTable(!IgnoreCase);

            keyWords = new LookupTable(!IgnoreCase);
            prevMarkers = new LookupTable(!IgnoreCase);
            nextMarkers = new LookupTable(!IgnoreCase);

            nodes = el.GetElementsByTagName("KeyWords");
            foreach (XmlElement el2 in nodes)
            {//关键字
                HighlightStyle color = new HighlightStyle(el2);
                //关键字分组
                string ca = el2.GetAttribute("name");
                KeywordCategory category = new KeywordCategory(ca, this, color);
                XmlNodeList keys = el2.GetElementsByTagName("Key");
                foreach (XmlElement node in keys)
                {
                    category.Add(node.Attributes["word"].InnerText);
                }
                this.KwCategorys.Add(ca, category);
            }

            nodes = el.GetElementsByTagName("Span");
            foreach (XmlElement el2 in nodes)
            {//范围规则
                Spans.Add(new Span(el2));
                /*
				Span span = new Span(el2);
				Spans[span.Begin] = span;*/
            }

            nodes = el.GetElementsByTagName("MarkPrevious");
            foreach (XmlElement el2 in nodes)
            {//标记上一个
                PrevMarker prev = new PrevMarker(el2);
                prevMarkers[prev.What] = prev;
            }

            nodes = el.GetElementsByTagName("MarkFollowing");
            foreach (XmlElement el2 in nodes)
            {//标记下一个
                NextMarker next = new NextMarker(el2);
                nextMarkers[next.What] = next;
            }
        }

        LookupTable keyWords;
        ArrayList spans = new ArrayList();
        LookupTable prevMarkers;
        LookupTable nextMarkers;
        IStyleStrategy highlighter = null;
        bool noEscapeSequences = false;

        bool ignoreCase = false;
        string name = null;

        bool[] delimiters = new bool[256];

        string reference = null;

        /// <summary>
        /// 范围规则
        /// </summary>
        public ArrayList Spans
        {
            get
            {
                return spans;
            }
        }

        /// <summary>
        /// 引用的外部高亮策略
        /// </summary>
        internal IStyleStrategy Highlighter
        {
            get
            {
                return highlighter;
            }
            set
            {
                highlighter = value;
            }
        }

        /// <summary>
        /// 关键字列表
        /// </summary>
        public LookupTable KeyWords
        {
            get
            {
                return keyWords;
            }
        }

        /// <summary>
        /// 上一个标记
        /// </summary>
        public LookupTable PrevMarkers
        {
            get
            {
                return prevMarkers;
            }
        }

        /// <summary>
        /// 下一个标记
        /// </summary>
        public LookupTable NextMarkers
        {
            get
            {
                return nextMarkers;
            }
        }

        /// <summary>
        /// 分隔符
        /// </summary>
        public bool[] Delimiters
        {
            get
            {
                return delimiters;
            }
        }

        /// <summary>
        /// 无转义序列
        /// </summary>
        public bool NoEscapeSequences
        {
            get
            {
                return noEscapeSequences;
            }
        }

        /// <summary>
        /// 忽略大小写
        /// </summary>
        public bool IgnoreCase
        {
            get
            {
                return ignoreCase;
            }
        }

        /// <summary>
        /// 高亮规则名称，系统默认项为null
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// 外部引用规则文件
        /// </summary>
        public string Reference
        {
            get
            {
                return reference;
            }
        }

        /// <summary>
        /// 关键字分组
        /// </summary>
        public Dictionary<string, KeywordCategory> KwCategorys
        {
            get;
        }

        /// <summary>
        /// 将其他规则集中的span等合并到此规则集中
        /// </summary>
        public void MergeFrom(HighlightRuleSet ruleSet)
        {
            for (int i = 0; i < delimiters.Length; i++)
            {
                delimiters[i] |= ruleSet.delimiters[i];
            }
            // insert merged spans in front of old spans
            ArrayList oldSpans = spans;
            spans = (ArrayList)ruleSet.spans.Clone();
            spans.AddRange(oldSpans);
            //keyWords.MergeFrom(ruleSet.keyWords);
            //prevMarkers.MergeFrom(ruleSet.prevMarkers);
            //nextMarkers.MergeFrom(ruleSet.nextMarkers);
        }
    }
}
