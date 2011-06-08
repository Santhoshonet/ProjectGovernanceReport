using System;
using System.Data;
using System.Diagnostics;
using Microsoft.SharePoint;

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public partial class ProjectGroupConfigure : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string SiteUrl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);

                // For Group Repeater control

                var GroupTable = new DataTable();
                GroupTable.Columns.Add("title");
                GroupTable.Columns.Add("grpid");

                // sample row
                var row = GroupTable.NewRow();
                row["title"] = "Group1";
                row["grpid"] = Guid.NewGuid();
                GroupTable.Rows.Add(row);

                RptrGroupnames.DataSource = GroupTable;
                RptrGroupnames.DataBind();

                // For Project name Repeater Control
                var ProjectTable = new DataTable();
                ProjectTable.Columns.Add("title");
                ProjectTable.Columns.Add("projid");

                // sample row
                row = ProjectTable.NewRow();
                row["title"] = "Project1";
                row["projid"] = Guid.NewGuid();
                ProjectTable.Rows.Add(row);

                RptrProjectnames.DataSource = ProjectTable;
                RptrProjectnames.DataBind();
            }
            catch (Exception ex)
            {
                MyUtilities.ErrorLog("Error at Project Group Congure load due to " + ex.Message, EventLogEntryType.Error);
            }
        }
    }
}