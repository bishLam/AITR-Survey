using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AITR_Survey
{
    public class Option
    {
        private Int32 multipleChoiceOptionID;
        private Int32 questionID;
        private Int32 nextQuestionID;
        private String optionText;

        public Option()
        {
        }

        public Option(int multipleChoiceOptionID, int questionID, int nextQuestionID, string optionText)
        {
            this.multipleChoiceOptionID = multipleChoiceOptionID;
            this.questionID = questionID;
            this.nextQuestionID = nextQuestionID;
            this.optionText = optionText;
        }

        public int MultipleChoiceOptionID { get => multipleChoiceOptionID; set => multipleChoiceOptionID = value; }
        public int QuestionID { get => questionID; set => questionID = value; }
        public int NextQuestionID { get => nextQuestionID; set => nextQuestionID = value; }
        public string OptionText { get => optionText; set => optionText = value; }
    }
}