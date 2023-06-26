// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Ivo Kovacka" email="ivok@internet.sk"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Drawing;

namespace MeltuiCodeEditor.Document
{
	/// <summary>
	/// �Զ����й�����
	/// </summary>
	public interface ICustomLineManager
	{
		/// <value>
		/// ���������Զ�����
		/// </value>
		ArrayList CustomLines {
			get;
		}
		
		/// <remarks>
		/// ������ɫ�����<code>lineNr</code> ���Զ���bg��ɫ
		/// ���򷵻� <code>defaultColor</code>
		/// </remarks>
		Color GetCustomColor(int lineNr, Color defaultColor);
		
		/// <remarks>
		/// �����棬����� <code>lineNr</code> ����ȡ
		/// </remarks>
		bool IsReadOnly(int lineNr, bool defaultReadOnly);

		/// <remarks>
		/// �����棬��� <code>selection</code> ����ȡ
		/// </remarks>
		bool IsReadOnly(ISelection selection, bool defaultReadOnly);

		/// <remarks>
		/// ����������Զ�����<code>lineNr</code>
		/// </remarks>
		void AddCustomLine(int lineNr, Color customColor, bool readOnly);
		
		/// <remarks>
		/// ����������Զ�����<code>startLineNr</code> �� <code>endLineNr</code>
		/// </remarks>
		void AddCustomLine(int startLineNr,  int endLineNr, Color customColor, bool readOnly);
		
		/// <remarks>
		/// ������ɾ���Զ����� <code>lineNr</code>
		/// </remarks>
		void RemoveCustomLine(int lineNr);
		
		/// <remarks>
		/// ��������Զ���ɫ��
		/// </remarks>
		void Clear();
		
		/// <remarks>
		/// ����
		/// </remarks>
		event EventHandler BeforeChanged;

		/// <remarks>
		/// ����
		/// </remarks>
		event EventHandler Changed;
	}
}
