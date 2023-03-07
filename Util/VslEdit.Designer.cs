namespace VCodeEditor.Util.Vsl
{
    partial class VslEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VslEdit));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.VslTreeView = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.RuleSetMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CommonMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextMenuStrip3 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.StripItem_New = new System.Windows.Forms.ToolStripButton();
            this.StripItem_Open = new System.Windows.Forms.ToolStripButton();
            this.StripItem_Save = new System.Windows.Forms.ToolStripButton();
            this.StripItem_Config = new System.Windows.Forms.ToolStripButton();
            this.StripItem_Close = new System.Windows.Forms.ToolStripButton();
            this.StripItem_Info = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StripItem_New,
            this.StripItem_Open,
            this.StripItem_Save,
            this.toolStripSeparator1,
            this.StripItem_Config,
            this.StripItem_Close,
            this.toolStripSeparator2,
            this.StripItem_Info});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(931, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // VslTreeView
            // 
            this.VslTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.VslTreeView.Location = new System.Drawing.Point(0, 25);
            this.VslTreeView.Name = "VslTreeView";
            this.VslTreeView.Size = new System.Drawing.Size(316, 548);
            this.VslTreeView.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(316, 25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(615, 548);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(607, 522);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(607, 522);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // RuleSetMenu
            // 
            this.RuleSetMenu.Name = "RuleSetMenu";
            this.RuleSetMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // CommonMenu
            // 
            this.CommonMenu.Name = "CommonMenu";
            this.CommonMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // contextMenuStrip3
            // 
            this.contextMenuStrip3.Name = "contextMenuStrip3";
            this.contextMenuStrip3.Size = new System.Drawing.Size(61, 4);
            // 
            // StripItem_New
            // 
            this.StripItem_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StripItem_New.Image = ((System.Drawing.Image)(resources.GetObject("StripItem_New.Image")));
            this.StripItem_New.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StripItem_New.Name = "StripItem_New";
            this.StripItem_New.Size = new System.Drawing.Size(23, 22);
            this.StripItem_New.Text = "新建文件";
            // 
            // StripItem_Open
            // 
            this.StripItem_Open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StripItem_Open.Image = ((System.Drawing.Image)(resources.GetObject("StripItem_Open.Image")));
            this.StripItem_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StripItem_Open.Name = "StripItem_Open";
            this.StripItem_Open.Size = new System.Drawing.Size(23, 22);
            this.StripItem_Open.Text = "打开文件";
            // 
            // StripItem_Save
            // 
            this.StripItem_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StripItem_Save.Image = ((System.Drawing.Image)(resources.GetObject("StripItem_Save.Image")));
            this.StripItem_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StripItem_Save.Name = "StripItem_Save";
            this.StripItem_Save.Size = new System.Drawing.Size(23, 22);
            this.StripItem_Save.Text = "保存";
            // 
            // StripItem_Config
            // 
            this.StripItem_Config.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StripItem_Config.Image = ((System.Drawing.Image)(resources.GetObject("StripItem_Config.Image")));
            this.StripItem_Config.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StripItem_Config.Name = "StripItem_Config";
            this.StripItem_Config.Size = new System.Drawing.Size(23, 22);
            this.StripItem_Config.Text = "配置";
            // 
            // StripItem_Close
            // 
            this.StripItem_Close.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StripItem_Close.Image = ((System.Drawing.Image)(resources.GetObject("StripItem_Close.Image")));
            this.StripItem_Close.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StripItem_Close.Name = "StripItem_Close";
            this.StripItem_Close.Size = new System.Drawing.Size(23, 22);
            this.StripItem_Close.Text = "关闭";
            // 
            // StripItem_Info
            // 
            this.StripItem_Info.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StripItem_Info.Image = ((System.Drawing.Image)(resources.GetObject("StripItem_Info.Image")));
            this.StripItem_Info.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.StripItem_Info.Name = "StripItem_Info";
            this.StripItem_Info.Size = new System.Drawing.Size(23, 22);
            this.StripItem_Info.Text = "关于";
            // 
            // VslEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 573);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.VslTreeView);
            this.Controls.Add(this.toolStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "VslEdit";
            this.Text = "Vsl文件编辑器";
            this.Load += new System.EventHandler(this.VslEdit_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton StripItem_New;
        private System.Windows.Forms.ToolStripButton StripItem_Open;
        private System.Windows.Forms.ToolStripButton StripItem_Save;
        private System.Windows.Forms.TreeView VslTreeView;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton StripItem_Close;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton StripItem_Info;
        private System.Windows.Forms.ToolStripButton StripItem_Config;
        private System.Windows.Forms.ContextMenuStrip RuleSetMenu;
        private System.Windows.Forms.ContextMenuStrip CommonMenu;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip3;
    }
}