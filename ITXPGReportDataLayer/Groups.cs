using System.Collections.Generic;
using Telerik.OpenAccess;

namespace ITXPGReportDataLayer
{
    [Persistent]
    public class Groups
    {
        public Groups()
        {
            projects = new List<Projects>();
        }

        public string UID { get; set; }

        public string name { get; set; }

        public IList<Projects> projects { get; set; }
    }
}