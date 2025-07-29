using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AITR_Survey.Staff.StaffDashboard
{
    public partial class StaffDashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void allRespondentsButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstants.redirectToViewAllRespondents);
        }

        protected void searchRespondentsButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstants.redirectToSearchAllRespondents);
        }
    }
}