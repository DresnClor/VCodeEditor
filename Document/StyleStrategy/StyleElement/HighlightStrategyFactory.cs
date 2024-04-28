// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 高亮策略工厂
	/// </summary>
	public class HighlightStrategyFactory
	{
		public static IStyleStrategy CreateHLStrategy()
		{
			return (IStyleStrategy)StyleManager.DefaultStyleStrategy;
		}
		
		public static IStyleStrategy CreateHLStrategy(string name)
		{
			IStyleStrategy highlightingStrategy  = StyleManager.StyleProvider.GetStyle(name);
			
			if (highlightingStrategy == null) {
				return CreateHLStrategy();
			}
			return highlightingStrategy;
		}
		
		public static IStyleStrategy CreateHLStrategyForFile(string fileName)
		{
			string ext=Path.GetExtension(fileName);
			IStyleStrategy highlightingStrategy  = StyleManager.StyleProvider.GetStyleFor(ext);
			if (highlightingStrategy == null) {
				return CreateHLStrategy();
			}
			return highlightingStrategy;
		}
	}
}
