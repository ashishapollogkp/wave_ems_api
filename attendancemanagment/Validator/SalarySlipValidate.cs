using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class SalarySlipValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(SalarySlipDto inputObj,Dictionary<string,string> validationExceptionMap)
        {

            SalaryMasterDto result = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.company_code)
                {
                    //create case
                    result = (from e in db.SalarySlip
                              where (e.pay_code.ToLower().Trim() == inputObj.company_code.ToLower().Trim())
                              && e.month.Equals(inputObj.month) && e.year.Equals(inputObj.year)
                              select new SalaryMasterDto
                              {
                                  pay_code = e.pay_code
                              }).FirstOrDefault();

                    

                    if (null != result)
                    {
                        if (!validationExceptionMap.ContainsKey("company_code"))
                        {
                            validationExceptionMap.Add("company_code", ExceptionMessage.getExceptionMessage("PaycodeAttribute"));
                        }
                    }

                }

            }
        }

    }
}