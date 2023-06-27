using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCodeEditor.Util
{
    /// <summary>
    /// 根据类
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// 获取相对路径的绝对路径
        /// <para />
        /// 包含符号:..\上级目录 .\当前目录 \根目录（为基础路径） 无前缀表示当前目录
        /// </summary>
        /// <param name="basePath">基础路径</param>
        /// <param name="curPath">当前路径</param>
        /// <param name="relative">要解析的相对路径</param>
        /// <returns>失败返回空字符串</returns>
        public static string GetAbsolutePath(string basePath, string curPath, string relative)
        {
            if (string.IsNullOrWhiteSpace(basePath) ||
                string.IsNullOrWhiteSpace(curPath) ||
                string.IsNullOrWhiteSpace(relative))
                return string.Empty;
            try
            {
                //获取上级目录数量
                Func<string, int> getList = (str) =>
                {
                    int count = 0;
                    for (int i = 0; i < str.Length; i += 3)
                    {
                        string tl = str.Substring(i, 3);
                        if (tl == "..\\")
                            count += 1;
                        else
                            break;
                    }
                    return count;
                };

                if (relative.StartsWith("\\"))
                {//\根目录（为基础路径）
                    return Path.Combine(basePath, relative.Substring(1));
                }
                else if (relative.StartsWith("..\\"))
                {//..\上级目录
                    int len = getList(relative);
                    //获取目录有效名称
                    string name = relative.Substring(len * 3);
                    string curp = curPath;
                    //循环获取上级
                    for (int i = 0; i < len; i++)
                    {
                        curp = Path.GetDirectoryName(curp);
                    }
                    return Path.Combine(curp, name);
                }
                else if (relative.StartsWith(".\\"))
                {//.\当前目录
                    return Path.Combine(curPath, relative.Substring(2));
                }
                else if (relative.StartsWith(""))
                {//无前缀
                    return Path.Combine(curPath, relative);
                }
            }
            catch { }
            return string.Empty;
        }

    }
}
