using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using Microsoft.SharePoint;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public partial class ITXProjectGovernanceReport : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string siteurl = SPContext.Current != null
                                     ? SPContext.Current.Site.Url
                                     : MyUtilities.ProjectServerInstanceURL;
                var result = MyUtilities.GetGovernanceReport(siteurl);
                JSONData.Text = Serialize(result);
            }
            catch (Exception ex)
            {
                MyUtilities.ErrorLog(ex.Message, EventLogEntryType.Error);
            }
        }

        public string Serialize(object value)
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