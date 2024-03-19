using attendancemanagment.Models;
using attendancemanagment.Validator;
using ClosedXML.Excel;
//using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text;
using System.Web.UI;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace attendancemanagment.Controllers
{
    [Authorize]
    public class AdministratorController : Controller
    {
        AttendanceContext db = new AttendanceContext();
        // GET: Administrator
        public ActionResult Index()
        {
            return View();
        }

        // my dashboard
        public ActionResult MyDashboard()
        {
            string employee_id = HttpContext.User.Identity.Name;
            if (!string.IsNullOrEmpty(employee_id))
            {
                string userName = HttpContext.User.Identity.Name;
                //int currentMonth = DateTime.Now.Month;
                EmployeeMaster results = null;
                results = (from e in db.EmployeeMaster
                           where e.employee_id.Equals(userName)
                           && e.status.Equals("Active")
                           select e).FirstOrDefault();

                string employeeId = CryptoEngine.Encrypt(results.employee_id, "skym-3hn8-sqoy19");

                string accessKey = CryptoEngine.Encrypt(System.Configuration.ConfigurationManager.AppSettings["accesskey"], "skym-3hn8-sqoy19");

                //code for function index
                //check if there is a department name in the request parameter
                //if not and the system role of the employee is Management 
                //default the department as Sales
                //populate the model with the path to the cshtml for the department
                //ViewBag.name = results.name;
                ViewBag.employee_id = employeeId;
                ViewBag.accessKey = accessKey;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        // change password
        public ActionResult ChangePassword()
        {
            string employee_id = HttpContext.User.Identity.Name;
            if (!string.IsNullOrEmpty(employee_id))
            {
                string userName = HttpContext.User.Identity.Name;
                //int currentMonth = DateTime.Now.Month;
                EmployeeMaster results = null;
                results = (from e in db.EmployeeMaster
                           where e.employee_id.Equals(userName)
                           && e.status.Equals("Active")
                           select e).FirstOrDefault();

                var count = (from d in db.EmployeeMaster where d.reporting_to.Equals(results.employee_id) select d).Count();

                string employeeId = CryptoEngine.Encrypt(results.employee_id, "skym-3hn8-sqoy19");

                //code for function index
                //check if there is a department name in the request parameter
                //if not and the system role of the employee is Management 
                //default the department as Sales
                //populate the model with the path to the cshtml for the department
                ViewBag.name = results.name;
                ViewBag.employee_id = employeeId;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChangePassword(ChangePassword model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.new_password != null && model.confirm_password != null)
                    {    // Lets first check if the Model is valid or not

                        if (model.new_password == model.confirm_password)
                        {
                            string password = model.confirm_password;
                            string encPassword = null;
                            var pass = password;
                            string EncryptionKey = @System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"];
                            byte[] clearBytes = Encoding.Unicode.GetBytes(pass);
                            using (TripleDES encryptor = TripleDES.Create())
                            {

                                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                                encryptor.Key = pdb.GetBytes(24);
                                encryptor.IV = pdb.GetBytes(8);
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                                    {
                                        cs.Write(clearBytes, 0, clearBytes.Length);
                                        cs.Close();
                                    }
                                    encPassword = Convert.ToBase64String(ms.ToArray());

                                }
                            }

                            string employee_id = HttpContext.User.Identity.Name;

                            if (!string.IsNullOrEmpty(employee_id))
                            {
                                EmployeeMaster resultsDb = null;
                                resultsDb = (from e in db.EmployeeMaster
                                             where (e.employee_id.Equals(employee_id))
                                             select e).FirstOrDefault();
                                if (resultsDb != null)
                                {
                                    resultsDb.password = encPassword;
                                    resultsDb.password_count = 0;
                                    db.SaveChanges();

                                    return RedirectToAction("MyDashboard", "Administrator");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "The provided details not matched");
                                //ViewBag.Message = "The all deatils provided is incorrect.";
                            }


                        }
                        else
                        {
                            ModelState.AddModelError("", "The confirm password and new password is not match");
                            //ViewBag.Message = "The all deatils provided is incorrect.";
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                //string message = e.Message;
                ModelState.AddModelError("", ex.Message);
            }

            // If we got this far, something failed, redisplay form
            return View();
        }


        // export excel format
        public void ExportExcel(object sender, EventArgs e)
        {
            string employee_id = HttpContext.User.Identity.Name;
            if (!string.IsNullOrEmpty(employee_id))
            {
                DateTime from_date = new DateTime();
                DateTime to_date = new DateTime();
                string fileName = Request.QueryString["name"];
                string fromDate = Request.QueryString["fromDate"];
                string toDate = Request.QueryString["toDate"];
                if (!string.IsNullOrEmpty(fromDate))
                {
                    from_date = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    to_date = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                }
                
                
                string constr = ConfigurationManager.ConnectionStrings["AttendanceContext"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    string queryData = "";
                    if (fileName == "Employee")
                    {
                        queryData = "SELECT employee_id, name, email, gender, dob, doj, designation, department, role, father_name, mobile_no, employeement_type, pay_code FROM  EmployeeMaster";
                    }
                    if (fileName == "ActiveEmp")
                    {
                        queryData = "SELECT employee_id, name, email, gender, dob, doj, designation, department, role, father_name, mobile_no, employeement_type, pay_code FROM  EmployeeMaster where status='Active'";
                    }
                    if (fileName == "Shift")
                    {
                        queryData = "SELECT * FROM  Shift";
                    }
                    if (fileName == "Department")
                    {
                        queryData = "SELECT Key_code, key_description FROM  KeyValue where key_type='department'";
                    }
                    if (fileName == "Designation")
                    {
                        queryData = "SELECT key_description FROM  KeyValue where key_type='designation'";
                    }
                    if (fileName == "LeaveType")
                    {
                        queryData = "SELECT Key_code, key_description FROM  KeyValue where key_type='leave'";
                    }
                    if (fileName == "Companies")
                    {
                        queryData = "SELECT * FROM  Companies";
                    }
                    else if (fileName == "DailyAttendance")
                    {
                        to_date = to_date.AddDays(1);
                        queryData = "SELECT EmployeeMaster.name, EmployeeMaster.employee_id, AttendanceLog.date, AttendanceLog.status, AttendanceLog.shift, AttendanceLog.in_lat, AttendanceLog.in_long FROM  AttendanceLog join EmployeeMaster on AttendanceLog.employee_id = EmployeeMaster.employee_id " +
                            "where date >='"+ from_date.ToString("yyyy-MM-dd") + "' and date <='"+ to_date.ToString("yyyy-MM-dd") + "' order by date asc";
                    }


                    using (SqlCommand cmd = new SqlCommand(queryData))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            using (DataTable dt = new DataTable())
                            {
                                sda.Fill(dt);

                                if (fileName == "DailyAttendance")
                                {
                                    dt.Columns.Add("Address");

                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        string inLat = dt.Rows[i][5].ToString().Trim();
                                        string inLong = dt.Rows[i][6].ToString().Trim();
                                        if (!string.IsNullOrEmpty(inLat) && !string.IsNullOrEmpty(inLong))
                                        {
                                            string locationName = "";
                                            //string locationAddress = "";
                                            string url = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key=AIzaSyB8Dw5oim-hw0V_bYUSAR0or-0aeJr2FF8&latlng={0},{1}&sensor=false", inLat, inLong);
                                            XElement xml = XElement.Load(url);
                                            if (xml.Element("status").Value == "OK")
                                            {
                                                locationName = string.Format("{0}",
                                                    xml.Element("result").Element("formatted_address").Value);
                                                dt.Rows[i][7] = locationName;
                                            }
                                        }
                                        else
                                        {
                                            dt.Rows[i][7] = "NA";
                                        }

                                    }
                                }

                                    

                                using (XLWorkbook wb = new XLWorkbook())
                                {
                                    wb.Worksheets.Add(dt, fileName);

                                    Response.Clear();
                                    Response.Buffer = true;
                                    Response.Charset = "";
                                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                    Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xlsx");
                                    using (MemoryStream MyMemoryStream = new MemoryStream())
                                    {
                                        wb.SaveAs(MyMemoryStream);
                                        MyMemoryStream.WriteTo(Response.OutputStream);
                                        Response.Flush();
                                        Response.End();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // return View();
        }

        // employee attendance for excel format
        public void ExportAttendanceExcel(object sender, EventArgs e)
        {
            string employee_id = HttpContext.User.Identity.Name;
            if (!string.IsNullOrEmpty(employee_id))
            {
                DateTime from_date = new DateTime();
                DateTime to_date = new DateTime();
                string fileName = Request.QueryString["name"];
                string fromDate = Request.QueryString["fromDate"];
                string toDate = Request.QueryString["toDate"];
                string pay_code = Request.QueryString["payCode"];

                if (!string.IsNullOrEmpty(fromDate))
                {
                    from_date = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    to_date = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                }


                string constr = ConfigurationManager.ConnectionStrings["AttendanceContext"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    List<EmployeeMaster> employeeMaster = (from d in db.EmployeeMaster
                                                           where d.pay_code.Equals(pay_code)
                                                           && d.role != "SuperAdmin"
                                                           && d.status.Equals("Active")
                                                           orderby d.employee_id ascending
                                                           select d).ToList();

                    string queryData = "";

                    DataTable dtReturn = new DataTable();

                    DataTable dt = new DataTable();

                    int rows = 0;
                    double days = (to_date - from_date).TotalDays;
                    decimal totalDays = Convert.ToDecimal(days + 1);

                    for (int i = 1; i <= totalDays + 1; i++)
                    {
                        dt.Columns.Add(i.ToString());
                    }

                    foreach (EmployeeMaster emp in employeeMaster)
                    {

                        List<AttendanceMaster> attendanceList = (from d in db.AttendanceMaster
                                                                 where d.employee_id.Equals(emp.employee_id)
                                                                 && d.date >= from_date && d.date <= to_date
                                                                 orderby d.date ascending
                                                                 select d).ToList();

                        if (attendanceList.Count() > 0)
                        {
                            dt.Rows.Add(emp.name + "(" + emp.employee_id + ")");
                            rows++;
                            dt.Rows.Add("Date");
                            int column = 1;
                            for (int j = 0; j < attendanceList.Count(); j++)
                            {
                               
                                dt.Rows[rows][column] = attendanceList[j].date.ToString("dd/MM/yyyy");
                                column++;
                            }
                            rows++;
                            dt.Rows.Add("Status");
                            column = 1;
                            for (int j = 0; j < attendanceList.Count(); j++)
                            {
                                dt.Rows[rows][column] = attendanceList[j].status + attendanceList[j].mis + attendanceList[j].early + attendanceList[j].late + attendanceList[j].absent;
                                column++;
                            }
                            rows++;
                            dt.Rows.Add("In Time");
                            column = 1;
                            for (int j = 0; j < attendanceList.Count(); j++)
                            {
                                dt.Rows[rows][column] = attendanceList[j].in_time;
                                column++;
                            }
                            rows++;
                            dt.Rows.Add("Out Time");
                            column = 1;
                            for (int j = 0; j < attendanceList.Count(); j++)
                            {
                                dt.Rows[rows][column] = attendanceList[j].out_time;
                                column++;
                            }
                            rows++;
                            dt.Rows.Add("Total Hrs.");
                            column = 1;
                            for (int j = 0; j < attendanceList.Count(); j++)
                            {
                                dt.Rows[rows][column] = attendanceList[j].total_hrs;
                                column++;
                            }
                            rows++;
                            dt.Rows.Add("");
                            rows++;
                        }

                        
                        //if (fileName == "Attendance")
                        //{
                        //    queryData = "select employee_id, shift, date, in_time, out_time, status, mis, early, late, absent, total_hrs from Attendance where employee_id='"+ emp.employee_id + "' and Date >='" + from_date + "' and Date <='" + to_date + "' order by date asc";
                        //}

                        //using (SqlCommand cmd = new SqlCommand(queryData))
                        //{
                        //    using (SqlDataAdapter sda = new SqlDataAdapter())
                        //    {
                        //        cmd.Connection = con;
                        //        sda.SelectCommand = cmd;
                        //        using (DataTable dt = new DataTable())
                        //        {
                        //            sda.Fill(dt);
                        //            dtReturn = GetInversedDataTable(dt);

                        //        }
                        //    }
                        //}
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt, fileName);

                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xlsx");
                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    }


                }
            }

            // return View();
        }

        public static DataTable GetInversedDataTable(DataTable table, params string[] columnsToIgnore)
        {
            //Create a DataTable to Return
            DataTable returnTable = new DataTable();

            //if (columnX == "")
            //    columnX = table.Columns[0].ColumnName;

            ////Add a Column at the beginning of the table

            //returnTable.Columns.Add(columnX);

            ////Read all DISTINCT values from columnX Column in the provided DataTale
            //List<string> columnXValues = new List<string>();

            ////Creates list of columns to ignore
            //List<string> listColumnsToIgnore = new List<string>();
            //if (columnsToIgnore.Length > 0)
            //    listColumnsToIgnore.AddRange(columnsToIgnore);

            //if (!listColumnsToIgnore.Contains(columnX))
            //    listColumnsToIgnore.Add(columnX);

            //foreach (DataRow dr in table.Rows)
            //{
            //    string columnXTemp = dr[columnX].ToString();
            //    //Verify if the value was already listed
            //    if (!columnXValues.Contains(columnXTemp))
            //    {
            //        //if the value id different from others provided, add to the list of 
            //        //values and creates a new Column with its value.
            //        columnXValues.Add(columnXTemp);
            //        returnTable.Columns.Add(columnXTemp);
            //    }
            //    else
            //    {
            //        //Throw exception for a repeated value
            //        throw new Exception("The inversion used must have " +
            //                            "unique values for column " + columnX);
            //    }
            //}

            ////Add a line for each column of the DataTable

            //foreach (DataColumn dc in table.Columns)
            //{
            //    if (!columnXValues.Contains(dc.ColumnName) &&
            //        !listColumnsToIgnore.Contains(dc.ColumnName))
            //    {
            //        DataRow dr = returnTable.NewRow();
            //        dr[0] = dc.ColumnName;
            //        returnTable.Rows.Add(dr);
            //    }
            //}
            int rows = 0;
            int columns = 0;
            //DataRow dr = ;
            DataRow dr = returnTable.NewRow();
            dr[1] = "Date";
            returnTable.Rows.Add(dr);
            dr[2] = "Status";
            returnTable.Rows.Add(dr);
            dr[3] = "In Time";
            returnTable.Rows.Add(dr);
            dr[4] = "Out Time";
            returnTable.Rows.Add(dr);
            dr[5] = "Hrs";
            returnTable.Rows.Add(dr);

            //returnTable.Rows.Add("Date");//[1][columns] = "Date";
            //returnTable.Rows.Add("Status");//[2][columns] = "Status";
            //returnTable.Rows.Add("In Time");//[3][columns] = "In Time";
            //returnTable.Rows.Add("Out Time");//[4][columns] = "Out Time";
            //returnTable.Rows.Add("Hrs");//[5][columns] = "Hrs";
            ////Complete the datatable with the values
            for (int i = 1; i < table.Rows.Count; i++)
            {
                columns++;
                returnTable.Rows[1][columns] = table.Rows[i][3];
                returnTable.Rows[2][columns] = table.Rows[i][7];
                returnTable.Rows[3][columns] = table.Rows[i][5];
                returnTable.Rows[4][columns] = table.Rows[i][6];
                returnTable.Rows[5][columns] = table.Rows[i][12];
                //for (int j = 1; j < returnTable.Columns.Count; j++)
                //{
                //    returnTable.Rows[i][j] =
                //      table.Rows[j - 1][returnTable.Rows[i][0].ToString()].ToString();
                //}
            }

            return returnTable;
        }

        public void GenerateForm12And14(object sender, EventArgs e)
        {
            string employee_id = HttpContext.User.Identity.Name;
            if (!string.IsNullOrEmpty(employee_id))
            {
                DateTime from_date = new DateTime();
                DateTime to_date = new DateTime();
                string name = Request.QueryString["name"];
                string fromDate = Request.QueryString["fromDate"];
                string toDate = Request.QueryString["toDate"];
                if (!string.IsNullOrEmpty(fromDate))
                {
                    from_date = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    to_date = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                }
                try
                {
                    string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                    Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate());
                    var fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"]; //Server.MapPath("~/customerdoc/SID/AccountStatement/");
                    PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                    int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                    //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                    Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                    //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                    doc.Open();


                    //iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(Server.MapPath("~/Images/logo.png"));
                    //image.ScalePercent(50f);
                    //image.SetAbsolutePosition(30, 550);
                    //doc.Add(image);

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<body style='Font-family:Verdana; font-size:7px;'>");

                    if (name == "form12")
                    {
                        sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5'>");
                        sb.Append("<tr><td colspan='8' style='text-align:center;'>Ageing Analysis</td></tr>");
                        sb.Append("<tr><td colspan='2'>Ledger Name</td><td>0-30Days</td><td>31-45Days</td><td>46-60Days</td><td>61-90Days</td><td>>90Days</td><td>Total</td></tr>");
                        sb.Append("<tr><td colspan='8'></td></tr>");
                        sb.Append("</table>");

                        sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5'>");
                        sb.Append("<tr><td rowspan='2'>S.No</td><td rowspan='2'>Name</td>" +
                            "<td bgcolor='#cdcdcd'>Father's Name</td><td rowspan='2'>Nature of Work</td><td rowspan='2'>Department</td>" +
                            "<td colspan='2'>Correspondent to that in form 11</td>" +
                            "<td>1st</td><td>2nd</td><td>3rd</td><td>4th</td><td>5th</td><td>6th</td><td>7th</td><td>8th</td><td>9th</td><td>10th</td><td>11th</td><td>12th</td>" +
                            "<td>13th</td><td>14th</td><td>15th</td><td>16th</td><td>17th</td><td>18th</td><td>19th</td><td>20th</td><td>21th</td><td>21th</td><td>22th</td>" +
                            "<td>23th</td><td>24th</td><td>25th</td><td>26th</td><td>27th</td><td>28th</td><td>29th</td><td>30th</td><td>31th</td>" +
                            "<td>Total number of days worked</td><td>Rate of allowance if any</td><td>Total hours of overtime</td><td>Amount due</td></tr>");
                    }
                    else if (name == "form14")
                    {

                    }
                    
                    //for (int i = 0; i < DTAccountStatement.Rows.Count; i++)
                    //{
                    //    if (Convert.ToString(DTAccountStatement.Rows[i]["BalanceSign"]) == "D")
                    //    {
                    //        if (i == 0 || i == (DTAccountStatement.Rows.Count - 1))
                    //        {
                    //            sb.Append("<tr><td bgcolor='#cef7ce'>" + (i + 1) + "</td><td bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherID"]) + "</td><td bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherDate"]) + "</td><td bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherTypeName"]) + "</td><td colspan='5' bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["Narration"]) + "</td><td bgcolor='#cef7ce' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["DebitAmount"]) + "</td><td bgcolor='#cef7ce' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["CreditAmount"]) + "</td><td bgcolor='#cef7ce' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["Balance"]) + "</td><td bgcolor='#FF0000' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["BalanceSign"]) + "</td></tr>");
                    //        }
                    //        else
                    //        {
                    //            sb.Append("<tr><td>" + (i + 1) + "</td><td>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherID"]) + "</td><td>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherDate"]) + "</td><td>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherTypeName"]) + "</td><td colspan='5'>" + Convert.ToString(DTAccountStatement.Rows[i]["Narration"]) + "</td><td style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["DebitAmount"]) + "</td><td style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["CreditAmount"]) + "</td><td style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["Balance"]) + "</td><td bgcolor='#FF0000' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["BalanceSign"]) + "</td></tr>");
                    //        }

                    //    }
                    //    else
                    //    {
                    //        if (i == 0 || i == (DTAccountStatement.Rows.Count - 1))
                    //        {
                    //            sb.Append("<tr><td bgcolor='#cef7ce'>" + (i + 1) + "</td><td bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherID"]) + "</td><td bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherDate"]) + "</td><td bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherTypeName"]) + "</td><td colspan='5' bgcolor='#cef7ce'>" + Convert.ToString(DTAccountStatement.Rows[i]["Narration"]) + "</td><td bgcolor='#cef7ce' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["DebitAmount"]) + "</td><td bgcolor='#cef7ce' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["CreditAmount"]) + "</td><td bgcolor='#cef7ce' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["Balance"]) + "</td><td bgcolor='#008000' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["BalanceSign"]) + "</td></tr>");
                    //        }
                    //        else
                    //        {
                    //            sb.Append("<tr><td>" + (i + 1) + "</td><td>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherID"]) + "</td><td>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherDate"]) + "</td><td>" + Convert.ToString(DTAccountStatement.Rows[i]["VoucherTypeName"]) + "</td><td colspan='5'>" + Convert.ToString(DTAccountStatement.Rows[i]["Narration"]) + "</td><td style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["DebitAmount"]) + "</td><td style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["CreditAmount"]) + "</td><td style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["Balance"]) + "</td><td bgcolor='#008000' style='text-align:right;'>" + Convert.ToString(DTAccountStatement.Rows[i]["BalanceSign"]) + "</td></tr>");
                    //        }

                    //    }

                    //}

                    sb.Append("</table>");
                    sb.Append("</body>");
                    HTMLWorker hw = new HTMLWorker(doc);
                    hw.Parse(new StringReader(sb.ToString()));
                    doc.Close();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename="+ fileName);
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(doc);
                    Response.End();
                }
                catch(Exception ex)
                {
                    throw;
                }
            }

        }


        // typehead
        // get company type
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult EmployeeTypehead(string Prefix, string employee_id, string accessKey)
        {
            string status = "error";

            List<EmployeeDetails> docMaster = null;
            try
            {
                string accessKey1 = CryptoEngine.Decrypt(accessKey, "skym-3hn8-sqoy19");
                if (accessKey1 == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
                {
                    string employeeId = CryptoEngine.Decrypt(employee_id, "skym-3hn8-sqoy19");
                    bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

                    if (userValid)
                    {
                        EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                         where d.employee_id.Equals(employeeId)
                                                         select d).FirstOrDefault();
                        docMaster = (from d in db.EmployeeMaster
                                     where d.name.ToLower().Contains(Prefix) && d.status.Equals("Active") &&
                                     d.pay_code.Equals(employeeMaster.pay_code)
                                     select new EmployeeDetails
                                     {
                                         employee_id = d.employee_id,
                                         name = d.name
                                     }).ToList();

                        if (docMaster.Count() == 0)
                        {
                            docMaster = (from d in db.EmployeeMaster
                                         where d.employee_id.ToLower().Contains(Prefix) && d.status.Equals("Active") &&
                                         d.pay_code.Equals(employeeMaster.pay_code)
                                         select new EmployeeDetails
                                         {
                                             employee_id = d.employee_id,
                                             name = d.name
                                         }).ToList();
                        }
                    }

                }
                   

            }
            catch (Exception ex)
            {
                status = ex.Message;
            }

            var jsonsuccess = new
            {
                rsBody = new
                {
                    data = docMaster,
                    status = status
                }
            };

            return Json(docMaster, JsonRequestBehavior.AllowGet);
        }
    }
}
