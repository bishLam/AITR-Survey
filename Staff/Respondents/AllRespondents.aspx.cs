using AITR_Survey.Class;
using System;
using System.Collections.Generic;
using System.Data;
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
            var registeredRespondents = GetResgisteredRespondents();
            var unregisteredRespondents = GetNonResgisteredRespondents();
            //DataTable dataTable = new DataTable();
            //dataTable.Columns.Add("Respondent ID", typeof(String));
            //dataTable.Columns.Add("Date Responded", typeof(String));
            //dataTable.Columns.Add("IP Address", typeof(String));
            //dataTable.Columns.Add("First Name", typeof(String));
            //dataTable.Columns.Add("Last Name", typeof(String));
            //dataTable.Columns.Add("Contact Number", typeof(String));
            //dataTable.Columns.Add("Date of Birth", typeof(String));
            //dataTable.Columns.Add("Registered", typeof(String));


            //foreach (var respondent in respondents)
            //{
            //    DataRow row = dataTable.NewRow();
            //    row["Respondent ID"] = respondent.RespondentID;
            //    row["Date Responded"] = respondent.DateResponded.ToString();
            //    row["IP Address"] = respondent.IPAddress;
            //    row["First Name"] = respondent.FirstName;
            //    row["Last Name"] = respondent.LastName;
            //    row["Contact Number"] = respondent.ContactNumber;
            //    row["Date of Birth"] = respondent.DOB.ToString();
            //    row["Registered"] = respondent.IsRegistered.ToString();

            //    dataTable.Rows.Add(row);
            //}
            registeredRespondentsGridView.DataSource = registeredRespondents;
            registeredRespondentsGridView.DataBind();

            unregisteredRespondentsGridView.DataSource = unregisteredRespondents;
            unregisteredRespondentsGridView.DataBind();
        }

        public List<Respondent> GetResgisteredRespondents()
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
                if (reader["isRegistered"].ToString() == "1")
                {
                    Respondent respondent = new Respondent();
                    respondent.RespondentID = Int32.Parse(reader["RespondentID"].ToString());
                    String dateResponded = reader["Date"].ToString();
                    respondent.DateResponded = DateTime.Parse(dateResponded);
                    respondent.IPAddress = reader["IPAddress"].ToString();
                    respondent.FirstName = reader["Firstname"].ToString();
                    respondent.LastName = reader["Lastname"].ToString();
                    respondent.ContactNumber = reader["ContactNumber"].ToString();
                    respondent.DOB = DateTime.Parse(reader["DOB"].ToString());
                    respondent.IsRegistered = reader["isRegistered"].ToString() == "1" ? true : false;

                    respondents.Add(respondent);
                }

            }
            conn.Close();
            return respondents;
        }

        public List<Respondent> GetNonResgisteredRespondents()
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
                if (reader["isRegistered"].ToString() == "0")
                {
                    Respondent respondent = new Respondent();
                    respondent.RespondentID = Int32.Parse(reader["RespondentID"].ToString());
                    String dateResponded = reader["Date"].ToString();
                    respondent.DateResponded = DateTime.Parse(dateResponded);
                    respondent.IPAddress = reader["IPAddress"].ToString();
                    respondent.FirstName = reader["Firstname"].ToString();
                    respondent.LastName = reader["Lastname"].ToString();
                    respondent.ContactNumber = reader["ContactNumber"].ToString();
                    respondent.IsRegistered = reader["isRegistered"].ToString() == "1" ? true : false;

                    respondents.Add(respondent);
                }

            }
            conn.Close();
            return respondents;
        }
    }
}