<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AnswersConfirmation.aspx.cs" Inherits="AITR_Survey.SurveyApp.FinishSurvey" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button class="submitButton" ID="LoginButton" runat="server" Text="Submit" OnClick="SubmitButton_Click" />
            <asp:Button class="cancelButton" ID="CancelButton" runat="server" Text="Cancel" OnClick="CancelButton_Click" />
        </div>
    </form>
</body>
</html>
