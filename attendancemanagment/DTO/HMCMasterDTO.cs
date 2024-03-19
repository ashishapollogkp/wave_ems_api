using attendancemanagment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace attendancemanagment.dto
{
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
}
