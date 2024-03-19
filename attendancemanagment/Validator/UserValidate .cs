using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class UserValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(EmployeeDetails inputObj,Dictionary<string,string> validationExceptionMap)
        {

            EmployeeDetails result = null;
            EmployeeDetails EmpId = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.email)
                {
                    //create case
                    result = (from e in db.EmployeeMaster
                              where (e.email.ToLower().Trim() == inputObj.email.ToLower().Trim())
                              select new EmployeeDetails {
                                  email = e.email
                              }).FirstOrDefault();

                    EmpId = (from e in db.EmployeeMaster
                             where (e.employee_id.ToLower().Trim() == inputObj.employee_code.ToLower().Trim())
                             select new EmployeeDetails
                             {
                                 employee_id = e.employee_id
                             }).FirstOrDefault();


                    if (null != result)
                    {
                        if (!validationExceptionMap.ContainsKey("email"))
                        {
                            //validationExceptionMap.Add("email", ExceptionMessage.getExceptionMessage("EmployeeBaseDetails.email.Unique"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }

                    Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                    Match match = regex.Match(inputObj.email);

                    if (!match.Success)
                    {
                       // validationExceptionMap.Add("email", ExceptionMessage.getExceptionMessage("EmailAddressAttribute"));
                    }

                    if (null != EmpId)
                    {
                        if (!validationExceptionMap.ContainsKey("employee_code"))
                        {
                            validationExceptionMap.Add("employee_code", ExceptionMessage.getExceptionMessage("EmployeeBaseDetails.adpId.Unique"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }
                    
                    //string phoneNumber = phone.Length.ToString();
                    if (inputObj.aadhar_number != null)
                    {
                        string aadhar_number = inputObj.aadhar_number;
                        if (aadhar_number.Length != 12)
                        {
                            validationExceptionMap.Add("aadhar_number", ExceptionMessage.getExceptionMessage("AadharAttribute"));
                        }
                    }
                    

                    string mobile_no = inputObj.mobile_no;
                    if (mobile_no.Length != 10)
                    {
                        validationExceptionMap.Add("mobile_no", ExceptionMessage.getExceptionMessage("PhoneAttribute"));
                    }

                    string mobileNo = (from e in db.EmployeeMaster
                                        where (e.mobile_no.ToLower().Trim() == inputObj.mobile_no.ToLower().Trim())
                                        select e.mobile_no).FirstOrDefault();

                    if (!string.IsNullOrEmpty(mobileNo))
                    {
                        if (!validationExceptionMap.ContainsKey("mobile_no"))
                        {
                            validationExceptionMap.Add("mobile_no", ExceptionMessage.getExceptionMessage("EmployeeMaster.Phone.Unique"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }

                }

            }
            else
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Match match = regex.Match(inputObj.email);

                if (!match.Success)
                {
                    validationExceptionMap.Add("email", ExceptionMessage.getExceptionMessage("EmailAddressAttribute"));
                }

                if (inputObj.aadhar_number != null)
                {
                    string aadhar_number = inputObj.aadhar_number;
                    if (aadhar_number.Length != 12)
                    {
                        validationExceptionMap.Add("aadhar_number", ExceptionMessage.getExceptionMessage("AadharAttribute"));
                    }
                }

                string mobile_no = inputObj.mobile_no;
                if (mobile_no.Length != 10)
                {
                    validationExceptionMap.Add("mobile_no", ExceptionMessage.getExceptionMessage("PhoneAttribute"));
                }
            }
        }

    }
}
