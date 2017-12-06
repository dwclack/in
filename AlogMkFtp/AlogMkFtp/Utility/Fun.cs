using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Collections;

namespace AlogMkFtp.Utility
{

    public class Fun
    {
        public Fun()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public static ArrayList ReadFileByLine(string FileName)
        {
            ArrayList retList = new ArrayList();

            StreamReader objReader = new StreamReader(FileName);

            string sLine = "";

            while (!objReader.EndOfStream)
            {
                sLine = objReader.ReadLine();
                sLine = sLine.Replace("'", "’");
                retList.Add(sLine);
            }
            objReader.Close();

            return retList;
        }

        public static void WriteServerLogFile(string StrContent)
        { 
            FileStream fs;
            Byte[] info;
            if (!File.Exists(Utility.App.g_strCurrentApplicationPath + "\\服务日志.txt"))
            {
                fs = new FileStream(Utility.App.g_strCurrentApplicationPath + "\\服务日志.txt", FileMode.CreateNew);
            }
            else
            {
                fs = new FileStream(Utility.App.g_strCurrentApplicationPath + "\\服务日志.txt", FileMode.Append);
            }
            info = new UTF8Encoding(true).GetBytes(StrContent + "\r\n");
            fs.Write(info, 0, info.Length);
            fs.Close();
        }

        public static void WriteFile(string LogFileName, string StrContent)
        {
            string Errs = "";
            FileStream fs;
            Byte[] info;
            if (!ExistsDirectory(Utility.App.g_strCurrentApplicationPath + "\\log" + "\\" + DateTime.Now.ToString("yyyyMMdd"), out Errs))
            {//不存在log文件夹或者创建log文件夹失败
                if (!File.Exists(Utility.App.g_strCurrentApplicationPath + "\\" + LogFileName))
                {
                    fs = new FileStream(Utility.App.g_strCurrentApplicationPath + "\\" +DateTime.Now.ToString("yyyyMMdd")+ LogFileName, FileMode.CreateNew);
                }
                else
                {
                    fs = new FileStream(Utility.App.g_strCurrentApplicationPath + "\\" +DateTime.Now.ToString("yyyyMMdd")+ LogFileName, FileMode.Append);
                }
            }
            else
            {//存在log文件夹或者创建log文件夹成功
                if (!File.Exists(Utility.App.g_strCurrentApplicationPath + "\\log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + LogFileName))
                {
                    fs = new FileStream(Utility.App.g_strCurrentApplicationPath + "\\log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + LogFileName, FileMode.CreateNew);
                }
                else
                {
                    fs = new FileStream(Utility.App.g_strCurrentApplicationPath + "\\log\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + LogFileName, FileMode.Append);
                }
            }
            info = new UTF8Encoding(true).GetBytes(StrContent + "\r\n");
            fs.Write(info, 0, info.Length);
            fs.Close();
        }

        public static string ReadNodeOfXML(string KeyName, string NodeName)
        {
            string result = string.Empty;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                string xlmPath = Utility.App.g_strCurrentApplicationPath + "\\Alog_Config.xml";
                xmlDoc.Load(xlmPath);//读入xlm配置文档
                XmlNodeList nodeList = xmlDoc.SelectSingleNode("root").ChildNodes;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (i == 19 || i == 22)
                    {
                        ;
                    }


                    XmlElement xe = (XmlElement)nodeList[i];
                    if (xe.GetAttribute("key").ToUpper() == KeyName.ToUpper())
                    {
                        result = xe.GetAttribute(NodeName);
                        break;
                    }
                }
            }
            catch (System.NullReferenceException NullEx)
            {
                throw NullEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// 检查文件路径如果没有则创建
        /// </summary>
        /// <param name="Directorys">文件相对程序的路径</param>
        /// <param name="Err">错误信息</param>
        /// <returns>是否成功</returns>
        private static bool ExistsDirectory(string Directorys, out string Err)
        {
            Err = "";
            try
            {
                if (!System.IO.Directory.Exists(Directorys))
                    Directory.CreateDirectory(Directorys);
                return true;
            }
            catch (Exception ex)
            {
                Err = ex.Message;
                return false;
            }
        }
    }
}
