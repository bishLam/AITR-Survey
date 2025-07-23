using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class SurveyQuestion : System.Web.UI.Page
    {
        private Int32 currentQuestionID;
        private int nextQuestionForTextInput;
        private String CurrentPlaceholderType;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack) //this is the first time the page is loaded
            {
                LoadFirstQuestion();
                previousButton.Visible = false; //hide the previous button initially
            }
            else
            {
                currentQuestionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);

                if (currentQuestionID > 0)  
                {
                    //LoadNextQuestion();
                        Question question = GetQuestionFromQuestionID(currentQuestionID);
                        //now store the current value in the session and prompt them with the next question
                        SetQuestionTextInAFormat(question);
                        var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
                        SetUpListOfOptions(listOfOptions, question);
                    
                }
                else
                {
                    
                }
            }
        }

        protected void nextButton_Click(object sender, EventArgs e)
        {
            //1. Get the selected option
            if (answerPlaceholder.Controls.Count > 0) //meaning there is at least one control
            {
                
                if (CurrentPlaceholderType.Equals(AppConstants.PlaceholderTypeRadioButton))
                {
                    //if the current placeholder type is radio button, then we need to find the selected option
                    for (int i = 0; i < answerPlaceholder.Controls.Count; i++)
                    {
                        if (answerPlaceholder.Controls[i] is RadioButton rb && rb.Checked)
                        {
                            //now store the data into the session for later
                            Int32 selectedOptionID = Int32.Parse(rb.ID);
                            //System.Diagnostics.Debug.WriteLine(selectedOptionID);
                            int nextQuestionID;
                            nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);
                            HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                            LoadNextQuestion(nextQuestionID);
                            return;
                        }
                    }
                }
                else if (CurrentPlaceholderType.Equals(AppConstants.PlaceholderTypeTextBox))
                {
                    // if the current placeholder is a text box, we need to get the value of text box
                    TextBox textbox = FindControl("TextBox1") as TextBox;
                    string answer = textbox.Text;

                    //now display the next question
                    //since we have already assigned the currentQuestionID when we initialise the text box, this is safe to do use it here   
                    currentQuestionID = nextQuestionForTextInput;
                    LoadNextQuestion(currentQuestionID);
                    return;
                }
            }

        }

        protected void LoadNextQuestion(Int32 nextQuestionID)
        {

            Question question = GetQuestionFromQuestionID(nextQuestionID);

            //now store the current value in the session and prompt them with the next question
            previousButton.Visible = true; //make the previous button visible
            currentQuestionID = question.QuestionID; //set the current question ID to the next question ID

            SetQuestionTextInAFormat(question);
            var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
            SetUpListOfOptions(listOfOptions, question);
        }

        protected Int32 FindNextQuestionFromOptionID(int optionID)
        {
            string _connectionString = GetConnectionString();
            int nextQuestionID = 0; //default value if not found
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
                return ""; //Use the different connection string here
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
                var maxAnswerSelection = reader["MaxAnswerSelection"];
                //WRONG
                if (DBNull.Value == null)
                {
                    question.MaxSelection = Int32.Parse(reader["MaxAnswerSelection"].ToString());
                }
                else
                {
                    question.MaxSelection = 0; //default value if not set
                }
                
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

        protected void SetUpListOfOptions(List<Option> options, Question question)
        {
            string questionType = question.QuestionType;
            answerPlaceholder.Controls.Clear(); //clear existing controls
            if (questionType == AppConstants.QuestionTypeSingleChoice)
            {
                //Response.Write("This is a Single choice question.");
                //populate the placeholder with radio button options

                //get the placeholder first and clear the existing ones              
                foreach(Option option in options)
                {
                    RadioButton radioButton = new RadioButton();
                    radioButton.ID = option.MultipleChoiceOptionID.ToString();
                    radioButton.GroupName = "options";
                    radioButton.Text = option.OptionText;
                    CurrentPlaceholderType = AppConstants.PlaceholderTypeRadioButton;
                    answerPlaceholder.Controls.Add(radioButton);
                    answerPlaceholder.Controls.Add(new LiteralControl("<br />")); //add a line break after each radio button
                }


            }
            else if (questionType.Equals(AppConstants.QuestionTypeMultipleChoice))
            {
                //Response.Write("This is a multiple choice question.");
                
            }

            else if (questionType.Equals(AppConstants.QuestionTypeTextInput))
            {          
                
                //add a text box to the placeholder
                CurrentPlaceholderType = AppConstants.PlaceholderTypeTextBox;
                TextBox textBox = new TextBox();
                textBox.ID = "TextBox1";   
                textBox.Text = currentQuestionID.ToString();
                //HttpContext.Current.Session["currentQuestionID"] = currentQuestionID.ToString();
                //textBox.TextMode = TextBoxMode.MultiLine;
                nextQuestionForTextInput = Int32.Parse(question.NextQuestionForTextInput);
                answerPlaceholder.Controls.Add(textBox);

            }

            answerPlaceholder.Controls.Add(new LiteralControl("<br /> <br />"));
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
            currentQuestionID = firstQuestion.QuestionID; //set the current question ID to the first question ID
            HttpContext.Current.Session["currentQuestionID"] = currentQuestionID.ToString(); //store the current question ID in the session
            reader.Close();
            //at this point we would have the first question. Now lets take the options
            List<Option> listOfOptions = GetAllOptionsFromQuestionID(firstQuestion.QuestionID);
            conn.Close();
            //Now we have the question and options, lets display them
            SetQuestionTextInAFormat(firstQuestion); //display the question text
            SetUpListOfOptions(listOfOptions, firstQuestion);
            
        }
    }
}