using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey.Error
{
    public partial class ErrorScreen : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void GoHomeButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstants.redirectToHome);
        }
    }
}