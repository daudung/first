using HttpRequest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tool_Via;
using Tool_Via.Properties;

namespace maxcare
{
    public partial class fActive : Form
    {
        int typeError = 0;
        public fActive(int typeError,string idKey)
        {
            InitializeComponent();
            this.typeError = typeError;
            if (typeError == 1)
            {
                lblStatus.Text = "Vui lòng đăng nhập để sử dụng phần mềm!!!";
            }else if(typeError == 2)
            {
                lblStatus.Text = "Thiết bị của bạn chưa được kích hoạt!!!";
            }else if (typeError == 3)
            {
                lblStatus.Text = "Thiết bị của bạn đã hết hạn sử dụng!!!";
            }
            lblKey.Text = idKey;
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (txbUserName.Text == "" || txbPassword.Text == "")
            {
                MessageBox.Show("Vui lòng kiểm điền đầy đủ thông tin", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (Comon.IsValidMail(txbUserName.Text)==false)
            {
                lblStatus.Invoke((MethodInvoker)delegate ()
                {
                    lblStatus.Text = "Email bạn nhập không đúng!!!";
                });
                return;
            }
            new Thread(new ThreadStart(() =>
            {
                lblStatus.Invoke((MethodInvoker)delegate ()
                {
                    lblStatus.Text = "Đang kiểm tra đăng nhập...";
                });
                btnLogin.Invoke((MethodInvoker)delegate ()
                {
                    btnLogin.Enabled = false;
                });
                string result = Comon.CheckKeyAndLogin(txbUserName.Text, txbPassword.Text, "");
                JObject objStt = JObject.Parse(result);
                if (objStt["account"].ToString().Equals("True") && objStt["device"].ToString().Equals("True") && objStt["expried"].ToString().Equals("False"))
                {
                    lblStatus.Invoke((MethodInvoker)delegate ()
                    {
                        lblStatus.Text = "Đăng nhập thành công!";
                    });
                    MessageBox.Show("Thiết bị của bạn đã được kích hoạt, cảm ơn đã sử dụng dịch vụ minsoftware", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Settings.Default.UserName = txbUserName.Text;
                    Settings.Default.PassWord = txbPassword.Text;
                    Settings.Default.Save();
                    Environment.Exit(0);
                }
                else
                {
                    int typeError = 0;
                    //check account invalid
                    if (objStt["account"].ToString().Equals("False"))
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
                        if (typeError == 1)
                        {
                            lblStatus.Invoke((MethodInvoker)delegate ()
                            {
                                lblStatus.Text = "Tài khoản hoặc mật khẩu không đúng!!!";
                            });
                        }
                        else if (typeError == 2)
                        {
                            lblStatus.Invoke((MethodInvoker)delegate ()
                            {
                                lblStatus.Text = "Thiết bị của bạn chưa được kích hoạt!!!";
                            });
                        }
                        else if (typeError == 3)
                        {
                            lblStatus.Invoke((MethodInvoker)delegate ()
                            {
                                lblStatus.Text = "Thiết bị của bạn đã hết hạn sử dụng!!!";
                            });
                        }
                        try
                        {
                            Process.Start("chrome.exe", "http://minsoftware.xyz");
                        }
                        catch
                        {
                            Process.Start("http://minsoftware.xyz");
                        }
                    }

                }
                btnLogin.Invoke((MethodInvoker)delegate ()
                {
                    btnLogin.Enabled = true;
                });
            })).Start();
            
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

        private void FActive_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("chrome.exe", "http://minsoftware.xyz");
            }
            catch {
                Process.Start("http://minsoftware.xyz");
            }
        }

        private void LblKey_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(lblKey.Text);
        }
    }
}
