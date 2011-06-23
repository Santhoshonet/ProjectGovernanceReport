using System;
using System.Data.SqlClient;
using ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport;
using ITXProjectsLibrary;
using Microsoft.SharePoint;
using Telerik.OpenAccess;

namespace ITXProjectGovernanceReport
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //return;
            string SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (var Site = new SPSite(SiteUrl))
                {
                    string connstr = Utilities.GetProjectServerSQLDatabaseConnectionString(Site.ID, Utilities.DatabaseType.PublishedDatabase);
                    var builder = new SqlConnectionStringBuilder(connstr);
                    ITXPGReportDataLayer.ObjectScopeProvider1.AdjustForDynamicLoad("ITXPGReportDataLayer", builder.DataSource);
                    using (IObjectScope scope = ITXPGReportDataLayer.ObjectScopeProvider1.GetNewObjectScope())
                    {
                    }
                }
            }
        );
        }
    }
}