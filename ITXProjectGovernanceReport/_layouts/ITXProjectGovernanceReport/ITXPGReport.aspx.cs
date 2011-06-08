using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Web.Services;
using Microsoft.SharePoint;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport
{
    public partial class ITXPGReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string siteurl = MyUtilities.ProjectServerInstanceURL(SPContext.Current);
                var result = MyUtilities.GetGovernanceReport(siteurl);
                //Repeater1.DataSource = result;
                //Repeater1.DataBind();
                if (result.Rows.Count > 0)
                    JSONData.Text = Serialize(result);
                else
                    JSONData.Text = "";
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

        [WebMethod]
        public static string AddGroup(string gname)
        {
            string outputstr;
            System.Threading.Thread.Sleep(1000);
            outputstr = Guid.NewGuid().ToString();
            return outputstr;
        }

        [WebMethod]
        public static bool ModifyGroup(object details)
        {
            System.Threading.Thread.Sleep(1000);
            return true;
        }

        [WebMethod]
        public static bool PlaceProject(string groupuid, string projectuid)
        {
            System.Threading.Thread.Sleep(1000);
            return true;
        }

        [WebMethod]
        public static string GetProjects(string groupuid)
        {
            var outputTable = new DataTable();
            outputTable.Columns.Add("uid");
            outputTable.Columns.Add("name");
            try
            {
                var testrow = outputTable.NewRow();
                testrow["uid"] = Guid.NewGuid();
                testrow["name"] = "Project" + DateTime.Today.ToLongTimeString();
                outputTable.Rows.Add(testrow);
            }
            catch (Exception)
            { }
            System.Threading.Thread.Sleep(1000);
            return Serialize_static(outputTable);
        }

        public static string Serialize_static(object value)
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