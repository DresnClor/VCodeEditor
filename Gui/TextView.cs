// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 970 $</version>
// </file>

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

using VCodeEditor.Document;
using System.Security.Policy;

namespace VCodeEditor
{
    /// <summary>
    /// 文本区域视图
    /// </summary>
    public class TextView : AbstractMargin, IDisposable
    {
        int fontHeight;
        //Hashtable    charWitdh           = new Hashtable();
        StringFormat measureStringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
        Highlight highlight;
        int physicalColumn = 0; // 这个类描绘了文本区域。 ...

        public void Dispose()
        {
            measureCache.Clear();
            measureStringFormat.Dispose();
        }

        /// <summary>
        /// 括号高亮
        /// </summary>
        public Highlight Highlight
        {
            get
            {
                return highlight;
            }
            set
            {
                highlight = value;
            }
        }

        /// <summary>
        /// 光标
        /// </summary>
        public override Cursor Cursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }

        /// <summary>
        /// 第一个物理行
        /// </summary>
        public int FirstPhysicalLine
        {
            get
            {
                return textArea.VirtualTop.Y / fontHeight;
            }
        }

        /// <summary>
        /// 行高度余数
        /// </summary>
        public int LineHeightRemainder
        {
            get
            {
                return textArea.VirtualTop.Y % fontHeight;
            }
        }
        /// <summary>
        /// 获得第一个可见行
        /// </summary>
        public int FirstVisibleLine
        {
            get
            {
                return textArea.Document.GetFirstLogicalLine(textArea.VirtualTop.Y / fontHeight);
            }
            set
            {
                if (FirstVisibleLine != value)
                {
                    textArea.VirtualTop = new Point(textArea.VirtualTop.X, textArea.Document.GetVisibleLine(value) * fontHeight);

                }
            }
        }

        /// <summary>
        /// 可视行绘制余数
        /// </summary>
        public int VisibleLineDrawingRemainder
        {
            get
            {
                return textArea.VirtualTop.Y % fontHeight;
            }
        }

        /// <summary>
        /// 字体高度
        /// </summary>
        public int FontHeight
        {
            get
            {
                return fontHeight;
            }
        }

        /// <summary>
        /// 可视行数量
        /// </summary>
        public int VisibleLineCount
        {
            get
            {
                return 1 + DrawingPosition.Height / fontHeight;
            }
        }

        /// <summary>
        /// 可视列数量
        /// </summary>
        public int VisibleColumnCount
        {
            get
            {
                return (int)(DrawingPosition.Width / WideSpaceWidth) - 1;
            }
        }

        public TextView(TextArea textArea) : base(textArea)
        {
            measureStringFormat.LineAlignment = StringAlignment.Near;
            measureStringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces |
                StringFormatFlags.FitBlackBox |
                StringFormatFlags.NoWrap |
                StringFormatFlags.NoClip;

            OptionsChanged();
        }

        private IHLStrategy HLStrategy
        {
            get => this.TextArea.Document.HighlightingStrategy;
        }

        /// <summary>
        /// 字体高度
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        static int GetFontHeight(Font font)
        {
            int h = font.Height;
            return (h < 16) ? h + 1 : h;
        }

        float spaceWidth;

        /// <summary>
        /// 获取空白字符的宽度。
        ///此值在某些字体中可能相当小 - 请考虑使用宽空间宽。
        /// </summary>
        public float SpaceWidth
        {
            get
            {
                return spaceWidth;
            }
        }

        float wideSpaceWidth;

        /// <summary>
        /// 获取"宽空间"的宽度（如果选项卡设置为 4 个空间，则=选项卡的四分之一）。
        ///在单空间字体上，这与空间宽值相同。
        /// </summary>
        public float WideSpaceWidth
        {
            get
            {
                return wideSpaceWidth;
            }
        }

        Font lastFont;

        /// <summary>
        /// /选项改变
        /// </summary>
        public void OptionsChanged()
        {
            this.lastFont = TextEditorProperties.Font;
            this.fontHeight = GetFontHeight(lastFont);
            // 使用小宽度-在某些字体中，空间没有宽度，而是使用角线
            // -> DivideByZeroException
            this.spaceWidth = Math.Max(GetWidth(' ', lastFont), 1);
            // 选项卡的宽度应为 4*'x'
            this.wideSpaceWidth = Math.Max(spaceWidth, GetWidth('x', lastFont));
        }

        #region Paint functions
        /// <summary>
        /// 重绘
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            // 只是为了确保字体高和字符宽度始终正确。。。
            if (lastFont != TextEditorProperties.Font)
            {
                OptionsChanged();
                base.TextArea.BeginInvoke(new MethodInvoker(base.TextArea.Refresh));
            }

            int horizontalDelta = (int)(textArea.VirtualTop.X * WideSpaceWidth);
            if (horizontalDelta > 0)
            {
                g.SetClip(this.DrawingPosition);
            }

            for (int y = 0; y < (DrawingPosition.Height + VisibleLineDrawingRemainder) / fontHeight + 1; ++y)
            {
                Rectangle lineRectangle = new Rectangle(DrawingPosition.X - horizontalDelta,
                                                        DrawingPosition.Top + y * fontHeight - VisibleLineDrawingRemainder,
                                                        DrawingPosition.Width + horizontalDelta,
                                                        fontHeight);

                if (rect.IntersectsWith(lineRectangle))
                {
                    int fvl = textArea.Document.GetVisibleLine(
                        FirstVisibleLine);
                    int currentLine = textArea.Document.GetFirstLogicalLine(
                        textArea.Document.GetVisibleLine(
                            FirstVisibleLine) + y);
                    PaintDocumentLine(
                        g,
                        currentLine,
                        lineRectangle);
                }
            }

            if (horizontalDelta > 0)
            {
                g.ResetClip();
            }
        }

        /// <summary>
        /// 重绘文档行
        /// </summary>
        /// <param name="g"></param>
        /// <param name="lineNumber"></param>
        /// <param name="lineRectangle"></param>
        void PaintDocumentLine(Graphics g, int lineNumber, Rectangle lineRectangle)
        {
            Debug.Assert(lineNumber >= 0);
            Brush bgColorBrush = GetBgColorBrush(lineNumber);
            Brush backgroundBrush = textArea.Enabled ? bgColorBrush : SystemBrushes.InactiveBorder;

            if (lineNumber >= textArea.Document.TotalNumberOfLines)
            {
                g.FillRectangle(backgroundBrush, lineRectangle);
                if (TextEditorProperties.ShowInvalidLines)
                {
                    DrawInvalidLineMarker(
                        g,
                        lineRectangle.Left,
                        lineRectangle.Top);
                }
                if (TextEditorProperties.ShowVerticalRuler)
                {
                    DrawVerticalRuler(
                        g,
                        lineRectangle);
                }
                //				bgColorBrush.Dispose();
                return;
            }

            float physicalXPos = lineRectangle.X;
            // 不能有一个折叠的wich开始在上面的行，并在这里结束，因为该行是一个新的，
            //必须有一个返回之前，这条线。
            int column = 0;
            physicalColumn = 0;
            if (TextEditorProperties.EnableFolding)
            {
                while (true)
                {
                    List<FoldMarker> starts = textArea.Document.FoldingManager.GetFoldedFoldingsWithStartAfterColumn(lineNumber, column - 1);
                    if (starts == null || starts.Count <= 0)
                    {
                        if (lineNumber < textArea.Document.TotalNumberOfLines)
                        {
                            physicalXPos = PaintLinePart(
                                g,
                                lineNumber,
                                column,
                                textArea.Document.GetLineSegment(lineNumber).Length,
                                lineRectangle,
                                physicalXPos);
                        }
                        break;
                    }
                    // 搜索第一个开始折叠
                    FoldMarker firstFolding = (FoldMarker)starts[0];
                    foreach (FoldMarker fm in starts)
                    {
                        if (fm.StartColumn < firstFolding.StartColumn)
                        {
                            firstFolding = fm;
                        }
                    }
                    starts.Clear();

                    physicalXPos = PaintLinePart(
                        g,
                        lineNumber,
                        column,
                        firstFolding.StartColumn,
                        lineRectangle,
                        physicalXPos);
                    column = firstFolding.EndColumn;
                    lineNumber = firstFolding.EndLine;

                    ColumnRange selectionRange2 = textArea.SelectionManager.GetSelectionAtLine(lineNumber);
                    bool drawSelected = ColumnRange.WholeColumn.Equals(selectionRange2) || firstFolding.StartColumn >= selectionRange2.StartColumn && firstFolding.EndColumn <= selectionRange2.EndColumn;

                    physicalXPos = PaintFoldingText(
                        g,
                        lineNumber,
                        physicalXPos,
                        lineRectangle,
                        firstFolding.FoldText,
                        drawSelected);
                }
            }
            else
            {
                physicalXPos = PaintLinePart(
                    g,
                    lineNumber,
                    0,
                    textArea.Document.GetLineSegment(lineNumber).Length,
                    lineRectangle,
                    physicalXPos);
            }

            if (lineNumber < textArea.Document.TotalNumberOfLines)
            {
                // 线结束后油漆东西
                ColumnRange selectionRange = textArea.SelectionManager.GetSelectionAtLine(lineNumber);
                LineSegment currentLine = textArea.Document.GetLineSegment(lineNumber);
                HighlightColor selectionColor = textArea.Document.HighlightingStrategy.GetColorFor("Selection");

                bool selectionBeyondEOL = selectionRange.EndColumn > currentLine.Length || ColumnRange.WholeColumn.Equals(selectionRange);

                if (TextEditorProperties.ShowEOLMarker)
                {
                    HighlightColor eolMarkerColor = textArea.Document.HighlightingStrategy.GetColorFor("EOLMarkers");
                    physicalXPos += DrawEOLMarker(
                        g,
                        eolMarkerColor.Color,
                        selectionBeyondEOL ? bgColorBrush : backgroundBrush,
                        physicalXPos,
                        lineRectangle.Y);
                }
                else
                {
                    if (selectionBeyondEOL)
                    {
                        g.FillRectangle(
                            BrushRegistry.GetBrush(selectionColor.BackgroundColor),
                            new RectangleF(physicalXPos, lineRectangle.Y, WideSpaceWidth, lineRectangle.Height));
                        physicalXPos += WideSpaceWidth;
                    }
                }

                Brush fillBrush = selectionBeyondEOL && TextEditorProperties.AllowCaretBeyondEOL ? bgColorBrush : backgroundBrush;
                g.FillRectangle(fillBrush,
                                new RectangleF(physicalXPos, lineRectangle.Y, lineRectangle.Width - physicalXPos + lineRectangle.X, lineRectangle.Height));
            }
            if (TextEditorProperties.ShowVerticalRuler)
            {
                DrawVerticalRuler(g, lineRectangle);
            }
            //			bgColorBrush.Dispose();
        }

        bool DrawLineMarkerAtLine(int lineNumber)
        {
            return lineNumber == base.textArea.Caret.Line && textArea.MotherTextAreaControl.TextEditorProperties.LineViewerStyle == LineViewerStyle.FullRow;
        }

        Brush GetBgColorBrush(int lineNumber)
        {
            if (DrawLineMarkerAtLine(lineNumber))
            {
                HighlightColor caretLine = textArea.Document.HighlightingStrategy.GetColorFor("CaretMarker");
                return BrushRegistry.GetBrush(caretLine.Color);
            }
            HLBackground background = (HLBackground)textArea.Document.HighlightingStrategy.GetColorFor("Default");
            Color bgColor = background.BackgroundColor;
            if (textArea.MotherTextAreaControl.TextEditorProperties.UseCustomLine == true)
            {
                bgColor = textArea.Document.CustomLineManager.GetCustomColor(lineNumber, bgColor);
            }
            return BrushRegistry.GetBrush(bgColor);
        }

        float PaintFoldingText(Graphics g, int lineNumber, float physicalXPos, Rectangle lineRectangle, string text, bool drawSelected)
        {
            // TODO: 从突出显示文件获取字体和颜色
            HighlightColor selectionColor = textArea.Document.HighlightingStrategy.GetColorFor("Selection");
            Brush bgColorBrush = drawSelected ? BrushRegistry.GetBrush(selectionColor.BackgroundColor) : GetBgColorBrush(lineNumber);
            Brush backgroundBrush = textArea.Enabled ? bgColorBrush : SystemBrushes.InactiveBorder;

            float wordWidth = MeasureStringWidth(g, text, textArea.Font);
            RectangleF rect = new RectangleF(
                physicalXPos,
                lineRectangle.Y,
                wordWidth,
                lineRectangle.Height - 1);

            g.FillRectangle(backgroundBrush, rect);

            physicalColumn += text.Length;
            g.DrawString(text,
                         textArea.Font,
                         BrushRegistry.GetBrush(drawSelected ? selectionColor.Color : Color.Gray),
                         rect,
                         measureStringFormat);
            g.DrawRectangle(
                BrushRegistry.GetPen(
                drawSelected ? Color.DarkGray : Color.Gray),
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height);

            // 问题的 Bugfix - 透支右矩形线。
            float ceiling = (float)Math.Ceiling(physicalXPos + wordWidth);
            if (ceiling - (physicalXPos + wordWidth) < 0.5)
            {
                ++ceiling;
            }
            return ceiling;
        }

        void DrawMarker(Graphics g, TextMarker marker, RectangleF drawingRect)
        {
            float drawYPos = drawingRect.Bottom - 1;
            float drawXPos = drawingRect.Top - 1;
            switch (marker.TextMarkerType)
            {
                case TextMarkerType.Underlined:
                    {
                        Pen pen = BrushRegistry.GetPen(marker.Color);
                        pen.DashStyle = DashStyle.Solid;
                        pen.Width = 1;
                        g.DrawLine(
                            pen, //BrushRegistry.GetPen(marker.Color),
                            drawingRect.X,
                            drawYPos,
                            drawingRect.Right,
                            drawYPos);

                    }
                    break;
                case TextMarkerType.WaveLine:
                    {
                        int reminder = ((int)drawingRect.X) % 6;
                        Pen pen = BrushRegistry.GetPen(marker.Color);
                        pen.DashStyle = DashStyle.Dot;
                        pen.Width = 1;
                        for (float i = drawingRect.X - reminder; i < drawingRect.Right + reminder; i += 6)
                        {
                            g.DrawLine(
                               pen, //BrushRegistry.GetPen(marker.Color),
                                i,
                                drawYPos + 3 - 4,
                                i + 3,
                                drawYPos + 1 - 4);
                            g.DrawLine(
                                pen, //BrushRegistry.GetPen(marker.Color),
                                i + 3,
                                drawYPos + 1 - 4,
                                i + 6,
                                drawYPos + 3 - 4);
                        }
                    }
                    break;
                case TextMarkerType.SolidBlock:
                    g.FillRectangle(
                        BrushRegistry.GetBrush(marker.Color),
                        drawingRect);
                    break;
                //新加：
                case TextMarkerType.TT:// TT符号
                    {
                    }
                    break;
                case TextMarkerType.Strike:// 文本删除线
                    {
                        Pen pen = BrushRegistry.GetPen(marker.Color);
                        pen.DashStyle = DashStyle.Solid;
                        pen.Width = 1;
                        float y = drawYPos - drawingRect.Height / 2- (drawingRect.Height%5);
                        g.DrawLine(
                       pen,
                       drawingRect.X,
                       y,//drawYPos,
                       drawingRect.Right,
                       y//drawYPos
                         );
                    }
                    break;
                case TextMarkerType.Box:// 边框 bug
                    {
                        Pen pen = new Pen(marker.Color);
                        pen.Width = 1;
                        g.DrawRectangle(
                        pen,
                        drawingRect.X,
                        drawingRect.Y - 1,
                        drawingRect.Width,
                        drawingRect.Height - 1);
                    }
                    break;
                case TextMarkerType.Dash:// 虚线
                    {
                        Pen pen = BrushRegistry.GetPen(marker.Color);
                        pen.DashStyle = DashStyle.Dash;
                        pen.Width = 1;
                        g.DrawLine(
                       pen,
                       drawingRect.X,
                       drawYPos,
                       drawingRect.Right,
                       drawYPos);
                    }
                    break;
                case TextMarkerType.Dots:// 点线
                    {
                        Pen pen = BrushRegistry.GetPen(marker.Color);
                        pen.DashStyle = DashStyle.Dot;
                        pen.Width = 1;
                        g.DrawLine(
                       pen,
                       drawingRect.X,
                       drawYPos,
                       drawingRect.Right,
                       drawYPos);
                    }
                    break;
                case TextMarkerType.DotBox:// 点边框 bug
                    {
                        Pen pen = new Pen(marker.Color);
                        pen.DashStyle = DashStyle.Dot;
                        pen.Width = 1;
                        g.DrawRectangle(
                        pen,
                        drawingRect.X,
                        drawingRect.Y - 1,
                        drawingRect.Width,
                        drawingRect.Height - 1);
                    }
                    break;
                case TextMarkerType.Gradient:// 上 到 下 渐变填充 bug
                    {
                        //线性渐变
                        LinearGradientBrush b = new LinearGradientBrush(
                            drawingRect,
                            Color.FromArgb(60,marker.Color), 
                            Color.FromArgb(00,00,00,00), 90f); 
                        /*g.FillRectangle(
                        b,
                        drawingRect);*/
                        
                        g.FillRectangle(
                        b,//BrushRegistry.GetBrush(marker.Color),
                        drawingRect);
                        b.Dispose(); // 释放资源
                    }
                    break;
                case TextMarkerType.GradientCentre:// 居中渐变填充 bug
                    {

                    }
                    break;
                case TextMarkerType.TextFore:// 绘制文本前景
                    {

                    }
                    break;
                case TextMarkerType.Triangle:// 三角箭头
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// 在给定位置获取标记刷（用于固定块标记）。
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="markers">已找到的所有标记.</param>
        /// <returns>当没有标记时刷子或空.</returns>
        Brush GetMarkerBrushAt(int offset, int length, ref Color foreColor, out List<TextMarker> markers)
        {
            markers = Document.MarkerStrategy.GetMarkers(offset, length);
            foreach (TextMarker marker in markers)
            {
                if (marker.TextMarkerType == TextMarkerType.SolidBlock)
                {
                    if (marker.OverrideForeColor)
                    {
                        foreColor = marker.ForeColor;
                    }
                    return BrushRegistry.GetBrush(marker.Color);
                }
            }
            return null;
        }

        float PaintLinePart(Graphics g, int lineNumber, int startColumn, int endColumn, Rectangle lineRectangle, float physicalXPos)
        {
            bool drawLineMarker = DrawLineMarkerAtLine(lineNumber);
            Brush bgColorBrush = GetBgColorBrush(lineNumber);
            Brush backgroundBrush = textArea.Enabled ? bgColorBrush : SystemBrushes.InactiveBorder;

            HighlightColor selectionColor = textArea.Document.HighlightingStrategy.GetColorFor("Selection");
            ColumnRange selectionRange = textArea.SelectionManager.GetSelectionAtLine(lineNumber);
            HighlightColor tabMarkerColor = textArea.Document.HighlightingStrategy.GetColorFor("TabMarkers");
            HighlightColor spaceMarkerColor = textArea.Document.HighlightingStrategy.GetColorFor("SpaceMarkers");

            LineSegment currentLine = textArea.Document.GetLineSegment(lineNumber);

            int logicalColumn = startColumn;
            //选择背景画刷
            Brush selectionBackgroundBrush = BrushRegistry.GetBrush(selectionColor.BackgroundColor);
            //未选择背景画刷
            Brush unselectedBackgroundBrush = backgroundBrush;

            if (currentLine.Words != null)
            {
                int startword = 0;
                // 搜索开始后的第一个单词哥伦布和更新物理哥伦布，如果一个词是选项卡
                int wordOffset = 0;
                for (; startword < currentLine.Words.Count; ++startword)
                {
                    if (wordOffset >= startColumn)
                    {
                        break;
                    }
                    TextWord currentWord = ((TextWord)currentLine.Words[startword]);
                    if (currentWord.Type == TextWordType.Tab)
                    {
                        ++wordOffset;
                    }
                    else if (currentWord.Type == TextWordType.Space)
                    {
                        ++wordOffset;
                    }
                    else
                    {
                        wordOffset += currentWord.Length;
                    }
                }


                for (int i = startword; i < currentLine.Words.Count && physicalXPos < lineRectangle.Right; ++i)
                {

                    // 如果已经所有的话之前结束哥伦布被绘制：打破
                    if (logicalColumn >= endColumn)
                    {
                        break;
                    }

                    //标记
                    List<TextMarker> markers = Document.MarkerStrategy.GetMarkers(currentLine.Offset + wordOffset);
                    foreach (TextMarker marker in markers)
                    {
                        if (marker.TextMarkerType == TextMarkerType.SolidBlock)
                        {
                            unselectedBackgroundBrush = 
                                BrushRegistry.GetBrush(marker.Color);
                            break;
                        }
                    }


                    // TODO: 切字，如果开始哥伦布或结束科林是在单词中：
                    // 需要折叠wich可以开始或结束在一个字的中间
                    TextWord currentWord = ((TextWord)currentLine.Words[i]);
                    switch (currentWord.Type)
                    {
                        case TextWordType.Space:
                            RectangleF spaceRectangle = new RectangleF(physicalXPos, lineRectangle.Y, (float)Math.Ceiling(SpaceWidth), lineRectangle.Height);

                            Brush spaceBackgroundBrush;
                            Color spaceMarkerForeColor = spaceMarkerColor.Color;
                            if (ColumnRange.WholeColumn.Equals(selectionRange) || logicalColumn >= selectionRange.StartColumn && logicalColumn < selectionRange.EndColumn)
                            {
                                spaceBackgroundBrush = selectionBackgroundBrush;
                            }
                            else
                            {
                                Brush markerBrush = GetMarkerBrushAt(currentLine.Offset + logicalColumn, 1, ref spaceMarkerForeColor, out markers);
                                if (!drawLineMarker && markerBrush != null)
                                {
                                    spaceBackgroundBrush = markerBrush;
                                }
                                else if (!drawLineMarker && currentWord.SyntaxColor != null && currentWord.SyntaxColor.HasBackground)
                                {
                                    spaceBackgroundBrush = BrushRegistry.GetBrush(currentWord.SyntaxColor.BackgroundColor);
                                }
                                else
                                {
                                    spaceBackgroundBrush = unselectedBackgroundBrush;
                                }
                            }
                            g.FillRectangle(spaceBackgroundBrush, spaceRectangle);

                            if (TextEditorProperties.ShowSpaces)
                            {
                                DrawSpaceMarker(g, spaceMarkerForeColor, physicalXPos, lineRectangle.Y);
                            }
                            //
                            foreach (TextMarker marker in markers)
                            {
                                if (marker.TextMarkerType != TextMarkerType.SolidBlock)
                                {
                                    DrawMarker(g, marker, spaceRectangle);
                                }
                            }

                            physicalXPos += SpaceWidth;

                            ++logicalColumn;
                            ++physicalColumn;
                            break;

                        case TextWordType.Tab:

                            physicalColumn += TextEditorProperties.TabIndent;
                            physicalColumn = (physicalColumn / TextEditorProperties.TabIndent) * TextEditorProperties.TabIndent;
                            // 转到下一个标签页
                            float physicalTabEnd = (int)((physicalXPos + MinTabWidth - lineRectangle.X)
                                                         / WideSpaceWidth / TextEditorProperties.TabIndent)
                                * WideSpaceWidth * TextEditorProperties.TabIndent + lineRectangle.X;
                            physicalTabEnd += WideSpaceWidth * TextEditorProperties.TabIndent;
                            RectangleF tabRectangle = new RectangleF(physicalXPos, lineRectangle.Y, (float)Math.Ceiling(physicalTabEnd - physicalXPos), lineRectangle.Height);
                            Color tabMarkerForeColor = tabMarkerColor.Color;

                            if (ColumnRange.WholeColumn.Equals(selectionRange) || logicalColumn >= selectionRange.StartColumn && logicalColumn <= selectionRange.EndColumn - 1)
                            {
                                spaceBackgroundBrush = selectionBackgroundBrush;
                            }
                            else
                            {
                                Brush markerBrush = GetMarkerBrushAt(currentLine.Offset + logicalColumn, 1, ref tabMarkerForeColor, out markers);
                                if (!drawLineMarker && markerBrush != null)
                                {
                                    spaceBackgroundBrush = markerBrush;
                                }
                                else if (!drawLineMarker && currentWord.SyntaxColor != null && currentWord.SyntaxColor.HasBackground)
                                {
                                    spaceBackgroundBrush = BrushRegistry.GetBrush(currentWord.SyntaxColor.BackgroundColor);
                                }
                                else
                                {
                                    spaceBackgroundBrush = unselectedBackgroundBrush;
                                }
                            }
                            g.FillRectangle(spaceBackgroundBrush, tabRectangle);

                            if (TextEditorProperties.ShowTabs)
                            {
                                DrawTabMarker(g, tabMarkerForeColor, physicalXPos, lineRectangle.Y);
                            }
                            //
                            foreach (TextMarker marker in markers)
                            {
                                if (marker.TextMarkerType != TextMarkerType.SolidBlock)
                                {
                                    DrawMarker(g, marker, tabRectangle);
                                }
                            }

                            physicalXPos = physicalTabEnd;

                            ++logicalColumn;
                            break;

                        case TextWordType.Word:
                            string word = currentWord.Word;
                            float lastPos = physicalXPos;

                            Color wordForeColor = currentWord.Color;
                            Brush bgMarkerBrush = GetMarkerBrushAt(currentLine.Offset + logicalColumn, word.Length, ref wordForeColor, out markers);
                            Brush wordBackgroundBrush;
                            if (!drawLineMarker && bgMarkerBrush != null)
                            {
                                wordBackgroundBrush = bgMarkerBrush;
                            }
                            else if (!drawLineMarker && currentWord.SyntaxColor.HasBackground)
                            {
                                wordBackgroundBrush = BrushRegistry.GetBrush(currentWord.SyntaxColor.BackgroundColor);
                            }
                            else
                            {
                                wordBackgroundBrush = unselectedBackgroundBrush;
                            }


                            if (ColumnRange.WholeColumn.Equals(selectionRange) || selectionRange.EndColumn - 1 >= word.Length + logicalColumn &&
                                selectionRange.StartColumn <= logicalColumn)
                            {
                                physicalXPos += DrawDocumentWord(g,
                                                                 word,
                                                                 new Point((int)physicalXPos, lineRectangle.Y),
                                                                 currentWord.Font,
                                                                 selectionColor.HasForgeground ? selectionColor.Color : wordForeColor,
                                                                 selectionBackgroundBrush);
                            }
                            else
                            {
                                if (ColumnRange.NoColumn.Equals(selectionRange)  /* || selectionRange.StartColumn > logicalColumn + word.Length || selectionRange.EndColumn  - 1 <= logicalColumn */)
                                {
                                    physicalXPos += DrawDocumentWord(g,
                                                                     word,
                                                                     new Point((int)physicalXPos, lineRectangle.Y),
                                                                     currentWord.Font,
                                                                     wordForeColor,
                                                                     wordBackgroundBrush);
                                }
                                else
                                {
                                    int offset1 = Math.Min(word.Length, Math.Max(0, selectionRange.StartColumn - logicalColumn));
                                    int offset2 = Math.Max(offset1, Math.Min(word.Length, selectionRange.EndColumn - logicalColumn));

                                    physicalXPos += DrawDocumentWord(g,
                                                                     word.Substring(0, offset1),
                                                                     new Point((int)physicalXPos, lineRectangle.Y),
                                                                     currentWord.Font,
                                                                     wordForeColor,
                                                                     wordBackgroundBrush);

                                    physicalXPos += DrawDocumentWord(g,
                                                                     word.Substring(offset1, offset2 - offset1),
                                                                     new Point((int)physicalXPos, lineRectangle.Y),
                                                                     currentWord.Font,
                                                                     selectionColor.HasForgeground ? selectionColor.Color : wordForeColor,
                                                                     selectionBackgroundBrush);

                                    physicalXPos += DrawDocumentWord(g,
                                                                     word.Substring(offset2),
                                                                     new Point((int)physicalXPos, lineRectangle.Y),
                                                                     currentWord.Font,
                                                                     wordForeColor,
                                                                     wordBackgroundBrush);
                                }
                            }
                            //
                            foreach (TextMarker marker in markers)
                            {
                                if (marker.TextMarkerType != TextMarkerType.SolidBlock)
                                {
                                    DrawMarker(g, marker, new RectangleF(lastPos, lineRectangle.Y, (physicalXPos - lastPos), lineRectangle.Height));
                                }
                            }

                            // 绘制支架高光
                            if (highlight != null)
                            {
                                if (highlight.OpenBrace.Y == lineNumber && highlight.OpenBrace.X == logicalColumn ||
                                    highlight.CloseBrace.Y == lineNumber && highlight.CloseBrace.X == logicalColumn)
                                {
                                    DrawBracketHighlight(g, new Rectangle((int)lastPos, lineRectangle.Y, (int)(physicalXPos - lastPos) - 1, lineRectangle.Height - 1));
                                }
                            }
                            physicalColumn += word.Length;
                            logicalColumn += word.Length;
                            break;
                    }
                }
            }

            return physicalXPos;
        }

        //int num;

        float DrawDocumentWord(Graphics g, string word, Point position, Font font, Color foreColor, Brush backBrush)
        {
            if (word == null || word.Length == 0)
            {
                return 0f;
            }

            if (word.Length > MaximumWordLength)
            {
                float width = 0;
                for (int i = 0; i < word.Length; i += MaximumWordLength)
                {
                    Point pos = position;
                    pos.X += (int)width;
                    if (i + MaximumWordLength < word.Length)
                        width += DrawDocumentWord(g, word.Substring(i, MaximumWordLength), pos, font, foreColor, backBrush);
                    else
                        width += DrawDocumentWord(g, word.Substring(i, word.Length - i), pos, font, foreColor, backBrush);
                }
                return width;
            }

            float wordWidth = MeasureStringWidth(g, word, font);

            //num = ++num % 3;
            g.FillRectangle(backBrush, //num == 0 ? Brushes.LightBlue : num == 1 ? Brushes.LightGreen : Brushes.Yellow,
                            new RectangleF(position.X, position.Y, (float)Math.Ceiling(wordWidth + 1), FontHeight));

            g.DrawString(word,
                         font,
                         BrushRegistry.GetBrush(foreColor),
                         position.X,
                         position.Y,
                         measureStringFormat);
            return wordWidth;
        }

        struct WordFontPair
        {
            string word;
            Font font;
            public WordFontPair(string word, Font font)
            {
                this.word = word;
                this.font = font;
            }
            public override bool Equals(object obj)
            {
                WordFontPair myWordFontPair = (WordFontPair)obj;
                if (!word.Equals(myWordFontPair.word))
                    return false;
                return font.Equals(myWordFontPair.font);
            }

            public override int GetHashCode()
            {
                return word.GetHashCode() ^ font.GetHashCode();
            }
        }

        Dictionary<WordFontPair, float> measureCache = new Dictionary<WordFontPair, float>();

        // 1000个字符后的拆分单词。例如，修复GDI+在很长的单词上崩溃
        //100 KB Base64文件，没有任何断线。
        const int MaximumWordLength = 1000;

        float MeasureStringWidth(Graphics g, string word, Font font)
        {
            float width;

            if (word == null || word.Length == 0)
                return 0;
            if (word.Length > MaximumWordLength)
            {
                width = 0;
                for (int i = 0; i < word.Length; i += MaximumWordLength)
                {
                    if (i + MaximumWordLength < word.Length)
                        width += MeasureStringWidth(g, word.Substring(i, MaximumWordLength), font);
                    else
                        width += MeasureStringWidth(g, word.Substring(i, word.Length - i), font);
                }
                return width;
            }
            if (measureCache.TryGetValue(new WordFontPair(word, font), out width))
            {
                return width;
            }
            if (measureCache.Count > 1000)
            {
                measureCache.Clear();
            }

            // 此处的此代码提供比测量串更好的结果！
            //测量错误的示例行：
            //Txt。从查因德克斯（txt.）获取位置。选择开始）
            //（韦尔达纳 10， 突出显示使 Getp...粗体）->注意"x"和"之间的空间（"
            //这也修复了选择非单空间字体时的"跳跃"字符
            Rectangle rect = new Rectangle(0, 0, 32768, 1000);
            CharacterRange[] ranges = { new CharacterRange(0, word.Length) };
            Region[] regions = new Region[1];
            measureStringFormat.SetMeasurableCharacterRanges(ranges);
            regions = g.MeasureCharacterRanges(word, font, rect, measureStringFormat);
            width = regions[0].GetBounds(g).Right;
            measureCache.Add(new WordFontPair(word, font), width);
            return width;
        }
        #endregion

        #region Conversion Functions
        Dictionary<Font, Dictionary<char, float>> fontBoundCharWidth = new Dictionary<Font, Dictionary<char, float>>();

        /// <summary>
        /// 取字符宽度
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public float GetWidth(char ch, Font font)
        {
            if (!fontBoundCharWidth.ContainsKey(font))
            {
                fontBoundCharWidth.Add(font, new Dictionary<char, float>());
            }
            if (!fontBoundCharWidth[font].ContainsKey(ch))
            {
                using (Graphics g = textArea.CreateGraphics())
                {
                    return GetWidth(g, ch, font);
                }
            }
            return (float)fontBoundCharWidth[font][ch];
        }

        public float GetWidth(Graphics g, char ch, Font font)
        {
            if (!fontBoundCharWidth.ContainsKey(font))
            {
                fontBoundCharWidth.Add(font, new Dictionary<char, float>());
            }
            if (!fontBoundCharWidth[font].ContainsKey(ch))
            {
                //Console.WriteLine("Calculate character width: " + ch);
                fontBoundCharWidth[font].Add(ch, MeasureStringWidth(g, ch.ToString(), font));
            }
            return (float)fontBoundCharWidth[font][ch];
        }

        /// <summary>
        /// 取可视列
        /// </summary>
        /// <param name="logicalLine"></param>
        /// <param name="logicalColumn"></param>
        /// <returns></returns>
        public int GetVisualColumn(int logicalLine, int logicalColumn)
        {
            int column = 0;
            using (Graphics g = textArea.CreateGraphics())
            {
                CountColumns(ref column, 0, logicalColumn, logicalLine, g);
            }
            return column;
        }

        /// <summary>
        /// 快速获取可视列
        /// </summary>
        /// <param name="line"></param>
        /// <param name="logicalColumn"></param>
        /// <returns></returns>
        public int GetVisualColumnFast(LineSegment line, int logicalColumn)
        {
            int lineOffset = line.Offset;
            int tabIndent = Document.TextEditorProperties.TabIndent;
            int guessedColumn = 0;
            for (int i = 0; i < logicalColumn; ++i)
            {
                char ch;
                if (i >= line.Length)
                {
                    ch = ' ';
                }
                else
                {
                    ch = Document.GetCharAt(lineOffset + i);
                }
                switch (ch)
                {
                    case '\t':
                        guessedColumn += tabIndent;
                        guessedColumn = (guessedColumn / tabIndent) * tabIndent;
                        break;
                    default:
                        ++guessedColumn;
                        break;
                }
            }
            return guessedColumn;
        }

        /// <summary>
        /// 返回可视点位置的行/列
        /// </summary>
        public Point GetLogicalPosition(int xPos, int yPos)
        {
            xPos += (int)(textArea.VirtualTop.X * WideSpaceWidth);
            int clickedVisualLine = Math.Max(0, (yPos + this.textArea.VirtualTop.Y) / fontHeight);
            int logicalLine = Document.GetFirstLogicalLine(clickedVisualLine);
            Point pos = GetLogicalColumn(logicalLine, xPos);
            return pos;
        }

        /// <summary>
        /// 返回可视点的逻辑行数
        /// </summary>
        public int GetLogicalLine(Point mousepos)
        {
            int clickedVisualLine = Math.Max(0, (mousepos.Y + this.textArea.VirtualTop.Y) / fontHeight);
            return Document.GetFirstLogicalLine(clickedVisualLine);
        }

        /// <summary>
        /// 获取逻辑列点
        /// </summary>
        /// <param name="firstLogicalLine"></param>
        /// <param name="xPos"></param>
        /// <returns></returns>
        public Point GetLogicalColumn(int firstLogicalLine, int xPos)
        {
            float spaceWidth = WideSpaceWidth;
            LineSegment line = firstLogicalLine < Document.TotalNumberOfLines ? Document.GetLineSegment(firstLogicalLine) : null;
            if (line == null)
            {
                return new Point((int)(xPos / spaceWidth), firstLogicalLine);
            }

            int lineNumber = firstLogicalLine;
            int tabIndent = Document.TextEditorProperties.TabIndent;
            int column = 0;
            int logicalColumn = 0;
            float paintPos = 0;

            List<FoldMarker> starts = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(lineNumber);
            while (true)
            {
                // 保存当前画刷位置
                float oldPaintPos = paintPos;

                // 搜索折叠
                if (starts.Count > 0)
                {
                    foreach (FoldMarker folding in starts)
                    {
                        if (folding.IsFolded && logicalColumn >= folding.StartColumn && (logicalColumn < folding.EndColumn || lineNumber != folding.EndLine))
                        {
                            column += folding.FoldText.Length;
                            paintPos += folding.FoldText.Length * spaceWidth;
                            // 当xPos在折叠标记内的特殊情况
                            if (xPos <= paintPos - (paintPos - oldPaintPos) / 2)
                            {
                                return new Point(logicalColumn, lineNumber);
                            }
                            logicalColumn = folding.EndColumn;
                            if (lineNumber != folding.EndLine)
                            {
                                lineNumber = folding.EndLine;
                                line = Document.GetLineSegment(lineNumber);
                                starts = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(lineNumber);
                            }
                            break;
                        }
                    }
                }

                // --> 没有折叠，继续计数
                char ch = logicalColumn >= line.Length ? ' ' : Document.GetCharAt(line.Offset + logicalColumn);
                switch (ch)
                {
                    case '\t':
                        int oldColumn = column;
                        column += tabIndent;
                        column = (column / tabIndent) * tabIndent;
                        paintPos += (column - oldColumn) * spaceWidth;
                        break;
                    default:
                        paintPos += GetWidth(ch, TextEditorProperties.Font);
                        ++column;
                        break;
                }

                // 当到达画刷位置时，将其还给下一个字符
                if (xPos <= paintPos - (paintPos - oldPaintPos) / 2)
                {
                    return new Point(logicalColumn, lineNumber);
                }

                ++logicalColumn;
            }
        }

        /// <summary>
        /// 返回可视点位置的行/列
        /// </summary>
        public FoldMarker GetFoldMarkerFromPosition(int xPos, int yPos)
        {
            xPos += (int)(textArea.VirtualTop.X * WideSpaceWidth);
            int clickedVisualLine = (yPos + this.textArea.VirtualTop.Y) / fontHeight;
            int logicalLine = Document.GetFirstLogicalLine(clickedVisualLine);
            return GetFoldMarkerFromColumn(logicalLine, xPos);
        }

        /// <summary>
        /// 获取指定列的折叠标记
        /// </summary>
        /// <param name="firstLogicalLine"></param>
        /// <param name="xPos"></param>
        /// <returns></returns>
        public FoldMarker GetFoldMarkerFromColumn(int firstLogicalLine, int xPos)
        {
            LineSegment line = firstLogicalLine < Document.TotalNumberOfLines ? Document.GetLineSegment(firstLogicalLine) : null;
            if (line == null)
            {
                return null;
            }

            int lineNumber = firstLogicalLine;
            int tabIndent = Document.TextEditorProperties.TabIndent;
            int column = 0;
            int logicalColumn = 0;
            float paintPos = 0;

            List<FoldMarker> starts = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(lineNumber);
            while (true)
            {
                // 保存当前油漆位置 save current paint position
                float oldPaintPos = paintPos;

                // 搜索折叠
                if (starts.Count > 0)
                {
                    foreach (FoldMarker folding in starts)
                    {
                        if (folding.IsFolded && logicalColumn >= folding.StartColumn && (logicalColumn < folding.EndColumn || lineNumber != folding.EndLine))
                        {
                            column += folding.FoldText.Length;
                            paintPos += folding.FoldText.Length * WideSpaceWidth;
                            // 当xPos在折叠标记内的特殊情况
                            if (xPos <= paintPos)
                            {
                                return folding;
                            }
                            logicalColumn = folding.EndColumn;
                            if (lineNumber != folding.EndLine)
                            {
                                lineNumber = folding.EndLine;
                                line = Document.GetLineSegment(lineNumber);
                                starts = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(lineNumber);
                            }
                            break;
                        }
                    }
                }

                // --> 没有折叠，继续计数
                char ch = logicalColumn >= line.Length ? ' ' : Document.GetCharAt(line.Offset + logicalColumn);
                switch (ch)
                {
                    case '\t':
                        int oldColumn = column;
                        column += tabIndent;
                        column = (column / tabIndent) * tabIndent;
                        paintPos += (column - oldColumn) * WideSpaceWidth;
                        break;
                    default:
                        paintPos += GetWidth(ch, TextEditorProperties.Font);
                        ++column;
                        break;
                }

                // 当到达paint位置时，将其还给下一个字符
                if (xPos <= paintPos - (paintPos - oldPaintPos) / 2)
                {
                    return null;
                }

                ++logicalColumn;
            }
        }

        const float MinTabWidth = 4;

        float CountColumns(ref int column, int start, int end, int logicalLine, Graphics g)
        {
            if (start > end)
                throw new ArgumentException("start > end");
            if (start == end)
                return 0;
            float spaceWidth = SpaceWidth;
            float drawingPos = 0;
            int tabIndent = Document.TextEditorProperties.TabIndent;
            LineSegment currentLine = Document.GetLineSegment(logicalLine);
            List<TextWord> words = currentLine.Words;
            if (words == null)
                return 0;
            int wordCount = words.Count;
            int wordOffset = 0;
            for (int i = 0; i < wordCount; i++)
            {
                TextWord word = words[i];
                if (wordOffset >= end)
                    break;
                if (wordOffset + word.Length < start)
                    continue;
                switch (word.Type)
                {
                    case TextWordType.Space:
                        drawingPos += spaceWidth;
                        break;
                    case TextWordType.Tab:
                        // 转到下一个选项卡位置
                        drawingPos = (int)((drawingPos + MinTabWidth) / tabIndent / WideSpaceWidth) * tabIndent * WideSpaceWidth;
                        drawingPos += tabIndent * WideSpaceWidth;
                        break;
                    case TextWordType.Word:
                        int wordStart = Math.Max(wordOffset, start);
                        int wordLength = Math.Min(wordOffset + word.Length, end) - wordStart;
                        string text = Document.GetText(currentLine.Offset + wordStart, wordLength);
                        drawingPos += MeasureStringWidth(g, text, word.Font ?? TextEditorProperties.Font);
                        break;
                }
                wordOffset += word.Length;
            }
            for (int j = currentLine.Length; j < end; j++)
            {
                drawingPos += WideSpaceWidth;
            }
            // 在列计算中添加一个像素以解释浮点计算错误
            column += (int)((drawingPos + 1) / WideSpaceWidth);

            /* OLD Code (does not work for fonts like Verdana)旧代码（不适用于像韦尔达纳这样的字体）
			for (int j = start; j < end; ++j) {
				char ch;
				if (j >= line.Length) {
					ch = ' ';
				} else {
					ch = Document.GetCharAt(line.Offset + j);
				}
				
				switch (ch) {
					case '\t':
						int oldColumn = column;
						column += tabIndent;
						column = (column / tabIndent) * tabIndent;
						drawingPos += (column - oldColumn) * spaceWidth;
						break;
					default:
						++column;
						TextWord word = line.GetWord(j);
						if (word == null || word.Font == null) {
							drawingPos += GetWidth(ch, TextEditorProperties.Font);
						} else {
							drawingPos += GetWidth(ch, word.Font);
						}
						break;
				}
			}
			//*/
            return drawingPos;
        }

        public int GetDrawingXPos(int logicalLine, int logicalColumn)
        {
            List<FoldMarker> foldings = Document.FoldingManager.GetTopLevelFoldedFoldings();
            int i;
            FoldMarker f = null;
            // 搜索最后的折叠，这是中间
            for (i = foldings.Count - 1; i >= 0; --i)
            {
                f = foldings[i];
                if (f.StartLine < logicalLine || f.StartLine == logicalLine && f.StartColumn < logicalColumn)
                {
                    break;
                }
                FoldMarker f2 = foldings[i / 2];
                if (f2.StartLine > logicalLine || f2.StartLine == logicalLine && f2.StartColumn >= logicalColumn)
                {
                    i /= 2;
                }
            }
            int lastFolding = 0;
            int firstFolding = 0;
            int column = 0;
            int tabIndent = Document.TextEditorProperties.TabIndent;
            float drawingPos;
            Graphics g = textArea.CreateGraphics();
            // 如果没有折叠是干预
            if (f == null || !(f.StartLine < logicalLine || f.StartLine == logicalLine && f.StartColumn < logicalColumn))
            {
                drawingPos = CountColumns(ref column, 0, logicalColumn, logicalLine, g);
                return (int)(drawingPos - textArea.VirtualTop.X * WideSpaceWidth);
            }

            // 如果逻辑线/逻辑线在折叠中
            if (f.EndLine > logicalLine || f.EndLine == logicalLine && f.EndColumn > logicalColumn)
            {
                logicalColumn = f.StartColumn;
                logicalLine = f.StartLine;
                --i;
            }
            lastFolding = i;

            // 向后搜索，直到重新安排新的可见线
            for (; i >= 0; --i)
            {
                f = (FoldMarker)foldings[i];
                if (f.EndLine < logicalLine)
                { // 到达一个新的可见线的开始
                    break;
                }
            }
            firstFolding = i + 1;

            if (lastFolding < firstFolding)
            {
                drawingPos = CountColumns(ref column, 0, logicalColumn, logicalLine, g);
                return (int)(drawingPos - textArea.VirtualTop.X * WideSpaceWidth);
            }

            int foldEnd = 0;
            drawingPos = 0;
            for (i = firstFolding; i <= lastFolding; ++i)
            {
                f = foldings[i];
                drawingPos += CountColumns(ref column, foldEnd, f.StartColumn, f.StartLine, g);
                foldEnd = f.EndColumn;
                column += f.FoldText.Length;
                drawingPos += MeasureStringWidth(g, f.FoldText, TextEditorProperties.Font);
            }
            drawingPos += CountColumns(ref column, foldEnd, logicalColumn, logicalLine, g);
            g.Dispose();
            return (int)(drawingPos - textArea.VirtualTop.X * WideSpaceWidth);
        }
        #endregion

        #region DrawHelper functions
        /// <summary>
        /// 高亮匹配括号绘制
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        //Edit:DresnClor
        void DrawBracketHighlight(Graphics g, Rectangle rect)
        {
            Color color = Color.FromArgb(50, 0, 0, 255);
            Color r = color;
            if (HLStrategy.Properties.ContainsKey("DrawBracketColor"))
            {
                HighlightColor hcolor = HLStrategy.GetHLColor(
                    HLStrategy.Properties["DrawBracketColor"]);
                color = hcolor.Color;
                r = hcolor.BackgroundColor;
            }
            //填充矩形
            g.FillRectangle(BrushRegistry.GetBrush(
                color),
                 rect);
            if (HLStrategy.Properties.ContainsKey("DrawBracketRectang"))
                if (HLStrategy.Properties["DrawBracketRectang"] == "true")
                {
                    //绘制边框
                    g.DrawRectangle(new Pen(r), rect);
                }
        }

        //绘制无效行
        //Edit:DresnClor
        void DrawInvalidLineMarker(Graphics g, float x, float y)
        {
            HighlightColor invalidLinesColor =
                textArea.Document.HighlightingStrategy.GetColorFor(
                    "InvalidLines");
            g.DrawString(
                "~",
                invalidLinesColor.Font,
                BrushRegistry.GetBrush(invalidLinesColor.Color),
                x,
                y,
                measureStringFormat);
        }

        //绘制空格标记
        //Edit:DresnClor
        void DrawSpaceMarker(Graphics g, Color color, float x, float y)
        {
            HighlightColor spaceMarkerColor =
                textArea.Document.HighlightingStrategy.GetColorFor(
                    "SpaceMarkers");
            //原符号：\u00B7
            g.DrawString(
                "·",
                spaceMarkerColor.Font,
                BrushRegistry.GetBrush(color),
                x , //符号会向右偏移
                y,
                measureStringFormat);
        }
        //绘制选项卡标记
        //Edit:DresnClor
        void DrawTabMarker(Graphics g, Color color, float x, float y)
        {
            HighlightColor tabMarkerColor =
                textArea.Document.HighlightingStrategy.GetColorFor(
                    "TabMarkers");
            //原符号：\u00BB
            g.DrawString(
                "→",
                tabMarkerColor.Font,
                BrushRegistry.GetBrush(color),
                x,
                y,
                measureStringFormat);
        }

        //绘制结束符标记
        //Edit:DresnClor
        float DrawEOLMarker(Graphics g, Color color, Brush backBrush, float x, float y)
        {
            //原符号：\u00B6
            float width = GetWidth('\u00B6', TextEditorProperties.Font);
            g.FillRectangle(backBrush,
                            new RectangleF(
                                x,
                                y,
                                width,
                                fontHeight));

            HighlightColor eolMarkerColor =
                textArea.Document.HighlightingStrategy.GetColorFor(
                    "EOLMarkers");
            g.DrawString(
                "\u00B6",
                eolMarkerColor.Font,
                BrushRegistry.GetBrush(color),
                x,
                y,
                measureStringFormat);
            return width;
        }

        //绘制垂直标尺
        void DrawVerticalRuler(Graphics g, Rectangle lineRectangle)
        {
            if (TextEditorProperties.VerticalRulerRow < textArea.VirtualTop.X)
            {
                return;
            }
            HighlightColor vRulerColor =
                textArea.Document.HighlightingStrategy.GetColorFor(
                    "VRuler");

            int xpos = (int)(drawingPosition.Left + WideSpaceWidth * (TextEditorProperties.VerticalRulerRow - textArea.VirtualTop.X));
            g.DrawLine(BrushRegistry.GetPen(vRulerColor.Color),
                       xpos,
                       lineRectangle.Top,
                       xpos,
                       lineRectangle.Bottom);
        }
        #endregion
    }
}
