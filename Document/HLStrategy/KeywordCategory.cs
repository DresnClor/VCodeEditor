// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="DresnClor" url="http://www.dresnclor.com"/>
//     <version>$Revision: 001 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeltuiCodeEditor.Document
{
    /// <summary>
    /// 关键字分组管理
    /// </summary>
    public class KeywordCategory
    {
        public KeywordCategory(string name, HighlightRuleSet highlightRuleSet, HighlightStyle highlightColor)
        {
            this.Keywords = new List<string>();
            this.HighlightColor = highlightColor;
            this.Name = name;
            this.RuleSet = highlightRuleSet;
        }
        /// <summary>
        /// 规则集
        /// </summary>
        private HighlightRuleSet RuleSet;

        /// <summary>
        /// 高亮颜色
        /// </summary>
        private HighlightStyle HighlightColor;

        /// <summary>
        /// 分组名称
        /// </summary>
        private string Name;

        /// <summary>
        /// 分组高亮颜色
        /// </summary>
        public HighlightStyle Color
        {
            get; set;
        }

        /// <summary>
        /// 关键字列表
        /// </summary>
        public List<string> Keywords
        {
            get;
            private set;
        }

        /// <summary>
        /// 关键字数量
        /// </summary>
        public int Count
        {
            get => this.Keywords.Count;
        }

        /// <summary>
        /// 移除关键字
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (this.Keywords.Contains(key))
            {
                this.Keywords.Remove(key);
                this.RuleSet.KeyWords.Del(key);
            }
        }

        /// <summary>
        /// 添加关键字
        /// </summary>
        /// <param name="key"></param>
        public void Add(string key)
        {
            if (this.Keywords.Contains(key))
                return;
            this.Keywords.Add(key);
            this.RuleSet.KeyWords[key] = this.HighlightColor;
        }
    }
}
