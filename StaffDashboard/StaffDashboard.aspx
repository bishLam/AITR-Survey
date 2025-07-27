<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StaffDashboard.aspx.cs" Inherits="AITR_Survey.StaffDashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Staff Search</title>
    <link href="../staffdashboard/StaffDashboard.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" class="formContainer" runat="server">
        <asp:Label ID="Label1" class="titleHeader" runat="server" Text="Staff Search Page"></asp:Label>
        <div class="searchContainer">
            <asp:Label ID="firstNameLabel" runat="server" Text="FirstName"></asp:Label>
            <asp:TextBox ID="firstNameTextBox" runat="server" MaxLength="200" />
        </div>
        <div class="searchContainer">
            <asp:Label ID="banksUsedSearchLabel" runat="server" Text="Banks Used"></asp:Label>
            <asp:TextBox ID="banksUsedSearchTextBox" runat="server" MaxLength="200" />
        </div>
        <div class="searchContainer">
            <asp:Label ID="banksServiceSearchLabel" runat="server" Text="Banks Service Used"></asp:Label>
            <asp:TextBox ID="banksServiceSearchTextBox" runat="server" MaxLength="200" />
        </div>
        <div class="searchContainer">
            <asp:Label ID="newpaperSearchLabel" runat="server" Text="NewsPaper Read"></asp:Label>
            <asp:TextBox ID="newpaperSearchTextBox" runat="server" MaxLength="200" />
        </div>

        <asp:Button class="submitButton" ID="LoginButton" runat="server" Text="Search" />
        

        <asp:DataList ID="answerDataList" runat="server"></asp:DataList>

    </form>
</body>
</html>
