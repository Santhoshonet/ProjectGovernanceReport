using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using ITXProjectsLibrary;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

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
            //Deployment.CopyFilesAndFolders(Application.StartupPath + "\\Features", MyUtilities.FeaturesFolderPath, true);

            //Deployment.CreateGenericList(Site.RootWeb, "Project Governance Report", "For Project Governance Report.", true);

            //Deployment.InsertWebPart(Site.RootWeb, "Project Governance Report", true, "MSPageViewer.dwp", "Link",
            //                               "/_layouts/ITXCloseTasksUpdate/LockProjectTasks.ascx", "", 0, true);

            /*try
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

            */

            // executing database schema for Group Configurations

            try
            {
                string QRY = @"
                CREATE TABLE [groups] (
                [groups_id] INT NOT NULL,
                [<_u_i_d>k___backing_field] VARCHAR(255) NULL,
                [<name>k___backing_field] VARCHAR(255) NULL,
                [voa_version] SMALLINT NOT NULL,
                CONSTRAINT [pk_groups] PRIMARY KEY ([groups_id])
            );
            CREATE TABLE [groups_projects] (
                [groups_id] INT NOT NULL,
                [seq] INT NOT NULL,
                [projects_id] INT NULL,
                CONSTRAINT [pk_groups_projects] PRIMARY KEY ([groups_id], [seq])
            );
            CREATE TABLE [projects] (
                [projects_id] INT NOT NULL,
                [<name>k___backing_field] VARCHAR(255) NULL,
                [<uid>k___backing_field] VARCHAR(255) NULL,
                [voa_version] SMALLINT NOT NULL,
                CONSTRAINT [pk_projects] PRIMARY KEY ([projects_id])
            );
            CREATE TABLE [users] (
                [users_id] INT NOT NULL,
                [<_rsrc_u_i_d>k___backing_field] VARCHAR(255) NULL,
                [voa_version] SMALLINT NOT NULL,
                CONSTRAINT [pk_users] PRIMARY KEY ([users_id])
            );
            CREATE TABLE [users_groups] (
                [users_id] INT NOT NULL,
                [seq] INT NOT NULL,
                [groups_id] INT NULL,
                CONSTRAINT [pk_users_groups] PRIMARY KEY ([users_id], [seq])
            );
            CREATE TABLE [voa_keygen] (
                [table_name] VARCHAR(64) NOT NULL,
                [last_used_id] INT NOT NULL,
                CONSTRAINT [pk_voa_keygen] PRIMARY KEY ([table_name])
            );
            ALTER TABLE [groups_projects] ADD CONSTRAINT [ref_groups_projects_groups] FOREIGN KEY ([groups_id]) REFERENCES [groups]([groups_id]);
            ALTER TABLE [groups_projects] ADD CONSTRAINT [ref_groups_projects_projects] FOREIGN KEY ([projects_id]) REFERENCES [projects]([projects_id]);
            ALTER TABLE [users_groups] ADD CONSTRAINT [ref_users_groups_users] FOREIGN KEY ([users_id]) REFERENCES [users]([users_id]);
            ALTER TABLE [users_groups] ADD CONSTRAINT [ref_users_groups_groups] FOREIGN KEY ([groups_id]) REFERENCES [groups]([groups_id]);";
                string connstr = Utilities.GetProjectServerSQLDatabaseConnectionString(Site.ID,
                                                                                       Utilities.DatabaseType.
                                                                                           PublishedDatabase);
                var builder = new SqlConnectionStringBuilder(connstr);
                builder.InitialCatalog = MyUtilities.GovernanceReportGroupConfigDBname;
                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(QRY, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                        continue;
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