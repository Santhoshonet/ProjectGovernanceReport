namespace Installer
{
    partial class Installer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Installer));
            this.office2007BlackTheme1 = new Telerik.WinControls.Themes.Office2007BlackTheme();
            this.PnlConfiguration = new Telerik.WinControls.UI.RadPanel();
            this.BtnInstall = new Telerik.WinControls.UI.RadButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtUrl = new System.Windows.Forms.TextBox();
            this.LblStatus = new System.Windows.Forms.Label();
            this.PrbStatus = new Telerik.WinControls.UI.RadWaitingBar();
            this.PnlInstaller = new Telerik.WinControls.UI.RadPanel();
            this.PnlConfiguration2 = new Telerik.WinControls.UI.RadPanel();
            this.CmbWeb = new Telerik.WinControls.UI.RadComboBox();
            this.BtnInstall1 = new Telerik.WinControls.UI.RadButton();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PnlConfiguration)).BeginInit();
            this.PnlConfiguration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInstall)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PrbStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PnlInstaller)).BeginInit();
            this.PnlInstaller.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PnlConfiguration2)).BeginInit();
            this.PnlConfiguration2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CmbWeb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInstall1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // PnlConfiguration
            // 
            this.PnlConfiguration.Controls.Add(this.BtnInstall);
            this.PnlConfiguration.Controls.Add(this.label1);
            this.PnlConfiguration.Controls.Add(this.label2);
            this.PnlConfiguration.Controls.Add(this.TxtUrl);
            this.PnlConfiguration.Dock = System.Windows.Forms.DockStyle.Top;
            this.PnlConfiguration.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PnlConfiguration.Location = new System.Drawing.Point(0, 85);
            this.PnlConfiguration.Name = "PnlConfiguration";
            // 
            // 
            // 
            this.PnlConfiguration.RootElement.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PnlConfiguration.Size = new System.Drawing.Size(466, 85);
            this.PnlConfiguration.TabIndex = 11;
            // 
            // BtnInstall
            // 
            this.BtnInstall.AllowShowFocusCues = true;
            this.BtnInstall.ForeColor = System.Drawing.Color.White;
            this.BtnInstall.Location = new System.Drawing.Point(380, 56);
            this.BtnInstall.Name = "BtnInstall";
            // 
            // 
            // 
            this.BtnInstall.RootElement.ForeColor = System.Drawing.Color.White;
            this.BtnInstall.Size = new System.Drawing.Size(79, 21);
            this.BtnInstall.TabIndex = 43;
            this.BtnInstall.Text = "Install";
            this.BtnInstall.ThemeName = "Office2007Black";
            this.BtnInstall.Click += new System.EventHandler(this.BtnInstall_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(3, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Ex:- http://epm2007server/pwa01";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label2.Location = new System.Drawing.Point(3, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(320, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Enter PWA Instance URL from where you want to install";
            // 
            // TxtUrl
            // 
            this.TxtUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TxtUrl.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtUrl.Location = new System.Drawing.Point(6, 31);
            this.TxtUrl.Name = "TxtUrl";
            this.TxtUrl.Size = new System.Drawing.Size(453, 21);
            this.TxtUrl.TabIndex = 0;
            // 
            // LblStatus
            // 
            this.LblStatus.AutoSize = true;
            this.LblStatus.BackColor = System.Drawing.Color.Transparent;
            this.LblStatus.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblStatus.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.LblStatus.Location = new System.Drawing.Point(4, 20);
            this.LblStatus.Name = "LblStatus";
            this.LblStatus.Size = new System.Drawing.Size(210, 15);
            this.LblStatus.TabIndex = 1;
            this.LblStatus.Text = "Installing Application Please Wait.... ";
            // 
            // PrbStatus
            // 
            this.PrbStatus.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PrbStatus.Location = new System.Drawing.Point(7, 56);
            this.PrbStatus.Name = "PrbStatus";
            // 
            // 
            // 
            this.PrbStatus.RootElement.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PrbStatus.Size = new System.Drawing.Size(452, 11);
            this.PrbStatus.TabIndex = 2;
            this.PrbStatus.Text = "radWaitingBar1";
            this.PrbStatus.ThemeName = "ControlDefault";
            this.PrbStatus.WaitingSpeed = 10;
            // 
            // PnlInstaller
            // 
            this.PnlInstaller.Controls.Add(this.LblStatus);
            this.PnlInstaller.Controls.Add(this.PrbStatus);
            this.PnlInstaller.Dock = System.Windows.Forms.DockStyle.Top;
            this.PnlInstaller.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PnlInstaller.Location = new System.Drawing.Point(0, 0);
            this.PnlInstaller.Name = "PnlInstaller";
            // 
            // 
            // 
            this.PnlInstaller.RootElement.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PnlInstaller.Size = new System.Drawing.Size(466, 85);
            this.PnlInstaller.TabIndex = 10;
            // 
            // PnlConfiguration2
            // 
            this.PnlConfiguration2.Controls.Add(this.CmbWeb);
            this.PnlConfiguration2.Controls.Add(this.BtnInstall1);
            this.PnlConfiguration2.Controls.Add(this.label4);
            this.PnlConfiguration2.Dock = System.Windows.Forms.DockStyle.Top;
            this.PnlConfiguration2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PnlConfiguration2.Location = new System.Drawing.Point(0, 170);
            this.PnlConfiguration2.Name = "PnlConfiguration2";
            // 
            // 
            // 
            this.PnlConfiguration2.RootElement.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.PnlConfiguration2.Size = new System.Drawing.Size(466, 85);
            this.PnlConfiguration2.TabIndex = 44;
            // 
            // CmbWeb
            // 
            this.CmbWeb.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            this.CmbWeb.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CmbWeb.Location = new System.Drawing.Point(6, 32);
            this.CmbWeb.Name = "CmbWeb";
            // 
            // 
            // 
            this.CmbWeb.RootElement.AutoSizeMode = Telerik.WinControls.RadAutoSizeMode.WrapAroundChildren;
            this.CmbWeb.RootElement.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.CmbWeb.Size = new System.Drawing.Size(453, 20);
            this.CmbWeb.TabIndex = 44;
            this.CmbWeb.ThemeName = "Office2007Black";
            // 
            // BtnInstall1
            // 
            this.BtnInstall1.AllowShowFocusCues = true;
            this.BtnInstall1.ForeColor = System.Drawing.Color.White;
            this.BtnInstall1.Location = new System.Drawing.Point(380, 57);
            this.BtnInstall1.Name = "BtnInstall1";
            // 
            // 
            // 
            this.BtnInstall1.RootElement.ForeColor = System.Drawing.Color.White;
            this.BtnInstall1.Size = new System.Drawing.Size(79, 21);
            this.BtnInstall1.TabIndex = 43;
            this.BtnInstall1.Text = "Install";
            this.BtnInstall1.ThemeName = "Office2007Black";
            this.BtnInstall1.Click += new System.EventHandler(this.BtnInstall_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label4.Location = new System.Drawing.Point(3, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(153, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Select site name to install";
            // 
            // Installer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 283);
            this.Controls.Add(this.PnlConfiguration2);
            this.Controls.Add(this.PnlConfiguration);
            this.Controls.Add(this.PnlInstaller);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Installer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ITX Drawing Management Installer";
            this.ThemeName = "Office2007Black";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Installer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PnlConfiguration)).EndInit();
            this.PnlConfiguration.ResumeLayout(false);
            this.PnlConfiguration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInstall)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PrbStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PnlInstaller)).EndInit();
            this.PnlInstaller.ResumeLayout(false);
            this.PnlInstaller.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PnlConfiguration2)).EndInit();
            this.PnlConfiguration2.ResumeLayout(false);
            this.PnlConfiguration2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CmbWeb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnInstall1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Telerik.WinControls.Themes.Office2007BlackTheme office2007BlackTheme1;
        private Telerik.WinControls.UI.RadPanel PnlConfiguration;
        private Telerik.WinControls.UI.RadButton BtnInstall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtUrl;
        private System.Windows.Forms.Label LblStatus;
        private Telerik.WinControls.UI.RadWaitingBar PrbStatus;
        private Telerik.WinControls.UI.RadPanel PnlInstaller;
        private Telerik.WinControls.UI.RadPanel PnlConfiguration2;
        private Telerik.WinControls.UI.RadComboBox CmbWeb;
        private Telerik.WinControls.UI.RadButton BtnInstall1;
        private System.Windows.Forms.Label label4;
    }
}