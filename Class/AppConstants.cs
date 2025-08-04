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
        public static String QuestionTypeTextInputEmail = "Text Input - Email";
        public static String QuestionTypeTextInputSuburb = "Text Input - Suburb";
        public static String QuestionTypeTextInputPostCode = "Text Input - PostCode";
        public static String QuestionTypeDate = "Date";
        public static String QuestionTypeFinish = "Finish";

        public static String EmailValidatorRegex = "^([\\w\\.\\-]+)@([\\w\\-]+)((\\.(\\w){2,3})+)$";

        //links to redirect users between pages
        public static String redirectToHome = "~/DefaultPage/Default.aspx";
        public static String redirectToViewAllRespondents = "~/Staff/Respondents/AllRespondents.aspx";
        public static String redirectToSearchAllRespondents = "~/Staff/StaffSearch/StaffSearch.aspx";
        public static String redirectToRegisterRespondents = "~/RespondentRegister/RespondentRegister.aspx";
        public static String redirectToAnswerConfirmation = "~/SurveyApp/AnswersConfirmation.aspx";
        public static String redirectToStaffDashboard = "~/Staff/StaffDashboard/StaffDashboard.aspx";


        //question ids for getting options for staff search
        public static int QuestionIDForStaffSearchBank = 7;
        public static int QuestionIDForStaffSearchBankService = 15;
        public static int QuestionIDForStaffSearchNewsPaper = 8;


        protected Int32 ParseStringToInt(String numberToParseInString)
        {

            if (numberToParseInString == null) return 0;

            Int32 numberToReturn = 0;

            return 0;

            }
        }
    }