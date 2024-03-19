using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace attendancemanagment.Models
{

  public class HmcResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeBaseDetails data { get; set; }
    public string alert { get; set; }
  }

  public class HmcResponseNew
  {
    public string status { get; set; }
    public string flag { get; set; }
    public object data { get; set; }
    public string alert { get; set; }
  }

  public class HmcNewResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeDetailsNewDTO data { get; set; }
    public string alert { get; set; }
  }

  public class EmployeeMasterDTO
  {
    [Required]
    public string employee_id { get; set; }
   
    public string password { get; set; }



  
    public string name { get; set; }
    public string email { get; set; }
    
    public string accesskey { get; set; }
    public Boolean rememberMe { get; set; }
    public string fcm_id { get; set; }
    public string device_id { get; set; }


  }

  public class EmployeeDashboardDTO
  {
    public List<ApplyLeaveResponse> applyleave { get; set; }
    public decimal[] targets { get; set; }
    public decimal[] actuals { get; set; }
    public decimal[] balance { get; set; }
    public List<decimal> donatActuals { get; set; }
    public string[] bgNames { get; set; }
    public List<String> donatBgNames { get; set; }
  }

  public class EmployeeLoginMobileDTO
  {
    [Required]
    public string mobile_no { get; set; }
    public string otp { get; set; }
  }

  public class SendOtpToMobile
  {
    public string method { get; set; }
    public string api_key { get; set; }
    public string to { get; set; }
    public string sender { get; set; }
    public string unicode { get; set; }
    public string message { get; set; }
    public string format { get; set; }
  }

  public class EmployeeResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeBasicdetails data { get; set; }
    public string alert { get; set; }
  }

  public class ActiveEmployeeResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<EmployeeBasicdetails> data { get; set; }
    public string alert { get; set; }
  }

  public class AMSResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public dynamic data { get; set; }
    public string alert { get; set; }
  }

  public class HmcTypeHeadResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<HmcTypeHeadValue> data { get; set; }
    public string alert { get; set; }
  }
  public class ResetPassword
  {
    [Required]
    [Display(Name = "Employee Id")]
    public string employee_id { get; set; }
    [Required]
    [Display(Name = "Mobile Number")]
    public string mobile_no { get; set; }
    [Required]
    public string name { get; set; }
  }

  // typehead
  public class TyapeheadReq
  {
    public string Prefix { get; set; }
    public string employee_id { get; set; }
    public string accesskey { get; set; }
    public string pay_code { get; set; }
  }

  public class HmcTypeHeadValue
  {
    public string name { get; set; }
    public string employee_id { get; set; }
  }

  public class HmcJsonResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeBaseDetails data { get; set; }
    public dynamic alert { get; set; }
  }

  public class ChangePassword
  {
    [Required]
    [DataType(DataType.Password)]
    public string new_password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string confirm_password { get; set; }
    public string employee_id { get; set; }
    public string accessKey { get; set; }
  }

  public class VarifyOtpRequest
  {
    [Required]
    public string otp { get; set; }
    [Required]
    public string mobile_no { get; set; }
  }

  public class VarifyOtpResponse
  {
    public string status { get; set; }
    public string msg { get; set; }
    public string errorCode { get; set; }
    public string data { get; set; }
  }

  public class ApprovalRequest
  {
    public string employee_id { get; set; }
    public string type { get; set; }
    public int ids { get; set; }
    public string token { get; set; }
    public string status { get; set; }
  }

  public class PortalLogin
  {
    public string employee_id { get; set; }
    //public string password { get; set; }
  }

  public class HmcStringResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public string data { get; set; }
    public string alert { get; set; }
  }

  public class CheckInOut
  {
    [Required]
    public string accessKey { get; set; }
    [Required]
    public string employee_id { get; set; }
    public string qr_code { get; set; }
    public string lat { get; set; }
    public string log { get; set; }
    public string feature_image { get; set; }
    public string address { get; set; }

  }

  public class CheckInOutNew
  {
    [Required]
    public string accesskey { get; set; }
    [Required]
    public string employee_id { get; set; }
    public string lat { get; set; }
    public string log { get; set; }
    public string imi_no { get; set; }
    public string address { get; set; }

    [DisplayFormat(DataFormatString = "YYYY-MM-DD HH:mm:ss")]
    public string timedate { get; set; }

  }

  public class WorkFormHomeRequest
  {
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string user_id { get; set; }
    public List<AddDateListDto> dateList { get; set; }
  }

  public class AddDateListDto
  {
    public string date { get; set; }
  }

  public class HmcAccessRoleResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public AccessMenuRequest data { get; set; }
    public string alert { get; set; }
  }


  public class EmployeeDashboardPerameter
  {
    public string role { get; set; }
    public int totalemployee { get; set; }
    public int activeemployee { get; set; }
    public int inactiveemployee { get; set; }
    public int totalleave { get; set; }
    public int pendingleave { get; set; }
    public int approveleave { get; set; }
    public int rejectleave { get; set; }
    public decimal total { get; set; }
    public decimal availd { get; set; }
    public decimal balancel { get; set; }
    public EmployeeBasicdetails employeebasicdetails { get; set; }
    public List<HolidayMaster> holidays { get; set; }
    public List<ApplyLeaveResponse> applyleave { get; set; }
    public decimal[] targets { get; set; }
    public decimal[] actuals { get; set; }
    public decimal[] balance { get; set; }
    public List<Dictionary<string, decimal>> donatActuals { get; set; }
    public string[] bgNames { get; set; }
    public List<String> donatBgNames { get; set; }
  }

  public class EmployeeBasicdetails
  {
    public string employee_id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string mobile_no { get; set; }
    public string reporting_to { get; set; }
    public string reporting_to_name { get; set; }
    public string father_name { get; set; }
    public string employee_type { get; set; }
    public string designation { get; set; }
    public string department { get; set; }
    public string company_name { get; set; }
    public string company_code { get; set; }
    public string aadhar_no { get; set; }
    public DateTime dob { get; set; }
    public DateTime doj { get; set; }
    public string dobs { get; set; }
    public string dojs { get; set; }
    public string el { get; set; }
    public string cl { get; set; }
    public string sl { get; set; }
    public string role { get; set; }
    public int password_count { get; set; }
    public string feature_image { get; set; }
    public string accessKey { get; set; }
    public string attendance_type { get; set; }
    public string pay_code { get; set; }
    public List<LatLongModel> location_list { get; set; }
  }

  public class LatLongModel
  {
    public string lat { get; set; }
    public string log { get; set; }
  }

  public class EmployeeDashboardResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeDashboardPerameter data { get; set; }
    public string alert { get; set; }
  }

  public class EmployeeProfile
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeBasicdetails data { get; set; }
    public string alert { get; set; }
  }
  public class HmcAttResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public AttResponse data { get; set; }
    public string alert { get; set; }
  }


  public class DailyAttResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<AttendanceLogResponse> data { get; set; }
    public string alert { get; set; }
  }

  public class EmployeeShiftResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<EmployeeShiftMaster> data { get; set; }
    public string alert { get; set; }
  }

  public class HrsMintResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public string role { get; set; }
    public List<HrsMint> hrs { get; set; }
    public List<HrsMint> mint { get; set; }
    public string alert { get; set; }
  }

  public class HmcMonthlyAttResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<AttendanceResponse> data { get; set; }
    public string alert { get; set; }
  }

  public class MyTeamSummaryResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public MyTeamSummaryData data { get; set; }
    public string alert { get; set; }
  }

  public class MyTeamSummaryData
  {
    public List<AttendanceResponse> attendance { get; set; }
    public EmployeeDashboardPerameter leavesummary { get; set; }
    public List<TeamDetails> myteam { get; set; }

  }












  public class ApplyLeaveResponse
  {
    public int id { get; set; }
    public string employee_id { get; set; }
    public string apply_by_name { get; set; }
    public string applydate { get; set; }
    public DateTime date { get; set; }
    public string fromdate { get; set; }
    public string todate { get; set; }
    public DateTime from_date { get; set; }
    public DateTime to_date { get; set; }
    public string reason { get; set; }
    public DateTime created_date { get; set; }
    public string status { get; set; }
    public string assign_by { get; set; }
    public string assign_by_name { get; set; }
    public int leave_type { get; set; }
    public string leave_code { get; set; }
    public int duration_type { get; set; }
    public string duration_code { get; set; }
    public string no_of_leave { get; set; }

  }

  public class MispunchResponse
  {
    public int id { get; set; }
    public string employee_id { get; set; }
    public string apply_by_name { get; set; }
    public string applydate { get; set; }
    public DateTime date { get; set; }
    public DateTime misdate { get; set; }
    public string empmisdate { get; set; }
    public string in_time { get; set; }
    public string out_time { get; set; }
    public string shift { get; set; }
    public string reason { get; set; }
    public string status { get; set; }
    public string assign_by { get; set; }
    public string assign_by_name { get; set; }

  }

  public class AttendanceResponse
  {
    public int id { get; set; }
    public string employee_id { get; set; }
    public string name { get; set; }
    public DateTime date { get; set; }
    public string day { get; set; }
    public string in_time { get; set; }
    public string out_time { get; set; }
    public string sdate { get; set; }
    public string shift { get; set; }
    public double total_hrs { get; set; }
    public string status { get; set; }
    public string absent { get; set; }
    public string mis { get; set; }
    public string early { get; set; }
    public string late { get; set; }
    public string in_latitude { get; set; }
    public string in_longitude { get; set; }
    public string location_address { get; set; }
  }

  public class AttResponse
  {
    public string name { get; set; }
    public string employee_id { get; set; }
    public string print_date { get; set; }
    public string company_name { get; set; }
    public List<AttendanceResponse> attendaceList { get; set; }
    public string department { get; set; }
    public string shift { get; set; }
  }

  public class AttendanceRepoertResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<AttendanceReport> data { get; set; }
    public string alert { get; set; }
  }

  public class AttendanceReport
  {
    public string department { get; set; }
    public List<EmployeeAttendance> employeeAttendance { get; set; }
  }

  public class EmployeeAttendance
  {
    public string employee_id { get; set; }
    public string name { get; set; }
    public List<string> inTime { get; set; }
    public List<string> outTime { get; set; }
    public List<string> date { get; set; }
    public List<string> status { get; set; }
    public List<AttendanceResponse> attendance { get; set; }
  }


  public class HmcRequest
  {
    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string pageNo { get; set; }
    public int id { get; set; }
    public string pageSize { get; set; }
    public string status { get; set; }
    public string date { get; set; }
    public string search_result { get; set; }
    public string currentUrl { get; set; }
    public string menuType { get; set; }
  }


 

  public class GridPDFGenerate
  {
    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string GridHtml { get; set; }
  }

  // shift
  public class ShiftResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<Shift> data { get; set; }
    public string alert { get; set; }
  }

  public class ShiftData
  {
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public int id { get; set; }
    public string shift_code { get; set; }
    public string shift_name { get; set; }
    public string start_time { get; set; }
    public string end_time { get; set; }
    public string grace_time { get; set; }
    public string break_start_time { get; set; }
    public string break_end_time { get; set; }
    public string latitude { get; set; }
    public string longitude { get; set; }
    public string pay_code { get; set; }
  }

  // access details
  public class HmcAccessResponse
  {
    public string employee_id { get; set; }
    public string accessKey { get; set; }
  }

  // crm data 
  public class UploadCrmDataRequest
  {
    public string employee_id { get; set; }
    public string mobile_no { get; set; }
    public List<UploadCrmData> attendance_list { get; set; }
  }

  public class UploadCrmData
  {
    public string employee_id { get; set; }
    public string date { get; set; }
    public string in_time { get; set; }
    public string out_time { get; set; }
  }

  public class EmployeeBaseDetails
  {
    public string employee_id { get; set; }
    public string role { get; set; }
    public string feature_image { get; set; }
    public string name { get; set; }
    public string pay_code { get; set; }
    public string attendance_type { get; set; }
    public int notification { get; set; }
    public List<MenuDetails> menuList { get; set; }
  }

  public class MenuResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public string alert { get; set; }
    public List<MenuDetails> data { get; set; }
  }

  public class MenuDetails
  {
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string icons { get; set; }
    public List<MenuMaster> sub_menu { get; set; }
  }

  public class EmployeeListResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<EmployeeMaster> data { get; set; }
    public string alert { get; set; }
  }

  public class OfferLetterResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<OfferLetterMaster> data { get; set; }
    public string alert { get; set; }
  }

  // salary master response
  public class SalaryMasterResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<SalaryMaster> data { get; set; }
    public string alert { get; set; }
  }

  public class SalarySlipResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<SalarySlip> data { get; set; }
    public string alert { get; set; }
  }

  public class EmployeeConveyanceResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<EmployeeConveyanceDto> data { get; set; }
    public string alert { get; set; }
  }

  // team response
  public class TeamResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<TeamDetails> data { get; set; }
    public string alert { get; set; }
  }

  public class GetApplyLeaveResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<ApplyLeaveResponse> data { get; set; }
    public string alert { get; set; }
  }

  public class GetMispunchResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<MispunchResponse> data { get; set; }
    public string alert { get; set; }
  }

  public class LeaveApplyResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmpployeeLeaveCalculation data { get; set; }
    public string alert { get; set; }
  }

  public class LeaveApplyNewResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public EmployeeDashboardDTO data { get; set; }
    public string alert { get; set; }
  }

  public class EmpployeeLeaveCalculation
  {
    public decimal[] targets { get; set; }
    public decimal[] actuals { get; set; }
    public decimal[] balance { get; set; }
    public List<decimal> donatActuals { get; set; }
    public string[] bgNames { get; set; }
    public List<String> donatBgNames { get; set; }
  }

  public class ApplyLeaveMaster
  {
    public string employee_id { get; set; }
    public string employee_code { get; set; }
    public string from_date { get; set; }
    public string to_date { get; set; }
    public string reason { get; set; }
    public string accessKey { get; set; }
    public string leave_type { get; set; }
    public string shift { get; set; }
    public string from_time { get; set; }
    public string to_time { get; set; }
    public string duration_type { get; set; }
  }

  public class ApplyLeaveDTO
  {
    public int leave_id { get; set; }
    public decimal no_of_leave { get; set; }
  }

  public class LeaveDTO
  {
    public string name { get; set; }
    public decimal value { get; set; }
  }

  public class LeaveType
  {
    public string el { get; set; }
    public string cl { get; set; }
    public string sl { get; set; }
    public string a_el { get; set; }
    public string a_cl { get; set; }
    public string a_sl { get; set; }
    public string b_el { get; set; }
    public string b_cl { get; set; }
    public string b_sl { get; set; }
  }

  public class KeyValue
  {
    public int id { get; set; }
    public string key_type { get; set; }
    public string value_type { get; set; }
    public string key_description { get; set; }
    public string key_code { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string pay_code { get; set; }
  }

  public class AccessMenuRequest
  {
    public int id { get; set; }
    public string key_type { get; set; }
    public string value_type { get; set; }
    public string pay_code { get; set; }
    public string key_description { get; set; }
    public List<int> access_menu { get; set; }
    public string key_code { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
  }

  public class KeyValueData
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<KeyValueMaster> data { get; set; }
    public string alert { get; set; }
  }

  public class MasterData
  {
    public string status { get; set; }
    public string flag { get; set; }
    public string data { get; set; }
    public List<KeyValueMaster> department { get; set; }
    public List<KeyValueMaster> designation { get; set; }
    public List<KeyValueMaster> role { get; set; }
    public List<Shift> shift { get; set; }
    public string alert { get; set; }
  }

  public class DeleteDTO
  {

    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string delete_id { get; set; }
    public int id { get; set; }
  }

  public class NotificationList
  {
    public List<ApplyLeaveResponse> applyLeave { get; set; }
    public List<MispunchResponse> attendanceTicket { get; set; }
  }

  public class NotificationResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public NotificationList data { get; set; }
    public string alert { get; set; }
  }

  public class SearchEmployeeAttRequest
  {

    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string employee_code { get; set; }
    public string from_date { get; set; }
    public string to_date { get; set; }
    public string type { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string days { get; set; }
    public string department { get; set; }
    public string shift { get; set; }
    public string pageNo { get; set; }
    public string pageSize { get; set; }
    public string status { get; set; }
    public int id { get; set; }
    public string pay_code { get; set; }
  }

  public class AttendanceLogResponse
  {
    public int id { get; set; }
    public string employee_id { get; set; }
    public string name { get; set; }
    public DateTime date { get; set; }
    public string shift { get; set; }
    public string status { get; set; }
    public string day { get; set; }
    public string pay_code { get; set; }
    public string in_lat { get; set; }
    public string in_long { get; set; }
    public string feature_image { get; set; }
    public string location_address { get; set; }
  }

  public class HolidayMasterRes
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<HolidayData> data { get; set; }
    public string alert { get; set; }
  }

  public class CompanyResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public List<CompaniesMaster> data { get; set; }
    public string alert { get; set; }
  }

  public class HolidayData
  {
    public int id { get; set; }
    public string title { get; set; }
    public DateTime holiday_date { get; set; }
    public string start { get; set; }
    public string end { get; set; }
    public string holidayDate { get; set; }
    public string rendering { get; set; }
    public string name { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string pay_code { get; set; }
  }

  public class EmployeeShiftReq
  {
    public int id { get; set; }
    public string employee_code { get; set; }
    public string from_date { get; set; }
    public string to_date { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string pay_code { get; set; }
    public string shift { get; set; }
  }

  public class CompanyRequest
  {
    public int id { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string name { get; set; }
    public string company_code { get; set; }
    public string company_address { get; set; }
    public string companye_mail { get; set; }
    public string companye_contact_no { get; set; }
    public string company_website { get; set; }
    public string status { get; set; }
  }

  public class ShowKeyValue
  {
    public KeyValueMaster data { get; set; }
    public string status { get; set; }
    public string flag { get; set; }
    public string alert { get; set; }
  }
  public class SendQuery
  {
    public string employee_code { get; set; }
    public string employee_id { get; set; }
    public string p_date { get; set; }
    public string attendance_id { get; set; }
    public string in_time { get; set; }
    public string out_time { get; set; }
    public string reason { get; set; }
    public string accessKey { get; set; }
    public string shift { get; set; }
    public string type { get; set; }
  }

  public class EmployeeDetails
  {
    public int id { get; set; }
    public string employee_id { get; set; }
    public string employee_code { get; set; }
    public string email { get; set; }
    public string status { get; set; }
    public string name { get; set; }
    public string designation { get; set; }
    public string department { get; set; }
    public string shift { get; set; }
    public string reporting_to { get; set; }
    public string mobile_no { get; set; }
    public DateTime dojs { get; set; }
    public DateTime dobs { get; set; }
    public DateTime dols { get; set; }
    public string doj { get; set; }
    public string dob { get; set; }
    public string dol { get; set; }
    public string blood_group { get; set; }
    public string father_name { get; set; }
    public string el { get; set; }
    public string cl { get; set; }
    public string sl { get; set; }
    public string pay_code { get; set; }
    public string accessKey { get; set; }
    public string feature_image { get; set; }
    public Boolean office_emp { get; set; }
    public Boolean marketing_emp { get; set; }
    public string role { get; set; }
    public string aadhar_number { get; set; }
    public string gender { get; set; }
    public string employee_type { get; set; }
    public string bank_name { get; set; }
    public string acc_no { get; set; }
    public string ifsc_code { get; set; }
    public decimal employee_salary { get; set; }
    public string pan_no { get; set; }
    public string uan_no { get; set; }
    public int machine_id { get; set; }
    public string attendance_type { get; set; }

  }

  public class ShowEmployee
  {
    public EmployeeDetails data { get; set; }
    public string status { get; set; }
    public string flag { get; set; }
    public string alert { get; set; }
  }

  public class TeamDetails
  {
    public string employee_id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string designation { get; set; }
    public string encyptemployeeid { get; set; }
    public string feature_image { get; set; }
    public List<AttendanceResponse> attendance { get; set; }
  }

  // offer letter request
  public class OfferLetterRequest
  {
    public int id { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string offer_date { get; set; }
    public string f_name { get; set; }
    public string l_name { get; set; }
    public string mobile_no { get; set; }
    public string designation { get; set; }
    public string department { get; set; }
    public string reporting_to { get; set; }
    public string pay_code { get; set; }
    public string address { get; set; }
    public string city { get; set; }
    public string state { get; set; }
    public string district { get; set; }
    public string gross_salary { get; set; }
  }

  // salary master details
  public class SalaryMasterDto
  {
    public int id { get; set; }
    public string pay_code { get; set; }
    public string hra { get; set; }
    public string basic { get; set; }
    public string allowance { get; set; }
    public string pf { get; set; }
    public string tds { get; set; }
    public string esi { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
  }

  public class SalarySlipDto
  {
    public int id { get; set; }
    public string company_code { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
  }

  public class EmployeeConveyanceDto
  {
    public int id { get; set; }
    public string employee_code { get; set; }
    public decimal mobile_bill { get; set; }
    public decimal conveyance { get; set; }
    public decimal performance_variable { get; set; }
    public decimal other_received { get; set; }
    public decimal advanced_salary { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string name { get; set; }
  }

  public class CountMaster
  {
    public string status { get; set; }
    public string flag { get; set; }
    public CountResponse data { get; set; }
    public string alert { get; set; }
  }
  public class PolicyMasterResponse
  {
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string employee_id { get; set; }
    public string accesskey { get; set; }
  }

  public class CountResponse
  {
    public int totalEmployee { get; set; }
    public int totalMispunch { get; set; }
    public int totalApplyLeave { get; set; }
    public int activeEmployee { get; set; }
    public int inActiveEmployee { get; set; }
    public List<MispuchEmpResponse> mispunchEmployee { get; set; }
  }
  public class MispuchEmpResponse
  {
    public string employee_id { get; set; }
    public string name { get; set; }
    public int miscount { get; set; }
    public string approve_by { get; set; }
    public string approve_by_name { get; set; }
  }
  public class CountRes
  {

    public int count { get; set; }
    public string employee_id { get; set; }
  }

  public class DesboardDataResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public object data { get; set; }
    public string alert { get; set; }
  }

  public class DesboardResponse
  {
    public List<ApplyLeaveResponse> applyleave { get; set; }
    public List<ResetPassword> resetpassword { get; set; }
    public List<MyTeamDTO> team { get; set; }
    public List<HolidayData> holiday { get; set; }
    public string role { get; set; }
  }
  public class MyTeamDTO
  {

    public string employee_id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string designation { get; set; }
    public string department { get; set; }
    public string feature_image { get; set; }
  }

  public class EmployeeDetailsNewDTO
  {
    public int id { get; set; }
    public string employee_id { get; set; }
    public string name { get; set; }
    public string father_name { get; set; }
    public string email { get; set; }
    public string designation { get; set; }
    public string reporting_to_name { get; set; }
    public string repoting_to { get; set; }
    public string mobile_no { get; set; }
    public string pay_code { get; set; }
    public string role { get; set; }
    public int password_count { get; set; }
    public string accesskey { get; set; }
    public int notification { get; set; }
    public string dob { get; set; }
    public string doj { get; set; }
    public string feature_image { get; set; }
    public string forgotId { get; set; }
    public string department { get; set; }
    public string fcm_id { get; set; }
    public string device_id { get; set; }

    public int RoleId { get; set; }
    public int OfficeId { get; set; }
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }

    public string RoleName { get; set; }

    public int? GenderId { get; set; }
    public string EmpCode { get; set; }
    public string EmpBioCode { get; set; }
    public int? GradeId { get; set; }
    public int? LocationId { get; set; }
    public int? JobTypeId { get; set; }
    public int? ProbInMonth { get; set; }
    public int? ProbInYear { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public string SpouseName { get; set; }
    public string EmgcyPersonName { get; set; }
    public string EmgcyPersonMobile { get; set; }

   
    public string attendance_type { get; set; }

    public List<MenuDetails> menuList { get; set; }




  }




  // leave master
  public class LeaveMasterResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public KeyMaster data { get; set; }
    public string alert { get; set; }
  }

  public class KeyMaster
  {
    public List<KeyValueMaster> leave_type { get; set; }
    public List<KeyValueMaster> duration_type { get; set; }
    public List<KeyValueMaster> short_type { get; set; }
  }


  public class GetAttendance_DataQuery
  {
    [Required]
    public string accesskey { get; set; }
    [Required]
    public string employee_id { get; set; }      
    public string from_date { get; set; }   
    public string to_date { get; set; }

    public string month { get; set; }
    public string year { get; set; }

    public DateTime date { get; set; }

  }

  public class GetManagerListQuery
  {
    [Required]
    public string accesskey { get; set; }
    [Required]
    public string employee_id { get; set; }
    

  }

  public class DownloadExcelResponse
  {
    public string type { get; set; }
    public string fileType { get; set; }
    public string employee_id { get; set; }
    public string accesskey { get; set; }
  }
  public class DownloadMasterData
  {
    public string employee_id { get; set; }
    public string employee_name { get; set; }
    public string shift { get; set; }
    public string designation { get; set; }
    public string department { get; set; }
    public string area { get; set; }
    public string in_time { get; set; }
    public string min_time { get; set; }
    public string out_time { get; set; }
    public string mout_time { get; set; }
    public string reason { get; set; }
    public string mreason { get; set; }
    public string no_of_leave { get; set; }
    public string duration_type { get; set; }
    public string leave_id { get; set; }
    public string reporting_to { get; set; }
    public string areporting_to { get; set; }
    public string mreporting_to { get; set; }
    public string email { get; set; }
    public string mobile_no { get; set; }
    public string fathers_name { get; set; }
    public DateTime dob { get; set; }
    public string dobs { get; set; }
    public DateTime doj { get; set; }
    public DateTime created_date { get; set; }
    public DateTime approve_date { get; set; }
    public DateTime? approve_date2 { get; set; }
    public string dojs { get; set; }
    public string status { get; set; }
    public int password_count { get; set; }

  }

  public class HmcRequestShift
  {
    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string pageNo { get; set; }
    public int id { get; set; }
    public string pageSize { get; set; }
    public string status { get; set; }
    public string date { get; set; }
    public string search_result { get; set; }
    public string currentUrl { get; set; }
    public string menuType { get; set; }
    public string pay_code { get; set; }
  }

  public class GetDateRequest
  {  
    public string from_date { get; set; }

  }


  public class EmployeeAttendanceRequest
  {

    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string from_date { get; set; }
    public string to_date { get; set; }
    public string query_for { get; set; }

  }

  public class MonthlyAttendanceResponcesV2
  {
    public object HeaderData { get; set; }
    public object ChildData { get; set; }
  }

  public class PageAccessDTO
  {
    

    public int AccessId { get; set; }
    public string EmployeeId { get; set; }
    public string PageCode { get; set; }
    public int IsAccess { get; set; }

  }

  public class PageAccessVMDTO
  {
    public string employee_id { get; set; }

    
    public string accessKey { get; set; }
    public List<PageAccessDTO> PageAccessDTO { get; set; }

  }

  public class EmployeePageAccessDTO
  {

    public int PageId { get; set; }
    public int AccessId { get; set; }
    public string EmployeeId { get; set; }
    public string employee_code { get; set; }
    public string EmployeeName { get; set; }

    public string PageName { get; set; }
    public string PageCode { get; set; }
    public string ShortName { get; set; }

    public int IsAccess { get; set; }
    public string IsPermission { get; set; }

  }


  public class HmcChatRequest
  {
    public string employee_id { get; set; }
    public string accessKey { get; set; }

    public Int64 meessageid { get; set; }
    public string senderid { get; set; }
    public string receiverid { get; set; }
    public string message { get; set; }

  }


  public class employeechatDTO
  {


    public Int64 meessageid { get; set; }
    public string senderid { get; set; }
    public string receiverid { get; set; }
    public string message { get; set; }
    public DateTime entrydate { get; set; }
    public int IsDeleted { get; set; }
    public int IsRead { get; set; }

    public string sendername { get; set; }
    public string receivername { get; set; }


  }



  public class AppversionDTO
  {
    public string versioncode { get; set; }
    public string status { get; set; }
    public string flag { get; set; }
    public string alert { get; set; }
    public string employee_id { get; set; }
    public string accessKey { get; set; }
    public string mobile_type { get; set; }

  }


  public class HmcHolidayRequest
  {
    public string employee_id { get; set; }
    public string accessKey { get; set; }
    //public string htype { get; set; }

  }

  public class HmcHolidayResponse
  {
    public string status { get; set; }
    public string flag { get; set; }
    public dynamic alert { get; set; }
    public List<HmcHolidayModel> data { get; set; }
  }


  public class HmcHolidayModel
  {
    public int id { get; set; }
    public string title { get; set; }
    public DateTime holiday_date { get; set; } 
   
    public string pay_code { get; set; }
    public string h_type { get; set; }
    public string holidayType { get; set; }

  }


  public class HmcCheckHolidayRequest
  {
    public string accessKey { get; set; }
    public string employee_id { get; set; }
    public string check_date { get; set; }
    //public string htype { get; set; }

  }

  public class HmcCheckHolidayResponse
  {
    public string status { get; set; }
    public string flag { get; set; }

    public string check_flag { get; set; }
    public string check_id { get; set; }

  }

}
