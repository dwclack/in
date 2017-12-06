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
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace HGOwnerFtpServices
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

        private void button1_Click(object sender, EventArgs e)
        {
            
            //FTPHelper ftp = new FTPHelper(NSFTPAddress, FtpServiceUpPath, FtpUserName, FtpPwd);
            try
            {
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceUpPath;
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;
                ftp.Put(FtpUpPath, "*.xml", FtpUpPathBak);
                //foreach (string file in System.IO.Directory.GetFiles(FtpUpPath, "*.xml"))
                //{
                //    ftp.Upload(file);
                //    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                //                    @"" + FtpUpPathBak + DateTime.Now.ToString("yyyyMMdd") + @"\");
                //    //di.ReadFile(file);
                //    ClsLog.DeleteFile(file);
                //}

            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ClsLog.AppendLog("下载FTP文件开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                FTPClient ftp = new FTPClient();
                ftp.RemoteHost = NSFTPAddressIP;
                ftp.RemotePort = Convert.ToInt32(NSFTPAddressPort);
                ftp.RemotePath = FtpServiceDownPath;
                ftp.RemoteUser = FtpUserName;
                ftp.RemotePass = FtpPwd;

                ftp.GetAll("*.xml", HZPath, HZPathBak);
                ClsLog.AppendLog("下载FTP文件结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
            }

            //FTPHelper ftp = new FTPHelper(NSFTPAddress, FtpServiceDownPath, FtpUserName, FtpPwd);
            //string url = NSFTPAddress + @"\" + FtpServiceDownPath + @"\";
            //foreach (string file in ftp.GetFileList(url))
            //{
            //    ftp.Download(url, file);
            //    ftp.Delete(file);
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                FTPHelper ftp = new FTPHelper(NSFTPAddress, FtpServiceDownPath, FtpUserName, FtpPwd);
                string url = "ftp://" + NSFTPAddress + @"/" + FtpServiceDownPath + @"/";
                List<string> listFileList = new List<string>();
                //string[] strFiles = ftp.GetFileList(url);
                string[] strFiles = ftp.GetAllList(url);
                
                int dataCount = strFiles.Length;
                var list = strFiles.Cast<string>().ToArray();
                int pageCount = 30;//分页数默认10页
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
                //foreach (string file in ftp.GetFilesDetailList())
                //{
                //    ftp.Download(url, file);
                //    listFileList.Add(file);
                //    //scount++;
                //    //if (scount > 50)
                //    //{
                //    //    break;
                //    //}
                //    //ftp.Delete(file);
                //}

                //foreach (string strFile in listFileList)
                //{
                //    if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
                //    {
                //        //ftp.Delete(strFile);
                //    }
                //}
                //decimal s = listFileList.Count;
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "上传日志");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                 Thread[] threadHZ=new Thread[1];
                for (int i = 0; i < threadHZ.Length; i++)
                {
                    threadHZ[i] = new Thread(DownFile);
                    threadHZ[i].Start();
                    threadHZ[i].Join();
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
            }
        }

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
    }
}
