using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace attendancemanagment.Models
{
  public class AttendanceContext : DbContext
  {

    public DbSet<EmployeeMaster> EmployeeMaster { get; set; }
    public DbSet<MenuMaster> MenuMaster { get; set; }
    public DbSet<AccessMenu> AccessMenu { get; set; }
    public DbSet<KeyValueMaster> KeyValueMaster { get; set; }
    public DbSet<ApplyLeaveMasters> ApplyLeaveMasters { get; set; }
    public DbSet<VarifyOtp> VarifyOtp { get; set; }
    public DbSet<AttendanceMaster> AttendanceMaster { get; set; }
    public DbSet<CompaniesMaster> CompaniesMaster { get; set; }
    public DbSet<EmployeeShiftMaster> EmployeeShiftMaster { get; set; }
    public DbSet<AttendanceMispunch> AttendanceMispunch { get; set; }
    public DbSet<Shift> Shift { get; set; }
    public DbSet<HolidayMaster> HolidayMaster { get; set; }
    public DbSet<AttendanceLogMaster> AttendanceLogMaster { get; set; }
    public DbSet<HrsMint> HrsMint { get; set; }
    public DbSet<SalaryMaster> SalaryMaster { get; set; }
    public DbSet<SalarySlip> SalarySlip { get; set; }
    public DbSet<EmployeeConveyance> EmployeeConveyance { get; set; }
    public DbSet<EmployeeFund> EmployeeFund { get; set; }
    public DbSet<EmployeeMachineMaster> EmployeeMachineMaster { get; set; }
    public DbSet<OfferLetterMaster> OfferLetterMaster { get; set; }
    public DbSet<QrCodeMaster> QrCodeMaster { get; set; }
    public DbSet<EmployeeWorkFromHome> EmployeeWorkFromHome { get; set; }
    public DbSet<EmployeeTracking> EmployeeTracking { get; set; }

    public DbSet<PageSetup> PageSetup { get; set; }
    public DbSet<PageAccess> PageAccess { get; set; }
    public DbSet<employeechat> employeechat { get; set; }

  }
}
