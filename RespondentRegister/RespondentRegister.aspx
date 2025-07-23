<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RespondentRegister.aspx.cs" Inherits="AITR_Survey.RespondentRegister" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>registerPage</title>
    <link href="../RespondentRegister.css" rel="stylesheet" />
</head>
<body>
    <form id="registerForm" runat="server">
        <div class="formContainer">
            <div class="greetingDiv">
                <asp:Label ID="greetingLabel" runat="server" Text="AITR SURVEY"></asp:Label>
            </div>
            <div class="inputContainer">
                <asp:Label ID="firstNameLabel" runat="server" Text="FirstName"></asp:Label>
                <asp:TextBox class="inputTextBox" ID="firstNameTextBox" runat="server"></asp:TextBox>
            </div>
            <br />
            <div class="inputContainer">
                <asp:Label ID="lastNamelabel" runat="server" Text="LastName"></asp:Label>
                <asp:TextBox class="inputTextBox" ID="lastNameTextBox" runat="server"></asp:TextBox>
            </div>
            <br />
            <div class="inputContainer">
                <asp:Label ID="contactNumberLabel" runat="server" Text="Contact Number"></asp:Label>
                <asp:TextBox class="inputTextBox" ID="ContactNumberTextBox" runat="server"></asp:TextBox>
            </div>
            <br />
            <div class="inputContainer">
                <asp:Label ID="DOBLabel" runat="server" Text="Date of Birth"></asp:Label>
                <input type="date" class="inputTextBox" id="DOBDatePicker" name="birthday" />
            </div>
            <br />

            <asp:Button class="submitButton" ID="submitButton" runat="server" OnClick="Submit_Button_Click" Text="Submit" />
        </div>
    </form>
</body>
</html>

