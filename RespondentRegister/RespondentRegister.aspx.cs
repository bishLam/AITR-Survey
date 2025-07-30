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
            //get all the values from the textboxes
            if (!IsPostBack)
            {

            }

        }

        protected void Submit_Button_Click(object sender, EventArgs e)
        {

            //get all the values from the text boxes and store it in the session
            String enteredFirstName = firstNameTextBox.Text;
            String enteredLastName = lastNameTextBox.Text;
            String contactNumber = ContactNumberTextBox.Text;
            DateTime DOB;

            if (DateTime.TryParse(DOBDatePicker.Value, out DOB))
            {
                // Valid date, use DOB
                DOB = DateTime.Parse(DOBDatePicker.Value);
                if (DOB.Date > DateTime.Now.Date)
                {
                    //this means the entered date is in future
                    CustomValidator validator = new CustomValidator();
                    validator.ControlToValidate = "DOBDatePicker";
                    validator.IsValid = false;
                    validator.Display = ValidatorDisplay.Dynamic;
                    validator.ErrorMessage = "Date of birth cannot be in the future date";
                    validatorPlaceholder.Controls.Add(validator);
                    return;
                }
                
            }
            else
            {
                CustomValidator validator = new CustomValidator();
                validator.ControlToValidate = "DOBDatePicker";
                validator.IsValid = false;
                validator.Display = ValidatorDisplay.Dynamic;
                validator.ErrorMessage = "Date is not in a correct format";
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
    }
}