<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RespondentRegister.aspx.cs" Inherits="AITR_Survey.RespondentRegister" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>registerPage</title>
    <link href="~/RespondentRegister/RespondentRegister.css" rel="stylesheet" />
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
                <asp:RequiredFieldValidator Display="Dynamic" ControlToValidate="firstNameTextBox" ID="RequiredFieldValidator1" runat="server" ErrorMessage="First Name is required"></asp:RequiredFieldValidator>
            </div>
            <br />
            <div class="inputContainer">
                <asp:Label ID="lastNamelabel" runat="server" Text="LastName"></asp:Label>
                <asp:TextBox class="inputTextBox" ID="lastNameTextBox" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator Display="Dynamic" ControlToValidate="lastNameTextBox" ID="RequiredFieldValidator2" runat="server" ErrorMessage="Last Name is required"></asp:RequiredFieldValidator>
            </div>
            <br />
            <div class="inputContainer">
                <asp:Label ID="contactNumberLabel" runat="server" Text="Contact Number"></asp:Label>
                <asp:TextBox TextMode="Number"  AutoCompleteType="HomePhone" class="inputTextBox" ID="ContactNumberTextBox" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator Display="Dynamic" ControlToValidate="ContactNumberTextBox" ID="RequiredFieldValidator3" runat="server" ErrorMessage="Contact Number is required"></asp:RequiredFieldValidator>
            </div>
            <br />
            <div class="inputContainer">
                <asp:Label ID="DOBLabel" runat="server" Text="Date of Birth"></asp:Label>
                <input type="date" id="DOBDatePicker" runat="server" class="inputTextBox" />
                <asp:RequiredFieldValidator Display="Dynamic" ControlToValidate="DOBDatePicker" ID="RequiredFieldValidator4" runat="server" ErrorMessage="DOB is required"></asp:RequiredFieldValidator>
            </div>
            <br />
            <asp:PlaceHolder ID="validatorPlaceholder" runat="server"></asp:PlaceHolder>


            <asp:Button class="submitButton" ID="submitButton" runat="server" OnClick="Submit_Button_Click" Text="Submit" />
        </div>
    </form>
</body>
</html>

