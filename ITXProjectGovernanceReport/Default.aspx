<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ITXProjectGovernanceReport._Default" %>

<%@ Register Src="_layouts/ITXProjectGovernanceReport/ITXProjectGovernanceReport.ascx"
    TagName="ITXProjectGovernanceReport" TagPrefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:ITXProjectGovernanceReport ID="ITXProjectGovernanceReport1" runat="server" />
    </div>
    </form>
</body>
</html>