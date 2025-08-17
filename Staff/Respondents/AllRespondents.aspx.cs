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
            var registeredRespondents = GetRegisteredRespondents();
            var unregisteredRespondents = GetNonRegisteredRespondents();
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

            respondentInfoLabel.Visible = false;



            // if the user clicks on the specific respondent, it will display the respondent details with questions and answers they answered

        }

        //hide the first 3 columns of the unregistered respondents grid view when the page load is comple
        //protected void unregisteredRespondentsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    // Hide the first 3 columns of the unregistered respondents grid view
        //    e.Row.Cells[1].Visible = false;
        //    e.Row.Cells[2].Visible = false;
        //    e.Row.Cells[3].Visible = false;
        //}

        protected void registeredRespondentsGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected row
            GridViewRow selectedRow = registeredRespondentsGridView.SelectedRow;
            // Get the RespondentID from the firth cell of the selected row


            int respondentID = convertStringToInt(selectedRow.Cells[5].Text);           
            //show the details label and gridview with data
            respondentInfoLabel.Visible = true;
            respondentInfoLabel.Text = "Survey details for the selected respondents with ID " + respondentID + " are as below: ";
            DisplayRespondentInfoFromRespondentID(respondentID);

        }

        protected void unregisteredRespondentsGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected row
            GridViewRow selectedRow = unregisteredRespondentsGridView.SelectedRow;
            // Get the RespondentID from the firth cell of the selected row


            int respondentID = convertStringToInt(selectedRow.Cells[5].Text);
            //show the details label and gridview with data
            respondentInfoLabel.Visible = true;
            respondentInfoLabel.Text = "Survey details for the selected respondents with ID " + respondentID + " are as below: ";
            DisplayRespondentInfoFromRespondentID(respondentID);

        }


        /// <summary>
        /// Queries the database and retrieves all respondents who are registered.
        /// </summary>
        /// <remarks>
        /// This method connects to the database using the connection string from the SurveyQuestion class,
        /// executes a query to select all records from the Respondent table, and filters the results to include
        /// only those respondents where the "isRegistered" field is "1". It constructs a list of Respondent
        /// objects for each registered respondent and returns this list.
        /// </remarks>
        /// <returns>A list of Respondent objects representing registered respondents.</returns>
        public List<Respondent> GetRegisteredRespondents()
        {


            SurveyQuestion surveyApp = new SurveyQuestion();
            string _connectionString = surveyApp.GetConnectionString();
            
            List<Respondent> respondents = new List<Respondent>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            try
            {
                conn.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Respondent", conn);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["isRegistered"].ToString() == "1")
                    {
                        Respondent respondent = new Respondent();
                        respondent.RespondentID = convertStringToInt(reader["RespondentID"].ToString());
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
            }

            catch(Exception ex)
            {
                Response.Write("Exception occured." + ex.Message);
            }
            conn.Close();
            return respondents;
        }

        /// <summary>
        /// Queries the database and retrieves all respondents who are not registered.
        /// </summary>
        /// <remarks>
        /// This method connects to the database using the connection string from the SurveyQuestion class,
        /// executes a query to select all records from the Respondent table, and filters the results to include
        /// only those respondents where the "isRegistered" field is "0". It constructs a list of Respondent
        /// objects for each unregistered respondent and returns this list.
        /// </remarks>
        /// <returns>A list of Respondent objects representing unregistered respondents.</returns>
        public List<Respondent> GetNonRegisteredRespondents()
        {
            SurveyQuestion surveyApp = new SurveyQuestion();
            string _connectionString = surveyApp.GetConnectionString();

            List<Respondent> respondents = new List<Respondent>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            try
            {
                conn.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Respondent", conn);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["isRegistered"].ToString() == "0")
                    {
                        Respondent respondent = new Respondent();
                        respondent.RespondentID = convertStringToInt(reader["RespondentID"].ToString());
                        String dateResponded = reader["Date"].ToString();
                        DateTime dateRespondentConverted;
                        if (DateTime.TryParse(dateResponded, out dateRespondentConverted))
                        {
                            respondent.DateResponded = dateRespondentConverted;
                        }

                        respondent.IPAddress = reader["IPAddress"].ToString();
                        respondent.FirstName = "Anonymous";
                        respondent.LastName = "Anonymous";
                        respondent.ContactNumber = "Anonymous";
                        respondent.IsRegistered = reader["isRegistered"].ToString() == "1" ? true : false;
                        respondents.Add(respondent);
                    }

                }
            }
            catch(Exception ex)
            {
                Response.Write("Exception occured." + ex.Message);
            }
            conn.Close();
            return respondents;
        }

        /// <summary>
        /// Tries to convert the provided into Integer and redirects user to the error screen with error information if there is any
        /// </summary>
        /// <param name="valueToConvert"></param>
        /// <returns>returns the converted value in Int32 format if it is parsable, otherwise redirects to the error page</returns>
        public Int32 convertStringToInt(String valueToConvert)
        {
            Int32 valueToStore;
            if (Int32.TryParse(valueToConvert, out valueToStore))
            {
                return valueToStore;
            }
            else
            {
                HttpContext.Current.Session["ErrorMessage"] = "String could not be converted to integer. Please try again. Provided String: " + valueToConvert;
                Response.Redirect(AppConstants.redirectToErrorPage);
                return 0;
            }
        }

        /// <summary>
        /// Displays detailed information about a respondent based on the provided respondent ID.
        /// </summary>
        /// <remarks>This method retrieves respondent details, including personal information,
        /// registration status, and answers to survey questions,  from the database and binds the data to a grid view
        /// for display. The data is fetched from the "RespondentAnswerView" database view.</remarks>
        /// <param name="resID">The unique identifier of the respondent whose information is to be displayed. Must be a valid integer.</param>
        protected void DisplayRespondentInfoFromRespondentID(Int32 resID)
        {
            try
            {

                // now we initialise the datatable with all the columns
                DataTable dt = new DataTable();
                dt.Columns.Add("Respondent ID", typeof(String));
                dt.Columns.Add("Date", typeof(String));
                dt.Columns.Add("First Name", typeof(String));
                dt.Columns.Add("Last Name", typeof(String));
                dt.Columns.Add("Contact Number", typeof(String));
                dt.Columns.Add("DOB", typeof(String));
                dt.Columns.Add("Registered?", typeof(String));
                dt.Columns.Add("Answer ID", typeof(String));
                dt.Columns.Add("Question ID", typeof(String));
                dt.Columns.Add("Question Text", typeof(String));
                dt.Columns.Add("MultipleChoice Answer ID", typeof(String));
                dt.Columns.Add("Option Text", typeof(String));
                dt.Columns.Add("TextInput Answer", typeof(String));

                //get the connection string
                SurveyQuestion surveyApp = new SurveyQuestion();
                String connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                // now we generate the dynamic query based on the selected id's and query the database
                SqlCommand sqlCommand = new SqlCommand($"SELECT * FROM RespondentAnswerView WHERE RespondentID = @resID", conn);
                sqlCommand.Parameters.AddWithValue("@resID", resID);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    // as long as there are new data rows from the database, we add the row to the datatable
                    DataRow row = dt.NewRow();
                    row["Question Text"] = reader["QuestionText"] as String;
                    row["First Name"] = reader["Firstname"] as String == "" ? "N/A" : reader["Firstname"] as String;
                    row["Last Name"] = reader["Lastname"] as String == "" ? "N/A" : reader["Lastname"] as String;
                    row["Contact Number"] = reader["ContactNumber"] as String == "" ? "N/A" : reader["ContactNumber"] as String;
                    row["Option Text"] = reader["OptionText"] as String == "" ? "N/A" : reader["OptionText"] as String;
                    row["TextInput Answer"] = reader["TextInputAnswer"] as String == "" ? "N/A" : reader["TextInputAnswer"] as String;
                    row["Registered?"] = reader["isRegistered"] as String == "" ? "N/A" : reader["isRegistered"] as String;

                    Int32 questionID = 0;
                    if (Int32.TryParse(reader["QuestionID"].ToString(), out questionID))
                    {
                        row["Question ID"] = questionID;

                    }

                    Int32 multipleChoiceAnswerID = 0;
                    if (Int32.TryParse(reader["MultipleChoiceAnswerID"].ToString(), out multipleChoiceAnswerID))
                    {
                        row["MultipleChoice Answer ID"] = multipleChoiceAnswerID;
                    }
                    else
                    {
                        row["MultipleChoice Answer ID"] = "N/A";
                    }

                    Int32 respondentID = 0;
                    if (Int32.TryParse(reader["RespondentID"].ToString(), out respondentID))
                    {
                        row["Respondent ID"] = respondentID.ToString();
                    }
                    else
                    {
                        row["Respondent ID"] = "N/A";
                    }

                    DateTime date = new DateTime();
                    if (DateTime.TryParse(reader["Date"].ToString(), out date))
                    {
                        row["Date"] = date.ToString();
                    }
                    else
                    {
                        row["Date"] = "N/A";
                    }

                    DateTime DOB = new DateTime();
                    if (DateTime.TryParse(reader["DOB"].ToString(), out DOB))
                    {
                        row["DOB"] = DOB.ToString();
                    }
                    else
                    {
                        row["DOB"] = "N/A";
                    }

                    Int32 answerID = 0;
                    if (Int32.TryParse(reader["AnswerID"].ToString(), out answerID))
                    {
                        row["Answer ID"] = answerID.ToString();
                    }
                    dt.Rows.Add(row);
                }
                //after we add all the returned data into the datatable, we need to bind it and display it to the grid view
                respondentDetailsGridView.DataSource = dt;
                respondentDetailsGridView.DataBind();
                conn.Close();
            }
            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
            }
        }
    }
}