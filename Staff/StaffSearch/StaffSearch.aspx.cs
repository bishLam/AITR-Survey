using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class StaffDashboard : System.Web.UI.Page
    {

        //enum values to get the search criterias
        public enum SearchCriteria
        {
            Banks,
            BankServices,
            Newspapers
        }

        private List<Int32> respondentIDFromName = new List<Int32>();
        private List<Int32> respondentIDFromBanks = new List<Int32>();
        private List<Int32> respondentIDFromBankServices = new List<Int32>();
        private List<Int32> respondentIDFromNewspaper = new List<Int32>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                // get all the options to display
                Dictionary<Int32, String> banksUsedOptions = GetOptionsForSearchCriterias(SearchCriteria.Banks);
                Dictionary<Int32, String> banksServicesUsedOptions = GetOptionsForSearchCriterias(SearchCriteria.BankServices);
                Dictionary<Int32, String> newspaperReadOptions = GetOptionsForSearchCriterias(SearchCriteria.Newspapers);


                //display options for banks used section
                banksUsedCheckBoxList.DataSource = banksUsedOptions.ToList();
                banksUsedCheckBoxList.DataTextField = "Value";
                banksUsedCheckBoxList.DataValueField = "Key";
                banksUsedCheckBoxList.DataBind();

                //display options for banks services used section
                bankServicesUsedCheckBoxList.DataSource = banksServicesUsedOptions.ToList();
                bankServicesUsedCheckBoxList.DataTextField = "Value";
                bankServicesUsedCheckBoxList.DataValueField = "Key";
                bankServicesUsedCheckBoxList.DataBind();

                //display options for newspaper read section
                newspaperReadCheckBoxList.DataSource = newspaperReadOptions.ToList();
                newspaperReadCheckBoxList.DataTextField = "Value";
                newspaperReadCheckBoxList.DataValueField = "Key";
                newspaperReadCheckBoxList.DataBind();
            }
        }


        /// <summary>
        /// Retrieves a dictionary of options based on the specified search criteria.
        /// </summary>
        /// <remarks>This method queries a database to retrieve options corresponding to the provided
        /// search criteria.  Ensure that the database connection string is correctly configured and that the database
        /// contains  the expected data for the specified criteria.</remarks>
        /// <param name="criteria">The search criteria that determines which set of options to retrieve.  Valid values are <see
        /// cref="SearchCriteria.Banks"/>, <see cref="SearchCriteria.BankServices"/>,  and <see
        /// cref="SearchCriteria.Newspapers"/>.</param>
        /// <returns>A dictionary where the key is the unique identifier of the option and the value is the option's text. 
        /// Returns an empty dictionary if the search criteria is invalid or if an error occurs during execution.</returns>
        protected Dictionary<Int32, String> GetOptionsForSearchCriterias(SearchCriteria criteria)
        {
            try
            {
                
                SurveyQuestion surveyApp = new SurveyQuestion();
                string _connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                SqlCommand sqlCommand = new SqlCommand("");

                // generate dynamic query string based on which search criteria options is needed
                if (criteria == SearchCriteria.Banks)
                {
                    sqlCommand = new SqlCommand("SELECT MultipleChoiceOptionID, OptionText FROM MultipleChoiceOption WHERE QuestionID = " + AppConstants.QuestionIDForStaffSearchBank, conn);
                }
                else if (criteria == SearchCriteria.BankServices)
                {
                    sqlCommand = new SqlCommand("SELECT MultipleChoiceOptionID, OptionText FROM MultipleChoiceOption WHERE QuestionID = " + AppConstants.QuestionIDForStaffSearchBankService, conn);
                }
                else if (criteria == SearchCriteria.Newspapers)
                {
                    sqlCommand = new SqlCommand("SELECT MultipleChoiceOptionID, OptionText FROM MultipleChoiceOption WHERE QuestionID = " + AppConstants.QuestionIDForStaffSearchNewsPaper, conn);
                }
                else
                {
                    Response.Write("Invalid search criteria specified.");
                    return new Dictionary<Int32, String>();
                }

                SqlDataReader reader = sqlCommand.ExecuteReader();
                Dictionary<Int32, String> options = new Dictionary<Int32, String>();
                while (reader.Read())
                {
                    Int32 optionID = convertStringToInt(reader["MultipleChoiceOptionID"].ToString());
                    String optionText = reader["OptionText"].ToString();
                    options.Add(optionID, optionText);
                } 
                conn.Close();
                return options;
            }

            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return new Dictionary<int, string>();
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return new Dictionary<int, string>();
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return new Dictionary<int, string>();
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return new Dictionary<int, string>();
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return new Dictionary<int, string>();
            }

        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            //here we will handle the search button click event
            //we will get the selected options from the checkboxes and then search for respondents who have selected those options
            String firstName = firstNameTextBox.Text.Trim();
            List<Int32> selectedBankOptions = new List<Int32>();
            List<Int32> selectedBankServiceOptions = new List<Int32>();
            List<Int32> selectedNewspaperOptions = new List<Int32>();


            // get all the selected banks used
            foreach (ListItem item in banksUsedCheckBoxList.Items)
            {
                if (item.Selected)
                {
                   selectedBankOptions.Add(convertStringToInt(item.Value));
                }
            }

            // get all the selected banks services
            foreach (ListItem item in bankServicesUsedCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    selectedBankServiceOptions.Add(convertStringToInt(item.Value));
                }
            }

            // get all the selected newspapers
            foreach (ListItem item in newspaperReadCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    selectedNewspaperOptions.Add(convertStringToInt(item.Value));
                }
            }



            // Now we can use these selected options to search for respondents in the database          
            if (!String.IsNullOrEmpty(firstName))
            {
                //get all the respondents ID that matches the firstname
                getRespondentIDFromName(firstName);
                //if we don't find anything, then the list is empty display the message
                if (respondentIDFromName.Count == 0)
                {
                    headerLabel.Text = "No users found with the specified first name.";
                    answerGridView.Visible = false;
                    return;
                }

            }

            if (selectedBankOptions.Count > 0)
            {
                // get all the respondents ID which matches the all the list of selected banks 
                getRespondentIDFromBanks(selectedBankOptions);
                //if we don't find anything, then the list is empty display the message
                if (respondentIDFromBanks.Count == 0)
                {
                    headerLabel.Text = "No users found with the specified bank options.";
                    answerGridView.Visible = false;
                    return;
                }
            }

            if (selectedBankServiceOptions.Count > 0)
            {
                // get all the respondents ID which matches the all the list of selected banks services
                getRespondentIDFromBankServices(selectedBankServiceOptions);
                //if we don't find anything, then the list is empty display the message
                if (respondentIDFromBankServices.Count == 0)
                {
                    headerLabel.Text = "No users found with the specified bank service options.";
                    answerGridView.Visible = false;
                    return;
                }
            }

            if (selectedNewspaperOptions.Count > 0)
            {
                // get all the respondents ID which matches the all the list of selected newspaper 
                getRespondentIDFromNewspaper(selectedNewspaperOptions);
                //if we don't find anything, then the list is empty display the message
                if (respondentIDFromNewspaper.Count == 0)
                {
                    headerLabel.Text = "No users found with the specified newspaper options.";
                    answerGridView.Visible = false;
                    return;
                }
            }

            // if the first name is empty and no options were selected, then we don't need to do anything
            if (String.IsNullOrEmpty(firstName) && selectedBankOptions.Count == 0 && selectedBankServiceOptions.Count == 0 && selectedNewspaperOptions.Count == 0)
            {
                headerLabel.Text = "Please enter at least one search criteria. Or visit all respondents page if you want to view all respondents.";
                return;
            }

            // make sure the grid is visible if it was hidden in the beginning
            answerGridView.Visible = true;

            //now, from the received respondent ID's from each of all the selected options, we look for the intersection between them to find repondent who answered all of them as requested
            var listOfLists = new List<List<int>>() { respondentIDFromBanks, respondentIDFromBankServices, respondentIDFromName, respondentIDFromNewspaper };
            if(listOfLists.Count == 0) // if the list is empty, no users were found
            {
                headerLabel.Text = "No users found with the specified criteria.";
                return;
            }
            //actual intersection formulae
            List<Int32> intersection = listOfLists.Where(l => l.Any()).Aggregate<IEnumerable<int>>((previousList, nextList) => previousList.Intersect(nextList)).ToList();

            // if there is no intersection, then there was no users with the matching information
            if(intersection.Count == 0)
            {
                headerLabel.Text = "No users found with the specified criteria.";
                return;
            }
            headerLabel.Text = $"We have found {intersection.Count} user(s) with the matching criterias. Look at the table below for more information";
            
            // Now we can display the results in a grid 
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
                String idParams = String.Join(",", intersection.Select((x, i) => "@id" + i));
                SqlCommand sqlCommand = new SqlCommand($"SELECT * FROM RespondentAnswerView WHERE RespondentID IN ({idParams})", conn);

                for (int i = 0; i < intersection.Count; i++)
                {
                    sqlCommand.Parameters.AddWithValue("@id" + i, intersection[i]);
                }
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
                    row["Registered?"] = reader["isRegistered"] as String == "0" ? "No" : "Yes";

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
                    answerGridView.DataSource = dt;
                    answerGridView.DataBind();
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

        /// <summary>
        /// Retrieves the respondent IDs associated with the specified first name.
        /// </summary>
        /// <remarks>This method queries the database to find all distinct respondent IDs that match the
        /// provided first name. The results are added to a collection for further processing. Ensure that the database
        /// connection string is correctly configured before calling this method.</remarks>
        /// <param name="firstName">The first name of the respondent to search for. Cannot be null or empty.</param>
        protected void getRespondentIDFromName(String firstName)
        {
            try
            {
                //get the connection string
                SurveyQuestion surveyApp = new SurveyQuestion();
                String connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                //query the database
                SqlCommand sqlCommand = new SqlCommand("SELECT Distinct(RespondentID) FROM RespondentAnswerView WHERE Firstname = @FirstName", conn);
                sqlCommand.Parameters.AddWithValue("@FirstName", firstName);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                // as long as thee is data, add it to the list
                    while (reader.Read())
                    {
                        Int32 respondentID = convertStringToInt(reader["RespondentID"].ToString());
                        respondentIDFromName.Add(respondentID);
                    }
                conn.Close();
            }
            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
            }

        }


        
        /// <summary>
        /// Retrieves the respondent IDs from the database based on the selected bank options.
        /// </summary>
        /// <remarks>This method queries the database to find respondent IDs that match all the provided
        /// bank options.  The query ensures that only respondents who selected all the specified options are included
        /// in the results.</remarks>
        /// <param name="selectedBankOptions">A list of integers representing the selected bank option IDs. Each ID corresponds to a multiple-choice
        /// answer.</param>
        protected void getRespondentIDFromBanks(List<Int32> selectedBankOptions)
        {
            try
            {
                // get the connection string
                SurveyQuestion surveyApp = new SurveyQuestion();
                String connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                String bankOptionsString = String.Join(",", selectedBankOptions.Select((x, i) => "@option" + i));
           
                //generare the dynamic query based on all the selected options
                String query = $"SELECT RespondentID FROM RespondentAnswerView WHERE QuestionID = 7 AND (MultipleChoiceAnswerID in ({bankOptionsString})) GROUP BY RespondentID HAVING COUNT(DISTINCT MultipleChoiceAnswerID) = {selectedBankOptions.Count}";
                
                SqlCommand sqlCommand = new SqlCommand(query, conn);
                
                for(int i = 0; i<selectedBankOptions.Count; i++)
                {
                    sqlCommand.Parameters.AddWithValue("@option" + i, selectedBankOptions[i]);
                }

                //execute the reader and add it to the list as long as there is unique respondent id
                SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Int32 respondentID = convertStringToInt(reader["RespondentID"].ToString());
                        respondentIDFromBanks.Add(respondentID);
                    }

                conn.Close();
            }
            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
            }
        }



        /// <summary>
        /// Retrieves the respondent IDs from the database based on the selected bank services options.
        /// </summary>
        /// <remarks>This method queries the database to find respondent IDs that match all the provided
        /// bank services options.  The query ensures that only respondents who selected all the specified options are included
        /// in the results.</remarks>
        /// <param name="selectedBankServiceOptions">A list of integers representing the selected bank services option IDs. Each ID corresponds to a multiple-choice
        /// answer</param>
        protected void getRespondentIDFromBankServices(List<Int32> selectedBankServiceOptions)
        {
            try
            {
                SurveyQuestion surveyApp = new SurveyQuestion();
                String connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                String bankOptionsString = String.Join(",", selectedBankServiceOptions.Select((x, i) => "@option" + i));
                String query = $"SELECT RespondentID FROM RespondentAnswerView WHERE QuestionID = 15 AND (MultipleChoiceAnswerID in ({bankOptionsString})) GROUP BY RespondentID HAVING COUNT(DISTINCT MultipleChoiceAnswerID) = {selectedBankServiceOptions.Count}";

                SqlCommand sqlCommand = new SqlCommand(query, conn);

                for (int i = 0; i < selectedBankServiceOptions.Count; i++)
                {
                    sqlCommand.Parameters.AddWithValue("@option" + i, selectedBankServiceOptions[i]);
                }

                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    Int32 respondentID = convertStringToInt(reader["RespondentID"].ToString());
                    respondentIDFromBankServices.Add(respondentID);
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
            }
        }


        /// <summary>
        /// Retrieves the respondent IDs from the database based on the selected newspaper options.
        /// </summary>
        /// <remarks>This method queries the database to find respondent IDs that match all the provided
        /// newspaper options.  The query ensures that only respondents who selected all the specified newspaper options are included
        /// in the results.</remarks>
        /// <param name="selectedNewspaperOptions">A list of integers representing the selected newspaper option IDs. Each ID corresponds to a multiple-choice
        /// answer</param>
        protected void getRespondentIDFromNewspaper(List<Int32> selectedNewspaperOptions)
        {
            try
            {
                // get the connection string
                SurveyQuestion surveyApp = new SurveyQuestion();
                String connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                //generate dynamic query based on selected options
                String newspaperOptionsString = String.Join(",", selectedNewspaperOptions.Select((x, i) => "@option" + i));
                String query = $"SELECT RespondentID FROM RespondentAnswerView WHERE QuestionID = 8 AND (MultipleChoiceAnswerID in ({newspaperOptionsString})) GROUP BY RespondentID HAVING COUNT(DISTINCT MultipleChoiceAnswerID) = {selectedNewspaperOptions.Count}";

                SqlCommand sqlCommand = new SqlCommand(query, conn);

                for (int i = 0; i < selectedNewspaperOptions.Count; i++)
                {
                    sqlCommand.Parameters.AddWithValue("@option" + i, selectedNewspaperOptions[i]);
                }             

                // execute the reader and add the data as long as there is unique respondent values in the database
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    Int32 respondentID = convertStringToInt(reader["RespondentID"].ToString());
                    respondentIDFromNewspaper.Add(respondentID);
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
            }
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



    }


}
