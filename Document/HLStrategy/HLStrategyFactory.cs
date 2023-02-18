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
	public class HLStrategyFactory
	{
		public static IHLStrategy CreateHLStrategy()
		{
			return (IHLStrategy)HLManager.Manager.HLDefines["Default"];
		}
		
		public static IHLStrategy CreateHLStrategy(string name)
		{
			IHLStrategy highlightingStrategy  = HLManager.Manager.FindHL(name);
			
			if (highlightingStrategy == null) {
				return CreateHLStrategy();
			}
			return highlightingStrategy;
		}
		
		public static IHLStrategy CreateHLStrategyForFile(string fileName)
		{
			IHLStrategy highlightingStrategy  = HLManager.Manager.FindHLForFile(fileName);
			if (highlightingStrategy == null) {
				return CreateHLStrategy();
			}
			return highlightingStrategy;
		}
	}
}
