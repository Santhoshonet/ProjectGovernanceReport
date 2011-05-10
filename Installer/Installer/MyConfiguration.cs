using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ITXProjectsLibrary;
using ITXProjectsLibrary.WebSvcProject;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Navigation;

namespace Installer
{
    class MyConfiguration
    {
        public static bool IsDemoOn { get { return true; } }

        public static string Application_Title { get { return "ITXProjectGovernanceReport"; } }

        public static MyUtilities.DeploymentLevel DeploymentLevel { get { return MyUtilities.DeploymentLevel.Site; } }

        public static string DefaultSiteUrl { get { return "http://epm2007demo/pwa01"; } }

        public static void StartInstallation(SPSite Site, SPWeb Web, Label LblStatus)
        {
            if (Web != null)
                Web.AllowUnsafeUpdates = true;

            MyUtilities.UpdateStatus(" Copying required files  ", LblStatus);
            Deployment.CopyFilesAndFolders(Application.StartupPath + "\\layouts", MyUtilities.LayoutsFolderPath, true);
            Deployment.CopyFilesAndFolders(Application.StartupPath + "\\Features", MyUtilities.FeaturesFolderPath, true);

            //Deployment.CreateGenericList(Site.RootWeb, "Project Governance Report", "For Project Governance Report.", true);

            //Deployment.InsertWebPart(Site.RootWeb, "Project Governance Report", true, "MSPageViewer.dwp", "Link",
            //                               "/_layouts/ITXCloseTasksUpdate/LockProjectTasks.ascx", "", 0, true);

            try
            {
                var navigationnode = new SPNavigationNode("Project Governance Report", "/" + Site.RootWeb.ServerRelativeUrl + "/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx", true);
                Site.RootWeb.Navigation.QuickLaunch.AddAsLast(navigationnode);
                Site.RootWeb.Update();
            }
            catch (Exception)
            {
            }

            MyUtilities.UpdateStatus(" Installing required feature ", LblStatus);
            // Creating an Batch file to install the Feature
            string WebPartInstallerPath = Application.StartupPath + "\\Install2.bat";
            if (File.Exists(WebPartInstallerPath))
                File.Delete(WebPartInstallerPath);
            try
            {
                var Writer = new StreamWriter(WebPartInstallerPath);
                Writer.WriteLine("@ echo off");
                Writer.WriteLine(@"stsadm -o installfeature -filename ITXProjectGovernanceReport\feature.xml");
                Writer.Flush();
                Writer.Close();
                Writer.Dispose();
            }
            catch (Exception)
            {
            }
            Deployment.ExecuteProcess(WebPartInstallerPath, ProcessWindowStyle.Hidden, true);

            try
            {
                MyUtilities.UpdateStatus(" Creatng configuration list and adding project records ", LblStatus);
                // define project server web service
                var Project_Svc = new Project
                {
                    Url = Site.Url + "/_vti_bin/psi/project.asmx",
                    UseDefaultCredentials = true,
                    AllowAutoRedirect = true
                };
                SPList configurationList = ITXPGReportUtilities.MyUtilities.GetConfigurationList(Site);
                if (configurationList != null)
                {
                    ProjectDataSet ProjectList = Project_Svc.ReadProjectList();
                    foreach (ProjectDataSet.ProjectRow PRow in ProjectList.Project.Rows)
                    {
                        ITXPGReportUtilities.MyUtilities.CheckandAddEntry(configurationList, PRow.PROJ_UID, Project_Svc);
                    }
                }
            }
            catch (Exception ex)
            {
                ITXPGReportUtilities.MyUtilities.ErrorLog("Error at populating the project records in the list due to " + ex.Message, EventLogEntryType.Error);
            }

            try
            {
                MyUtilities.UpdateStatus(" Updating web.config file ", LblStatus);
                //To Get Port from Siteurl
                Uri uri = new Uri(Site.Url);
                int Port = uri.Port;
                if (Port != 80)
                {
                    string Wss80Path = Deployment.GetWssVirtualDirectoryPath("80");
                    if (Wss80Path != string.Empty)
                    {
                        // Moving files from Current Application Path to Virtual directory
                        Deployment.CopyFilesAndFolders(Application.StartupPath + @"\bin", Wss80Path + @"\bin", true);
                        Wss80Path += @"\web.config";
                        Utilities.SetAttributeValueInWebConfig(Wss80Path, "configuration/system.web/trust", "level", "Full");
                    }
                }
                var ExtendedUrlList = new List<string>();
                foreach (int zoneindex in Enum.GetValues(typeof(SPUrlZone)))
                {
                    string ExtendedUrl = Deployment.GetVirtualDirectoryPath((SPUrlZone)zoneindex, Site);
                    bool Found = false;
                    foreach (string s in ExtendedUrlList)
                    {
                        if (s.ToLower().Trim() == ExtendedUrl.ToLower().Trim())
                        {
                            Found = true;
                            break;
                        }
                    }
                    if (!Found)
                    {
                        ExtendedUrlList.Add(ExtendedUrl);
                    }
                }
                foreach (string Urls in ExtendedUrlList)
                {
                    try
                    {
                        // Moving files from Current Application Path to Virtual directory
                        Deployment.CopyFilesAndFolders(Application.StartupPath + @"\bin", Urls + @"\bin", true);
                        Utilities.SetAttributeValueInWebConfig(Urls + @"\web.config", "configuration/system.web/trust", "level", "Full");
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }

            // Restart IIS
            MyUtilities.UpdateStatus("Restart IIS started ...", LblStatus);
            if (MyUtilities.ShowDialog("This will Reset the IIS. Are you sure? \r\n Recommended Yes", Telerik.WinControls.RadMessageIcon.Question) == DialogResult.Yes)
            {
                Application.DoEvents();
                Deployment.RestartIIS(Application.StartupPath);
            }
            Application.Exit();
        }
    }
}