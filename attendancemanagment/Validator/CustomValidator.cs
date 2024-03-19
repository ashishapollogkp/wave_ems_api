using attendancemanagment.Container;
using attendancemanagment.utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace attendancemanagment.Validators
{
    public class CustomValidator
    {
        

        public static ExceptionResponseContainer applyValidations(object inputObj, Type validatorClass)
        {
            ExceptionResponseContainer retVal = new ExceptionResponseContainer() { isValid = true };
            Dictionary<string, string> validationExceptionMap = new Dictionary<string, string>();

            PropertyInfo[] props = inputObj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);


            foreach (var prop in props)
            {
                applyPropertyValidations(inputObj , prop, validationExceptionMap);
            }

            //do some custom validations using the validatorClass
            if( null != validatorClass)
            {
                MethodInfo customValidateMethod = validatorClass.GetMethod("IsValid", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                customValidateMethod.Invoke(customValidateMethod, new object[] { inputObj, validationExceptionMap });
            }

            if(validationExceptionMap.Count() > 0)
            {
                retVal.isValid = false;
                retVal.jsonResponse = GenerateResponse.generateValidateExceptionResponse(validationExceptionMap);
            }

            return retVal;
        }

        private static void applyPropertyValidations(object inputObj , PropertyInfo propertyInfo, Dictionary<string, string> validationExceptionMap)
        {
            var customAttributes = propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true);

            object propertyValue = null;

            if (customAttributes.Count() > 0)
            {
                propertyValue = propertyInfo.GetValue(inputObj, null);
            }

            foreach (var attribute in customAttributes)
            {
                var attrType = attribute.GetType();
               // ConstructorInfo attrConstructor = attrType.GetConstructor(Type.EmptyTypes);
                //object attrClassObj = attrConstructor.Invoke(new object[] { });

                // Get the ItsMagic method and invoke with a parameter value of 100

                MethodInfo validateMethod = attrType.GetMethod("IsValid");
                Boolean isValid = (Boolean)validateMethod.Invoke(attribute, new object[] { propertyValue });

               

                if (isValid == false)
                {
                    //add to dictionary with error message and break
                    validationExceptionMap.Add(propertyInfo.Name, ExceptionMessage.getExceptionMessage(attribute.GetType().Name));
                    return;
                }
               
            }
        }


        
    }
}