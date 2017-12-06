using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Alog_WSKJSD;
using System.IO;

namespace NSDDDisServices
{
    public partial class Service1 : ServiceBase
    {
        static string FtpServiceUpPath = ClsLog.GetAppSettings("FtpServiceUpPath");
        static string FtpUpPath = ClsLog.GetAppSettings("FtpUpPath");
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
                int tmrtimer3 = 300000;
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("FtpUpInterval")))
                {
                    tmrtimer1 = Convert.ToInt32(ClsLog.GetAppSettings("FtpUpInterval"));
                }
                //======获取报文类型并启动定时器
                if (tmrtimer1 > 0)
                {
                    timer1.Interval = tmrtimer1;
                    timer1.Enabled = true;
                    timer1.Start();
                }
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("GJFtpUpInterval")))
                {
                    tmrtimer2 = Convert.ToInt32(ClsLog.GetAppSettings("GJFtpUpInterval"));
                }
                //======获取报文类型并启动定时器
                if (tmrtimer2 > 0)
                {
                    timer2.Interval = tmrtimer2;
                    timer2.Enabled = true;
                    timer2.Start();
                }

                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HGZSFtpUpInterval")))
                {
                    tmrtimer3 = Convert.ToInt32(ClsLog.GetAppSettings("HGZSFtpUpInterval"));
                }
                //======获取报文类型并启动定时器
                if (tmrtimer3 > 0)
                {
                    timer3.Interval = tmrtimer3;
                    timer3.Enabled = true;
                    timer3.Start();
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
            timer3.Stop();
            timer3.Enabled = false;
            ClsLog.AppendLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 南沙报文订单分文件夹
            timer1.Enabled = false;
            ImportXMLData di = new ImportXMLData();
            try
            {
                foreach (string file in System.IO.Directory.GetFiles(FtpServiceUpPath))
                {
                    string XMLDirName = di.OPNSXmlData(file);
                    if (XMLDirName!="-1")
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + FtpUpPath + @"\" + XMLDirName+ @"\" );

                        ClsLog.DeleteFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer1.Enabled = true;
        }


        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 机场国检报文订单分文件夹
            timer2.Enabled = false;
            string GJFtpServiceUpPath = ClsLog.GetAppSettings("GJFtpServiceUpPath");
            string GJFtpUpPath = ClsLog.GetAppSettings("GJFtpUpPath");
            ImportXMLData di = new ImportXMLData();
            try
            {
                foreach (string file in System.IO.Directory.GetFiles(GJFtpServiceUpPath))
                {
                    string XMLDirName = di.OPGJJCXmlData(file);
                    if (XMLDirName != "-1")
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + GJFtpUpPath + @"\" + XMLDirName + @"\");

                        ClsLog.DeleteFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer2.Enabled = true;
        }

        private void timer3_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 海关总署订单分文件夹
            timer3.Enabled = false;
            string HGZSFtpServiceUpPath = ClsLog.GetAppSettings("HGZSFtpServiceUpPath");
            string HGZSFtpUpPath = ClsLog.GetAppSettings("HGZSFtpUpPath");
            string HGZSDirNode = ClsLog.GetAppSettings("HGZSDirNode");
            ImportXMLData di = new ImportXMLData();
            try
            {
                foreach (string file in System.IO.Directory.GetFiles(HGZSFtpServiceUpPath))
                {
                    string XMLDirName = di.OPHGZSXmlData(file, HGZSDirNode);
                    if (XMLDirName != "-1")
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + HGZSFtpUpPath + @"\" + XMLDirName + @"\");

                        ClsLog.DeleteFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer3.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer3.Enabled = true;
        }
    }
}
