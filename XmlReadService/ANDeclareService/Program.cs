using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Alog.Common;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ANDeclareService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
             

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
              
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + dr["RepTitle"].ToString() + "异常信息: " + ex.Message + ex.StackTrace, "服务日志");
                    i++;
                    continue;
                }
                i++;
            }
            Task.WaitAll(tasks);







            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new Service1() 
            //};
            //ServiceBase.Run(ServicesToRun);

        }

         

    }
}
