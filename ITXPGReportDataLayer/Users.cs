using System.Collections.Generic;
using Telerik.OpenAccess;

namespace ITXPGReportDataLayer
{
    [Persistent]
    public class Users
    {
        public Users()
        {
            groups = new List<Groups>();
        }

        public string ResourceUID
        {
            get;
            set;
        }

        public IList<Groups> groups { get; set; }
    }
}