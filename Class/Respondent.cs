using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AITR_Survey.Class
{
    public class Respondent
    {
        private String firstName;
        private String lastName;
        private String contactNumber;
        private DateTime dOB;
        private Int32 respondentID;
        private DateTime dateResponded;
        private String iPAddress;
        private Boolean isRegistered;

        public Respondent()
        {
        }

        public Respondent(string firstName, string lastName, string contactNumber, DateTime dOB, int respondentID, DateTime dateResponded, string iPAddress, bool isRegistered)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.contactNumber = contactNumber;
            this.dOB = dOB;
            this.respondentID = respondentID;
            this.dateResponded = dateResponded;
            this.iPAddress = iPAddress;
            this.isRegistered = isRegistered;
        }

        public string FirstName { get => firstName; set => firstName = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string ContactNumber { get => contactNumber; set => contactNumber = value; }
        public DateTime DOB { get => dOB; set => dOB = value; }
        public int RespondentID { get => respondentID; set => respondentID = value; }
        public DateTime DateResponded { get => dateResponded; set => dateResponded = value; }
        public string IPAddress { get => iPAddress; set => iPAddress = value; }
        public bool IsRegistered { get => isRegistered; set => isRegistered = value; }
    }
}