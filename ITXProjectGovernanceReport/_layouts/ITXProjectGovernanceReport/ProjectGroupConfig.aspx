<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProjectGroupConfig.aspx.cs"
    Inherits="ITXProjectGovernanceReport._layouts.ITXProjectGovernanceReport.ProjectGroupConfig" %>

<%@ Register Src="/_layouts/ITXProjectGovernanceReport/ProjectGroupConfigure.ascx"
    TagName="ProjectGroupConfigure" TagPrefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:ProjectGroupConfigure ID="ProjectGroupConfigure1" runat="server" />
    </div>
    </form>
</body>
</html>