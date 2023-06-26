using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MeltuiCodeEditor.Util.Vsl
{
    public partial class VslEdit : Form
    {
        public VslEdit()
        {
            this.InitializeComponent();
            this.ControlEnable(false);
        }
        /*
         资源
         树视图结构
         全局样式表
         环境设置
         属性值
         断点
         数字
         规则集
            规则
                区域
                区分符
                标记
                关键字 样式
                    关键字列表
         */



        //加载窗体
        private void VslEdit_Load(object sender, EventArgs e)
        {

        }

        //工具条项被单击
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == this.StripItem_New)
            {//新建文件

            }
            else if (e.ClickedItem == this.StripItem_Open)
            {//打开文件

            }
            else if (e.ClickedItem == this.StripItem_Save)
            {//保存文件

            }
            else if (e.ClickedItem == this.StripItem_Config)
            {//文件配置

            }
            else if (e.ClickedItem == this.StripItem_Close)
            {//关闭文件

            }
            else if (e.ClickedItem == this.StripItem_Info)
            {//信息
                Vsl.AboutBox aboutBox = new Vsl.AboutBox();
                aboutBox.ShowDialog();
            }
        }

        #region XML文件节点

        #endregion XML文件节点

        #region vsl文件处理
        /// <summary>
        /// 加载vsl文件
        /// </summary>
        /// <param name="file"></param>
        private void LoadVsl(string file)
        {

        }

        /// <summary>
        /// 保存vsl文件
        /// </summary>
        /// <param name="file"></param>
        private void SaveVsl(string file)
        {

        }

        /// <summary>
        /// 新建vsl文件
        /// </summary>
        /// <param name="file"></param>
        private void NewVsl(string file)
        {

        }

        /// <summary>
        /// 关闭vsl文件
        /// </summary>
        /// <param name="file"></param>
        private void CloseVsl(string file)
        {

        }

        /// <summary>
        /// 控件启用状态
        /// </summary>
        /// <param name="enabled"></param>
        private void ControlEnable(bool enabled)
        {
            this.VslTreeView.Enabled = enabled;
            this.StripItem_Save.Enabled = enabled;
            this.StripItem_Config.Enabled = enabled;
            this.StripItem_Close.Enabled = enabled;
        }
        #endregion vsl文件处理



    }
}
