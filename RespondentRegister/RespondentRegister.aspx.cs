using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class RespondentRegister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Submit_Button_Click(object sender, EventArgs e)
        {

            //get all the values from the text boxes and store it in the session
            String enteredFirstName = firstNameTextBox.Text;
            String enteredLastName = lastNameTextBox.Text;
            String contactNumber = ContactNumberTextBox.Text;
            DateTime Eighteen = DateTime.Now.Date.AddDays(-6573);

            if (DateTime.TryParse(DOBDatePicker.Value, out DateTime DOB))
            {
                // Valid date, use DOB

                //check if the age is over 18 or not
                if (Eighteen <= Convert.ToDateTime(DOB))
                {
                    //this means the entered date is in future
                    CustomValidator validator = new CustomValidator();
                    validator.ControlToValidate = "DOBDatePicker";
                    validator.IsValid = false;
                    validator.Display = ValidatorDisplay.Dynamic;
                    validator.ErrorMessage = "You must be at least 18 to continue";
                    validatorPlaceholder.Controls.Add(validator);
                    return;
                }
            }
            else
            {
                // this means the try parse failed which means that the date was not in correct format
                CustomValidator validator = new CustomValidator();
                validator.ControlToValidate = "DOBDatePicker";
                validator.IsValid = false;
                validator.Display = ValidatorDisplay.Dynamic;
                validator.ErrorMessage = "Date is not in a correct format";
                validatorPlaceholder.Controls.Add(validator);
                return;
            }

            if (!validatePhoneNumber(contactNumber))
            {
                //this means the entered phone number is not 
                CustomValidator validator = new CustomValidator();
                validator.ControlToValidate = "ContactNumberTextBox";
                validator.IsValid = false;
                validator.Display = ValidatorDisplay.Dynamic;
                validator.ErrorMessage = "Phone number is not correct";
                validatorPlaceholder.Controls.Add(validator);
                return;
            }


            //this means everything is correct
            HttpContext.Current.Session["respondentFirstName"] = enteredFirstName;
            HttpContext.Current.Session["respondentLastName"] = enteredLastName;
            HttpContext.Current.Session["respondentContactNumber"] = contactNumber;
            HttpContext.Current.Session["respondentDOB"] = DOB.ToString();
            Response.Redirect(AppConstants.redirectToAnswerConfirmation);
        }


        /// <summary>
        ///     Validated the phone number entered by the user
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns>returns if the provided phone numbemr is a valid australian number. If valid, returns true otherwise false</returns>
        protected bool validatePhoneNumber(String phoneNumber)
        {
            System.Text.RegularExpressions.Regex numberRegex = new System.Text.RegularExpressions.Regex(AppConstants.ContactNumberValidatorRegex);
            if (numberRegex.IsMatch(phoneNumber))
            {
                return true;
            }
            return false;
        }
    }

}