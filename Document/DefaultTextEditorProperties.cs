// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="none" email=""/>
//     <version>$Revision: 1118 $</version>
// </file>

using System;
using System.Drawing;
using System.Text;

namespace VCodeEditor.Document
{
	/// <summary>
	/// 括号匹配样式
	/// </summary>
	public enum BracketMatchingStyle {
		/// <summary>
		/// 之前
		/// </summary>
		Before,
		/// <summary>
		/// 之后
		/// </summary>
		After
	}
	
	/// <summary>
	/// 默认编辑属性实现
	/// </summary>
	public class DefaultTextEditorProperties : ITextEditorProperties
	{
		int                   tabIndent             = 4;
		IndentStyle           indentStyle           = IndentStyle.Smart;
		DocumentSelectionMode documentSelectionMode = DocumentSelectionMode.Normal;
		Encoding              encoding              = System.Text.Encoding.UTF8;
		BracketMatchingStyle  bracketMatchingStyle  = BracketMatchingStyle.After;
		
		bool        allowCaretBeyondEOL = false;
		
		bool        showMatchingBracket = true;
		bool        showLineNumbers     = true;
		
		bool        showSpaces          = false;
		bool        showTabs            = false;
		bool        showEOLMarker       = false;
		
		bool        showInvalidLines    = false;
		
		bool        isIconBarVisible    = true;
		bool isBreakpointBarVisible = true;

        bool        enableFolding       = true;
		bool        showHorizontalRuler = false;
		bool        showVerticalRuler   = false;
		bool        convertTabsToSpaces = false;
		bool        useAntiAliasedFont  = true;
		bool        createBackupCopy    = false;
		bool        mouseWheelScrollDown = true;
		bool        mouseWheelTextZoom   = true;
		
		bool        hideMouseCursor      = false;
		bool        cutCopyWholeLine     = true;
		
		int         verticalRulerRow    = 80;
		LineViewerStyle lineViewerStyle = LineViewerStyle.FullRow;//.None;
		string      lineTerminator = "\r\n";
		bool        autoInsertCurlyBracket = true;
		bool        useCustomLine = false;
		
		public int TabIndent {
			get {
				return tabIndent;
			}
			set {
				tabIndent = value;
			}
		}
		
		
		public IndentStyle IndentStyle {
			get {
				return indentStyle;
			}
			set {
				indentStyle = value;
			}
		}
		public DocumentSelectionMode DocumentSelectionMode {
			get {
				return documentSelectionMode;
			}
			set {
				documentSelectionMode = value;
			}
		}
		public bool AllowCaretBeyondEOL {
			get {
				return allowCaretBeyondEOL;
			}
			set {
				allowCaretBeyondEOL = value;
			}
		}
		public bool ShowMatchingBracket {
			get {
				return showMatchingBracket;
			}
			set {
				showMatchingBracket = value;
			}
		}
		public bool ShowLineNumbers {
			get {
				return showLineNumbers;
			}
			set {
				showLineNumbers = value;
			}
		}
		public bool ShowSpaces {
			get {
				return showSpaces;
			}
			set {
				showSpaces = value;
			}
		}
		public bool ShowTabs {
			get {
				return showTabs;
			}
			set {
				showTabs = value;
			}
		}
		public bool ShowEOLMarker {
			get {
				return showEOLMarker;
			}
			set {
				showEOLMarker = value;
			}
		}
		public bool ShowInvalidLines {
			get {
				return showInvalidLines;
			}
			set {
				showInvalidLines = value;
			}
		}

        public bool IsBreakpointBarVisible
        {
            get=> this.isBreakpointBarVisible;
            set=> this.isBreakpointBarVisible=value;
        }
        public bool IsIconBarVisible {
			get {
				return isIconBarVisible;
			}
			set {
				isIconBarVisible = value;
			}
		}
		public bool EnableFolding {
			get {
				return enableFolding;
			}
			set {
				enableFolding = value;
			}
		}
		public bool ShowHorizontalRuler {
			get {
				return showHorizontalRuler;
			}
			set {
				showHorizontalRuler = value;
			}
		}
		public bool ShowVerticalRuler {
			get {
				return showVerticalRuler;
			}
			set {
				showVerticalRuler = value;
			}
		}
		public bool ConvertTabsToSpaces {
			get {
				return convertTabsToSpaces;
			}
			set {
				convertTabsToSpaces = value;
			}
		}
		public bool UseAntiAliasedFont {
			get {
				return useAntiAliasedFont;
			}
			set {
				useAntiAliasedFont = value;
			}
		}
		public bool CreateBackupCopy {
			get {
				return createBackupCopy;
			}
			set {
				createBackupCopy = value;
			}
		}
		public bool MouseWheelScrollDown {
			get {
				return mouseWheelScrollDown;
			}
			set {
				mouseWheelScrollDown = value;
			}
		}
		public bool MouseWheelTextZoom {
			get {
				return mouseWheelTextZoom;
			}
			set {
				mouseWheelTextZoom = value;
			}
		}
		
		public bool HideMouseCursor {
			get {
				return hideMouseCursor;
			}
			set {
				hideMouseCursor = value;
			}
		}

		public bool CutCopyWholeLine {
			get {
				return cutCopyWholeLine;
			}
			set {
				cutCopyWholeLine = value;
			}
		}

		public Encoding Encoding {
			get {
				return encoding;
			}
			set {
				encoding = value;
			}
		}
		public int VerticalRulerRow {
			get {
				return verticalRulerRow;
			}
			set {
				verticalRulerRow = value;
			}
		}
		public LineViewerStyle LineViewerStyle {
			get {
				return lineViewerStyle;
			}
			set {
				lineViewerStyle = value;
			}
		}
		public string LineTerminator {
			get {
				return lineTerminator;
			}
			set {
				lineTerminator = value;
			}
		}
		public bool AutoInsertCurlyBracket {
			get {
				return autoInsertCurlyBracket;
			}
			set {
				autoInsertCurlyBracket = value;
			}
		}
		
		public Font Font {
			get {
				return FontContainer.DefaultFont;
			}
			set {
				FontContainer.DefaultFont = value;
			}
		}
		
		public BracketMatchingStyle  BracketMatchingStyle {
			get {
				return bracketMatchingStyle;
			}
			set {
				bracketMatchingStyle = value;
			}
		}
		
		public bool UseCustomLine {
			get {
				return useCustomLine;
			}
			set {
				useCustomLine = value;
			}
		}		
	}
}
