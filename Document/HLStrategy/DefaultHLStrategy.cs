﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1262 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Text;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 默认高亮定义
    /// </summary>
    public class DefaultHLStrategy : IStyleStrategy
    {
        string name;
        List<HighlightRuleSet> rules = new List<HighlightRuleSet>();

        Dictionary<string, HighlightStyle> environmentColors = new Dictionary<string, HighlightStyle>();
        Dictionary<string, string> properties = new Dictionary<string, string>();
        string[] extensions;

        internal Dictionary<string, HighlightStyle> Colors = new Dictionary<string, HighlightStyle>();

        HighlightStyle digitColor;
        HighlightRuleSet defaultRuleSet = null;

        public HighlightStyle DigitColor
        {
            get
            {
                return digitColor;
            }
            set
            {
                digitColor = value;
            }
        }

        public HighlightStyle this[string name]
        {
            get
            {
                return environmentColors[name];
            }
            set
            {
                environmentColors[name] = value;
            }
        }


        public DefaultHLStrategy() : this("Default")
        {
        }

        public DefaultHLStrategy(string name)
        {
            this.name = name;

            digitColor = new HighlightStyle(SystemColors.WindowText, false, false);
            defaultTextColor = new HighlightStyle(SystemColors.WindowText, false, false);

            // set small 'default color environment'
            environmentColors["Default"] = new HLBackground("WindowText", "Window", false, false);
            environmentColors["Selection"] = new HighlightStyle("HighlightText", "Highlight", false, false);
            environmentColors["VRuler"] = new HighlightStyle("ControlLight", "Window", false, false);
            environmentColors["InvalidLines"] = new HighlightStyle(Color.Red, false, false);
            environmentColors["CaretMarker"] = new HighlightStyle(Color.FromArgb(224, 229, 235), false, false);
            environmentColors["LineNumbers"] = new HLBackground("ControlDark", "Window", false, false);

            environmentColors["FoldLine"] = new HighlightStyle(Color.FromArgb(0x80, 0x80, 0x80), Color.Black, false, false);
            environmentColors["FoldMarker"] = new HighlightStyle(Color.FromArgb(0x80, 0x80, 0x80), Color.White, false, false);
            environmentColors["SelectedFoldLine"] = new HighlightStyle(Color.Black, false, false);
            environmentColors["EOLMarkers"] = new HighlightStyle("ControlLight", "Window", false, false);
            environmentColors["SpaceMarkers"] = new HighlightStyle("ControlLight", "Window", false, false);
            environmentColors["TabMarkers"] = new HighlightStyle("ControlLight", "Window", false, false);

        }

        public Dictionary<string, string> Properties
        {
            get
            {
                return properties;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

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

        public HighlightRuleSet DefaultRuleSet
        {
            get => this.RuleSet(null);
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

        internal void ResolveReferences()
        {
            // Resolve references from Span definitions to RuleSets
            ResolveRuleSetReferences();
            // Resolve references from RuleSet defintitions to Highlighters defined in an external mode file
            ResolveExternalReferences();
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
            foreach (HighlightRuleSet ruleSet in Rules)
            {
                if (ruleSet.Reference != null)
                {
                    IStyleStrategy highlighter = HLManager.Manager.FindHL(ruleSet.Reference);

                    if (highlighter != null)
                    {
                        ruleSet.Highlighter = highlighter;
                    }
                    else
                    {
                        MessageBox.Show("The mode defintion " + ruleSet.Reference + " which is refered from the " + this.Name + " mode definition could not be found", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                        ruleSet.Highlighter = this;
                    }
                }
                else
                {
                    ruleSet.Highlighter = this;
                }
            }
        }

        //		internal void SetDefaultColor(HLBackground color)
        //		{
        //			return (HighlightColor)environmentColors[name];
        //			defaultColor = color;
        //		}

        HighlightStyle defaultTextColor;

        public HighlightStyle DefaultTextColor
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
        internal void SetColorFor(string name, HighlightStyle color)
        {
            if (name == "Default")
                defaultTextColor = new HighlightStyle(color.Color, color.Bold, color.Italic);
            environmentColors[name] = color;
        }

        public HighlightStyle GetStyleFor(string name)
        {
            if (!environmentColors.ContainsKey(name))
            {
                throw new Exception("Color : " + name + " not found!");
            }
            return (HighlightStyle)environmentColors[name];
        }

        public HighlightStyle GetStyle(IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
        {
            return GetColor(defaultRuleSet, document, currentSegment, currentOffset, currentLength);
        }

        HighlightStyle GetColor(HighlightRuleSet ruleSet, IDocument document, LineSegment currentSegment, int currentOffset, int currentLength)
        {
            if (ruleSet != null)
            {
                if (ruleSet.Reference != null)
                {
                    return ruleSet.Highlighter.GetStyle(document, currentSegment, currentOffset, currentLength);
                }
                else
                {
                    return (HighlightStyle)ruleSet.KeyWords[document, currentSegment, currentOffset, currentLength];
                }
            }
            return null;
        }
        public HighlightStyle GetHighlightStyle(string name)
        {
            if (this.Colors.ContainsKey(name))
                return this.Colors[name];
            return new HighlightStyle(Color.Black, Color.White, false, false);
        }
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

        void UpdateSpanStateVariables()
        {
            inSpan = (currentSpanStack != null && currentSpanStack.Count > 0);
            activeSpan = inSpan ? (Span)currentSpanStack.Peek() : null;
            activeRuleSet = GetRuleSet(activeSpan);
        }

        List<TextWord> ParseLine(IDocument document)
        {
            List<TextWord> words = new List<TextWord>();
            HighlightStyle markNext = null;

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
        void PushCurWord(IDocument document, ref HighlightStyle markNext, List<TextWord> words)
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
                                    prevWord.SyntaxColor = marker.Color;
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
                    HighlightStyle c = null;
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
                    HighlightStyle c = markNext != null ? markNext : GetColor(activeRuleSet, document, currentLine, currentOffset, currentLength);
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
                            prevword.SyntaxColor = nextMarker.Color;
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
