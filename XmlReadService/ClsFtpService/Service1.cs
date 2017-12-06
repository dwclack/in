using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using DotNet.Utilities;
using Alog_WSKJSD;
using System.IO;

namespace ClsFtpService
{
    public partial class Service1 : ServiceBase
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
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ClsLog.AppendLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                //======获取并设置定时间隔
                int tmrtimer1 = 300000;
                int tmrtimer2 = 300000;
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("FtpUpInterval")))
                {
                    tmrtimer1 = Convert.ToInt32(ClsLog.GetAppSettings("FtpUpInterval"));
                }
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("FtpDownInterval")))
                {
                    tmrtimer2 = Convert.ToInt32(ClsLog.GetAppSettings("FtpDownInterval"));
                }
                //======获取报文类型并启动定时器
                if (tmrtimer1 > 0)
                {
                    timer1.Interval = tmrtimer1;
                    timer1.Enabled = true;
                    timer1.Start();
                }
                if (tmrtimer2 > 0)
                {
                    if (!System.IO.Directory.Exists(HZPath))
                        System.IO.Directory.CreateDirectory(HZPath);
                    if (!System.IO.Directory.Exists(HZPath + @"\temp"))
                        System.IO.Directory.CreateDirectory(HZPath + @"\temp");
                    timer2.Interval = tmrtimer2;
                    timer2.Enabled = true;
                    timer2.Start();
                }
                ClsLog.AppendLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                ClsLog.AppendLog("  Service Error Text: Flag=" + ex.Message, "服务日志");
            }
        }

        protected override void OnStop()
        {
            timer1.Stop();
            timer1.Enabled = false;
            timer2.Stop();
            timer2.Enabled = false;
            ClsLog.AppendLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }

        /// <summary>
        /// 上传海关订单报文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                ClsLog.AppendLog("上传FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceUpPath;
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;
                ftp.Put(FtpUpPath, "*.xml", FtpUpPathBak);
                ClsLog.AppendLog("上传FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");

            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
            }
            timer1.Enabled = true;
        }




        /// <summary>
        /// 下载海关回执报文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer2.Enabled = false;

            try
            {
                ClsLog.AppendLog("下载FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceDownPath;
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;

                ftp.GetAll("*.xml", HZPath, HZPathBak);
                ClsLog.AppendLog("下载FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
            }
            timer2.Enabled = true;
        }
    }
}
