using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections;
using System.Text.RegularExpressions;

namespace AlogMkFtp.Utility
{
    class MKUpload
    {
        /// <summary>
        /// 创建NetworkCredential对象
        /// </summary>
        /// <returns></returns>
        private NetworkCredential NetworkCredential(HostNum Type)
        {
            switch ((int)Type)
            {
                case (int)HostNum.Report:
                    return new NetworkCredential(Utility.App.g_strFtpUSerReport, Utility.App.g_strFtpHostReport);
                case (int)HostNum.HZ:
                    return new NetworkCredential(Utility.App.g_strFtpUSerHZ, Utility.App.g_strFtpPwdHZ);
                default:
                    return new NetworkCredential(Utility.App.g_strFtpUSerReport, Utility.App.g_strFtpPwdReport);
            }
        }

        /// <summary>
        /// 创建NetworkCredential对象
        /// </summary>
        /// <returns></returns>
        public string GetHost(HostNum Type)
        {
            switch ((int)Type)
            {
                case (int)HostNum.Report:
                    return App.g_strFtpHostReport;
                case (int)HostNum.HZ:
                    return App.g_strFtpHostHZ;
                default:
                    return App.g_strFtpHostReport;
            }
        }



        /// <summary>
        /// 中转站服务器回执备份
        /// </summary>
        /// <param name="FileNameList"></param>
        /// <param name="Paht"></param>
        public bool PCLocalHZMoveToBak(HostNum Host, string OldPath, string NewPath, out string err)
        {
            err = "";
            try
            {
                if (File.Exists(NewPath))
                {
                    File.Delete(NewPath);
                }
                File.Move(OldPath, NewPath);
                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;

                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",PCLocalHZMoveToBak()：回执备份失败[" + OldPath + "]：" + err);
                return false;
            }
        }


        public bool UploadPCLocalHZ(HostNum Host, string LocalFile, string filename, out string Err)
        {
            Err = "";
            Stream requestStream = null;
            FileStream fileStream = null;
            FtpWebResponse uploadResponse = null;
            try
            {
                FtpWebRequest uploadRequest =
                  (FtpWebRequest)
                  System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" + App.WS_HZ +
                                                  "/" + filename);
                uploadRequest.UsePassive = true;
                uploadRequest.KeepAlive = false;
                uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                uploadRequest.Credentials = NetworkCredential(Host);
                requestStream = uploadRequest.GetRequestStream();
                fileStream = File.Open(LocalFile, FileMode.Open);
                byte[] buffer = new byte[1024];
                int bytesRead;
                while (true)
                {
                    bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    requestStream.Write(buffer, 0, bytesRead);
                }
                requestStream.Close();
                uploadResponse = (FtpWebResponse)uploadRequest.GetResponse();
                return true;
            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检回执上传同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Utility.Fun.WriteFile(GetHost(Host) + "上传日志.txt", DateTime.Now.ToString() + ",回执[" + LocalFile + "]上传失败时发送邮件失败：" + ex2);
                //    Error += ex2.Message;
                //}

                Err = "uploadFile==>错误：" + ex.Message + " ；" + Error;
                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",UploadPCLocalHZ（）回执[" + LocalFile + "]上传失败：" + Err);
                return false;
            }
            finally
            {
                if (uploadResponse != null)
                    uploadResponse.Close();
                if (fileStream != null)
                    fileStream.Close();
                if (requestStream != null)
                    requestStream.Close();
            }
        }


        public bool MoveServerHZ(HostNum Host, string LocalFile, string filename, out string Err)
        {
            Err = "";
            Stream ftpStream = null;
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest reqFTP =
                  (FtpWebRequest)
                  System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" + App.WS_HZTemp +
                                                  "/" + filename);
                //reqFTP.UsePassive = true;
                reqFTP.KeepAlive = false;
                reqFTP.UseBinary = true;
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                //reqFTP.RenameTo = @"WS_KJSD_Program188/6_GZ_1.0_HG_ftpTest/WS_KJSD_HZ" + "/" + filename;
                reqFTP.RenameTo = "~/" + App.WS_HZ.Replace(@"\", @"/") +
                                                  "/" + filename;
                reqFTP.Credentials = NetworkCredential(Host);

                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",MoveServerHZ（）"
                    + reqFTP.RenameTo.ToString());

                response = (FtpWebResponse)reqFTP.GetResponse();
                ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检回执上传同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Utility.Fun.WriteFile(GetHost(Host) + "上传日志.txt", DateTime.Now.ToString() + ",回执[" + LocalFile + "]上传失败时发送邮件失败：" + ex2);
                //    Error += ex2.Message;
                //}

                Err = "uploadFile==>错误：" + ex.Message + " ；" + Error;
                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",MoveServerHZ（）回执[" + LocalFile + "]上传失败：" + Err);
                return false;
            }
            finally
            {
                if (response != null)
                    response.Close();
                if (ftpStream != null)
                    ftpStream.Close();
            }
        }




        /// <summary>
        /// 从中转本地WS_KJSD_HZ文件夹获取回执 (读取回执目录第一级所有文件夹的xml文件)
        /// </summary>
        /// <returns></returns>
        public ArrayList GetPCLocalHZListAll(HostNum Host)
        {
            ArrayList FileList = new ArrayList();
            FileInfo[] inf = new FileInfo[0];
            //获取日期子目录文件
            DateTime dt = DateTime.Now.AddDays(-7);
            //DateTime dt = DateTime.Now;
            ArrayList PathList = new ArrayList();
            try
            {
                PathList.Add(App.PCHZ); //添加主目录    ：主目录必须添加




                if (App.ReadAllFirstLeavlDirHZ != null && (App.ReadAllFirstLeavlDirHZ.Trim().ToLower() == "true"))
                {//读取所有一级文件夹
                    DirectoryInfo[] DirTemp = new DirectoryInfo(App.PCHZ).GetDirectories();//读取该文件夹的XML文件
                    if (DirTemp!=null&&DirTemp.Length>0)
                    {
                        //PathList.AddRange(DirTemp);
                        foreach (var item in DirTemp)
                        {
                            if (item != null)
                            {
                                PathList.Add(item.FullName);
                            }
                        }
                    } 
                }
                else
                {
                    int MonthAcount = 0;
                    int DayAcount = 1;
                    int MonthDay = 0;
                    string ReadHZMonths = App.ReadHZBackwardMonths == null ? "" : App.ReadHZBackwardMonths;
                    string ReadHZDays = App.ReadHZDays == null ? "" : App.ReadHZDays;
                    if (!IsNumber(ReadHZMonths))
                    {
                        MonthAcount = 0;//默认往前推0个月
                    }
                    else
                    {
                        MonthAcount = int.Parse(ReadHZMonths);
                    }

                    if (!IsNumber(ReadHZDays))
                    {
                        DayAcount = 1;//当月默认第一天
                    }
                    else
                    {
                        DayAcount = int.Parse(ReadHZDays);
                    }

                    MonthAcount = MonthAcount < 0 ? 0 : MonthAcount;
                    DayAcount = DayAcount < 1 ? 1 : DayAcount;


                    if (MonthAcount < 1 && dt.Day <= DayAcount)
                    { 
                        //当没有进行往前推月份设置时 且日期为每月的第一天则需要读取前一个月文件夹 
                        DateTime dtTemp = dt.AddMonths(-1);
                        DateTime dtFirstDay = dtTemp.AddDays(-dtTemp.Day + 1);
                        MonthDay = DateTime.DaysInMonth(dtTemp.Year, dtTemp.Month);


                        for (int i = 0; i < MonthDay; i++)
                        {
                            PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(i).ToString("yyyyMMdd"));
                            PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(i).ToString("yyyy-MM-dd"));
                        }
                    }

                    for (int i = MonthAcount; i >= 0; i--)
                    {
                        DateTime dtTemp = dt.AddMonths(-i);
                        DateTime dtFirstDay = dtTemp.AddDays(-dtTemp.Day + 1);
                        MonthDay = DateTime.DaysInMonth(dtTemp.Year, dtTemp.Month);
                        for (int j = 0; j < MonthDay; j++)
                        {
                            PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(j).ToString("yyyyMMdd"));
                            PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(j).ToString("yyyy-MM-dd"));
                        }
                    }
                }
                  
                foreach (var item in PathList)
                {
                    if (item != null && item.ToString().Trim() != "" && Directory.Exists(item.ToString()) == true)
                    {//文件夹存在
                        FileInfo[] infTemp = new DirectoryInfo(item.ToString()).GetFiles("*.xml");//读取该文件夹的XML文件
                        if (infTemp != null && infTemp.Count() > 0)
                        {
                            inf.Concat(infTemp);
                            inf = inf.Concat(infTemp).ToArray();
                            if (inf.Length>10000)
                            {//文件数大于一万则先处理完毕
                                break;
                            } 
                        }
                    }
                }


                foreach (FileInfo finf in inf)
                {
                    if (finf.Extension.Equals(".xml"))
                        //如果扩展名为“.xml”  
                        FileList.Add(finf.FullName);
                    //if (finf.Extension.Equals(".xml"))
                    //    //如果扩展名为“.txt”  
                    //    FileList.Add(finf.FullName); 
                }


                return FileList;
            }
            catch (Exception ex)
            {
                string Error = "";
                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误：！GetPCLocalHZList" + ex.Message + "；" + Error);
                return FileList;
            }
        }

        /// <summary>
        /// 从中转本地WS_KJSD_HZ文件夹获取回执 (仅仅都取文件夹以格式为YYYYmmdd 或者YYYY-MM-DD的文件夹xml文件)
        /// </summary>
        /// <returns></returns>
        public ArrayList GetPCLocalHZListyyyymmdd(HostNum Host)
        {
            ArrayList FileList = new ArrayList();
            FileInfo[] inf = new FileInfo[0];
            //获取日期子目录文件
            DateTime dt = DateTime.Now.AddDays(-7);
            //DateTime dt = DateTime.Now;
            ArrayList PathList = new ArrayList();
            try
            {
                PathList.Add(App.PCHZ); //主目录


                int MonthAcount = 0;
                int DayAcount = 1;
                int MonthDay = 0;
                string ReadHZMonths = App.ReadHZBackwardMonths == null ? "" : App.ReadHZBackwardMonths;
                string ReadHZDays = App.ReadHZDays == null ? "" : App.ReadHZDays;
                if (!IsNumber(ReadHZMonths))
                {
                    MonthAcount = 0;
                }
                else
                {
                    MonthAcount = int.Parse(ReadHZMonths);
                }

                if (!IsNumber(ReadHZDays))
                {
                    DayAcount = 1;
                }
                else
                {
                    DayAcount = int.Parse(ReadHZDays);
                }

                MonthAcount = MonthAcount < 0 ? 0 : MonthAcount;
                DayAcount = DayAcount < 1 ? 1 : DayAcount;


                if (MonthAcount < 1 && dt.Day <= DayAcount)
                { //当没有进行往前推月份设置时 且日期为每月的第一天则需要读取前一个月文件夹 
                    DateTime dtTemp = dt.AddMonths(-1);
                    DateTime dtFirstDay = dtTemp.AddDays(-dtTemp.Day + 1);
                    MonthDay = DateTime.DaysInMonth(dtTemp.Year, dtTemp.Month);


                    for (int i = 0; i < MonthDay; i++)
                    {
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(i).ToString("yyyyMMdd"));
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(i).ToString("yyyy-MM-dd"));
                    }
                }

                for (int i = MonthAcount; i >= 0; i--)
                {
                    DateTime dtTemp = dt.AddMonths(-i);
                    DateTime dtFirstDay = dtTemp.AddDays(-dtTemp.Day + 1);
                    MonthDay = DateTime.DaysInMonth(dtTemp.Year, dtTemp.Month);
                    for (int j = 0; j < MonthDay; j++)
                    {
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(j).ToString("yyyyMMdd"));
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(j).ToString("yyyy-MM-dd"));
                    }
                }
                foreach (var item in PathList)
                {
                    if (item != null && Directory.Exists(item.ToString()) == true)
                    {//文件夹存在
                        FileInfo[] infTemp = new DirectoryInfo(item.ToString()).GetFiles("*.xml");//读取该文件夹的XML文件
                        if (infTemp != null && infTemp.Count() > 0)
                        {
                            inf.Concat(infTemp);
                            inf = inf.Concat(infTemp).ToArray();
                        }
                    }
                }

                foreach (FileInfo finf in inf)
                {
                    if (finf.Extension.Equals(".xml"))
                        //如果扩展名为“.xml”  
                        FileList.Add(finf.FullName);
                    //if (finf.Extension.Equals(".xml"))
                    //    //如果扩展名为“.txt”  
                    //    FileList.Add(finf.FullName); 
                }


                return FileList;
            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检回执上传同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}
                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误：！GetPCLocalHZList" + ex.Message + "；" + Error);
                return FileList;
            }
        }

        /// <summary>
        /// 从中转本地WS_KJSD_HZ文件夹获取回执 (仅仅都取文件夹以格式为YYYYmmdd的文件夹xml文件)
        /// </summary>
        /// <returns></returns>
        public ArrayList GetPCLocalHZListyyyy_mm_dd(HostNum Host)
        {
            ArrayList FileList = new ArrayList();
            FileInfo[] inf = new FileInfo[0];
            //获取日期子目录文件
            DateTime dt = DateTime.Now.AddDays(-7);
            //DateTime dt = DateTime.Now;
            ArrayList PathList = new ArrayList();
            try
            {
                PathList.Add(App.PCHZ); //主目录


                int MonthAcount = 0;
                int DayAcount = 1;
                int MonthDay = 0;
                string ReadHZMonths = App.ReadHZBackwardMonths == null ? "" : App.ReadHZBackwardMonths;
                string ReadHZDays = App.ReadHZDays == null ? "" : App.ReadHZDays;
                if (!IsNumber(ReadHZMonths))
                {
                    MonthAcount = 0;
                }
                else
                {
                    MonthAcount = int.Parse(ReadHZMonths);
                }

                if (!IsNumber(ReadHZDays))
                {
                    DayAcount = 1;
                }
                else
                {
                    DayAcount = int.Parse(ReadHZDays);
                }

                MonthAcount = MonthAcount < 0 ? 0 : MonthAcount;
                DayAcount = DayAcount < 1 ? 1 : DayAcount;


                if (MonthAcount < 1 && dt.Day <= DayAcount)
                { //当没有进行往前推月份设置时 且日期为每月的第一天则需要读取前一个月文件夹 
                    DateTime dtTemp = dt.AddMonths(-1);
                    DateTime dtFirstDay = dtTemp.AddDays(-dtTemp.Day + 1);
                    MonthDay = DateTime.DaysInMonth(dtTemp.Year, dtTemp.Month);


                    for (int i = 0; i < MonthDay; i++)
                    {

                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(i).ToString("yyyyMMdd"));
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(i).ToString("yyyy-MM-dd"));
                    }
                }

                for (int i = MonthAcount; i >= 0; i--)
                {
                    DateTime dtTemp = dt.AddMonths(-i);
                    DateTime dtFirstDay = dtTemp.AddDays(-dtTemp.Day + 1);
                    MonthDay = DateTime.DaysInMonth(dtTemp.Year, dtTemp.Month);
                    for (int j = 0; j < MonthDay; j++)
                    {
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(j).ToString("yyyyMMdd"));
                        PathList.Add(App.PCHZ + @"/" + dtFirstDay.AddDays(j).ToString("yyyy-MM-dd"));
                    }
                }
                foreach (var item in PathList)
                {
                    if (item != null && Directory.Exists(item.ToString()) == true)
                    {
                        FileInfo[] infTemp = new DirectoryInfo(item.ToString()).GetFiles("*.xml");//读取该文件夹的XML文件
                        if (infTemp != null && infTemp.Count() > 0)
                        {
                            inf.Concat(infTemp);
                            inf = inf.Concat(infTemp).ToArray();
                        }
                    }
                }

                foreach (FileInfo finf in inf)
                {
                    if (finf.Extension.Equals(".xml"))
                        //如果扩展名为“.xml”  
                        FileList.Add(finf.FullName);
                    //if (finf.Extension.Equals(".xml"))
                    //    //如果扩展名为“.txt”  
                    //    FileList.Add(finf.FullName); 
                }


                return FileList;
            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检回执上传同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}
                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误：！GetPCLocalHZList" + ex.Message + "；" + Error);
                return FileList;
            }
        }

        /// <summary>
        /// 从中转本地WS_KJSD_HZ文件夹获取回执按月读回执
        /// </summary>
        /// <returns></returns>
        public ArrayList GetPCLocalHZList2(HostNum Host)
        {
            ArrayList FileList = new ArrayList();
            FileInfo[] inf = new FileInfo[0];
            //获取日期子目录文件
            //DateTime dt = DateTime.Now.AddDays(-1);
            DateTime dt = DateTime.Now;
            ArrayList PathList = new ArrayList();
            try
            {
                PathList.Add(App.PCHZ); //主目录


                int MonthAcount = 0;
                int DayAcount = 1;
                string ReadHZMonths = App.ReadHZBackwardMonths == null ? "" : App.ReadHZBackwardMonths;
                string ReadHZDays = App.ReadHZDays == null ? "" : App.ReadHZDays;
                if (!IsNumber(ReadHZMonths))
                {
                    MonthAcount = 0;
                }
                else
                {
                    MonthAcount = int.Parse(ReadHZMonths);
                }

                if (!IsNumber(ReadHZDays))
                {
                    DayAcount = 1;
                }
                else
                {
                    DayAcount = int.Parse(ReadHZDays);
                }

                MonthAcount = MonthAcount < 0 ? 0 : MonthAcount;
                DayAcount = DayAcount < 1 ? 1 : DayAcount;


                if (MonthAcount < 1 && dt.Day <= DayAcount)
                { //当没有进行往前推月份设置时 且日期为每月的第一天则需要读取前一个月文件夹 
                    PathList.Add(App.PCHZ + @"/" + dt.AddMonths(-1).ToString("yyyyMM"));
                    PathList.Add(App.PCHZ + @"/" + dt.AddMonths(-1).ToString("yyyy-MM"));
                }

                for (int i = MonthAcount; i >= 0; i--)
                {
                    PathList.Add(App.PCHZ + @"/" + dt.AddMonths(-i).ToString("yyyyMM"));
                    PathList.Add(App.PCHZ + @"/" + dt.AddMonths(-i).ToString("yyyy-MM"));
                }
                foreach (var item in PathList)
                {
                    if (item != null && Directory.Exists(item.ToString()) == true)
                    {
                        FileInfo[] infTemp = new DirectoryInfo(item.ToString()).GetFiles("*.xml");//读取该文件夹的XML文件
                        if (infTemp != null && infTemp.Count() > 0)
                        {
                            inf.Concat(infTemp);
                            inf = inf.Concat(infTemp).ToArray();
                        }
                    }
                }

                foreach (FileInfo finf in inf)
                {
                    if (finf.Extension.Equals(".xml"))
                        //如果扩展名为“.xml”  
                        FileList.Add(finf.FullName);
                    //if (finf.Extension.Equals(".txt"))
                    //    //如果扩展名为“.txt”  
                    //    FileList.Add(finf.FullName); 
                }

                return FileList;
            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检回执上传同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}
                Utility.Fun.WriteFile("上传日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误：！GetPCLocalHZList" + ex.Message + "；" + Error);
                return FileList;
            }
        }

        /// <summary>
        /// 从中转本地WS_KJSD_HZ文件夹获取回执
        /// </summary>
        /// <returns></returns>
        public bool IsNumber(string Value)
        {
            try
            {
                //string Reg = @"^(d{1,9}$)";^[1-9]\d*$
                string Reg = @"^[1-9]\d*$";
                Regex x = new Regex(Reg, RegexOptions.Compiled);
                Match M = x.Match(Value);
                if (!M.Success)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Utility.Fun.WriteFile("Erro.txt", DateTime.Now.ToString() + ",错误：！IsNumber()" + ex.Message + " 错误");
                return false;
            }
        }
    }
}