using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using ITXPGReportDataLayer;
using ITXProjectsLibrary.WebSvcProject;
using ITXProjectsLibrary.WebSvcResource;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Telerik.OpenAccess;

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public partial class ProjectGroupConfigure : System.Web.UI.UserControl
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
                    string SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);

                    var Resource_Svc = new Resource();
                    var Project_Svc = new Project();

                    Resource_Svc.UseDefaultCredentials = true;
                    Project_Svc.UseDefaultCredentials = true;

                    Resource_Svc.Url = SiteUrl + "/_vti_bin/psi/resource.asmx";
                    Project_Svc.Url = SiteUrl + "/_vti_bin/psi/project.asmx";

                    Resource_Svc.AllowAutoRedirect = true;
                    Project_Svc.AllowAutoRedirect = true;

                    if (MyUtilities.IndividualPages)
                        LnkConfigButton.PostBackUrl = SiteUrl + "/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx";
                    else
                        LnkConfigButton.Visible = false;

                    // setting current user uid
                    LblCurUserUId.Text = Resource_Svc.GetCurrentUserUid().ToString();

                    // For Group Repeater control

                    var GroupTable = new DataTable();
                    GroupTable.Columns.Add("title");
                    GroupTable.Columns.Add("grpid");

                    // impersonation here
                    try
                    {
                        var wik = WindowsIdentity.Impersonate(IntPtr.Zero);
                    }
                    catch (Exception)
                    {
                    }

                    MyUtilities.ModifyConnectionString(SiteUrl);

                    using (IObjectScope scope = ObjectScopeProvider1.GetNewObjectScope())
                    {
                        // creating the user account into db if not exists
                        List<Users> userses = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                               where c.ResourceUID.Equals(LblCurUserUId.Text)
                                               select c).ToList();
                        if (userses.Count == 0)
                        {
                            scope.Transaction.Begin();
                            var new_user = new Users();
                            new_user.ResourceUID = LblCurUserUId.Text;
                            scope.Add(new_user);
                            scope.Transaction.Commit();

                            userses = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                       where c.ResourceUID.Equals(LblCurUserUId.Text)
                                       select c).ToList();
                        }

                        List<Groups> groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                               from d in c.groups
                                               where
                                                   c.ResourceUID.Equals(LblCurUserUId.Text) &&
                                                   !d.UID.Equals(Guid.Empty.ToString())
                                               select d).ToList();
                        foreach (var group in groups)
                        {
                            var new_row = GroupTable.NewRow();
                            new_row["title"] = group.name;
                            new_row["grpid"] = group.UID;
                            GroupTable.Rows.Add(new_row);
                        }

                        RptrGroupnames.DataSource = GroupTable;
                        RptrGroupnames.DataBind();

                        // For Project name Repeater Control
                        var ProjectTable = MyUtilities.GetProjects_DataTable(SiteUrl, new Guid(LblCurUserUId.Text));

                        groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                  from d in c.groups
                                  where c.ResourceUID.Equals(LblCurUserUId.Text) && d.UID.Equals(Guid.Empty.ToString())
                                  select d).ToList();

                        if (groups.Count == 0)
                        {
                            if (userses.Count > 0)
                            {
                                scope.Transaction.Begin();
                                var new_group = new Groups();
                                new_group.name = "Not Grouped.";
                                new_group.UID = Guid.Empty.ToString();
                                userses[0].groups.Add(new_group);
                                scope.Add(userses[0]);
                                scope.Transaction.Commit();
                            }
                            groups = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                      from d in c.groups
                                      where
                                          c.ResourceUID.Equals(LblCurUserUId.Text) &&
                                          d.UID.Equals(Guid.Empty.ToString())
                                      select d).ToList();
                        }

                        // Checking and adding missed projects to the user
                        foreach (DataRow row in ProjectTable.Rows)
                        {
                            var count = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                         from d in c.groups
                                         from f in d.projects
                                         where
                                             c.ResourceUID.Equals(LblCurUserUId.Text) &&
                                             f.uid.Equals(row["ProjectUID"].ToString())
                                         select e).Count();
                            if (count == 0 && groups.Count > 0)
                            {
                                scope.Transaction.Begin();
                                var new_proj_row = new Projects();
                                new_proj_row.name = row["Title"].ToString();
                                new_proj_row.uid = row["ProjectUID"].ToString();
                                groups[0].projects.Add(new_proj_row);
                                scope.Add(groups[0]);
                                scope.Transaction.Commit();
                            }
                        }

                        RptrProjectnames.DataSource = (from c in scope.GetOqlQuery<Users>().ExecuteEnumerable()
                                                       from d in c.groups
                                                       from f in d.projects
                                                       where
                                                           c.ResourceUID.Equals(LblCurUserUId.Text) &&
                                                           d.UID.Equals(Guid.Empty.ToString())
                                                       select f).AsEnumerable();
                        RptrProjectnames.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    MyUtilities.ErrorLog("Error at Project Group Congure load due to " + ex.Message,
                                         EventLogEntryType.Error);
                    if (MyUtilities.DevelopMode)
                        Response.Write(ex.Message);
                }
            }
        }
    }
}