using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Web;
using System.Net;
using ASPNetPortal.DB;
namespace Alog.Common
{
    public class RepXml
    {
        //int Flag = 0;
        //StringBuilder sbResult = new StringBuilder();
        //public static bool[] AllOK = new bool[300];
        public static Dictionary<string, bool> AllOK = new Dictionary<string, bool>();
        public static readonly object padlock = new object();

        #region 缓存模板设置
        /// <summary>
        /// 运行的版本号
        /// </summary>
        private static string _repVersion;
        public static string repVersion
        {
            get
            {
                if (_repVersion == null)
                {
                    _repVersion = ClsLog.GetAppSettings("RepVersion");
                }
                return _repVersion;
            }
        }
        public static int RepXml_dtRepXmlSet_Value = -39178;
        /// <summary>
        /// XML报文模板设置
        /// </summary>
        private static DataTable _dtRepXmlSet;
        public static DataTable dtRepXmlSet
        {
            get
            {
                if (_dtRepXmlSet == null)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "更新缓存，标识值为：" + RepXml_dtRepXmlSet_Value, "dtRepXmlSet缓存");
                    string sql = @"SELECT  [AutoId]
                                  ,ISNULL([parentid],0) AS parentid
                                  ,[LabelCode]
                                  ,[DSID]
                                  ,[RepVersion]
                                  ,[RepTitle]
                                  ,[RepFileName]
                                  ,[MasterSQL]
                                  ,[QuerySQL]
                                  ,[SlaveSQL]
                                  ,[XMLContent]
                                  ,[CreatedByUser]
                                  ,[CreatedDate]
                                  ,[Interval]
                                  ,[BatchSql]
                                  ,[PrimaryKey]
                                  ,[EncodedColumns_M]
                                  ,[EncodedColumns_Q]
                                  ,[EncodedColumns_S]
                                  ,[CompleteSQL],ErrorSQL from RepXmlSet where isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and  RepVersion=" + repVersion;
                                 return _dtRepXmlSet = DBAccess.GetDataTable(sql, 7); ;
                }
                return _dtRepXmlSet;
            }
            set
            {
                _dtRepXmlSet = value;
            }
        }
        private static DateTime _RijnSetTime;
        public static DateTime RijnSetTime
        {
            get
            {
                if (_RijnSetTime == null)
                {
                    _RijnSetTime = Convert.ToDateTime("2016-1-1 00:00:00");
                }
                return _RijnSetTime;
            }
            set { _RijnSetTime = value; }
        }
        private static DataTable _rijnSet;

        public static DataTable RijnSet
        {
            get
            {
                DateTime dt1 = DateTime.Now;
                DateTime dt2 = RijnSetTime;
                TimeSpan ts = dt1 - dt2;
                if (_rijnSet == null || ts.Minutes>15)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "更新缓存，标识值为：" + RepXml_dtRepXmlSet_Value, "rijnSet缓存");

                    string sql = @"SELECT [VersionId]
                                  ,[Key]
                                  ,[IV]
                                  ,[IsValid]
                                  ,[createtime]
                              FROM [dbo].[RijnVersion]  
                              order by VersionId";
                    RijnSetTime = DateTime.Now;
                    return _rijnSet = DBAccess.GetDataTable(sql, "connectionstring"); ;

                }
                return _rijnSet;
            }
            set { _rijnSet = value; }
        }

        private static string _versionId;

        public static string VersionId
        {
            get
            {
                if (string.IsNullOrEmpty(_versionId))
                {
                    if (RijnSet == null)
                        return _versionId = "";

                    DataRow[] drs = RijnSet.Select("isnull(IsValid,0) = 1", "VersionId desc");
                    if(drs.Length == 0)
                        return _versionId = "";

                    return _versionId = drs[0]["VersionId"].ToString();
                }

                return _versionId;
            }
        }


        private static string _aesKey;
        private static string _aesVector;
        

        public static string AesKey
        {
            get
            {
                if (string.IsNullOrEmpty(_aesKey))
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "更新aesKey缓存，标识值为：" + RepXml_dtRepXmlSet_Value, "AesKey缓存");

                    DataRow[] drs = RijnSet.Select("VersionId = " + _versionId);
                    if (drs.Length == 0)
                        return _aesKey = "";

                    return _aesKey = drs[0]["Key"].ToString();
                }

                return _aesKey;
            }
        }

        public static string AesVector
        {
            get
            {
                if (string.IsNullOrEmpty(_aesVector))
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        + "更新aesVector缓存，标识值为：" + RepXml_dtRepXmlSet_Value, "AesVector缓存");

                    DataRow[] drs = RijnSet.Select("VersionId = " + _versionId);
                    if (drs.Length == 0)
                        return _aesVector = "";

                    return _aesVector = drs[0]["IV"].ToString();
                }

                return _aesVector;
            }
        }


        #endregion

        /// <summary>
        /// 插入api操作日志到数据库,用id关联api操作结果日志表
        /// </summary>
        public int ApiLog(string methodType, string postData, string msg_id, string guid, string appKey, string status, string data_digest, string datatype, string Md5Str, int DSID, string SendReturnMessage = "")
        {
            int apiLogId = 0;
            string INSERT_SQL = "INSERT INTO [ApiLog]([methodType],[postData],[guid],[appKey],[status],data_digest,datatype,Md5Str,msg_id,SendReturnMessage) VALUES ";
            INSERT_SQL += "(@methodType,@postData,@guid,@appKey,@status,@data_digest,@datatype,@Md5Str,@msg_id,@SendReturnMessage);select SCOPE_IDENTITY() as apiLogId;";
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
            paras[9] = new SqlParameter("@SendReturnMessage", SqlDbType.NVarChar);
            paras[9].Value = SendReturnMessage;
            DataTable dt = DBAccess.GetDataTableByParam(INSERT_SQL, paras, DSID);
            if (dt != null && dt.Rows.Count > 0)
            {
                apiLogId = Convert.ToInt32(dt.Rows[0]["apiLogId"].ToString());
            }
            return apiLogId;
        }

        /// <summary>
        /// 从指定URL回传数据并返回结果
        /// </summary>
        /// <param name="url">WLB对应的URL</param>
        /// <param name="postData">回传的参数</param>
        /// <param name="encodeType">编码</param>
        /// <param name="err">返回的错误</param>
        /// <returns>结果字串</returns>
        public string GetPage(string url, string postData, string encodeType, out string err)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            Encoding encoding = Encoding.GetEncoding(encodeType);

            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(url) as HttpWebRequest;

                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();

                outstream.Write(data, 0, data.Length);
                outstream.Close();

                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;

                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();

                sr = new StreamReader(instream, encoding);

                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                err = string.Empty;

                return content;

            }
            catch (Exception ex)
            {
                err = ex.Message;
                return string.Empty;
            }
        }


        /// <summary>
        /// 递归读取标替换签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        public void InitLabelXML(string parentid)
        {
            #region
            if (dtRepXmlSet.Select("parentid=" + parentid).Count() > 0)
            {
                DataRow[] drs = dtRepXmlSet.Select("parentid=" + parentid);
                for (int i = 0; i < drs.Count(); i++)
                {
                    string temphtml = "";
                    string LabelCode = "";
                    string MasterSQL = "";
                    string SlaveSQL = "";
                    string EncodedColumns_M = "";
                    string EncodedColumns_Q = "";
                    int ToDSID = 0;
                    LabelCode = drs[i]["LabelCode"].ToString();
                    string str = @"(" + LabelCode + @"(\((\w|,)*\)))|(" + LabelCode + ")";//表达
                    MatchCollection macths = Regex.Matches(ParentTempLabelHtml, str, RegexOptions.RightToLeft);
                    foreach (Match macth in macths)
                    {
                        string temptag = "";
                        temptag = macth.ToString();//把匹配的项付给变量
                        string[] Parameters = null;
                        if (temptag.IndexOf(@"\(") > 0)
                        {
                            Parameters = temptag.Split('(')[1].Split(')')[0].Split(',');
                        }
                        MasterSQL = drs[i]["MasterSQL"].ToString();
                        MasterSQL = ReplaceParameterValue(MasterSQL, Parameters);
                        SlaveSQL = drs[i]["QuerySQL"].ToString();
                        EncodedColumns_M = drs[i]["EncodedColumns_M"].ToString();
                        EncodedColumns_Q = drs[i]["EncodedColumns_Q"].ToString();
                        ToDSID = int.Parse(drs[i]["DSID"].ToString());
                        DataTable dtMaster = new DataTable();
                        if (!string.IsNullOrEmpty(MasterSQL))
                        {
                            MasterSQL = MasterSQL.Replace("~", "'");

                            dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);
                            Aes256Util.DataTableAes256Decode(dtMaster, EncodedColumns_M);
                            temphtml = ReplaceMasterValue(temphtml, dtMaster.Rows[0]); ;
                        }
                        if (!string.IsNullOrEmpty(SlaveSQL))
                        {
                            SlaveSQL = SlaveSQL.Replace("~", "'");
                            SlaveSQL = ReplaceParameterValue(SlaveSQL, Parameters);
                            if (dtMaster.Rows.Count > 0)
                            {
                                SlaveSQL = ReplaceMasterValue(SlaveSQL, dtMaster.Rows[0]); ;
                            }
                            DataTable dtSlave = DBAccess.GetDataTable(SlaveSQL, ToDSID);
                            Aes256Util.DataTableAes256Decode(dtSlave, EncodedColumns_Q);
                            temphtml = ReplaceSlaveValue(temphtml, dtSlave);
                        }
                        ParentTempLabelHtml = ParentTempLabelHtml.Replace(temptag, temphtml);

                    }

                    parentid = drs[i]["AutoId"].ToString();
                    InitLabelXML(parentid);

                }
            }
            #endregion
        }
        /// <summary>
        /// 替换参数
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public string ReplaceParameterValue(string SQL, string[] Parameters)
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                SQL = SQL.Replace("{" + (i + 1).ToString() + "}", Parameters[i]);
            }
            return SQL;
        }

        public string ReplaceParentSlaveValue(string temphtml, DataRow Slavedr)
        {

            for (int i = 0; i < Slavedr.Table.Columns.Count; i++)
            {
                string tempStr = XmlTextEncoder.Encode(Slavedr[Slavedr.Table.Columns[i].ColumnName].ToString());
                temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]PARENT[}].[{][*]#" + Slavedr.Table.Columns[i].ColumnName + "}", tempStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]PARENT[}].[{]#" + Slavedr.Table.Columns[i].ColumnName + "}", tempStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return temphtml;
        }


        /// <summary>
        /// 替换子表
        /// </summary>
        /// <param name="temphtml"></param>
        /// <param name="Slavedt"></param>
        /// <returns></returns>
        public string ReplaceSlaveValue(string temphtml, DataTable Slavedt)
        {
            if (temphtml.IndexOf("<loop>") > -1 && temphtml.IndexOf("</loop>") > -1)
            {

                string OutHTML = "";
                string[] Themlstr = System.Text.RegularExpressions.Regex.Split(temphtml, "<loop>");
                string temphtml_1 = Themlstr[0];
                OutHTML += temphtml_1;
                //string temphtml_3 = System.Text.RegularExpressions.Regex.Split(Themlstr[Themlstr.Length-1], "</loop>")[1];
                for (int j = 1; j < Themlstr.Length; j++)
                {
                    string temphtml_2_temp = System.Text.RegularExpressions.Regex.Split(Themlstr[j], "</loop>")[0];
                    string loopstrtempEnd = System.Text.RegularExpressions.Regex.Split(Themlstr[j], "</loop>")[1];

                    if (Slavedt.Rows.Count > 0)
                    {
                        for (int k = 0; k < Slavedt.Rows.Count; k++)
                        {
                            string looptempstr = temphtml_2_temp;
                            for (int i = 0; i < Slavedt.Columns.Count; i++)
                            {
                                string tempStr = XmlTextEncoder.Encode(Slavedt.Rows[k][Slavedt.Columns[i].ColumnName].ToString());
                                looptempstr = System.Text.RegularExpressions.Regex.Replace(looptempstr, "[{][*]#" + Slavedt.Columns[i].ColumnName + "}", tempStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                looptempstr = System.Text.RegularExpressions.Regex.Replace(looptempstr, "[{]#" + Slavedt.Columns[i].ColumnName + "}", tempStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            }

                            OutHTML += looptempstr;
                        }


                    }

                    if (OutHTML.Length > 0)
                    {
                        if (OutHTML.LastIndexOf(',') == OutHTML.Length - 1)
                        {
                            OutHTML = OutHTML.Substring(0, OutHTML.Length - 1);
                        }
                    }
                    OutHTML += loopstrtempEnd;

                }
                OutHTML = System.Text.RegularExpressions.Regex.Replace(OutHTML, @"{\#\w+}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                temphtml = OutHTML;
            }
            else
            {//如果没有LOOP,就直接只返回一行数据

                if (Slavedt.Rows.Count > 0)
                {

                    if (Slavedt.Rows.Count > 0)
                    {

                        for (int i = 0; i < Slavedt.Columns.Count; i++)
                        {
                            string tempStr = XmlTextEncoder.Encode(Slavedt.Rows[0][Slavedt.Columns[i].ColumnName].ToString());
                            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{][*]#" + Slavedt.Columns[i].ColumnName + "}", tempStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]#" + Slavedt.Columns[i].ColumnName + "}", tempStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        }
                    }
                    else
                    {

                        for (int i = 0; i < Slavedt.Columns.Count; i++)
                        {
                            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{][*]#" + Slavedt.Columns[i].ColumnName + "}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]#" + Slavedt.Columns[i].ColumnName + "}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        }
                    }
                    temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, @"\{\#\w+\}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                }

            }
            return temphtml;
        }

        /// <summary>
        /// 替换主表
        /// </summary>
        /// <param name="temphtml"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public string ReplaceParentMasterValue(string temphtml, DataRow dr)
        {
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string tempStr = XmlTextEncoder.Encode(dr[dr.Table.Columns[i].ColumnName].ToString());
                temphtml = temphtml.Replace("[{]PARENT[}].{*$" + dr.Table.Columns[i].ColumnName + "$}", tempStr);
                temphtml = temphtml.Replace("[{]PARENT[}].{$" + dr.Table.Columns[i].ColumnName + "$}", tempStr);
            }

            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, @"[{]PARENT[}].\{\$\w+\$\}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return temphtml;
        }
        /// <summary>
        /// 替换主表
        /// </summary>
        /// <param name="temphtml"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public string ReplaceMasterValue(string temphtml, DataRow dr)
        {
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string tempStr = XmlTextEncoder.Encode(dr[dr.Table.Columns[i].ColumnName].ToString());
                temphtml = temphtml.Replace("{*$" + dr.Table.Columns[i].ColumnName + "$}", tempStr);
                temphtml = temphtml.Replace("{$" + dr.Table.Columns[i].ColumnName + "$}", tempStr);
            }

            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, @"\{\$\w+\$\}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return temphtml;
        }



        private string _ParentTempLabelHtml;
        public string ParentTempLabelHtml
        {
            get
            {
                return _ParentTempLabelHtml;
            }
            set
            {

                _ParentTempLabelHtml = value;
            }

        }

        private DataTable _htmlDTRepXmlSet;
        public DataTable htmlDTRepXmlSet
        {
            get
            {
                if (_htmlDTRepXmlSet != null)
                {
                    return _htmlDTRepXmlSet;
                }
                else
                {
                    _htmlDTRepXmlSet = DBAccess.GetDataTable("SELECT * FROM RepXmlSet", 7);

                }
                return _htmlDTRepXmlSet;
            }
        }


        public DataTable GetWarehouseData(string warehouseCode)
        {
            string sql = string.Format(@"SELECT 
                                           [WmsDataDigest]
                                          ,[CYBAPIURL],WmsCode
                                         FROM [t_Warehouse]
                                         WHERE WmsCode ='{0}'", warehouseCode);
            DataSet ds = DBAccess.GetDataSet(sql, 1);

            if(ds == null || ds.Tables.Count == 0)
            {
                sql = string.Format(@"SELECT 
                                           [WmsDataDigest]
                                          ,[CYBAPIURL],WmsCode
                                         FROM [t_Warehouse]
                                         WHERE WarehouseCode ='{0}'", warehouseCode);
                ds = DBAccess.GetDataSet(sql, 1);
            }

            DataTable dt = null;
            if (ds != null && ds.Tables.Count > 0)
            {
                dt = ds.Tables[0];
            }
            return dt;

        }


    }
}
