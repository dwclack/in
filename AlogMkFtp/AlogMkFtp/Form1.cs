using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AlogMkFtp.Utility;
using System.Collections;

namespace AlogMkFtp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }



        private void work(HostNum Host)
        {
            Host = HostNum.Report;
            string Hoststring = "";

            try
            {
                #region FTP1
                Host = HostNum.Report;
                Hoststring = App.g_strFtpHostReport;
                MKpub mkPub = new MKpub(Host);
                ArrayList FileList = new ArrayList();
                //获取后缀为xml或者txt的文件
                FileList = mkPub.GetFTPReportXMLFiles(Host, App.WS_XML_Ftp);
                //下载服务器的文件到PC端  并将文件备份 同时移到国检读取的文件夹低下
                //Utility.Fun.WriteFile(mkPub.GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ",报文数量：" + FileList.Count);
                if (FileList != null && FileList.Count > 0)
                {
                    //Utility.Fun.WriteFile(mkPub.GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ",开始下载：");
                    mkPub.DownloadFTPReportXML(Host, FileList);
                }
                #endregion



                #region FTP2

                //if (App.g_strFtpHostReportSecond != null && !string.IsNullOrEmpty(App.g_strFtpHostReportSecond.Trim()))
                //{
                //    Host = HostNum.ReportSecond;
                //    Hoststring = App.g_strFtpHostReportSecond;
                //    mkPub = new MKpub(Host);
                //    //获取后缀为xml或者txt的文件
                //    FileList.Clear();
                //    FileList = mkPub.GetFTPReportXMLFiles(Host, App.WS_XML_Ftp);
                //    //下载服务器的文件到PC端  并将文件备份 同时移到国检读取的文件夹低下
                //    if (FileList != null && FileList.Count > 0)
                //    {
                //        mkPub.DownloadFTPReportXML(Host, FileList);
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Utility.Fun.WriteFile("下载日志.txt", Hoststring + DateTime.Now.ToString() + "");
            }
        }


        private void workSecond(HostNum Host)
        {
            Host = HostNum.ReportSecond;
            string Hoststring = "";

            try
            {
                if (App.g_strFtpHostReportSecond != null && !string.IsNullOrEmpty(App.g_strFtpHostReportSecond.Trim()))
                {
                    ArrayList FileList = new ArrayList();
                    Host = HostNum.ReportSecond;
                    Hoststring = App.g_strFtpHostReportSecond;
                    Host = HostNum.ReportSecond;
                    MKpub mkPubSecond = new MKpub(Host);
                    //获取后缀为xml或者txt的文件
                    FileList.Clear();
                    FileList = mkPubSecond.GetFTPReportXMLFiles(Host, App.WS_XML_Ftp);
                    Utility.Fun.WriteFile("下载日志.txt", Hoststring + DateTime.Now.ToString() + ",报文数量：" + FileList.Count);
                    //下载服务器的文件到PC端  并将文件备份 同时移到国检读取的文件夹低下
                    if (FileList != null && FileList.Count > 0)
                    {
                        Utility.Fun.WriteFile("下载日志.txt", Hoststring + DateTime.Now.ToString() + ",开始下载：");
                        mkPubSecond.DownloadFTPReportXML(Host, FileList);
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.Fun.WriteFile("下载日志.txt", Hoststring + DateTime.Now.ToString() + "");
            }
        }

        void UploadWord(HostNum Host)
        {
            MKUpload MKUpload = new MKUpload();
            //获取需要上传的文件
            ArrayList Pre_FileList = new ArrayList();

            int GetHZDicType = 0;

            switch (GetHZDicType)
            {
                case 0:
                    Pre_FileList = MKUpload.GetPCLocalHZListAll(Host);
                    break;
                case 1:
                    Pre_FileList = MKUpload.GetPCLocalHZListyyyymmdd(Host);
                    break;
                case 2:
                    Pre_FileList = MKUpload.GetPCLocalHZListyyyy_mm_dd(Host);
                    break;
                default:
                    Pre_FileList = MKUpload.GetPCLocalHZListAll(Host);
                    break;
            }



            string Err = "";
            string TodayDic = DateTime.Now.ToString("yyyyMM");
            Utility.Fun.WriteFile("上传日志.txt", MKUpload.GetHost(Host) + DateTime.Now.ToString() + ",回执数量：" + Pre_FileList.Count);
            if (Pre_FileList != null && Pre_FileList.Count > 0)
            {
                Utility.Fun.WriteFile("上传日志.txt", MKUpload.GetHost(Host) + DateTime.Now.ToString() + ",开始上传：");
                int FileNo = 0;
                foreach (var item in Pre_FileList)
                {
                    FileNo++;
                    var PathIndex = item.ToString().LastIndexOf(@"\");
                    var fileName = item.ToString().Substring(PathIndex + 1, item.ToString().Length - 1 - PathIndex);

                    //上传文件
                    if (MKUpload.UploadPCLocalHZ(Host, item.ToString(), fileName, out Err))
                    {
                        //移动本地文件到Bak 
                        string BakFilePath = item.ToString().Replace(App.PCHZ, App.PCBakHZ + @"\" + TodayDic);
                        var NewPathIndex = BakFilePath.ToString().LastIndexOf(@"\");
                        var dic = BakFilePath.Substring(0, NewPathIndex); //App.PCBakHZ + @"\" + TodayDic;
                        if (!System.IO.Directory.Exists(dic))
                            System.IO.Directory.CreateDirectory(dic);


                        ////移动到FTP服务器上回执的正式文件夹   
                        //if (!MKUpload.MoveServerHZ(Host, item.ToString(), fileName, out Err))
                        //{
                        //    Utility.Fun.WriteFile(MKUpload.GetHost(Host) + "上传日志.txt", DateTime.Now.ToString() + ",回执[" + fileName + "]移动到FTP服务器的正式文件夹失败！" + Err);
                        //    continue;
                        //}

                        //移动到回执备份的文件夹   
                        if (!MKUpload.PCLocalHZMoveToBak(Host, item.ToString(), BakFilePath, out Err))
                        {
                            Utility.Fun.WriteFile("上传日志.txt", MKUpload.GetHost(Host) + DateTime.Now.ToString() + ",回执[" + fileName + "]备份失败！" + Err);
                            continue;
                        }


                    }
                    else
                    {
                        Utility.Fun.WriteFile("上传日志.txt", MKUpload.GetHost(Host) + DateTime.Now.ToString() + ",回执[" + fileName + "]上传失败" + Err);
                        continue;
                    }



                    //上传结束  
                    Utility.Fun.WriteFile("上传日志.txt", MKUpload.GetHost(Host) + DateTime.Now.ToString() + ",回执[" + fileName + "]上传成功");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",服务启动！");

            Utility.App.g_strWMSDB = Alog.Encryption.CrytMd5.DecryptTo(Utility.Fun.ReadNodeOfXML("WMSDB", "value"));
            Utility.App.g_strOwnerCode = Utility.Fun.ReadNodeOfXML("OwnerCode", "value");
            Utility.App.g_strFtpUploadInterVal = Utility.Fun.ReadNodeOfXML("FtpUploadInterVal", "value");
            Utility.App.g_strFtpDownloadInterVal = Utility.Fun.ReadNodeOfXML("FtpDownloadInterVal", "value");

            Utility.App.g_strFtpHostReport = Utility.Fun.ReadNodeOfXML("FTP_HostReport", "value");
            Utility.App.g_strFtpPwdReport = Utility.Fun.ReadNodeOfXML("PWDReport", "value");
            Utility.App.g_strFtpUSerReport = Utility.Fun.ReadNodeOfXML("USERReport", "value");
            Utility.App.g_strFtpHostReportSecond = Utility.Fun.ReadNodeOfXML("FTP_HostReportSecond", "value");
            Utility.App.g_strFtpPwdReportSecond = Utility.Fun.ReadNodeOfXML("PWDReportSecond", "value");
            Utility.App.g_strFtpUSerReportSecond = Utility.Fun.ReadNodeOfXML("USERReportSecond", "value");
            Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",服务启动2！");

            Utility.App.g_strFtpHostHZ = Utility.Fun.ReadNodeOfXML("FTP_HostHZ", "value");
            Utility.App.g_strFtpPwdHZ = Utility.Fun.ReadNodeOfXML("PWDHZ", "value");
            Utility.App.g_strFtpUSerHZ = Utility.Fun.ReadNodeOfXML("USERHZ", "value");

            // Utility.App.g_strRDCCode = Utility.Fun.ReadNodeOfXML("RDCCode", "value");
            Utility.App.g_strCompanyCode = Utility.Fun.ReadNodeOfXML("CompanyCode", "value");
            Utility.App.g_strCompanyShortName = Utility.Fun.ReadNodeOfXML("CompanyShortName", "value");
            Utility.App.g_strZipNextNumber = Utility.Fun.ReadNodeOfXML("ZipNextNumber", "value");

            Utility.App.g_strDailyStockSettlementTime = Utility.Fun.ReadNodeOfXML("DailyStockSettlementTime", "value").Split(':').GetValue(0).ToString();
            Utility.App.g_strDailyStockSettlementMinute = Utility.Fun.ReadNodeOfXML("DailyStockSettlementTime", "value").Split(':').GetValue(1).ToString();
            App.DownloadFolder = Utility.Fun.ReadNodeOfXML("DownloadFolder", "value");
            App.SmtpHostAddress = Utility.Fun.ReadNodeOfXML("SmtpHostAddress", "value");
            App.SendMailAddress = Utility.Fun.ReadNodeOfXML("SendMailAddress", "value");
            App.SendMailPassword = Utility.Fun.ReadNodeOfXML("SendMailPassword", "value");
            App.ErrorReceiveMailAddress = Utility.Fun.ReadNodeOfXML("ErrorReceiveMailAddress", "value");
            Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",服务启动22！");

            //App.WS_XML = Utility.Fun.ReadNodeOfXML("WS_XML", "value");
            App.WS_XML_BAK = Utility.Fun.ReadNodeOfXML("WS_XML_BAK", "value");
            App.WS_XML_Ftp = Utility.Fun.ReadNodeOfXML("WS_XML_Ftp", "value");
            App.WS_HZ = Utility.Fun.ReadNodeOfXML("WS_HZ", "value");
            App.WS_HZTemp = Utility.Fun.ReadNodeOfXML("WS_HZTemp", "value");
            App.PCBakHZ = Utility.Fun.ReadNodeOfXML("PCBakHZ", "value");
            App.BakXml = Utility.Fun.ReadNodeOfXML("BakXml", "value");
            //App.SendXMl = Utility.Fun.ReadNodeOfXML("SendXMl", "value");
            App.FtpXML = Utility.Fun.ReadNodeOfXML("FtpXML", "value");
            App.FtpXMLTemp = Utility.Fun.ReadNodeOfXML("FtpXMLTemp", "value");
            //App.PCFileRoot = Utility.Fun.ReadNodeOfXML("PCFileRoot", "value");
            App.PCHZ = Utility.Fun.ReadNodeOfXML("PCHZ", "value");
            App.ReadAllFirstLeavlDirHZ = Utility.Fun.ReadNodeOfXML("ReadAllFirstLeavlDirHZ", "value");
            App.ReadHZBackwardMonths = Utility.Fun.ReadNodeOfXML("ReadHZBackwardMonths", "value");
            App.ReadHZDays = Utility.Fun.ReadNodeOfXML("ReadHZDays", "value");
            App.DownLoadOverThenBackFTPXML = Utility.Fun.ReadNodeOfXML("DownLoadOverThenBackFTPXML", "value");
            App.StartTimerFTP_HostReport = Utility.Fun.ReadNodeOfXML("StartTimerFTP_HostReport", "value");
            App.StartTimerFTP_HostReportSecond = Utility.Fun.ReadNodeOfXML("StartTimerFTP_HostReportSecond", "value");
            if (App.StartTimerFTP_HostReport != null && App.StartTimerFTP_HostReport.ToLower() == "true")
            {

                Fun.WriteServerLogFile(DateTime.Now.ToString() + ",服务下载报文开始启动！");
            }

            if (App.StartTimerFTP_HostReportSecond != null && App.StartTimerFTP_HostReportSecond.ToLower() == "true")
            {

                Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",服务下载报文2开始启动！");
            }




            Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",服务上传回执开始启动！");



            //string Errs;
            //if (!ExistsDirectory("AlogDIR\\DOWN\\his_txt", out Errs))
            //    Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",创建文件错误==>" + Errs);

            //if (!ExistsDirectory("AlogDIR\\DOWN\\his_zip", out Errs))
            //    Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",创建文件错误==>" + Errs);

            //if (!ExistsDirectory("AlogDIR\\UP\\his_txt", out Errs))
            //    Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",创建文件错误==>" + Errs);

            //if (!ExistsDirectory("AlogDIR\\UP\\his_zip", out Errs))
            //    Utility.Fun.WriteServerLogFile(DateTime.Now.ToString() + ",创建文件错误==>" + Errs);

            //timerFtpDownload.Start();
            //timerFtpUpload.Start();




            //HostNum Host = HostNum.Report;
            //string Hoststring = "";
            //try
            //{
            //   // timerFtpDownload.Enabled = false;
            //    Host = HostNum.Report;
            //    Hoststring = App.g_strFtpHostReport;
            //    work(Host);
            //}
            //catch (Exception ex)
            //{
            //    Utility.Fun.WriteFile(Hoststring + "下载日志.txt", DateTime.Now.ToString() + ",定时下载同步任务：" + ex.Message);
            //}
            //finally
            //{
            //   // timerFtpDownload.Enabled = true;
            //}

            HostNum Host;
            string Hoststring = "";
            try
            {
                //Host = HostNum.HZ;
                //Hoststring = App.g_strFtpHostHZ;
                //UploadWord(Host);
                //Utility.Fun.WriteFile(App.g_strFtpHostHZ + "上传日志.txt", DateTime.Now.ToString() + ",定时任务开始执行ing....");

                Host = HostNum.HZ;
                Hoststring = App.g_strFtpHostHZ;
                work(Host);
            }
            catch (Exception ex)
            {
                Utility.Fun.WriteFile("上传日志.txt", Hoststring + DateTime.Now.ToString() + "," + ex.Message);
            }
            finally
            {
            }
        }
    
    
    }
}
