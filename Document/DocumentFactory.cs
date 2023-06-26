// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1054 $</version>
// </file>

using System;
using System.IO;
using System.Text;

namespace MeltuiCodeEditor.Document
{
    /// <summary>
    /// 文档工厂
    /// </summary>
    public class DocumentFactory
    {
        /// <remarks>
        /// 创建新的<see cref="IDocument"/> 对象。仅创建 <see cref="IDocument"/> 用这种方法。
        /// </remarks>
        public IDocument CreateDocument()
        {
            DefaultDocument doc = new DefaultDocument();
            doc.TextBufferStrategy = new GapTextBufferStrategy();
            doc.FormattingStrategy = new DefaultFormattingStrategy();
            doc.LineManager = new DefaultLineManager(doc, null);
            doc.FoldingManager = new FoldingManager(doc, doc.LineManager);
            doc.FoldingManager.FoldingStrategy = null; //new ParserFoldingStrategy();
            doc.MarkerStrategy = new MarkerStrategy(doc);
            doc.BookmarkManager = new BookmarkManager(doc, doc.LineManager);
            doc.CustomLineManager = new CustomLineManager(doc.LineManager);
            return doc;
        }

        /// <summary>
        /// 创建新文档并加载给定文本内容
        /// </summary>
        public IDocument CreateFromTextBuffer(ITextBufferStrategy textBuffer)
        {
            DefaultDocument doc = (DefaultDocument)CreateDocument();
            doc.TextContent = textBuffer.GetText(0, textBuffer.Length);
            doc.TextBufferStrategy = textBuffer;
            return doc;
        }

        /// <summary>
        /// 创建新文档并加载给定文件
        /// </summary>
        public IDocument CreateFromFile(string fileName)
        {
            IDocument document = CreateDocument();
            Encoding encoding = Encoding.Default;
            document.TextContent = Util.FileReader.ReadFileContent(fileName, ref encoding, encoding);
            return document;
        }
    }
}
