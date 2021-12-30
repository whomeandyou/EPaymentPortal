using System.IO;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using TMLM.EPayment.Batch.Helpers;

namespace TMLM.EPayment.Batch.Service
{
    public static class EmailServices
    {
       public static void SendEmail(string subject, string body, string to, string SuccessFilePath, string FailFilePath)
        {
            using (MailMessage mm = new MailMessage(ConfigurationHelper.GetValue("FromEmail"), to))
            {
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = false;
                Attachment attachmentSucessFile = new Attachment(SuccessFilePath);
                attachmentSucessFile.Name = Path.GetFileName(SuccessFilePath);
                mm.Attachments.Add(attachmentSucessFile);

                Attachment attachmentFailFile = new Attachment(FailFilePath);
                attachmentFailFile.Name = Path.GetFileName(FailFilePath);
                mm.Attachments.Add(attachmentFailFile);
                var isSSL = false;
                if (ConfigurationHelper.GetValue("SSL").ToLower().Equals("true"))
                {
                    isSSL = true;
                }
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationHelper.GetValue("Host"),
                    EnableSsl = isSSL
                };
                NetworkCredential NetworkCred = new NetworkCredential(ConfigurationHelper.GetValue("Username"), ConfigurationHelper.GetValue("Password"));
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = int.Parse(ConfigurationHelper.GetValue("Port"));
                smtp.Send(mm);
            }
        }

        public static void SendEmailWithOutAttachment(string subject, string body, string to)
        {
            using (MailMessage mm = new MailMessage(ConfigurationHelper.GetValue("FromEmail"), to))
            {
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = false;

                var isSSL = false;
                if (ConfigurationHelper.GetValue("SSL").ToLower().Equals("true"))
                {
                    isSSL = true;
                }
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationHelper.GetValue("Host"),
                    EnableSsl = isSSL
                };
                NetworkCredential NetworkCred = new NetworkCredential(ConfigurationHelper.GetValue("Username"), ConfigurationHelper.GetValue("Password"));
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = int.Parse(ConfigurationHelper.GetValue("Port"));
                smtp.Send(mm);
            }
        }

        public static void SendEmailDataReconsolidation(string subject, string body, string to, string SuccessFilePath)
        {
            using (MailMessage mm = new MailMessage(ConfigurationHelper.GetValue("FromEmail"), to))
            {
                mm.Subject = subject;
                mm.Body = body;
                mm.IsBodyHtml = false;
                Attachment attachmentSucessFile = new Attachment(SuccessFilePath);
                attachmentSucessFile.Name = Path.GetFileName(SuccessFilePath);
                mm.Attachments.Add(attachmentSucessFile);

                var isSSL = false;
                if (ConfigurationHelper.GetValue("SSL").ToLower().Equals("true"))
                {
                    isSSL = true;
                }
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationHelper.GetValue("Host"),
                    EnableSsl = isSSL
                };
                NetworkCredential NetworkCred = new NetworkCredential(ConfigurationHelper.GetValue("Username"), ConfigurationHelper.GetValue("Password"));
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = int.Parse(ConfigurationHelper.GetValue("Port"));
                smtp.Send(mm);
            }
        }
    }
}
