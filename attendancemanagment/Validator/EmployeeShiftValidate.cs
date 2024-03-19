using attendancemanagment.Models;
using attendancemanagment.utilities;
using attendancemanagment.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class EmployeeShiftValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(EmployeeShiftReq inputObj,Dictionary<string,string> validationExceptionMap)
        {

            EmployeeShiftMaster result = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.employee_code)
                {
                    
                    DateTime fromDate = DateTime.ParseExact(inputObj.from_date, "dd/MM/yyyy", null);
                    DateTime toDate = DateTime.ParseExact(inputObj.to_date, "dd/MM/yyyy", null);
                    string employeeId = CryptoEngine.Decrypt(inputObj.employee_id, "skym-3hn8-sqoy19");
                    EmployeeMaster employeeMaster = (from d in db.EmployeeMaster
                                                     where d.employee_id.Equals(employeeId)
                                                     select d).FirstOrDefault();
                    //create case
                    result = (from e in db.EmployeeShiftMaster
                              where (e.employee_id.ToLower().Trim() == inputObj.employee_code.ToLower().Trim()
                              && e.pay_code.Equals(employeeMaster.pay_code)
                              && e.from_date >= fromDate && e.to_date <= toDate)
                              select e).FirstOrDefault();


                    if (null != result)
                    {
                        if (!validationExceptionMap.ContainsKey("from_date"))
                        {
                            validationExceptionMap.Add("from_date", ExceptionMessage.getExceptionMessage("DateCodeAttribute"));
                            validationExceptionMap.Add("to_date", ExceptionMessage.getExceptionMessage("DateCodeAttribute"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }
                }

            }
        }

    }
}