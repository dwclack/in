using ASPNetPortal.DB;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Alog.Common
{
    public class MySqlDataImport
    {
        ClsLog log = new ClsLog();
        private string importConnStrName;
        private string UserName = string.Empty;
        #region 导入数据库
        public void ImportToDB(string importData, string datatype, string method, Dictionary<string, string> parameters, string NewID, out string resultStr)
        {
            int ImportFileFlag = 0;
            resultStr = string.Empty;
            string ErrorSQL = "";
            try
            {
                DataTable Import_tempView = Import_temp.Clone();
                ImportFileFlag = 1;

                string ownerId = string.Empty;
                string warehouseId = string.Empty;

                if (parameters.ContainsKey("OwnerId"))
                {
                    ownerId = parameters["OwnerId"];
                }
                if (parameters.ContainsKey("WarehouseId"))
                {
                    warehouseId = parameters["WarehouseId"];
                }

                DataRow[] drs = null;
                drs = Import_temp.Select("methodname = '" + method.ToString() + "'");
                ImportFileFlag = 2;
                if (drs.Length > 0 && !string.IsNullOrEmpty(importData))
                {
                    Import_tempView.ImportRow(drs[0]);
                    ErrorSQL = Import_tempView.Rows[0]["ErrorSQL"].ToString();
                    importConnStrName = GetImportConnStrName(Import_tempView.Rows[0]["Parameters"].ToString());

                    string DataSourceType = datatype;//数据源类型
                    string strID = Import_tempView.Rows[0]["AutoID"].ToString();

                    DataTable dtImportTablesInfo = Import_table.Clone();
                    ImportFileFlag = 3;
                    DataRow[] drsImportTable = Import_table.Select("TMPT_ID=" + strID.ToString(), "ORDER_NUM");

                    foreach (DataRow dr in drsImportTable)
                    {
                        dtImportTablesInfo.ImportRow(dr);
                    }
                    ImportFileFlag = 4;
                    #region 根据数据源类型导入
                    DataSet dsImportDatas = GetImportDSFromPostDataStr(importData, DataSourceType, parameters, ref resultStr);
                    ImportFileFlag = 5;

                    //循环导入xml节点
                    InsertDataSet(parameters, dsImportDatas, dtImportTablesInfo, NewID, ownerId, warehouseId, out resultStr);
                    ImportFileFlag = 6;

                    #endregion
                    #region 导入完后运行的SQL
                    if (resultStr.Contains("错") || resultStr.Contains("败") || resultStr.Contains("异"))
                    {
                        if (!string.IsNullOrEmpty(ErrorSQL))
                        {

                            ErrorSQL = ErrorSQL.Replace("~", "'");
                            ErrorSQL = ErrorSQL.Replace("{#GUID}", NewID);
                            DBAccess.GetDataTable(ErrorSQL, importConnStrName);
                        }

                    }
                    else
                    {

                        string ExecSql = Import_tempView.Rows[0]["ExecSql"].ToString();
                        if (resultStr == string.Empty)
                        {
                            if (!string.IsNullOrEmpty(ExecSql))
                            {
                                try
                                {
                                    ImportFileFlag = 7;
                                    ExecSql = ExecSql.Replace("~", "'");
                                    DBAccess.ExeuteSQL(ExecSql, importConnStrName);
                                }
                                catch (Exception ex)
                                {
                                    ClsLog.AppendDbLog("ImportFileFlag=" + ImportFileFlag + "," + ex.Message, "ImportFile", ErrorLevel.Error, ClsLog.ProjectName);

                                    throw ex;
                                }
                            }
                        }
                    }
                    #endregion

                }
                else
                {
                    ImportFileFlag = 8;
                    resultStr = ErrorCode.ERROR_CODE.S0003.ToString();
                }
            }
            catch (Exception ex)
            {
                string tempErrorSQL = ErrorSQL;
                resultStr = "出错了！" + ex.Message.ToString();
                if (resultStr.Contains("错") || resultStr.Contains("败") || resultStr.Contains("异"))
                {
                    if (!string.IsNullOrEmpty(tempErrorSQL))   //若非空，做收尾处理，更新状态
                    {
                        tempErrorSQL = tempErrorSQL.Replace("~", "'");
                        tempErrorSQL = tempErrorSQL.Replace("{#GUID}", NewID);
                        tempErrorSQL = common.ReplaceParameter(tempErrorSQL, parameters);

                        DBAccess.ExeuteSQL(tempErrorSQL, importConnStrName);
                    }
                }
                ClsLog.AppendDbLog("ImportFileFlag=" + ImportFileFlag + "," + ex.Message, "ImportFile", ErrorLevel.Error, ClsLog.ProjectName);

            }
        }

        private string GetImportConnStrName(string paramStr)
        {
            List<string> paramList = paramStr.Split('&').ToList();
            Dictionary<string, string> paramDic = new Dictionary<string, string>();
            foreach (var param in paramList)
            {
                List<string> keyValue = param.Split('=').ToList();
                paramDic.Add(keyValue[0], keyValue[1]);
            }
            return paramDic["ImportConnectionStringName"];
        }

        /// <summary>
        /// 将报文转换为数据集
        /// </summary>
        /// <param name="postdata">报文字符串</param>
        /// <param name="DataSourceType">报文类型（xml或json）</param>
        /// <param name="parameters">参数</param>
        /// <param name="resultStr">返回字符串</param>
        /// <returns></returns>
        private DataSet GetImportDSFromPostDataStr(string postdata, string DataSourceType, Dictionary<string, string> parameters, ref string resultStr)
        {
            DataSet dsImportDatas = new DataSet();

            string specialNodes = string.Empty;
            if (parameters.ContainsKey("specialNodes"))
            {
                specialNodes = parameters["specialNodes"].ToString();
            }

            if (DataSourceType == "xml")
            {
                #region xml导入
                using (StringReader sr = new StringReader(postdata))
                {
                    try
                    {
                        dsImportDatas.ReadXml(sr, XmlReadMode.InferSchema);
                    }
                    catch (Exception ex)
                    {
                        resultStr = "出错了！" + ex.Message.ToString();
                        throw ex;
                    }
                }
                #endregion
            }
            else if (DataSourceType == "json")
            {
                #region json导入
                postdata = postdata.IndexOf("[") > -1 ? postdata : "{Table:[" + postdata + "]}";
                try
                {
                    dsImportDatas = JsonToDS(postdata, specialNodes);
                }
                catch (Exception ex)
                {
                    resultStr = "出错了！" + ex.Message.ToString();
                    throw ex;
                }
                #endregion
            }

            return dsImportDatas;
        }
        /// <summary>
        /// 将json格式报文转换为数据集
        /// </summary>
        /// <param name="jsonStr">json格式报文</param>
        /// <param name="specialNodes">特殊节点：节点内容不再解析，作为节点值</param>
        /// <returns></returns>
        private DataSet JsonToDS(string jsonStr, string specialNodes)
        {
            int JsonToDSFlag = 0;
            DataSet result = new DataSet();

            try
            {
                XmlDocument xmlDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonStr);
                JsonToDSFlag = 1;
                if (!string.IsNullOrEmpty(specialNodes))
                {
                    List<string> nodeList = specialNodes.Split(',').ToList();
                    JsonToDSFlag = 2;
                    foreach (var nodeName in nodeList)
                    {
                        XmlNodeList nodes = xmlDoc.SelectNodes("//" + nodeName);
                        JsonToDSFlag = 3;
                        foreach (XmlNode node in nodes)
                        {
                            if (node.HasChildNodes && node.FirstChild.HasChildNodes)
                            {
                                string replaceStr = string.Empty;
                                JsonToDSFlag = 4;
                                foreach (XmlNode childNode in node.ChildNodes)
                                {
                                    replaceStr += childNode.Name + ":" + childNode.InnerXml + ",";
                                }
                                if (replaceStr != string.Empty)
                                {
                                    JsonToDSFlag = 5;
                                    replaceStr = replaceStr.Remove(replaceStr.Length - 1);
                                    node.InnerXml = replaceStr;
                                }
                                JsonToDSFlag = 6;
                            }
                        }
                    }
                }
                JsonToDSFlag = 7;
                result.ReadXml(new XmlNodeReader(xmlDoc));
                JsonToDSFlag = 8;
            }
            catch (Exception ex)
            {
                ClsLog.AppendDbLog("JsonToDSFlag=" + JsonToDSFlag + "," + ex.Message, "JsonToDS", ErrorLevel.Error, ClsLog.ProjectName);
                throw new ArgumentException(ex.Message);
            }

            return result;
        }
        #endregion
        #region 导入数据集到数据库
        private void InsertDataSet(Dictionary<string, string> parameters, DataSet dsImportDatas, DataTable dtImportTablesInfo, string NewID, string ownerId, string warehouseId, out string resultStr)
        {
            int InsertDataSetFlag = 0;
            resultStr = "";
            string handleStr = "";

            try
            {
                int DataGridIndex = 0;
                DataRow[] rows = dtImportTablesInfo.Select("1=1", "ORDER_NUM");
                InsertDataSetFlag = 1;
                int dataCount = 0;
                foreach (DataRow drImportTableInfo in rows)
                {
                    InsertDataSetFlag = 2;
                    if (handleStr == string.Empty)
                    {
                        if (dsImportDatas.Tables.Contains(drImportTableInfo["TABLE_NAME"].ToString()))
                        {
                            //XML导入数据开始
                            DataGridIndex++;
                            string TABLE_NAME = drImportTableInfo["TABLE_NAME"].ToString();
                            string SELEDT_SQL = drImportTableInfo["SELEDT_SQL"].ToString().Replace("~", "'");
                            string ISREPEAT_SQL = drImportTableInfo["ISREPEAT_SQL"].ToString().Replace("~", "'");
                            string INSERT_SQL = drImportTableInfo["INSERT_SQL"].ToString().Replace("~", "'");
                            string newInsertSql = (drImportTableInfo["newinsertsql"] != DBNull.Value && drImportTableInfo["newinsertsql"] != null) ? drImportTableInfo["newinsertsql"].ToString().Replace("~", "'") : null;
                            string TARGET_TABLE_MasterKey = drImportTableInfo["TARGET_TABLE_MasterKey"].ToString();
                            string SUR_TABLE_MasterKey = drImportTableInfo["SUR_TABLE_MasterKey"].ToString();
                            InsertDataSetFlag = 3;

                            if (dataCount == 0)
                            {
                                dataCount = dsImportDatas.Tables[TABLE_NAME].Rows.Count;
                            }

                            //替换参数
                            DataTable importColumnsInfo = Import_columns.Clone();
                            DataRow[] ys_drs = Import_columns.Select("TABLE_ID = " + drImportTableInfo["TABLE_ID"].ToString(), "COLUMN_ID");

                            foreach (DataRow dr in ys_drs)
                            {
                                importColumnsInfo.ImportRow(dr);
                            }
                            InsertDataSetFlag = 4;
                            //给源数据增加表达式列
                            AddRepeatRowInfo(dsImportDatas, drImportTableInfo, TABLE_NAME, importColumnsInfo);
                            string tableKey;
                            AddColumnsFromExpression(dsImportDatas, ref InsertDataSetFlag, out tableKey, TABLE_NAME, importColumnsInfo);
                            InsertDataSetFlag = 15;
                            InsertDataTable(parameters, dsImportDatas.Tables[TABLE_NAME], tableKey, NewID, ownerId, warehouseId, SELEDT_SQL, ISREPEAT_SQL, INSERT_SQL, importColumnsInfo, TABLE_NAME, SUR_TABLE_MasterKey, TARGET_TABLE_MasterKey, out handleStr);
                            InsertDataSetFlag = 16;
                            if (handleStr.Contains("错") || handleStr.Contains("败") || handleStr.Contains("异"))
                            {
                                InsertDataSetFlag = 17;
                                resultStr = handleStr;
                                return;
                            }
                            if (dataCount > 1)
                            {
                                handleStr = string.Empty;
                            }
                        }

                    }

                }
                resultStr = handleStr;
                InsertDataSetFlag = 18;
            }
            catch (Exception ex)
            {
                ClsLog.AppendDbLog("InsertDataSetFlag = " + InsertDataSetFlag + "," + ex.Message.ToString(), "InsertDataSet", ErrorLevel.Error, ClsLog.ProjectName);
                resultStr = "系统异常，" + ex.Message;
            }
        }

        /// <summary>
        /// 给列信息增加是否重复行信息
        /// </summary>
        /// <param name="dsImportDatas"></param>
        /// <param name="drImportTableInfo"></param>
        /// <param name="TABLE_NAME"></param>
        /// <param name="importColumnsInfo"></param>
        private static void AddRepeatRowInfo(DataSet dsImportDatas, DataRow drImportTableInfo, string TABLE_NAME, DataTable importColumnsInfo)
        {
            if (dsImportDatas.Tables[TABLE_NAME].ParentRelations.Count > 0)
            {
                string parentTableName = dsImportDatas.Tables[TABLE_NAME].ParentRelations[0].ParentTable.TableName;
                if (dsImportDatas.Tables[parentTableName].Columns.Contains("isRepeatRow"))
                {
                    DataRow dr = importColumnsInfo.NewRow();
                    dr["TABLE_ID"] = drImportTableInfo["TABLE_ID"].ToString();
                    dr["COLUMN_CODE"] = "isRepeatRow";
                    dr["DATA_COLUMN"] = "isRepeatRow";
                    dr["DATA_Expression"] = "parent=1,column=isRepeatRow";
                    importColumnsInfo.Rows.Add(dr);
                }
            }
        }

        /// <summary>
        /// 从表达式推导出列信息，添加到待导入数据
        /// </summary>
        /// <param name="dsImportDatas">待导入数据</param>
        /// <param name="InsertDataSetFlag"></param>
        /// <param name="tableKey">表主键名</param>
        /// <param name="TABLE_NAME">表明</param>
        /// <param name="columnsInfo">列信息</param>
        private void AddColumnsFromExpression(DataSet dsImportDatas, ref int InsertDataSetFlag, out string tableKey, string TABLE_NAME, DataTable columnsInfo)
        {
            tableKey = string.Empty;

            foreach (DataRow columnInfor in columnsInfo.Rows)
            {
                string DATA_COLUMN = columnInfor["DATA_COLUMN"].ToString();
                if (!dsImportDatas.Tables[TABLE_NAME].Columns.Contains(DATA_COLUMN))
                {
                    dsImportDatas.Tables[TABLE_NAME].Columns.Add(DATA_COLUMN, typeof(System.String));
                }
                string dataExpression = columnInfor["DATA_Expression"].ToString();
                if (dataExpression != "")
                {
                    InsertDataSetFlag = 5;
                    string sourceColumnName = "";
                    string wherestr = "";
                    string sourceTableName = TABLE_NAME;

                    if (dataExpression.Contains("parent="))
                    {
                        InsertDataSetFlag = 6;
                        IDictionary<string, string> dict1 = StringToDictionary(dataExpression);

                        if (dict1.ContainsKey("parent"))
                        {
                            InsertDataSetFlag = 7;
                            int parentLevel = int.Parse(dict1["parent"]);
                            for (int i = 0; i < parentLevel; i++)
                            {
                                sourceTableName = dsImportDatas.Tables[sourceTableName].ParentRelations[0].ParentTable.TableName;
                            }
                            if (dict1.ContainsKey("column"))
                            {
                                InsertDataSetFlag = 8;
                                sourceColumnName = dict1["column"];
                                if (dict1.ContainsKey("where"))
                                {
                                    wherestr = dict1["where"];

                                }

                                GetOtherTableData(dsImportDatas, TABLE_NAME, DATA_COLUMN, sourceTableName, sourceColumnName, wherestr);
                            }
                        }
                        InsertDataSetFlag = 9;
                    }
                    else if (dataExpression.Contains("table="))
                    {
                        InsertDataSetFlag = 10;
                        IDictionary<string, string> dict2 = StringToDictionary(dataExpression);
                        if (dict2.ContainsKey("table") && dict2.ContainsKey("column"))
                        {
                            InsertDataSetFlag = 11;
                            sourceTableName = dict2["table"];

                            sourceColumnName = dict2["column"];
                            if (dict2.ContainsKey("where"))
                            {
                                wherestr = dict2["where"];
                            }
                            GetOtherTableData(dsImportDatas, TABLE_NAME, DATA_COLUMN, sourceTableName, sourceColumnName, wherestr);
                        }
                        InsertDataSetFlag = 12;
                    }
                    else if (dataExpression == "IDENTITY")
                    {
                        InsertDataSetFlag = 13;
                        tableKey = DATA_COLUMN;
                    }
                    else
                    {
                        InsertDataSetFlag = 14;
                        dsImportDatas.Tables[TABLE_NAME].Columns[DATA_COLUMN].Expression = columnInfor["DATA_Expression"].ToString().Replace("~", ",");
                    }
                }
            }
        }

        private IDictionary<string, string> WhereStringToDictionary(string query)
        {
            IDictionary<string, string> queryDict = new Dictionary<string, string>();
            string[] queryParams = query.Split('&');

            if (queryParams.Length > 0)
            {
                foreach (string queryParam in queryParams)
                {
                    string[] oneParam = queryParam.Split('=');
                    if (oneParam.Length >= 2)
                    {
                        queryDict.Add(oneParam[0].Trim(), oneParam[1].Trim());
                    }
                }
            }
            return queryDict;
        }

        private IDictionary<string, string> StringToDictionary(string query)
        {
            IDictionary<string, string> queryDict = new Dictionary<string, string>();
            string[] queryParams = query.Split(',');

            if (queryParams.Length > 0)
            {
                foreach (string queryParam in queryParams)
                {
                    string[] oneParam = queryParam.Split('=');
                    if (oneParam.Length >= 2)
                    {
                        queryDict.Add(oneParam[0].Trim(), oneParam[1].Trim());
                    }
                }
            }
            return queryDict;
        }

        public static double ComputeExpression(string sExpression)
        {
            DataTable eval = new DataTable();
            object result = eval.Compute(sExpression, "");
            return double.Parse(result.ToString());
        }
        /// <summary>
        /// 从其他表格中获取列数据
        /// </summary>
        /// <param name="ds">数据集</param>
        /// <param name="targetTableName">目标表名</param>
        /// <param name="targetColumnName">目标列名</param>
        /// <param name="sourceTableName">源表名</param>
        /// <param name="sourceColumnName">源列名</param>
        /// <param name="wherestr">条件字符串</param>
        private void GetOtherTableData(DataSet ds, string targetTableName, string targetColumnName, string sourceTableName, string sourceColumnName, string wherestr)
        {
            if (wherestr.Length > 0)
            {
                wherestr = " and " + wherestr.Replace("＝", "=");
                wherestr = wherestr.Replace("~", "'");
            }
            string RowNo = "-1";
            int i = 0;
            foreach (DataRow dr in ds.Tables[targetTableName].Rows)
            {
                string wherestrtemp = wherestr;
                if (wherestrtemp.Length > 0)
                {
                    //替换行号

                    wherestrtemp = wherestrtemp.Replace("{RowNo}", i.ToString());
                    wherestrtemp = wherestrtemp.Replace("and", "&");
                    IDictionary<string, string> dict1 = WhereStringToDictionary(wherestrtemp);
                    if (dict1.ContainsKey("RowNo"))
                    {
                        RowNo = ComputeExpression(dict1["RowNo"]).ToString();
                    }
                    wherestrtemp = wherestrtemp.Replace("&", "and");

                    //替换当前行的列数据
                    for (int j = 0; j < ds.Tables[targetTableName].Columns.Count; j++)
                    {
                        wherestrtemp = wherestrtemp.Replace("{#" + ds.Tables[targetTableName].Columns[j].ColumnName + '}', dr[ds.Tables[targetTableName].Columns[j].ColumnName].ToString());
                    }
                }

                if (ds.Tables[targetTableName].ParentRelations.Count != 0)
                {
                    string parentTableName = ds.Tables[targetTableName].ParentRelations[0].ParentTable.TableName;
                    string parentId = dr[parentTableName + "_id"].ToString();
                    bool isParent = true;

                    while (parentTableName != sourceTableName && isParent)
                    {
                        if (ds.Tables[parentTableName].ParentRelations.Count == 0)
                        {
                            isParent = false;
                        }
                        else
                        {
                            string directParent = parentTableName;
                            string directParentId = parentId;
                            parentTableName = ds.Tables[parentTableName].ParentRelations[0].ParentTable.TableName;
                            parentId = ds.Tables[directParent].Select(directParent + "_id=" + directParentId)[0][parentTableName + "_id"].ToString();
                        }
                    }

                    if (isParent)
                    {
                        string targetTableId = parentId;
                        if (RowNo != "-1")
                        {
                            dr[targetColumnName] = ds.Tables[sourceTableName].Rows[int.Parse(RowNo)][sourceColumnName];
                        }
                        else
                        {
                            DataRow[] drs = ds.Tables[sourceTableName].Select(sourceTableName + "_id=" + targetTableId + wherestrtemp);
                            if (drs.Length > 0)
                            {
                                dr[targetColumnName] = drs[0][sourceColumnName];
                            }
                        }
                    }
                    else
                    {
                        if (RowNo != "-1")
                        {
                            dr[targetColumnName] = ds.Tables[sourceTableName].Rows[int.Parse(RowNo)][sourceColumnName];
                        }
                        else
                        {
                            DataRow[] drs = ds.Tables[sourceTableName].Select("1=1 " + wherestrtemp);
                            if (drs.Length > 0)
                            {
                                dr[targetColumnName] = drs[0][sourceColumnName];
                            }
                        }
                    }
                }
                else
                {
                    if (RowNo != "-1")
                    {
                        dr[targetColumnName] = ds.Tables[sourceTableName].Rows[int.Parse(RowNo)][sourceColumnName];
                    }
                    else
                    {
                        DataRow[] drs = ds.Tables[sourceTableName].Select("1=1 " + wherestrtemp);
                        if (drs.Length > 0)
                        {
                            dr[targetColumnName] = drs[0][sourceColumnName];
                        }
                    }
                }
                i++;
            }


        }

        /// <summary>
        /// 导入数据表到数据库
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="dtImportDatas">数据集</param>
        /// <param name="tableKey">表主键名</param>
        /// <param name="GUID"></param>
        /// <param name="ownerId">货主ID</param>
        /// <param name="warehouseId">仓库ID</param>
        /// <param name="SELEDT_SQL"></param>
        /// <param name="ISREPEAT_SQL"></param>
        /// <param name="INSERT_SQL"></param>
        /// <param name="importColumnsInfo">导入列信息</param>
        /// <param name="TABLE_NAME">表名</param>
        /// <param name="SUR_TABLE_MasterKey"></param>
        /// <param name="TARGET_TABLE_MasterKey"></param>
        /// <param name="resultStr"></param>
        private void InsertDataTable(Dictionary<string, string> parameters, DataTable dtImportDatas, string tableKey, string GUID, string ownerId, string warehouseId, string SELEDT_SQL, string ISREPEAT_SQL, string INSERT_SQL, DataTable importColumnsInfo, string TABLE_NAME, string SUR_TABLE_MasterKey, string TARGET_TABLE_MasterKey, out string resultStr)
        {
            int InsertDataTableFlag = 0;
            resultStr = "";
            string repidlst = "";//重复数据单号
            string exceplst = "";//错误数据单号
            String rtn = null;
            int sums = 0;// 导入行数
            int success = 0;// 成功行数
            int cfs = 0;  //重复
            int cws = 0;  //错误
            int i_OrderCount = 0; //总共行数

            DataRow[] importDataRows = dtImportDatas.Select();

            SELEDT_SQL = common.ReplaceParameter(SELEDT_SQL, parameters);
            InsertDataTableFlag = 1;
            if (!string.IsNullOrEmpty(SELEDT_SQL))
            {
                importDataRows = dtImportDatas.Select(SELEDT_SQL);
            }
            i_OrderCount = importDataRows.Length;
            InsertDataTableFlag = 2;
            /////////////////////
            DB_sql dbsql = new DB_sql();

            InsertDataTableFlag = 3;
            //////////////
            int RowNum = 0;//记录循环到哪一行。
            string RowNumdec = "";//用于显示的错误行数

            try
            {
                InsertDataTableFlag = 4;
                //增加参数列
                AddColumnsInfos(dtImportDatas, importColumnsInfo);

                InsertDataTableFlag = 5;

                //Dictionary<string, SqlParam> parameterParams = new Dictionary<string, SqlParam>();
                //INSERT_SQL = common.ReplaceParameter(INSERT_SQL, parameters, parameterParams);
                //ISREPEAT_SQL = common.ReplaceParameter(ISREPEAT_SQL, parameters, parameterParams);

                INSERT_SQL = common.ReplaceParameter(INSERT_SQL, parameters);
                ISREPEAT_SQL = common.ReplaceParameter(ISREPEAT_SQL, parameters);

                Dictionary<string, SqlParam> masterParams = new Dictionary<string, SqlParam>();
                string masterPreStr = "?Master_";
                Dictionary<string, SqlParam> slaveParams = new Dictionary<string, SqlParam>();
                string slavePreStr = "?Slave_";

                ReplaceSqlWithParams(GUID, ownerId, warehouseId, ref ISREPEAT_SQL, ref INSERT_SQL, masterParams, masterPreStr, slaveParams, slavePreStr);
                GetParamsInfo(importColumnsInfo, slaveParams, slavePreStr);

                //在此处加入事务？
                InsertDataTableFlag = 6;
                foreach (DataRow importDataRow in importDataRows)
                {
                    RowNum++;
                    INSERT_SQL = INSERT_SQL.Replace("{RowNo}", RowNum.ToString());
                    ISREPEAT_SQL = ISREPEAT_SQL.Replace("{RowNo}", RowNum.ToString());
                    InsertDataTableFlag = 7;
                    #region 验证待导入数据、替换源数据
                    VerifyDatasAndSetIntoSQLs(ref ISREPEAT_SQL, ref INSERT_SQL, importColumnsInfo, importDataRow, slaveParams, slavePreStr);
                    List<MySqlParameter> allSqlParams = GetAllSqlParams(masterParams, slaveParams);
                    string SUR_TABLE_MasterKeyValue = "";
                    if (dtImportDatas.Columns.Contains(SUR_TABLE_MasterKey))
                    {
                        SUR_TABLE_MasterKeyValue = importDataRow[SUR_TABLE_MasterKey].ToString();
                    }
                    #endregion

                    #region 判断是否重复导入,是则跳出本次循环
                    InsertDataTableFlag = 8;
                    if (!string.IsNullOrEmpty(ISREPEAT_SQL))
                    {
                        InsertDataTableFlag = 9;
                        List<MySqlParameter> repeatSqlParams = GetSqlParams(ISREPEAT_SQL, allSqlParams);
                        DataTable dtisrep = DBAccess.GetDataTableByParam(ISREPEAT_SQL, repeatSqlParams.ToArray(), importConnStrName, DBType.MySql);

                        InsertDataTableFlag = 10;
                        if (dtisrep.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtisrep.Rows.Count; i++)
                            {
                                string result = "";
                                for (int j = 0; j < dtisrep.Columns.Count; j++)
                                {
                                    result += (result == "" ? "" : ",") + dtisrep.Columns[j].ColumnName + ":" + dtisrep.Rows[i][j];
                                }
                                repidlst += (repidlst == "" ? "" : ";") + result;
                            }
                            InsertDataTableFlag = 11;
                            //数据重复，或者不允许插入，写入日志
                            cfs++;
                            //ClsLog.WriteLog("GUID:" + GUID + ",SUR_TABLE_MasterKeyValue:" + SUR_TABLE_MasterKeyValue + ",TABLE_NAME:" + TABLE_NAME + ".数据重复，或者不允许插入!" + ISREPEAT_SQL, importConnStrName);
                            if (!dtImportDatas.Columns.Contains("isRepeatRow"))
                            {
                                dtImportDatas.Columns.Add("isRepeatRow", typeof(string));
                            }
                            importDataRow["isRepeatRow"] = "1";
                            resultStr = "数据重复，或者不允许插入：" + repidlst;
                            continue;
                        }
                    }
                    #endregion

                    try
                    {
                        InsertDataTableFlag = 12;
                        if (INSERT_SQL != string.Empty && !(dtImportDatas.Columns.Contains("isRepeatRow") && importDataRow["isRepeatRow"].ToString() == "1"))
                        {
                            //执行sql有错误，返回resultStr信息；成功则不返回。
                            INSERT_SQL = Regex.Replace(INSERT_SQL, @"\{\#\w+\}", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            InsertDataTableFlag = 13;
                            List<MySqlParameter> insertSqlParams = GetSqlParams(INSERT_SQL, allSqlParams);
                            DataTable dtResult = DBAccess.GetDataTableByParam(INSERT_SQL, insertSqlParams.ToArray(), importConnStrName, DBType.MySql);
                            InsertDataTableFlag = 14;
                            if (dtResult != null && dtResult.Rows.Count > 0)
                            {
                                if (dtResult.Columns.Contains("returnstr"))
                                {
                                    resultStr += dtResult.Rows[0]["returnstr"].ToString();

                                    ClsLog.WriteLog(DateTime.Now, "GUID:" + GUID + ",SUR_TABLE_MasterKeyValue:" + SUR_TABLE_MasterKeyValue + ",TABLE_NAME:" + TABLE_NAME + ",导入失败：" + resultStr + ",导入SQL：" + INSERT_SQL, importConnStrName);
                                }
                                if (dtResult.Columns.Contains("IDENTITY"))
                                {
                                    importDataRow[tableKey] = dtResult.Rows[0]["IDENTITY"].ToString();
                                }
                                if (dtResult.Columns.Contains("InGUID"))
                                {
                                    resultStr += "数据重复" + dtResult.Rows[0]["InGUID"].ToString();
                                }

                            }
                            else
                            {
                                success++;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        cws++;
                        RowNumdec = RowNumdec + "错误行数：" + RowNum.ToString() + ";";
                        if (!string.IsNullOrEmpty(SUR_TABLE_MasterKey) && importDataRow.Table.Columns.Contains(SUR_TABLE_MasterKey))
                        {
                            exceplst += (exceplst == "" ? "" : ",") + importDataRow[SUR_TABLE_MasterKey].ToString() + ",";
                        }
                        rtn = err.Message;
                        resultStr = err.Message;
                        ClsLog.WriteLog(DateTime.Now, "GUID:" + GUID + ",SUR_TABLE_MasterKeyValue:" + SUR_TABLE_MasterKeyValue + ",TABLE_NAME:" + TABLE_NAME + ",导入失败：" + resultStr, importConnStrName);
                        ClsLog.WriteLog(DateTime.Now, "GUID:" + GUID + ",SUR_TABLE_MasterKeyValue:" + SUR_TABLE_MasterKeyValue + ",TABLE_NAME:" + TABLE_NAME + ",导入SQL：" + INSERT_SQL, importConnStrName);

                        ClsLog.AppendDbLog("InsertDataTableFlag =" + InsertDataTableFlag + "," + err.Message.ToString(), "InsertDataTable", ErrorLevel.Error, ClsLog.ProjectName);
                    }
                }
            }
            catch (Exception ee)
            {
                ClsLog.AppendDbLog("InsertDataTableFlag =" + InsertDataTableFlag + "," + ee.Message.ToString(), "InsertDataTable", ErrorLevel.Error, ClsLog.ProjectName);
                resultStr = "出错了！" + ee.Message;
                ClsLog.WriteLog(DateTime.Now, "GUID:" + GUID + ",TABLE_NAME:" + TABLE_NAME + ",INSERT_SQL:" + INSERT_SQL + ee.Message.ToString(), importConnStrName);
            }

            StringBuilder r = new StringBuilder();

            r.Append("总共"); r.Append(i_OrderCount.ToString()); r.Append("条,\r\n");
            r.Append("导入"); r.Append(sums.ToString()); r.Append("条,\r\n");
            r.Append("成功"); r.Append(success.ToString()); r.Append("条,\r\n");
            r.Append("重复"); r.Append(cfs.ToString()); r.Append("条,\r\n");
            if (!string.IsNullOrEmpty(repidlst))
            {
                r.Append("重复数据:"); r.Append(repidlst.ToString()); r.Append("\r\n");
            }
            r.Append("错误"); r.Append(cws.ToString()); r.Append("条," + RowNumdec + "\r\n");
            if (!string.IsNullOrEmpty(exceplst))
            {
                r.Append("错误数据:"); r.Append(exceplst.ToString()); r.Append("\r\n");
            }
            if (!string.IsNullOrEmpty(rtn)) { r.Append("\r\n出错:" + rtn); }
        }

        /// <summary>
        /// 从配置获取参数类型和长度
        /// </summary>
        /// <param name="importColumnsInfo"></param>
        /// <param name="slaveParams"></param>
        private static void GetParamsInfo(DataTable importColumnsInfo, Dictionary<string, SqlParam> slaveParams, string preStr)
        {
            foreach (DataRow columnInfo in importColumnsInfo.Rows)
            {
                string columnName = preStr + columnInfo["DATA_COLUMN"].ToString();
                if (slaveParams.ContainsKey(columnName))
                {
                    if (columnInfo["DataType"] != null && !string.IsNullOrEmpty(columnInfo["DataType"].ToString()))
                    {
                        slaveParams[columnName].Type = StringToSqlDbType(columnInfo["DataType"].ToString());
                    }
                    if (columnInfo["Length"] != null && !string.IsNullOrEmpty(columnInfo["Length"].ToString()))
                    {
                        slaveParams[columnName].Length = int.Parse(columnInfo["Length"].ToString());
                    }
                }
            }
        }

        private static SqlDbType StringToSqlDbType(string type)
        {
            SqlDbType result = SqlDbType.VarChar;

            foreach (var item in Enum.GetValues(typeof(SqlDbType)))
            {
                if (type.ToLower().Equals(item.ToString().ToLower()))
                {
                    result = (SqlDbType)Enum.Parse(typeof(SqlDbType), item.ToString());
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// 将所有参数转换为SQL参数
        /// </summary>
        /// <param name="masterParams"></param>
        /// <param name="slaveParams"></param>
        /// <returns></returns>
        private static List<MySqlParameter> GetAllSqlParams(Dictionary<string, SqlParam> masterParams, Dictionary<string, SqlParam> slaveParams)
        {
            List<MySqlParameter> allSqlParams = new List<MySqlParameter>();
            foreach (var param in masterParams)
            {
                MySqlParameter sqlParam = new MySqlParameter(param.Key, ToMySqlDbType(param.Value.Type), param.Value.Length);
                sqlParam.Value = param.Value.Value != null ? param.Value.Value : (object)DBNull.Value;
                allSqlParams.Add(sqlParam);
            }
            foreach (var param in slaveParams)
            {
                MySqlParameter sqlParam = new MySqlParameter(param.Key, ToMySqlDbType(param.Value.Type), param.Value.Length);
                sqlParam.Value = param.Value.Value != null ? param.Value.Value : (object)DBNull.Value;
                allSqlParams.Add(sqlParam);
            }

            return allSqlParams;
        }

        private static MySqlDbType ToMySqlDbType(SqlDbType type)
        {
            MySqlDbType result = MySqlDbType.VarChar;
            switch (type)
            {
                case SqlDbType.VarChar:
                    result = MySqlDbType.VarChar;
                    break;
                case SqlDbType.BigInt:
                    result = MySqlDbType.Int64;
                    break;
                case SqlDbType.Binary:
                    result = MySqlDbType.Binary;
                    break;
                case SqlDbType.Bit:
                    result = MySqlDbType.Bit;
                    break;
                case SqlDbType.Char:
                    result = MySqlDbType.VarChar;
                    break;
                case SqlDbType.Date:
                    result = MySqlDbType.Date;
                    break;
                case SqlDbType.DateTime:
                    result = MySqlDbType.DateTime;
                    break;
                case SqlDbType.Decimal:
                    result = MySqlDbType.Decimal;
                    break;
                case SqlDbType.Float:
                    result = MySqlDbType.Float;
                    break;
                case SqlDbType.Int:
                    result = MySqlDbType.Int32;
                    break;
                default:
                    result = MySqlDbType.VarChar;
                    break;
            }
            return result;
        }

        /// <summary>
        /// 筛选出SQL语句中的参数
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="allSqlParams"></param>
        /// <returns></returns>
        private static List<MySqlParameter> GetSqlParams(string sqlStr, List<MySqlParameter> allSqlParams)
        {
            List<MySqlParameter> insertSqlParams = new List<MySqlParameter>();
            foreach (MySqlParameter param in allSqlParams)
            {
                if (sqlStr.Contains(param.ParameterName))
                {
                    MySqlParameter newParam = new MySqlParameter(param.ParameterName, param.MySqlDbType, param.Size);
                    newParam.Value = param.Value;
                    insertSqlParams.Add(newParam);
                }
            }

            return insertSqlParams;
        }

        /// <summary>
        /// 将SQL语句中的参数替换成参数形式
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="ownerId"></param>
        /// <param name="warehouseId"></param>
        /// <param name="ISREPEAT_SQL"></param>
        /// <param name="INSERT_SQL"></param>
        /// <param name="masterParams"></param>
        /// <param name="masterPreStr"></param>
        /// <param name="slaveParams"></param>
        /// <param name="slavePreStr"></param>
        private void ReplaceSqlWithParams(string GUID, string ownerId, string warehouseId, ref string ISREPEAT_SQL, ref string INSERT_SQL, Dictionary<string, SqlParam> masterParams, string masterPreStr, Dictionary<string, SqlParam> slaveParams, string slavePreStr)
        {
            string masterStartStr = "'{$";
            string masterEndStr = "$}'";
            string masterStartStr1 = "{$";
            string masterEndStr1 = "$}";
            ISREPEAT_SQL = SqlHelper.GetSqlParamsAndReplaceParams(ISREPEAT_SQL, masterPreStr, masterStartStr, masterEndStr, masterStartStr1, masterEndStr1, masterParams);
            INSERT_SQL = SqlHelper.GetSqlParamsAndReplaceParams(INSERT_SQL, masterPreStr, masterStartStr, masterEndStr, masterStartStr1, masterEndStr1, masterParams);

            string slaveStartStr = "'{#";
            string slaveEndStr = "}'";
            string slaveStartStr1 = "{#";
            string slaveEndStr1 = "}";
            ISREPEAT_SQL = SqlHelper.GetSqlParamsAndReplaceParams(ISREPEAT_SQL, slavePreStr, slaveStartStr, slaveEndStr, slaveStartStr1, slaveEndStr1, slaveParams);
            INSERT_SQL = SqlHelper.GetSqlParamsAndReplaceParams(INSERT_SQL, slavePreStr, slaveStartStr, slaveEndStr, slaveStartStr1, slaveEndStr1, slaveParams);

            if (slaveParams.ContainsKey(slavePreStr + "GUID"))
            {
                slaveParams[slavePreStr + "GUID"].Value = GUID;
            }
            if (masterParams.ContainsKey(masterPreStr + "UserName"))
            {
                masterParams[masterPreStr + "UserName"].Value = UserName;
            }
            if (slaveParams.ContainsKey(slavePreStr + "OwnerId"))
            {
                slaveParams[slavePreStr + "OwnerId"].Value = ownerId;
            }
            if (slaveParams.ContainsKey(slavePreStr + "WarehouseId"))
            {
                slaveParams[slavePreStr + "WarehouseId"].Value = warehouseId;
            }
        }

        /// <summary>
        /// 增加导入列信息
        /// </summary>
        /// <param name="dtImportDatas">待导入数据表</param>
        /// <param name="importColumnsInfo">导入列信息</param>
        private static void AddColumnsInfos(DataTable dtImportDatas, DataTable importColumnsInfo)
        {
            foreach (string column in dtImportDatas.Columns.Cast<DataColumn>().Select(x => x.ColumnName))
            {
                if (!importColumnsInfo.Columns.Contains(column))
                {
                    DataRow drAdd = importColumnsInfo.NewRow();
                    drAdd["DATA_COLUMN"] = column;
                    drAdd["DATA_TYPE"] = "string";
                    importColumnsInfo.Rows.Add(drAdd);
                }
            }
        }
        /// <summary>
        /// 验证数据并替换到SQL语句
        /// </summary>
        /// <param name="ISREPEAT_SQL"></param>
        /// <param name="INSERT_SQL"></param>
        /// <param name="importColumnsInfo">数据列信息</param>
        /// <param name="importDataRow">数据行</param>
        private static void VerifyDatasAndSetIntoSQLs(ref string ISREPEAT_SQL, ref string INSERT_SQL, DataTable importColumnsInfo, DataRow importDataRow, Dictionary<string, SqlParam> allSqlParams, string slavePreStr)
        {
            foreach (DataRow importColumnInfo in importColumnsInfo.Rows)
            {
                string importColumnName = importColumnInfo["DATA_COLUMN"].ToString();
                string importColumnType = importColumnInfo["DATA_TYPE"].ToString();  //数据验证类型
                string importParamName = slavePreStr + importColumnName;
                #region 数据验证
                switch (importColumnType)
                {
                    case "*"://必填
                        if (string.IsNullOrEmpty(importDataRow[importColumnName].ToString()))
                        {

                            throw new Exception("字段【" + importColumnName + "】不允许为空");
                        }
                        break;
                    case "d"://日期
                        if (string.IsNullOrEmpty(importDataRow[importColumnName].ToString()))
                        {
                            if (allSqlParams.ContainsKey(importParamName))
                            {
                                allSqlParams[importParamName].Value = null;
                            }
                        }
                        else
                        {
                            DateTime dtDate;
                            if (!DateTime.TryParse(importDataRow[importColumnName].ToString(), out dtDate))
                            {

                                throw new Exception("字段【" + importColumnName + "】不是正确的日期格式类型！");
                            }
                        }
                        break;
                    case "m"://手机
                        if (!System.Text.RegularExpressions.Regex.IsMatch(importDataRow[importColumnName].ToString(), @"^[1]+[3,5,8,7]+\d{9}"))
                        {

                            throw new Exception("字段【" + importColumnName + "】不是正确的手机号码");
                        }
                        break;
                    case "n"://数字

                        if (string.IsNullOrEmpty(importDataRow[importColumnName].ToString()))
                        {
                            if (allSqlParams.ContainsKey(importParamName))
                            {
                                allSqlParams[importParamName].Value = null;
                            }
                        }
                        else
                        {
                            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^(-?\d+)(\.\d+)?");
                            if (!reg1.IsMatch(importDataRow[importColumnName].ToString()))
                            {
                                throw new Exception("字段【" + importColumnName + "】不是正确的数字");
                            }
                        }
                        break;
                    case "idcard"://身份证
                        IDCardValidation card = new IDCardValidation();
                        if (!card.CheckIDCard(importDataRow[importColumnName].ToString()))
                        {

                            throw new Exception("字段【" + importColumnName + "】不是正确的身份证号码");
                        }
                        break;
                }
                #endregion
                #region 替换数据
                if (allSqlParams.ContainsKey(importParamName))
                {
                    allSqlParams[importParamName].Value = importDataRow[importColumnName] == DBNull.Value ? null : importDataRow[importColumnName].ToString();
                }
                #endregion
            }
        }
        #endregion
        #region 缓存导入设置

        private DataTable _Import_temp;
        protected DataTable Import_temp
        {
            get
            {
                if (_Import_temp == null)
                {
                    string sql = "SELECT [AutoID],[TemplateName],[methodname],[TemplateXml],[TemplateJson],[DataSourceType],[Split1],[Split2],[DataSourceSelect],[ToDSID],[ExecSql],[parentid],[LabelCode],[MasterSQL],[QuerySQL],[CompleteSQL],[XMLContent],[JSONContent],ErrorSQL,Parameters  FROM [Apo_temp]";
                    _Import_temp = DBAccess.GetDataTable(sql, 7);

                }
                return _Import_temp;
            }
        }

        /// <summary>
        /// datatable转化成xml
        /// </summary>
        private string ConvertDataTableToXML(DataTable xmlDS)
        {
            MemoryStream stream = null;
            XmlTextWriter writer = null;
            try
            {
                stream = new MemoryStream();
                writer = new XmlTextWriter(stream, Encoding.Default);
                xmlDS.WriteXml(writer);
                int count = (int)stream.Length;
                byte[] arr = new byte[count];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(arr, 0, count);
                UTF8Encoding utf = new UTF8Encoding();
                return utf.GetString(arr).Trim();
            }
            catch
            {
                return String.Empty;
            }
            finally
            {
                if (writer != null) writer.Close();
            }
        }

        /// <summary>
        /// 构造页面上的查询选择条件控件
        /// </summary>
        /// 
        private DataTable _Import_table;
        protected DataTable Import_table
        {
            get
            {
                if (_Import_table == null)
                {
                    _Import_table = DBAccess.GetDataTable("SELECT * FROM Apo_table where IS_Apo= 1", 7);
                }
                return _Import_table;
            }
        }

        private DataTable _Import_columns;
        protected DataTable Import_columns
        {
            get
            {
                if (_Import_columns == null)
                {
                    _Import_columns = DBAccess.GetDataTable("SELECT [COLUMN_ID],[TABLE_ID],[COLUMN_CODE],[DATA_COLUMN],[DATA_TYPE],[DATA_Expression],[DataType],[Length] FROM Apo_columns", 7);

                }
                return _Import_columns;
            }
        }
        #endregion
        #region 接口服务登陆资料缓存
        private DataTable _OAuth_APP;
        public DataTable OAuth_APP
        {
            get
            {
                if (_OAuth_APP != null)
                {
                    return _OAuth_APP;
                }
                else
                {
                    _OAuth_APP = DBAccess.GetDataTable("SELECT * FROM OAuth_APP", 7);
                }
                return _OAuth_APP;
            }
        }
        #endregion
    }
}
