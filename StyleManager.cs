using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VCodeEditor.Document;

namespace VCodeEditor
{
	/// <summary>
	/// 样式管理器
	/// </summary>
	public static class StyleManager
	{
		static StyleManager()
		{
			StyleManager.StyleProvider = new StyleModeProvider();
			StyleManager.globalStyle = new DefaultGlobalStyle();
			StyleManager.DefaultStyleStrategy = new StyleStrategy("Default");
			StyleManager.DefaultColorStyle = new ColorStyle(Color.White, false, false);
		}

		/// <summary>
		/// 样式列表
		/// </summary>
		public static StyleModeProvider StyleProvider { get; private set; }

		public static IStyleStrategy DefaultStyleStrategy { get; }


		public static ColorStyle DefaultColorStyle { get; private set; }

		/// <summary>
		/// 全局高亮样式
		/// </summary>
		public static IGlobalStyle GlobalStyle
		{
			get => StyleManager.globalStyle;
			private set
			{
				if (globalStyle != value)
				{
					StyleManager.globalStyle = value;
					StyleManager.GlobalStyleChanged?.Invoke(StyleManager.globalStyle, new EventArgs());
				}
			}
		}
		private static IGlobalStyle globalStyle;

		/// <summary>
		/// 全局样式改变
		/// </summary>
		public static EventHandler GlobalStyleChanged;

		/// <value>
		/// 默认粗体字体
		/// </value>
		public static Font BoldFont => Document.FontContainer.BoldFont;

		/// <value>
		/// 默认字体
		/// </value>
		public static Font DefaultFont => Document.FontContainer.DefaultFont;

		/// <value>
		/// 默认粗体+斜体字体
		/// </value>
		public static Font BoldItalicFont => Document.FontContainer.BoldItalicFont;

		/// <value>
		/// 默认斜体字体
		/// </value>
		public static Font ItalicFont => Document.FontContainer.ItalicFont;





		/// <summary>
		/// 加载全局样式
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static IGlobalStyle LoadGlobalStyle(string file)
		{
			return new DefaultGlobalStyle(file);
		}

		/// <summary>
		/// 设置全局样式
		/// </summary>
		/// <param name="gs"></param>
		/// <returns></returns>
		public static void SetGlobalStyle(IGlobalStyle gs)
		{
			StyleManager.GlobalStyle = gs;
		}




	}
}
