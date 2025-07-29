<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SurveyComplete.aspx.cs" Inherits="AITR_Survey.SurveyApp.SurveyComplete" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Survey Complete</title>
    <style>
        .thankyou-container {
            margin: 100px auto;
            width: 400px;
            text-align: center;
            font-family: Arial, sans-serif;
        }
        .thankyou-message {
            font-size: 1.5em;
            margin-bottom: 30px;
            color: #333;
        }
        .back-button {
            padding: 10px 30px;
            font-size: 1em;
            background-color: #0078d7;
            color: #fff;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }
        .back-button:hover {
            background-color: #005fa3;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="thankyou-container">
            <div class="thankyou-message">
                Thank you for completing the survey!
            </div>
            <asp:Button ID="btnBackToHome" runat="server" Text="Back to Home" CssClass="back-button" OnClick="btnBackToHome_Click" />
        </div>
    </form>
</body>
</html>
