using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {

            //validate staff credentials

            //get the username and password from the textboxes
            string username = StaffUserNameTextBox.Text;
            string password = StaffPasswordTextBox .Text;

            try
            {
                SurveyQuestion surveyQuestion = new SurveyQuestion();
                String connectionString = surveyQuestion.GetConnectionString();
                SqlConnection conn = new SqlConnection(connectionString);
                conn.ConnectionString = connectionString;
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Staff WHERE Username = @Username AND Password = @Password", conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    //valid credentials
                    Response.Redirect(AppConstants.redirectToStaffDashboard);

                }
                else
                {
                    //invalid credentials
                    CustomValidator validator = new CustomValidator();
                    validator.IsValid = false;
                    validator.ErrorMessage = "Invalid username or password. Please try again.";
                    validator.Display = ValidatorDisplay.Dynamic;

                    return;
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                //handle exception
                Response.Write("<script>alert('An error occurred while processing your request. Please try again later.');</script>");
                Response.Write("<p>To find more information, check below: " + ex.Message + "<br />" + ex.StackTrace);
            }
            }
    }
}