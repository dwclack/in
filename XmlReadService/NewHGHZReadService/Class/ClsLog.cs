using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

using System.Threading;
using System.Xml;
using System.Configuration;

namespace Alog_WSKJSD
{
    public class ClsLog
    {
        static string LogPath = ClsLog.GetAppSettings("LogPath");
        #region  写日志
        static public void AppendLog(string Line, string ParamType)
        {
            string strDirectory = LogPath+@"\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
            if (!System.IO.Directory.Exists(strDirectory))
            {
                System.IO.Directory.CreateDirectory(strDirectory);
            }
            if (System.IO.File.Exists(strDirectory + ParamType + ".txt"))
            {
                WriteLog(Line, ParamType);
            }

            else
            {
                CreateLog(ParamType);
                WriteLog(Line, ParamType);
            }
        }

        static public void CreateLog(string ParamType)
        {
            System.IO.StreamWriter SW;
            if (!System.IO.Directory.Exists(LogPath))
                System.IO.Directory.CreateDirectory(LogPath);
            string strDirectory = LogPath + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
            if (!System.IO.Directory.Exists(strDirectory))
            {
                System.IO.Directory.CreateDirectory(strDirectory);
            }

            SW = System.IO.File.CreateText(strDirectory + ParamType + ".txt");

            SW.WriteLine("Log created at: " +
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            SW.Close();
        }

        static public void WriteLog(string Log, string ParamType)
        {
            string strDirectory = LogPath + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
            if (!System.IO.Directory.Exists(strDirectory))
            {
                System.IO.Directory.CreateDirectory(strDirectory);
            }

            using (System.IO.StreamWriter SW = System.IO.File.AppendText(strDirectory + ParamType + ".txt"))
            {
                SW.WriteLine(Log);
                SW.Close();
            }
        }
        #endregion

        #region 写XML文件
        static public void AppendXML(string Line, string XMLFile, string Path)
        {
            if (System.IO.File.Exists(Path + XMLFile))
            {
                WriteXML(Line, XMLFile, Path);
            }

            else
            {
                CreateXML(XMLFile, Path);
                WriteXML(Line, XMLFile, Path);
            }
        }

        static public void AppendXMLCreate(string Line, string XMLFile, string Path)
        {
            if (System.IO.File.Exists(Path + XMLFile))
            {
                //delete
                DeleteFile(Path + XMLFile);
            }

            CreateXML(XMLFile, Path);
            WriteXML(Line, XMLFile, Path);
        }

        static public void CreateXML(string XMLFile, string Path)
        {
            System.IO.StreamWriter SW;
            if (!System.IO.Directory.Exists(Path))
                System.IO.Directory.CreateDirectory(Path);
            
            SW = System.IO.File.CreateText(Path + XMLFile);

            SW.Close();
        }

        static public void WriteXML(string XML, string XMLFile, string Path)
        {
           
            using ( System.IO.StreamWriter SW =
                new System.IO.StreamWriter(Path + XMLFile,true,System.Text.Encoding.GetEncoding("GB2312")))
            {
            //using (System.IO.StreamWriter SW = System.IO.File.AppendText(Path + XMLFile))
            //{
                
                SW.WriteLine(XML);
                SW.Close();
            }
        }

        #endregion

        static public void CopyFile(string XMLFile, string PathSou, string PathDes)
        {
            if (!System.IO.Directory.Exists(PathDes))
                System.IO.Directory.CreateDirectory(PathDes);
            System.IO.FileInfo f = new System.IO.FileInfo(PathSou + XMLFile);
            f.CopyTo(PathDes + XMLFile, true);
            f = null;

        }

        static public string ReplaceMessageID(string strText, string strSource, string strReplace)
        {
            strText = strText.Replace(strSource, strReplace);
            return strText;
        }

        static public void DeleteFile(string FileName)
        {
            if (System.IO.File.Exists(FileName))
                System.IO.File.Delete(FileName);
        }

        static public void MoveFile(string XMLFile, string PathSou, string PathDes)
        {
            if (!System.IO.Directory.Exists(PathDes))
                System.IO.Directory.CreateDirectory(PathDes);
            System.IO.FileInfo f = new System.IO.FileInfo(PathSou + XMLFile);
            //if desFile exist,then delete
            DeleteFile(PathDes + XMLFile);
            f.MoveTo(PathDes + XMLFile);

            f = null;
        }

        static public string GetAppSettings(string AppKey)
        {
            return ConfigurationManager.AppSettings[AppKey].ToString();
        }
    }


}
