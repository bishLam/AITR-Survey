using AITR_Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AITR_Survey
{
    public class Question
    {
        private Int32 questionID;
        private String questionText;
        private String questionType; 
        private String isFirstQuestion;
        private String hasBranch;
        private String nextQuestionForTextInput;
        private Int32 maxSelection;

        public Question()
        {
        }

        public Question(int questionID, string questionText, string questionType, string isFirstQuestion, string hasBranch, string nextQuestionForTextInput, int maxSelection)
        {
            this.QuestionID = questionID;
            this.QuestionText = questionText;
            this.QuestionType = questionType;
            this.IsFirstQuestion = isFirstQuestion;
            this.HasBranch = hasBranch;
            this.NextQuestionForTextInput = nextQuestionForTextInput;
            this.MaxSelection = maxSelection;
        }

        public int QuestionID { get => questionID; set => questionID = value; }
        public string QuestionText { get => questionText; set => questionText = value; }
        public string QuestionType { get => questionType; set => questionType = value; }
        public string IsFirstQuestion { get => isFirstQuestion; set => isFirstQuestion = value; }
        public string HasBranch { get => hasBranch; set => hasBranch = value; }
        public string NextQuestionForTextInput { get => nextQuestionForTextInput; set => nextQuestionForTextInput = value; }
        public int MaxSelection { get => maxSelection; set => maxSelection = value; }
    }
}
