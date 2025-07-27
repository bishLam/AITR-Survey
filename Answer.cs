using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AITR_Survey
{
    public class Answer
    {
        private Int32 questionID;
        private String textInputAnswer;
        private Nullable<Int32> singleChoiceAnswerID;

        public Answer()
        {
        }

        public Answer(int questionID, string textInputAnswer, int? singleChoiceAnswerID)
        {
            this.questionID = questionID;
            this.textInputAnswer = textInputAnswer;
            this.singleChoiceAnswerID = singleChoiceAnswerID;
        }

        public int QuestionID { get => questionID; set => questionID = value; }
        public string TextInputAnswer { get => textInputAnswer; set => textInputAnswer = value; }
        public int? SingleChoiceAnswerID { get => singleChoiceAnswerID; set => singleChoiceAnswerID = value; }
    }
}