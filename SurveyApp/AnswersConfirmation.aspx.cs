using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey.SurveyApp
{
    public partial class FinishSurvey : System.Web.UI.Page 
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            //get users IP address and store it in the session here
            string userIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(userIpAddress))
            {
                userIpAddress = Request.ServerVariables["REMOTE_ADDR"];
                HttpContext.Current.Session["userIpAddress"] = userIpAddress; // Store the IP address in the session
                if (string.IsNullOrEmpty(userIpAddress))
                {
                    Response.Write("<p>Unable to retrieve IP address. Please try again</p>");
                    return;
                }
                HttpContext.Current.Session["userIpAddress"] = userIpAddress; // Store the IP address in the session
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SubmitButton.Enabled = true; // Enable the submit button in case it was disabled previously
            CancelButton.Enabled = true; // Enable the cancel button in case it was disabled previously
            // get all the session stored variables
            var answerDictionarySession = HttpContext.Current.Session["answerList"];

            if (answerDictionarySession != null) //if the session is not null
            {
                List<Answer> answers = (List<Answer>)answerDictionarySession; //since this is stored as list of Answer, this conversion should be fine
                if (answers.Count > 0)
                {
                    // Display the thank you message and the answers
                    Response.Write("<h2>Thank you for completing the survey!</h2>");
                    Response.Write("<h3>Your Answers:</h3>");

                    // loop and display all the answers along with their id and questionn
                    foreach (var answer in answers)
                    {
                        Int32 questionID = answer.QuestionID;                      
                        Response.Write("<ul>");                       
                        SurveyQuestion surveyQuestion = new SurveyQuestion(); //getting this class to use the method from this page
                        Question question = surveyQuestion.GetQuestionFromQuestionID(questionID); // Placeholder for actual question text retrieval
                        Response.Write("(Question ID: " + answer.QuestionID + ")  " + question.QuestionText);

                        //in case of text input answer
                        if(answer.SingleChoiceAnswerID == null && answer.TextInputAnswer != null)
                        {
                            Response.Write("<p> TextInputAnswer: " + answer.TextInputAnswer + "</p>");
                        }

                        else  //otherwise this is a question with multiple/single choice, so we get the option text and id
                        {

                            String optionText = getOptionTextFromOptionID(answer.SingleChoiceAnswerID);
                            Response.Write("<p> (Multiple Choice Option ID: " + answer.SingleChoiceAnswerID + ") :" + optionText + "</p>");       
                        }
                        Response.Write("</ul><br />");
                    }
                    
                }
                else
                {
                    Response.Write("No answers submitted.");
                }
            }
            else
            {
                Response.Write("Session expired or no answers submitted.");
            }
        }
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            // Send the data to the database
            String userIpAddress = HttpContext.Current.Session["userIpAddress"] as String; // Store the IP address in the session

            if (userIpAddress == null) //if the user IP address is null, then we cannot proceed
            {
                Response.Write("<p>Error: Unable to retrieve user IP address. Please try again later</p>");
                return;
            }
            try
            {
                // Retrieve the answer list from the session
                var answerDictionarySession = HttpContext.Current.Session["answerList"];

                // Check if the session contains answers
                if (answerDictionarySession != null)
                {
                    List<Answer> answers = (List<Answer>)answerDictionarySession;
                    Response.Write("Saving your answers to the database. Please do not make any changes");
                    SubmitButton.Visible = false; // Disable the button to prevent multiple submissions
                    CancelButton.Visible = false; // Disable the cancel button to prevent further actions
                    SaveRespondentToTheDatabase(userIpAddress); // Save the respondent to the database
                    SaveAnswerToDatabase(answers); // Save the answers to the database

                    // Clear the session data after submission
                    HttpContext.Current.Session["answerList"] = null;
                    Response.Redirect("../SurveyApp/SurveyComplete.aspx");

                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during submission
                Response.Write("<p>Error submitting answers: " + ex.Message + "</p>");
            }
        }


        protected void CancelButton_Click(object sender, EventArgs e)
        {
            // Clear the session data after displaying the answers
            HttpContext.Current.Session["answerList"] = null;
            Response.Redirect("../SurveyApp/SurveyQuestion.aspx");
        }


        protected void SaveAnswerToDatabase(List<Answer> answers)
        {
            Int32 respondentID = convertStringToInt(HttpContext.Current.Session["respondentID"].ToString());
            try
            {
                foreach (Answer answer in answers)
                {
                    SurveyQuestion surveyQuestion = new SurveyQuestion();
                    string _connectionString = surveyQuestion.GetConnectionString();
                    SqlConnection conn = new SqlConnection();
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO Answer (QuestionID, MultipleChoiceAnswerID, TextInputAnswer) VALUES (@QuestionID, @MultipleChoiceAnswerID, @TextInputAnswer); SELECT SCOPE_IDENTITY();", conn);
                    if (answer.SingleChoiceAnswerID == null && answer.TextInputAnswer != null)
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", answer.QuestionID);
                        cmd.Parameters.AddWithValue("@MultipleChoiceAnswerID", DBNull.Value);
                        cmd.Parameters.AddWithValue("@TextInputAnswer", answer.TextInputAnswer);
                        //insert into the database and get  the answer ID to match respondent and answer
                        Object answerID = cmd.ExecuteScalar();
                        int insertedId = Convert.ToInt32(answerID);

                        //insert the respondent and answer into the database
                        InsertRespondentAnswerInDatabase(insertedId, respondentID);
                    }

                    else
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", answer.QuestionID);
                        cmd.Parameters.AddWithValue("@MultipleChoiceAnswerID", answer.SingleChoiceAnswerID);
                        cmd.Parameters.AddWithValue("@TextInputAnswer", DBNull.Value);

                        //insert into the database and get  the answer ID to match respondent and answer
                        Object answerID = cmd.ExecuteScalar();
                        int insertedId = Convert.ToInt32(answerID);

                        //insert the respondent and answer into the database
                        InsertRespondentAnswerInDatabase(insertedId, respondentID);
                    }
                    conn.Close();
                }
            }
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return;
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return;
            }   


        }


        protected void InsertRespondentAnswerInDatabase(Int32 answerID, Int32 respondentID)
        {
            try
            {
                SurveyQuestion surveyQuestion = new SurveyQuestion();
                string _connectionString = surveyQuestion.GetConnectionString();
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Respondent_Answer VALUES (@RespondentID, @AnswerID)", conn);
                cmd.Parameters.AddWithValue("@RespondentID", respondentID);
                cmd.Parameters.AddWithValue("@AnswerID", answerID);

                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return;
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return;
            }
        }

        protected void SaveRespondentToTheDatabase(string ipAddress)
        {
            try
            {
                SurveyQuestion surveyQuestion = new SurveyQuestion();
                string _connectionString = surveyQuestion.GetConnectionString();
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                var firstname = HttpContext.Current.Session["respondentFirstName"] as String;
                var lastname = HttpContext.Current.Session["respondentLastName"] as String;
                var contactNumber = HttpContext.Current.Session["respondentContactNumber"] as String;

                String sFirstName;
                String sLastName;
                String sContactNumber;
                DateTime dDOB;
                Boolean isRegistered;


                if (firstname != null && lastname != null && contactNumber != null )
                {
                    //this means that the user opted to register in the program
                    sFirstName = firstname;
                    sLastName = lastname;
                    sContactNumber = contactNumber;


                    if (DateTime.TryParse(HttpContext.Current.Session["respondentDOB"] as String, out dDOB))
                    {
                        dDOB = DateTime.Parse(HttpContext.Current.Session["respondentDOB"] as String);
                        isRegistered = true;
                    }
                    else
                    {

                        //show a validation message here
                        return;
                    }
                }
                else{
                    //this means they did not opted to register into the program
                    sFirstName = "";
                    sLastName = "";
                    sContactNumber = "";
                    DateTime defaultDT = new DateTime(1000, 1, 1);
                    dDOB = defaultDT;
                    isRegistered = false;
                }



                SqlCommand cmd = new SqlCommand("INSERT INTO Respondent (Date, IPAddress, Firstname, Lastname, ContactNumber, DOB, isRegistered) VALUES (@Date, @IPAddress, @Firstname, @Lastname, @ContactNumber, @DOB, @isRegistered); SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                cmd.Parameters.AddWithValue("@Firstname", sFirstName);
                cmd.Parameters.AddWithValue("@Lastname", sLastName);
                cmd.Parameters.AddWithValue("@ContactNumber", sContactNumber);

                DateTime dt = new DateTime(1000, 1, 1);
                if (dDOB == dt)
                {
                    cmd.Parameters.AddWithValue("@DOB", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@DOB", dDOB);
                }               
                cmd.Parameters.AddWithValue("@isRegistered", isRegistered);

                Object respondentId = cmd.ExecuteScalar();
                int insertedId = Convert.ToInt32(respondentId);
                HttpContext.Current.Session["respondentID"] = insertedId;
                conn.Close();
            }
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return;
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return;
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

        protected String getOptionTextFromOptionID(Int32? optionIDNullable)
        {
            try
            {
                if (optionIDNullable == null || optionIDNullable == 0)
                {
                    HttpContext.Current.Session["ErrorMessage"] = "Option ID is not valid. Something went wrong, please try again later";
                    Response.Redirect(AppConstants.redirectToErrorPage);
                    return "";
                }

                Int32 optionID = convertStringToInt(optionIDNullable.ToString());

                String optionText = "";
                SurveyQuestion surveyQuestion = new SurveyQuestion();
                string _connectionString = surveyQuestion.GetConnectionString();
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();
               
                SqlCommand cmd = new SqlCommand("SELECT OptionText FROM MultipleChoiceOption WHERE MultipleChoiceOptionID = @optionID", conn);
                cmd.Parameters.AddWithValue("@optionID", optionID);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    optionText = reader["OptionText"].ToString();
                }
                else
                {
                    Response.Write("No option found for the provided ID: " + optionID);
                }
                conn.Close();
                return optionText;
            }
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return "";
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return "";
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return "";
            }

            catch (IOException ex)
            {
                Response.Write("IOException exception found. Error: " + ex.ToString());
                return "";
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return "";
            }
        }
    }
}