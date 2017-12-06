using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using ASPNetPortal;
using Alog_WSKJSD;
using System.Data.SqlClient;

namespace XmlReadService
{
    public class ImportXMLData
    {
        public int ImportReadData(string filename)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "READXML日志");
                return -1;
            }
            string sql = @"insert into T_SXmlLogList([FileName],XmlContent,CreateTime)
values(@FileName,@XmlContent,getdate())";

            
            //string UpPath = ClsLog.GetAppSettings("UpPath");
            //string SearchPath = ClsLog.GetAppSettings("SearchPath");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
                string xmlContent = doc.InnerXml;
                string sfileName = Path.GetFileName(filename);
                //如果包含异常信息，则写入错误日志表里面
                if (sfileName.Contains("T") || sfileName.Contains("S"))
                {
                    //报文异常暂时不用，只用于将异常报文直接放进去
                    //if (xmlContent.Contains("<ReturnInfo>报文格式异常，无法解析。</ReturnInfo>"))
                    //{

                    //    string CopyFileName = "";
                    //    if (sfileName.Contains("0_"))
                    //    {
                    //        CopyFileName = sfileName.Replace("0_", "");
                    //    }

                    //    if (!string.IsNullOrEmpty(SearchPath.Trim()))
                    //    {
                    //        for (int i = 0; i < SearchPath.Split(';').Length; i++)
                    //        {
                    //            string tempPath = SearchPath.Split(';')[i];
                    //            ClsLog.AppendLog("CopyFileName" + tempPath, "READXML日志");
                    //            if (!string.IsNullOrEmpty(tempPath.Trim()))
                    //            {
                    //                if (File.Exists(tempPath + @"\" + CopyFileName))
                    //                {

                    //                    ClsLog.CopyFile(Path.GetFileName(CopyFileName), tempPath + @"\",
                    //                                    @"" + UpPath + @"\");
                    //                }
                    //                else
                    //                {
                    //                    //如果不存在再查找一遍子文件夹进行遍历查找
                    //                    string[] _xmlFiles = Directory.GetFiles(tempPath + @"\", "*.xml", SearchOption.AllDirectories);
                    //                    for (int j = 0; j < _xmlFiles.Length; j++)
                    //                    {
                    //                        string tempFileName = Path.GetFileName(_xmlFiles[j]);
                    //                        if (tempFileName == CopyFileName)
                    //                        {
                    //                            ClsLog.AppendLog("jinrul ", "READXML日志");
                    //                            ClsLog.CopyFile(Path.GetFileName(CopyFileName), (Path.GetDirectoryName(_xmlFiles[i])) + @"\",
                    //                                    @"" + UpPath + @"\");
                    //                            break;
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    SqlParameter[] sqlParms =
                        {
                            new SqlParameter("@FileName", sfileName),
                            new SqlParameter("@XmlContent", xmlContent),
                        };
                    DBAccess.ExecuteNonQuerySql(sql, sqlParms, "ConnectionStringData");
                    return 0;
                }
                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }

        public int CopyFileData(string filename)
        {
            if (!File.Exists(filename))
            {
                return -1;
            }
            try
            {
                string HGHZFileEx = ClsLog.GetAppSettings("HGHZFileEx");
                string HGHZNoFileEx = ClsLog.GetAppSettings("HGHZNoFileEx");
                string sfileName = Path.GetFileName(filename);
                for (int j = 0; j < HGHZNoFileEx.Split(';').Length; j++)
                {
                    string tempHGHZNoFileEx = HGHZNoFileEx.Split(';')[j];
                    if (sfileName.Contains(tempHGHZNoFileEx))
                    {
                        return -1;
                    }
                }
                for (int i = 0; i < HGHZFileEx.Split(';').Length; i++)
                {
                    string tempHGHZFileEx = HGHZFileEx.Split(';')[i];
                    if (sfileName.Contains(tempHGHZFileEx))
                    {
                        return 0;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }

        /// <summary>
        /// 复制行邮税
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public int CopyXYSFileData(string filename)
        {
            if (!File.Exists(filename))
            {
                return -1;
            }
            try
            {
                string XYSStartDate = ClsLog.GetAppSettings("XYSStartDate");
                string XYSEndDate = ClsLog.GetAppSettings("XYSEndDate");
                string sfileName = Path.GetFileName(filename);
                DateTime dt = File.GetLastWriteTime(filename);
                //只读取指定日期的行邮税回执
                if (dt >= Convert.ToDateTime(XYSStartDate) && dt <= Convert.ToDateTime(XYSEndDate))
                {
                        return 0;
                }
                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }

        public int ImportNSReadData(string filename)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "READXML日志");
                return -1;
            }
            string sql = @"insert into T_SXmlLogList([FileName],XmlContent,CreateTime)
values(@FileName,@XmlContent,getdate())";

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
                string xmlContent = doc.InnerXml;
                string sfileName = Path.GetFileName(filename);
                //如果包含异常信息，则写入错误日志表里面

                SqlParameter[] sqlParms =
                    {
                        new SqlParameter("@FileName", sfileName),
                        new SqlParameter("@XmlContent", xmlContent),
                    };
                DBAccess.ExecuteNonQuerySql(sql, sqlParms, "ConnectionStringData");
                return 0;
                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }


        public int CopyHGDDReadData(string filename)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "READXML日志");
                return -1;
            }
            string HGDDOwnerId = ClsLog.GetAppSettings("HGDDOwnerId");
            string sql = @"select OwnerId,OwnerCode from T_Owner where isnull(isdel,0)=0 and OwnerId in(" + HGDDOwnerId + ")";
            DataTable dt = DBAccess.GetDataSet(sql, "ConnectionStringData").Tables[0];
            XmlDocument doc = new XmlDocument();
            try
            {
                string HGDDPathBakFtp = ClsLog.GetAppSettings("HGDDPathBakFtp");
                doc.Load(filename);
                string xmlContent = doc.InnerXml;
                string sfileName = Path.GetFileName(filename);
                //文件按备案号移动到不同的目录下
                for (int i = 0; (i) < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    string OwnerId = row["OwnerId"].ToString();
                    string OwnerCode = row["OwnerCode"].ToString();
                    if (xmlContent.Contains("<EntRecordNo>" + OwnerCode + "</EntRecordNo>"))
                    {
                        ClsLog.CopyFile(Path.GetFileName(filename), Path.GetDirectoryName(filename) + @"\",
                                        @"" + HGDDPathBakFtp + OwnerCode + @"\");
                        return 0;
                    }
                }

                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }

        public int CopyDYCKFileData(string filename)
        {
            if (!File.Exists(filename))
            {
                return -1;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                string DYCKHZPathSearch = ClsLog.GetAppSettings("DYCKHZPathSearch");
                doc.Load(filename);
                string xmlContent = doc.InnerXml;
                string sfileName = Path.GetFileName(filename);
                for (int j = 0; j < DYCKHZPathSearch.Split(';').Length; j++)
                {
                    string tempHGHZNoFileEx = DYCKHZPathSearch.Split(';')[j];
                    if (sfileName.Contains(tempHGHZNoFileEx))
                    {
                        if (xmlContent.Contains("<Status>F</Status>"))
                        {
                            return -2;
                        }
                        return 0;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }

        public int ImportNSHZReadData(string filename)
        {
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Msg, "READXML日志");
                return -1;
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
                string xmlContent = doc.InnerXml;
                string sfileName = Path.GetFileName(filename);
                
                string NSGJHZPathSearch = ClsLog.GetAppSettings("NSGJHZPathSearch");
                for (int j = 0; j < NSGJHZPathSearch.Split(';').Length; j++)
                {
                    string tempEx = NSGJHZPathSearch.Split(';')[j];
                    //如果包含异常信息，则写入错误日志表里面
                    if (xmlContent.Contains(tempEx) && !xmlContent.Contains("<Status>20</Status>"))
                    {
                        return 0;
                    }
                    if (xmlContent.Contains(tempEx) && xmlContent.Contains("<Status>20</Status>"))
                    {
                        return -2;
                    }
                }
             
                return -1;
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "READXML日志");
                return -1;
            }
        }
    }
}
