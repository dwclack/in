using System;
using System.Collections.Generic;
using System.IO;
using Alog_WSKJSD;

namespace AlogFtp
{
    public class FtpHelper
    {
        static private string topath = ClsLog.GetAppSettings("FtpUpPathBak");
        /// <summary>
        /// 报文上传至FTP服务器
        /// </summary>
        /// <returns></returns>
        public bool Upload()
        {
            try
            {
                ClsLog.AppendLog("上传Xml开始---------------------------------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                string path = ClsLog.GetAppSettings("FtpUpPath");

                if (!Directory.Exists(path))
                {
                    return false;
                }

                List<string> needMoveList = new List<string>();

                string[] files = Directory.GetFiles(path);

                int num = 0;

                foreach (string file in files)
                {
                    string folder = string.Empty;
                    //判断上传目录下是否有文件
                    string filename = Path.GetFileName(file);

                    bool isOk = false;

                    if (filename.IndexOf("661105") > -1) //商品备案
                    {
                        FtpUtil.SetUpLoadPath("4200.IMPBA.SWBCARGOBACK.REPORT");
                        isOk = true;
                    }

                    if (filename.IndexOf("661101") > -1) //电商订单
                    {
                        FtpUtil.SetUpLoadPath("4200.IMPBA.SWBEBTRADE.REPORT");
                        isOk = true;
                    }

                    if (filename.IndexOf("661103") > -1) //进境清单
                    {
                        FtpUtil.SetUpLoadPath("4200.IMPBA.SWBENTRYELIST.REPORT"); //改变了
                        isOk = true;
                    }

                    if (filename.IndexOf("661102") > -1) //进仓单
                    {
                        FtpUtil.SetUpLoadPath("4200.IMPBA.SWBOOKING.REPORT"); //改变了
                        isOk = true;
                    }

                    if (filename.IndexOf("661104") > -1) //装载单
                    {
                        FtpUtil.SetUpLoadPath("4200.IMPBA.SWBLOADBILL.REPORT");
                        isOk = true;
                    }

                    if (filename.IndexOf("661108") > -1) //物流信息 
                    {
                        FtpUtil.SetUpLoadPath("LOGISTICS");
                        isOk = true;
                    }

                    if (filename.IndexOf("661107") > -1) //支付信息
                    {
                        FtpUtil.SetUpLoadPath("PAY");
                        isOk = true;
                    }

                    if (filename.IndexOf("661109_") > -1) //出入库信息
                    {
                        FtpUtil.SetUpLoadPath("inoutstock2.0");
                        isOk = true;
                    }

                    if (!isOk)
                    {
                        continue;
                    }

                    try
                    {

                        //上传到南沙国检报文
                        FtpUtil.FileUpLoad(file, folder);
                        //上传文件成功
                        needMoveList.Add(filename);

                        num++;

                        if (num >= 200)
                        {
                            ClsLog.AppendLog("一次只上传200个" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ClsLog.AppendLog("上传文件失败" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                    }
                }

                #region 移动文件

                //发送完成报文目录(国检)
                //string topath = @"D:\KJ\KJ上载报文\发送完成报文目录";
               
                foreach (string fName in needMoveList)
                {
                    try
                    {

                        FileMove(path, topath, fName);
                    }
                    catch (Exception ex)
                    {
                        ClsLog.AppendLog("移动文件失败" + fName + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                    }
                }

                #endregion 移动文件
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog("上传FTP出错" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
            }
            ClsLog.AppendLog("上传Xml结束---------------------------------"  + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
            return true;
        }

        /// <summary>
        /// 下载FTP服务器回执
        /// </summary>
        /// <returns></returns>
        public bool Download()
        {
            ClsLog.AppendLog("下载Xml开始---------------------------------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "获取回执日志");
            //商品备案
            DownloadByXML("4200.IMPBA.SWBCARGOBACK.REPORT");
            //电商订单
            DownloadByXML("4200.IMPBA.SWBEBTRADE.REPORT");

            //进境清单
            DownloadByXML("4200.IMPBA.SWBENTRYE.REPORT");
            //进境清单
            DownloadByXML("4200.IMPBA.SWBENTRYE.AUDIT");

            //进仓单
            DownloadByXML("4200.IMPBA.SWBOOKING.REPORT");

            //进仓单审核
            DownloadByXML("4200.IMPBA.SWBOOKING.AUDIT");

            //装载单
            DownloadByXML("4200.IMPBA.SWBLOADBILL.REPORT");

            //装载单审核
            DownloadByXML("4200.IMPBA.SWBLOADBILL.AUDIT");

            //物流信息
            DownloadByXML("LOGISTICS");
            //支付信息
            DownloadByXML("PAY");
            //出入库信息
            DownloadByXML("inoutstock2.0");
            //不太清楚
            DownloadByXML("4200.IMPBA.SWBCARGOBACK.AUDIT");
            DownloadByXML("4200.IMPBA.SWBOOKING.AUDIT");
            DownloadByXML("4200.IMPBA.SWBOOKING.JZSB");

            DownloadByXML("4200.IMPBA.SWBCARGOBACK.AUDIT");
            DownloadByXML("4200.IMPBA.SWBENTRYELIST.AUDIT");
            DownloadByXML("4200.IMPBA.SWBENTRYELIST.REPORT");

            ClsLog.AppendLog("下载Xml结束---------------------------------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "获取回执日志");

            return true;
        }
        static private string sftppath = ClsLog.GetAppSettings("NSFTPAddress");
        static private string hzpath = ClsLog.GetAppSettings("HZPath");
        /// <summary>
        /// 下载不同目录下的XML
        /// </summary>
        /// <param name="value"></param>
        public void DownloadByXML(string value)
        {
            try
            {
                string ftpPath = sftppath + value + "/out/";

                FtpUtil.SetDownLoadPath(value);
                string[] fileList = FtpUtil.GetFileList("");
                List<string> finshList = new List<string>();
                try
                {

                    int num = 0;

                    //string path = @"D:\KJ\KJ上载报文\报文回执目录";
                    //string path = ClsLog.GetAppSettings("HZPath");

                    foreach (var fileName in fileList)
                    {
                        if (string.IsNullOrEmpty(fileName))
                            continue;

                        int result = FtpUtil.Download(hzpath, fileName, ftpPath);
                        if (result != 0)
                        {
                            //当下载的文件流成功下载报文，增加到已完成集合下
                            finshList.Add(fileName);

                            num++;

                            if (num >= 50)
                            {
                                ClsLog.AppendLog("一次只下载50个" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "获取回执日志");
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ClsLog.AppendLog("下载FTP文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "获取回执日志");
                }

                #region 移除文件

                try
                {
                    //删除对应已下载报文
                    foreach (string fName in finshList)
                    {
                        FtpUtil.SetDownLoadPath(value);
                        FtpUtil.DeleteFileName(fName);
                    }
                }
                catch (Exception ex)
                {
                    ClsLog.AppendLog("删除FTP文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "获取回执日志");
                }

                #endregion 移除文件

            }
            catch (Exception ex)
            {
                ClsLog.AppendLog("处理回执失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "获取回执日志");
            }
        }

        /// <summary>
        /// 复制到指定目录
        /// </summary>
        /// <param name="sPath">源目录</param>
        /// <param name="sToPath">目标目录</param>
        /// <param name="sFileName">文件名</param>
        /// <returns></returns>
        protected bool zCopyToPath(string sPath, string sToPath, string sFileName)
        {
            bool flag = false;
            if (!string.IsNullOrEmpty(sPath))
            {
                if (!Directory.Exists(sToPath))
                    Directory.CreateDirectory(sToPath);

                if (!Directory.Exists(sPath))
                {
                    throw new Exception("源目录不存在！");
                }

                string fileName = sPath + "\\" + sFileName;

                if (!File.Exists(fileName))
                {
                    throw new Exception("xml不存在！");
                }

                string toFileName = sToPath + "\\" + sFileName;

                File.Copy(fileName, toFileName, true); //复制文件
            }

            return flag;
        }

        /// <summary>
        /// 移动到指定目录
        /// </summary>
        /// <param name="sPath">源目录</param>
        /// <param name="sToPath">目标目录</param>
        /// <param name="sFileName">文件名</param>
        /// <returns></returns>
        protected bool FileMove(string sPath, string sToPath, string sFileName)
        {
            bool flag = false;
            string fileName = string.Empty;
            try
            {

                if (!string.IsNullOrEmpty(sPath))
                {
                    if (!Directory.Exists(sToPath))
                        Directory.CreateDirectory(sToPath);

                    if (!Directory.Exists(sPath))
                    {
                        throw new Exception("源目录不存在！");
                    }

                    fileName = sPath + "\\" + sFileName;

                    if (!File.Exists(fileName))
                    {
                        throw new Exception("xml不存在！");
                    }

                    string toFileName = sToPath + "\\" + sFileName;

                    File.Move(fileName, toFileName); //移动文件
                }
            }
            catch (IOException ex)
            {
                //logger.Error("", ex);
                File.Delete(fileName);
                ClsLog.AppendLog("xml" + sFileName + "已存在！" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "移动文件日志");
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog("xml" + sFileName + "已存在！" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "移动文件日志");
            }

            return flag;
        }
    }
}
