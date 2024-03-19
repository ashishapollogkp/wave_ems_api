using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class ShiftValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(ShiftData inputObj,Dictionary<string,string> validationExceptionMap)
        {

            ShiftData keyCode = null;
            //KeyValue keyDescription = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.shift_code)
                {
                    //DateTime holidayDate = DateTime.ParseExact(inputObj.holidayDate, "yyyy-MM-dd", null);
                    //create case
                    keyCode = (from e in db.Shift
                               where (e.shift_code.ToLower().Trim() == inputObj.shift_code.ToLower().Trim())
                               && e.pay_code.Equals(inputObj.pay_code)
                               select new ShiftData
                               {
                                   shift_code = e.shift_code
                               }).FirstOrDefault();


                    if (null != keyCode)
                    {
                        if (!validationExceptionMap.ContainsKey("shift_code"))
                        {
                            validationExceptionMap.Add("shift_code", ExceptionMessage.getExceptionMessage("KeyCodeAttribute"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }
                }

            }
        }

    }
}