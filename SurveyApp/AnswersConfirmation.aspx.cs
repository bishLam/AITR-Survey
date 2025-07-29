using System;
using System.Collections.Generic;
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
            var answerDictionarySession = HttpContext.Current.Session["answerList"];

            if (answerDictionarySession != null)
            {

                List<Answer> answers = (List<Answer>)answerDictionarySession;
                if (answers.Count > 0)
                {

                    // Display the thank you message and the answers
                    Response.Write("<h2>Thank you for completing the survey!</h2>");
                    Response.Write("<h3>Your Answers:</h3>");

                    foreach (var answer in answers)
                    {


                        Int32 questionID = answer.QuestionID;
                       
                        Response.Write("<ul>");
                        
                        SurveyQuestion surveyQuestion = new SurveyQuestion(); //getting this class to use the method from this page
                        Question question = surveyQuestion.GetQuestionFromQuestionID(questionID); // Placeholder for actual question text retrieval
                        Response.Write("(Question ID: " + answer.QuestionID + ")  " + question.QuestionText);

                        if(answer.SingleChoiceAnswerID == null && answer.TextInputAnswer != null)
                        {
                            Response.Write("<p> TextInputAnswer: " + answer.TextInputAnswer + "</p>");
                        }

                        else  
                        {
                            Response.Write("<p> Multiple Choice Option ID: " + answer.SingleChoiceAnswerID + "</p>");       
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

            if (userIpAddress == null)
            {
                Response.Write("<p>Error: Unable to retrieve user IP address. Please try again later</p>");
                return;
            }
            try
            {
                var answerDictionarySession = HttpContext.Current.Session["answerList"];

                if (answerDictionarySession != null)
                {
                    List<Answer> answers = (List<Answer>)answerDictionarySession;
                    SaveRespondentToTheDatabase(userIpAddress);
                    
                    foreach (Answer answer in answers)
                    {
                        SaveAnswerToDatabase(answer);
                    }
                    // Save the respondent's IP address to the database
                    

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

        protected void SaveAnswerToDatabase(Answer answer)
        {
            Int32 respondentID = Int32.Parse(HttpContext.Current.Session["respondentID"].ToString());
            try
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
                SqlCommand cmd = new SqlCommand("INSERT INTO Respondent (Date, IPAddress) VALUES (@Date, @IPAddress); SELECT SCOPE_IDENTITY();", conn);
                cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                
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
    }
}