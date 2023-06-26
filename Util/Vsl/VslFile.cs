using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MeltuiCodeEditor.Util.Vsl
{
    /// <summary>
    /// vsl文件信息
    /// </summary>
    internal class VslFile
    {
        internal VslFile()
        {
            this.EnvColors = new Dictionary<string, Style>();
            this.Extensions = new List<string>();
            this.Properties = new Dictionary<string, string>();
            this.Resources = new List<ResItem>();
            this.RuleSets = new Dictionary<string, RuleSet>();
            this.Styles = new List<Style>();
        }

        internal VslFile(string name, string savaFile)
            : this()
        {
            this.Name = name;
            this.File=savaFile;

        }

        internal VslFile(string file)
            : this()
        {
            this.File = file;

        }

        /// <summary>
        /// 保存文件
        /// </summary>
        internal void Save(string file = null)
        {
            if (file == null)
                file = this.File;
            XmlDocument xml = new XmlDocument();
            XmlElement root = xml.CreateElement("Vsl");
            root.SetAttribute("name", this.Name);
            root.SetAttribute("language", this.Languge);
            root.SetAttribute("extensions", String.Join(";", this.Extensions));
            root.SetAttribute("description", this.Description);
            {//Styles

            }
            {//EnvColors

            }
            {//Properties

            }
            {//Resources

            }
            {//Breakpoint

            }
            {//Digits

            }
            {//RuleSets

               
            }

            xml.Save(file);
        }


        internal void Parser(string file)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(file);
            XmlElement root = xml.DocumentElement;
            {//Styles

            }
            {//EnvColors

            }
            {//Properties

            }
            {//Resources

            }
            {//Breakpoint

            }
            {//Digits

            }
            {//RuleSets


            }
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        internal string File { get; }

        /// <summary>
        /// 名称 name
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// 语言 language
        /// </summary>
        internal string Languge { get; set; }

        /// <summary>
        /// 扩展名列表
        /// </summary>
        internal List<string> Extensions { get; }

        /// <summary>
        /// 说明信息
        /// </summary>
        internal string Description { get; set; }

        /// <summary>
        /// 编辑器属性
        /// </summary>
        internal Dictionary<string, string> Properties { get; }

        /// <summary>
        /// 默认环境项
        /// </summary>
        internal Dictionary<string, Style> EnvColors { get; }

        /// <summary>
        /// 全局样式项
        /// </summary>
        internal List<Style> Styles { get; }

        /// <summary>
        /// 全局资源
        /// </summary>
        internal List<ResItem> Resources { get; }

        /// <summary>
        /// 断点样式
        /// </summary>
        internal Breakpoint Breakpoint { get; }

        /// <summary>
        /// 数字样式
        /// </summary>
        internal Style Digits { get; }

        /// <summary>
        /// 默认规则集
        /// </summary>
        internal RuleSet DefaultRuleSet { get; }

        /// <summary>
        /// 规则列表，不包含默认规则
        /// </summary>
        internal Dictionary<string, RuleSet> RuleSets { get; }

    }

    internal class Style
    {
        /// <summary>
        /// 粗体
        /// </summary>
        internal bool Bold { get; set; }

        /// <summary>
        /// 斜体
        /// </summary>
        internal bool Italic { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        internal string Color { get; set; }

        /// <summary>
        /// 背景颜色
        /// </summary>
        internal string BgColor { get; set; }

        /// <summary>
        /// 字体
        /// </summary>
        internal string Font { get; set; }
    }

    internal class ResItem
    {
        internal enum ResType
        {
            Image,
            Font,
        }
        /// <summary>
        /// 资料类型
        /// </summary>
        internal ResType Type { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// 资源相对路径
        /// </summary>
        internal string Path { get; set; }
    }

    /// <summary>
    /// 断点样式
    /// </summary>
    internal class Breakpoint
    {
        /// <summary>
        /// 正常 图片名称
        /// </summary>
        internal string Normal { get; set; }
        /// <summary>
        /// 禁用 图片名称
        /// </summary>
        internal string Disable { get; set; }
        /// <summary>
        /// 无法命中 图片名称
        /// </summary>
        internal string UnableToHit { get; set; }
    }

    /// <summary>
    /// 规则
    /// </summary>
    internal class RuleSet
    {
        internal RuleSet()
        {
            this.Spans = new List<Span>();
            this.KeyWords = new List<KeyWord>();
        }

        /// <summary>
        /// 规则名称
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// 忽略大小写
        /// </summary>
        internal bool IgnoreCase { get; set; }

        /// <summary>
        /// 分隔符
        /// </summary>
        internal string Delimiters { get; set; }

        /// <summary>
        /// 区域列表
        /// </summary>
        internal List<Span> Spans { get; }

        /// <summary>
        /// 关键字列表
        /// </summary>
        internal List<KeyWord> KeyWords { get; }
    }

    /// <summary>
    /// 规则项
    /// </summary>
    internal class RulesetItem
    {
        /// <summary>
        /// 名称
        /// </summary>
        internal string Name { get; set; }
    }

    /// <summary>
    /// 区域
    /// </summary>
    internal class Span : RulesetItem
    {

        internal Span()
        {

        }
        /// <summary>
        /// 样式表
        /// </summary>
        internal Style Style { get; }

        /// <summary>
        /// 到行尾结束
        /// </summary>
        internal bool Stopateol { get; set; }

        /// <summary>
        /// 忽略转义序列
        /// </summary>
        internal bool NoEscapeSequences { get; set; }

        /// <summary>
        /// 开始样式
        /// </summary>
        internal Style BeginStyle { get; set; }

        /// <summary>
        /// 结束样式
        /// </summary>
        internal Style EndStyle { get; set; }

        /// <summary>
        /// 开始内容
        /// </summary>
        internal string BeginText { get; set; }

        /// <summary>
        /// 结束内容
        /// </summary>
        internal string EndText { get; set; }

        /// <summary>
        /// 引用规则名称
        /// </summary>
        internal string RuleName { get; set; }
    }

    /// <summary>
    /// 前置高亮
    /// </summary>
    internal class MarkPrevious
    {
        internal MarkPrevious()
        {
            this.Style = new Style();
            this.Text = string.Empty;
        }
        /// <summary>
        /// 样式表
        /// </summary>
        internal Style Style { get; }

        /// <summary>
        /// 符号
        /// </summary>
        internal string Text { get; set; }

    }

    /// <summary>
    /// 关键字
    /// </summary>
    internal class KeyWord : RulesetItem
    {

        internal KeyWord()
        {
            this.Style = new Style();
            this.Keys = new List<string>();
        }

        /// <summary>
        /// 样式表
        /// </summary>
        internal Style Style { get; }

        /// <summary>
        /// 关键字列表
        /// </summary>
        internal List<string> Keys { get; }
    }

}
