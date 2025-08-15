<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StaffSearch.aspx.cs" Inherits="AITR_Survey.StaffDashboard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Staff Search</title>
    <link href="~/Staff/StaffSearch/StaffSearch.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" class="formContainer" runat="server">
        <asp:Label ID="Label1" class="titleHeader" runat="server" Text="Staff Search Page"></asp:Label>
        <div class="searchContainer">
            <asp:Label ID="firstNameLabel" runat="server" Text="FirstName"></asp:Label>
            <asp:TextBox ID="firstNameTextBox" runat="server" MaxLength="200" />
        </div>
        <div class="searchContainer">
            <asp:Label ID="banksUsedSearchLabel" runat="server" Text="Search by Banks Used"></asp:Label>
            <asp:CheckBoxList ID="banksUsedCheckBoxList" runat="server"></asp:CheckBoxList>
        </div>
        <div class="searchContainer">
            <asp:Label ID="banksServiceSearchLabel" runat="server" Text="Banks Service Used"></asp:Label>
           <asp:CheckBoxList ID="bankServicesUsedCheckBoxList" runat="server"></asp:CheckBoxList>
        </div>
        <div class="searchContainer">
            <asp:Label ID="newpaperSearchLabel" runat="server" Text="NewsPaper Read"></asp:Label>
            <asp:CheckBoxList ID="newspaperReadCheckBoxList" runat="server"></asp:CheckBoxList>
        </div>


        <asp:Button class="submitButton" ID="LoginButton" runat="server" Text="Search" OnClick="SearchButton_Click" />
        <br />
        <br />
        <br />
        <asp:Label ID="headerLabel" runat="server" Text=""></asp:Label>

        <asp:Label ID="searchResultHeader" runat="server" Text="Search Results:" Font-Bold="True" Font-Italic="True" Visible="False"></asp:Label>
        <asp:GridView ID="answerGridView" runat="server"></asp:GridView>
    </form>
</body>
</html>
