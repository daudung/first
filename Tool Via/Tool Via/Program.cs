using Maxcare;
using proSignUp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tool_Via.Properties;
using xNet;

namespace Tool_Via
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainForm(null));
            //source by Ken Viruss - Nguyen Dac Tai
            //https://tienichmmo.net
            // group facebook : Tool không ngon, xóa group !
            //#region Khai báo request
            //xNet.HttpRequest request = new xNet.HttpRequest();
            //request.KeepAlive = true;
            //request.Cookies = new CookieDictionary();
            //request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            //request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            //request.UserAgent = Http.ChromeUserAgent();
            //#endregion

            //try
            //{
            //    string check = request.Get(fLogin.domain + "SelectMachine/?RequestKey=" + md5(md5(GetHDDSerialNumber()) + "handsome") + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString();
            //    check = check.Replace("\"", "");
            //    if (check == "Error")
            //    {
            //        MessageBox.Show("Truy cập không hợp lệ!!!");
            //        return;
            //    }

            //    String[] x = check.Split('|');
            //    bool kt = true;
            //    if (request.Get(fLogin.domain + "CheckExpired/?MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString() == "true")
            //    {
            //        kt = false;
            //        MessageBox.Show("Phần mềm đã hết hạn sử dụng, vui lòng liên hệ với bộ phận hỗ trợ của MIN SOFTWARE để gia hạn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //        Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
            //    }
            //    else
            //    if (request.Get(fLogin.domain + "CheckActive/?MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString() == "true")
            //    {
            //        kt = false;
            //        MessageBox.Show("Thiết bị của bạn đã bị vô hiệu hóa, vui lòng liên hệ với bộ phận hỗ trợ của MIN SOFTWARE để biết thêm thông tin chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //        Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
            //    }

            //    if (x[0] == "true" && kt == true)
            //    {
            //        //Insert data to CustomerLog    
            //        string insert = request.Post(fLogin.domain + "AddCustomerLog/?Id_Soft=" + fLogin.softIndex + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&UserName=" + Settings.Default.UserName).ToString();
            //        if (insert.Replace("\"", "") == "true")
            //        {
            //            Application.Run(new mainForm(Convert.ToDateTime(x[1]).ToString("dd-MM-yyyy")));
            //        }
            //        else
            //        {
            //            //MessageBox.Show("Đã có lỗi xảy ra, vui lòng thử lại sau");
            //            Application.Run(new fLogin());
            //        }
            //    }
            //    else
            //    {
            //        Application.Run(new fLogin());
            //    }
        //}
        //    catch
        //    {
        //        MessageBox.Show("Không thể kết nối đến máy chủ, vui lòng thử lại!", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        }

        

        public static string GetIdMarchine()
        {
            return md5(GetHDDSerialNumber());
        }

        public static bool CheckMachine()
        {
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            string ck = request.Get(fLogin.domain + "CheckMachine/?RequestKey=" + md5(md5(GetHDDSerialNumber()) + "handsome") + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString();
            if (ck == "true")
            {
                return true;
            }
            return false;
        }

        public static string GetHDDSerialNumber(string drive = null)
        {
            //check to see if the user provided a drive letter
            //if not default it to "C"
            if (drive == "" || drive == null)
            {
                drive = "C";
            }
            //create our ManagementObject, passing it the drive letter to the
            //DevideID using WQL
            ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + drive + ":\"");
            //bind our management object
            disk.Get();
            //return the serial number
            return disk["VolumeSerialNumber"].ToString();
        }
        public static string md5(string txt)
        {
            String str = "";
            Byte[] buffer = System.Text.Encoding.UTF8.GetBytes(txt);
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            buffer = md5.ComputeHash(buffer);
            foreach (Byte b in buffer)
            {
                str += b.ToString("X2");
            }
            return str.ToLower();
        }

        
    }
}
