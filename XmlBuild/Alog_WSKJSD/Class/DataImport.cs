using alogeip.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using ASPNetPortal;
using Alog.Common;

namespace Alog_WSKJSD
{
    public class DataImport
    {
        ClsLog log = new ClsLog();
        #region 导入数据库
        public void ImportFile(string datatype, string method, Dictionary<string, string> parameters, string newId, string ownerId, string warehouseId, out string resultStr, out int apiLogId)
        {
            apiLogId = 0;
            resultStr = string.Empty;
            string errorSql = string.Empty;
            int dataSourceId = 1;

            try
            {
                string inputDataStr = parameters["postdata"].ToString();

                ImportTemplateInfo template = GetApoTemplate(method);
                DataSet importDS = GetImportDS(inputDataStr, datatype, parameters);
                CopyTables(importDS, template);
                VerifyImportDatas(importDS, template);

                if (template != null && importDS != null)
                {
                    errorSql = template.ErrorSql;
                    dataSourceId = template.DataSourceId;
                    string tempGuid = newId;
                    resultStr = ImportDatasIntoDB(parameters, newId, ownerId, warehouseId, template, importDS, ref tempGuid);
                }
                else
                {
                    resultStr = ErrorCode.ERROR_CODE.S0003.ToString();
                }
            }
            catch (Exception ex)
            {
                string tempErrorSQL = errorSql;
                resultStr = "出错了！" + ex.Message.ToString();
                if (resultStr.Contains("错") || resultStr.Contains("败") || resultStr.Contains("异"))
                {
                    if (!string.IsNullOrEmpty(tempErrorSQL))   //若非空，做收尾处理，更新状态
                    {
                        tempErrorSQL = tempErrorSQL.Replace("~", "'");
                        tempErrorSQL = tempErrorSQL.Replace("{#GUID}", newId);
                        tempErrorSQL = common.ReplaceParameter(tempErrorSQL, parameters);

                        DBAccess.ExeuteSQL(tempErrorSQL, dataSourceId);
                    }
                }
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
                //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(ex), "ImportFile", ErrorLevel.Error, ClsLog.ProjectName);
            }
        }

        public void PreVerify(string datatype, string method, DataSet importDS, out string resultStr)
        {
            resultStr = string.Empty;

            try
            {
                ImportTemplateInfo template = GetApoTemplate(method);
                CopyTables(importDS, template);
                VerifyImportDatas(importDS, template);
                if (template != null && importDS != null)
                {
                    //resultStr = template.ErrorSql;
                }
                else
                {
                    resultStr = ErrorCode.ERROR_CODE.S0003.ToString();
                }
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
                //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(ex), "PreVerify", ErrorLevel.Error, ClsLog.ProjectName);
            }

        }


        private void CopyTables(DataSet ds, ImportTemplateInfo template)
        {
            foreach (var table in template.Tables)
            {
                if (!string.IsNullOrEmpty(table.AddTableNames))
                {
                    string tableName = table.SourceTableName;
                    List<string> addTables = table.AddTableNames.Split('+').ToList();
                    foreach (string addTable in addTables)
                    {
                        CopyTable(ds, tableName, addTable);
                    }
                }
            }
        }

        private void CopyTable(DataSet ds, string targetTable, string addTable)
        {
            if (ds.Tables.Contains(targetTable) && ds.Tables.Contains(addTable))
            {
                foreach (DataColumn column in ds.Tables[addTable].Columns)
                {
                    string newColumnName;
                    if (!ds.Tables[targetTable].Columns.Contains(column.ColumnName))
                    {
                        newColumnName = column.ColumnName;
                    }
                    else
                    {
                        newColumnName = addTable + "_" + column.ColumnName;
                    }
                    ds.Tables[targetTable].Columns.Add(newColumnName);

                    for (int i = 0; i < ds.Tables[addTable].Rows.Count; i++)
                    {
                        ds.Tables[targetTable].Rows[i][newColumnName] = ds.Tables[addTable].Rows[i][column.ColumnName];
                    }
                }
            }
        }

        private string ImportDatasIntoDB(Dictionary<string, string> parameters, string newId, string ownerId, string warehouseId, ImportTemplateInfo template, DataSet importDS, ref string inGuid)
        {
            string resultStr;
            string errorSql = template.ErrorSql;
            int dataSourceId = template.DataSourceId;
            string tempGuid = inGuid;
            InsertDataSet(parameters, importDS, template, newId, ownerId, warehouseId, out resultStr, ref tempGuid);

            if (!string.IsNullOrEmpty(tempGuid))
            {
                inGuid = tempGuid;
            }
            #region 导入完后运行的SQL
            if (resultStr.Contains("错") || resultStr.Contains("败") || resultStr.Contains("异"))
            {
                if (!string.IsNullOrEmpty(errorSql))
                {
                    errorSql = errorSql.Replace("~", "'");
                    errorSql = errorSql.Replace("{#GUID}", !string.IsNullOrEmpty(tempGuid) ? tempGuid : newId);
                    DBAccess.GetDataTable(errorSql, dataSourceId);
                }
            }
            else
            {
                string completeSql = template.CompleteSql;
                if (resultStr == string.Empty)
                {
                    if (!string.IsNullOrEmpty(completeSql))
                    {
                        try
                        {
                            completeSql = completeSql.Replace("~", "'");
                            completeSql = completeSql.Replace("{#GUID}", tempGuid)
                                                     .Replace("{#WarehouseId}", warehouseId)
                                                     .Replace("{#OwnerId}", ownerId);
                            completeSql = common.ReplaceParameter(completeSql, parameters);
                            DBAccess.ExeuteSQL(completeSql, dataSourceId);
                        }
                        catch (Exception ex)
                        {
                            if (HttpContext.Current != null && HttpContext.Current.Items != null)
                            {
                                HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S9999;
                            }
                            ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
                            //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(ex), "ImportFile",
                            //    ErrorLevel.Error, ClsLog.ProjectName);
                            throw ex;
                        }
                    }
                }
            }
            #endregion

            return resultStr;
        }

        private void VerifyImportDatas(DataSet importDS, ImportTemplateInfo template)
        {
            AddColumnsIfNotExist(importDS, template);
            BasicVerify(importDS, template);
            BusinessVerify(importDS, template);
        }

        private static void AddColumnsIfNotExist(DataSet importDS, ImportTemplateInfo template)
        {
            foreach (var table in template.Tables)
            {
                foreach (var column in table.Columns)
                {
                    if (importDS.Tables.Contains(table.SourceTableName) && !importDS.Tables[table.SourceTableName].Columns.Contains(column.SourceColumnName))
                    {
                        DataColumn dc = new DataColumn(column.SourceColumnName);
                        importDS.Tables[table.SourceTableName].Columns.Add(dc);
                    }
                }
            }
        }

        /// <summary>
        /// 业务逻辑验证
        /// </summary>
        /// <param name="importDS"></param>
        /// <param name="template"></param>
        private void BusinessVerify(DataSet ds, ImportTemplateInfo template)
        {
            foreach (var verifyInfo in template.VerifyInfos)
            {
                string expression = verifyInfo.Expression.Replace("~", "'");
                var verifyColumn = GetVerifyColumns(ds, expression);
                List<string> finalExps = GetExpressions(ds, expression, verifyColumn);

                foreach (string exp in finalExps)
                {
                    bool verify = bool.Parse(new DataTable().Compute("iif(" + exp + ",True,False)", "").ToString());
                    if (verify == false)
                    {
                        string errorStr = expression.Replace("{", "").Replace("}", "");
                        if (HttpContext.Current != null && HttpContext.Current.Items != null)
                        {
                            HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0009;
                            HttpContext.Current.Items["ERROR_MSG"] = "数据逻辑验证失败：" + errorStr;
                        }
                        throw new Exception("数据逻辑验证失败：" + errorStr);
                    }
                }
            }
        }

        /// <summary>
        /// 获取数据验证列信息
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private List<VerifyColumn> GetVerifyColumns(DataSet ds, string expression)
        {
            List<string> tableColumns = StringHelper.GetListBetweenStr(expression, "{", "]}");
            List<VerifyColumn> columnInfos = new List<VerifyColumn>();
            foreach (string item in tableColumns)
            {
                VerifyColumn column = new VerifyColumn()
                {
                    TableName = item.Split('[')[0],
                    ColumnName = item.Split('[')[1]
                };
                columnInfos.Add(column);
            }

            #region 计算数据在DataSet中的深度
            foreach (var item in columnInfos)
            {
                int depth = 0;
                string tableName = item.TableName;
                var parentCount = ds.Tables[tableName].ParentRelations.Count;
                while (parentCount > 0)
                {
                    depth++;
                    tableName = ds.Tables[tableName].ParentRelations[0].ParentTable.TableName;
                    parentCount = ds.Tables[tableName].ParentRelations.Count;
                }
                item.TableDepth = depth;
            }
            #endregion
            columnInfos = columnInfos.OrderBy(c => c.TableDepth).ToList();

            return columnInfos;
        }

        /// <summary>
        /// 替换列表达式，获得最终逻辑验证表达式
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="expression"></param>
        /// <param name="verifyColumns"></param>
        /// <returns></returns>
        private List<string> GetExpressions(DataSet ds, string expression, List<VerifyColumn> verifyColumns)
        {
            List<string> results = new List<string>();

            string subExp = StringHelper.GetBetweenStr(expression, "sum(", ")");
            int expCount = ds.Tables[verifyColumns[0].TableName].Rows.Count;
            for (int i = 0; i < expCount; i++)
            {
                string exp = expression;
                string finalSubExp = string.Empty;
                bool isFirstSubColumn = true;

                foreach (var item in verifyColumns)
                {
                    if (string.IsNullOrEmpty(subExp) || !subExp.Contains("{" + item.TableName + "[" + item.ColumnName + "]}"))
                    {
                        exp = exp.Replace("{" + item.TableName + "[" + item.ColumnName + "]}", ds.Tables[item.TableName].Rows[i][item.ColumnName].ToString());
                    }
                    else if (!string.IsNullOrEmpty(subExp) || subExp.Contains("{" + item.TableName + "[" + item.ColumnName + "]}"))
                    {
                        string parentIdName = ds.Tables[item.TableName].ParentRelations[0].ParentTable.TableName + "_Id";
                        List<DataRow> drs = ds.Tables[item.TableName].Rows.Cast<DataRow>().ToList().FindAll(r => int.Parse(r[parentIdName].ToString()) == i);
                        for (int j = 0; j < drs.Count; j++)
                        {
                            DataRow dr = drs[j];
                            if (isFirstSubColumn)
                            {
                                finalSubExp += subExp.Replace("}", "}_" + j.ToString()) + " + ";
                            }
                            finalSubExp = finalSubExp.Replace("{" + item.TableName + "[" + item.ColumnName + "]}_" + j.ToString(), dr[item.ColumnName].ToString());
                        }
                        if (isFirstSubColumn)
                        {
                            isFirstSubColumn = false;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(finalSubExp))
                {
                    finalSubExp = finalSubExp.Remove(finalSubExp.Length - 3);
                    exp = exp.Replace("sum(" + subExp + ")", finalSubExp);
                }
                results.Add(exp);
            }

            return results;
        }

        /// <summary>
        /// 基本验证，包括必填数据是否为空、数据类型是否正确等
        /// </summary>
        /// <param name="importDS"></param>
        /// <param name="template"></param>
        private void BasicVerify(DataSet importDS, ImportTemplateInfo template)
        {
            foreach (ImportTableInfo table in template.Tables)
            {
                if (importDS.Tables.Contains(table.SourceTableName))
                {
                    DataTable importDT = importDS.Tables[table.SourceTableName];
                    foreach (DataRow dr in importDT.Rows)
                    {
                        foreach (ImportColumnInfo column in table.Columns)
                        {
                            if (importDT.Columns.Contains(column.SourceColumnName))
                            {
                                List<string> limits = column.DataLimit.Split('|').ToList();
                                foreach (var limit in limits)
                                {
                                    VerifyData(dr[column.SourceColumnName].ToString(), column.SourceColumnName, limit);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="importData">导入数据</param>
        /// <param name="columnName">列名</param>
        /// <param name="dataLimit">数据限制</param>
        private void VerifyData(string importData, string columnName, string dataLimit)
        {
            switch (dataLimit)
            {
                case "*"://必填
                    if (string.IsNullOrEmpty(importData))
                    {
                        if (HttpContext.Current != null && HttpContext.Current.Items != null)
                        {
                            HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0009;
                            HttpContext.Current.Items["ERROR_MSG"] = "字段【" + columnName + "】不允许为空！";
                        }
                        throw new Exception("字段【" + columnName + "】不允许为空");
                    }
                    break;
                case "d"://日期
                    if (!string.IsNullOrEmpty(importData))
                    {
                        DateTime dtDate;
                        if (!DateTime.TryParse(importData, out dtDate))
                        {
                            if (HttpContext.Current != null && HttpContext.Current.Items != null)
                            {
                                HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0009;
                                HttpContext.Current.Items["ERROR_MSG"] = "字段【" + columnName + "】不是正确的日期格式类型！";
                            }
                            throw new Exception("字段【" + columnName + "】不是正确的日期格式类型！");
                        }
                    }
                    break;
                case "m"://手机
                    if (!Regex.IsMatch(importData, @"^[1]+[3,5,8,7]+\d{9}"))
                    {
                        if (HttpContext.Current != null && HttpContext.Current.Items != null)
                        {
                            HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0009;
                            HttpContext.Current.Items["ERROR_MSG"] = "字段【" + columnName + "】不是正确的手机号码";
                        }
                        throw new Exception("字段【" + columnName + "】不是正确的手机号码");
                    }
                    break;
                case "n"://数字
                    if (!string.IsNullOrEmpty(importData))
                    {
                        Regex reg1 = new Regex(@"^(-?\d+)(\.\d+)?");
                        if (!reg1.IsMatch(importData))
                        {
                            if (HttpContext.Current != null && HttpContext.Current.Items != null)
                            {
                                HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0009;
                                HttpContext.Current.Items["ERROR_MSG"] = "字段【" + columnName + "】不是正确的数字";
                            }
                            throw new Exception("字段【" + columnName + "】不是正确的数字");
                        }
                    }
                    break;
                case "idcard"://身份证
                    IDCardValidation card = new IDCardValidation();
                    if (!card.CheckIDCard(importData))
                    {
                        if (HttpContext.Current != null && HttpContext.Current.Items != null)
                        {
                            HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0009;
                            HttpContext.Current.Items["ERROR_MSG"] = "字段【" + columnName + "】不是正确的身份证号码";
                        }
                        throw new Exception("字段【" + columnName + "】不是正确的身份证号码");
                    }
                    break;
            }
        }

        private ImportTemplateInfo GetApoTemplate(string method)
        {
            ImportTemplateInfo template = null;

            DataRow drTemplate = null;
            DataRow[] drs = null;
            drs = Apo_temp.Select("methodname = '" + method + "'");
            if (drs.Length > 0)
            {
                drTemplate = drs[0];
            }

            if (drTemplate != null)
            {
                template = new ImportTemplateInfo()
                {
                    Id = Convert.ToInt64(drTemplate["AutoID"].ToString()),
                    DataSourceId = GetDBIntValue(drTemplate["ToDSID"]),
                    MethodName = GetDBStringValue(drTemplate["methodname"]),
                    TemplateName = GetDBStringValue(drTemplate["TemplateName"]),
                    MasterSql = GetDBStringValue(drTemplate["MasterSQL"]),
                    SlaveSql = GetDBStringValue(drTemplate["QuerySQL"]),
                    CompleteSql = GetDBStringValue(drTemplate["CompleteSQL"]),
                    ErrorSql = GetDBStringValue(drTemplate["ErrorSQL"]),
                    ExecSql = GetDBStringValue(drTemplate["ExecSql"])
                };
                string dataFormat = drTemplate["DataSourceType"].ToString();
                if (dataFormat.Equals(DataFormat.Xml.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    template.DataFormat = DataFormat.Xml;
                    template.RequestTemplate = GetDBStringValue(drTemplate["XMLContent"]);
                    template.ResponseTemplate = GetDBStringValue(drTemplate["TemplateXml"]);
                }
                else if (dataFormat.Equals(DataFormat.Json.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    template.DataFormat = DataFormat.Json;
                    template.RequestTemplate = GetDBStringValue(drTemplate["JSONContent"]);
                    template.ResponseTemplate = GetDBStringValue(drTemplate["TemplateJson"]);
                }

                GetApoTablesInfo(template);
                GetApoVerifyInfos(template);
            }

            return template;
        }

        private ImportTemplateInfo GetImportTemplate(int importId)
        {
            ImportTemplateInfo template = null;

            DataRow drTemplate = null;
            DataRow[] drs = null;
            drs = Import_temp.Select("AutoID = " + importId.ToString());
            if (drs.Length > 0)
            {
                drTemplate = drs[0];
            }

            if (drTemplate != null)
            {
                template = new ImportTemplateInfo()
                {
                    Id = Convert.ToInt64(drTemplate["AutoID"].ToString()),
                    DataSourceId = GetDBIntValue(drTemplate["ToDSID"]),
                    TemplateName = GetDBStringValue(drTemplate["TemplateName"]),
                    ExecSql = GetDBStringValue(drTemplate["ExecSql"])
                };
                string dataFormat = drTemplate["DataSourceType"].ToString();
                if (dataFormat.Equals(DataFormat.Xml.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    template.DataFormat = DataFormat.Xml;
                }

                GetImportTablesInfo(template);
            }

            return template;
        }

        private string GetDBStringValue(object obj)
        {
            return obj != DBNull.Value ? obj.ToString() : string.Empty;
        }

        private int GetDBIntValue(object obj)
        {
            return obj != DBNull.Value ? Convert.ToInt32(obj.ToString()) : 0;
        }

        private void GetApoTablesInfo(ImportTemplateInfo template)
        {
            DataRow[] drsImportTable = Apo_table.Select("TMPT_ID=" + template.Id.ToString(), "ORDER_NUM");
            template.Tables = new List<ImportTableInfo>();

            foreach (DataRow dr in drsImportTable)
            {
                ImportTableInfo table = new ImportTableInfo()
                {
                    Id = Convert.ToInt64(dr["TABLE_ID"].ToString()),
                    TableOrder = GetDBIntValue(dr["ORDER_NUM"]),
                    SourceTableName = GetDBStringValue(dr["TABLE_NAME"]),
                    AddTableNames = GetDBStringValue(dr["ADD_TABLES"]),
                    TargetTableName = GetDBStringValue(dr["TARGET_TABLE"]),
                    SourceTableKey = GetDBStringValue(dr["SUR_TABLE_MasterKey"]),
                    TargetTableKey = GetDBStringValue(dr["TARGET_TABLE_MasterKey"]),
                    InsertSql = GetDBStringValue(dr["INSERT_SQL"]),
                    CheckRepeatSql = GetDBStringValue(dr["ISREPEAT_SQL"]),
                    FilterSql = GetDBStringValue(dr["SELEDT_SQL"]),
                    BulkInsertSql = GetDBStringValue(dr["NewInsertSql"]),
                    InsertSqlNew = GetDBStringValue(dr["INSERT_SQL_New"]),
                    DataArea = GetDBStringValue(dr["DATA_AREA"]),
                    IsApo = GetDBIntValue(dr["IS_Apo"]),
                    StartRow = GetDBIntValue(dr["STAR_ROW"]),
                    TopColumnName = GetDBIntValue(dr["TOPCOLNAME"])
                };
                GetApoColumnsInfo(table);

                template.Tables.Add(table);
            }
        }

        private void GetImportTablesInfo(ImportTemplateInfo template)
        {
            DataRow[] drsImportTable = Import_table.Select("TMPT_ID=" + template.Id.ToString(), "ORDER_NUM");
            template.Tables = new List<ImportTableInfo>();

            foreach (DataRow dr in drsImportTable)
            {
                ImportTableInfo table = new ImportTableInfo()
                {
                    Id = Convert.ToInt64(dr["TABLE_ID"].ToString()),
                    TableOrder = GetDBIntValue(dr["ORDER_NUM"]),
                    SourceTableName = GetDBStringValue(dr["TABLE_NAME"]),
                    TargetTableName = GetDBStringValue(dr["TARGET_TABLE"]),
                    SourceTableKey = GetDBStringValue(dr["SUR_TABLE_MasterKey"]),
                    TargetTableKey = GetDBStringValue(dr["TARGET_TABLE_MasterKey"]),
                    InsertSql = GetDBStringValue(dr["INSERT_SQL"]),
                    CheckRepeatSql = GetDBStringValue(dr["ISREPEAT_SQL"]),
                    FilterSql = GetDBStringValue(dr["SELEDT_SQL"]),
                    BulkInsertSql = GetDBStringValue(dr["NewInsertSql"]),
                    InsertSqlNew = GetDBStringValue(dr["INSERT_SQL_New"]),
                    DataArea = GetDBStringValue(dr["DATA_AREA"]),
                    StartRow = GetDBIntValue(dr["STAR_ROW"]),
                    TopColumnName = GetDBIntValue(dr["TOPCOLNAME"])
                };
                GetImportColumnsInfo(table);

                template.Tables.Add(table);
            }
        }

        private void GetApoVerifyInfos(ImportTemplateInfo template)
        {
            DataRow[] drsVerify = ApoVerify.Select("TempId=" + template.Id.ToString());
            template.VerifyInfos = new List<ImportVerifyInfo>();

            foreach (DataRow dr in drsVerify)
            {
                ImportVerifyInfo verifyInfo = new ImportVerifyInfo()
                {
                    Id = Convert.ToInt64(dr["ID"].ToString()),
                    Expression = GetDBStringValue(dr["Expression"]),
                    Description = GetDBStringValue(dr["Description"])
                };
                template.VerifyInfos.Add(verifyInfo);
            }
        }

        /// <summary>
        /// 将报文转换为数据集
        /// </summary>
        /// <param name="postdata">报文字符串</param>
        /// <param name="dataFormat">报文类型（xml或json）</param>
        /// <param name="parameters">参数</param>
        /// <param name="resultStr">返回字符串</param>
        /// <returns></returns>
        public DataSet GetImportDS(string postdata, string dataFormat, Dictionary<string, string> parameters)
        {
            DataSet dsImportDatas = null;

            if (!string.IsNullOrEmpty(postdata))
            {
                dsImportDatas = new DataSet();

                string specialNodes = string.Empty;
                if (parameters.ContainsKey("specialNodes"))
                {
                    specialNodes = parameters["specialNodes"].ToString();
                }

                if (dataFormat.Equals("xml", StringComparison.OrdinalIgnoreCase))
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
                            throw ex;
                        }
                    }
                    #endregion
                }
                else if (dataFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    #region json导入
                    postdata = postdata.IndexOf("[") > -1 ? postdata : "{Table:[" + postdata + "]}";
                    try
                    {
                        //ds = JsonConvert.DeserializeObject<DataSet>(postdata, new DataSetConverter());
                        dsImportDatas = JsonToDS(postdata, specialNodes);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    #endregion
                }
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
            DataSet result = new DataSet();

            try
            {
                XmlDocument xmlDoc = null;
                try
                {
                    xmlDoc = JsonConvert.DeserializeXmlNode(jsonStr);
                }
                catch (Exception ex)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
                    //ClsLog.AppendDbLog("Json格式错误：" + ex.Message + "。JsonStr：" + jsonStr, "JsonToDS", ErrorLevel.Warning, ClsLog.ProjectName);
                }

                if (xmlDoc != null)
                {
                    if (!string.IsNullOrEmpty(specialNodes))
                    {
                        List<string> nodeList = specialNodes.Split(',').ToList();
                        foreach (var nodeName in nodeList)
                        {
                            XmlNodeList nodes = xmlDoc.SelectNodes("//" + nodeName);
                            foreach (XmlNode node in nodes)
                            {
                                if (node.HasChildNodes && node.FirstChild.HasChildNodes)
                                {
                                    string replaceStr = string.Empty;
                                    foreach (XmlNode childNode in node.ChildNodes)
                                    {
                                        replaceStr += childNode.Name + ":" + childNode.InnerXml + ",";
                                    }
                                    if (replaceStr != string.Empty)
                                    {
                                        replaceStr = replaceStr.Remove(replaceStr.Length - 1);
                                        node.InnerXml = replaceStr;
                                    }
                                }
                            }
                        }
                    }
                    result.ReadXml(new XmlNodeReader(xmlDoc));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        #endregion
        #region 导入数据集到数据库
        private void InsertDataSet(Dictionary<string, string> parameters, DataSet importDS, ImportTemplateInfo template, string newId, string ownerId, string warehouseId, out string resultStr, ref string inGuid)
        {
            resultStr = "";
            string handleStr = "";

            try
            {
                int dataCount = 0;
                foreach (var table in template.Tables)
                {
                    if (handleStr == string.Empty)
                    {
                        if (importDS.Tables.Contains(table.SourceTableName))
                        {
                            //XML导入数据开始
                            string tableName = table.SourceTableName;
                            table.FilterSql = table.FilterSql.Replace("~", "'");
                            table.CheckRepeatSql = table.CheckRepeatSql.Replace("~", "'");
                            table.InsertSql = table.InsertSql.Replace("~", "'");
                            table.BulkInsertSql = table.BulkInsertSql.Replace("~", "'");

                            if (dataCount == 0)
                            {
                                dataCount = importDS.Tables[tableName].Rows.Count;
                            }

                            //给源数据增加表达式列
                            AddRepeatRowInfo(importDS, table);
                            string tableKey;
                            AddColumnsFromExpression(importDS, out tableKey, table);
                            DataTable currentTable = importDS.Tables[tableName];
                            string tempGuid = inGuid;
                            if (!string.IsNullOrEmpty(table.BulkInsertSql))
                            {
                                InsertTable(currentTable, newId, table.FilterSql, table.CheckRepeatSql, table.BulkInsertSql, template.DataSourceId, table.TargetTableKey);
                            }
                            else
                            {
                                InsertDataTable(parameters, currentTable, tableKey, newId, ownerId, warehouseId, table, template.DataSourceId, out handleStr, ref tempGuid);
                            }
                            if (!string.IsNullOrEmpty(tempGuid))
                            {
                                inGuid = tempGuid;
                            }
                            if (handleStr.Contains("错") || handleStr.Contains("败") || handleStr.Contains("异"))
                            {
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
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
                //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(ex), "InsertDataSet", ErrorLevel.Error, ClsLog.ProjectName);
                resultStr = "系统异常，" + ex.Message;
            }
        }

        private void GetApoColumnsInfo(ImportTableInfo table)
        {
            DataRow[] ys_drs = Apo_columns.Select("TABLE_ID = " + table.Id.ToString(), "COLUMN_ID");
            table.Columns = new List<ImportColumnInfo>();

            foreach (DataRow dr in ys_drs)
            {
                ImportColumnInfo column = new ImportColumnInfo()
                {
                    Id = Convert.ToInt64(dr["COLUMN_ID"].ToString()),
                    SourceColumnName = GetDBStringValue(dr["DATA_COLUMN"]),
                    TargetColumnName = GetDBStringValue(dr["COLUMN_CODE"]),
                    DataLimit = GetDBStringValue(dr["DATA_TYPE"]),
                    DataExpression = GetDBStringValue(dr["DATA_Expression"]),
                    DataType = GetDBStringValue(dr["DataType"]),
                    DataLength = GetDBIntValue(dr["Length"])
                };
                table.Columns.Add(column);
            }
        }

        private void GetImportColumnsInfo(ImportTableInfo table)
        {
            DataRow[] ys_drs = Import_columns.Select("TABLE_ID = " + table.Id.ToString(), "COLUMN_ID");
            table.Columns = new List<ImportColumnInfo>();

            foreach (DataRow dr in ys_drs)
            {
                ImportColumnInfo column = new ImportColumnInfo()
                {
                    Id = Convert.ToInt64(dr["COLUMN_ID"].ToString()),
                    SourceColumnName = GetDBStringValue(dr["DATA_COLUMN"]),
                    TargetColumnName = GetDBStringValue(dr["COLUMN_CODE"]),
                    DataLimit = GetDBStringValue(dr["DATA_TYPE"]),
                    DataExpression = GetDBStringValue(dr["DATA_Expression"])
                };
                table.Columns.Add(column);
            }
        }

        /// <summary>
        /// 给列信息增加是否重复行信息
        /// </summary>
        /// <param name="dsImportDatas"></param>
        /// <param name="drImportTableInfo"></param>
        /// <param name="TABLE_NAME"></param>
        /// <param name="importColumnsInfo"></param>
        private static void AddRepeatRowInfo(DataSet dsImportDatas, ImportTableInfo table)
        {
            if (dsImportDatas.Tables[table.SourceTableName].ParentRelations.Count > 0)
            {
                string parentTableName = dsImportDatas.Tables[table.SourceTableName].ParentRelations[0].ParentTable.TableName;
                if (dsImportDatas.Tables[parentTableName].Columns.Contains("isRepeatRow"))
                {
                    ImportColumnInfo column = new ImportColumnInfo()
                    {
                        SourceColumnName = "isRepeatRow",
                        TargetColumnName = "isRepeatRow",
                        DataExpression = "parent=1,column=isRepeatRow"
                    };
                    table.Columns.Add(column);
                }
            }
        }

        /// <summary>
        /// 从表达式推导出列信息，添加到待导入数据
        /// </summary>
        /// <param name="dsImportDatas">待导入数据</param>
        /// <param name="InsertDataSetFlag"></param>
        /// <param name="tableKey">表主键名</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnsInfo">列信息</param>
        private void AddColumnsFromExpression(DataSet dsImportDatas, out string tableKey, ImportTableInfo table)
        {
            tableKey = string.Empty;

            string tableName = table.SourceTableName;

            foreach (ImportColumnInfo column in table.Columns)
            {
                string sourceColumn = column.SourceColumnName;
                if (!dsImportDatas.Tables[tableName].Columns.Contains(sourceColumn))
                {
                    dsImportDatas.Tables[tableName].Columns.Add(sourceColumn, typeof(string));
                }
                string dataExpression = column.DataExpression;
                if (dataExpression != "")
                {
                    string sourceColumnName = "";
                    string wherestr = "";
                    string sourceTableName = tableName;

                    if (dataExpression.Contains("parent="))
                    {
                        IDictionary<string, string> dict1 = StringToDictionary(dataExpression);

                        if (dict1.ContainsKey("parent"))
                        {
                            int parentLevel = int.Parse(dict1["parent"]);
                            for (int i = 0; i < parentLevel; i++)
                            {
                                sourceTableName = dsImportDatas.Tables[sourceTableName].ParentRelations[0].ParentTable.TableName;
                            }
                            if (dict1.ContainsKey("column"))
                            {
                                sourceColumnName = dict1["column"];
                                if (dict1.ContainsKey("where"))
                                {
                                    wherestr = dict1["where"];

                                }

                                GetOtherTableData(dsImportDatas, tableName, sourceColumn, sourceTableName, sourceColumnName, wherestr);
                            }
                        }
                    }
                    else if (dataExpression.Contains("table="))
                    {
                        IDictionary<string, string> dict2 = StringToDictionary(dataExpression);
                        if (dict2.ContainsKey("table") && dict2.ContainsKey("column"))
                        {
                            sourceTableName = dict2["table"];

                            sourceColumnName = dict2["column"];
                            if (dict2.ContainsKey("where"))
                            {
                                wherestr = dict2["where"];
                            }
                            GetOtherTableData(dsImportDatas, tableName, sourceColumn, sourceTableName, sourceColumnName, wherestr);
                        }
                    }
                    else if (dataExpression == "IDENTITY")
                    {
                        tableKey = sourceColumn;
                    }
                    else
                    {
                        dsImportDatas.Tables[tableName].Columns[sourceColumn].Expression = column.DataExpression.Replace("~", ",");
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
            if (ds.Tables[sourceTableName].Columns.Contains(sourceColumnName))
            {
                foreach (DataRow dr in ds.Tables[targetTableName].Rows)
                {
                    dr[targetColumnName] = string.Empty;
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
                            if (RowNo != "-1" && ds.Tables[sourceTableName].Columns.Contains(sourceColumnName))
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
                            if (RowNo != "-1" && ds.Tables[sourceTableName].Columns.Contains(sourceColumnName))
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
                        if (RowNo != "-1" && ds.Tables[sourceTableName].Columns.Contains(sourceColumnName))
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

        }

        /// <summary>
        /// 导入数据表到数据库
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <param name="importDT">数据集</param>
        /// <param name="tableKey">表主键名</param>
        /// <param name="guid"></param>
        /// <param name="ownerId">货主ID</param>
        /// <param name="warehouseId">仓库ID</param>
        /// <param name="filterSql"></param>
        /// <param name="isRepeatSql"></param>
        /// <param name="insertSql"></param>
        /// <param name="importColumnsInfo">导入列信息</param>
        /// <param name="tableName">表名</param>
        /// <param name="sourceTableKeyName"></param>
        /// <param name="targetTableKeyName"></param>
        /// <param name="resultStr"></param>
        private void InsertDataTable(Dictionary<string, string> parameters, DataTable importDT, string tableKey, string guid, string ownerId, string warehouseId, ImportTableInfo table, int dataSourceId, out string resultStr, ref string inGuid)
        {
            resultStr = "";
            string repeatOrderNos = "";//重复数据单号
            string errorOrderNos = "";//错误数据单号
            string rtn = null;
            int importRows = 0;// 导入行数
            int successRows = 0;// 成功行数
            int repeatCount = 0;  //重复
            int errorCount = 0;  //错误
            int totalCount = 0; //总共行数

            DataRow[] importDrs = importDT.Select();

            table.FilterSql = common.ReplaceParameter(table.FilterSql, parameters);
            if (!string.IsNullOrEmpty(table.FilterSql))
            {
                importDrs = importDT.Select(table.FilterSql);
            }
            totalCount = importDrs.Length;

            int RowNum = 0;//记录循环到哪一行。
            string RowNumdec = "";//用于显示的错误行数

            try
            {
                //增加参数列
                AddColumnsInfos(importDT, table);

                Dictionary<string, SqlParam> parameterParams = new Dictionary<string, SqlParam>();
                table.InsertSql = common.ReplaceParameter(table.InsertSql, parameters, parameterParams);
                table.CheckRepeatSql = common.ReplaceParameter(table.CheckRepeatSql, parameters, parameterParams);

                Dictionary<string, SqlParam> masterParams = new Dictionary<string, SqlParam>();
                string masterPreStr = "@Master_";
                Dictionary<string, SqlParam> slaveParams = new Dictionary<string, SqlParam>();
                string slavePreStr = "@Slave_";

                string appKey = parameters.ContainsKey("appkey") ? parameters["appkey"].ToString() : string.Empty;
                ReplaceSqlWithParams(guid, ownerId, warehouseId, appKey, table, masterParams, masterPreStr, slaveParams, slavePreStr);
                GetParamsInfo(table, slaveParams, slavePreStr);

                foreach (DataRow importDataRow in importDrs)
                {
                    RowNum++;
                    string tempInsertSql = table.InsertSql.Replace("{RowNo}", RowNum.ToString());
                    string tempCheckRepeatSql = table.CheckRepeatSql.Replace("{RowNo}", RowNum.ToString());
                    #region 替换源数据
                    SetSqlParamsValue(table, importDataRow, slaveParams, slavePreStr);
                    List<SqlParameter> allSqlParams = GetAllSqlParams(masterParams, slaveParams);
                    string sourceTableKeyName = "";
                    if (importDT.Columns.Contains(table.SourceTableKey))
                    {
                        sourceTableKeyName = importDataRow[table.SourceTableKey].ToString();
                    }
                    #endregion

                    #region 判断是否重复导入,是则跳出本次循环
                    if (!string.IsNullOrEmpty(tempCheckRepeatSql))
                    {
                        List<SqlParameter> repeatSqlParams = GetSqlParams(tempCheckRepeatSql, allSqlParams);
                        DataTable dtRepeatDatas = DBAccess.GetDataTableByParam(tempCheckRepeatSql, repeatSqlParams.ToArray(), dataSourceId);

                        if (dtRepeatDatas.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtRepeatDatas.Rows.Count; i++)
                            {
                                string result = "";
                                for (int j = 0; j < dtRepeatDatas.Columns.Count; j++)
                                {
                                    result += (result == "" ? "" : ",") + dtRepeatDatas.Columns[j].ColumnName + ":" + dtRepeatDatas.Rows[i][j];
                                }
                                repeatOrderNos += (repeatOrderNos == "" ? "" : ";") + result;
                            }
                            //数据重复，或者不允许插入，写入日志
                            repeatCount++;
                            log.InsertImportLog(guid, sourceTableKeyName, table.SourceTableName, 0, "数据重复，或者不允许插入!" + tempCheckRepeatSql, dataSourceId);
                            if (!importDT.Columns.Contains("isRepeatRow"))
                            {
                                importDT.Columns.Add("isRepeatRow", typeof(string));
                            }
                            importDataRow["isRepeatRow"] = "1";
                            resultStr = "数据重复，或者不允许插入：" + repeatOrderNos;
                            continue;
                        }
                    }
                    #endregion

                    try
                    {
                        bool isRepeatRow = importDT.Columns.Contains("isRepeatRow") && importDataRow["isRepeatRow"].ToString() == "1";
                        if (tempInsertSql != string.Empty && !isRepeatRow)
                        {
                            tempInsertSql = Regex.Replace(tempInsertSql, @"\{\#\w+\}", "", RegexOptions.IgnoreCase);
                            List<SqlParameter> insertSqlParams = GetSqlParams(tempInsertSql, allSqlParams);
                            DataTable dtResult = DBAccess.GetDataTableByParam(tempInsertSql, insertSqlParams.ToArray(), dataSourceId);
                            if (dtResult != null && dtResult.Rows.Count > 0)
                            {
                                if (dtResult.Columns.Contains("returnstr"))
                                {
                                    resultStr += dtResult.Rows[0]["returnstr"].ToString();
                                }
                                if (dtResult.Columns.Contains("IDENTITY"))
                                {
                                    importDataRow[tableKey] = dtResult.Rows[0]["IDENTITY"].ToString();
                                }
                                if (dtResult.Columns.Contains("InGUID"))
                                {
                                    inGuid = dtResult.Rows[0]["InGUID"].ToString();
                                    resultStr += "数据重复" + dtResult.Rows[0]["InGUID"].ToString();
                                }

                                log.InsertImportLog(guid, sourceTableKeyName, table.SourceTableName, 0, "导入失败：" + resultStr, dataSourceId);
                                log.InsertImportLog(guid, sourceTableKeyName, table.SourceTableName, 0, tempInsertSql, dataSourceId);

                            }
                            else
                            {
                                successRows++;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        errorCount++;
                        RowNumdec = RowNumdec + "错误行数：" + RowNum.ToString() + ";";
                        if (!string.IsNullOrEmpty(table.SourceTableKey) && importDataRow.Table.Columns.Contains(table.SourceTableKey))
                        {
                            errorOrderNos += (errorOrderNos == "" ? "" : ",") + importDataRow[table.SourceTableKey].ToString() + ",";
                        }
                        rtn = err.Message;
                        resultStr = "出错了！" + err.Message;

                        log.InsertImportLog(guid, sourceTableKeyName, table.SourceTableName, 0, "导入失败：" + resultStr, dataSourceId);
                        log.InsertImportLog(guid, sourceTableKeyName, table.SourceTableName, 0, tempInsertSql, dataSourceId);
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + err.Message, "服务日志");
                        //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(err), "InsertDataTable", ErrorLevel.Error, ClsLog.ProjectName);
                    }
                }
            }
            catch (Exception ee)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ee.Message, "服务日志");
                //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(ee), "InsertDataTable", ErrorLevel.Error, ClsLog.ProjectName);
                resultStr = "出错了！" + ee.Message;
                log.InsertImportLog(guid, "", table.SourceTableName, 0, "INSERT_SQL:" + table.InsertSql + ee.Message.ToString(), dataSourceId);
            }

            StringBuilder r = new StringBuilder();

            r.Append("总共"); r.Append(totalCount.ToString()); r.Append("条,\r\n");
            r.Append("导入"); r.Append(importRows.ToString()); r.Append("条,\r\n");
            r.Append("成功"); r.Append(successRows.ToString()); r.Append("条,\r\n");
            r.Append("重复"); r.Append(repeatCount.ToString()); r.Append("条,\r\n");
            if (!string.IsNullOrEmpty(repeatOrderNos))
            {
                r.Append("重复数据:"); r.Append(repeatOrderNos.ToString()); r.Append("\r\n");
            }
            r.Append("错误"); r.Append(errorCount.ToString()); r.Append("条," + RowNumdec + "\r\n");
            if (!string.IsNullOrEmpty(errorOrderNos))
            {
                r.Append("错误数据:"); r.Append(errorOrderNos.ToString()); r.Append("\r\n");
            }
            if (!string.IsNullOrEmpty(rtn)) { r.Append("\r\n出错:" + rtn); }
        }

        /// <summary>
        /// 从配置获取参数类型和长度
        /// </summary>
        /// <param name="importColumnsInfo"></param>
        /// <param name="slaveParams"></param>
        private static void GetParamsInfo(ImportTableInfo table, Dictionary<string, SqlParam> slaveParams, string preStr)
        {
            foreach (ImportColumnInfo column in table.Columns)
            {
                string columnName = preStr + column.SourceColumnName;
                if (slaveParams.ContainsKey(columnName))
                {
                    if (!string.IsNullOrEmpty(column.DataType))
                    {
                        slaveParams[columnName].Type = StringToSqlDbType(column.DataType);
                    }
                    if (column.DataLength > 0)
                    {
                        slaveParams[columnName].Length = column.DataLength;
                    }
                }
            }
        }

        private static SqlDbType StringToSqlDbType(string type)
        {
            SqlDbType result = SqlDbType.VarChar;

            foreach (var item in Enum.GetValues(typeof(SqlDbType)))
            {
                if (type.Trim().Equals(item.ToString(), StringComparison.OrdinalIgnoreCase))
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
        private static List<SqlParameter> GetAllSqlParams(Dictionary<string, SqlParam> masterParams, Dictionary<string, SqlParam> slaveParams)
        {
            List<SqlParameter> allSqlParams = new List<SqlParameter>();
            foreach (var param in masterParams)
            {
                SqlParameter sqlParam = new SqlParameter(param.Key, param.Value.Type, param.Value.Length);
                sqlParam.Value = param.Value.Value != null ? param.Value.Value : (object)DBNull.Value;
                allSqlParams.Add(sqlParam);
            }
            foreach (var param in slaveParams)
            {
                SqlParameter sqlParam = new SqlParameter(param.Key, param.Value.Type, param.Value.Length);
                sqlParam.Value = param.Value.Value != null ? param.Value.Value : (object)DBNull.Value;
                allSqlParams.Add(sqlParam);
            }

            return allSqlParams;
        }

        /// <summary>
        /// 筛选出SQL语句中的参数
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <param name="allSqlParams"></param>
        /// <returns></returns>
        private static List<SqlParameter> GetSqlParams(string sqlStr, List<SqlParameter> allSqlParams)
        {
            List<SqlParameter> insertSqlParams = new List<SqlParameter>();
            foreach (SqlParameter param in allSqlParams)
            {
                if (sqlStr.Contains(param.ParameterName))
                {
                    SqlParameter newParam = new SqlParameter(param.ParameterName, param.SqlDbType, param.Size);
                    newParam.Value = param.Value;
                    insertSqlParams.Add(newParam);
                }
            }

            return insertSqlParams;
        }

        /// <summary>
        /// 将SQL语句中的参数替换成参数形式
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="ownerId"></param>
        /// <param name="warehouseId"></param>
        /// <param name="ISREPEAT_SQL"></param>
        /// <param name="INSERT_SQL"></param>
        /// <param name="masterParams"></param>
        /// <param name="masterPreStr"></param>
        /// <param name="slaveParams"></param>
        /// <param name="slavePreStr"></param>
        private void ReplaceSqlWithParams(string guid, string ownerId, string warehouseId, string appKey, ImportTableInfo table, Dictionary<string, SqlParam> masterParams, string masterPreStr, Dictionary<string, SqlParam> slaveParams, string slavePreStr)
        {
            string masterStartStr = "'{$";
            string masterEndStr = "$}'";
            string masterStartStr1 = "{$";
            string masterEndStr1 = "$}";
            table.CheckRepeatSql = SqlHelper.GetSqlParamsAndReplaceParams(table.CheckRepeatSql, masterPreStr, masterStartStr, masterEndStr, masterStartStr1, masterEndStr1, masterParams);
            table.InsertSql = SqlHelper.GetSqlParamsAndReplaceParams(table.InsertSql, masterPreStr, masterStartStr, masterEndStr, masterStartStr1, masterEndStr1, masterParams);

            string slaveStartStr = "'{#";
            string slaveEndStr = "}'";
            string slaveStartStr1 = "{#";
            string slaveEndStr1 = "}";
            table.CheckRepeatSql = SqlHelper.GetSqlParamsAndReplaceParams(table.CheckRepeatSql, slavePreStr, slaveStartStr, slaveEndStr, slaveStartStr1, slaveEndStr1, slaveParams);
            table.InsertSql = SqlHelper.GetSqlParamsAndReplaceParams(table.InsertSql, slavePreStr, slaveStartStr, slaveEndStr, slaveStartStr1, slaveEndStr1, slaveParams);

            if (slaveParams.ContainsKey(slavePreStr + "GUID"))
            {
                slaveParams.Add(slavePreStr + "sysGUID", slaveParams[slavePreStr + "GUID"]);
                slaveParams.Remove(slavePreStr + "GUID");
                slaveParams[slavePreStr + "sysGUID"].Value = guid;
                table.InsertSql = table.InsertSql.Replace(slavePreStr + "GUID", slavePreStr + "sysGUID");
                table.CheckRepeatSql = table.CheckRepeatSql.Replace(slavePreStr + "GUID", slavePreStr + "sysGUID");
            }
            if (masterParams.ContainsKey(masterPreStr + "UserName") && HttpContext.Current != null && HttpContext.Current.Items != null && HttpContext.Current.Items.Contains("UserName"))
            {
                masterParams[masterPreStr + "UserName"].Value = HttpContext.Current.Items["UserName"].ToString();
            }
            if (slaveParams.ContainsKey(slavePreStr + "OwnerId") && !string.IsNullOrEmpty(ownerId))
            {
                slaveParams[slavePreStr + "OwnerId"].Value = ownerId;
            }
            if (slaveParams.ContainsKey(slavePreStr + "WarehouseId") && !string.IsNullOrEmpty(warehouseId))
            {
                slaveParams[slavePreStr + "WarehouseId"].Value = warehouseId;
            }
            if (masterParams.ContainsKey(masterPreStr + "appKey") && !string.IsNullOrEmpty(appKey))
            {
                masterParams[masterPreStr + "UserName"].Value = appKey;
            }
        }

        /// <summary>
        /// 增加导入列信息
        /// </summary>
        /// <param name="dtImportDatas">待导入数据表</param>
        /// <param name="importColumnsInfo">导入列信息</param>
        private static void AddColumnsInfos(DataTable dtImportDatas, ImportTableInfo table)
        {
            foreach (string columnName in dtImportDatas.Columns.Cast<DataColumn>().Select(x => x.ColumnName))
            {
                if (table.Columns.Find(c => c.SourceColumnName == columnName) == null)
                {
                    ImportColumnInfo column = new ImportColumnInfo()
                    {
                        SourceColumnName = columnName,
                        DataType = "string"
                    };
                    table.Columns.Add(column);
                }
            }
        }
        /// <summary>
        /// 验证数据并替换到SQL语句
        /// </summary>
        /// <param name="importColumnsInfo">数据列信息</param>
        /// <param name="importDataRow">数据行</param>
        private static void SetSqlParamsValue(ImportTableInfo table, DataRow importDataRow, Dictionary<string, SqlParam> allSqlParams, string slavePreStr)
        {
            foreach (ImportColumnInfo column in table.Columns)
            {
                string importColumnName = column.SourceColumnName;
                string importParamName = slavePreStr + importColumnName;

                if (string.IsNullOrEmpty(importDataRow[importColumnName].ToString()) && allSqlParams.ContainsKey(importParamName))
                {
                    allSqlParams[importParamName].Value = null;
                }
                #region 替换数据
                if (allSqlParams.ContainsKey(importParamName))
                {
                    allSqlParams[importParamName].Value = importDataRow[importColumnName] == DBNull.Value ? null : importDataRow[importColumnName].ToString().Replace("~", "‘");
                }
                #endregion
            }
        }
        #endregion
        #region 批量导入数据
        /// <summary>
        /// 批量导入数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="insertSql"></param>
        private void InsertTable(DataTable indt, string GUID, string SELEDT_SQL, string ISREPEAT_SQL, string insertSql, int dsId, string TARGET_TABLE_MasterKey)
        {
            string Listrep = "";
            int i_OrderCount = 0; //总共行数
            int sums = 0;// 导入行数
            int chenggong = 0;// 成功行数
            int cfs = 0;  //重复
            i_OrderCount = indt.Rows.Count;

            if (!indt.Columns.Contains("GUID"))
            {
                indt.Columns.Add(new DataColumn("GUID"));
            }
            foreach (DataRow row in indt.Rows)
            {
                row["GUID"] = GUID;
            }

            //筛选数据
            DataTable dt = indt.Clone();
            if (!string.IsNullOrEmpty(SELEDT_SQL))
            {
                DataRow[] AllDr = indt.Select();
                AllDr = indt.Select(SELEDT_SQL);

                foreach (DataRow dr in AllDr)
                {
                    dt.ImportRow(dr);
                }
            }
            else
            {
                dt = indt.Copy();
            }



            if (!string.IsNullOrEmpty(insertSql))
            {
                DataRow[] itemRow = DBAccess.DataSource.Select("dsid=" + dsId);
                string strConn = DBAccess.DesDecrypt(itemRow[0]["ConnectionString"].ToString());
                SqlConnection conn = new SqlConnection(strConn);
                try
                {
                    string tbName = "#tempTable" + DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Millisecond;
                    conn.Open();
                    //创建临时表
                    string createSql = "create table [" + tbName + "] (";
                    foreach (DataColumn item in dt.Columns)
                    {
                        createSql += "[" + item.ColumnName + "] varchar(1000),";
                    }
                    createSql = createSql.Remove(createSql.Length - 1, 1);
                    createSql += ")";
                    try
                    {
                        SqlCommand cmd = new SqlCommand(createSql, conn);
                        int rows = cmd.ExecuteNonQuery();
                    }
                    catch (Exception e3)
                    {
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + e3.Message, "服务日志");
                        string errstr3 = e3.Message.ToString().Replace("'", "‘") + "。SQL:" + createSql;
                        //Alog.Pubs.errLog.WriteLog(errstr3);
                    }
                    //批量导入临时表数据
                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 10000;
                        sqlBC.BulkCopyTimeout = 60;
                        sqlBC.NotifyAfter = 15000;
                        sqlBC.DestinationTableName = tbName;
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sqlBC.ColumnMappings.Add(dt.Columns[i].ColumnName.ToString(), dt.Columns[i].ColumnName.ToString());
                        }
                        sqlBC.WriteToServer(dt);
                    }
                    if (!string.IsNullOrEmpty(ISREPEAT_SQL))
                    {

                        SqlCommand ISREPEATcmd = new SqlCommand(ISREPEAT_SQL.Replace("#tempTable", tbName), conn);
                        SqlDataAdapter sdr = new SqlDataAdapter(ISREPEATcmd);
                        DataSet ds = new DataSet();
                        sdr.Fill(ds);
                        DataTable dtisrep = ds.Tables[0];
                        try
                        {
                            if (dtisrep.Columns.Contains(TARGET_TABLE_MasterKey))
                            {
                                for (int i = 0; i < dtisrep.Rows.Count; i++)
                                {
                                    Listrep = Listrep + dtisrep.Rows[i][TARGET_TABLE_MasterKey].ToString() + ",";
                                }
                                cfs = dtisrep.Rows.Count;
                            }
                            else
                            {
                                cfs = int.Parse(dtisrep.Rows[0][0].ToString());
                            }


                        }
                        catch (Exception e1)
                        {
                            ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + e1.Message, "服务日志");
                            string errstr = e1.Message.ToString().Replace("'", "‘") + "。SQL:" + ISREPEAT_SQL;
                            //Alog.Pubs.errLog.WriteLog(errstr);
                        }
                    }
                    //导入正式数据
                    SqlCommand cmd1 = new SqlCommand(insertSql.Replace("#tempTable", tbName), conn);
                    chenggong = cmd1.ExecuteNonQuery();
                    // Page.ClientScript.RegisterStartupScript(GetType(), "", "alert('成功导入" + chenggong + "条数据!')", true);
                }
                catch (Exception ee)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ee.Message, "服务日志");
                    string ClientScript1 = ee.Message.ToString().Replace("'", "‘") + "。SQL:" + insertSql;
                    //Alog.Pubs.errLog.WriteLog(ClientScript1);
                }
                finally
                {
                    conn.Close();
                }
            }

            if (!string.IsNullOrEmpty(Listrep))
            {
                Listrep = Listrep.Substring(0, Listrep.Length - 1);
            }
            StringBuilder r = new StringBuilder();
            r.Append("总共"); r.Append(i_OrderCount.ToString()); r.Append("条,\r\n");
            r.Append("导入"); r.Append(sums.ToString()); r.Append("条,\r\n");
            r.Append("成功"); r.Append(chenggong.ToString()); r.Append("条,\r\n");
            r.Append("重复"); r.Append(cfs.ToString()); r.Append("条,\r\n");
            if (!string.IsNullOrEmpty(Listrep))
            {
                r.Append("重复数据:"); r.Append(Listrep); r.Append("\r\n");
            }


        }
        #endregion
        #region 缓存导入设置

        private static DataTable _Apo_temp;
        protected DataTable Apo_temp
        {
            get
            {
                if (_Apo_temp == null)
                {
                    string sql = "SELECT [AutoID],[TemplateName],[methodname],[TemplateXml],[TemplateJson],[DataSourceType],[Split1],[Split2],[DataSourceSelect],[ToDSID],[ExecSql],[parentid],[LabelCode],[MasterSQL],[QuerySQL],[CompleteSQL],[XMLContent],[JSONContent],ErrorSQL  FROM [Apo_temp]";
                    _Apo_temp = DBAccess.GetDataTable(sql, 7);

                }
                return _Apo_temp;
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
        private static DataTable _Apo_table;
        protected DataTable Apo_table
        {
            get
            {
                if (_Apo_table == null)
                {
                    //                    string sql = @"SELECT [TABLE_ID],[TMPT_ID],[TABLE_NAME],[DATA_AREA],[TARGET_TABLE]
                    //,[TARGET_TABLE_MasterKey],[SUR_TABLE_MasterKey],[SELEDT_SQL]
                    //,[ISREPEAT_SQL],[NewInsertSql],[INSERT_SQL],[IS_Apo],[STAR_ROW]
                    //,[TOPCOLNAME],[ORDER_NUM],[INSERT_SQL_New] FROM Apo_table where IS_Apo= 1";
                    _Apo_table = DBAccess.GetDataTable("SELECT * FROM Apo_table where IS_Apo= 1", 7);
                }
                return _Apo_table;
            }
        }

        /// <summary>
        /// 数据业务逻辑验证表达式
        /// </summary>
        private static DataTable _ApoVerify;
        protected DataTable ApoVerify
        {
            get
            {
                if (_ApoVerify == null)
                {
                    _ApoVerify = DBAccess.GetDataTable(@"SELECT  [ID], [TempId], [Expression], [Description]
FROM    [dbo].[Apo_verify]", 7);
                }
                return _ApoVerify;
            }
        }

        private static DataTable _Apo_columns;
        protected DataTable Apo_columns
        {
            get
            {
                if (_Apo_columns == null)
                {
                    _Apo_columns = DBAccess.GetDataTable("SELECT [COLUMN_ID],[TABLE_ID],[COLUMN_CODE],[DATA_COLUMN],[DATA_TYPE],[DATA_Expression],[DataType],[Length] FROM Apo_columns", 7);

                }
                return _Apo_columns;
            }
        }

        private static DataTable _ImportRco2Tmp;
        protected DataTable ImportRco2Tmp
        {
            get
            {
                if (_Apo_temp == null)
                {
                    string RcoID = System.Configuration.ConfigurationManager.AppSettings["RcoID"];
                    string sql = "SELECT [RcoID],[ImpID],[MessageType]  FROM [ImportRco2Tmp] where [RcoID] = " + RcoID;
                    _ImportRco2Tmp = DBAccess.GetDataTable(sql, 7);
                }
                return _ImportRco2Tmp;
            }
        }

        private static DataTable _Import_temp;
        protected DataTable Import_temp
        {
            get
            {
                if (_Import_temp == null)
                {
                    string RcoID = System.Configuration.ConfigurationManager.AppSettings["RcoID"];
                    string sql = @"SELECT [AutoID],[TemplateName],[Templatefile]
                    ,[DataSourceType],[Split1],[Split2],[DataSourceSelect],[ToDSID],[ExecSql]
                    FROM  [Import_temp] join ImportRco2Tmp on [Import_temp].AutoID = ImportRco2Tmp.ImpID and ImportRco2Tmp.RcoID=" + RcoID;
                    _Import_temp = DBAccess.GetDataTable(sql, 7);
                }
                return _Import_temp;
            }
        }

        private static DataTable _Import_table;
        protected DataTable Import_table
        {
            get
            {
                if (_Import_table != null)
                {
                    return _Import_table;
                }
                else
                {
                    _Import_table = DBAccess.GetDataTable("SELECT * FROM Import_table where IS_IMPORT= 1", 7);
                    //HttpRuntime.Cache["Import_table"] = _Import_table;
                }
                return _Import_table;
            }
        }

        private static DataTable _Import_columns;
        protected DataTable Import_columns
        {
            get
            {
                if (_Import_columns != null)
                {
                    return _Import_columns;
                }
                else
                {
                    _Import_columns = DBAccess.GetDataTable("SELECT COLUMN_ID,TABLE_ID,COLUMN_NAME,COLUMN_CODE,COLUMN_TYPE,COLUMN_LENGTH,DATA_COLUMN,MAX_VALUE,MIN_VALUE,FORMAT_VALUE,VARIDATE,ISPASS,DATA_TYPE,DATA_Expression FROM Import_columns", 7);

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

        public void ImportFile(string datatype, string method, Dictionary<string, string> parameters, string newId, out string resultStr, out string inGuid, out int apiLogId)
        {
            string errorSql = "";
            apiLogId = 0;
            inGuid = newId;
            resultStr = string.Empty;
            int dataSourceId = 1;

            try
            {
                string importDataStr = parameters["postdata"].ToString();

                ImportTemplateInfo template = GetApoTemplate(method);
                DataSet importDS = GetImportDS(importDataStr, datatype);
                CopyTables(importDS, template);
                VerifyImportDatas(importDS, template);

                if (template != null && importDS != null)
                {
                    errorSql = template.ErrorSql;
                    dataSourceId = template.DataSourceId;
                    resultStr = ImportDatasIntoDB(parameters, newId, null, null, template, importDS, ref inGuid);
                }
                else
                {
                    resultStr = "出错了！非法的服务名称！";
                    HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0003;
                }
            }
            catch (Exception ex)
            {
                string tempErrorSQL = errorSql;
                resultStr = "出错了！" + ex.Message.ToString();
                if (resultStr.Contains("错") || resultStr.Contains("败") || resultStr.Contains("异"))
                {
                    if (!string.IsNullOrEmpty(tempErrorSQL))   //若非空，做收尾处理，更新状态
                    {
                        tempErrorSQL = tempErrorSQL.Replace("~", "'");
                        tempErrorSQL = tempErrorSQL.Replace("{#GUID}", inGuid);
                        tempErrorSQL = common.ReplaceParameter(tempErrorSQL, parameters);

                        DBAccess.ExeuteSQL(tempErrorSQL, dataSourceId);
                    }
                }
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
                //ClsLog.AppendDbLog(StringHelper.GetExceptionMsg(ex), "ImportFile", ErrorLevel.Error, ClsLog.ProjectName);
            }
        }

        private static DataSet GetImportDS(string importDataStr, string dataFormat)
        {
            DataSet ds = null;

            if (!string.IsNullOrEmpty(importDataStr))
            {
                ds = new DataSet();
                if (dataFormat.Equals(DataFormat.Xml.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    #region xml导入
                    using (StringReader sr = new StringReader(importDataStr))
                    {
                        try
                        {
                            ds.ReadXml(sr, XmlReadMode.InferSchema);
                        }
                        catch (Exception ex)
                        {
                            HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0007;
                            throw ex;
                        }
                    }
                    #endregion
                }
                else if (dataFormat.Equals(DataFormat.Json.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    #region json导入
                    importDataStr = importDataStr.IndexOf("[") > -1 ? importDataStr : "{ \"Table\":[" + importDataStr + "]}";
                    try
                    {
                        ds = JsonConvert.DeserializeObject<DataSet>(importDataStr, new DataSetConverter());
                    }
                    catch (Exception ex)
                    {
                        HttpContext.Current.Items["ERROR_CODE"] = ErrorCode.ERROR_CODE.S0007;
                        throw ex;
                    }
                    #endregion
                }
            }

            return ds;
        }

        #region 读取文件
        /// <summary>
        /// 读取文件
        /// </summary>
        public int ReadFile(string filename)
        {
            int ret = 1;
            if (!File.Exists(filename))
            {
                string Msg = "文件：" + filename + "不存在！";
                ret = -1;
                //Alog.Pubs.errLog.WriteLog(Msg);
            }
            else
            {
                try
                {
                    string FileName = Path.GetFileName(filename);
                    foreach (DataRow dr in ImportRco2Tmp.Rows)
                    {
                        string MessageType = dr["MessageType"].ToString();
                        if (MessageType != null && MessageType != "" && FileName.IndexOf(MessageType) > -1)
                        {
                            int ImpID = int.Parse(dr["ImpID"].ToString());
                            //根据ImpID找到导入模板
                            ImportFile(filename, ImpID);
                            ret = 0;
                            break;
                        }
                        ret += 1;
                    }
                }
                catch (Exception ex)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  读取文件：" + filename + "出错，" + ex.Message, "服务日志");
                    throw ex;
                }

            }
            return ret;
        }

        /// <summary>
        /// 导入文件
        /// </summary>
        /// <param name="filename">文件名（带路径）</param>
        /// <param name="ImpID">导入模板ID</param>
        protected void ImportFile(string filename, int ImpID)
        {
            ImportTemplateInfo template = GetImportTemplate(ImpID);
            DataSet importDS = new DataSet();
            if (template.DataFormat == DataFormat.Xml)
            {
                importDS.ReadXml(filename);
            }

            CopyTables(importDS, template);
            VerifyImportDatas(importDS, template);

            if (template != null && importDS != null)
            {
                string tempGuid = string.Empty;
                string newId = Guid.NewGuid().ToString();
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                ImportDatasIntoDB(parameters, newId, string.Empty, string.Empty, template, importDS, ref tempGuid);
            }
        }
        #endregion
    }

    internal class ImportTemplateInfo
    {
        internal ImportTemplateInfo()
        {
            Tables = new List<ImportTableInfo>();
            VerifyInfos = new List<ImportVerifyInfo>();
        }
        public long Id { get; set; }
        public int DataSourceId { get; set; }
        public string MethodName { get; set; }
        public string TemplateName { get; set; }
        public DataFormat DataFormat { get; set; }
        public string RequestTemplate { get; set; }
        public string ResponseTemplate { get; set; }
        public string MasterSql { get; set; }
        public string SlaveSql { get; set; }
        public string CompleteSql { get; set; }
        public string ErrorSql { get; set; }
        public string ExecSql { get; set; }
        public List<ImportTableInfo> Tables { get; set; }
        public List<ImportVerifyInfo> VerifyInfos { get; set; }
    }

    internal class ImportTableInfo
    {
        internal ImportTableInfo()
        {
            Columns = new List<ImportColumnInfo>();
        }
        public long Id { get; set; }
        public int TableOrder { get; set; }
        public string SourceTableName { get; set; }
        public string AddTableNames { get; set; }
        public string TargetTableName { get; set; }
        public string SourceTableKey { get; set; }
        public string TargetTableKey { get; set; }
        public string InsertSql { get; set; }
        public string CheckRepeatSql { get; set; }
        public string FilterSql { get; set; }
        public string BulkInsertSql { get; set; }
        public string InsertSqlNew { get; set; }
        public string DataArea { get; set; }
        public int IsApo { get; set; }
        public int StartRow { get; set; }
        public int TopColumnName { get; set; }
        public List<ImportColumnInfo> Columns { get; set; }
    }

    internal class ImportVerifyInfo
    {
        public long Id { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
    }

    internal class ImportColumnInfo
    {
        public long Id { get; set; }
        public string SourceColumnName { get; set; }
        public string TargetColumnName { get; set; }
        public string DataLimit { get; set; }
        public string DataExpression { get; set; }
        public string DataType { get; set; }
        public int DataLength { get; set; }
    }

    internal class VerifyColumn
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int TableDepth { get; set; }
    }

    public enum DataFormat
    {
        Xml = 0,
        Json,
        Text
    }
}