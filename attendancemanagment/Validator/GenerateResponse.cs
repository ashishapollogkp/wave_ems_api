using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace attendancemanagment.utilities
{
    public class GenerateResponse
    {

        public static dynamic generateSuccessResponse(object returnObject)
        {
            if (null != returnObject)
            {
                var successResponse = new
                {
                    rsBody = returnObject
                };
                return successResponse;
            }
            else
            {
                var successResponse = new
                {
                    rsBody = "success"
                };
                return successResponse;
            }

            

            
        }

        public static dynamic  generateValidateExceptionResponse(Dictionary<string, string> validationExceptionMap)
        {
            Dictionary<string, object> rsBody = new Dictionary<string, object>()
            {
                {"rsBody" , new Dictionary<string, object>
                                {
                                    {
                                          "exceptionBlock" , new Dictionary<string, object>
                                {
                                    {
                                          "msg" , new Dictionary<string, object>
                                {
                                    {
                                          "validationException" ,validationExceptionMap
                                    }
                                }
                                    }
                                }
                                    }
                                }

                }
            };




            return rsBody;

        }


        public static dynamic generateBusinessExceptionResponse(string businessError)
        {
            Dictionary<string, object> rsBody = new Dictionary<string, object>()
            {
                {"rsBody" , new Dictionary<string, object>
                                {
                                    {
                                          "exceptionBlock" , new Dictionary<string, object>
                                {
                                    {
                                          "msg" , new Dictionary<string, object>
                                {
                                    {
                                          "businessException" ,businessError
                                    }
                                }
                                    }
                                }
                                    }
                                }

                }
            };




            return rsBody;

        }


        public static dynamic generateTechnicalExceptionResponse()
        {
            Dictionary<string, object> rsBody = new Dictionary<string, object>()
            {
                {"rsBody" , new Dictionary<string, object>
                                {
                                    {
                                          "exceptionBlock" , new Dictionary<string, object>
                                {
                                    {
                                          "msg" , new Dictionary<string, object>
                                {
                                    {
                                          "validationException" , "The System is currently unavailable.\nPlease contact the System Administrator for further details."
                                    }
                                }
                                    }
                                }
                                    }
                                }

                }
            };




            return rsBody;

        }


    }
}