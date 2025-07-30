<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SurveyQuestion.aspx.cs" Inherits="AITR_Survey.SurveyQuestion" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>

        </div>
        <asp:Label ID="QuestionLabel" runat="server" Text="Question 1: This is a default Question?"></asp:Label>
        <br />
        <br />
        <!--Placeholder here to display different types of answer selection options like radio button, multiple choice. text input -->
        <asp:PlaceHolder ID="answerPlaceholder" runat="server"></asp:PlaceHolder>
        <br />
        <asp:Button ID="previousButton" runat="server" Text="Previous Question" />
        <asp:Button class="nextQuestionButton" ID="nextButton" runat="server" Text="Next Question" OnClick="nextButton_Click" />
    </form>
</body>
</html>
