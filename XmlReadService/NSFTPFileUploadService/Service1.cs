using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using AlogFtp;
using Alog_WSKJSD;

namespace NSFTPFileUploadService
{
    public partial class Service1 : ServiceBase
    {
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

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 回执处理定时器
            timer1.Enabled = false;
            FtpHelper di = new FtpHelper();
            try
            {
                di.Upload();
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
            //======== 回执处理定时器
            timer2.Enabled = false;
            FtpHelper di = new FtpHelper();
            try
            {
                di.Download();
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer2.Enabled = true;
        }

        
    }
}
