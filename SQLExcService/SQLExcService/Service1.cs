using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SQLExcService
{
    public partial class Service1 : ServiceBase
    {
        static string HZPath = ClsLog.GetAppSettings("Interval");
        static string ThreadCount = ClsLog.GetAppSettings("ThreadCount");
        static string SQLThreadCount = ClsLog.GetAppSettings("SQLThreadCount");
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                NLogger.WriteLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
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
                NLogger.WriteLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                NLogger.WriteLog("  Service Error Text: Flag=" + ex.Message, "服务日志");
            }
        }

        protected override void OnStop()
        {
            timer1.Stop();
            timer1.Enabled = false;
            NLogger.WriteLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                NLogger.WriteLog("执行SQL开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "运行日志");
                ExcSQL();
                NLogger.WriteLog("执行SQL结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "运行日志");

            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "错误日志");
            }
            finally
            {
                timer1.Enabled = true;
            }
            timer1.Enabled = true;
        }

        public void ExcSQL()
        {
            int scount = 0;
            scount = Convert.ToInt32(SQLThreadCount);
            string sqlSelect = @"select top " + scount + @" ID  from  T_LogisticsOutorder 
where WarehouseId IN (1045) 
and OwnerId is not null 
and ISNULL(CancelStatus,0)=0 and CreateTime>'2017-11-01' 
and isnull(OrderStatus,0)=0  and orderCode like '%LBX%' and isnuLL(AuditFailRemark,'')=''";
            DataTable dt = DbHelperSQL.Query(sqlSelect).Tables[0];

            int dataCount = dt.Rows.Count;
            var list = dt.Rows.Cast<DataRow>().ToArray();
            int pageCount =Convert.ToInt32(ThreadCount);
            int pageSize = dataCount % pageCount == 0 ? (dataCount / pageCount) : (dataCount / pageCount + 1);
            NLogger.WriteLog("下载分页开始 , Data Count : " + dataCount + ", Page Size:" + pageSize, "MultiPage");
            Parallel.For(0, pageCount, pageIndex =>
            {
                var data = list.Skip(pageIndex * pageSize).Take(pageSize);
                foreach (DataRow row in data)
                {
                    string sql = "exec [BBC_HGOrderAduitInfo]  " + row["ID"];
                    try
                    {
                        DbHelperSQL.ExecuteSql(sql);
                    }
                    catch (Exception ex)
                    {
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + sql +" "+ ex.Message, "错误日志");
                    }
                }
            });
        }
    }
}
