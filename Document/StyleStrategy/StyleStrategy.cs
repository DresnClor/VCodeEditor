using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using VCodeEditor.Implement;
using VCodeEditor.Util;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 默认样式策略实现
	/// </summary>
	public class StyleStrategy : IStyleStrategy
	{
		private string VslDir;
		/// <param name="name">高亮名称</param>
		/// <param name="file">配置文件</param>
		public StyleStrategy(string name, string file) : this(name)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException("文件不存在! ", file);
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(file);
				this.VslDir = Path.GetDirectoryName(file);
				//根属性
				XmlElement root = doc.DocumentElement;
				if (root.HasAttribute("name"))
					this.name = root.Attributes["name"].InnerText;

				if (root.HasAttribute("extensions"))
					this.Extensions = root
						.GetAttribute("extensions")
						.Split(new char[] { ';', '|', ',' });

				if (root.HasAttribute("language"))
					this.Language = root.GetAttribute("language");
				//解析资源
				XmlElement resources = root["Resources"];
				if (resources != null)
				{
					foreach (XmlNode resource in resources.ChildNodes)
					{
						if (resource is XmlElement xe)
						{
							string iname = xe.GetAttribute("name");
							string ipath = Utils.GetAbsolutePath(this.VslDir, this.VslDir, xe.GetAttribute("path"));
							if (File.Exists(ipath))
							{
								if (xe.Name == "Image")
								{//图片
									this.Images[iname] = Image.FromFile(ipath);
								}
								else if (xe.Name == "Font")
								{//字体
									FontContainer.AddFontFile(ipath);
								}
							}
						}
					}
				}
				//解析断点栏
				XmlElement breakpoint = doc.DocumentElement["Breakpoint"];
				if (breakpoint != null)
				{
					Func<string, string, Image> getImage = (image, path) =>
					{//获取图片
						if (string.IsNullOrWhiteSpace(path))
						{
							return this.GetImage(name);
						}
						else
						{
							string p = Utils.GetAbsolutePath(this.VslDir, this.VslDir, path);
							if (File.Exists(p))
							{
								return Image.FromFile(p);
							}
							else
							{
								return null;
							}
						}
					};

					//解析配置
					XmlElement normal = breakpoint["Normal"];

					if (normal != null)
						this.BreakpointStyle.Normal = getImage(
							normal.GetAttribute("image"),
							normal.GetAttribute("path"));
					XmlElement disable = breakpoint["Disable"];

					if (disable != null)
						this.BreakpointStyle.Disable = getImage(
							disable.GetAttribute("image"),
							disable.GetAttribute("path"));
					XmlElement unableToHit = breakpoint["UnableToHit"];

					if (unableToHit != null)
						this.BreakpointStyle.UnableToHit = getImage(
							unableToHit.GetAttribute("image"),
							unableToHit.GetAttribute("path"));
				}
				
				//解析引用高亮文件列表
				XmlElement syntaxs = root["Syntaxs"];
				if (syntaxs != null)
				{
					foreach (XmlElement c in syntaxs.ChildNodes)
					{
						string n = c.Attributes["name"].InnerText;
						try
						{
							string sf = Path.Combine(
								Path.GetDirectoryName(file),
								c.Attributes["file"].InnerText);
							StyleStrategy hl = new StyleStrategy(n, sf);
							this.references.Add(n, hl);
						}
						catch { }
					}
				}
				//解析样式
				XmlElement styles = root["Styles"];
				if (styles != null)
				{
					foreach (XmlElement c in styles.ChildNodes)
					{
						string n = c.Attributes["name"].InnerText;
						this.HighlightStyles.Add(n, new ColorStyle(c));
					}
				}
				//解析环境配置
				XmlElement env = root["Environments"];
				if (env != null)
				{
					foreach (XmlNode node in env.ChildNodes)
					{
						if (node is XmlElement el)
						{
							if (el.HasAttribute("ref:color"))
							{
								string n = el.GetAttribute("ref:color");
								ColorStyle colorStyle = StyleManager.GlobalStyle.GetColorStyle(n);
								this.SetColorFor(el.Name, colorStyle);
							}
							else
							{
								this.SetColorFor(
									el.Name,
									el.HasAttribute("bgcolor") ?
										new HighlightBackground(el) :
										new ColorStyle(el));
							}
						}
					}
				}
				//解析属性
				if (doc.DocumentElement["Properties"] != null)
				{
					foreach (XmlElement pe in doc.DocumentElement["Properties"].ChildNodes)
					{
						this.Properties[pe.Attributes["name"].InnerText] =
							pe.Attributes["value"].InnerText;
					}
				}
				//解析数字
				XmlElement digits = root["Digits"];
				if (digits != null)
				{
					if (digits.HasAttribute("ref:color"))
					{
						string n = digits.GetAttribute("ref:color");
						this.DigitColor = StyleManager.GlobalStyle.GetColorStyle(n);
					}
					else
						this.DigitColor = new ColorStyle(digits);
				}
				//解析规则
				XmlElement nodes = root["RuleSets"];
				if (nodes != null)
				{
					XmlNodeList rss = nodes.GetElementsByTagName("RuleSet");
					foreach (XmlElement element in rss)
					{//遍历规则集
						this.AddRuleSet(new HighlightRuleSet(element));
					}
				}
				this.ResolveReferences();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Vsl Parser Error!\nMsg: " + ex.Message);
			}
		}

		/// <param name="name">高亮名称</param>
		public StyleStrategy(string name)
		{
			this.name = name;
			this.references = new Dictionary<string, IStyleStrategy>();
			this.digitStyle = new ColorStyle(SystemColors.WindowText, false, false);
			this.defaultTextColor = new ColorStyle(SystemColors.WindowText, false, false);
			this.environmentStyle["Default"] = new HighlightBackground("WindowText", "Window", false, false);
			this.environmentStyle["Selection"] = new ColorStyle("HighlightText", "Highlight", false, false);
			this.environmentStyle["VRuler"] = new ColorStyle("ControlLight", "Window", false, false);
			this.environmentStyle["InvalidLines"] = new ColorStyle(Color.Red, false, false);
			this.environmentStyle["CaretMarker"] = new ColorStyle(Color.FromArgb(224, 229, 235), false, false);
			this.environmentStyle["LineNumbers"] = new HighlightBackground("ControlDark", "Window", false, false);
			this.environmentStyle["BreakpointBar"] = new HighlightBackground("ControlDark", "Window", false, false);
			this.environmentStyle["IconBar"] = new HighlightBackground("ControlDark", "Window", false, false);
			this.environmentStyle["FoldLine"] = new ColorStyle(Color.FromArgb(0x80, 0x80, 0x80), Color.Black, false, false);
			this.environmentStyle["FoldMarker"] = new ColorStyle(Color.FromArgb(0x80, 0x80, 0x80), Color.White, false, false);
			this.environmentStyle["SelectedFoldLine"] = new ColorStyle(Color.Black, false, false);
			this.environmentStyle["EOLMarkers"] = new ColorStyle("ControlLight", "Window", false, false);
			this.environmentStyle["SpaceMarkers"] = new ColorStyle("ControlLight", "Window", false, false);
			this.environmentStyle["TabMarkers"] = new ColorStyle("ControlLight", "Window", false, false);
		}

		/// <summary>
		/// 高亮名称
		/// </summary>
		string name;
		/// <summary>
		/// 规则列表
		/// </summary>
		List<HighlightRuleSet> rules = new List<HighlightRuleSet>();
		/// <summary>
		/// 环境颜色列表
		/// </summary>
		Dictionary<string, ColorStyle> environmentStyle = new Dictionary<string, ColorStyle>();
		/// <summary>
		/// 属性列表
		/// </summary>
		Dictionary<string, string> properties = new Dictionary<string, string>();
		/// <summary>
		/// 扩展名数组
		/// </summary>
		string[] extensions;
		/// <summary>
		/// 高亮颜色列表
		/// </summary>
		internal Dictionary<string, ColorStyle> HighlightStyles = new Dictionary<string, ColorStyle>();
		/// <summary>
		/// 数字高亮颜色
		/// </summary>
		ColorStyle digitStyle;
		/// <summary>
		/// 图片资源
		/// </summary>
		Dictionary<string, Image> Images = new Dictionary<string, Image>();

		/// <summary>
		/// 外部引用规则集列表
		/// </summary>
		Dictionary<string, IStyleStrategy> references;

		/// <summary>
		/// 默认规则集
		/// </summary>
		HighlightRuleSet defaultRuleSet = null;

		public BreakpointStyle BreakpointStyle { get; } = new BreakpointStyle();

		public string Language { get; }

		/// <summary>
		/// 数字颜色
		/// </summary>
		public ColorStyle DigitColor
		{
			get
			{
				return digitStyle;
			}
			set
			{
				digitStyle = value;
			}
		}

		/// <summary>
		/// 高亮颜色集
		/// </summary>
		/// <param name="name">颜色名称</param>
		/// <returns></returns>
		public ColorStyle this[string name]
		{
			get
			{
				if (environmentStyle.ContainsKey(name))
					return environmentStyle[name];
				return null;
			}
			set
			{
				environmentStyle[name] = value;
			}
		}



		/// <summary>
		/// 属性列表
		/// </summary>
		public Dictionary<string, string> Properties
		{
			get
			{
				return properties;
			}
		}

		/// <summary>
		/// 高亮名称
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
		}

		/// <summary>
		/// 扩展名
		/// </summary>
		public string[] Extensions
		{
			set
			{
				extensions = value;
			}
			get
			{
				return extensions;
			}
		}

		/// <summary>
		/// 默认规则
		/// </summary>
		public HighlightRuleSet DefaultRuleSet
		{
			get => /*this.RuleSet(null);*/this.defaultRuleSet;
		}

		/// <summary>
		/// 高亮规则列表
		/// </summary>
		public List<HighlightRuleSet> Rules
		{
			get
			{
				return rules;
			}
		}

		/// <summary>
		/// 获取指定规则集
		/// </summary>
		/// <param name="name">规则集名称</param>
		/// <returns></returns>
		public HighlightRuleSet RuleSet(string name)
		{
			foreach (HighlightRuleSet rule in this.Rules)
			{
				if (rule.Name == name)
					return rule;
			}
			return null;
		}

		/// <summary>
		/// 查找高亮规则集名称
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public HighlightRuleSet FindHighlightRuleSet(string name)
		{
			foreach (HighlightRuleSet ruleSet in rules)
			{
				if (ruleSet.Name == name)
				{
					return ruleSet;
				}
			}
			return null;
		}

		/// <summary>
		/// 添加高亮规则集
		/// </summary>
		/// <param name="aRuleSet"></param>
		public void AddRuleSet(HighlightRuleSet aRuleSet)
		{
			HighlightRuleSet existing = FindHighlightRuleSet(aRuleSet.Name);
			if (existing != null)
			{
				existing.MergeFrom(aRuleSet);
			}
			else
			{
				rules.Add(aRuleSet);
			}
		}

		public Image GetImage(string name)
		{
			if (this.Images.ContainsKey(name))
				return this.Images[name];
			return null;
		}

		internal void ResolveReferences()
		{
			// Resolve references from Span definitions to RuleSets
			this.ResolveRuleSetReferences();
			// Resolve references from RuleSet defintitions to Highlighters defined in an external mode file
			this.ResolveExternalReferences();
		}

		void ResolveRuleSetReferences()
		{
			foreach (HighlightRuleSet ruleSet in Rules)
			{
				if (ruleSet.Name == null)
				{
					defaultRuleSet = ruleSet;
				}

				foreach (Span aSpan in ruleSet.Spans)
				{
					if (aSpan.Rule != null)
					{
						bool found = false;
						foreach (HighlightRuleSet refSet in Rules)
						{
							if (refSet.Name == aSpan.Rule)
							{
								found = true;
								aSpan.RuleSet = refSet;
								break;
							}
						}
						if (!found)
						{
							MessageBox.Show("The RuleSet " + aSpan.Rule + " could not be found in mode definition " + this.Name, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
							aSpan.RuleSet = null;
						}
					}
					else
					{
						aSpan.RuleSet = null;
					}
				}
			}

			if (defaultRuleSet == null)
			{
				MessageBox.Show("No default RuleSet is defined for mode definition " + this.Name, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
			}
		}

		void ResolveExternalReferences()
		{
			foreach (HighlightRuleSet ruleSet in this.Rules)
			{
				if (ruleSet.Reference != null)
				{
					//自身引用高亮策略
					if (this.references.ContainsKey(ruleSet.Reference))
					{
						ruleSet.Highlighter = this.references[ruleSet.Reference];
						continue;
					}
					else
					{
						ruleSet.Highlighter = StyleManager
							.StyleProvider
							.GetStyle(ruleSet.Reference);
					}
				}
				ruleSet.Highlighter = this;
			}
		}

		//		internal void SetDefaultColor(HLBackground color)
		//		{
		//			return (HighlightColor)environmentColors[name];
		//			defaultColor = color;
		//		}

		ColorStyle defaultTextColor;

		/// <summary>
		/// 默认文本颜色
		/// </summary>
		public ColorStyle DefaultTextColor
		{
			get
			{
				return defaultTextColor;
			}
		}

		/// <summary>
		/// 设置指定名称的高亮颜色配置
		/// </summary>
		/// <param name="name"></param>
		/// <param name="color"></param>
		internal void SetColorFor(string name, ColorStyle color)
		{
			if (name == "Default")
				defaultTextColor = new ColorStyle(color.Color, color.Bold, color.Italic);
			environmentStyle[name] = color;
		}

		/// <summary>
		/// 获取指定名称的高亮颜色
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ColorStyle GetStyleFor(string name)
		{
			if (!environmentStyle.ContainsKey(name))
			{
				throw new Exception("Color : " + name + " not found!");
			}
			return (ColorStyle)environmentStyle[name];
		}

		public ColorStyle GetHighlightStyle(string name)
		{
			if (this.HighlightStyles.ContainsKey(name))
				return this.HighlightStyles[name];
			return new ColorStyle(Color.Black, Color.White, false, false);
		}

		/// <summary>
		/// 获取指定位置的高亮颜色
		/// </summary>
		/// <param name="document"></param>
		/// <param name="currentSegment"></param>
		/// <param name="currentOffset"></param>
		/// <param name="currentLength"></param>
		/// <returns></returns>
		public ColorStyle GetStyle(IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
		{
			return GetColor(defaultRuleSet, document, currentSegment, currentOffset, currentLength);
		}

		ColorStyle GetColor(HighlightRuleSet ruleSet, IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
		{
			if (ruleSet != null)
			{
				if (ruleSet.Reference != null)
				{
					return ruleSet.Highlighter.GetStyle(document, currentSegment, currentOffset, currentLength);
				}
				else
				{
					return (ColorStyle)ruleSet.KeyWords[document, currentSegment, currentOffset, currentLength];
				}
			}
			return null;
		}

		/// <summary>
		/// 获取规则
		/// </summary>
		/// <param name="aSpan"></param>
		/// <returns></returns>
		public HighlightRuleSet GetRuleSet(Span aSpan)
		{
			if (aSpan == null)
			{
				return this.defaultRuleSet;
			}
			else
			{
				if (aSpan.RuleSet != null)
				{
					if (aSpan.RuleSet.Reference != null)
					{
						return aSpan.RuleSet.Highlighter.GetRuleSet(null);
					}
					else
					{
						return aSpan.RuleSet;
					}
				}
				else
				{
					return null;
				}
			}
		}

		// Line state variable
		/// <summary>
		/// 行状态变量。
		/// 当前行
		/// </summary>
		LineSegment currentLine;

		// Span stack state variable
		/// <summary>
		/// 范围栈状态变量。
		/// 当前范围栈
		/// </summary>
		Stack<Span> currentSpanStack;

		/// <summary>
		/// 更新高亮单词集
		/// </summary>
		/// <param name="document"></param>
		public void MarkTokens(IDocument document)
		{
			if (Rules.Count == 0)
			{
				return;
			}

			int lineNumber = 0;

			while (lineNumber < document.TotalNumberOfLines)
			{
				LineSegment previousLine = (lineNumber > 0 ? document.GetLineSegment(lineNumber - 1) : null);
				if (lineNumber >= document.LineSegmentCollection.Count)
				{ // may be, if the last line ends with a delimiter
					break;                                                // then the last line is not in the collection :)
				}

				currentSpanStack = ((previousLine != null && previousLine.HighlightSpanStack != null) ? new Stack<Span>(previousLine.HighlightSpanStack.ToArray()) : null);

				if (currentSpanStack != null)
				{
					while (currentSpanStack.Count > 0 && ((Span)currentSpanStack.Peek()).StopEOL)
					{
						currentSpanStack.Pop();
					}
					if (currentSpanStack.Count == 0)
						currentSpanStack = null;
				}

				currentLine = (LineSegment)document.LineSegmentCollection[lineNumber];

				if (currentLine.Length == -1)
				{ // happens when buffer is empty !
					return;
				}

				List<TextWord> words = ParseLine(document);
				// Alex: clear old words
				if (currentLine.Words != null)
				{
					currentLine.Words.Clear();
				}
				currentLine.Words = words;
				currentLine.HighlightSpanStack = (currentSpanStack == null || currentSpanStack.Count == 0) ? null : currentSpanStack;

				++lineNumber;
			}
			document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			document.CommitUpdate();
			currentLine = null;
		}

		bool MarkTokensInLine(IDocument document, int lineNumber, ref bool spanChanged)
		{
			bool processNextLine = false;
			LineSegment previousLine = (lineNumber > 0 ? document.GetLineSegment(lineNumber - 1) : null);

			currentSpanStack = ((previousLine != null && previousLine.HighlightSpanStack != null) ? new Stack<Span>(previousLine.HighlightSpanStack.ToArray()) : null);
			if (currentSpanStack != null)
			{
				while (currentSpanStack.Count > 0 && currentSpanStack.Peek().StopEOL)
				{
					currentSpanStack.Pop();
				}
				if (currentSpanStack.Count == 0)
				{
					currentSpanStack = null;
				}
			}

			currentLine = (LineSegment)document.LineSegmentCollection[lineNumber];

			if (currentLine.Length == -1)
			{ // happens when buffer is empty !
				return false;
			}

			List<TextWord> words = ParseLine(document);

			if (currentSpanStack != null && currentSpanStack.Count == 0)
			{
				currentSpanStack = null;
			}

			// Check if the span state has changed, if so we must re-render the next line
			// This check may seem utterly complicated but I didn't want to introduce any function calls
			// or alllocations here for perf reasons.
			if (currentLine.HighlightSpanStack != currentSpanStack)
			{
				if (currentLine.HighlightSpanStack == null)
				{
					processNextLine = false;
					foreach (Span sp in currentSpanStack)
					{
						if (!sp.StopEOL)
						{
							spanChanged = true;
							processNextLine = true;
							break;
						}
					}
				}
				else if (currentSpanStack == null)
				{
					processNextLine = false;
					foreach (Span sp in currentLine.HighlightSpanStack)
					{
						if (!sp.StopEOL)
						{
							spanChanged = true;
							processNextLine = true;
							break;
						}
					}
				}
				else
				{
					IEnumerator<Span> e1 = currentSpanStack.GetEnumerator();
					IEnumerator<Span> e2 = currentLine.HighlightSpanStack.GetEnumerator();
					bool done = false;
					while (!done)
					{
						bool blockSpanIn1 = false;
						while (e1.MoveNext())
						{
							if (!((Span)e1.Current).StopEOL)
							{
								blockSpanIn1 = true;
								break;
							}
						}
						bool blockSpanIn2 = false;
						while (e2.MoveNext())
						{
							if (!((Span)e2.Current).StopEOL)
							{
								blockSpanIn2 = true;
								break;
							}
						}
						if (blockSpanIn1 || blockSpanIn2)
						{
							if (blockSpanIn1 && blockSpanIn2)
							{
								if (e1.Current != e2.Current)
								{
									done = true;
									processNextLine = true;
									spanChanged = true;
								}
							}
							else
							{
								spanChanged = true;
								done = true;
								processNextLine = true;
							}
						}
						else
						{
							done = true;
							processNextLine = false;
						}
					}
				}
			}
			else
			{
				processNextLine = false;
			}

			//// Alex: remove old words
			if (currentLine.Words != null)
				currentLine.Words.Clear();
			currentLine.Words = words;
			currentLine.HighlightSpanStack = (currentSpanStack != null && currentSpanStack.Count > 0) ? currentSpanStack : null;

			return processNextLine;
		}

		/// <summary>
		/// 更新高亮单词集
		/// </summary>
		/// <param name="document"></param>
		/// <param name="inputLines"></param>
		public void MarkTokens(IDocument document, List<LineSegment> inputLines)
		{
			if (Rules.Count == 0)
			{
				return;
			}

			Dictionary<LineSegment, bool> processedLines = new Dictionary<LineSegment, bool>();

			bool spanChanged = false;

			foreach (LineSegment lineToProcess in inputLines)
			{
				if (!processedLines.ContainsKey(lineToProcess))
				{
					int lineNumber = document.GetLineNumberForOffset(lineToProcess.Offset);
					bool processNextLine = true;

					if (lineNumber != -1)
					{
						while (processNextLine && lineNumber < document.TotalNumberOfLines)
						{
							if (lineNumber >= document.LineSegmentCollection.Count)
							{ // may be, if the last line ends with a delimiter
								break;                                                // then the last line is not in the collection :)
							}

							processNextLine = MarkTokensInLine(document, lineNumber, ref spanChanged);
							processedLines[currentLine] = true;
							++lineNumber;
						}
					}
				}
			}

			if (spanChanged)
			{
				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
			}
			else
			{
				//				document.Caret.ValidateCaretPos();
				//				document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, document.GetLineNumberForOffset(document.Caret.Offset)));
				//
				foreach (LineSegment lineToProcess in inputLines)
				{
					document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, document.GetLineNumberForOffset(lineToProcess.Offset)));
				}

			}
			document.CommitUpdate();
			currentLine = null;
		}

		// Span state variables
		bool inSpan;
		Span activeSpan;
		HighlightRuleSet activeRuleSet;

		// Line scanning state variables
		int currentOffset;
		int currentLength;

		/// <summary>
		/// 更新范围状态变量
		/// </summary>
		void UpdateSpanStateVariables()
		{
			inSpan = (currentSpanStack != null && currentSpanStack.Count > 0);
			activeSpan = inSpan ? (Span)currentSpanStack.Peek() : null;
			activeRuleSet = GetRuleSet(activeSpan);
		}

		/// <summary>
		/// 解析行
		/// </summary>
		/// <param name="document"></param>
		/// <returns></returns>
		List<TextWord> ParseLine(IDocument document)
		{
			List<TextWord> words = new List<TextWord>();
			ColorStyle markNext = null;

			currentOffset = 0;
			currentLength = 0;
			UpdateSpanStateVariables();

			for (int i = 0; i < currentLine.Length; ++i)
			{
				char ch = document.GetCharAt(currentLine.Offset + i);
				switch (ch)
				{
					case '\n':
					case '\r':
						PushCurWord(document, ref markNext, words);
						++currentOffset;
						break;
					case ' ':
						PushCurWord(document, ref markNext, words);
						if (activeSpan != null && activeSpan.Color.HasBackground)
						{
							words.Add(new TextWord.SpaceTextWord(activeSpan.Color));
						}
						else
						{
							words.Add(TextWord.Space);
						}
						++currentOffset;
						break;
					case '\t':
						PushCurWord(document, ref markNext, words);
						if (activeSpan != null && activeSpan.Color.HasBackground)
						{
							words.Add(new TextWord.TabTextWord(activeSpan.Color));
						}
						else
						{
							words.Add(TextWord.Tab);
						}
						++currentOffset;
						break;
					case '\\': // handle escape chars
						if ((activeRuleSet != null && activeRuleSet.NoEscapeSequences) ||
							(activeSpan != null && activeSpan.NoEscapeSequences))
						{
							goto default;
						}
						++currentLength;
						if (i + 1 < currentLine.Length)
						{
							++currentLength;
						}
						PushCurWord(document, ref markNext, words);
						++i;
						continue;
					default:
						{
							// highlight digits
							if (!inSpan && (Char.IsDigit(ch) || (ch == '.' && i + 1 < currentLine.Length && Char.IsDigit(document.GetCharAt(currentLine.Offset + i + 1)))) && currentLength == 0)
							{
								bool ishex = false;
								bool isfloatingpoint = false;

								if (ch == '0' && i + 1 < currentLine.Length && Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1)) == 'X')
								{ // hex digits
									const string hex = "0123456789ABCDEF";
									++currentLength;
									++i; // skip 'x'
									++currentLength;
									ishex = true;
									while (i + 1 < currentLine.Length && hex.IndexOf(Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1))) != -1)
									{
										++i;
										++currentLength;
									}
								}
								else
								{
									++currentLength;
									while (i + 1 < currentLine.Length && Char.IsDigit(document.GetCharAt(currentLine.Offset + i + 1)))
									{
										++i;
										++currentLength;
									}
								}
								if (!ishex && i + 1 < currentLine.Length && document.GetCharAt(currentLine.Offset + i + 1) == '.')
								{
									isfloatingpoint = true;
									++i;
									++currentLength;
									while (i + 1 < currentLine.Length && Char.IsDigit(document.GetCharAt(currentLine.Offset + i + 1)))
									{
										++i;
										++currentLength;
									}
								}

								if (i + 1 < currentLine.Length && Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1)) == 'E')
								{
									isfloatingpoint = true;
									++i;
									++currentLength;
									if (i + 1 < currentLine.Length && (document.GetCharAt(currentLine.Offset + i + 1) == '+' || document.GetCharAt(currentLine.Offset + i + 1) == '-'))
									{
										++i;
										++currentLength;
									}
									while (i + 1 < currentLine.Length && Char.IsDigit(document.GetCharAt(currentLine.Offset + i + 1)))
									{
										++i;
										++currentLength;
									}
								}

								if (i + 1 < currentLine.Length)
								{
									char nextch = Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1));
									if (nextch == 'F' || nextch == 'M' || nextch == 'D')
									{
										isfloatingpoint = true;
										++i;
										++currentLength;
									}
								}

								if (!isfloatingpoint)
								{
									bool isunsigned = false;
									if (i + 1 < currentLine.Length && Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1)) == 'U')
									{
										++i;
										++currentLength;
										isunsigned = true;
									}
									if (i + 1 < currentLine.Length && Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1)) == 'L')
									{
										++i;
										++currentLength;
										if (!isunsigned && i + 1 < currentLine.Length && Char.ToUpper(document.GetCharAt(currentLine.Offset + i + 1)) == 'U')
										{
											++i;
											++currentLength;
										}
									}
								}

								words.Add(new TextWord(document, currentLine, currentOffset, currentLength, DigitColor, false));
								currentOffset += currentLength;
								currentLength = 0;
								continue;
							}

							// Check for SPAN ENDs
							if (inSpan)
							{
								if (activeSpan.End != null && !activeSpan.End.Equals(""))
								{
									if (currentLine.MatchExpr(activeSpan.End, i, document))
									{
										PushCurWord(document, ref markNext, words);
										string regex = currentLine.GetRegString(activeSpan.End, i, document);
										currentLength += regex.Length;
										words.Add(new TextWord(document, currentLine, currentOffset, currentLength, activeSpan.EndColor, false));
										currentOffset += currentLength;
										currentLength = 0;
										i += regex.Length - 1;
										currentSpanStack.Pop();
										UpdateSpanStateVariables();
										continue;
									}
								}
							}

							// check for SPAN BEGIN
							if (activeRuleSet != null)
							{
								foreach (Span span in activeRuleSet.Spans)
								{
									if (currentLine.MatchExpr(span.Begin, i, document))
									{
										PushCurWord(document, ref markNext, words);
										string regex = currentLine.GetRegString(span.Begin, i, document);
										currentLength += regex.Length;
										words.Add(new TextWord(document, currentLine, currentOffset, currentLength, span.BeginColor, false));
										currentOffset += currentLength;
										currentLength = 0;

										i += regex.Length - 1;
										if (currentSpanStack == null)
										{
											currentSpanStack = new Stack<Span>();
										}
										currentSpanStack.Push(span);

										UpdateSpanStateVariables();

										goto skip;
									}
								}
							}

							// check if the char is a delimiter
							if (activeRuleSet != null && (int)ch < 256 && activeRuleSet.Delimiters[(int)ch])
							{
								PushCurWord(document, ref markNext, words);
								if (currentOffset + currentLength + 1 < currentLine.Length)
								{
									++currentLength;
									PushCurWord(document, ref markNext, words);
									goto skip;
								}
							}

							++currentLength;
						skip:
							continue;
						}
				}
			}

			PushCurWord(document, ref markNext, words);

			return words;
		}

		/// <summary>
		/// pushes the curWord string on the word list, with the
		/// correct color.
		/// </summary>
		void PushCurWord(IDocument document, ref ColorStyle markNext, List<TextWord> words)
		{
			// Svante Lidman : Need to look through the next prev logic.
			if (currentLength > 0)
			{
				if (words.Count > 0 && activeRuleSet != null)
				{
					TextWord prevWord = null;
					int pInd = words.Count - 1;
					while (pInd >= 0)
					{
						if (!((TextWord)words[pInd]).IsWhiteSpace)
						{
							prevWord = (TextWord)words[pInd];
							if (prevWord.HasDefaultColor)
							{
								PrevMarker marker = (PrevMarker)activeRuleSet.PrevMarkers[document, currentLine, currentOffset, currentLength];
								if (marker != null)
								{
									prevWord.SyntaxStyle = marker.Color;
									//									document.Caret.ValidateCaretPos();
									//									document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, document.GetLineNumberForOffset(document.Caret.Offset)));
								}
							}
							break;
						}
						pInd--;
					}
				}

				if (inSpan)
				{
					ColorStyle c = null;
					bool hasDefaultColor = true;
					if (activeSpan.Rule == null)
					{
						c = activeSpan.Color;
					}
					else
					{
						c = GetColor(activeRuleSet, document, currentLine, currentOffset, currentLength);
						hasDefaultColor = false;
					}

					if (c == null)
					{
						c = activeSpan.Color;
						if (c.Color == Color.Transparent)
						{
							c = this.DefaultTextColor;
						}
						hasDefaultColor = true;
					}
					words.Add(new TextWord(document, currentLine, currentOffset, currentLength, markNext != null ? markNext : c, hasDefaultColor));
				}
				else
				{
					ColorStyle c = markNext != null ? markNext : GetColor(activeRuleSet, document, currentLine, currentOffset, currentLength);
					if (c == null)
					{
						words.Add(new TextWord(document, currentLine, currentOffset, currentLength, this.DefaultTextColor, true));
					}
					else
					{
						words.Add(new TextWord(document, currentLine, currentOffset, currentLength, c, false));
					}
				}

				if (activeRuleSet != null)
				{
					NextMarker nextMarker = (NextMarker)activeRuleSet.NextMarkers[document, currentLine, currentOffset, currentLength];
					if (nextMarker != null)
					{
						if (nextMarker.MarkMarker && words.Count > 0)
						{
							TextWord prevword = ((TextWord)words[words.Count - 1]);
							prevword.SyntaxStyle = nextMarker.Color;
						}
						markNext = nextMarker.Color;
					}
					else
					{
						markNext = null;
					}
				}
				currentOffset += currentLength;
				currentLength = 0;
			}
		}

	}

}
