using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class HolidayValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(HolidayData inputObj,Dictionary<string,string> validationExceptionMap)
        {

            HolidayData result = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.title)
                {
                    DateTime holidayDate = DateTime.ParseExact(inputObj.holidayDate, "yyyy-MM-dd", null);
                    //create case
                    result = (from e in db.HolidayMaster
                              where (e.title.ToLower().Trim() == inputObj.title.ToLower().Trim()
                              && e.pay_code.Equals(inputObj.pay_code)
                              && e.holiday_date.Equals(holidayDate))
                              select new HolidayData
                              {
                                  title = e.title
                              }).FirstOrDefault();


                    if (null != result)
                    {
                        if (!validationExceptionMap.ContainsKey("title"))
                        {
                            validationExceptionMap.Add("title", ExceptionMessage.getExceptionMessage("TitleAttribute"));
                            //checkValidEmployeeStatus(inputObj);
                        }
                    }
                }

            }
        }

    }
}