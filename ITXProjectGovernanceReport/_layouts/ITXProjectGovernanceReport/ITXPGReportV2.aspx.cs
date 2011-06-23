using System;
using System.Diagnostics;
using ITXProjectsLibrary.WebSvcResource;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public partial class ITXPGReportV2 : System.Web.UI.Page
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
                    var Resource_Svc = new Resource()
                                           {
                                               AllowAutoRedirect = true,
                                               Url = siteurl + "/_vti_bin/psi/resource.asmx",
                                               UseDefaultCredentials = true
                                           };
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
    }
}