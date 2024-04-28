// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="DresnClor" url="http://www.dresnclor.com"/>
//     <version>$Revision: 001 $</version>
// </file>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCodeEditor.Gui;

namespace VCodeEditor.Actions
{


    /// <summary>
    /// 代码编辑器扩展接口，用于代码提示，自动补全，代码跳转
    /// </summary>
    public interface IEditorExtsable
    {

        /// <summary>
        /// 转到定义位置
        /// </summary>
        /// <param name="codeEditor">代码编辑器实例</param>
        void GotoDefine(TextEditorControl codeEditor);

        /// <summary>
        /// 插入代码
        /// </summary>
        /// <param name="codeEditor">代码编辑器实例</param>
        void InstCode(TextEditorControl codeEditor);

        /// <summary>
        /// 重命名标识符
        /// </summary>
        /// <param name="codeEditor">代码编辑器实例</param>
        void RenameIdent(TextEditorControl codeEditor);

        /// <summary>
        /// 查看代码树
        /// </summary>
        /// <param name="codeEditor">代码编辑器实例</param>
        void CodeTreeView(TextEditorControl codeEditor);


        /// <summary>
        /// 保存代码
        /// </summary>
        /// <param name="codeEditor">代码编辑器实例</param>
        void Save(TextEditorControl codeEditor);

    }
}
