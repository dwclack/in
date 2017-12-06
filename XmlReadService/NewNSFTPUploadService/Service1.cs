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
using System.Threading.Tasks;

namespace NewNSFTPUploadService
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
        static int HZThreadCount = 1;
        public Service1()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HZThreadCount")))
            {
                HZThreadCount = Convert.ToInt32(ClsLog.GetAppSettings("HZThreadCount"));
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
                    timer2.Interval = tmrtimer2;
                    timer2.Enabled = true;
                    timer2.Start();
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
            timer2.Stop();
            timer2.Enabled = false;
            NLogger.WriteLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                NLogger.WriteLog("上传FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
               
                ///物流信息
                MulUpFile("/LOGISTICS/in/", "661108_*.xml");
                ///进境清单
                MulUpFile("/4200.IMPBA.SWBENTRYELIST.REPORT/in/", "661103_*.xml");
                ///装载单
                MulUpFile("/4200.IMPBA.SWBLOADBILL.REPORT/in/", "661104_*.xml");
                ///绑码信息报文
                MulUpFile("/4200.IMPBA.PSPZCODEINFO/in/", "661121_*.xml");
                ///出入库报文信息
                MulUpFile("/inoutstock2.0/in/", "661109_*.xml");
                ///商品备案
                MulUpFile("/4200.IMPBA.SWBCARGOBACK.REPORT/in/", "661105_*.xml");
                ///电商订单
                MulUpFile("/4200.IMPBA.SWBEBTRADE.REPORT/in/", "661101_*.xml");
                ///进仓单
                MulUpFile("/4200.IMPBA.SWBOOKING.REPORT/in/", "661102_*.xml");
                /////支付信息
                //UploadFile("PAY", "661107_*.xml");

                NLogger.WriteLog("上传FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
            }
            timer1.Enabled = true;
        }

        public void MulUpFile(string upPath, string searchPattern)
        {
            string pathS = upPath;
            FTPHelper ftp = new FTPHelper(NSFTPAddress, pathS, FtpUserName, FtpPwd);
            string url = "ftp://" + NSFTPAddress + @"/" + pathS + @"/";
            string[] strFiles = Directory.GetFiles(FtpUpPath, searchPattern);

            int dataCount = strFiles.Length;
            var list = strFiles.Cast<string>().ToArray();
            int pageCount = HZThreadCount;//分页数默认10页
            int pageSize = dataCount % pageCount == 0 ? (dataCount / pageCount) : (dataCount / pageCount + 1);
            NLogger.WriteLog("分页开始 , Data Count : " + dataCount + ", Page Size:" + pageSize, "MultiPage");
            Parallel.For(0, pageCount, pageIndex =>
            {
                var data = list.Skip(pageIndex * pageSize).Take(pageSize);
                foreach (string strFile in data)
                {
                    if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
                    {
                        ftp.Upload(strFile);
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "上传FTP文件成功:" + strFile, "上传日志");
                        try
                        {
                            ClsLog.CopyFile(Path.GetFileName(strFile), Path.GetDirectoryName(strFile) + @"\",
                                                @"" + FtpUpPathBak + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\");
                            ClsLog.DeleteFile(strFile);
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件成功:" + strFile, "上传日志");
                        }
                        catch (Exception ex)
                        {
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                        }
                    }
                }
            });
        }

        ///// <summary>
        ///// 上传
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    timer1.Enabled = false;
        //    try
        //    {
        //        NLogger.WriteLog("上传FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
        //        ///商品备案
        //        UploadFile("4200.IMPBA.SWBCARGOBACK.REPORT", "661105_*.xml");
        //        ///电商订单
        //        UploadFile("4200.IMPBA.SWBEBTRADE.REPORT", "661101_*.xml");
        //        ///进境清单
        //        UploadFile("4200.IMPBA.SWBENTRYELIST.REPORT", "661103_*.xml");
        //        ///进仓单
        //        UploadFile("4200.IMPBA.SWBOOKING.REPORT", "661102_*.xml");
        //        ///装载单
        //        UploadFile("4200.IMPBA.SWBLOADBILL.REPORT", "661104_*.xml");
        //        ///物流信息
        //        UploadFile("LOGISTICS", "661108_*.xml");
        //        ///支付信息
        //        UploadFile("PAY", "661107_*.xml");
        //        ///绑码信息报文
        //        UploadFile("4200.IMPBA.PSPZCODEINFO", "661121_*.xml");
        //        ///出入库报文信息
        //        UploadFile("inoutstock2.0", "661109_*.xml");

        //        NLogger.WriteLog("上传FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
        //    }
        //    catch (Exception ex)
        //    {
        //        //当数据库服务器连接断开导致异常时，定时器状态需要开启
        //        timer1.Enabled = true;
        //        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
        //    }
        //    timer1.Enabled = true;
        //}

        private void UploadFile(string UpDir, string strFileNameMask)
        {
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceUpPath + UpDir + "/in/";
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;
                ftp.Put(FtpUpPath, strFileNameMask, FtpUpPathBak);
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer2.Enabled = false;

            try
            {
                NLogger.WriteLog("下载FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                
                ///进仓单
                MulDownLoad("/4200.IMPBA.SWBOOKING.REPORT/out/");
                //进仓单审核
                MulDownLoad("/4200.IMPBA.SWBOOKING.AUDIT/out/");
                ///装载单
                MulDownLoad("/4200.IMPBA.SWBLOADBILL.REPORT/out/");
                ///装载单审核
                MulDownLoad("/4200.IMPBA.SWBLOADBILL.AUDIT/out/");

                NLogger.WriteLog("下载FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
            }
            timer2.Enabled = true;
        }

        public void MulDownLoad(string Path)
        {
            FTPHelper ftp = new FTPHelper(NSFTPAddress, Path, FtpUserName, FtpPwd);
            string url = "ftp://" + NSFTPAddress + @"/" + Path + @"/";
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
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                        }
                    }
                }
            });
        }

        ///// <summary>
        ///// 下载
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    timer2.Enabled = false;

        //    try
        //    {
        //        //FTPClient ftp = new FTPClient();
        //        //ftp.RemoteHost = NSFTPAddressIP;
        //        //ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
        //        //ftp.RemotePath = FtpServiceDownPath;
        //        //ftp.RemoteUser = FtpUserName;
        //        //ftp.RemotePass = FtpPwd;


        //        NLogger.WriteLog("下载FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
        //        ///商品备案
        //        ///DownloadFile("4200.IMPBA.SWBCARGOBACK.REPORT");
        //        ///电商订单
        //        /// DownloadFile("4200.IMPBA.SWBEBTRADE.REPORT");
        //        ///进境清单
        //        ///DownloadFile("4200.IMPBA.SWBENTRYELIST.REPORT");
        //        //进境清单审核
        //        /// DownloadFile("4200.IMPBA.SWBENTRYELIST.AUDIT");
        //        ///进仓单
        //        DownloadFile("4200.IMPBA.SWBOOKING.REPORT");
        //        //进仓单审核
        //        DownloadFile("4200.IMPBA.SWBOOKING.AUDIT");
        //        ///装载单
        //        DownloadFile("4200.IMPBA.SWBLOADBILL.REPORT");
        //        ///装载单审核
        //        DownloadFile("4200.IMPBA.SWBLOADBILL.AUDIT");
        //        ///物流信息
        //        //DownloadFile("LOGISTICS");
        //        ///支付信息
        //        //DownloadFile("PAY");
        //        ///绑码信息报文
        //        /// DownloadFile("4200.IMPBA.PSPZCODEINFO");
        //        ///出入库报文信息
        //        ///DownloadFile("inoutstock2.0");
        //        //未知
        //        ///DownloadFile("4200.IMPBA.SWBCARGOBACK.AUDIT");

        //        NLogger.WriteLog("下载FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
        //    }
        //    catch (Exception ex)
        //    {
        //        //当数据库服务器连接断开导致异常时，定时器状态需要开启
        //        timer2.Enabled = true;
        //        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
        //    }
        //    timer2.Enabled = true;
        //}

        private void DownloadFile(string DownDir)
        {
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceUpPath + DownDir + "/out/";
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;
                ftp.GetAll("*.xml", HZPath, HZPathBak);
        }
    }
}
