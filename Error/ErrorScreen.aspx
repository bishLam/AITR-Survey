<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorScreen.aspx.cs" Inherits="AITR_Survey.Error.ErrorScreen" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Sorry, Something unexpected happened. Please try again later. You can also view details below for information about the error</h2>
            <asp:Button ID="Button1" runat="server" Text="Go to Home" OnClick="GoHomeButton_Click" />
        </div>
    </form>
</body>
</html>
