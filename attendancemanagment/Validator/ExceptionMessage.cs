using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace attendancemanagment.utilities
{
    public class ExceptionMessage
    {

        private static Dictionary<string, string> messageMap = new Dictionary<string, string>()
        {
            { "RequiredAttribute" , "value is required"},
            { "RequiredAttribute.Multiple" , "a code & plant exist with the same code "},
            { "EmailAddressAttribute" , "invalid email address"},
            { "PhoneAttribute" , "invalid mobile number"},
            { "EmployeeMaster.Phone.Unique" , "a registered user exists with the same mobile number"},
            { "TitleAttribute" , "a holiday exists with the same title, date and company"},
            { "KeyCodeAttribute" , "a code exists with the same code"},
            { "DateCodeAttribute" , "a shift exists with the same date"},
            { "AadharAttribute" , "invalid aadhar number"},
            { "PaycodeAttribute" , "a company code already exist with same month and year"},
            { "EmployeeBaseDetails.email.Unique" , "a registered user exists with the same email address"},
            { "EmployeeBaseDetails.phone.Unique" , "a registered user exists with the same mobile number"},
            { "EmployeeBaseDetails.adpId.Unique" , "a registered user exists with the same employee ID"},
            { "EmployeeBaseDetails.dolStr.Unique" , "value is required"},
            { "EmployeeRole.channelCode.Unique" , "invalid channel code"},
            { "CustomerBaseDetails.soldToCode.Unique" , "a registered user exists with the same code"},
            { "BranchDetails.branchName.Unique" , "a registered branch exists with the same name & code"},
            { "EmployeeRoleAndAreaDTO.region.Unique" , "a registered user exists with the same employee ID"},
            { "EmployeeRoleAndAreaDTO.role.Unique" , "a registered user exists with the same employee ID"},
            { "EmployeeRoleAndAreaDTO.branchDetailsFK.Unique" , "a registered user exists with the same employee ID"},
            { "EmployeeRoleAndAreaDTO.subChannelCode.Unique" , "a registered user exists with the same employee ID"},
            { "EmployeeRoleAndAreaDTO.channelCode.Unique" , "a registered user exists with the same employee ID"},
            { "AccrualAmount.adpId.Unique" , "a registered user exists with the same employee ID"},
            { "EmployeeRoleAndAreaDTO.endMonth.Unique" , "end month should be greater than or equal to the start month"},
            { "SplitTargetValueDTO.splitValue.Unique" , "a value is equal to 100%"},
            { "BranchDetails.docStr.Unique" , "value is required"},
            { "ErrorMessage.UniqueNoData" , "No Data Found"},
            { "Relationship.Unique" , "No Customer Realtionship Data Found"},
            { "Message.Sucess" , "sucess"},
            { "CustomerBaseDetails.soldToCode.UniqueValid" , "enter either sold to code or ship to code"},
            { "CustomerBaseDetails.shipToCode.UniqueValid" , "enter either sold to code or ship to code"},
            { "EmployeeRoleAndAreaDTO.endMonth.UniqueOverLapRole" , "you are trying to grant multiple active roles to the employee.Please close the current active role before adding a new one."},
            { "EmployeeRoleAndAreaDTO.startMonth.UniqueOverLapRole" , "you are trying to add employee start month is greater than for current start month"},
            { "CustomerBasicRelationDTO.endMonth.UniqueOverLapRole" , "you are trying to grant multiple active relationships to the customer.Please close the current active customer relationship before adding a new one."},
            { "ErrorMessage.Unique" , "no active and relavent employee found for the combination of channel, region, branch / area"},
            { "EmployeeRoleAndAreaDTO.VACANT.UniqueOverLapRole" , "a registered user exist vacant user with the same channel, role, region. you want to close this position"},
            { "ErrorMessage.UniqueEmployee" , "a registered customer does not exist with the same code"},
            { "ErrorMessage.UniqueCr" , "a registered employee role change request exist with the same employee ID "},
            { "InitiateIncentiveOverride.Unique" , "a registered employee change request exist with the same employee ID "},
            { "IncentiveCalculationRequest.incentiveEndMonth.Unique" , "a registered incentive exist with the same year, incentive name"},
            { "IncentiveCalculationRequest.quarter.Unique" , "value is required"},
            { "EmployeeDetailsDTO.Password.Unique" , "a password is duplicate value"},
            { "EmployeeDetailsDTO.oldPassword.Unique" , "a registered employee does not exist with the old password"},
            { "CustomerBasicRelationDTO.ChannelCode.Unique" , "you do not have permission for change channel"},
            { "CustomerBasicRelationDTO.region.Unique" , "you do not have permission for change region"},
            { "CustomerBasicRelationDTO.branch.Unique" , "you do not have permission for change branch"},
            { "IncentiveRequest.Incentive.Unique" , "An active incentive request for this or some other quarter is still pending closure.A new incentive request cannot be raised."},
            { "IncentiveRequest.Close.Unique" , "This incentive request cannot be closed as one or more line items are still open"},
            { "IncentiveConfirmation.Payslip.Unique" , "Approvals for Performance data has not been obtained from related entities. The request for payslip generation is being rejected. Please try after all relevant approvals have been obtained."},
            { "YearlyCustomerTargetsDTO.buCode.Unique" , "a buName is duplicate value"},
            { "CustomerBasicRelationDTO.startMonthLess.Unique" , "a customer relationship start month is less than to current customer relationship start month "},
            { "CustomerBasicRelationDTO.startMonthEqual.Unique" , "a customer relationship is registered with the same relationship start month. Please selected relalationship start month greater than to select relationship start month"},
            { "CustomerDetailsDTO.EmployeeRoleFK.Unique" , "a change request already raised change request for selected employee Id"},
            { "EmployeeRoleAndAreaDTO.startMonth.UniqueExist" , "a registered employee role allready exist with the same employee role. Please try after delete role or end role for assign employee"},
        };


        public static string getExceptionMessage(string exceptionType)
        {
            return messageMap[exceptionType];
        }


        
    }
}