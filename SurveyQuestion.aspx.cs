using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class SurveyQuestion : System.Web.UI.Page
    {
        Int32 currentQuestionID;
        Int32 nextQuestionID;
        String CurrentPlaceholderType = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) //postback is false only when the page is loaded for the first time.
            {
                LoadFirstQuestion();
            }
            else
            {
                //LoadNextQuestion();
                Question question = GetQuestionFromQuestionID(nextQuestionID);
                //now store the current value in the session and prompt them with the next question
                SetQuestionTextInAFormat(question);
                var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
                SetUpListOfOptions(listOfOptions, question.QuestionType);
            }
            
        }

        protected void nextButton_Click(object sender, EventArgs e)
        {
            //1. Get the selected option
            if (answerPlaceholder.Controls.Count > 0) //meaning there is at least one control
            {
                if (CurrentPlaceholderType.Equals("RadioButton"))
                {
                    for (int i = 0; i < answerPlaceholder.Controls.Count; i++)
                    {
                        if (answerPlaceholder.Controls[i] is RadioButton rb && rb.Checked)
                        {
                            //now store the data into the session for later
                            Int32 selectedOptionID = Int32.Parse(rb.ID);
                            //System.Diagnostics.Debug.WriteLine(selectedOptionID);
                            nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);    
                        }
                    }
                }
            }
        }

        protected Int32 FindNextQuestionFromOptionID(int optionID)
        {
            string _connectionString = GetConnectionString();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            conn.Open();

            //2.Prepare the consume instruction
            SqlCommand questionCommand = new SqlCommand("SELECT * FROM MultipleChoiceOption WHERE MultipleChoiceOptionID =" + optionID, conn); //command to get the questions

            //3. Consume
            SqlDataReader reader = questionCommand.ExecuteReader();
            reader.Read();
            nextQuestionID = Int32.Parse(reader["NextQuestionID"].ToString());
            conn.Close();
            return nextQuestionID;
            
        }

        protected string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["DevelopmentConnectionString"].ConnectionString.Equals("Dev"))
            {
                string _connectionString = AppConstants._connectionString.ToString();
                return _connectionString;
            }
            else
            {
                return ""; // Use the different connection string here
            }
        }

        protected Question GetQuestionFromQuestionID(Int32 questionID)
        {
            string _connectionString = GetConnectionString();
            Question question = new Question();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            conn.Open();

            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Question WHERE QuestionID =" + questionID, conn);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.Read())
            {
                question.QuestionID = Int32.Parse(reader["QuestionID"].ToString());
                question.QuestionText = reader["QuestionText"].ToString();
                question.NextQuestionForTextInput = reader["NextQuestionForTextInput"].ToString(); 
                question.IsFirstQuestion = reader["isFirstQuestion"].ToString();
                question.QuestionType = reader["QuestionType"].ToString();
                question.HasBranch = reader["HasBranch"].ToString();
                question.MaxSelection = Int32.Parse(reader["MaxAnswerSelection"].ToString());
            }
            conn.Close();
            return question;
        }

        protected void SetQuestionTextInAFormat(Question question)
        {
            QuestionLabel.Text = "(Question " + question.QuestionID + ")  " + question.QuestionText; //display the question text
        }
        protected List<Option> GetAllOptionsFromQuestionID(int questionID)
        {
            var listOfOptions = new List<Option>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = GetConnectionString();
            conn.Open();
            //at this point we would have the first question. Now lets take the options
            SqlCommand optionsCommand = new SqlCommand("SELECT * FROM MultipleChoiceOption WHERE QuestionID =" + questionID, conn);
            SqlDataReader optionsReader = optionsCommand.ExecuteReader(); //execute the command

            while (optionsReader.Read())
            {
                Option option = new Option();
                option.MultipleChoiceOptionID = Int32.Parse(optionsReader["MultipleChoiceOptionID"].ToString());
                option.QuestionID = Int32.Parse(optionsReader["QuestionID"].ToString());
                option.NextQuestionID = Int32.Parse(optionsReader["NextQuestionID"].ToString());
                option.OptionText = optionsReader["OptionText"].ToString();
                listOfOptions.Add(option);
            }
            conn.Close();
            return listOfOptions;
        }

        protected void SetUpListOfOptions(List<Option> options, String questionType)
        {
            if(questionType == "Single Choice")
            {
                //populate the placeholder with radio button options

                //get the placeholder first and clear the existing ones
                answerPlaceholder.Controls.Clear();
                foreach(Option option in options)
                {
                    RadioButton radioButton = new RadioButton();
                    radioButton.ID = option.MultipleChoiceOptionID.ToString();
                    radioButton.GroupName = "options";
                    radioButton.Text = option.OptionText;
                    CurrentPlaceholderType = "RadioButton";
                    answerPlaceholder.Controls.Add(radioButton);

                }


            }
        }

        protected void LoadFirstQuestion()
        {
            //here connect the database

            //1. Open the connection string
            String _connectionString = GetConnectionString();

            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = _connectionString;
            conn.Open();

            //2.Prepare the consume instruction
            SqlCommand questionCommand = new SqlCommand("SELECT * FROM Question WHERE IsFirstQuestion='True'", conn); //command to get the questions

            //3. Consume
            SqlDataReader reader = questionCommand.ExecuteReader();


            //4. for the storage
            Question firstQuestion= new Question();
            while (reader.Read())
            {
                firstQuestion.QuestionID = Int32.Parse(reader["QuestionID"].ToString());
                firstQuestion.QuestionText = reader["QuestionText"].ToString();
                firstQuestion.NextQuestionForTextInput = reader["NextQuestionForTextInput"].ToString();
                firstQuestion.IsFirstQuestion = reader["isFirstQuestion"].ToString();
                firstQuestion.QuestionType = reader["QuestionType"].ToString();
                firstQuestion.HasBranch = reader["HasBranch"].ToString();
                firstQuestion.MaxSelection = Int32.Parse(reader["MaxAnswerSelection"].ToString());
            }
            reader.Close();
            //at this point we would have the first question. Now lets take the options
            SqlCommand optionsCommand = new SqlCommand("SELECT * FROM MultipleChoiceOption WHERE QuestionID =" + firstQuestion.QuestionID, conn);
            SqlDataReader optionsReader = optionsCommand.ExecuteReader(); //execute the command
            List<Option> listOfOptions = new List<Option>();
            while (optionsReader.Read())
            {
                Option option = new Option();
                option.MultipleChoiceOptionID = Int32.Parse(optionsReader["MultipleChoiceOptionID"].ToString());
                option.QuestionID = Int32.Parse(optionsReader["QuestionID"].ToString());
                option.NextQuestionID = Int32.Parse(optionsReader["NextQuestionID"].ToString());
                option.OptionText = optionsReader["OptionText"].ToString();
                listOfOptions.Add(option);
            }
            conn.Close();
            //Now we have the question and options, lets display them
            QuestionLabel.Text = "(Question " + firstQuestion.QuestionID + ")  " + firstQuestion.QuestionText.ToString(); //display the question text
            currentQuestionID = firstQuestion.QuestionID;

            String questionType = firstQuestion.QuestionType;
            if (questionType.Equals("Single Choice"))
            {
                //Response.Write("This is a single choice question.");
                //display the options

                foreach (Option option in listOfOptions)
                {
                    RadioButton radioButton = new RadioButton();
                    radioButton.ID = option.MultipleChoiceOptionID.ToString();
                    radioButton.GroupName = "radioOptions"; //group the radio buttons
                    radioButton.Text = option.OptionText.ToString();
                    CurrentPlaceholderType = "RadioButton"; //set the current placeholder type
                    answerPlaceholder.Controls.Add(radioButton); //add the radio button to the placeholder
                }
            }
            else if (questionType.Equals("Multiple Choice"))
            {
                Response.Write("This is a multiple choice question.");

            }

            else if (questionType.Equals("TextInput"))
            {
                Response.Write("This is a Text Input question.");
                nextQuestionID = Int32.Parse(firstQuestion.NextQuestionForTextInput);

                //add a text box to the placeholder
            }
        }
    }
}