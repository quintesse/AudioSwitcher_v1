﻿using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using FortyOne.AudioSwitcher.Helpers;
using FortyOne.AudioSwitcher.Properties;

namespace FortyOne.AudioSwitcher
{
    public partial class ExceptionDisplayForm : Form
    {
        private readonly Exception exception;

        public ExceptionDisplayForm()
        {
            InitializeComponent();
        }

        public ExceptionDisplayForm(string title, string text, Exception ex)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            exception = ex;

            //if(Form.ActiveForm != null)
            //    Form.ActiveForm.Cursor = Cursors.Default;

            Text = title;
            //if (!string.IsNullOrEmpty(text))
            //    this.txtErrorDetails.Text = text;
            //else
            //    this.txtErrorDetails.Text = ex.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            try
            {
                var p = new Ping();
                p.Send(new Uri(Resources.WebServiceURL).Host);

                //using (Cursor = Cursors.WaitCursor)
                using (AudioSwitcherService.AudioSwitcher client = ConnectionHelper.GetAudioSwitcherProxy())
                {
                    if (client == null)
                        return;

                    Assembly asm = Assembly.GetExecutingAssembly();
                    object[] attribs = (asm.GetCustomAttributes(typeof (GuidAttribute), true));
                    string guid = (attribs[0] as GuidAttribute).Value;

                    string body = "";
                    body += "Audio Switcher" + Environment.NewLine;
                    body += "Version: " + Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine;
                    body += "Operating System: " + Environment.OSVersion + (IntPtr.Size == 8 ? " 64-bit" : " 32-bit") +
                            Environment.NewLine;
                    body += "Administrator Privileges: " + IsUserAdministrator() + Environment.NewLine;
                    body += "Client Email Address: " + txtEmail.Text + Environment.NewLine;

                    string x = client.SendBugReport(guid, txtErrorDetails.Text, body, exception.ToString());

                    if (x != "")
                    {
                        MessageBox.Show(this, "Bug Report Received. Thank you!\r\nBug ID: " + x);
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(this, "Error Sending bug report. Please try again later");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error Sending bug report. Please try again later");
            }
        }
    }
}