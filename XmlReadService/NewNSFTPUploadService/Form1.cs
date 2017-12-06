using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Text;
using DotNet.Utilities;
using Alog_WSKJSD;
using System.IO;
using System.Threading.Tasks;

namespace NewNSFTPUploadService
{
    public partial class Form1 : Form
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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

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

        private void button2_Click(object sender, EventArgs e)
        {
            ///商品备案
            DownloadFile("4200.IMPBA.SWBCARGOBACK.REPORT");
            ///电商订单
            DownloadFile("4200.IMPBA.SWBEBTRADE.REPORT");
            ///进境清单
            DownloadFile("4200.IMPBA.SWBENTRYELIST.REPORT");
            //进境清单审核
            DownloadFile("4200.IMPBA.SWBENTRYELIST.AUDIT");
            ///进仓单
            DownloadFile("4200.IMPBA.SWBOOKING.REPORT");
            //进仓单审核
            DownloadFile("4200.IMPBA.SWBOOKING.AUDIT");
            ///装载单
            DownloadFile("4200.IMPBA.SWBLOADBILL.REPORT");
            ///装载单审核
            DownloadFile("4200.IMPBA.SWBLOADBILL.AUDIT");
            ///物流信息
            DownloadFile("LOGISTICS");
            ///支付信息
            //DownloadFile("PAY");
            ///出入库报文信息
            DownloadFile("inoutstock2.0");
            //未知
            DownloadFile("4200.IMPBA.SWBCARGOBACK.AUDIT");
        }


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

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                string NSFTPAddress = ClsLog.GetAppSettings("NSFTPAddress");
                string NSFTPAddressIP = ClsLog.GetAppSettings("NSFTPAddressIP");
                string NSFTPAddressPort = ClsLog.GetAppSettings("NSFTPAddressPort");
                string FtpUserName = ClsLog.GetAppSettings("FtpUserName");
                string FtpPwd = ClsLog.GetAppSettings("FtpPwd");
                string FtpServiceUpPath = ClsLog.GetAppSettings("FtpServiceUpPath");
                string FtpUpPath = ClsLog.GetAppSettings("FtpUpPath");
                string FtpUpPathBak = ClsLog.GetAppSettings("FtpUpPathBak");
                string FtpServiceDownPath = ClsLog.GetAppSettings("FtpServiceDownPath");
                string HZPath = ClsLog.GetAppSettings("HZPath");
                string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
                //物流信息
                UploadFile("LOGISTICS", "661108_*.xml");
                 ///商品备案
                UploadFile("4200.IMPBA.SWBCARGOBACK.REPORT", "661105_*.xml");
                 ///电商订单
                UploadFile("4200.IMPBA.SWBEBTRADE.REPORT", "661101_*.xml");
                 ///进境清单
                UploadFile("4200.IMPBA.SWBENTRYELIST.REPORT", "661103_*.xml");
                 ///进仓单
                UploadFile("4200.IMPBA.SWBOOKING.REPORT", "661102_*.xml");
                 ///装载单
                UploadFile("4200.IMPBA.SWBLOADBILL.REPORT", "661104_*.xml");
                 ///物流信息
                UploadFile("LOGISTICS", "661108_*.xml");
                 ///支付信息
                UploadFile("PAY", "661107_*.xml");
                 ///出入库报文信息
                UploadFile("inoutstock", "661109_*.xml");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                MulUpFile("/inoutstock2.0/out1/", "661109_*.xml");
                MulUpFile("/LOGISTICS/out3/", "661108_*.xml");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
            }
        }

        public void MulUpFile(string upPath,string searchPattern)
        {
            string pathS = upPath;
            FTPHelper ftp = new FTPHelper(NSFTPAddress, pathS, FtpUserName, FtpPwd);
            string url = "ftp://" + NSFTPAddress + @"/" + pathS + @"/";
            string[] strFiles = Directory.GetFiles(FtpUpPath, searchPattern);

            int dataCount = strFiles.Length;
            var list = strFiles.Cast<string>().ToArray();
            int pageCount = 30;//分页数默认10页
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

        private void button4_Click(object sender, EventArgs e)
        {
            ///进仓单
            MulDownLoad("/4200.IMPBA.SWBOOKING.REPORT/out/");
            //进仓单审核
            MulDownLoad("/4200.IMPBA.SWBOOKING.AUDIT/out/");
            ///装载单
            MulDownLoad("/4200.IMPBA.SWBLOADBILL.REPORT/out/");
            ///装载单审核
            MulDownLoad("/4200.IMPBA.SWBLOADBILL.AUDIT/out/");
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
            int pageCount = 30;//分页数默认1页
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

    }
}
