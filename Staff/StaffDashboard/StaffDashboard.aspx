<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StaffDashboard.aspx.cs" Inherits="AITR_Survey.Staff.StaffDashboard.StaffDashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dashboard</title>
    <link href="~/staff/staffdashboard/StaffDashboard.css" rel="stylesheet" />
</head>
<body>
        <form id="form" runat="server">
            <div>
                <h1>Dashboard</h1>
                <!-- Staff details-->
                <div class="staffDetailsContainer">
                    <p>You are logged in as: Bish</p>

                    <asp:Button class="logoutButton" ID="logoutButton" runat="server" Text="Logout"/>
                </div>
            </div>
            <div class="buttonsRow">
                <asp:Button ID="allRespondentsButton" runat="server" Height="100px" Text="View All Respondents" Width="300px" OnClick="allRespondentsButton_Click" />
                <asp:Button ID="searchRespondentsButton" runat="server" Height="100px" Text="Search Respondents" Width="300px" OnClick="searchRespondentsButton_Click" />
                <asp:Button ID="changeQuestionOrderButton" runat="server" Height="100px" Text="Change Ouestion Order" Width="300px" />
                <asp:Button ID="moreQuestionsButton" runat="server" Height="100px" Text="More >" Width="300px" />
            </div>
        </form>
</body>
</html>
