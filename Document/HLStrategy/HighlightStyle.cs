// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
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

namespace VCodeEditor.Document
{
    /// <summary>
    /// 高亮样式
    /// </summary>
    public class HighlightStyle
    {
        bool systemColor = false;
        string systemColorName = null;

        bool systemBgColor = false;
        string systemBgColorName = null;

        Color color;
        Color backgroundcolor = System.Drawing.Color.WhiteSmoke;

        Font font;

        bool bold = false;
        bool italic = false;
        bool hasForgeground = false;
        bool hasBackground = false;
        /// <summary>
        /// 从xml元素创建高亮颜色
        /// </summary>
        public HighlightStyle(XmlElement el)
        {
            Debug.Assert(el != null, "VCodeEditor.Document.SyntaxColor(XmlElement el) : el == null");
            if (el.Attributes["name"] != null)
            {//名称
                this.Name = el.Attributes["name"].InnerText;
            }
            else
            {
                this.Name = "";
            }
            if (el.Attributes["bold"] != null)
            {//粗体
                bold = Boolean.Parse(el.Attributes["bold"].InnerText);
            }

            if (el.Attributes["italic"] != null)
            {//斜体
                italic = Boolean.Parse(el.Attributes["italic"].InnerText);
            }

            if (el.Attributes["font"] != null)
            {
                string[] descr = el.Attributes["font"].InnerText.Split(new char[] { ',', '=' });
                if (descr.Length == 4)
                {
                    FontStyle fontStyle = FontStyle.Regular;
                    if (bold)
                        fontStyle = (FontStyle)fontStyle | FontStyle.Bold;
                    if (italic)
                        fontStyle = (FontStyle)fontStyle | FontStyle.Italic;
                    FontFamily fontFamily;
                    if (descr[0] == "Ref")
                    {
                        fontFamily = FontContainer.GetPrivateFont(descr[1]);
                    }
                    else
                    {
                        fontFamily = new FontFamily(descr[1]);
                    }
                    if (fontFamily != null)
                        if (fontFamily.IsStyleAvailable(fontStyle))
                            font = new Font(fontFamily, float.Parse(descr[3]), fontStyle);
                }
            }

            if (el.Attributes["color"] != null)
            {//颜色
                string c = el.Attributes["color"].InnerText;
                if (c[0] == '#')
                {
                    color = ParseColor(c);
                }
                else if (c.StartsWith("SystemColors."))
                {
                    systemColor = true;
                    systemColorName = c.Substring("SystemColors.".Length);
                }
                else
                {
                    color = (Color)(Color.GetType()).InvokeMember(c, BindingFlags.GetProperty, null, Color, new object[0]);
                }
                hasForgeground = true;
            }
            else
            {
                color = Color.Transparent; // to set it to the default value.
            }

            if (el.Attributes["bgcolor"] != null)
            {//背景颜色
                string c = el.Attributes["bgcolor"].InnerText;
                if (c[0] == '#')
                {
                    backgroundcolor = ParseColor(c);
                }
                else if (c.StartsWith("SystemColors."))
                {
                    systemBgColor = true;
                    systemBgColorName = c.Substring("SystemColors.".Length);
                }
                else
                {
                    backgroundcolor = (Color)(Color.GetType()).InvokeMember(c, BindingFlags.GetProperty, null, Color, new object[0]);
                }
                hasBackground = true;
            }
        }

        /// <summary>
        /// 从xml元素创建高亮颜色，如果xml元素未指定，则使用默认
        /// </summary>
        public HighlightStyle(XmlElement el, HighlightStyle defaultColor)
        {
            Debug.Assert(el != null, "VCodeEditor.Document.SyntaxColor(XmlElement el) : el == null");
            if (el.Attributes["name"] != null)
            {//名称
                this.Name = el.Attributes["name"].InnerText;
            }
            else
            {
                this.Name = "";
            }
            if (el.Attributes["bold"] != null)
            {
                bold = Boolean.Parse(el.Attributes["bold"].InnerText);
            }
            else
            {
                bold = defaultColor.Bold;
            }

            if (el.Attributes["italic"] != null)
            {
                italic = Boolean.Parse(el.Attributes["italic"].InnerText);
            }
            else
            {
                italic = defaultColor.Italic;
            }

            if (el.Attributes["font"] != null)
            {
                string[] descr = el.Attributes["font"].InnerText.Split(new char[] { ',', '=' });
                if (descr.Length == 4)
                {
                    FontStyle fontStyle = FontStyle.Regular;
                    if (bold)
                        fontStyle = (FontStyle)fontStyle | FontStyle.Bold;
                    if (italic)
                        fontStyle = (FontStyle)fontStyle | FontStyle.Italic;
                    FontFamily fontFamily;
                    if (descr[0] == "Ref")
                    {
                        fontFamily = FontContainer.GetPrivateFont(descr[1]);
                    }
                    else
                    {
                        fontFamily = new FontFamily(descr[1]);
                    }
                    if (fontFamily != null)
                        if (fontFamily.IsStyleAvailable(fontStyle))
                            font = new Font(fontFamily, float.Parse(descr[3]), fontStyle);
                }
            }

            if (el.Attributes["color"] != null)
            {
                string c = el.Attributes["color"].InnerText;
                if (c[0] == '#')
                {
                    color = ParseColor(c);
                }
                else if (c.StartsWith("SystemColors."))
                {
                    systemColor = true;
                    systemColorName = c.Substring("SystemColors.".Length);
                }
                else
                {
                    color = (Color)(Color.GetType()).InvokeMember(
                        c,
                        BindingFlags.GetProperty,
                        null,
                        Color,
                        new object[0]);
                }
                hasForgeground = true;
            }
            else
            {
                color = defaultColor.color;
            }

            if (el.Attributes["bgcolor"] != null)
            {
                string c = el.Attributes["bgcolor"].InnerText;
                if (c[0] == '#')
                {
                    backgroundcolor = ParseColor(c);
                }
                else if (c.StartsWith("SystemColors."))
                {
                    systemBgColor = true;
                    systemBgColorName = c.Substring("SystemColors.".Length);
                }
                else
                {
                    backgroundcolor = (Color)(Color.GetType()).InvokeMember(c, BindingFlags.GetProperty, null, Color, new object[0]);
                }
                hasBackground = true;
            }
            else
            {
                backgroundcolor = defaultColor.BackgroundColor;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="HighlightStyle"/>
        /// </summary>
        public HighlightStyle(Color color, bool bold, bool italic)
        {
            hasForgeground = true;
            this.color = color;
            this.bold = bold;
            this.italic = italic;
        }

        /// <summary>
        /// Creates a new instance of <see cref="HighlightStyle"/>
        /// </summary>
        public HighlightStyle(Color color, Color backgroundcolor, bool bold, bool italic)
        {
            hasForgeground = true;
            hasBackground = true;
            this.color = color;
            this.backgroundcolor = backgroundcolor;
            this.bold = bold;
            this.italic = italic;
        }


        /// <summary>
        /// Creates a new instance of <see cref="HighlightStyle"/>
        /// </summary>
        public HighlightStyle(string systemColor, string systemBackgroundColor, bool bold, bool italic)
        {
            hasForgeground = true;
            hasBackground = true;

            this.systemColor = true;
            systemColorName = systemColor;

            systemBgColor = true;
            systemBgColorName = systemBackgroundColor;

            this.bold = bold;
            this.italic = italic;
        }
        public bool HasForgeground
        {
            get
            {
                return hasForgeground;
            }
        }

        public bool HasBackground
        {
            get
            {
                return hasBackground;
            }
        }

        /// <summary>
        /// 颜色名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <value>
        /// 粗体
        /// </value>
        public bool Bold
        {
            get
            {
                return bold;
            }
        }

        /// <value>
        /// 斜体
        /// </value>
        public bool Italic
        {
            get
            {
                return italic;
            }
        }

        /// <value>
        /// 背景颜色
        /// </value>
        public Color BackgroundColor
        {
            get
            {
                if (!systemBgColor)
                {
                    return backgroundcolor;
                }
                return ParseColorString(systemBgColorName);
            }
        }

        /// <value>
        /// 前景颜色
        /// </value>
        public Color Color
        {
            get
            {
                if (!systemColor)
                {
                    return color;
                }
                return ParseColorString(systemColorName);
            }
        }

        /// <value>
        /// 字体
        /// </value>
        public Font Font
        {
            get
            {
                if (font != null)
                    return font;
                if (Bold)
                {
                    return Italic ? FontContainer.BoldItalicFont : FontContainer.BoldFont;
                }
                return Italic ? FontContainer.ItalicFont : FontContainer.DefaultFont;
            }
        }

        Color ParseColorString(string colorName)
        {
            string[] cNames = colorName.Split('*');
            PropertyInfo myPropInfo = typeof(System.Drawing.SystemColors).GetProperty(cNames[0], BindingFlags.Public |
                                                                                                 BindingFlags.Instance | BindingFlags.Static);
            Color c = (Color)myPropInfo.GetValue(null, null);

            if (cNames.Length == 2)
            {
                // hack : can't figure out how to parse doubles with '.' (culture info might set the '.' to ',')
                double factor = Double.Parse(cNames[1]) / 100;
                c = Color.FromArgb((int)((double)c.R * factor), (int)((double)c.G * factor), (int)((double)c.B * factor));
            }

            return c;
        }



        static Color ParseColor(string c)
        {
            int a = 255;
            int offset = 0;
            if (c.Length > 7)
            {
                offset = 2;
                a = Int32.Parse(c.Substring(1, 2), NumberStyles.HexNumber);
            }

            int r = Int32.Parse(c.Substring(1 + offset, 2), NumberStyles.HexNumber);
            int g = Int32.Parse(c.Substring(3 + offset, 2), NumberStyles.HexNumber);
            int b = Int32.Parse(c.Substring(5 + offset, 2), NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Converts a <see cref="HighlightStyle"/> instance to string (for debug purposes)
        /// </summary>
        public override string ToString()
        {
            return "[HighlightStyle: Name = " + Font.Name +
                                  ", Bold = " + Bold +
                                  ", Italic = " + Italic +
                                  ", Color = " + Color +
                                  ", BackgroundColor = " + BackgroundColor + "]";
        }
    }
}
