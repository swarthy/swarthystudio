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
            this.tpTetrads = new System.Windows.Forms.TabPage();
            this.tbTetradList = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tpLexems.SuspendLayout();
            this.tpTetrads.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpLexems);
            this.tabControl1.Controls.Add(this.tpTetrads);
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
            this.tbLexems.Location = new System.Drawing.Point(3, 3);
            this.tbLexems.Multiline = true;
            this.tbLexems.Name = "tbLexems";
            this.tbLexems.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLexems.Size = new System.Drawing.Size(577, 240);
            this.tbLexems.TabIndex = 0;
            // 
            // tpTetrads
            // 
            this.tpTetrads.Controls.Add(this.tbTetradList);
            this.tpTetrads.Location = new System.Drawing.Point(4, 22);
            this.tpTetrads.Name = "tpTetrads";
            this.tpTetrads.Size = new System.Drawing.Size(583, 246);
            this.tpTetrads.TabIndex = 2;
            this.tpTetrads.Text = "Тетрады";
            this.tpTetrads.UseVisualStyleBackColor = true;
            // 
            // tbTetradList
            // 
            this.tbTetradList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTetradList.Location = new System.Drawing.Point(3, 3);
            this.tbTetradList.Multiline = true;
            this.tbTetradList.Name = "tbTetradList";
            this.tbTetradList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbTetradList.Size = new System.Drawing.Size(577, 240);
            this.tbTetradList.TabIndex = 0;
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
            this.tpTetrads.ResumeLayout(false);
            this.tpTetrads.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLexems;
        private System.Windows.Forms.TextBox tbLexems;
        private System.Windows.Forms.TabPage tpTetrads;
        private System.Windows.Forms.TextBox tbTetradList;
    }
}