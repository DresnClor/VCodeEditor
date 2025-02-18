﻿using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VCodeEditor.Document;

namespace VCodeEditor
{
    /// <summary>
    /// 断点栏
    /// </summary>
    public class BreakpointBarMargin : AbstractMargin
    {
        const int iconBarWidth = 18;

        static readonly Size iconBarSize = new Size(iconBarWidth, -1);

        /// <summary>
        /// 大小
        /// </summary>
        public override Size Size
        {
            get
            {
                return iconBarSize;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return textArea.TextEditorProperties.IsBreakpointBarVisible;
            }
        }


        public BreakpointBarMargin(TextArea textArea) : base(textArea)
        {
        }

        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }
            // paint background
            g.FillRectangle(SystemBrushes.Control, new Rectangle(drawingPosition.X, rect.Top, drawingPosition.Width - 1, rect.Height));
            //g.DrawLine(SystemPens.ControlDark, base.drawingPosition.Right - 1, rect.Top, base.drawingPosition.Right - 1, rect.Bottom);

            // paint icons
          /*  foreach (Bookmark mark in textArea.Document.BookmarkManager.Marks)
            {
                int lineNumber = textArea.Document.GetVisibleLine(mark.LineNumber);
                int lineHeight = textArea.TextView.FontHeight;
                int yPos = (int)(lineNumber * lineHeight) - textArea.VirtualTop.Y;
                if (IsLineInsideRegion(yPos, yPos + lineHeight, rect.Y, rect.Bottom))
                {
                    if (lineNumber == textArea.Document.GetVisibleLine(mark.LineNumber - 1))
                    {
                        // 标记位于折叠区域内，请不要绘制
                        continue;
                    }
                    mark.Draw(this, g, new Point(0, yPos));
                }
            }*/
            base.Paint(g, rect);
        }

        public override void HandleMouseDown(Point mousePos, MouseButtons mouseButtons)
        {
            List<Bookmark> marks = textArea.Document.BookmarkManager.Marks;
            int oldCount = marks.Count;
            foreach (Bookmark mark in marks)
            {
                int lineNumber = textArea.Document.GetVisibleLine(mark.LineNumber);
                int fontHeight = textArea.TextView.FontHeight;
                int yPos = lineNumber * fontHeight - textArea.VirtualTop.Y;
                if (mousePos.Y >= yPos && mousePos.Y < yPos + fontHeight)
                {
                    if (lineNumber == textArea.Document.GetVisibleLine(mark.LineNumber - 1))
                    {
                        // 标记位于折叠区域内，无法单击
                        continue;
                    }
                    mark.Click(textArea, new MouseEventArgs(mouseButtons, 1, mousePos.X, mousePos.Y, 0));
                    if (oldCount != marks.Count)
                    {
                        textArea.UpdateLine(lineNumber);
                    }
                    return;
                }
            }
            base.HandleMouseDown(mousePos, mouseButtons);
        }

        #region Drawing functions
        public void DrawBreakpoint(Graphics g, int y, bool isEnabled, bool willBeHit)
        {
            int diameter = Math.Min(iconBarWidth - 2, textArea.TextView.FontHeight);
            Rectangle rect = new Rectangle(1,
                                           y + (textArea.TextView.FontHeight - diameter) / 2,
                                           diameter,
                                           diameter);


            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (PathGradientBrush pthGrBrush = new PathGradientBrush(path))
                {
                    pthGrBrush.CenterPoint = new PointF(rect.Left + rect.Width / 3, rect.Top + rect.Height / 3);
                    pthGrBrush.CenterColor = Color.MistyRose;
                    Color[] colors = { willBeHit ? Color.Firebrick : Color.Olive };
                    pthGrBrush.SurroundColors = colors;

                    if (isEnabled)
                    {
                        g.FillEllipse(pthGrBrush, rect);
                    }
                    else
                    {
                        g.FillEllipse(SystemBrushes.Control, rect);
                        using (Pen pen = new Pen(pthGrBrush))
                        {
                            g.DrawEllipse(pen, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2));
                        }
                    }
                }
            }
        }

        public void DrawBookmark(Graphics g, int y, bool isEnabled)
        {
            int delta = textArea.TextView.FontHeight / 8;
            Rectangle rect = new Rectangle(1, y + delta, base.drawingPosition.Width - 4, textArea.TextView.FontHeight - delta * 2);

            if (isEnabled)
            {
                //绘制书签内部
                /*using (Brush brush = new LinearGradientBrush(new Point(rect.Left, rect.Top),
				                                             new Point(rect.Right, rect.Bottom),
				                                             Color.SkyBlue,
				                                             Color.White)) {
					FillRoundRect(g, brush, rect);
				}*/
                //使用本地图片作为书签标志
                g.DrawImage(Resource.Bookmark, rect.Location);
            }
            else
            {
                FillRoundRect(g, Brushes.White, rect);
            }
            //绘制书签边框
            /*using (Brush brush = new LinearGradientBrush(new Point(rect.Left, rect.Top),
			                                             new Point(rect.Right, rect.Bottom),
			                                             Color.SkyBlue,
			                                             Color.Blue)) {
				using (Pen pen = new Pen(brush)) {
					DrawRoundRect(g, pen, rect);
				}
			}*/
        }

        public void DrawArrow(Graphics g, int y)
        {
            int delta = textArea.TextView.FontHeight / 8;
            Rectangle rect = new Rectangle(1, y + delta, base.drawingPosition.Width - 4, textArea.TextView.FontHeight - delta * 2);
            using (Brush brush = new LinearGradientBrush(new Point(rect.Left, rect.Top),
                                                         new Point(rect.Right, rect.Bottom),
                                                         Color.LightYellow,
                                                         Color.Yellow))
            {
                FillArrow(g, brush, rect);
            }

            using (Brush brush = new LinearGradientBrush(new Point(rect.Left, rect.Top),
                                                         new Point(rect.Right, rect.Bottom),
                                                         Color.Yellow,
                                                         Color.Brown))
            {
                using (Pen pen = new Pen(brush))
                {
                    DrawArrow(g, pen, rect);
                }
            }
        }

        GraphicsPath CreateArrowGraphicsPath(Rectangle r)
        {
            GraphicsPath gp = new GraphicsPath();
            int halfX = r.Width / 2;
            int halfY = r.Height / 2;
            gp.AddLine(r.X, r.Y + halfY / 2, r.X + halfX, r.Y + halfY / 2);
            gp.AddLine(r.X + halfX, r.Y + halfY / 2, r.X + halfX, r.Y);
            gp.AddLine(r.X + halfX, r.Y, r.Right, r.Y + halfY);
            gp.AddLine(r.Right, r.Y + halfY, r.X + halfX, r.Bottom);
            gp.AddLine(r.X + halfX, r.Bottom, r.X + halfX, r.Bottom - halfY / 2);
            gp.AddLine(r.X + halfX, r.Bottom - halfY / 2, r.X, r.Bottom - halfY / 2);
            gp.AddLine(r.X, r.Bottom - halfY / 2, r.X, r.Y + halfY / 2);
            gp.CloseFigure();
            return gp;
        }

        GraphicsPath CreateRoundRectGraphicsPath(Rectangle r)
        {
            GraphicsPath gp = new GraphicsPath();
            int radius = r.Width / 2;
            gp.AddLine(r.X + radius, r.Y, r.Right - radius, r.Y);
            gp.AddArc(r.Right - radius, r.Y, radius, radius, 270, 90);

            gp.AddLine(r.Right, r.Y + radius, r.Right, r.Bottom - radius);
            gp.AddArc(r.Right - radius, r.Bottom - radius, radius, radius, 0, 90);

            gp.AddLine(r.Right - radius, r.Bottom, r.X + radius, r.Bottom);
            gp.AddArc(r.X, r.Bottom - radius, radius, radius, 90, 90);

            gp.AddLine(r.X, r.Bottom - radius, r.X, r.Y + radius);
            gp.AddArc(r.X, r.Y, radius, radius, 180, 90);

            gp.CloseFigure();
            return gp;
        }

        void DrawRoundRect(Graphics g, Pen p, Rectangle r)
        {
            using (GraphicsPath gp = CreateRoundRectGraphicsPath(r))
            {
                g.DrawPath(p, gp);
            }
        }

        void FillRoundRect(Graphics g, Brush b, Rectangle r)
        {
            using (GraphicsPath gp = CreateRoundRectGraphicsPath(r))
            {
                g.FillPath(b, gp);
            }
        }

        void DrawArrow(Graphics g, Pen p, Rectangle r)
        {
            using (GraphicsPath gp = CreateArrowGraphicsPath(r))
            {
                g.DrawPath(p, gp);
            }
        }

        void FillArrow(Graphics g, Brush b, Rectangle r)
        {
            using (GraphicsPath gp = CreateArrowGraphicsPath(r))
            {
                g.FillPath(b, gp);
            }
        }

        #endregion

        static bool IsLineInsideRegion(int top, int bottom, int regionTop, int regionBottom)
        {
            if (top >= regionTop && top <= regionBottom)
            {
                // .区域重叠线的顶边缘
                return true;
            }
            else if (regionTop > top && regionTop < bottom)
            {
                // .区域顶部边缘内线
                return true;
            }
            return false;
        }
    }
}
