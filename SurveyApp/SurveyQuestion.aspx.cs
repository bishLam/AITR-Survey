using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class SurveyQuestion : System.Web.UI.Page
    {
        private int nextQuestionForTextInput;
        private String CurrentPlaceholderType;
        private Boolean hasQueue = false;
        private List<Int32> questionsQueue = new List<Int32>();

        protected void Page_Init(object sender, EventArgs e)
        {

            var sessionQueue = HttpContext.Current.Session["QuestionQueue"];
            var sessionHasQueue = HttpContext.Current.Session["hasQueue"];
            if (sessionQueue != null && sessionHasQueue != null)
            {
                questionsQueue = (List<Int32>)sessionQueue;
                hasQueue = (Boolean)sessionHasQueue;
            }
            
        }
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack) //this is the first time the page is loaded
            {
                LoadFirstQuestion();
                previousButton.Visible = false; //hide the previous button initially
            }
            else
            {
                Int32 currentQuestionID = 0;
                if (hasQueue)
                {
                    currentQuestionID = questionsQueue[0]; //if there is a queue, then we need to get the first question from the queue
                }
                else
                {
                    currentQuestionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
                }
                Question question = GetQuestionFromQuestionID(currentQuestionID);
                //now store the current value in the session and prompt them with the next question
                SetQuestionTextInAFormat(question);
                var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
                SetUpListOfOptions(listOfOptions, question);                   
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
                            DisplayQuestionAndOptions(nextQuestionID);
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
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                else if (CurrentPlaceholderType.Equals(AppConstants.PlaceholderTypeCheckBox))
                {
                    //loop through all the controls in the placeholder
                    Int32 ultimateQuestionID = 0;
                    for(int i = 0; i<answerPlaceholder.Controls.Count; i++)
                    {
                        //if the current question has branch, then we need to find the next question from the selected options
                        Int32 currentQuestionID = 0;
                        if (hasQueue)
                        {
                            currentQuestionID = questionsQueue[0]; //if there is a queue, then we need to get the first question from the queue
                        }
                        else
                        {
                            currentQuestionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
                        }
                        //get the question from the question ID
                        Question currentQuestion = GetQuestionFromQuestionID(currentQuestionID);

                        if (currentQuestion == null) return;

                        if (answerPlaceholder.Controls[i] is CheckBoxList cb)
                        {
                            List<ListItem> selectedItems = cb.Items.Cast<ListItem>().Where(n => n.Selected).ToList();


                            //only if the selected options is more than 1, we can have a queue of questions so
                            if (selectedItems.Count > 1)
                            {
                                List<Int32> listOfSelectedOptionsNextQuestion = new List<Int32>();


                                foreach (ListItem item in selectedItems)
                                {
                                    Int32 optionID = Int32.Parse(item.Value.ToString());
                                    Int32 nextQuestionIDFromOption = FindNextQuestionFromOptionID(optionID);
                                    listOfSelectedOptionsNextQuestion.Add(nextQuestionIDFromOption);
                                    Question nextQuestionFromOption = GetQuestionFromQuestionID(nextQuestionIDFromOption);


                                    ////this is the questions queue which we are supposed to iterate before going into the ultimate one.
                                    if (!questionsQueue.Contains(nextQuestionFromOption.QuestionID))
                                    {
                                        questionsQueue.Add(nextQuestionFromOption.QuestionID);
                                        if (questionsQueue.Count > 1)
                                        {
                                            hasQueue = true; //if there is more than one question in the queue, then we have a queue
                                        }
                                        else
                                        {
                                            hasQueue = false; //if there is only one question in the queue, then we don't have a queue
                                        }
                                    }
                                }

                                if(listOfSelectedOptionsNextQuestion.Distinct().Count() == 1) //this means all of the options lead to the same question and are same
                                {
                                    //now store the data into the session for later
                                    Int32 nextQuestionID = listOfSelectedOptionsNextQuestion.First();
                                    //System.Diagnostics.Debug.WriteLine(selectedOptionID);
                                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                                    HttpContext.Current.Session.Remove("QuestionQueue");
                                    HttpContext.Current.Session.Remove("hasQueue");
                                    DisplayQuestionAndOptions(nextQuestionID);
                                    hasQueue = false;
                                    return;
                                }

                                else
                                {
                                    hasQueue = true;
                                }


                            }
                            else
                            {
                                int nextQuestionID;
                                nextQuestionID = FindNextQuestionFromOptionID(Int32.Parse(selectedItems[0].Value));
                                HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                                DisplayQuestionAndOptions(nextQuestionID);
                                HttpContext.Current.Session.Remove("QuestionQueue");
                                HttpContext.Current.Session.Remove("hasQueue");
                                return;
                            }
                            
                            if (hasQueue)
                            {
                                //now we need to find options from each of this queue to find the next questionID
                                foreach (Int32 insideQuestion in questionsQueue)
                                {
                                    List<Option> insideOptions = GetAllOptionsFromQuestionID(insideQuestion);
                                    foreach (Option option in insideOptions)
                                    {
                                        if (questionsQueue.Contains(option.NextQuestionID))
                                        {
                                            ultimateQuestionID = option.NextQuestionID;
                                            HttpContext.Current.Session["ultimateQuestionID"] = ultimateQuestionID;
                                        }
                                    }
                                }
                            }
                                

                                //from the questionsQueue, until ultimate question is the only questionLeft, display those questions


                            if(questionsQueue.Count <= 1)
                            {
                                ultimateQuestionID = Int32.Parse(HttpContext.Current.Session["ultimateQuestionID"].ToString());
                                HttpContext.Current.Session["currentQuestionID"] = ultimateQuestionID.ToString();
                                DisplayQuestionAndOptions(ultimateQuestionID);
                                HttpContext.Current.Session.Remove("QuestionQueue");
                                HttpContext.Current.Session.Remove("hasQueue");
                                return;
                            }

                            else
                            {


                                questionsQueue.Remove(ultimateQuestionID);
                                var nextQuestionToDisplay = questionsQueue.First();
                                HttpContext.Current.Session["currentQuestionID"] = nextQuestionToDisplay.ToString();
                                DisplayQuestionAndOptions(nextQuestionToDisplay);
                                questionsQueue.Remove(nextQuestionToDisplay);
                                questionsQueue.Add(ultimateQuestionID);
                                return;



                                //for (Int32 j = 0; j < questionsQueue.Count; j++)
                                //{
                                //    ultimateQuestionID = Int32.Parse(HttpContext.Current.Session["ultimateQuestionID"].ToString());
                                //    if (!questionsQueue[j].Equals(ultimateQuestionID)) //not checking when the questions queue is 1 and only the last onwe
                                //    {
                                //        //System.Diagnostics.Debug.WriteLine(selectedOptionID);
                                //        Int32 nextQuestionID = questionsQueue[j];
                                //        HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                                //        DisplayQuestionAndOptions(nextQuestionID);
                                //        questionsQueue.Remove(nextQuestionID);
                                //        //before returning I need to store the other two queues in the session

                                //        HttpContext.Current.Session["QuestionQueue"] = questionsQueue;
                                //        HttpContext.Current.Session["hasQueue"] = hasQueue;

                                //    }
                                //    else if (questionsQueue[j].Equals(ultimateQuestionID) && questionsQueue.Count == 1)
                                //    {
                                //        Response.Write("This is the ultimagte Question");
                                //        HttpContext.Current.Session.Remove("QuestionQueue");
                                //        HttpContext.Current.Session.Remove("hasQueue");
                                //    }
                                //}
                            }
                                
                        }
                        return;
                    }
                }

            }

        }

        //protected void GetOptionFromOptionID(Int32 optionID)
        //{
        //    string _connectionString = GetConnectionString();
        //    Option option = new Option(); //default value if not found
        //    SqlConnection conn = new SqlConnection();
        //    conn.ConnectionString = _connectionString;
        //    conn.Open();

        //    //2.Prepare the consume instruction
        //    SqlCommand questionCommand = new SqlCommand("SELECT * FROM MultipleChoiceOption WHERE MultipleChoiceOptionID =" + optionID, conn); //command to get the questions

        //    //3. Consume
        //    SqlDataReader reader = questionCommand.ExecuteReader();
        //    reader.Read();
        //    nextQuestionID = Int32.Parse(reader["NextQuestionID"].ToString());
        //    conn.Close();
        //    return nextQuestionID;
        //}

        protected void DisplayQuestionAndOptions(Int32 questionID)
        {

            Question question = GetQuestionFromQuestionID(questionID);

            //now store the current value in the session and prompt them with the next question
            previousButton.Visible = true; //make the previous button visible
            Int32 currentQuestionID = question.QuestionID; //set the current question ID to the next question ID

            SetQuestionTextInAFormat(question);
            var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
            SetUpListOfOptions(listOfOptions, question);
        }

        protected Int32 FindNextQuestionFromOptionID(Int32 optionID)
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
            try
            {
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
            }
            catch(SqlException ex)
            {
                Label label = new Label();
                label.ID = "ErrorMessage";
                label.Text = "An error occured: " + ex.Message;
            }

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
                CheckBoxList cb = new CheckBoxList();
                cb.ID = "CheckBox";

                foreach(Option option in options)
                {
                    ListItem item = new ListItem();
                    item.Text = option.OptionText;
                    item.Value = option.MultipleChoiceOptionID.ToString();
                    cb.Items.Add(item);
                    //answerPlaceholder.Controls.Add(new LiteralControl("<br />")); //add a line break after each radio button
                }
                CurrentPlaceholderType = AppConstants.PlaceholderTypeCheckBox;
                answerPlaceholder.Controls.Add(cb);
                
                


                
            }

            else if (questionType.Equals(AppConstants.QuestionTypeTextInput))
            {          
                
                //add a text box to the placeholder
                CurrentPlaceholderType = AppConstants.PlaceholderTypeTextBox;
                TextBox textBox = new TextBox();
                textBox.ID = "TextBox1";              
                textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                //HttpContext.Current.Session["currentQuestionID"] = currentQuestionID.ToString();
                //textBox.TextMode = TextBoxMode.MultiLine;
                nextQuestionForTextInput = Int32.Parse(question.NextQuestionForTextInput);

                textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
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

                if (DBNull.Value == reader["MaxAnswerSelection"])
                {
                    firstQuestion.MaxSelection = 0; //default value if not set
                }
                else
                {
                    //if the value is not null, then parse it to int
                    firstQuestion.MaxSelection = Int32.Parse(reader["MaxAnswerSelection"].ToString());
                }                   
            }
            //currentQuestionID = firstQuestion.QuestionID; //set the current question ID to the first question ID
            HttpContext.Current.Session["currentQuestionID"] = firstQuestion.QuestionID.ToString(); //store the current question ID in the session
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