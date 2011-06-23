using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Services;
using ITXPGReportDataLayer;
using ITXProjectsLibrary.WebSvcResource;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Telerik.OpenAccess;

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public partial class ITXPGReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Browser detection and redirecting to error page
            if (Request.Browser.Browser.ToLower() == "ie" && Convert.ToDouble(Request.Browser.Version) < 7)
            {
                SPUtility.TransferToErrorPage("To view this report use later versions of IE 6.0");
            }
            else
            {
                try
                {
                    string siteurl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);
                    var Resource_Svc = new Resource
                                           {
                                               AllowAutoRedirect = true,
                                               Url = siteurl + "/_vti_bin/psi/resource.asmx",
                                               UseDefaultCredentials = true
                                           };
                    if (MyUtilities.IndividualPages)
                        LnkConfigButton.PostBackUrl = siteurl +
                                                      "/_layouts/ITXProjectGovernanceReport/ProjectGroupConfig.aspx";
                    else
                        LnkConfigButton.Visible = false;
                    var result = MyUtilities.GetGovernanceReport(siteurl, Resource_Svc.GetCurrentUserUid());
                    //Repeater1.DataSource = result;
                    //Repeater1.DataBind();
                    JSONData.Text = result.Rows.Count > 0 ? MyUtilities.Serialize(result) : "";
                }
                catch (Exception ex)
                {
                    MyUtilities.ErrorLog(ex.Message, EventLogEntryType.Error);
                }
            }
        }

        [WebMethod]
        public static string AddGroup(string gname, string currentuseruid)
        {
            string Output = string.Empty;
            try
            {
                // open access dynamic databse configuration
                string SiteUrl = HttpContext.Current.Request.UrlReferrer.Scheme + "://" +
                                 HttpContext.Current.Request.UrlReferrer.Host + ":" +
                                 HttpContext.Current.Request.UrlReferrer.Port + "/" +
                                 HttpContext.Current.Request.UrlReferrer.Segments[1];
                if (MyUtilities.DevelopMode)
                    SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);

                MyUtilities.ModifyConnectionString(SiteUrl);

                using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                {
                    List<Groups> groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                           from d in c.groups
                                           where c.ResourceUID.Equals(currentuseruid) && d.name.Equals(gname)
                                           select d).ToList();
                    if (groups.Count == 0)
                    {
                        List<Users> userses = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                               where c.ResourceUID.Equals(currentuseruid)
                                               select c).ToList();
                        if (userses.Count > 0)
                        {
                            scope.Transaction.Begin();
                            var new_group = new Groups();
                            new_group.name = gname;
                            new_group.UID = Guid.NewGuid().ToString();
                            userses[0].groups.Add(new_group);
                            scope.Add(userses[0]);
                            scope.Transaction.Commit();
                            Output = new_group.UID;
                        }
                    }
                    else
                    {
                        Output = groups[0].UID;
                    }
                }
            }
            catch (Exception)
            {
                // Error log here
            }
            return Output;
        }

        [WebMethod]
        public static bool PlaceProject(string groupuid, string projectuid, string currentuseruid)
        {
            bool Output = false;
            try
            {
                // open access dynamic databse configuration
                string SiteUrl = HttpContext.Current.Request.UrlReferrer.Scheme + "://" +
                                 HttpContext.Current.Request.UrlReferrer.Host + ":" +
                                 HttpContext.Current.Request.UrlReferrer.Port + "/" +
                                 HttpContext.Current.Request.UrlReferrer.Segments[1];
                if (MyUtilities.DevelopMode)
                    SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);

                MyUtilities.ModifyConnectionString(SiteUrl);

                using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                {
                    List<Projects> projects = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                               from d in c.groups
                                               from f in d.projects
                                               where c.ResourceUID.Equals(currentuseruid) && f.uid.Equals(projectuid)
                                               select f).ToList();
                    if (projects.Count > 0)
                    {
                        List<Groups> new_groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                   from d in c.groups
                                                   where c.ResourceUID.Equals(currentuseruid) && d.UID.Equals(groupuid)
                                                   select d).ToList();
                        if (new_groups.Count > 0)
                        {
                            List<Groups> old_groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                       from d in c.groups
                                                       from f in d.projects
                                                       where c.ResourceUID.Equals(currentuseruid) && f.uid.Equals(projectuid)
                                                       select d).ToList();
                            // remove from current group first
                            if (old_groups.Count > 0)
                            {
                                if (old_groups[0].projects.Contains(projects[0]))
                                {
                                    scope.Transaction.Begin();
                                    old_groups[0].projects.Remove(projects[0]);
                                    scope.Add(old_groups[0]);
                                    scope.Transaction.Commit();
                                }
                            }

                            scope.Transaction.Begin();
                            new_groups[0].projects.Add(projects[0]);
                            scope.Add(new_groups[0]);
                            scope.Transaction.Commit();
                            Output = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // error log here
            }
            return Output;
        }

        [WebMethod]
        public static string GetProjects(string groupuid, string currentuseruid)
        {
            var outputTable = new DataTable();
            outputTable.Columns.Add("uid");
            outputTable.Columns.Add("name");
            try
            {
                // open access dynamic databse configuration
                string SiteUrl = HttpContext.Current.Request.UrlReferrer.Scheme + "://" +
                                 HttpContext.Current.Request.UrlReferrer.Host + ":" +
                                 HttpContext.Current.Request.UrlReferrer.Port + "/" +
                                 HttpContext.Current.Request.UrlReferrer.Segments[1];
                if (MyUtilities.DevelopMode)
                    SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);

                MyUtilities.ModifyConnectionString(SiteUrl);

                using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                {
                    List<Groups> groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                           from d in c.groups
                                           where c.ResourceUID.Equals(currentuseruid) && d.UID.Equals(groupuid)
                                           select d).ToList();
                    if (groups.Count > 0)
                    {
                        foreach (var project in groups[0].projects)
                        {
                            var testrow = outputTable.NewRow();
                            testrow["uid"] = project.uid;
                            testrow["name"] = project.name;
                            outputTable.Rows.Add(testrow);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // error log here
            }
            return MyUtilities.Serialize(outputTable);
        }

        [WebMethod]
        public static bool ModifyGroup(object details)
        {
            // first row in details contains current userid
            bool Output = false;
            try
            {
                var top_array = (Array)details;
                if (top_array.Length > 0)
                {
                    // open access dynamic databse configuration
                    string SiteUrl = HttpContext.Current.Request.UrlReferrer.Scheme + "://" +
                                     HttpContext.Current.Request.UrlReferrer.Host + ":" +
                                     HttpContext.Current.Request.UrlReferrer.Port + "/" +
                                     HttpContext.Current.Request.UrlReferrer.Segments[1];
                    if (MyUtilities.DevelopMode)
                        SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);

                    MyUtilities.ErrorLog(SiteUrl, EventLogEntryType.Error);

                    MyUtilities.ModifyConnectionString(SiteUrl);

                    using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                    {
                        var array = (Array)top_array.GetValue(0);
                        string currentuseruid = array.GetValue(0).ToString();
                        for (var index = 1; index < top_array.Length; index++)
                        {
                            array = (Array)top_array.GetValue(index);
                            if (array.Length > 0)
                            {
                                string groupuid = array.GetValue(0).ToString();
                                string groupname = array.GetValue(1).ToString();
                                List<Groups> groupses = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                         from d in c.groups
                                                         where c.ResourceUID.Equals(currentuseruid) && d.UID.Equals(groupuid)
                                                         select d).ToList();
                                if (groupses.Count > 0)
                                {
                                    scope.Transaction.Begin();
                                    groupses[0].name = groupname;
                                    scope.Add(groupses[0]);
                                    scope.Transaction.Commit();
                                }
                            }
                        }
                    }
                }
                Output = true;
            }
            catch (Exception)
            {
                // error log goes here
            }
            return Output;
        }
    }
}