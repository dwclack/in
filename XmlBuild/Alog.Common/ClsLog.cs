using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Configuration;
using ASPNetPortal.DB;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Web;

namespace Alog.Common
{
    public class ClsLog
    {
        static string sysPath = HttpContext.Current != null ? HttpContext.Current.Server.MapPath("~") : System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string sysLogPath = sysPath + "\\log\\";
        static string getLogPath = string.IsNullOrEmpty(ClsLog.GetAppSettings("LogPath")) ? sysLogPath : ClsLog.GetAppSettings("LogPath");
        static string LogPath = getLogPath.Contains(":") ? getLogPath : sysPath + "\\" + getLogPath;

        public static string ProjectName = ClsLog.GetAppSettings("ProjectName");

        #region  写日志

        public static void AppendDbLog(string logText, string logType)
        {
            try
            {
                if (string.IsNullOrEmpty(ClsLog.GetAppSettings("LogDbConnection").Trim()))
                {
                    AppendLog(logText, logType);
                    return;
                }

                if (@"读取xml日志写入t_CP_Order日志绑圆通运单仓易宝生成电子面单(EMS)发送电子运单信息(EMS)浙大中控创建订单仓易JsonToDS
                宝生成电子面单(SF)服务日志errlog绑中通运单推送订单给中通国际下发天猫数据到仓易宝ImportFileInsertDataSetInsertDataTable
                入库订单通知接口发货订单通知接口下发打包指令商品同步入库订单取消接口发货订单取消接口WMS_SUBSCRIPTION_NOTIFY下发清关通
                过出库指令商家订购仓库创建货主errlog  WMS_CONSIGN_ORDER_CONFIRMcybgatewayaspxKJSDLogcybKJSDLogCancalgatewayaspxInsertDataSetInsertDataTableImportFilehwCybWeightHandlerSxmlLogHandlerKJSDLogcyb_error"
                .Contains(logType))
                {
                    if (!string.IsNullOrWhiteSpace(GetAppSettings("LogDbConnection")))
                    {
                        var sps = new SqlParameter[3];
                        sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                        sps[1] = new SqlParameter("@LogMessage", SqlDbType.NVarChar, Int32.MaxValue);
                        sps[2] = new SqlParameter("@Method", SqlDbType.NVarChar, 50);
                        sps[0].Value = logType;
                        sps[1].Value = logText;
                        sps[2].Value = string.IsNullOrWhiteSpace(ProjectName) ? "未更新项目" : ProjectName;
                        DBAccess.ExecuteNonQuerySql("exec AddErrorLog @LogType, @LogMessage, @Method", sps, "LogDbConnection");
                    }
                    else
                    {
                        var sps = new SqlParameter[2];
                        sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                        sps[1] = new SqlParameter("@LogStr", SqlDbType.NVarChar, Int32.MaxValue);
                        sps[0].Value = logType;
                        sps[1].Value = logText;

                        Task.Factory.StartNew(
                            () =>
                                DBAccess.ExecuteNonQuerySql("exec AddSysMessageLog @LogType,@LogStr", sps, 7));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(GetAppSettings("LogDbConnection")))
                    {
                        var sps = new SqlParameter[2];
                        sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                        sps[1] = new SqlParameter("@LogStr", SqlDbType.NVarChar, Int32.MaxValue);
                        sps[0].Value = logType;
                        sps[1].Value = logText;
                        Task.Factory.StartNew(
                            () => DBAccess.ExecuteNonQuerySql("exec AddSysLog @LogType,@LogStr", sps, "LogDbConnection"));
                    }
                    else
                    {
                        var sps = new SqlParameter[2];
                        sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                        sps[1] = new SqlParameter("@LogStr", SqlDbType.NVarChar, Int32.MaxValue);
                        sps[0].Value = logType;
                        sps[1].Value = logText;

                        Task.Factory.StartNew(
                            () =>
                                DBAccess.ExecuteNonQuerySql("exec AddSysMessageLog @LogType,@LogStr", sps, 7));
                    }

                }
            }
            catch (Exception ex)
            {
                AppendLog(ex.Message, "写日志出错：" + logType);
                AppendLog(logText, logType);
            }



        }

        public static void AppendDbLog(string logText, string logType, ErrorLevel level, string method)
        {
            try
            {
                if (string.IsNullOrEmpty(ClsLog.GetAppSettings("LogDbConnection").Trim()))
                {
                    AppendLog(logText, logType);
                    return;
                }

                if (level == ErrorLevel.Error)
                {
                    if (string.IsNullOrWhiteSpace(method))
                        method = ProjectName;
                    var sps = new SqlParameter[3];
                    sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                    sps[1] = new SqlParameter("@LogMessage", SqlDbType.NVarChar, Int32.MaxValue);
                    sps[2] = new SqlParameter("@Method", SqlDbType.NVarChar, 50);
                    sps[0].Value = logType;
                    sps[1].Value = logText;
                    sps[2].Value = method;
                    DBAccess.ExecuteNonQuerySql("exec AddErrorLog @LogType, @LogMessage, @Method", sps, "LogDbConnection");
                }
                else
                {
                    if (level == ErrorLevel.Warning)
                    {
                        if (string.IsNullOrWhiteSpace(method))
                            method = ProjectName;
                        var sps = new SqlParameter[3];
                        sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                        sps[1] = new SqlParameter("@LogMessage", SqlDbType.NVarChar, Int32.MaxValue);
                        sps[2] = new SqlParameter("@Method", SqlDbType.NVarChar, 50);
                        sps[0].Value = logType;
                        sps[1].Value = logText;
                        sps[2].Value = method;
                        Task.Factory.StartNew(
                            () => DBAccess.ExecuteNonQuerySql("exec AddWarningLog @LogType, @LogMessage, @Method", sps, "LogDbConnection"));
                    }
                    else
                    {
                        var sps = new SqlParameter[2];
                        sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
                        sps[1] = new SqlParameter("@LogStr", SqlDbType.NVarChar, Int32.MaxValue);
                        sps[0].Value = logType;
                        sps[1].Value = logText;
                        Task.Factory.StartNew(
                            () => DBAccess.ExecuteNonQuerySql("exec AddSysLog @LogType,@LogStr", sps, "LogDbConnection"));
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog(ex.Message, "写日志出错：" + logType);
                AppendLog(logText, logType);
            }
        }

        static public void AppendLog(string Line, string ParamType)
        {
            string strDirectory = LogPath + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
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

        private static object obj = new object();
        static public void WriteLog(string Log, string ParamType)
        {
            string strDirectory = LogPath + @"\" + DateTime.Now.ToString("yyyyMMdd") + @"\";
            if (!System.IO.Directory.Exists(strDirectory))
            {
                System.IO.Directory.CreateDirectory(strDirectory);
            }

            lock (obj)
            {
                using (System.IO.StreamWriter SW = System.IO.File.AppendText(strDirectory + ParamType + ".txt"))
                {
                    SW.WriteLine(Log);
                    SW.Close();
                }
            }

        }

        public static void WriteLog(DateTime time, string log, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[")
                .Append(time)
                .Append("]")
                .Append(" ")
                .Append(log);
            WriteLog(sb.ToString(), fileName);
        }

        public static void WriteLog(DateTime time, Exception ex, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[")
                .Append(time)
                .Append("]")
                .Append("\r\n");
            sb.Append("Message:" + ex.Message + "\r\n");
            sb.Append("StackTrace:" + ex.StackTrace);
            WriteLog(sb.ToString(), fileName);
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
            using (System.IO.StreamWriter SW = System.IO.File.AppendText(Path + XMLFile))
            {
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
            try
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[AppKey]))
                {
                    return ConfigurationManager.AppSettings[AppKey].ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }

        }

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID)
        {
            int apiLogId = 0;
            //string SELECT_SQL = "select Md5Str from ApiLog where Md5Str = '" + Md5Str + "'";
            //if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            //{
            //    return apiLogId;
            //}
            string INSERT_SQL = "INSERT INTO [ApiLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            SqlParameter[] paras = new SqlParameter[9];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            if (dt != null && dt.Rows.Count > 0)
            {
                apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            }
            return apiLogId;
        }

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, out bool Logstatus, out string InGUID)
        {
            int apiLogId = 0;
            Logstatus = false;
            InGUID = "";
            //string SELECT_SQL = "select [guid],[status],[id] from [dbo].ApiLog where Md5Str = '" + Md5Str + "' and status = 1";
            //SqlDataReader sdr = DBAccess.GetDataReader(SELECT_SQL, 1);
            //while (sdr.Read())
            //{
            //    apiLogId = int.Parse(sdr["id"].ToString());
            //    Logstatus = bool.Parse(sdr["status"].ToString());
            //    InGUID = sdr["guid"].ToString();
            //}
            //sdr.Close();
            //if (Logstatus)
            //{
            //    return apiLogId;
            //}
            InGUID = guid;
            string INSERT_SQL = "INSERT INTO [dbo].[ApiLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            SqlParameter[] paras = new SqlParameter[9];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            if (dt != null && dt.Rows.Count > 0)
            {
                apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            }
            return apiLogId;
        }

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiCybLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID)
        {
            int apiLogId = 0;
            //string SELECT_SQL = "select Md5Str from ApiLog where Md5Str = '" + Md5Str + "'";
            //if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            //{
            //    return apiLogId;
            //}
            string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            SqlParameter[] paras = new SqlParameter[9];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            if (dt != null && dt.Rows.Count > 0)
            {
                apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            }
            return apiLogId;
        }

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiCybLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb, int Readstatus)
        {
            int apiLogId = 0;

            string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb,Readstatus) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb,@Readstatus)";

            SqlParameter[] paras = new SqlParameter[11];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;
            paras[10] = new SqlParameter("@Readstatus", SqlDbType.Int);
            paras[10].Value = Readstatus;

            apiLogId = DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);

            return apiLogId;
        }

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiCybLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb)
        {
            int apiLogId = 0;
            string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb)";

            SqlParameter[] paras = new SqlParameter[10];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;

            //DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            //}
            return apiLogId;
        }

        public void ApiMngLog(string methodType, string guid, string appkey, string ipAddress, int DSID)
        {
            string insert_Sql = "INSERT INTO [ApiMngShotLog]([methodType],[guid],[appkey],[ip])VALUES(@methodType, @guid, @appkey, @ip)";
            SqlParameter[] paras = new SqlParameter[4];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar) { Value = methodType };
            paras[1] = new SqlParameter("@guid", SqlDbType.NVarChar) { Value = guid };
            paras[2] = new SqlParameter("@appkey", SqlDbType.NVarChar) { Value = appkey };
            paras[3] = new SqlParameter("@ip", SqlDbType.NVarChar) { Value = ipAddress };

            DBAccess.ExecuteNonQuerySql(insert_Sql, paras, DSID);
        }

        public int ApiMngLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb)
        {
            int apiLogId = 0;
            string INSERT_SQL = "INSERT INTO [apiMnglog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb)";

            SqlParameter[] paras = new SqlParameter[10];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;

            //DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            //}
            return apiLogId;
        }


        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiZJLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb)
        {
            int apiLogId = 0;
            //string SELECT_SQL = "select msg_id from ApiLog where methodType = '" + methodType + "' and msg_id ='" + msg_id + "'";
            //if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            //{
            //    return apiLogId;
            //}
            //string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            //INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            string INSERT_SQL = "INSERT INTO [ApiZJLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb)";
            SqlParameter[] paras = new SqlParameter[10];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;
            //DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            //}
            return apiLogId;
        }
        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiHZZCLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb)
        {
            int apiLogId = 0;
            //string SELECT_SQL = "select msg_id from ApiLog where methodType = '" + methodType + "' and msg_id ='" + msg_id + "'";
            //if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            //{
            //    return apiLogId;
            //}
            //string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            //INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            string INSERT_SQL = "INSERT INTO [ApiHZZCLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb)";

            SqlParameter[] paras = new SqlParameter[10];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;
            //DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            //}
            return apiLogId;
        }


        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiCYBRECLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb)
        {
            int apiLogId = 0;
            //string SELECT_SQL = "select msg_id from ApiCYBRECLog where methodType = '" + methodType + "' and Md5Str ='" + Md5Str + "'";
            //if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            //{
            //    return apiLogId;
            //}
            int Readstatus = 0;
            //if (methodType == "WMS_ORDER_STATUS_UPLOAD" && appKey != "STORE_XGSJ" && appKey != "WYC" && appKey != "HTH_ZZC" && appKey != "HTH_ZZC")
            //{
            //    Readstatus = -2;
            //}
            //string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            //INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            string INSERT_SQL = "INSERT INTO [ApiCYBRECLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb,Readstatus) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb,@Readstatus)";

            SqlParameter[] paras = new SqlParameter[11];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;
            paras[10] = new SqlParameter("@Readstatus", SqlDbType.Int);
            paras[10].Value = Readstatus;
            //DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            //}
            return apiLogId;
        }

        public int ApiCYBRECLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, int SendCyb, int Readstatus)
        {
            int apiLogId = 0;
            //string SELECT_SQL = "select top 1 msg_id from ApiCYBRECLog where methodType = '" + methodType + "' and Md5Str ='" + Md5Str + "'  order  by id desc ";
            //if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            //{
            //    return apiLogId;
            //}
            // int Readstatus = 0;

            //if (methodType == "WMS_ORDER_STATUS_UPLOAD" && appKey != "STORE_XGSJ" && appKey != "WYC" && appKey != "HTH_ZZC" && appKey != "HTH_ZZC")
            //{
            //    Readstatus = -2;
            //}
            //string INSERT_SQL = "INSERT INTO [ApiCYBLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id) VALUES ";
            //INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id);select SCOPE_IDENTITY() as apiLogId;";
            string INSERT_SQL = "INSERT INTO [ApiCYBRECLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id, SendCyb,Readstatus) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id, @sendCyb,@Readstatus)";

            SqlParameter[] paras = new SqlParameter[11];
            paras[0] = new SqlParameter("@methodType", SqlDbType.NVarChar);
            paras[0].Value = methodType;
            paras[1] = new SqlParameter("@postData", SqlDbType.NVarChar, -1);
            paras[1].Value = postData;
            paras[2] = new SqlParameter("@guid", SqlDbType.NVarChar);
            paras[2].Value = guid;
            paras[3] = new SqlParameter("@appKey", SqlDbType.NVarChar);
            paras[3].Value = appKey;
            paras[4] = new SqlParameter("@status", SqlDbType.NVarChar);
            paras[4].Value = status;
            paras[5] = new SqlParameter("@data_digest", SqlDbType.NVarChar);
            paras[5].Value = data_digest;
            paras[6] = new SqlParameter("@datatype", SqlDbType.NVarChar);
            paras[6].Value = datatype;
            paras[7] = new SqlParameter("@Md5Str", SqlDbType.NVarChar);
            paras[7].Value = Md5Str;
            paras[8] = new SqlParameter("@msg_id", SqlDbType.NVarChar);
            paras[8].Value = msg_id;
            paras[9] = new SqlParameter("@sendCyb", SqlDbType.Int);
            paras[9].Value = SendCyb;
            paras[10] = new SqlParameter("@Readstatus", SqlDbType.Int);
            paras[10].Value = Readstatus;
            //DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            DBAccess.ExecuteNonQuerySql(INSERT_SQL, paras, DSID);
            //if (dt != null && dt.Rows.Count > 0)
            //{
            //    apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            //}
            return apiLogId;
        }

        /// <summary>
        /// 插入api操作结果日志到数据库
        /// </summary>
        public void ApiResultLog(int apiLogId, string message, string actionResult, int DSID)
        {
            string INSERT_SQL = "INSERT INTO [ApiResultLog]([Id],[message],[actionResult]) VALUES ";
            INSERT_SQL += "(" + apiLogId + ",'" + message + "'," + actionResult + ")";
            //DBAccess.ExeuteSQL(INSERT_SQL, DSID);

            if (string.IsNullOrWhiteSpace(GetAppSettings("LogDbConnection")))
            {
                Task.Factory.StartNew(() => DBAccess.ExeuteSQL(INSERT_SQL, DSID));
            }
            else
            {
                Task.Factory.StartNew(
                            () => DBAccess.ExecuteNonQuerySql(INSERT_SQL, null, "LogDbConnection"));
            }

        }
        #region 插入导入日志
        /// <summary>
        /// 插入导入日志到数据库
        /// </summary>
        /// <param name="NewGuid"></param>
        /// <param name="SUR_TABLE_MasterKey"></param>
        /// <param name="TABLE_NAME"></param>
        /// <param name="Log_Type"></param>
        /// <param name="Log_Str"></param>
        /// <param name="DSID"></param>
        public void InsertImportLog(string NewGuid, string SUR_TABLE_MasterKey, string TABLE_NAME, int Log_Type, string Log_Str, int DSID)
        {
            string INSERT_SQL = "INSERT INTO [Apo_log]([GUIID],[TABLE_NAME],[TABLE_MasterKey],[UserName],[Log_times],[Log_Type],[Log_Str]) VALUES ";
            INSERT_SQL += "('" + NewGuid + "','" + TABLE_NAME + "','" + SUR_TABLE_MasterKey + "','001',getdate()," + Log_Type.ToString() + ",'" + Log_Str.Replace("'", "＇") + "')";
            DBAccess.ExeuteSQL(INSERT_SQL, DSID);
        }
        #endregion
    }

    public enum ErrorLevel
    {
        Info = 0,

        Warning,

        Error
    }

}
