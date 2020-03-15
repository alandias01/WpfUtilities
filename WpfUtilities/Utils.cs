using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Linq.Expressions;

namespace WpfUtilities
{
    public class Utils
    {
        static string logFileLocation = AppDomain.CurrentDomain.BaseDirectory + "logfile.txt";

        /// <summary>
        /// Use this method to store any settings for your application
        /// </summary>
        /// <typeparam name="T">They type of the object that will get serialized for the 3rd argument</typeparam>
        /// <param name="settingsDirectoryName">The Name you want to give to the folder where settings will be stored.  For windows 7, it gets stored under \Users\%username%\AppData\Local\%settingsDirectoryName%</param>
        /// <param name="settingsFile">The filename you want it to have. Ex. filename.xml</param>
        /// <param name="u">This will be the object that will get serialized</param>
        public static void SaveSetting<T>(string settingsDirectoryName, string settingsFile, T u)
        {
            string settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            settingsDirectory += "\\" + settingsDirectoryName + "\\";
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            settingsFile = settingsDirectory + settingsFile;
                        
            using (StreamWriter sw = new StreamWriter(settingsFile, false))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(sw, u);
            }
 
        }

        public static void SaveSettingLocal<T>(string settingsFile, T u)
        {
            
                using (StreamWriter sw = new StreamWriter(settingsFile, false))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    xs.Serialize(sw, u);
                }
        }

        public static T LoadSetting<T>(string settingsDirectoryName, string settingsFile)
        {
            string settingsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            settingsDirectory += "\\" + settingsDirectoryName + "\\";
            settingsFile = settingsDirectory + settingsFile;

            T u;

            try
            {
                using (StreamReader sr = new StreamReader(settingsFile))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    u = (T)xs.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                throw;
            }

            return u;
        }

       

        public static T LoadSettingLocal<T>(string settingsFile)
        {
            T u;

            try
            {
                using (StreamReader sr = new StreamReader(settingsFile))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    u = (T)xs.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                Utils.LogError(e.Message);
                throw;
            }

            return u;
        }

        private static void CheckLogsize()
        {
            try
            {
                FileInfo fi = new FileInfo(logFileLocation);
                if (fi.Exists && fi.Length > 5000000)
                {
                    string newFileName = Path.GetDirectoryName(logFileLocation) + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_") + fi.Name;
                    File.Move(logFileLocation, newFileName);
                }
            }
            catch (Exception e)
            {
                //LogError(e.Message);  //DONOT log as it will run checkLogSize in an infinite loop
            }
        }
                
        public static void LogError(string msg)
        {
            CheckLogsize();
            msg = DateTime.Now.ToString() + " " + Environment.UserName + " ERROR: " + msg + Environment.NewLine;
            File.AppendAllText(logFileLocation, msg);
        }

        public static void LogTrace(string msg)
        {
            msg = DateTime.Now.ToString() + " " + Environment.UserName + " TRACE: " + msg + Environment.NewLine;
            File.AppendAllText(logFileLocation, msg);
        }

        //If html is set to true, you cannot use \n for endline, use <br> since it's html
        public static void SendEmail(string emailAddress, string subject, string message, bool html)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();
                mailMsg.IsBodyHtml = html;
                string recipients = emailAddress;
                string[] recipientsList = recipients.Split(';');
                foreach (string recipient in recipientsList)
                {
                    mailMsg.To.Add(new MailAddress(recipient));
                }
                mailMsg.Subject = subject;
                mailMsg.Body = message;
                mailMsg.From = new MailAddress("taskmaster@mapleusa.com");
                SmtpClient client = new SmtpClient("nightcrawler", 25);
                client.Send(mailMsg);
            }

            catch (Exception ex) { Utils.LogError("Email Error" + ex.Message); }

        }

        //public static DateTime GetLastTradeDate()
        //{
        //    string cs = "Data Source=Newton;Initial Catalog=StockLoan;Integrated Security=True";

        //    DateTime lastTradeDate = DateTime.Today;
        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(cs))
        //        {

        //            conn.Open();

        //            SqlCommand cmd = new SqlCommand("spGetLastTradeDate", conn);
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //            string country = "US";

        //            SqlParameter cc = new SqlParameter("@CountryShortCode", SqlDbType.VarChar, 10);
        //            cc.Direction = ParameterDirection.Input;
        //            cc.Value = country;
        //            cmd.Parameters.Add(cc);

        //            SqlParameter p = new SqlParameter("@Date", SqlDbType.DateTime);
        //            p.Direction = ParameterDirection.Output;
        //            cmd.Parameters.Add(p);

        //            cmd.UpdatedRowSource = UpdateRowSource.OutputParameters;

        //            cmd.ExecuteNonQuery();

        //            //string newRegionDescription = ((DateTime)cmd.Parameters["@Date"].Value).ToString();

        //            DateTime dt = (DateTime)cmd.Parameters["@Date"].Value;

        //            lastTradeDate = dt;

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogError(e.Message);
        //        throw;
        //    }

        //    return lastTradeDate;
        //}

        /// <summary>
        /// Ex.
        /// string Name {get; set;}
        /// string pname=GetPropertyName(() => Name);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            return (propertyExpression.Body as MemberExpression).Member.Name;
        }
    }    
}
