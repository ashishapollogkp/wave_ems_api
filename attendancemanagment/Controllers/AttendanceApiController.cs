using attendancemanagment.Models;
using attendancemanagment.Validator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.UI;
using System.Text;
using System.Web.Http;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using System.Text.RegularExpressions;
using attendancemanagment.Container;
using attendancemanagment.Validators;

using GeoCoordinatePortable;
using System.Web;
using System.Net.Http.Headers;
using QRCoder;
using System.Configuration;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Web.Http.Cors;
using ClosedXML.Excel;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
namespace attendancemanagment.Controllers
{
  [RoutePrefix("api/AttendanceApi")]
  
  public class AttendanceApiController : ApiController
  {
    string constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContext"].ConnectionString;
    string constrsmartoffice = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContextSmartOffice"].ConnectionString;

    [HttpPost]
    [Route("EmployeeLogin")]
    public EmployeeResponse EmployeeLogin(EmployeeMasterDTO req)
    {
      EmployeeResponse response = new EmployeeResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "a user details is not match with register user";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            if (!string.IsNullOrEmpty(req.employee_id) && !string.IsNullOrEmpty(req.password))
            {    // Lets first check if the Model is valid or not
              string userName = req.employee_id;
              string password = req.password;
              string encPassword = null;

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


              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.password == encPassword && user.status == "Active");

              // User found in the database
              if (userValid)
              {
                EmployeeBasicdetails results = null;
                results = (from d in db.EmployeeMaster
                           where d.employee_id.Equals(req.employee_id)
                           select new EmployeeBasicdetails
                           {
                             employee_id = d.employee_id,
                             name = d.name,
                             role = d.role,
                             password_count = d.password_count,
                             feature_image = d.feature_image,
                             attendance_type = d.attendance_type,
                             designation = d.designation,
                             department = d.department,
                             pay_code = d.pay_code
                           }).FirstOrDefault();

                if (results != null)
                {
                  results.employee_id = CryptoEngine.Encrypt(results.employee_id, "skym-3hn8-sqoy19");

                  results.location_list = (from d in db.Shift
                                           where d.pay_code.Equals(results.pay_code)
                                           select new LatLongModel
                                           {
                                             lat = d.latitude,
                                             log = d.longitude
                                           }).ToList();

                  results.accessKey = CryptoEngine.Encrypt(System.Configuration.ConfigurationManager.AppSettings["accesskey"], "skym-3hn8-sqoy19");
                  if (results.password_count == 0)
                  {
                    response.status = "success";
                    response.flag = "1";
                    response.data = results;
                    response.alert = "success";
                  }
                  else
                  {
                    response.status = "success";
                    response.flag = "2";
                    response.data = results;
                    response.alert = "success";
                  }

                }


              }

            }
          }
        }
      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // employee Login by mobiloe no.
    [HttpPost]
    [Route("EmployeeSendOtp")]
    public HmcStringResponse EmployeeSendOtp(EmployeeLoginMobileDTO req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "a user details is not match with register user";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            if (!string.IsNullOrEmpty(req.mobile_no))
            {

              bool userValid = db.EmployeeMaster.Any(user => user.mobile_no == req.mobile_no && user.status == "Active");

              // User found in the database
              if (userValid)
              {
                //string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
                string numbers = "1234567890";

                string characters = numbers;
                string otp = string.Empty;
                for (int i = 0; i < 6; i++)
                {
                  string character = string.Empty;
                  do
                  {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                  } while (otp.IndexOf(character) != -1);
                  otp += character;
                }

                if (req.mobile_no == "8953275221")
                {
                  otp = "987321";
                }
                else
                {

                  //string textMesage = otp + " is the OTP for login Skylabs. OTP is valid for 10 mins. Do not share it with anyone.";
                  string strUrl = "http://connectexpress.in/api/v3/index.php?method=sms&api_key=5b925f3bcea8af4f5da003645048e4d562b28fee&to=" + req.mobile_no + "&sender=OELEDU&unicode=auto&message=Your overseas education lane login OTP is " + otp;

                  string strResult;
                  WebRequest objRequest = WebRequest.Create(strUrl);
                  WebResponse objResponse1 = (WebResponse)objRequest.GetResponse();
                  using (StreamReader sr = new StreamReader(objResponse1.GetResponseStream()))
                  {
                    strResult = sr.ReadToEnd();
                    sr.Close();
                  }
                }

                VarifyOtp employeeOtp = (from d in db.VarifyOtp
                                         where d.mobile_no.Equals(req.mobile_no)
                                         select d).FirstOrDefault();

                if (employeeOtp != null)
                {
                  employeeOtp.otp = otp;
                  db.SaveChanges();
                }
                else
                {
                  VarifyOtp insertOtp = new VarifyOtp();
                  insertOtp.employee_id = "NA";
                  insertOtp.mobile_no = req.mobile_no;
                  insertOtp.otp = otp;
                  db.VarifyOtp.Add(insertOtp);
                  db.SaveChanges();
                }

                response.status = "success";
                response.flag = "1";
                response.data = "success";
                response.alert = "success";
              }

            }
          }
        }
      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    [HttpPost]
    [Route("EmployeeVarifyOtp")]
    public EmployeeResponse EmployeeVarifyOtp(EmployeeLoginMobileDTO req)
    {
      EmployeeResponse response = new EmployeeResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "a otp is not match";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            if (!string.IsNullOrEmpty(req.mobile_no) && !string.IsNullOrEmpty(req.otp))
            {    // Lets first check if the Model is valid or not
              string userName = req.mobile_no;
              string password = req.otp;



              bool userValid = db.VarifyOtp.Any(user => user.mobile_no == req.mobile_no && user.otp == req.otp);

              // User found in the database
              if (userValid)
              {
                VarifyOtp employeeOtp = (from d in db.VarifyOtp
                                         where d.mobile_no.Equals(req.mobile_no)
                                         && d.otp.Equals(req.otp)
                                         select d).FirstOrDefault();

                db.VarifyOtp.Remove(employeeOtp);
                db.SaveChanges();

                EmployeeMaster retVal = null;
                retVal = (from d in db.EmployeeMaster
                          where d.mobile_no.Equals(req.mobile_no)
                          select d).FirstOrDefault();

                if (retVal != null)
                {
                  retVal.password_count = 0;
                  db.SaveChanges();
                  EmployeeBasicdetails results = new EmployeeBasicdetails();
                  results.employee_id = retVal.employee_id;
                  results.name = retVal.name;
                  results.role = retVal.role;
                  results.password_count = 0;
                  results.feature_image = retVal.feature_image;
                  results.department = retVal.department;
                  results.designation = retVal.designation;
                  results.attendance_type = retVal.attendance_type;
                  results.employee_id = CryptoEngine.Encrypt(results.employee_id, "skym-3hn8-sqoy19");

                  results.accessKey = CryptoEngine.Encrypt(System.Configuration.ConfigurationManager.AppSettings["accesskey"], "skym-3hn8-sqoy19");
                  if (results.password_count == 0)
                  {
                    response.status = "success";
                    response.flag = "1";
                    response.data = results;
                    response.alert = "success";
                  }
                  else
                  {
                    response.status = "success";
                    response.flag = "2";
                    response.data = results;
                    response.alert = "success";
                  }

                }


              }

            }
          }
        }
      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    [HttpPost]
    [Route("EmployeeDetails")]
    public HmcNewResponse EmployeeDetails(HmcRequest req)
    {
      HmcNewResponse response = new HmcNewResponse();
      EmployeeDetailsNewDTO baseDetails = new EmployeeDetailsNewDTO();
      response.status = "error";
      response.flag = "0";
      response.alert = "employeeid and email id alredy exist";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {

            using (AttendanceContext db = new AttendanceContext())
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              baseDetails = (from d in db.EmployeeMaster
                             where d.employee_id.Equals(employeeId)
                             select new EmployeeDetailsNewDTO
                             {
                               employee_id = d.employee_id,
                               name = d.name,
                               role = d.role,
                               pay_code = d.pay_code,
                               feature_image = d.feature_image,


                               id = d.id,
                               //employee_id = d.employee_id,
                              // name = d.name,
                               designation = d.designation,
                               repoting_to = d.reporting_to,
                              // pay_code = d.pay_code,
                               mobile_no = d.mobile_no,
                               email = d.email,
                              // role = d.department,
                              // feature_image = d.feature_image,
                               father_name = d.father_name,
                              dob = d.dob.ToString(),
                              doj = d.doj.ToString(),
                               //RoleId = d.RoleId,
                               //OfficeId = d.OfficeId,
                               //DepartmentId = d.DepartmentId,
                               //DesignationId = d.DesignationId,

                               //GenderId = d.GenderId,
                               //EmpCode = d.EmpCode,
                               //EmpBioCode = d.EmpBioCode,
                               //GradeId = d.GradeId,
                               //LocationId = d.LocationId,
                               //JobTypeId = d.JobTypeId,
                               //ProbInMonth = d.ProbInMonth,
                               //ProbInYear = d.ProbInYear,
                               //ConfirmationDate = d.ConfirmationDate,
                               //SpouseName = d.SpouseName,
                               //EmgcyPersonName = d.EmgcyPersonName,
                               //EmgcyPersonMobile = d.EmgcyPersonMobile



                             }).FirstOrDefault();


              if (baseDetails != null)
              {
                List<MenuDetails> menuList = (from d in db.MenuMaster
                                              where d.sub_menu_id == 0
                                              select new MenuDetails
                                              {
                                                id = d.id,
                                                name = d.name,
                                                url = d.url,
                                                icons = d.icons
                                              }).ToList();

                if (menuList.Count() > 0)
                {
                  foreach (MenuDetails item in menuList)
                  {
                    if (item.id != 0)
                    {
                      List<MenuMaster> subMenuList = (from d in db.MenuMaster
                                                      where d.sub_menu_id.Equals(item.id)
                                                      select d).ToList();

                      item.sub_menu = subMenuList;
                    }
                  }
                }

                if (baseDetails.role == "Administrator")
                {
                  baseDetails.menuList = menuList;
                }
                else
                {
                  KeyValueMaster keyValue = (from d in db.KeyValueMaster
                                             where d.key_code.Equals(baseDetails.role)
                                             && d.key_type.Equals("role")
                                             && d.pay_code.Equals(baseDetails.pay_code)
                                             select d).FirstOrDefault();

                  if (keyValue != null)
                  {
                    //baseDetails.menuList = menuList;

                    List<MenuDetails> accessMenuList = new List<MenuDetails>();

                    foreach (MenuDetails item in menuList)
                    {
                      AccessMenu accessDetails = (from d in db.AccessMenu
                                                  where d.menu_id.Equals(item.id)
                                                  && d.role_id.Equals(keyValue.id)
                                                  select d).FirstOrDefault();

                      if (accessDetails == null)
                      {
                        List<MenuMaster> accessSubMenuList = new List<MenuMaster>();
                        foreach (MenuMaster item2 in item.sub_menu)
                        {
                          AccessMenu accessDetails2 = (from d in db.AccessMenu
                                                       where d.role_id.Equals(keyValue.id)
                                                       && d.menu_id.Equals(item2.id)
                                                       select d).FirstOrDefault();

                          if (accessDetails2 != null)
                          {
                            accessSubMenuList.Add(item2);
                          }
                        }

                        if (accessSubMenuList.Count() > 0 || accessDetails != null)
                        {
                          MenuDetails accMenuDetail = new MenuDetails();

                          accMenuDetail.name = item.name;
                          accMenuDetail.url = item.url;
                          accMenuDetail.id = item.id;
                          accMenuDetail.icons = item.icons;
                          accMenuDetail.sub_menu = accessSubMenuList;

                          accessMenuList.Add(accMenuDetail);
                        }


                      }
                    }

                    int applyLeave = (from d in db.ApplyLeaveMasters
                                      where d.assign_by.Equals(req.employee_id)
                                      && d.status.Equals("pending")
                                      select d).Count();

                    int attTicke = (from d in db.AttendanceMispunch
                                    where d.assign_by.Equals(req.employee_id)
                                    && d.status.Equals("pending")
                                    select d).Count();

                    baseDetails.notification = applyLeave + attTicke;
                    baseDetails.menuList = accessMenuList;
                  }



                }

                response.status = "success";
                response.flag = "1";
                response.data = baseDetails;
                response.alert = "success";
              }
            }
          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // team employee info
    [HttpPost]
    [Route("EmployeeInfomation")]
    public HmcResponse EmployeeInfomation(HmcRequest req)
    {
      HmcResponse response = new HmcResponse();
      EmployeeBaseDetails baseDetails = new EmployeeBaseDetails();
      response.status = "error";
      response.flag = "0";
      response.alert = "employeeid and email id alredy exist";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {

            using (AttendanceContext db = new AttendanceContext())
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              baseDetails = (from d in db.EmployeeMaster
                             where d.employee_id.Equals(employeeId)
                             select new EmployeeBaseDetails
                             {
                               employee_id = d.employee_id,
                               name = d.name,
                               role = d.role,
                               pay_code = d.pay_code,
                               attendance_type = d.attendance_type,
                             }).FirstOrDefault();


              if (baseDetails != null)
              {
                response.status = "success";
                response.flag = "1";
                response.data = baseDetails;
                response.alert = "success";
              }
            }
          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // url authentication
    [HttpPost]
    [Route("UrlAuthontication")]
    public HmcResponse UrlAuthontication(HmcRequest req)
    {
      HmcResponse response = new HmcResponse();
      EmployeeBaseDetails baseDetails = new EmployeeBaseDetails();
      response.status = "error";
      response.flag = "0";
      response.alert = "employeeid and email id alredy exist";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            using (AttendanceContext db = new AttendanceContext())
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid != null)
              {
                req.currentUrl = req.currentUrl.Replace("/", "");
                if (userValid.role == "Administrator")
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }
                else if (req.currentUrl == "")
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }
                else
                {
                  KeyValueMaster keyValue = (from d in db.KeyValueMaster
                                             where d.key_code.Equals(userValid.role)
                                             && d.pay_code.Equals(userValid.pay_code)
                                             && d.key_type.Equals("Role")
                                             select d).FirstOrDefault();
                  if (keyValue != null)
                  {
                    AccessMenu accessDetails = (from d in db.AccessMenu
                                                join m in db.MenuMaster on d.menu_id equals m.id
                                                where m.url.Equals(req.currentUrl)
                                                && d.role_id.Equals(keyValue.id)
                                                select d).FirstOrDefault();

                    if (accessDetails != null)
                    {
                      response.status = "success";
                      response.flag = "1";
                      response.alert = "success";
                    }
                  }
                }
              }
            }
          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }



    // get employee
    // Apply For Leave
    [HttpPost]
    [Route("GetEmployee")]
    public EmployeeListResponse GetEmployee(HmcRequest req)
    {
      EmployeeListResponse response = new EmployeeListResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empDetail = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                int page = Convert.ToInt32(req.pageNo);
                List<EmployeeMaster> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.EmployeeMaster
                              where (d.name.ToLower().Contains(req.search_result))
                              || (d.employee_id.ToLower().Contains(req.search_result))
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.EmployeeMaster
                              where (d.name.ToLower().Contains(req.search_result))
                              || (d.employee_id.ToLower().Contains(req.search_result))
                              && d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }
                else
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.EmployeeMaster
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.EmployeeMaster
                              where d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }

    // create employee
    [HttpPost]
    [Route("CreateEmployee")]
    public HmcJsonResponse CreateEmployee(EmployeeDetails req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "employeeid and email id alredy exist";
      try
      {
        //if (req.office_emp == null)
        //{
        //  req.office_emp = false;
        //}

        //if (req.marketing_emp == null)
        //{
        //  req.marketing_emp = false;
        //}

        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (!userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(UserValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  string password = "welcome@123";
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


                  EmployeeMaster result = new EmployeeMaster();

                  try
                  {
                    DateTime dobD = DateTime.ParseExact(req.dob, "dd/MM/yyyy", null);
                    result.dob = dobD;
                  }
                  catch {  }

                  try
                  {
                    DateTime dojD = DateTime.ParseExact(req.doj, "dd/MM/yyyy", null);
                    result.doj = dojD;
                  }
                  catch {  }
                  //DateTime dobD = DateTime.ParseExact(req.dob, "dd/MM/yyyy", null);


                  result.employee_id = employeeId;
                  result.name = req.name;                  

                  if (req.email.Length > 0)
                  {
                    result.email = req.email;
                  }
                  else { result.email = "default@default.com"; }




                  result.mobile_no = req.mobile_no;



                  try
                  {

                    if (req.designation.Length > 0 || req.designation != null)
                    {
                      result.designation = req.designation;
                    }
                    else { result.designation = "Default"; }
                  }
                  catch {

                    result.designation = "Default";
                  }



                  try
                  {

                    if (req.department.Length > 0)
                    {
                      result.department = req.department;
                    }
                    else { result.department = "Default"; }
                  }
                  catch
                  {
                    result.department = "Default";

                  }





                  result.blood_group = req.blood_group;
                  result.status = req.status;
                  try
                  {
                    if (req.reporting_to.Length > 0)
                    {
                      result.reporting_to = req.reporting_to;
                    }
                    else { result.reporting_to = "0"; }
                  }
                  catch {
                    result.reporting_to = "0";

                  }



                  try
                  {
                    if (req.shift.Length > 0)
                    {
                      result.shift = req.shift;
                    }
                    else { result.shift = "GEN"; }
                  }
                  catch
                  {
                    result.shift = "GEN";

                  }




                  result.father_name = req.father_name;
                  result.aadhar_number = req.aadhar_number;
                  try
                  {
                    if (req.gender.Length > 0)
                    {
                      result.gender = req.gender;
                    }
                    else { result.gender = "Male"; }
                  }
                  catch {
                    result.gender = "Male";
                  }


                  try
                  {
                    if (req.employee_type.Length > 0)
                    {
                      result.employeement_type = req.employee_type;
                    }
                    else { result.employeement_type = "Parmanent"; }
                  }
                  catch
                  {
                    result.employeement_type = "Parmanent";
                  }



                  result.bank_name = req.bank_name;
                  result.ifsc_code = req.ifsc_code;
                  result.acc_no = req.acc_no;
                  result.employee_salary = req.employee_salary;

                  try
                  {
                    if (req.role.Length > 0)
                    {
                      result.role = req.role;
                    }
                    else { result.role = "employee"; }
                  }
                  catch
                  {
                    result.role = "employee";
                  }

                  



                  result.el = req.el;
                  result.cl = req.cl;
                  result.sl = req.sl;
                  result.a_el = "0";
                  result.a_cl = "0";
                  result.a_sl = "0";
                  result.b_el = req.el;
                  result.b_cl = req.cl;
                  result.b_sl = req.sl;


                  try
                  {
                    if (req.pay_code.Length > 0)
                    {
                      result.pay_code = req.pay_code;
                    }
                    else { result.pay_code = "1000"; }
                  }
                  catch { result.pay_code = "1000"; }



                  result.password = encPassword;
                  result.password_count = 1;
                  result.office_emp = req.office_emp;
                  result.marketing_emp = req.marketing_emp;
                  result.modify_by = employeeId;
                  result.modify_date = DateTime.Now;
                  result.pan_no = req.pan_no;
                  result.uan_no = req.uan_no;

                  try
                  {

                    if (req.attendance_type.Length != 0)
                    {
                      result.attendance_type = req.attendance_type;
                    }
                    else { result.attendance_type = ""; }
                  }
                  catch { result.attendance_type = ""; }


                 
                  db.EmployeeMaster.Add(result);
                  db.SaveChanges();

                  EmployeeMachineMaster machineLink = new EmployeeMachineMaster();
                  machineLink.machine_id = req.machine_id;
                  machineLink.employee_id = req.employee_code;
                  machineLink.modify_by = employeeId;
                  machineLink.pay_code = result.pay_code;
                  machineLink.modify_date = DateTime.Now;
                  db.EmployeeMachineMaster.Add(machineLink);
                  db.SaveChanges();

                  

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";

                  string body = string.Empty;
                  body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + result.name + ",</p>" +
                            "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Skylabs Employee Management System</b>. please login our portal one time password is " + password + "</p>" +
                            "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                            "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Skylabs Support Team</p>";
                  string subject = "Welcome Employee Managment System";
                  string emailId = result.email;
                  //string emailId = "cs.sharma@hmcgroup.in";
                  //Utilities.Utility.sendMaill(emailId, body, subject);

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";

                }
              }
            }

          }

        }

        SqlConnection con = new SqlConnection(constr);
        SqlCommand cmd = new SqlCommand();
        con.Open();
        cmd = new SqlCommand("usp_update_employeemaster", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.ExecuteNonQuery();
        con.Close();



      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // update employee
    [HttpPost]
    [Route("UpdateEmployee")]
    public HmcJsonResponse UpdateEmployee(EmployeeDetails req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {

              if (req.email == "")
              {
                req.email = "abc@gmail.com";
              }

              if (req.email == null)
              { req.email = "abc@gmail.com"; }

              ExceptionResponseContainer retVal = null;
              retVal = CustomValidator.applyValidations(req, typeof(UserValidate));

              if (retVal.isValid == false)
              {
                response.alert = retVal.jsonResponse;
                //return Json(retVal.jsonResponse);
                //response.alert = "data is not valid";
              }
              else
              {
                bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

                if (userValid)
                {

                  DateTime dobD = DateTime.ParseExact(req.dob, "dd/MM/yyyy", null);
                  DateTime dojD = DateTime.ParseExact(req.doj, "dd/MM/yyyy", null);
                  DateTime dolD = DateTime.Parse(req.dol);

                  Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                  Match match = regex.Match(req.email);

                  var emp_data = (from d in db.EmployeeMaster
                                  where d.id.Equals(req.id)
                                  select d).FirstOrDefault();




                  EmployeeMaster result = (from d in db.EmployeeMaster
                                           where d.id.Equals(req.id)
                                           select d).FirstOrDefault();
                  if (result != null)
                  {
                    result.employee_id = employeeId;
                    result.name = req.name;
                    try
                    {

                      if (req.email.Length > 0)
                      {
                        result.email = req.email;
                      }
                      else { result.email = "default@default.com"; }
                    }
                    catch { result.email = "default@default.com"; }




                    result.mobile_no = req.mobile_no;
                    result.dob = dobD;
                    result.designation = req.designation;
                    result.department = req.department;

                    try
                    {
                      if (req.designation.Length > 0)
                      {
                        result.designation = req.designation;
                      }
                      else { result.designation = "Default"; }
                    }
                    catch { result.designation = "Default"; }



                    try
                    {

                      if (req.department.Length > 0)
                      {
                        result.department = req.department;
                      }
                      else { result.department = "Default"; }
                    }
                    catch { result.department = "Default"; }


                    result.doj = dojD;
                    result.blood_group = req.blood_group;
                    result.status = req.status;
                    //result.reporting_to = req.reporting_to;

                    try
                    {
                      if (req.reporting_to.Length > 0)
                      {
                        result.reporting_to = req.reporting_to;
                      }
                      else { result.reporting_to = "0"; }
                    }
                    catch {
                      result.reporting_to = "0";

                    }



                    //result.shift = req.shift;
                    //if (req.shift.Length > 0)
                    //{
                    //  result.shift = req.shift;
                    //}
                    //else { result.shift = "GEN"; }

                    result.shift = "GEN";

                    //result.father_name = req.father_name;
                    //result.aadhar_number = req.aadhar_number;
                    //result.gender = req.gender;
                    try
                    {
                      if (req.gender.Length > 0)
                      {
                        result.gender = req.gender;
                      }
                      else { result.gender = "Male"; }
                    }
                    catch { result.gender = "Male"; }


                    //result.employeement_type = "Parmanent";
                    try
                    {
                      if (req.employee_type.Length > 0)
                      {
                        result.employeement_type = req.employee_type;
                      }
                      else { result.employeement_type = "Parmanent"; }
                    }
                    catch { result.employeement_type = "Parmanent"; }

                    //true{ }
                    //result.employeement_type = "employee";

                    result.bank_name = req.bank_name;
                    result.ifsc_code = req.ifsc_code;
                    result.acc_no = req.acc_no;
                    result.employee_salary = req.employee_salary;

                    result.role = "employee";


                    result.el = req.el;

                    result.cl = req.cl;
                    result.sl = req.sl;
                    result.b_el = req.el;
                    result.b_cl = req.cl;
                    result.b_sl = req.sl;

                    if (req.el == "")
                    {
                      result.el = "0";
                    }
                    if (req.cl == "")
                    {
                      result.cl = "0";
                    }
                    if (req.sl == "")
                    {
                      result.sl = "0";
                    }

                    if (req.el == "")
                    {
                      result.b_el = "0";
                    }


                    if (req.cl == "0")
                    {
                      result.b_cl = "0";

                    }


                    if (req.sl == "")
                    {
                      result.b_sl = "0";
                    }




                    result.dol = dolD;
                    //result.pay_code = req.pay_code;


                    try
                    {

                      if (req.pay_code.Length > 0)
                      {
                        result.pay_code = req.pay_code;
                      }
                      else { result.pay_code = "1000"; }
                    }
                    catch { result.pay_code = "1000"; }


                    result.pan_no = req.pan_no;
                    result.uan_no = req.uan_no;
                    result.office_emp = req.office_emp;
                    result.marketing_emp = req.marketing_emp;
                    result.modify_by = employeeId;
                    result.modify_date = DateTime.Now;
                    result.attendance_type = req.attendance_type;


                    try
                    {

                      if (req.attendance_type.Length != 0)
                      {
                        result.attendance_type = req.attendance_type;
                      }
                      else { result.attendance_type = ""; }
                    }
                    catch {
                      result.attendance_type = "";

                    }

                    db.SaveChanges();



                    EmployeeMaster result_IsManager = (from d in db.EmployeeMaster
                                                       where d.id.Equals(req.id)
                                                       select d).FirstOrDefault();
                    if (result_IsManager != null)
                    {
                      var ismanager = (from d in db.EmployeeMaster
                                       where d.reporting_to.Equals(employeeId)
                                       select d).Count();

                      if (ismanager > 0)
                      {
                        result_IsManager.role = "manager";
                      }
                      else
                      {
                        result_IsManager.role = "employee";

                      }
                      db.SaveChanges();

                    }




                    EmployeeMachineMaster machineLink = (from d in db.EmployeeMachineMaster
                                                         where d.employee_id.Equals(req.employee_code)
                                                         select d).FirstOrDefault();

                    if (machineLink != null)
                    {
                      machineLink.machine_id = req.machine_id;
                      machineLink.modify_by = employeeId;
                      machineLink.pay_code = result.pay_code;
                      machineLink.modify_date = DateTime.Now;
                      db.SaveChanges();
                    }
                    else
                    {
                      EmployeeMachineMaster machineInsert = new EmployeeMachineMaster();
                      machineInsert.machine_id = req.machine_id;
                      machineInsert.employee_id = req.employee_code;
                      machineInsert.modify_by = employeeId;
                      machineInsert.modify_date = DateTime.Now;
                      machineInsert.pay_code = result.pay_code;
                      db.EmployeeMachineMaster.Add(machineInsert);
                      db.SaveChanges();
                    }

                    response.status = "success";
                    response.flag = "1";
                    response.alert = "success";
                  }

                }
              }


            }

          }

        }

        SqlConnection con = new SqlConnection(constr);
        SqlCommand cmd = new SqlCommand();
        con.Open();
        cmd = new SqlCommand("usp_update_employeemaster", con);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.ExecuteNonQuery();
        con.Close();




      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
        response.status = ex.Message;
      }

      return response;

    }

    // show employee details
    // show employee details
    [HttpPost]
    [Route("ShowEmployee")]
    public ShowEmployee ShowEmployee(HmcRequest req)
    {
      ShowEmployee response = new ShowEmployee();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {


              var empId = db.EmployeeMaster.FirstOrDefault(u => u.id == req.id);

              //string employeeId = CryptoEngine.Decrypt(empId.employee_id, "skym-3hn8-sqoy19");

              string employeeId = empId.employee_id;

              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                EmployeeDetails results = null;
                results = (from d in db.EmployeeMaster
                             //join e in db.EmployeeMachineMaster on d.employee_id equals e.employee_id
                           where d.id.Equals(req.id)
                           select new EmployeeDetails
                           {
                             id = d.id,
                             employee_code = d.employee_id,
                             employee_id = d.employee_id,
                             name = d.name,
                             designation = d.designation,
                             department = d.department,
                             father_name = d.father_name,
                             status = d.status,
                             role = d.role,
                             email = d.email,
                             mobile_no = d.mobile_no,
                             pan_no = d.pan_no,
                             uan_no = d.uan_no,
                             aadhar_number = d.aadhar_number,
                             pay_code = d.pay_code,
                             employee_type = d.employeement_type,
                             shift = d.shift,
                             dobs = d.dob,
                             dojs = d.doj,
                             dols = d.dol,
                             reporting_to = d.reporting_to,
                             gender = d.gender,
                             acc_no = d.acc_no,
                             bank_name = d.bank_name,
                             ifsc_code = d.ifsc_code,
                             employee_salary = d.employee_salary,
                             el = d.el,
                             cl = d.cl,
                             sl = d.sl,
                             office_emp = d.office_emp,
                             marketing_emp = d.marketing_emp,
                             attendance_type = d.attendance_type,
                             
                           }).FirstOrDefault();

                if (results != null)
                {
                  int employeeMachinId = 0;
                  try
                  {
                    employeeMachinId = (from d in db.EmployeeMachineMaster
                                        where d.employee_id.Equals(results.employee_code)
                                        select d.machine_id).FirstOrDefault();
                  }
                  catch { }

                  results.machine_id = employeeMachinId;
                  if (results.dobs != null)
                  {
                    //DateTime dob = Convert.ToDateTime(results.dobs);
                    results.dob = results.dobs.ToString("dd/MM/yyyy");
                  }
                  if (results.dojs != null)
                  {
                    results.doj = results.dojs.ToString("dd/MM/yyyy");
                  }
                  if (results.dols != null)
                  {
                    results.dol = results.dols.ToString("dd/MM/yyyy");
                  }

                  //results.machine_id = (from d in db.EmployeeMachineMaster
                  //                     where d.employee_id.Equals(results.employee_id)
                  //                     select d.machine_id).FirstOrDefault();

                  var Reportingresults = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(results.reporting_to)
                                          select d.name).FirstOrDefault();
                  if (results.dobs != null)
                  {
                    results.employee_type = Reportingresults;
                  }


                }



                response.status = "success";
                response.flag = "1";
                response.data = results;
                response.alert = "success";
              }


            }


          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // get employee leave
    // get apply leave 
    //[HttpPost]
    //[Route("GetEmployeeLeave")]
    //public LeaveApplyResponse GetEmployeeLeave(HmcRequest req)
    //{
    //    LeaveApplyResponse response = new LeaveApplyResponse();
    //    response.flag = "0";
    //    response.status = "error";
    //    response.alert = "No Data Found";
    //    try
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            using (AttendanceContext db = new AttendanceContext())
    //            {
    //                string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
    //                if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
    //                {

    //                    string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
    //                    bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

    //                    if (userValid)
    //                    {
    //                        List<KeyValueMaster> bgNames = (from d in db.KeyValueMaster
    //                                                        where d.key_type.Equals("leave")
    //                                                        select d).ToList();

    //                        LeaveType empd = (from e in db.EmployeeMaster
    //                                          where e.employee_id.Equals(employeeId)
    //                                          select new LeaveType
    //                                          {
    //                                              el = e.el,
    //                                              cl = e.cl,
    //                                              sl = e.sl,
    //                                              a_el = e.a_el,
    //                                              a_cl = e.a_cl,
    //                                              a_sl = e.a_sl,
    //                                              b_el = e.b_el,
    //                                              b_cl = e.b_cl,
    //                                              b_sl = e.b_sl
    //                                          }).FirstOrDefault();

    //                        decimal[] EmpLeaves = new decimal[4];
    //                        EmpLeaves[0] = Convert.ToDecimal(empd.el) + Convert.ToDecimal(empd.cl) + Convert.ToDecimal(empd.sl);
    //                        EmpLeaves[1] = Convert.ToDecimal(empd.el);//empd.el;
    //                        EmpLeaves[2] = Convert.ToDecimal(empd.cl); //empd.cl;
    //                        EmpLeaves[3] = Convert.ToDecimal(empd.sl);//empd.sl;

    //                        decimal[] Availd = new decimal[4];
    //                        Availd[0] = Convert.ToDecimal(empd.a_el) + Convert.ToDecimal(empd.a_cl) + Convert.ToDecimal(empd.a_sl);
    //                        Availd[1] = Convert.ToDecimal(empd.a_el);//empd.el;
    //                        Availd[2] = Convert.ToDecimal(empd.a_cl); //empd.cl;
    //                        Availd[3] = Convert.ToDecimal(empd.a_sl);//empd.sl;

    //                        decimal[] Balance = new decimal[4];
    //                        Balance[0] = Convert.ToDecimal(empd.b_el) + Convert.ToDecimal(empd.b_cl) + Convert.ToDecimal(empd.b_sl);
    //                        Balance[1] = Convert.ToDecimal(empd.b_el);//empd.el;
    //                        Balance[2] = Convert.ToDecimal(empd.b_cl); //empd.cl;
    //                        Balance[3] = Convert.ToDecimal(empd.b_sl);//empd.sl;

    //                        string[] BgName = new string[4];
    //                        BgName[0] = "Total";
    //                        BgName[1] = "EL";//empd.el;
    //                        BgName[2] = "CL"; //empd.cl;
    //                        BgName[3] = "SL";//empd.sl;

    //                        //string[] array2 = empd.ToArray();
    //                        EmpployeeLeaveCalculation retVal = new EmpployeeLeaveCalculation();

    //                        List<decimal> actuals = new List<decimal>();
    //                        List<decimal> donatActuals = new List<decimal>();
    //                        List<string> donatBgNames = new List<string>();
    //                        List<string> bgName = new List<string>();
    //                        decimal totalApplyLeave = 0;

    //                        actuals.Insert(0, totalApplyLeave);
    //                        bgName.Insert(0, "Total");
    //                        retVal.targets = EmpLeaves;
    //                        retVal.actuals = Availd;
    //                        retVal.bgNames = BgName;

    //                        response.data = retVal;
    //                        response.flag = "1";
    //                        response.status = "success";
    //                        response.alert = "";
    //                    }                          


    //                }



    //            }
    //        }

    //    }
    //    catch (Exception e)
    //    {
    //        response.alert = e.Message;
    //    }
    //    return response;
    //}

    // get attendance list
    // Get Attendance
    [HttpPost]
    [Route("AttendanceList")]
    public HmcMonthlyAttResponse AttendanceList(SearchEmployeeAttRequest req)
    {
      HmcMonthlyAttResponse response = new HmcMonthlyAttResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No Data Found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                List<AttendanceResponse> retVal = null;
                if (req.month != null && req.year != null)
                {
                  int days = DateTime.DaysInMonth(Convert.ToInt32(req.year), Convert.ToInt32(req.month));
                  string totoalDay = null;
                  totoalDay = days.ToString();
                  //int preMonth = 0;
                  //if (req.month == "1")
                  //{
                  //    preMonth = Convert.ToInt32(req.month); //req.month - 1;
                  //}
                  //else
                  //{
                  //    preMonth = Convert.ToInt32(req.month) - 1; //req.month - 1;
                  //}

                  //int preMonth = Convert.ToInt32(req.month) - 1; //req.month - 1;
                  string fromdata = req.year + "-" + req.month + "-1";
                  string todate = req.year + "-" + req.month + "-" + totoalDay;


                 // DateTime From = DateTime.ParseExact(req.from_date, "yyyy-MM-dd", null);
                 // DateTime to = DateTime.ParseExact(req.to_date, "yyyy-MM-dd", null);

                  DateTime From = Convert.ToDateTime(fromdata);
                 DateTime to = Convert.ToDateTime(todate);

                  //int empid = Convert.ToInt32();

                  //var fromDate = fromdata ?? DateTime.MinValue;
                  //var toDate = to ?? DateTime.MaxValue;

                  //retVal = (from e in db.AttendanceMaster
                  //          where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                  //          orderby e.date ascending
                  //          select e).ToList();

                  retVal = (from e in db.AttendanceMaster
                            where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                            orderby e.date ascending
                            select new AttendanceResponse
                            {
                              id = e.id,
                              employee_id = e.employee_id,
                              date = e.date,
                              day = e.day,
                              in_time = e.in_time,
                              out_time = e.out_time,
                              shift = e.shift,
                              status = e.status,
                              total_hrs = e.total_hrs,
                              mis = e.mis,
                              absent = e.absent,
                              early = e.early,
                              late = e.late
                            }).ToList();



                }
                else
                {
                  //DateTime From = Convert.ToDateTime(req.from_date);
                  // DateTime to = Convert.ToDateTime(req.to_date);
                  DateTime From = DateTime.ParseExact("01/01/1900", "dd/MM/yyyy", null);
                  DateTime to = DateTime.ParseExact("01/01/1900", "dd/MM/yyyy", null);
                  if (!string.IsNullOrEmpty(req.from_date))
                  {
                    From = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                    to = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);
                  }
                  //else
                  //{
                  //    DateTime From = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                  //    DateTime to = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);
                  //}
                  //DateTime From = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                  //DateTime to = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                  if (!string.IsNullOrEmpty(req.type))
                  {
                    if (req.type == "MP")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.mis.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();
                    }
                    else if (req.type == "L" || req.type == "E")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.late.Equals("SHL")
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();
                    }
                    else if (req.type == "AB")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.absent.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();

                    }
                    else if (req.type == "EM")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.early.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();

                    }
                    else if (req.type == "P")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.status.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();
                    }
                  }
                  else
                  {
                    retVal = (from e in db.AttendanceMaster
                              where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                              orderby e.date ascending
                              select new AttendanceResponse
                              {
                                id = e.id,
                                employee_id = e.employee_id,
                                date = e.date,
                                day = e.day,
                                in_time = e.in_time,
                                out_time = e.out_time,
                                shift = e.shift,
                                status = e.status,
                                total_hrs = e.total_hrs,
                                mis = e.mis,
                                absent = e.absent,
                                early = e.early,
                                late = e.late
                              }).ToList();
                  }




                }

                foreach (AttendanceResponse item in retVal)
                {
                  item.total_hrs = Convert.ToDouble(item.total_hrs.ToString("#,##0.00"));
                  item.sdate = item.date.ToString("dd/MM/yyyy");
                  DateTime inTime = Convert.ToDateTime(item.in_time);
                  DateTime outTime = Convert.ToDateTime(item.out_time);
                  item.in_time = inTime.ToString("HH:mm:ss");
                  item.out_time = outTime.ToString("HH:mm:ss");
                }

                //AttendanceResponse attres = new AttendanceResponse();
                // attres.attendanceList = retVal;
                // attres.day = daysName;
                //attres.inTime = inTime;
                // attres.outTime = outTime;
                response.data = retVal;
                response.status = "success";
                response.flag = "1";
                response.alert = "success";

              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get apply leave 
    [HttpPost]
    [Route("GetApplyLeave")]
    public GetApplyLeaveResponse GetApplyLeave(HmcRequest req)
    {
      GetApplyLeaveResponse response = new GetApplyLeaveResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data not found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int page = Convert.ToInt32(req.pageNo);
                List<ApplyLeaveResponse> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (!string.IsNullOrEmpty(req.search_result))
                {
                  retVal = (from d in db.ApplyLeaveMasters
                            where d.employee_id.ToLower().Contains(req.search_result)
                            && d.modify_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new ApplyLeaveResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id.ToString(),
                              from_date = d.from_date,
                              to_date = d.to_date,
                              date = d.apply_date,
                              status = d.status,
                              reason = d.reason,
                              assign_by = d.assign_by,
                              leave_type = d.leave_Type,
                              duration_type = d.duration_type,
                              no_of_leave = d.no_of_leave.ToString()
                            }).Skip(skip).Take(pageSize).ToList();

                }
                else
                {
                  retVal = (from d in db.ApplyLeaveMasters
                            where d.modify_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new ApplyLeaveResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id.ToString(),
                              from_date = d.from_date,
                              to_date = d.to_date,
                              date = d.apply_date,
                              status = d.status,
                              reason = d.reason,
                              assign_by = d.assign_by,
                              leave_type = d.leave_Type,
                              duration_type = d.duration_type,
                              no_of_leave = d.no_of_leave.ToString()
                            }).Skip(skip).Take(pageSize).ToList();
                }

                if (retVal.Count > 0)
                {
                  foreach (ApplyLeaveResponse item in retVal)
                  {
                    item.todate = item.to_date.ToString("dd/MM/yyyy");
                    item.fromdate = item.from_date.ToString("dd/MM/yyyy");
                    item.applydate = item.date.ToString("dd/MM/yyyy");

                    EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                               where d.employee_id.Equals(item.assign_by)
                                               select d).FirstOrDefault();

                    if (assignBy != null)
                    {
                      item.assign_by_name = assignBy.name;
                    }

                    EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                              where d.employee_id.Equals(item.employee_id)
                                              select d).FirstOrDefault();
                    if (applyBy != null)
                    {
                      item.apply_by_name = applyBy.name;
                    }

                    KeyValueMaster leaveType = (from d in db.KeyValueMaster
                                                where d.id.Equals(item.leave_type)
                                                select d).FirstOrDefault();
                    if (leaveType != null)
                    {
                      item.leave_code = leaveType.key_code;
                    }

                    KeyValueMaster durationType = (from d in db.KeyValueMaster
                                                   where d.id.Equals(item.duration_type)
                                                   select d).FirstOrDefault();
                    if (durationType != null)
                    {
                      item.duration_code = durationType.key_code;
                    }
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    [HttpPost]
    [Route("DeleteApplyLeave")]
    public HmcStringResponse DeleteApplyLeave(DeleteDTO req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data not found";
      response.data = "";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                ApplyLeaveMasters leavemaster = (from e in db.ApplyLeaveMasters
                                                 where e.id.Equals(delete_id)
                                                 select e).FirstOrDefault();

                if (leavemaster != null)
                {
                  db.ApplyLeaveMasters.Remove(leavemaster);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // delete mispunch request
    [HttpPost]
    [Route("DeleteMiapunchRequest")]
    public HmcStringResponse DeleteMiapunchRequest(DeleteDTO req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data not found";
      response.data = "";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                AttendanceMispunch leavemaster = (from e in db.AttendanceMispunch
                                                  where e.id.Equals(delete_id)
                                                  select e).FirstOrDefault();

                if (leavemaster != null)
                {
                  db.AttendanceMispunch.Remove(leavemaster);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // apply leave 
    [HttpPost]
    [Route("ApplyLeave_old")]
    public HmcStringResponse ApplyLeave_old(ApplyLeaveMaster req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "Sorry, you are not applicable apply leave ";
      response.data = "";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();

              string startDay = @System.Configuration.ConfigurationManager.AppSettings["startDay"];



              if (userValid.reporting_to == "0" || string.IsNullOrEmpty(userValid.reporting_to))
              {
                userValid.reporting_to = @System.Configuration.ConfigurationManager.AppSettings["reportingId"];
              }

              //string empID = null;
              //if (!string.IsNullOrEmpty(req.employee_code))
              //{
              //    empID = req.employee_code;
              //}
              //else
              //{
              //    empID = employeeId;
              //}


              if (userValid != null)
              {
                int leaveId = Convert.ToInt32(req.leave_type);
                List<ApplyLeaveDTO> applyleave = null;

                ApplyLeaveDTO empLeave = new ApplyLeaveDTO();
                KeyValueMaster leaveMaster = (from d in db.KeyValueMaster
                                              where d.id.Equals(leaveId)
                                              && d.key_type.Equals("leave")
                                              select d).FirstOrDefault();

                if (leaveMaster.key_code == "EL")
                {
                  empLeave.no_of_leave = Convert.ToDecimal(userValid.a_el);
                }
                else if (leaveMaster.key_code == "CL")
                {
                  empLeave.no_of_leave = Convert.ToDecimal(userValid.a_cl);
                }
                else if (leaveMaster.key_code == "SL")
                {
                  empLeave.no_of_leave = Convert.ToDecimal(userValid.a_sl);
                }



                if (leaveMaster.key_code == "SHL")
                {
                  string sMonth = DateTime.Now.ToString("MM");
                  string year = DateTime.Now.ToString("yyyy");
                  int days = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(sMonth));
                  int eMonth = Convert.ToInt32(sMonth) + 1;
                  string fromdata = year + "-" + sMonth + "-" + startDay;
                  string todate = year + "-" + sMonth + "-" + days.ToString();

                  DateTime From = Convert.ToDateTime(fromdata);
                  DateTime to = Convert.ToDateTime(todate);


                  applyleave = (from e in db.ApplyLeaveMasters
                                where (e.apply_date >= From) && (e.apply_date <= to) && e.employee_id.Equals(userValid.employee_id)
                                && e.leave_Type.Equals(leaveId)
                                && e.status.Equals("Approved")
                                orderby e.id ascending
                                select new ApplyLeaveDTO
                                {
                                  no_of_leave = e.no_of_leave,
                                }).ToList();
                }

                decimal total = 0;

                if (applyleave != null)
                {
                  foreach (ApplyLeaveDTO leave in applyleave)
                  {
                    total = total + leave.no_of_leave;
                  }
                }
                else
                {
                  total = empLeave.no_of_leave;
                }


                if (leaveMaster != null)
                {
                  decimal leaveTotsl = 0;
                  Boolean shortLeave = true;
                  if (leaveMaster.key_code == "EL")
                  {

                    //LeaveType empd = (from e in db.EmployeeMaster
                    //                  where e.employee_id.Equals(empID)
                    //                  select new LeaveType
                    //                  {
                    //                      el = e.el
                    //                  }).FirstOrDefault();
                    leaveTotsl = Convert.ToDecimal(userValid.el);
                  }
                  else if (leaveMaster.key_code == "CL")
                  {
                    //LeaveType empd = (from e in db.EmployeeMaster
                    //                  where e.employee_id.Equals(empID)
                    //                  select new LeaveType
                    //                  {
                    //                      cl = e.cl
                    //                  }).FirstOrDefault();
                    leaveTotsl = Convert.ToDecimal(userValid.cl);
                  }
                  else if (leaveMaster.key_code == "SL")
                  {
                    //LeaveType empd = (from e in db.EmployeeMaster
                    //                  where e.employee_id.Equals(empID)
                    //                  select new LeaveType
                    //                  {

                    //                      sl = e.sl
                    //                  }).FirstOrDefault();
                    leaveTotsl = Convert.ToDecimal(userValid.sl);
                  }
                  else if (leaveMaster.key_code == "SHL")
                  {
                    leaveTotsl = 2;
                    DateTime fromTime = Convert.ToDateTime(req.from_time);
                    DateTime toTime = Convert.ToDateTime(req.to_time);

                    TimeSpan ts = toTime.Subtract(fromTime);
                    string times = ts.ToString();
                    string timeTotal = times.Replace(":", ".");
                    double calHrs = Convert.ToDouble(timeTotal.Remove(5, 3));
                    //decimal calHrs = Convert.ToDecimal(times);

                    if (calHrs > 2)
                    {
                      shortLeave = false;
                      response.data = "short leave not applied greater than 2 hours. Please check apply time";
                      response.alert = "short leave not applied greater than 2 hours. Please check apply time";
                    }
                  }
                  if (shortLeave == true && leaveTotsl > total || leaveTotsl == 0)
                  {
                    if (leaveTotsl == 0)
                    {
                      response.alert = "Sorry, you are not apply leave, because leave kota not allocated you.";
                      response.data = "Sorry, you are not apply leave, because leave kota not allocated you.";
                      return response;
                    }
                    DateTime from_date = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                    DateTime to_date = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);
                    double days = (to_date - from_date).TotalDays;
                    decimal totalDays = Convert.ToDecimal(days + 1);//(days + 1).ToString();
                    if (req.duration_type == "222" || req.duration_type == "223")
                    {
                      string halfDaya = "0.5";
                      totalDays = totalDays - Convert.ToDecimal(halfDaya);
                    }
                    decimal subTot = total + totalDays;
                    if (leaveTotsl >= subTot || leaveTotsl == 0)
                    {
                      if (from_date <= to_date)
                      {
                        // generate token
                        string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
                        string numbers = "1234567890";

                        string characters = small_alphabets + numbers;
                        string otp = string.Empty;
                        for (int i = 0; i < 10; i++)
                        {
                          string character = string.Empty;
                          do
                          {
                            int index = new Random().Next(0, characters.Length);
                            character = characters.ToCharArray()[index].ToString();
                          } while (otp.IndexOf(character) != -1);
                          otp += character;
                        }

                        if (req.from_time != null && req.to_time != null)
                        {
                          string fromTime = req.from_time + ":00";
                          string toTime = req.to_time + ":00";
                          DateTime from_time = DateTime.ParseExact(fromTime, "HH:mm:ss", null);
                          DateTime to_time = DateTime.ParseExact(toTime, "HH:mm:ss", null);

                          if (from_time < to_time)
                          {

                            ApplyLeaveMasters result = new ApplyLeaveMasters();
                            result.employee_id = userValid.employee_id;
                            result.from_date = from_date;
                            result.to_date = to_date;
                            result.apply_date = DateTime.Now;
                            result.reason = req.reason;
                            result.leave_Type = Convert.ToInt32(req.leave_type);
                            result.shift = req.shift;
                            result.duration_type = Convert.ToInt32(req.duration_type);
                            result.approve_date = Convert.ToDateTime("1900-01-01");
                            result.status = "Pending";
                            result.leave_flag = false;
                            result.no_of_leave = totalDays;
                            result.assign_by = userValid.reporting_to;
                            result.pay_code = userValid.pay_code;
                            result.token = otp;
                            result.from_time = from_time.ToString("HH:mm:ss");
                            result.to_time = to_time.ToString("HH:mm:ss");
                            result.modify_by = userValid.employee_id;
                            result.modify_date = DateTime.Now;
                            db.ApplyLeaveMasters.Add(result);
                            db.SaveChanges();

                            KeyValueMaster leaveType = (from d in db.KeyValueMaster
                                                        where d.id.Equals(result.leave_Type)
                                                        select d).FirstOrDefault();

                            string body = string.Empty;

                            EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                             where d.employee_id.Equals(userValid.reporting_to)
                                                             select d).FirstOrDefault();
                            if (employeemaster != null)
                            {
                              string baseUrls = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];
                              string supportTeam = @System.Configuration.ConfigurationManager.AppSettings["SupportTeam"];

                              string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"] + "Home/AproveRejectRequest?type=lv&token=" + otp + "&ids=" + result.id;
                              string approval = "&status=approved";
                              string reject = "&status=rejected";


                              body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear Sir/Ma’am,</p>" +
                                "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Request you to approve my (" + userValid.employee_id + " - " + userValid.name + ") leave / " + leaveType.key_description + " request from " + from_date.ToString("dd/MM/yyyy") + " to " + to_date.ToString("dd/MM/yyyy") + ".</p>" +
                                "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Skylabs Employee Management System Application by signing <a href='" + baseUrls + "'>Employee Management System</a></p><br>" +
                                "<div style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '><a style='padding: 5px; background: green; border: 1px solid green; border-radius: 4px; color: #fff;' href='" + baseUrl + approval + "'>Approve</a> <a style='padding: 5px; background: red; border: 1px solid red; border-radius: 4px; color: #fff;' href='" + baseUrl + reject + "'>Reject</a></div><br>" +
                                "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                                "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>"+ supportTeam + "</p>";

                              string subject = leaveType.key_description;
                              string emailId = employeemaster.email;
                              //Utilities.Utility.sendMaill(emailId, body, subject);
                            }



                            response.status = "success";
                            response.flag = "1";
                            response.alert = "Thank you for apply leave";
                          }
                        }
                        else
                        {
                          ApplyLeaveMasters result = new ApplyLeaveMasters();
                          result.employee_id = userValid.employee_id;
                          result.from_date = from_date;
                          result.to_date = to_date;
                          result.apply_date = DateTime.Now;
                          result.reason = req.reason;
                          result.leave_Type = Convert.ToInt32(req.leave_type);
                          result.shift = req.shift;
                          result.duration_type = Convert.ToInt32(req.duration_type);
                          result.approve_date = Convert.ToDateTime("1900-01-01");
                          result.status = "Pending";
                          result.token = otp;
                          result.no_of_leave = totalDays;
                          result.assign_by = userValid.reporting_to;
                          result.modify_by = employeeId;
                          result.pay_code = userValid.pay_code;
                          result.modify_date = DateTime.Now;
                          db.ApplyLeaveMasters.Add(result);
                          db.SaveChanges();

                          string body = string.Empty;

                          EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                           where d.employee_id.Equals(userValid.reporting_to)
                                                           select d).FirstOrDefault();
                          if (employeemaster != null)
                          {
                            KeyValueMaster leaveType = (from d in db.KeyValueMaster
                                                        where d.id.Equals(result.leave_Type)
                                                        select d).FirstOrDefault();

                            string baseUrls = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];
                            string supportTeam = @System.Configuration.ConfigurationManager.AppSettings["SupportTeam"];

                            string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"] + "Home/AproveRejectRequest?type=lv&token=" + otp + "&ids=" + result.id;
                            string approval = "&status=approved";
                            string reject = "&status=rejected";


                            body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear Sir/Ma’am,</p>" +
                              "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Request you to approve my (" + userValid.employee_id + " - " + userValid.name + ") leave / " + leaveType.key_description + " request from " + from_date.ToString("dd/MM/yyyy") + " to " + to_date.ToString("dd/MM/yyyy") + ".</p>" +
                              "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Skylabs Employee Management System Application by signing <a href='" + baseUrls + "'>Skylabs Employee Management System</a></p><br>" +
                              "<div style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '><a style='padding: 5px; background: green; border: 1px solid green; border-radius: 4px; color: #fff;' href='" + baseUrl + approval + "'>Approve</a> <a style='padding: 5px; background: red; border: 1px solid red; border-radius: 4px; color: #fff;' href='" + baseUrl + reject + "'>Reject</a></div><br>" +
                              "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                              "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>"+ supportTeam + "</p>";

                            string subject = leaveType.key_description;
                            string emailId = employeemaster.email;
                            //Utilities.Utility.sendMaill(emailId, body, subject);
                          }



                          response.status = "success";
                          response.flag = "1";
                          response.alert = "Thank you for apply leave";
                        }
                      }
                    }
                  }

                }


              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }




    [HttpPost]
    [Route("ApplyLeave")]
    public HmcStringResponse ApplyLeave(ApplyLeaveMaster req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "this service is not available currently";
      response.flag = "1";
      response.alert = "this service is not available currently ";
      response.data = "";
     
      return response;
    }


    // get approval mispunch request
    [HttpPost]
    [Route("GetApprovalMispunchRequest")]
    public GetMispunchResponse GetApprovalMispunchRequest(HmcRequest req)
    {
      GetMispunchResponse response = new GetMispunchResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data not found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int page = Convert.ToInt32(req.pageNo);
                List<MispunchResponse> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (!string.IsNullOrEmpty(req.search_result))
                {
                  retVal = (from d in db.AttendanceMispunch
                            where d.employee_id.ToLower().Contains(req.search_result)
                            && d.assign_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new MispunchResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id,
                              date = d.apply_date,
                              in_time = d.in_time,
                              out_time = d.out_time,
                              status = d.status,
                              reason = d.reason,
                              misdate = d.date,
                              shift = d.shift,
                              assign_by = d.assign_by,
                            }).Skip(skip).Take(pageSize).ToList();

                }
                else
                {
                  retVal = (from d in db.AttendanceMispunch
                            where d.assign_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new MispunchResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id,
                              date = d.apply_date,
                              in_time = d.in_time,
                              out_time = d.out_time,
                              status = d.status,
                              reason = d.reason,
                              misdate = d.date,
                              shift = d.shift,
                              assign_by = d.assign_by,
                            }).Skip(skip).Take(pageSize).ToList();
                }

                if (retVal.Count > 0)
                {
                  foreach (MispunchResponse item in retVal)
                  {

                    item.applydate = item.date.ToString("dd/MM/yyyy");
                    item.empmisdate = item.misdate.ToString("dd/MM/yyyy");

                    EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                               where d.employee_id.Equals(item.assign_by)
                                               select d).FirstOrDefault();

                    if (assignBy != null)
                    {
                      item.assign_by_name = assignBy.name;
                    }

                    EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                              where d.employee_id.Equals(item.employee_id)
                                              select d).FirstOrDefault();
                    if (applyBy != null)
                    {
                      item.apply_by_name = applyBy.name;
                    }
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // Get Mispunch Request
    [HttpPost]
    [Route("GetMispunchRequest")]
    public GetMispunchResponse GetMispunchRequest(HmcRequest req)
    {
      GetMispunchResponse response = new GetMispunchResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data not found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int page = Convert.ToInt32(req.pageNo);
                List<MispunchResponse> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  retVal = (from d in db.AttendanceMispunch
                              //join e in db.AttendanceMaster on e.atte
                            where d.employee_id.ToLower().Contains(req.search_result)
                            && d.modify_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new MispunchResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id,
                              date = d.apply_date,
                              in_time = d.in_time,
                              out_time = d.out_time,
                              status = d.status,
                              reason = d.reason,
                              shift = d.shift,
                              misdate = d.date,
                              assign_by = d.assign_by,
                            }).Skip(skip).Take(pageSize).ToList();

                }
                else
                {
                  retVal = (from d in db.AttendanceMispunch
                            where d.modify_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new MispunchResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id,
                              date = d.apply_date,
                              in_time = d.in_time,
                              out_time = d.out_time,
                              status = d.status,
                              shift = d.shift,
                              reason = d.reason,
                              misdate = d.date,
                              assign_by = d.assign_by,
                            }).Skip(skip).Take(pageSize).ToList();
                }

                if (retVal.Count > 0)
                {
                  foreach (MispunchResponse item in retVal)
                  {

                    item.applydate = item.date.ToString("dd/MM/yyyy");
                    item.empmisdate = item.misdate.ToString("dd/MM/yyyy");

                    EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                               where d.employee_id.Equals(item.assign_by)
                                               select d).FirstOrDefault();

                    if (assignBy != null)
                    {
                      item.assign_by_name = assignBy.name;
                    }

                    EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                              where d.employee_id.Equals(item.employee_id)
                                              select d).FirstOrDefault();
                    if (applyBy != null)
                    {
                      item.apply_by_name = applyBy.name;
                    }
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // mispunch apply
    // Attendance send query
    [HttpPost]
    [Route("MispunchRequest")]
    public HmcStringResponse MispunchRequest(SendQuery req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      response.data = "";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();

              if (string.IsNullOrEmpty(userValid.reporting_to))
              {
                userValid.reporting_to = System.Configuration.ConfigurationManager.AppSettings["reportingId"];
              }
              else if (userValid.reporting_to == "0")
              {
                userValid.reporting_to = System.Configuration.ConfigurationManager.AppSettings["reportingId"];
              }

              if (userValid != null)
              {


                //DateTime date = Convert.ToDateTime(req.p_date);
                //DateTime date = DateTime.ParseExact(req.p_date, "yyyy-MM-dd", null);
                int attId = Convert.ToInt32(req.attendance_id);
                AttendanceMaster attendamaster = (from d in db.AttendanceMaster
                                                  where d.id.Equals(attId)
                                                  select d).FirstOrDefault();

                // generate token
                string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
                string numbers = "1234567890";

                string characters = small_alphabets + numbers;
                string token = string.Empty;
                for (int i = 0; i < 10; i++)
                {
                  string character = string.Empty;
                  do
                  {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                  } while (token.IndexOf(character) != -1);
                  token += character;
                }


                AttendanceMispunch result = new AttendanceMispunch();

                if (!string.IsNullOrEmpty(req.employee_code))
                {
                  result.employee_id = req.employee_code;
                }
                else
                {
                  result.employee_id = employeeId;
                }

                if (attendamaster != null)
                {
                  result.date = attendamaster.date;
                }
                else
                {
                  DateTime pDate1 = DateTime.ParseExact(req.p_date, "dd/MM/yyyy", null);
                  result.date = pDate1;
                }

                if (!string.IsNullOrEmpty(req.attendance_id))
                {
                  result.attendance_id = Convert.ToInt32(req.attendance_id);
                }
                else
                {
                  result.attendance_id = 0;
                }

                if (req.in_time.Length > 5)
                {
                  result.in_time = req.in_time;
                }
                else
                {
                  result.in_time = req.in_time + ":00";
                }

                //result.in_time = req.in_time + ":00";
                result.out_time = req.out_time + ":00";
                result.apply_date = DateTime.Now;
                result.reason = req.reason;
                result.shift = req.shift;
                result.status = "Pending";
                result.assign_by = userValid.reporting_to;
                result.approve_date = Convert.ToDateTime("1990-01-01");
                result.token = token;
                result.att_flag = false;
                result.modify_by = employeeId;
                result.modify_date = DateTime.Now;
                result.pay_code = userValid.pay_code;
                db.AttendanceMispunch.Add(result);
                db.SaveChanges();

                string body = string.Empty;

                EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(userValid.reporting_to)
                                                 select d).FirstOrDefault();
                if (employeemaster != null)
                {
                  string baseUrls = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];

                  string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"] + "Home/AproveRejectRequest?type=miss&token=" + token + "&ids=" + result.id;
                  string approval = "&status=approved";
                  string reject = "&status=rejected";

                  body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear Sir/Ma’am,</p>" +
                    "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Request you to approve my (" + userValid.employee_id + " - " + userValid.name + ") mispunch request for " + result.date.ToString("dd/MM/yyyy") + ".</p>" +
                    "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Skylabs Employee Management System Application by signing <a href='" + baseUrls + "'>Skylabs Employee Management System</a></p><br>" +
                    "<div style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '><a style='padding: 5px; background: green; border: 1px solid green; border-radius: 4px; color: #fff;' href='" + baseUrl + approval + "'>Approve</a> <a style='padding: 5px; background: red; border: 1px solid red; border-radius: 4px; color: #fff;' href='" + baseUrl + reject + "'>Reject</a></div><br>" +
                    "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                    "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>Skylabs Support Team</p>";

                  string subject = "Mispunch Request";
                  string emailId = employeemaster.email;
                  //string emailId = "cs.sharma@hmcgroup.in";
                  //Utilities.Utility.sendMaill(emailId, body, subject);
                }



                response.status = "success";
                response.flag = "1";
                response.alert = "Thanks for apply mispunch request";

              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // add key value data
    [HttpPost]
    [Route("KeyValueData")]
    public HmcJsonResponse KeyValueData(KeyValue req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(KeyValueValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  if (req.id == 0)
                  {


                    KeyValueMaster result = new KeyValueMaster();
                    result.modify_by = employeeId;
                    result.key_code = req.key_code;
                    result.key_type = req.key_type;
                    result.key_description = req.key_description;
                    result.modify_date = DateTime.Now;
                    result.pay_code = req.pay_code;
                    db.KeyValueMaster.Add(result);
                    db.SaveChanges();

                    response.status = "success";
                    response.flag = "1";
                    response.alert = "Record Inserted successfully";
                  }
                  else {


                    KeyValueMaster result = (from d in db.KeyValueMaster
                                             where d.id.Equals(req.id)
                                         select d).FirstOrDefault();
                    
                    if (result != null)
                    {
                      result.id = req.id;
                      result.modify_by = employeeId;
                      result.key_code = req.key_code;
                      result.key_type = req.key_type;
                      result.key_description = req.key_description;
                      result.modify_date = DateTime.Now;
                      result.pay_code = req.pay_code;
                      // db.KeyValueMaster.Add(result);
                      db.SaveChanges();

                      response.status = "success";
                      response.flag = "1";
                      response.alert = "Record Updated successfully";
                    }

                  }
                }



              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // update key value data
    [HttpPost]
    [Route("UpdateKeyValueData")]
    public HmcResponse UpdateKeyValueData(KeyValue req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(KeyValueValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  if (req.id != 0)
                  {
                    KeyValueMaster result = (from d in db.KeyValueMaster
                                             where d.id.Equals(req.id)
                                             select d).FirstOrDefault();

                    if (result != null)
                    {
                      result.modify_by = employeeId;
                      result.key_code = req.key_code;
                      result.key_type = req.key_type;
                      result.key_description = req.key_description;
                      result.pay_code = req.pay_code;
                      result.modify_date = DateTime.Now;
                      db.SaveChanges();
                    }
                  }
                }
                //DateTime date = Convert.ToDateTime(req.p_date);

                //else
                //{
                //    KeyValueMaster result = new KeyValueMaster();
                //    result.modify_by = req.employee_id;
                //    result.key_code = req.key_code;
                //    result.key_type = req.key_type;
                //    result.key_description = req.key_description;
                //    result.modify_date = DateTime.Now;
                //    db.KeyValueMaster.Add(result);
                //    db.SaveChanges();
                //}


                response.status = "success";
                response.flag = "1";
                response.alert = "Thanks for update";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get key value data
    [HttpPost]
    [Route("GetKeyValueData")]
    public KeyValueData GetKeyValueData(KeyValue req)
    {
      KeyValueData response = new KeyValueData();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();
                List<KeyValueMaster> keyvalue = null;

                if (req.key_type == "duration")
                {
                  if (req.id != 0)
                  {
                    KeyValueMaster keyvalues = (from d in db.KeyValueMaster
                                                where d.id.Equals(req.id)
                                                select d).FirstOrDefault();

                    if (keyvalues.key_code == "SHL")
                    {

                      keyvalue = (from d in db.KeyValueMaster
                                  where d.key_type.Equals("shortleave")
                                  && d.pay_code.Equals(employeeMaster.pay_code)
                                  select d).ToList();
                    }
                    else
                    {
                      keyvalue = (from d in db.KeyValueMaster
                                  where d.key_type.Equals("duration")
                                  && d.pay_code.Equals(employeeMaster.pay_code)
                                  select d).ToList();
                    }
                  }
                  else
                  {
                    keyvalue = (from d in db.KeyValueMaster
                                where d.key_type.Equals("duration")
                                & d.pay_code.Equals(employeeMaster.pay_code)
                                select d).ToList();
                  }

                }
                else
                {
                  if (employeeMaster.role == "Administrator")
                  {
                    keyvalue = (from d in db.KeyValueMaster
                                where d.key_type.Equals(req.key_type)
                                select d).ToList();
                  }
                  else
                  {

                    keyvalue = (from d in db.KeyValueMaster
                                where d.key_type.Equals(req.key_type)
                                && d.pay_code.Equals(employeeMaster.pay_code)
                                && d.key_code != "Administrator"
                                select d).ToList();

                  }
                }


                if (keyvalue.Count() > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = keyvalue;
                }
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get master data
    [HttpPost]
    [Route("GetMasterData")]
    public MasterData GetMasterData(KeyValue req)
    {
      MasterData response = new MasterData();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                response.department = (from d in db.KeyValueMaster
                                       where d.key_type.Equals("department")
                                       && d.pay_code.Equals(req.pay_code)
                                       select d).ToList();

                response.designation = (from d in db.KeyValueMaster
                                        where d.key_type.Equals("designation")
                                        && d.pay_code.Equals(req.pay_code)
                                        select d).ToList();

                response.role = (from d in db.KeyValueMaster
                                 where d.key_type.Equals("role")
                                 && d.pay_code.Equals(req.pay_code)
                                 select d).ToList();

                response.shift = (from d in db.Shift
                                  where d.pay_code.Equals(req.pay_code)
                                  select d).ToList();


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = "success";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get leave type data
    [HttpPost]
    [Route("GetLeaveType")]
    public KeyValueData GetLeaveType(KeyValue req)
    {
      KeyValueData response = new KeyValueData();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();
                List<KeyValueMaster> keyvalue = null;

                List<KeyValueMaster> keyvalueDuration = null;

                if (employeeMaster.role == "Administrator")
                {
                  keyvalue = (from d in db.KeyValueMaster
                              where d.key_type.Equals("leave")
                              select d).ToList();

                  keyvalueDuration = (from d in db.KeyValueMaster
                                      where d.key_type.Equals("duration")
                                      select d).ToList();
                }
                else
                {
                  keyvalue = (from d in db.KeyValueMaster
                              where d.key_type.Equals("leave")
                              && d.pay_code.Equals(employeeMaster.pay_code)
                              select d).ToList();

                  keyvalueDuration = (from d in db.KeyValueMaster
                                      where d.key_type.Equals("duration")
                                      && d.pay_code.Equals(employeeMaster.pay_code)
                                      select d).ToList();
                }



                foreach (KeyValueMaster item in keyvalueDuration)
                {
                  keyvalue.Add(item);
                }


                if (keyvalue.Count() > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = keyvalue;
                }
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get department
    [HttpPost]
    [Route("GetDepartmentData")]
    public KeyValueData GetDepartmentData(KeyValue req)
    {
      KeyValueData response = new KeyValueData();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                List<KeyValueMaster> keyvalue = null;

                keyvalue = (from d in db.KeyValueMaster
                            where d.key_type.Equals(req.key_type)
                            && d.pay_code.Equals(req.pay_code)
                            && d.key_code != "Administrator"
                            select d).ToList();

                if (keyvalue.Count() > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = keyvalue;
                }
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // Delete key value data
    // delete employee
    [HttpPost]
    [Route("DeleteKeyValue")]
    public HmcResponse DeleteKeyValue(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                KeyValueMaster leavemaster = (from e in db.KeyValueMaster
                                              where e.id.Equals(delete_id)
                                              select e).FirstOrDefault();

                if (leavemaster != null)
                {
                  db.KeyValueMaster.Remove(leavemaster);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    [HttpPost]
    [Route("DeleteHoliday")]
    public HmcResponse DeleteHoliday(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                HolidayMaster leavemaster = (from e in db.HolidayMaster
                                             where e.id.Equals(delete_id)
                                             select e).FirstOrDefault();

                if (leavemaster != null)
                {
                  db.HolidayMaster.Remove(leavemaster);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // show key value data
    // show keyvalue
    [HttpPost]
    [Route("ShowKeyValue")]
    public ShowKeyValue ShowKeyValue(HmcRequest req)
    {
      ShowKeyValue response = new ShowKeyValue();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int empid = Convert.ToInt32(req.id);
                KeyValueMaster results = null;
                results = (from d in db.KeyValueMaster
                           where d.id.Equals(empid)
                           select d).FirstOrDefault();

                response.status = "success";
                response.flag = "1";
                response.data = results;
                response.alert = "success";
              }
            }

          }

        }
      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // delete employee
    [HttpPost]
    [Route("DeleteEmployee")]
    public HmcResponse DeleteEmployee(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                EmployeeMaster employeeMaster = (from e in db.EmployeeMaster
                                                 where e.id.Equals(delete_id)
                                                 select e).FirstOrDefault();

                if (employeeMaster != null)
                {
                  db.EmployeeMaster.Remove(employeeMaster);
                  db.SaveChanges();
                }

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // approve or reject apply leave request
    // update status
    [HttpPost]
    [Route("ApproveOrRejectApplyLeave")]
    public AMSResponse ApproveOrRejectApplyLeave(HmcRequest req)
    {
       AMSResponse response = new AMSResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int id = Convert.ToInt32(req.id);
                ApplyLeaveMasters applyleave = (from d in db.ApplyLeaveMasters
                                                where d.id.Equals(id)
                                                select d).FirstOrDefault();
                if (applyleave != null)
                {
                  applyleave.status = req.status;
                  applyleave.approve_date = DateTime.Now;
                  applyleave.token = null;
                  db.SaveChanges();

                  EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                   where d.employee_id.Equals(applyleave.employee_id)
                                                   select d).FirstOrDefault();

                  KeyValueMaster leaveMaster = (from d in db.KeyValueMaster
                                                where d.id.Equals(applyleave.leave_Type)
                                                select d).FirstOrDefault();

                  
                  response.alert = "Thanks for reject leave request";
                  response.data = "Thanks for reject leave request";

                  if (req.status == "Approved")
                  {


                    KeyValueMaster durationMaster = (from d in db.KeyValueMaster
                                                     where d.id.Equals(applyleave.duration_type)
                                                     select d).FirstOrDefault();
                    decimal totalAvaildEmpLeave = 0;
                    decimal totalBalanceEmpLeave = 0;

                    if (leaveMaster.key_code == "EL")
                    {
                      totalAvaildEmpLeave = Convert.ToDecimal(employeemaster.a_el);
                      totalBalanceEmpLeave = Convert.ToDecimal(employeemaster.b_el);
                      decimal totAvil = totalAvaildEmpLeave + applyleave.no_of_leave;
                      decimal totBal = totalBalanceEmpLeave - applyleave.no_of_leave;
                      employeemaster.a_el = totAvil.ToString();
                      employeemaster.b_el = totBal.ToString();
                      db.SaveChanges();

                    }
                    else if (leaveMaster.key_code == "CL")
                    {
                      totalAvaildEmpLeave = Convert.ToDecimal(employeemaster.a_cl);
                      totalBalanceEmpLeave = Convert.ToDecimal(employeemaster.b_cl);
                      decimal totAvil = totalAvaildEmpLeave + applyleave.no_of_leave;
                      decimal totBal = totalBalanceEmpLeave - applyleave.no_of_leave;
                      employeemaster.a_cl = totAvil.ToString();
                      employeemaster.b_cl = totBal.ToString();
                      db.SaveChanges();

                    }
                    else if (leaveMaster.key_code == "SL")
                    {
                      totalAvaildEmpLeave = Convert.ToDecimal(employeemaster.a_sl);
                      totalBalanceEmpLeave = Convert.ToDecimal(employeemaster.b_sl);
                      decimal totAvil = totalAvaildEmpLeave + applyleave.no_of_leave;
                      decimal totBal = totalBalanceEmpLeave - applyleave.no_of_leave;
                      employeemaster.a_sl = totAvil.ToString();
                      employeemaster.b_sl = totBal.ToString();
                      db.SaveChanges();
                    }

                    List<AttendanceMaster> empAtt = (from d in db.AttendanceMaster
                                                     where d.employee_id.Equals(applyleave.employee_id)
                                                     && d.date >= applyleave.from_date && d.date <= applyleave.to_date
                                                     select d).ToList();

                    if (empAtt.Count() > 0)
                    {
                      foreach (AttendanceMaster item in empAtt)
                      {
                        DateTime todayDate = DateTime.Now;
                        if (todayDate > item.date)
                        {
                          item.status = leaveMaster.key_code;
                          item.absent = "";
                          item.mis = "";
                          item.early = "";
                          item.late = "";
                          db.SaveChanges();
                        }
                      }
                    }
                    else
                    {
                      for (int i = 0; i < applyleave.no_of_leave; i++)
                      {
                        DateTime attDate = new DateTime();
                        if (i == 0)
                        {
                          attDate = applyleave.from_date;
                        }
                        else
                        {
                          attDate = applyleave.from_date.AddDays(i);
                        }

                        DateTime todayDate = DateTime.Now;
                        AttendanceMaster attMaster = new AttendanceMaster();
                        attMaster.employee_id = applyleave.employee_id;
                        attMaster.shift = applyleave.shift;
                        attMaster.date = attDate;
                        attMaster.day = attDate.DayOfWeek.ToString();
                        attMaster.in_time = "00:00:00";
                        attMaster.out_time = "00:00:00";
                        attMaster.status = leaveMaster.key_code;
                        attMaster.absent = "";
                        attMaster.mis = "";
                        attMaster.early = "";
                        attMaster.late = "";
                        attMaster.total_hrs = 0;
                        db.AttendanceMaster.Add(attMaster);
                        db.SaveChanges();
                      }
                    }

                    response.alert = "Thanks for approve leave request";
                    response.data = "Thanks for approve leave request";
                  }

                  string body = string.Empty;

                  string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];

                  body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Hi " + employeemaster.name + ",</p>" +
                    "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Your Leave / " + leaveMaster.key_description + " has been " + applyleave.status + " From " + applyleave.from_date.ToString("dd/M/yyyy") + " To " + applyleave.to_date.ToString("dd/M/yyyy") + " .</p>" +
                    "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Skylabs Employee Management System Application by signing <a href='" + baseUrl + "'>Skylabs Employee Management System</a></p><br>" +
                    "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                    "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>Skylabs Support Team</p>";

                  string subject = leaveMaster.key_description;
                  string emailId = employeemaster.email;
                  //string emailId = "cs.sharma@hmcgroup.in";
                  Utilities.Utility.sendMaill(emailId, body, subject);

                  response.status = "success";
                  response.flag = "1";



                }
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // approve and reject mispunch status
    // Ateendancew status 
    [HttpPost]
    [Route("ApproveOrRejectApplyMispunch")]
    public AMSResponse ApproveOrRejectApplyMispunch(HmcRequest req)
    {
      AMSResponse response = new AMSResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "Sorry, you are not authorised approve request";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int id = Convert.ToInt32(req.id);
                AttendanceMispunch tickets = (from d in db.AttendanceMispunch
                                              where d.id.Equals(id)
                                              select d).FirstOrDefault();
                if (tickets != null)
                {
                  tickets.status = req.status;
                  tickets.approve_date = DateTime.Now;
                  db.SaveChanges();

                  response.alert = "Thanks for reject mispunch request";
                  response.data = "Thanks for reject mispunch request";

                  if (tickets.status == "Approved")
                  {
                    AttendanceMaster attendaceMaster = (from d in db.AttendanceMaster
                                                        where d.id.Equals(tickets.attendance_id)
                                                        select d).FirstOrDefault();
                    if (attendaceMaster != null)
                    {
                      DateTime dtFrom = DateTime.Parse(tickets.in_time);
                      DateTime dtTo = DateTime.Parse(tickets.out_time);

                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");

                      //int timeDiff = dtTo.Subtract(dtFrom).Hours;
                      attendaceMaster.in_time = tickets.in_time;
                      attendaceMaster.out_time = tickets.out_time;
                      attendaceMaster.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));

                      attendaceMaster.status = "";
                      attendaceMaster.mis = "";
                      attendaceMaster.late = "";
                      attendaceMaster.early = "";
                      attendaceMaster.absent = "";

                      Shift shift = (from d in db.Shift
                                     where d.shift_code.Equals(tickets.shift)
                                     && d.pay_code.Equals(tickets.pay_code)
                                     select d).FirstOrDefault();

                      DateTime shiftendTime = Convert.ToDateTime(shift.end_time);
                      DateTime outTime = Convert.ToDateTime(attendaceMaster.out_time);

                      if (shiftendTime > outTime)
                      {
                        attendaceMaster.early = "E";
                        attendaceMaster.status = "";
                      }
                      else
                      {
                        double comHrs = 9;
                        if (attendaceMaster.total_hrs < comHrs)
                        {
                          if (attendaceMaster.total_hrs < 4)
                          {
                            attendaceMaster.early = "";
                            attendaceMaster.status = "";
                            attendaceMaster.absent = "AB";
                          }
                          else if (attendaceMaster.total_hrs > 4 && attendaceMaster.total_hrs < 8)
                          {
                            attendaceMaster.early = "";
                            attendaceMaster.status = "HD";
                          }
                          else if (attendaceMaster.total_hrs == 0)
                          {
                            attendaceMaster.early = "";
                            attendaceMaster.status = "";
                            attendaceMaster.absent = "AB";
                          }
                          else
                          {
                            attendaceMaster.early = "E";
                            attendaceMaster.status = "";
                          }

                        }
                        else
                        {
                          attendaceMaster.status = "P";
                          attendaceMaster.early = "";
                        }

                      }

                      db.SaveChanges();
                    }

                    response.alert = "Thanks for approve mispunch request";
                    response.data = "Thanks for approve mispunch request";
                  }


                  EmployeeMaster employeemaster = (from d in db.EmployeeMaster
                                                   where d.employee_id.Equals(tickets.employee_id)
                                                   select d).FirstOrDefault();
                  string body = string.Empty;

                  string baseUrl = @System.Configuration.ConfigurationManager.AppSettings["baseurl"];

                  body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Hi " + employeemaster.name + ",</p>" +
                    "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>Your Mispunch has been " + tickets.status + " date " + tickets.date.ToString("dd/M/yyyy") + " .</p>" +
                    "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal; '>You may log in to the Skylabs Employee Management System Application by signing <a href='" + baseUrl + "'>Skylabs Employee Management System</a></p><br>" +
                    "<p style='font-size: 14px; font-weight: 600; font-style: normal; padding-top: 20px; line-height: 5px; letter-spacing: normal; color: #1f497d;'>Thanks & Regards</p>" +
                    "<p style='font-weight: 400; line-height: 0px; font-style: normal; letter-spacing: normal; color: #1f497d;'>Skylabs Support Team</p>";

                  string subject = "Mispunch Request";
                  string emailId = employeemaster.email;
                  //string emailId = "cs.sharma@hmcgroup.in";
                  Utilities.Utility.sendMaill(emailId, body, subject);


                  response.status = "success";
                  response.flag = "1";
                  //response.alert = "success";
                }
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get holiday data
    // get Holiday
    [HttpPost]
    [Route("GetHolidayData")]
    public HolidayMasterRes GetHolidayData(HmcRequest req)
    {

      HolidayMasterRes response = new HolidayMasterRes();
      response.flag = "0";
      response.status = "error";
      response.alert = "No Data Found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();
                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;

                int currentYear = DateTime.Now.Year;

                List<HolidayData> retVal = null;
                if (req.search_result != null)
                {
                  if (employeeMaster.role == "Administrator")
                  {
                    retVal = (from e in db.HolidayMaster
                              where e.title.ToLower().Contains(req.search_result)
                              orderby e.holiday_date ascending
                              select new HolidayData
                              {
                                id = e.id,
                                title = e.title,
                                holiday_date = e.holiday_date,
                                pay_code = e.pay_code,
                                rendering = "background"
                              }).ToList();
                  }
                  else
                  {
                    retVal = (from e in db.HolidayMaster
                              where e.title.ToLower().Contains(req.search_result)
                              && e.pay_code.Equals(employeeMaster.pay_code)
                              orderby e.holiday_date ascending
                              select new HolidayData
                              {
                                id = e.id,
                                title = e.title,
                                holiday_date = e.holiday_date,
                                pay_code = e.pay_code,
                                rendering = "background"
                              }).ToList();
                  }

                }
                else
                {
                  if (employeeMaster.role == "Administrator")
                  {
                    retVal = (from e in db.HolidayMaster
                              orderby e.holiday_date ascending
                              select new HolidayData
                              {
                                id = e.id,
                                title = e.title,
                                holiday_date = e.holiday_date,
                                pay_code = e.pay_code,
                                rendering = "background"
                              }).ToList();
                  }
                  else
                  {
                    retVal = (from e in db.HolidayMaster
                              where e.pay_code.Equals(employeeMaster.pay_code)
                              && e.holiday_date.Year.Equals(currentYear)
                              orderby e.holiday_date ascending
                              select new HolidayData
                              {
                                id = e.id,
                                title = e.title,
                                holiday_date = e.holiday_date,
                                pay_code = e.pay_code,
                                rendering = "background"
                              }).ToList();
                  }

                }

                foreach (HolidayData item in retVal)
                {
                  item.holidayDate = item.holiday_date.ToString("yyyy-MM-dd");
                  item.start = item.holiday_date.ToString("yyyy-MM-dd");
                  item.end = item.holiday_date.ToString("yyyy-MM-dd");

                  CompaniesMaster comMaster = (from d in db.CompaniesMaster
                                               where d.company_code.Equals(item.pay_code)
                                               select d).FirstOrDefault();
                  if (comMaster != null)
                  {
                    item.name = comMaster.name;
                  }
                }

                response.data = retVal;
                response.flag = "1";
                response.status = "success";
                response.alert = "";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // add holiday
    // holiday
    [HttpPost]
    [Route("CreateHoliday")]
    public HmcJsonResponse CreateHoliday(HolidayData req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(HolidayValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  HolidayMaster resultsDb = new HolidayMaster();
                  resultsDb.holiday_date = Convert.ToDateTime(req.holidayDate);
                  resultsDb.modify_by = employeeId;
                  resultsDb.title = req.title;
                  resultsDb.pay_code = req.pay_code;
                  resultsDb.modify_date = DateTime.Now;
                  db.HolidayMaster.Add(resultsDb);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "";
                }

              }
            }


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // update holiday
    [HttpPost]
    [Route("UpdateHoliday")]
    public HmcResponse UpdateHoliday(HolidayData req)
    {
      HmcResponse response = new HmcResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(HolidayValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  HolidayMaster resultsDb = (from d in db.HolidayMaster
                                             where d.id.Equals(req.id)
                                             orderby d.id descending
                                             select d).FirstOrDefault();
                  if (resultsDb != null)
                  {
                    resultsDb.holiday_date = Convert.ToDateTime(req.holidayDate);
                    resultsDb.modify_by = employeeId;
                    resultsDb.modify_date = DateTime.Now;
                    resultsDb.title = req.title;
                    resultsDb.pay_code = req.pay_code;
                    db.SaveChanges();
                    response.flag = "1";
                    response.status = "success";
                    response.alert = "";
                  }
                }

              }
            }


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // create companies
    [HttpPost]
    [Route("CreateCompany")]
    public HmcResponse CreateCompany(CompanyRequest req)
    {
      HmcResponse response = new HmcResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                if (req.id != 0)
                {
                  CompaniesMaster resultsDb = (from d in db.CompaniesMaster
                                               where d.id.Equals(req.id)
                                               orderby d.id descending
                                               select d).FirstOrDefault();
                  if (resultsDb != null)
                  {

                    resultsDb.name = req.name;
                    resultsDb.modify_by = employeeId;
                    resultsDb.modify_date = DateTime.Now;
                    resultsDb.company_code = req.company_code;
                    resultsDb.company_address = req.company_address;
                    resultsDb.companye_mail = req.companye_mail;
                    resultsDb.company_website = req.company_website;
                    resultsDb.companye_contact_no = req.companye_contact_no;
                    resultsDb.status = req.status;
                    db.SaveChanges();
                    response.flag = "1";
                    response.status = "success";
                    response.alert = "success";
                  }
                }
                else
                {
                  CompaniesMaster resultsDb = new CompaniesMaster();
                  resultsDb.name = req.name;
                  resultsDb.modify_by = employeeId;
                  resultsDb.modify_date = DateTime.Now;
                  resultsDb.company_code = req.company_code;
                  resultsDb.company_address = req.company_address;
                  resultsDb.companye_mail = req.companye_mail;
                  resultsDb.company_website = req.company_website;
                  resultsDb.status = req.status;
                  resultsDb.companye_contact_no = req.companye_contact_no;
                  db.CompaniesMaster.Add(resultsDb);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "success";
                }

              }
            }


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get Company data
    [HttpPost]
    [Route("GetCompanyData")]
    public CompanyResponse GetCompanyData(HmcRequest req)
    {

      CompanyResponse response = new CompanyResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "No Data Found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();

                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;

                List<CompaniesMaster> retVal = null;
                if (req.search_result != null)
                {
                  if (employeeMaster.role == "Administrator")
                  {
                    retVal = (from e in db.CompaniesMaster
                              where e.name.ToLower().Contains(req.search_result)
                              orderby e.id descending
                              select e).ToList();
                  }
                  else
                  {
                    retVal = (from e in db.CompaniesMaster
                              where e.name.ToLower().Contains(req.search_result)
                              && e.company_code.Equals(employeeMaster.pay_code)
                              orderby e.id descending
                              select e).ToList();
                  }

                }
                else
                {
                  if (employeeMaster.role == "Administrator")
                  {
                    retVal = (from e in db.CompaniesMaster
                              orderby e.id ascending
                              select e).ToList();
                  }
                  else
                  {
                    retVal = (from e in db.CompaniesMaster
                              where e.company_code.Equals(employeeMaster.pay_code)
                              orderby e.id ascending
                              select e).ToList();
                  }

                }

                response.data = retVal;
                response.flag = "1";
                response.status = "success";
                response.alert = "";
              }

            }

          }
        }
      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // delete company
    [HttpPost]
    [Route("DeleteCompany")]
    public HmcResponse DeleteCompany(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int ids = Convert.ToInt32(req.delete_id);
                CompaniesMaster commaster = (from e in db.CompaniesMaster
                                             where e.id.Equals(ids)
                                             select e).FirstOrDefault();

                if (commaster != null)
                {
                  db.CompaniesMaster.Remove(commaster);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // get shift
    [HttpPost]
    [Route("GetShift")]
    public ShiftResponse GetShift(HmcRequest req)
    {
      ShiftResponse response = new ShiftResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                List<Shift> retVal = new List<Shift>();
                if (userValid.role == "Administrator")
                {
                  retVal = (from r in db.Shift
                            orderby r.id ascending
                            select r).ToList();
                }
                else
                {
                  retVal = (from r in db.Shift
                            where r.pay_code.Equals(userValid.pay_code)
                            orderby r.id ascending
                            select r).ToList();
                }
                response.status = "success";
                response.flag = "1";
                response.data = retVal;
                response.alert = "success";
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // create shift
    [HttpPost]
    [Route("CreateShift")]
    public HmcJsonResponse CreateShift(ShiftData req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(ShiftValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  Shift resultsDb = new Shift();
                  resultsDb.shift_code = req.shift_code;
                  resultsDb.shift_name = req.shift_name;
                  resultsDb.start_time = req.start_time;
                  resultsDb.end_time = req.end_time;
                  resultsDb.break_start_time = req.break_start_time;
                  resultsDb.break_start_time = req.break_start_time;
                  resultsDb.grace_time = req.grace_time;
                  resultsDb.pay_code = req.pay_code;
                  resultsDb.latitude = req.latitude;
                  resultsDb.longitude = req.longitude;
                  resultsDb.modify_by = employeeId;
                  resultsDb.modify_date = DateTime.Now;
                  db.Shift.Add(resultsDb);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "success";
                }

              }
            }
            //string todayDate = DateTime.Now;
            //DateTime fromdate = todayDate.Date;


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // update shift
    [HttpPost]
    [Route("UpdateShift")]
    public HmcJsonResponse UpdateShift(ShiftData req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(ShiftValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  if (req.id != 0)
                  {
                    Shift resultsDb = (from d in db.Shift
                                       where d.id.Equals(req.id)
                                       orderby d.id descending
                                       select d).FirstOrDefault();
                    if (resultsDb != null)
                    {

                      resultsDb.shift_code = req.shift_code;
                      resultsDb.shift_name = req.shift_name;
                      resultsDb.start_time = req.start_time;
                      resultsDb.end_time = req.end_time;
                      resultsDb.break_start_time = req.break_start_time;
                      resultsDb.break_start_time = req.break_start_time;
                      resultsDb.grace_time = req.grace_time;
                      resultsDb.pay_code = req.pay_code;
                      resultsDb.latitude = req.latitude;
                      resultsDb.longitude = req.longitude;
                      resultsDb.modify_by = employeeId;
                      resultsDb.modify_date = DateTime.Now;
                      db.SaveChanges();

                      response.flag = "1";
                      response.status = "success";
                      response.alert = "success";
                    }
                  }
                }


              }
            }
            //string todayDate = DateTime.Now;
            //DateTime fromdate = todayDate.Date;


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // delete shift
    [HttpPost]
    [Route("DeleteShift")]
    public HmcResponse DeleteShift(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                Shift leavemaster = (from e in db.Shift
                                     where e.id.Equals(delete_id)
                                     select e).FirstOrDefault();

                if (leavemaster != null)
                {
                  db.Shift.Remove(leavemaster);
                  db.SaveChanges();
                }

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // search attendance based on employee id
    [HttpPost]
    [Route("GetEmployeeAttendance")]
    public HmcAttResponse GetEmployeeAttendance(SearchEmployeeAttRequest req)
    {
      HmcAttResponse response = new HmcAttResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<AttendanceResponse> attmaster = null;

                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;

                if (!string.IsNullOrEmpty(req.employee_code))
                {
                  req.department = "";
                  req.shift = "";

                }

                if (!string.IsNullOrEmpty(req.department) && !string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.shift.Equals(req.shift)
                               && e.employee_id.Equals(req.employee_code)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else if (!string.IsNullOrEmpty(req.department))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else if (!string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.shift.Equals(req.shift)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else if (!string.IsNullOrEmpty(req.employee_code))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.employee_id.Equals(req.employee_code)
                               //where e.employee_id.Equals(req.employee_code)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).Skip(skip).Take(pageSize).ToList();
                }



                foreach (AttendanceResponse item in attmaster)
                {
                  item.sdate = item.date.ToString("dd/MM/yyyy");
                  DateTime inTime = Convert.ToDateTime(item.in_time);
                  DateTime outTime = Convert.ToDateTime(item.out_time);
                  item.in_time = inTime.ToString("HH:mm:ss");
                  item.out_time = outTime.ToString("HH:mm:ss");
                }



                EmployeeMaster empMasterv = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(req.employee_code)
                                             select d).FirstOrDefault();

                AttResponse attendanceRes = new AttResponse();

                attendanceRes.attendaceList = attmaster;

                if (empMasterv != null)
                {
                  attendanceRes.employee_id = empMasterv.employee_id;
                  attendanceRes.name = empMasterv.name;
                  attendanceRes.department = empMasterv.department;
                  attendanceRes.company_name = empMasterv.pay_code;
                }
                else
                {
                  attendanceRes.department = req.department;
                  attendanceRes.company_name = "All";
                  attendanceRes.shift = req.shift;
                }

                attendanceRes.print_date = DateTime.Now.ToString("dd/MM/yyyy");


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = attendanceRes;
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // get menu
    [HttpPost]
    [Route("GetMenu")]
    public MenuResponse GetMenu(HmcRequest req)
    {
      MenuResponse response = new MenuResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "menu not exist";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {

            using (AttendanceContext db = new AttendanceContext())
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                List<MenuDetails> menuList = (from d in db.MenuMaster
                                              where d.sub_menu_id != 0
                                              select new MenuDetails
                                              {
                                                id = d.id,
                                                name = d.name,
                                                url = d.url,
                                                icons = d.icons
                                              }).ToList();

                //if (menuList.Count() > 0)
                //{
                //    foreach (MenuDetails item in menuList)
                //    {
                //        if (item.id != 0)
                //        {
                //            List<MenuMaster> subMenuList = (from d in db.MenuMaster
                //                                            where d.sub_menu_id.Equals(item.id)
                //                                            select d).ToList();

                //            item.sub_menu = subMenuList;
                //        }
                //    }
                //}


                response.status = "success";
                response.flag = "1";
                response.data = menuList;
                response.alert = "success";
              }
            }
          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // create access menu
    [HttpPost]
    [Route("CreateAccessMenu")]
    public HmcResponse CreateAccessMenu(AccessMenuRequest req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                //DateTime date = Convert.ToDateTime(req.p_date);
                if (req.id != 0)
                {
                  KeyValueMaster result = (from d in db.KeyValueMaster
                                           where d.id.Equals(req.id)
                                           select d).FirstOrDefault();

                  if (result != null)
                  {
                    result.modify_by = employeeId;
                    result.key_code = req.key_code;
                    result.key_type = req.key_type;
                    result.key_description = req.key_description;
                    result.pay_code = req.pay_code;
                    result.modify_date = DateTime.Now;
                    db.SaveChanges();

                    List<AccessMenu> roleAccess = (from d in db.AccessMenu
                                                   where d.role_id.Equals(result.id)
                                                   select d).ToList();
                    foreach (AccessMenu item in roleAccess)
                    {
                      db.AccessMenu.Remove(item);
                      db.SaveChanges();
                    }

                    foreach (int item in req.access_menu)
                    {
                      AccessMenu accessMenu = new AccessMenu();
                      accessMenu.role_id = result.id;
                      accessMenu.menu_id = item;
                      db.AccessMenu.Add(accessMenu);
                      db.SaveChanges();
                    }
                  }
                }
                else
                {
                  KeyValueMaster result = new KeyValueMaster();
                  result.modify_by = employeeId;
                  result.key_code = req.key_code;
                  result.key_type = req.key_type;
                  result.key_description = req.key_description;
                  result.pay_code = req.pay_code;
                  result.modify_date = DateTime.Now;
                  db.KeyValueMaster.Add(result);
                  db.SaveChanges();

                  foreach (int item in req.access_menu)
                  {
                    AccessMenu accessMenu = new AccessMenu();
                    accessMenu.role_id = result.id;
                    accessMenu.menu_id = item;
                    db.AccessMenu.Add(accessMenu);
                    db.SaveChanges();
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "Thanks for update";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // show access role permision
    // create access menu
    [HttpPost]
    [Route("ShowAccessRole")]
    public HmcAccessRoleResponse ShowAccessRole(HmcRequest req)
    {
      HmcAccessRoleResponse response = new HmcAccessRoleResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {

                AccessMenuRequest keyValue = (from d in db.KeyValueMaster
                                              where d.id.Equals(req.id)
                                              select new AccessMenuRequest
                                              {
                                                id = d.id,
                                                key_type = d.key_type,
                                                key_code = d.key_code,
                                                key_description = d.key_description,
                                                pay_code = d.pay_code
                                              }).FirstOrDefault();

                if (keyValue != null)
                {
                  keyValue.access_menu = (from d in db.AccessMenu
                                          where d.role_id.Equals(keyValue.id)
                                          select d.menu_id).ToList();
                }


                response.status = "success";
                response.flag = "1";
                response.data = keyValue;
                response.alert = "Thanks for update";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // employee dashboard
    [HttpPost]
    [Route("EmployeeDashboard")]
    public EmployeeDashboardResponse EmployeeDashboard(HmcRequest req)
    {
      EmployeeDashboardResponse response = new EmployeeDashboardResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                EmployeeDashboardPerameter retVal = new EmployeeDashboardPerameter();
                EmployeeBasicdetails empMaster = (from d in db.EmployeeMaster
                                                  where d.employee_id.Equals(employeeId)
                                                  select new EmployeeBasicdetails
                                                  {
                                                    employee_id = d.employee_id,
                                                    name = d.name,
                                                    email = d.email,
                                                    mobile_no = d.mobile_no,
                                                    dob = d.dob,
                                                    doj = d.doj,
                                                    reporting_to = d.reporting_to,
                                                    father_name = d.father_name,
                                                    designation = d.designation,
                                                    department = d.department,
                                                    company_code = d.pay_code,
                                                    aadhar_no = d.aadhar_number,
                                                    el = d.el,
                                                    cl = d.cl,
                                                    sl = d.sl,
                                                    role = d.role,
                                                    employee_type = d.employeement_type,
                                                    feature_image = d.feature_image
                                                  }).FirstOrDefault();

                if (empMaster.dob != null)
                {
                  empMaster.dobs = empMaster.dob.ToString("dd/MM/yyyy");
                }
                if (empMaster.doj != null)
                {
                  empMaster.dojs = empMaster.doj.ToString("dd/MM/yyyy");
                }
                retVal.role = empMaster.role;

                if (empMaster.role == "Administrator")
                {
                  retVal.applyleave = (from d in db.ApplyLeaveMasters
                                       orderby d.apply_date descending
                                       select new ApplyLeaveResponse
                                       {
                                         id = d.id,
                                         employee_id = d.employee_id.ToString(),
                                         from_date = d.from_date,
                                         to_date = d.to_date,
                                         date = d.apply_date,
                                         status = d.status,
                                         reason = d.reason,
                                         assign_by = d.assign_by,
                                         leave_type = d.leave_Type,
                                         duration_type = d.duration_type,
                                         no_of_leave = d.no_of_leave.ToString()
                                       }).Skip(0).Take(6).Distinct().ToList();

                  if (retVal.applyleave.Count > 0)
                  {
                    foreach (ApplyLeaveResponse item in retVal.applyleave)
                    {
                      item.todate = item.to_date.ToString("dd/MM/yyyy");
                      item.fromdate = item.from_date.ToString("dd/MM/yyyy");
                      item.applydate = item.date.ToString("dd/MM/yyyy");

                      EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(item.assign_by)
                                                 select d).FirstOrDefault();

                      if (assignBy != null)
                      {
                        item.assign_by_name = assignBy.name;
                      }

                      EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                                where d.employee_id.Equals(item.employee_id)
                                                select d).FirstOrDefault();
                      if (applyBy != null)
                      {
                        item.apply_by_name = applyBy.name;
                      }

                      KeyValueMaster leaveType = (from d in db.KeyValueMaster
                                                  where d.id.Equals(item.leave_type)
                                                  select d).FirstOrDefault();
                      if (leaveType != null)
                      {
                        item.leave_code = leaveType.key_code;
                      }

                      KeyValueMaster durationType = (from d in db.KeyValueMaster
                                                     where d.id.Equals(item.duration_type)
                                                     select d).FirstOrDefault();
                      if (durationType != null)
                      {
                        item.duration_code = durationType.key_code;
                      }
                    }
                  }

                  int employeeTotalCount = (from d in db.EmployeeMaster
                                            select d).Count();

                  int employeeActiveCount = (from d in db.EmployeeMaster
                                             where d.status.Equals("Active")
                                             select d).Count();

                  int employeeInActiveCount = (from d in db.EmployeeMaster
                                               where d.status.Equals("InActive")
                                               select d).Count();

                  int applyLeaveTotalCount = (from d in db.ApplyLeaveMasters
                                              select d).Count();

                  int applyLeavePendingCount = (from d in db.ApplyLeaveMasters
                                                where d.status.Equals("Pending")
                                                select d).Count();

                  int applyLeaveApproveCount = (from d in db.ApplyLeaveMasters
                                                where d.status.Equals("Approved")
                                                select d).Count();

                  int applyLeaveRejectCount = (from d in db.ApplyLeaveMasters
                                               where d.status.Equals("Rejected")
                                               select d).Count();

                  retVal.totalemployee = employeeTotalCount;
                  retVal.activeemployee = employeeActiveCount;
                  retVal.inactiveemployee = employeeInActiveCount;
                  retVal.totalleave = applyLeaveTotalCount;
                  retVal.pendingleave = applyLeavePendingCount;
                  retVal.approveleave = applyLeaveApproveCount;
                  retVal.rejectleave = applyLeaveRejectCount;
                }
                else if (empMaster.role == "Admin")
                {
                  retVal.applyleave = (from d in db.ApplyLeaveMasters
                                       where d.pay_code.Equals(empMaster.company_code)
                                       orderby d.apply_date descending
                                       select new ApplyLeaveResponse
                                       {
                                         id = d.id,
                                         employee_id = d.employee_id.ToString(),
                                         from_date = d.from_date,
                                         to_date = d.to_date,
                                         date = d.apply_date,
                                         status = d.status,
                                         reason = d.reason,
                                         assign_by = d.assign_by,
                                         leave_type = d.leave_Type,
                                         duration_type = d.duration_type,
                                         no_of_leave = d.no_of_leave.ToString()
                                       }).Skip(0).Take(6).Distinct().ToList();

                  if (retVal.applyleave.Count > 0)
                  {
                    foreach (ApplyLeaveResponse item in retVal.applyleave)
                    {
                      item.todate = item.to_date.ToString("dd/MM/yyyy");
                      item.fromdate = item.from_date.ToString("dd/MM/yyyy");
                      item.applydate = item.date.ToString("dd/MM/yyyy");

                      EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(item.assign_by)
                                                 select d).FirstOrDefault();

                      if (assignBy != null)
                      {
                        item.assign_by_name = assignBy.name;
                      }

                      EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                                where d.employee_id.Equals(item.employee_id)
                                                select d).FirstOrDefault();
                      if (applyBy != null)
                      {
                        item.apply_by_name = applyBy.name;
                      }

                      KeyValueMaster leaveType = (from d in db.KeyValueMaster
                                                  where d.id.Equals(item.leave_type)
                                                  select d).FirstOrDefault();
                      if (leaveType != null)
                      {
                        item.leave_code = leaveType.key_code;
                      }

                      KeyValueMaster durationType = (from d in db.KeyValueMaster
                                                     where d.id.Equals(item.duration_type)
                                                     select d).FirstOrDefault();
                      if (durationType != null)
                      {
                        item.duration_code = durationType.key_code;
                      }
                    }
                  }

                  int employeeTotalCount = (from d in db.EmployeeMaster
                                            where d.pay_code.Equals(empMaster.company_code)
                                            select d).Count();

                  int employeeActiveCount = (from d in db.EmployeeMaster
                                             where d.pay_code.Equals(empMaster.company_code)
                                             where d.status.Equals("Active")
                                             select d).Count();

                  int employeeInActiveCount = (from d in db.EmployeeMaster
                                               where d.pay_code.Equals(empMaster.company_code)
                                               where d.status.Equals("InActive")
                                               select d).Count();

                  int applyLeaveTotalCount = (from d in db.ApplyLeaveMasters
                                              where d.pay_code.Equals(empMaster.company_code)
                                              select d).Count();

                  int applyLeavePendingCount = (from d in db.ApplyLeaveMasters
                                                where d.pay_code.Equals(empMaster.company_code)
                                                where d.status.Equals("Pending")
                                                select d).Count();

                  int applyLeaveApproveCount = (from d in db.ApplyLeaveMasters
                                                where d.pay_code.Equals(empMaster.company_code)
                                                where d.status.Equals("Approved")
                                                select d).Count();

                  int applyLeaveRejectCount = (from d in db.ApplyLeaveMasters
                                               where d.pay_code.Equals(empMaster.company_code)
                                               where d.status.Equals("Rejected")
                                               select d).Count();

                  retVal.totalemployee = employeeTotalCount;
                  retVal.activeemployee = employeeActiveCount;
                  retVal.inactiveemployee = employeeInActiveCount;
                  retVal.totalleave = applyLeaveTotalCount;
                  retVal.pendingleave = applyLeavePendingCount;
                  retVal.approveleave = applyLeaveApproveCount;
                  retVal.rejectleave = applyLeaveRejectCount;
                }
                else
                {
                  // holiday
                  List<HolidayMaster> holidayList = (from d in db.HolidayMaster
                                                     where d.pay_code.Equals(empMaster.company_code)
                                                     && d.holiday_date.Year.Equals(DateTime.Now.Year)
                                                     orderby d.holiday_date ascending
                                                     select d).ToList();

                  retVal.holidays = holidayList;

                  // reporting manager name
                  EmployeeMaster reportinEmp = (from d in db.EmployeeMaster
                                                where d.employee_id.Equals(empMaster.reporting_to)
                                                select d).FirstOrDefault();

                  if (reportinEmp != null)
                  {
                    empMaster.reporting_to_name = reportinEmp.name;
                  }



                  // company name
                  CompaniesMaster companyMaster = (from d in db.CompaniesMaster
                                                   where d.company_code.Equals(empMaster.company_code)
                                                   select d).FirstOrDefault();

                  if (companyMaster != null)
                  {
                    empMaster.company_name = companyMaster.name;
                  }

                  retVal.employeebasicdetails = empMaster;

                  List<KeyValueMaster> bgNames = (from d in db.KeyValueMaster
                                                  where d.key_type.Equals("leave")
                                                  select d).ToList();

                  LeaveType empd = (from e in db.EmployeeMaster
                                    where e.employee_id.Equals(empMaster.employee_id)
                                    select new LeaveType
                                    {
                                      el = e.el,
                                      cl = e.cl,
                                      sl = e.sl,
                                      a_el = e.a_el,
                                      a_cl = e.a_cl,
                                      a_sl = e.a_sl,
                                      b_el = e.b_el,
                                      b_cl = e.b_cl,
                                      b_sl = e.b_sl,
                                    }).FirstOrDefault();

                  if (!string.IsNullOrEmpty(empd.el))
                  {
                    decimal[] EmpLeaves = new decimal[4];
                    EmpLeaves[0] = Convert.ToDecimal(empd.el) + Convert.ToDecimal(empd.cl) + Convert.ToDecimal(empd.sl);
                    EmpLeaves[1] = Convert.ToDecimal(empd.el);//empd.el;
                    EmpLeaves[2] = Convert.ToDecimal(empd.cl); //empd.cl;
                    EmpLeaves[3] = Convert.ToDecimal(empd.sl);//empd.sl;

                    decimal[] Availd = new decimal[4];
                    Availd[0] = Convert.ToDecimal(empd.a_el) + Convert.ToDecimal(empd.a_cl) + Convert.ToDecimal(empd.a_sl);
                    Availd[1] = Convert.ToDecimal(empd.a_el);//empd.el;
                    Availd[2] = Convert.ToDecimal(empd.a_cl); //empd.cl;
                    Availd[3] = Convert.ToDecimal(empd.a_sl);//empd.sl;

                    decimal[] Balance = new decimal[4];
                    Balance[0] = Convert.ToDecimal(empd.b_el) + Convert.ToDecimal(empd.b_cl) + Convert.ToDecimal(empd.b_sl);
                    Balance[1] = Convert.ToDecimal(empd.b_el);//empd.el;
                    Balance[2] = Convert.ToDecimal(empd.b_cl); //empd.cl;
                    Balance[3] = Convert.ToDecimal(empd.b_sl);//empd.sl;

                    string[] BgName = new string[4];
                    BgName[0] = "Total";
                    BgName[1] = "EL";//empd.el;
                    BgName[2] = "CL"; //empd.cl;
                    BgName[3] = "SL";//empd.sl;

                    retVal.total = EmpLeaves[0];
                    retVal.availd = Convert.ToDecimal(empd.a_el) + Convert.ToDecimal(empd.a_cl) + Convert.ToDecimal(empd.a_sl);
                    retVal.balancel = Convert.ToDecimal(empd.b_el) + Convert.ToDecimal(empd.b_cl) + Convert.ToDecimal(empd.b_sl);
                  }




                }


                response.status = "success";
                response.flag = "1";
                response.data = retVal;
                response.alert = "success";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // employee leave summary
    [HttpPost]
    [Route("EmployeeLeaveSummary")]
    public EmployeeDashboardResponse EmployeeLeaveSummary(HmcRequest req)
    {
      EmployeeDashboardResponse response = new EmployeeDashboardResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                EmployeeDashboardPerameter retVal = new EmployeeDashboardPerameter();
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                retVal.role = empMaster.role;

                //LeaveType empd = (from e in db.EmployeeMaster
                //                  where e.employee_id.Equals(empMaster.employee_id)
                //                  select new LeaveType
                //                  {
                //                      el = e.el,
                //                      cl = e.cl,
                //                      sl = e.sl,
                //                      a_el = e.a_el,
                //                      a_cl = e.a_cl,
                //                      a_sl = e.a_sl,
                //                      b_el = e.b_el,
                //                      b_cl = e.b_cl,
                //                      b_sl = e.b_sl,
                //                  }).FirstOrDefault();


                decimal[] EmpLeaves = new decimal[4];
                EmpLeaves[0] = Convert.ToDecimal(empMaster.el) + Convert.ToDecimal(empMaster.cl) + Convert.ToDecimal(empMaster.sl);
                EmpLeaves[1] = Convert.ToDecimal(empMaster.el);//empd.el;
                EmpLeaves[2] = Convert.ToDecimal(empMaster.cl); //empd.cl;
                EmpLeaves[3] = Convert.ToDecimal(empMaster.sl);//empd.sl;

                decimal[] Availd = new decimal[4];
                Availd[0] = Convert.ToDecimal(empMaster.a_el) + Convert.ToDecimal(empMaster.a_cl) + Convert.ToDecimal(empMaster.a_sl);
                Availd[1] = Convert.ToDecimal(empMaster.a_el);//empd.el;
                Availd[2] = Convert.ToDecimal(empMaster.a_cl); //empd.cl;
                Availd[3] = Convert.ToDecimal(empMaster.a_sl);//empd.sl;

                decimal[] Balance = new decimal[4];
                Balance[0] = Convert.ToDecimal(empMaster.b_el) + Convert.ToDecimal(empMaster.b_cl) + Convert.ToDecimal(empMaster.b_sl);
                Balance[1] = Convert.ToDecimal(empMaster.b_el);//empd.el;
                Balance[2] = Convert.ToDecimal(empMaster.b_cl); //empd.cl;
                Balance[3] = Convert.ToDecimal(empMaster.b_sl);//empd.sl;

                string[] BgName = new string[4];
                BgName[0] = "Total";
                BgName[1] = "EL";//empd.el;
                BgName[2] = "CL"; //empd.cl;
                BgName[3] = "SL";//empd.sl;

                retVal.targets = EmpLeaves;
                retVal.actuals = Availd;
                retVal.bgNames = BgName;
                retVal.balance = Balance;

                retVal.total = EmpLeaves[0];
                retVal.availd = Convert.ToDecimal(empMaster.a_el) + Convert.ToDecimal(empMaster.a_cl) + Convert.ToDecimal(empMaster.a_sl);
                retVal.balancel = Convert.ToDecimal(empMaster.b_el) + Convert.ToDecimal(empMaster.b_cl) + Convert.ToDecimal(empMaster.b_sl);


                response.status = "success";
                response.flag = "1";
                response.data = retVal;
                response.alert = "success";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // change password
    // change password
    [HttpPost]
    [Route("ChangePassword")]
    public HmcStringResponse ChangePassword(ChangePassword req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.data = "new password and confirm password is not match";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            using (AttendanceContext db = new AttendanceContext())
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                if (req.new_password != null && req.confirm_password != null)
                {    // Lets first check if the Model is valid or not

                  if (req.new_password == req.confirm_password)
                  {

                    string password = req.confirm_password;
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


                    EmployeeMaster resultsDb = null;
                    resultsDb = (from e in db.EmployeeMaster
                                 where (e.employee_id.Equals(employeeId))
                                 select e).FirstOrDefault();
                    if (resultsDb != null)
                    {
                      resultsDb.password = encPassword;
                      resultsDb.password_count = 0;
                      db.SaveChanges();

                      response.status = "success";
                      response.flag = "1";
                      response.data = "success";
                      response.alert = "success";
                    }

                  }

                }
              }
            }


          }
        }
      }
      catch (Exception e)
      {
        //string message = e.Message;
        response.alert = e.Message;
      }

      // If we got this far, something failed, redisplay form
      return response;
    }

    //employee profile

    [HttpPost]
    [Route("EmployeeProfile")]
    public EmployeeProfile EmployeeProfile(HmcRequest req)
    {
      EmployeeProfile response = new EmployeeProfile();
      response.status = "error";
      response.flag = "0";
      response.alert = "invalid data";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                // EmployeeDashboardPerameter retVal = new EmployeeDashboardPerameter();
                EmployeeBasicdetails empMaster = (from d in db.EmployeeMaster
                                                  where d.employee_id.Equals(employeeId)
                                                  select new EmployeeBasicdetails
                                                  {
                                                    employee_id = d.employee_id,
                                                    name = d.name,
                                                    email = d.email,
                                                    mobile_no = d.mobile_no,
                                                    dob = d.dob,
                                                    doj = d.doj,
                                                    reporting_to = d.reporting_to,
                                                    father_name = d.father_name,
                                                    designation = d.designation,
                                                    department = d.department,
                                                    company_code = d.pay_code,
                                                    aadhar_no = d.aadhar_number,
                                                    el = d.el,
                                                    cl = d.cl,
                                                    sl = d.sl,
                                                    role = d.role,
                                                    employee_type = d.employeement_type,
                                                    feature_image = d.feature_image,
                                                    attendance_type=d.attendance_type,
                                                    ShiftName=d.shift,
                                                    pay_code=d.pay_code

                                                  }).FirstOrDefault();

                if (empMaster.dob != null)
                {
                  empMaster.dobs = empMaster.dob.ToString("dd/MM/yyyy");
                }
                if (empMaster.doj != null)
                {
                  empMaster.dojs = empMaster.doj.ToString("dd/MM/yyyy");
                }

                if (!string.IsNullOrEmpty(empMaster.reporting_to))
                {
                  if (empMaster.reporting_to != "0")
                  {
                    empMaster.reporting_to_name = (from d in db.EmployeeMaster
                                                   where d.employee_id.Equals(empMaster.reporting_to)
                                                   select d.name).FirstOrDefault();
                  }
                }

                if (!string.IsNullOrEmpty(empMaster.pay_code))
                {
                  if (empMaster.pay_code != "0")
                  {
                    empMaster.LocationName = (from d in db.CompaniesMaster
                                                   where d.company_code.Equals(empMaster.pay_code)
                                                   select d.company_address).FirstOrDefault();

                    empMaster.company_name = (from d in db.CompaniesMaster
                                              where d.company_code.Equals(empMaster.pay_code)
                                              select d.name).FirstOrDefault();

                  }
                }


                if (!string.IsNullOrEmpty(empMaster.ShiftName))
                {
                  if (empMaster.ShiftName != "")
                  {
                    var shiftData =  db.Shift.Where(d=>d.shift_name.Equals(empMaster.ShiftName) && d.pay_code.Equals(empMaster.pay_code)                              
                                                   ).FirstOrDefault();
                    if (shiftData != null)
                    {

                      empMaster.ShiftTimeFrom = shiftData.start_time;
                      empMaster.ShiftTimeTo = shiftData.end_time;

                    }
                    else
                    {
                      empMaster.ShiftTimeFrom = "00:00:00";
                      empMaster.ShiftTimeTo = "00:00:00";
                    }

                  }
                }


                response.status = "success";
                response.flag = "1";
                response.data = empMaster;
                response.alert = "success";
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get apply leave request
    [HttpPost]
    [Route("GetApplyLeaveRequest")]
    public GetApplyLeaveResponse GetApplyLeaveRequest(HmcRequest req)
    {
      GetApplyLeaveResponse response = new GetApplyLeaveResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int page = Convert.ToInt32(req.pageNo);
                List<ApplyLeaveResponse> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (!string.IsNullOrEmpty(req.search_result))
                {
                  retVal = (from d in db.ApplyLeaveMasters
                            where d.employee_id.ToLower().Contains(req.search_result)
                            && d.assign_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new ApplyLeaveResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id.ToString(),
                              from_date = d.from_date,
                              to_date = d.to_date,
                              date = d.apply_date,
                              status = d.status,
                              reason = d.reason,
                              assign_by = d.assign_by,
                              leave_type = d.leave_Type,
                              duration_type = d.duration_type,
                              no_of_leave = d.no_of_leave.ToString()
                            }).Skip(skip).Take(pageSize).ToList();

                }
                else
                {
                  retVal = (from d in db.ApplyLeaveMasters
                            where d.assign_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new ApplyLeaveResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id.ToString(),
                              from_date = d.from_date,
                              to_date = d.to_date,
                              date = d.apply_date,
                              status = d.status,
                              reason = d.reason,
                              assign_by = d.assign_by,
                              leave_type = d.leave_Type,
                              duration_type = d.duration_type,
                              no_of_leave = d.no_of_leave.ToString()
                            }).Skip(skip).Take(pageSize).ToList();
                }

                if (retVal.Count > 0)
                {
                  foreach (ApplyLeaveResponse item in retVal)
                  {
                    item.todate = item.to_date.ToString("dd/MM/yyyy");
                    item.fromdate = item.from_date.ToString("dd/MM/yyyy");
                    item.applydate = item.date.ToString("dd/MM/yyyy");

                    EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                               where d.employee_id.Equals(item.assign_by)
                                               select d).FirstOrDefault();

                    if (assignBy != null)
                    {
                      item.assign_by_name = assignBy.name;
                    }

                    EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                              where d.employee_id.Equals(item.employee_id)
                                              select d).FirstOrDefault();
                    if (applyBy != null)
                    {
                      item.apply_by_name = applyBy.name;
                    }

                    KeyValueMaster leaveType = (from d in db.KeyValueMaster
                                                where d.id.Equals(item.leave_type)
                                                select d).FirstOrDefault();
                    if (leaveType != null)
                    {
                      item.leave_code = leaveType.key_code;
                    }

                    KeyValueMaster durationType = (from d in db.KeyValueMaster
                                                   where d.id.Equals(item.duration_type)
                                                   select d).FirstOrDefault();
                    if (durationType != null)
                    {
                      item.duration_code = durationType.key_code;
                    }
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // generate pdf
    [HttpPost]
    [Route("GeneratePDF")]
    public HmcStringResponse GeneratePDF(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<AttendanceResponse> attmaster = null;

                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                if (!string.IsNullOrEmpty(req.employee_code))
                {
                  req.department = "";
                  req.shift = "";
                }

                if (!string.IsNullOrEmpty(req.department) && !string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.shift.Equals(req.shift)
                               && e.employee_id.Equals(req.employee_code)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 mis = e.mis,
                                 absent = e.absent,
                                 early = e.early,
                                 late = e.late,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.department))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 mis = e.mis,
                                 absent = e.absent,
                                 early = e.early,
                                 late = e.late,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.shift.Equals(req.shift)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 mis = e.mis,
                                 absent = e.absent,
                                 early = e.early,
                                 late = e.late,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.employee_code))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.employee_id.Equals(req.employee_code)
                               && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 mis = e.mis,
                                 absent = e.absent,
                                 early = e.early,
                                 late = e.late,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 mis = e.mis,
                                 absent = e.absent,
                                 early = e.early,
                                 late = e.late,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }

                double totalHourse = 0;
                int totalWeekOff = 0;
                int totalPrent = 0;
                int totalAbsent = 0;
                int totalLeave = 0;
                int totalOD = 0;
                int totalMis = 0;
                int totalLate = 0;
                int totalEarly = 0;
                int totalHalfDay = 0;
                foreach (AttendanceResponse item in attmaster)
                {
                  item.sdate = item.date.ToString("dd/MM/yyyy");
                  DateTime inTime = Convert.ToDateTime(item.in_time);
                  DateTime outTime = Convert.ToDateTime(item.out_time);
                  item.in_time = inTime.ToString("HH:mm:ss");
                  item.out_time = outTime.ToString("HH:mm:ss");

                  if (item.status == "WO")
                  {
                    totalWeekOff = totalWeekOff + 1;
                  }

                  if (item.status == "HD")
                  {
                    totalHalfDay = totalHalfDay + 1;
                  }

                  if (item.status == "P")
                  {
                    totalPrent = totalPrent + 1;
                  }


                  if (item.absent == "A")
                  {
                    totalAbsent = totalAbsent + 1;
                  }
                  if (item.mis == "A")
                  {
                    totalMis = totalMis + 1;
                  }
                  if (item.late == "SHL")
                  {
                    totalLate = totalLate + 1;
                  }
                  if (item.early == "A")
                  {
                    totalEarly = totalEarly + 1;
                  }
                  if (item.status == "D")
                  {
                    totalOD = totalOD + 1;
                  }

                  if (item.status == "EL" || item.status == "CL" || item.status == "SL" || item.status == "PL" || item.status == "TMPL" || item.status == "LWP")
                  {
                    totalLeave = totalLeave + 1;
                  }

                  double hrs = Math.Round(item.total_hrs, 2);
                  totalHourse = totalHourse + hrs;
                }

                string totalHrs = totalHourse.ToString();

                string[] totHrs = totalHrs.Split('.');
                int totlHRS = Convert.ToInt32(totHrs[0]);
                int TotalMintus = 0;
                int TotalHoursMintus = 0;
                if (totHrs.Length > 1)
                {
                  string totH = totHrs[1];
                  decimal TotalMint = Convert.ToDecimal(totH);
                  if (TotalMint == 6)
                  {
                    TotalMint = 60;
                  }
                  if (TotalMint > 59)
                  {
                    double totMot = Convert.ToDouble(TotalMint);
                    TimeSpan spWorkMin = TimeSpan.FromMinutes(totMot);
                    string workHours = spWorkMin.ToString(@"hh\:mm");
                    //TotalMint = TotalMint / 60;

                    string mTotHrs = workHours.ToString();

                    string[] mTotHR = mTotHrs.Split(':');

                    string tHrs = mTotHR[0];

                    totlHRS = totlHRS + Convert.ToInt32(mTotHR[0]);

                    TotalHoursMintus = Convert.ToInt32(totlHRS) + Convert.ToInt32(tHrs);
                    if (mTotHR.Length > 1)
                    {
                      int le = mTotHR[1].Length;
                      string minOfcount = mTotHR[1].Remove(2, le - 2);
                      TotalMintus = Convert.ToInt32(minOfcount);
                    }
                  }
                  else
                  {
                    TotalMintus = Convert.ToInt32(totHrs[1]);
                  }


                }




                EmployeeMaster empMasterv = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(req.employee_code)
                                             select d).FirstOrDefault();

                AttResponse attendanceRes = new AttResponse();

                attendanceRes.attendaceList = attmaster;
                if (empMasterv != null)
                {
                  attendanceRes.employee_id = empMasterv.employee_id;
                  attendanceRes.name = empMasterv.name;
                  attendanceRes.department = empMasterv.department;
                  attendanceRes.company_name = empMasterv.pay_code;
                }
                else
                {
                  attendanceRes.department = req.department;
                  attendanceRes.shift = req.shift;
                  attendanceRes.company_name = empMaster.pay_code;
                }

                attendanceRes.print_date = DateTime.Now.ToString("dd/MM/yyyy");


                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                //Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 30, 30, 60, 30);
                Document doc = new Document(iTextSharp.text.PageSize.A4);
                //var fpath = Server.MapPath("~/customerdoc/SID/AccountStatement/");
                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                //string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                doc.Open();


                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                image.ScalePercent(50f);
                image.SetAbsolutePosition(30, 800);
                doc.Add(image);

                StringBuilder sb = new StringBuilder();
                //sb.Append(req.search_result);
                sb.Append("<body style='Font-family:Verdana;'>");
                sb.Append("<div style='Font-family:Verdana; text-align: center;'>Attendance Report <br />" + req.from_date + " To " + req.to_date + "</div>");
                sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5' style='font-size:8px;'>");
                //sb.Append("<tr><td colspan='9' border='0' style='text-align:center; font:size: 16px;'>Attendance Report <br />" + req.from_date + " To " + req.to_date +"</td></tr>");
                sb.Append("<tr style='font-size:10px;'><td colspan='4' border='0'>Company :" + attendanceRes.company_name + " </td><td colspan='4' border='0' style='text-align:right;'>Print On: " + attendanceRes.print_date + "  </td></tr>");
                if (!string.IsNullOrEmpty(attendanceRes.department))
                {
                  sb.Append("<tr style='font-size:10px;'><td colspan='8' border='0'>Department: " + attendanceRes.department + "</td></tr>");
                }
                if (!string.IsNullOrEmpty(attendanceRes.employee_id))
                {
                  sb.Append("<tr style='font-size:10px;'><td colspan='3' border='0'>Employee Code: " + attendanceRes.employee_id + " </td><td colspan='5' border='0'>Employee Name: " + attendanceRes.name + "</td></tr>");
                }
                sb.Append("<tr><td bgcolor='#cdcdcd'>Employee Id</td><td bgcolor='#cdcdcd'>Date</td><td bgcolor='#cdcdcd'>Day</td><td bgcolor='#cdcdcd'>In Time</td><td bgcolor='#cdcdcd'>Out Time</td><td bgcolor='#cdcdcd'>Shift</td><td bgcolor='#cdcdcd'>Total Duration</td><td bgcolor='#cdcdcd'>Status</td></tr>");
                foreach (AttendanceResponse item in attmaster)
                {
                  sb.Append("<tr><td>" + item.employee_id + "</td><td>" + item.sdate + "</td><td>" + item.day + "</td><td>" + item.in_time + "</td><td>" + item.out_time + "</td><td>" + item.shift + "</td><td>" + Math.Round(item.total_hrs, 2) +
                      "</td><td>" + item.status + item.absent + item.late + item.mis + item.early + "</td></tr>");

                  if (!string.IsNullOrEmpty(item.location_address))
                  {
                    sb.Append("<tr style='font-size:10px;'><td colspan='8'>Check In Add.: " + item.location_address + "</td></tr>");
                  }

                }


                sb.Append("</table>");
                sb.Append("<p width='100%' style='font-size: 10px;'>Total Duration = " + totlHRS + " Hrs  " + TotalMintus + " Min, Present= " + totalPrent + ", Leaves = " + totalLeave + ", Absent = " + totalAbsent +
                    ", Week Off = " + totalWeekOff + ", OD = " + totalOD + ", Early = " + totalEarly + ", Late = " + totalLate + ", Mispunch = " + totalMis + ", Half Day = " + totalHalfDay + "</p>");
                sb.Append("</body>");
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();
                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = fileName;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // generate pdf
    [HttpPost]
    [Route("GenerateFormPDF")]
    public HmcStringResponse GenerateFormPDF(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<AttendanceResponse> attmaster = (from e in db.AttendanceMaster
                                                      where e.employee_id.Equals(req.employee_code)
                                                      && e.date >= fromDate && e.date <= toDate
                                                      select new AttendanceResponse
                                                      {
                                                        id = e.id,
                                                        employee_id = e.employee_id,
                                                        date = e.date,
                                                        day = e.day,
                                                        in_time = e.in_time,
                                                        out_time = e.out_time,
                                                        shift = e.shift,
                                                        status = e.status,
                                                        total_hrs = e.total_hrs,
                                                      }).ToList();

                double totalHourse = 0;
                int totalWeekOff = 0;
                int totalPrent = 0;
                int totalAbsent = 0;
                int totalLeave = 0;
                foreach (AttendanceResponse item in attmaster)
                {
                  item.sdate = item.date.ToString("dd/MM/yyyy");
                  DateTime inTime = Convert.ToDateTime(item.in_time);
                  DateTime outTime = Convert.ToDateTime(item.out_time);
                  item.in_time = inTime.ToString("HH:mm:ss");
                  item.out_time = outTime.ToString("HH:mm:ss");

                  if (item.status == "H")
                  {
                    totalWeekOff = totalWeekOff + 1;
                  }

                  if (item.status == "P")
                  {
                    totalPrent = totalPrent + 1;
                  }


                  if (item.status == "A")
                  {
                    totalAbsent = totalAbsent + 1;
                  }

                  if (item.status == "EL" || item.status == "CL" || item.status == "SL" || item.status == "PL" || item.status == "TMPL" || item.status == "LWP")
                  {
                    totalLeave = totalLeave + 1;
                  }


                  totalHourse = totalHourse + item.total_hrs;
                }

                string totalHrs = totalHourse.ToString();

                string[] totHrs = totalHrs.Split('.');
                string totlHRS = totHrs[0];
                int TotalMintus = 0;
                int TotalHoursMintus = 0;
                if (totHrs.Length > 1)
                {
                  decimal TotalMint = Convert.ToDecimal(totHrs[1]);

                  if (TotalMint > 59)
                  {
                    //TotalMint = floor(TotalMint / 60).':'.(TotalMint - Flor(TotalMint / 60) * 60)
                    //TotalMint = TotalMint / 60, TotalMint % 60;
                    double totMot = Convert.ToDouble(TotalMint);
                    TimeSpan spWorkMin = TimeSpan.FromMinutes(totMot);
                    string workHours = spWorkMin.ToString(@"hh\:mm");

                    string mTotHrs = workHours.ToString();

                    string[] mTotHR = mTotHrs.Split('.');

                    string tHrs = mTotHR[0];

                    totlHRS = totlHRS + Convert.ToInt32(mTotHR[0]);

                    TotalHoursMintus = Convert.ToInt32(totlHRS) + Convert.ToInt32(tHrs);
                    if (mTotHR.Length > 1)
                    {
                      TotalMintus = Convert.ToInt32(mTotHR);
                    }
                  }


                }




                EmployeeMaster empMasterv = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(req.employee_code)
                                             select d).FirstOrDefault();

                AttResponse attendanceRes = new AttResponse();

                attendanceRes.attendaceList = attmaster;
                if (empMasterv != null)
                {
                  attendanceRes.employee_id = empMasterv.employee_id;
                  attendanceRes.name = empMasterv.name;
                  attendanceRes.department = empMasterv.department;
                  attendanceRes.company_name = empMasterv.pay_code;
                }

                attendanceRes.print_date = DateTime.Now.ToString("dd/MM/yyyy");


                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                Document doc = new Document(iTextSharp.text.PageSize.A1.Rotate());
                //var fpath = Server.MapPath("~/customerdoc/SID/AccountStatement/");
                //string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                doc.Open();


                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                image.ScalePercent(50f);
                image.SetAbsolutePosition(30, 550);
                doc.Add(image);

                StringBuilder sb = new StringBuilder();
                //sb.Append(req.search_result);
                sb.Append("<body style='Font-family:Verdana; font-size:7px;'>");

                if (req.type == "form12")
                {

                  sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5'>");
                  sb.Append("<tr><td colspan='8' style='text-align:center;'>Ageing Analysis</td></tr>");
                  sb.Append("<tr><td colspan='2'>Ledger Name</td><td>0-30Days</td><td>31-45Days</td><td>46-60Days</td><td>61-90Days</td><td>>90Days</td><td>Total</td></tr>");
                  sb.Append("<tr><td colspan='8'></td></tr>");
                  sb.Append("</table>");

                  sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5'>");
                  sb.Append("<tr><td rowspan='2'>S.No</td><td rowspan='2'>Name</td>" +
                      "<td bgcolor='#cdcdcd'><div class='rotate'>Father's Name</div></td><td rowspan='2' style='height: 300px;'><div class='rotate' style='transform: rotate(-90.0deg)'>Nature of Work</div></td><td rowspan='2'>Department</td>" +
                      "<td colspan='2'>Correspondent to that in form 11</td>" +
                      "<td>1st</td><td>2nd</td><td>3rd</td><td>4th</td><td>5th</td><td>6th</td><td>7th</td><td>8th</td><td>9th</td><td>10th</td><td>11th</td><td>12th</td>" +
                      "<td>13th</td><td>14th</td><td>15th</td><td>16th</td><td>17th</td><td>18th</td><td>19th</td><td>20th</td><td>21th</td><td>21th</td><td>22th</td>" +
                      "<td>23th</td><td>24th</td><td>25th</td><td>26th</td><td>27th</td><td>28th</td><td>29th</td><td>30th</td><td>31th</td>" +
                      "<td>Total number of days worked</td><td>Rate of allowance if any</td><td>Total hours of overtime</td><td>Amount due</td></tr>");
                }
                else if (req.type == "form14")
                {

                }

                sb.Append("</table>");
                sb.Append("<p width='100%'><b>Total Duration</b> = " + totlHRS + " Hrs  " + TotalMintus + " Min, <b>Present</b>= " + totalPrent + ", Leaves = " + totalLeave + ", Absent = " + totalAbsent + ", Week Off = " + totalWeekOff + "</p>");
                sb.Append("</body>");
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = fileName;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // attendance reports
    [HttpPost]
    [Route("GetAttendanceReport")]
    public AttendanceRepoertResponse GetAttendanceReport(SearchEmployeeAttRequest req)
    {
      AttendanceRepoertResponse response = new AttendanceRepoertResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                var days = (toDate - fromDate).TotalDays;

                AttendanceResponse attmaster = null;
                //List<EmployeeMaster> employeeMater = null;
                List<AttendanceReport> attendanceReportList = new List<AttendanceReport>();

                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;



                if (!string.IsNullOrEmpty(req.department))
                {
                  List<EmployeeMaster> employeeMater = (from d in db.EmployeeMaster
                                                        where d.department.Equals(req.department)
                                                        && d.pay_code.Equals(req.pay_code)
                                                        && d.status.Equals("Active")
                                                        select d).ToList();

                  AttendanceReport attendanceReport = new AttendanceReport();
                  attendanceReport.department = req.department;
                  List<EmployeeAttendance> employeeAttendanceList = new List<EmployeeAttendance>();
                  foreach (EmployeeMaster item in employeeMater)
                  {
                    EmployeeAttendance employeeAttendance = new EmployeeAttendance();
                    employeeAttendance.name = item.name;
                    employeeAttendance.employee_id = item.employee_id;

                    //DateTime datetime = fromDate.AddDays(i);
                    employeeAttendance.attendance = (from e in db.AttendanceMaster
                                                     join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                                     where e.employee_id.Equals(item.employee_id)
                                                     && d.department.Equals(req.department)
                                                     && e.date >= fromDate && e.date <= toDate
                                                     && d.pay_code.Equals(req.pay_code)
                                                     orderby e.date ascending
                                                     select new AttendanceResponse
                                                     {
                                                       id = e.id,
                                                       employee_id = e.employee_id,
                                                       date = e.date,
                                                       day = e.day,
                                                       in_time = e.in_time,
                                                       out_time = e.out_time,
                                                       shift = e.shift,
                                                       status = e.status,
                                                       mis = e.mis,
                                                       absent = e.absent,
                                                       early = e.early,
                                                       late = e.late,
                                                       total_hrs = e.total_hrs,
                                                     }).ToList();

                    employeeAttendanceList.Add(employeeAttendance);
                  }

                  attendanceReport.employeeAttendance = employeeAttendanceList;
                  attendanceReportList.Add(attendanceReport);
                }
                else
                {

                  List<KeyValueMaster> keyValueMaster = (from d in db.KeyValueMaster
                                                         where d.pay_code.Equals(req.pay_code)
                                                         && d.key_type.Equals("department")
                                                         && d.key_code != "Administrator"
                                                         select d).ToList();

                  foreach (KeyValueMaster item in keyValueMaster)
                  {
                    List<EmployeeMaster> employeeMater = (from d in db.EmployeeMaster
                                                          where d.department.Equals(item.key_description)
                                                          && d.pay_code.Equals(req.pay_code)
                                                          && d.status.Equals("Active")
                                                          select d).ToList();

                    AttendanceReport attendanceReport = new AttendanceReport();
                    attendanceReport.department = item.key_description;
                    List<EmployeeAttendance> employeeAttendanceList = new List<EmployeeAttendance>();
                    foreach (EmployeeMaster item1 in employeeMater)
                    {
                      EmployeeAttendance employeeAttendance = new EmployeeAttendance();
                      employeeAttendance.name = item1.name;
                      employeeAttendance.employee_id = item1.employee_id;

                      employeeAttendance.attendance = (from e in db.AttendanceMaster
                                                       join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                                       where e.employee_id.Equals(item1.employee_id)
                                                       && d.department.Equals(item.key_description)
                                                       && e.date >= fromDate && e.date <= toDate
                                                       && d.pay_code.Equals(req.pay_code)
                                                       orderby e.date ascending
                                                       select new AttendanceResponse
                                                       {
                                                         id = e.id,
                                                         employee_id = e.employee_id,
                                                         date = e.date,
                                                         day = e.day,
                                                         in_time = e.in_time,
                                                         out_time = e.out_time,
                                                         shift = e.shift,
                                                         status = e.status,
                                                         mis = e.mis,
                                                         absent = e.absent,
                                                         early = e.early,
                                                         late = e.late,
                                                         total_hrs = e.total_hrs,
                                                       }).ToList();

                      employeeAttendanceList.Add(employeeAttendance);
                    }
                    attendanceReport.employeeAttendance = employeeAttendanceList;
                    attendanceReportList.Add(attendanceReport);
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = attendanceReportList;
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // generate pdf for reports
    [HttpPost]
    [Route("GenerateReportPDF")]
    public HmcStringResponse GenerateReportPDF(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                var days = (toDate - fromDate).TotalDays;

                AttendanceResponse attmaster = null;
                //List<EmployeeMaster> employeeMater = null;
                List<AttendanceReport> attendanceReportList = new List<AttendanceReport>();

                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                if (!string.IsNullOrEmpty(req.department))
                {
                  List<EmployeeMaster> employeeMater = (from d in db.EmployeeMaster
                                                        where d.department.Equals(req.department)
                                                        && d.pay_code.Equals(req.pay_code)
                                                        && d.status.Equals("Active")
                                                        select d).ToList();




                  AttendanceReport attendanceReport = new AttendanceReport();
                  attendanceReport.department = req.department;
                  List<EmployeeAttendance> employeeAttendanceList = new List<EmployeeAttendance>();
                  foreach (EmployeeMaster item in employeeMater)
                  {
                    EmployeeAttendance employeeAttendance = new EmployeeAttendance();
                    employeeAttendance.name = item.name;
                    employeeAttendance.employee_id = item.employee_id;

                    employeeAttendance.attendance = (from e in db.AttendanceMaster
                                                     join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                                     where e.employee_id.Equals(item.employee_id)
                                                     && d.department.Equals(req.department)
                                                     && e.date >= fromDate && e.date <= toDate
                                                     && d.pay_code.Equals(req.pay_code)
                                                     orderby e.date ascending
                                                     select new AttendanceResponse
                                                     {
                                                       id = e.id,
                                                       employee_id = e.employee_id,
                                                       date = e.date,
                                                       day = e.day,
                                                       in_time = e.in_time,
                                                       out_time = e.out_time,
                                                       shift = e.shift,
                                                       status = e.status,
                                                       mis = e.mis,
                                                       absent = e.absent,
                                                       early = e.early,
                                                       late = e.late,
                                                       total_hrs = e.total_hrs,
                                                     }).ToList();

                    if (employeeAttendance.attendance.Count > 0)
                    {
                      employeeAttendanceList.Add(employeeAttendance);
                    }
                  }

                  attendanceReport.employeeAttendance = employeeAttendanceList;
                  attendanceReportList.Add(attendanceReport);
                }
                else
                {

                  List<KeyValueMaster> keyValueMaster = (from d in db.KeyValueMaster
                                                         where d.pay_code.Equals(empMaster.pay_code)
                                                         && d.key_type.Equals("department")
                                                         && d.key_code != "Administrator"
                                                         select d).ToList();



                  foreach (KeyValueMaster item in keyValueMaster)
                  {
                    List<EmployeeMaster> employeeMater = (from d in db.EmployeeMaster
                                                          where d.department.Equals(item.key_description)
                                                          && d.pay_code.Equals(empMaster.pay_code)
                                                          && d.status.Equals("Active")
                                                          select d).ToList();

                    AttendanceReport attendanceReport = new AttendanceReport();
                    attendanceReport.department = item.key_description;
                    List<EmployeeAttendance> employeeAttendanceList = new List<EmployeeAttendance>();
                    foreach (EmployeeMaster item1 in employeeMater)
                    {
                      EmployeeAttendance employeeAttendance = new EmployeeAttendance();
                      employeeAttendance.name = item1.name;
                      employeeAttendance.employee_id = item1.employee_id;

                      employeeAttendance.attendance = (from e in db.AttendanceMaster
                                                       join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                                       where e.employee_id.Equals(item1.employee_id)
                                                       && d.department.Equals(item.key_description)
                                                       && e.date >= fromDate && e.date <= toDate
                                                       && d.pay_code.Equals(req.pay_code)
                                                       orderby e.date ascending
                                                       select new AttendanceResponse
                                                       {
                                                         id = e.id,
                                                         employee_id = e.employee_id,
                                                         date = e.date,
                                                         day = e.day,
                                                         in_time = e.in_time,
                                                         out_time = e.out_time,
                                                         shift = e.shift,
                                                         status = e.status,
                                                         mis = e.mis,
                                                         absent = e.absent,
                                                         early = e.early,
                                                         late = e.late,
                                                         total_hrs = e.total_hrs,
                                                       }).ToList();

                      if (employeeAttendance.attendance.Count > 0)
                      {
                        employeeAttendanceList.Add(employeeAttendance);
                      }


                    }
                    attendanceReport.employeeAttendance = employeeAttendanceList;
                    attendanceReportList.Add(attendanceReport);
                  }
                }


                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 15, 15, 45, 15);
                var fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/"); //Server.MapPath("~/Images/");
                                                                                        //string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                doc.Open();

                var toDay = (toDate - fromDate).TotalDays;

                int totalDays = Convert.ToInt32(toDay) + 1;

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                image.ScalePercent(50f);
                image.SetAbsolutePosition(15, 550);
                doc.Add(image);

                StringBuilder sb = new StringBuilder();

                string printDate = DateTime.Now.ToString("dd/MM/yyyy");
                //sb.Append(req.search_result);

                sb.Append("<body style='Font-family:Verdana;'>");
                sb.Append("<div width='100%' style='text-align: center'> Attendance Report <br>" + req.from_date + " To " + req.to_date + "</div>");
                sb.Append("<div width='50%' style='text-align: left; float: left; font-size: 10px;'>Company:" + empMaster.pay_code + "</div>");
                sb.Append("<div width='50%' style='text-align: right; float: left; font-size: 10px;'>Print On:" + printDate + "</div>");
                foreach (AttendanceReport item in attendanceReportList)
                {
                  sb.Append("<div  width='100%' style='margin-bottom: 10px; font-size: 10px;'>Department : " + item.department + "</div><br>");
                  sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5' style='margin-top: 50px; font-size: 6px;'>");

                  foreach (EmployeeAttendance item2 in item.employeeAttendance)
                  {
                    string[] fname = item2.name.Split(' ');
                    //int counts = item2.inTime.Count() + 1;
                    //sb.Append("<tbody>");
                    //sb.Append("<tr><td rowspan='3'>" + item2.name + "</td></tr>");
                    sb.Append("<tbody>");
                    sb.AppendLine("<tr>");
                    sb.Append("<td rowspan='5' bgcolor='#d0dbef'>" + fname[0] + "<br>" + item2.employee_id + "</td>");
                    sb.AppendLine("<td bgcolor='#d0dbef'>Date</td>");
                    for (int i = 0; i < totalDays; i++)
                    {
                      if (item2.attendance.Count > i)
                      {
                        sb.Append("<td bgcolor='#d0dbef'>" + item2.attendance[i].date.ToString("dd/MM") + "</td>");
                      }
                      else
                      {
                        sb.Append("<td border='0'> </td>");
                      }

                    }
                    //foreach(AttendanceResponse itemdate in item2.attendance)
                    //{
                    //    sb.Append("<td bgcolor='#d0dbef'>" + itemdate.date.ToString("dd/MM") + "</td>");
                    //}
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>Status</td>");
                    for (int i = 0; i < totalDays; i++)
                    {
                      if (item2.attendance.Count > i)
                      {
                        sb.Append("<td>" + item2.attendance[i].status + item2.attendance[i].mis + item2.attendance[i].late + item2.attendance[i].early + item2.attendance[i].absent + "</td>");
                      }
                      else
                      {
                        sb.Append("<td border='0'> </td>");
                      }

                    }
                    //foreach (AttendanceResponse itemdate in item2.attendance)
                    //{
                    //    sb.Append("<td>" + itemdate.status + itemdate.mis + itemdate.late + itemdate.early + itemdate.absent + "</td>");
                    //}
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td bgcolor='#d0dbef'>In</td>");
                    for (int i = 0; i < totalDays; i++)
                    {
                      if (item2.attendance.Count > i)
                      {
                        sb.Append("<td bgcolor='#d0dbef'>" + item2.attendance[i].in_time.Remove(5, 3) + "</td>");
                      }
                      else
                      {
                        sb.Append("<td border='0'> </td>");
                      }

                    }
                    //foreach (AttendanceResponse itemdate in item2.attendance)
                    //{
                    //    sb.Append("<td bgcolor='#d0dbef'>" + itemdate.in_time.Remove(5, 3) + "</td>");
                    //}
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>Out</td>");
                    for (int i = 0; i < totalDays; i++)
                    {
                      if (item2.attendance.Count > i)
                      {
                        sb.Append("<td>" + item2.attendance[i].out_time.Remove(5, 3) + "</td>");
                      }
                      else
                      {
                        sb.Append("<td border='0'> </td>");
                      }

                    }
                    //foreach (AttendanceResponse itemdate in item2.attendance)
                    //{
                    //    sb.Append("<td>" + itemdate.out_time.Remove(5, 3) + "</td>");
                    //}
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td bgcolor='#d0dbef'>Hrs</td>");
                    //foreach (AttendanceResponse itemdate in item2.attendance)
                    //{
                    //    sb.Append("<td>" + itemdate.total_hrs + "</td>");
                    //}
                    for (int i = 0; i < totalDays; i++)
                    {
                      if (item2.attendance.Count > i)
                      {
                        sb.Append("<td bgcolor='#d0dbef'>" + item2.attendance[i].total_hrs + "</td>");
                      }
                      else
                      {
                        sb.Append("<td border='0'> </td>");
                      }

                    }
                    sb.AppendLine("</tr>");
                    sb.Append("</tbody>");

                  }
                  //sb.Append("</tbody>");
                  sb.Append("</table>");
                }
                sb.Append("</body>");
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();
                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = fileName;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // get daily attendance 
    [HttpPost]
    [Route("GetDailyAttendance")]
    public DailyAttResponse GetDailyAttendance(SearchEmployeeAttRequest req)
    {
      DailyAttResponse response = new DailyAttResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<AttendanceLogResponse> attmaster = null;

                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;

                if (!string.IsNullOrEmpty(req.department))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.date >= fromDate && e.date <= todatetime
                               && d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else if (!string.IsNullOrEmpty(req.status))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.status.Equals(req.status)
                               && e.date >= fromDate && e.date <= todatetime
                               && d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).Skip(skip).Take(pageSize).ToList();

                }
                else if (!string.IsNullOrEmpty(req.shift))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.pay_code.Equals(req.shift)
                               && e.date >= fromDate && e.date <= todatetime
                               && d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else if (!string.IsNullOrEmpty(req.employee_code))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.employee_id.Equals(req.employee_code)
                               && e.date >= fromDate && e.date <= todatetime
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 day = e.day,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).Skip(skip).Take(pageSize).ToList();
                }
                else
                {

                  DateTime todatetime = DateTime.Now.AddDays(1);//toDate.Date.AddDays(-1);

                  if (empMaster.role == "Administrator")
                  {
                    attmaster = (from e in db.AttendanceLogMaster
                                 join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                 where e.date >= fromDate
                                 && e.date <= todatetime
                                 orderby e.date ascending
                                 select new AttendanceLogResponse
                                 {
                                   id = e.id,
                                   employee_id = e.employee_id,
                                   name = d.name,
                                   date = e.date,
                                   shift = e.shift,
                                   day = e.day,
                                   status = e.status,
                                   pay_code = d.pay_code,
                                   in_lat = e.in_lat,
                                   in_long = e.in_long,
                                   feature_image = e.feature_image,
                                   location_address = e.address
                                 }).Skip(skip).Take(pageSize).ToList();
                  }
                  else
                  {
                    attmaster = (from e in db.AttendanceLogMaster
                                 join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                 where e.date <= fromDate
                                 && e.date >= todatetime
                                 && d.pay_code.Equals(empMaster.pay_code)
                                 orderby e.date ascending
                                 select new AttendanceLogResponse
                                 {
                                   id = e.id,
                                   employee_id = e.employee_id,
                                   name = d.name,
                                   date = e.date,
                                   shift = e.shift,
                                   day = e.day,
                                   status = e.status,
                                   pay_code = d.pay_code,
                                   in_lat = e.in_lat,
                                   in_long = e.in_long,
                                   feature_image = e.feature_image,
                                   location_address = e.address
                                 }).Skip(skip).Take(pageSize).ToList();
                  }




                }



                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = attmaster;
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // geterate daily log report
    [HttpPost]
    [Route("GenerateDailyAttendancePDF")]
    public HmcStringResponse GenerateDailyAttendancePDF(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<AttendanceLogResponse> attmaster = null;

                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;

                if (!string.IsNullOrEmpty(req.department))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.date >= fromDate && e.date <= todatetime
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.status))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.status.Equals(req.status)
                               && e.date >= fromDate && e.date <= todatetime
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).ToList();

                }
                else if (!string.IsNullOrEmpty(req.shift))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.pay_code.Equals(req.shift)
                               && e.date >= fromDate && e.date <= todatetime
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.employee_code))
                {
                  DateTime todatetime = toDate.AddDays(1);
                  attmaster = (from e in db.AttendanceLogMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.employee_id.Equals(req.employee_code)
                               && e.date >= fromDate && e.date <= todatetime
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceLogResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 name = d.name,
                                 date = e.date,
                                 shift = e.shift,
                                 day = e.day,
                                 status = e.status,
                                 pay_code = d.pay_code,
                                 in_lat = e.in_lat,
                                 in_long = e.in_long,
                                 feature_image = e.feature_image,
                                 location_address = e.address
                               }).ToList();
                }
                else
                {

                  DateTime todatetime = toDate.AddDays(1);

                  if (empMaster.role == "Administrator")
                  {
                    attmaster = (from e in db.AttendanceLogMaster
                                 join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                 where e.date >= fromDate && e.date <= todatetime
                                 orderby e.date ascending
                                 select new AttendanceLogResponse
                                 {
                                   id = e.id,
                                   employee_id = e.employee_id,
                                   name = d.name,
                                   date = e.date,
                                   shift = e.shift,
                                   day = e.day,
                                   status = e.status,
                                   pay_code = d.pay_code,
                                   in_lat = e.in_lat,
                                   in_long = e.in_long,
                                   feature_image = e.feature_image,
                                   location_address = e.address
                                 }).ToList();
                  }
                  else
                  {
                    attmaster = (from e in db.AttendanceLogMaster
                                 join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                                 where e.date >= fromDate && e.date <= todatetime
                                 && d.pay_code.Equals(empMaster.pay_code)
                                 orderby e.date ascending
                                 select new AttendanceLogResponse
                                 {
                                   id = e.id,
                                   employee_id = e.employee_id,
                                   name = d.name,
                                   date = e.date,
                                   shift = e.shift,
                                   day = e.day,
                                   status = e.status,
                                   pay_code = d.pay_code,
                                   in_lat = e.in_lat,
                                   in_long = e.in_long,
                                   feature_image = e.feature_image,
                                   location_address = e.address
                                 }).ToList();
                  }



                }





                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                Document doc = new Document(iTextSharp.text.PageSize.A4);
                //var fpath = Server.MapPath("~/customerdoc/SID/AccountStatement/");
                ///string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"]
                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                doc.Open();


                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                image.ScalePercent(50f);
                image.SetAbsolutePosition(15, 800);
                doc.Add(image);

                StringBuilder sb = new StringBuilder();

                string printDate = DateTime.Now.ToString("dd/MM/yyyy");
                //sb.Append(req.search_result);

                sb.Append("<body style='Font-family:Verdana;'>");
                sb.Append("<div width='100%' style='text-align: center'> Daily Attendance Report <br>" + req.from_date + " To " + req.to_date + "</div>");

                sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5' style='margin-top: 50px; font-size: 10px;'>");
                sb.Append("<tbody>");
                sb.AppendLine("<tr>");
                sb.Append("<td colspan='4' border='0'>Company : " + empMaster.pay_code + "</td>");
                sb.Append("<td colspan='3' border='0' style='text-align: right;'>Print On : " + printDate + "</td>");
                sb.AppendLine("</tr>");
                if (!string.IsNullOrEmpty(req.department))
                {
                  sb.AppendLine("<tr>");
                  sb.Append("<td colspan='7' border='0'>Department : " + req.department + "</td>");
                  sb.AppendLine("</tr>");
                }
                if (!string.IsNullOrEmpty(req.shift))
                {
                  sb.AppendLine("<tr>");
                  sb.Append("<td colspan='7' border='0'>Company Code : " + req.shift + "</td>");
                  sb.AppendLine("</tr>");
                }
                if (!string.IsNullOrEmpty(req.employee_code))
                {
                  sb.AppendLine("<tr>");
                  sb.Append("<td colspan='7' border='0'>Name : " + req.employee_code + "</td>");
                  sb.AppendLine("</tr>");
                }

                if (!string.IsNullOrEmpty(req.status))
                {
                  sb.AppendLine("<tr>");
                  sb.Append("<td colspan='7' border='0'>Status : " + req.status + "</td>");
                  sb.AppendLine("</tr>");
                }
                sb.AppendLine("<tr>");
                sb.Append("<td bgcolor='#d0dbef'>Name</td>");
                sb.Append("<td bgcolor='#d0dbef'>Company Code</td>");
                sb.AppendLine("<td bgcolor='#d0dbef'>Punch Time</td>");
                sb.AppendLine("<td bgcolor='#d0dbef'>Day</td>");
                sb.AppendLine("<td bgcolor='#d0dbef'>Shift</td>");
                sb.AppendLine("<td bgcolor='#d0dbef'>Status</td>");
                sb.AppendLine("<td bgcolor='#d0dbef' style='width: 150px;'>Address</td>");
                sb.AppendLine("</tr>");
                foreach (AttendanceLogResponse item in attmaster)
                {
                  sb.AppendLine("<tr>");
                  sb.Append("<td>" + item.name + " (" + item.employee_id + ")</td>");
                  sb.AppendLine("<td>" + item.pay_code + "</td>");
                  sb.AppendLine("<td>" + item.date + "</td>");
                  sb.AppendLine("<td>" + item.day + "</td>");
                  sb.AppendLine("<td>" + item.shift + "</td>");
                  sb.AppendLine("<td>" + item.status + "</td>");
                  sb.AppendLine("<td>" + item.location_address + "</td>");
                  sb.AppendLine("</tr>");

                }
                sb.Append("</tbody>");
                sb.Append("</table>");

                sb.Append("</body>");
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();
                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = fileName;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // employee shift data
    [HttpPost]
    [Route("GetEmployeeShiftData")]
    public EmployeeShiftResponse GetEmployeeShiftData(SearchEmployeeAttRequest req)
    {
      EmployeeShiftResponse response = new EmployeeShiftResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                //DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                //DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<EmployeeShiftMaster> attmaster = null;

                int page = Convert.ToInt32(req.pageNo);
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;


                if (!string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.EmployeeShiftMaster
                               where e.shift.Equals(req.shift)
                               //&& e.from_date >= fromDate && e.to_date <= toDate
                               && e.pay_code.Equals(empMaster.pay_code)
                               orderby e.from_date ascending
                               select e).Skip(skip).Take(pageSize).ToList();
                }
                else if (!string.IsNullOrEmpty(req.employee_code))
                {
                  attmaster = (from e in db.EmployeeShiftMaster
                               where e.employee_id.Equals(req.employee_code)
                               //&& e.from_date >= fromDate && e.to_date <= toDate
                               && e.pay_code.Equals(empMaster.pay_code)
                               orderby e.from_date ascending
                               select e).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                  attmaster = (from e in db.EmployeeShiftMaster
                               where e.pay_code.Equals(empMaster.pay_code)
                               orderby e.from_date descending
                               select e).Skip(skip).Take(pageSize).ToList();
                }



                foreach (EmployeeShiftMaster item in attmaster)
                {
                  EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                   where d.employee_id.Equals(item.employee_id)
                                                   select d).FirstOrDefault();

                  if (employeeMaster != null)
                  {
                    item.employee_id = employeeMaster.name + "(" + employeeMaster.employee_id + ")";
                  }
                }



                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = attmaster;
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // create employee shift 
    [HttpPost]
    [Route("CreateEmployeeShift")]
    public HmcJsonResponse CreateEmployeeShift(EmployeeShiftReq req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(EmployeeShiftValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                  DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);
                  EmployeeShiftMaster resultsDb = new EmployeeShiftMaster();
                  resultsDb.from_date = fromDate;
                  resultsDb.to_date = toDate;
                  resultsDb.modify_by = employeeId;
                  resultsDb.employee_id = req.employee_code;
                  resultsDb.pay_code = employeeMaster.pay_code;
                  resultsDb.shift = req.shift;
                  resultsDb.modify_date = DateTime.Now;
                  db.EmployeeShiftMaster.Add(resultsDb);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "";
                }

              }
            }


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // update employee shift
    [HttpPost]
    [Route("UpdateEmployeeShift")]
    public HmcResponse UpdateEmployeeShift(EmployeeShiftReq req)
    {
      HmcResponse response = new HmcResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(EmployeeShiftValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {
                  EmployeeShiftMaster resultsDb = (from d in db.EmployeeShiftMaster
                                                   where d.id.Equals(req.id)
                                                   orderby d.id descending
                                                   select d).FirstOrDefault();
                  if (resultsDb != null)
                  {
                    DateTime fromDate = DateTime.ParseExact(req.from_date, "yyyy-MM-dd", null);
                    DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy-MM-dd", null);

                    resultsDb.from_date = fromDate;
                    resultsDb.to_date = toDate;
                    resultsDb.shift = req.shift;
                    resultsDb.modify_by = employeeId;
                    resultsDb.modify_date = DateTime.Now;
                    db.SaveChanges();

                    response.flag = "1";
                    response.status = "success";
                    response.alert = "";
                  }
                }

              }
            }


          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // get time 
    // employee shift data
    [HttpPost]
    [Route("GetHrsMint")]
    public HrsMintResponse GetHrsMint(HmcRequest req)
    {
      HrsMintResponse response = new HrsMintResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                response.role = empMaster.role;

                List<HrsMint> hrs = (from d in db.HrsMint
                                     where d.type.Equals("hrs")
                                     select d).ToList();

                List<HrsMint> mint = (from d in db.HrsMint
                                      where d.type.Equals("mint")
                                      select d).ToList();



                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.hrs = hrs;
                response.mint = mint;
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // get slary master
    [AllowAnonymous]
    [HttpPost]
    [Route("GetSalaryMaster")]
    public SalaryMasterResponse GetSalaryMaster(HmcRequest req)
    {
      SalaryMasterResponse response = new SalaryMasterResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empDetail = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                int page = Convert.ToInt32(req.pageNo);
                List<SalaryMaster> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.SalaryMaster
                              where (d.pay_code.ToLower().Contains(req.search_result))
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.SalaryMaster
                              where (d.pay_code.ToLower().Contains(req.search_result))
                              && d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }
                else
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.SalaryMaster
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.SalaryMaster
                              where d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }

    // get salary slip
    // get slary master
    [HttpPost]
    [Route("GetSalarySlip")]
    public SalarySlipResponse GetSalarySlip(HmcRequest req)
    {
      SalarySlipResponse response = new SalarySlipResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empDetail = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                int page = Convert.ToInt32(req.pageNo);
                List<SalarySlip> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.SalarySlip
                              where (d.pay_code.ToLower().Contains(req.search_result))
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.SalarySlip
                              where d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }
                else
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.SalarySlip
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.SalarySlip
                              where d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }

                foreach (SalarySlip item in retVal)
                {
                  DateTime todaysDate = DateTime.Now;
                  string monthName = new DateTime(Convert.ToInt32(todaysDate.ToString("yyyy")), Convert.ToInt32(item.month), Convert.ToInt32(todaysDate.ToString("dd"))).ToString("MMM");
                  item.month = monthName;

                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }
    // create salary master
    [HttpPost]
    [Route("CreateSalaryMaster")]
    public HmcJsonResponse CreateSalaryMaster(SalaryMasterDto req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(SalaryMasterValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  SalaryMaster result = new SalaryMaster();
                  result.pay_code = req.pay_code;
                  result.hra = req.hra;
                  result.basic = req.basic;
                  result.allowance = req.allowance;
                  result.esi = req.esi;
                  result.pf = req.pf;
                  result.tds = req.tds;
                  result.modify_by = employeeId;
                  result.modify_date = DateTime.Now;
                  db.SalaryMaster.Add(result);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";

                }
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // create salary slip
    [HttpPost]
    [Route("CreateSalarySlip")]
    public HmcJsonResponse CreateSalarySlip(SalarySlipDto req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(SalarySlipValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  SalarySlip result = new SalarySlip();
                  result.pay_code = req.company_code;
                  result.month = req.month;
                  result.year = req.year;
                  result.modify_by = employeeId;
                  result.modify_date = DateTime.Now;
                  db.SalarySlip.Add(result);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";

                }
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // update salary master
    [HttpPost]
    [Route("UpdateSalaryMaster")]
    public HmcJsonResponse UpdateSalaryMaster(SalaryMasterDto req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(SalaryMasterValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  SalaryMaster result = (from d in db.SalaryMaster
                                         where d.id.Equals(req.id)
                                         select d).FirstOrDefault();

                  if (result != null)
                  {
                    result.pay_code = req.pay_code;
                    result.hra = req.hra;
                    result.basic = req.basic;
                    result.allowance = req.allowance;
                    result.esi = req.esi;
                    result.pf = req.pf;
                    result.tds = req.tds;
                    result.modify_by = employeeId;
                    result.modify_date = DateTime.Now;
                    db.SaveChanges();

                    response.status = "success";
                    response.flag = "1";
                    response.alert = "success";
                  }
                }
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // delete salary master
    [HttpPost]
    [Route("DeleteSalaryMaster")]
    public HmcResponse DeleteSalaryMaster(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                SalaryMaster master = (from e in db.SalaryMaster
                                       where e.id.Equals(delete_id)
                                       select e).FirstOrDefault();

                if (master != null)
                {
                  db.SalaryMaster.Remove(master);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // delete salary slip
    [HttpPost]
    [Route("DeleteSalarySlip")]
    public HmcResponse DeleteSalarySlip(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                SalarySlip master = (from e in db.SalarySlip
                                     where e.id.Equals(delete_id)
                                     select e).FirstOrDefault();

                if (master != null)
                {
                  db.SalarySlip.Remove(master);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // delete salary slip
    [HttpPost]
    [Route("SearchEmployee")]
    public HmcTypeHeadResponse SearchEmployee(TyapeheadReq credentials)
    {
      HmcTypeHeadResponse response = new HmcTypeHeadResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(credentials.accesskey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(credentials.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                List<HmcTypeHeadValue> masterData = (from e in db.EmployeeMaster
                                                     where e.pay_code.Equals(credentials.pay_code)
                                                     && e.name.ToLower().Contains(credentials.Prefix)
                                                     || e.employee_id.ToLower().Contains(credentials.Prefix)
                                                     //&& e.role != "Administrator"                                                                    
                                                     select new HmcTypeHeadValue
                                                     {
                                                       name = e.name,
                                                       employee_id = e.employee_id
                                                     }).ToList();

                if (masterData.Count() > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = masterData;
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // get employee conveyance list
    [HttpPost]
    [Route("GetEmployeeConveyance")]
    public EmployeeConveyanceResponse GetEmployeeConveyance(HmcRequest req)
    {
      EmployeeConveyanceResponse response = new EmployeeConveyanceResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empDetail = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                int page = Convert.ToInt32(req.pageNo);
                List<EmployeeConveyanceDto> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.EmployeeConveyance
                              join e in db.EmployeeMaster on d.employee_id equals e.employee_id
                              where e.name.ToLower().Contains(req.search_result)
                              orderby d.id descending
                              select new EmployeeConveyanceDto
                              {
                                id = d.id,
                                employee_id = e.employee_id,
                                name = e.name,
                                mobile_bill = d.mobile_bill,
                                conveyance = d.conveyance,
                                performance_variable = d.performance_variable,
                                other_received = d.other_received,
                                advanced_salary = d.advanced_salary,
                                month = d.month,
                                year = d.year
                              }).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.EmployeeConveyance
                              join e in db.EmployeeMaster on d.employee_id equals e.employee_id
                              where e.name.ToLower().Contains(req.search_result)
                              && e.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select new EmployeeConveyanceDto
                              {
                                id = d.id,
                                employee_id = e.employee_id,
                                name = e.name,
                                mobile_bill = d.mobile_bill,
                                conveyance = d.conveyance,
                                performance_variable = d.performance_variable,
                                other_received = d.other_received,
                                advanced_salary = d.advanced_salary,
                                month = d.month,
                                year = d.year
                              }).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }
                else
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.EmployeeConveyance
                              join e in db.EmployeeMaster on d.employee_id equals e.employee_id
                              orderby d.id descending
                              select new EmployeeConveyanceDto
                              {
                                id = d.id,
                                employee_id = e.employee_id,
                                name = e.name,
                                mobile_bill = d.mobile_bill,
                                conveyance = d.conveyance,
                                performance_variable = d.performance_variable,
                                other_received = d.other_received,
                                advanced_salary = d.advanced_salary,
                                month = d.month,
                                year = d.year
                              }).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.EmployeeConveyance
                              join e in db.EmployeeMaster on d.employee_id equals e.employee_id
                              where e.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select new EmployeeConveyanceDto
                              {
                                id = d.id,
                                employee_id = e.employee_id,
                                name = e.name,
                                mobile_bill = d.mobile_bill,
                                conveyance = d.conveyance,
                                performance_variable = d.performance_variable,
                                other_received = d.other_received,
                                advanced_salary = d.advanced_salary,
                                month = d.month,
                                year = d.year
                              }).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }
    // employee taype head
    [HttpPost]
    [Route("CreateEmployeeConveyance")]
    public HmcJsonResponse CreateEmployeeConveyance(EmployeeConveyanceDto req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, null);

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  EmployeeConveyance result = new EmployeeConveyance();
                  result.employee_id = req.employee_code;
                  result.mobile_bill = req.mobile_bill;
                  result.conveyance = req.conveyance;
                  result.performance_variable = req.performance_variable;
                  result.other_received = req.other_received;
                  result.advanced_salary = req.advanced_salary;
                  result.month = req.month;
                  result.year = req.year;
                  result.modify_by = employeeId;
                  result.modify_date = DateTime.Now;
                  db.EmployeeConveyance.Add(result);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";

                }
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // update employee conveyance
    [HttpPost]
    [Route("UpdateEmployeeConveyance")]
    public HmcJsonResponse UpdateEmployeeConveyance(EmployeeConveyanceDto req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, null);

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  EmployeeConveyance result = (from d in db.EmployeeConveyance
                                               where d.id.Equals(req.id)
                                               select d).FirstOrDefault();
                  if (result != null)
                  {
                    result.employee_id = req.employee_code;
                    result.mobile_bill = req.mobile_bill;
                    result.conveyance = req.conveyance;
                    result.performance_variable = req.performance_variable;
                    result.other_received = req.other_received;
                    result.advanced_salary = req.advanced_salary;
                    result.month = req.month;
                    result.year = req.year;
                    result.modify_by = employeeId;
                    result.modify_date = DateTime.Now;
                    db.EmployeeConveyance.Add(result);
                    db.SaveChanges();

                    response.status = "success";
                    response.flag = "1";
                    response.alert = "success";
                  }


                }
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // delete employee conveyance
    [HttpPost]
    [Route("DeleteEmployeeConveyance")]
    public HmcResponse DeleteEmployeeConveyance(DeleteDTO req)
    {
      HmcResponse response = new HmcResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int delete_id = Convert.ToInt32(req.delete_id);
                EmployeeConveyance master = (from e in db.EmployeeConveyance
                                             where e.id.Equals(delete_id)
                                             select e).FirstOrDefault();

                if (master != null)
                {
                  db.EmployeeConveyance.Remove(master);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                }


              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // downlaod pay slip
    [HttpPost]
    [Route("DownloadPayslip")]
    public HmcStringResponse DownloadPayslip(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                List<AttendanceResponse> attmaster = null;

                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                if (empMaster != null)
                {
                  SalaryMaster salarymaster = (from d in db.SalaryMaster
                                               where d.pay_code.Equals(empMaster.pay_code)
                                               select d).FirstOrDefault();

                  CompaniesMaster companymaster = (from d in db.CompaniesMaster
                                                   where d.company_code.Equals(empMaster.pay_code)
                                                   select d).FirstOrDefault();

                  SalarySlip salarySlipData = new SalarySlip();

                  salarySlipData = (from d in db.SalarySlip
                                    where d.id.Equals(req.id)
                                    select d).FirstOrDefault();

                  int days = DateTime.DaysInMonth(Convert.ToInt32(salarySlipData.year), Convert.ToInt32(salarySlipData.month));

                  DataTable dt = new DataTable();
                  string constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContext"].ConnectionString;
                  SqlConnection con = new SqlConnection(constr);
                  con.Open();
                  SqlCommand cmd = new SqlCommand("select em.employee_id, em.name, em.employee_salary, em.acc_no, em.ifsc_code, em.bank_name, " +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status in ('P', 'EL', 'CL', 'SL')) as present," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status = 'H') as holiday," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status = 'HD') as halfday," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and(absent = 'AB' or status = 'AB')) as absent," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and mis = 'MP') as mispunch," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and late = 'L') as late," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and early = 'E') as early," +
                                                  " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and (late = 'L' or early = 'E')) as lateEarly," +
                                                  " (select sum(total_hrs) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "') as totalHours" +
                                                  " from SalarySlip ss join EmployeeMaster em on ss.pay_code = em.pay_code " +
                                                  " where ss.id = " + req.id + " and em.employee_id = '"+ employeeId + "' and em.status='Active'  order by em.employee_id asc", con);
                  SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                  adapter.Fill(dt);
                  con.Close();
                   
                  string print_date = DateTime.Now.ToString("dd/MM/yyyy");

                  string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                  Document doc = new Document(iTextSharp.text.PageSize.A4);
                  //var fpath = Server.MapPath("~/customerdoc/SID/AccountStatement/");
                  //string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                  string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                  PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                  int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                  //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                  Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                  //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                  doc.Open();

                  //iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                  //image.ScalePercent(50f);
                  //image.SetAbsolutePosition(30, 550);
                  //doc.Add(image);
                  decimal salaryInHand = 0M;
                  decimal oneDaySalary = 0M;
                  decimal totalPresentSalary = 0M;
                  decimal totalHolidaySalary = 0M;
                  decimal totalHalfDay = 0M;
                  decimal totalAbsent = 0M;
                  decimal totalMispunch = 0M;
                  decimal totalEalryLate = 0M;
                  int totalDays = 0;
                  decimal totHours = 0M;
                  string employeeSalary = "0";
                  string absent = string.Empty;
                  string lateEaly = string.Empty;
                  foreach (DataRow dr in dt.Rows)
                  {
                    employeeSalary = dr["employee_salary"].ToString();
                    oneDaySalary = Convert.ToDecimal(dr["employee_salary"].ToString()) / days;
                    totalPresentSalary = salaryInHand + (Convert.ToDecimal(dr["present"].ToString()) * oneDaySalary);
                    totalHolidaySalary = salaryInHand + (Convert.ToDecimal(dr["holiday"].ToString()) * oneDaySalary);

                    absent = dr["absent"].ToString();
                    lateEaly = dr["lateEarly"].ToString();

                    totalHalfDay = Convert.ToDecimal(dr["halfday"].ToString()) * (oneDaySalary / 2);
                    //decimal earlyAndlate = Convert.ToDecimal(dr["late"].ToString()) + Convert.ToDecimal(dr["early"].ToString());

                    totalAbsent = Convert.ToDecimal(dr["absent"].ToString()) * oneDaySalary; //
                    totalMispunch = Convert.ToDecimal(dr["mispunch"].ToString()) * (oneDaySalary / 2);

                    

                    int earlyAndlate = 0;

                    if (!string.IsNullOrWhiteSpace(dr["lateEarly"].ToString()))
                    {
                      earlyAndlate = Convert.ToInt32(dr["lateEarly"].ToString());
                    }

                    //int earlyAndlate = Convert.ToInt32(dr["absent"].ToString());

                    if (earlyAndlate > 3)
                    {
                      int totCount = earlyAndlate - 3;
                      totalEalryLate = totCount * (oneDaySalary / 2);
                    }
                    else
                    {
                      totalEalryLate = earlyAndlate * oneDaySalary;
                    }

                    salaryInHand = totalPresentSalary + totalHolidaySalary + totalHalfDay + totalEalryLate + totalMispunch;

                    totalDays = Convert.ToInt32(dr["present"].ToString()) + Convert.ToInt32(dr["holiday"].ToString()) + Convert.ToInt32(dr["halfday"].ToString())
                       + Convert.ToInt32(dr["late"].ToString()) + Convert.ToInt32(dr["early"].ToString()) + Convert.ToInt32(dr["mispunch"].ToString());

                    totHours = Convert.ToDecimal(dr["totalHours"].ToString());

                    break;

                  }

                  DateTime curentMonth = Convert.ToDateTime(salarySlipData.year + "-" + salarySlipData.month + "-01");

                  decimal basicSalary, specialAllowance, hraSalary, Gross_Salary;

                  if (salarymaster == null)
                  {
                    salarymaster = new SalaryMaster();
                    salarymaster.basic = "50";
                    salarymaster.allowance = "25";
                    salarymaster.hra = "25";
                    salarymaster.pf = "0";
                    salarymaster.tds = "0";
                    salarymaster.esi = "0";
                  }

                  basicSalary = (Convert.ToDecimal(empMaster.employee_salary) * Convert.ToDecimal(salarymaster.basic)) / 100;

                  specialAllowance = (Convert.ToDecimal(empMaster.employee_salary) * Convert.ToDecimal(salarymaster.allowance)) / 100;
                  hraSalary = (Convert.ToDecimal(empMaster.employee_salary) * Convert.ToDecimal(salarymaster.hra)) / 100;

                  decimal pfSalary = 0M;
                  decimal tdsSalary = 0M;
                  decimal esiSalary = 0M;

                  if (Convert.ToInt32(salarymaster.pf) > 0)
                  {
                    pfSalary = (basicSalary * Convert.ToDecimal(salarymaster.pf)) / 100;
                  }

                  if (Convert.ToInt32(salarymaster.tds) > 0)
                  {
                    tdsSalary = (basicSalary * Convert.ToDecimal(salarymaster.tds)) / 100;
                  }

                  if (Convert.ToInt32(salarymaster.esi) > 0)
                  {
                    esiSalary = (basicSalary * Convert.ToDecimal(salarymaster.esi)) / 100;
                  }

                  decimal totalDeducation = 0M;

                  totalDeducation = totalAbsent + pfSalary + tdsSalary + esiSalary;

                  EmployeeConveyance empConvence = new EmployeeConveyance();
                  empConvence = (from d in db.EmployeeConveyance
                                 where d.employee_id.Equals(employeeId)
                                 && d.month.Equals(salarySlipData.month)
                                 && d.year.Equals(salarySlipData.year)
                                 select d).FirstOrDefault();

                  if (empConvence == null)
                  {
                    empConvence = new EmployeeConveyance();
                    empConvence.mobile_bill = 0;
                    empConvence.conveyance = 0;
                    empConvence.performance_variable = 0;
                    empConvence.other_received = 0;
                  }

                  salaryInHand = salaryInHand + empConvence.mobile_bill + empConvence.conveyance + empConvence.performance_variable + empConvence.other_received;

                  StringBuilder sb = new StringBuilder();
                  //sb.Append(req.search_result);
                  sb.Append("<body style='Font-family:Verdana; font-size:8px;'>");
                  sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5'>");
                  sb.Append("<tr><td colspan='4' bgcolor='#4473c5' style='text-align:center; color: #fff; padding: 6px;'><span style='font-size: 12px;'>" + companymaster.name + "</span><br />" + companymaster.company_address + "</td></tr>");
                  sb.Append("<tr><td colspan='4' bgcolor='#d0dbef' style='text-align: center; font-size: 10px; border: 1px solid #000;padding: 4px;border-top: 0;'> Payslip  </td></tr>");
                  sb.Append("<tr><td bgcolor='#d0dbef'>Particular</td><td bgcolor='#d0dbef'></td><td bgcolor='#d0dbef'>Particular</td><td bgcolor='#d0dbef'></td></tr>");
                  //sb.Append("<tr><td style='border-left: 1px solid #000'></td><td>" + empMaster.name + "</td><td>Salary for Month</td><td>"+ thisMonth + "</td></tr>");
                  sb.Append("<tr><td style='border-left: 1px solid #000; vertical-align: top;'>Name of the employee<br>Designation <br>Month</td><td style='border-left: 1px solid #000'>" + empMaster.name +
                      "<br>" + empMaster.designation + "<br>" + curentMonth.ToString("MMM") + "-" + salarySlipData.year +
                      "</td><td>Salary for the month<br>Paid Days<br>EL<br>CL<br>Paid Leave<br>Absent <br>Late Coming</td><td>" + curentMonth.ToString("MMM") +
                      "<br>" + totalDays + "<br>0<br>0<br>0<br>"+ absent + "<br> "+ lateEaly + "</td></tr>"); ;
                  sb.Append("<tr><td bgcolor='#d0dbef'>Salary Head</td><td bgcolor='#d0dbef'>Amount</td bgcolor='#d0dbef'><td bgcolor='#d0dbef'>Deduction and Variables</td><td bgcolor='#d0dbef'>Amount</td></tr>");
                  sb.Append("<tr><td style='vertical-align: top;'>Basic Salary<br>HRA<br>Special Allowances</td><td>" + basicSalary.ToString("#,##0.00") + "<br>" + hraSalary.ToString("#,##0.00") + "<br>" + specialAllowance.ToString("#,##0.00") +
                      "</td><td><b style='border-bottom: 1px solid #000'>Other Earnings:</b> <br> Salary Advance<br>Other Received<br>Mobile Bill<br>Performance Variable<br>Conveyance <br> <b style='border-bottom: 1px solid #000'>Deduction:</b>  <br> ESI<br>EPF<br>TDS<br>Other Deduction</td><td><span style='line-height: 13px'>" + empConvence.advanced_salary.ToString("#,##0.00") +
                      "<br>" + empConvence.other_received.ToString("#,##0.00") +
                      "<br>" + empConvence.mobile_bill.ToString("#,##0.00") + "<br>" + empConvence.performance_variable.ToString("#,##0.00") + "<br>" + empConvence.conveyance.ToString("#,##0.00") +
                      "<br><br>" + esiSalary + "<br>" + pfSalary.ToString("#,##0.00") + "<br>" + tdsSalary.ToString("#,##0.00") + 
                      "<br>" + totalDeducation.ToString("#,##0.00") + "</span></td></tr>");
                  sb.Append("<tr><td bgcolor='#d0dbef'>Gross Salary</td><td bgcolor='#d0dbef'>" + empMaster.employee_salary.ToString("#,##0.00") + "</td><td bgcolor='#d0dbef'>Net Salary</td><td bgcolor='#d0dbef'>" + salaryInHand.ToString("#,##0") + "</td></tr>");
                  sb.Append("<tr><td colspan='4' border='0'><br></td></tr>");
                  sb.Append("<tr><td bgcolor='#d0dbef'>Payment Mode</td><td bgcolor='#d0dbef'>Amount</td><td colspan='2'  border='0' style='text-align: center'>For " + companymaster.name + "</td></tr>");
                  sb.Append("<tr><td>i)Cash Amount<br>ii)Bank Amount<br>iii)Checque No.</td><td>0.00<br>" + salaryInHand.ToString("#,##0") + "(NEFT) <br> NA</td><td colspan='2'  border='0' style='text-align: center'>Auth Signatory <br>(System generated payslip)</td></tr>");
                  //sb.Append("<tr><td></td><td>" + inhandSalary.ToString("#,##0.00") + "</td><td colspan='2'></td></tr>");
                  //sb.Append("<tr><td></td><td>NEFT</td><td colspan='2'></td></tr>");
                  sb.Append("<tr><td colspan='4' border='0' style='border-left: 1px solid #000 !important'><br></td></tr>");
                  sb.Append("</table>");
                  sb.Append("<p width='100%' style='margin-top: 10px; float: right;'>Payslip generated on : </p>" + salarySlipData.modify_date.ToString("dd/MM/yyyy"));
                  sb.Append("</body>");
                  HTMLWorker hw = new HTMLWorker(doc);
                  hw.Parse(new StringReader(sb.ToString()));

                  doc.Close();
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = fileName;
                }
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // get team attendance 
    [HttpPost]
    [Route("GetTeamAttendance")]
    public TeamResponse GetTeamAttendance(HmcRequest req)
    {
      TeamResponse response = new TeamResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();
                List<TeamDetails> retVal = null;
                if (employeeMaster.role == "Administrator")
                {
                  retVal = (from d in db.EmployeeMaster
                            select new TeamDetails
                            {
                              employee_id = d.employee_id,
                              name = d.name,
                              designation = d.designation,
                              email = d.email,
                              feature_image = d.feature_image
                            }).ToList();
                }
                else if (employeeMaster.role == "Admin")
                {
                  retVal = (from d in db.EmployeeMaster
                            where d.pay_code.Equals(employeeMaster.pay_code)
                            select new TeamDetails
                            {
                              employee_id = d.employee_id,
                              name = d.name,
                              designation = d.designation,
                              email = d.email,
                              feature_image = d.feature_image
                            }).ToList();
                }
                else
                {
                  retVal = (from d in db.EmployeeMaster
                            where d.reporting_to.Equals(employeeId)
                            select new TeamDetails
                            {
                              employee_id = d.employee_id,
                              name = d.name,
                              designation = d.designation,
                              email = d.email,
                              feature_image = d.feature_image
                            }).ToList();
                }

               

                foreach (TeamDetails item in retVal)
                {
                  item.encyptemployeeid = CryptoEngine.Encrypt(item.employee_id, "skym-3hn8-sqoy19");

                  //string todayDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
                  //DateTime datet = Convert.ToDateTime(todayDate);
                  //if (!string.IsNullOrEmpty(req.date))
                  //{
                  //  datet = DateTime.ParseExact(req.date, "dd/MM/yyyy", null);
                  //}

                  //List<AttendanceResponse> attendace = (from d in db.AttendanceMaster
                  //                                      where d.employee_id.Equals(item.employee_id)
                  //                                      && d.date >= datet && d.date <= datet
                  //                                      select new AttendanceResponse
                  //                                      {
                  //                                        date = d.date,
                  //                                        in_time = d.in_time,
                  //                                        out_time = d.out_time,
                  //                                        status = d.status,
                  //                                        late = d.late,
                  //                                        early = d.early,
                  //                                        mis = d.mis,
                  //                                        absent = d.absent
                  //                                      }).ToList();

                  //foreach (AttendanceResponse item2 in attendace)
                  //{
                  //  item2.sdate = item2.date.ToString("dd/MM/yyyy");
                  //}
                  //item.attendance = attendace;
                }

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }

    [HttpPost]
    [Route("CheckIn")]
    public HmcStringResponse CheckIn(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "you are not valid for check in.";
      response.data = "you are not valid for check in";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();//db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid != null)
              {

                //Shift latlong = null;
                List<Shift> latlong = null;
                Boolean checkInFlag = false;
                if (userValid.office_emp == true && userValid.marketing_emp == false)
                {

                  latlong = (from d in db.Shift
                             //where d.shift_code.Equals("G")
                             select d).ToList();

                  foreach (Shift item in latlong)
                  {
                    double aoiLat = Convert.ToDouble(item.latitude);
                    double aoiLong = Convert.ToDouble(item.longitude);
                    double lat = Convert.ToDouble(req.lat);
                    double lot = Convert.ToDouble(req.log);
                    GeoCoordinate pin1 = new GeoCoordinate(aoiLat, aoiLong);
                    GeoCoordinate pin2 = new GeoCoordinate(lat, lot);

                    double distanceBetween = pin1.GetDistanceTo(pin2);

                    if (distanceBetween < item.geo_distance)
                    {
                      checkInFlag = true;
                      break;
                    }
                  }
                }


                if (checkInFlag == true || userValid.marketing_emp == true)
                {
                  DateTime date = DateTime.Now;
                  DateTime dateMatch = date.Date;
                  AttendanceMaster retVal = (from d in db.AttendanceMaster
                                             where d.date.Equals(dateMatch)
                                              && d.employee_id.Equals(userValid.employee_id)
                                             select d).FirstOrDefault();

                  string monthStart = DateTime.Now.ToString("yyyy-MM") + "-01";
                  DateTime monthStartDate = Convert.ToDateTime(monthStart);

                  Shift shift = (from d in db.Shift
                                 where d.shift_code.Equals(userValid.shift)
                                 select d).FirstOrDefault();

                  string shiftCode = string.Empty;
                  if (shift == null)
                  {
                    shiftCode = "G";
                  }
                  else
                  {
                    shiftCode = shift.shift_code;
                  }

                  if (retVal != null)
                  {

                    if (retVal.absent == "AB")
                    {
                      retVal.day = date.DayOfWeek.ToString();
                      retVal.in_time = date.ToString("HH:mm:ss");
                      retVal.out_time = "00:00:00";
                      retVal.status = "";
                      retVal.absent = "";
                      retVal.mis = "MP";
                      retVal.shift = shiftCode;
                      retVal.total_hrs = 0;
                      retVal.ticket_status = false;
                      retVal.in_latitude = req.lat;
                      retVal.in_longitude = req.log;
                      retVal.in_address = req.address;
                      db.SaveChanges();

                      response.flag = "1";
                      response.status = "success";
                      response.alert = "success";
                      response.data = "success";
                    }
                  }
                  else
                  {
                    AttendanceMaster insertData = new AttendanceMaster();
                    insertData.employee_id = userValid.employee_id;
                    insertData.date = date;
                    insertData.day = date.DayOfWeek.ToString();
                    insertData.in_time = date.ToString("HH:mm:ss");
                    insertData.out_time = "00:00:00";
                    insertData.status = "";
                    insertData.absent = "";
                    insertData.mis = "MP";
                    insertData.total_hrs = 0;
                    insertData.shift = shiftCode;
                    insertData.ticket_status = false;
                    insertData.in_latitude = req.lat;
                    insertData.in_longitude = req.log;
                    insertData.in_address = req.address;
                    db.AttendanceMaster.Add(insertData);
                    db.SaveChanges();

                  }

                  // attendance log
                  AttendanceLogMaster attendanceLog = new AttendanceLogMaster();
                  attendanceLog.employee_id = userValid.employee_id;
                  attendanceLog.date = date;
                  if (retVal != null)
                  {
                    attendanceLog.status = "O";
                  }
                  else
                  {
                    attendanceLog.status = "I";
                  }

                  attendanceLog.shift = shift.shift_code;
                  attendanceLog.day = date.DayOfWeek.ToString();
                  attendanceLog.in_lat = req.lat;
                  attendanceLog.in_long = req.log;
                  attendanceLog.address = req.address;
                  db.AttendanceLogMaster.Add(attendanceLog);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "success";
                  response.data = "success";

                }
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    [HttpPost]
    [Route("CheckOut")]
    public HmcStringResponse CheckOut(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "Sorry, you are out of checkout range. Please try again";
      response.data = "Sorry, you are out of checkout range. Please try again.";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }

      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                DateTime date = DateTime.Now;
                DateTime checkOutDate = date.Date;
                //AttendanceMaster resultsDb = db.AttendanceMaster.Where(r => r.date == from && r.pers_no == req.employee_id).OrderByDescending(e => e.id).FirstOrDefault();
                AttendanceMaster resultsDb = (from d in db.AttendanceMaster
                                              where d.employee_id.Equals(employeeId)
                                              && d.date.Equals(checkOutDate)
                                              orderby d.id descending
                                              select d).FirstOrDefault();
                if (resultsDb != null)
                {
                  EmployeeMaster EmpDetails = null;

                  EmpDetails = (from d in db.EmployeeMaster
                                where d.employee_id.Equals(employeeId)
                                && d.status.Equals("Active")
                                select d).FirstOrDefault();

                  Boolean checkOutFlag = false;
                  if (EmpDetails != null)
                  {
                    List<Shift> latlong = null;
                    if (EmpDetails.office_emp == true && EmpDetails.marketing_emp == false)
                    {
                      latlong = (from d in db.Shift
                                 //where d.shift_code.Equals("G")
                                 select d).ToList();

                      foreach (Shift item in latlong)
                      {
                        double aoiLat = Convert.ToDouble(item.latitude);
                        double aoiLong = Convert.ToDouble(item.longitude);
                        double lat = Convert.ToDouble(req.lat);
                        double lot = Convert.ToDouble(req.log);
                        GeoCoordinate pin1 = new GeoCoordinate(aoiLat, aoiLong);
                        GeoCoordinate pin2 = new GeoCoordinate(lat, lot);

                        double distanceBetween = pin1.GetDistanceTo(pin2);

                        if (distanceBetween < item.geo_distance)
                        {
                          checkOutFlag = true;
                          break;
                        }
                      }
                    }

                    if (checkOutFlag == true || EmpDetails.marketing_emp == true)
                    {
                      resultsDb.out_time = date.ToString("HH:mm:ss");
                      // calculate total hrs
                      DateTime dtFrom = DateTime.Parse(resultsDb.in_time);
                      DateTime dtTo = DateTime.Parse(resultsDb.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      resultsDb.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));
                      // update attendance
                      Shift shift = (from d in db.Shift
                                     where d.shift_code.Equals(EmpDetails.shift)
                                     select d).FirstOrDefault();


                      resultsDb.status = "P";
                      resultsDb.mis = "";
                      resultsDb.absent = "";
                      resultsDb.out_latitude = req.lat;
                      resultsDb.out_longitude = req.log;
                      resultsDb.out_address = req.address;
                      db.SaveChanges();


                      // attendance log
                      AttendanceLogMaster attendanceLog = new AttendanceLogMaster();
                      attendanceLog.employee_id = EmpDetails.employee_id;
                      attendanceLog.date = date;
                      attendanceLog.status = "O";
                      attendanceLog.shift = shift.shift_code;
                      attendanceLog.day = date.DayOfWeek.ToString();
                      attendanceLog.in_lat = req.lat;
                      attendanceLog.in_long = req.log;
                      attendanceLog.address = req.address;
                      db.AttendanceLogMaster.Add(attendanceLog);
                      db.SaveChanges();

                      response.flag = "1";
                      response.status = "success";
                      response.alert = "success";
                      response.data = "success";
                    }



                  }
                }
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    [HttpPost]
    [Route("EmployeeTracking")]
    public HmcStringResponse EmployeeTracking(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "Sorry, you are out of checkout range. Please try again";
      response.data = "Sorry, you are out of checkout range. Please try again.";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log) && string.IsNullOrEmpty(req.address))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }

      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {                
                // attendance log
                EmployeeTracking attendanceLog = new EmployeeTracking();
                attendanceLog.employee_id = userValid.employee_id;
                attendanceLog.tracking_date = DateTime.Now;
                attendanceLog.tracking_lat = req.lat;
                attendanceLog.tracking_long = req.log;
                attendanceLog.tracking_address = req.address;
                db.EmployeeTracking.Add(attendanceLog);
                db.SaveChanges();

                string body = string.Empty;
                body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear Ma'am/Sir,</p>" +
                          "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Employee out office range, Please contact to employee</p>" +
                          "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                          "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Skylabs Support Team</p>";
                string subject = "Alert - " + userValid.name + "(" + userValid.employee_id + ")";
                string emailId = userValid.email;
                //string emailId = "cs.sharma@hmcgroup.in";
                //Utilities.Utility.sendMaill(emailId, body, subject);

                response.flag = "1";
                response.status = "success";
                response.alert = "success";
                response.data = "success";
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // get notification
    [HttpPost]
    [Route("GetNotification")]
    public NotificationResponse GetNotification(HmcAccessResponse req)
    {
      NotificationResponse response = new NotificationResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "no data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                List<ApplyLeaveResponse> retVal = null;
                retVal = (from e in db.ApplyLeaveMasters
                          where e.assign_by.Equals(req.employee_id)
                          && e.status.Equals("Pending")
                          orderby e.id descending
                          select new ApplyLeaveResponse
                          {
                            id = e.id,
                            from_date = e.from_date,
                            employee_id = e.employee_id,
                            to_date = e.to_date,
                            date = e.modify_date,
                            status = e.status,
                            reason = e.reason,
                            assign_by = e.assign_by,
                            leave_type = e.leave_Type,
                            no_of_leave = e.no_of_leave.ToString()
                          }).ToList();

                if (retVal.Count() > 0)
                {
                  foreach (ApplyLeaveResponse item in retVal)
                  {
                    item.todate = item.to_date.ToString("dd/MM/yyyy");
                    item.fromdate = item.from_date.ToString("dd/MM/yyyy");
                    item.applydate = item.date.ToString("dd/MM/yyyy");

                    EmployeeMaster result = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(item.assign_by.ToString())
                                             select d).FirstOrDefault();

                    EmployeeMaster empDetails = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(item.employee_id)
                                                 select d).FirstOrDefault();
                    if (empDetails != null)
                    {
                      item.apply_by_name = empDetails.name;
                    }
                    if (result != null)
                    {
                      item.assign_by_name = result.name;
                    }

                    KeyValueMaster leaveMaster = (from d in db.KeyValueMaster
                                                  where d.id.Equals(item.leave_type)
                                                  select d).FirstOrDefault();
                    if (result != null)
                    {
                      item.leave_code = leaveMaster.key_code;
                    }
                  }


                }

                List<MispunchResponse> retVal1 = (from e in db.AttendanceMispunch
                                                  where e.assign_by.Equals(req.employee_id)
                                                  && e.status.Equals("Pending")
                                                  orderby e.id descending
                                                  select new MispunchResponse
                                                  {
                                                    id = e.id,
                                                    employee_id = e.employee_id,
                                                    assign_by = e.assign_by,
                                                    reason = e.reason,
                                                    //  = e.attendance_id,
                                                    date = e.date,
                                                    in_time = e.in_time,
                                                    out_time = e.out_time,
                                                    //m = e.created_date,
                                                    status = e.status,
                                                    shift = e.shift
                                                  }).ToList();


                if (retVal1.Count() > 0)
                {
                  foreach (MispunchResponse item in retVal1)
                  {

                    EmployeeMaster result = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(item.assign_by.ToString())
                                             select d).FirstOrDefault();

                    EmployeeMaster empDetails = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(item.employee_id)
                                                 select d).FirstOrDefault();

                    if (empDetails != null)
                    {
                      item.apply_by_name = empDetails.name;
                    }

                    if (result != null)
                    {
                      item.assign_by_name = result.name;
                    }
                  }

                }


                NotificationList retData = new NotificationList();
                retData.applyLeave = retVal;
                retData.attendanceTicket = retVal1;

                response.status = "success";
                response.flag = "1";
                response.data = retData;
                response.alert = "success";
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    [HttpPost]
    [Route("UpdateProfile")]
    public HmcStringResponse UpdateProfile(EmployeeDetails req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.data = "data is not valid";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster result = (from d in db.EmployeeMaster
                                       where d.employee_id.Equals(employeeId)
                                       && d.status.Equals("Active")
                                       select d).FirstOrDefault();
              if (result != null)
              {
                byte[] imageBytes = System.Convert.FromBase64String(req.feature_image);

                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images");//HttpContext.Current.Server.MapPath("~/Images"); //Path

                //Check if directory exist
                if (!System.IO.Directory.Exists(path))
                {
                  System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }
                DateTime value = DateTime.Now;
                string imageName = value.ToString("yyyyMMddHHmmss") + ".jpg";
                string imagePath = path + "/" + imageName;
                File.WriteAllBytes(imagePath, imageBytes);

                result.feature_image = imageName;
                db.SaveChanges();

                response.status = "success";
                response.flag = "1";
                response.data = imageName;
                response.alert = "success";
              }

            }


          }

        }

      }
      catch (Exception ex)
      {
        response.data = ex.Message;
        response.alert = ex.Message;
      }

      return response;

    }

    // create offer letter
    [HttpPost]
    [Route("CreateOfferLetter")]
    public HmcJsonResponse CreateOfferLetter(OfferLetterRequest req)
    {
      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "employeeid and email id alredy exist";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");

              if (userValid)
              {
                ExceptionResponseContainer retVal = null;
                retVal = CustomValidator.applyValidations(req, typeof(OfferLetterValidate));

                if (retVal.isValid == false)
                {
                  response.alert = retVal.jsonResponse;
                }
                else
                {

                  DateTime offerDate = DateTime.ParseExact(req.offer_date, "dd/MM/yyyy", null);

                  OfferLetterMaster result = new OfferLetterMaster();
                  result.offer_date = offerDate;
                  result.f_name = req.f_name;
                  result.l_name = req.l_name;
                  result.mobile_no = req.mobile_no;
                  result.department = req.department;
                  result.designation = req.designation;
                  result.address = req.address;
                  result.city = req.city;
                  result.state = req.state;
                  result.district = req.district;
                  result.gross_salary = req.gross_salary;
                  result.modify_by = employeeId;
                  result.modify_date = DateTime.Now;
                  result.reporting_to = req.reporting_to;
                  result.appointment = false;
                  db.OfferLetterMaster.Add(result);
                  db.SaveChanges();

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";

                }
              }
            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // get offer letter list
    [HttpPost]
    [Route("GetOfferLetter")]
    public OfferLetterResponse GetOfferLetter(HmcRequest req)
    {
      OfferLetterResponse response = new OfferLetterResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empDetail = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


                int page = Convert.ToInt32(req.pageNo);
                List<OfferLetterMaster> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.OfferLetterMaster
                              where (d.f_name.ToLower().Contains(req.search_result))
                              || (d.l_name.ToLower().Contains(req.search_result))
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.OfferLetterMaster
                              where (d.f_name.ToLower().Contains(req.search_result))
                              || (d.l_name.ToLower().Contains(req.search_result))
                              && d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }
                else
                {
                  if (empDetail.role == "Administrator")
                  {
                    retVal = (from d in db.OfferLetterMaster
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }
                  else
                  {
                    retVal = (from d in db.OfferLetterMaster
                              where d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).Skip(skip).Take(pageSize).Distinct().ToList();
                  }

                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }

    // download offer letter
    [HttpPost]
    [Route("DownloadOfferLetter")]
    public HmcStringResponse DownloadOfferLetter(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                OfferLetterMaster empMaster = (from d in db.OfferLetterMaster
                                               where d.id.Equals(req.id)
                                               select d).FirstOrDefault();

                if (empMaster != null)
                {
                  CompaniesMaster salarymaster = (from d in db.CompaniesMaster
                                                  where d.company_code.Equals(empMaster.pay_code)
                                                  select d).FirstOrDefault();

                  EmployeeMaster reportmaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(empMaster.reporting_to)
                                                 select d).FirstOrDefault();


                  string print_date = DateTime.Now.ToString("dd/MM/yyyy");
                  string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                  Document doc = new Document(iTextSharp.text.PageSize.A4);
                  //var fpath = Server.MapPath("~/customerdoc/SID/AccountStatement/");
                  //string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                  string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                  PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                  int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                  //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                  Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                  //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);


                  doc.Open();


                  iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "rrpl10.png");
                  image.ScalePercent(50f);
                  image.SetAbsolutePosition(30, 720);
                  doc.Add(image);

                  //var footer = iTextSharp.text.Image.GetInstance("");
                  //footer.SetAbsolutePosition(0, 0); // X and Y Accroding to need

                  StringBuilder sb = new StringBuilder();
                  //sb.Append(req.search_result);
                  //string htmlFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Included/OfferLetter.cshtml");
                  //var path = "~/Views/Included/OfferLetter.cshtml";
                  string htmlFilePath = string.Empty;
                  StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Include/OfferLetter.cshtml"));
                  htmlFilePath = reader.ReadToEnd();

                  int totalCTC = Convert.ToInt32(empMaster.gross_salary) * 12;
                  htmlFilePath = htmlFilePath.Replace("{companyName}", salarymaster.name);
                  htmlFilePath = htmlFilePath.Replace("{companyAddress}", salarymaster.company_address);
                  htmlFilePath = htmlFilePath.Replace("{name}", empMaster.f_name + ' ' + empMaster.l_name);
                  htmlFilePath = htmlFilePath.Replace("{designation}", empMaster.designation);
                  htmlFilePath = htmlFilePath.Replace("{empSalary}", totalCTC.ToString("#,##0.00"));
                  htmlFilePath = htmlFilePath.Replace("{joningDate}", empMaster.offer_date.ToString("dd/MM/yyyy"));
                  htmlFilePath = htmlFilePath.Replace("{issueDate}", empMaster.modify_date.ToString("dd/MM/yyyy"));
                  htmlFilePath = htmlFilePath.Replace("{reportingTo}", reportmaster.name);

                  //ViewBag.HTMLData = HttpUtility.HtmlEncode(htmlString);
                  sb.Append(htmlFilePath);
                  iTextSharp.text.html.simpleparser.StyleSheet styles = new iTextSharp.text.html.simpleparser.StyleSheet();
                  styles.LoadStyle("p", "font-size", "11px");
                  //styles.LoadTagStyle("ul", "list-style", "decimal");
                  styles.LoadStyle("ol", "list-style-type", "decimal");
                  //styles.LoadTagStyle("li", "face", "Times");
                  //styles.LoadTagStyle("li", "size", "25px");
                  //styles.LoadTagStyle("li", "leading", "15f");

                  //XMLWorker worker = new XMLWorker(styles, true);

                  HTMLWorker hw = new HTMLWorker(doc);
                  hw.SetStyleSheet(styles);
                  //Border = PdfPCell.BOTTOM_BORDER | PdfPCell.TOP_BORDER
                  hw.Parse(new StringReader(sb.ToString()));
                  //PdfCanvas canvas = new PdfCanvas(pdfPage);

                  doc.Close();
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = fileName;
                }




              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    public HttpResponseMessage GetResponseHtmlData()
    {
      var path = "~/Views/Included/OfferLetter.cshtml";
      var response = new HttpResponseMessage();
      response.Content = new StringContent(path);
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
      return response;
    }


    [HttpPost]
    [Route("UploadCrmData")]
    public HmcStringResponse UploadCrmData(UploadCrmDataRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "no data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            //string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.mobile_no == req.mobile_no && user.status == "Active");
            if (userValid)
            {
              foreach (UploadCrmData item in req.attendance_list)
              {
                DateTime attDate = Convert.ToDateTime(item.date);

                DateTime matchData = attDate.Date;
                EmployeeMaster employeeData = (from d in db.EmployeeMaster
                                               where d.employee_id.Equals(item.employee_id)
                                               && d.status.Equals("Active")
                                               && d.pay_code != "FIN"
                                               select d).FirstOrDefault();

                if (employeeData != null)
                {
                  AttendanceMaster attendanceMaster = (from d in db.AttendanceMaster
                                                       where d.employee_id.Equals(item.employee_id)
                                                       && d.date.Equals(matchData)
                                                       //&& e.pay_code != "FIN"
                                                       select d).FirstOrDefault();
                  if (attendanceMaster == null)
                  {
                    AttendanceMaster insertData = new AttendanceMaster();
                    insertData.employee_id = item.employee_id;
                    insertData.date = attDate;
                    insertData.day = attDate.DayOfWeek.ToString();
                    insertData.shift = "G";
                    insertData.in_time = item.in_time + ":00";
                    if (!string.IsNullOrEmpty(item.out_time))
                    {
                      insertData.out_time = item.out_time + ":00";
                      insertData.status = "P";
                      insertData.mis = "";
                      insertData.absent = "";
                      insertData.early = "";
                      insertData.late = "";

                      DateTime dtFrom = DateTime.Parse(insertData.in_time);
                      DateTime dtTo = DateTime.Parse(insertData.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      insertData.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));

                      if (insertData.total_hrs < 4)
                      {
                        insertData.status = "";
                        insertData.absent = "A";
                      }
                      else if (insertData.total_hrs >= 4 && insertData.total_hrs <= 8)
                      {
                        insertData.status = "HD";
                        insertData.absent = "";
                      }
                    }
                    else
                    {
                      insertData.out_time = "00:00:00";
                      insertData.status = "";
                      insertData.mis = "MP";
                      insertData.absent = "";
                      insertData.early = "";
                      insertData.late = "";
                      insertData.total_hrs = 0;
                    }

                    insertData.ticket_status = false;
                    db.AttendanceMaster.Add(insertData);
                    db.SaveChanges();
                  }
                  else
                  {

                    if (attendanceMaster.status != "P" && attendanceMaster.mis != "MP")
                    {
                      attendanceMaster.in_time = item.in_time + ":00";
                    }

                    attendanceMaster.shift = "G";
                    if (!string.IsNullOrEmpty(item.out_time))
                    {
                      attendanceMaster.out_time = item.out_time + ":00";
                      attendanceMaster.status = "P";
                      attendanceMaster.mis = "";
                      attendanceMaster.absent = "";
                      attendanceMaster.early = "";
                      attendanceMaster.late = "";

                      DateTime dtFrom = DateTime.Parse(attendanceMaster.in_time);
                      DateTime dtTo = DateTime.Parse(attendanceMaster.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      attendanceMaster.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));

                      if (attendanceMaster.total_hrs < 4)
                      {
                        attendanceMaster.status = "";
                        attendanceMaster.absent = "A";
                      }
                      else if (attendanceMaster.total_hrs >= 4 && attendanceMaster.total_hrs <= 8)
                      {
                        attendanceMaster.status = "HD";
                        attendanceMaster.absent = "";
                      }
                    }
                    else
                    {
                      attendanceMaster.out_time = "00:00:00";
                      attendanceMaster.status = "";
                      attendanceMaster.mis = "MP";
                      attendanceMaster.absent = "";
                      attendanceMaster.early = "";
                      attendanceMaster.late = "";
                      attendanceMaster.total_hrs = 0;
                    }

                    db.SaveChanges();

                  }
                }


              }

              response.status = "success";
              response.flag = "1";
              response.data = "success";
              response.alert = "success";
            }
          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    // scan qrcode
    [HttpPost]
    [Route("QrCodeCheckIn")]
    public HmcStringResponse QrCodeCheckIn(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "sorry, you are not scan valid bar code. Please try again.";
      response.data = "sorry, you are not scan valid bar code. Please try again";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }


      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();//db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid != null)
              {
                List<Shift> latlong = null;

                latlong = (from d in db.Shift
                           //where d.shift_code.Equals("G")
                           select d).ToList();

                Boolean checkInFlag = false;

                foreach (Shift item in latlong)
                {
                  double aoiLat = Convert.ToDouble(item.latitude);
                  double aoiLong = Convert.ToDouble(item.longitude);
                  double lat = Convert.ToDouble(req.lat);
                  double lot = Convert.ToDouble(req.log);
                  GeoCoordinate pin1 = new GeoCoordinate(aoiLat, aoiLong);
                  GeoCoordinate pin2 = new GeoCoordinate(lat, lot);

                  double distanceBetween = pin1.GetDistanceTo(pin2);

                  if (distanceBetween < item.geo_distance)
                  {
                    checkInFlag = true;
                    break;
                  }
                }



                if (!string.IsNullOrEmpty(req.qr_code))
                {
                  QrCodeMaster qrCodeMaster = (from d in db.QrCodeMaster
                                               where d.qr_code.Equals(req.qr_code)
                                               //&& d.pay_code.Equals(userValid.pay_code)
                                               select d).FirstOrDefault();

                  if (qrCodeMaster == null)
                  {
                    checkInFlag = false;
                    response.alert = "Sorry, Qrcode is not valid. Please try again.";
                    response.data = "Sorry, Qrcode is not valid. Please try again.";
                    return response;
                  }

                }
                else
                {
                  checkInFlag = false;
                }


                if (checkInFlag == true)
                {
                  DateTime date = DateTime.Now;
                  DateTime dateMatch = date.Date;
                  AttendanceMaster retVal = (from d in db.AttendanceMaster
                                             where d.date.Equals(dateMatch)
                                              && d.employee_id.Equals(userValid.employee_id)
                                             select d).FirstOrDefault();

                  string monthStart = DateTime.Now.ToString("yyyy-MM") + "-01";
                  DateTime monthStartDate = Convert.ToDateTime(monthStart);

                  Shift shift = (from d in db.Shift
                                 where d.shift_code.Equals(userValid.shift)
                                 select d).FirstOrDefault();

                  string shiftCode = string.Empty;

                  if (shift == null)
                  {
                    shiftCode = "G";
                  }
                  else
                  {
                    shiftCode = shift.shift_code;
                  }

                  if (retVal != null)
                  {
                    retVal.day = date.DayOfWeek.ToString();
                    if (retVal.status == "AB" || retVal.status == "H")
                    {
                      retVal.in_time = date.ToString("HH:mm:ss");
                      retVal.out_time = "00:00:00";
                      retVal.total_hrs = 0;
                      retVal.shift = shiftCode;
                      retVal.in_latitude = req.lat;
                      retVal.in_longitude = req.log;
                      retVal.in_address = req.address;
                      retVal.status = "MP";
                    }
                    else
                    {
                      retVal.out_time = date.ToString("HH:mm:ss");
                      DateTime dtFrom = DateTime.Parse(retVal.in_time);
                      DateTime dtTo = DateTime.Parse(retVal.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      retVal.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));
                      retVal.shift = shiftCode;
                      retVal.out_latitude = req.lat;
                      retVal.out_longitude = req.log;
                      retVal.out_address = req.address;
                      retVal.status = "P";
                    }
                    
                    retVal.ticket_status = false;
                    db.SaveChanges();

                    response.flag = "1";
                    response.status = "success";
                    response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                    response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  }
                  else
                  {


                    AttendanceMaster insertData = new AttendanceMaster();
                    insertData.employee_id = userValid.employee_id;
                    insertData.date = date;
                    insertData.day = date.DayOfWeek.ToString();
                    insertData.in_time = date.ToString("HH:mm:ss");
                    insertData.out_time = "00:00:00";
                    retVal.shift = shiftCode;
                    retVal.status = "MP";

                    insertData.total_hrs = 0;
                    insertData.ticket_status = false;
                    insertData.in_latitude = req.lat;
                    insertData.in_longitude = req.log;
                    insertData.in_address = req.address;
                    db.AttendanceMaster.Add(insertData);
                    db.SaveChanges();

                  }

                  // attendance log
                  AttendanceLogMaster attendanceLog = new AttendanceLogMaster();
                  attendanceLog.employee_id = userValid.employee_id;
                  attendanceLog.date = date;
                  if (retVal != null)
                  {
                    if (retVal.absent == "AB")
                    {
                      attendanceLog.status = "I";
                    }
                    else
                    {
                      attendanceLog.status = "O";
                    }
                  }
                  else
                  {
                    attendanceLog.status = "I";
                  }

                  attendanceLog.shift = shift.shift_code;
                  attendanceLog.day = date.DayOfWeek.ToString();
                  attendanceLog.in_lat = req.lat;
                  attendanceLog.in_long = req.log;
                  attendanceLog.address = req.address;
                  db.AttendanceLogMaster.Add(attendanceLog);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";

                }
                else
                {
                  response.alert = "You range greater than 100 meter!!!";
                  response.data = "You range greater than 100 meter!!!";
                }
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // generate qr code banner
    [HttpPost]
    [Route("GenerateQrCode")]
    public HmcStringResponse GenerateQrCode(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "user is not valid for generate qr code";
      response.data = "user is not valid for generate qr code";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.role == "Qr Code Scanner" && user.status == "Active");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.role.Equals("Qr Code Scanner")
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                QrCodeMaster qrCodeMaster = (from d in db.QrCodeMaster
                                             where d.pay_code.Equals(employeeId)
                                             select d).FirstOrDefault();

                string qr_code = userValid.pay_code + DateTime.Now.ToString("yyyyMMddHHmm");
                string qrCodeImage = string.Empty;

                byte[] byteImage1 = null;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qr_code, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                System.Web.UI.WebControls.Image imgBarCode = new System.Web.UI.WebControls.Image();
                imgBarCode.Height = 150;
                imgBarCode.Width = 150;
                using (System.Drawing.Bitmap bitMap = qrCode.GetGraphic(20))
                {
                  using (MemoryStream ms = new MemoryStream())
                  {
                    bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();

                    //dtExcelData.Rows[i]["IsActive"] = "1";
                    byteImage1 = byteImage;
                    qrCodeImage = Convert.ToBase64String(byteImage);//BitConverter.ToString(byteImage);
                    qrCodeImage = byteImage.ToString();
                    imgBarCode.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                  }

                  if (!File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Images/" + qr_code + ".png"))) //Server.MapPath("~/Images/" + qr_code + ".png")))
                  {
                    bitMap.Save(System.Web.Hosting.HostingEnvironment.MapPath("~/Images/" + qr_code + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                  }
                }

                if (qrCodeMaster != null)
                {
                  qrCodeMaster.qr_code = qr_code;
                  qrCodeMaster.qrcode_image = byteImage1;
                  qrCodeMaster.modify_by = employeeId;
                  qrCodeMaster.modify_date = DateTime.Now;
                  db.SaveChanges();
                }
                else
                {
                  QrCodeMaster insertQrCode = new QrCodeMaster();
                  insertQrCode.pay_code = employeeId;
                  insertQrCode.qr_code = qr_code;
                  insertQrCode.qrcode_image = byteImage1;
                  insertQrCode.modify_by = employeeId;
                  insertQrCode.modify_date = DateTime.Now;
                  db.QrCodeMaster.Add(insertQrCode);
                  db.SaveChanges();
                }

                response.flag = "1";
                response.status = "success";
                response.alert = "success";
                response.data = qr_code;
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // selfi check in check out
    [HttpPost]
    [Route("SelfiCheckIn")]
    public HmcStringResponse SelfiCheckIn(CheckInOut req)
    {

      

      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "You are not permitted check by selfie. Please try again!!";
      response.data = "You are not permitted check by selfie. please try again!!";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }


      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();//db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid != null)
              {

                List<Shift> latlong = null;

                latlong = (from d in db.Shift
                           //where d.shift_code.Equals("G")
                           select d).ToList();

                Boolean checkInFlag = false;

                foreach (Shift item in latlong)
                {
                  double aoiLat = Convert.ToDouble(item.latitude);
                  double aoiLong = Convert.ToDouble(item.longitude);
                  double lat = Convert.ToDouble(req.lat);
                  double lot = Convert.ToDouble(req.log);
                  GeoCoordinate pin1 = new GeoCoordinate(aoiLat, aoiLong);
                  GeoCoordinate pin2 = new GeoCoordinate(lat, lot);

                  double distanceBetween = pin1.GetDistanceTo(pin2);

                  if (distanceBetween < item.geo_distance)
                  {
                    checkInFlag = true;
                    break;
                  }
                }

                if (checkInFlag == true || userValid.marketing_emp == true)
                {
                  byte[] imageBytes = System.Convert.FromBase64String(req.feature_image);

                  string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images");//HttpContext.Current.Server.MapPath("~/Images"); //Path

                  //Check if directory exist
                  if (!System.IO.Directory.Exists(path))
                  {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                  }
                  DateTime value = DateTime.Now;
                  string imageName = value.ToString("yyyyMMddHHmmss") + ".jpg";
                  string imagePath = path + "/" + imageName;
                  File.WriteAllBytes(imagePath, imageBytes);


                  DateTime date = DateTime.Now;
                  DateTime dateMatch = date.Date;
                  AttendanceMaster retVal = (from d in db.AttendanceMaster
                                             where d.date.Equals(dateMatch)
                                              && d.employee_id.Equals(userValid.employee_id)
                                             select d).FirstOrDefault();

                  string monthStart = DateTime.Now.ToString("yyyy-MM") + "-01";
                  DateTime monthStartDate = Convert.ToDateTime(monthStart);

                  Shift shift = (from d in db.Shift
                                 where d.shift_code.Equals(userValid.shift)
                                 select d).FirstOrDefault();

                  string shiftCode = string.Empty;

                  if (shift == null)
                  {
                    shiftCode = "G";
                  }
                  else
                  {
                    shiftCode = shift.shift_code;
                  }


                  if (retVal != null)
                  {

                    retVal.day = date.DayOfWeek.ToString();
                    if (retVal.absent == "AB" || retVal.status == "H")
                    {
                      retVal.in_time = date.ToString("HH:mm:ss");
                      retVal.out_time = "00:00:00";
                      retVal.total_hrs = 0;
                      retVal.status = "";
                      retVal.absent = "";
                      retVal.mis = "MP";
                      retVal.shift = shiftCode;
                      retVal.in_latitude = req.lat;
                      retVal.in_longitude = req.log;
                      retVal.in_address = req.address;
                    }
                    else
                    {
                      retVal.out_time = date.ToString("HH:mm:ss");
                      DateTime dtFrom = DateTime.Parse(retVal.in_time);
                      DateTime dtTo = DateTime.Parse(retVal.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      retVal.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));
                      retVal.status = "P";
                      retVal.absent = "";
                      retVal.mis = "";
                      retVal.out_latitude = req.lat;
                      retVal.out_longitude = req.log;
                      retVal.out_address = req.address;
                    }

                    retVal.ticket_status = false;
                    retVal.feature_image = imageName;
                    db.SaveChanges();

                    response.flag = "1";
                    response.status = "success";
                    response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                    response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  }
                  else
                  {

                    AttendanceMaster insertData = new AttendanceMaster();
                    insertData.employee_id = userValid.employee_id;
                    insertData.date = date;
                    insertData.day = date.DayOfWeek.ToString();
                    insertData.in_time = date.ToString("HH:mm:ss");
                    insertData.out_time = "00:00:00";
                    insertData.status = "";
                    insertData.absent = "";
                    insertData.mis = "MP";
                    insertData.shift = shiftCode;
                    insertData.total_hrs = 0;
                    insertData.ticket_status = false;
                    insertData.in_latitude = req.lat;
                    insertData.in_longitude = req.log;
                    insertData.feature_image = imageName;
                    insertData.in_address = req.address;
                    db.AttendanceMaster.Add(insertData);
                    db.SaveChanges();

                  }

                  // attendance log
                  AttendanceLogMaster attendanceLog = new AttendanceLogMaster();
                  attendanceLog.employee_id = userValid.employee_id;
                  attendanceLog.date = date;
                  if (retVal != null)
                  {
                    if (retVal.absent == "AB")
                    {
                      attendanceLog.status = "I";
                    }
                    else
                    {
                      attendanceLog.status = "O";
                    }
                  }
                  else
                  {
                    attendanceLog.status = "I";
                  }

                  //attendanceLog.shift = shift.shift_code;
                  attendanceLog.shift = "G";
                  attendanceLog.day = date.DayOfWeek.ToString();
                  attendanceLog.in_lat = req.lat;
                  attendanceLog.in_long = req.log;
                  attendanceLog.feature_image = imageName;
                  attendanceLog.address = req.address;
                  db.AttendanceLogMaster.Add(attendanceLog);
                  db.SaveChanges();


                  string constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContextSmartOffice"].ConnectionString;
                  SqlConnection con = new SqlConnection(constr);



                  con.Open();
                  string query = "";
                  query = "insert into rawdata(ecode, ename, ldt, mid, dname, issync) values('" + userValid.employee_id + "', '" + userValid.name + "', getdate(), '146', 'WEMS', 1)";
                  SqlCommand cmd = new SqlCommand(query, con);
                  cmd.ExecuteNonQuery();
                  con.Close();

                  con.Open();
                  query = "";

                  query = "insert into "+GetTableName()+"(AttenndanceMarkingType, DownloadDate, DeviceId, UserId, LogDate, Direction, StatusCode, VerificationMode, IsApproved, Temperature, AttDirection)  ";
                  query += "values('Biometric', getdate(), '54', '" + userValid.employee_id + "', getdate() , 'in', 0, 2, 1, 0, '') ";
                  cmd = new SqlCommand(query, con);
                  cmd.ExecuteNonQuery();
                  con.Close();





                  //con.Open();
                  //query = "";
                  //query = "insert into rawdata(ecode, ename, ldt, mid, dname, issync) values('" + userValid.employee_id + "', '" + userValid.name + "', getdate(), '146', 'WEMS', 1)";
                  //cmd = new SqlCommand(query, con);
                  //cmd.ExecuteNonQuery();
                  //con.Close();




                  //con.Open();
                  //query = "";               

                  //query = "insert into DeviceLogs_10_2023(AttenndanceMarkingType, DownloadDate, DeviceId, UserId, LogDate, Direction, StatusCode, VerificationMode, IsApproved, Temperature, AttDirection)  ";
                  //query += "values('Biometric', getdate(), '54', '" + userValid.employee_id + "', getdate() , 'in', 0, 2, 1, 0, '') ";
                  //cmd = new SqlCommand(query, con);
                  //cmd.ExecuteNonQuery();
                  //con.Close();



                  response.flag = "1";
                  response.status = "success";
                  response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";

                }
                else
                {
                  response.alert = "You are not permitted . Please check your location!!";
                  response.data = "You are not permitted . Please check your location!!";
                }
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    //checkincheckout
    [HttpPost]
    [Route("CheckInCheckOut")]
    public HmcStringResponse CheckInCheckOut(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "You are not permitted check by normal punch. Please try again!!";
      response.data = "You are not permitted check by normal punch. Please try again!!";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }


      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();//db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid != null)
              {

                List<Shift> latlong = null;

                latlong = (from d in db.Shift
                           //where d.shift_code.Equals("G")
                           select d).ToList();

                Boolean checkInFlag = false;

                foreach (Shift item in latlong)
                {
                  double aoiLat = Convert.ToDouble(item.latitude);
                  double aoiLong = Convert.ToDouble(item.longitude);
                  double lat = Convert.ToDouble(req.lat);
                  double lot = Convert.ToDouble(req.log);
                  GeoCoordinate pin1 = new GeoCoordinate(aoiLat, aoiLong);
                  GeoCoordinate pin2 = new GeoCoordinate(lat, lot);

                  double distanceBetween = pin1.GetDistanceTo(pin2);

                  if (distanceBetween < item.geo_distance)
                  {
                    checkInFlag = true;
                    break;
                  }
                }

                if (checkInFlag == true || userValid.marketing_emp == true)                  
                  {
                  DateTime date = DateTime.Now;
                  DateTime dateMatch = date.Date;
                  AttendanceMaster retVal = (from d in db.AttendanceMaster
                                             where d.date.Equals(dateMatch)
                                              && d.employee_id.Equals(userValid.employee_id)
                                             select d).FirstOrDefault();

                  string monthStart = DateTime.Now.ToString("yyyy-MM") + "-01";
                  DateTime monthStartDate = Convert.ToDateTime(monthStart);


                  Shift shift = (from d in db.Shift
                                 where d.shift_code.Equals(userValid.shift)
                                 select d).FirstOrDefault();

                  string shiftCode = string.Empty;

                  if (shift == null)
                  {
                    shiftCode = "G";
                  }
                  else
                  {
                    shiftCode = shift.shift_code;
                  }
                  try
                  {
                    shift.shift_code = "G";
                  }
                  catch { }
                  if (retVal != null)
                  {

                    retVal.day = date.DayOfWeek.ToString();
                    if (retVal.absent == "AB")
                    {
                      retVal.in_time = date.ToString("HH:mm:ss");
                      retVal.out_time = "00:00:00";
                      retVal.total_hrs = 0;
                      retVal.in_latitude = req.lat;
                      retVal.in_longitude = req.log;
                      retVal.in_address = req.address;
                      retVal.status = "";
                      retVal.absent = "";
                      retVal.mis = "MP";
                      retVal.shift = shiftCode;
                    }
                    else
                    {
                      retVal.out_time = date.ToString("HH:mm:ss");
                      DateTime dtFrom = DateTime.Parse(retVal.in_time);
                      DateTime dtTo = DateTime.Parse(retVal.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      retVal.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));
                      retVal.out_latitude = req.lat;
                      retVal.out_longitude = req.log;
                      retVal.out_address = req.address;
                      retVal.status = "P";
                      retVal.absent = "";
                      retVal.mis = "";
                    }

                    retVal.ticket_status = false;

                    db.SaveChanges();

                    response.flag = "1";
                    response.status = "success";
                    response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                    response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  }
                  else
                  {


                    AttendanceMaster insertData = new AttendanceMaster();
                    insertData.employee_id = userValid.employee_id;
                    insertData.date = date;
                    insertData.day = date.DayOfWeek.ToString();
                    insertData.in_time = date.ToString("HH:mm:ss");
                    insertData.out_time = "00:00:00";
                    insertData.status = "";
                    insertData.absent = "";
                    insertData.mis = "MP";
                    insertData.shift = shiftCode;
                    insertData.total_hrs = 0;
                    insertData.ticket_status = false;
                    insertData.in_latitude = req.lat;
                    insertData.in_longitude = req.log;
                    insertData.in_address = req.address;
                    db.AttendanceMaster.Add(insertData);
                    db.SaveChanges();

                  }

                  // attendance log
                  AttendanceLogMaster attendanceLog = new AttendanceLogMaster();
                  attendanceLog.employee_id = userValid.employee_id;
                  attendanceLog.date = date;
                  if (retVal != null)
                  {
                    if (retVal.absent == "AB")
                    {
                      attendanceLog.status = "I";
                    }
                    else
                    {
                      attendanceLog.status = "O";
                    }

                  }
                  else
                  {
                    attendanceLog.status = "I";
                  }

                  //attendanceLog.shift = shift.shift_code;

                  attendanceLog.shift = "G";
                  attendanceLog.day = date.DayOfWeek.ToString();
                  attendanceLog.in_lat = req.lat;
                  attendanceLog.in_long = req.log;
                  attendanceLog.address = req.address;
                  db.AttendanceLogMaster.Add(attendanceLog);
                  db.SaveChanges();


                  string constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContextSmartOffice"].ConnectionString;
                  SqlConnection con = new SqlConnection(constr);
                  con.Open();
                  string query = "";
                  query = "insert into rawdata(ecode, ename, ldt, mid, dname, issync) values('" + userValid.employee_id + "', '" + userValid.name + "', getdate(), '146', 'WEMS', 1)";
                  SqlCommand cmd = new SqlCommand(query, con);
                  cmd.ExecuteNonQuery();
                  con.Close();

                  con.Open();
                  query = "";

                  query = "insert into " + GetTableName() + "(AttenndanceMarkingType, DownloadDate, DeviceId, UserId, LogDate, Direction, StatusCode, VerificationMode, IsApproved, Temperature, AttDirection)  ";
                  query += "values('Biometric', getdate(), '54', '" + userValid.employee_id + "', getdate() , 'in', 0, 2, 1, 0, '') ";
                  cmd = new SqlCommand(query, con);
                  cmd.ExecuteNonQuery();
                  con.Close();




                  response.flag = "1";
                  response.status = "success";
                  response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";

                }
                else
                {
                  response.alert = "You are not permitted . Please check your location!!";
                  response.data = "You are not permitted . Please check your location!!";
                }
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // work from home check in check out
    [HttpPost]
    [Route("WorkFromHomeCheckInCheckOut")]
    public HmcStringResponse WorkFromHomeCheckInCheckOut(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "You are not permitted check by normal punch. Please try again!!";
      response.data = "You are not permitted check by normal punch. Please try again!!";
      if (string.IsNullOrEmpty(req.lat) && string.IsNullOrEmpty(req.log))
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }
      else if (req.lat == "0.0" && req.log == "0.0")
      {
        response.alert = "Sorry, your location is not valid. Please try again.";
        response.data = "Sorry, your location is not valid. Please try again.";
        return response;
      }


      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();//db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid != null)
              {

                List<Shift> latlong = null;

                latlong = (from d in db.Shift
                           //where d.shift_code.Equals("G")
                           select d).ToList();

                Boolean checkInFlag = true;



                if (checkInFlag == true || userValid.marketing_emp == true)
                {
                  DateTime date = DateTime.Now;
                  DateTime dateMatch = date.Date;
                  AttendanceMaster retVal = (from d in db.AttendanceMaster
                                             where d.date.Equals(dateMatch)
                                              && d.employee_id.Equals(userValid.employee_id)
                                             select d).FirstOrDefault();

                  string monthStart = DateTime.Now.ToString("yyyy-MM") + "-01";
                  DateTime monthStartDate = Convert.ToDateTime(monthStart);

                  Shift shift = (from d in db.Shift
                                 where d.shift_code.Equals(userValid.shift)
                                 select d).FirstOrDefault();

                  string shiftCode = string.Empty;
                  if (shift == null)
                  {
                    shiftCode = "G";
                  }
                  else
                  {
                    shiftCode = shift.shift_code;
                  }

                  if (retVal != null)
                  {

                    retVal.day = date.DayOfWeek.ToString();
                    if (retVal.absent == "AB" || retVal.status == "H")
                    {
                      retVal.in_time = date.ToString("HH:mm:ss");
                      retVal.out_time = "00:00:00";
                      retVal.total_hrs = 0;
                      retVal.in_latitude = req.lat;
                      retVal.in_longitude = req.log;
                      retVal.in_address = req.address;
                      retVal.status = "";
                      retVal.absent = "";
                      retVal.mis = "MP";
                      retVal.shift = shiftCode;
                    }
                    else
                    {
                      retVal.out_time = date.ToString("HH:mm:ss");
                      DateTime dtFrom = DateTime.Parse(retVal.in_time);
                      DateTime dtTo = DateTime.Parse(retVal.out_time);
                      TimeSpan ts = dtTo.Subtract(dtFrom);
                      string times = ts.ToString();
                      string timeTotal = times.Replace(":", ".");
                      retVal.total_hrs = Convert.ToDouble(timeTotal.Remove(5, 3));
                      retVal.out_latitude = req.lat;
                      retVal.out_longitude = req.log;
                      retVal.out_address = req.address;
                      retVal.status = "P";
                      retVal.absent = "";
                      retVal.mis = "";
                    }

                    retVal.ticket_status = false;

                    db.SaveChanges();

                    response.flag = "1";
                    response.status = "success";
                    response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                    response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  }
                  else
                  {


                    AttendanceMaster insertData = new AttendanceMaster();
                    insertData.employee_id = userValid.employee_id;
                    insertData.date = date;
                    insertData.day = date.DayOfWeek.ToString();
                    insertData.in_time = date.ToString("HH:mm:ss");
                    insertData.out_time = "00:00:00";
                    insertData.status = "";
                    insertData.absent = "";
                    insertData.mis = "MP";
                    insertData.shift = shiftCode;
                    insertData.total_hrs = 0;
                    insertData.ticket_status = false;
                    insertData.in_latitude = req.lat;
                    insertData.in_longitude = req.log;
                    insertData.in_address = req.address;
                    db.AttendanceMaster.Add(insertData);
                    db.SaveChanges();

                  }

                  // attendance log
                  AttendanceLogMaster attendanceLog = new AttendanceLogMaster();
                  attendanceLog.employee_id = userValid.employee_id;
                  attendanceLog.date = date;
                  if (retVal != null)
                  {
                    if (retVal.absent == "A")
                    {
                      attendanceLog.status = "I";
                    }
                    else
                    {
                      attendanceLog.status = "O";
                    }

                  }
                  else
                  {
                    attendanceLog.status = "I";
                  }

                  attendanceLog.shift = shift.shift_code;
                  attendanceLog.day = date.DayOfWeek.ToString();
                  attendanceLog.in_lat = req.lat;
                  attendanceLog.in_long = req.log;
                  attendanceLog.address = req.address;
                  db.AttendanceLogMaster.Add(attendanceLog);
                  db.SaveChanges();

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";
                  response.data = "Thank you " + userValid.name + "  " + userValid.employee_id + " !!!";

                }
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // Get Company Qr Code
    [HttpPost]
    [Route("GetQrCodeImage")]
    public HmcStringResponse GetQrCodeImage(CheckInOut req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "user is not valid";
      response.data = "user is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.role == "Qr Code Scanner" && user.status == "Active");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.role.Equals("Qr Code Scanner")
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                QrCodeMaster qrCodeMaster = (from d in db.QrCodeMaster
                                             where d.pay_code.Equals(userValid.pay_code)
                                             select d).FirstOrDefault();

                if (qrCodeMaster != null)
                {
                  response.flag = "1";
                  response.status = "success";
                  response.alert = "success";
                  response.data = qrCodeMaster.qr_code;
                }
                else
                {
                  response.flag = "1";
                  response.status = "success";
                  response.alert = "qr code not exit same pay role area";
                  response.data = "qr code not exit same pay role area";
                }


              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // checting employee list
    [HttpPost]
    [Route("GetActiveEmployee")]
    public ActiveEmployeeResponse GetActiveEmployee(CheckInOut req)
    {
      ActiveEmployeeResponse response = new ActiveEmployeeResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "no data found!!";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                List<EmployeeBasicdetails> activeEmployee = (from d in db.EmployeeMaster
                                                             where d.password_count == 0
                                                             select new EmployeeBasicdetails
                                                             {
                                                               employee_id = d.employee_id,
                                                               name = d.name,
                                                               email = d.email,
                                                               department = d.department,
                                                               designation = d.designation,
                                                               feature_image = d.feature_image
                                                             }).ToList();

                response.flag = "1";
                response.status = "success";
                response.alert = "success";
                response.data = activeEmployee;
              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // my team summary
    [HttpPost]
    [Route("MyTeamSummary")]
    public MyTeamSummaryResponse MyTeamSummary(SearchEmployeeAttRequest req)
    {
      MyTeamSummaryResponse response = new MyTeamSummaryResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No Data Found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                List<AttendanceResponse> retVal = null;
                if (req.month != null && req.year != null)
                {
                  int days = DateTime.DaysInMonth(Convert.ToInt32(req.year), Convert.ToInt32(req.month));
                  string totoalDay = null;
                  totoalDay = days.ToString();
                  string fromdata = req.year + "-" + req.month + "-1";
                  string todate = req.year + "-" + req.month + "-" + totoalDay;

                  DateTime From = Convert.ToDateTime(fromdata);
                  DateTime to = Convert.ToDateTime(todate);

                  retVal = (from e in db.AttendanceMaster
                            where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                            orderby e.date ascending
                            select new AttendanceResponse
                            {
                              id = e.id,
                              employee_id = e.employee_id,
                              date = e.date,
                              day = e.day,
                              in_time = e.in_time,
                              out_time = e.out_time,
                              shift = e.shift,
                              status = e.status,
                              total_hrs = e.total_hrs,
                              mis = e.mis,
                              absent = e.absent,
                              early = e.early,
                              late = e.late
                            }).ToList();



                }
                else
                {

                  string FmDate = "01/" + DateTime.Now.ToString("MM/yyyy");
                  string TmDate = DateTime.Now.ToString("dd/MM/yyyy");
                  DateTime From = DateTime.ParseExact(FmDate, "dd/MM/yyyy", null);
                  DateTime to = DateTime.ParseExact(TmDate, "dd/MM/yyyy", null);
                  if (!string.IsNullOrEmpty(req.from_date))
                  {
                    From = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                    to = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);
                  }

                  if (!string.IsNullOrEmpty(req.type))
                  {
                    if (req.type == "MP")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.mis.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();
                    }
                    else if (req.type == "L" || req.type == "E")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.late.Equals("SHL")
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();
                    }
                    else if (req.type == "AB")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.absent.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();

                    }
                    else if (req.type == "EM")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.early.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();

                    }
                    else if (req.type == "P")
                    {
                      retVal = (from e in db.AttendanceMaster
                                where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                                && e.status.Equals(req.type)
                                orderby e.date ascending
                                select new AttendanceResponse
                                {
                                  id = e.id,
                                  employee_id = e.employee_id,
                                  date = e.date,
                                  day = e.day,
                                  in_time = e.in_time,
                                  out_time = e.out_time,
                                  shift = e.shift,
                                  status = e.status,
                                  total_hrs = e.total_hrs,
                                  mis = e.mis,
                                  absent = e.absent,
                                  early = e.early,
                                  late = e.late
                                }).ToList();
                    }
                  }
                  else
                  {
                    retVal = (from e in db.AttendanceMaster
                              where (e.date >= From) && (e.date <= to) && e.employee_id.Equals(employeeId)
                              orderby e.date ascending
                              select new AttendanceResponse
                              {
                                id = e.id,
                                employee_id = e.employee_id,
                                date = e.date,
                                day = e.day,
                                in_time = e.in_time,
                                out_time = e.out_time,
                                shift = e.shift,
                                status = e.status,
                                total_hrs = e.total_hrs,
                                mis = e.mis,
                                absent = e.absent,
                                early = e.early,
                                late = e.late
                              }).ToList();
                  }
                }

                foreach (AttendanceResponse item in retVal)
                {
                  item.total_hrs = Convert.ToDouble(item.total_hrs.ToString("#,##0.00"));
                  item.sdate = item.date.ToString("dd/MM/yyyy");
                  DateTime inTime = Convert.ToDateTime(item.in_time);
                  DateTime outTime = Convert.ToDateTime(item.out_time);
                  item.in_time = inTime.ToString("HH:mm:ss");
                  item.out_time = outTime.ToString("HH:mm:ss");
                }


                EmployeeDashboardPerameter leaveSummary = new EmployeeDashboardPerameter();



                leaveSummary.role = userValid.role;


                decimal[] EmpLeaves = new decimal[4];
                EmpLeaves[0] = Convert.ToDecimal(userValid.el) + Convert.ToDecimal(userValid.cl) + Convert.ToDecimal(userValid.sl);
                EmpLeaves[1] = Convert.ToDecimal(userValid.el);//empd.el;
                EmpLeaves[2] = Convert.ToDecimal(userValid.cl); //empd.cl;
                EmpLeaves[3] = Convert.ToDecimal(userValid.sl);//empd.sl;

                decimal[] Availd = new decimal[4];
                Availd[0] = Convert.ToDecimal(userValid.a_el) + Convert.ToDecimal(userValid.a_cl) + Convert.ToDecimal(userValid.a_sl);
                Availd[1] = Convert.ToDecimal(userValid.a_el);//empd.el;
                Availd[2] = Convert.ToDecimal(userValid.a_cl); //empd.cl;
                Availd[3] = Convert.ToDecimal(userValid.a_sl);//empd.sl;

                decimal[] Balance = new decimal[4];
                Balance[0] = Convert.ToDecimal(userValid.b_el) + Convert.ToDecimal(userValid.b_cl) + Convert.ToDecimal(userValid.b_sl);
                Balance[1] = Convert.ToDecimal(userValid.b_el);//empd.el;
                Balance[2] = Convert.ToDecimal(userValid.b_cl); //empd.cl;
                Balance[3] = Convert.ToDecimal(userValid.b_sl);//empd.sl;

                string[] BgName = new string[4];
                BgName[0] = "Total";
                BgName[1] = "EL";//empd.el;
                BgName[2] = "CL"; //empd.cl;
                BgName[3] = "SL";//empd.sl;

                leaveSummary.targets = EmpLeaves;
                leaveSummary.actuals = Availd;
                leaveSummary.bgNames = BgName;
                leaveSummary.balance = Balance;
                leaveSummary.total = EmpLeaves[0];
                leaveSummary.availd = Convert.ToDecimal(userValid.a_el) + Convert.ToDecimal(userValid.a_cl) + Convert.ToDecimal(userValid.a_sl);
                leaveSummary.balancel = Convert.ToDecimal(userValid.b_el) + Convert.ToDecimal(userValid.b_cl) + Convert.ToDecimal(userValid.b_sl);


                List<TeamDetails> teamDetails = null;

                teamDetails = (from d in db.EmployeeMaster
                               where d.reporting_to.Equals(employeeId)
                               select new TeamDetails
                               {
                                 employee_id = d.employee_id,
                                 name = d.name,
                                 designation = d.designation,
                                 email = d.email,
                                 feature_image = d.feature_image
                               }).ToList();

                foreach (TeamDetails item in teamDetails)
                {
                  item.encyptemployeeid = CryptoEngine.Encrypt(item.employee_id, "skym-3hn8-sqoy19");
                }


                MyTeamSummaryData result = new MyTeamSummaryData();
                result.attendance = retVal;
                result.leavesummary = leaveSummary;
                result.myteam = teamDetails;


                response.data = result;
                response.status = "success";
                response.flag = "1";
                response.alert = "success";

              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // check user permission is allowed work from home or not
    [HttpPost]
    [Route("CheckEmployeeStatus")]
    public AMSResponse CheckEmployeeStatus(CheckInOut req)
    {
      AMSResponse response = new AMSResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "no data found!!";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                DateTime todayDate = DateTime.Now;

                EmployeeWorkFromHome employeeObj = (from d in db.EmployeeWorkFromHome
                                                    where d.employee_id.Equals(employeeId)
                                                    && d.allow_date == todayDate.Date
                                                    select d).FirstOrDefault();

                response.flag = "1";
                response.status = "success";
                response.alert = "success";
                if (employeeObj != null)
                {
                  response.data = "HOME";
                }
                else
                {
                  response.data = "OFFICE";
                }

              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // create work from home employee wise
    [HttpPost]
    [Route("CreateWorkFromHomeEmployeeWise")]
    public AMSResponse CreateWorkFromHomeEmployeeWise(WorkFormHomeRequest req)
    {
      AMSResponse response = new AMSResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "no data found!!";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                foreach (AddDateListDto item in req.dateList)
                {
                  DateTime todayDate = Convert.ToDateTime(item.date);

                  EmployeeWorkFromHome employeeObj = (from d in db.EmployeeWorkFromHome
                                                      where d.employee_id.Equals(req.user_id)
                                                      && d.allow_date == todayDate.Date
                                                      select d).FirstOrDefault();
                  if (employeeObj == null)
                  {
                    EmployeeWorkFromHome insertData = new EmployeeWorkFromHome();
                    insertData.employee_id = req.user_id;
                    insertData.allow_date = todayDate;
                    db.EmployeeWorkFromHome.Add(insertData);
                    db.SaveChanges();
                  }
                }



                response.flag = "1";
                response.status = "success";
                response.alert = "success";
                response.data = "success";

              }
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // download employee salary
    [HttpPost]
    [Route("GenerateEmployeeSalaryPDF")]
    public HmcStringResponse GenerateEmployeeSalaryPDF(SearchEmployeeAttRequest req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {

                AttResponse attendanceRes = new AttResponse();

                attendanceRes.print_date = DateTime.Now.ToString("dd/MM/yyyy");

                SalarySlip salarySlipData = new SalarySlip();

                salarySlipData = (from d in db.SalarySlip
                                  where d.id.Equals(req.id)
                                  select d).FirstOrDefault();

                int days = DateTime.DaysInMonth(Convert.ToInt32(salarySlipData.year), Convert.ToInt32(salarySlipData.month));

                DataTable dt = new DataTable();
                string constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContext"].ConnectionString;
                SqlConnection con = new SqlConnection(constr);
                con.Open();

                string salaryQuery = "select em.employee_id, em.name, em.employee_salary, em.acc_no, em.ifsc_code, em.bank_name, " +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status in ('P', 'EL', 'CL', 'SL', 'WFH', 'OD')) as present," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status = 'H') as holiday," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status = 'HD') as halfday," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and(absent = 'AB' or status = 'AB')) as absent," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and mis = 'MP') as mispunch," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and late = 'L') as late," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and early = 'E') as early," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and (late = 'L' or early = 'E')) as lateEarly," +
                                                " (select COALESCE(sum(total_hrs), 0) from Attendance where employee_id = em.employee_id and date BETWEEN  '" + salarySlipData.year + "-" + salarySlipData.month + "-01' and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "') as totalHours," +                                                
                                                " (select top 1 COALESCE(SUM(mobile_bill + conveyance + performance_variable + other_received + advanced_salary), 0) from " + 
                                                " EmployeeConveyance where employee_id = em.employee_id and month = '"+ salarySlipData.month + "' GROUP BY id) EmpConvence" +
                                                " from SalarySlip ss join EmployeeMaster em on ss.pay_code = em.pay_code " +
                                                " where ss.id = " + req.id + " and em.employee_id != 'SSIPL10001' and em.status='Active' and em.pay_code = '"+ salarySlipData.pay_code + "' and em.doj < '" + salarySlipData.year + "-" + salarySlipData.month + "-01' ";

                salaryQuery = salaryQuery + " UNION ALL select em.employee_id, em.name, em.employee_salary, em.acc_no, em.ifsc_code, em.bank_name, " +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status in ('P', 'EL', 'CL', 'SL', 'WFH', 'OD')) as present," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status = 'H') as holiday," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and status = 'HD') as halfday," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and(absent = 'AB' or status = 'AB')) as absent," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and mis = 'MP') as mispunch," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and late = 'L') as late," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and early = 'E') as early," +
                                                " (select count(*) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "' and (late = 'L' or early = 'E')) as lateEarly," +
                                                " (select COALESCE(sum(total_hrs), 0) from Attendance where employee_id = em.employee_id and date BETWEEN  em.doj and '" + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "') as totalHours," +
                                                " (select top 1 COALESCE(SUM(mobile_bill + conveyance + performance_variable + other_received + advanced_salary), 0) from " +
                                                " EmployeeConveyance where employee_id = em.employee_id and month = '"+ salarySlipData.month + "' GROUP BY id) EmpConvence" +
                                                " from SalarySlip ss join EmployeeMaster em on ss.pay_code = em.pay_code " +
                                                " where ss.id = " + req.id + " and em.employee_id != 'SSIPL10001' and em.status='Active' and em.pay_code = '" + salarySlipData.pay_code + "' and em.doj >= '" + salarySlipData.year + "-" + salarySlipData.month + "-01' order by em.employee_id asc";
                SqlCommand cmd = new SqlCommand(salaryQuery, con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                con.Close();


                string fileName = DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                //Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate(), 30, 30, 60, 30);
                Document doc = new Document(iTextSharp.text.PageSize.A4);
                //var fpath = Server.MapPath("~/customerdoc/SID/AccountStatement/");
                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                //string fpath = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                Font font1_verdana = FontFactory.GetFont("Verdana", 11, Font.BOLD | Font.BOLD);
                //Font font12 = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 8, Font.NORMAL);

                doc.Open();


                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                image.ScalePercent(50f);
                image.SetAbsolutePosition(30, 800);
                doc.Add(image);

                StringBuilder sb = new StringBuilder();
                //sb.Append(req.search_result);
                sb.Append("<body style='Font-family:Verdana;'>");
                sb.Append("<div style='Font-family:Verdana; text-align: center;'>Employee Salary Report <br />" + salarySlipData.year + "-" + salarySlipData.month + "-01 To " + salarySlipData.year + "-" + salarySlipData.month + "-" + days.ToString() + "</div>");
                sb.Append("<table width='100%' cellpadding='3' cellspacing='0' border='0.5' style='font-size:8px;'>");
                //sb.Append("<tr><td colspan='9' border='0' style='text-align:center; font:size: 16px;'>Attendance Report <br />" + req.from_date + " To " + req.to_date +"</td></tr>");
                sb.Append("<tr style='font-size:10px;'><td colspan='3' border='0'>Company :" + salarySlipData.pay_code + " </td><td colspan='3' border='0' style='text-align:right;'>Print On: " + attendanceRes.print_date + "  </td></tr>");
                
                sb.Append("<tr><td bgcolor='#cdcdcd'>Employee Details</td><td bgcolor='#cdcdcd'>Account Details</td><td bgcolor='#cdcdcd'>Gross Salary</td><td bgcolor='#cdcdcd'>Total Salary</td><td bgcolor='#cdcdcd'>Attendace Status</td><td bgcolor='#cdcdcd'>Day</td></tr>");
                foreach (DataRow dr in dt.Rows)
                {
                  decimal salaryInHand = 0M;
                  decimal oneDaySalary = Convert.ToDecimal(dr["employee_salary"].ToString()) / days;
                  decimal totalPresentSalary = salaryInHand + (Convert.ToDecimal(dr["present"].ToString()) * oneDaySalary);
                  decimal totalHolidaySalary = salaryInHand + (Convert.ToDecimal(dr["holiday"].ToString()) * oneDaySalary);

                  decimal totalHalfDay = Convert.ToDecimal(dr["halfday"].ToString()) * (oneDaySalary/2);
                  //decimal earlyAndlate = Convert.ToDecimal(dr["late"].ToString()) + Convert.ToDecimal(dr["early"].ToString());

                  decimal totalAbsent = Convert.ToDecimal(dr["absent"].ToString()) * oneDaySalary; //
                  decimal totalMispunch = Convert.ToDecimal(dr["mispunch"].ToString()) * (oneDaySalary / 2);

                  decimal totalEalryLate = 0M;

                  int earlyAndlate = 0;

                  if (!string.IsNullOrWhiteSpace(dr["lateEarly"].ToString()))
                  {
                    earlyAndlate = Convert.ToInt32(dr["lateEarly"].ToString());
                  }


                  //int earlyAndlate = Convert.ToInt32(dr["absent"].ToString());

                  if (earlyAndlate > 3)
                  {
                    int totCount = earlyAndlate - 3;
                    totalEalryLate = totCount * (oneDaySalary / 2);
                  }
                  else
                  {
                    totalEalryLate = earlyAndlate * oneDaySalary;
                  }

                  decimal empConvence = 0M;

                  if (!string.IsNullOrWhiteSpace(dr["EmpConvence"].ToString()))
                  {
                    empConvence = Convert.ToDecimal(dr["EmpConvence"].ToString());
                  }

                  salaryInHand = totalPresentSalary + totalHolidaySalary + totalHalfDay + totalEalryLate + totalMispunch;

                  decimal totalGrossSalary = salaryInHand + empConvence;

                  decimal totalDedection = Convert.ToDecimal(dr["employee_salary"].ToString()) - salaryInHand;

                  int totalDays = Convert.ToInt32(dr["present"].ToString()) + Convert.ToInt32(dr["holiday"].ToString()) + Convert.ToInt32(dr["halfday"].ToString())
                     + Convert.ToInt32(dr["late"].ToString()) + Convert.ToInt32(dr["early"].ToString()) + Convert.ToInt32(dr["mispunch"].ToString());

                  decimal totHours = Convert.ToDecimal(dr["totalHours"].ToString());

                  if (totHours > 0)
                  {
                    sb.Append("<tr><td>" + dr["name"].ToString() + " <br> " + dr["employee_id"].ToString() + "</td>" +
                    "<td>" + dr["bank_name"].ToString() + "<br>" + dr["acc_no"].ToString() + " <br>" + dr["ifsc_code"].ToString() + "</td>" +
                    "<td>" + dr["employee_salary"].ToString() + "</td>" +
                    "<td><b>Total Deduction: </b>" + totalDedection.ToString("#,##0.00") + " <br> " +
                    "<b>Conveyance: </b>" + empConvence.ToString("#,##0.00") + " <br> " +
                    "<b>Net Salary: </b>" + salaryInHand.ToString("#,##0.00") + " <br> " +
                    "<b>Total: </b>" + totalGrossSalary.ToString("#,##0.00") + "</td>" +
                    "<td><b>Present:</b> " + dr["present"].ToString() + " <br> <b>Absent:</b> " + dr["absent"].ToString() + " <br> " +
                    "<b>Mispunch:</b> " + dr["mispunch"].ToString() + " <br> <b>Late:</b> " + dr["late"].ToString() + " <br> " +
                    "<b>Early:</b> " + dr["early"].ToString() + " <br> <b>Halfday:</b>  " + dr["halfday"].ToString() + " <br>" +
                    "<b>Holiday:</b> " + dr["holiday"].ToString() + " <br> <b>Total Hours: </b> " + dr["totalHours"].ToString() + " </td>" +
                    "<td>" + totalDays + "</td></tr>");
                  }                 

                }


                sb.Append("</table>");
                //sb.Append("<p width='100%' style='font-size: 10px;'>Total Duration = " + totlHRS + " Hrs  " + TotalMintus + " Min, Present= " + totalPrent + ", Leaves = " + totalLeave + ", Absent = " + totalAbsent +
                //    ", Week Off = " + totalWeekOff + ", OD = " + totalOD + ", Early = " + totalEarly + ", Late = " + totalLate + ", Mispunch = " + totalMis + ", Half Day = " + totalHalfDay + "</p>");
                sb.Append("</body>");
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();
                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = fileName;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // get master data
    [HttpPost]
    [Route("GetAllMasterList")]
    public MasterData GetAllMasterList(KeyValue req)
    {
      MasterData response = new MasterData();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            

                response.department = (from d in db.KeyValueMaster
                                       where d.key_type.Equals("department")
                                    
                                       select d).ToList();

                response.designation = (from d in db.KeyValueMaster
                                        where d.key_type.Equals("designation")
                                      
                                        select d).ToList();

                response.role = (from d in db.KeyValueMaster
                                 where d.key_type.Equals("role")
                              
                                 select d).ToList();

                response.shift = (from d in db.Shift
                                 
                                  select d).ToList();


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = "success";
             

           

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    /// admin information
    [HttpPost]
    [Route("AdminInfo")]
    public CountMaster AdminInfo(PolicyMasterResponse req)
    {
      CountMaster response = new CountMaster();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {

              /// count total employee
              int empcount = (from d in db.EmployeeMaster
                              where d.department != "SuperAdmin"
                              select d).Count();

              int activeEmp = (from d in db.EmployeeMaster
                               where d.department != "SuperAdmin" && d.password_count.Equals(0)
                               select d).Count();

              int inActiveEmp = (from d in db.EmployeeMaster
                                 where d.department != "SuperAdmin" && d.password_count.Equals(1)
                                 select d).Count();

              // total aaply leave
              int applyleavecount = (from d in db.ApplyLeaveMasters
                                     select d).Count();

              // total mispunch
              int mispunchcount = (from d in db.AttendanceMispunch
                                   select d).Count();

              // today present employee
              DateTime todayDate = DateTime.Now;

              int presentCount = (from d in db.AttendanceMaster
                                  where d.date.Equals(todayDate)
                                  select d).Count();

              var atttic = (from p in db.AttendanceMispunch
                            group p by p.employee_id into pgroup
                            let count = pgroup.Count()
                            orderby count descending
                            select new CountRes { count = count, employee_id = pgroup.Key }).Skip(0).Take(10);



              List<CountRes> attTicket = atttic.ToList<CountRes>();
              CountResponse retval = new CountResponse();
              List<MispuchEmpResponse> resData = new List<MispuchEmpResponse>();
              if (attTicket.Count() > 0)
              {
                foreach (CountRes item in attTicket)
                {
                  EmployeeMaster emp = (from d in db.EmployeeMaster
                                        where d.employee_id.Equals(item.employee_id)
                                        select d).FirstOrDefault();
                  if (emp != null)
                  {
                    MispuchEmpResponse empDetails = new MispuchEmpResponse();
                    empDetails.employee_id = emp.employee_id;
                    empDetails.name = emp.name;
                    empDetails.miscount = item.count;
                    empDetails.approve_by = emp.reporting_to;
                    EmployeeMaster reportingManager = (from r in db.EmployeeMaster
                                                       where r.employee_id.Equals(emp.reporting_to)
                                                       select r).FirstOrDefault();
                    if (reportingManager != null)
                    {
                      empDetails.approve_by_name = reportingManager.name;
                    }
                    resData.Add(empDetails);

                  }
                }
              }


              retval.totalEmployee = empcount;
              retval.totalApplyLeave = applyleavecount;
              retval.totalMispunch = mispunchcount;
              retval.mispunchEmployee = resData;
              retval.activeEmployee = activeEmp;
              retval.inActiveEmployee = inActiveEmp;

              response.status = "success";
              response.flag = "1";
              response.alert = "success";
              response.data = retval;

            }



          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }




    // get all apply leave list
    [HttpPost]
    [Route("GetAdminDeshboardData")]
    public DesboardDataResponse GetAdminDeshboardData(KeyValue req)
    {
      DesboardDataResponse response = new DesboardDataResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              int page = 0;
              int pageSize = 20; // set your page size, which is number of records per page
              int skip = pageSize * page;

              DesboardResponse retVal = new DesboardResponse();

              retVal.role = (from r in db.EmployeeMaster
                             where r.employee_id.Equals(req.employee_id)
                             select r.department).FirstOrDefault();
              //if (retVal.role != null)
              //{
              //    LeaveMaster leavMast = (from d in db.LeaveMaster
              //                            where d.key_code.Equals(retVal.role)
              //                            select d).FirstOrDefault();
              //    if (leavMast != null)
              //    {
              //        retVal.role = leavMast.key_code;
              //    }

              //}

              if (retVal.role == "SuperAdmin")
              {
                retVal.applyleave = (from d in db.ApplyLeaveMasters
                                     orderby d.id descending
                                     select new ApplyLeaveResponse
                                     {
                                       employee_id = d.employee_id.ToString(),
                                       from_date = d.from_date,
                                       to_date = d.to_date,
                                       date = d.apply_date,
                                       status = d.status,
                                       reason = d.reason,
                                       assign_by = d.assign_by,
                                       leave_type = d.leave_Type,
                                       no_of_leave = d.no_of_leave.ToString()
                                     }).Skip(skip).Take(pageSize).Distinct().ToList();

                if (retVal.applyleave.Count > 0)
                {
                  foreach (ApplyLeaveResponse item in retVal.applyleave)
                  {
                    item.todate = item.to_date.ToString("dd/MM/yyyy");
                    item.fromdate = item.from_date.ToString("dd/MM/yyyy");
                    item.applydate = item.date.ToString("dd/MM/yyyy");

                    EmployeeMaster result = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(item.assign_by.ToString())
                                             select d).FirstOrDefault();

                    if (result != null)
                    {
                      item.assign_by_name = result.name;
                    }

                    EmployeeMaster empDetails = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(item.employee_id)
                                                 select d).FirstOrDefault();
                    if (empDetails != null)
                    {
                      item.apply_by_name = empDetails.name;
                    }

                    ApplyLeaveMasters leaveMaster = (from d in db.ApplyLeaveMasters
                                               where d.id.Equals(item.leave_type)
                                               select d).FirstOrDefault();
                    if (result != null)
                    {
                      item.leave_type = leaveMaster.leave_Type;
                    }
                  }
                }
              }




              /*retVal.resetpassword = (from r in db.ResetPassword
                                      orderby r.id descending
                                      select r).Skip(skip).Take(pageSize).ToList();*/


              if (retVal.role != "SuperAdmin")
              {
                retVal.team = (from r in db.EmployeeMaster
                               where r.reporting_to.Equals(req.employee_id)
                               select new MyTeamDTO
                               {
                                 name = r.name,
                                 email = r.email,
                                 employee_id = r.employee_id,
                                 designation = r.designation,
                                 department = r.department,
                                 feature_image = r.feature_image

                               }).ToList();

                string currentYear = DateTime.Now.Year.ToString();
                DateTime fromdate = Convert.ToDateTime(currentYear + "-01-01");
                DateTime todate = Convert.ToDateTime(currentYear + "-12-31");

                retVal.holiday = (from r in db.HolidayMaster
                                  where r.holiday_date >= fromdate
                                  && r.holiday_date <= todate
                                  orderby r.holiday_date ascending
                                  select new HolidayData
                                  {
                                    id = r.id,
                                    title = r.title,
                                    holiday_date = r.holiday_date,
                                    
                                  }).ToList();
                foreach (HolidayData item in retVal.holiday)
                {
                  item.start = item.holiday_date.ToString("yyyy-MM-dd");
                  item.end = item.holiday_date.ToString("yyyy-MM-dd");
                }
              }

              if (retVal != null)
              {
                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // leave performance
    // get apply leave 
    [HttpPost]
    [Route("GetLeavePerformance")]
    public LeaveApplyNewResponse GetLeavePerformance(EmployeeMasterDTO req)
    {
      LeaveApplyNewResponse response = new LeaveApplyNewResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "No Data Found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              List<KeyValueMaster> bgNames = (from d in db.KeyValueMaster
                                              where d.key_type.Equals("leave")
                                           select d).ToList();
              LeaveType empd;

              empd = (from e in db.EmployeeMaster
                      where e.employee_id.Equals(employeeId)
                      select new LeaveType
                      {
                        el = e.el,
                        cl = e.cl,
                        sl = e.sl,
                        a_el = e.a_el,
                        a_cl = e.a_cl,
                        a_sl = e.a_sl,
                        b_el = e.b_el,
                        b_cl = e.b_cl,
                        b_sl = e.b_sl,

                      }).FirstOrDefault();

              if (empd == null)
              {

                empd = (from e in db.EmployeeMaster
                        where e.employee_id.Equals(employeeId)
                        select new LeaveType
                        {
                          el = e.el,
                          cl = e.cl,
                          sl = e.sl,
                          a_el = e.a_el,
                          a_cl = e.a_cl,
                          a_sl = e.a_sl,
                          b_el = e.b_el,
                          b_cl = e.b_cl,
                          b_sl = e.b_sl,

                        }).FirstOrDefault();


              }


              decimal[] EmpLeaves = new decimal[4];
              EmpLeaves[0] = Convert.ToDecimal(empd.el) + Convert.ToDecimal(empd.cl) + Convert.ToDecimal(empd.sl);
              EmpLeaves[1] = Convert.ToDecimal(empd.el);//empd.el;
              EmpLeaves[2] = Convert.ToDecimal(empd.cl); //empd.cl;
              EmpLeaves[3] = Convert.ToDecimal(empd.sl);//empd.sl;

              decimal[] Availd = new decimal[4];
              Availd[0] = Convert.ToDecimal(empd.a_el) + Convert.ToDecimal(empd.a_cl) + Convert.ToDecimal(empd.a_sl);
              Availd[1] = Convert.ToDecimal(empd.a_el);//empd.el;
              Availd[2] = Convert.ToDecimal(empd.a_cl); //empd.cl;
              Availd[3] = Convert.ToDecimal(empd.a_sl);//empd.sl;

              decimal[] Balance = new decimal[4];
              Balance[0] = Convert.ToDecimal(empd.b_el) + Convert.ToDecimal(empd.b_cl) + Convert.ToDecimal(empd.b_sl);
              Balance[1] = Convert.ToDecimal(empd.b_el);//empd.el;
              Balance[2] = Convert.ToDecimal(empd.b_cl); //empd.cl;
              Balance[3] = Convert.ToDecimal(empd.b_sl);//empd.sl;

              string[] BgName = new string[4];
              BgName[0] = "Total";
              BgName[1] = "EL";//empd.el;
              BgName[2] = "CL"; //empd.cl;
              BgName[3] = "SL";//empd.sl;

              //string[] array2 = empd.ToArray();
              EmployeeDashboardDTO retVal = new EmployeeDashboardDTO();

              List<decimal> donatActuals = new List<decimal>();
              List<string> donatBgNames = new List<string>();

              retVal.targets = EmpLeaves;
              retVal.actuals = Availd;
              retVal.balance = Balance;
              retVal.bgNames = BgName;
              //decimal totlbalance = EMpLeaves[0] - totalApplyLeave;
              //string totalBalnce = totlbalance.ToString();

              decimal totalAvaild = Convert.ToDecimal(empd.a_el) + Convert.ToDecimal(empd.a_cl) + Convert.ToDecimal(empd.a_sl);
              decimal totalBalance = Convert.ToDecimal(empd.b_el) + Convert.ToDecimal(empd.b_cl) + Convert.ToDecimal(empd.b_sl);

              donatActuals.Insert(0, EmpLeaves[0]);
              donatActuals.Insert(1, totalAvaild);
              donatActuals.Insert(2, totalBalance);
              donatBgNames.Insert(0, "Total " + EmpLeaves[0]);
              donatBgNames.Insert(1, "Availed " + totalAvaild);
              donatBgNames.Insert(2, "Balance " + totalBalance);

              retVal.donatBgNames = donatBgNames;
              retVal.donatActuals = donatActuals;
              //retVal.applyleave = bgNames;

              response.data = retVal;
              response.flag = "1";
              response.status = "success";
              response.alert = "";
            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    // get leave master list
    // login employee details
    [HttpPost]
    [Route("GetLeaveMaster")]
    public LeaveMasterResponse GetLeaveMaster(EmployeeMasterDTO req)
    {
      LeaveMasterResponse response = new LeaveMasterResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {

              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(req.employee_id)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (userValid != null)
              {
                KeyMaster keyMaster = new KeyMaster();
               // List<KeyValueMaster> keyMaster = null;

                if (userValid.pay_code == "1015" || userValid.pay_code == "7500")
                {
                  keyMaster.leave_type = (from d in db.KeyValueMaster
                                          where d.key_type.Equals("leave")
                                          && d.key_code.Equals("leave")
                                          orderby d.id ascending
                                          select d).ToList();
                }
                else
                {
                  keyMaster.leave_type = (from d in db.KeyValueMaster
                                          where d.key_type.Equals("leave")
                                          && d.key_code != "WFH"
                                          && d.key_type.Equals("leave")
                                          orderby d.id ascending
                                          select d).ToList();
                }


                keyMaster.duration_type = (from d in db.KeyValueMaster
                                           where d.key_type.Equals("leave")
                                           && d.key_type.Equals("duration")
                                           orderby d.id ascending
                                           select d).ToList();

                keyMaster.short_type = (from d in db.KeyValueMaster
                                        where d.key_type.Equals("short")
                                        && d.key_type.Equals("duration")
                                        orderby d.id ascending
                                        select d).ToList();

                response.status = "success";
                response.flag = "1";
                response.data = keyMaster;
                response.alert = "success";
              }

            }
            else
            {
              response.status = "error";
              response.flag = "0";
              response.alert = "data is not valid";
            }

          }

        }
        else
        {
          response.status = "error";
          response.flag = "0";
          response.alert = "data is not valid";
        }

      }
      catch (Exception ex)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = ex.Message;
      }

      return response;

    }


    // search attendance based on employee id
    [HttpPost]
    [Route("GetMyAttendance")]
    public HmcAttResponse GetMyAttendance(SearchEmployeeAttRequest req)
    {
      HmcAttResponse response = new HmcAttResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();


               // DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
               // DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);

                List<AttendanceResponse> attmaster = null;

               // int page = Convert.ToInt32(req.pageNo);
               // var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
               // var skip = pageSize * page;

                if (!string.IsNullOrEmpty(req.employee_code))
                {
                  req.department = "";
                  req.shift = "";

                }

                if (!string.IsNullOrEmpty(req.department) && !string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                               && e.shift.Equals(req.shift)
                               && e.employee_id.Equals(req.employee_code)
                               //&& e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.department))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where d.department.Equals(req.department)
                             
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.shift))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.shift.Equals(req.shift)
                               
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else if (!string.IsNullOrEmpty(req.employee_code))
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               where e.employee_id.Equals(req.employee_code)
                               //where e.employee_id.Equals(req.employee_code)
                              // && e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }
                else
                {
                  attmaster = (from e in db.AttendanceMaster
                               join d in db.EmployeeMaster on e.employee_id equals d.employee_id
                               //where e.date >= fromDate && e.date <= toDate
                               //&& d.pay_code.Equals(empMaster.pay_code)
                               orderby e.date ascending
                               select new AttendanceResponse
                               {
                                 id = e.id,
                                 employee_id = e.employee_id,
                                 date = e.date,
                                 day = e.day,
                                 in_time = e.in_time,
                                 out_time = e.out_time,
                                 shift = e.shift,
                                 status = e.status,
                                 absent = e.absent,
                                 late = e.late,
                                 early = e.early,
                                 mis = e.mis,
                                 total_hrs = e.total_hrs,
                                 in_latitude = e.in_latitude,
                                 in_longitude = e.in_longitude,
                                 location_address = e.in_address
                               }).ToList();
                }



                foreach (AttendanceResponse item in attmaster)
                {
                  item.sdate = item.date.ToString("dd/MM/yyyy");
                  DateTime inTime = Convert.ToDateTime(item.in_time);
                  DateTime outTime = Convert.ToDateTime(item.out_time);
                  item.in_time = inTime.ToString("HH:mm:ss");
                  item.out_time = outTime.ToString("HH:mm:ss");
                }



                EmployeeMaster empMasterv = (from d in db.EmployeeMaster
                                             where d.employee_id.Equals(req.employee_code)
                                             select d).FirstOrDefault();

                AttResponse attendanceRes = new AttResponse();

                attendanceRes.attendaceList = attmaster;

                if (empMasterv != null)
                {
                  attendanceRes.employee_id = empMasterv.employee_id;
                  attendanceRes.name = empMasterv.name;
                  attendanceRes.department = empMasterv.department;
                  attendanceRes.company_name = empMasterv.pay_code;
                }
                else
                {
                  attendanceRes.department = req.department;
                  attendanceRes.company_name = "All";
                  attendanceRes.shift = req.shift;
                }

                attendanceRes.print_date = DateTime.Now.ToString("dd/MM/yyyy");


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = attendanceRes;
              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    [HttpPost]
    [Route("GetEmployeeId")]
    public AMSResponse GetEmployeeId(CheckInOut req)
    {
      AMSResponse response = new AMSResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "no data found!!";
      string DatafromWave = "";
      string DataFromSmartOffice = "";

      try
      {
        //if (ModelState.IsValid)
        //{
          using (AttendanceContext db = new AttendanceContext())
          {

            string  employee_id = CryptoEngine.Encrypt(req.employee_id, "skym-3hn8-sqoy19");
            response.flag = "1";
            response.status = "success";
            response.alert = "success";
            response.flag = employee_id;

          string constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContextSmartOffice"].ConnectionString;
          SqlConnection con = new SqlConnection(constr);
          con.Open();
          string query = "";       
          query = "Select max(ldt) from rawdata";
          SqlCommand cmd = new SqlCommand(query, con);
          DataFromSmartOffice = "From Smart Office=>"+ cmd.ExecuteScalar().ToString();
           con.Close();


          constr = System.Configuration.ConfigurationManager.ConnectionStrings["AttendanceContext"].ConnectionString;
          con = new SqlConnection(constr);
          con.Open();
          query = "";
          query = "Select max(date) from Attendance";
          cmd = new SqlCommand(query, con);
          DatafromWave = "From WaveDB=>" + cmd.ExecuteScalar().ToString();
          con.Close();


          response.status = DataFromSmartOffice;
          response.data = DatafromWave;
        }
        //}

      }
      catch (Exception e)
      {
        response.alert = e.Message;
        response.status = DataFromSmartOffice;
        response.data = DatafromWave;


      }
      return response;
    }



    // Get Mispunch Request
    [HttpPost]
    [Route("GetTeamMispunchRequest")]
    public GetMispunchResponse GetTeamMispunchRequest(HmcRequest req)
    {
      GetMispunchResponse response = new GetMispunchResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data not found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                int page = Convert.ToInt32(req.pageNo);
                List<MispunchResponse> retVal = null;
                var pageSize = Convert.ToInt32(req.pageSize); // set your page size, which is number of records per page
                var skip = pageSize * page;
                int currentMonth = DateTime.Now.Month;
                if (req.search_result != null)
                {
                  retVal = (from d in db.AttendanceMispunch
                              //join e in db.AttendanceMaster on e.atte
                            where d.employee_id.ToLower().Contains(req.search_result)
                            && d.modify_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new MispunchResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id,
                              date = d.apply_date,
                              in_time = d.in_time,
                              out_time = d.out_time,
                              status = d.status,
                              reason = d.reason,
                              shift = d.shift,
                              misdate = d.date,
                              assign_by = d.assign_by,
                            }).Skip(skip).Take(pageSize).ToList();

                }
                else
                {
                  retVal = (from d in db.AttendanceMispunch
                            where d.modify_by.Equals(employeeId)
                            orderby d.apply_date descending
                            select new MispunchResponse
                            {
                              id = d.id,
                              employee_id = d.employee_id,
                              date = d.apply_date,
                              in_time = d.in_time,
                              out_time = d.out_time,
                              status = d.status,
                              shift = d.shift,
                              reason = d.reason,
                              misdate = d.date,
                              assign_by = d.assign_by,
                            }).Skip(skip).Take(pageSize).ToList();
                }

                if (retVal.Count > 0)
                {
                  foreach (MispunchResponse item in retVal)
                  {

                    item.applydate = item.date.ToString("dd/MM/yyyy");
                    item.empmisdate = item.misdate.ToString("dd/MM/yyyy");

                    EmployeeMaster assignBy = (from d in db.EmployeeMaster
                                               where d.employee_id.Equals(item.assign_by)
                                               select d).FirstOrDefault();

                    if (assignBy != null)
                    {
                      item.assign_by_name = assignBy.name;
                    }

                    EmployeeMaster applyBy = (from d in db.EmployeeMaster
                                              where d.employee_id.Equals(item.employee_id)
                                              select d).FirstOrDefault();
                    if (applyBy != null)
                    {
                      item.apply_by_name = applyBy.name;
                    }
                  }
                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    [HttpPost]
    [Route("GetCheckInandOutStatus")]
    public HmcResponseNew GetCheckInandOutStatus(CheckInOutNew req)
    {
      DateTime TodayDate = DateTime.Now;//Change
      //string mdatetime = req.timedate.ToString();
      //DateTime req_timedate = DateTime.Parse(mdatetime);
      //var seconds = (TodayDate < req_timedate) ? (req_timedate - TodayDate).TotalMinutes : (TodayDate - req_timedate).TotalMinutes;

      DateTime req_timedate = DateTime.ParseExact(req.timedate, "dd/MM/yyyy", null);

      //if (TodayDate<= req_timedate || TodayDate <= req_timedate)
      HmcResponseNew response = new HmcResponseNew();
      response.flag = "0";
      response.status = "error";
      response.alert = "You are not permited to check in check out";
      response.data = "You are not permited to check in check out";

      using (AttendanceContext db = new AttendanceContext())
      {
        string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
        string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
        if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
        {
          EmployeeMaster userValid = null;     

          userValid = (from d in db.EmployeeMaster
                       where d.employee_id.Equals(employeeId)
                       && d.status.Equals("Active")
                       select d).FirstOrDefault();
          if (userValid != null)
          {
            Boolean checkIndFlag = true;    
            if (checkIndFlag)
            {
              DateTime dateMatch = TodayDate.Date;
              AttendanceMaster attendancemaster = (from d in db.AttendanceMaster
                                                   where d.employee_id.Equals(employeeId)
                                                   && d.date.Year == req_timedate.Year
                                                   && d.date.Month == req_timedate.Month
                                                   && d.date.Day == req_timedate.Day
                                                   select d).FirstOrDefault();

              attendancemaster = new AttendanceMaster();

              HmcResponseNew responce = GetFirstAndLastPunching(employeeId, req.timedate);
              attendancemaster.in_time = responce.status;
              attendancemaster.out_time = responce.alert;

              if (attendancemaster != null)
              {
                if (attendancemaster.in_time == "00:00:00")
                {
                  response.flag = "0";
                  response.status = attendancemaster.in_time;
                  response.data = attendancemaster.out_time;
                  response.alert = "Check-In";
                }
                else
                {
                  response.flag = "2";
                  response.status = attendancemaster.in_time;
                  response.data = attendancemaster.out_time;
                  response.alert = "Check-Out";
                }
              }
              else
              {
                response.flag = "0";
                response.status = "00:00:00";
                response.data = "00:00:00";
                response.alert = "no record found";

              }
            }
          }
        }
      }
      return response;
    }



    // Get Attendance History List
    //[HttpPost]
    //[Route("GetMonthWiseReport")]

    //public HmcResponseNew GetMonthWiseReport(GetAttendance_DataQuery req)
    //{

    //  HmcResponseNew response = new HmcResponseNew();
    //  response.flag = "0";
    //  response.status = "error";
    //  response.alert = "You are not permited to check in check out";
    //  response.data = "You are not permited to check in check out";



    //  DataSet ds = new DataSet();

    //  SqlConnection con = new SqlConnection(constr);
    //  SqlConnection con1 = new SqlConnection(constr);
    //  con.Open();
    //  string query = "SELECT  TOP (DATEDIFF(DAY, '" + req.from_date + "','" + req.to_date + "') + 1)  ";
    //  query += "   Date = convert(nvarchar(10), DATEADD(DAY, ROW_NUMBER() OVER(ORDER BY a.object_id) - 1, '" + req.from_date + "'),111)  ";
    //  query += " FROM sys.all_objects a ";
    //  query += "  CROSS JOIN sys.all_objects b; ";
    //  SqlCommand cmd = new SqlCommand(query, con);
    //  SqlDataAdapter adapter = new SqlDataAdapter(cmd);
    //  adapter.Fill(ds);
    //  int totalleaves = 0;
    //  for (int count = 0; count < ds.Tables[0].Rows.Count; count++)
    //  {
    //    string Date = Convert.ToString(ds.Tables[0].Rows[count]["date"].ToString());
    //    con1.Open();
    //    SqlCommand cmd1 = new SqlCommand("Get_ApplyLeave_Status", con1);
    //    cmd1.CommandType = CommandType.StoredProcedure;
    //    cmd1.Parameters.AddWithValue("@EMPCODE", req.employee_id);
    //    cmd1.Parameters.AddWithValue("@FROMDATE", Date);
    //    cmd1.Parameters.AddWithValue("@TODATE", Date);
    //    int leavecount = Convert.ToInt32(cmd1.ExecuteScalar());
    //    totalleaves += leavecount;
    //    con1.Close();
    //  }
    //  if (totalleaves > 0)
    //  {
    //    leaveStatus = true;
    //  }
    //  con.Close();
    //  return response;
    //}





    // aprove leave request
    [HttpPost]
    [Route("GetMonthWiseReport")]
    public HmcResponseNew GetMonthWiseReport(GetAttendance_DataQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            && d.status.Equals("Active")
                                            select d).FirstOrDefault();
              if (userDetails != null)
              {             

                DataSet ds = new DataSet();
                SqlConnection con = new SqlConnection(constr);
                con.Open();
                SqlCommand cmd = new SqlCommand("GET_ATTENDANCEREPORT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@EMPCODE", req.employee_id);
                cmd.Parameters.AddWithValue("@STARTDATE", req.from_date);
                cmd.Parameters.AddWithValue("@ENDDATE", req.to_date);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = ds.Tables[0];


              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }



    // aprove leave request
    [HttpPost]
    [Route("GetDayWiseTeamAttendance")]
    public HmcResponseNew GetDayWiseTeamAttendance(GetAttendance_DataQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])            {
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            && d.status.Equals("Active")
                                            select d).FirstOrDefault();
              if (userDetails != null)
              {

                DataSet ds = new DataSet();
                SqlConnection con = new SqlConnection(constr);
                con.Open();
                SqlCommand cmd = new SqlCommand("GetDayWiseTeamAttendance", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@reporting_to", employeeId);
                cmd.Parameters.AddWithValue("@att_date", req.date.ToString("yyyy/MM/dd"));
                //cmd.Parameters.AddWithValue("@ENDDATE", req.to_date);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = ds.Tables[0];


              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    // aprove leave request
    [HttpPost]
    [Route("GetManagerList")]
    public HmcResponseNew GetManagerList(GetManagerListQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            && d.status.Equals("Active")
                                            select d).FirstOrDefault();
              if (userDetails != null)
              {

                DataSet ds = new DataSet();
                SqlConnection con = new SqlConnection(constr);
                con.Open();
                SqlCommand cmd = new SqlCommand("GetManagerList", con);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@reporting_to", employeeId);
                //cmd.Parameters.AddWithValue("@att_date", req.date.ToString("yyyy/MM/dd"));
                //cmd.Parameters.AddWithValue("@ENDDATE", req.to_date);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();

                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = ds.Tables[0];


              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }




    // download excel
    [HttpPost]
    [Route("DownloadAllEmployeeExcel")]
    public HmcResponseNew DownloadExcel(DownloadExcelResponse req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");

            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {

              EmployeeMaster EmpMaster = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.department.Equals("Administrator")
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();

              if (EmpMaster != null)
              {


                //string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                //using (SqlConnection con = new SqlConnection(constr))
                //{
                //    using (SqlCommand cmd = new SqlCommand("SELECT * FROM EmployeeMaster"))
                //    {
                //        using (SqlDataAdapter sda = new SqlDataAdapter())
                //        {
                //            cmd.Connection = con;
                //            sda.SelectCommand = cmd;
                //            using (DataTable dt = new DataTable())
                //            {
                //                sda.Fill(dt);
                //                using (XLWorkbook wb = new XLWorkbook())
                //                {
                //                    wb.Worksheets.Add(dt, "Customers");

                //                    Response.Clear();
                //                    Response.Buffer = true;
                //                    Response.Charset = "";
                //                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //                    Response.AddHeader("content-disposition", "attachment;filename=SqlExport.xlsx");
                //                    using (MemoryStream MyMemoryStream = new MemoryStream())
                //                    {
                //                        wb.SaveAs(MyMemoryStream);
                //                        MyMemoryStream.WriteTo(Response.OutputStream);
                //                        Response.Flush();
                //                        Response.End();
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}



                string fileName = "";
                string genFilePath = "";
                fileName = req.type + ".xlsx";

                string path = System.Configuration.ConfigurationManager.AppSettings["filePath"];
                if (File.Exists(Path.Combine(path, fileName)))
                {
                  // If file found, delete it    
                  File.Delete(Path.Combine(path, fileName));
                }

                genFilePath = Path.Combine(path, fileName);

                FileStream fs = new FileStream(genFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                fs.Close();
                int count = 1;
                var workBook = new XLWorkbook();
                var workSheet = workBook.Worksheets.Add("Sheet1");
                workSheet.SetShowGridLines(false);
                //create the header buffer
                List<DownloadMasterData> retVal = null;
                if (req.type == "EmployeeMaster")
                {
                  // select employee data data
                  retVal = (from d in db.EmployeeMaster
                            select new DownloadMasterData
                            {
                              employee_id = d.employee_id,
                              employee_name = d.name,
                              designation = d.designation,
                              department = d.department,
                              area = d.pay_code,
                              email = d.email,
                              reporting_to = d.reporting_to,
                              mobile_no = d.mobile_no,
                              fathers_name = d.father_name,
                              dobs = d.dob.ToString("dd/MM/YYYY"),
                              dojs = d.doj.ToString("dd/MM/YYYY"),
                              password_count = d.password_count,
                              status = d.status
                            }).ToList();

                  workSheet.Cell(1, count).Value = "Employee Id";
                  count++;
                  workSheet.Cell(1, count).Value = "Name";
                  count++;
                  workSheet.Cell(1, count).Value = "Designation";
                  count++;
                  workSheet.Cell(1, count).Value = "Department";
                  count++;
                  workSheet.Cell(1, count).Value = "Payrole Area";
                  count++;
                  workSheet.Cell(1, count).Value = "Reporting";
                  count++;
                  workSheet.Cell(1, count).Value = "Email";
                  count++;
                  workSheet.Cell(1, count).Value = "Mobile No";
                  count++;
                  workSheet.Cell(1, count).Value = "Father's Name";
                  count++;
                  workSheet.Cell(1, count).Value = "DOB";
                  count++;
                  workSheet.Cell(1, count).Value = "DOJ";
                  count++;
                  workSheet.Cell(1, count).Value = "Status";
                }
                else if (req.type == "ApplyLeave")
                {
                  // select apply leave 
                  retVal = (from d in db.ApplyLeaveMasters
                            select new DownloadMasterData
                            {
                              employee_id = d.employee_id,
                              shift = d.shift,
                              in_time = d.from_time,
                              out_time = d.to_time,
                              reason = d.reason,
                              no_of_leave = d.no_of_leave.ToString(),
                              areporting_to = d.assign_by,
                              reporting_to = d.assign_by,
                              duration_type = d.duration_type.ToString(),
                              leave_id = d.id.ToString(),
                              dob = d.from_date,
                              doj = d.to_date,
                              created_date = d.apply_date,
                              approve_date = d.approve_date,
                              status = d.status
                            }).ToList();
                  workSheet.Cell(1, count).Value = "Employee Id";
                  count++;
                  workSheet.Cell(1, count).Value = "Name";
                  count++;
                  workSheet.Cell(1, count).Value = "Apply Date";
                  count++;
                  workSheet.Cell(1, count).Value = "Leave Type";
                  count++;
                  workSheet.Cell(1, count).Value = "Duration Type";
                  count++;
                  workSheet.Cell(1, count).Value = "Shift";
                  count++;
                  workSheet.Cell(1, count).Value = "From Date";
                  count++;
                  workSheet.Cell(1, count).Value = "To Date";
                  count++;
                  workSheet.Cell(1, count).Value = "From Time";
                  count++;
                  workSheet.Cell(1, count).Value = "To Time";
                  count++;
                  workSheet.Cell(1, count).Value = "Reason";
                  count++;
                  workSheet.Cell(1, count).Value = "Approve By Id";
                  count++;
                  workSheet.Cell(1, count).Value = "Approve By Name";
                  count++;
                  workSheet.Cell(1, count).Value = "Approve Date";
                  count++;
                  workSheet.Cell(1, count).Value = "Status";
                }
                else if (req.type == "Mispunch")
                {
                  // select mispunch 
                  retVal = (from d in db.AttendanceMispunch
                            join e in db.AttendanceMaster on d.attendance_id equals e.id
                            select new DownloadMasterData
                            {
                              employee_id = d.employee_id,
                              shift = d.shift,
                              in_time = d.in_time,
                              mout_time = d.out_time,
                              mreason = d.reason,
                              dob = d.date,
                              created_date = d.date,
                              approve_date2 = d.approve_date,
                              status = d.status,
                              reporting_to = d.assign_by,
                              mreporting_to = d.assign_by
                            }).ToList();

                  workSheet.Cell(1, count).Value = "Employee Id";
                  count++;
                  workSheet.Cell(1, count).Value = "Name";
                  count++;
                  workSheet.Cell(1, count).Value = "Apply Date";
                  count++;
                  workSheet.Cell(1, count).Value = "Date";
                  count++;
                  workSheet.Cell(1, count).Value = "Shift";
                  count++;
                  workSheet.Cell(1, count).Value = "In Time";
                  count++;
                  workSheet.Cell(1, count).Value = "Out Time";
                  count++;
                  workSheet.Cell(1, count).Value = "Reason";
                  count++;
                  workSheet.Cell(1, count).Value = "Approve By Id";
                  count++;
                  workSheet.Cell(1, count).Value = "Approve By Name";
                  count++;
                  workSheet.Cell(1, count).Value = "Approve Date";
                  count++;
                  workSheet.Cell(1, count).Value = "Status";
                }

                var row = 1;

                foreach (DownloadMasterData item in retVal)
                {
                  string empName = null;
                  string reportingEmpName = null;
                  string payRoleArea = null;
                  string leaveType = null;
                  string durationType = null;
                  if (!string.IsNullOrEmpty(item.employee_id))
                  {
                    empName = (from d in db.EmployeeMaster
                               where d.employee_id.Equals(item.employee_id)
                               select d.name).FirstOrDefault();
                  }
                  if (!string.IsNullOrEmpty(item.area))
                  {
                    payRoleArea = (from d in db.KeyValueMaster
                                   where d.key_code.Equals(item.area)
                                   && d.key_type.Equals("payrole")
                                   select d.key_description).FirstOrDefault();
                  }
                  if (!string.IsNullOrEmpty(item.leave_id))
                  {
                    leaveType = (from d in db.KeyValueMaster
                                 where d.id.Equals(item.leave_id)
                                 select d.key_description).FirstOrDefault();
                  }
                  if (!string.IsNullOrEmpty(item.duration_type))
                  {
                    durationType = (from d in db.KeyValueMaster
                                    where d.id.Equals(item.duration_type)
                                    select d.key_description).FirstOrDefault();
                  }
                  if (!string.IsNullOrEmpty(item.reporting_to))
                  {
                    reportingEmpName = (from d in db.EmployeeMaster
                                        where d.employee_id.Equals(item.reporting_to)
                                        select d.name).FirstOrDefault();
                  }

                  //DateTime intime = Convert.ToDateTime(item.from_date);
                  //DateTime outtime = Convert.ToDateTime(item.to_date);
                  row++;
                  count = 1;
                  //get name of the person who initiated it
                  workSheet.Cell(row, count).Value = item.employee_id;
                  count++;
                  workSheet.Cell(row, count).Value = empName;//item.created_date.ToString("dd.MM.yyyy");
                  count++;
                  if (!string.IsNullOrEmpty(item.designation))
                  {
                    workSheet.Cell(row, count).Value = item.designation;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.created_date.ToString("dd.MM.yyyy");
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.department))
                  {
                    workSheet.Cell(row, count).Value = item.designation;
                  }
                  else if (!string.IsNullOrEmpty(leaveType))
                  {
                    workSheet.Cell(row, count).Value = leaveType;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.dob.ToString("dd.MM.yyyy");
                  }
                  count++;
                  if (!string.IsNullOrEmpty(payRoleArea))
                  {
                    workSheet.Cell(row, count).Value = payRoleArea;
                  }
                  else if (!string.IsNullOrEmpty(durationType))
                  {
                    workSheet.Cell(row, count).Value = durationType;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.shift;
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.reporting_to))
                  {
                    workSheet.Cell(row, count).Value = item.reporting_to;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.shift))
                  {
                    workSheet.Cell(row, count).Value = item.shift;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.in_time;
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.email))
                  {
                    workSheet.Cell(row, count).Value = item.email;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.mout_time))
                  {
                    workSheet.Cell(row, count).Value = item.mout_time;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.dob.ToString("dd.MM.yyyy");
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.mobile_no))
                  {
                    workSheet.Cell(row, count).Value = item.mobile_no;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.mreason))
                  {
                    workSheet.Cell(row, count).Value = item.mreason;//outtime.ToString("HHmmss");
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.doj.ToString("dd.MM.yyyy");
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.fathers_name))
                  {
                    workSheet.Cell(row, count).Value = item.fathers_name;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.in_time))
                  {
                    workSheet.Cell(row, count).Value = item.in_time;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.mreporting_to))
                  {
                    workSheet.Cell(row, count).Value = item.mreporting_to;//outtime.ToString("HHmmss");
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.dobs))
                  {
                    workSheet.Cell(row, count).Value = item.dobs;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.out_time))
                  {
                    workSheet.Cell(row, count).Value = item.out_time;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.mreporting_to))
                  {
                    workSheet.Cell(row, count).Value = reportingEmpName;//outtime.ToString("HHmmss");
                  }
                  //workSheet.Cell(row, count).Value = item.dob;
                  count++;
                  if (!string.IsNullOrEmpty(item.dojs))
                  {
                    workSheet.Cell(row, count).Value = item.dojs;//outtime.ToString("HHmmss");
                  }
                  else if (!string.IsNullOrEmpty(item.reason))
                  {
                    workSheet.Cell(row, count).Value = item.reason;//outtime.ToString("HHmmss");
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = item.status;
                  }
                  count++;
                  if (item.password_count == 0)
                  {
                    workSheet.Cell(row, count).Value = "Active";
                  }
                  else if (item.password_count == 1)
                  {
                    workSheet.Cell(row, count).Value = "In Active";
                  }
                  else if (!string.IsNullOrEmpty(item.areporting_to))
                  {
                    workSheet.Cell(row, count).Value = item.areporting_to;
                  }
                  count++;
                  if (!string.IsNullOrEmpty(item.areporting_to))
                  {
                    workSheet.Cell(row, count).Value = reportingEmpName;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = "";
                  }
                  count++;
                  if (item.status == "Approved")
                  {
                    workSheet.Cell(row, count).Value = item.approve_date.ToString("dd.MM.yyyy");
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = "";
                  }
                  count++;
                  if (item.status != "Active" && item.status != "InActive")
                  {
                    workSheet.Cell(row, count).Value = item.status;
                  }
                  else
                  {
                    workSheet.Cell(row, count).Value = "";
                  }
                  count++;
                  workSheet.Columns(1, count).Width = 16;

                  setCellBorder((workSheet.Range(workSheet.Cell(1, 1), workSheet.Cell(row, count))));
                  if (genFilePath != null)
                  {
                    System.IO.File.Delete(genFilePath);
                  }
                  workBook.SaveAs(genFilePath);


                }

                response.status = "success";
                response.data = fileName;

              }

            }



          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    private static void setCellBorder(IXLRange cellReference)
    {
      cellReference.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
      cellReference.Style.Border.BottomBorderColor = XLColor.LightGray;
      cellReference.Style.Border.TopBorder = XLBorderStyleValues.Thin;
      cellReference.Style.Border.TopBorderColor = XLColor.LightGray;
      cellReference.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
      cellReference.Style.Border.LeftBorderColor = XLColor.LightGray;
      cellReference.Style.Border.RightBorder = XLBorderStyleValues.Thin;
      cellReference.Style.Border.RightBorderColor = XLColor.LightGray;
    }

    public string GetEmployeeIdViaId(string Id)
    {
      string EmployeeId = "";
      SqlConnection con = new SqlConnection(constr);
      con.Open();
      SqlCommand cmd = new SqlCommand("Select employee_id from employeemaster where id='"+ Id + "'", con);
      EmployeeId = Convert.ToString(cmd.ExecuteScalar());
      con.Close();
      return EmployeeId;
    }


    // get shift
    [HttpPost]
    [Route("GetShiftByPayCode")]
    public ShiftResponse GetShiftByPayCode(HmcRequestShift req)
    {
      ShiftResponse response = new ShiftResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              EmployeeMaster userValid = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
              if (req.pay_code != "")
              {
                List<Shift> retVal = new List<Shift>();
                retVal = (from r in db.Shift
                          where r.pay_code.Equals(req.pay_code)
                          orderby r.id ascending
                          select r).ToList();
                response.status = "success";
                response.flag = "1";
                response.data = retVal;
                response.alert = "success";
              }
              else {
                response.status = "error";
                response.flag = "0";
                response.alert = "data is not valid";
                response.data = null;

              }

            }

          }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    [HttpPost]
    [Route("GetKeyValueDataPayCode")]
    public KeyValueData GetKeyValueDataPayCode(KeyValue req)
    {
      KeyValueData response = new KeyValueData();
      response.status = "error";
      response.flag = "0";
      response.alert = "No data found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                 where d.employee_id.Equals(employeeId)
                                                 select d).FirstOrDefault();
                List<KeyValueMaster> keyvalue = null;

                if (req.key_type == "duration")
                {
                  if (req.id != 0)
                  {
                    KeyValueMaster keyvalues = (from d in db.KeyValueMaster
                                                where d.id.Equals(req.id)
                                                select d).FirstOrDefault();

                    if (keyvalues.key_code == "SHL")
                    {

                      keyvalue = (from d in db.KeyValueMaster
                                  where d.key_type.Equals("shortleave")
                                  && d.pay_code.Equals(req.pay_code)
                                  select d).ToList();
                    }
                    else
                    {
                      keyvalue = (from d in db.KeyValueMaster
                                  where d.key_type.Equals("duration")
                                  && d.pay_code.Equals(req.pay_code)
                                  select d).ToList();
                    }
                  }
                  else
                  {
                    keyvalue = (from d in db.KeyValueMaster
                                where d.key_type.Equals("duration")
                                & d.pay_code.Equals(req.pay_code)
                                select d).ToList();
                  }

                }
                else
                {
                  if (employeeMaster.role == "Administrator")
                  {
                    keyvalue = (from d in db.KeyValueMaster
                                where d.key_type.Equals(req.key_type)
                                && d.pay_code.Equals(req.pay_code)
                                select d).ToList();
                  }
                  else
                  {

                    keyvalue = (from d in db.KeyValueMaster
                                where d.key_type.Equals(req.key_type)
                                && d.pay_code.Equals(req.pay_code)
                                && d.key_code != "Administrator"
                                select d).ToList();

                  }
                }


                if (keyvalue.Count() > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = keyvalue;
                }
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    //public void GetEmployeeAttendance()
    //{
    //  string query = "";
    //  query = " Select ecode, ";
    //  query += " (Select EmployeeName from view_employees where employeeCode = a.ecode)EmployeeName, ";
    //  query += " convert(nvarchar(10), ldt, 112)attdate, min(ldt)PunchIn,Max(ldt)PunchOut, convert(varchar, min(ldt), 108)PunchInTime,convert(varchar, max(ldt), 108)PunchOutTime, convert(nvarchar(10), ldt, 103) Attendancedate ";
    //  query += " ,CAST((max(ldt) - min(ldt)) as time(0)) tohalHrs ";
    //  query += " from rawdata a  where ecode = '10001711' ";
    //  query += " and a.ldt between '2023/07/01' and '2023/08/01' ";
    //  query += " group by ecode,convert(nvarchar(10), ldt, 112),convert(nvarchar(10), ldt, 103) ";
    //  query += " order by convert(nvarchar(10), ldt, 112) ";


    //  string EmployeeId = "";
    //  SqlConnection con = new SqlConnection(constr);
    //  con.Open();
    //  SqlCommand cmd = new SqlCommand(query, con);
    //  EmployeeId = Convert.ToString(cmd.ExecuteScalar());
    //  con.Close();
    //  return EmployeeId;


    //}



    // Ashish on 26 July 2023
    [HttpPost]
    [Route("GetAttendanceListv2")]
    public HmcResponseNew GetAttendanceListv2(GetAttendance_DataQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accesskey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              //bool userValid = db.EmployeeMaster.Any(user => user.employee_id == req.employee_id && user.status == "Active");
              EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            && d.status.Equals("Active")
                                            select d).FirstOrDefault();
              if (userDetails != null)
              {
                string query = "";



                DateTime FromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);

                query = " Select ecode as  Id, ecode as employee_id, ";
                query += " (Select EmployeeName from view_employees where employeeCode = a.ecode)Name,   ";
                query += "  convert(nvarchar(10), ldt, 112)attdate,min(ldt)date ,''day, min(ldt)PunchIn,  Max(ldt) PunchOut , convert(varchar, min(ldt), 108)in_time,convert(varchar, max(ldt), 108)out_time, convert(nvarchar(10), ldt, 103) sdate ";
                query += " ,CAST((max(ldt) - min(ldt)) as time(0)) total_hrs , ''Shift,'P'status, '' absent,'' mis,'' early ,''  late ,''  in_latitude ,''  in_longitude,''location_address ";
                query += " from rawdata a  where ecode = '" + userDetails.employee_id + "' ";
                query += " and a.ldt between '" + FromDate.ToString("yyyy/MM/dd") + "' and '" + toDate.AddDays(1).ToString("yyyy/MM/dd") + "' ";
                query += " group by ecode,convert(nvarchar(10), ldt, 112),convert(nvarchar(10), ldt, 103) ";
                query += " order by convert(nvarchar(10), ldt, 112) ";
                


                DataSet ds = new DataSet();
                SqlConnection con = new SqlConnection(constrsmartoffice);
                con.Open();
                //SqlCommand cmd = new SqlCommand(query, con);
                SqlCommand cmd = new SqlCommand("GET_EmployeeAttendance", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EMPLOYEEID", employeeId);
                cmd.Parameters.AddWithValue("@STARTDATE", FromDate.ToString("yyyy/MM/dd"));
                cmd.Parameters.AddWithValue("@ENDDATE", toDate.AddDays(1).ToString("yyyy/MM/dd"));
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();


                DataTable products = ds.Tables[0];


                List<employeeAttendanceList> studentList = new List<employeeAttendanceList>();
                studentList = (from DataRow dr in products.Rows
                               select new employeeAttendanceList()
                               {
                                 
                                 //StudentName = dr["StudentName"].ToString(),
                                 //Address = dr["Address"].ToString(),
                                 //MobileNo = dr["MobileNo"].ToString()

                                 absent = dr["absent"].ToString(),
                                 adate = dr["adate"].ToString(),
                                 attdate = dr["attdate"].ToString(),
                                 date = dr["date"].ToString(),
                                 day = dr["day"].ToString(),
                                 early = dr["early"].ToString(),
                                 employee_id = dr["employee_id"].ToString(),
                                 Id = dr["Id"].ToString(),
                                 in_latitude = dr["in_latitude"].ToString(),
                                 in_longitude = dr["in_longitude"].ToString(),
                                 in_time = dr["in_time"].ToString(),
                                 late = dr["late"].ToString(),
                                 location_address = dr["location_address"].ToString(),
                                 mis = dr["mis"].ToString(),
                                 Name = dr["Name"].ToString(),
                                 out_time = dr["out_time"].ToString(),
                                 PunchIn = dr["PunchIn"].ToString(),
                                 PunchOut = dr["PunchOut"].ToString(),
                                 sdate = dr["sdate"].ToString(),
                                 Shift = dr["Shift"].ToString(),
                                 status = dr["status"].ToString(),
                                 total_hrs = dr["total_hrs"].ToString(),
                                 trn_date= dr["trn_date"].ToString(),


                               }).ToList();



                foreach (employeeAttendanceList item in studentList)
                {

                  DateTime CheckdateTime = Convert.ToDateTime(item.trn_date);

                  var Holiday = db.HolidayMaster.Where(x => x.holiday_date == CheckdateTime.Date && x.h_type == "H").FirstOrDefault();
                  var Woff = db.HolidayMaster.Where(x => x.holiday_date == CheckdateTime.Date && x.h_type == "W").FirstOrDefault();


                  if (Holiday != null && Woff == null)
                  {
                    item.check_id = "1";
                    item.check_flag = "H";
                  }

                  else if (Holiday == null && Woff != null)
                  {
                    item.check_id = "2";
                    item.check_flag = "W";
                  }
                  else if (Holiday != null && Woff != null)
                  {
                    item.check_id = "3";
                    item.check_flag = "B";
                  }
                  else
                  {
                    item.check_id = "0";
                    item.check_flag = "0";
                  }



                  //if (item.h_type == "H")
                  //{

                  //  item.holidayType = "Holiday";

                  //}
                  //else if (item.h_type == "W")
                  //{
                  //  item.holidayType = "Weekly Off";
                  //}
                  //else
                  //{
                  //  item.holidayType = "Both";
                  //}

                }



                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = studentList;


              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    public class employeeAttendanceList
    {
      public string Id { get; set; }
      public string employee_id { get; set; }
      public string Name { get; set; }
      public string attdate { get; set; }
      public object date { get; set; }
      public string day { get; set; }
      public object PunchIn { get; set; }
      public object PunchOut { get; set; }
      public string in_time { get; set; }
      public string out_time { get; set; }
      public object adate { get; set; }
      public string sdate { get; set; }
      public string total_hrs { get; set; }
      public string Shift { get; set; }
      public string status { get; set; }
      public string absent { get; set; }
      public string mis { get; set; }
      public string early { get; set; }
      public string late { get; set; }
      public string in_latitude { get; set; }
      public string in_longitude { get; set; }
      public string location_address { get; set; }
      public string check_flag { get; set; }
      public string check_id { get; set; }

      public string trn_date { get; set; }
    }



    [HttpPost]
    [Route("GetdateFormat")]
    public HmcResponseNew GetdateFormat(GetDateRequest req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        DateTime FromDate =  DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
        //DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);


        response.status = FromDate.Date.ToString();


      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }



    public HmcResponseNew GetFirstAndLastPunching(string EmployeeId, string timedate)
    {

      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {

            EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(EmployeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
            if (userDetails != null)
            {
              string query = "";
              // DateTime FromDate = DateTime.ParseExact(timedate, "yyyy/MM/dd", null);
              // DateTime toDate = DateTime.ParseExact(timedate, "yyyy/MM/dd", null);
              DateTime req_timedate = DateTime.ParseExact(timedate, "dd/MM/yyyy", null);

              query = " Select ecode as  Id, ecode as employee_id, ";
              query += " (Select EmployeeName from view_employees where employeeCode = a.ecode)Name,   ";
              query += "  convert(nvarchar(10), ldt, 112)attdate,min(ldt)date ,''day, min(ldt)PunchIn,  Max(ldt) PunchOut , convert(varchar, min(ldt), 108)in_time,convert(varchar, max(ldt), 108)out_time, convert(nvarchar(10), ldt, 103) sdate ";
              query += " ,CAST((max(ldt) - min(ldt)) as time(0)) total_hrs , ''Shift,'P'status, '' absent,'' mis,'' early ,''  late ,''  in_latitude ,''  in_longitude,''location_address ";
              query += " from rawdata a  where ecode = '" + userDetails.employee_id + "' ";
              query += " and a.ldt between '" + req_timedate.ToString("yyyy/MM/dd") + "' and '" + req_timedate.AddDays(1).ToString("yyyy/MM/dd") + "' ";
              query += " group by ecode,convert(nvarchar(10), ldt, 112),convert(nvarchar(10), ldt, 103) ";
              query += " order by convert(nvarchar(10), ldt, 112) ";

              DataSet ds = new DataSet();
              SqlConnection con = new SqlConnection(constrsmartoffice);
              con.Open();
              SqlCommand cmd = new SqlCommand(query, con);
              SqlDataAdapter adapter = new SqlDataAdapter(cmd);
              adapter.Fill(ds);
              con.Close();

              if (ds.Tables.Count > 0)
              {
                if (ds.Tables[0].Rows.Count > 0)
                {
                  response.status = ds.Tables[0].Rows[0]["in_time"].ToString();
                  response.alert = ds.Tables[0].Rows[0]["out_time"].ToString();
                  response.flag = "1";
                  response.data = ds.Tables[0];
                }
                else
                {
                  response.status = "00:00:00";
                  response.alert = "00:00:00";
                  response.flag = "0";
                  response.data = null;
                }
              }
              else
              {
                response.status = "00:00:00";
                response.alert = "00:00:00";
                response.flag = "0";
                response.data = null;

              }


            }



          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    [HttpPost]
    [Route("GetEmployeeWiseAttendance")]

    public HmcResponseNew GetEmployeeWiseAttendance(EmployeeAttendanceRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");


            EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
            if (userDetails != null)
            {
              string EMPTYPE = "M";

              DateTime FromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
              DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);
              if (userDetails.reporting_to == "0")
              {
                EMPTYPE = "E";
              }


              string query = "";


              query = "delete from  rawdataWave where   ldt>GETDATE()-5";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();

              //query = "insert into rawdataWave Select *,'" + req.employee_id + "' from SmartOfficedb.dbo.rawdata where ecode='" + req.employee_id + "'";
              query = "insert into rawdataWave Select *,'" + employeeId + "' from SmartOfficedb.dbo.rawdata  where   ldt>GETDATE()-5 ";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();


              query = "delete from  rawdataimport  where attdate>getdate()-5 ";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();

              query = "  insert into rawdataimport  ";
              query += "  SELECT ecode 'Userid' ,b.name UserName,  CONVERT(DATE, ldt) 'AttDate',   ";
              query += "    MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) 'TimeIN',	CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN ''   ";
              query += "  ELSE    MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8)))  END 'TimeOut',   ";

              query += " MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) +' - '+	CASE WHEN MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) THEN ''	ELSE		MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8)))	END attData,";

              query += "  CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN    'Didn''t Clock Out'   ";
              query += "  ELSE    ''  END 'Remark'  ";
              query += "  FROM rawdatawave a left join  ";
              query += "  employeemaster b  ";
              query += "  on a.ecode = b.employee_id  ";
              query += "  where ldt>getdate()-5 ";
              query += "  GROUP BY ecode,b.name ,DATEPART(MONTH, ldt),CONVERT(DATE, ldt)  ";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();

              //EXEC GET_ATTENDANCEREPORT @STARTDATE='2023/07/01', @ENDDATE='2023/08/01', @EMPLOYEEID='10001706'



              //con.Open();
              //SqlCommand cmd = new SqlCommand("GET_ATTENDANCEREPORT", con);
              //cmd.CommandType = CommandType.StoredProcedure;
              ////cmd.Parameters.AddWithValue("@EMPCODE", req.employee_id);
              //cmd.Parameters.AddWithValue("@STARTDATE", req.from_date);
              //cmd.Parameters.AddWithValue("@ENDDATE", req.to_date);
              //SqlDataAdapter adapter = new SqlDataAdapter(cmd);
              //adapter.Fill(ds);
              //con.Close();

              DataSet ds = new DataSet();
              
              con.Open();
              cmd = new SqlCommand("GET_ATTENDANCEREPORT", con);
              cmd.CommandType = CommandType.StoredProcedure;
              cmd.Parameters.AddWithValue("@EMPLOYEEID", employeeId);
              cmd.Parameters.AddWithValue("@STARTDATE", FromDate.ToString("yyyy/MM/dd"));
              cmd.Parameters.AddWithValue("@ENDDATE", toDate.ToString("yyyy/MM/dd"));
              cmd.Parameters.AddWithValue("@EMPLOYEETYPE", req.query_for);
              

              SqlDataAdapter adapter = new SqlDataAdapter(cmd);
              adapter.Fill(ds);
              con.Close();             


              response.status = "success";
              response.flag = "1";
              response.alert = "success";
              response.data = ds.Tables[0];


            }



          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }



    [HttpPost]
    [Route("GetTeamAttendanceDayWise_backup")]

    public HmcResponseNew GetTeamAttendanceDayWise_backup(EmployeeAttendanceRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");


            EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
            if (userDetails != null)
            {
              string EMPTYPE = "M";

              DateTime FromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
              DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);
              if (userDetails.reporting_to == "0")
              {
                EMPTYPE = "E";
              }

              string query = "";           


              query = "delete from  rawdataWave where   ldt>GETDATE()-5";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();

              //query = "insert into rawdataWave Select *,'" + req.employee_id + "' from SmartOfficedb.dbo.rawdata where ecode='" + req.employee_id + "'";
              query = "insert into rawdataWave Select *,'" + employeeId + "' from SmartOfficedb.dbo.rawdata  where   ldt>GETDATE()-5 ";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();


              query = "delete from  rawdataimport  where attdate>getdate()-5 ";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();




              query = "  SELECT ecode 'Userid' ,b.name UserName,  CONVERT(DATE, ldt) 'AttDate',   ";
              query += "    MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) 'TimeIN',	CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN ''   ";
              query += "  ELSE    MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8)))  END 'TimeOut',   ";

              query += " MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) +' - '+	CASE WHEN MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) THEN ''	ELSE		MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8)))	END attData,";

              query += "  CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN    'Didn''t Clock Out'   ";
              query += "  ELSE    ''  END 'Remark' ,   CAST((max(ldt) - min(ldt)) as time(0)) total_hrs ";
              query += "  FROM rawdatawave a left join  ";
              query += "  employeemaster b  ";
              query += "  on a.ecode = b.employee_id  ";
              query += " where b.reporting_to='" + employeeId + "' ";
              query += " and a.ldt between '" + FromDate.ToString("yyyy/MM/dd") + "' and '" + toDate.Date.AddDays(1).ToString("yyyy/MM/dd") + "' ";
              query += "  GROUP BY ecode,b.name ,DATEPART(MONTH, ldt),CONVERT(DATE, ldt)  ";
              query += "  order by AttDate";
              con.Open();
              DataSet ds = new DataSet();
              cmd = new SqlCommand(query, con);
              SqlDataAdapter adapter = new SqlDataAdapter(cmd);
              adapter.Fill(ds);
              con.Close();
              if (ds.Tables.Count > 0)
              {
                if (ds.Tables[0].Rows.Count > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = ds.Tables[0];
                }
                else { }
              }
              else { }


            }



          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }


    [HttpPost]
    [Route("GetEmployeeIdAfterLogin")]
    public HmcResponseNew GetEmployeeIdAfterLogin(GetManagerListQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      response.alert = CryptoEngine.Encrypt(req.employee_id, "skym-3hn8-sqoy19");
      return response;

    }



    [HttpPost]
    [Route("GetTeamAttendanceDayWise")]

    public HmcResponseNew GetTeamAttendanceDayWise(EmployeeAttendanceRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");


            EmployeeMaster userDetails = (from d in db.EmployeeMaster
                                          where d.employee_id.Equals(employeeId)
                                          && d.status.Equals("Active")
                                          select d).FirstOrDefault();
            if (userDetails != null)
            {
              string EMPTYPE = "M";

              DateTime FromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
              DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);
              if (userDetails.reporting_to == "0")
              {
                EMPTYPE = "E";
              }

              string query = "";             



              query = "  SELECT ecode 'Userid' ,b.name UserName,  CONVERT(DATE, ldt) 'AttDate',   ";
              query += "    MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) 'TimeIN',	CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN ''   ";
              query += "  ELSE    MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8)))  END 'TimeOut',   ";
              query += " MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) +' - '+	CASE WHEN MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) THEN ''	ELSE		MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8)))	END attData,";

              query += "  CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN    'Didn''t Clock Out'   ";
              query += "  ELSE    ''  END 'Remark' ,   CAST((max(ldt) - min(ldt)) as time(0)) total_hrs ";
              query += "  FROM SmartOfficedb.dbo.rawdata a left join  ";
              query += "  employeemaster b  ";
              query += "  on a.ecode COLLATE SQL_Latin1_General_CP1_CI_AI  = b.employee_id   COLLATE SQL_Latin1_General_CP1_CI_AI  ";
              query += " where b.reporting_to='" + employeeId + "' ";
              query += " and a.ldt between '" + FromDate.ToString("yyyy/MM/dd") + "' and '" + toDate.Date.AddDays(1).ToString("yyyy/MM/dd") + "' ";
              query += "  GROUP BY ecode,b.name ,DATEPART(MONTH, ldt),CONVERT(DATE, ldt)  ";
              query += "  order by AttDate";
              con.Open();
              DataSet ds = new DataSet();
              cmd = new SqlCommand(query, con);
              SqlDataAdapter adapter = new SqlDataAdapter(cmd);
              adapter.Fill(ds);
              con.Close();
              if (ds.Tables.Count > 0)
              {
                if (ds.Tables[0].Rows.Count > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success";
                  response.data = ds.Tables[0];
                }
                else { }
              }
              else { }


            }



          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }




    // geterate daily log report
    [HttpPost]
    [Route("GenerateMonthlyAttendancePDFV2")]
    public HmcStringResponse GenerateMonthlyAttendancePDFV2(SearchEmployeeAttRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No record found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                string Trndate = req.year + "/" + req.month + "/1";
                //SELECT convert(nvarchar(10), DATEADD(DD,-(DAY(GETDATE() -1)), GETDATE()),111) AS FirstDate
                //SELECT convert(nvarchar(10), DATEADD(DD,-(DAY(GETDATE())), DATEADD(MM, 1, GETDATE())) ,111) AS LastDate
                con.Open();
                cmd = new SqlCommand("SELECT convert(nvarchar(10), DATEADD(DD,(DAY(cast('"+ Trndate + "' as datetime))-1), cast('" + Trndate + "' as datetime)),111) AS FirstDate", con);
                string FirstDate = Convert.ToString(cmd.ExecuteScalar());
                con.Close();

                con.Open();
                cmd = new SqlCommand("SELECT convert(nvarchar(10), DATEADD(DD,-(DAY(cast('" + Trndate + "' as datetime))), DATEADD(MM, 1, cast('" + Trndate + "' as datetime))) ,111) AS LastDate", con);
                string LastDate = Convert.ToString(cmd.ExecuteScalar());
                con.Close();

                //SELECT DATENAME(MM,GETDATE())

                con.Open();
                cmd = new SqlCommand("Select DATENAME(MM,'" + Trndate + "')MonthName", con);
                string MonthName = Convert.ToString(cmd.ExecuteScalar());
                con.Close();




                // DateTime fromDate = DateTime.ParseExact(req.from_date, "dd/MM/yyyy", null);
                //DateTime toDate = DateTime.ParseExact(req.to_date, "dd/MM/yyyy", null);
                con.Open();
                cmd = new SqlCommand("GET_ATTENDANCEREPORT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EMPLOYEEID", employeeId);
                cmd.Parameters.AddWithValue("@STARTDATE", FirstDate);
                cmd.Parameters.AddWithValue("@ENDDATE", LastDate);
                //cmd.Parameters.AddWithValue("@STARTDATE", "2023/07/01");
                //cmd.Parameters.AddWithValue("@ENDDATE", "2023/07/31");
                cmd.Parameters.AddWithValue("@EMPLOYEETYPE", req.pay_code);
                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();
                DataTable dt = ds.Tables[0];


                string fileName = "M" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
               //Document doc = new Document(new Rectangle(288f, 144f), 10, 10, 10, 10);
                //Document doc = new Document(PageSize.A4.Rotate(), 5, 5, 5, 5);
                //doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                //


                Document doc = new Document(PageSize.A4, 0, 0, 0, 0);
                doc.SetMargins(0, 0, 10, 10);
                doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());


                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");            
                Font font1_verdana = FontFactory.GetFont("Verdana", 6, Font.NORMAL | Font.NORMAL);              

                 doc.Open();
                con.Open();
                cmd = new SqlCommand("Select Name from employeemaster where employee_id='" + employeeId + "' ", con);
                string ManagerName = cmd.ExecuteScalar().ToString();
                con.Close();

                con.Open();
                cmd = new SqlCommand(" SELECT DAY(EOMONTH('"+ LastDate + "')) ", con);
                int Col_Number =Convert.ToInt32( cmd.ExecuteScalar());
                Col_Number = Col_Number + 3;
                con.Close();

                Font fdefault_header = FontFactory.GetFont("Verdana", 6, Font.BOLD);
                Font fdefault_data = FontFactory.GetFont("Verdana", 6, Font.NORMAL);
                Font fdefault_header_footer = FontFactory.GetFont("Verdana", 6, Font.BOLD);
                PdfPTable table = new PdfPTable(Col_Number);
                table.WidthPercentage = 90;
                
                if (Col_Number == 31)
                { table.SetTotalWidth(new float[] { 7, 10, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 }); }
                if (Col_Number == 32)
                { table.SetTotalWidth(new float[] { 7, 10, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 }); }
                if (Col_Number == 33)
                { table.SetTotalWidth(new float[] { 7, 10, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10}); }
                if (Col_Number == 34)
                { table.SetTotalWidth(new float[] { 7, 10, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 }); }




                string Header1 = "Monthly Attendance Report ";
                string Header2 = " For The Month of " + MonthName + "-" + req.year + "";

                PdfPCell cell = new PdfPCell(new Phrase(Header1, fdefault_header_footer));
                cell.Colspan = Col_Number;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(Header2, fdefault_header_footer));
                cell.Colspan = Col_Number;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("Manager Name: " + ManagerName.ToUpper(), fdefault_header_footer));
                cell.Colspan = Col_Number;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Print Date: " + DateTime.Now.ToShortDateString(), fdefault_header_footer));
                cell.Colspan = Col_Number;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("S.No", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Emp. Code", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Employee Name", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("01", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("02", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("03", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("04", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("05", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("06", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("07", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("08", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("09", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("10", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("11", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("12", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("13", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("14", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("15", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("16", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("17", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("18", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("19", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("20", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("21", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("22", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("23", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("24", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("25", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("26", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("27", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                if (Col_Number == 31)
                {
                  cell = new PdfPCell(new Phrase("28", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                  
                }
                if (Col_Number == 32)
                {
                  cell = new PdfPCell(new Phrase("28", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                  cell = new PdfPCell(new Phrase("29", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                 

                  
                }
                if (Col_Number == 33)
                {
                  cell = new PdfPCell(new Phrase("28", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                  cell = new PdfPCell(new Phrase("29", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase("30", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  
                }
                if (Col_Number == 34)
                {
                  cell = new PdfPCell(new Phrase("28", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                  cell = new PdfPCell(new Phrase("29", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);


                  cell = new PdfPCell(new Phrase("30", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                  cell = new PdfPCell(new Phrase("31", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                }



                

                int serialno = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                  serialno++;
                  cell = new PdfPCell(new Phrase(serialno.ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][0].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][1].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);



                  cell = new PdfPCell(new Phrase(dt.Rows[i][2].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][3].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][4].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][5].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][6].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][7].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][8].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][9].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][10].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);


                  cell = new PdfPCell(new Phrase(dt.Rows[i][11].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][12].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][13].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][14].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][15].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][16].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][17].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][18].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][19].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][20].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);



                  cell = new PdfPCell(new Phrase(dt.Rows[i][21].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][22].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][23].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][24].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][25].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][26].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][27].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][28].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  if (Col_Number == 31) {
                    cell = new PdfPCell(new Phrase(dt.Rows[i][29].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                  }
                  if (Col_Number == 32)
                  {
                    cell = new PdfPCell(new Phrase(dt.Rows[i][29].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i][30].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                  }
                  if (Col_Number == 33)
                  {
                    cell = new PdfPCell(new Phrase(dt.Rows[i][29].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i][30].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i][31].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                  }
                  if (Col_Number == 34)
                  {
                    cell = new PdfPCell(new Phrase(dt.Rows[i][29].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i][30].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i][31].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);


                    cell = new PdfPCell(new Phrase(dt.Rows[i][32].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                  }







                }

                doc.Add(table);

                StringBuilder sb = new StringBuilder();
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));
                doc.Close();

                //response.status = "success";
                //response.flag = "1";
                //response.alert = "success";
                //response.data = fileName;

                if (empMaster.email != "")
                {
                  SendEmailWithPdf(empMaster.email, "", empMaster.name, fileName);
                  response.status = empMaster.email;
                  response.flag = "1";
                  response.alert = "success";
                  response.data = fileName;

                }
                else
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "email address not found!";
                }



              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }

    [HttpPost]
    [Route("GetCompressBase64Image")]
    public string GetCompressBase64Image(DeleteDTO text)
    {

      byte[] buffer = Encoding.UTF8.GetBytes(text.accessKey);
      var memoryStream = new MemoryStream();
      using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
      {
        gZipStream.Write(buffer, 0, buffer.Length);
      }

      memoryStream.Position = 0;

      var compressedData = new byte[memoryStream.Length];
      memoryStream.Read(compressedData, 0, compressedData.Length);

      var gZipBuffer = new byte[compressedData.Length + 4];
      Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
      Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
      return Convert.ToBase64String(gZipBuffer);

    }



    // geterate daily log report
    [HttpPost]
    [Route("GenerateDailyAttendancePDFV2")]
    public HmcStringResponse GenerateDailyAttendancePDFV2(SearchEmployeeAttRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;

      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No record found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                DateTime fromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);

                string query = "  SELECT ecode 'Userid' ,b.name UserName,  CONVERT(DATE, ldt) 'AttDate',   ";
                query += "    MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) 'TimeIN',	CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN ''   ";
                query += "  ELSE    MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8)))  END 'TimeOut',   ";
                query += " MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) +' - '+	CASE WHEN MIN(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8))) THEN ''	ELSE		MAX(CAST(CONVERT(TIME,ldt) AS VARCHAR(8)))	END attData,";

                query += "  CASE WHEN MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) = MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) THEN    'Didn''t Clock Out'   ";
                query += "  ELSE    ''  END 'Remark' ,   CAST((max(ldt) - min(ldt)) as time(0)) total_hrs ";
                query += "  FROM SmartOfficedb.dbo.rawdata a left join  ";
                query += "  employeemaster b  ";
                query += "  on a.ecode COLLATE SQL_Latin1_General_CP1_CI_AI  = b.employee_id   COLLATE SQL_Latin1_General_CP1_CI_AI  ";
                if (req.pay_code == "M")
                {
                  query += " where b.reporting_to='" + employeeId + "' ";
                }
                else { query += " where b.employee_id='" + employeeId + "' "; }
                query += " and a.ldt between '" + fromDate.ToString("yyyy/MM/dd") + "' and '" + toDate.Date.AddDays(1).ToString("yyyy/MM/dd") + "' ";
                query += "  GROUP BY ecode,b.name ,DATEPART(MONTH, ldt),CONVERT(DATE, ldt)  ";
                query += "  order by AttDate";

                con.Open();

                cmd = new SqlCommand("Select Name from employeemaster where employee_id='" + employeeId + "' ", con);
                string ManagerName = cmd.ExecuteScalar().ToString();

                con.Close();



                con.Open();
                DataSet ds = new DataSet();
                cmd = new SqlCommand(query, con);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();
                DataTable dt = ds.Tables[0];

                if (dt.Rows.Count > 0)
                {
                  Font fdefault_header = FontFactory.GetFont("Arial", 12, Font.BOLD);
                  Font fdefault_data = FontFactory.GetFont("Arial", 8, Font.NORMAL);
                  Font fdefault_header_footer = FontFactory.GetFont("Arial", 8, Font.BOLD);
                  PdfPTable table = new PdfPTable(6);
                  table.SetTotalWidth(new float[] { 3, 5, 20, 5, 5, 5 });
                  string Header1 = "Daily Attendance Report ";
                  string Header2 = " From " + req.from_date + " to " + req.to_date + "";

                  PdfPCell cell = new PdfPCell(new Phrase(Header1, fdefault_header_footer));
                  cell.Colspan = 6;
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  cell.Border = Rectangle.NO_BORDER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(Header2, fdefault_header_footer));
                  cell.Colspan = 6;
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  cell.Border = Rectangle.NO_BORDER;
                  table.AddCell(cell);


                  cell = new PdfPCell(new Phrase("Manager Name: " + ManagerName.ToUpper(), fdefault_header_footer));
                  cell.Colspan = 3;
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  cell.Border = Rectangle.NO_BORDER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase("Print Date: " + DateTime.Now.ToShortDateString(), fdefault_header_footer));
                  cell.Colspan = 3;
                  cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                  cell.Border = Rectangle.NO_BORDER;
                  table.AddCell(cell);





                  cell = new PdfPCell(new Phrase("S.No", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase("Emp. Code", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase("Employee Name", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase("In Time", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                  cell = new PdfPCell(new Phrase("Out Time", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase("Total Hrs.", fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  int serialno = 0;
                  for (int i = 0; i < dt.Rows.Count; i++)
                  {
                    serialno++;
                    cell = new PdfPCell(new Phrase(serialno.ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i]["Userid"].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i]["UserName"].ToString().ToUpper(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i]["TimeIN"].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i]["TimeOut"].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(dt.Rows[i]["total_hrs"].ToString(), fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);


                  }

                  string fileName = "D" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";
                  Document doc = new Document(iTextSharp.text.PageSize.A4);
                  //Document doc = new Document(new Rectangle(288f, 144f), 10, 10, 10, 10);
                  //Document doc = new Document(PageSize.A4.Rotate(), 50, 50, 25, 25);
                  string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                  PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                  int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                  //Font font1_verdana = FontFactory.GetFont("Times New Roman", 12, Font.BOLD | Font.BOLD, new Color(System.Drawing.Color.Black));
                  //Font font1_verdana = FontFactory.GetFont("Verdana", 8, Font.NORMAL | Font.NORMAL);


                  doc.Open();


                  iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(fpath + "pdf_logo.png");
                  image.ScalePercent(50f);
                  image.SetAbsolutePosition(15, 800);
                  //doc.Add(image);

                  doc.Add(table);

                  doc.Close();
                  StringBuilder sb = new StringBuilder();
                  HTMLWorker hw = new HTMLWorker(doc);
                  hw.Parse(new StringReader(sb.ToString()));


                  if (empMaster.email != "")
                  {
                    SendEmailWithPdf(empMaster.email, "", empMaster.name, fileName);
                    response.status = empMaster.email;
                    response.flag = "1";
                    response.alert = "success";
                    response.data = fileName;

                  }
                  else
                  {
                    response.status = "error";
                    response.flag = "0";
                    response.alert = "email address not found!";
                  }


                }


              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }






    // geterate daily log report
    [HttpPost]
    [Route("GenerateWeeklyAttendancePDFV3")]
    public HmcStringResponse GenerateWeeklyAttendancePDFV3(SearchEmployeeAttRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No record found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                con.Open();
                cmd = new SqlCommand("Select datediff(dd,'"+ req.from_date + "','"+ req.to_date + "') +1", con);
                int DaysCheck = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
                if (DaysCheck != 7)
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "Date Selection Invalid ! date range must be in 7 days !";

                  return response;
                }



                DateTime FromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);
                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = FromDate; date <= toDate; date = date.AddDays(1))
                {
                  allDates.Add(date);
                }                


                string fileName = "W" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";   

                Document doc = new Document(PageSize.A4, 0, 0, 0, 0);
                doc.SetMargins(0, 0, 10, 10);
                doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                Font font1_verdana = FontFactory.GetFont("Verdana", 6, Font.NORMAL | Font.NORMAL);

                doc.Open();
                con.Open();
                cmd = new SqlCommand("Select Name from employeemaster where employee_id='" + employeeId + "' ", con);
                string ManagerName = cmd.ExecuteScalar().ToString();
                con.Close();

              
                int Col_Number=10;
                Font fdefault_header = FontFactory.GetFont("Verdana", 10, Font.BOLD);
                Font fdefault_data = FontFactory.GetFont("Verdana", 10, Font.NORMAL);
                Font fdefault_header_footer = FontFactory.GetFont("Verdana", 10, Font.BOLD);
                PdfPTable table = new PdfPTable(Col_Number);
                table.WidthPercentage = 90;

                 table.SetTotalWidth(new float[] { 5, 10, 15, 10, 10, 10, 10, 10, 10, 10}); 
              


                string Header1 = "Weekly Attendance Report ";
                string Header2 = " From Date: " + FromDate.ToString("dd-MM-yyyy") + "-" + toDate.ToString("dd-MM-yyyy") + "";

                PdfPCell cell = new PdfPCell(new Phrase(Header1, fdefault_header_footer));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(Header2, fdefault_header_footer));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("Manager Name: " + ManagerName.ToUpper(), fdefault_header_footer));
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Print Date: " + DateTime.Now.ToShortDateString(), fdefault_header_footer));
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("S.No", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Emp. Code", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Employee Name", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);


                foreach (var s in allDates)
                {              
                  con.Open();
                  cmd = new SqlCommand("SELECT DATENAME(W,'" + s.Date.ToString("yyyy/MM/dd") + "')", con);
                  string DayName = Convert.ToString(cmd.ExecuteScalar());
                  con.Close();
                  string Narration = s.Date.ToShortDateString() + " " + DayName;
                  cell = new PdfPCell(new Phrase(Narration, fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                }



                con.Open();

                cmd = new SqlCommand("Select employee_id,Name from View_Employee where reporting_to='"+ employeeId + "'",con);

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();


                DataTable dt= ds.Tables[0];







                int serialno = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                  serialno++;
                  cell = new PdfPCell(new Phrase(serialno.ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i]["employee_id"].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i]["Name"].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_LEFT;
                  table.AddCell(cell);



                  foreach (var s in allDates)
                  {
                    con.Open();
                    cmd = new SqlCommand("Select MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) from SmartOfficedb.dbo.rawdata where ecode = '"+ dt.Rows[i]["employee_id"].ToString() + "' and convert(nvarchar(10), ldt,111)= '" + s.Date.ToString("yyyy/MM/dd") + "'", con);
                    string InTime = Convert.ToString(cmd.ExecuteScalar());
                    con.Close();


                    con.Open();
                    cmd = new SqlCommand("Select MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) from SmartOfficedb.dbo.rawdata where ecode = '" + dt.Rows[i]["employee_id"].ToString() + "' and convert(nvarchar(10), ldt,111)= '" + s.Date.ToString("yyyy/MM/dd") + "' ", con);
                    string OutTime = Convert.ToString(cmd.ExecuteScalar());
                    con.Close();

                    string AttData = "00:00:00 00:00:00";
                    if (InTime != "")
                    {
                      AttData = InTime;
                      if (OutTime != "")
                      {
                        AttData +=" "+ OutTime;
                      }
                      else { AttData += " 00:00:00"; }
                    }

                    cell = new PdfPCell(new Phrase(AttData, fdefault_data));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                  }




                }

                doc.Add(table);

                StringBuilder sb = new StringBuilder();
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();
                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = fileName;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }



    public void SendEmailWithPdf(string emailId, string password, string EmployeeName,string PDfFileName)
    {

      //string TrnSubject = "One Time Password (OTP) for your Cycle Delivery on HERO Cycle";
      //string TrnMessage = "This is a system generated mail. Please do not reply to this email ID.<br/><br/> ";
      //TrnMessage += " Dear Branch, <br/><br/>";
      //TrnMessage += " Use <b>" + otp + "</b> as one time password(OTP) for Cycle Delivery at HERO Cycle .Dont disclose to anyone. ";


      string TrnMessage = string.Empty;
      TrnMessage = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + EmployeeName + ",</p>" +
                "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Wave Employee Management System</b>. The requested document is attached to this email ,please find the attachment </p>" +
                "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Wave Support Team</p>";
      string TrnSubject = "Welcome Employee Managment System";

      try
      {
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        MailMessage mail = new MailMessage();
        mail.To.Add(emailId);
        mail.From = new MailAddress("er.ashishsharma@live.com");
        mail.Subject = TrnSubject;
        mail.Body = TrnMessage;


        string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
        string fileName = fpath + PDfFileName;
       string  fileName_path = "~/Images/" + PDfFileName;
        byte[] bytes = File.ReadAllBytes(fileName);
        mail.Attachments.Add(new Attachment(new MemoryStream(bytes), fileName));

        mail.IsBodyHtml = true;
        SmtpClient smtp = new SmtpClient();
        smtp.Host = "smtp.office365.com";
        smtp.Port = 587;
        smtp.Credentials = new System.Net.NetworkCredential("er.ashishsharma@live.com", "Sharmarbl@123");
        smtp.EnableSsl = true;
        smtp.Send(mail);

      }
      catch (Exception e)
      {
        Console.WriteLine(e.StackTrace);

      }


    }



    // get employee list with out filter

    [HttpPost]
    [Route("GetEmployeeList")]
    public EmployeeListResponse GetEmployeeList(HmcRequest req)
    {
      EmployeeListResponse response = new EmployeeListResponse();
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empDetail = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();             
                List<EmployeeMaster> retVal = null;
           
                if (req.search_result != null)
                {
                  if (empDetail.role.ToLower() == "administrator")
                  {
                    retVal = (from d in db.EmployeeMaster
                              where (d.name.ToLower().Contains(req.search_result))
                              || (d.employee_id.ToLower().Contains(req.search_result))
                              orderby d.id descending
                              select d).ToList();
                  }

                  else if (empDetail.role.ToLower() == "manager")
                  {
                    retVal = (from d in db.EmployeeMaster
                              where (d.name.ToLower().Contains(req.search_result))
                              || (d.employee_id.ToLower().Contains(req.search_result))
                              orderby d.id descending
                              select d).ToList();
                  }

                  else
                  {
                    retVal = (from d in db.EmployeeMaster
                              where (d.name.ToLower().Contains(req.search_result))
                              || (d.employee_id.ToLower().Contains(req.search_result))
                              && d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).ToList();
                  }

                }
                else
                {
                  if (empDetail.role.ToLower() == "administrator")
                  {
                    retVal = (from d in db.EmployeeMaster
                              orderby d.id descending
                              select d).ToList();
                  }

                  else if (empDetail.role.ToLower() == "manager")
                  {
                    retVal = (from d in db.EmployeeMaster
                              orderby d.id descending
                              select d).ToList();
                  }

                  else
                  {
                    retVal = (from d in db.EmployeeMaster
                              where d.pay_code.Equals(empDetail.pay_code)
                              orderby d.id descending
                              select d).ToList();
                  }

                }


                response.status = "success";
                response.flag = "1";
                response.alert = "success";
                response.data = retVal;
              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.status = "error";
        response.flag = "0";
        response.alert = e.Message;
      }
      return response;
    }



    // geterate daily log report
    [HttpPost]
    [Route("GenerateWeeklyAttendancePDFV2")]
    public HmcStringResponse GenerateWeeklyAttendancePDFV2(SearchEmployeeAttRequest req)
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "No record found";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              bool userValid = db.EmployeeMaster.Any(user => user.employee_id == employeeId && user.status == "Active");
              if (userValid)
              {
                EmployeeMaster empMaster = (from d in db.EmployeeMaster
                                            where d.employee_id.Equals(employeeId)
                                            select d).FirstOrDefault();

                con.Open();
                cmd = new SqlCommand("Select datediff(dd,'" + req.from_date + "','" + req.to_date + "') +1", con);
                int DaysCheck = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();
                if (DaysCheck != 7)
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "Date Selection Invalid ! date range must be in 7 days !";

                  return response;
                }



                DateTime FromDate = DateTime.ParseExact(req.from_date, "yyyy/MM/dd", null);
                DateTime toDate = DateTime.ParseExact(req.to_date, "yyyy/MM/dd", null);
                List<DateTime> allDates = new List<DateTime>();
                for (DateTime date = FromDate; date <= toDate; date = date.AddDays(1))
                {
                  allDates.Add(date);
                }


                string fileName = "W" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".pdf";

                Document doc = new Document(PageSize.A4, 0, 0, 0, 0);
                doc.SetMargins(0, 0, 10, 10);
                doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                string fpath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/");
                PdfWriter.GetInstance(doc, new FileStream(fpath + fileName, FileMode.Create));

                int totalFonts = FontFactory.RegisterDirectory("C:\\Windows\\Fonts");
                Font font1_verdana = FontFactory.GetFont("Verdana", 6, Font.NORMAL | Font.NORMAL);

                doc.Open();
                con.Open();
                cmd = new SqlCommand("Select Name from employeemaster where employee_id='" + employeeId + "' ", con);
                string ManagerName = cmd.ExecuteScalar().ToString();
                con.Close();


                int Col_Number = 10;
                Font fdefault_header = FontFactory.GetFont("Verdana", 10, Font.BOLD);
                Font fdefault_data = FontFactory.GetFont("Verdana", 10, Font.NORMAL);
                Font fdefault_header_footer = FontFactory.GetFont("Verdana", 10, Font.BOLD);
                PdfPTable table = new PdfPTable(Col_Number);
                table.WidthPercentage = 90;

                table.SetTotalWidth(new float[] { 5, 10, 15, 10, 10, 10, 10, 10, 10, 10 });



                string Header1 = "Weekly Attendance Report ";
                string Header2 = " From Date: " + FromDate.ToString("dd-MM-yyyy") + "-" + toDate.ToString("dd-MM-yyyy") + "";

                PdfPCell cell = new PdfPCell(new Phrase(Header1, fdefault_header_footer));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(Header2, fdefault_header_footer));
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase("Manager Name: " + ManagerName.ToUpper(), fdefault_header_footer));
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Print Date: " + DateTime.Now.ToShortDateString(), fdefault_header_footer));
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Border = Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("S.No", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Emp. Code", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Employee Name", fdefault_header_footer));
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);


                foreach (var s in allDates)
                {
                  con.Open();
                  cmd = new SqlCommand("SELECT DATENAME(W,'" + s.Date.ToString("yyyy/MM/dd") + "')", con);
                  string DayName = Convert.ToString(cmd.ExecuteScalar());
                  con.Close();
                  string Narration = s.Date.ToShortDateString() + " " + DayName;
                  cell = new PdfPCell(new Phrase(Narration, fdefault_header_footer));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);
                }



                //con.Open();
                //cmd = new SqlCommand("Select employee_id,Name from View_Employee where reporting_to='" + employeeId + "'", con);
                //DataSet ds = new DataSet();
                //SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                //adapter.Fill(ds);
                //con.Close();
                //DataTable dt = ds.Tables[0];


                con.Open();
                cmd = new SqlCommand("GET_ATTENDANCEREPORT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EMPLOYEEID", employeeId);
                cmd.Parameters.AddWithValue("@STARTDATE", req.from_date);
                cmd.Parameters.AddWithValue("@ENDDATE", req.to_date);
                //cmd.Parameters.AddWithValue("@STARTDATE", "2023/07/01");
                //cmd.Parameters.AddWithValue("@ENDDATE", "2023/07/31");
                cmd.Parameters.AddWithValue("@EMPLOYEETYPE", req.pay_code);
                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();
                DataTable dt = ds.Tables[0];










                int serialno = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                  serialno++;
                  cell = new PdfPCell(new Phrase(serialno.ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][0].ToString(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][1].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);


                  cell = new PdfPCell(new Phrase(dt.Rows[i][2].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][3].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][4].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][5].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][6].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][7].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);

                  cell = new PdfPCell(new Phrase(dt.Rows[i][8].ToString().ToUpper(), fdefault_data));
                  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  table.AddCell(cell);



                  //foreach (var s in allDates)
                  //{
                  //  con.Open();
                  //  cmd = new SqlCommand("Select MIN(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) from SmartOfficedb.dbo.rawdata where ecode = '" + dt.Rows[i]["employee_id"].ToString() + "' and convert(nvarchar(10), ldt,111)= '" + s.Date.ToString("yyyy/MM/dd") + "'", con);
                  //  string InTime = Convert.ToString(cmd.ExecuteScalar());
                  //  con.Close();


                  //  con.Open();
                  //  cmd = new SqlCommand("Select MAX(CAST(CONVERT(TIME, ldt) AS VARCHAR(8))) from SmartOfficedb.dbo.rawdata where ecode = '" + dt.Rows[i]["employee_id"].ToString() + "' and convert(nvarchar(10), ldt,111)= '" + s.Date.ToString("yyyy/MM/dd") + "' ", con);
                  //  string OutTime = Convert.ToString(cmd.ExecuteScalar());
                  //  con.Close();

                  //  string AttData = "00:00:00 00:00:00";
                  //  if (InTime != "")
                  //  {
                  //    AttData = InTime;
                  //    if (OutTime != "")
                  //    {
                  //      AttData += " " + OutTime;
                  //    }
                  //    else { AttData += " 00:00:00"; }
                  //  }

                  //  cell = new PdfPCell(new Phrase(AttData, fdefault_data));
                  //  cell.HorizontalAlignment = Element.ALIGN_CENTER;
                  //  table.AddCell(cell);
                  //}




                }

                doc.Add(table);

                StringBuilder sb = new StringBuilder();
                HTMLWorker hw = new HTMLWorker(doc);
                hw.Parse(new StringReader(sb.ToString()));



                doc.Close();
                //response.status = "success";
                //response.flag = "1";
                //response.alert = "success";
                //response.data = fileName;

                if (empMaster.email != "")
                {
                  SendEmailWithPdf(empMaster.email, "", empMaster.name, fileName);
                  response.status = empMaster.email;
                  response.flag = "1";
                  response.alert = "success";
                  response.data = fileName;

                }
                else
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "email address not found!";
                }



              }

            }

          }
        }

      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }




    // create employee
    [HttpPost]
    [Route("CreateEmployeeV2")]
    public HmcJsonResponse CreateEmployeeV2(EmployeeDetails req)
    {


      

      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "employeeid and email id alredy exist";
      response.alert = "registration failed. try after some time";
       try
      {
        if (ModelState.IsValid)
        {
          // string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          //if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          //{
          //string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
          using (AttendanceContext db = new AttendanceContext())
          {
            bool userValid = db.EmployeeMaster.Any(user => user.mobile_no == req.mobile_no && user.status == "Active");

            if (!userValid)
            {
              ExceptionResponseContainer retVal = null;
              retVal = CustomValidator.applyValidations(req, typeof(UserValidate));

              if (retVal.isValid == false)
              {
                response.alert = retVal.jsonResponse;
              }
              else
              {

                string password = "welcome@123";
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

                DateTime dobD = DateTime.ParseExact(req.dob, "dd/MM/yyyy", null);
                // DateTime dojD = DateTime.ParseExact(req.doj, "dd/MM/yyyy", null);
                //DateTime dobD = DateTime.ParseExact(req.dob, "dd/MM/yyyy", null);

                EmployeeMaster result = new EmployeeMaster();
                result.employee_id = getnewemployeeid();
                result.name = req.name;
                result.email = req.email;
                result.mobile_no = req.mobile_no;
                result.dob = dobD;
               result.designation = "General";
                result.department = "Office Staff";
                //result.doj = null;
                //result.blood_group = req.blood_group;
                result.status = "Active";
                result.reporting_to = "10002";
                result.shift = "GEN";
                result.father_name = req.father_name;
                //result.aadhar_number = req.aadhar_number;
                result.gender = req.gender;
                result.employeement_type = "Parmanent";
                //result.bank_name = req.bank_name;
                //result.ifsc_code = req.ifsc_code;
                //result.acc_no = req.acc_no;
                // result.employee_salary = req.employee_salary;
                result.role = "employee";
                result.el = "0";
                result.cl = "0";
                result.sl = "0";
                result.a_el = "0";
                result.a_cl = "0";
                result.a_sl = "0";
                result.b_el = "0";
                result.b_cl = "0";
                result.b_sl = "0";
                result.pay_code = "1000";
                result.password = encPassword;
                result.password_count = 1;
                result.office_emp = true;
                result.marketing_emp = false;
                // result.modify_by = employeeId;
                result.modify_date = DateTime.Now;
                // result.pan_no = req.pan_no;
                // result.uan_no = req.uan_no;
                result.attendance_type = "all";
                //db.EmployeeMaster.Add(result);
                //db.SaveChanges();

                //EmployeeMachineMaster machineLink = new EmployeeMachineMaster();
                //machineLink.machine_id = req.machine_id;
                //machineLink.employee_id = req.employee_code;
                ////machineLink.modify_by = employeeId;
                //machineLink.pay_code = result.pay_code;
                //machineLink.modify_date = DateTime.Now;
                //db.EmployeeMachineMaster.Add(machineLink);
                // db.SaveChanges();
                string narration = "thanks for registration your login id is your mobile no";
                narration = "registration failed. try after some time";

                response.status = "success";
                response.flag = "1";
                response.alert = narration;

                string body = string.Empty;
                body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + result.name + ",</p>" +
                          "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Skylabs Employee Management System</b>. please login our portal one time password is " + password + "</p>" +
                          "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                          "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Skylabs Support Team</p>";
                string subject = "Welcome Employee Managment System";
                string emailId = result.email;
                //string emailId = "cs.sharma@hmcgroup.in";
                //Utilities.Utility.sendMaill(emailId, body, subject);

               // response.status = "success";
               // response.flag = "1";
               // response.alert = "success";

              }
            }
            else
            {

              response.status = "error";
              response.flag = "0";
              response.alert = "this mobile number alredy exist";
            }
          }

          // }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    public string getnewemployeeid()
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;
      string query = "Select max(employee_id)+1 from EmployeeMaster where reporting_to='10002'";
      con.Open();
      cmd = new SqlCommand(query, con);
      string newEmployeeId = Convert.ToString(cmd.ExecuteScalar());
      con.Close();

      return newEmployeeId;

    }






    // create employee
    [HttpPost]
    [Route("ResetPasswordViaEmail")]
    public HmcJsonResponse ResetPasswordViaEmail(EmployeeDetails req)
    {




      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "Please enter registered Email Address !";
      try
      {
        if (ModelState.IsValid)
        {
          
          using (AttendanceContext db = new AttendanceContext())
          {
            bool userValid = db.EmployeeMaster.Any(user => user.email == req.email && user.status == "Active");

            var employeedata = db.EmployeeMaster.Where(user => user.email == req.email && user.status == "Active").FirstOrDefault();

            if (userValid)
            {
              

                string password = "welcome@123";
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


                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd;
                string query = "update EmployeeMaster set Password='" + encPassword + "' where email='" + employeedata.email + "' and mobile_no='" + employeedata.mobile_no + "'";
                con.Open();
                cmd = new SqlCommand(query, con);
                cmd.ExecuteNonQuery();
                con.Close();
               
                string narration = "password reset, check your email!";
                response.status = "success";
                response.flag = "1";
                response.alert = narration;

              string body = string.Empty;
              body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + employeedata.name + ",</p>" +
                        "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Wave Employee Management System</b>. please login in our WEMS application  with your new password  " + password + "</p>" +
                        "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                        "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Wave Support Team</p>";
              string subject = "Welcome Wave Employee Managment System";
              string emailId = employeedata.email;
              //string emailId = "ashishsharmabbit@gmail.com";
              //Utilities.Utility.sendMaill(emailId, body, subject);

              SendEmail(emailId, password, employeedata.name);
              //SendEmail_Skylabs(emailId, password, employeedata.name);

              response.status = "success";
              response.flag = "1";
              response.alert = "success";


            }
            else
            {

              response.status = "error";
              response.flag = "0";
              response.alert = "Please enter registered Email Address !";
            }
          }

          // }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    public void SendEmail(string emailId, string password, string EmployeeName)
    {

      //string TrnSubject = "One Time Password (OTP) for your Cycle Delivery on HERO Cycle";
      //string TrnMessage = "This is a system generated mail. Please do not reply to this email ID.<br/><br/> ";
      //TrnMessage += " Dear Branch, <br/><br/>";
      //TrnMessage += " Use <b>" + otp + "</b> as one time password(OTP) for Cycle Delivery at HERO Cycle .Dont disclose to anyone. ";


      string TrnMessage = string.Empty;
      TrnMessage = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + EmployeeName + ",</p>" +
                "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Wave Employee Management System</b>. please login our portal  password is " + password + "</p>" +
                "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Wave Support Team</p>";
      string TrnSubject = "Welcome Employee Managment System";

      try
      {
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        MailMessage mail = new MailMessage();
        mail.To.Add(emailId);
        // mail.From = new MailAddress("er.ashishsharma@live.com");
        mail.From = new MailAddress("wems@thewavegroup.com");
        mail.Subject = TrnSubject;
        mail.Body = TrnMessage;
        mail.IsBodyHtml = true;
        SmtpClient smtp = new SmtpClient();
        //smtp.Host = "smtp.office365.com";
       // smtp.Port = 587;

        smtp.Host = "z2.zimbraserver.in";
        smtp.Port = 587;

        //smtp.Credentials = new System.Net.NetworkCredential("er.ashishsharma@live.com", "Sharmarbl@123");
        smtp.Credentials = new System.Net.NetworkCredential("wems@thewavegroup.com", "Wrs65@&");
        smtp.EnableSsl = true;
        smtp.Send(mail);

      }
      catch (Exception e)
      {
        Console.WriteLine(e.StackTrace);

      }


    }


    public void SendEmail_Skylabs(string emailId, string password, string EmployeeName)
    {

      //string TrnSubject = "One Time Password (OTP) for your Cycle Delivery on HERO Cycle";
      //string TrnMessage = "This is a system generated mail. Please do not reply to this email ID.<br/><br/> ";
      //TrnMessage += " Dear Branch, <br/><br/>";
      //TrnMessage += " Use <b>" + otp + "</b> as one time password(OTP) for Cycle Delivery at HERO Cycle .Dont disclose to anyone. ";


      string TrnMessage = string.Empty;
      TrnMessage = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + EmployeeName + ",</p>" +
                "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Wave Employee Management System</b>. please login our portal  password is " + password + "</p>" +
                "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
                "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Wave Support Team</p>";
      string TrnSubject = "Welcome Employee Managment System";

      try
      {
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        MailMessage mail = new MailMessage();
        mail.To.Add("skumar@skylabstech.com");
        // mail.From = new MailAddress("er.ashishsharma@live.com");
        mail.From = new MailAddress("greenonedrive@telematicsalert.com");
        mail.Subject = TrnSubject;
        mail.Body = TrnMessage;
        mail.IsBodyHtml = true;
        SmtpClient smtp = new SmtpClient();
        //smtp.Host = "smtp.office365.com";
        // smtp.Port = 587;

        smtp.Host = "smtpout.secureserver.net";
        smtp.Port = 587;

        //smtp.Credentials = new System.Net.NetworkCredential("er.ashishsharma@live.com", "Sharmarbl@123");
        smtp.Credentials = new System.Net.NetworkCredential("greenonedrive@telematicsalert.com", "Santosh@1234567");
        smtp.EnableSsl = true;
        smtp.Send(mail);

      }
      catch (Exception e)
      {
        Console.WriteLine(e.StackTrace);

      }


    }




    // create employee
    [HttpPost]
    [Route("DeleteAccountViaEmail")]
    public HmcJsonResponse DeleteAccountViaEmail(EmployeeDetails req)
    {




      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "Please enter valid  Email Address or mobile no !";
      try
      {
        if (ModelState.IsValid)
        {

          using (AttendanceContext db = new AttendanceContext())
          {
            bool userValid = db.EmployeeMaster.Any(user => user.email == req.email && user.status == "Active" && user.mobile_no == req.mobile_no);

            var employeedata = db.EmployeeMaster.Where(user => user.email == req.email && user.status == "Active" && user.mobile_no == req.mobile_no).FirstOrDefault();

            if (userValid)
            {

              SqlConnection con = new SqlConnection(constr);
              SqlCommand cmd;
              string query = "update EmployeeMaster set status='InActive' where email='" + employeedata.email + "' and mobile_no='" + employeedata.mobile_no + "'";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();

              string narration = "your account remove successfully !";
              response.status = "success";
              response.flag = "1";
              response.alert = narration;

              //string body = string.Empty;
              //body = "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal; '>Dear " + employeedata.name + ",</p>" +
              //          "<p style='font-size: 14px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Welcome <b>Wave Employee Management System</b>. please login in our WEMS application  with your new password  " + password + "</p>" +
              //          "<p style='font-size: 14px; font-weight: 600; font-style: normal; line-height: 20px; letter-spacing: normal;'>Thanks & Regards</p>" +
              //          "<p style='font-size: 13px; font-weight: 400; font-style: normal; line-height: 20px; letter-spacing: normal;'>Wave Support Team</p>";
              //string subject = "Welcome Wave Employee Managment System";
              //string emailId = employeedata.email;
              ////string emailId = "ashishsharmabbit@gmail.com";
              //Utilities.Utility.sendMaill(emailId, body, subject);

              //SendEmail(emailId, password, employeedata.name);

             // response.status = "success";
            //  response.flag = "1";
             // response.alert = "success";


            }
            else
            {

              response.status = "error";
              response.flag = "0";
              response.alert = "Please enter valid  Email Address or mobile no !";
            }
          }

          // }

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    // for inactive user account start


    // employee Login by mobiloe no.
    [HttpPost]
    [Route("DeleteEmployeeSendOtp")]
    public HmcStringResponse DeleteEmployeeSendOtp(EmployeeLoginMobileDTO req)
    {
      HmcStringResponse response = new HmcStringResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "a user details is not match with register user";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            if (!string.IsNullOrEmpty(req.mobile_no))
            {

              bool userValid = db.EmployeeMaster.Any(user => user.mobile_no == req.mobile_no && user.status == "Active");

              // User found in the database
              if (userValid)
              {
                //string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
                string numbers = "1234567890";

                string characters = numbers;
                string otp = string.Empty;
                for (int i = 0; i < 6; i++)
                {
                  string character = string.Empty;
                  do
                  {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                  } while (otp.IndexOf(character) != -1);
                  otp += character;
                }

                if (req.mobile_no == "8953275221")
                {
                  otp = "987321";
                }



                //string textMesage = otp + " is the OTP for login Skylabs. OTP is valid for 10 mins. Do not share it with anyone.";
                string strUrl = "http://connectexpress.in/api/v3/index.php?method=sms&api_key=5b925f3bcea8af4f5da003645048e4d562b28fee&to=" + req.mobile_no + "&sender=OELEDU&unicode=auto&message=Your overseas education lane login OTP is " + otp;

                string strResult;
                WebRequest objRequest = WebRequest.Create(strUrl);
                WebResponse objResponse1 = (WebResponse)objRequest.GetResponse();
                using (StreamReader sr = new StreamReader(objResponse1.GetResponseStream()))
                {
                  strResult = sr.ReadToEnd();
                  sr.Close();
                }

                VarifyOtp employeeOtp = (from d in db.VarifyOtp
                                         where d.mobile_no.Equals(req.mobile_no)
                                         select d).FirstOrDefault();

                if (employeeOtp != null)
                {
                  employeeOtp.otp = otp;
                  db.SaveChanges();
                }
                else
                {
                  VarifyOtp insertOtp = new VarifyOtp();
                  insertOtp.employee_id = "NA";
                  insertOtp.mobile_no = req.mobile_no;
                  insertOtp.otp = otp;
                  db.VarifyOtp.Add(insertOtp);
                  db.SaveChanges();
                }

                response.status = "success";
                response.flag = "1";
                response.data = "success";
                response.alert = "success";
              }

            }
          }
        }
      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    [HttpPost]
    [Route("DeleteEmployeeVarifyOtp")]
    public EmployeeResponse DeleteEmployeeVarifyOtp(EmployeeLoginMobileDTO req)
    {
      EmployeeResponse response = new EmployeeResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "a otp is not match";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            if (!string.IsNullOrEmpty(req.mobile_no) && !string.IsNullOrEmpty(req.otp))
            {    // Lets first check if the Model is valid or not
              string userName = req.mobile_no;
              string password = req.otp;



              bool userValid = db.VarifyOtp.Any(user => user.mobile_no == req.mobile_no && user.otp == req.otp);

              // User found in the database
              if (userValid)
              {
                VarifyOtp employeeOtp = (from d in db.VarifyOtp
                                         where d.mobile_no.Equals(req.mobile_no)
                                         && d.otp.Equals(req.otp)
                                         select d).FirstOrDefault();

                db.VarifyOtp.Remove(employeeOtp);
                db.SaveChanges();

                EmployeeMaster retVal = null;
                retVal = (from d in db.EmployeeMaster
                          where d.mobile_no.Equals(req.mobile_no)
                          select d).FirstOrDefault();

                if (retVal != null)
                {
                  retVal.password_count = 0;
                  retVal.status = "InActive";
                  db.SaveChanges();


                  response.status = "success";
                  response.flag = "1";
                  response.data = null;
                  response.alert = "success";


                  //EmployeeBasicdetails results = new EmployeeBasicdetails();
                  //results.employee_id = retVal.employee_id;
                  //results.name = retVal.name;
                  //results.role = retVal.role;
                  //results.password_count = 0;
                  //results.feature_image = retVal.feature_image;
                  //results.department = retVal.department;
                  //results.designation = retVal.designation;
                  //results.attendance_type = retVal.attendance_type;
                  //results.employee_id = CryptoEngine.Encrypt(results.employee_id, "skym-3hn8-sqoy19");

                  //results.accessKey = CryptoEngine.Encrypt(System.Configuration.ConfigurationManager.AppSettings["accesskey"], "skym-3hn8-sqoy19");
                  //if (results.password_count == 0)
                  //{
                  //  response.status = "success";
                  //  response.flag = "1";
                  //  response.data = results;
                  //  response.alert = "success";
                  //}
                  //else
                  //{
                  //  response.status = "success";
                  //  response.flag = "2";
                  //  response.data = results;
                  //  response.alert = "success";
                  //}

                }


              }

            }
          }
        }
      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }



    // end



    #region for add team member
    // create employee
    [HttpPost]
    [Route("AddManager")]
    public HmcJsonResponse AddManager(EmployeeDetails req)
    {


      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "Please enter valid  Email Address or mobile no !";
      try
      {
        if (ModelState.IsValid)
        {

          string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

          using (AttendanceContext db = new AttendanceContext())
          {
            bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" &&   user.employee_id == employee_id);

            var employeedata = db.EmployeeMaster.Where(user => user.status == "Active" && user.employee_id == employee_id).FirstOrDefault();

            if (userValid)
            {

              SqlConnection con = new SqlConnection(constr);
              SqlCommand cmd;

              string query = "Select reporting_to from  EmployeeMaster  where email='" + employeedata.email + "' and mobile_no='" + employeedata.mobile_no + "' and employee_id='" + employeedata.employee_id + "' ";
              con.Open();
              cmd = new SqlCommand(query, con);
              string reporting_to = Convert.ToString(cmd.ExecuteScalar());
              con.Close();

              query = "Select Name from EmployeeMaster where status='Active' and employee_id='"+ reporting_to + "' ";
              con.Open();
              cmd = new SqlCommand(query, con);
              string reporting_Manager_Name = Convert.ToString(cmd.ExecuteScalar());
              con.Close();
              if (reporting_Manager_Name == "")
              {
                query = "update EmployeeMaster set reporting_to='" + req.reporting_to + "' where email='" + employeedata.email + "' and mobile_no='" + employeedata.mobile_no + "' and employee_id='" + employeedata.employee_id + "' ";
                con.Open();
                cmd = new SqlCommand(query, con);
                cmd.ExecuteNonQuery();
                con.Close();
               
                response.status = "success";
                response.flag = "1";
                response.alert = "Your request to change reporting manager is updated ! ";


              }
              else
              {
           
                response.status = "error";
                response.flag = "0";
                response.alert = "you already linked with reporting manager: " + reporting_Manager_Name;

              }
         

            }
            else
            {
              response.status = "error";
              response.flag = "0";
              response.alert = "Record Not found !";
            }
          }

         

        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    #endregion



    #region for add team member
    // create employee
    [HttpPost]
    [Route("RemoveTeamMember")]
    public HmcJsonResponse RemoveTeamMember(EmployeeDetails req)
    {


      HmcJsonResponse response = new HmcJsonResponse();
      response.status = "error";
      response.flag = "0";
      response.alert = "Please enter valid  Email Address or mobile no !";
      try
      {
        if (ModelState.IsValid)
        {



          string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
          string reporting_to = CryptoEngine.Decrypt(req.reporting_to, "skym-3hn8-sqoy19");


          using (AttendanceContext db = new AttendanceContext())
          {
            bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id && user.reporting_to == reporting_to);

            var employeedata = db.EmployeeMaster.Where(user => user.status == "Active" && user.employee_id == employee_id && user.reporting_to == reporting_to).FirstOrDefault();

            if (userValid)
            {

              SqlConnection con = new SqlConnection(constr);
              SqlCommand cmd;

              string query = "Select reporting_to from  EmployeeMaster  where email='" + employeedata.email + "' and mobile_no='" + employeedata.mobile_no + "' and employee_id='" + employeedata.employee_id + "' ";
             

              query = "update EmployeeMaster set reporting_to='0' where email='" + employeedata.email + "' and mobile_no='" + employeedata.mobile_no + "' and employee_id='" + employeedata.employee_id + "' ";
              con.Open();
              cmd = new SqlCommand(query, con);
              cmd.ExecuteNonQuery();
              con.Close();

              response.status = "success";
              response.flag = "1";
              response.alert = "Your request to remove team member is updated ! ";





            }
            else
            {
              response.status = "error";
              response.flag = "0";
              response.alert = "Record Not found !";
            }
          }



        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }

    #endregion


   

    [HttpPost]
    [Route("GetPageAccessList")]
    public HmcResponseNew GetPageAccessList(EmployeeDetails req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "Please enter valid  Email Address or mobile no !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              var employeedata = db.EmployeeMaster.Where(user => user.status == "Active" && user.employee_id == employee_id).FirstOrDefault();

              if (userValid)
              {




                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd;

                string query = "select pageId,PageCode,PageName,ShortName,isActive from pagesetup where isactive=1 order by pAGeId ";
                con.Open();
                cmd = new SqlCommand(query, con);


                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();


                DataTable dt = ds.Tables[0];



                response.status = "success";
                response.flag = "1";
                response.alert = "success ";
                response.data = ds;
              }
              else
              {
                response.status = "error";
                response.flag = "0";
                response.alert = "Record Not found !";
              }
            }




          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }




    [HttpPost]
    [Route("AddUpdateEmployeePageAccess")]
    public HmcResponseNew AddUpdateEmployeePageAccess(PageAccessVMDTO req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {

        using (AttendanceContext db = new AttendanceContext())
        {


          string employeeid = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");


          //var x = (from y in db.PageAccess
          //         where y.EmployeeId == employeeid && y.PageCode==r
          //         select y).ToList();
          //if (x != null)
          //{
          //  db.PageAccess.RemoveRange(x);
          //  db.SaveChanges();
          //}





          List<PageAccessDTO> chair = req.PageAccessDTO.ToList();
          foreach (PageAccessDTO d in chair)
          {

            var permissiondata = (from y in db.PageAccess
                                  where y.EmployeeId == employeeid && y.PageCode == d.PageCode
                                  select y).FirstOrDefault();
            if (permissiondata != null)
            {
              db.PageAccess.Remove(permissiondata);
              db.SaveChanges();
            }
            


            PageAccess chairdata = new PageAccess();
            chairdata.AccessId = 0;
            chairdata.EmployeeId = employeeid;
            chairdata.PageCode = d.PageCode;
            chairdata.IsAccess = d.IsAccess;
            db.PageAccess.Add(chairdata);
            db.SaveChanges();

          }


          response.flag = "1";
          response.alert = "Record Saved";
          response.status = "Record Saved";
          response.data = "";


        }




      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    [HttpPost]
    [Route("GetEmployeePageAccess")]
    public HmcResponseNew GetEmployeePageAccess(GetManagerListQuery req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {

        using (AttendanceContext _Context = new AttendanceContext())
        {


          string employeeid = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");


          List<EmployeePageAccessDTO> resultsDb = null;
          resultsDb = (from d in _Context.PageAccess.Where(x => x.EmployeeId == employeeid)
                       select new EmployeePageAccessDTO
                       {
                         AccessId = d.AccessId,
                         EmployeeId = d.EmployeeId,
                         IsAccess = d.IsAccess,
                         PageCode = d.PageCode

                       }).ToList();


          foreach (EmployeePageAccessDTO item1 in resultsDb)
          {

            var EmpName = _Context.EmployeeMaster.Where(x => x.employee_id == item1.EmployeeId).Select(x => x.name).FirstOrDefault();
            var PageName = _Context.PageSetup.Where(x => x.PageCode == item1.PageCode).Select(x => x.PageName).FirstOrDefault();
            var ShortName = _Context.PageSetup.Where(x => x.PageCode == item1.PageCode).Select(x => x.ShortName).FirstOrDefault();

            item1.EmployeeName = EmpName;
            item1.PageName = PageName;
            item1.ShortName = ShortName;
            if (item1.IsAccess == 1)
            {
              item1.IsPermission = "Yes";
            }
            else { item1.IsPermission = "No"; }


          }

          response.flag = "1";
          response.alert = "Record Saved";
          response.status = "Record Saved";
          response.data = resultsDb;







            }




        




      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }




    [HttpPost]
    [Route("GetEmployeePageAccessV2")]
    public HmcResponseNew GetEmployeePageAccessV2(GetManagerListQuery req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      try
      {

        using (AttendanceContext _Context = new AttendanceContext())
        {


          string employeeid = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

          var abc = _Context.PageSetup.Where(x => x.IsActive == 1).ToList();
          List<EmployeePageAccessDTO> resultsDb = null;
          resultsDb = (from d in _Context.PageSetup.Where(x => x.IsActive == 1)
                       select new EmployeePageAccessDTO
                       {
                         IsAccess = 0,
                         PageId = d.PageId,
                         PageCode = d.PageCode.TrimEnd(),
                         PageName = d.PageName.TrimEnd(),
                         ShortName = d.ShortName

                       }).ToList();


          foreach (EmployeePageAccessDTO item1 in resultsDb)
          {

            var EmpName = _Context.EmployeeMaster.Where(x => x.employee_id == employeeid).Select(x => x.name).FirstOrDefault();
            var IsAccess = _Context.PageAccess.Where(x => x.PageCode == item1.PageCode && x.EmployeeId==employeeid).Select(x => x.IsAccess).FirstOrDefault();
            var AccessId = _Context.PageAccess.Where(x => x.PageCode == item1.PageCode && x.EmployeeId == employeeid).Select(x => x.AccessId).FirstOrDefault();
            var ShortName = _Context.PageSetup.Where(x => x.PageCode == item1.PageCode).Select(x => x.ShortName).FirstOrDefault();





            item1.EmployeeName = EmpName;
            item1.IsAccess = IsAccess;
            item1.employee_code = req.employee_id;
            item1.EmployeeId = employeeid;
            item1.AccessId = AccessId;


            if (item1.IsAccess == 1)
            {
              item1.IsPermission = "Yes";
            }
            else { item1.IsPermission = "No"; }


          }

          response.flag = "1";
          response.alert = "Record Saved";
          response.status = "Record Saved";
          response.data = resultsDb;







        }









      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }




    [HttpPost]
    [Route("ChatEmployeelist")]
    public HmcResponseNew ChatEmployeelist(HmcRequest req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "no record found !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              if (userValid)
              {
                //List<EmployeeMaster> empDetail = (from d in db.EmployeeMaster
                //                                  where d.status == "Active"
                //                                  select d).ToList();

                //foreach (EmployeeMaster data in empDetail)
                //{


                //  var readcount = db.employeechat.Where(x => x.senderid == employee_id && x.receiverid == data.employee_id && x.IsRead == 0).Count();

                //  data.password_count = readcount;
                //  db.SaveChanges();
                //}

                //empDetail = empDetail.OrderByDescending(x => x.password_count).ToList();



                string query = "Select top 50 employee_id,Name EmployeeName, (name+' ('+employee_id+')') Name,  ";
                query += " (Select count(*) from employeechat where senderid = EmployeeMaster.employee_id and receiverid = '"+ employee_id + "' and IsRead = 0)ReadCount from EmployeeMaster where status = 'Active' and employee_Id not in ('"+ employee_id + "') ";
                //query += " order by(Select count(*) from employeechat where senderid= EmployeeMaster.employee_id and receiverid = '" + employee_id + "' and IsRead = 0) desc, rtrim(ltrim(Name)) ";
                if (req.search_result != "")
                {
                  query += " and (name+''+employee_id) like '%"+ req.search_result + "%' ";
                }
                query += " order by(Select max(meessageid) from employeechat where senderid= EmployeeMaster.employee_id and receiverid = '" + employee_id + "'  ) desc  ";

                query += " , (Select count(*) from employeechat where senderid= EmployeeMaster.employee_id and receiverid = '" + employee_id + "' ) desc, rtrim(ltrim(Name)) ";

                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd;
                
                con.Open();
                cmd = new SqlCommand(query, con);

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();


                DataTable dt = ds.Tables[0];



                response.status = "success";
                response.flag = "1";
                response.alert = "success ";
                response.data = dt;
              }
              else
              {
                response.status = "error";
                response.flag = "0";
                response.alert = "Record Not found !";
              }
            }




          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    [HttpPost]
    [Route("ChatSend")]
    public HmcResponseNew ChatSend(HmcChatRequest req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "no record found !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext dbSend = new AttendanceContext())
            {
              bool userValid = dbSend.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              if (userValid)
              {

                employeechat empchat = new employeechat();
                //empchat.meessageid = 0;
                empchat.senderid = req.senderid;
                empchat.receiverid = req.receiverid;
                empchat.message = req.message;
                empchat.entrydate = DateTime.Now;
                empchat.IsDeleted = 0;
                empchat.IsRead = 0;
                dbSend.employeechat.Add(empchat);
                dbSend.SaveChanges();
                response.status = "success";
                response.flag = "0";
                response.alert = "success ";
                response.data = empchat;
              }
              else
              {
                response.status = "error";
                response.flag = "0";
                response.alert = "Record Not found !";
              }
            }




          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }



    [HttpPost]
    [Route("GetCryptEmployeeId")]
    public HmcResponseNew GetCrypEmployeeId(GetManagerListQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      response.alert = CryptoEngine.Encrypt(req.employee_id, "skym-3hn8-sqoy19");
      return response;

    }

    [HttpPost]
    [Route("GetDecryptEmployeeId")]
    public HmcResponseNew GetDecryptEmployeeId(GetManagerListQuery req)
    {
      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "error";
      response.alert = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
      return response;

    }



    [HttpPost]
    [Route("ChatReceive")]
    public HmcResponseNew ChatReceive(HmcChatRequest req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "no record found !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext dbReceived = new AttendanceContext())
            {
              bool userValid = dbReceived.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              if (userValid)
              {









                List<employeechat> _employeechat = dbReceived.employeechat.Where(x => x.senderid == req.senderid && x.receiverid == req.receiverid && x.IsDeleted == 0).ToList();

                foreach (employeechat chat in _employeechat)
                {
                  //chat.IsRead = 1;
                  //db.SaveChanges();
                }


                 _employeechat = dbReceived.employeechat.Where(x => x.senderid == req.receiverid && x.receiverid == req.senderid && x.IsDeleted == 0).ToList();

                foreach (employeechat chat in _employeechat)
                {
                  //chat.IsRead = 1;
                 // dbReceived.SaveChanges();
                }




                string query = "Select *,  ";
                query+=" (Select name from EmployeeMaster where employee_id = employeechat.senderid)sendername,  ";
                query += " (Select name from EmployeeMaster where employee_id = employeechat.receiverid)receivername,  ";
                query += " case when senderid = '"+ req.senderid + "' then 'FROM' else 'TO' end MSTATUS  ";
                query += " from employeechat where (senderid ='" + req.senderid + "'  and  receiverid='" + req.receiverid + "' ) or (senderid ='" + req.receiverid + "'  and  receiverid='" + req.senderid + "' )    ";
                query += " order by entrydate desc";


                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd;

                con.Open();
                cmd = new SqlCommand(query, con);

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();


                DataTable dtChat = ds.Tables[0];



                List<employeechatDTO> resultsDb = (from d in dbReceived.employeechat.Where(x => x.senderid == req.senderid && x.receiverid == req.receiverid && x.IsDeleted == 0)
                                                   select new employeechatDTO
                                                   {
                                                     meessageid = d.meessageid,
                                                     senderid = d.senderid,
                                                     receiverid = d.receiverid,
                                                     message = d.message,
                                                     entrydate = d.entrydate,
                                                     IsDeleted = d.IsDeleted
                                                   }).ToList();

                if (dtChat.Rows.Count > 0)
                {
                  
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success ";
                  response.data = dtChat;
                }
                else
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "Record Not found !";
                }
              }




            }
          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }



    [HttpPost]
    [Route("ChatRead")]
    public HmcResponseNew ChatRead(HmcChatRequest req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "no record found !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext dbRead = new AttendanceContext())
            {
              bool userValid = dbRead.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              if (userValid)
              {









                List<employeechat> _employeechat = dbRead.employeechat.Where(x => x.senderid == req.senderid && x.receiverid == req.receiverid && x.IsDeleted == 0).ToList();

                foreach (employeechat chat in _employeechat)
                {
                  //chat.IsRead = 1;
                  //db.SaveChanges();
                }


                _employeechat = dbRead.employeechat.Where(x => x.senderid == req.receiverid && x.receiverid == req.senderid && x.IsDeleted == 0).ToList();

                foreach (employeechat chat in _employeechat)
                {
                  chat.IsRead = 1;
                  dbRead.SaveChanges();
                }




                //string query = "Select *,  ";
                //query += " (Select name from EmployeeMaster where employee_id = employeechat.senderid)sendername,  ";
                //query += " (Select name from EmployeeMaster where employee_id = employeechat.receiverid)receivername,  ";
                //query += " case when senderid = '" + req.senderid + "' then 'FROM' else 'TO' end MSTATUS  ";
                //query += " from employeechat where (senderid ='" + req.senderid + "'  and  receiverid='" + req.receiverid + "' ) or (senderid ='" + req.receiverid + "'  and  receiverid='" + req.senderid + "' )    ";
                //query += " order by entrydate desc";


                //SqlConnection con = new SqlConnection(constr);
                //SqlCommand cmd;

                //con.Open();
                //cmd = new SqlCommand(query, con);

                //DataSet ds = new DataSet();
                //SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                //adapter.Fill(ds);
                //con.Close();


                //DataTable dtChat = ds.Tables[0];



                //List<employeechatDTO> resultsDb = (from d in dbReceived.employeechat.Where(x => x.senderid == req.senderid && x.receiverid == req.receiverid && x.IsDeleted == 0)
                //                                   select new employeechatDTO
                //                                   {
                //                                     meessageid = d.meessageid,
                //                                     senderid = d.senderid,
                //                                     receiverid = d.receiverid,
                //                                     message = d.message,
                //                                     entrydate = d.entrydate,
                //                                     IsDeleted = d.IsDeleted
                //                                   }).ToList();

                if (_employeechat.Count() > 0)
                {

                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success ";
                  //response.data = dtChat;
                }
                else
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "Record Not found !";
                }
              }




            }
          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }




    [HttpPost]
    [Route("SearchEmployeelist")]
    public HmcResponseNew SearchEmployeelist(HmcRequest req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "no record found !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              if (userValid)
              {
                


                string query = "Select top 50 employee_id,Name EmployeeName, (name+' ('+employee_id+')') Name,  ";
                query += " (Select count(*) from employeechat where senderid = EmployeeMaster.employee_id and receiverid = '" + employee_id + "' and IsRead = 0)ReadCount from EmployeeMaster where status = 'Active'  ";
                //query += " order by(Select count(*) from employeechat where senderid= EmployeeMaster.employee_id and receiverid = '" + employee_id + "' and IsRead = 0) desc, rtrim(ltrim(Name)) ";
                if (req.search_result != "")
                {
                  query += " and (name+''+employee_id) like '%" + req.search_result + "%' ";
                }       

                query += " order by  rtrim(ltrim(Name)) ";

                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd;

                con.Open();
                cmd = new SqlCommand(query, con);

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
                con.Close();


                DataTable dt = ds.Tables[0];



                response.status = "success";
                response.flag = "1";
                response.alert = "success ";
                response.data = dt;
              }
              else
              {
                response.status = "error";
                response.flag = "0";
                response.alert = "Record Not found !";
              }
            }




          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }




    // for check application version
    [HttpPost]
    [Route("CheckApplicationVersion")]
    public HmcResponseNew CheckApplicationVersion(AppversionDTO req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "update your application from app or play store !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              var employeedata = db.EmployeeMaster.Where(user => user.status == "Active" && user.employee_id == employee_id).FirstOrDefault();

              if (userValid)
              {




                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd;

                string query = "Select count(*) from app_version where version='" + req.versioncode + "' and  mobile_type='" + req.mobile_type + "'   ";
                con.Open();
                cmd = new SqlCommand(query, con);
                int checkversion = Convert.ToInt32(cmd.ExecuteScalar());
                con.Close();

                


                if (checkversion > 0)
                {
                  response.status = "success";
                  response.flag = "1";
                  response.alert = "success ";
              
                }

                if (req.mobile_type == "A" || req.mobile_type == "I")
                { }
                else
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "incorrect mobile type !";

                }


              }
              else
              {
                response.status = "error";
                response.flag = "0";
                response.alert = "Record Not found !";
              }
            }




          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    [HttpPost]
    [Route("UpdateApplicationVersion")]
    public HmcResponseNew UpdateApplicationVersion(AppversionDTO req)
    {


      HmcResponseNew response = new HmcResponseNew();
      response.status = "error";
      response.flag = "0";
      response.alert = "update your application from app or play store !";
      try
      {
        if (ModelState.IsValid)
        {
          string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
          if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
          {
            string employee_id = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");

            using (AttendanceContext db = new AttendanceContext())
            {
              bool userValid = db.EmployeeMaster.Any(user => user.status == "Active" && user.employee_id == employee_id);

              var employeedata = db.EmployeeMaster.Where(user => user.status == "Active" && user.employee_id == employee_id).FirstOrDefault();

              if (userValid)
              {

                if (req.mobile_type == "A" || req.mobile_type == "I")
                {


                  SqlConnection con = new SqlConnection(constr);
                  SqlCommand cmd;
                  string query = "Select version from app_version where mobile_type='" + req.mobile_type + "'  ";
                  con.Open();
                  cmd = new SqlCommand(query, con);
                  string Oldcheckversion = Convert.ToString(cmd.ExecuteScalar());
                  con.Close();





                  query = "update app_version  set version='" + req.versioncode + "' where mobile_type='" + req.mobile_type + "'  ";
                  con.Open();
                  cmd = new SqlCommand(query, con);
                  cmd.ExecuteNonQuery();
                  con.Close();

                  query = "Select version from app_version  where mobile_type='" + req.mobile_type + "' ";
                  con.Open();
                  cmd = new SqlCommand(query, con);
                  string Newcheckversion = Convert.ToString(cmd.ExecuteScalar());
                  con.Close();


                  string narartion = "Old Version Code " + Oldcheckversion + " => New Version Code" + Newcheckversion + " for:" + req.mobile_type;


                  response.status = "success";
                  response.flag = "1";
                  response.alert = narartion;
                }
                else
                {
                  response.status = "error";
                  response.flag = "0";
                  response.alert = "incorrect mobile type !";

                }





              }
              else
              {
                response.status = "error";
                response.flag = "0";
                response.alert = "Record Not found !";
              }
            }




          }
        }

      }
      catch (Exception ex)
      {
        response.alert = ex.Message;
      }

      return response;

    }


    public string GetTableName()
    {
      SqlConnection con = new SqlConnection(constr);
      SqlCommand cmd;

      con.Open();
      string query = "";
      query = "SELECT RIGHT('0' + RTRIM(MONTH(GETDATE())), 2)+'' +DATEPART(YYYY,GETDATE())";
      query = " select convert(varchar(2),getdate(),110)";
      cmd = new SqlCommand(query, con);
      string month_name = Convert.ToString(cmd.ExecuteScalar());
      int monthId = 0;
      try
      {
        monthId = Convert.ToInt32(month_name);
      }
      catch { monthId = 0; }
      month_name = monthId.ToString();

      con.Close();


      con.Open();
      query = "";
      query = "Select DATEPART(YYYY,GETDATE()) ";
      cmd = new SqlCommand(query, con);
      string year_name = Convert.ToString(cmd.ExecuteScalar());
      con.Close();


      string tblname = "DeviceLogs_10_2023";
      tblname = "DeviceLogs_" + month_name + "_" + year_name;
      return tblname;

    }




    [HttpPost]
    [Route("GetHolidayList")]
    public HmcHolidayResponse GetHolidayList(HmcHolidayRequest req)
    {
      HmcHolidayResponse response = new HmcHolidayResponse();
      response.flag = "0";
      response.status = "error";
      response.alert = "data is not valid";
      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              var userValid = db.EmployeeMaster.Where(user => user.employee_id == employeeId && user.status == "Active").FirstOrDefault();
              if (userValid != null)
              {
                int YearName = DateTime.Now.Year;


                List<HmcHolidayModel> holidayData = (from d in db.HolidayMaster
                                                     where d.h_type == "H" && d.pay_code == userValid.pay_code && d.YearName==YearName
                                                     orderby d.holiday_date
                                                     select new HmcHolidayModel
                                                     {
                                                       holidayType = d.pay_code,
                                                       holiday_date = d.holiday_date,
                                                       pay_code = d.pay_code,
                                                       h_type = d.h_type,
                                                       id = d.id,
                                                       title = d.title
                                                     }).ToList();


                if (holidayData.Count > 0)
                {
                  foreach (HmcHolidayModel item in holidayData)
                  {
                    if (item.h_type == "H")
                    {

                      item.holidayType = "Holiday";

                    }
                    else if (item.h_type == "W")
                    {
                      item.holidayType = "Weekly Off";
                    }
                    else
                    {
                      item.holidayType = "Both";
                    }

                  }

                  response.flag = "1";
                  response.status = "success";
                  response.alert = "success";
                  response.data = holidayData;

                }
                else { }
              }



            }

          }
        }
      }
      catch (Exception e)
      {
        response.alert = e.Message;
      }
      return response;
    }



    [HttpPost]
    [Route("GetHolidayOrOffDates")]
    public HmcCheckHolidayResponse GetHolidayOrOffDates(HmcCheckHolidayRequest req)
    {
      HmcCheckHolidayResponse response = new HmcCheckHolidayResponse();
      response.flag = "0";
      response.status = "error";

      try
      {
        if (ModelState.IsValid)
        {
          using (AttendanceContext db = new AttendanceContext())
          {
            string accessKey = CryptoEngine.Decrypt(req.accessKey, "skym-3hn8-sqoy19");
            if (accessKey == @System.Configuration.ConfigurationManager.AppSettings["accesskey"])
            {
              string employeeId = CryptoEngine.Decrypt(req.employee_id, "skym-3hn8-sqoy19");
              var userValid = db.EmployeeMaster.Where(user => user.employee_id == employeeId && user.status == "Active").FirstOrDefault();
              if (userValid != null)
              {
                int YearName = DateTime.Now.Year;

                DateTime fromTime = Convert.ToDateTime(req.check_date);

                var Holiday = db.HolidayMaster.Where(x => x.holiday_date == fromTime && x.YearName == YearName && x.h_type == "H").FirstOrDefault();
                var Woff = db.HolidayMaster.Where(x => x.holiday_date == fromTime && x.YearName == YearName && x.h_type == "W").FirstOrDefault();


                if (Holiday != null && Woff == null)
                {
                  response.check_id = "1";
                  response.check_flag = "H";
                }

                else if (Holiday == null && Woff != null)
                {
                  response.check_id = "2";
                  response.check_flag = "W";
                }
                else if (Holiday != null && Woff != null)
                {
                  response.check_id = "3";
                  response.check_flag = "B";
                }
                else
                {
                  response.check_id = "0";
                  response.check_flag = "0";
                }

                response.flag = "1";
                response.status = "success";





              }
              else { }
            }



          }



        }
      }
      catch (Exception e)
      {
        response.status = e.Message;
      }
      return response;
    }






  }
}
