using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using DotNet.Utilities;
using Alog_WSKJSD;
using System.IO;

namespace ClsFtpService
{
    public partial class Form1 : Form
    {
        static string NSFTPAddress = ClsLog.GetAppSettings("NSFTPAddress");
        static string NSFTPAddressIP = ClsLog.GetAppSettings("NSFTPAddressIP");
        static string NSFTPAddressPort = ClsLog.GetAppSettings("NSFTPAddressPort");
        static string FtpUserName = ClsLog.GetAppSettings("FtpUserName");
        static string FtpPwd = ClsLog.GetAppSettings("FtpPwd");
        static string FtpServiceUpPath = ClsLog.GetAppSettings("FtpServiceUpPath");
        static string FtpUpPath = ClsLog.GetAppSettings("FtpUpPath");
        static string FtpUpPathBak = ClsLog.GetAppSettings("FtpUpPathBak");
        static string FtpServiceDownPath = ClsLog.GetAppSettings("FtpServiceDownPath");
        static string HZPath = ClsLog.GetAppSettings("HZPath");
        static string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Uri u = new Uri(NSFTPAddressIP + ":" + NSFTPAddressPort);
            //u.f
            ClsLog.AppendLog("上传FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
            clsFTP ftp = new clsFTP(u, FtpUserName, FtpPwd);
            ftp.DownloadFileAsync(FtpServiceDownPath, HZPath);
            //ftp.RemoteHost = NSFTPAddressIP;
            //ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
            //ftp.RemotePath = FtpServiceUpPath;
            //ftp.RemoteUser = FtpUserName;
            //ftp.RemotePass = FtpPwd;
            //ftp.Put(FtpUpPath, "*.xml", FtpUpPathBak);
            ClsLog.AppendLog("上传FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
           
        }
    }
}
