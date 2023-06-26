// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 915 $</version>
// </file>

using System;
using System.IO;
using System.Text;

namespace MeltuiCodeEditor.Util
{
	/// <summary>
	/// 可通过自动检测编码打开文本文件的类。
	/// </summary>
	public static class FileReader
	{
		/// <summary>
		/// 是否为Unicode代码页
		/// </summary>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static bool IsUnicode(Encoding encoding)
		{
			int codepage = encoding.CodePage;
			// 返回真实，如果代码页是任何UTF代码页
			return codepage == 65001 || codepage == 65000 || codepage == 1200 || codepage == 1201;
		}
		
		/// <summary>
		/// 读取文件内容
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="encoding"></param>
		/// <param name="defaultEncoding"></param>
		/// <returns></returns>
		public static string ReadFileContent(string fileName, ref Encoding encoding, Encoding defaultEncoding)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
				using (StreamReader reader = OpenStream(fs, encoding, defaultEncoding)) {
					encoding = reader.CurrentEncoding;
					return reader.ReadToEnd();
				}
			}
		}
		
		/// <summary>
		/// 打开流
		/// </summary>
		/// <param name="fs"></param>
		/// <param name="suggestedEncoding"></param>
		/// <param name="defaultEncoding"></param>
		/// <returns></returns>
		public static StreamReader OpenStream(FileStream fs, Encoding suggestedEncoding, Encoding defaultEncoding)
		{
			if (fs.Length > 3) {
				//流读器的自动读取无法检测差异 ISO-8859-1和UTF-8之间没有BOM。
				int firstByte = fs.ReadByte();
				int secondByte = fs.ReadByte();
				switch ((firstByte << 8) | secondByte) {
					case 0x0000: // UTF-32大恩迪安或二进制文件;使用流读取器
					case 0xfffe: // 单码 BOM （UTF-16 LE 或 UTF-32 LE）
					case 0xfeff: // UTF-16 BE BOM
					case 0xefbb: // start of UTF-8 BOM
						// 流读器自动读取工作
						fs.Position = 0;
						return new StreamReader(fs);
					default:
						return AutoDetect(fs, (byte)firstByte, (byte)secondByte, defaultEncoding);
				}
			} else {
				if (suggestedEncoding != null) {
					return new StreamReader(fs, suggestedEncoding);
				} else {
					return new StreamReader(fs);
				}
			}
		}
		
		static StreamReader AutoDetect(FileStream fs, byte firstByte, byte secondByte, Encoding defaultEncoding)
		{
			int max = (int)Math.Min(fs.Length, 500000); // 查看最大500kb
			const int ASCII = 0;
			const int Error = 1;
			const int UTF8  = 2;
			const int UTF8Sequence = 3;
			int state = ASCII;
			int sequenceLength = 0;
			byte b;
			for (int i = 0; i < max; i++) {
				if (i == 0) {
					b = firstByte;
				} else if (i == 1) {
					b = secondByte;
				} else {
					b = (byte)fs.ReadByte();
				}
				if (b < 0x80) {
					// normal ASCII character
					if (state == UTF8Sequence) {
						state = Error;
						break;
					}
				} else if (b < 0xc0) {
					// 10xxxxxx : continues UTF8 byte sequence
					if (state == UTF8Sequence) {
						--sequenceLength;
						if (sequenceLength < 0) {
							state = Error;
							break;
						} else if (sequenceLength == 0) {
							state = UTF8;
						}
					} else {
						state = Error;
						break;
					}
				} else if (b >= 0xc2 && b < 0xf5) {
					// 手提包序列的开头 beginning of byte sequence
					if (state == UTF8 || state == ASCII) {
						state = UTF8Sequence;
						if (b < 0xe0) {
							sequenceLength = 1; // 一个更多的手提包以下
						} else if (b < 0xf0) {
							sequenceLength = 2; // two more bytes following
						} else {
							sequenceLength = 3; // three more bytes following
						}
					} else {
						state = Error;
						break;
					}
				} else {
					// 0xc0, 0xc1, 0xf5 to 0xff are invalid in UTF-8 (see RFC 3629)
					state = Error;
					break;
				}
			}
			fs.Position = 0;
			switch (state) {
				case ASCII:
				case Error:
					// 当文件似乎是ASCII或非UTF8时，
					//我们使用它使用用户指定的编码，所以它再次保存
					//使用该编码。
					if (IsUnicode(defaultEncoding)) {
						// 该文件不是统一代码，所以不要读它使用Unicode，即使
						//用户已选择Unicode作为默认编码。
						
						// 如果我们不这样做，SD最终总是会添加一个拜特订单标记
						//到ASCII文件。
						
						defaultEncoding = Encoding.Default; // use system encoding instead
					}
					return new StreamReader(fs, defaultEncoding);
				default:
					return new StreamReader(fs);
			}
		}
	}
}
