using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using ITXPGReportDataLayer;
using ITXProjectsLibrary;
using ITXProjectsLibrary.WebSvcCustomFields;
using Microsoft.SharePoint;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Telerik.OpenAccess;
using DataStoreEnum = ITXProjectsLibrary.WebSvcProject.DataStoreEnum;
using Project = ITXProjectsLibrary.WebSvcProject.Project;
using ProjectDataSet = ITXProjectsLibrary.WebSvcProject.ProjectDataSet;

/*
 using System.Collections;
using ITXProjectsLibrary.WebSvcCustomFields;
using ITXProjectsLibrary.WebSvcLookupTable;
using ITXProjectsLibrary.WebSvcSecurity;
using Microsoft.Office.Project.Server.Library;
using Security = ITXProjectsLibrary.WebSvcSecurity.Security;
 */

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public class MyUtilities
    {
        // Project Governance Report Database name here
        public static bool IndividualPages = true;
        public static string GovernanceReportGroupConfigDBname = "ITXPGReportDataLayer"; //"ITXBaseLineLogs";//

        public static string CustomFieldName = "CIMBTaskType";
        public static string GroupListName = "ProjectType";
        public static string GroupFieldName = "Group";
        public static string ProjectUIDFieldName = "ProjectUID";

        public static string Project_Stream_Fieldname = "Program_Code";
        public static string Project_Status_Fieldname = "Project Status";

        public static bool DevelopMode = true;

        public static string ProjectServerInstanceURL(SPContext context)
        {
            if (context != null)
                return context.Site.Url;
            return "http://epm2007demo/pwa04";
        }

        public static void ErrorLog(string LogStr, EventLogEntryType Type)
        {
            try
            {
                System.Security.Principal.WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero);
                var El = new EventLog();
                if (EventLog.SourceExists("ITXProjectGovernanceReport") == false)
                    EventLog.CreateEventSource("ITXProjectGovernanceReport", "ITXProjectGovernanceReport");
                El.Source = "ITXProjectGovernanceReport";
                El.WriteEntry(LogStr, Type);
                El.Close();
                wic.Undo();
            }
            catch (Exception Ex87)
            {
                WriteTextLog(Ex87.Message + "\r" + LogStr);
            }
        }

        public static void WriteTextLog(string LogStr)
        {
            try
            {
                System.Security.Principal.WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero);
                var Writer = new StreamWriter(@"c:\ITXProjectGovernanceReport.txt", true);
                Writer.WriteLine(LogStr);
                Writer.Close();
                Writer.Dispose();
            }
            catch
            {
                return;
            }
        }

        public static DataTable GetGovernanceReport(string SiteUrl, Guid CurrentUserUID)
        {
            var ResultDataTable = new DataTable();
            ResultDataTable.Columns.Add("Title");
            ResultDataTable.Columns.Add("Start");
            ResultDataTable.Columns.Add("Finish");
            ResultDataTable.Columns.Add("Type");
            try
            {
                //User Impersionation
                WindowsImpersonationContext wik = null;
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (var Site = new SPSite(SiteUrl))
                    {
                        SiteUrl = Utilities.GetDefaultZoneUri(Site);

                        try
                        {
                            wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                        }
                        catch (Exception)
                        { }

                        ModifyConnectionString(SiteUrl);

                        using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                        {
                            // Adding the current user if not exits
                            int count = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                         where c.ResourceUID.Equals(CurrentUserUID.ToString())
                                         select c).Count();
                            if (count == 0)
                            {
                                scope.Transaction.Begin();
                                var user = new Users();
                                user.ResourceUID = CurrentUserUID.ToString();
                                scope.Add(user);
                                scope.Transaction.Commit();
                            }

                            List<Users> users = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                 where c.ResourceUID.Equals(CurrentUserUID.ToString())
                                                 select c).ToList();

                            var Project_Svc = new Project()
                                                  {
                                                      AllowAutoRedirect = true,
                                                      Url = SiteUrl + "/_vti_bin/psi/project.asmx",
                                                      UseDefaultCredentials = true
                                                  };

                            var CustomField_Svc = new CustomFields()
                                                      {
                                                          AllowAutoRedirect = true,
                                                          Url = SiteUrl + "/_vti_bin/psi/customfields.asmx",
                                                          UseDefaultCredentials = true
                                                      };

                            if (Utilities.IsCustomFieldFound(CustomField_Svc, CustomFieldName))
                            {
                                var ProjectIDs = Utilities.GetProjectUIDList(Project_Svc, false, false);

                                string ProjectsList = string.Empty;
                                foreach (var projectID in ProjectIDs)
                                {
                                    if (projectID != Guid.Empty)
                                    {
                                        if (ProjectsList == string.Empty)
                                            ProjectsList += "('" + projectID;
                                        else
                                            ProjectsList += "','" + projectID;
                                    }
                                }
                                ProjectsList += "')";
                                string Qry =
                                     @"SELECT  puv.ProjectUID,puv." + Project_Stream_Fieldname + @", puv.ProjectName, puv.ProjectStartDate, puv.ProjectFinishDate, '0' as [Type]
                                into #t1
                                FROM         MSP_EpmProject_UserView as puv
                                WHERE      (puv.[" + Project_Status_Fieldname + @"] <> N'[Closed]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Cancelled]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Replaced]')
                                SELECT     tuv.ProjectUID, puv." + Project_Stream_Fieldname + @", " +
                                 CustomFieldName +
                                 @" AS Title, MIN(tuv.TaskStartDate) AS Start, MAX(tuv.TaskFinishDate) AS [End], '1' as [Type]
                                into #t2
                                FROM        MSP_EpmTask_UserView AS tuv INNER JOIN
                                            MSP_EpmProject_UserView AS puv ON tuv.ProjectUID = puv.ProjectUID
                                WHERE      (puv.[" + Project_Status_Fieldname + @"] <> N'[Closed]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Cancelled]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Replaced]')
                                GROUP BY " + CustomFieldName + @", puv.Program_Code, tuv.ProjectUID HAVING (CIMBTaskType IS NOT NULL)
                                INSERT into #t2
                                select #t1.ProjectUID,#t1.Program_Code, #t1.ProjectName, #t1.ProjectStartDate, #t1.ProjectFinishDate, #t1.[Type]
                                FROM #t1 INNER JOIN (SELECT DISTINCT #t2.ProjectUID from #t2) AS t2temp ON t2temp.ProjectUID =#t1.ProjectUID
                                SELECT     *
                                FROM         [#t2]
                                where ProjectUID in " +
                                 ProjectsList +
                                 @"
                                ORDER BY ProjectUID,[Type], Start
                                drop table #t1
                                drop table #t2";

                                var Conn = new SqlConnection(GetDataBaseConnectionString(SiteUrl));
                                Conn.Open();

                                var Result_set = new DataSet();
                                var Adapter = new SqlDataAdapter(new SqlCommand(Qry, Conn));
                                Adapter.Fill(Result_set);

                                DataRow newrow;
                                var grouptable = new Hashtable();
                                var datarows = new List<datarow>();
                                string groupname = string.Empty;
                                foreach (DataRow row in Result_set.Tables[0].Rows)
                                {
                                    if (row["Type"].ToString() == "0")
                                    {
                                        List<Groups> groups =
                                            (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                             from d in c.groups
                                             from e in d.projects
                                             where
                                                 c.ResourceUID.Equals(CurrentUserUID.ToString()) &&
                                                 e.uid.Equals(row["ProjectUID"].ToString())
                                             select d).ToList();
                                        if (groups.Count > 0)
                                        {
                                            groupname = groups[0].name;
                                            var drow = new datarow();
                                            drow.type = "Group";
                                            drow.title = groupname;
                                            drow.startdate = DateTime.MinValue;
                                            drow.enddate = DateTime.MaxValue;
                                            if (grouptable.ContainsKey(groupname))
                                            {
                                                datarows = (List<datarow>)grouptable[groupname];

                                                drow = new datarow();
                                                drow.type = "Project";
                                                drow.startdate = Convert.ToDateTime(row["Start"]);
                                                drow.enddate = Convert.ToDateTime(row["End"]);
                                                drow.title = row["Title"].ToString();

                                                datarows.Add(drow);
                                                grouptable[groupname] = datarows;
                                            }
                                            else
                                            {
                                                datarows = new List<datarow>();
                                                datarows.Add(drow);

                                                drow = new datarow();
                                                drow.type = "Project";
                                                drow.startdate = Convert.ToDateTime(row["Start"]);
                                                drow.enddate = Convert.ToDateTime(row["End"]);
                                                drow.title = row["Title"].ToString();

                                                datarows.Add(drow);
                                                grouptable.Add(groupname, datarows);
                                            }
                                        }
                                        else
                                        {
                                            groupname = string.Empty;
                                            string[] streams = row["Program_Code"].ToString().Split('.');
                                            if (streams.Length > 3)
                                                groupname = streams[4];
                                            else if (streams.Length > 0)
                                                groupname = streams[streams.Length - 1];
                                            if (groupname == string.Empty)
                                                groupname = "Not Configured.";

                                            groups =
                                                (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                 from d in c.groups
                                                 where
                                                     c.ResourceUID.Equals(CurrentUserUID.ToString()) &&
                                                     d.name.Equals(groupname)
                                                 select d).ToList();
                                            if (groups.Count == 0)
                                            {
                                                scope.Transaction.Begin();
                                                var group = new Groups();
                                                group.name = groupname;
                                                group.UID = Guid.NewGuid().ToString();
                                                var project = new Projects();
                                                project.name = row["Title"].ToString();
                                                project.uid = row["ProjectUID"].ToString();
                                                group.projects.Add(project);
                                                users[0].groups.Add(group);
                                                scope.Add(users[0]);
                                                scope.Transaction.Commit();
                                            }
                                            else
                                            {
                                                scope.Transaction.Begin();
                                                var project = new Projects();
                                                project.name = row["Title"].ToString();
                                                project.uid = row["ProjectUID"].ToString();
                                                groups[0].projects.Add(project);
                                                scope.Add(groups[0]);
                                                scope.Transaction.Commit();
                                            }

                                            var drow = new datarow();
                                            drow.type = "Group";
                                            drow.title = groupname;
                                            drow.startdate = DateTime.MinValue;
                                            drow.enddate = DateTime.MaxValue;
                                            if (grouptable.ContainsKey(groupname))
                                            {
                                                datarows = (List<datarow>)grouptable[groupname];
                                                drow = new datarow();
                                                drow.type = "Project";
                                                drow.startdate = Convert.ToDateTime(row["Start"]);
                                                drow.enddate = Convert.ToDateTime(row["End"]);
                                                drow.title = row["Title"].ToString();
                                                datarows.Add(drow);
                                                grouptable[groupname] = datarows;
                                            }
                                            else
                                            {
                                                datarows = new List<datarow>();
                                                datarows.Add(drow);
                                                drow = new datarow();
                                                drow.type = "Project";
                                                drow.startdate = Convert.ToDateTime(row["Start"]);
                                                drow.enddate = Convert.ToDateTime(row["End"]);
                                                drow.title = row["Title"].ToString();
                                                datarows.Add(drow);
                                                grouptable.Add(groupname, datarows);
                                            }
                                        }
                                    }
                                    else if (groupname != string.Empty)
                                    {
                                        datarows =
                                            (List<datarow>)
                                            grouptable[groupname];
                                        var drow = new datarow();
                                        drow.title = row["Title"].ToString();
                                        drow.startdate =
                                            Convert.ToDateTime(row["Start"]);
                                        drow.enddate =
                                            Convert.ToDateTime(row["End"]);
                                        drow.type = "CF";
                                        datarows.Add(drow);
                                        grouptable[groupname] = datarows;
                                    }
                                }

                                // Adding the rows into datatable
                                foreach (DictionaryEntry drws in grouptable)
                                {
                                    foreach (datarow drow in (List<datarow>)drws.Value)
                                    {
                                        DataRow row = ResultDataTable.NewRow();
                                        row["Title"] = drow.title;
                                        row["Start"] = drow.startdate;
                                        row["Finish"] = drow.enddate;
                                        row["Type"] = drow.type;
                                        ResultDataTable.Rows.Add(row);
                                    }
                                }
                            }
                            else
                            {
                                ErrorLog(
                                    "The Custom field called " + CustomFieldName + " is not found in the instanse " +
                                    Site.Url, EventLogEntryType.FailureAudit);
                            }
                        }
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                ErrorLog("Error at loading project list due to " + ex.Message, EventLogEntryType.Error);
            }
            return ResultDataTable;
        }

        public static DataTable GetProjects_DataTable(string SiteUrl, Guid CurrentUserUID)
        {
            var ResultDataTable = new DataTable();
            ResultDataTable.Columns.Add("ProjectUID");
            ResultDataTable.Columns.Add("Title");
            try
            {
                //User Impersionation
                WindowsImpersonationContext wik = null;
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    using (var Site = new SPSite(SiteUrl))
                    {
                        SiteUrl = Utilities.GetDefaultZoneUri(Site);

                        try
                        {
                            wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                        }
                        catch (Exception)
                        { }

                        ModifyConnectionString(SiteUrl);

                        using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                        {
                            List<Users> users = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                 where c.ResourceUID.Equals(CurrentUserUID.ToString())
                                                 select c).ToList();

                            var Project_Svc = new Project()
                            {
                                AllowAutoRedirect = true,
                                Url = SiteUrl + "/_vti_bin/psi/project.asmx",
                                UseDefaultCredentials = true
                            };

                            var CustomField_Svc = new CustomFields()
                            {
                                AllowAutoRedirect = true,
                                Url = SiteUrl + "/_vti_bin/psi/customfields.asmx",
                                UseDefaultCredentials = true
                            };

                            if (Utilities.IsCustomFieldFound(CustomField_Svc, CustomFieldName))
                            {
                                var ProjectIDs = Utilities.GetProjectUIDList(Project_Svc, false, false);

                                string ProjectsList = string.Empty;
                                foreach (var projectID in ProjectIDs)
                                {
                                    if (projectID != Guid.Empty)
                                    {
                                        if (ProjectsList == string.Empty)
                                            ProjectsList += "('" + projectID;
                                        else
                                            ProjectsList += "','" + projectID;
                                    }
                                }
                                ProjectsList += "')";

                                string Qry =
                                    @"SELECT  puv.ProjectUID,puv." + Project_Stream_Fieldname + @", puv.ProjectName, puv.ProjectStartDate, puv.ProjectFinishDate, '0' as [Type]
                                into #t1
                                FROM         MSP_EpmProject_UserView as puv
                                WHERE      (puv.[" + Project_Status_Fieldname + @"] <> N'[Closed]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Cancelled]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Replaced]')
                                SELECT     tuv.ProjectUID, puv." + Project_Stream_Fieldname + @", " +
                                CustomFieldName +
                                @" AS Title, MIN(tuv.TaskStartDate) AS Start, MAX(tuv.TaskFinishDate) AS [End], '1' as [Type]
                                into #t2
                                FROM        MSP_EpmTask_UserView AS tuv INNER JOIN
                                            MSP_EpmProject_UserView AS puv ON tuv.ProjectUID = puv.ProjectUID
                                WHERE      (puv.[" + Project_Status_Fieldname + @"] <> N'[Closed]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Cancelled]') AND (puv.[" + Project_Status_Fieldname + @"] <> N'[Replaced]')
                                GROUP BY " + CustomFieldName + @", puv.Program_Code, tuv.ProjectUID HAVING (CIMBTaskType IS NOT NULL)
                                INSERT into #t2
                                select #t1.ProjectUID,#t1.Program_Code, #t1.ProjectName, #t1.ProjectStartDate, #t1.ProjectFinishDate, #t1.[Type]
                                FROM #t1 INNER JOIN (SELECT DISTINCT #t2.ProjectUID from #t2) AS t2temp ON t2temp.ProjectUID =#t1.ProjectUID
                                SELECT     ProjectUID,Title,Program_Code
                                FROM         [#t2]
                                where ProjectUID in " +
                                ProjectsList +
                                @" and [Type] = 0
                                ORDER BY ProjectUID,[Type], Start
                                drop table #t1
                                drop table #t2";

                                var Conn = new SqlConnection(GetDataBaseConnectionString(SiteUrl));
                                Conn.Open();

                                var Result_set = new DataSet();
                                var Adapter = new SqlDataAdapter(new SqlCommand(Qry, Conn));
                                Adapter.Fill(Result_set);

                                string groupname = string.Empty;
                                foreach (DataRow row in Result_set.Tables[0].Rows)
                                {
                                    List<Groups> groups =
                                        (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                         from d in c.groups
                                         from e in d.projects
                                         where
                                             c.ResourceUID.Equals(CurrentUserUID.ToString()) &&
                                             e.uid.Equals(row["ProjectUID"].ToString())
                                         select d).ToList();
                                    if (groups.Count > 0)
                                    {
                                        DataRow new_row = ResultDataTable.NewRow();
                                        new_row["ProjectUID"] = row["ProjectUID"].ToString();
                                        new_row["Title"] = row["Title"].ToString();
                                        ResultDataTable.Rows.Add(new_row);
                                    }
                                    else
                                    {
                                        groupname = string.Empty;
                                        string[] streams = row["Program_Code"].ToString().Split('.');
                                        if (streams.Length > 3)
                                            groupname = streams[4];
                                        else if (streams.Length > 0)
                                            groupname = streams[streams.Length - 1];
                                        if (groupname == string.Empty)
                                            groupname = "Not Configured.";

                                        groups =
                                            (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                             from d in c.groups
                                             where
                                                 c.ResourceUID.Equals(CurrentUserUID.ToString()) &&
                                                 d.name.Equals(groupname)
                                             select d).ToList();
                                        if (groups.Count == 0)
                                        {
                                            scope.Transaction.Begin();
                                            var group = new Groups();
                                            group.name = groupname;
                                            group.UID = Guid.NewGuid().ToString();
                                            var project = new Projects();
                                            project.name = row["Title"].ToString();
                                            project.uid = row["ProjectUID"].ToString();
                                            group.projects.Add(project);
                                            users[0].groups.Add(group);
                                            scope.Add(users[0]);
                                            scope.Transaction.Commit();
                                        }
                                        else
                                        {
                                            scope.Transaction.Begin();
                                            var project = new Projects();
                                            project.name = row["Title"].ToString();
                                            project.uid = row["ProjectUID"].ToString();
                                            groups[0].projects.Add(project);
                                            scope.Add(groups[0]);
                                            scope.Transaction.Commit();
                                        }
                                        DataRow new_row = ResultDataTable.NewRow();
                                        new_row["ProjectUID"] = row["ProjectUID"].ToString();
                                        new_row["Title"] = row["Title"].ToString();
                                        ResultDataTable.Rows.Add(new_row);
                                    }
                                }
                            }
                            else
                            {
                                ErrorLog(
                                    "The Custom field called " + CustomFieldName + " is not found in the instanse " +
                                    Site.Url, EventLogEntryType.FailureAudit);
                            }
                        }
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                ErrorLog("Error at loading project list due to " + ex.Message, EventLogEntryType.Error);
            }
            return ResultDataTable;
        }

        #region "Version 3"

        //        public static DataTable GetGovernanceReport(string SiteUrl)
        //        {
        //            var ResultDataTable = new DataTable();
        //            ResultDataTable.Columns.Add("Title");
        //            ResultDataTable.Columns.Add("Start");
        //            ResultDataTable.Columns.Add("Finish");
        //            ResultDataTable.Columns.Add("Type");
        //            try
        //            {
        //                //User Impersionation
        //                WindowsImpersonationContext wik = null;
        //                SPSecurity.RunWithElevatedPrivileges(delegate
        //                {
        //                    using (var Site = new SPSite(SiteUrl))
        //                    {
        //                        SiteUrl = Utilities.GetDefaultZoneUri(Site);

        //                        SPList GroupList = null;
        //                        try
        //                        {
        //                            GroupList = Site.RootWeb.Lists[GroupListName];
        //                        }
        //                        catch (Exception)
        //                        {
        //                        }
        //                        if (GroupList == null)
        //                        {
        //                            ErrorLog("The Project Group Configuration List called  " + GroupListName + " not found in the Site called " + SiteUrl, EventLogEntryType.Error);
        //                        }
        //                        else
        //                        {
        //                            try
        //                            {
        //                                wik = WindowsIdentity.Impersonate(IntPtr.Zero);
        //                            }
        //                            catch (Exception)
        //                            { }

        //                            var Project_Svc = new Project()
        //                            {
        //                                AllowAutoRedirect = true,
        //                                Url = SiteUrl + "/_vti_bin/psi/project.asmx",
        //                                UseDefaultCredentials = true
        //                            };

        //                            var CustomField_Svc = new CustomFields()
        //                            {
        //                                AllowAutoRedirect = true,
        //                                Url = SiteUrl + "/_vti_bin/psi/customfields.asmx",
        //                                UseDefaultCredentials = true
        //                            };
        //                            if (Utilities.IsCustomFieldFound(CustomField_Svc, CustomFieldName))
        //                            {
        //                                var ProjectIDs = Utilities.GetProjectUIDList(Project_Svc, false, false);

        //                                string ProjectsList = string.Empty;
        //                                foreach (var projectID in ProjectIDs)
        //                                {
        //                                    if (ProjectsList == string.Empty)
        //                                        ProjectsList += "('" + projectID;
        //                                    else
        //                                        ProjectsList += "','" + projectID;
        //                                }
        //                                ProjectsList += "')";

        //                                string Qry =
        //                                    @"SELECT  ProjectUID, ProjectName, ProjectStartDate, ProjectFinishDate, '0' as [Type]
        //                                                                                            into #t1
        //                                                                                        FROM         MSP_EpmProject_UserView
        //
        //                                                                                        SELECT     ProjectUID, " +
        //                                    CustomFieldName +
        //                                    @" AS Title, MIN(TaskStartDate) AS Start, MAX(TaskFinishDate) AS [End], '1' as [Type]
        //                                                                                        into #t2
        //                                                                                        FROM         MSP_EpmTask_UserView
        //                                                                                        GROUP BY " +
        //                                    CustomFieldName +
        //                                    @", ProjectUID
        //                                                                                        HAVING      (CIMBTaskType IS NOT NULL)
        //
        //                                                                                        INSERT into #t2
        //                                                                                        select #t1.ProjectUID, #t1.ProjectName, #t1.ProjectStartDate, #t1.ProjectFinishDate, #t1.[Type]
        //                                                                                        FROM #t1 INNER JOIN (SELECT DISTINCT #t2.ProjectUID from #t2) AS t2temp ON t2temp.ProjectUID =#t1.ProjectUID
        //
        //                                                                                        SELECT     *
        //                                                                                        FROM         [#t2]
        //                                                                                        where ProjectUID in " +
        //                                    ProjectsList +
        //                                    @"
        //                                                                                        ORDER BY ProjectUID,[Type], Start
        //
        //                                                                                        drop table #t1
        //                                                                                        drop table #t2";

        //                                var Conn = new SqlConnection(GetDataBaseConnectionString(SiteUrl));
        //                                Conn.Open();

        //                                var Result_set = new DataSet();
        //                                var Adapter = new SqlDataAdapter(new SqlCommand(Qry, Conn));
        //                                Adapter.Fill(Result_set);

        //                                DataRow newrow;
        //                                var grouptable = new Hashtable();
        //                                var datarows = new List<datarow>();
        //                                string groupname = string.Empty;
        //                                foreach (DataRow row in Result_set.Tables[0].Rows)
        //                                {
        //                                    if (row["Type"].ToString() == "0")
        //                                    {
        //                                        var query = new SPQuery
        //                                        {
        //                                            Query =
        //                                                @"<Where><Eq><FieldRef Name='ProjectUID' /><Value Type='Text'>" +
        //                                                row[
        //                                                    "ProjectUID"
        //                                                    ] +
        //                                                @"</Value></Eq></Where>"
        //                                        };
        //                                        SPListItemCollection collection = GroupList.GetItems(query);
        //                                        if (collection.Count > 0)
        //                                        {
        //                                            SPListItem item = collection[0];
        //                                            groupname = item[GroupFieldName].ToString();
        //                                            var drow = new datarow();
        //                                            drow.type = "Group";
        //                                            drow.title = item[GroupFieldName].ToString();
        //                                            drow.startdate = DateTime.MinValue;
        //                                            drow.enddate = DateTime.MaxValue;
        //                                            if (grouptable.ContainsKey(item[GroupFieldName].ToString()))
        //                                            {
        //                                                datarows =
        //                                                    (List<datarow>)
        //                                                    grouptable[
        //                                                        item[GroupFieldName].
        //                                                            ToString()];

        //                                                drow = new datarow();
        //                                                drow.type = "Project";
        //                                                drow.startdate =
        //                                                    Convert.ToDateTime(
        //                                                        row["Start"]);
        //                                                drow.enddate =
        //                                                    Convert.ToDateTime(
        //                                                        row["End"]);
        //                                                drow.title =
        //                                                    row["Title"].ToString();

        //                                                datarows.Add(drow);
        //                                                grouptable[
        //                                                    item[GroupFieldName].
        //                                                        ToString()] = datarows;
        //                                            }
        //                                            else
        //                                            {
        //                                                datarows = new List<datarow>();
        //                                                datarows.Add(drow);

        //                                                drow = new datarow();
        //                                                drow.type = "Project";
        //                                                drow.startdate =
        //                                                    Convert.ToDateTime(
        //                                                        row["Start"]);
        //                                                drow.enddate =
        //                                                    Convert.ToDateTime(
        //                                                        row["End"]);
        //                                                drow.title =
        //                                                    row["Title"].ToString();

        //                                                datarows.Add(drow);
        //                                                grouptable.Add(
        //                                                    item[GroupFieldName].
        //                                                        ToString(), datarows);
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            groupname = "Not Configured.";
        //                                            var drow = new datarow();
        //                                            drow.type = "Group";
        //                                            drow.title = groupname;
        //                                            drow.startdate = DateTime.MinValue;
        //                                            drow.enddate = DateTime.MaxValue;
        //                                            if (grouptable.ContainsKey(groupname))
        //                                            {
        //                                                datarows = (List<datarow>)grouptable[groupname];
        //                                                drow = new datarow();
        //                                                drow.type = "Project";
        //                                                drow.startdate = Convert.ToDateTime(row["Start"]);
        //                                                drow.enddate = Convert.ToDateTime(row["End"]);
        //                                                drow.title = row["Title"].ToString();
        //                                                datarows.Add(drow);
        //                                                grouptable[groupname] = datarows;
        //                                            }
        //                                            else
        //                                            {
        //                                                datarows = new List<datarow>();
        //                                                datarows.Add(drow);
        //                                                drow = new datarow();
        //                                                drow.type = "Project";
        //                                                drow.startdate = Convert.ToDateTime(row["Start"]);
        //                                                drow.enddate = Convert.ToDateTime(row["End"]);
        //                                                drow.title = row["Title"].ToString();
        //                                                datarows.Add(drow);
        //                                                grouptable.Add(groupname, datarows);
        //                                            }
        //                                        }
        //                                    }
        //                                    else if (groupname != string.Empty)
        //                                    {
        //                                        datarows =
        //                                            (List<datarow>)
        //                                            grouptable[groupname];
        //                                        var drow = new datarow();
        //                                        drow.title = row["Title"].ToString();
        //                                        drow.startdate =
        //                                            Convert.ToDateTime(row["Start"]);
        //                                        drow.enddate =
        //                                            Convert.ToDateTime(row["End"]);
        //                                        drow.type = "CF";
        //                                        datarows.Add(drow);
        //                                        grouptable[groupname] = datarows;
        //                                    }
        //                                }

        //                                // Adding the rows into datatable
        //                                foreach (DictionaryEntry drws in grouptable)
        //                                {
        //                                    foreach (datarow drow in (List<datarow>)drws.Value)
        //                                    {
        //                                        DataRow row = ResultDataTable.NewRow();
        //                                        row["Title"] = drow.title;
        //                                        row["Start"] = drow.startdate;
        //                                        row["Finish"] = drow.enddate;
        //                                        row["Type"] = drow.type;
        //                                        ResultDataTable.Rows.Add(row);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                ErrorLog("The Custom field called " + CustomFieldName + " is not found in the instanse " + Site.Url, EventLogEntryType.FailureAudit);
        //                            }
        //                        }
        //                    }
        //                }
        //                    );
        //            }
        //            catch (Exception ex)
        //            {
        //                ErrorLog("Error at loading project list due to " + ex.Message, EventLogEntryType.Error);
        //            }
        //            return ResultDataTable;
        //        }

        #endregion "Version 3"

        #region "Version 2"

        //public static DataTable GetGovernanceReport(SPContext context)
        //{
        //    var ResultDataTable = new DataTable();
        //    ResultDataTable.Columns.Add("Title");
        //    ResultDataTable.Columns.Add("Start");
        //    ResultDataTable.Columns.Add("Finish");
        //    ResultDataTable.Columns.Add("Type");
        //    try
        //    {
        //        string SiteUrl = context != null ? context.Site.Url : ProjectServerInstanceURL;
        //        SPListItemCollection ProjectItemCollection = null;
        //        SPSecurity.RunWithElevatedPrivileges(delegate
        //                                                 {
        //                                                     using (var Site = new SPSite(SiteUrl))
        //                                                     {
        //                                                         SPList GroupList = null;
        //                                                         try
        //                                                         {
        //                                                             GroupList = Site.RootWeb.Lists[GroupListName];
        //                                                         }
        //                                                         catch (Exception)
        //                                                         {
        //                                                         }
        //                                                         if (GroupList == null)
        //                                                         {
        //                                                             ErrorLog("The Project Group Configuration List called  " + GroupListName + " not found in the Site called " + SiteUrl, EventLogEntryType.Error);
        //                                                         }
        //                                                         else
        //                                                         {
        //                                                             var query = new SPQuery
        //                                                                             {
        //                                                                                 Query =
        //                                                                                     @"<OrderBy><FieldRef Name='" + GroupFieldName + "' /><FieldRef Name='Title' /></OrderBy>"
        //                                                                             };
        //                                                             ProjectItemCollection = GroupList.GetItems(query);
        //                                                         }

        //                                                         SiteUrl = Utilities.GetDefaultZoneUri(Site);
        //                                                     }
        //                                                 }
        //            );
        //        if (ProjectItemCollection != null)
        //        {
        //            var Project_Svc = new Project
        //                                  {
        //                                      UseDefaultCredentials = true,
        //                                      Url = (SiteUrl + "/_vti_bin/psi/project.asmx"),
        //                                      AllowAutoRedirect = true
        //                                  };
        //            var Security_Svc = new Security
        //                                   {
        //                                       Url = (SiteUrl + "/_vti_bin/psi/Security.asmx"),
        //                                       UseDefaultCredentials = true,
        //                                       AllowAutoRedirect = true
        //                                   };
        //            var Customfield_Svc = new CustomFields
        //                                      {
        //                                          Url = (SiteUrl + "/_vti_bin/psi/customfields.asmx"),
        //                                          UseDefaultCredentials = true,
        //                                          AllowAutoRedirect = true
        //                                      };

        //            var Lookuptable_Svc = new LookupTable
        //                                      {
        //                                          Url = (SiteUrl + "/_vti_bin/psi/lookuptable.asmx"),
        //                                          UseDefaultCredentials = true,
        //                                          AllowAutoRedirect = true
        //                                      };
        //            Guid PublishProjectPermGuid = Guid.Empty;
        //            Guid SaveProjectPermGuid = Guid.Empty;
        //            PermissionDataSet CategoryPermissionDS = Security_Svc.ReadCategoryPermissions();
        //            foreach (PermissionDataSet.ActionsRow Arow in CategoryPermissionDS.Actions.Rows)
        //            {
        //                if (Arow.WSEC_FEA_ACT_NAME == "Publish Project")
        //                {
        //                    PublishProjectPermGuid = Arow.WSEC_FEA_ACT_UID;
        //                }
        //                else if (Arow.WSEC_FEA_ACT_NAME == "Save Project to Project Server")
        //                {
        //                    SaveProjectPermGuid = Arow.WSEC_FEA_ACT_UID;
        //                }
        //            }

        //            if (PublishProjectPermGuid != Guid.Empty && SaveProjectPermGuid != Guid.Empty)
        //            {
        //                // Getting CustomFieldRow using filter
        //                var customFieldDataSet = new CustomFieldDataSet();
        //                var equal = Filter.FieldOperationType.Equal;
        //                var cfFilter = new Filter();
        //                cfFilter.FilterTableName = customFieldDataSet.CustomFields.TableName;
        //                cfFilter.Fields.Add(new Filter.Field(customFieldDataSet.CustomFields.TableName,
        //                                                     customFieldDataSet.CustomFields.MD_PROP_NAMEColumn.
        //                                                         ColumnName, Filter.SortOrderTypeEnum.Asc));
        //                cfFilter.Fields.Add(
        //                    new Filter.Field(customFieldDataSet.CustomFields.MD_LOOKUP_TABLE_UIDColumn.ColumnName));
        //                cfFilter.Fields.Add(
        //                    new Filter.Field(customFieldDataSet.CustomFields.MD_PROP_UIDColumn.ColumnName));
        //                cfFilter.Criteria = new Filter.FieldOperator(equal,
        //                                                             customFieldDataSet.CustomFields.MD_PROP_NAMEColumn.
        //                                                                 ColumnName, CustomFieldName);
        //                customFieldDataSet = Customfield_Svc.ReadCustomFields(cfFilter.GetXml(), false);
        //                if (customFieldDataSet.CustomFields.Rows.Count > 0)
        //                {
        //                    Guid CustomFieldGuid = customFieldDataSet.CustomFields[0].MD_PROP_UID;
        //                    if (!customFieldDataSet.CustomFields[0].IsMD_LOOKUP_TABLE_UIDNull())
        //                    {
        //                        LookupTableDataSet LookupTableDS;
        //                        var results_master = new Hashtable();
        //                        LookupTableDS = Lookuptable_Svc.ReadLookupTables("", false, 0);
        //                        if (LookupTableDS.LookupTables.Rows.Count > 0)
        //                        {
        //                            LookupTableDataSet.LookupTablesRow LookupRow = null;
        //                            foreach (
        //                                LookupTableDataSet.LookupTablesRow lookupTablesRow in
        //                                    LookupTableDS.LookupTables.Rows)
        //                            {
        //                                if (lookupTablesRow.LT_UID ==
        //                                    customFieldDataSet.CustomFields[0].MD_LOOKUP_TABLE_UID)
        //                                {
        //                                    LookupRow = lookupTablesRow;
        //                                    foreach (
        //                                        LookupTableDataSet.LookupTableTreesRow TRow in
        //                                            LookupRow.GetLookupTableTreesRows())
        //                                    {
        //                                        results_master.Add(TRow.LT_VALUE_TEXT,
        //                                                           new taskdates
        //                                                               {
        //                                                                   startdate = DateTime.MinValue,
        //                                                                   enddate = DateTime.MaxValue
        //                                                               });
        //                                    }
        //                                    break;
        //                                }
        //                            }
        //                            if (LookupRow != null)
        //                            {
        //                                string groupname = string.Empty;
        //                                foreach (SPListItem Item in ProjectItemCollection)
        //                                {
        //                                    ProjectDataSet ProjectList = Project_Svc.ReadProjectList();
        //                                    Guid ProjectGuid = Guid.Empty;
        //                                    foreach (ProjectDataSet.ProjectRow PRow in ProjectList.Project.Rows)
        //                                    {
        //                                        if (PRow.PROJ_NAME.ToUpper() == Item.Title.ToUpper())
        //                                        {
        //                                            ProjectGuid = PRow.PROJ_UID;
        //                                            break;
        //                                        }
        //                                    }
        //                                    if (ProjectGuid != Guid.Empty)
        //                                    {
        //                                        // Checking the Permission for the current user
        //                                        bool PublishFlag;
        //                                        bool SaveFlag;
        //                                        try
        //                                        {
        //                                            PublishFlag = Security_Svc.CheckUserProjectPermission(ProjectGuid,
        //                                                                                                  PublishProjectPermGuid);
        //                                            SaveFlag = Security_Svc.CheckUserProjectPermission(ProjectGuid,
        //                                                                                               SaveProjectPermGuid);
        //                                        }
        //                                        catch (Exception)
        //                                        {
        //                                            PublishFlag = false;
        //                                            SaveFlag = false;
        //                                        }
        //                                        if (PublishFlag && SaveFlag)
        //                                        {
        //                                            var results = (Hashtable)results_master.Clone();
        //                                            bool IsProjectLevelResultAdded = false;
        //                                            ProjectDataSet ProjectDataDS =
        //                                                Project_Svc.ReadProjectEntities(ProjectGuid, 66,
        //                                                                                DataStoreEnum.WorkingStore);
        //                                            DataRow newrow;
        //                                            foreach (
        //                                                ProjectDataSet.TaskCustomFieldsRow TCFRow in
        //                                                    ProjectDataDS.TaskCustomFields.Rows)
        //                                            {
        //                                                if (CustomFieldGuid == TCFRow.MD_PROP_UID)
        //                                                {
        //                                                    foreach (
        //                                                        LookupTableDataSet.LookupTableTreesRow TRow in
        //                                                            LookupRow.GetLookupTableTreesRows())
        //                                                    {
        //                                                        if (TCFRow.IsCODE_VALUENull() == false &&
        //                                                            TRow.LT_STRUCT_UID == TCFRow.CODE_VALUE)
        //                                                        {
        //                                                            ProjectDataSet.TaskRow taskRow =
        //                                                                ProjectDataDS.Task.FindByTASK_UIDPROJ_UID(
        //                                                                    TCFRow.TASK_UID, ProjectGuid);
        //                                                            if (groupname != Item[GroupFieldName].ToString())
        //                                                            {
        //                                                                newrow = ResultDataTable.NewRow();
        //                                                                newrow["Title"] =
        //                                                                    Item[GroupFieldName].ToString();
        //                                                                newrow["Start"] = DateTime.MinValue;
        //                                                                newrow["Finish"] = DateTime.MaxValue;
        //                                                                newrow["Type"] = "Group";
        //                                                                ResultDataTable.Rows.Add(newrow);
        //                                                                groupname = Item[GroupFieldName].ToString();
        //                                                            }
        //                                                            if (!IsProjectLevelResultAdded)
        //                                                            {
        //                                                                newrow = ResultDataTable.NewRow();
        //                                                                newrow["Title"] = Item.Title;
        //                                                                newrow["Start"] =
        //                                                                    ProjectDataDS.Task[0].TASK_START_DATE;
        //                                                                newrow["Finish"] =
        //                                                                    ProjectDataDS.Task[0].TASK_FINISH_DATE;
        //                                                                newrow["Type"] = "Project";
        //                                                                ResultDataTable.Rows.Add(newrow);
        //                                                                IsProjectLevelResultAdded = true;
        //                                                            }
        //                                                            if (results.ContainsKey(TRow.LT_VALUE_TEXT))
        //                                                            {
        //                                                                var taskdate =
        //                                                                    (taskdates)results[TRow.LT_VALUE_TEXT];
        //                                                                if (
        //                                                                    ((taskdates)results[TRow.LT_VALUE_TEXT])
        //                                                                        .
        //                                                                        startdate == DateTime.MinValue ||
        //                                                                    DateTime.Compare(
        //                                                                        ((taskdates)
        //                                                                         results[TRow.LT_VALUE_TEXT])
        //                                                                            .startdate,
        //                                                                        taskRow.TASK_START_DATE) >
        //                                                                    0)
        //                                                                {
        //                                                                    taskdate.startdate =
        //                                                                        taskRow.TASK_START_DATE;
        //                                                                }
        //                                                                if (
        //                                                                    ((taskdates)results[TRow.LT_VALUE_TEXT])
        //                                                                        .
        //                                                                        enddate == DateTime.MaxValue ||
        //                                                                    DateTime.Compare(
        //                                                                        ((taskdates)
        //                                                                         results[TRow.LT_VALUE_TEXT])
        //                                                                            .enddate,
        //                                                                        taskRow.TASK_FINISH_DATE) <
        //                                                                    0)
        //                                                                {
        //                                                                    taskdate.enddate =
        //                                                                        taskRow.TASK_FINISH_DATE;
        //                                                                }
        //                                                                results[TRow.LT_VALUE_TEXT] = taskdate;
        //                                                            }
        //                                                            break;
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                            if (IsProjectLevelResultAdded)
        //                                            {
        //                                                foreach (DictionaryEntry entry in results)
        //                                                {
        //                                                    var taskdate = (taskdates)entry.Value;
        //                                                    if (taskdate.startdate != DateTime.MinValue &&
        //                                                        taskdate.enddate != DateTime.MaxValue)
        //                                                    {
        //                                                        newrow = ResultDataTable.NewRow();
        //                                                        newrow["Title"] = entry.Key;
        //                                                        newrow["Start"] = taskdate.startdate;
        //                                                        newrow["Finish"] = taskdate.enddate;
        //                                                        newrow["Type"] = "CF";
        //                                                        ResultDataTable.Rows.Add(newrow);
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        ErrorLog(
        //                                            "Invalid project name called " + Item.Title +
        //                                            " entered in configuration list called '" + GroupListName +
        //                                            "' in the instance '" + SiteUrl + "'",
        //                                            EventLogEntryType.FailureAudit);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                ErrorLog(
        //                                    "The Lookup table is not configured for the custom field '" +
        //                                    CustomFieldName + "' in the instance '" + SiteUrl + "'",
        //                                    EventLogEntryType.FailureAudit);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        ErrorLog(
        //                            "The Lookup table is not configured for the custom field '" + CustomFieldName +
        //                            "' in the instance '" + SiteUrl + "'", EventLogEntryType.FailureAudit);
        //                    }
        //                }
        //                else
        //                {
        //                    ErrorLog(
        //                        "The custom field called '" + CustomFieldName + "' was not found in the instance '" +
        //                        SiteUrl + "'", EventLogEntryType.FailureAudit);
        //                }
        //            }
        //        }
        //        return ResultDataTable;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog("Error at loading project list due to " + ex.Message, EventLogEntryType.Error);
        //    }
        //    return null;
        //}

        #endregion "Version 2"

        #region "Version 1"

        //public static DataTable GetGovernanceReport(SPContext context)
        //{
        //    var ResultDataTable = new DataTable();
        //    ResultDataTable.Columns.Add("Title");
        //    ResultDataTable.Columns.Add("Start");
        //    ResultDataTable.Columns.Add("Finish");
        //    ResultDataTable.Columns.Add("Type");
        //    try
        //    {
        //        string SiteUrl = context != null ? context.Site.Url : ProjectServerInstanceURL;
        //        SPSecurity.RunWithElevatedPrivileges(delegate
        //                                                 {
        //                                                     using (var site = new SPSite(SiteUrl))
        //                                                     {
        //                                                         SiteUrl = Utilities.GetDefaultZoneUri(site);
        //                                                     }
        //                                                 }
        //            );
        //        var Project_Svc = new Project
        //        {
        //            UseDefaultCredentials = true,
        //            Url = (SiteUrl + "/_vti_bin/psi/project.asmx"),
        //            AllowAutoRedirect = true
        //        };
        //        var Security_Svc = new Security
        //        {
        //            Url = (SiteUrl + "/_vti_bin/psi/Security.asmx"),
        //            UseDefaultCredentials = true,
        //            AllowAutoRedirect = true
        //        };
        //        var Customfield_Svc = new CustomFields
        //        {
        //            Url = (SiteUrl + "/_vti_bin/psi/customfields.asmx"),
        //            UseDefaultCredentials = true,
        //            AllowAutoRedirect = true
        //        };

        //        var Lookuptable_Svc = new LookupTable
        //        {
        //            Url = (SiteUrl + "/_vti_bin/psi/lookuptable.asmx"),
        //            UseDefaultCredentials = true,
        //            AllowAutoRedirect = true
        //        };
        //        Guid PublishProjectPermGuid = Guid.Empty;
        //        Guid SaveProjectPermGuid = Guid.Empty;
        //        PermissionDataSet CategoryPermissionDS = Security_Svc.ReadCategoryPermissions();
        //        foreach (PermissionDataSet.ActionsRow Arow in CategoryPermissionDS.Actions.Rows)
        //        {
        //            if (Arow.WSEC_FEA_ACT_NAME == "Publish Project")
        //            {
        //                PublishProjectPermGuid = Arow.WSEC_FEA_ACT_UID;
        //            }
        //            else if (Arow.WSEC_FEA_ACT_NAME == "Save Project to Project Server")
        //            {
        //                SaveProjectPermGuid = Arow.WSEC_FEA_ACT_UID;
        //            }
        //        }

        //        if (PublishProjectPermGuid != Guid.Empty && SaveProjectPermGuid != Guid.Empty)
        //        {
        //            // Getting CustomFieldRow using filter
        //            var customFieldDataSet = new CustomFieldDataSet();
        //            var equal = Filter.FieldOperationType.Equal;
        //            var cfFilter = new Filter();
        //            cfFilter.FilterTableName = customFieldDataSet.CustomFields.TableName;
        //            cfFilter.Fields.Add(new Filter.Field(customFieldDataSet.CustomFields.TableName, customFieldDataSet.CustomFields.MD_PROP_NAMEColumn.ColumnName, Filter.SortOrderTypeEnum.Asc));
        //            cfFilter.Fields.Add(new Filter.Field(customFieldDataSet.CustomFields.MD_LOOKUP_TABLE_UIDColumn.ColumnName));
        //            cfFilter.Fields.Add(new Filter.Field(customFieldDataSet.CustomFields.MD_PROP_UIDColumn.ColumnName));
        //            cfFilter.Criteria = new Filter.FieldOperator(equal, customFieldDataSet.CustomFields.MD_PROP_NAMEColumn.ColumnName, CustomFieldName);
        //            customFieldDataSet = Customfield_Svc.ReadCustomFields(cfFilter.GetXml(), false);
        //            if (customFieldDataSet.CustomFields.Rows.Count > 0)
        //            {
        //                Guid CustomFieldGuid = customFieldDataSet.CustomFields[0].MD_PROP_UID;
        //                if (!customFieldDataSet.CustomFields[0].IsMD_LOOKUP_TABLE_UIDNull())
        //                {
        //                    LookupTableDataSet LookupTableDS;
        //                    var results_master = new Hashtable();
        //                    LookupTableDS = Lookuptable_Svc.ReadLookupTables("", false, 0);
        //                    if (LookupTableDS.LookupTables.Rows.Count > 0)
        //                    {
        //                        LookupTableDataSet.LookupTablesRow LookupRow = null;
        //                        foreach (LookupTableDataSet.LookupTablesRow lookupTablesRow in LookupTableDS.LookupTables.Rows)
        //                        {
        //                            if (lookupTablesRow.LT_UID == customFieldDataSet.CustomFields[0].MD_LOOKUP_TABLE_UID)
        //                            {
        //                                LookupRow = lookupTablesRow;
        //                                foreach (LookupTableDataSet.LookupTableTreesRow TRow in LookupRow.GetLookupTableTreesRows())
        //                                {
        //                                    results_master.Add(TRow.LT_VALUE_TEXT, new taskdates { startdate = DateTime.MinValue, enddate = DateTime.MaxValue });
        //                                }
        //                                break;
        //                            }
        //                        }
        //                        var ProjectDatasets = new List<ProjectDataSet>();
        //                        if (LookupRow != null)
        //                        {
        //                            ProjectDatasets.Add(Project_Svc.ReadProjectStatus(Guid.Empty, DataStoreEnum.WorkingStore, string.Empty, 6));
        //                            ProjectDatasets.Add(Project_Svc.ReadProjectStatus(Guid.Empty, DataStoreEnum.WorkingStore, string.Empty, 5));
        //                            ProjectDatasets.Add(Project_Svc.ReadProjectStatus(Guid.Empty, DataStoreEnum.WorkingStore, string.Empty, 0));
        //                            foreach (ProjectDataSet ProjDS in ProjectDatasets)
        //                            {
        //                                foreach (ProjectDataSet.ProjectRow PRow in ProjDS.Project.Rows)
        //                                {
        //                                    // checking the Checked out status
        //                                    if (PRow.IsPROJ_CHECKOUTBYNull())
        //                                    {
        //                                        // Checking the Permission for the current user
        //                                        bool PublishFlag;
        //                                        bool SaveFlag;
        //                                        try
        //                                        {
        //                                            PublishFlag = Security_Svc.CheckUserProjectPermission(PRow.PROJ_UID,
        //                                                                                                  PublishProjectPermGuid);
        //                                            SaveFlag = Security_Svc.CheckUserProjectPermission(PRow.PROJ_UID,
        //                                                                                               SaveProjectPermGuid);
        //                                        }
        //                                        catch (Exception)
        //                                        {
        //                                            PublishFlag = false;
        //                                            SaveFlag = false;
        //                                        }
        //                                        if (PublishFlag && SaveFlag)
        //                                        {
        //                                            var results = (Hashtable)results_master.Clone();
        //                                            bool IsProjectLevelResultAdded = false;
        //                                            ProjectDataSet ProjectDataDS = Project_Svc.ReadProjectEntities(PRow.PROJ_UID, 66, DataStoreEnum.WorkingStore);
        //                                            DataRow newrow;
        //                                            foreach (ProjectDataSet.TaskCustomFieldsRow TCFRow in ProjectDataDS.TaskCustomFields.Rows)
        //                                            {
        //                                                if (CustomFieldGuid == TCFRow.MD_PROP_UID)
        //                                                {
        //                                                    foreach (LookupTableDataSet.LookupTableTreesRow TRow in LookupRow.GetLookupTableTreesRows())
        //                                                    {
        //                                                        if (TCFRow.IsCODE_VALUENull() == false && TRow.LT_STRUCT_UID == TCFRow.CODE_VALUE)
        //                                                        {
        //                                                            ProjectDataSet.TaskRow taskRow = ProjectDataDS.Task.FindByTASK_UIDPROJ_UID(TCFRow.TASK_UID, PRow.PROJ_UID);
        //                                                            if (!IsProjectLevelResultAdded)
        //                                                            {
        //                                                                newrow = ResultDataTable.NewRow();
        //                                                                newrow["Title"] = PRow.PROJ_NAME;
        //                                                                newrow["Start"] = ProjectDataDS.Task[0].TASK_START_DATE;
        //                                                                newrow["Finish"] = ProjectDataDS.Task[0].TASK_FINISH_DATE;
        //                                                                newrow["Type"] = "Project";
        //                                                                ResultDataTable.Rows.Add(newrow);
        //                                                                IsProjectLevelResultAdded = true;
        //                                                            }
        //                                                            if (results.ContainsKey(TRow.LT_VALUE_TEXT))
        //                                                            {
        //                                                                var taskdate = (taskdates)results[TRow.LT_VALUE_TEXT];
        //                                                                if (((taskdates)results[TRow.LT_VALUE_TEXT]).startdate == DateTime.MinValue || DateTime.Compare(((taskdates)results[TRow.LT_VALUE_TEXT]).startdate, taskRow.TASK_START_DATE) > 0)
        //                                                                {
        //                                                                    taskdate.startdate = taskRow.TASK_START_DATE;
        //                                                                }
        //                                                                if (((taskdates)results[TRow.LT_VALUE_TEXT]).enddate == DateTime.MaxValue || DateTime.Compare(((taskdates)results[TRow.LT_VALUE_TEXT]).enddate, taskRow.TASK_FINISH_DATE) < 0)
        //                                                                {
        //                                                                    taskdate.enddate = taskRow.TASK_FINISH_DATE;
        //                                                                }
        //                                                                results[TRow.LT_VALUE_TEXT] = taskdate;
        //                                                            }
        //                                                            break;
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                            if (IsProjectLevelResultAdded)
        //                                            {
        //                                                foreach (DictionaryEntry entry in results)
        //                                                {
        //                                                    var taskdate = (taskdates)entry.Value;
        //                                                    if (taskdate.startdate != DateTime.MinValue &&
        //                                                        taskdate.enddate != DateTime.MaxValue)
        //                                                    {
        //                                                        newrow = ResultDataTable.NewRow();
        //                                                        newrow["Title"] = entry.Key;
        //                                                        newrow["Start"] = taskdate.startdate;
        //                                                        newrow["Finish"] = taskdate.enddate;
        //                                                        newrow["Type"] = "CF";
        //                                                        ResultDataTable.Rows.Add(newrow);
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ErrorLog("The Lookup table is not configured for the custom field '" + CustomFieldName + "' in the instance '" + SiteUrl + "'", EventLogEntryType.FailureAudit);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    ErrorLog("The Lookup table is not configured for the custom field '" + CustomFieldName + "' in the instance '" + SiteUrl + "'", EventLogEntryType.FailureAudit);
        //                }
        //            }
        //            else
        //            {
        //                ErrorLog("The custom field called '" + CustomFieldName + "' was not found in the instance '" + SiteUrl + "'", EventLogEntryType.FailureAudit);
        //            }
        //        }
        //        return ResultDataTable;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog("Error at loading project list due to " + ex.Message, EventLogEntryType.Error);
        //    }
        //    return null;
        //}

        #endregion "Version 1"

        public static DataTable GetProjectTasks(SPContext context, Guid ProjectUID)
        {
            var ResultTaskTable = new DataTable();
            try
            {
                string SiteUrl = ProjectServerInstanceURL(context);
                SPSecurity.RunWithElevatedPrivileges(delegate
                                                         {
                                                             using (var site = new SPSite(SiteUrl))
                                                             {
                                                                 SiteUrl = Utilities.GetDefaultZoneUri(site);
                                                             }
                                                         }
                    );
                var Project_Svc = new Project
                {
                    UseDefaultCredentials =
                        true,
                    Url = (SiteUrl +
                           "/_vti_bin/psi/project.asmx"),
                    AllowAutoRedirect = true
                };
                var projectDataSet = Project_Svc.ReadProjectEntities(ProjectUID, 2, DataStoreEnum.WorkingStore);
                ResultTaskTable.Columns.Add("ID");
                ResultTaskTable.Columns.Add("Task");
                ResultTaskTable.Columns.Add("UID");
                ResultTaskTable.Columns.Add("start date");
                ResultTaskTable.Columns.Add("end date");
                foreach (ProjectDataSet.TaskRow taskRow in projectDataSet.Task.Rows)
                {
                    DataRow NewRow = ResultTaskTable.NewRow();
                    NewRow["ID"] = taskRow.TASK_ID;
                    NewRow["Task"] = taskRow.TASK_NAME;
                    NewRow["UID"] = taskRow.TASK_UID;
                    if (!taskRow.IsTASK_START_DATENull())
                        NewRow["start date"] = taskRow.TASK_START_DATE.ToString("dd/MM/yy");
                    else
                        NewRow["start date"] = " -- ";
                    if (!taskRow.IsTASK_FINISH_DATENull())
                        NewRow["end date"] = taskRow.TASK_FINISH_DATE.ToString("dd/MM/yy");
                    else
                        NewRow["end date"] = " -- ";
                    ResultTaskTable.Rows.Add(NewRow);
                }
            }
            catch (Exception ex)
            {
                ErrorLog("Error at getting project tasks due to " + ex.Message, EventLogEntryType.Error);
            }
            return ResultTaskTable;
        }

        public static string GetDataBaseConnectionString(string SiteURL)
        {
            return Utilities.GetProjectServerSQLDatabaseConnectionString(SiteURL, Utilities.DatabaseType.ReportingDatabase);
        }

        ////struct taskdates
        //{
        //    public DateTime startdate;
        //    public DateTime enddate;
        //}

        struct datarow
        {
            public string title;
            public DateTime startdate;
            public DateTime enddate;
            public string type;
        }

        public static void ModifyConnectionString(string SiteURL)
        {
            // open access dynamic databse configuration
            string connstr = Utilities.GetProjectServerSQLDatabaseConnectionString(SiteURL, Utilities.DatabaseType.PublishedDatabase);
            var builder = new SqlConnectionStringBuilder(connstr);
            ObjectScopeProvider1.AdjustForDynamicLoad(GovernanceReportGroupConfigDBname, builder.DataSource);
        }

        public static string Serialize(object value)
        {
            Type type = value.GetType();
            var json = new JsonSerializer();
            json.NullValueHandling = NullValueHandling.Ignore;
            json.ObjectCreationHandling = ObjectCreationHandling.Replace;
            json.MissingMemberHandling = MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            if (type == typeof(DataTable))
                json.Converters.Add(new DataTableConverter());
            else if (type == typeof(DataSet))
                json.Converters.Add(new DataSetConverter());
            var sw = new StringWriter();
            var writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.Indented;
            writer.QuoteChar = '"';
            json.Serialize(writer, value);
            string output = sw.ToString();
            writer.Close();
            sw.Close();
            return output;
        }
    }
}