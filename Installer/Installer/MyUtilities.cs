using System;
using System.Windows.Forms;
using Microsoft.SharePoint.Utilities;
using Telerik.WinControls;

namespace Installer
{
    class MyUtilities
    {
        // Project Governance Report Database name here
        public static string GovernanceReportGroupConfigDBname = "ITXBaseLineLogs"; //"ITXPGReportDataLayer";

        public static string LayoutsFolderPath { get { return SPUtility.GetGenericSetupPath(string.Empty) + "\\Template\\layouts"; } }

        public static string FeaturesFolderPath { get { return SPUtility.GetGenericSetupPath(string.Empty) + "\\Template\\Features"; } }

        public static void UpdateStatus(String Status, Label LblStatus)
        {
            LblStatus.Text = Status;
            Application.DoEvents();
        }

        public static DialogResult ShowDialog(string Message, RadMessageIcon Info)
        {
            RadMessageBox.SetThemeName("Office2007Black");
            if (Info == RadMessageIcon.Question)
                return RadMessageBox.Show(Message, MyConfiguration.Application_Title, MessageBoxButtons.YesNo, Info);
            else
                return RadMessageBox.Show(Message, MyConfiguration.Application_Title, MessageBoxButtons.OK, Info);
        }

        public enum DeploymentLevel
        {
            Site,
            Workspace,
            None
        }
    }
}