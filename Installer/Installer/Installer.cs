using System;
using System.Security.Principal;
using Microsoft.SharePoint;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Application=System.Windows.Forms.Application;
using Cursors=System.Windows.Forms.Cursors;
namespace Installer
{
    public partial class Installer : RadForm
    {
        public Installer()
        {
            InitializeComponent();
        }
        string FolderPath = string.Empty;
        private string SiteUrl = string.Empty;
        private void Installer_Load(object sender, EventArgs e)
        {
            if (MyConfiguration.DeploymentLevel != MyUtilities.DeploymentLevel.None)
            {
                this.Text = MyConfiguration.Application_Title;
                this.Cursor = Cursors.WaitCursor;
                MyUtilities.UpdateStatus("Getting Site Url ....", LblStatus);
                PrbStatus.StartWaiting();
                Refresh();
                PnlInstaller.Visible = false;
                PnlConfiguration2.Visible = false;
                PnlConfiguration.Visible = true;
                this.Height = PnlConfiguration.Height + 25;
                if (MyConfiguration.IsDemoOn)
                    TxtUrl.Text = MyConfiguration.DefaultSiteUrl;
                Application.DoEvents();
                this.Cursor = Cursors.Default;
            }
            else
            {
                PnlConfiguration.Visible = false;
                PnlConfiguration2.Visible = false;
                PnlInstaller.Visible = true;
                this.Height = PnlInstaller.Height + 25;
                MyUtilities.UpdateStatus("Initializing ...", LblStatus);
                Application.DoEvents();
                MyConfiguration.StartInstallation(null, null, LblStatus);
            }
        }
        private void BtnInstall_Click(object sender, EventArgs e)
        {
            PnlConfiguration.Visible = false;
            PnlConfiguration2.Visible = false;
            PnlInstaller.Visible = true;
            this.Height = PnlInstaller.Height + 25;
            Application.DoEvents();
            MyUtilities.UpdateStatus("Connecting to SharePoint Site ....",LblStatus);
            try
            {
                SiteUrl = string.Empty;
                using (SPSite Site = new SPSite(TxtUrl.Text.Trim()))
                {
                    try
                    {
                        FolderPath = Application.StartupPath + "\\";
                        Application.DoEvents();
                        WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero);
                        MyUtilities.UpdateStatus("Installing Solutions ...",LblStatus);
                        MyConfiguration.StartInstallation(Site,Site.AllWebs[new Guid(CmbWeb.SelectedValue.ToString())],LblStatus);
                    }
                    catch (Exception ex)
                    {
                      MyUtilities.ShowDialog("An unknown error occured. Further details contact your vendor." + Environment.NewLine + "Error : " +Environment.NewLine + ex.Message, Telerik.WinControls.RadMessageIcon.Error);
                    }
                    finally
                    {
                        this.Close();
                        Application.Exit();
                    }
                }
                SiteUrl = TxtUrl.Text.Trim();
                this.Close();
            }
            catch (Exception ex)
            {
                MyUtilities.ShowDialog(ex.Message, Telerik.WinControls.RadMessageIcon.Error);
                PnlInstaller.Visible = false;
                PnlConfiguration.Visible = true;
                this.Height = PnlConfiguration.Height + 25;
                TxtUrl.Focus();
                Application.DoEvents();
                return;
            }
        }
        private void BtnInstall_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (MyConfiguration.DeploymentLevel != MyUtilities.DeploymentLevel.None)
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (TxtUrl.Text.Trim() == "")
                    {
                        this.Cursor = Cursors.Default;
                        MyUtilities.ShowDialog("Enter Site Url and then Press OK.", RadMessageIcon.Error);
                        TxtUrl.Focus();
                        return;
                    }
                    using (SPSite Site = new SPSite(TxtUrl.Text.Trim()))
                    {
                        if (MyConfiguration.DeploymentLevel == MyUtilities.DeploymentLevel.Workspace)
                        {
                            CmbWeb.Items.Clear();
                            RadComboBoxItem item1 = new RadComboBoxItem();
                            item1.Text = "Root Web";
                            CmbWeb.Items.Add(item1);
                            foreach (SPWeb Web in Site.AllWebs)
                            {
                                if (!Web.IsRootWeb)
                                {
                                    RadComboBoxItem item = new RadComboBoxItem();
                                    item.Text = Web.Title;
                                    item.Value = Web.ID.ToString();
                                    CmbWeb.Items.Add(item);
                                }
                            }
                            if (CmbWeb.Items.Count > 0)
                                CmbWeb.SelectedIndex = 0;
                            PnlConfiguration.Visible = false;
                            PnlInstaller.Visible = false;
                            PnlConfiguration2.Visible = true;
                            this.Height = PnlConfiguration2.Height + 25;
                            Application.DoEvents();
                            this.Cursor = Cursors.Default;
                        }
                        else
                        {
                            PnlConfiguration.Visible = false;
                            PnlConfiguration2.Visible = false;
                            PnlInstaller.Visible = true;
                            this.Height = PnlInstaller.Height + 25;
                            Application.DoEvents();
                            MyUtilities.UpdateStatus("Connecting to SharePoint Site ....", LblStatus);
                            MyConfiguration.StartInstallation(Site, Site.OpenWeb(), LblStatus);
                        }
                    }
                }
                else
                {
                    PnlConfiguration.Visible = false;
                    PnlConfiguration2.Visible = false;
                    PnlInstaller.Visible = true;
                    this.Height = PnlInstaller.Height + 25;
                    Application.DoEvents();
                    MyUtilities.UpdateStatus("Initializing ...", LblStatus);
                    MyConfiguration.StartInstallation(null, null, LblStatus);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MyUtilities.ShowDialog(ex.Message, RadMessageIcon.Error);
            }
        }
    }
}

