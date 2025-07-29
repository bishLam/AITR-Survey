using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GetStartedButton_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Session["currentQuestionID"] = "0";
            Response.Redirect("~/SurveyApp/SurveyQuestion.aspx");
        }

        protected void StaffLoginButton_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Staff/StaffLogin/StaffLogin.aspx");
        }
    }
}