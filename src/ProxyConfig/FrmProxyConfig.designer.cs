namespace ProxyConfig
{
    partial class FrmProxyConfig
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
            this.btOk = new System.Windows.Forms.Button();
            this.gbEnableProxy = new System.Windows.Forms.GroupBox();
            this.tbProxyUserPassword = new System.Windows.Forms.TextBox();
            this.lbProxyUserPassword = new System.Windows.Forms.Label();
            this.tbProxyUserName = new System.Windows.Forms.TextBox();
            this.lbProxyUserName = new System.Windows.Forms.Label();
            this.lbProxyPort = new System.Windows.Forms.Label();
            this.lbProxyAddress = new System.Windows.Forms.Label();
            this.tbProxyPort = new System.Windows.Forms.TextBox();
            this.tbProxyAddress = new System.Windows.Forms.TextBox();
            this.gbProxyType = new System.Windows.Forms.GroupBox();
            this.rbSocks5 = new System.Windows.Forms.RadioButton();
            this.rbSocks4 = new System.Windows.Forms.RadioButton();
            this.rbHttp = new System.Windows.Forms.RadioButton();
            this.btCancel = new System.Windows.Forms.Button();
            this.cbEnableProxy = new System.Windows.Forms.CheckBox();
            this.gbEnableProxy.SuspendLayout();
            this.gbProxyType.SuspendLayout();
            this.SuspendLayout();
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(190, 214);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 23);
            this.btOk.TabIndex = 0;
            this.btOk.Text = "Ок";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // gbEnableProxy
            // 
            this.gbEnableProxy.Controls.Add(this.tbProxyUserPassword);
            this.gbEnableProxy.Controls.Add(this.lbProxyUserPassword);
            this.gbEnableProxy.Controls.Add(this.tbProxyUserName);
            this.gbEnableProxy.Controls.Add(this.lbProxyUserName);
            this.gbEnableProxy.Controls.Add(this.lbProxyPort);
            this.gbEnableProxy.Controls.Add(this.lbProxyAddress);
            this.gbEnableProxy.Controls.Add(this.tbProxyPort);
            this.gbEnableProxy.Controls.Add(this.tbProxyAddress);
            this.gbEnableProxy.Controls.Add(this.gbProxyType);
            this.gbEnableProxy.Location = new System.Drawing.Point(12, 12);
            this.gbEnableProxy.Name = "gbEnableProxy";
            this.gbEnableProxy.Size = new System.Drawing.Size(334, 196);
            this.gbEnableProxy.TabIndex = 1;
            this.gbEnableProxy.TabStop = false;
            this.gbEnableProxy.Text = "gpEnableProxy";
            // 
            // tbProxyUserPassword
            // 
            this.tbProxyUserPassword.Location = new System.Drawing.Point(190, 171);
            this.tbProxyUserPassword.Name = "tbProxyUserPassword";
            this.tbProxyUserPassword.PasswordChar = '*';
            this.tbProxyUserPassword.Size = new System.Drawing.Size(138, 20);
            this.tbProxyUserPassword.TabIndex = 9;
            // 
            // lbProxyUserPassword
            // 
            this.lbProxyUserPassword.AutoSize = true;
            this.lbProxyUserPassword.Location = new System.Drawing.Point(187, 155);
            this.lbProxyUserPassword.Name = "lbProxyUserPassword";
            this.lbProxyUserPassword.Size = new System.Drawing.Size(48, 13);
            this.lbProxyUserPassword.TabIndex = 8;
            this.lbProxyUserPassword.Text = "Пароль:";
            // 
            // tbProxyUserName
            // 
            this.tbProxyUserName.Location = new System.Drawing.Point(12, 171);
            this.tbProxyUserName.Name = "tbProxyUserName";
            this.tbProxyUserName.Size = new System.Drawing.Size(171, 20);
            this.tbProxyUserName.TabIndex = 7;
            // 
            // lbProxyUserName
            // 
            this.lbProxyUserName.AutoSize = true;
            this.lbProxyUserName.Location = new System.Drawing.Point(9, 155);
            this.lbProxyUserName.Name = "lbProxyUserName";
            this.lbProxyUserName.Size = new System.Drawing.Size(106, 13);
            this.lbProxyUserName.TabIndex = 6;
            this.lbProxyUserName.Text = "Имя пользователя:";
            // 
            // lbProxyPort
            // 
            this.lbProxyPort.AutoSize = true;
            this.lbProxyPort.Location = new System.Drawing.Point(275, 116);
            this.lbProxyPort.Name = "lbProxyPort";
            this.lbProxyPort.Size = new System.Drawing.Size(35, 13);
            this.lbProxyPort.TabIndex = 5;
            this.lbProxyPort.Text = "Порт:";
            // 
            // lbProxyAddress
            // 
            this.lbProxyAddress.AutoSize = true;
            this.lbProxyAddress.Location = new System.Drawing.Point(9, 116);
            this.lbProxyAddress.Name = "lbProxyAddress";
            this.lbProxyAddress.Size = new System.Drawing.Size(80, 13);
            this.lbProxyAddress.TabIndex = 4;
            this.lbProxyAddress.Text = "Адрес прокси:";
            // 
            // tbProxyPort
            // 
            this.tbProxyPort.Location = new System.Drawing.Point(278, 132);
            this.tbProxyPort.MaxLength = 5;
            this.tbProxyPort.Name = "tbProxyPort";
            this.tbProxyPort.Size = new System.Drawing.Size(50, 20);
            this.tbProxyPort.TabIndex = 3;
            this.tbProxyPort.TextChanged += new System.EventHandler(this.tbProxyPort_TextChanged);
            // 
            // tbProxyAddress
            // 
            this.tbProxyAddress.Location = new System.Drawing.Point(12, 132);
            this.tbProxyAddress.Name = "tbProxyAddress";
            this.tbProxyAddress.Size = new System.Drawing.Size(260, 20);
            this.tbProxyAddress.TabIndex = 2;
            // 
            // gbProxyType
            // 
            this.gbProxyType.Controls.Add(this.rbSocks5);
            this.gbProxyType.Controls.Add(this.rbSocks4);
            this.gbProxyType.Controls.Add(this.rbHttp);
            this.gbProxyType.Location = new System.Drawing.Point(6, 23);
            this.gbProxyType.Name = "gbProxyType";
            this.gbProxyType.Size = new System.Drawing.Size(322, 90);
            this.gbProxyType.TabIndex = 1;
            this.gbProxyType.TabStop = false;
            this.gbProxyType.Text = "Тип прокси:";
            // 
            // rbSocks5
            // 
            this.rbSocks5.AutoSize = true;
            this.rbSocks5.Enabled = false;
            this.rbSocks5.Location = new System.Drawing.Point(6, 65);
            this.rbSocks5.Name = "rbSocks5";
            this.rbSocks5.Size = new System.Drawing.Size(67, 17);
            this.rbSocks5.TabIndex = 2;
            this.rbSocks5.TabStop = true;
            this.rbSocks5.Text = "SOCKS5";
            this.rbSocks5.UseVisualStyleBackColor = true;
            // 
            // rbSocks4
            // 
            this.rbSocks4.AutoSize = true;
            this.rbSocks4.Enabled = false;
            this.rbSocks4.Location = new System.Drawing.Point(6, 42);
            this.rbSocks4.Name = "rbSocks4";
            this.rbSocks4.Size = new System.Drawing.Size(67, 17);
            this.rbSocks4.TabIndex = 1;
            this.rbSocks4.TabStop = true;
            this.rbSocks4.Text = "SOCKS4";
            this.rbSocks4.UseVisualStyleBackColor = true;
            // 
            // rbHttp
            // 
            this.rbHttp.AutoSize = true;
            this.rbHttp.Location = new System.Drawing.Point(6, 19);
            this.rbHttp.Name = "rbHttp";
            this.rbHttp.Size = new System.Drawing.Size(54, 17);
            this.rbHttp.TabIndex = 0;
            this.rbHttp.TabStop = true;
            this.rbHttp.Text = "HTTP";
            this.rbHttp.UseVisualStyleBackColor = true;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(271, 214);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // cbEnableProxy
            // 
            this.cbEnableProxy.AutoSize = true;
            this.cbEnableProxy.Location = new System.Drawing.Point(18, 12);
            this.cbEnableProxy.Name = "cbEnableProxy";
            this.cbEnableProxy.Size = new System.Drawing.Size(141, 17);
            this.cbEnableProxy.TabIndex = 3;
            this.cbEnableProxy.Text = "Использовать прокси:";
            this.cbEnableProxy.UseVisualStyleBackColor = true;
            this.cbEnableProxy.CheckedChanged += new System.EventHandler(this.cbEnableProxy_CheckedChanged);
            // 
            // FrmProxyConfig
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(353, 242);
            this.Controls.Add(this.cbEnableProxy);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.gbEnableProxy);
            this.Controls.Add(this.btOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FrmProxyConfig";
            this.Text = "Настройка прокси:";
            this.Load += new System.EventHandler(this.FrmProxyConfig_Load);
            this.gbEnableProxy.ResumeLayout(false);
            this.gbEnableProxy.PerformLayout();
            this.gbProxyType.ResumeLayout(false);
            this.gbProxyType.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.GroupBox gbEnableProxy;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.GroupBox gbProxyType;
        private System.Windows.Forms.RadioButton rbSocks5;
        private System.Windows.Forms.RadioButton rbSocks4;
        private System.Windows.Forms.RadioButton rbHttp;
        private System.Windows.Forms.TextBox tbProxyUserPassword;
        private System.Windows.Forms.Label lbProxyUserPassword;
        private System.Windows.Forms.TextBox tbProxyUserName;
        private System.Windows.Forms.Label lbProxyUserName;
        private System.Windows.Forms.Label lbProxyPort;
        private System.Windows.Forms.Label lbProxyAddress;
        private System.Windows.Forms.TextBox tbProxyPort;
        private System.Windows.Forms.TextBox tbProxyAddress;
        private System.Windows.Forms.CheckBox cbEnableProxy;
    }
}