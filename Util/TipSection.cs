// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="none" email=""/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Diagnostics;
using System.Drawing;

namespace VCodeEditor.Util
{
	/// <summary>
	/// 提示节 抽象类
	/// </summary>
	abstract class TipSection
	{
		SizeF    tipAllocatedSize;
		Graphics tipGraphics;
		SizeF    tipMaxSize;
		SizeF    tipRequiredSize;
		
		protected TipSection(Graphics graphics)
		{
			tipGraphics = graphics;
		}
		
		public abstract void Draw(PointF location);

		/// <summary>
		/// 获取所需大小
		/// </summary>
		/// <returns></returns>
		public SizeF GetRequiredSize()
		{
			return tipRequiredSize;
		}

		/// <summary>
		/// 设置分配的大小
		/// </summary>
		/// <param name="allocatedSize"></param>
		public void SetAllocatedSize(SizeF allocatedSize)
		{
			Debug.Assert(allocatedSize.Width >= tipRequiredSize.Width &&
			             allocatedSize.Height >= tipRequiredSize.Height);
			
			tipAllocatedSize = allocatedSize; OnAllocatedSizeChanged();
		}
		/// <summary>
		/// 设置最大大小
		/// </summary>
		/// <param name="maximumSize"></param>
		public void SetMaximumSize(SizeF maximumSize)
		{
			tipMaxSize = maximumSize; OnMaximumSizeChanged();
		}
		
		protected virtual void OnAllocatedSizeChanged()
		{
			
		}
		
		protected virtual void OnMaximumSizeChanged()
		{
			
		}
		
		protected void SetRequiredSize(SizeF requiredSize)
		{
			requiredSize.Width  = Math.Max(0, requiredSize.Width);
			requiredSize.Height = Math.Max(0, requiredSize.Height);
			requiredSize.Width  = Math.Min(tipMaxSize.Width, requiredSize.Width);
			requiredSize.Height = Math.Min(tipMaxSize.Height, requiredSize.Height);
			
			tipRequiredSize = requiredSize;
		}
		
		protected Graphics Graphics	{
			get {
				return tipGraphics;
			}
		}
		
		protected SizeF AllocatedSize {
			get {
				return tipAllocatedSize;
			}
		}
		
		protected SizeF MaximumSize {
			get {
				return tipMaxSize;
			}
		}
	}
}
