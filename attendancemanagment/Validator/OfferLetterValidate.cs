using attendancemanagment.Models;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace attendancemanagment.Validators
{
    public class OfferLetterValidate
    {
        static AttendanceContext db = new AttendanceContext();
        private static bool isError;

        public static void IsValid(OfferLetterRequest inputObj,Dictionary<string,string> validationExceptionMap)
        {

            OfferLetterRequest result = null;

            //check if its a create or an update
            if (inputObj.id == 0)
            {
                if (null != inputObj.mobile_no)
                {
                    //create case
                    result = (from e in db.OfferLetterMaster
                              where (e.mobile_no.ToLower().Trim() == inputObj.mobile_no.ToLower().Trim())
                              select new OfferLetterRequest
                              {
                                  mobile_no = e.mobile_no
                              }).FirstOrDefault();


                    if (null != result)
                    {
                        if (!validationExceptionMap.ContainsKey("mobile_no"))
                        {
                            validationExceptionMap.Add("mobile_no", ExceptionMessage.getExceptionMessage("EmployeeBaseDetails.phone.Unique"));
                            //checkValidEmployeeStatus(inputObj);
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
}