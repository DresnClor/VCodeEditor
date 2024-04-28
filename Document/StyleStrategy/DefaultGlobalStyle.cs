using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 全局高亮样式默认实现
	/// </summary>
	internal class DefaultGlobalStyle : IGlobalStyle
	{
		public DefaultGlobalStyle()
		{
			this.Fonts = new Dictionary<string, Font>();
			this.ColorStyles = new List<ColorStyle>();
			this.Name = "Default";
			this.Description = "Default Global Style";

		}
		public DefaultGlobalStyle(string file)
		{
			this.Fonts = new Dictionary<string, Font>();
			this.ColorStyles = new List<ColorStyle>();

			//解析xml
			XmlDocument xml = new XmlDocument();
			xml.Load(file);
			XmlElement root = xml.DocumentElement;
			this.Name = root.GetAttribute("name");
			this.Description = root.GetAttribute("description");
			//遍历子级
			foreach (XmlNode node in root.ChildNodes)
			{
				if (node is XmlElement element)
				{
					if (element.Name == "FontStyle")
					{
						Font font = FontContainer.ParseFont(element.GetAttribute("value"));
						string fontName = element.GetAttribute("name");
						this.Fonts[fontName] = font;
					}
					else if (element.Name == "ColorStyle")
					{
						this.ColorStyles.Add(new ColorStyle(element));
					}
				}
			}
		}



		public string Name { get; }

		public string Description { get; }

		public Dictionary<string, Font> Fonts { get; }

		public List<ColorStyle> ColorStyles { get; }

		public ColorStyle GetColorStyle(string name)
		{
			foreach (ColorStyle colorStyle in this.ColorStyles)
				if (colorStyle.Name == name)
					return colorStyle;
			return StyleManager.DefaultColorStyle;
		}

		public Font GetFont(string name)
		{
			if (this.Fonts.ContainsKey(name))
				return this.Fonts[name];
			return StyleManager.DefaultFont;
		}
	}
}
