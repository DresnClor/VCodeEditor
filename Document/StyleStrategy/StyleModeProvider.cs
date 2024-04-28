using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using VCodeEditor.Util;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 样式配置文件提供者
	/// </summary>
	public class StyleModeProvider
	{
		public StyleModeProvider()
		{
			this.modeItems = new List<ModeItem>();
			this.currentPath = "";
		}

		private string currentPath;

		private List<ModeItem> modeItems;

		public void Load(string file)
		{
			this.currentPath = Path.GetDirectoryName(file);
			XmlDocument xml = new XmlDocument();
			xml.Load(file);
			foreach (XmlNode node in xml.DocumentElement.ChildNodes)
			{
				if (node is XmlElement element)
					this.modeItems.Add(new ModeItem(element));
			}
		}

		/// <summary>
		/// 获取样式
		/// </summary>
		/// <param name="name">指定名称</param>
		/// <returns></returns>
		public IStyleStrategy GetStyle(string name)
		{
			foreach (ModeItem mi in this.modeItems)
			{
				if (mi.Name == name)
				{
					return this.GetStyleStrategy(mi.File);
				}
			}
			return null;
		}

		/// <summary>
		/// 获取样式
		/// </summary>
		/// <param name="ext">文件扩展名</param>
		/// <returns></returns>
		public IStyleStrategy GetStyleFor(string ext)
		{
			foreach (ModeItem mi in this.modeItems)
			{
				if (mi.HashExtension(ext))
				{
					return this.GetStyleStrategy(mi.File);
				}
			}
			return null;
		}


		private IStyleStrategy GetStyleStrategy(string file)
		{
			string path = Utils.GetAbsolutePath(this.currentPath, this.currentPath, file);
			if (File.Exists(path))
			{
				return new StyleStrategy(null, path);
			}
			return null;
		}

		internal class ModeItem
		{
			public ModeItem(XmlElement element)
			{
				this.Name = element.GetAttribute("name");
				this.File = element.GetAttribute("file");
				this.Extensions = element.GetAttribute("extensions")
					.ToLower()
					.Split(
						new char[] { ';', ',', '|' },
						StringSplitOptions.RemoveEmptyEntries);
				this.Description = element.GetAttribute("description");
			}

			public string Name { get; }

			public string File { get; }

			public string[] Extensions { get; }

			public string Description { get; }

			/// <summary>
			/// 扩展名是否存在
			/// </summary>
			/// <param name="extension"></param>
			/// <returns></returns>
			public bool HashExtension(string extension)
			{
				if (this.Extensions != null)
				{
					extension = extension.ToLower();
					foreach (string ext in this.Extensions)
					{
						if (ext == extension)
							return true;
					}
				}
				return false;
			}

		}
	}
}
