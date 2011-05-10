using System;
using System.Diagnostics;
using ITXProjectsLibrary;
using ITXProjectsLibrary.WebSvcEvents;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace ITXPGReportUtilities
{
    public class FeatureEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            //Adding Project Published Event Handling Code starts here
            MyUtilities.ErrorLog("Feature Event-Activated Started", EventLogEntryType.SuccessAudit);
            SPSite site = null;
            if (properties.Definition.Scope == SPFeatureScope.Site)
            {
                site = (SPSite)properties.Feature.Parent;
                var spWebApplication = site.WebApplication;
                // Creating the Project Server Events
                try
                {
                    using (var Site = new SPSite(site.Url))
                    {
                        var Events_Svc = new Events
                                             {
                                                 UseDefaultCredentials = true,
                                                 AllowAutoRedirect = true,
                                                 Url = (Site.Url + @"/_vti_bin/psi/Events.asmx")
                                             };
                        string FilePath = SPUtility.GetGenericSetupPath(string.Empty) +
                                          @"\Template\layouts\ITXProjectGovernanceReport\ITXPGReportUtilities.dll";
                        if (!Utilities.CreatePSEvent(Events_Svc, FilePath, "WssInterop", "WssWorkspaceCreated",
                                                     "ITXPGReportconfigurationSync",
                                                     "For updating Project Governance Report, configuration list when a project workspace is created."))
                        {
                            MyUtilities.ErrorLog(
                                "Unable to Register WssInterop-WSssWorkspaceCreated event handler to project server. Please add manually using with the following details Assembly - ITXPGReportUtilities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=488363079fbfb28c , Class Name - ITXPGReportUtilities.ProjectWssWorkspaceCreatedEvent ",
                                EventLogEntryType.Error);
                        }
                    }
                }
                catch (Exception Ex1)
                {
                    MyUtilities.ErrorLog("Error at Registering event handler to project server due to : " + Ex1.Message, EventLogEntryType.Error);
                }

                if (spWebApplication != null)
                {
                    // Adding a Timejob in web application workspace permission synchronization
                    try
                    {
                        // Creating Timer Job if it not exists, otherwise skip
                        bool IsTimerJobFound = false;
                        // Make sure the job isn't already registered.
                        foreach (SPJobDefinition job in spWebApplication.JobDefinitions)
                        {
                            if (job.Name == PGReportJobDefinition._JOBNAME)
                            {
                                IsTimerJobFound = true;
                                break;
                            }
                        }
                        if (IsTimerJobFound == false)
                        {
                            // Install the job.
                            var jobDefinition = new PGReportJobDefinition(spWebApplication);
                            var Daily = new SPDailySchedule { BeginHour = 0 };
                            jobDefinition.Schedule = Daily;
                            jobDefinition.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        MyUtilities.ErrorLog("Error at Creating a Timer Job due to : " + ex.Message, EventLogEntryType.Error);
                    }
                }
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPSite site = null;
            if (properties.Definition.Scope == SPFeatureScope.Site)
            {
                site = (SPSite)properties.Feature.Parent;
                var spWebApplication = site.WebApplication;
                bool IsFeatureActivated_At_SomeOtherInstance = false;
                bool IsProceedValidate = false;
                if (spWebApplication != null)
                {
                    foreach (SPSite Site in spWebApplication.Sites)
                    {
                        if (Site.ID.ToString() != site.ID.ToString())
                            IsProceedValidate = true;

                        if (IsProceedValidate)
                        {
                            foreach (SPFeature Feature in Site.Features)
                            {
                                try
                                {
                                    if (Feature.Definition.DisplayName == MyUtilities.SPFeatureName)
                                    {
                                        if (Feature.Definition.ActivateOnDefault)
                                        {
                                            IsFeatureActivated_At_SomeOtherInstance = true;
                                            break;
                                        }
                                    }
                                }
                                catch (Exception) { }
                            }
                        }

                        IsProceedValidate = false;
                        if (IsFeatureActivated_At_SomeOtherInstance)
                            break;
                    }
                    if (IsFeatureActivated_At_SomeOtherInstance == false)
                    {
                        // Delete the job.
                        foreach (SPJobDefinition job in spWebApplication.JobDefinitions)
                        {
                            if (job.Name == PGReportJobDefinition._JOBNAME)
                            {
                                job.Delete();
                                spWebApplication.Update();
                                break;
                            }
                        }
                    }
                }
                // Deleting the Events
                try
                {
                    MyUtilities.ErrorLog("Feature Event-De-Activated Started", EventLogEntryType.SuccessAudit);
                    using (var Site = new SPSite(site.Url))
                    {
                        var Events_Svc = new Events();
                        Events_Svc.UseDefaultCredentials = true;
                        Events_Svc.AllowAutoRedirect = true;
                        Events_Svc.Url = Site.Url + @"/_vti_bin/psi/Events.asmx";
                        Utilities.DeletePSEvent(Events_Svc, "ITXPGReportUtilities.ProjectWssWorkspaceCreatedEvent");
                    }
                }
                catch (Exception Ex23)
                {
                    MyUtilities.ErrorLog("Error at removing Event handler due to : " + Ex23.Message, EventLogEntryType.Error);
                }
            }
        }

        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
        }

        public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        {
        }
    }
}