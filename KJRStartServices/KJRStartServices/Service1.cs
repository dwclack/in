using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace KJRStartServices
{
    public partial class Service1 : ServiceBase
    {
        static string IsStop = ClsLog.GetAppSettings("IsStop");
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                NLogger.WriteLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //======获取并设置定时间隔
                int tmrtimer1 = 300000;
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("Interval")))
                {
                    tmrtimer1 = Convert.ToInt32(ClsLog.GetAppSettings("Interval"));
                }

                //======获取报文类型并启动定时器
                if (tmrtimer1 > 0)
                {
                    timer1.Interval = tmrtimer1;
                    timer1.Enabled = true;
                    timer1.Start();
                }
                NLogger.WriteLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                NLogger.WriteLog("  Service Error Text: Flag=" + ex.Message);
            }
        }

        protected override void OnStop()
        {
            timer1.Stop();
            timer1.Enabled = false;
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                //int ExTime = Convert.ToInt32(ClsLog.GetAppSettings("ExTime"));
                //int Hours = DateTime.Now.Hour;
                //if (Hours > ExTime && Hours <= ExTime + 1)
                //{
                NLogger.WriteLog("============== 重启开始:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                string ExeList=ClsLog.GetAppSettings("ExeList");
                for (int i = 0; i < ExeList.Split(',').Length; i++)
                {
                    string serverName = ExeList.Split(',')[i].Trim();
                    ServiceController service = new ServiceController(serverName);
                    
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                    NLogger.WriteLog("============== 重启服务成功:" + serverName + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                if (IsStop == "1")
                {
                    ServiceController serviceStop = new ServiceController("KQServiceData");

                    if (serviceStop.Status == ServiceControllerStatus.Running)
                    {
                        serviceStop.Stop();
                        serviceStop.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                }

                NLogger.WriteLog("============== 重启结束:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //}
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("============== 读取数据异常Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                NLogger.WriteLog("  Service Error Text: Flag=" + ex.Message);
            }
            timer1.Enabled = true;
        }

    }
}
