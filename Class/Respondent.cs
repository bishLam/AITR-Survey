using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AITR_Survey.Class
{
    public class Respondent
    {
        private Int32 respondentID;
        private DateTime dateResponded;
        private String iPAddress;

        public Respondent(int respondentID, DateTime dateResponded, string iPAddress)
        {
            RespondentID = respondentID;
            DateResponded = dateResponded;
            IPAddress = iPAddress;
        }

        public Respondent()
        {
        }

        public int RespondentID { get => respondentID; set => respondentID = value; }
        public DateTime DateResponded { get => dateResponded; set => dateResponded = value; }
        public string IPAddress { get => iPAddress; set => iPAddress = value; }
    }
}