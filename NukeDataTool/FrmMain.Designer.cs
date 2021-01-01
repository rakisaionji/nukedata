namespace NukeDataTool
{
    partial class FrmMain
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tblParams = new System.Windows.Forms.TableLayoutPanel();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.txtSrc = new System.Windows.Forms.TextBox();
            this.txtDst = new System.Windows.Forms.TextBox();
            this.btnKey = new System.Windows.Forms.Button();
            this.btnSource = new System.Windows.Forms.Button();
            this.btnDest = new System.Windows.Forms.Button();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.barProgress = new System.Windows.Forms.ProgressBar();
            this.tblMain = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.tblParams.SuspendLayout();
            this.tblMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Keyfile:";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "&Input:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "&Output:";
            // 
            // tblParams
            // 
            this.tblParams.AutoSize = true;
            this.tblParams.ColumnCount = 4;
            this.tblMain.SetColumnSpan(this.tblParams, 3);
            this.tblParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tblParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblParams.Controls.Add(this.label1, 0, 0);
            this.tblParams.Controls.Add(this.label3, 0, 2);
            this.tblParams.Controls.Add(this.label2, 0, 1);
            this.tblParams.Controls.Add(this.txtKey, 1, 0);
            this.tblParams.Controls.Add(this.txtSrc, 1, 1);
            this.tblParams.Controls.Add(this.txtDst, 1, 2);
            this.tblParams.Controls.Add(this.btnKey, 3, 0);
            this.tblParams.Controls.Add(this.btnSource, 3, 1);
            this.tblParams.Controls.Add(this.btnDest, 3, 2);
            this.tblParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblParams.Location = new System.Drawing.Point(13, 14);
            this.tblParams.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tblParams.Name = "tblParams";
            this.tblParams.RowCount = 3;
            this.tblParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblParams.Size = new System.Drawing.Size(574, 93);
            this.tblParams.TabIndex = 0;
            // 
            // txtKey
            // 
            this.txtKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tblParams.SetColumnSpan(this.txtKey, 2);
            this.txtKey.Location = new System.Drawing.Point(63, 4);
            this.txtKey.Name = "txtKey";
            this.txtKey.ReadOnly = true;
            this.txtKey.Size = new System.Drawing.Size(472, 23);
            this.txtKey.TabIndex = 1;
            // 
            // txtSrc
            // 
            this.txtSrc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tblParams.SetColumnSpan(this.txtSrc, 2);
            this.txtSrc.Location = new System.Drawing.Point(63, 35);
            this.txtSrc.Name = "txtSrc";
            this.txtSrc.ReadOnly = true;
            this.txtSrc.Size = new System.Drawing.Size(472, 23);
            this.txtSrc.TabIndex = 4;
            // 
            // txtDst
            // 
            this.txtDst.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tblParams.SetColumnSpan(this.txtDst, 2);
            this.txtDst.Location = new System.Drawing.Point(63, 66);
            this.txtDst.Name = "txtDst";
            this.txtDst.ReadOnly = true;
            this.txtDst.Size = new System.Drawing.Size(472, 23);
            this.txtDst.TabIndex = 7;
            // 
            // btnKey
            // 
            this.btnKey.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnKey.Location = new System.Drawing.Point(541, 4);
            this.btnKey.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnKey.Name = "btnKey";
            this.btnKey.Size = new System.Drawing.Size(30, 23);
            this.btnKey.TabIndex = 2;
            this.btnKey.Text = "...";
            this.btnKey.UseVisualStyleBackColor = true;
            this.btnKey.Click += new System.EventHandler(this.btnKey_Click);
            // 
            // btnSource
            // 
            this.btnSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSource.Location = new System.Drawing.Point(541, 35);
            this.btnSource.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSource.Name = "btnSource";
            this.btnSource.Size = new System.Drawing.Size(30, 23);
            this.btnSource.TabIndex = 5;
            this.btnSource.Text = "...";
            this.btnSource.UseVisualStyleBackColor = true;
            this.btnSource.Click += new System.EventHandler(this.btnSource_Click);
            // 
            // btnDest
            // 
            this.btnDest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnDest.Location = new System.Drawing.Point(541, 66);
            this.btnDest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDest.Name = "btnDest";
            this.btnDest.Size = new System.Drawing.Size(30, 23);
            this.btnDest.TabIndex = 8;
            this.btnDest.Text = "...";
            this.btnDest.UseVisualStyleBackColor = true;
            this.btnDest.Click += new System.EventHandler(this.btnDest_Click);
            // 
            // btnDecrypt
            // 
            this.btnDecrypt.Location = new System.Drawing.Point(76, 120);
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size(85, 30);
            this.btnDecrypt.TabIndex = 0;
            this.btnDecrypt.Text = "&Decrypt";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(167, 127);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(43, 16);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Ready";
            this.lblStatus.TextChanged += new System.EventHandler(this.lblStatus_TextChanged);
            // 
            // barProgress
            // 
            this.barProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tblMain.SetColumnSpan(this.barProgress, 2);
            this.barProgress.Location = new System.Drawing.Point(76, 162);
            this.barProgress.Maximum = 2147483647;
            this.barProgress.Name = "barProgress";
            this.barProgress.Size = new System.Drawing.Size(511, 23);
            this.barProgress.TabIndex = 3;
            // 
            // tblMain
            // 
            this.tblMain.ColumnCount = 5;
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tblMain.Controls.Add(this.tblParams, 1, 1);
            this.tblMain.Controls.Add(this.lblStatus, 3, 3);
            this.tblMain.Controls.Add(this.btnDecrypt, 2, 3);
            this.tblMain.Controls.Add(this.barProgress, 2, 5);
            this.tblMain.Controls.Add(this.label4, 1, 5);
            this.tblMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblMain.Location = new System.Drawing.Point(0, 0);
            this.tblMain.Name = "tblMain";
            this.tblMain.RowCount = 7;
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblMain.Size = new System.Drawing.Size(600, 200);
            this.tblMain.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label4.Location = new System.Drawing.Point(13, 165);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "© RAKI";
            // 
            // FrmMain
            // 
            this.AcceptButton = this.btnDecrypt;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 200);
            this.Controls.Add(this.tblMain);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2000, 240);
            this.MinimumSize = new System.Drawing.Size(500, 200);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NukeDataTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.tblParams.ResumeLayout(false);
            this.tblParams.PerformLayout();
            this.tblMain.ResumeLayout(false);
            this.tblMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tblParams;
        private System.Windows.Forms.Button btnKey;
        private System.Windows.Forms.Button btnSource;
        private System.Windows.Forms.Button btnDest;
        private System.Windows.Forms.ProgressBar barProgress;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel tblMain;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.TextBox txtKey;
        internal System.Windows.Forms.TextBox txtSrc;
        internal System.Windows.Forms.TextBox txtDst;
        private System.Windows.Forms.Button btnDecrypt;
    }
}

