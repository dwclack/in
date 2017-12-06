using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Alog.Common;

namespace ANDeclareService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ClsLog.AppendLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            //timer1
            int timer1Interval = 300000;
            if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("timer1Interval")))
            {
                timer1Interval = Convert.ToInt32(ClsLog.GetAppSettings("timer1Interval"));
            }
            if (timer1Interval > 0)
            {
                timer1.Interval = timer1Interval;
                timer1.Enabled = true;
                timer1.AutoReset = false;  //执行完才进入下一个循环
                timer1.Start();
            }
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;
            timer1.Stop();
            ClsLog.AppendLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }

        /// <summary>
        /// 发送联华爱农申请申报报文
        /// </summary>
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                     timer1.Enabled = false;
                     int i = 0;
                     DataRow[] drs = RepXml.dtRepXmlSet.Select();
                     Task[] tasks = new Task[drs.Length];

                     foreach (DataRow dr in drs)  //处理每种报文类型
                     {
                         try
                         {
                             ClsThreadParam ClsParam = new ClsThreadParam();
                             ClsParam.RepTitle = dr["RepTitle"].ToString();
                             ClsParam.dr = dr;
                             ClsParam.tCount = i;
                             ClsParam.AutoID = Convert.ToInt16(dr["AutoID"].ToString());
                             ClsParam.DSID = Convert.ToInt16(dr["DSID"].ToString());

                             RepXml rx = new RepXml();
                             tasks[i] = Task.Factory.StartNew(() => rx.ThreadHandle(ClsParam));                         
                         }
                         catch (Exception ex)
                         {
                             //当数据库服务器连接断开导致异常时，定时器状态需要开启
                             timer1.Enabled = true;
                             ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + dr["RepTitle"].ToString() + "异常信息: " + ex.Message+ex.StackTrace, "服务日志");
                             i++;
                             continue;
                         }
                         i++;
                     }
                     Task.WaitAll(tasks);

                     timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                timer1.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 服务异常信息: " + ex.Message+ex.StackTrace, "服务日志");
            }



        }



    }
}
