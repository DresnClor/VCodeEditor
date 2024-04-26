using System;
using System.Collections.Generic;
using System.Linq;
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
			StyleManager.StyleStrategies = new List<IStyleStrategy>();




		}

		/// <summary>
		/// 样式列表
		/// </summary>
		private static List<IStyleStrategy> StyleStrategies { get; set; }















	}
}
