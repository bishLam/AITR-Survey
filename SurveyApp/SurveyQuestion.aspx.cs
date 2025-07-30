using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        private List<Answer> answers = new List<Answer>();
       
        protected void Page_Init(object sender, EventArgs e)
        {
            var sessionQueue = HttpContext.Current.Session["QuestionQueue"];
            var sessionHasQueue = HttpContext.Current.Session["HasQueue"];
            var answerDictionarySession = HttpContext.Current.Session["answerList"];
            if(answerDictionarySession != null)
            {
                answers = (List<Answer>)HttpContext.Current.Session["answerList"];
            }
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
                TextBox dateTextbox = answerPlaceholder.FindControl("DateTextBox") as TextBox;
                TextBox emailTextbox = answerPlaceholder.FindControl("TextBoxEmail") as TextBox;

                if (rbl != null && cb == null && textbox == null && dateTextbox == null && emailTextbox == null)
                {
                   Int32 selectedOptionID =  Int32.Parse(rbl.SelectedItem.Value);


                    //add the answer and question to the session
                    Int32 questionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
                    

                    Answer answer = new Answer();
                    answer.QuestionID = questionID;
                    answer.SingleChoiceAnswerID = selectedOptionID;
                    answer.TextInputAnswer = null; //since this is a single choice question, the text input answer is null
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    int nextQuestionID;
                    nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);
                    if (nextQuestionID == 19) // this is the last question if this is the case
                    {
                        //redirect them to confirmation page
                        Response.Redirect("~/SurveyApp/AnswersConfirmation.aspx");
                    }
                    ;
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                    DisplayQuestionAndOptions(nextQuestionID);
                    return;
                }

                else if(rbl == null && cb == null && textbox != null && dateTextbox == null && emailTextbox == null)
                {
                    string tbxText = textbox.Text;


                    //add the answer and question to the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];

                    Answer answer = new Answer();
                    answer.QuestionID = Int32.Parse(questionID.ToString());
                    answer.SingleChoiceAnswerID = null;
                    answer.TextInputAnswer = tbxText;
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    if (nextQuestionForTextInput == 0)
                    {
                        Response.Redirect("../SurveyApp/AnswersConfirmation.aspx");
                        return;
                    }
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                else if (dateTextbox != null && rbl == null && cb == null && textbox == null && emailTextbox == null)
                {
                    Int32 questionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
                    //add the answer and question to the session
                    Answer answer = new Answer();
                    answer.QuestionID = questionID;
                    answer.SingleChoiceAnswerID = null;
                    answer.TextInputAnswer = dateTextbox.Text;
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                //in case of email
                else if (dateTextbox == null && rbl == null && cb == null && textbox == null && emailTextbox != null)
                {
                    string tbxText = emailTextbox.Text;
                    //add the answer and question to the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];

                    Answer answer = new Answer();
                    answer.QuestionID = Int32.Parse(questionID.ToString());
                    answer.SingleChoiceAnswerID = null;
                    answer.TextInputAnswer = tbxText;
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    if (nextQuestionForTextInput == 0)
                    {
                        Response.Redirect("../SurveyApp/AnswersConfirmation.aspx");
                        return;
                    }
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                else if (rbl == null && cb != null && textbox == null && dateTextbox == null && emailTextbox == null)
                {
                    //loop through all the controls in the placeholder
                    Int32 ultimateQuestionID = 0;
                    List<ListItem> selectedOptions = cb.Items.Cast<ListItem>().Where(n => n.Selected).ToList();




                    //first we need to validate the maximun or minimum selection
                    Question currentQuestion = GetQuestionFromQuestionID(Int32.Parse(HttpContext.Current.Session["currentQuestionID"].ToString()));
                    if (currentQuestion.MaxSelection > 0 && selectedOptions.Count > currentQuestion.MaxSelection)
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "CheckBoxValidator";
                        validator.ErrorMessage = "Please select maximun of " + currentQuestion.MaxSelection + " options";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    else if (currentQuestion.MaxSelection < 0 && selectedOptions.Count < Math.Abs(currentQuestion.MaxSelection))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "CheckBoxValidator";
                        validator.ErrorMessage = "Please select minimum of " + Math.Abs(currentQuestion.MaxSelection) + " options";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    //check if the selected options are less than 1
                    foreach (ListItem item in selectedOptions)
                    {
                        //System.Diagnostics.Debug.WriteLine(item.Value);
                        Int32 questionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
                        //add the answer and question to the session
                        Answer answer = new Answer();
                        answer.QuestionID = questionID;
                        answer.SingleChoiceAnswerID = Int32.Parse(item.Value);
                        answer.TextInputAnswer = null; //since this is a single choice question, the text input answer is null
                        answers.Add(answer);
                        HttpContext.Current.Session["answerList"] = answers;
                    }

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


                        Int32 questionID = Int32.Parse(HttpContext.Current.Session["currentQuestionID"] as String);
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
                            HttpContext.Current.Session["QuestionQueue"] = questionsQueue;
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

            if(questionID == 10)
            {
                Response.Redirect(AppConstants.redirectToRegisterRespondents);
                return;
            }
            SetQuestionTextInAFormat(question);
            var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
            SetUpListOfOptions(listOfOptions, question);
        }

        protected Int32 FindNextQuestionFromOptionID(Int32 optionID)
        {
            try
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

            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return 0;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return 0;
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return 0;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return 0;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return 0;
            }
 
        }

        public string GetConnectionString()
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

        public Question GetQuestionFromQuestionID(Int32 questionID)
        {
            Question emptyQuestion = new Question();
            try
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
                    if (maxAnswerSelection == DBNull.Value)
                    {
                        question.MaxSelection = 0;
                    }
                    else
                    {
                        question.MaxSelection = Int32.Parse(reader["MaxAnswerSelection"].ToString());
                    }

                }
                conn.Close();
                return question;
            }

            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return emptyQuestion;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return emptyQuestion;
            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return emptyQuestion;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return emptyQuestion;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return emptyQuestion;
            }
        }

        protected void SetQuestionTextInAFormat(Question question)
        {
            Int32 maximumSelection = question.MaxSelection;

            if(maximumSelection == 1 || maximumSelection == 0)
            {
                QuestionLabel.Text = "(Question " + question.QuestionID + ")  " + question.QuestionText; //display the question text
            }
            else if(maximumSelection < 0)
            {
                QuestionLabel.Text = "(Question " + question.QuestionID + ")  " + question.QuestionText + "     (minimum selection: " + Math.Abs(maximumSelection) + " )"; //display the question text with minimmum selection option
            }
            else if (maximumSelection > 1)
            {
                QuestionLabel.Text = "(Question " + question.QuestionID + ")  " + question.QuestionText + "     (maximum selection: " + maximumSelection + ")"; //display the question text with minimmum selection option
            }
            else
            {
                QuestionLabel.Text = "Something unexpected happened. Please try again";
                return;
            }  
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
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());

            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
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
                validator.Display = ValidatorDisplay.Dynamic;
                validator.ErrorMessage = "Please select one option";
                answerPlaceholder.Controls.Add(validator);
            }

            else if (questionType == AppConstants.QuestionTypeMultipleChoice)
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
                answerPlaceholder.Controls.Add(cb);

                CustomValidator validator = new CustomValidator();
                validator.ID = "validator";
                validator.Display = ValidatorDisplay.Dynamic;
                //validator.ErrorMessage = "Please select at least one option";
                validator.ServerValidate += new ServerValidateEventHandler(ValidateCheckboxList);

                answerPlaceholder.Controls.Add(validator);

               
            }

            else if (questionType == AppConstants.QuestionTypeTextInput)
            {          
                
                //add a text box to the placeholder
                TextBox textBox = new TextBox();
                textBox.ID = "TextBox";              
                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                nextQuestionForTextInput = Int32.Parse(question.NextQuestionForTextInput);

                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                RequiredFieldValidator validator = new RequiredFieldValidator();
                validator.ID = "validator";
                validator.ControlToValidate = "TextBox";
                validator.ErrorMessage = "Textbox cannot be empty";
                validator.Display = ValidatorDisplay.Dynamic;
                answerPlaceholder.Controls.Add(validator);
            }

            else if (questionType == AppConstants.QuestionTypeDate)
            {
                TextBox textBox = new TextBox();
                textBox.ID = "DateTextBox";
                textBox.TextMode = TextBoxMode.Date; //set the text box to date mode
                textBox.Attributes.Add("placeholder", "Select a date");
                nextQuestionForTextInput = Int32.Parse(question.NextQuestionForTextInput);
                answerPlaceholder.Controls.Add(textBox);

                RequiredFieldValidator validator = new RequiredFieldValidator();
                validator.ID = "DateValidator";
                validator.ControlToValidate = "DateTextBox";
                validator.ErrorMessage = "Please select a date";
                validator.Display = ValidatorDisplay.Dynamic;
                answerPlaceholder.Controls.Add(validator);
            }

            else if(questionType == AppConstants.QuestionTypeFinish || question.NextQuestionForTextInput == "0")
            {
                Response.Redirect("../SurveyApp/AnswersConfirmation.aspx");
            }

            else if(questionType == AppConstants.QuestionTypeTextInputEmail)
            {
                //add a text box to the placeholder
                TextBox textBox = new TextBox();
                textBox.ID = "TextBoxEmail";
                textBox.TextMode = TextBoxMode.Email;
                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                nextQuestionForTextInput = Int32.Parse(question.NextQuestionForTextInput);

                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                RequiredFieldValidator validator = new RequiredFieldValidator();
                validator.ID = "validator";
                validator.ControlToValidate = "TextBoxEmail";
                validator.ErrorMessage = "Email cannot be empty";
                validator.Display = ValidatorDisplay.Dynamic;
                answerPlaceholder.Controls.Add(validator);

                RegularExpressionValidator regularExpressionValidator = new RegularExpressionValidator();
                regularExpressionValidator.ID = "validateEmail";
                regularExpressionValidator.ControlToValidate = "TextBoxEmail";
                regularExpressionValidator.ErrorMessage = "Please enter a valid email";
                regularExpressionValidator.ValidationExpression = AppConstants.EmailValidatorRegex;
                regularExpressionValidator.Display = ValidatorDisplay.Dynamic;
                answerPlaceholder.Controls.Add(regularExpressionValidator);
            }
                answerPlaceholder.Controls.Add(new LiteralControl("<br />"));
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
                Response.Redirect("../SurveyApp/SurveyApp.aspx");
                Label label = new Label();
                label.Text = "Something went wrong. Please see the msg below: " + ex.Message;
                answerPlaceholder.Controls.Clear();
                answerPlaceholder.Controls.Add(label);
            }
        }
    }
}