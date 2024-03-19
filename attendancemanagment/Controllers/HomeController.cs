using attendancemanagment.Models;
using attendancemanagment.Validator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Script.Serialization;

namespace attendancemanagment.Controllers
{
    
    public class HomeController : Controller
    {
        AttendanceContext db = new AttendanceContext();
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(EmployeeLogin model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.employee_id != null && model.password != null)
                    {    // Lets first check if the Model is valid or not
                        string userName = model.employee_id;
                        string password = model.password;
                        string encPassword = null;
                        //string rememberMe = Convert.ToString(model.rememberMe);

                        // Now if our password was enctypted or hashed we would have done the
                        // same operation on the user entered password here, But for now
                        // since the password is in plain text lets just authenticate directly
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



                        bool userValid = db.EmployeeMaster.Any(user => user.employee_id == model.employee_id && user.password == encPassword && user.status == "Active");

                        // User found in the database
                        if (userValid)
                        {
                            EmployeeMaster resultsDb = null;
                            resultsDb = (from e in db.EmployeeMaster
                                         where (e.employee_id.Equals(model.employee_id))
                                         select e).FirstOrDefault();

                            FormsAuthentication.SetAuthCookie(model.employee_id, false);

                            if (resultsDb != null)
                            {
                                if (resultsDb.password_count == 0)
                                {
                                    return RedirectToAction("MyDashboard", "Administrator");
                                }
                                else
                                {
                                    return RedirectToAction("ChangePassword", "Administrator");
                                }


                            }
                            else
                            {
                                ModelState.AddModelError("", "The role is not define for register employee.");
                            }


                        }
                        else
                        {
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
                        }

                    }

                }
            }
            catch (Exception e)
            {
                //string message = e.Message;
                ModelState.AddModelError("", e.Message);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // logout
        [Authorize]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        //[Authorize]
        public ActionResult SubmitOtp(PortalLogin model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.employee_id != null)
                    {    // Lets first check if the Model is valid or not
                        string userName = model.employee_id;
                        bool userValid = db.EmployeeMaster.Any(user => user.employee_id == model.employee_id && user.status == "Active");

                        // User found in the database
                        if (userValid)
                        {
                            EmployeeMaster resultsDb = null;
                            resultsDb = (from e in db.EmployeeMaster
                                         where (e.employee_id.Equals(model.employee_id))
                                         select e).FirstOrDefault();

                            //FormsAuthentication.SetAuthCookie(model.employee_id, false);

                            if (resultsDb.mobile_no != null)
                            {
                                ViewBag.mobile_no = resultsDb.mobile_no;
                            }
                            else
                            {
                                return RedirectToAction("Error", "Home");
                            }


                        }
                        else
                        {
                            return RedirectToAction("Error", "Home");
                        }

                    }

                }
            }
            catch (Exception e)
            {
                //string message = e.Message;
                ModelState.AddModelError("", e.Message);
            }

            // If we got this far, something failed, redisplay form
            return View();
        }
        [Authorize]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SubmitOtp(VarifyOtpRequest model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(model.otp))
                    {    // Lets first check if the Model is valid or not

                        bool userValid = db.EmployeeMaster.Any(user => user.mobile_no == model.mobile_no && user.status == "Active");

                        if (userValid)
                        {
                            
                            EmployeeMaster resultsDb = null;
                            resultsDb = (from e in db.EmployeeMaster
                                         where (e.mobile_no.Equals(model.mobile_no))
                                         select e).FirstOrDefault();

                            string strUrl = "https://api.fincart.com/api/v2/user/OTP/validate/workpointpayrolllogin/"+ model.otp + "/" + model.mobile_no;//"https://api.fincart.com/api/user/OTP/validate/workpointpayrolllogin/" + model.otp + "/" + model.mobile_no;//"https://api.fincart.com/api/user/OTP/send/workpointpayrolllogin/" + resultsDb.employee_id + "/" + resultsDb.mobile_no;

                            string strResult;
                            WebRequest objRequest = WebRequest.Create(strUrl);
                            WebResponse objResponse1 = (WebResponse)objRequest.GetResponse();
                            using (StreamReader sr = new StreamReader(objResponse1.GetResponseStream()))
                            {
                                strResult = sr.ReadToEnd();
                                sr.Close();
                            }

                            VarifyOtpResponse table = Newtonsoft.Json.JsonConvert.DeserializeObject<VarifyOtpResponse>(strResult);
                            ViewBag.mobile_no = model.mobile_no;
                            if (table.status == "Success")
                            {
                                FormsAuthentication.SetAuthCookie(resultsDb.employee_id, false);

                                if (resultsDb != null)
                                {
                                    if (resultsDb.password_count == 0)
                                    {
                                        return RedirectToAction("MyDashboard", "Administrator");
                                    }
                                    else
                                    {
                                        return RedirectToAction("ChangePassword", "Administrator");
                                    }


                                }
                                else
                                {
                                    ModelState.AddModelError("", "The employee is not define for register employee.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", table.msg);
                            }


                        }
                        else
                        {
                            ModelState.AddModelError("", "The otp details not matched");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The otp details not matched");
                    }
                }
            }
            catch (Exception ex)
            {
                //string message = e.Message;
                ModelState.AddModelError("", ex.Message);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize]
        [HttpGet]
        [AllowAnonymous]
        public ActionResult AproveRejectRequest(ApprovalRequest model, string returnUrl)
        {

            if (model.type == "lv")
            {
                ApplyLeaveMasters applyLeave = (from d in db.ApplyLeaveMasters
                                               where d.token.Equals(model.token) && d.id.Equals(model.ids)
                                               select d).FirstOrDefault();

                if (applyLeave != null)
                {
                    applyLeave.status = model.status;
                    applyLeave.token = null;
                    applyLeave.approve_date = DateTime.Now;
                    db.SaveChanges();

                    KeyValueMaster leaveMaster = (from d in db.KeyValueMaster
                                                 where d.id.Equals(applyLeave.leave_Type)
                                                 select d).FirstOrDefault();

                    EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                     where d.employee_id.Equals(applyLeave.employee_id)
                                                     select d).FirstOrDefault();
                    string body = string.Empty;

                    string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];

                    body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Hi " + employeemaster.name + ",</p>" +
                      "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Your Leave / " + leaveMaster.key_description + " has been " + applyLeave.status + " From " + applyLeave.from_date.ToString("dd/M/yyyy") + " To " + applyLeave.to_date.ToString("dd/M/yyyy") + " .</p>" +
                      "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Wave Employee Management System Application by signing</p><br>" +
                      "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                      "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>WEMS Support Team</p>";

                    string subject = leaveMaster.key_description;
                    string emailId = employeemaster.email;
                    //string emailId = "mahadev.mca14@gmail.com";
                    Utilities.Utility.sendMaill(emailId, body, subject);

                    return RedirectToAction("Success", "Home");
                }
            }
            else if (model.type == "miss")
            {
                AttendanceMispunch attTicket = (from d in db.AttendanceMispunch
                                                where d.token.Equals(model.token) && d.id.Equals(model.ids)
                                                select d).FirstOrDefault();
                if (attTicket != null)
                {
                    attTicket.status = model.status;
                    attTicket.token = null;
                    attTicket.approve_date = DateTime.Now;
                    db.SaveChanges();

                    if (attTicket.status == "approved")
                    {
                        AttendanceMaster attendaceMaster = (from d in db.AttendanceMaster
                                                            where d.id.Equals(attTicket.attendance_id)
                                                            select d).FirstOrDefault();
                        if (attendaceMaster != null)
                        {
                            DateTime dtFrom = DateTime.Parse(attTicket.in_time);
                            DateTime dtTo = DateTime.Parse(attTicket.out_time);

                            TimeSpan ts = dtTo.Subtract(dtFrom);

                            attendaceMaster.in_time = attTicket.in_time;
                            attendaceMaster.out_time = attTicket.out_time;
                            attendaceMaster.total_hrs = Convert.ToDouble(Math.Round(ts.TotalHours, 2));
                            attendaceMaster.status = "P";
                            attendaceMaster.mis = "";
                            attendaceMaster.late = "";
                            db.SaveChanges();
                        }
                    }

                    EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                     where d.employee_id.Equals(attTicket.employee_id)
                                                     select d).FirstOrDefault();
                    string body = string.Empty;

                    string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];

                    body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Hi " + employeemaster.name + ",</p>" +
                      "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Your Mispunch has been " + attTicket.status + " date " + attTicket.date.ToString("dd/M/yyyy") + " .</p>" +
                      "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Wave Employee Management System Application by signing</p><br>" +
                      "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                      "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>WEMS Support Team</p>";

                    string subject = "Mispunch Request";
                    string emailId = employeemaster.email;
                    //string emailId = "mahadev.mca14@gmail.com";
                    Utilities.Utility.sendMaill(emailId, body, subject);

                    return RedirectToAction("Success", "Home");
                }
            }




            else
            {
                //return RedirectToAction("Error");
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPassword model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.employee_id != null && model.name != null && model.mobile_no != null)
                    {    // Lets first check if the Model is valid or not   

                        bool userValid = db.EmployeeMaster.Any(user => user.employee_id == model.employee_id && user.name == model.name && user.mobile_no == model.mobile_no && user.status == "Active");

                        // User found in the database
                        if (userValid)
                        {
                            EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                                        where d.employee_id.Equals(model.employee_id)
                                                        select d).FirstOrDefault();
                            string EncryptionKey = @System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"];
                            //string EncryptionKey = "incentive_application";
                            string newpassword = null;
                            byte[] cipherBytes = Convert.FromBase64String(empMaster.password);
                            using (TripleDES encryptor = TripleDES.Create())
                            {
                                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                                encryptor.Key = pdb.GetBytes(24);
                                encryptor.IV = pdb.GetBytes(8);
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                                    {
                                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                                        cs.Close();
                                    }
                                    newpassword = Encoding.Unicode.GetString(ms.ToArray());
                                }
                            }
                            string strMessage = "Your password is " + newpassword;
                            string strUrl = "http://www.txtguru.in/imobile/api.php?username=herocycles&password=singhrs&source=HeroCy&dmobile=" + model.mobile_no + "&message=" + strMessage + "";


                            string strResult;
                            WebRequest objRequest = WebRequest.Create(strUrl);
                            WebResponse objResponse1 = (WebResponse)objRequest.GetResponse();
                            using (StreamReader sr = new StreamReader(objResponse1.GetResponseStream()))
                            {
                                strResult = sr.ReadToEnd();
                                sr.Close();
                            }

                            ModelState.Clear();

                            ViewBag.Message = "Password send your registered mobile number!! ";

                        }
                        else
                        {
                            ModelState.AddModelError("", "The all deatils provided is incorrect.");
                            //ViewBag.Message = "The all deatils provided is incorrect.";
                        }

                    }

                }
            }
            catch (Exception e)
            {
                //string message = e.Message;
                ModelState.AddModelError("", e.Message);
            }

            // If we got this far, something failed, redisplay form
            return View();
        }

        [HttpGet]
        public JsonResult GetUserName(string id)
        {
            //string userName = HttpContext.User.Identity.Name;

            //string strName = db.EmployeeMaster.where(x => x.id == 2).SingleOrDefault()?.Name;
            ResetPassword results = null;

            results = (from d in db.EmployeeMaster
                       where d.employee_id.Equals(id)
                       select new ResetPassword
                       {
                           employee_id = d.employee_id,
                           name = d.name
                       }).FirstOrDefault();

            var jsonsuccess = new
            {
                rsBody = results.name
            };


            return Json(jsonsuccess, JsonRequestBehavior.AllowGet);
        }
    }
}
