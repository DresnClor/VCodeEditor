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

namespace VCodeEditor.Document
{
	/// <summary>
	/// 单词分组管理
	/// </summary>
	public class WordCategory
	{
		public WordCategory(string name, HighlightRuleSet highlightRuleSet, ColorStyle highlightColor)
		{
			this.words = new List<string>();
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
		private ColorStyle HighlightColor;

		/// <summary>
		/// 分组名称
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 分组高亮颜色
		/// </summary>
		public ColorStyle Color
		{
			get; set;
		}
		public List<string> words;

		/// <summary>
		/// 关键字列表
		/// </summary>
		public string[] Words
		{
			get => this.words.ToArray();
		}

		/// <summary>
		/// 关键字数量
		/// </summary>
		public int Count
		{
			get => this.words.Count;
		}

		/// <summary>
		/// 移除关键字
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			if (this.words.Contains(key))
			{
				this.words.Remove(key);
				this.RuleSet.KeyWords.Del(key);
			}
		}

		/// <summary>
		/// 添加关键字
		/// </summary>
		/// <param name="key"></param>
		public void Add(string key)
		{
			if (this.words.Contains(key))
				return;
			this.words.Add(key);
			this.RuleSet.KeyWords[key] = this.HighlightColor;
		}
	}
}
