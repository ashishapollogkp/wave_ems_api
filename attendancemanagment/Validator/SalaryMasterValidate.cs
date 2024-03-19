using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class SalaryMasterValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(SalaryMasterDto inputObj,Dictionary<string,string> validationExceptionMap)
        {

            SalaryMasterDto result = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.pay_code)
                {
                    //create case
                    result = (from e in db.SalaryMaster
                              where (e.pay_code.ToLower().Trim() == inputObj.pay_code.ToLower().Trim())
                              select new SalaryMasterDto
                              {
                                  pay_code = e.pay_code
                              }).FirstOrDefault();

                    

                    if (null != result)
                    {
                        if (!validationExceptionMap.ContainsKey("pay_code"))
                        {
                            validationExceptionMap.Add("pay_code", ExceptionMessage.getExceptionMessage("EmployeeBaseDetails.email.Unique"));
                        }
                    }

                }

            }
        }

    }
}