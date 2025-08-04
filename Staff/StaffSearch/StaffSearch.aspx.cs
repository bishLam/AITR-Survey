using System;
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
            Dictionary<Int32, String> banksUsedOptions =  GetOptionsForSearchCriterias(SearchCriteria.Banks);
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
       

        protected Dictionary<Int32, String> GetOptionsForSearchCriterias(SearchCriteria criteria)
        {   // This method will retrieve all banks from the database
            // and bind them to a control
            // Implementation details would depend on the specific requirements.
            try
            {
                SurveyQuestion surveyApp = new SurveyQuestion();
                string _connectionString = surveyApp.GetConnectionString();
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                SqlCommand sqlCommand = new SqlCommand("");

                if (criteria == SearchCriteria.Banks)
                {
                   sqlCommand = new SqlCommand("SELECT MultipleChoiceOptionID, OptionText FROM MultipleChoiceOption WHERE QuestionID = " + AppConstants.QuestionIDForStaffSearchBank, conn);
                }
                else if(criteria == SearchCriteria.BankServices)
                {
                    sqlCommand = new SqlCommand("SELECT MultipleChoiceOptionID, OptionText FROM MultipleChoiceOption WHERE QuestionID = " + AppConstants.QuestionIDForStaffSearchBankService, conn);
                }
                else if(criteria == SearchCriteria.Newspapers)
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
                if (reader.Read())
                {                
                    do
                    {
                        Int32 optionID = Int32.Parse(reader["MultipleChoiceOptionID"].ToString());
                        String optionText = reader["OptionText"].ToString();
                        options.Add(optionID, optionText);
                    } while (reader.Read());
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
            List<Int32> respondentID = new List<Int32>();

            //booleans to store if they are to be searched
            Boolean isNameEmpty = true;
            Boolean isBankEmpty = true;
            Boolean isBankServicesEmpty = true;
            Boolean isNewspaperReadEmpty = true;

            foreach (ListItem item in banksUsedCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    selectedBankOptions.Add(Int32.Parse(item.Value));
                }
            }

            foreach (ListItem item in bankServicesUsedCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    selectedBankServiceOptions.Add(Int32.Parse(item.Value));
                }
            }

            foreach (ListItem item in newspaperReadCheckBoxList.Items)
            {
                if (item.Selected)
                {
                    selectedNewspaperOptions.Add(Int32.Parse(item.Value));
                }
            }

            // Now we can use these selected options to search for respondents in the database
            if(firstName != "")
            {
                isNameEmpty = false;
            }

            if(selectedBankOptions.Count > 0)
            {
                isBankEmpty = false;
            }

            if(selectedBankServiceOptions.Count > 0)
            {
                isBankServicesEmpty = false;
            }

            if (selectedNewspaperOptions.Count > 0){
                isNewspaperReadEmpty = false;
            }

            if (!isNameEmpty)
            {
                String sqlQuery = "SELECT DISTINCT RespondentID FROM RespondentAnswerView WHERE Firstname = '" + firstName + "'";

                try
                {
                    //DataTable dt = new DataTable();
                    //dt.Columns.Add("Question ID", typeof(String));
                    //dt.Columns.Add("Question Text", typeof(String));
                    //dt.Columns.Add("MultipleChoice Answer ID", typeof(String));
                    //dt.Columns.Add("Option Text", typeof(String));
                    //dt.Columns.Add("TextInput Answer", typeof(String));


                    SurveyQuestion surveyApp = new SurveyQuestion();
                    String connectionString = surveyApp.GetConnectionString();
                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();
                    SqlCommand sqlCommand = new SqlCommand(sqlQuery, conn);

                    SqlDataReader reader = sqlCommand.ExecuteReader();
                        while (reader.Read())
                        {
                          Int32 resID = Int32.Parse(reader["RespondentID"].ToString());
                        respondentIDFromName.Add(resID);
                        //DataRow row = dt.NewRow();

                        //Int32 questionID = 0;
                        //if (Int32.TryParse(reader["QuestionID"].ToString(), out questionID))
                        //{
                        //    questionID = Int32.Parse(reader["QuestionID"].ToString());
                        //}
                        //row["Question ID"] = questionID;
                        //row["Question Text"] = reader["QuestionText"] as String;
                        //Int32 multipleChoiceAnswerID = 0;
                        //if (Int32.TryParse(reader["MultipleChoiceAnswerID"].ToString(), out multipleChoiceAnswerID))
                        //{
                        //    multipleChoiceAnswerID = Int32.Parse(reader["MultipleChoiceAnswerID"].ToString());
                        //}
                        //row["MultipleChoice Answer ID"] = multipleChoiceAnswerID;
                        //row["Option Text"] = reader["OptionText"] as String;
                        //row["TextInput Answer"] = reader["TextInputAnswer"] as String;    
                        //dt.Rows.Add(row);
                    }
                    //answerGridView.DataSource = dt;
                    //answerGridView.DataBind();
                    //Response.Write("Answer gridview count: " + dt.Rows.Count);

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

            //if (!isBankEmpty)
            //{
            //    foreach (Int32 item in selectedBankOptions)
            //    {
            //        String sqlQuery = "SELECT RespondentID FROM Answer WHERE questionID = 7 AND MultipleChoiceAnswerID = '" + firstName + "'";
            //    }


            //    try
            //    {
            //        SurveyQuestion surveyApp = new SurveyQuestion();
            //        String connectionString = surveyApp.GetConnectionString();
            //        SqlConnection conn = new SqlConnection(connectionString);
            //        conn.Open();
            //        SqlCommand sqlCommand = new SqlCommand(sqlQuery, conn);

            //        SqlDataReader reader = sqlCommand.ExecuteReader();
            //        while (reader.Read())
            //        {
            //            Int32 respondent = Int32.Parse(reader["RespondentID"].ToString());
            //            respondentID.Add(respondent);
            //        }
            //        testLabel.Text = "Respondent ID count: " + respondentID.Count;

            //        conn.Close();

            //    }
            //    catch (SqlException ex)
            //    {
            //        Response.Write("SqlException exception found. Error: " + ex.ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        Response.Write("Exception found. Error: " + ex.ToString());
            //    }
            //}





        }
    }
}
