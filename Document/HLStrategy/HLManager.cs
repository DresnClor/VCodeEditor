// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1262 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 高亮管理器
	/// </summary>
	[Obsolete]
	public class HLManager
	{
		/// <summary>
		/// 高亮模式提供者
		/// </summary>
		ArrayList syntaxModeFileProviders = new ArrayList();
		
		/// <summary>
		/// 高亮管理器
		/// </summary>
		static HLManager highlightingManager;
		/// <summary>
		/// 默认高亮哈希表
		/// </summary>
		Hashtable highlightingDefs = new Hashtable();
        /// <summary>
        /// 扩展名称哈希表
        /// </summary>
        Hashtable extensionsToName = new Hashtable();
		
		/// <summary>
		/// 高亮定义
		/// </summary>
		public Hashtable HLDefines {
			get {
				return highlightingDefs;
			}
		}
		/// <summary>
		/// 高亮管理器
		/// </summary>
		public static HLManager Manager {
			get {
				return highlightingManager;		
			}
		}
		
		static HLManager()
		{
			highlightingManager = new HLManager();
			highlightingManager.AddSyntaxModeFileProvider(
				   new ResourceSyntaxModeProvider());
		}
		
		public HLManager()
		{
			CreateDefaultHighlightingStrategy();
		}
		
		public void AddSyntaxModeFileProvider(ISyntaxModeFileProvider syntaxModeFileProvider)
		{
			foreach (SyntaxMode syntaxMode in syntaxModeFileProvider.SyntaxModes) {
				highlightingDefs[syntaxMode.Name] = 
					new DictionaryEntry(syntaxMode, syntaxModeFileProvider);
				
				foreach (string extension in syntaxMode.Extensions) {
					extensionsToName[extension.ToUpperInvariant()] = 
						syntaxMode.Name;
				}
			}
			
			if (!syntaxModeFileProviders.Contains(syntaxModeFileProvider)) {
				syntaxModeFileProviders.Add(syntaxModeFileProvider);
			}
		}

		/// <summary>
		/// 重新加载语法模式
		/// </summary>
		public void ReloadSyntaxModes()
		{
			highlightingDefs.Clear();
			extensionsToName.Clear();
			CreateDefaultHighlightingStrategy();
			
			foreach (ISyntaxModeFileProvider provider in syntaxModeFileProviders) {
				provider.UpdateSyntaxModeList();
				AddSyntaxModeFileProvider(provider);
			}
			
			OnReloadSyntaxHighlighting(EventArgs.Empty);
		}
		
		/// <summary>
		/// 创建默认高亮风格
		/// </summary>
		void CreateDefaultHighlightingStrategy()
		{
			DefaultHLStrategy defaultHighlightingStrategy = new DefaultHLStrategy(); 
			defaultHighlightingStrategy.Extensions = new string[] {};
			defaultHighlightingStrategy.Rules.Add(new HighlightRuleSet());
			highlightingDefs["Default"] = defaultHighlightingStrategy;
		}
		
		/// <summary>
		/// /加载定义
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		IStyleStrategy LoadDefinition(DictionaryEntry entry)
		{
			SyntaxMode              syntaxMode             = (SyntaxMode)entry.Key;
			ISyntaxModeFileProvider syntaxModeFileProvider = 
				(ISyntaxModeFileProvider)entry.Value;
			
			DefaultHLStrategy highlightingStrategy = 
				HLDefineParser.Parse(
					syntaxMode, 
					 syntaxModeFileProvider.GetSyntaxModeFile(
							syntaxMode));
			if (highlightingStrategy == null) {
				highlightingStrategy = DefaultHL;
			}
			highlightingDefs[syntaxMode.Name] = highlightingStrategy;
			highlightingStrategy.ResolveReferences();
			
			return highlightingStrategy;
		}
		
		/// <summary>
		/// 默认高亮配置
		/// </summary>
		public DefaultHLStrategy DefaultHL {
			get {
				return (DefaultHLStrategy)highlightingDefs["Default"];
			}
		}
		
		internal KeyValuePair<SyntaxMode, ISyntaxModeFileProvider> FindHighlighterEntry(string name)
		{
			foreach (ISyntaxModeFileProvider provider in syntaxModeFileProviders) {
				foreach (SyntaxMode mode in provider.SyntaxModes) {
					if (mode.Name == name) {
						return new KeyValuePair<SyntaxMode, ISyntaxModeFileProvider>(mode, provider);
					}
				}
			}
			return new KeyValuePair<SyntaxMode, ISyntaxModeFileProvider>(null, null);
		}
		
		/// <summary>
		/// 查找高亮风格定义
		/// </summary>
		/// <param name="name">风格名称</param>
		/// <returns>如果不存在该风格，则返回默认风格</returns>
		public IStyleStrategy FindHL(string name)
		{
			object def = highlightingDefs[name];
			if (def is DictionaryEntry) {

				return LoadDefinition((DictionaryEntry)def);
			}
			return def == null ? DefaultHL : (IStyleStrategy)def;
		}
		
		/// <summary>
		/// 从文件查找高亮风格
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public IStyleStrategy FindHLForFile(string fileName)
		{
			string highlighterName = (string)extensionsToName[Path.GetExtension(fileName).ToUpperInvariant()];
			if (highlighterName != null) {
				object def = highlightingDefs[highlighterName];
				if (def is DictionaryEntry) {
					return LoadDefinition((DictionaryEntry)def);
				}
				return def == null ? DefaultHL : (IStyleStrategy)def;
			} else {
				return DefaultHL;
			}
		}
		
		protected virtual void OnReloadSyntaxHighlighting(EventArgs e)
		{
			if (ReloadSyntaxHL != null) {
				ReloadSyntaxHL(this, e);
			}
		}
		
		/// <summary>
		/// 重新加载语法高亮事件
		/// </summary>
		public event EventHandler ReloadSyntaxHL;
	}
}
