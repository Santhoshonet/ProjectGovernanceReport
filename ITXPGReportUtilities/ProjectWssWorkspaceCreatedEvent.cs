using System;
using System.Diagnostics;
using ITXProjectsLibrary;
using ITXProjectsLibrary.WebSvcProject;
using Microsoft.Office.Project.Server.Events;
using Microsoft.SharePoint;
using PSLibrary = Microsoft.Office.Project.Server.Library;

namespace ITXPGReportUtilities
{
    public class ProjectWssWorkspaceCreatedEvent : WssInteropEventReceiver
    {
        public override void OnWssWorkspaceCreated(PSLibrary.PSContextInfo contextInfo, WssWorkspaceCreatedEventArgs e)
        {
            try
            {
                using (var Site = new SPSite(contextInfo.SiteGuid))
                {
                    string defaultSiteUrl = string.Empty;
                    try
                    {
                        defaultSiteUrl = Utilities.GetDefaultZoneUri(Site);
                    }
                    catch (Exception)
                    { }

                    // define project server web service
                    var Project_Svc = new Project
                                          {
                                              Url = defaultSiteUrl + "/_vti_bin/psi/project.asmx",
                                              UseDefaultCredentials = true,
                                              AllowAutoRedirect = true
                                          };

                    Guid ProjectGuid = contextInfo.TrackingGuid;
                    SPList configurationList = MyUtilities.GetConfigurationList(Site);
                    if (configurationList != null)
                    {
                        MyUtilities.CheckandAddEntry(configurationList, ProjectGuid, Project_Svc);
                    }
                }
            }
            catch (Exception ex)
            {
                MyUtilities.ErrorLog("Error at WSSWorkspaceCreated Event while updating project information in the configuration list " + ex.Message, EventLogEntryType.Error);
            }
            base.OnWssWorkspaceCreated(contextInfo, e);
        }
    }
}