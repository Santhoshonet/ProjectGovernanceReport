using Telerik.OpenAccess;

namespace ITXPGReportDataLayer
{
    [Persistent]
    public class Projects
    {
        public string uid
        {
            get;
            set;
        }

        public string name { get; set; }
    }
}