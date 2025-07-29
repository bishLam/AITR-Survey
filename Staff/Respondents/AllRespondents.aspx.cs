using AITR_Survey.Class;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey.Staff.Respondents
{
    public partial class AllRespondents : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var respondents = GetAllRespondents();
            foreach (var respondent in respondents)
            {
                Response.Write("Respondent ID: " + respondent.RespondentID + "<br />");
                Response.Write("Respondent Date: " + respondent.DateResponded + "<br />");
                Response.Write("Respondent IP Address: " + respondent.IPAddress + "<br />");
            }
        }

        public List<Respondent> GetAllRespondents()
        {
            SurveyQuestion surveyApp = new SurveyQuestion();
            string _connectionString = surveyApp.GetConnectionString();
            
            List<Respondent> respondents = new List<Respondent>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            conn.Open();

            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Respondent", conn);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                Respondent respondent = new Respondent();
                respondent.RespondentID = Int32.Parse(reader["RespondentID"].ToString());
                String dateResponded = reader["Date"].ToString();
                respondent.DateResponded = DateTime.Parse(dateResponded);
                respondent.IPAddress = reader["IPAddress"].ToString();
                respondents.Add(respondent);
            }
            conn.Close();
            return respondents;
        }
    }
}