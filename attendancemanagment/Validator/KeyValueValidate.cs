using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class KeyValueValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(KeyValue inputObj,Dictionary<string,string> validationExceptionMap)
        {

            KeyValue keyCode = null;
            //KeyValue keyDescription = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.key_code)
                {
                    //DateTime holidayDate = DateTime.ParseExact(inputObj.holidayDate, "yyyy-MM-dd", null);
                    //create case
                    keyCode = (from e in db.KeyValueMaster
                              where (e.key_code.ToLower().Trim() == inputObj.key_code.ToLower().Trim()
                              && e.key_type.Equals(inputObj.key_type)
                              && e.pay_code.Equals(inputObj.pay_code))
                              select new KeyValue
                              {
                                  key_code = e.key_code
                              }).FirstOrDefault();


                    if (null != keyCode)
                    {
                        if (!validationExceptionMap.ContainsKey("key_code"))
                        {
                            validationExceptionMap.Add("key_code", ExceptionMessage.getExceptionMessage("KeyCodeAttribute"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }
                }

            }
        }

    }
}
