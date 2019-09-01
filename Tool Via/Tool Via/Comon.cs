using HttpRequest;
using Maxcare;
using proSignUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tool_Via
{
    public class Comon
    {
        public static string GetIdKey(string a = "")
        {
            if (a.Equals(""))
                a = GetIdCpu();
            return EncodeMD5(PrivateKey(a));
        }

        public static string GetIdCpu()
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["processorID"].Value.ToString();
                break;
            }
            return cpuInfo;
        }

        public static string EncodeMD5(string txt)
        {
            String str = "";
            Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(txt);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            buffer = md5.ComputeHash(buffer);
            foreach (Byte b in buffer)
            {
                str += b.ToString("X2");
            }
            return str.ToLower();
        }
        private static string PrivateKey(string s)
        {
            return s + "minsoftware.tk";
        }

        public static string GetAccessKey(string a = "")
        {
            if (a.Equals(""))
                a = GetIdKey();
            return EncodeMD5(PrivateKey(a));
        }

        public static string CheckKeyAndLogin(string userName, string passWord, string idKey = "", int softIndex = 9)
        {
            string str = "";
            if (idKey.Equals(""))
                idKey = GetIdKey();
            #region Khai báo request
            RequestHTTP request = new RequestHTTP();
            request.SetSSL(System.Net.SecurityProtocolType.Tls12);
            request.SetKeepAlive(true);
            request.SetDefaultHeaders(new string[]
            {
                    "content-type: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    "user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36",
            });
            #endregion
            string link = fLogin.domain + "CheckAll/?RequestKey=" + GetAccessKey() + "&MachineSerial=" + idKey + "&UserName=" + userName + "&PassWord=" + passWord + "&SoftIndex=" + softIndex;
            str = request.Request("GET", link).ToString();
            return str;
        }

        public static bool IsValidMail(string emailaddress)
        {
            try
            {
                System.Net.Mail.MailAddress m = new System.Net.Mail.MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
