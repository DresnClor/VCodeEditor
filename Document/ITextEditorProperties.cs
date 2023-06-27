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
        bool AutoInsertCurlyBracket
        {
            get;
            set;
        }

        /// <summary>
        /// �������ָ��
        /// </summary>
        bool HideMouseCursor
        {
            get;
            set;
        }

        /// <summary>
        /// ͼ�����Ƿ�ɼ�
        /// </summary>
        bool IsIconBarVisible
        {
            get;
            set;
        }

        /// <summary>
        /// �ϵ����Ƿ�ɼ�
        /// </summary>
        bool IsBreakpointBarVisible
        {
            get;
            set;
        }

        /// <summary>
        /// ���������ų���EOL
        /// </summary>
        bool AllowCaretBeyondEOL
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾƥ������
        /// </summary>
        bool ShowMatchingBracket
        {
            get;
            set;
        }

        /// <summary>
        /// ������л�������
        /// </summary>
        bool CutCopyWholeLine
        {
            get;
            set;
        }

        /// <summary>
        /// ʹ�ÿ��������
        /// </summary>
        bool UseAntiAliasedFont
        {
            get;
            set;
        }

        /// <summary>
        /// �������������¹���
        /// </summary>
        bool MouseWheelScrollDown
        {
            get;
            set;
        }

        /// <summary>
        /// ���������������ı�
        /// </summary>
        bool MouseWheelTextZoom
        {
            get;
            set;
        }

        /// <summary>
        /// ����ֹ��
        /// </summary>
        string LineTerminator
        {
            get;
            set;
        }

        /// <summary>
        /// �������ݸ���
        /// </summary>
        bool CreateBackupCopy
        {
            get;
            set;
        }

        /// <summary>
        /// ����ͼ��ʽ
        /// </summary>
        LineViewerStyle LineViewerStyle
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾ��Ч��
        /// </summary>
        bool ShowInvalidLines
        {
            get;
            set;
        }

        /// <summary>
        /// ��ֱ�����
        /// </summary>
        int VerticalRulerRow
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾ�հ��ַ�
        /// </summary>
        bool ShowSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾtab�ַ�
        /// </summary>
        bool ShowTabs
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾEOL���
        /// </summary>
        bool ShowEOLMarker
        {
            get;
            set;
        }

        /// <summary>
        /// ת��tabΪ�ո�
        /// </summary>
        bool ConvertTabsToSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾˮƽ���
        /// </summary>
        bool ShowHorizontalRuler
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾ��ֱ���
        /// </summary>
        bool ShowVerticalRuler
        {
            get;
            set;
        }

        /// <summary>
        /// ����
        /// </summary>
        Encoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// �����۵�
        /// </summary>
        bool EnableFolding
        {
            get;
            set;
        }

        /// <summary>
        /// ��ʾ�к�
        /// </summary>
        bool ShowLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// tab�������
        /// </summary>
        int TabIndent
        {
            get;
            set;
        }

        /// <summary>
        /// ������ʽ
        /// </summary>
        IndentStyle IndentStyle
        {
            get;
            set;
        }

        /// <summary>
        /// �ĵ�ѡ��ģʽ
        /// </summary>
        DocumentSelectionMode DocumentSelectionMode
        {
            get;
            set;
        }

        /// <summary>
        /// ����
        /// </summary>
        Font Font
        {
            get;
            set;
        }

        /// <summary>
        /// ����ƥ����ʽ
        /// </summary>
        BracketMatchingStyle BracketMatchingStyle
        {
            get;
            set;
        }

        /// <summary>
        /// ʹ���Զ�����
        /// </summary>
        bool UseCustomLine
        {
            get;
            set;
        }
    }
}
