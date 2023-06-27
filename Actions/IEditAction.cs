using System.Windows.Forms;

//编辑接口

namespace VCodeEditor.Actions
{
	/// <summary>
	/// 文本编辑快捷键 接口
	/// </summary>
	public interface IEditAction
	{
		/// <value>
		/// 快捷键组合
		/// </value>
		Keys[] Keys {
			get;
			set;
		}
		
		/// <remarks>
		/// 按下快捷键时，触发此方法。
		/// </remarks>
		void Execute(TextArea textArea);
	}

	/// <summary>
	/// 文本编辑快捷键 抽象类，继承IEditAction
	/// </summary>
	public abstract class AbstractEditAction : IEditAction
	{
		Keys[] keys = null;

		/// <value>
		/// 快捷键组合
		/// </value>
		public Keys[] Keys {
			get {
				return keys;
			}
			set {
				keys = value;
			}
		}

		/// <remarks>
		/// 按下快捷键时，触发此方法。
		/// </remarks>
		public abstract void Execute(TextArea textArea);
	}		
}
