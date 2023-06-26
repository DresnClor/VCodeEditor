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
using System.Drawing.Text;

namespace MeltuiCodeEditor.Document
{
    /// <summary>
    /// 字体容器
    /// </summary>
    public class FontContainer
    {
        static FontContainer()
        {
            DefaultFont = new Font("Courier New", 10.0f);
            PriFontCollection = new PrivateFontCollection();
            Fonts = new Dictionary<string, Font>();
        }
        static Font defaultfont = null;
        static Font boldfont = null;
        static Font italicfont = null;
        static Font bolditalicfont = null;
        /// <summary>
        /// 私有字体集合
        /// </summary>
        static PrivateFontCollection PriFontCollection;
        /// <summary>
        /// 私有字体列表
        /// </summary>
        static Dictionary<string, Font> Fonts;

        /// <value>
        /// 默认粗体字体
        /// </value>
        public static Font BoldFont
        {
            get
            {
                Debug.Assert(boldfont != null, "VCodeEditor.Document.FontContainer : boldfont == null");
                return boldfont;
            }
        }

        /// <value>
        /// 默认斜体字体
        /// </value>
        public static Font ItalicFont
        {
            get
            {
                Debug.Assert(italicfont != null, "VCodeEditor.Document.FontContainer : italicfont == null");
                return italicfont;
            }
        }

        /// <value>
        /// 默认粗体+斜体字体
        /// </value>
        public static Font BoldItalicFont
        {
            get
            {
                Debug.Assert(bolditalicfont != null, "VCodeEditor.Document.FontContainer : bolditalicfont == null");
                return bolditalicfont;
            }
        }

        /// <value>
        /// 默认字体
        /// </value>
        public static Font DefaultFont
        {
            get
            {
                return defaultfont;
            }
            set
            {
                if (defaultfont != null)
                    defaultfont.Dispose();
                defaultfont = value;
                if (boldfont != null)
                    boldfont.Dispose();
                boldfont = new Font(defaultfont, FontStyle.Bold);
                if (italicfont != null)
                    italicfont.Dispose();
                italicfont = new Font(defaultfont, FontStyle.Italic);
                if (bolditalicfont != null)
                    bolditalicfont.Dispose();
                bolditalicfont = new Font(defaultfont, FontStyle.Bold | FontStyle.Italic);
            }
        }

        //		static void CheckFontChange(object sender, PropertyEventArgs e)
        //		{
        //			if (e.Key == "DefaultFont") {
        //				DefaultFont = ParseFont(e.NewValue.ToString());
        //			}
        //		}

        /// <summary>
        /// 解析字体
        /// <para />
        /// 系统字体：Name=字体名称,Size=字体大小
        /// 资源引用：Ref=字体引用名称,Size=字体大小
        /// </summary>
        /// <param name="font">字体配置</param>
        /// <returns>失败返回null</returns>
        public static Font ParseFont(string font)
        {
            string[] descr = font.Split(new char[] { ',', '=' });
            if (descr.Length < 4)
                return null;
            //返回加载的资源字体
            if (descr[0] == "Ref")
            {
                return new Font(GetPrivateFont(descr[1]), float.Parse(descr[3]));
            }
            //返回系统字体
            return new Font(descr[1], float.Parse(descr[3]));
        }

        /*/// <summary>
        /// 获取私有字体
        /// </summary>
        /// <param name="name">私有字体名称</param>
        /// <returns>如果不存在，返回默认字体</returns>
        public static Font GetFont(string name)
        {
            if (Fonts.ContainsKey(name))
                return Fonts[name];
            return DefaultFont;
        }

        /// <summary>
        /// 判断私有字体是否存在
        /// </summary>
        /// <param name="name">私有字体名称</param>
        public static bool ContainsFont(string name)
        {
            return Fonts.ContainsKey(name);
        }
        
        /// <summary>
        /// 添加私有字体，如果字体存在将覆盖
        /// </summary>
        /// <param name="name">字体名称</param>
        /// <param name="font">实例</param>
        public static void AddFont(string name, Font font)
        {
            if (Fonts.ContainsKey(name))
                Fonts[name] = font;
            else
                Fonts.Add(name, font);
        }
        */
        /// <summary>
        /// 添加字体文件
        /// </summary>
        /// <param name="file">字体文件路径</param>
        public static void AddFontFile(string file)
        {
            PriFontCollection.AddFontFile(file);
        }

        /// <summary>
        /// 获取私有字体
        /// </summary>
        /// <param name="fontName">字体名称</param>
        /// <returns>如果不存在私有字体，则返回默认字体</returns>
        public static FontFamily GetPrivateFont(string fontName)
        {
            foreach(FontFamily fontFamily in PriFontCollection.Families)
            {
                if(fontFamily.Name == fontName)
                    return fontFamily;
            }
            return defaultfont.FontFamily;
        }
    }
}
