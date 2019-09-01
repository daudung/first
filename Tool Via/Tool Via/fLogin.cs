using Maxcare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tool_Via.Properties;
using xNet;

namespace proSignUp
{
    public partial class fLogin : Form
    {
        public static string domain = "http://minsoftware.xyz/keyminsoftware/api.php/";
        public static int softIndex = 9;
        public fLogin()
        {
            InitializeComponent();
        }
        public static string convertToUnSign3(string s)//Chuyen co dau về không dấu
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            if (txtHoTen.Text == "" ||txbSignupLink.Text == "" || txbSignupUser.Text == "" || txbSignupPass.Text == "" || txbSignupEmail.Text == "" || txbSignupPhone.Text == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            //email
            int email = 1;
            try
            {
                MailAddress m = new MailAddress(txbSignupEmail.Text);
                email = 1;
            }
            catch (FormatException)
            {
                email = 0;
            }
            if (email==0)
            {
                MessageBox.Show("Định dạng Email không đúng, vui lòng nhập lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txbSignupEmail.Focus();
                return;
            }
            //link fb
            if (!txbSignupLink.Text.Contains("facebook"))
            {
                MessageBox.Show("Link facebook không đúng, vui lòng nhập lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txbSignupLink.Focus();
                return;
            }


            //max ki tu
            if (txtHoTen.Text.Length > 250)
            {
                MessageBox.Show("Họ tên không được nhập quá 250 ký tự");
                txtHoTen.Focus();
                return;

            }

            if (txbSignupUser.Text.Length >= 251)
            {
                MessageBox.Show("Tài khoản không được nhập quá 250 ký tự");
                txbSignupUser.Focus();
                return;
            }

            if (txbSignupPass.Text.Length >= 251)
            {
                MessageBox.Show("Mật khẩu không được nhập quá 250 ký tự");
                txbSignupPass.Focus();
                return;
            }

            if (txbSignupEmail.Text.Length >= 251)
            {
                MessageBox.Show("Email không được nhập quá 250 ký tự");
                txbSignupEmail.Focus();
                return;
            }

            if (txbSignupPhone.Text.Length >= 251)
            {
                MessageBox.Show("Số điện thoại không được nhập quá 250 ký tự");
                txbSignupPhone.Focus();
                return;
            }

            if (txbSignupLink.Text.Length >= 251)
            {
                MessageBox.Show("Link facebook không được nhập quá 250 ký tự");
                txbSignupLink.Focus();
                return;
            }

            //min ky tu
            if (txbSignupUser.Text.Length <6)
            {
                MessageBox.Show("Tài khoản phải có từ 6 ký tự trở lên!");
                txbSignupUser.Focus();
                return;
            }
            if (txbSignupPass.Text.Length <6)
            {
                MessageBox.Show("Mật khẩu phải có từ 6 ký tự trở lên!");
                txbSignupPass.Focus();
                return;
            }

            if (txbSignupPhone.Text.Length <10)
            {
                MessageBox.Show("Số điện thoại không đúng");
                txbSignupPhone.Focus();
                return;
            }
            

            //ko có dấu
            if (txbSignupUser.Text!= convertToUnSign3(txbSignupUser.Text))
            {
                MessageBox.Show("Tài khoản không được có dấu tiếng tiệt!");
                txbSignupUser.Focus();
                return;
            }
            if (txbSignupPass.Text!= convertToUnSign3(txbSignupPass.Text))
            {
                MessageBox.Show("Mật khẩu không được có dấu tiếng tiệt!");
                txbSignupPass.Focus();
                return;
            }
            if (txbSignupEmail.Text!= convertToUnSign3(txbSignupEmail.Text))
            {
                MessageBox.Show("Email không được có dấu tiếng tiệt!");
                txbSignupEmail.Focus();
                return;
            }


            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            string checkIsHave = request.Get(domain + "CheckAccount?RequestKey=" + md5(md5(GetHDDSerialNumber()) + "handsome") + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&UserName=" + txbSignupUser.Text).ToString();

            if (checkIsHave.Replace("\"", "") == "Error")
            {
                MessageBox.Show("Truy cập không hợp lệ!");
                return;
            }

            if (checkIsHave.Replace("\"", "") == "false")
            {
                string check = request.Post(domain + "Register/?MachineSerial=" + md5(GetHDDSerialNumber()) + "&CustomerName=" + txtHoTen.Text + "&LinkFacebook=" + txbSignupLink.Text + "&UserName=" + txbSignupUser.Text + "&PassWord=" + txbSignupPass.Text + "&Email=" + txbSignupEmail.Text + "&Phone=" + txbSignupPhone.Text).ToString();
                if (check.Replace("\"", "") == "true")
                {
                    MessageBox.Show("Đăng kí tài khoản thành công!");
                    txbUserName.Text = txbSignupUser.Text;
                    txbPass.Text = txbSignupPass.Text;
                    pnlLogin.Visible = true;
                    pnlSignup.Visible = false;
                }
                else
                {
                    MessageBox.Show("Đã có lỗi xảy ra, vui lòng thử lại sau");
                }
            }
            else
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại");
                txbSignupUser.Focus();
            }
        }

        private void btnSwitchSignup_Click(object sender, EventArgs e)
        {
            pnlLogin.Visible = false;
            pnlSignup.Visible = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSwitchSignIn_Click(object sender, EventArgs e)
        {
            pnlLogin.Visible = true;
            pnlSignup.Visible = false;
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
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txbUserName.Text == "" || txbPass.Text == "")
            {
                MessageBox.Show("Vui lòng kiểm điền đầy đủ thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            #region Khai báo request
            xNet.HttpRequest request = new xNet.HttpRequest();
            request.KeepAlive = true;
            request.Cookies = new CookieDictionary();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
            request.UserAgent = Http.ChromeUserAgent();
            #endregion
            string userName = txbUserName.Text;
            string login = request.Get(domain + "Login/?RequestKey=" + md5(md5(GetHDDSerialNumber()) + "handsome") + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&UserName=" + txbUserName.Text + "&PassWord=" + txbPass.Text).ToString();

            if (login.Replace("\"", "") == "Error")
            {
                MessageBox.Show("Truy cập không hợp lệ!");
                return;
            }


            if (login.Equals("true"))
            {

                //login success
                string serial = GetHDDSerialNumber();
                
                Settings.Default.UserName = txbUserName.Text;
                Settings.Default.PassWord = txbPass.Text;
                Settings.Default.isRemember = true;
                Settings.Default.Save();


                string check = request.Get(fLogin.domain + "SelectMachine/?RequestKey=" + md5(md5(GetHDDSerialNumber()) + "handsome") + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString();
                check = check.Replace("\"", "");
                if (check == "Error")
                {
                    MessageBox.Show("Truy cập không hợp lệ!!!");
                    return;
                }

                String[] x = check.Split('|');
                bool kt = true;
                if (request.Get(fLogin.domain + "CheckExpired/?MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString() == "true")
                {
                    kt = false;
                    MessageBox.Show("Phần mềm đã hết hạn sử dụng, vui lòng liên hệ với bộ phận hỗ trợ của MIN SOFTWARE để gia hạn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
                }
                else
                if (request.Get(fLogin.domain + "CheckActive/?MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + fLogin.softIndex).ToString() == "true")
                {
                    kt = false;
                    MessageBox.Show("Thiết bị của bạn đã bị vô hiệu hóa, vui lòng liên hệ với bộ phận hỗ trợ của MIN SOFTWARE để biết thêm thông tin chi tiết!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
                }

                if (x[0] == "true")
                {
                    if (kt)
                    {
                        MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        
                        
                        //Insert data to CustomerLog    
                        string insert = request.Post(fLogin.domain + "AddCustomerLog/?Id_Soft=" + fLogin.softIndex + "&MachineSerial=" + md5(GetHDDSerialNumber()) + "&UserName=" + Settings.Default.UserName).ToString();
                        if (insert.Replace("\"", "") == "true")
                        {
                            mainForm f = new mainForm(Convert.ToDateTime(x[1]).ToString("dd-MM-yyyy"));
                            this.Hide();
                            f.Show();
                        }
                        else
                        {
                            MessageBox.Show("Đã có lỗi xảy ra, vui lòng thử lại sau!");
                        }                                                                       
                    }
                }
                else
                {
                    //add machine to table wait
                    if (MessageBox.Show("Thiết bị của bạn chưa được kích hoạt, bạn có muốn gửi yêu cầu kích hoạt tới Admin?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string checkSent = request.Get(domain + "SelectMachineWait/?RequestKey=" + md5(md5(GetHDDSerialNumber()) + "handsome") + "&MachineSerial=" + md5(serial) + "&SoftIndex=" + softIndex).ToString();

                        if (checkSent.Equals("true"))
                        {
                            MessageBox.Show("Bạn đã gửi yêu cầu trước đó, vui lòng liên hệ admin để được kích hoạt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            try
                            {
                                Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
                            }
                            catch { }
                            return;
                        }
                        else
                        {
                            string sendRequest = request.Post(domain + "AddMachine/?MachineSerial=" + md5(GetHDDSerialNumber()) + "&SoftIndex=" + softIndex + "&UserName=" + userName).ToString();
                            if (sendRequest.Replace("\"", "") == "true")
                            {
                                MessageBox.Show("Gửi yêu cầu thành công, vui lòng liên hệ admin để được kích hoạt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                try
                                {
                                    Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
                                }
                                catch { }
                            }
                            else
                            {
                                MessageBox.Show("Đã có lỗi xảy ra, vui lòng thử lại sau!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Tài khoản hoặc mật khẩu không đúng, vui lòng kiểm tra lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetHDDSerialNumber(string drive = null)
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
        //private string GetIp()
        //{
        //    string ipReturn = "";
        //    try
        //    {
        //        #region Khai báo request
        //        RequestHTTP request = new RequestHTTP();
        //        #endregion

        //        string checkIP = request.Request("GET","https://whoer.net/fr").ToString();
        //        Match ip = Regex.Match(checkIP, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        //        if (ip.Success)
        //        {
        //            ipReturn = ip.Value.Trim();
        //        }
        //    }
        //    catch
        //    {
        //        ipReturn = "Lỗi lấy IP";
        //    }
        //    return ipReturn;
        //}
        private void fLogin_Load(object sender, EventArgs e)
        {
            txbMachine.Text = md5(GetHDDSerialNumber());
            if (Settings.Default.isRemember)
            {
                txbUserName.Text =Settings.Default.UserName;
                txbPass.Text = Settings.Default.PassWord;
                ckbRemember.Checked = Settings.Default.isRemember;
                Settings.Default.Save();
            }
        }

        private void ptbShowPass_Click(object sender, EventArgs e)
        {
            if (txbPass.isPassword == true)
            {
                txbPass.isPassword = false;
                ptbShowPass.Image = new Bitmap("images/Eye_50px.png");
            }
            else
            {
                txbPass.isPassword = true;
                ptbShowPass.Image = new Bitmap("images/Invisible.png");
            }
        }

        private void ptbCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txbMachine.Text);
        }

        private void linkFacebook_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("chrome.exe", linkFacebook.Text);
            }
            catch { }
        }

        private void linkYoutobe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("chrome.exe", linkYoutobe.Text);
            }
            catch { }
        }

        private void linkWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("chrome.exe", linkWeb.Text);
            }
            catch { }
        }

        private void txbSignupPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txbSignupPass_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            if ((e.KeyChar == (char)Keys.Space) || ((!char.IsDigit(e.KeyChar)) && (!char.IsLetter(e.KeyChar)) && (!char.IsControl(e.KeyChar))))
            {
                MessageBox.Show("Tài khoản chỉ bao gồm chữ cái và chữ số, không chứa ký tự đặc biệt!");
                e.Handled = true;
            }
        }

        private void txbSignupPass_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == (char)Keys.Space) || ((!char.IsDigit(e.KeyChar)) && (!char.IsLetter(e.KeyChar)) && (!char.IsControl(e.KeyChar))))
            {
                MessageBox.Show("Tài khoản chỉ bao gồm chữ cái và chữ số, không chứa ký tự đặc biệt!");
                e.Handled = true;
            }
            
        }

        private void txtHoTen_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((!char.IsLetter(e.KeyChar)) && (!char.IsControl(e.KeyChar))) && (e.KeyChar != (char)Keys.Space))
            {
                MessageBox.Show("Họ tên chỉ bao gồm chữ cái, không chứa ký tự đặc biệt!");
                e.Handled = true;
            }
        }

        private void txtHoTen_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void linkForgetPass_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ với bộ phận hỗ trợ của MIN SOFTWARE để lấy lại mật khẩu!");
            Process.Start("chrome.exe", "https://www.facebook.com/minsoftware.vn/");
        }
    }
}
