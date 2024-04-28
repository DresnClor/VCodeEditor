using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 全局样式数据接口
	/// </summary>
	public interface IGlobalStyle
	{
		/// <summary>
		/// 名称
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 说明信息
		/// </summary>
		string Description { get; }

		/// <summary>
		/// 字体列表
		/// </summary>
		Dictionary<string, Font> Fonts { get; }

		/// <summary>
		/// 颜色列表
		/// </summary>
		List<ColorStyle> ColorStyles { get; }


		ColorStyle GetColorStyle (string name);

		Font GetFont(string name);




	}
}
