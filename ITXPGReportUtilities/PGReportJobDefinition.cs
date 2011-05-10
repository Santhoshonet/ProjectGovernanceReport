using System;
using System.Diagnostics;
using ITXProjectsLibrary.WebSvcProject;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace ITXPGReportUtilities
{
    public class PGReportJobDefinition : SPJobDefinition
    {
        internal const string _JOBNAME = "ITXPGReportConfigurationSync";

        public PGReportJobDefinition() : base() { }

        public PGReportJobDefinition(SPWebApplication webApp)
            : base(_JOBNAME, webApp, null, SPJobLockType.Job)
        {
            this.Title = _JOBNAME;
        }

        public override void Execute(Guid targetInstanceId)
        {
            MyUtilities.ErrorLog("The Process Started at " + DateTime.Now.ToString(), EventLogEntryType.SuccessAudit);
            try
            {
                foreach (SPSite Site in WebApplication.Sites)
                {
                    foreach (SPFeature Feature in Site.Features)
                    {
                        try
                        {
                            if (Feature.Definition.DisplayName == MyUtilities.SPFeatureName)
                            {
                                if (Feature.Definition.ActivateOnDefault)
                                {
                                    SPList configurationList = MyUtilities.GetConfigurationList(Site);
                                    if (configurationList != null)
                                    {
                                        // define project server web service
                                        var Project_Svc = new Project
                                        {
                                            Url = Site.Url + "/_vti_bin/psi/project.asmx",
                                            UseDefaultCredentials = true,
                                            AllowAutoRedirect = true
                                        };
                                        ProjectDataSet ProjectList = Project_Svc.ReadProjectList();
                                        foreach (ProjectDataSet.ProjectRow PRow in ProjectList.Project.Rows)
                                        {
                                            MyUtilities.CheckandAddEntry(configurationList, PRow.PROJ_UID, Project_Svc);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception ex)
            {
                MyUtilities.ErrorLog("Error at timer job while synchronizing configuration list due to " + ex.Message, EventLogEntryType.Error);
            }
            try
            {
                var weeklySchedule = new SPWeeklySchedule() { BeginHour = 0, BeginDayOfWeek = DayOfWeek.Sunday };
                Schedule = weeklySchedule;
                Update();
                base.Execute(targetInstanceId);
            }
            catch (Exception ex)
            {
                MyUtilities.ErrorLog("Error at timer job while updating its schedule due to " + ex.Message, EventLogEntryType.Error);
            }
        }
    }
}