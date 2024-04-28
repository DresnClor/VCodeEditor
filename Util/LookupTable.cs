// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Collections;
using System.Drawing;
using VCodeEditor.Document;

namespace VCodeEditor.Util
{
	/// <summary>
	/// 查阅表，此类实现关键字映射。它实现数字搜索树（尝试）找到一个字。
	/// </summary>
	public class LookupTable
	{
		Node root = new Node(null, null);
		bool casesensitive;
		int length;

		/// <summary>
		/// 表中的元素数
		/// </summary>
		public int Count
		{
			get
			{
				return length;
			}
		}

		/// <summary>
		/// 获取入关键字下的对象（行，在偏移，长度），
		///返回无效，如果没有插入此类关键字。
		/// </summary>
		public object this[IDocument document, LineSegment line, int offset, int length]
		{
			get
			{
				if (length == 0)
				{
					return null;
				}
				Node next = root;

				int wordOffset = line.Offset + offset;
				if (casesensitive)
				{
					for (int i = 0; i < length; ++i)
					{
						int index = ((int)document.GetCharAt(wordOffset + i)) % 256;
						next = next.leaf[index];

						if (next == null)
						{
							return null;
						}

						if (next.color != null && TextUtility.RegionMatches(document, wordOffset, length, next.word))
						{
							return next.color;
						}
					}
				}
				else
				{
					for (int i = 0; i < length; ++i)
					{
						int index = ((int)Char.ToUpper(document.GetCharAt(wordOffset + i))) % 256;

						next = next.leaf[index];

						if (next == null)
						{
							return null;
						}

						if (next.color != null && TextUtility.RegionMatches(document, casesensitive, wordOffset, length, next.word))
						{
							return next.color;
						}
					}
				}
				return null;
			}
		}

		public void Del(string keyword)
		{
			Node node = root;
			Node next = root;
			if (!casesensitive)
			{
				keyword = keyword.ToUpper();
			}
			++length;

			// 在树上插入单词
			for (int i = 0; i < keyword.Length; ++i)
			{
				int index = ((int)keyword[i]) % 256; //求余256
													 //bool d = keyword[i] == '\\';

				next = next.leaf[index];             // 获取此索引的节点

				if (next == null)
				{ // 没有节点
					break;
				}

				if (next.word != null && next.word.Length != i)
				{
					//节点存在，取节点内容，并再次插入
					next.word = null;                 //这个词将入1层更深（更好，不需要太多 
					next.color = null;
				}

				if (i == keyword.Length - 1)
				{ // 达到关键字的末尾，插入节点在那里，如果一个节点在这里，它是
					next.word = null;                 //这个词将入1层更深（更好，不需要太多 
					next.color = null;
					break;
				}

				node = next;
			}
		}

		/// <summary>
		/// 在关键字下将对象插入树中
		/// </summary>
		public object this[string keyword]
		{
			get
			{
				object result = null;
				Node node = root;
				Node next = root;
				if (!casesensitive)
				{
					keyword = keyword.ToUpper();
				}
				++length;

				// 在树上插入单词
				for (int i = 0; i < keyword.Length; ++i)
				{
					int index = ((int)keyword[i]) % 256; //求余256
														 //bool d = keyword[i] == '\\';

					next = next.leaf[index];             // 获取此索引的节点

					if (next == null)//为空退出
					{
						break;
					}

					if (next.word != null && next.word == keyword)
					{//节点存在，取节点内容              
					 //这个词将入1层更深（更好，不需要太多 
					 // this[next.word];               // 字符串比较查找。

						string tmpword = next.word;                 //这个词将入1层更深（更好，不需要太多 
						object tmpcolor = next.color;                   // 字符串比较查找。
						if (tmpword == keyword)
							return tmpcolor;
					}

					/*if (next != null&& next.word== null)
					{//下一个
						
					}
					*/
					if (i == keyword.Length - 1)
					{ // 达到关键字的末尾，插入节点在那里，如果一个节点在这里，它是
					  // 重新插入，如果它有相同的长度（关键字等于这个词），它将被覆盖
						result = next.color;
						break;
					}

					node = next;
				}
				return result;
			}
			set
			{
				Node node = root;
				Node next = root;
				if (!casesensitive)
				{
					keyword = keyword.ToUpper();
				}
				++length;

				// 在树上插入单词
				for (int i = 0; i < keyword.Length; ++i)
				{
					int index = ((int)keyword[i]) % 256; //求余256
					bool d = keyword[i] == '\\';

					next = next.leaf[index];             // 获取此索引的节点

					if (next == null)
					{ // 没有节点创建->插入单词在这里
						node.leaf[index] = new Node(value, keyword);
						break;
					}

					if (next.word != null && next.word.Length != i)
					{
						//节点存在，取节点内容，并再次插入
						string tmpword = next.word;                    //这个词将入1层更深（更好，不需要太多 
						object tmpcolor = next.color;                  // 字符串比较查找。
						next.color = null;
						next.word = null;
						this[tmpword] = tmpcolor;
					}

					if (i == keyword.Length - 1)
					{ // 达到关键字的末尾，插入节点在那里，如果一个节点在这里，它是
						next.word = keyword;       // 重新插入，如果它有相同的长度（关键字等于这个词），它将被覆盖
						next.color = value;
						break;
					}

					node = next;
				}
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="LookupTable"/>
		/// </summary>
		public LookupTable(bool casesensitive)
		{
			this.casesensitive = casesensitive;
		}

		/// <summary>
		/// 节点
		/// </summary>
		class Node
		{
			public Node(object color, string word)
			{
				this.word = word;
				this.color = color;
			}

			/// <summary>
			/// 关键字
			/// </summary>
			public string word;

			/// <summary>
			/// 颜色对象索引
			/// </summary>
			public object color;

			/// <summary>
			/// 节点页列表
			/// </summary>
			public Node[] leaf = new Node[256];
		}

	}
}
