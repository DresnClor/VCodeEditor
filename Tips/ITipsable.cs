using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tips
{
	/// <summary>
	/// 提示系统接口
	/// </summary>
	public interface ITipsable
	{
		/// <summary>
		/// 提示标题名称
		/// </summary>
		string Name { get; }

		/// <summary>
		/// 提示说明信息
		/// </summary>
		string Description { get; }
	}
}
