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
using System.Threading;
using System.Threading.Tasks;

namespace HGOwnerFtpServices
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
        static int sNum = 0;
        static  int HZThreadCount=1;
        private readonly Thread[] threadHZ;
        private int CloseTimes = 0;//当执行200次时自动关闭并重启所有线程防止堵塞
        public Service1()
        {
            try
            {
                NLogger.WriteLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                InitializeComponent();
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HZThreadCount")))
                {
                    HZThreadCount = Convert.ToInt32(ClsLog.GetAppSettings("HZThreadCount"));
                }
                threadHZ = new Thread[HZThreadCount];
                NLogger.WriteLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                NLogger.WriteLog("  Service Error Text: Flag=" + ex.Message, "服务日志");
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                NLogger.WriteLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
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
                NLogger.WriteLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "错误日志");
                NLogger.WriteLog("  Service Error Text: Flag=" + ex.Message, "服务日志");
            }
        }

        protected override void OnStop()
        {
            timer1.Stop();
            timer1.Enabled = false;
            timer2.Stop();
            timer2.Enabled = false;
            NLogger.WriteLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
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
                NLogger.WriteLog("上传FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceUpPath;
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;
                ftp.Put(FtpUpPath, "*.xml", FtpUpPathBak);
                NLogger.WriteLog("上传FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
               
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "错误日志");
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
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "下载开始", "下载日志");
                MulDownLoad();
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "下载结束", "下载日志");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                if (ex.Message.IndexOf("超时")>=0)
                {
                    sNum++;
                    if (sNum > 20)
                    {
                        ServiceController service = new ServiceController("KQServiceData");

                        if (service.Status != ServiceControllerStatus.Running)
                        {
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running);
                        }
                        sNum = 0;
                    }
                }
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "错误日志");
            }
            finally
            {
                timer2.Enabled = true;
            }
        }

        public void MulDownLoad()
        {
            FTPHelper ftp = new FTPHelper(NSFTPAddress, FtpServiceDownPath, FtpUserName, FtpPwd);
            string url = "ftp://" + NSFTPAddress + @"/" + FtpServiceDownPath + @"/";
            List<string> listFileList = new List<string>();
            //string[] strFiles = ftp.GetFileList(url);
            string[] strFiles = ftp.GetAllList(url);

            int dataCount = strFiles.Length;
            var list = strFiles.Cast<string>().ToArray();
            int pageCount = HZThreadCount;//分页数默认1页
            int pageSize = dataCount % pageCount == 0 ? (dataCount / pageCount) : (dataCount / pageCount + 1);
            NLogger.WriteLog("下载分页开始 , Data Count : " + dataCount + ", Page Size:" + pageSize, "MultiPage");
            Parallel.For(0, pageCount, pageIndex =>
            {
                var data = list.Skip(pageIndex * pageSize).Take(pageSize);
                foreach (string strFile in data)
                {
                    if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
                    {
                        ftp.Download(HZPath + @"\temp" + @"\", strFile.Trim());
                        FileInfo fileInfo = new FileInfo(HZPath + @"\temp" + @"\" + strFile.Trim());
                        if (fileInfo.Length <= 0)
                        {
                            //如果文件小于等于0就执行下一个文件下载
                            continue;
                        }
                        ClsLog.CopyFile(strFile.Trim(), HZPath + @"\temp\",
                                           HZPath + @"\");
                        ClsLog.DeleteFile(HZPath + @"\temp\" + strFile.Trim());
                        ClsLog.CopyFile(strFile.Trim(), HZPath + @"\",
                                           @"" + HZPathBak + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\");

                        try
                        {
                            ftp.Delete(strFile.Trim());
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件成功:" + strFile, "下载日志");
                        }
                        catch (Exception ex)
                        {
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "错误日志");
                        }
                    }
                }
            });
        }

      

        #region 旧的下载报文功能，不是很好用，换成新的多线程滴
        ///// <summary>
        ///// 下载海关回执报文
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    timer2.Enabled = false;

        //    try
        //    {
        //        //if (!AllThreadIsAlive())
        //        //{
        //        for (int i = 0; i < threadHZ.Length; i++)
        //        {
        //            threadHZ[i] = new Thread(DownFile);
        //            threadHZ[i].Start();
        //            threadHZ[i].Join();
        //        }
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        //当数据库服务器连接断开导致异常时，定时器状态需要开启
        //        timer2.Enabled = true;
        //        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
        //    }
        //    timer2.Enabled = true;
        //}

        public void DownFile()
        {
            try
            {
                NLogger.WriteLog("下载FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceDownPath;
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;

                ftp.GetAllThread("*.xml", HZPath, HZPathBak);
                NLogger.WriteLog("下载FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
            }
        }

        public void DownFileMul()
        {
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
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +"所有线程终止停止下载", "下载日志");
                CloseTimes = 0;
                timer2.Enabled = true;
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

        #endregion
    }
}
