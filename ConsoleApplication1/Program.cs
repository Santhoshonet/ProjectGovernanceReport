using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;

namespace ConsoleApplication1
{
    class Program
    {
        // Configuration List name
        public static string GroupListName = "ProjectType2";

        // Configuration group field name
        public static string GroupFieldName = "Group";

        // Configuration project uid field name
        public static string ProjectUIDFieldName = "ProjectUID";

        // To put the default group name into the list
        public static string DefaultGroupValue = "Not Configured.";

        public static string SPFeatureName = "ITX Project Governance Report";

        static void Main(string[] args)
        {
            string url = "http://epm2007demo/pwa03";
            using (var Site = new SPSite(url))
            {
                var navigationnode = new SPNavigationNode("Project Governance Report", "/" + Site.RootWeb.ServerRelativeUrl + "/_layouts/ITXProjectGovernanceReport/ITXPGReport.aspx", true);
                Site.RootWeb.Navigation.QuickLaunch.AddAsLast(navigationnode);
                Site.RootWeb.Update();

                return;
                Guid ListUID = Guid.Empty;
                try
                {
                    string listname = GroupListName;
                    if (GroupListName != string.Empty)
                    {
                        ListUID = Site.RootWeb.Lists.Add(listname, "List for configuring Project Governance Report Groups.",
                                                         SPListTemplateType.GenericList);
                        Site.RootWeb.Update();
                        Site.RootWeb.AllowUnsafeUpdates = true;
                        SPList ConfigurationList = Site.RootWeb.Lists[listname];
                        ConfigurationList.Fields.Add(GroupFieldName, SPFieldType.Choice, true);

                        // Creating a choice field here
                        var choice_field = (SPFieldChoice)ConfigurationList.Fields[GroupFieldName];
                        choice_field.Description = "Project Group name";
                        choice_field.EditFormat = SPChoiceFormatType.Dropdown;
                        choice_field.Required = true;
                        choice_field.FillInChoice = true;
                        choice_field.Choices.Add("Unknown");
                        choice_field.Update();
                        try
                        {
                            ConfigurationList.DefaultView.ViewFields.Add(choice_field);
                        }
                        catch (Exception)
                        {
                        }
                        for (int index = 0; index < ConfigurationList.Views.Count; index++)
                        {
                            try
                            {
                                ConfigurationList.Views[index].ViewFields.Add(choice_field);
                                ConfigurationList.Views[index].Update();
                            }
                            catch (Exception)
                            {
                            }
                        }

                        // Creating a ProjectUID field
                        ConfigurationList.Fields.Add(ProjectUIDFieldName, SPFieldType.Text, true);
                        var text_field = (SPFieldText)ConfigurationList.Fields[ProjectUIDFieldName];
                        try
                        {
                            ConfigurationList.DefaultView.ViewFields.Add(text_field);
                        }
                        catch (Exception)
                        {
                        }
                        for (int index = 0; index < ConfigurationList.Views.Count; index++)
                        {
                            try
                            {
                                ConfigurationList.Views[index].ViewFields.Add(text_field);
                                ConfigurationList.Views[index].Update();
                            }
                            catch (Exception)
                            {
                            }
                        }
                        ConfigurationList.Update();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            Console.Read();
        }
    }
}