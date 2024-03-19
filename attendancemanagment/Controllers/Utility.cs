using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using attendancemanagment.Models;
using System.Net.Mail;
using System.Configuration;

namespace attendancemanagment.Utilities
{
    public class Utility
    {
        private static AttendanceContext db = new AttendanceContext();

        public static void sendMaill(string emailId, string body, string subject)
        {
            string mailId = ConfigurationManager.AppSettings["mailId"];
            string mailPass = ConfigurationManager.AppSettings["mailPass"];
            string MailSMTP = ConfigurationManager.AppSettings["MailSMTP"];

            MailMessage mail = new MailMessage();
            mail.To.Add(emailId);
            mail.From = new MailAddress(mailId);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = MailSMTP;
            smtp.Credentials = new System.Net.NetworkCredential
                 (mailId, mailPass);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }

    }
}