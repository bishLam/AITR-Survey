<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StaffLogin.aspx.cs" Inherits="AITR_Survey.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="../StaffLogin/StaffLogin.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div class="greetingDiv">
                <asp:Label runat="server" ID="staffLoginLabel">Staff Login</asp:Label>
            </div>
            <div class="inputContainer">
                <asp:Label runat="server" ID="usernameLabel">Username</asp:Label>
                <asp:TextBox AutoCompleteType="Email" ID="StaffUserNameTextBox" runat="server">admin</asp:TextBox>
                <asp:RequiredFieldValidator ID="requredFieldValidator" runat="server" class ="validator"
                    ErrorMessage="*required"
                    ControlToValidate="StaffUserNameTextBox"
                    Display="Dynamic" />
            </div>
            <br />
            <!-- Password Field -->
            <div class="inputContainer">
                <asp:Label runat="server" ID="passwordLabel">Password</asp:Label>
                <asp:TextBox ID="StaffPasswordTextBox" runat="server">admin</asp:TextBox>
                <asp:RequiredFieldValidator class ="validator" ID="RequiredFieldValidator1" runat="server"
                    ErrorMessage="*required"
                    ControlToValidate="StaffPasswordTextBox"
                    Display="Dynamic" />
            </div>
            <asp:Button class="submitButton" ID="LoginButton" runat="server" Text="Log In" OnClick="LoginButton_Click" />
        </div>
    </form>
</body>
</html>
