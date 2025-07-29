using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AITR_Survey
{
    public class AppConstants
    {
        public static String _connectionString = "Data Source=SQL5110.site4now.net;Initial Catalog=db_9ab8b7_25dda13209;User Id=db_9ab8b7_25dda13209_admin;Password=w3WDUvmt;";
        public static String QuestionTypeSingleChoice = "Single Choice";
        public static String QuestionTypeMultipleChoice = "Multiple Choice";
        public static String QuestionTypeTextInput = "Text Input";
        public static String QuestionTypeDate = "Date";
        public static String QuestionTypeFinish = "Finish";

        //links to redirect users between pages
        public static String redirectToViewAllRespondents = "~/Staff/Respondents/AllRespondents.aspx";
        public static String redirectToSearchAllRespondents = "~/Staff/StaffSearch/StaffSearch.aspx";
    }
}