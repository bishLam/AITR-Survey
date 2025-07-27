using System;
using System.Collections;
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
            var sessionHasQueue = HttpContext.Current.Session["HasQueue"];
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
                currentQuestionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
                DisplayQuestionAndOptions(currentQuestionID);             
            }
        }

        protected void nextButton_Click(object sender, EventArgs e)
        {
            //1. Get the selected option
            if (answerPlaceholder.Controls.Count > 0) //meaning there is at least one control
            {


                //check if the current placeholder type is radio button, text box or check box

                RadioButtonList rbl = answerPlaceholder.FindControl("RadioButtonList") as RadioButtonList;
                CheckBoxList cb = answerPlaceholder.FindControl("CheckBoxList") as CheckBoxList;
                TextBox textbox = answerPlaceholder.FindControl("TextBox") as TextBox;

                if (rbl != null && cb == null && textbox == null)
                {
                   Int32 selectedOptionID =  Int32.Parse(rbl.SelectedItem.Value);
                    int nextQuestionID;
                    nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                    DisplayQuestionAndOptions(nextQuestionID);
                    return;
                }

                else if(rbl == null && cb == null && textbox != null)
                {
                    string answer = textbox.Text;

                    //now display the next question
                    //since we have already assigned the currentQuestionID when we initialise the text box, this is safe to do use it here   
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                else if (rbl == null && cb != null && textbox == null)
                {
                    //loop through all the controls in the placeholder
                    Int32 ultimateQuestionID = 0;
                    List<ListItem> selectedOptions = cb.Items.Cast<ListItem>().Where(n => n.Selected).ToList();

                    if (selectedOptions.Count < 1)
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "CheckBoxValidator";
                        validator.ErrorMessage = "Please select at least one option";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    else if (selectedOptions.Count == 1 && hasQueue == false)
                    {
                        //now store the data into the session for later
                        Int32 selectedOptionID = Int32.Parse(selectedOptions[0].Value);
                        //System.Diagnostics.Debug.WriteLine(selectedOptionID);
                        int nextQuestionID;
                        nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);
                        HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                        DisplayQuestionAndOptions(nextQuestionID);
                        return;
                    }

                    else if (selectedOptions.Count > 1 && hasQueue == false)
                    {
                        List<Int32> questionsQueue = new List<Int32>();
                        List<Int32> nextQuestionIDsFromSelectedOptions = new List<Int32>();
                        foreach (ListItem singleOption in selectedOptions)
                        {
                            Int32 singleSelectedOption = Int32.Parse(singleOption.Value.ToString());
                            Int32 nextQuestionIDFromOption = FindNextQuestionFromOptionID(singleSelectedOption);

                            if (!nextQuestionIDsFromSelectedOptions.Contains(nextQuestionIDFromOption))
                            {
                                nextQuestionIDsFromSelectedOptions.Add(nextQuestionIDFromOption);
                            }
                        }

                        if (nextQuestionIDsFromSelectedOptions.Count == 1)
                        {
                            Int32 nextQuestionID = Int32.Parse(nextQuestionIDsFromSelectedOptions[0].ToString());
                            HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                            DisplayQuestionAndOptions(nextQuestionID);
                            return;
                        }
                        if (nextQuestionIDsFromSelectedOptions.Count > 1)
                        {
                            //this means there needs to be a queue now
                            HttpContext.Current.Session["QuestionQueue"] = nextQuestionIDsFromSelectedOptions;

                            foreach (Int32 insideQuestion in nextQuestionIDsFromSelectedOptions)
                            {
                                List<Option> insideOptions = GetAllOptionsFromQuestionID(insideQuestion);
                                foreach (Option option in insideOptions)
                                {
                                    if (nextQuestionIDsFromSelectedOptions.Contains(option.NextQuestionID))
                                    {
                                        ultimateQuestionID = option.NextQuestionID;
                                        HttpContext.Current.Session["ultimateQuestionID"] = ultimateQuestionID;
                                    }
                                }
                            }

                            if (ultimateQuestionID == 0)
                            {
                                //now the next question to display wouldbe other questions than ultimate one
                                questionsQueue.Clear();
                                questionsQueue = nextQuestionIDsFromSelectedOptions;
                                Int32 nextQuestionID = questionsQueue.First(); //get the first question from the queue
                                HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                                DisplayQuestionAndOptions(nextQuestionID);
                                questionsQueue.Remove(nextQuestionID);
                                HttpContext.Current.Session["HasQueue"] = true; //set the hasQueue to true
                                hasQueue = true; //set the hasQueue to true
                                HttpContext.Current.Session["QuestionQueue"] = questionsQueue; //store the que
                                                                                               //stions queue in the session
                                return;
                            }
                            else
                            {
                                //now the next question to display wouldbe other questions than ultimate one
                                questionsQueue.Clear();
                                questionsQueue = nextQuestionIDsFromSelectedOptions;
                                questionsQueue.Remove(ultimateQuestionID);
                                Int32 nextQuestionID = questionsQueue.First(); //get the first question from the queue
                                HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                                DisplayQuestionAndOptions(nextQuestionID);
                                questionsQueue.Remove(nextQuestionID);
                                questionsQueue.Add(ultimateQuestionID); //add the ultimate question to the end of the queue
                                HttpContext.Current.Session["HasQueue"] = true; //set the hasQueue to true
                                hasQueue = true; //set the hasQueue to true
                                HttpContext.Current.Session["QuestionQueue"] = questionsQueue; //store the que

                            }
                            //stions queue in the session
                            return;
                        }

                    }

                    else if (hasQueue == true && questionsQueue.Count == 1)
                    {

                        Int32 nextQuestionID = questionsQueue.First();
                        //HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                        DisplayQuestionAndOptions(nextQuestionID);
                        HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                        HttpContext.Current.Session.Remove("QuestionQueue");
                        HttpContext.Current.Session.Remove("hasQueue");
                        HttpContext.Current.Session.Remove("ultimateQuestionID");
                        return;
                    }

                    else if (hasQueue == true && questionsQueue.Count > 1)
                    {

                        var utimateQuestionSessionID = HttpContext.Current.Session["ultimateQuestionID"];

                        if (utimateQuestionSessionID == null)
                        {
                            Int32 nextQuestionID = questionsQueue.First(); //get the first question from the queue
                            HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                            DisplayQuestionAndOptions(nextQuestionID);
                            questionsQueue.Remove(nextQuestionID);
                            HttpContext.Current.Session["HasQueue"] = true; //set the hasQueue to true
                            hasQueue = true; //set the hasQueue to true
                            HttpContext.Current.Session["QuestionQueue"] = questionsQueue; //store the que
                                                                                           //stions queue in the session
                            return;
                        }
                        else
                        {
                            ultimateQuestionID = (Int32)utimateQuestionSessionID;
                            questionsQueue.Remove(ultimateQuestionID);
                            Int32 nextQuestionID = questionsQueue.First(); //get the first question from the queue
                            HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                            DisplayQuestionAndOptions(nextQuestionID);
                            questionsQueue.Remove(nextQuestionID);
                            questionsQueue.Add(ultimateQuestionID); //add the ultimate question to the end of the queue
                            HttpContext.Current.Session["HasQueue"] = true; //set the hasQueue to true
                            hasQueue = true; //set the hasQueue to true
                            HttpContext.Current.Session["QuestionQueue"] = questionsQueue; //store the que
                                                                                           //stions queue in the session
                            return;
                        }


                    }


                }

            }

        }

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
                RadioButtonList rbl = new RadioButtonList();
                rbl.ID = "RadioButtonList";
                foreach (Option option in options)
                {
                    ListItem li = new ListItem();
                    li.Text = option.OptionText;
                    li.Value = option.MultipleChoiceOptionID.ToString();
                    rbl.Items.Add(li);
                }
                answerPlaceholder.Controls.Add(rbl);

                RequiredFieldValidator validator = new RequiredFieldValidator();
                validator.ID = "validator";
                validator.ControlToValidate = "RadioButtonList";
                validator.ErrorMessage = "Please select one option";
                answerPlaceholder.Controls.Add(validator);
            }

            else if (questionType.Equals(AppConstants.QuestionTypeMultipleChoice))
            {
                CheckBoxList cb = new CheckBoxList();
                cb.ID = "CheckBoxList";

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

                CustomValidator validator = new CustomValidator();
                validator.ID = "validator";
                //validator.ErrorMessage = "Please select at least one option";
                validator.ServerValidate += new ServerValidateEventHandler(ValidateCheckboxList);

                answerPlaceholder.Controls.Add(validator);

               
            }

            else if (questionType.Equals(AppConstants.QuestionTypeTextInput))
            {          
                
                //add a text box to the placeholder
                CurrentPlaceholderType = AppConstants.PlaceholderTypeTextBox;
                TextBox textBox = new TextBox();
                textBox.ID = "TextBox";              
                textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                //HttpContext.Current.Session["currentQuestionID"] = currentQuestionID.ToString();
                //textBox.TextMode = TextBoxMode.MultiLine;
                nextQuestionForTextInput = Int32.Parse(question.NextQuestionForTextInput);

                textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                RequiredFieldValidator validator = new RequiredFieldValidator();
                validator.ID = "validator";
                validator.ControlToValidate = "TextBox";
                validator.ErrorMessage = "Textbox cannot be empty";
                answerPlaceholder.Controls.Add(validator);


            }

            answerPlaceholder.Controls.Add(new LiteralControl("<br /> <br />"));
        }

        protected void ValidateCheckboxList(Object o, ServerValidateEventArgs e)
        {
            CheckBoxList cbl = (CheckBoxList)answerPlaceholder.FindControl("CheckBoxList");
            if (cbl != null && cbl.Items.Cast<ListItem>().Any(item => item.Selected))
            {
                e.IsValid = true;
            }
            else
            {
                e.IsValid = false;
            }
        }

        protected void LoadFirstQuestion()
        {
            //here connect the database

            //1. Open the connection string
            try
            {
                String _connectionString = GetConnectionString();

                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = _connectionString;
                conn.Open();

                //2.Prepare the consume instruction
                SqlCommand questionCommand = new SqlCommand("SELECT * FROM Question WHERE IsFirstQuestion='True'", conn); //command to get the questions

                //3. Consume
                SqlDataReader reader = questionCommand.ExecuteReader();


                //4. for the storage
                Question firstQuestion = new Question();
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

            catch (Exception ex)
            {
                Response.Redirect("../SurveyApp/SurveyApp.aspx");
                Label label = new Label();
                label.Text = "Something went wrong. Please see the msg below: " + ex.Message;
                answerPlaceholder.Controls.Clear();
                answerPlaceholder.Controls.Add(label);               
            }
        }
    }
}