<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AITR_Survey.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AITR Survey</title>
    <link rel="stylesheet" href="DefaultPage/Default.css" />
</head>
<body>
    <form id="formContainer" runat="server">
        <div>
        </div>
        <p>Welcome to AITR Research</p>
        <p>
            Thank you for participating in this survey. Click on the button below to get started.
        </p>
        <p>
            <asp:Button ID="GetStartedButton" runat="server" Height="122px" Text="Get Started" Width="442px" OnClick="GetStartedButton_Click" />
        </p>
        <p>If you are a staff, Please click on the button below to login</p>
        <asp:Button ID="StaffLoginButton" runat="server" Text="Login" OnClick="StaffLoginButton_Click" />
    </form>
</body>
</html>
