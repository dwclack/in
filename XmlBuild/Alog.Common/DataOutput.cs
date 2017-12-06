using System;
using System.Collections.Generic;
using System.Linq;
using Alog.Common;
using ASPNetPortal.DB;
using System.Data.SqlClient;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using System.Web;

namespace Alog.Common
{
    public class DataOutput
    {
        int Flag = 0;
        ClsLog log = new ClsLog();
        #region 缓存模板设置


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
                    string sql = "SELECT [AutoID],[TemplateName],[methodname],[TemplateXml],[TemplateJson],[DataSourceType],[Split1],[Split2],[DataSourceSelect],[ToDSID],[ExecSql],[parentid],[LabelCode],[MasterSQL],[QuerySQL],[CompleteSQL],ErrorSQL,[XMLContent],[JSONContent]  FROM [Apo_temp]";
                    _dtRepXmlSet = DBAccess.GetDataTable(sql, 7);

                }

                return _dtRepXmlSet;
            }
            set
            {
                _dtRepXmlSet = value;
            }
        }


        #endregion

        public List<string> GetHtmlHandleList(Dictionary<string, string> parameters, object ClsParam, string NewID, string datatype, int apiLogId, out bool isTeamplate, string returnStr)
        {
            List<string> resultTemplates = new List<string>();
            ClsLog log;
            ClsThreadParam cls;
            int toDSID;
            string QuerySql, MasterSQL, CompleteSQL, ErrorSQL;

            string template = GetTemplateFromSetting(ClsParam, datatype, apiLogId, out isTeamplate, returnStr, out log, out cls, out toDSID, out QuerySql, out MasterSQL, out CompleteSQL, out ErrorSQL);
            try
            {
                List<DataRow> masterDrs = new List<DataRow>();
                ReplaceMasterSqlParams(parameters, NewID, ref QuerySql, ref MasterSQL);
                if (!string.IsNullOrEmpty(template) || !string.IsNullOrEmpty(QuerySql) ||
                    !string.IsNullOrEmpty(MasterSQL))
                {
                    template = ReplaceDataSetValue(cls.ds, template);
                    ClsLog.AppendDbLog(template, "ReplaceDataSetValue_template");

                    QuerySql = ReplaceDataSetValue(cls.ds, QuerySql);
                    MasterSQL = ReplaceDataSetValue(cls.ds, MasterSQL);
                    resultTemplates = GetTemplateFromMasterSql(out masterDrs, template, apiLogId, ref isTeamplate, log,
                        toDSID, MasterSQL);
                    resultTemplates = GetTemplateFromQuerySql(resultTemplates, template, parameters, NewID,
                        ref isTeamplate, toDSID, MasterSQL, QuerySql, masterDrs);

                    //如果主表、副表都没有值，则不输出模板
                    //if (!isTeamplate)
                    //{
                    //    return new List<string>();
                    //}

                    string parentid = cls.dr["AutoId"].ToString();
                    if (resultTemplates.Count > 0)
                    {
                        for (int i = 0; i < resultTemplates.Count; i++)
                        {
                            resultTemplates[i] = InitLabelXML(resultTemplates[i], parameters, parentid, datatype);
                                //递归生成报文
                        }
                    }
                }
                else
                {
                    ClsLog.AppendDbLog("template , QuerySql, MasterSQL are all empty", "GetHtmlHandleList", ErrorLevel.Warning, ClsLog.ProjectName);
                }
                DealWhenComplete(parameters, NewID, apiLogId, returnStr, log, cls, toDSID, CompleteSQL, ErrorSQL, masterDrs);

            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Flag=" + Flag.ToString() + ex.Message, cls.dr["TemplateName"].ToString());
            }
            return resultTemplates;
        }

        public string GetHtmlHandle(Dictionary<string, string> parameters, object ClsParam, string NewID, string datatype, int apiLogId, out bool isTeamplate, string returnStr)
        {
            //导入已经出错，直接返回
            if (!string.IsNullOrWhiteSpace(returnStr))
            {
                ClsLog.AppendDbLog("import error:" + returnStr, "GetHtmlHandle", ErrorLevel.Error, ClsLog.ProjectName);
                isTeamplate = false;
                return string.Empty;
            }
            List<string> results = GetHtmlHandleList(parameters, ClsParam, NewID, datatype, apiLogId, out isTeamplate, returnStr);

            if(results.Count > 0)
            {
                return results[0];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 收尾处理
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="NewID"></param>
        /// <param name="datatype"></param>
        /// <param name="apiLogId"></param>
        /// <param name="returnStr"></param>
        /// <param name="log"></param>
        /// <param name="cls"></param>
        /// <param name="toDSID"></param>
        /// <param name="CompleteSQL"></param>
        /// <param name="ErrorSQL"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        private void DealWhenComplete(Dictionary<string, string> parameters, string NewID, int apiLogId, string returnStr, ClsLog log, ClsThreadParam cls, int toDSID, string CompleteSQL, string ErrorSQL, List<DataRow> masterDrs)
        {         
            Flag = 7;
            string tempCompleteSQL = CompleteSQL;
            string tempErrorSQL = ErrorSQL;
            //log.ApiResultLog(apiLogId, "GetHtmlHandle-2：执行QuerySql后", "1", toDSID);
            if (returnStr.Contains("错") || returnStr.Contains("败") || returnStr.Contains("异"))
            {
                if (!string.IsNullOrEmpty(tempCompleteSQL)) //若非空，做收尾处理，更新状态
                {
                    tempCompleteSQL = tempCompleteSQL.Replace("~", "'");
                    tempCompleteSQL = tempCompleteSQL.Replace("{#GUID}", NewID);
                    tempCompleteSQL = common.ReplaceParameter(tempCompleteSQL, parameters);
                    foreach (DataRow dr in masterDrs)
                    {
                        tempCompleteSQL = ReplaceMasterValue(tempCompleteSQL, dr);
                        DBAccess.ExeuteSQL(tempCompleteSQL, toDSID);
                        //log.ApiResultLog(apiLogId, "GetHtmlHandle-3：执行CompleteSQL后", "1", toDSID);
                    }                    
                }
            }

            Flag = 8;
        }

        /// <summary>
        /// 根据从查询语句取得报文
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="NewID"></param>
        /// <param name="isTeamplate"></param>
        /// <param name="toDSID"></param>
        /// <param name="QuerySql"></param>
        /// <param name="masterDrs"></param>
        /// <returns></returns>
        private List<string> GetTemplateFromQuerySql(List<string> resultTemplates, string template, Dictionary<string, string> parameters, string NewID, ref bool isTeamplate, int toDSID, string MasterSQL, string QuerySql, List<DataRow> masterDrs)
        {
            if (resultTemplates.Count > 0)
            {
                for (int i = 0; i < resultTemplates.Count; i++)
                {
                    string tempQuerySql = QuerySql;
                    tempQuerySql = tempQuerySql.Replace("{#GUID}", NewID);
                    if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.Contains("UserName"))
                    {
                        tempQuerySql = tempQuerySql.Replace("{$UserName$}", HttpContext.Current.Items["UserName"].ToString());
                    }
                    tempQuerySql = common.ReplaceParameter(tempQuerySql, parameters);

                    if (masterDrs != null)
                    {
                        tempQuerySql = ReplaceMasterValue(tempQuerySql, masterDrs[i]);//替换表体SQL(循环用)查询的的主表数据
                    }
                    if (tempQuerySql != string.Empty)
                    {
                        DataTable dt = DBAccess.GetDataTable(tempQuerySql.Replace("~", "'"), toDSID);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            isTeamplate = true;
                            resultTemplates[i] = ReplaceSlaveValue(resultTemplates[i], dt);//替换XML模板的表体及表体明细数据
                        }
                        else 
                        {
                            //无数据时 替换掉loop内内容
                            string tmpRes = resultTemplates[i];
                            int startIndex = tmpRes.IndexOf("\"loop\":");
                            int endIndex = tmpRes.IndexOf(",\"endloop\":\"endloop\"");
                            if (startIndex > -1 && endIndex > -1) 
                            {
                                string[] loopSplit = tmpRes.Split(new string[] { "\"loop\":" }, StringSplitOptions.RemoveEmptyEntries);
                                string[] endloopSplit = loopSplit[1].Split(new string[] { ",\"endloop\":\"endloop\"" }, StringSplitOptions.RemoveEmptyEntries);
                                resultTemplates[i] = loopSplit[0] + endloopSplit[1];
                            }                            
                        }
                    }
                }
            }
            else if(string.IsNullOrEmpty(MasterSQL))
            {
                string tempQuerySql = QuerySql;
                tempQuerySql = tempQuerySql.Replace("{#GUID}", NewID);
                if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.Contains("UserName"))
                {
                    tempQuerySql = tempQuerySql.Replace("{$UserName$}", HttpContext.Current.Items["UserName"].ToString());
                }
                tempQuerySql = common.ReplaceParameter(tempQuerySql, parameters);

                if (tempQuerySql != string.Empty && !tempQuerySql.Contains("<loop>"))
                {
                    DataTable dt = new DataTable();

                    if (!parameters.ContainsKey("pagesize"))
                    {
                        dt = DBAccess.GetDataTable(tempQuerySql.Replace("~", "'"), toDSID);
                    }
                    else
                    {

                        int PageSize = 15;

                        if (parameters.ContainsKey("pagesize"))
                        {
                            PageSize = int.Parse(parameters["pagesize"]);
                        }
                        int PageNum = 1;
                        if (parameters.ContainsKey("pagenum"))
                        {
                            PageNum = int.Parse(parameters["pagenum"]);
                        }
                        string PageKey = "";
                        if (parameters.ContainsKey("pagekey"))
                        {
                            PageKey = parameters["pagekey"];
                        }

                        string DSConnectionString = DBAccess.DesDecrypt(DBAccess.DataSource.Select("dsid = " + toDSID.ToString())[0]["ConnectionString"].ToString());
                        int RecordCount = 0;
                        if (tempQuerySql.ToLower().IndexOf("exec ") > -1)
                        {
                            DataSet dstemp = DBAccess.GetDataSet(tempQuerySql, toDSID);
                            dt = Pager.Paging(dstemp, "", PageSize, PageNum, out RecordCount).Tables[0];
                        }
                        else
                        {
                            DataSet ds = Pager.Paging(tempQuerySql, PageKey, PageSize, PageNum, DSConnectionString, out RecordCount);
                            if (ds.Tables.Count > 0)
                            {
                                dt = ds.Tables[0];
                            }

                        }


                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        isTeamplate = true;
                        string temp = ReplaceSlaveValue(template, dt); //替换XML模板的表体及表体明细数据
                        resultTemplates.Add(temp);
                    }
                }
                else
                {
                    resultTemplates.Add(template);
                }
            }

            return resultTemplates;
        }

        /// <summary>
        /// 根据主查询语句获得报文
        /// </summary>
        /// <param name="apiLogId"></param>
        /// <param name="isTeamplate"></param>
        /// <param name="log"></param>
        /// <param name="toDSID"></param>
        /// <param name="MasterSQL"></param>
        /// <returns></returns>
        private List<string> GetTemplateFromMasterSql(out List<DataRow> masterDrs, string template, int apiLogId, ref bool isTeamplate, ClsLog log, int toDSID, string MasterSQL)
        {
            List<string> resultTemplates = new List<string>();
            masterDrs = new List<DataRow>();

            if (MasterSQL != string.Empty)
            {
                DataTable dtMaster = DBAccess.GetDataTable(MasterSQL.Replace("~", "'"), toDSID);

                Flag = 6;

                if (dtMaster.Rows.Count > 0)
                {
                    isTeamplate = true;

                    foreach (DataRow dr in dtMaster.Rows)
                    {
                        string temp = ReplaceMasterValue(template, dr);//替换XML模板的主表数据
                        resultTemplates.Add(temp);
                        masterDrs.Add(dr);
                    }
                }
                //log.ApiResultLog(apiLogId, "GetHtmlHandle-1：执行MasterSQL后", "1", toDSID);
            }

            return resultTemplates;
        }

        /// <summary>
        /// 替换主查询语句中的参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="NewID"></param>
        /// <param name="QuerySql"></param>
        /// <param name="MasterSQL"></param>
        private void ReplaceMasterSqlParams(Dictionary<string, string> parameters, string NewID, ref string QuerySql, ref string MasterSQL)
        {
            Flag = 5;
            MasterSQL = MasterSQL.Replace("~", "'");
            MasterSQL = MasterSQL.Replace("{#GUID}", NewID);
            if (parameters.ContainsKey("WarehouseId"))
            {
                string warehouseId = parameters["WarehouseId"];
                MasterSQL = MasterSQL.Replace("{#WarehouseId}", warehouseId);
                QuerySql = QuerySql.Replace("{#WarehouseId}", warehouseId);
            }
            if (parameters.ContainsKey("OwnerId"))
            {
                string ownerId = parameters["OwnerId"];
                MasterSQL = MasterSQL.Replace("{#OwnerId}", ownerId);
                QuerySql = QuerySql.Replace("{#OwnerId}", ownerId);
            }
            if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.Contains("UserName"))
            {
                MasterSQL = MasterSQL.Replace("{$UserName$}", HttpContext.Current.Items["UserName"].ToString());
            }
            MasterSQL = common.ReplaceParameter(MasterSQL, parameters);
            QuerySql = common.ReplaceParameter(QuerySql, parameters);
        }
        public static bool IsMatchWord(string Str1, string Str2, out int Index)
        {
            bool Match = false;
            Index = -1;
            try
            {

                Match MM = System.Text.RegularExpressions.Regex.Match(Str1, Str2 ,  RegexOptions.ExplicitCapture);

                if (MM.Success)
                {
                    Index = MM.Index;
                    Match = true;
                    return Match;
                }
                else
                {
                    MM = System.Text.RegularExpressions.Regex.Match(Str1, Str2 ,  RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                    if (MM.Success)
                    {
                        Index = MM.Index;
                        Match = true;
                        return Match;
                    }
                }
            }
            catch
            {
                Match = false;
                Index = -1;
            }

            return Match;

        }
        protected static string[] SplitStr(string str1, string str2)
        {
            string[] strlist = new string[] { "", "" };
            int index = -1;
            bool Match = IsMatchWord(str1, str2, out index);
            if (index > -1)
            {
                strlist[0] = str1.Substring(0, index);
                strlist[1] = str1.Substring(index + str2.Length - 1);
            }
            else
            {
                strlist[0] = str1;
            }
            return strlist;
        }
        private string ReplaceDataSetValue(DataSet ds, string str)
        {
            //if (ds == null)
            //{
            //    return str;
            //}
            string pattern = @"\{#\S+#\}";

            if (ds != null)
            {


                MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(str, pattern, RegexOptions.IgnoreCase);
                foreach (Match m in mc)
                {
                    string key = m.Value.Trim();
                    string keyvalue = GetDataSetValue(ds, key.Replace("{#", "").Replace("#}", ""));

                    str = str.Replace(key, keyvalue);

                    //     str =Regex.Replace(str, key, keyvalue, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
            }
            str = Regex.Replace(str, pattern, "''", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return str;
        }
        private string GetDataSetValue(DataSet ds, string ExpressionStr)
        {
            string retset = "";
            int TableIndex = 0;
            int RowsIndex = 0;
            string TableName = "";
            string columnName = "";
            bool IsInt;
            //ExpressionStr="tables[0].rows[0]["aaa"]";
            //ExpressionStr="tables[tablename].rows[0]["aaa]";
            try
            {
                TableName = SplitStr(SplitStr(ExpressionStr, "tables\\[")[1], "]")[0];
                
            }
            catch 
            {
                throw new Exception("表达式错误:表名出错");
            }
            IsInt = int.TryParse(TableName, out TableIndex);
            //表名不存在
            if (!IsInt && !ds.Tables.Contains(TableName))
            {
                throw new Exception("表达式错误:表名出错");
            }
            //表指数过界
            if (IsInt && (ds.Tables.Count <= TableIndex) || TableIndex < 0)
            {
                throw new Exception("表达式错误:表名出错");
            }

            DataTable table = IsInt ? ds.Tables[TableIndex] : ds.Tables[TableName];

            try
            {
                RowsIndex = int.Parse(SplitStr(SplitStr(ExpressionStr, "].rows\\[")[1], "]")[0]);
            }
            catch 
            {
                throw new Exception("表达式错误:行出错");
            }
            //行过界
            if (RowsIndex < 0 || RowsIndex >= table.Rows.Count)
            {
                throw new Exception("表达式错误:行出错");
            }

            try
            {
                columnName = SplitStr(SplitStr(ExpressionStr, "]\\[")[1], "]")[0];
            }
            catch (Exception)
            {
                throw new Exception("表达式错误");
            }
            ClsLog.AppendDbLog(TableName + "," + RowsIndex + "," + columnName, "GetDataSetValue", ErrorLevel.Warning, ClsLog.ProjectName);
            try
            {
                retset = table.Rows[RowsIndex][columnName].ToString();
            }
            catch (Exception ex)
            {

            }
            return retset;
        }
        /// <summary>
        /// 从接口配置模板中取得报文模板
        /// </summary>
        /// <param name="ClsParam"></param>
        /// <param name="datatype"></param>
        /// <param name="apiLogId"></param>
        /// <param name="isTeamplate"></param>
        /// <param name="returnStr"></param>
        /// <param name="log"></param>
        /// <param name="cls"></param>
        /// <param name="toDSID"></param>
        /// <param name="strHTML"></param>
        /// <param name="QuerySql"></param>
        /// <param name="MasterSQL"></param>
        /// <param name="CompleteSQL"></param>
        /// <param name="ErrorSQL"></param>
        private string GetTemplateFromSetting(object ClsParam, string datatype, int apiLogId, out bool isTeamplate, string returnStr, out ClsLog log, out ClsThreadParam cls, out int toDSID, out string QuerySql, out string MasterSQL, out string CompleteSQL, out string ErrorSQL)
        {
            string resultTemplate = string.Empty;

            //isTeamplate用来判断是否输出模板，如果副表、主表查询都没有值，则不输出模板
            isTeamplate = false;
            log = new ClsLog();
            cls = (ClsThreadParam)ClsParam;
            toDSID = int.Parse(cls.dr["ToDSID"].ToString());
            //log.ApiResultLog(apiLogId, "GetHtmlHandle-0：执行MasterSQL前", "1", toDSID);

            QuerySql = cls.dr["QuerySql"].ToString();
            MasterSQL = cls.dr["MasterSQL"].ToString();
            CompleteSQL = cls.dr["CompleteSQL"].ToString();
            ErrorSQL = cls.dr["ErrorSQL"].ToString();
            switch (datatype)
            {
                case "json":
                    resultTemplate = cls.dr["JSONContent"].ToString();
                    break;
                case "xml":
                    resultTemplate = cls.dr["XMLContent"].ToString();
                    break;
            }
            resultTemplate = resultTemplate.Replace("{#returnstr}", returnStr);
            if (returnStr.Contains("错") || returnStr.Contains("败") || returnStr.Contains("异"))
            {
                resultTemplate = resultTemplate.Replace("{#Success}", "false");
                resultTemplate = resultTemplate.Replace("{#ErrorCode}", "NO");
                resultTemplate = resultTemplate.Replace("{#ErrorInfo}", "执行失败");
            }

            return resultTemplate;
        }

        /// <summary>
        /// 递归读取标替换签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        public string InitLabelXML(string template, Dictionary<string, string> parameters, string parentid, string datatype)
        {
            #region
            if (dtRepXmlSet.Select("parentid=" + parentid).Count() > 0)
            {
                DataRow[] drs = dtRepXmlSet.Select("parentid=" + parentid);
                for (int i = 0; i < drs.Count(); i++)
                {
                    string temphtml = "";
                    switch (datatype)
                    {
                        case "json":
                            temphtml = drs[i]["JSONContent"].ToString();
                            break;
                        case "xml":
                            temphtml = drs[i]["XMLContent"].ToString();
                            break;
                    }
                    string LabelCode = "";
                    string MasterSQL = "";
                    string SlaveSQL = "";
                    int ToDSID = 0;
                    LabelCode = drs[i]["LabelCode"].ToString();
                    string str = @"(" + LabelCode + @"(\((\w|,)*\)))|(" + LabelCode + ")";//表达
                    MatchCollection macths = Regex.Matches(template, str, RegexOptions.RightToLeft);
                    foreach (Match macth in macths)
                    {
                        string temptag = "";
                        temptag = macth.ToString();//把匹配的项付给变量
                        MasterSQL = drs[i]["MasterSQL"].ToString();
                        MasterSQL = common.ReplaceParameter(MasterSQL, parameters);
                        if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.Contains("UserName"))
                        {
                            MasterSQL = MasterSQL.Replace("{$UserName$}", HttpContext.Current.Items["UserName"].ToString());
                        }
                        // MasterSQL = ReplaceParameterValue(MasterSQL, Parameters);
                        SlaveSQL = drs[i]["QuerySQL"].ToString();
                        ToDSID = int.Parse(drs[i]["ToDSID"].ToString());
                        DataTable dtMaster = new DataTable();
                        if (!string.IsNullOrEmpty(MasterSQL))
                        {
                            MasterSQL = MasterSQL.Replace("~", "'");

                            dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);

                            temphtml = ReplaceMasterValue(temphtml, dtMaster.Rows[0]); ;
                        }
                        if (!string.IsNullOrEmpty(SlaveSQL))
                        {
                            SlaveSQL = SlaveSQL.Replace("~", "'");
                            SlaveSQL = common.ReplaceParameter(SlaveSQL, parameters);
                            if (HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.Contains("UserName"))
                            {
                                SlaveSQL = SlaveSQL.Replace("{$UserName$}", HttpContext.Current.Items["UserName"].ToString());
                            }
                            if (dtMaster.Rows.Count > 0)
                            {
                                SlaveSQL = ReplaceMasterValue(SlaveSQL, dtMaster.Rows[0]); ;
                            }
                            try
                            {
                                DataTable dtSlave = DBAccess.GetDataTable(SlaveSQL, ToDSID);
                                temphtml = ReplaceSlaveValue(temphtml, dtSlave);
                            }
                            catch (Exception ex)
                            {
                                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                                {
                                    HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0007;
                                }
                                throw ex;
                            }


                        }
                        template = template.Replace(temptag, temphtml);

                    }

                    parentid = drs[i]["AutoId"].ToString();
                    template = InitLabelXML(template, parameters, parentid, datatype);

                }
            }

            return template;
            #endregion
        }

        /// <summary>
        /// 递归读取标替换签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        public void InitLabelXML(Dictionary<string, string> parameters, string parentid, string datatype)
        {
            InitLabelXML(ParentTempLabelHtml, parameters, parentid, datatype);
        }
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
                temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]PARENT[}].[{][*]#" + Slavedr.Table.Columns[i].ColumnName + "}", Slavedr[Slavedr.Table.Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]PARENT[}].[{]#" + Slavedr.Table.Columns[i].ColumnName + "}", Slavedr[Slavedr.Table.Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return temphtml;
        }



        public string ReplaceSlaveValue(string temphtml, DataTable Slavedt)
        {


            if ((temphtml.IndexOf("<loop>") > -1 && temphtml.IndexOf("</loop>") > -1) || (temphtml.IndexOf("\"loop\":") > -1 && temphtml.IndexOf(",\"endloop\":\"endloop\"") > -1))
            {
                //xml数据格式
                if ((temphtml.IndexOf("<loop>") > -1 && temphtml.IndexOf("</loop>") > -1))
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
                                string loophtml = temphtml_2_temp;
                                for (int i = 0; i < Slavedt.Columns.Count; i++)
                                {
                                    //loophtml = System.Text.RegularExpressions.Regex.Replace(temphtml_2_temp, "[{][*]#" + Slavedt.Columns[i].ColumnName + "}", Slavedt.Rows[k][Slavedt.Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    loophtml = System.Text.RegularExpressions.Regex.Replace(loophtml,
                                        "[{]#" + Slavedt.Columns[i].ColumnName + "}",
                                        Slavedt.Rows[k][Slavedt.Columns[i].ColumnName].ToString(),
                                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                }

                                OutHTML += loophtml;

                            }
                            OutHTML = OutHTML.Substring(0, OutHTML.Length - 1);

                        }
                        OutHTML += loopstrtempEnd;

                    }
                    OutHTML = System.Text.RegularExpressions.Regex.Replace(OutHTML, @"{\#\w+}", "",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    temphtml = OutHTML;
                }
                //json数据格式
                if ((temphtml.IndexOf("\"loop\":") > -1 && temphtml.IndexOf(",\"endloop\":\"endloop\"") > -1))
                {
                    string OutHTML = "";
                    string[] Themlstr = System.Text.RegularExpressions.Regex.Split(temphtml, "\"loop\":");
                    string temphtml_1 = Themlstr[0];
                    OutHTML += temphtml_1;
                    //string temphtml_3 = System.Text.RegularExpressions.Regex.Split(Themlstr[Themlstr.Length-1], "</loop>")[1];
                    for (int j = 1; j < Themlstr.Length; j++)
                    {
                        string temphtml_2_temp = System.Text.RegularExpressions.Regex.Split(Themlstr[j], ",\"endloop\":\"endloop\"")[0];
                        string loopstrtempEnd = System.Text.RegularExpressions.Regex.Split(Themlstr[j], ",\"endloop\":\"endloop\"")[1];

                        if (Slavedt.Rows.Count > 0)
                        {
                            for (int k = 0; k < Slavedt.Rows.Count; k++)
                            {
                                string loophtml = temphtml_2_temp;
                                for (int i = 0; i < Slavedt.Columns.Count; i++)
                                {
                                    //loophtml = System.Text.RegularExpressions.Regex.Replace(temphtml_2_temp, "[{][*]#" + Slavedt.Columns[i].ColumnName + "}", Slavedt.Rows[k][Slavedt.Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                    loophtml = System.Text.RegularExpressions.Regex.Replace(loophtml,
                                        "[{]#" + Slavedt.Columns[i].ColumnName + "}",
                                        Slavedt.Rows[k][Slavedt.Columns[i].ColumnName].ToString(),
                                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                                }

                                OutHTML += ((k == 0) ? "" : ",") + loophtml;

                            }


                        }
                        OutHTML += loopstrtempEnd;

                    }
                    OutHTML = System.Text.RegularExpressions.Regex.Replace(OutHTML, @"{\#\w+}", "",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    temphtml = OutHTML;
                }
            }
            else
            {//如果没有LOOP,就直接只返回一行数据

                if (Slavedt.Rows.Count > 0)
                {

                    if (Slavedt.Rows.Count > 0)
                    {

                        for (int i = 0; i < Slavedt.Columns.Count; i++)
                        {

                            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{][*]#" + Slavedt.Columns[i].ColumnName + "}", Slavedt.Rows[0][Slavedt.Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]#" + Slavedt.Columns[i].ColumnName + "}", Slavedt.Rows[0][Slavedt.Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);



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


        public string ReplaceParentMasterValue(string temphtml, DataRow dr)
        {


            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                temphtml = temphtml.Replace("[{]PARENT[}].{*$" + dr.Table.Columns[i].ColumnName + "$}", dr[dr.Table.Columns[i].ColumnName].ToString());
                temphtml = temphtml.Replace("[{]PARENT[}].{$" + dr.Table.Columns[i].ColumnName + "$}", dr[dr.Table.Columns[i].ColumnName].ToString());
            }

            temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, @"[{]PARENT[}].\{\$\w+\$\}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return temphtml;
        }

        public string ReplaceMasterValue(string temphtml, DataRow dr)
        {


            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                temphtml = temphtml.Replace("{*$" + dr.Table.Columns[i].ColumnName + "$}", dr[dr.Table.Columns[i].ColumnName].ToString());
                temphtml = temphtml.Replace("{$" + dr.Table.Columns[i].ColumnName + "$}", dr[dr.Table.Columns[i].ColumnName].ToString());
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

    }
}

