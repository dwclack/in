using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using ICSharpCode.SharpZipLib.Zip;

namespace AlogMkFtp.Utility
{
    public enum HostNum
    {
        Report = 1,
        HZ = 2,
        ReportSecond = 3,
    }

    public class MKpub
    {

        string g_strFtpHost;
        string g_strFtpUSer;
        string g_strFtpPwd;

        /// <summary>
        /// 创建NetworkCredential对象
        /// </summary>
        /// <returns></returns>
        public MKpub()
        {

        }


        /// <summary>
        /// 创建NetworkCredential对象
        /// </summary>
        /// <returns></returns>
        public MKpub(HostNum Type)
        {
            switch ((int)Type)
            {
                case (int)HostNum.Report:
                    g_strFtpHost = App.g_strFtpHostReport;
                    g_strFtpUSer = Utility.App.g_strFtpUSerReport;
                    g_strFtpPwd = Utility.App.g_strFtpPwdReport;
                    break;
                case (int)HostNum.ReportSecond:
                    g_strFtpHost = App.g_strFtpHostReportSecond;
                    g_strFtpUSer = Utility.App.g_strFtpUSerReportSecond;
                    g_strFtpPwd = Utility.App.g_strFtpPwdReportSecond;
                    break;
                case (int)HostNum.HZ:
                    g_strFtpHost = App.g_strFtpHostHZ;
                    g_strFtpUSer = Utility.App.g_strFtpUSerHZ;
                    g_strFtpPwd = Utility.App.g_strFtpPwdHZ;
                    break;
                default:
                    g_strFtpHost = App.g_strFtpHostReport;
                    g_strFtpUSer = Utility.App.g_strFtpUSerReport;
                    g_strFtpPwd = Utility.App.g_strFtpPwdReport;
                    break;
            }
        }

        /// <summary>
        /// 创建NetworkCredential对象
        /// </summary>
        /// <returns></returns>
        public NetworkCredential NetworkCredential(HostNum Type)
        {
            switch ((int)Type)
            {
                case (int)HostNum.Report:
                    return new NetworkCredential(Utility.App.g_strFtpUSerReport, Utility.App.g_strFtpPwdReport);
                case (int)HostNum.ReportSecond:
                    return new NetworkCredential(Utility.App.g_strFtpUSerReportSecond, Utility.App.g_strFtpPwdReportSecond);
                case (int)HostNum.HZ:
                    return new NetworkCredential(Utility.App.g_strFtpHostHZ, Utility.App.g_strFtpPwdHZ);
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
                case (int)HostNum.ReportSecond:
                    return App.g_strFtpHostReportSecond;
                default:
                    return App.g_strFtpHostReport;
            }
        }


        /// <summary>
        /// 获取文件后缀名称
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetFex(string filename)
        {
            return filename.Substring(filename.LastIndexOf(".") + 1).ToLower();
        }
        /// <summary>
        /// 获取文件名称不带后缀
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetFilename(string filename)
        {
            return filename.Substring(0, filename.LastIndexOf('.'));
        }
        /// <summary>
        /// 获取文件父目录路径
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetFileParentPath(string filename)
        {
            return filename.Substring(0, filename.LastIndexOf(@"\"));
        }


        /// <summary>
        /// 获取文件包括扩展名
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetFile(string filename)
        {
            var PathIndex = filename.LastIndexOf('/');
            var file = filename.Substring(PathIndex + 1);
            return file;
        }

        /// <summary>
        /// 1FTP服务器上的WS_KJSD_XML_Ftp文件夹的报文
        /// </summary>
        /// <returns></returns>
        public ArrayList Get_Report_FileList(HostNum Host, string Directorys)
        {
            ArrayList FileList = new ArrayList();
            FtpWebResponse listResponsemk = null;
            Stream responseStreammk = null;
            StreamReader readStreammk = null;
            try
            {

                Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + "ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" + Directorys +
                                                    "/");
                FtpWebRequest RequestMK1 =
                    (FtpWebRequest)
                    System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" + Directorys +
                                                    "/");
                RequestMK1.UsePassive = true;
                RequestMK1.KeepAlive = false;
                RequestMK1.Method = WebRequestMethods.Ftp.ListDirectory;
                RequestMK1.Credentials = NetworkCredential(Host);
                listResponsemk = (FtpWebResponse)RequestMK1.GetResponse();
                responseStreammk = listResponsemk.GetResponseStream();
                readStreammk = new StreamReader(responseStreammk, System.Text.Encoding.UTF8);
                string line = readStreammk.ReadLine();
                while (line != null)
                {
                    string filename = GetFex(line);
                    switch (filename)
                    {
                        //准备下载
                        //case "txt":
                        case "xml":
                            FileList.Add("" + Directorys + "/" + line);
                            break;
                    }
                    line = readStreammk.ReadLine();
                }
                readStreammk.Close();
                listResponsemk.Close();
                responseStreammk.Close();
            }
            catch (Exception ex)
            {

                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检报文下载同步出错Get_Report_FileList（） 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", "急,国检报文下载同步出错", out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}


                Utility.Fun.WriteFile("下载日志.txt",
                                      GetHost(Host) + DateTime.Now.ToString() +
                                      ",错误: Get_Report_FileList（） 错误:" + ex.Message + "；" + Error);
            }
            finally
            {
                if (listResponsemk != null)
                    listResponsemk.Close();

                if (responseStreammk != null)
                    responseStreammk.Close();

                if (readStreammk != null)
                    readStreammk.Close();
            }
            return FileList;
        }


        /// <summary>
        /// 获取需要下载的文件
        /// </summary>
        /// <param name="Directorys"></param>
        /// <returns></returns>
        public ArrayList GetFTPReportXMLFiles(HostNum Host, string Directorys)
        {
            ArrayList FileList = new ArrayList();
            FtpWebResponse listResponsemk = null;
            Stream responseStreammk = null;
            StreamReader readStreammk = null;
            FtpWebRequest RequestMK1 = null;
            try
            {
                RequestMK1 = (FtpWebRequest)
                    System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" +
                                                    Directorys + "/");
                RequestMK1.Method = WebRequestMethods.Ftp.ListDirectory;
                RequestMK1.UsePassive = true;
                RequestMK1.KeepAlive = false;
                RequestMK1.Credentials = NetworkCredential(Host);
                var listResponsemkddd = (FtpWebResponse)RequestMK1.GetResponse();
                listResponsemk = (FtpWebResponse)RequestMK1.GetResponse();
                responseStreammk = listResponsemk.GetResponseStream();
                readStreammk = new StreamReader(responseStreammk, System.Text.Encoding.UTF8);
                string line = readStreammk.ReadLine();
                while (line != null)
                {
                    string filename = GetFex(line);
                    switch (filename)
                    {
                        //准备下载
                        case "xml":
                            //case "txt":
                            FileList.Add("" + Directorys + "/" + line);
                            break;
                    }
                    line = readStreammk.ReadLine();
                }
                readStreammk.Close();
                listResponsemk.Close();
                responseStreammk.Close();
            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检报文下载同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", "急,国检报文回执同步错误", out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}

                Utility.Fun.WriteFile("下载日志.txt",
                                      GetHost(Host) + DateTime.Now.ToString() +
                                      ",错误：GetFTPReportXMLFiles() 错误：" + ex.Message + "； " + Error);
            }
            finally
            {
                if (listResponsemk != null)
                {
                    listResponsemk.Close();
                    listResponsemk = null;
                }
                if (responseStreammk != null)
                {
                    responseStreammk.Close();
                    responseStreammk = null;
                }
                if (readStreammk != null)
                {
                    readStreammk.Close();
                    readStreammk = null;
                }
                try
                {
                    RequestMK1.Abort();
                    RequestMK1 = null;
                }
                catch { }
            }
            return FileList;
        }



        /// <summary>
        /// 2下载
        /// </summary>
        /// <param name="MovFileList"></param>
        /// <returns></returns>
        public bool DownloadFTPReportXML(HostNum Host, ArrayList ReportFileList)
        {
            FileStream fileStream = null;
            Stream ftpStream = null;
            FtpWebResponse DowResponse = null;
            for (int i = 0; i < ReportFileList.Count; i++)
            {//一个一个文件执行
                byte[] bt = null;
                try
                {
                    FtpWebRequest DowRequest = (FtpWebRequest)System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" + ReportFileList[i].ToString());
                    DowRequest.UsePassive = true;
                    DowRequest.KeepAlive = false;
                    DowRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    DowRequest.Credentials = NetworkCredential(Host);
                    DowResponse = (FtpWebResponse)DowRequest.GetResponse();

                    #region 下载

                    Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",开始下载报文:" + ReportFileList[i]); 
                    ftpStream = DowResponse.GetResponseStream();
                    MemoryStream men = new MemoryStream(1024 * 500);
                    byte[] buffer = new byte[1024];
                    int byteRead = 0;
                    int TotalByteRead = 0;
                    while (true)
                    {
                        byteRead = ftpStream.Read(buffer, 0, buffer.Length);
                        TotalByteRead += byteRead;
                        if (byteRead == 0)
                            break;
                        men.Write(buffer, 0, byteRead);
                    }
                    if (men.Length > 0)
                    {
                        bt = men.ToArray();
                    }
                    else
                    {
                        bt = null;
                    }
                    fileStream = new FileStream(App.FtpXMLTemp + @"\" + GetFile(ReportFileList[i].ToString()), FileMode.Create);
                    if (bt != null)
                    {
                        fileStream.Write(bt, 0, bt.Length);
                    }
                    fileStream.Flush();
                    fileStream.Close();
                    #endregion



                    //下载完毕 后处理备份，删除，移动
                    string ThisFilePath = App.FtpXMLTemp + @"\" + GetFile(ReportFileList[i].ToString());
                    FileInfo ThisFile = new FileInfo(ThisFilePath);
                    if (ThisFile == null || ThisFile.Length <= 0)
                    {//找不到文件，或文件大小为零 不做移动，删除  直接跳过：主要为了防止文件尚未下载完成，空文件移动到正式文件夹被海关读走，出现报文格式解析错误问题
                        //拷贝到备份
                        if (File.Exists(ThisFilePath))
                        {
                            //删除已下载的文件
                            File.Delete(ThisFilePath);
                        } 
                        continue;
                    }

                    if (App.DownLoadOverThenBackFTPXML != null && (App.DownLoadOverThenBackFTPXML.Trim().ToLower() == "true"))
                    {
                        //将服务器上的文件根据配置是否转移在Ftp服务器上进行备份    并且将服务器上的文件删除   
                        MoveServerFileToBak(Host, ReportFileList[i]);
                    }
                    else
                    {
                        //将服务器上的文件删除  
                        DeleteServerFile(Host, ReportFileList[i].ToString());
                    }

                    // 将PCLocal的XML复制到Bak文件夹下 并且转移到海关读取的文件夹
                    MovePCLocalReportXMLToBak(Host, ReportFileList[i]);


                    string FileNo = "";
                    if (ReportFileList[i] != null)
                    {
                        var ReportFile = ReportFileList[i].ToString();
                        var PathIndex = ReportFile.LastIndexOf(@"/");
                        if (PathIndex > 0)
                        {
                            FileNo = ReportFile.Substring(PathIndex);
                        }
                        else
                        {
                            FileNo = ReportFile;
                        }

                    }


                    //Utility.Fun.WriteFile(GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ",报文[" + FileNo + "]下载成功！");

                }
                catch (Exception ex)
                {

                    string Error = "";
                    //try
                    //{
                    //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                    //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                    //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检报文下载同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", "急,国检报文下载同步出错", out Error);
                    //}
                    //catch (Exception ex2)
                    //{
                    //    Error += ex2.Message;
                    //}
                    Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + "," + ReportFileList[i].ToString() + "错误:DownloadFTPReportXML():" + ex.Message + " ；" + Error);
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream = null;
                    }
                    if (DowResponse != null)
                    {
                        DowResponse.Close();
                        DowResponse = null;
                    }
                    if (ftpStream != null)
                    {
                        ftpStream.Close();
                        DowResponse = null;
                    }
                }
            }
            return true;
        }



        /// <summary>
        /// 从本地WS_KJSD_XML_Ftp文件夹获取已经下载下来的报文
        /// </summary>
        /// <returns></returns>
        public ArrayList GetHaveDownloadReport_FileList(HostNum Host, string DDirectory)
        {
            ArrayList FileList = new ArrayList();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(App.FtpXMLTemp);
                //path为某个目录，如： “D:\Program Files” 
                FileInfo[] inf = dir.GetFiles();
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
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>国检报文下载同步出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}
                Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误：！GetHaveDownloadReport_FileList()：" + ex.Message + " ；" + Error);
                return FileList;
            }
            finally
            {

            }
        }


        /// <summary>
        /// 将服务器的XML移动到Bak文件夹下
        /// </summary>
        /// <param name="FileList"></param>
        public void MoveServerFileToBak(HostNum Host, object FileList)
        {
            try
            {
                if (FileList == null)
                {
                    return;
                }

                string New_zip_File = FileList.ToString();
                var dd = New_zip_File;
                New_zip_File = FileList.ToString().Replace(App.WS_XML_Ftp, App.WS_XML_BAK + @"\Bak_" + DateTime.Now.ToString("yyyyMMdd"));//文件备份的路径
                FtpWeb DirectoryWeb = new FtpWeb(GetHost(Host), App.DownloadFolder + @"\" + App.WS_XML_BAK, NetworkCredential(Host).UserName, NetworkCredential(Host).Password);

                //创建备份目录
                var NewPath = App.DownloadFolder + @"\" + App.WS_XML_BAK + @"\Bak_" + DateTime.Now.ToString("yyyyMMdd");
                string NewDic = @"\Bak_" + DateTime.Now.ToString("yyyyMMdd");//新文件夹

                try
                {
                    //校验是否存在文件夹
                    if (!DirectoryWeb.DirectoryExist("Bak_" + DateTime.Now.ToString("yyyyMMdd")))
                    {  //在Ftp服务器上正式创建
                        DirectoryWeb.MakeDir(NewDic);
                    }
                }
                catch (Exception ex)
                {
                    Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误:DirectoryExist() MakeDir():" + ex.Message);
                }


                string Err = "";
                DirectoryWeb = new FtpWeb(GetHost(Host), App.DownloadFolder, NetworkCredential(Host).UserName, NetworkCredential(Host).Password);
                var List = GetHaveDownloadReport_FileList(Host, App.FtpXMLTemp);//获取已经下载的文件
                foreach (var item in List)
                {
                    string name = item.ToString();
                    string FileListname = FileList.ToString();

                    name = item.ToString().Substring(name.LastIndexOf(@"\") + 1);
                    FileListname = FileListname.ToString().Substring(FileListname.LastIndexOf("/") + 1);
                    if (name == FileListname)
                    {
                        //上传已下载的的文件：由于几番测试移动FTP上的文件来世间备份皆未成功，所以目前只好以重新上传方式来实现，日后维护者找到或实现可改过来
                        var boo = UploadPCLocalHaveDownloadReportToServer(Host, item.ToString(), New_zip_File.ToString(), out Err);
                        if (!string.IsNullOrEmpty(Err))
                        {
                            Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",下载报文移动服务器报文备份时上传文件[" + item.ToString() + "]出错时，发送邮件出错 错误消息:" + Err);
                        }
                        else
                        {
                            //删除服务器的文件
                            DeleteServerFile(Host, FileList.ToString());
                        }
                        break;
                    }
                }

                // Utility.Fun.WriteFile(GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ",移动服务器报文[" + FileList + "]到备份文件夹");
            }
            catch (Exception ex)
            {
                Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误:MoveServerFileToBak():" + ex.Message);
            }
        }
        /// <summary>
        /// 将PCLocal的XML移动到Bak文件夹下
        /// </summary>
        /// <param name="FileList"></param>
        public void MovePCLocalReportXMLToBak(HostNum Host, object FileList)
        {
            //for (int i = 0; i < FileList.Count; i++)
            {
                if (FileList == null)
                {
                    throw new Exception("移动文件到PC备份时出错");
                }
                // file=FileList[i];
                string zipFile = App.FtpXMLTemp + @"\" + GetFile(FileList.ToString());
                try
                {
                    string New_zip_File = zipFile;
                    New_zip_File = zipFile.Replace(App.FtpXMLTemp, App.BakXml + @"\Bak_" + DateTime.Now.ToString("yyyyMMdd"));
                    var FilePath = GetFileParentPath(New_zip_File);
                    if (!System.IO.Directory.Exists(FilePath))
                        System.IO.Directory.CreateDirectory(FilePath);

                    //拷贝到备份
                    if (File.Exists(New_zip_File))
                    {
                        File.Delete(New_zip_File);
                    }
                    File.Copy(zipFile, New_zip_File);
                    //移动到正式读取的文件夹 
                    New_zip_File = zipFile.Replace(App.FtpXMLTemp, App.FtpXML);
                    if (File.Exists(New_zip_File))
                    {
                        File.Delete(New_zip_File);
                    }
                    File.Move(zipFile, New_zip_File);
                }
                catch (Exception ex)
                {
                    Utility.Fun.WriteFile("下载日志.txt", GetHost(Host) + DateTime.Now.ToString() + ",错误:MovePCLocalReportXMLToBak()" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void DeleteServerFile(HostNum Host, string filename)
        {
            string Err = "";
            Stream requestStream = null;
            FileStream fileStream = null;
            FtpWebResponse DeleteResponse = null;
            try
            {
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)
                  System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder + "/" +
                                                  "/" + filename);

                reqFTP.UsePassive = true;
                reqFTP.KeepAlive = false;
                reqFTP.Credentials = NetworkCredential(Host);
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                string result = String.Empty;
                DeleteResponse = (FtpWebResponse)reqFTP.GetResponse();
                long size = DeleteResponse.ContentLength;
                Stream datastream = DeleteResponse.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                DeleteResponse.Close();

                //Utility.Fun.WriteFile(GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ",备份后删除原有报文成功");

            }
            catch (Exception ex)
            {
                string Error = "";
                //try
                //{
                //    Utility.Fun.WriteFile(GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ", <br>DeleteServerFile():备份服务器报文时，删除原有文件出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT");

                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>备份服务器报文时，删除原有文件出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}
                Err = "uploadFile==>错误：" + ex.Message + "； " + Error;
                Utility.Fun.WriteFile("下载日志.txt",
                                    GetHost(Host) + DateTime.Now.ToString() +
                                    ",错误: DeleteServerFile（） 错误:" + ex.Message + "；" + Error);

            }
            finally
            {
                if (DeleteResponse != null)
                    DeleteResponse.Close();
                if (fileStream != null)
                    fileStream.Close();
                if (requestStream != null)
                    requestStream.Close();
            }
        }

        /// <summary>
        /// 将已下载的报文上传到服务器的备份目录
        /// </summary>
        /// <param name="LocalFile"></param>
        /// <param name="filename"></param>
        /// <param name="Err"></param>
        /// <returns></returns>
        public bool UploadPCLocalHaveDownloadReportToServer(HostNum Host, string LocalFile, string filename, out string Err)
        {
            Err = "";
            Stream requestStream = null;
            FileStream fileStream = null;
            FtpWebResponse uploadResponse = null;
            try
            {
                FtpWebRequest uploadRequest =
                  (FtpWebRequest)
                  System.Net.FtpWebRequest.Create("ftp://" + GetHost(Host) + "/" + App.DownloadFolder +
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
                //    Utility.Fun.WriteFile(GetHost(Host) + "下载日志.txt", DateTime.Now.ToString() + ", <br>UploadPCLocalHaveDownloadReportToServer():上传已下载的报文回到服务器备份文件夹出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT");

                //    AlogSendEmail.EmailHelper SendEmail = new AlogSendEmail.EmailHelper();
                //    SendEmail.SendEmail(App.SmtpHostAddress, 25, App.SendMailAddress, App.ErrorReceiveMailAddress,
                //    "yinzheng110", "utf-8", true, "时间 " + DateTime.Now.ToString() + " <br>下载报文移动服务器报文备份时上传文件[" + LocalFile + "]出错 错误消息:" + ex.Message + "<br>" + "如此邮件频繁发出，请及时联系IT", App.MailSubject, out Error);
                //}
                //catch (Exception ex2)
                //{
                //    Error += ex2.Message;
                //}
                Err = "uploadFile==> 错误：" + ex.Message + "；" + Error;
                Utility.Fun.WriteFile("下载日志.txt",
                                    GetHost(Host) + DateTime.Now.ToString() +
                                    ",错误: UploadPCLocalHaveDownloadReportToServer（） 错误:" + ex.Message + "；" + Error);
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
    }
}
