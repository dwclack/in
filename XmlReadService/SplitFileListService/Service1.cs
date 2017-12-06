using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Threading;

namespace SplitFileListService
{
    public partial class Service1 : ServiceBase
    {
        static string SPath = ClsLog.GetAppSettings("SPath");
        static string UpPath = ClsLog.GetAppSettings("UpPath");
        static string UpPathBak = ClsLog.GetAppSettings("UpPathBak");
        static string FileNum = ClsLog.GetAppSettings("FileNum");
        static string HZThreadCount = ClsLog.GetAppSettings("HZThreadCount");

        private readonly Thread[] threadHZ;
        static int CloseTimes = 0;//当执行200次时自动关闭并重启所有线程防止堵塞
        public Service1()
        {
            InitializeComponent();
            int HZThreadCount = 1;
            if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HZThreadCount")))
            {
                HZThreadCount = Convert.ToInt32(ClsLog.GetAppSettings("HZThreadCount"));
            }
            threadHZ = new Thread[HZThreadCount];
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                NLogger.WriteLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                //======获取并设置定时间隔
                int tmrtimer1 = 300000;
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
                if (!AllThreadIsAlive())
                {
                    for (int i = 0; i < threadHZ.Length; i++)
                    {
                        threadHZ[i] = new Thread(CopySFile);
                        threadHZ[i].Start();
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
            }
            timer1.Enabled = true;
        }

        public void CopySFile()
        {
            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "移动文件开始", "移动日志");
            string[] strFiles = Directory.GetFiles(SPath, "661108_*.xml");
            int len = Convert.ToInt32(FileNum);
            if (strFiles.Length > len)
            {
                int scount = 0;
                foreach (string strFile in strFiles)
                {
                    try
                    {
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "移动文件:" + strFile, "移动日志");
                        ClsLog.CopyFile(Path.GetFileName(strFile), Path.GetDirectoryName(strFile) + @"\",
                                            @"" + UpPathBak + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\");

                        ClsLog.MoveFile(Path.GetFileName(strFile), Path.GetDirectoryName(strFile) + @"\",UpPath + @"\");
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "移动文件成功:" + strFile, "移动日志");
                    }
                    catch (Exception ex)
                    {
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "移动文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                    }
                    scount++;
                    if (scount > 1000)
                    {
                        break;
                    }
                }
            }
            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "移动文件结束", "移动日志");
            
        }


        public bool AllThreadIsAlive()
        {
            bool IsAliv = false;
            //设置每执行超过200次就终止所有线程防止堵塞
            if (CloseTimes >= 200)
            {
                for (int i = 0; i < threadHZ.Length; i++)
                {
                    if (threadHZ[i] != null && threadHZ[i].IsAlive)
                    {
                        threadHZ[i].Abort();
                    }
                }
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "所有线程终止停止下载", "下载日志");
                CloseTimes = 0;
                timer1.Enabled = true;
                return false;
            }
            CloseTimes++;

            for (int i = 0; i < threadHZ.Length; i++)
            {
                if (threadHZ[i] != null && threadHZ[i].IsAlive)
                {
                    IsAliv = true;
                    return IsAliv;
                }
            }
            return IsAliv;
        }
    }
}
