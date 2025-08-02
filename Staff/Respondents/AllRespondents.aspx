<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AllRespondents.aspx.cs" Inherits="AITR_Survey.Staff.Respondents.AllRespondents" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/staff/respondents/AllRespondents.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Registered Users</h2>
            <asp:GridView ID="registeredRespondentsGridView" runat="server"></asp:GridView>

            <hr />
            <br />
            <h2>Unregistered Users</h2>
             <asp:GridView ID="unregisteredRespondentsGridView" runat="server"></asp:GridView>

        </div>
    </form>
</body>
</html>