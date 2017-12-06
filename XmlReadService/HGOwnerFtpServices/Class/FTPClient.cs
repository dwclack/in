using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Alog_WSKJSD;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.Utilities
{
    public class FTPClient
    {
        public static object obj = new object();

        #region 构造函数
        /// <summary>
        /// 缺省构造函数
        /// </summary>
        public FTPClient()
        {
            strRemoteHost = "";
            strRemotePath = "";
            strRemoteUser = "";
            strRemotePass = "";
            strRemotePort = 21;
            bConnected = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public FTPClient(string remoteHost, string remotePath, string remoteUser, string remotePass, int remotePort)
        {
            strRemoteHost = remoteHost;
            strRemotePath = remotePath;
            strRemoteUser = remoteUser;
            strRemotePass = remotePass;
            strRemotePort = remotePort;
            Connect();
        }
        #endregion

        #region 字段
        private int strRemotePort;
        private Boolean bConnected;
        private string strRemoteHost;
        private string strRemotePass;
        private string strRemoteUser;
        private string strRemotePath;

        /// <summary>
        /// 服务器返回的应答信息(包含应答码)
        /// </summary>
        private string strMsg;
        /// <summary>
        /// 服务器返回的应答信息(包含应答码)
        /// </summary>
        private string strReply;
        /// <summary>
        /// 服务器返回的应答码
        /// </summary>
        private int iReplyCode;
        /// <summary>
        /// 进行控制连接的socket
        /// </summary>
        private Socket socketControl;
        /// <summary>
        /// 传输模式
        /// </summary>
        private TransferType trType;
        /// <summary>
        /// 接收和发送数据的缓冲区
        /// </summary>
        private static int BLOCK_SIZE = 512;
        /// <summary>
        /// 编码方式
        /// </summary>
        Encoding ASCII = Encoding.ASCII;
        /// <summary>
        /// 字节数组
        /// </summary>
        Byte[] buffer = new Byte[BLOCK_SIZE];
        #endregion

        #region 属性
        /// <summary>
        /// FTP服务器IP地址
        /// </summary>
        public string RemoteHost
        {
            get
            {
                return strRemoteHost;
            }
            set
            {
                strRemoteHost = value;
            }
        }

        /// <summary>
        /// FTP服务器端口
        /// </summary>
        public int RemotePort
        {
            get
            {
                return strRemotePort;
            }
            set
            {
                strRemotePort = value;
            }
        }

        /// <summary>
        /// 当前服务器目录
        /// </summary>
        public string RemotePath
        {
            get
            {
                return strRemotePath;
            }
            set
            {
                strRemotePath = value;
            }
        }

        /// <summary>
        /// 登录用户账号
        /// </summary>
        public string RemoteUser
        {
            set
            {
                strRemoteUser = value;
            }
        }

        /// <summary>
        /// 用户登录密码
        /// </summary>
        public string RemotePass
        {
            set
            {
                strRemotePass = value;
            }
        }

        /// <summary>
        /// 是否登录
        /// </summary>
        public bool Connected
        {
            get
            {
                return bConnected;
            }
        }
        #endregion

        #region 链接
        /// <summary>
        /// 建立连接 
        /// </summary>
        public void Connect()
        {
            lock (obj)
            {
                socketControl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(RemoteHost), strRemotePort);
                try
                {
                    socketControl.Connect(ep);
                    
                }
                catch (Exception)
                {
                    throw new IOException("不能连接ftp服务器");
                }
            }
            ReadReply();
            if (iReplyCode != 220)
            {
                DisConnect();
                throw new IOException(strReply.Substring(4));
            }
            SendCommand("USER " + strRemoteUser);
            if (!(iReplyCode == 331 || iReplyCode == 230))
            {
                CloseSocketConnect();
                throw new IOException(strReply.Substring(4));
            }
            if (iReplyCode != 230)
            {
                SendCommand("PASS " + strRemotePass);
                if (!(iReplyCode == 230 || iReplyCode == 202))
                {
                    CloseSocketConnect();
                    throw new IOException(strReply.Substring(4));
                }
            }
            bConnected = true;
            ChDir(strRemotePath);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void DisConnect()
        {
            if (socketControl != null)
            {
                SendCommand("QUIT");
            }
            CloseSocketConnect();
        }
        #endregion

        #region 传输模式
        /// <summary>
        /// 传输模式:二进制类型、ASCII类型
        /// </summary>
        public enum TransferType { Binary, ASCII };

        /// <summary>
        /// 设置传输模式
        /// </summary>
        /// <param name="ttType">传输模式</param>
        public void SetTransferType(TransferType ttType)
        {
            if (ttType == TransferType.Binary)
            {
                SendCommand("TYPE I");//binary类型传输
            }
            else
            {
                SendCommand("TYPE A");//ASCII类型传输
            }
            if (iReplyCode != 200)
            {
                throw new IOException(strReply.Substring(4));
            }
            else
            {
                trType = ttType;
            }
        }

        /// <summary>
        /// 获得传输模式
        /// </summary>
        /// <returns>传输模式</returns>
        public TransferType GetTransferType()
        {
            return trType;
        }
        #endregion

        #region 文件操作
        /// <summary>
        /// 获得文件列表
        /// </summary>
        /// <param name="strMask">文件名的匹配字符串</param>
        public string[] Dir(string strMask)
        {
            if (!bConnected)
            {
                Connect();
            }
            Socket socketData = CreateDataSocket();
            SendCommand("NLST " + strMask);
            if (!(iReplyCode == 150 || iReplyCode == 125 || iReplyCode == 226))
            {
                throw new IOException(strReply.Substring(4));
            }
            strMsg = "";
            Thread.Sleep(2000);
            while (true)
            {
                int iBytes = socketData.Receive(buffer, buffer.Length, 0);
                strMsg += ASCII.GetString(buffer, 0, iBytes);
                if (iBytes < buffer.Length)
                {
                    break;
                }
            }
            char[] seperator = { '\n' };
            string[] strsFileList = strMsg.Split(seperator);
            socketData.Close(); //数据socket关闭时也会有返回码
            if (iReplyCode != 226)
            {
                ReadReply();
                if (iReplyCode != 226)
                {

                    throw new IOException(strReply.Substring(4));
                }
            }
            return strsFileList;
        }

        public void newPutByGuid(string strFileName, string strGuid)
        {
            if (!bConnected)
            {
                Connect();
            }
            string str = strFileName.Substring(0, strFileName.LastIndexOf("\\"));
            string strTypeName = strFileName.Substring(strFileName.LastIndexOf("."));
            strGuid = str + "\\" + strGuid;
            Socket socketData = CreateDataSocket();
            SendCommand("STOR " + Path.GetFileName(strGuid));
            if (!(iReplyCode == 125 || iReplyCode == 150))
            {
                throw new IOException(strReply.Substring(4));
            }
            using (FileStream input = new FileStream(strGuid, FileMode.Open))
            {
                input.Flush();
                int iBytes = 0;
                while ((iBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    socketData.Send(buffer, iBytes, 0);
                }
                input.Close();
            }
            if (socketData.Connected)
            {
                socketData.Close();
            }
            if (!(iReplyCode == 226 || iReplyCode == 250))
            {
                ReadReply();
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <returns>文件大小</returns>
        public long GetFileSize(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("SIZE " + Path.GetFileName(strFileName));
            long lSize = 0;
            if (iReplyCode == 213)
            {
                lSize = Int64.Parse(strReply.Substring(4));
            }
            else
            {
                throw new IOException(strReply.Substring(4));
            }
            return lSize;
        }


        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <returns>文件大小</returns>
        public string GetFileInfo(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            Socket socketData = CreateDataSocket();
            SendCommand("LIST " + strFileName);
            string strResult = "";
            if (!(iReplyCode == 150 || iReplyCode == 125
                || iReplyCode == 226 || iReplyCode == 250))
            {
                throw new IOException(strReply.Substring(4));
            }
            byte[] b = new byte[512];
            MemoryStream ms = new MemoryStream();

            while (true)
            {
                int iBytes = socketData.Receive(b, b.Length, 0);
                ms.Write(b, 0, iBytes);
                if (iBytes <= 0)
                {

                    break;
                }
            }
            byte[] bt = ms.GetBuffer();
            strResult = System.Text.Encoding.ASCII.GetString(bt);
            ms.Close();
            return strResult;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="strFileName">待删除文件名</param>
        public void Delete(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("DELE " + strFileName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        /// <summary>
        /// 重命名(如果新文件名与已有文件重名,将覆盖已有文件)
        /// </summary>
        /// <param name="strOldFileName">旧文件名</param>
        /// <param name="strNewFileName">新文件名</param>
        public void Rename(string strOldFileName, string strNewFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("RNFR " + strOldFileName);
            if (iReplyCode != 350)
            {
                throw new IOException(strReply.Substring(4));
            }
            //  如果新文件名与原有文件重名,将覆盖原有文件
            SendCommand("RNTO " + strNewFileName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
        }
        #endregion

        #region 上传和下载
        /// <summary>
        /// 下载一批文件
        /// </summary>
        /// <param name="strFileNameMask">文件名的匹配字符串</param>
        /// <param name="strFolder">本地目录(不得以\结束)</param>
        /// <param name="strFolder">本地备份</param>
        public void GetAll(string strFileNameMask, string strFolder, string bakFolder)
        {
            if (!bConnected)
            {
                Connect();
            }
           
            List<string> listFileList = new List<string>();
            int scount = 0;
            try
            {
                string[] strFiles = Dir(strFileNameMask);
                foreach (string strFile in strFiles)
                {
                    if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
                    {
                        Get(strFile.Trim(), strFolder+ @"\temp", strFile.Trim());
                        FileInfo fileInfo = new FileInfo(strFolder + @"\temp" + @"\" + strFile.Trim());
                        if (fileInfo.Length <= 0)
                        {
                            //如果文件小于等于0就执行下一个文件下载
                            continue;
                        }
                        ClsLog.CopyFile(strFile.Trim(), strFolder + @"\temp\",
                                           strFolder + @"\");
                        ClsLog.DeleteFile(strFolder + @"\temp\" + strFile.Trim());
                        ClsLog.CopyFile(strFile.Trim(), strFolder + @"\",
                                           @"" + bakFolder + DateTime.Now.ToString("yyyyMMdd") + @"\");
                        listFileList.Add(strFile);

                       
                        try
                        {
                            Delete(strFile.Trim());
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件成功:" + strFile, "下载日志");
                        }
                        catch (Exception ex)
                        {
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                        }
                        scount++;
                        if (scount > 100)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("下载FTP文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            }

            //try
            //{
            //    foreach (string strFile in listFileList)
            //    {
            //        if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
            //        {
            //            Delete(strFile.Trim());
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    NLogger.WriteLog("删除FTP文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            //}
        }

        /// <summary>
        /// 多线程下载一批文件
        /// </summary>
        /// <param name="strFileNameMask">文件名的匹配字符串</param>
        /// <param name="strFolder">本地目录(不得以\结束)</param>
        /// <param name="strFolder">本地备份</param>
        public void GetAllThread(string strFileNameMask, string strFolder, string bakFolder)
        {
            if (!bConnected)
            {
                Connect();
            }

            int scount = 0;
            try
            {
                string[] strFiles = Dir(strFileNameMask);
                int dataCount = strFiles.Length;
                var list = strFiles.Cast<string>().ToArray();
                int pageCount = 10;//分页数默认10页
                int pageSize = dataCount % pageCount == 0 ? (dataCount / pageCount) : (dataCount / pageCount + 1);
                NLogger.WriteLog("下载分页开始 , Data Count : " + dataCount + ", Page Size:" + pageSize, "MultiPage");
                Parallel.For(0, pageCount, pageIndex =>
                {

                    //List<string> listFileList = new List<string>();
                    var data = list.Skip(pageIndex * pageSize).Take(pageSize);
                    //var data = strFiles;
                    foreach (string strFile in data)
                    {
                        if (!strFile.Equals(""))//一般来说strFiles的最后一个元素可能是空字符串
                        {
                            Get(strFile.Trim(), strFolder + @"\temp", strFile.Trim());
                            FileInfo fileInfo = new FileInfo(strFolder + @"\temp" + @"\" + strFile.Trim());
                            if (fileInfo.Length <= 0)
                            {
                                //如果文件小于等于0就执行下一个文件下载
                                continue;
                            }
                            ClsLog.CopyFile(strFile.Trim(), strFolder + @"\temp\",
                                               strFolder + @"\");
                            ClsLog.DeleteFile(strFolder + @"\temp\" + strFile.Trim());
                            ClsLog.CopyFile(strFile.Trim(), strFolder + @"\",
                                               @"" + bakFolder + DateTime.Now.ToString("yyyyMMdd") + @"\");
                            //listFileList.Add(strFile);


                            try
                            {
                                Delete(strFile.Trim());
                                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件成功:" + strFile, "下载日志");
                            }
                            catch (Exception ex)
                            {
                                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
                            }
                            //scount++;
                            //if (scount > 100)
                            //{
                            //    break;
                            //}
                        }
                    }
                });
                NLogger.WriteLog("下载分页完成 , Data Count : " + dataCount + ", Page Size:" + pageSize, "MultiPage");
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("下载FTP文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "下载日志");
            }

        }

        /// <summary>
        /// 下载一个文件
        /// </summary>
        /// <param name="strRemoteFileName">要下载的文件名</param>
        /// <param name="strFolder">本地目录(不得以\结束)</param>
        /// <param name="strLocalFileName">保存在本地时的文件名</param>
        public void Get(string strRemoteFileName, string strFolder, string strLocalFileName)
        {
            Socket socketData = CreateDataSocket();
            try
            {
                if (!bConnected)
                {
                    Connect();
                }
                SetTransferType(TransferType.Binary);
                if (strLocalFileName.Equals(""))
                {
                    strLocalFileName = strRemoteFileName;
                }
                SendCommand("RETR " + strRemoteFileName);
                if (!(iReplyCode == 150 || iReplyCode == 125 || iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
                using (FileStream output = new FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create))
                {
                    while (true)
                    {
                        int iBytes = socketData.Receive(buffer, buffer.Length, 0);
                        output.Write(buffer, 0, iBytes);
                        if (iBytes <= 0)
                        {
                            break;
                        }
                    }
                    output.Close();
                }
                if (socketData.Connected)
                {
                    socketData.Close();
                }
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    ReadReply();
                    if (!(iReplyCode == 226 || iReplyCode == 250))
                    {
                        throw new IOException(strReply.Substring(4));
                    }
                }
            }
            catch(Exception ex)
            {
                socketData.Close();
                socketData = null;
                socketControl.Close();
                bConnected = false;
                socketControl = null;
                NLogger.WriteLog("GET方法" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "下载日志");
                throw ex;
            }
        }

        /// <summary>
        /// 下载一个文件
        /// </summary>
        /// <param name="strRemoteFileName">要下载的文件名</param>
        /// <param name="strFolder">本地目录(不得以\结束)</param>
        /// <param name="strLocalFileName">保存在本地时的文件名</param>
        public void GetNoBinary(string strRemoteFileName, string strFolder, string strLocalFileName)
        {
            if (!bConnected)
            {
                Connect();
            }

            if (strLocalFileName.Equals(""))
            {
                strLocalFileName = strRemoteFileName;
            }
            Socket socketData = CreateDataSocket();
            SendCommand("RETR " + strRemoteFileName);
            if (!(iReplyCode == 150 || iReplyCode == 125 || iReplyCode == 226 || iReplyCode == 250))
            {
                throw new IOException(strReply.Substring(4));
            }
            using (FileStream output = new FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create))
            {
                while (true)
                {
                    int iBytes = socketData.Receive(buffer, buffer.Length, 0);
                    output.Write(buffer, 0, iBytes);
                    if (iBytes <= 0)
                    {
                        break;
                    }
                }
                output.Close();
            }
            if (socketData.Connected)
            {
                socketData.Close();
            }
            if (!(iReplyCode == 226 || iReplyCode == 250))
            {
                ReadReply();
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
        }

        /// <summary>
        /// 上传一批文件
        /// </summary>
        /// <param name="strFolder">本地目录(不得以\结束)</param>
        /// <param name="strFileNameMask">文件名匹配字符(可以包含*和?)</param>
        /// /// <param name="FtpUpPathBak">备份目录</param>
        public void Put(string strFolder, string strFileNameMask, string FtpUpPathBak)
        {
            List<string> listFileList = new List<string>();
            int scount = 0;
            try
            {
                string[] strFiles = Directory.GetFiles(strFolder, strFileNameMask);
                foreach (string strFile in strFiles)
                {
                    listFileList.Add(strFile);
                    NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "开始上传:" + strFile, "上传日志");
                    Put(strFile);
                    NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "上传FTP文件成功:" + strFile, "上传日志");
                    try
                    {
                        ClsLog.CopyFile(Path.GetFileName(strFile), Path.GetDirectoryName(strFile) + @"\",
                                            @"" + FtpUpPathBak + DateTime.Now.ToString("yyyyMMdd") + @"\");
                        ClsLog.DeleteFile(strFile);
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件成功:" + strFile, "上传日志");
                    }
                    catch (Exception ex)
                    {
                        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "删除文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
                    }
                    scount++;
                    if (scount > 100)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("上传FTP文件失败：" + ex.Message + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "上传日志");
            }
        }

        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="strFileName">本地文件名</param>
        public void Put(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            Socket socketData = CreateDataSocket();
            if (Path.GetExtension(strFileName) == "")
                SendCommand("STOR " + Path.GetFileNameWithoutExtension(strFileName));
            else
                SendCommand("STOR " + Path.GetFileName(strFileName));

            if (!(iReplyCode == 125 || iReplyCode == 150))
            {
                throw new IOException(strReply.Substring(4));
            }

            using (FileStream input = new FileStream(strFileName, FileMode.Open))
            {
                int iBytes = 0;
                while ((iBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    socketData.Send(buffer, iBytes, 0);
                }
                input.Close();
            }
            if (socketData.Connected)
            {
                socketData.Close();
            }
           // NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "iReplyCode:" + iReplyCode, "上传日志");
            if (!(iReplyCode == 226 || iReplyCode == 250))
            {
                ReadReply();
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
        }


        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="strFileName">本地文件名</param>
        public void PutByGuid(string strFileName, string strGuid)
        {
            if (!bConnected)
            {
                Connect();
            }
            string str = strFileName.Substring(0, strFileName.LastIndexOf("\\"));
            string strTypeName = strFileName.Substring(strFileName.LastIndexOf("."));
            strGuid = str + "\\" + strGuid;
            System.IO.File.Copy(strFileName, strGuid);
            System.IO.File.SetAttributes(strGuid, System.IO.FileAttributes.Normal);
            Socket socketData = CreateDataSocket();
            SendCommand("STOR " + Path.GetFileName(strGuid));
            if (!(iReplyCode == 125 || iReplyCode == 150))
            {
                throw new IOException(strReply.Substring(4));
            }
            using (FileStream input = new FileStream(strGuid, FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                int iBytes = 0;
                while ((iBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    socketData.Send(buffer, iBytes, 0);
                }
                input.Close();
            }
            File.Delete(strGuid);
            if (socketData.Connected)
            {
                socketData.Close();
            }
            if (!(iReplyCode == 226 || iReplyCode == 250))
            {
                ReadReply();
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
        }
        #endregion

        #region 目录操作
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="strDirName">目录名</param>
        public void MkDir(string strDirName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("MKD " + strDirName);
            if (iReplyCode != 257)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="strDirName">目录名</param>
        public void RmDir(string strDirName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("RMD " + strDirName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        /// <summary>
        /// 改变目录
        /// </summary>
        /// <param name="strDirName">新的工作目录名</param>
        public void ChDir(string strDirName)
        {
            if (strDirName.Equals(".") || strDirName.Equals(""))
            {
                return;
            }
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("CWD " + strDirName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
            this.strRemotePath = strDirName;
        }
        #endregion

        #region 内部函数
        /// <summary>
        /// 将一行应答字符串记录在strReply和strMsg,应答码记录在iReplyCode
        /// </summary>
        private void ReadReply()
        {
            strMsg = "";
            strReply = ReadLine();
            iReplyCode = Int32.Parse(strReply.Substring(0, 3));
        }

        /// <summary>
        /// 建立进行数据连接的socket
        /// </summary>
        /// <returns>数据连接socket</returns>
        private Socket CreateDataSocket()
        {
            SendCommand("PASV");
            if (iReplyCode != 227)
            {
                throw new IOException(strReply.Substring(4));
            }
            int index1 = strReply.IndexOf('(');
            int index2 = strReply.IndexOf(')');
            string ipData = strReply.Substring(index1 + 1, index2 - index1 - 1);
            int[] parts = new int[6];
            int len = ipData.Length;
            int partCount = 0;
            string buf = "";
            for (int i = 0; i < len && partCount <= 6; i++)
            {
                char ch = Char.Parse(ipData.Substring(i, 1));
                if (Char.IsDigit(ch))
                    buf += ch;
                else if (ch != ',')
                {
                    throw new IOException("Malformed PASV strReply: " + strReply);
                }
                if (ch == ',' || i + 1 == len)
                {
                    try
                    {
                        parts[partCount++] = Int32.Parse(buf);
                        buf = "";
                    }
                    catch (Exception)
                    {
                        throw new IOException("Malformed PASV strReply: " + strReply);
                    }
                }
            }
            string ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
            int port = (parts[4] << 8) + parts[5];
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            
            try
            {
                s.Connect(ep);
            }
            catch (Exception)
            {
                throw new IOException("无法连接ftp服务器");
            }
            return s;
        }

        /// <summary>
        /// 关闭socket连接(用于登录以前)
        /// </summary>
        private void CloseSocketConnect()
        {
            lock (obj)
            {
                if (socketControl != null)
                {
                    socketControl.Close();
                    socketControl = null;
                }
                bConnected = false;
            }
        }

        /// <summary>
        /// 读取Socket返回的所有字符串
        /// </summary>
        /// <returns>包含应答码的字符串行</returns>
        private string ReadLine()
        {
            lock (obj)
            {
                while (true)
                {
                    //无限阻塞设置超时使用
                    socketControl.ReceiveTimeout = 30000;
                    socketControl.Blocking = true;
                    int iBytes = socketControl.Receive(buffer, buffer.Length, 0);
                    strMsg += ASCII.GetString(buffer, 0, iBytes);
                   // NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "ReceiveMsgsocketControl:" + strMsg + "iBytes" + iBytes.ToString() + "buffer.Length" + buffer.Length, "上传日志");
                    if (iBytes < buffer.Length)
                    {
                        break;
                    }
                }
            }
            char[] seperator = { '\n' };
            string[] mess = strMsg.Split(seperator);
           // NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "ReceiveMsgsocketControl:" + mess, "上传日志");
            if (strMsg.Length > 2)
            {
                strMsg = mess[mess.Length - 2];
            }
            else
            {
                strMsg = mess[0];
            }
          //  NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "strMsg:" + strMsg, "上传日志");
            if (!strMsg.Substring(3, 1).Equals(" ")) //返回字符串正确的是以应答码(如220开头,后面接一空格,再接问候字符串)
            {
                return ReadLine();
            }
            return strMsg;
        }

        /// <summary>
        /// 发送命令并获取应答码和最后一行应答字符串
        /// </summary>
        /// <param name="strCommand">命令</param>
        public void SendCommand(String strCommand)
        {
            lock (obj)
            {
                Byte[] cmdBytes = Encoding.ASCII.GetBytes((strCommand + "\r\n").ToCharArray());
                socketControl.Send(cmdBytes, cmdBytes.Length, 0);
                Thread.Sleep(500);
                ReadReply();
            }
        }
        #endregion
    }
}