using System;
using System.Diagnostics;
using System.Security.Principal;
using ITXProjectsLibrary.WebSvcProject;
using Microsoft.SharePoint;

namespace ITXPGReportUtilities
{
    public class MyUtilities
    {
        // Configuration List name
        public static string GroupListName = "ProjectType";

        // Configuration group field name
        public static string GroupFieldName = "Group";

        // Configuration project uid field name
        public static string ProjectUIDFieldName = "ProjectUID";

        // To put the default group name into the list
        public static string DefaultGroupValue = "Not Configured.";

        public static string SPFeatureName = "ITX Project Governance Report";

        public static void ErrorLog(string LogStr, EventLogEntryType Type)
        {
            try
            {
                System.Security.Principal.WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero);
                var El = new EventLog();
                if (EventLog.SourceExists("ITXProjectGovernanceReport") == false)
                    EventLog.CreateEventSource("ITXProjectGovernanceReport", "ITXProjectGovernanceReport");
                El.Source = "ITXProjectGovernanceReport";
                El.WriteEntry(LogStr, Type);
                El.Close();
                wic.Undo();
            }
            catch (Exception Ex87)
            {
                WriteTextLog(Ex87.Message + "\r" + LogStr);
            }
        }

        public static void WriteTextLog(string LogStr)
        {
            try
            {
                var Writer = new System.IO.StreamWriter(@"c:\ITXProjectGovernanceReport.txt", true);
                Writer.WriteLine(LogStr);
                Writer.Close();
                Writer.Dispose();
            }
            catch
            {
                return;
            }
        }

        public static Guid CreateConfigurationList(SPSite Site)
        {
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
                ErrorLog("Error at creating the configuration list due to " + ex.Message, EventLogEntryType.Error);
            }
            return ListUID;
        }

        public static SPList GetConfigurationList(SPSite Site)
        {
            SPList configurationList = null;
            try
            {
                try
                {
                    configurationList = Site.RootWeb.Lists[GroupListName];
                }
                catch (Exception)
                {
                }
                if (configurationList == null)
                {
                    var ListUID = CreateConfigurationList(Site);
                    if (ListUID != Guid.Empty)
                    {
                        configurationList = Site.RootWeb.Lists[ListUID];
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog("error at creating configuration list due to " + ex.Message, EventLogEntryType.Error);
            }
            return configurationList;
        }

        public static void CheckandAddEntry(SPList configurationList, Guid ProjectGuid, Project Project_Svc)
        {
            try
            {
                // Checking whether the project id is already available.
                var query = new SPQuery
                {
                    Query =
                        @"<Where><Eq><FieldRef Name='" + ProjectUIDFieldName + "' /><Value Type='Text'>" + ProjectGuid.ToString() + "</Value></Eq></Where>"
                };
                var ProjectItemCollection = configurationList.GetItems(query);
                if (ProjectItemCollection.Count == 0)
                {
                    configurationList.ParentWeb.Site.RootWeb.AllowUnsafeUpdates = true;
                    SPListItem item = configurationList.Items.Add();
                    //first project name
                    item["Title"] = Project_Svc.GetProjectNameFromProjectUid(ProjectGuid, DataStoreEnum.WorkingStore);
                    item[GroupFieldName] = DefaultGroupValue;
                    item[ProjectUIDFieldName] = ProjectGuid.ToString();
                    item.Update();
                    configurationList.ParentWeb.Site.RootWeb.Update();
                }
            }
            catch (Exception ex)
            {
                ErrorLog("Error at adding configuration item to the list due to " + ex.Message, EventLogEntryType.Error);
            }
        }
    }
}