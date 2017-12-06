using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using ASPNetPortal;
using System.IO;
using System.Text.RegularExpressions;
using C1.C1Excel;
using System.Text;
using System.Data.SqlClient;
using alogeip.Components;

namespace Alog_WSKJSD
{
    public class DataImport
    {
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
               try{
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
        #endregion
        #region 导入文件
        /// <summary>
        /// 导入文件
        /// </summary>
        /// <param name="filename">文件名（带路径）</param>
        /// <param name="ImpID">导入模板ID</param>
        protected void ImportFile(string filename, int ImpID)
        {

            Import_tempView = Import_temp.Clone();
            DataRow[] drs = null;
            drs = Import_temp.Select("AutoID = " + ImpID.ToString());
            string NewID = Guid.NewGuid().ToString();
            if (drs.Length > 0)
            {
                Import_tempView.ImportRow(drs[0]);
                StringBuilder r = new StringBuilder(); //返回信息
                string DataSourceType = Import_tempView.Rows[0]["DataSourceType"].ToString();//数据源类型
                string strID = ImpID.ToString();
                DataSet ds = new DataSet();
                DataTable dtImportTable = Import_table.Clone();
                DataRow[] drsImportTable = Import_table.Select("TMPT_ID=" + ImpID.ToString(), "ORDER_NUM");

                foreach (DataRow dr in drsImportTable)
                {
                    dtImportTable.ImportRow(dr);
                }
                #region 根据数据源类型导入
                if (DataSourceType == "xml")
                {
                    #region xml导入
                    try
                    {
                        ds.ReadXml(filename);
                    }
                    catch (Exception)
                    {
                        string Msg = filename + "不是有效的XML文件!";
                        //og.Pubs.errLog.WriteLog(Msg);

                    }

                    int DataGridIndex = 0;
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        DataRow[] rows = dtImportTable.Select(string.Format("TABLE_NAME='{0}'", ds.Tables[i].TableName));
                        //如果该表在导入列表中
                        if (rows.Length > 0)
                        {
                            //XML导入数据开始
                            DataGridIndex++;
                            //DataSet Import_table = DBAccess.GetDataSet("select STAR_ROW,TOPCOLNAME,TABLE_ID,TMPT_ID,TABLE_NAME,DATA_AREA,TARGET_TABLE,SELEDT_SQL,ISREPEAT_SQL,INSERT_SQL,IS_IMPORT from Import_table where TMPT_ID=" + ImpID.ToString() + " and TABLE_NAME= '" + ds.Tables[i].TableName + "'", "ConnectionString");
                            string TABLE_NAME = rows[0]["TABLE_NAME"].ToString();
                            string SELEDT_SQL = rows[0]["SELEDT_SQL"].ToString().Replace("~", "'");
                            string ISREPEAT_SQL = rows[0]["ISREPEAT_SQL"].ToString().Replace("~", "'");
                            string INSERT_SQL = rows[0]["INSERT_SQL"].ToString().Replace("~", "'");
                            string newInsertSql = (rows[0]["newinsertsql"] != DBNull.Value && rows[0]["newinsertsql"] != null) ? rows[0]["newinsertsql"].ToString().Replace("~", "'") : null;
                            string TARGET_TABLE_MasterKey = rows[0]["TARGET_TABLE_MasterKey"].ToString();
                            string SUR_TABLE_MasterKey = rows[0]["SUR_TABLE_MasterKey"].ToString();
                            //替换参数
                            DataTable ys_datatable = Import_columns.Clone();
                            DataRow[] ys_drs = Import_columns.Select("TABLE_ID = " + rows[0]["TABLE_ID"].ToString(), "COLUMN_ID");
                            foreach (DataRow dr in ys_drs)
                            {
                                ys_datatable.ImportRow(dr);
                            }
                            //给源数据增加表达式列
                            foreach (DataRow drcol in ys_datatable.Rows)
                            {
                                string DATA_COLUMN = drcol["DATA_COLUMN"].ToString();
                                if (!ds.Tables[i].Columns.Contains(DATA_COLUMN))
                                {
                                    ds.Tables[i].Columns.Add(DATA_COLUMN, typeof(System.String));
                                }
                                if (drcol["DATA_Expression"].ToString() != "")
                                {
                                    ds.Tables[i].Columns[DATA_COLUMN].Expression = drcol["DATA_Expression"].ToString().Replace("~", ",");
                                }
                            }
                            //插入暂存表
                            // InsertTempDataTable(ds.Tables[i], SELEDT_SQL, ISREPEAT_SQL, SUR_TABLE_MasterKey, TABLE_NAME, ys_dataset.Tables[0]);
                            //插入数据库

                            if (!string.IsNullOrEmpty(newInsertSql))
                            {
                                InsertTable(ds.Tables[i], NewID, SELEDT_SQL, ISREPEAT_SQL, newInsertSql, Convert.ToInt32(Import_tempView.Rows[0]["ToDSID"]), TARGET_TABLE_MasterKey);
                            }
                            else
                            {
                                InsertDataTable(ds.Tables[i], NewID, SELEDT_SQL, ISREPEAT_SQL, INSERT_SQL, ys_datatable, TABLE_NAME, SUR_TABLE_MasterKey, TARGET_TABLE_MasterKey);
                            }


                        }
                    }
                    #endregion
                }
                else if (DataSourceType == "xls")
                {
                    #region xls导入
                    C1XLBook objC1xloook = new C1XLBook(); //定义 Excel 读取处理的 类
                    try
                    {
                        objC1xloook.Load(filename); //加载 绝对路径的 EXCEL 文件 
                    }
                    catch (Exception)
                    {


                        //Alog.Pubs.errLog.WriteLog(filename + "不是有效的Excel文件!");
                    }
                    if (objC1xloook.Sheets.Count > 0)
                    {
                        int DataGridIndex = 0;

                        for (int i = 0; i < objC1xloook.Sheets.Count; i++)
                        {
                            DataRow[] rows = dtImportTable.Select(string.Format("TABLE_NAME='{0}'", objC1xloook.Sheets[i].Name.ToString()));
                            //如果该表在导入列表中
                            if (rows.Length > 0)
                            {
                                DataGridIndex++;
                                //获得该表的导入设置

                                string TABLE_NAME = rows[0]["TABLE_NAME"].ToString();
                                string SELEDT_SQL = rows[0]["SELEDT_SQL"].ToString().Replace("~", "'");
                                // this.Response.Write(SELEDT_SQL);
                                string ISREPEAT_SQL = rows[0]["ISREPEAT_SQL"].ToString().Replace("~", "'");
                                string INSERT_SQL = rows[0]["INSERT_SQL"].ToString().Replace("~", "'");
                                string newInsertSql = (rows[0]["newinsertsql"] != DBNull.Value && rows[0]["newinsertsql"] != null) ? rows[0]["newinsertsql"].ToString().Replace("~", "'") : null;
                                string TOPCOLNAME = rows[0]["TOPCOLNAME"].ToString();
                                string TARGET_TABLE_MasterKey = rows[0]["TARGET_TABLE_MasterKey"].ToString();
                                string SUR_TABLE_MasterKey = rows[0]["SUR_TABLE_MasterKey"].ToString();
                                int STAR_ROW = Convert.ToInt32(rows[0]["STAR_ROW"].ToString());
                                //如果首行是列名
                                if (TOPCOLNAME == "1")
                                {
                                    STAR_ROW = STAR_ROW + 1;
                                }

                                string ys_sql = "SELECT COLUMN_ID,TABLE_ID,COLUMN_NAME,COLUMN_CODE,COLUMN_TYPE,COLUMN_LENGTH,DATA_COLUMN,MAX_VALUE,MIN_VALUE,FORMAT_VALUE,VARIDATE,ISPASS,DATA_Expression  FROM Import_columns where TABLE_ID = " + rows[0]["TABLE_ID"].ToString() + " order by COLUMN_ID";
                                DataSet ys_dataset = DBAccess.GetDataSet(ys_sql, "ConnectionString");
                                DataTable dt = new DataTable("tbl1");
                                foreach (DataRow drcol in ys_dataset.Tables[0].Rows)
                                {
                                    dt.Columns.Add(drcol["DATA_COLUMN"].ToString(), typeof(System.String));
                                }
                                List<string> listStr = new List<string>();
                                #region 把excel数据导入到DataTable
                                if (objC1xloook.Sheets[i].Rows.Count >= STAR_ROW)// 如果有数据
                                {
                                    for (int h = 0; h < objC1xloook.Sheets[i].Columns.Count; h++)
                                    {
                                        try
                                        {
                                            if (objC1xloook.Sheets[i][STAR_ROW - 2, h].Value == null)
                                            {
                                                break;
                                            }
                                            string cname = objC1xloook.Sheets[i][STAR_ROW - 2, h].Value.ToString().Trim();
                                            cname = cname.Replace("（", "").Replace("）", "").Replace("/", "").Replace(",", "").Replace(";", "").Replace("？", "");
                                            objC1xloook.Sheets[i][STAR_ROW - 2, h].Value = cname.Trim();
                                            int cnum = 0;
                                            //;判断列名是否重名

                                            if (listStr.Count(t => t == cname) > 0)
                                            {
                                                try
                                                {
                                                    cnum = listStr.Count(t => t == cname);
                                                }
                                                catch { }
                                                string newName = cname + cnum;
                                                objC1xloook.Sheets[i][STAR_ROW - 2, h].Value = newName;
                                            }
                                            listStr.Add(cname);
                                        }
                                        catch { }
                                    }

                                    for (int n = STAR_ROW - 1; n <= objC1xloook.Sheets[i].Rows.Count - 1; n++)
                                    {

                                        DataRow dr = dt.NewRow();
                                        for (int j = 0; j < objC1xloook.Sheets[i].Columns.Count; j++)
                                        {
                                            object strVal = objC1xloook.Sheets[i][STAR_ROW - 2, j].Value;
                                            if (strVal == null || strVal == null) break;
                                            if (TOPCOLNAME == "1")
                                            {
                                                if (dt.Columns.Contains(strVal.ToString().Trim()))
                                                {
                                                    if (string.IsNullOrEmpty(dr[strVal.ToString().Trim()].ToString()))
                                                    {
                                                        dr[strVal.ToString().Trim()] = Convert.ToString(objC1xloook.Sheets[i][n, j].Value).Trim().Replace("~", "—").Replace("'", "‘");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (dr["Columns" + j.ToString()] != null)
                                                {
                                                    dr["Columns" + j.ToString()] = Convert.ToString(objC1xloook.Sheets[i][n, j].Value).Trim();
                                                }
                                            }

                                        }
                                        if (string.IsNullOrEmpty(dr[0].ToString()) && string.IsNullOrEmpty(dr[1].ToString()))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            dt.Rows.Add(dr);
                                        }

                                    }
                                }
                                #endregion
                                //给源数据增加表达式列
                                foreach (DataRow drcol in ys_dataset.Tables[0].Rows)
                                {
                                    string DATA_COLUMN = drcol["DATA_COLUMN"].ToString();
                                    //如果列不存在
                                    if (!dt.Columns.Contains(DATA_COLUMN))
                                    {
                                        dt.Columns.Add(DATA_COLUMN, typeof(System.String));

                                    }

                                    if (drcol["DATA_Expression"].ToString() != "")
                                    {
                                        dt.Columns[DATA_COLUMN].Expression = drcol["DATA_Expression"].ToString().Replace("~", ",");
                                    }
                                }
                                // InsertTempDataTable(dt, SELEDT_SQL, ISREPEAT_SQL, SUR_TABLE_MasterKey, TABLE_NAME, ys_dataset.Tables[0]);
                                //插入数据库

                                if (!string.IsNullOrEmpty(newInsertSql))
                                {
                                    InsertTable(dt, NewID, SELEDT_SQL, ISREPEAT_SQL, newInsertSql, Convert.ToInt32(Import_tempView.Rows[0]["ToDSID"]), TARGET_TABLE_MasterKey);
                                }
                                else
                                {
                                    InsertDataTable(dt, NewID, SELEDT_SQL, ISREPEAT_SQL, INSERT_SQL, ys_dataset.Tables[0], TABLE_NAME, SUR_TABLE_MasterKey, TARGET_TABLE_MasterKey);
                                }


                            }
                        }
                    }
                    objC1xloook.Dispose();
                    #endregion
                }
                else if (DataSourceType == "txt")
                {
                    #region txt导入
                    if (dtImportTable.Rows.Count > 0)
                    {
                        int DataGridIndex = 0;
                        string TABLE_NAME = dtImportTable.Rows[0]["TABLE_NAME"].ToString();
                        string SELEDT_SQL = dtImportTable.Rows[0]["SELEDT_SQL"].ToString().Replace("~", "'");
                        string ISREPEAT_SQL = dtImportTable.Rows[0]["ISREPEAT_SQL"].ToString().Replace("~", "'");
                        string INSERT_SQL = dtImportTable.Rows[0]["INSERT_SQL"].ToString().Replace("~", "'");
                        string TOPCOLNAME = dtImportTable.Rows[0]["TOPCOLNAME"].ToString();
                        string newInsertSql = (dtImportTable.Rows[0]["newinsertsql"] != DBNull.Value && dtImportTable.Rows[0]["newinsertsql"] != null) ? dtImportTable.Rows[0]["newinsertsql"].ToString().Replace("~", "'") : null;
                        string TARGET_TABLE_MasterKey = dtImportTable.Rows[0]["TARGET_TABLE_MasterKey"].ToString();
                        string SUR_TABLE_MasterKey = dtImportTable.Rows[0]["SUR_TABLE_MasterKey"].ToString();
                        string split1 = Import_tempView.Rows[0]["Split1"].ToString();
                        string split2 = Import_tempView.Rows[0]["Split2"].ToString();
                        string ys_sql = "SELECT COLUMN_ID,TABLE_ID,COLUMN_NAME,COLUMN_CODE,COLUMN_TYPE,COLUMN_LENGTH,DATA_COLUMN,MAX_VALUE,MIN_VALUE,FORMAT_VALUE,VARIDATE,ISPASS,DATA_Expression  FROM Import_columns where TABLE_ID = " + dtImportTable.Rows[0]["TABLE_ID"].ToString() + " order by COLUMN_ID";
                        DataSet ys_dataset = DBAccess.GetDataSet(ys_sql, "ConnectionString");
                        DataTable dt = new DataTable();
                        foreach (DataRow drcol in ys_dataset.Tables[0].Rows)
                        {
                            string colName = drcol["DATA_COLUMN"].ToString();
                            dt.Columns.Add(colName, typeof(System.String));
                        }
                        StreamReader sreader = new StreamReader(filename);
                        string[] colArray = null;
                        int startIndex = 0;
                        //读取数据
                        string strContent = sreader.ReadToEnd();
                        string[] valArray = Regex.Split(strContent, @"" + split2);
                        if (TOPCOLNAME == "1")
                        {
                            //读取首行列名
                            colArray = valArray[0].Split(new string[] { split1 }, StringSplitOptions.RemoveEmptyEntries);
                            startIndex = 1;
                        }
                        for (int i = startIndex; i < valArray.Length; i++)
                        {
                            DataRow drow = dt.NewRow();
                            if (string.IsNullOrEmpty(valArray[i])) { continue; }
                            string[] objArray = valArray[i].Split(new string[] { split1 }, StringSplitOptions.RemoveEmptyEntries);
                            for (int j = 0; j < objArray.Length; j++)
                            {
                                if (TOPCOLNAME == "1")
                                {
                                    drow[colArray[j]] = objArray[j];
                                }
                                else
                                {
                                    drow["Columns" + j] = objArray[j];
                                }
                            }
                            dt.Rows.Add(drow);
                        }
                        //插入数据库


                        if (!string.IsNullOrEmpty(newInsertSql))
                        {
                            InsertTable(dt, NewID, SELEDT_SQL, ISREPEAT_SQL, newInsertSql, Convert.ToInt32(Import_tempView.Rows[0]["ToDSID"]), TARGET_TABLE_MasterKey);
                        }
                        else
                        {
                            InsertDataTable(dt, NewID, SELEDT_SQL, ISREPEAT_SQL, INSERT_SQL, ys_dataset.Tables[0], TABLE_NAME, SUR_TABLE_MasterKey, TARGET_TABLE_MasterKey);
                        }


                    }
                    #endregion
                }
                #endregion
                #region 导入完后运行的SQL
                string ExecSql = Import_tempView.Rows[0]["ExecSql"].ToString();
                if (!string.IsNullOrEmpty(ExecSql))
                {

                    ExecSql = ExecSql.Replace("~", "'");
                    DBAccess.ExeuteSQL(ExecSql, int.Parse(Import_tempView.Rows[0]["ToDSID"].ToString()));
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

                            //this.Response.Write("<!--ISREPEAT_SQL:" + ISREPEAT_SQL+"-->");
                        }
                        catch (Exception e1)
                        {
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
            //r.Append("错误"); r.Append(cws.ToString()); r.Append("条,");
            //if (!string.IsNullOrEmpty(rtn)) { r.Append("\r\n出错:" + rtn); }

            //Alog.Pubs.errLog.WriteLog(dt.TableName + "导入结果:" + r.ToString());
            //LB_Msg.Text += r.ToString();
            //Alog.Pubs.errLog.WriteLog(dt.TableName + "导入结果:" + r.ToString());

        }
        #endregion
        #region 逐行导入数据
        /// <summary>
        /// 导入数据
        /// </summary>
        private void InsertDataTable(DataTable dt, string GUID, string SELEDT_SQL, string ISREPEAT_SQL, string INSERT_SQL, DataTable dtCOLUMN, string TABLE_NAME, string SUR_TABLE_MasterKey, string TARGET_TABLE_MasterKey)
        {
            string repidlst = "";//重复数据单号
            string exceplst = "";//错误数据单号
            String rtn = null;
            int sums = 0;// 导入行数
            int chenggong = 0;// 成功行数
            int cfs = 0;  //重复
            int cws = 0;  //错误
            int i_OrderCount = 0; //总共行数
            DataRow[] AllDr = dt.Select();
            string TempINSERT_SQL = INSERT_SQL;
            TempINSERT_SQL = TempINSERT_SQL.Replace("{#GUID}", GUID);
            string TempISREPEAT_SQL = ISREPEAT_SQL;
            TempISREPEAT_SQL = TempISREPEAT_SQL.Replace("{#GUID}", GUID);
            if (!string.IsNullOrEmpty(SELEDT_SQL))
            {
                AllDr = dt.Select(SELEDT_SQL);
            }
            i_OrderCount = AllDr.Length;
            /////////////////////
            DB_sql dbsql = new DB_sql();

            string connection = DBAccess.DesDecrypt(DBAccess.DataSource.Select("dsid = " + Import_tempView.Rows[0]["ToDSID"].ToString())[0]["ConnectionString"].ToString());

            SqlConnection conn = new SqlConnection(connection);
            conn.Open();

            //////////////
            int RowNum = 0;//记录循环到哪一行。
            string RowNumdec = "";//用于显示的错误行数

            try
            {
                foreach (DataRow Dr in AllDr)
                {
                    RowNum++;
                    INSERT_SQL = TempINSERT_SQL;
                    ISREPEAT_SQL = TempISREPEAT_SQL;
                    #region 替换源数据
                    foreach (DataRow drcol in dtCOLUMN.Rows)
                    {
                        string DATA_COLUMN = drcol["DATA_COLUMN"].ToString();
                        ISREPEAT_SQL = ISREPEAT_SQL.Replace("{#" + DATA_COLUMN + "}", Dr[DATA_COLUMN].ToString()).Replace("~", "'");
                        INSERT_SQL = INSERT_SQL.Replace("{#" + DATA_COLUMN + "}", Dr[DATA_COLUMN].ToString());
                        INSERT_SQL = INSERT_SQL.Replace("~", "'");
                    }
                    string SUR_TABLE_MasterKeyValue = "";
                    if (dt.Columns.Contains(SUR_TABLE_MasterKey))
                    {
                        SUR_TABLE_MasterKeyValue = Dr[SUR_TABLE_MasterKey].ToString();
                    }

                    #endregion
                    //判断是否导入
                    if (!string.IsNullOrEmpty(ISREPEAT_SQL))
                    {
                        DataTable dtisrep = DBAccess.GetDataSet(ISREPEAT_SQL, int.Parse(Import_tempView.Rows[0]["ToDSID"].ToString())).Tables[0];
                        if (dtisrep.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtisrep.Rows.Count; i++)
                            {
                                if (dtisrep.Columns.Contains(TARGET_TABLE_MasterKey) && !string.IsNullOrEmpty(TARGET_TABLE_MasterKey))
                                {
                                    repidlst = repidlst + dtisrep.Rows[i][TARGET_TABLE_MasterKey].ToString() + ",";
                                }
                            }
                            //TARGET_TABLE_MasterKey
                            //数据重复，或者不允许插入，写入日志
                            cfs++;
                            InsertImportLog(GUID, SUR_TABLE_MasterKeyValue, TABLE_NAME, 0, "数据重复，或者不允许插入!" + ISREPEAT_SQL);
                            ////Alog.Pubs.errLog.WriteLog("数据重复，或者不允许插入!" + ISREPEAT_SQL);
                        }
                        else
                        {
                            try
                            {
                                // this.Response.Write(INSERT_SQL);
                                SqlCommand myCommand = new SqlCommand(INSERT_SQL, conn);
                                myCommand.CommandType = CommandType.Text;
                                myCommand.ExecuteNonQuery();
                                InsertImportLog(GUID, SUR_TABLE_MasterKeyValue, TABLE_NAME, 1, "导入成功");
                                chenggong++;
                            }
                            catch (Exception err)
                            {
                                cws++; RowNumdec = RowNumdec + "错误行数：" + RowNum.ToString() + ";";
                                if (!string.IsNullOrEmpty(SUR_TABLE_MasterKey) && Dr.Table.Columns.Contains(SUR_TABLE_MasterKey))
                                {
                                    exceplst = exceplst + Dr[SUR_TABLE_MasterKey].ToString() + ",";
                                }
                                rtn = err.Message;
                                InsertImportLog(GUID, SUR_TABLE_MasterKeyValue, TABLE_NAME, 0, "INSERT_SQL:" + INSERT_SQL + rtn.Replace("'", "＇"));
                                //  //Alog.Pubs.errLog.WriteLog("INSERT_SQL:" + INSERT_SQL + rtn);

                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            //this.Response.Write(INSERT_SQL);
                            DBAccess.ExeuteSQL(INSERT_SQL, int.Parse(Import_tempView.Rows[0]["ToDSID"].ToString()));
                            InsertImportLog(GUID, SUR_TABLE_MasterKeyValue, TABLE_NAME, 1, "导入成功");
                            chenggong++;
                        }
                        catch (Exception err)
                        {
                            cws++;
                            RowNumdec = RowNumdec + "错误行数：" + RowNum.ToString() + ";";
                            if (!string.IsNullOrEmpty(SUR_TABLE_MasterKey) && Dr.Table.Columns.Contains(SUR_TABLE_MasterKey))
                            {
                                exceplst += Dr[SUR_TABLE_MasterKey].ToString() + ",";
                            }
                            rtn = err.Message;
                            InsertImportLog(GUID, SUR_TABLE_MasterKeyValue, TABLE_NAME, 0, "INSERT_SQL:" + INSERT_SQL + rtn.Replace("'", "＇"));
                            //Alog.Pubs.errLog.WriteLog("INSERT_SQL:" + INSERT_SQL + rtn);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(repidlst))
                {
                    repidlst = repidlst.Substring(0, repidlst.Length - 1);
                }
                if (!string.IsNullOrEmpty(exceplst))
                {
                    exceplst = exceplst.Substring(0, exceplst.Length - 1);
                }
            }
            catch (Exception ee)
            {
                // string ClientScript1 = ee.Message.ToString().Replace("'", "‘") + "。SQL:" + insertSql;
                //Alog.Pubs.errLog.WriteLog(ClientScript1);
                conn.Close();
            }
            finally
            {
                conn.Close();
            }

            conn.Close();
            StringBuilder r = new StringBuilder();
            r.Append("总共"); r.Append(i_OrderCount.ToString()); r.Append("条,\r\n");
            r.Append("导入"); r.Append(sums.ToString()); r.Append("条,\r\n");
            r.Append("成功"); r.Append(chenggong.ToString()); r.Append("条,\r\n");
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

            //Alog.Pubs.errLog.WriteLog(dt.TableName + "导入结果:" + r.ToString());
            //LB_Msg.Text += r.ToString();
            //Alog.Pubs.errLog.WriteLog(dt.TableName + "导入结果:" + r.ToString());
        }
        #endregion
        #region 插入导入日志
        /// <summary>
        /// 插入导入日志
        /// </summary>
        protected void InsertImportLog(string NewGuid, string SUR_TABLE_MasterKey, string TABLE_NAME, int Log_Type, string Log_Str)
        {

            string INSERT_SQL = "INSERT INTO [Import_log]([GUIID],[TABLE_NAME],[TABLE_MasterKey],[UserName],[Log_times],[Log_Type],[Log_Str]) VALUES ";
            INSERT_SQL += "('" + NewGuid + "','" + TABLE_NAME + "','" + SUR_TABLE_MasterKey + "','001',getdate()," + Log_Type.ToString() + ",'" + Log_Str.Replace("'", "＇") + "')";
            DBAccess.ExeuteSQL(INSERT_SQL, int.Parse(Import_tempView.Rows[0]["ToDSID"].ToString()));
        }
        #endregion
        #region 缓存导入设置
        private DataTable _ImportRco2Tmp;
        protected DataTable ImportRco2Tmp
        {
            get
            {
                if (_Import_temp == null)
                {
                    string RcoID = System.Configuration.ConfigurationManager.AppSettings["RcoID"];
                    string sql = "SELECT [RcoID],[ImpID],[MessageType]  FROM [ImportRco2Tmp] where [RcoID] = " + RcoID;
                    _ImportRco2Tmp = DBAccess.GetDataTable(sql, 7);
                }
                return _ImportRco2Tmp;
            }
        }
        private DataTable _Import_temp;
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
        private DataTable _Import_tempView;
        /// <summary>
        /// 导入模版的基本信息处理
        /// </summary>
        protected DataTable Import_tempView
        {
            get
            {
                return _Import_tempView;
            }
            set
            {
                _Import_tempView = value;
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

        private DataTable _Import_columns;
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
                    _Import_columns = DBAccess.GetDataTable("SELECT COLUMN_ID,TABLE_ID,COLUMN_NAME,COLUMN_CODE,COLUMN_TYPE,COLUMN_LENGTH,DATA_COLUMN,MAX_VALUE,MIN_VALUE,FORMAT_VALUE,VARIDATE,ISPASS,DATA_Expression FROM Import_columns", 7);
                   
                }
                return _Import_columns;
            }
        }
        #endregion

    }
}