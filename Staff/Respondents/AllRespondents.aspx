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
            <asp:GridView  AutoGenerateSelectButton="true" ID="registeredRespondentsGridView" runat="server"  OnSelectedIndexChanged="registeredRespondentsGridView_SelectedIndexChanged"></asp:GridView>

            <hr />
            <br />
            <h2>Unregistered Users</h2>
            <asp:GridView AutoGenerateSelectButton="true" ID="unregisteredRespondentsGridView" runat="server" OnSelectedIndexChanged="unregisteredRespondentsGridView_SelectedIndexChanged"></asp:GridView>


            <hr />
               <!--when the user wants to see the details of a specific respondent, they can click on the respondent's row-->  
            <br />
            <asp:label ID="respondentInfoLabel" runat="server" style="font-weight: 700"></asp:label>
            <br />
            <asp:GridView ID="respondentDetailsGridView" runat="server"></asp:GridView>
                

        </div>
    </form>
</body>
</html>