using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

using System.Threading;
using System.Xml;
using System.Configuration;
using ASPNetPortal;
using System.Data.SqlClient;
using System.Data;

namespace Alog_WSKJSD
{
    public class ClsLog
    {
        static string LogPath = ClsLog.GetAppSettings("LogPath");
        /// <summary>
        /// 生成XML文件编码格式
        /// </summary>
        private static string _encodeType;
        public static string EncodeType
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(_encodeType))
                    {
                        _encodeType = ClsLog.GetAppSettings("EncodeType");
                    }
                }
                catch (Exception ex)
                {
                }
                return _encodeType;
            }
        }
        #region  写日志
        public static void AppendDbLog(string logText, string logType)
        {
            var sps = new SqlParameter[2];
            sps[0] = new SqlParameter("@LogType", SqlDbType.VarChar, 50);
            sps[1] = new SqlParameter("@LogStr", SqlDbType.NVarChar, Int32.MaxValue);
            sps[0].Value = logType;
            sps[1].Value = logText;
            DBAccess.ExecuteNonQuerySql("exec AddSysMessageLog @LogType,@LogStr", sps, 7);

        }

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
            System.IO.StreamWriter SW;
            if (EncodeType == "1")
            {
                SW = new System.IO.StreamWriter(Path + XMLFile, true, System.Text.Encoding.GetEncoding("GB2312"));
            }
            else
            {
                SW = System.IO.File.AppendText(Path + XMLFile);
            }
            using (SW)
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
            return ConfigurationManager.AppSettings[AppKey].ToString();
        }

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID)
        {
            int apiLogId = 0;
            string SELECT_SQL = "select Md5Str from ApiLog where Md5Str = '" + Md5Str + "'";
            if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            {
                return apiLogId;
            }
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
        public int ApiCybLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID)
        {
            int apiLogId = 0;
            string SELECT_SQL = "select Md5Str from ApiLog where Md5Str = '" + Md5Str + "'";
            if (DBAccess.DB_GetObj(SELECT_SQL, 1) != null)
            {
                return apiLogId;
            }
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
        /// 插入api操作结果日志到数据库
        /// </summary>
        public void ApiResultLog(int apiLogId, string message, string actionResult, int DSID)
        {
            string INSERT_SQL = "INSERT INTO [ApiResultLog]([Id],[message],[actionResult]) VALUES ";
            INSERT_SQL += "(" + apiLogId + ",'" + message + "'," + actionResult + ")";
            DBAccess.ExeuteSQL(INSERT_SQL, DSID);
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

}
