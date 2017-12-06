﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using Alog_WSKJSD;
using System.Data.SqlClient;

namespace NSDDDisServices
{
    public class ImportXMLData
    {
        public string OPNSXmlData(string filename)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "异常日志");
                return "-1";
            }
           
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
                string xmlContent = doc.InnerXml;
                
                string sfileName = Path.GetFileName(filename);
                string CbeComcodeInnerText = doc.SelectNodes("//CbeComcode")[0].InnerText.Trim(); 
                //如果包含异常信息，则写入错误日志表里面
                if (!string.IsNullOrEmpty(CbeComcodeInnerText))
                {
                    return CbeComcodeInnerText;
                }
                return "-1";
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return "-1";
            }
        }

        /// <summary>
        /// 机场国检单一窗口订单报文分文件夹
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string OPGJJCXmlData(string filename)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "异常日志");
                return "-1";
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
                string xmlContent = doc.InnerXml;

                string sfileName = Path.GetFileName(filename);
                string CbeComcodeInnerText = doc.SelectNodes("//Sender")[0].InnerText.Trim();
                //如果包含异常信息，则写入错误日志表里面
                if (!string.IsNullOrEmpty(CbeComcodeInnerText))
                {
                    return CbeComcodeInnerText;
                }
                return "-1";
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return "-1";
            }
        }


        /// <summary>
        /// 总署订单报文分文件夹
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string OPHGZSXmlData(string filename, string HGZSDirNode)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "异常日志");
                return "-1";
            }

            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager m = new XmlNamespaceManager(doc.NameTable);
            m.AddNamespace("w3org", "http://www.chinaport.gov.cn/ceb");
            try
            {
                doc.Load(filename);
                string xmlContent = doc.InnerXml;

                string sfileName = Path.GetFileName(filename);
                string CbeComcodeInnerText = doc.SelectNodes("//w3org:" + HGZSDirNode, m)[0].InnerText.Trim();
                //如果包含异常信息，则写入错误日志表里面
                if (!string.IsNullOrEmpty(CbeComcodeInnerText))
                {
                    return CbeComcodeInnerText;
                }
                return "-1";
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return "-1";
            }
        }
    }
}
