using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace AITR_Survey.Error
{
    public partial class ErrorScreen : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String errorMessage = HttpContext.Current.Session["ErrorMessage"] as String;
            errorDetailsLabel.Text = errorMessage;
        }


        protected void GoHomeButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(AppConstants.redirectToHome);




            //Script used to insert postal address data into the database
            // Your MySQL connection string

            //        Get connection string from the class
            //        SurveyQuestion surveyApp = new SurveyQuestion();
            //        string connString = surveyApp.GetConnectionString();

            //        // Prepare a DataTable matching your Address table columns
            //        DataTable table = new DataTable();
            //        table.Columns.Add("postcode", typeof(int));
            //        table.Columns.Add("suburb", typeof(string));
            //        table.Columns.Add("state", typeof(string));

            //        // Read CSV
            //        using (var reader = new StreamReader(@"C:\Users\Acer\OneDrive\Desktop\AITR-Survey\AITR-Survey\Class\australian_postcodes.csv"))
            //        {
            //            bool isFirstLine = true;
            //            while (!reader.EndOfStream)
            //            {
            //                var line = reader.ReadLine();
            //                if (isFirstLine)
            //                {
            //                    isFirstLine = false; // skip header
            //                    continue;
            //                }

            //                var values = line.Split(',');

            //                if (values.Length == 3)
            //                {
            //                    // Parse values
            //                    int postcode = int.Parse(values[0].Trim());
            //    string locality = values[1].Trim();
            //    string state = values[2].Trim();

            //    // Add row to DataTable
            //    table.Rows.Add(postcode, locality, state);
            //                }
            //            }
            //        }

            //        // Bulk insert into SQL Server
            //        using (SqlConnection conn = new SqlConnection(connString))
            //{
            //    conn.Open();

            //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
            //    {
            //        bulkCopy.DestinationTableName = "Address";

            //        // Map columns to DB table columns
            //        bulkCopy.ColumnMappings.Add("postcode", "postcode");
            //        bulkCopy.ColumnMappings.Add("suburb", "suburb");
            //        bulkCopy.ColumnMappings.Add("state", "state");

            //        bulkCopy.WriteToServer(table);
            //    }
            //}

            //Console.WriteLine("CSV data inserted into SQL Server successfully!");


            //        }
            //    }
        }
    }
}