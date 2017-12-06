using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Alog_WSKJSD;

namespace AlogFtp
{
    public class FtpUtil
    {
        //基本设置
        static private string path =  ClsLog.GetAppSettings("NSFTPAddress");    //目标路径
        static private string temppath = ClsLog.GetAppSettings("NSFTPAddress");    //目标路径
        static private string ftpip = ClsLog.GetAppSettings("NSFTPAddressIP");    //ftp IP地址
        static private string username = ClsLog.GetAppSettings("FtpUserName");   //ftp用户名
        static private string password = ClsLog.GetAppSettings("FtpPwd");   //ftp密码

        /// <summary>
        /// 设置下载目标路径
        /// </summary>
        /// <param name="value"></param>
        public static void SetDownLoadPath(string value)
        {
            path = temppath + value + "/out/";
        }

        /// <summary>
        /// 设置下载目标路径
        /// </summary>
        /// <param name="value"></param>
        public static void SetUpLoadPath(string value)
        {
            path = temppath + value + "/in/";
        }

        /// <summary>
        /// 获取FTP目标路径
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            return path;
        }
        /// <summary>
        /// 获取ftp上面的文件和文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="objPath"></param>
        /// <returns></returns>
        public static string[] GetFileList(string dir)
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest request;
            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
                request.UseBinary = true;
                request.Credentials = new NetworkCredential(username, password);//设置用户名和密码
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    //throw new Exception(line);
                    line = reader.ReadLine();
                }
                // to remove the trailing '\n'
                if (result.ToString().LastIndexOf('\n') >= 0)
                {
                    result.Remove(result.ToString().LastIndexOf('\n'), 1);
                }
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                throw new Exception("获取ftp上面的文件和文件夹：" + ex.Message);
                downloadFiles = null;
                return downloadFiles;
            }
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="LocalPath">本地目录</param>
        /// <param name="ftpPath">FTP目录</param>
        public static int DownLoadFile(string filename, string LocalPath, string ftpPath)
        {
            int result = 0;
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpPath + filename);
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.UseBinary = true;
            req.UsePassive = true;
            req.Credentials = new NetworkCredential(username, password);
            try
            {
                //当FTP有相应时
                using (FtpWebResponse res = (FtpWebResponse)req.GetResponse())
                {
                    //组合本地目录文件
                    string localfile = Path.Combine(LocalPath, filename);
                    //创建文件流
                    FileStream fs = new FileStream(localfile, FileMode.Create, FileAccess.Write);
                    //1K缓冲
                    int buffer = 1024;
                    //创建文件流字节
                    byte[] b = new byte[buffer];
                    int i = 0;
                    Stream stream = res.GetResponseStream();
                    while ((i = stream.Read(b, 0, buffer)) > 0)
                    {
                        //写入本地目录
                        fs.Write(b, 0, i);
                    }
                    result++;
                }
            }
            catch (Exception ex)
            {
                result = 0;
                throw new Exception("下载文件失败错误为" + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 下载报文回执
        /// </summary>
        /// <param name="filePath">本地路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ftpPath">FTP路径</param>
        /// <returns></returns>
        public static int Download(string filePath, string fileName, string ftpPath)
        {
            FtpWebRequest reqFTP;

            int count = 0;
            try
            {
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + fileName));

                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;

                reqFTP.UseBinary = true;

                reqFTP.Credentials = new NetworkCredential(username, password);

                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                Stream ftpStream = response.GetResponseStream();

                long cl = response.ContentLength;

                int bufferSize = 2048;

                int readCount;

                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);

                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);

                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();

                outputStream.Close();

                response.Close();
                count++;
            }
            catch (Exception ex)
            {
                count = 0;
                throw new Exception("下载文件失败错误为" + ex.Message);
            }
            return count;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="file">ip服务器下的相对路径</param>
        /// <returns>文件大小</returns>
        public static int GetFileSize(string file)
        {
            StringBuilder result = new StringBuilder();
            FtpWebRequest request;
            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(new Uri(path + file));
                request.UseBinary = true;
                request.Credentials = new NetworkCredential(username, password);//设置用户名和密码
                request.Method = WebRequestMethods.Ftp.GetFileSize;

                int dataLength = (int)request.GetResponse().ContentLength;

                return dataLength;
            }
            catch (Exception ex)
            {
                throw new Exception("获取文件大小出错：" + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="filePath">原路径（绝对路径）包括文件名</param>
        /// <param name="objPath">目标文件夹：服务器下的相对路径 不填为根目录</param>
        public static void FileUpLoad(string filePath, string objPath)
        {
            try
            {
                string url = path;

                FtpWebRequest reqFTP = null;
                //待上传的文件 （全路径）

                FileInfo fileInfo = new FileInfo(filePath);
                string remotePath = url + fileInfo.Name;
                new Uri(remotePath);
                using (FileStream fs = fileInfo.OpenRead())
                {
                    long length = fs.Length;
                    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(url + fileInfo.Name));

                    //设置连接到FTP的帐号密码
                    reqFTP.Credentials = new NetworkCredential(username, password);
                    //设置请求完成后是否保持连接
                    reqFTP.KeepAlive = false;
                    //指定执行命令
                    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                    //指定数据传输类型
                    reqFTP.UseBinary = true;

                    using (Stream stream = reqFTP.GetRequestStream())
                    {
                        //设置缓冲大小
                        int BufferLength = 5120;
                        byte[] b = new byte[BufferLength];
                        int i;
                        while ((i = fs.Read(b, 0, BufferLength)) > 0)
                        {
                            stream.Write(b, 0, i);
                        }
                        //throw new Exception("上传文件成功");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("上传文件失败错误为" + ex.Message);
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">服务器下的相对路径 包括文件名</param>
        public static void DeleteFileName(string fileName)
        {
            try
            {
                FileInfo fileInf = new FileInfo(ftpip + "" + fileName);
                string uri = path + fileName;
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // 指定数据传输类型
                reqFTP.UseBinary = true;
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                // 默认为true，连接不会被关闭
                // 在一个命令之后被执行
                reqFTP.KeepAlive = false;
                // 指定执行什么命令
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("删除文件出错：" + ex.Message);
            }
        }

        /// <summary>
        /// 新建目录 上一级必须先存在
        /// </summary>
        /// <param name="dirName">服务器下的相对路径</param>
        public static void MakeDir(string dirName)
        {
            try
            {
                string uri = path + dirName;
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // 指定数据传输类型
                reqFTP.UseBinary = true;
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("创建目录出错：" + ex.Message);
            }
        }

        /// <summary>
        /// 删除目录 上一级必须先存在
        /// </summary>
        /// <param name="dirName">服务器下的相对路径</param>
        public static void DelDir(string dirName)
        {
            try
            {
                string uri = path + dirName;
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("删除目录出错：" + ex.Message);
            }
        }

        /// <summary>
        /// 从ftp服务器上获得文件夹列表
        /// </summary>
        /// <param name="RequedstPath">服务器下的相对路径</param>
        /// <returns></returns>
        public static List<string> GetDirctory(string RequedstPath)
        {
            List<string> strs = new List<string>();
            try
            {
                string uri = path + RequedstPath;   //目标路径 path为服务器地址
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());//中文文件名

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains("<DIR>"))
                    {
                        string msg = line.Substring(line.LastIndexOf("<DIR>") + 5).Trim();
                        strs.Add(msg);
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();
                return strs;
            }
            catch (Exception ex)
            {
                throw new Exception("获取目录出错：" + ex.Message);
            }
            return strs;
        }

        /// <summary>
        /// 从ftp服务器上获得文件列表
        /// </summary>
        /// <param name="RequedstPath">服务器下的相对路径</param>
        /// <returns></returns>
        public static List<string> GetFile(string RequedstPath)
        {
            List<string> strs = new List<string>();
            try
            {
                string uri = path + RequedstPath;   //目标路径 path为服务器地址
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());//中文文件名

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.Contains("<DIR>"))
                    {
                        string msg = line.Substring(39).Trim();
                        strs.Add(msg);
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();
                return strs;
            }
            catch (Exception ex)
            {
                throw new Exception("获取文件出错：" + ex.Message);
            }
            return strs;
        }
    }
}
