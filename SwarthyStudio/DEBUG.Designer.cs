namespace SwarthyStudio
{
    partial class DEBUG
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpLexems = new System.Windows.Forms.TabPage();
            this.tbLexems = new System.Windows.Forms.TextBox();
            this.tpSyntaxTree = new System.Windows.Forms.TabPage();
            this.tvSyntaxTree = new System.Windows.Forms.TreeView();
            this.tabControl1.SuspendLayout();
            this.tpLexems.SuspendLayout();
            this.tpSyntaxTree.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpLexems);
            this.tabControl1.Controls.Add(this.tpSyntaxTree);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(591, 272);
            this.tabControl1.TabIndex = 0;
            // 
            // tpLexems
            // 
            this.tpLexems.Controls.Add(this.tbLexems);
            this.tpLexems.Location = new System.Drawing.Point(4, 22);
            this.tpLexems.Name = "tpLexems";
            this.tpLexems.Padding = new System.Windows.Forms.Padding(3);
            this.tpLexems.Size = new System.Drawing.Size(583, 246);
            this.tpLexems.TabIndex = 0;
            this.tpLexems.Text = "Лексемы";
            this.tpLexems.UseVisualStyleBackColor = true;
            // 
            // tbLexems
            // 
            this.tbLexems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLexems.Location = new System.Drawing.Point(6, 6);
            this.tbLexems.Multiline = true;
            this.tbLexems.Name = "tbLexems";
            this.tbLexems.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLexems.Size = new System.Drawing.Size(571, 234);
            this.tbLexems.TabIndex = 0;
            // 
            // tpSyntaxTree
            // 
            this.tpSyntaxTree.Controls.Add(this.tvSyntaxTree);
            this.tpSyntaxTree.Location = new System.Drawing.Point(4, 22);
            this.tpSyntaxTree.Name = "tpSyntaxTree";
            this.tpSyntaxTree.Size = new System.Drawing.Size(583, 246);
            this.tpSyntaxTree.TabIndex = 1;
            this.tpSyntaxTree.Text = "Синтаксическое дерево";
            this.tpSyntaxTree.UseVisualStyleBackColor = true;
            // 
            // tvSyntaxTree
            // 
            this.tvSyntaxTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tvSyntaxTree.Location = new System.Drawing.Point(3, 3);
            this.tvSyntaxTree.Name = "tvSyntaxTree";
            this.tvSyntaxTree.Size = new System.Drawing.Size(577, 243);
            this.tvSyntaxTree.TabIndex = 0;
            // 
            // DEBUG
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(615, 296);
            this.Controls.Add(this.tabControl1);
            this.Name = "DEBUG";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DEBUG";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DEBUG_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tpLexems.ResumeLayout(false);
            this.tpLexems.PerformLayout();
            this.tpSyntaxTree.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLexems;
        private System.Windows.Forms.TextBox tbLexems;
        private System.Windows.Forms.TabPage tpSyntaxTree;
        private System.Windows.Forms.TreeView tvSyntaxTree;
    }
}