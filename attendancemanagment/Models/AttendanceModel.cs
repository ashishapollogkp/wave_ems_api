using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace attendancemanagment.Models
{
  public class EmployeeLogin
  {
    [Display(Name = "Employee Id")]
    [Required]
    public string employee_id { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string password { get; set; }
  }

  // employee master
  [Table("EmployeeMaster")]
  public class EmployeeMaster
  {
    [Key]
    public int id { get; set; }
    [Required]
    public string name { get; set; }
    [Required]
    public string employee_id { get; set; }
    public string mobile_no { get; set; }
    public string gender { get; set; }
  
    public string email { get; set; }
    public string password { get; set; }
    [Required]
    public string status { get; set; }
    public string employeement_type { get; set; }
    public DateTime dob { get; set; }
    public DateTime doj { get; set; }
    public DateTime dol { get; set; }
    public int password_count { get; set; }
    public string designation { get; set; }
    public string department { get; set; }
    public string shift { get; set; }
    public string blood_group { get; set; }
    public string reporting_to { get; set; }
    public string father_name { get; set; }
    public string pay_code { get; set; }
    public string feature_image { get; set; }
    public Boolean office_emp { get; set; }
    public Boolean marketing_emp { get; set; }
    public string el { get; set; }
    public string cl { get; set; }
    public string sl { get; set; }
    public string a_el { get; set; }
    public string a_cl { get; set; }
    public string a_sl { get; set; }
    public string b_el { get; set; }
    public string b_cl { get; set; }
    public string b_sl { get; set; }
    public string aadhar_number { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string role { get; set; }
    public string bank_name { get; set; }
    public string acc_no { get; set; }
    public string ifsc_code { get; set; }
    public decimal employee_salary { get; set; }
    public string pan_no { get; set; }
    public string uan_no { get; set; }
    public string attendance_type { get; set; }

  }

  // employee master
  [Table("Menu")]
  public class MenuMaster
  {
    [Key]
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public int sub_menu_id { get; set; }
    public string icons { get; set; }
  }

  // hrs mint
  [Table("HrsMint")]
  public class HrsMint
  {
    [Key]
    public int id { get; set; }
    public string hrs_mint { get; set; }
    public string type { get; set; }
  }

  [Table("EmployeeMachine")]
  public class EmployeeMachineMaster
  {
    [Key]
    public int id { get; set; }
    public string employee_id { get; set; }
    public int machine_id { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string pay_code { get; set; }
  }

  // employee master
  [Table("AttendanceLog")]
  public class AttendanceLogMaster
  {
    [Key]
    public int id { get; set; }
    public string employee_id { get; set; }
    public DateTime date { get; set; }
    public string shift { get; set; }
    public string status { get; set; }
    public string day { get; set; }
    public string in_lat { get; set; }
    public string in_long { get; set; }
    public string feature_image { get; set; }
    public string address { get; set; }
  }

  [Table("EmployeeTracking")]
  public class EmployeeTracking
  {
    [Key]
    public int id { get; set; }
    public string employee_id { get; set; }
    public DateTime tracking_date { get; set; }
    public string tracking_lat { get; set; }
    public string tracking_long { get; set; }
    public string tracking_address { get; set; }
  }

  // employee master
  [Table("EmployeeShift")]
  public class EmployeeShiftMaster
  {
    [Key]
    public int id { get; set; }
    public string employee_id { get; set; }
    public DateTime from_date { get; set; }
    public DateTime to_date { get; set; }
    public string shift { get; set; }
    public string pay_code { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
  }

  [Table("AccessMenu")]
  public class AccessMenu
  {
    [Key]
    public int id { get; set; }
    public int role_id { get; set; }
    public int menu_id { get; set; }
  }

  [Table("KeyValue")]
  public class KeyValueMaster
  {
    [Key]
    public int id { get; set; }
    public string key_type { get; set; }
    public string key_description { get; set; }
    public string key_code { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string pay_code { get; set; }

  }


  [Table("Attendance")]
  public class AttendanceMaster
  {
    [Key]
    public int id { get; set; }
    [Required]
    public string employee_id { get; set; }
    [Required]
    public string day { get; set; }
    public string shift { get; set; }
    public string in_time { get; set; }
    public string out_time { get; set; }
    public string mis { get; set; }
    public string status { get; set; }
    public string late { get; set; }
    public string early { get; set; }
    public string absent { get; set; }
    public Double total_hrs { get; set; }
    [Required]
    public DateTime date { get; set; }
    public Boolean ticket_status { get; set; }
    public string in_latitude { get; set; }
    public string in_longitude { get; set; }
    public string out_latitude { get; set; }
    public string out_longitude { get; set; }
    public string feature_image { get; set; }
    //public string department { get; set; }
    public string in_address { get; set; }
    public string out_address { get; set; }

  }

  [Table("AttendanceMispunch")]
  public class AttendanceMispunch
  {
    [Key]
    public int id { get; set; }
    [Required]
    public string employee_id { get; set; }
    //[Required]
    public int attendance_id { get; set; }
    [Required]
    public DateTime date { get; set; }
    [Required]
    public string in_time { get; set; }
    [Required]
    public string out_time { get; set; }
    //[Required]
    public DateTime apply_date { get; set; }
    [Required]
    public string assign_by { get; set; }
    [Required]
    public string status { get; set; }
    [Required]
    public string reason { get; set; }
    [Required]
    public string shift { get; set; }
    public DateTime approve_date { get; set; }
    public string token { get; set; }
    public Boolean att_flag { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string pay_code { get; set; }
  }

  [Table("Holiday")]
  public class HolidayMaster
  {
    [Key]
    public int id { get; set; }
    public string title { get; set; }
    public DateTime holiday_date { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string pay_code { get; set; }
    public string h_type { get; set; }
    public int YearName { get; set; }

  }


  [Table("ApplyLeave")]
  public class ApplyLeaveMasters
  {
    [Key]
    public int id { get; set; }
    [Required]
    public string employee_id { get; set; }
    [Required]
    public DateTime apply_date { get; set; }
    [Required]
    public string shift { get; set; }
    [Required]
    public int leave_Type { get; set; }
    public int duration_type { get; set; }
    [Required]
    public DateTime from_date { get; set; }
    [Required]
    public DateTime to_date { get; set; }
    public string reason { get; set; }
    public decimal no_of_leave { get; set; }
    public string status { get; set; }
    public string assign_by { get; set; }
    public DateTime approve_date { get; set; }
    public Boolean leave_flag { get; set; }
    public string token { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
    public string pay_code { get; set; }
    public string from_time { get; set; }
    public string to_time { get; set; }

  }

  [Table("Companies")]
  public class CompaniesMaster
  {
    [Key]
    public int id { get; set; }
    public string name { get; set; }
    public string company_code { get; set; }
    public string company_address { get; set; }
    public string companye_mail { get; set; }
    public string companye_contact_no { get; set; }
    public string company_website { get; set; }
    public string status { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }

  }

  [Table("Shift")]
  public class Shift
  {
    [Key]
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
    public int geo_distance { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }

  }

  // salary master
  [Table("SalaryMaster")]
  public class SalaryMaster
  {
    [Key]
    public int id { get; set; }
    public string hra { get; set; }
    public string basic { get; set; }
    public string allowance { get; set; }
    public string pf { get; set; }
    public string tds { get; set; }
    public string esi { get; set; }
    public string pay_code { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }

  }

  // salary slip
  [Table("SalarySlip")]
  public class SalarySlip
  {
    [Key]
    public int id { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string pay_code { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }

  }

  // employee convyance
  [Table("EmployeeConveyance")]
  public class EmployeeConveyance
  {
    [Key]
    public int id { get; set; }
    public string month { get; set; }
    public string employee_id { get; set; }
    public string year { get; set; }
    public decimal mobile_bill { get; set; }
    public decimal conveyance { get; set; }
    public decimal performance_variable { get; set; }
    public decimal other_received { get; set; }
    public decimal advanced_salary { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }

  }

  // employee fund
  [Table("EmployeeFund")]
  public class EmployeeFund
  {
    [Key]
    public int id { get; set; }
    public string month { get; set; }
    public string year { get; set; }
    public string employee_id { get; set; }
    public decimal epf { get; set; }
    public decimal tds { get; set; }
    public decimal esi { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }

  }

  [Table("VarifyOtp")]
  public class VarifyOtp
  {
    [Key]
    public int id { get; set; }
    public string employee_id { get; set; }
    public string otp { get; set; }
    public string mobile_no { get; set; }
  }

  [Table("OfferLetter")]
  public class OfferLetterMaster
  {
    [Key]
    public int id { get; set; }
    public DateTime offer_date { get; set; }
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
    public Boolean appointment { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
  }

  [Table("QrCode")]
  public class QrCodeMaster
  {
    [Key]
    public int id { get; set; }
    public string pay_code { get; set; }
    public string qr_code { get; set; }
    public byte[] qrcode_image { get; set; }
    public string modify_by { get; set; }
    public DateTime modify_date { get; set; }
  }

  // work from master
  [Table("EmployeeWorkFromHome")]
  public class EmployeeWorkFromHome
  {
    [Key]
    public int id { get; set; }
    public string employee_id { get; set; }
    public DateTime allow_date { get; set; }
  }


  [Table("PageSetup")]
  public class PageSetup
  {
    [Key]
    public int PageId { get; set; }
    public string PageCode { get; set; }
    public string PageName { get; set; }
    public string ShortName { get; set; }
    public int IsActive { get; set; }
    
  }

  [Table("PageAccess")]
  public class PageAccess
  {
    [Key]
    public int AccessId { get; set; }
    public string EmployeeId { get; set; }
    public string PageCode { get; set; }
    public int IsAccess { get; set; }

  }

  [Table("employeechat")]
  public class employeechat
  {
    [Key]
    public Int64 meessageid { get; set; }
    public string senderid { get; set; }
    public string receiverid { get; set; }
    public string message { get; set; }
    public DateTime entrydate { get; set; }
    public int IsDeleted { get; set; }
    public int IsRead { get; set; }
    

  }

}
