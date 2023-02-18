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
	/// �༭�����Խӿ�
	/// </summary>
	public interface ITextEditorProperties
	{
		/// <summary>
		/// �Զ����뻨����
		/// </summary>
		bool AutoInsertCurlyBracket { // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// �������ָ��
		/// </summary>
		bool HideMouseCursor { // �������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ͼ�����Ƿ�ɼ�
		/// </summary>
		bool IsIconBarVisible { // �������ı��༭��������
			get;
			set;
        }

        /// <summary>
        /// �ϵ����Ƿ�ɼ�
        /// </summary>
        bool IsBreakpointBarVisible
        { // �������ı��༭��������
            get;
            set;
        }

        /// <summary>
        /// ���������ų���EOL
        /// </summary>
        bool AllowCaretBeyondEOL {
			get;
			set;
		}
		
		/// <summary>
		/// ��ʾƥ������
		/// </summary>
		bool ShowMatchingBracket { // �������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ������л�������
		/// </summary>
		bool CutCopyWholeLine {
			get;
			set;
		}

		/// <summary>
		/// ʹ�ÿ��������
		/// </summary>
		bool UseAntiAliasedFont { //�������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// �������������¹���
		/// </summary>
		bool MouseWheelScrollDown {
			get;
			set;
		}

		/// <summary>
		/// ���������������ı�
		/// </summary>
		bool MouseWheelTextZoom {
			get;
			set;
		}

		/// <summary>
		/// ����ֹ��
		/// </summary>
		string LineTerminator {
			get;
			set;
		}

		/// <summary>
		/// �������ݸ���
		/// </summary>
		bool CreateBackupCopy
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ����ͼ��ʽ
		/// </summary>
		LineViewerStyle LineViewerStyle
		{ //�������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ��ʾ��Ч��
		/// </summary>
		bool ShowInvalidLines
		{ //�������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ��ֱ�����
		/// </summary>
		int VerticalRulerRow
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ��ʾ�հ��ַ�
		/// </summary>
		bool ShowSpaces
		{ //�������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ��ʾtab�ַ�
		/// </summary>
		bool ShowTabs
		{ // �������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ��ʾEOL���
		/// </summary>
		bool ShowEOLMarker
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ת��tabΪ�ո�
		/// </summary>
		bool ConvertTabsToSpaces
		{ //�������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ��ʾˮƽ���
		/// </summary>
		bool ShowHorizontalRuler
		{ // �������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ��ʾ��ֱ���
		/// </summary>
		bool ShowVerticalRuler
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ����
		/// </summary>
		Encoding Encoding {
			get;
			set;
		}
		
		/// <summary>
		/// �����۵�
		/// </summary>
		bool EnableFolding
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ��ʾ�к�
		/// </summary>
		bool ShowLineNumbers
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// tab�������
		/// </summary>
		int TabIndent
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ������ʽ
		/// </summary>
		IndentStyle IndentStyle
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// �ĵ�ѡ��ģʽ
		/// </summary>
		DocumentSelectionMode DocumentSelectionMode {
			get;
			set;
		}
		
		/// <summary>
		/// ����
		/// </summary>
		Font Font
		{ // �������ı��༭��������
			get;
			set;
		}

		/// <summary>
		/// ����ƥ����ʽ
		/// </summary>
		BracketMatchingStyle BracketMatchingStyle
		{ // �������ı��༭��������
			get;
			set;
		}
		
		/// <summary>
		/// ʹ���Զ�����
		/// </summary>
		bool UseCustomLine {
			get;
			set;
		}	
	}
}
