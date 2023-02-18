// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using VCodeEditor.Undo;

namespace VCodeEditor.Document
{
    /// <summary>
    /// 文本缓冲策略
    /// </summary>
    public class StringTextBufferStrategy : ITextBufferStrategy
    {
        string storedText = "";

        /// <summary>
        /// 文本长度
        /// </summary>
        public int Length
        {
            get
            {
                return storedText.Length;
            }
        }

        /// <summary>
        /// 指定位置插入文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="text"></param>
        public void Insert(int offset, string text)
        {
            if (text != null)
            {
                storedText = storedText.Insert(offset, text);
            }
        }

        /// <summary>
        /// 移除指定位置文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Remove(int offset, int length)
        {
            storedText = storedText.Remove(offset, length);
        }

        /// <summary>
        /// 替换指定位置文本
        /// </summary>
        /// <param name="offset">位置</param>
        /// <param name="length">替换长度</param>
        /// <param name="text">替换文本</param>
        public void Replace(int offset, int length, string text)
        {
            Remove(offset, length);
            Insert(offset, text);
        }

        /// <summary>
        /// 取指定位置 指定长度文本
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetText(int offset, int length)
        {
            if (length == 0)
            {
                return "";
            }
            if (offset == 0 && length >= storedText.Length)
            {
                return storedText;
            }
            return storedText.Substring(offset, Math.Min(length, storedText.Length - offset));
        }

        /// <summary>
        /// 取指定位置字符
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char GetCharAt(int offset)
        {
            if (offset == Length)
            {
                return '\0';
            }
            return storedText[offset];
        }

        /// <summary>
        /// 设置内容
        /// </summary>
        /// <param name="text"></param>
        public void SetContent(string text)
        {
            storedText = text;
        }

        public StringTextBufferStrategy()
        {
        }

        StringTextBufferStrategy(string fileName)
        {
            Encoding encoding = Encoding.Default;
            SetContent(Util.FileReader.ReadFileContent(fileName, ref encoding, encoding));
        }

        /// <summary>
        /// 创建文本缓冲文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ITextBufferStrategy CreateTextBufferFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new System.IO.FileNotFoundException(fileName);
            }
            return new StringTextBufferStrategy(fileName);
        }
    }
}
