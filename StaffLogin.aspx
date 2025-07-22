<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StaffLogin.aspx.cs" Inherits="AITR_Survey.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        #form1 {
            width: 1365px;
            height: 68px;
        }
    </style>
</head>
<body>
        <form id="form1" runat="server">
        <div>
            <asp:label runat="server" ID="staffLoginLabel">Staff Login</asp:label>
            <br />
            <br />
            <asp:label runat="server" ID="greetingLevel">Welcome, please enter the following details and press "Login" to continue</asp:label>
            <br />
            <br />
            <asp:label runat="server" ID="usernameLabel">Username</asp:label>
            <br />
            <asp:TextBox ID="StaffUserNameTextBox" runat="server"></asp:TextBox>
            <br />
            <br />

            <!-- Password Field -->
            <asp:label runat="server" ID="passwordLabel">Password</asp:label>
            <br />
            <asp:TextBox ID="StaffPasswwordTextBox" runat="server"></asp:TextBox>

        </div>
            <p>
                <asp:Button ID="LoginButton" runat="server" Height="85px" Text="Log In" Width="157px" />
            </p>
        </form>
    </body>
</html>
