using System;
using System.Collections.Generic;
using System.Linq;
using ASPNetPortal;
using System.Data.SqlClient;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;

namespace Alog_WSKJSD
{
    public class RepXml
    {
        int Flag = 0;
        //public static bool[] AllOK = new bool[300];
        public static Dictionary<string, bool> AllOK = new Dictionary<string, bool>();
        static readonly object padlock = new object();

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

        /// <summary>
        /// 生成XML文件编码格式
        /// </summary>
        private static string _encodeType;
        public static string EncodeType
        {
            get
            {
                if (_encodeType == null)
                {
                    _encodeType = ClsLog.GetAppSettings("EncodeType");
                }
                return _encodeType;
            }
        }

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
                    string sql = @"SELECT [AutoId]
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
                                  ,[CompleteSQL] from RepXmlSet where isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and  RepVersion=" + repVersion;
                    return _dtRepXmlSet = DBAccess.GetDataTable(sql, 7); ;
                }

                return _dtRepXmlSet;
            }
            set
            {
                _dtRepXmlSet = value;
            }
        }


        #endregion
        public void ThreadHandle(object ClsParam)
        {
            ClsThreadParam cls = (ClsThreadParam)ClsParam;
            lock (padlock)
            {
                RepXml.AllOK[cls.RepTitle] = false; //关闭，在线程结束时再打开
            }
            ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + cls.RepTitle + "_线程启动", cls.RepTitle);

            
            int ToDSID = int.Parse(cls.dr["DSID"].ToString());
            string strHTML = string.Empty;
            string xmlFile = string.Empty;
            string Path = ClsLog.GetAppSettings("Path");
            string PathFtp = ClsLog.GetAppSettings("PathFtp");
            string PathBak = ClsLog.GetAppSettings("PathBak") + "Bak_" + DateTime.Now.ToString("yyyyMMdd") + @"\";
            string QuerySql = cls.dr["QuerySql"].ToString();
            string MasterSQL = cls.dr["MasterSQL"].ToString();
            string CompleteSQL = cls.dr["CompleteSQL"].ToString();

            try
            {
                Flag = 5;
                MasterSQL = MasterSQL.Replace("~", "'");
                if (MasterSQL == string.Empty)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  主表sql语句为空" + cls.dr["RepTitle"].ToString() + "_线程结束", cls.dr["RepTitle"].ToString());
                    lock (padlock)
                    {
                        AllOK[cls.RepTitle] = true;
                    }
                    return;
                }

                DataTable dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);
                Flag = 6;
                if (dtMaster.Rows.Count <= 0)
                {
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  没有要处理的" + cls.dr["RepTitle"].ToString() + "_线程结束", cls.dr["RepTitle"].ToString());
                    lock (padlock)
                    {
                        AllOK[cls.RepTitle] = true;
                    }
                    return;
                }
                foreach (DataRow dr in dtMaster.Rows)   //对主表数据逐条处理，每条生成一个报文
                {
                    string tempQuerySql = QuerySql;
                    // xmlFile = DBAccess.DB_GetObj("exec [MakeRepFileName] '" + cls.dr["RepFileName"].ToString() + "'", 7).ToString(); 原来创建文件名的方法

                    xmlFile = dr["MessageID"].ToString();
                    string SendTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    //
                    string sql = "Update M_Head set SendTime = '" + SendTime + "' where MessageID = '" + dr["MessageID"].ToString()+"'";
                    DBAccess.ExeuteSQL(sql, 1);
                    //dr["MessageID"] = xmlFile;
                    dr["SendTime"] = SendTime;
                    //xmlFile = dr["MessageID"].ToString();

                    ParentTempLabelHtml = ReplaceMasterValue(cls.dr["XMLContent"].ToString(), dr);//替换XML模板的主表数据
                    tempQuerySql = ReplaceMasterValue(tempQuerySql, dr);//替换表体SQL(循环用)查询的的主表数据

                    if (tempQuerySql == string.Empty)
                    {
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  循环sql语句为空" + cls.dr["RepTitle"].ToString() + "_线程结束", cls.dr["RepTitle"].ToString());
                        lock (padlock)
                        {
                            AllOK[cls.RepTitle] = true;
                        }
                        return;
                    }
                    DataTable dt = DBAccess.GetDataTable(tempQuerySql, ToDSID);
                    ParentTempLabelHtml = ReplaceSlaveValue(ParentTempLabelHtml, dt);//替换XML模板的表体及表体明细数据
                    string parentid = cls.dr["AutoId"].ToString();

                    InitLabelXML(parentid);//递归生成报文

                    xmlFile += @".xml";
                    Flag = 7;
                    string tempCompleteSQL = CompleteSQL;
                    if (!string.IsNullOrEmpty(tempCompleteSQL))   //若非空，做收尾处理，更新状态
                    {
                        tempCompleteSQL = tempCompleteSQL.Replace("~", "'");
                        tempCompleteSQL = ReplaceMasterValue(tempCompleteSQL, dr);
                        DBAccess.ExeuteSQL(tempCompleteSQL, ToDSID);
                    }
                    Flag = 8;
                    strHTML = ParentTempLabelHtml;
                    //if (EncodeType == "1")
                    //{
                    //    strHTML = utf8_gb2312(strHTML);

                        //strHTML = Encoding..GetString(Encoding.UTF8.GetBytes(strHTML));
                    //}
                    ClsLog.AppendXMLCreate(strHTML, xmlFile, Path);     //生成XML文件
                    ClsLog.CopyFile(xmlFile, Path, PathBak);      //备份XML文件
                    ClsLog.MoveFile(xmlFile, Path, PathFtp);      //剪切到上传目录
                    //ClsLog.DeleteFile(Path + xmlFile);            //删除生成路径的文件

                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + cls.dr["RepTitle"].ToString() + ":" + xmlFile, cls.dr["RepTitle"].ToString());
                    Flag = 9;
                }
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + cls.dr["RepTitle"].ToString() + "_线程结束", cls.dr["RepTitle"].ToString());
                lock (padlock)
                {
                    AllOK[cls.RepTitle] = true;
                }
            }
            catch (Exception ex)
            { 
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Flag=" + Flag.ToString() + ex.Message, cls.dr["RepTitle"].ToString());
                lock (padlock)
                {
                    AllOK[cls.RepTitle] = true;
                }
            }
        }
        /// <summary>
        /// UTF8转换成GB2312
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string utf8_gb2312(string text)
        {
            //声明字符集   
            System.Text.Encoding utf8, gb2312;
            //utf8   
            utf8 = System.Text.Encoding.GetEncoding("utf-8");
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            byte[] utf;
            utf = utf8.GetBytes(text);
            utf = System.Text.Encoding.Convert(utf8, gb2312, utf);
            //return utf;
            //返回转换后的字符   
            return gb2312.GetString(utf);
        }

        /// <summary>
        /// GB2312转换成UTF8
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string gb2312_utf8(string text)
        {
            //声明字符集   
            System.Text.Encoding utf8, gb2312;
            //gb2312   
            gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            //utf8   
            utf8 = System.Text.Encoding.GetEncoding("utf-8");
            byte[] gb;
            gb = gb2312.GetBytes(text);
            gb = System.Text.Encoding.Convert(gb2312, utf8, gb);
            //返回转换后的字符   
            return utf8.GetString(gb);
        }


        /// <summary>
        /// 递归读取标替换签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        void InitLabelXML(string parentid)
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
                        ToDSID = int.Parse(drs[i]["DSID"].ToString());
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
                            SlaveSQL = ReplaceParameterValue(SlaveSQL, Parameters);
                            if (dtMaster.Rows.Count > 0)
                            {
                                SlaveSQL = ReplaceMasterValue(SlaveSQL, dtMaster.Rows[0]); ;
                            }
                            DataTable dtSlave = DBAccess.GetDataTable(SlaveSQL, ToDSID);
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

        public string ReplaceMasterValue(string temphtml, DataRow dr)
        {
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                string tempStr =XmlTextEncoder.Encode(dr[dr.Table.Columns[i].ColumnName].ToString()); 
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

    }
}
