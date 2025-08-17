using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class SurveyQuestion : System.Web.UI.Page
    {
        private int nextQuestionForTextInput;
        private Boolean hasQueue = false;
        private List<Int32> questionsQueue = new List<Int32>();
        private List<Answer> answers = new List<Answer>();

        protected void Page_Init(object sender, EventArgs e)
        {
            var sessionQueue = HttpContext.Current.Session["QuestionQueue"];
            var sessionHasQueue = HttpContext.Current.Session["HasQueue"];
            var answerDictionarySession = HttpContext.Current.Session["answerList"];
            if (answerDictionarySession != null)
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
                //previousButton.Visible = false; //hide the previous button initially
            }
            else
            {
                //since the placeholders and everything is lost during postback, this is essential;
                Int32 currentQuestionID = 0;
                if (!Int32.TryParse(HttpContext.Current.Session["currentQuestionID"] as String, out currentQuestionID)) return;
                DisplayQuestionAndOptions(currentQuestionID);
            }
        }

        protected void nextButton_Click(object sender, EventArgs e)
        {


            //Step 1 would be to find the controls in our placeholder and display it in the screen
            if (answerPlaceholder.Controls.Count > 0) //meaning there is at least one control
            {
                // The following will try to access relevant placeholder controls. Then we can check if any placeholder is null or not to retrieve the user input
                RadioButtonList rbl = answerPlaceholder.FindControl("RadioButtonList") as RadioButtonList;
                CheckBoxList cb = answerPlaceholder.FindControl("CheckBoxList") as CheckBoxList;
                TextBox textbox = answerPlaceholder.FindControl("TextBox") as TextBox;
                TextBox dateTextbox = answerPlaceholder.FindControl("DateTextBox") as TextBox;
                TextBox emailTextbox = answerPlaceholder.FindControl("TextBoxEmail") as TextBox;
                TextBox suburbTextbox = answerPlaceholder.FindControl("TextBoxSuburb") as TextBox;
                TextBox postCodeTextbox = answerPlaceholder.FindControl("TextBoxPostCode") as TextBox;

                // In case when radio button list is not null and other controls are. This applies to single choice questions in the database
                if (rbl != null && cb == null && textbox == null && dateTextbox == null && emailTextbox == null && suburbTextbox == null && postCodeTextbox == null)
                {
                    // we get the only selected value in the radio button list, get the current question ID from the session
                    //we need to have the validators here to validate the user input 
                    if (rbl.SelectedItem == null)
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "RadioButtonListValidator";
                        validator.ErrorMessage = "Please select an option";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    //get the selected option id and current question id and convert it into integer 
                    Int32 selectedOptionID = convertStringToInt(rbl.SelectedItem.Value);
                    Int32 questionID = convertStringToInt(HttpContext.Current.Session["currentQuestionID"] as String);

                    // we will initialise the Answer object to store this answer and questions so that we can store this in the session to later push it in the database
                    Answer answer = new Answer();
                    answer.QuestionID = questionID;
                    answer.SingleChoiceAnswerID = selectedOptionID;
                    answer.TextInputAnswer = null; //since this is a single choice question, the text input answer is null
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    //Now from the selected answer, we try to find the next question that will follow the question
                    int nextQuestionID;
                    nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);
                    if (nextQuestionID == 19) // this is the last question if this is the case
                    {
                        //redirect them to confirmation page instead
                        Response.Redirect("~/SurveyApp/AnswersConfirmation.aspx");
                    }

                    //we store this next question as current question in the session so our page load after postback will be able to get this question ID
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                    DisplayQuestionAndOptions(nextQuestionID);
                    return;
                }

                // In case when normal text box is not null and other controls are. This applies to text boxes type in the db which can accept anything as the input
                else if (rbl == null && cb == null && textbox != null && dateTextbox == null && emailTextbox == null && suburbTextbox == null && postCodeTextbox == null)
                {

                    //we need to have the validators here to validate the user input 
                    // Validate that the text box is not empty
                    if (string.IsNullOrWhiteSpace(textbox.Text))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "TextBoxValidator";
                        validator.ErrorMessage = "Textbox cannot be empty";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    string tbxText = textbox.Text; //get the user input 
                    //get the question ID from the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];

                    //intialise answer object and store the current answer in the session
                    Answer answer = new Answer();
                    Int32 tempQID;
                    Int32.TryParse(questionID.ToString(), out tempQID);
                    answer.QuestionID = tempQID;
                    answer.SingleChoiceAnswerID = null;
                    answer.TextInputAnswer = tbxText;
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    if (nextQuestionForTextInput == 0)  // This means the end of the questions
                    {
                        Response.Redirect("../SurveyApp/AnswersConfirmation.aspx");
                        return;
                    }
                    //we store this next question as current question in the session so our page load after postback will be able to get this question ID
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                // In case when date text box is not null and other controls are. This applies to text boxes type in the db which can accept only dates as the input
                else if (dateTextbox != null && rbl == null && cb == null && textbox == null && emailTextbox == null && suburbTextbox == null && postCodeTextbox == null)
                {
                    // Validate that the date is not empty
                    if (string.IsNullOrWhiteSpace(dateTextbox.Text))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "DateTextBoxValidator";
                        validator.ErrorMessage = "Please select a date";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    // Validate that the input is a valid date
                    DateTime parsedDate;
                    if (!DateTime.TryParse(dateTextbox.Text, out parsedDate))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "DateTextBoxValidator";
                        validator.ErrorMessage = "Invalid date format";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    Int32 questionID = convertStringToInt(HttpContext.Current.Session["currentQuestionID"] as String);
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

                // In case when email text box is not null and other controls are. This applies to text boxes type in the db which can accept only emails as the input
                else if (dateTextbox == null && rbl == null && cb == null && textbox == null && emailTextbox != null && suburbTextbox == null && postCodeTextbox == null)
                {


                    string tbxText = emailTextbox.Text;
                    //get the current question from the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];

                    //add the answer and question to the session
                    Answer answer = new Answer();
                    answer.QuestionID = convertStringToInt(questionID.ToString());
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

                // In case when suburb text box is not null and other controls are. This applies to text boxes type in the db which can accept only characters as the input like in suburbs

                else if (dateTextbox == null && rbl == null && cb == null && textbox == null && emailTextbox == null && suburbTextbox != null && postCodeTextbox == null)
                {
                    string tbxText = suburbTextbox.Text;

                    // Validate that the email is not empty
                    if (string.IsNullOrWhiteSpace(tbxText))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "SuburbValidator";
                        validator.ErrorMessage = "Suburb cannot be empty";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    if (!validateSuburb(tbxText)) 
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "SuburbValidator";
                        validator.ErrorMessage = "Suburb is not valid";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    };

                    //get the current question from the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];

                    //add the answer and question to the session
                    Answer answer = new Answer();
                    answer.QuestionID = convertStringToInt(questionID.ToString());
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
                // In case when email text box is not null and other controls are. This applies to text boxes type in the db which can accept only emails as the input
                else if (dateTextbox == null && rbl == null && cb == null && textbox == null && emailTextbox != null && suburbTextbox == null && postCodeTextbox == null)
                {
                    string tbxText = emailTextbox.Text;

                    // Validate that the email is not empty
                    if (string.IsNullOrWhiteSpace(tbxText))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "EmailValidator";
                        validator.ErrorMessage = "Email cannot be empty";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    // Validate email format using AppConstants email regex
                    System.Text.RegularExpressions.Regex emailRegex = new System.Text.RegularExpressions.Regex(AppConstants.EmailValidatorRegex);
                    if (!emailRegex.IsMatch(tbxText))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "EmailFormatValidator";
                        validator.ErrorMessage = "Please enter a valid email address";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    //get the current question from the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];

                    //add the answer and question to the session
                    Answer answer = new Answer();
                    answer.QuestionID = convertStringToInt(questionID.ToString());
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


                // In case when postcode text box is not null and other controls are. This applies to text boxes type in the db which can accept only numbers as the input such as postcodes
                else if (dateTextbox == null && rbl == null && cb == null && textbox == null && emailTextbox == null && suburbTextbox == null && postCodeTextbox != null)
                {
                    string tbxText = postCodeTextbox.Text.Trim();

                    Int32 temp = 0;
                    if(!Int32.TryParse(tbxText, out temp))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "PostcodeValidator";
                        validator.ErrorMessage = "Postcode should be numbers";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }
                    // validating if the postcode does not contain characters and is between 4 to 6 
                    if (!validatePostcode(temp))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "PostcodeValidator";
                        validator.ErrorMessage = "Postcode is not valid";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    };
                    //add the answer and question to the session
                    var questionID = HttpContext.Current.Session["currentQuestionID"];
                    Answer answer = new Answer();
                    answer.QuestionID = convertStringToInt(questionID.ToString());
                    answer.SingleChoiceAnswerID = null;
                    answer.TextInputAnswer = tbxText;
                    answers.Add(answer);
                    HttpContext.Current.Session["answerList"] = answers;

                    //next question for text input is 0, meaning there is no next question in queue and it is end of the survey
                    if (nextQuestionForTextInput == 0)
                    {
                        Response.Redirect("../SurveyApp/AnswersConfirmation.aspx");
                        return;
                    }
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionForTextInput.ToString();
                    DisplayQuestionAndOptions(nextQuestionForTextInput);
                    return;
                }

                // In case when multiplechoice text box is not null and other controls are. This applies to multiple choice type in the db
                else if (rbl == null && cb != null && textbox == null && dateTextbox == null && emailTextbox == null && suburbTextbox == null && postCodeTextbox == null)
                {


                    Int32 ultimateQuestionID = 0;
                    List<ListItem> selectedOptions = cb.Items.Cast<ListItem>().Where(n => n.Selected).ToList();

                    //first we need to validate the maximun or minimum selection
                    Question currentQuestion = GetQuestionFromQuestionID(convertStringToInt(HttpContext.Current.Session["currentQuestionID"].ToString()));

                    //In case user selects more than maximum allowed options
                    if (currentQuestion.MaxSelection > 0 && selectedOptions.Count > currentQuestion.MaxSelection)
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "CheckBoxValidator";
                        validator.ErrorMessage = "Please select maximun of " + currentQuestion.MaxSelection + " options";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }

                    //In case user selects less than minimum options
                    else if (currentQuestion.MaxSelection < 0 && selectedOptions.Count < Math.Abs(currentQuestion.MaxSelection))
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "CheckBoxValidator";
                        validator.ErrorMessage = "Please select minimum of " + Math.Abs(currentQuestion.MaxSelection) + " options";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }


                    // store all the selected options in the session as answers to store it in the database
                    foreach (ListItem item in selectedOptions)
                    {
                        //System.Diagnostics.Debug.WriteLine(item.Value);
                        Int32 questionID = convertStringToInt(HttpContext.Current.Session["currentQuestionID"] as String);
                        //add the answer and question to the session
                        Answer answer = new Answer();
                        answer.QuestionID = questionID;
                        answer.SingleChoiceAnswerID = convertStringToInt(item.Value);
                        answer.TextInputAnswer = null; //since this is a single choice question, the text input answer is null
                        answers.Add(answer);
                        HttpContext.Current.Session["answerList"] = answers;
                    }

                    //check if the selected options are less than 1
                    if (selectedOptions.Count < 1)
                    {
                        CustomValidator validator = new CustomValidator();
                        validator.ID = "CheckBoxValidator";
                        validator.ErrorMessage = "Please select at least one option";
                        validator.IsValid = false;
                        answerPlaceholder.Controls.Add(validator);
                        return;
                    }


                    //if the selected options is only one and there is no queue, we display the next question based on the selected answer
                    else if (selectedOptions.Count == 1 && hasQueue == false)
                    {
                        //now store the data into the session for later
                        Int32 selectedOptionID = convertStringToInt(selectedOptions[0].Value);
                        //System.Diagnostics.Debug.WriteLine(selectedOptionID);


                        Int32 questionID = convertStringToInt(HttpContext.Current.Session["currentQuestionID"] as String);
                        int nextQuestionID;
                        nextQuestionID = FindNextQuestionFromOptionID(selectedOptionID);
                        HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                        DisplayQuestionAndOptions(nextQuestionID);
                        return;
                    }

                    // if the selected options are more than 1 and there is no queue yet, we start the queue and add the questions to the queue
                    else if (selectedOptions.Count > 1 && hasQueue == false)
                    {
                        List<Int32> questionsQueue = new List<Int32>();
                        List<Int32> nextQuestionIDsFromSelectedOptions = new List<Int32>();
                        foreach (ListItem singleOption in selectedOptions)
                        {
                            Int32 singleSelectedOption = convertStringToInt(singleOption.Value.ToString());
                            Int32 nextQuestionIDFromOption = FindNextQuestionFromOptionID(singleSelectedOption);

                            if (!nextQuestionIDsFromSelectedOptions.Contains(nextQuestionIDFromOption))
                            {
                                nextQuestionIDsFromSelectedOptions.Add(nextQuestionIDFromOption);
                            }
                        }



                        if (nextQuestionIDsFromSelectedOptions.Count == 1)
                        {
                            Int32 nextQuestionID = convertStringToInt(nextQuestionIDsFromSelectedOptions[0].ToString());
                            HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                            DisplayQuestionAndOptions(nextQuestionID);
                            return;
                        }

                        // To determine ultimate question which will be our last question in the queue before going into next question

                        // ULTIMATE QUESTION LOGIC
                        // Main Question --> Option A --> Question 1 --> Options --> Ultimate Question
                        // Main Question --> Option B --> Question 2 --> Options --> Ultimate Question
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

                            //if no ultimate question id's are found, we go into this logic 
                            if (ultimateQuestionID == 0)
                            {
                                questionsQueue.Clear();
                                questionsQueue = nextQuestionIDsFromSelectedOptions;
                                Int32 nextQuestionID = questionsQueue.First(); //get the first question from the queue
                                HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                                DisplayQuestionAndOptions(nextQuestionID);
                                questionsQueue.Remove(nextQuestionID);
                                HttpContext.Current.Session["HasQueue"] = true; //set the hasQueue to true
                                hasQueue = true; //set the hasQueue to true
                                HttpContext.Current.Session["QuestionQueue"] = questionsQueue; //store the questions queue in the session
                                return;
                            }

                            //else if ultimate question is found
                            else
                            {
                                //now the next question to display wouldbe  questions other than ultimate one
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

                    // if there is queue but there is only one question left, this will be our ultimate/last question from the queue
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

                    // if there is queue and there is more than one question left in the queue, we display a question, remove it from the queue and update the queue list
                    else if (hasQueue == true && questionsQueue.Count > 1)
                    {

                        //get the ultimate question ID from the session
                        var utimateQuestionSessionID = HttpContext.Current.Session["ultimateQuestionID"];

                        //if there is no ultimate questionn display the other question from the queue without worrying about the ultimate question
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

                        //when there is ultimate question, we should display that at last
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

        /// <summary>
        /// Displays the specified question and its associated options to the user.
        /// </summary>
        /// <remarks>This method retrieves the question and its options based on the provided <paramref
        /// name="questionID"/> and updates the UI to display them. If the <paramref name="questionID"/> is 10, the user
        /// is redirected to the registration page.</remarks>
        /// <param name="questionID">The unique identifier of the question to be displayed.</param>
        protected void DisplayQuestionAndOptions(Int32 questionID)
        {
            Question question = GetQuestionFromQuestionID(questionID);
            //now store the current value in the session and prompt them with the next question
            //previousButton.Visible = true; //make the previous button visible
            Int32 currentQuestionID = question.QuestionID; //set the current question ID to the next question ID

            if (questionID == 10)
            {
                Response.Redirect(AppConstants.redirectToRegisterRespondents);
                return;
            }
            SetQuestionTextInAFormat(question);
            var listOfOptions = GetAllOptionsFromQuestionID(question.QuestionID);
            SetUpListOfOptions(listOfOptions, question);
        }

        /// <summary>
        /// Retrieves the ID of the next question associated with the specified option ID.
        /// </summary>
        /// <remarks>This method queries the database to find the next question ID associated with a given
        /// multiple-choice option. If an error occurs during the operation, the method returns 0 and logs the
        /// error.</remarks>
        /// <param name="optionID">The ID of the multiple-choice option for which the next question ID is to be retrieved.</param>
        /// <returns>The ID of the next question if found; otherwise, 0.</returns>
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
                nextQuestionID = convertStringToInt(reader["NextQuestionID"].ToString());
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

        /// <summary>
        /// Retrieves the connection string based on the current configuration.
        /// </summary>
        /// <remarks>This method checks the application's configuration to determine which connection
        /// string to use.  Ensure that the configuration key "DevelopmentConnectionString" is properly set in the
        /// application's configuration file.</remarks>
        /// <returns>A string representing the connection string. Returns an empty string if the configuration does not match the
        /// expected value.</returns>
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

        /// <summary>
        /// Retrieves a <see cref="Question"/> object based on the specified question ID.
        /// </summary>
        /// <remarks>This method connects to the database to retrieve the question details. Ensure that
        /// the database  connection string is correctly configured and that the database is accessible. The method
        /// handles  exceptions by returning an empty <see cref="Question"/> object and logging the error.</remarks>
        /// <param name="questionID">The unique identifier of the question to retrieve.</param>
        /// <returns>A <see cref="Question"/> object containing the details of the question with the specified ID.  If the
        /// question is not found or an error occurs, an empty <see cref="Question"/> object is returned.</returns>
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
                    question.QuestionID = convertStringToInt(reader["QuestionID"].ToString());
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
                        question.MaxSelection = convertStringToInt(reader["MaxAnswerSelection"].ToString());
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

        /// <summary>
        /// Sets the formatted question text in the associated label based on the question's properties.
        /// </summary>
        /// <remarks>The method formats the question text to include additional information about
        /// selection constraints: - If the maximum selection is 1 or 0, only the question text is displayed. - If the
        /// maximum selection is negative, the absolute value is treated as the minimum selection, and this is included
        /// in the text. - If the maximum selection is greater than 1, it is included in the text as the maximum
        /// selection. If an unexpected condition occurs, a default error message is displayed.</remarks>
        /// <param name="question">The <see cref="Question"/> object containing the question text, ID, and selection constraints.</param>
        protected void SetQuestionTextInAFormat(Question question)
        {
            Int32 maximumSelection = question.MaxSelection;

            if (maximumSelection == 1 || maximumSelection == 0)
            {
                QuestionLabel.Text = "(Question " + question.QuestionID + ")  " + question.QuestionText; //display the question text
            }
            else if (maximumSelection < 0)
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

        /// <summary>
        /// Retrieves all multiple-choice options associated with a specific question ID.
        /// </summary>
        /// <remarks>This method establishes a connection to the database, executes a query to retrieve
        /// all options associated with the specified question ID, and returns the results as a list of <see
        /// cref="Option"/> objects. Ensure that the database connection string is correctly configured before calling
        /// this method.</remarks>
        /// <param name="questionID">The unique identifier of the question for which to retrieve the associated options.</param>
        /// <returns>A list of <see cref="Option"/> objects representing the multiple-choice options for the specified question.
        /// Returns an empty list if no options are found.</returns>
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
                    option.MultipleChoiceOptionID = convertStringToInt(optionsReader["MultipleChoiceOptionID"].ToString());
                    option.QuestionID = convertStringToInt(optionsReader["QuestionID"].ToString());
                    option.NextQuestionID = convertStringToInt(optionsReader["NextQuestionID"].ToString());
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

        /// <summary>
        /// Configures the UI controls for a question based on its type and associated options.
        /// </summary>
        /// <remarks>This method dynamically sets up the appropriate input controls (e.g., radio buttons,
        /// checkboxes, text boxes) in the <c>answerPlaceholder</c> based on the type of the question. It also adds
        /// necessary validators to ensure valid user input.  Supported question types include: <list type="bullet">
        /// <item><description>Single choice: Displays a list of radio buttons.</description></item>
        /// <item><description>Multiple choice: Displays a list of checkboxes.</description></item>
        /// <item><description>Text input: Displays a text box for free-form input.</description></item>
        /// <item><description>Date input: Displays a text box for date selection.</description></item>
        /// <item><description>Email input: Displays a text box for email input with validation.</description></item>
        /// <item><description>Suburb and postcode input: Displays text boxes for specific input
        /// types.</description></item> </list> If the question type indicates the end of the survey, the user is
        /// redirected to the confirmation page.</remarks>
        /// <param name="options">A list of <see cref="Option"/> objects representing the available choices for the question.</param>
        /// <param name="question">The <see cref="Question"/> object containing details about the question, including its type and any
        /// additional metadata.</param>
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

                //RequiredFieldValidator validator = new RequiredFieldValidator();
                //validator.ID = "validator";
                //validator.ControlToValidate = "RadioButtonList";
                //validator.Display = ValidatorDisplay.Dynamic;
                //validator.ErrorMessage = "Please select one option";
                //answerPlaceholder.Controls.Add(validator);
            }

            else if (questionType == AppConstants.QuestionTypeMultipleChoice)
            {
                CheckBoxList cb = new CheckBoxList();
                cb.ID = "CheckBoxList";

                foreach (Option option in options)
                {
                    ListItem item = new ListItem();
                    item.Text = option.OptionText;
                    item.Value = option.MultipleChoiceOptionID.ToString();
                    cb.Items.Add(item);
                    //answerPlaceholder.Controls.Add(new LiteralControl("<br />")); //add a line break after each radio button
                }
                answerPlaceholder.Controls.Add(cb);

                //CustomValidator validator = new CustomValidator();
                //validator.ID = "validator";
                //validator.Display = ValidatorDisplay.Dynamic;
                //validator.ErrorMessage = "Please select at least one option";
                //validator.ServerValidate += new ServerValidateEventHandler(ValidateCheckboxList);

                //answerPlaceholder.Controls.Add(validator);


            }

            else if (questionType == AppConstants.QuestionTypeTextInput)
            {

                //add a text box to the placeholder
                TextBox textBox = new TextBox();
                textBox.ID = "TextBox";
                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                nextQuestionForTextInput = convertStringToInt(question.NextQuestionForTextInput);

                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                //RequiredFieldValidator validator = new RequiredFieldValidator();
                //validator.ID = "validator";
                //validator.ControlToValidate = "TextBox";
                //validator.ErrorMessage = "Textbox cannot be empty";
                //validator.Display = ValidatorDisplay.Dynamic;
                //answerPlaceholder.Controls.Add(validator);
            }

            else if (questionType == AppConstants.QuestionTypeDate)
            {
                TextBox textBox = new TextBox();
                textBox.ID = "DateTextBox";
                textBox.TextMode = TextBoxMode.Date; //set the text box to date mode
                textBox.Attributes.Add("placeholder", "Select a date");
                nextQuestionForTextInput = convertStringToInt(question.NextQuestionForTextInput);
                answerPlaceholder.Controls.Add(textBox);

                //RequiredFieldValidator validator = new RequiredFieldValidator();
                //validator.ID = "DateValidator";
                //validator.ControlToValidate = "DateTextBox";
                //validator.ErrorMessage = "Please select a date";
                //validator.Display = ValidatorDisplay.Dynamic;
                //answerPlaceholder.Controls.Add(validator);
            }

            else if (questionType == AppConstants.QuestionTypeFinish || question.NextQuestionForTextInput == "0")
            {
                Response.Redirect("../SurveyApp/AnswersConfirmation.aspx");
            }

            else if (questionType == AppConstants.QuestionTypeTextInputEmail)
            {
                //add a text box to the placeholder
                TextBox textBox = new TextBox();
                textBox.ID = "TextBoxEmail";
                textBox.TextMode = TextBoxMode.Email;
                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                nextQuestionForTextInput = convertStringToInt(question.NextQuestionForTextInput);

                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                //RequiredFieldValidator validator = new RequiredFieldValidator();
                //validator.ID = "validator";
                //validator.ControlToValidate = "TextBoxEmail";
                //validator.ErrorMessage = "Email cannot be empty";
                //validator.Display = ValidatorDisplay.Dynamic;
                //answerPlaceholder.Controls.Add(validator);

                //RegularExpressionValidator regularExpressionValidator = new RegularExpressionValidator();
                //regularExpressionValidator.ID = "validateEmail";
                //regularExpressionValidator.ControlToValidate = "TextBoxEmail";
                //regularExpressionValidator.ErrorMessage = "Please enter a valid email";
                //regularExpressionValidator.ValidationExpression = AppConstants.EmailValidatorRegex;
                //regularExpressionValidator.Display = ValidatorDisplay.Dynamic;
                //answerPlaceholder.Controls.Add(regularExpressionValidator);
            }


            else if (questionType == AppConstants.QuestionTypeTextInputSuburb)
            {
                //add a text box to the placeholder
                TextBox textBox = new TextBox();
                textBox.ID = "TextBoxSuburb";
                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                nextQuestionForTextInput = convertStringToInt(question.NextQuestionForTextInput);

                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                //RequiredFieldValidator validator = new RequiredFieldValidator();
                //validator.ID = "validator";
                //validator.ControlToValidate = "TextBoxSuburb";
                //validator.ErrorMessage = "Suburb cannot be empty";
                //validator.Display = ValidatorDisplay.Dynamic;
                //answerPlaceholder.Controls.Add(validator);
            }


            else if (questionType == AppConstants.QuestionTypeTextInputPostCode)
            {
                //add a text box to the placeholder
                TextBox textBox = new TextBox();
                textBox.ID = "TextBoxPostCode";
                textBox.TextMode = TextBoxMode.Number; //set the text box to number mode for post code
                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString();
                nextQuestionForTextInput = convertStringToInt(question.NextQuestionForTextInput);

                //textBox.Text = HttpContext.Current.Session["currentQuestionID"].ToString() + nextQuestionForTextInput;
                answerPlaceholder.Controls.Add(textBox);

                //RequiredFieldValidator validator = new RequiredFieldValidator();
                //validator.ID = "validator";
                //validator.ControlToValidate = "TextBoxPostCode";
                //validator.ErrorMessage = "PostCode cannot be empty";
                //validator.Display = ValidatorDisplay.Dynamic;
                //answerPlaceholder.Controls.Add(validator);
            }
            answerPlaceholder.Controls.Add(new LiteralControl("<br />"));
        }

        /// <summary>
        /// Validates whether at least one item in a <see cref="CheckBoxList"/> is selected.
        /// </summary>
        /// <remarks>This method checks a <see cref="CheckBoxList"/> control within the current context to
        /// ensure  that at least one item is selected. If no items are selected, the validation fails.</remarks>
        /// <param name="o">The source of the validation event. Typically unused in this method.</param>
        /// <param name="e">The <see cref="ServerValidateEventArgs"/> containing the validation result.</param>
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

        /// <summary>
        /// Loads the first question from the database and prepares it for display.
        /// </summary>
        /// <remarks>This method retrieves the first question marked as the starting point in the
        /// database,  along with its associated options, and sets up the UI for displaying the question and its
        /// options. The current question ID is stored in the session for subsequent operations.</remarks>
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
                    firstQuestion.QuestionID = convertStringToInt(reader["QuestionID"].ToString());
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
                        firstQuestion.MaxSelection = convertStringToInt(reader["MaxAnswerSelection"].ToString());
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

        /// <summary>
        /// Handles the click event for the "Skip Question" button, allowing the user to skip the current question.
        /// </summary>
        /// <remarks>If there is no current question to skip, an alert is displayed to the user. 
        /// Otherwise, the method determines the next question to display or redirects the user to the answer
        /// confirmation page  if no further questions are available.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> object containing the event data.</param>
        protected void skipQuestionButton_Click(object sender, EventArgs e)
        {


            var sessionQueue = HttpContext.Current.Session["QuestionQueue"];
            var sessionHasQueue = HttpContext.Current.Session["HasQueue"];
            var answerDictionarySession = HttpContext.Current.Session["answerList"];
            if (answerDictionarySession != null)
            {
                answers = (List<Answer>)HttpContext.Current.Session["answerList"];
            }
            if (sessionQueue != null && sessionHasQueue != null)
            {
                questionsQueue = (List<Int32>)sessionQueue;
                hasQueue = (Boolean)sessionHasQueue;
            }

            // for questions without the branching logic, we check if the queue is present. If no queue, no stress we simply skip the current question and display next question Id from database
            if (!hasQueue || questionsQueue.Count == 1)
            {
                var questionID = HttpContext.Current.Session["currentQuestionID"];

                if (questionID == null)
                {
                    Response.Write("<script>alert('No question to skip. Please try again later.');</script>");
                    return;
                }
                else
                {
                    Int32 currentQuestionID = convertStringToInt(questionID.ToString());
                    Question question = GetQuestionFromQuestionID(currentQuestionID);
                    if (question.NextQuestionForTextInput == "0")
                    {
                        Response.Redirect(AppConstants.redirectToAnswerConfirmation);
                    }
                    else
                    {
                        //if the next question is not 0, then we should display the next question
                        Int32 nextQuestionID = convertStringToInt(question.NextQuestionForTextInput);

                        if (nextQuestionID == 0 || nextQuestionID == 10)
                        {
                            Response.Redirect(AppConstants.redirectToAnswerConfirmation);
                            return;
                        }
                        HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString();
                        DisplayQuestionAndOptions(nextQuestionID);
                        HttpContext.Current.Session.Remove("QuestionQueue");
                        HttpContext.Current.Session.Remove("hasQueue");
                        HttpContext.Current.Session.Remove("ultimateQuestionID");
                        return;
                    }
                }
            }

            // if there is a queue, we need to handle the logic of queue and questionsqueue as we cannot simply take users to a single question if there is a queue
            else if (hasQueue && questionsQueue.Count > 1)
            {
                //if there is a queue, then we should display the next question in the queue


                //but if there is the ultimate question, we should display that at last
               var ultimateQuestion =  HttpContext.Current.Session["ultimateQuestionID"].ToString();


                //checking if ultimate question exists
                if (ultimateQuestion == null)
                {
                    //if it does not exists we simply skip the firs question in the list and go to next one
                    Int32 nextQuestionID = questionsQueue[0]; //get the next question ID from the queue
                    questionsQueue.RemoveAt(0); //remove the first question from the queue
                    HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString(); //store the current question ID in the session
                    DisplayQuestionAndOptions(nextQuestionID);
                    return;
                }

                else
                {
                    //if ultimate question exists, we cannot skip the ultimate question because we will need to traverse through other questions before going into the ultimate question
                    Int32 ultimateQuestionInt = 0;

                    if(Int32.TryParse(ultimateQuestion.ToString(), out ultimateQuestionInt))
                    {
                        if (ultimateQuestionInt == 0) return;

                        // in this case, we need to remove the ultimate question from the list, skip the next question on list and add the ultimate question back and go to the next question with it
                        questionsQueue.Remove(ultimateQuestionInt);
                        Int32 nextQuestionID = questionsQueue[0]; //get the next question ID from the queue
                        questionsQueue.RemoveAt(0); //remove the first question from the queue
                        questionsQueue.Add(ultimateQuestionInt);
                        HttpContext.Current.Session["currentQuestionID"] = nextQuestionID.ToString(); //store the current question ID in the session
                        DisplayQuestionAndOptions(nextQuestionID);
                        return;

                    }
                }              
            }
            else
            {
                Response.Write("<script>alert('No question to skip. Please try again later.');</script>");
            }
        }
        protected bool validateSuburb(String suburb)
        {
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                //at this point we would have the first question. Now lets take the options
                SqlCommand cmd = new SqlCommand("SELECT 1 FROM Address WHERE suburb = @suburb", conn);
                cmd.Parameters.AddWithValue("@suburb", suburb);
                SqlDataReader optionsReader = cmd.ExecuteReader(); //execute the command

                if (optionsReader.Read())
                {
                    conn.Close();
                    return true;
                }

                else
                {
                    conn.Close();
                    return false;
                }
                    
            }
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return false;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return false;

            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return false;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return false;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return false;
            }

        }

        protected bool validatePostcode(Int32 postcode)
        {
            try
            {
                SqlConnection conn = new SqlConnection();
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                //at this point we would have the first question. Now lets take the options
                SqlCommand cmd = new SqlCommand("SELECT 1 FROM Address WHERE postcode = @postcode", conn);
                cmd.Parameters.AddWithValue("@postcode", postcode);
                SqlDataReader optionsReader = cmd.ExecuteReader(); //execute the command

                if (optionsReader.Read())
                {
                    conn.Close();
                    return true;
                }

                else
                {
                    conn.Close();
                    return false;
                }

            }
            catch (InvalidCastException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return false;
            }

            catch (SqlException ex)
            {
                Response.Write("SqlException exception found. Error: " + ex.ToString());
                return false;

            }

            catch (InvalidOperationException ex)
            {

                Response.Write("InvalidOperationException exception found. Error: " + ex.ToString());
                return false;
            }

            catch (IOException ex)
            {

                Response.Write("IOException exception found. Error: " + ex.ToString());
                return false;
            }

            catch (Exception ex)
            {
                Response.Write("Exception found. Error: " + ex.ToString());
                return false;
            }

        }

        /// <summary>
        /// Tries to convert the provided into Integer and redirects user to the error screen with error information if there is any
        /// </summary>
        /// <param name="valueToConvert"> value in the string which is to be converted</param>
        /// <returns>returns the converted value in Int32 format if it is parsable, otherwise redirects to the error page</returns>
        protected Int32 convertStringToInt(String valueToConvert)
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