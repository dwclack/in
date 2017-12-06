using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Data.SqlClient;
using System.Collections;
using System.Text.RegularExpressions;
using ASPNetPortal.DB;

namespace Alog.Common
{
    /// <summary>
/// Pager 的摘要说明
/// 封装分页功能,结合分页控件使用　
/// </summary>
    public class Pager
    {

        /// <summary>
        /// 传入链接字符串，sql语句，绑定控件
        /// </summary>
        /// <param name="GridViewId">数据绑定控件</param>
        /// <param name="WebPagerId">分页控件</param>
        /// <param name="SQL">SQL</param>
        /// <param name="keyName">主键名</param>
        /// <param name="pageSize">每页显示多少条记录</param>
        /// <param name="currentIndex">当前页的索引值</param>
        /// <param name="DSConnectionString">链接字符串</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, string SQL, string keyName, int pageSize, int currentIndex, string DSConnectionString)
        {
            SqlConnection con = new SqlConnection(DSConnectionString);
            DataSet ds = Paging(GridViewId, WebPagerId, SQL, keyName, pageSize, currentIndex, con);
            return ds;
        }
        /// <summary>
        /// 分页，根据链接字符串，整个sql语句,不绑定控件
        /// </summary>
        /// <param name="SQL">SQL</param>
        /// <param name="keyName">主键</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="DSConnectionString">链接字符串</param>
        /// <param name="RecordCount"></param>
        /// <returns></returns>
        public static DataSet Paging(string SQL, string keyName, int pageSize, int currentIndex, string DSConnectionString, out int RecordCount)
        {
            SqlConnection con = new SqlConnection(DSConnectionString);
            DataSet ds = Paging(SQL, keyName, pageSize, currentIndex, con,out RecordCount);
            return ds;
        }
        /// <summary>
        /// 分页，根据SqlConnection，整个sql语句，不绑定控件
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="SQL">SQL</param>
        /// <param name="keyName">主键</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="SqlConnection">SqlConnection</param>
        /// <param name="RecordCount">返回记录数</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(string SQL, string keyName, int pageSize, int currentIndex, SqlConnection con, out int RecordCount)
        {
            string orderfld = ""; //排序字段
            int index = -1;
            //if (SQL.ToLower().IndexOf("order by") > -1)
            //{
            //    orderfld = System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(SQL.ToLower(), "order by")[1].ToString(), "desc")[0].Trim();
            //}
            if (IsMatchWord(SQL, "order by", out index))
            {
                orderfld = System.Text.RegularExpressions.Regex.Split(SplitStr(SQL, "order by")[1].ToString(), @"\sdesc")[0].ToString().Trim();
            }
            if (orderfld == "")
            {
                orderfld = keyName;
            }
            //\s
            string tblName = ""; //表名

            tblName = SplitStr(SplitStr(SplitStrFirst(SQL, "from")[1].ToString(), "where")[0], "order by")[0].Trim();
            string colName = SplitStrFirst(SQL.Substring(("select").Length + SQL.IndexOf("select") + 1), "from")[0].Trim();
            string strWhere = "";
            //if (SQL.ToLower().IndexOf("where") > -1)
            //{
            //    strWhere = System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(SQL.ToLower(), "where")[1].ToString(), "order  by")[0], "order by")[0].Trim();

            //}
            
            if (IsMatchWord(SQL,"where",out index))
            {
                strWhere = SplitStr(SplitStr(SplitStr(SQL, "where")[1].ToString(), "order  by")[0], "order by")[0].Trim();

            }
            int OrderType = 0;  //排序方式,倒序为1正序为0
            if (SQL.IndexOf(" desc") > -1)
            {
                OrderType = 1;
            }
            DataSet ds = Paging(tblName, keyName, OrderType, orderfld, strWhere, pageSize, currentIndex, colName, con, out RecordCount);
            return ds;
        }
        public static bool IsMatchWord(string Str1, string Str2, out int Index)
        {
            bool Match = false;
            Index = -1;
            try
            {

                Match MM = System.Text.RegularExpressions.Regex.Match(Str1, @"\s" + Str2 + @"\s", RegexOptions.RightToLeft | RegexOptions.ExplicitCapture );

                if (MM.Success)
                {
                    Index = MM.Index;
                    Match = true;
                    return Match;
                }
                else
                {
                     MM = System.Text.RegularExpressions.Regex.Match(Str1, @"\s" + Str2 + @"\s", RegexOptions.RightToLeft | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
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
        public static bool IsMatchWordFirst(string Str1, string Str2, out int Index)
        {
            bool Match = false;
            Index = -1;
            try
            {
                Match MM = System.Text.RegularExpressions.Regex.Match(Str1, @"\s" + Str2 + @"\s", RegexOptions.ExplicitCapture);
                if (MM.Success)
                {
                    Index = MM.Index;
                    Match = true;
                    return Match;
                }
                else
                {
                    MM = System.Text.RegularExpressions.Regex.Match(Str1, @"\s" + Str2 + @"\s", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                    if (MM.Success)
                    {
                        Index = MM.Index;
                        Match = true;
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
            string[] strlist = new string[]{"",""};
           int  index = -1;
           bool Match = IsMatchWord(str1, str2,out index);
           if (index > -1)
           {
               strlist[0] = str1.Substring(0, index+1);
               strlist[1] = str1.Substring(index + str2.Length+1);
           }
           else
           {
               strlist[0] = str1;
           }
           return strlist;
        }
        protected static string[] SplitStrFirst(string str1, string str2)
        {
            string[] strlist = new string[] {"",""};
            int index = -1;
            bool Match = IsMatchWordFirst(str1, str2, out index);
            if (index > -1)
            {
                strlist[0] = str1.Substring(0, index+1);
                strlist[1] = str1.Substring(index + str2.Length+1);
            }
            else
            {
                strlist[0] = str1;
            }
            return strlist;
        }
        /// <summary>
        /// 分页，根据SqlConnection，整个sql语句，绑定控件
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="SQL">SQL</param>
        /// <param name="keyName">主键</param>
         /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="SqlConnection">SqlConnection</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, string SQL, string keyName, int pageSize, int currentIndex, SqlConnection con)
        {
            string orderfld = ""; //排序字段
            int index = -1;
            if (IsMatchWord(SQL, "order by", out index))
            {
                orderfld = System.Text.RegularExpressions.Regex.Split(SplitStr(SQL, "order by")[1].ToString(), @"\sdesc")[0].ToString().Trim();
            }
            if (orderfld=="")
           {
               orderfld = keyName;
           }
            string tblName = ""; //表名
            tblName = SplitStr(SplitStrFirst(SplitStrFirst(SQL, "from")[1].ToString(), "where")[0], "order by")[0].Trim();
            string colName = SplitStrFirst(SQL.Substring(("select").Length + SQL.IndexOf("select") + 1), "from")[0].Trim();
            string strWhere="";
            if (IsMatchWord(SQL, "where", out index))
            {
                strWhere = SplitStr(SplitStr(SplitStrFirst(SQL, "where")[1].ToString(), "order  by")[0], "order by")[0].Trim();

            }
            int OrderType = 0;  //排序方式,倒序为1正序为0
            if (SQL.IndexOf(" desc")>-1)
            {
                OrderType = 1;
            }
            DataSet ds = Paging(GridViewId, WebPagerId, tblName, keyName, OrderType, orderfld, strWhere, pageSize, currentIndex, colName, con);
            return ds;
        }
        /// <summary>
        /// 分页，根据链接字符串，表名，条件
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="tblName">表名</param>
        /// <param name="keyName">主键</param>
        /// <param name="OrderType">升序，降序</param>
        /// <param name="orderfld">排序字段</param>
        /// <param name="strWhere">where条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="colName">字段名</param>
        /// <param name="DSConnectionString">链接字符串</param>
        /// <param name="RecordCount">返回记录数</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, string tblName, string keyName, int OrderType, string orderfld, string strWhere, int pageSize, int currentIndex, string colName, string DSConnectionString)
        {
            SqlConnection con = new SqlConnection(DSConnectionString);
            DataSet ds = Paging(GridViewId, WebPagerId, tblName, keyName, OrderType, orderfld, strWhere, pageSize, currentIndex, colName, con);
            return ds;
        }

        /// <summary>
        /// 传入拆分的表名，字段名，条件，绑定控件
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="tblName">表名</param>
        /// <param name="keyName">主键</param>
        /// <param name="OrderType">升序，降序</param>
        /// <param name="orderfld">排序字段</param>
        /// <param name="strWhere">where条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="colName">字段名</param>
        /// <param name="con">SqlConnection</param>
        /// <param name="RecordCount">返回记录数</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, string tblName, string keyName, int OrderType, string orderfld, string strWhere, int pageSize, int currentIndex, string colName, SqlConnection con)
        {

           // DB_sql Ex_sql = new DB_sql();
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter("sp_PagerList", con);     //分页存储过程；
            SqlParameter SqlParameterstrWhere = new SqlParameter("@strWhere", SqlDbType.VarChar, -1);
            SqlParameterstrWhere.Value = strWhere;
            SqlParameter[] sqlParams = new SqlParameter[] { new SqlParameter("@tblName", tblName), new SqlParameter("@PageSize", pageSize), new SqlParameter("@PageIndex", currentIndex), new SqlParameter("@IsReCount", 1), new SqlParameter("@OrderType", OrderType), SqlParameterstrWhere , new SqlParameter("@orderfld", orderfld), new SqlParameter("@fldName", keyName), new SqlParameter("@colname", colName) };
            //sqlParams[5].Value = strWhere;
            //HttpContext.Current.Response.Write(strWhere);

            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            AddDataAdapterParam(da, sqlParams);
            DataSet ds = new DataSet();
            da.Fill(ds);//打开数据库连接执行填充；
            int i = 0;
            if (ds.Tables[0].Rows.Count > 0)
            {
                i = int.Parse(ds.Tables[0].Rows[0][0].ToString());
               // HttpContext.Current.Response.Write(i.ToString());
            }
            if (i < pageSize)
            {
                sqlParams[1].Value = i;
                sqlParams[2].Value = 1;
               
                
            }

          
            //pager.RecordCount = i;
            ds.Clear();
            sqlParams[3].Value = 0;
            da.Fill(ds);
           // ds.Tables[0].Columns.Add("NuMCol");

            //数据绑定控件的类型转换
            if (GridViewId.GetType() == typeof(GridView))
            {
                GridView control = (GridView)GridViewId;

                control.DataSource = ds;
                control.DataBind();
            }else if (GridViewId.GetType() == typeof(DataList))
            {
                DataList control = (DataList)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            else if (GridViewId.GetType() == typeof(Repeater))
            {
                Repeater control = (Repeater)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            else if (GridViewId.GetType() == typeof(FormView))
            {
                FormView control = (FormView)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            else if (GridViewId.GetType() == typeof(DataGrid))
            {
                DataGrid control = (DataGrid)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }

            con.Close();
            return ds;
            //ds.Dispose();//释放资源
        }

        /// <summary>
        /// 传入拆分的表名，字段名，条件，不绑定控件
        /// </summary>
        /// <param name="tblName">表名</param>
        /// <param name="keyName">主键</param>
        /// <param name="OrderType">升序，降序</param>
        /// <param name="orderfld">排序字段</param>
        /// <param name="strWhere">where条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="colName">字段名</param>
        /// <param name="con">SqlConnection</param>
        /// <param name="RecordCount">返回记录数</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(string tblName, string keyName, int OrderType, string orderfld, string strWhere, int pageSize, int currentIndex, string colName, SqlConnection con, out int RecordCount)
        {
            con.Open();

            SqlDataAdapter da = new SqlDataAdapter("sp_PagerList", con);     //分页存储过程；
            SqlParameter SqlParameterstrWhere = new SqlParameter("@strWhere", SqlDbType.VarChar, -1);
            SqlParameterstrWhere.Value = strWhere;
            SqlParameter[] sqlParams = new SqlParameter[] { new SqlParameter("@tblName", tblName), new SqlParameter("@PageSize", pageSize), new SqlParameter("@PageIndex", currentIndex), new SqlParameter("@IsReCount", 1), new SqlParameter("@OrderType", OrderType), SqlParameterstrWhere, new SqlParameter("@orderfld", orderfld), new SqlParameter("@fldName", keyName), new SqlParameter("@colname", colName) };
            //sqlParams[5].Value = strWhere;
           // HttpContext.Current.Response.Write(strWhere);

            da.SelectCommand.CommandType = CommandType.StoredProcedure;
            AddDataAdapterParam(da, sqlParams);
            DataSet ds = new DataSet();
            da.Fill(ds);//打开数据库连接执行填充；
            int i = 0;
            if (ds.Tables[0].Rows.Count > 0)
            {
                i = int.Parse(ds.Tables[0].Rows[0][0].ToString());
                // HttpContext.Current.Response.Write(i.ToString());
            }
            RecordCount = i;
            //pager.RecordCount = i;
            ds.Clear();
            sqlParams[3].Value = 0;
            da.Fill(ds);
 
           

            con.Close();
            return ds;
            //ds.Dispose();//释放资源
        }
        /// <summary>
        /// 传入的DataSet返回DataSet的分页，绑定控件
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="dstable">传入的DataSet</param>
        /// <param name="keyName">主键名称</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, DataSet dstable, string keyName, int pageSize, int currentIndex)
        {
            DataSet ds = new DataSet();
            //Standny.WebControls.WebPager pager = (Standny.WebControls.WebPager)WebPagerId;//转换为用于应用的分页控件；
            DataTable dt = dstable.Tables[0];
   
            // pager.RecordCount = dt.Rows.Count;
     
            DataTable newdt = dt.Copy();
            newdt.Clear();
            int rowbegin = (currentIndex-1 ) * pageSize;
            int rowend = (currentIndex) * pageSize;
            
            if (rowend > dt.Rows.Count) 
            { rowend = dt.Rows.Count; }
            for (int ii = rowbegin; ii <= rowend-1 ; ii++)
            {         DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[ii];
                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }        
                newdt.Rows.Add(newdr); 
            }
            ds.Clear();
            ds.Tables.Add(newdt);
            int i = dt.Rows.Count;
  
           /// pager.RecordCount = i;
            //ds.Clear();

            //数据绑定控件的类型转换
            if (GridViewId.GetType() == typeof(GridView))
            {
                GridView control = (GridView)GridViewId;

                control.DataSource = ds;
                control.DataBind();
            }

            else if (GridViewId.GetType() == typeof(DataList))
            {
                DataList control = (DataList)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            else if (GridViewId.GetType() == typeof(Repeater))
            {
                Repeater control = (Repeater)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            else if (GridViewId.GetType() == typeof(FormView))
            {
                FormView control = (FormView)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            else if (GridViewId.GetType() == typeof(DataGrid))
            {
                DataGrid control = (DataGrid)GridViewId;
                control.DataSource = ds;
                control.DataBind();
            }
            return ds;
        }
        /// <summary>
        /// 传入的DataSet返回DataSet的分页,不绑定
        /// </summary>
        /// <param name="dstable">传入的DataSet</param>
        /// <param name="keyName">主键名称</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="RecordCount">输出总记录数</param>
        /// <returns></returns>
       
        public static DataSet Paging(DataSet dstable, string keyName, int pageSize, int currentIndex,out int RecordCount)
        {
            DataSet ds = new DataSet();
            
            //Standny.WebControls.WebPager pager = (Standny.WebControls.WebPager)WebPagerId;//转换为用于应用的分页控件；
            DataTable dt = dstable.Tables[0];
            RecordCount = dt.Rows.Count;
            
            // pager.RecordCount = dt.Rows.Count;

            DataTable newdt = dt.Copy();
            newdt.Clear();
            int rowbegin = (currentIndex - 1) * pageSize;
            int rowend = (currentIndex) * pageSize;

            if (rowend > dt.Rows.Count)
            { rowend = dt.Rows.Count; }
            for (int ii = rowbegin; ii <= rowend - 1; ii++)
            {
                DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[ii];
                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }
                newdt.Rows.Add(newdr);
            }
            ds.Clear();
            ds.Tables.Add(newdt);
           

            return ds;
        }

        /// <summary>
        /// 传入的DataTable返回DataSet的分页DataSet的分页，绑定控件
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="dstable">传入的DataTable</param>
        /// <param name="keyName">主键名称</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <returns></returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, DataTable dstable, string keyName, int pageSize, int currentIndex)
        {

            DataSet newds = Paging(GridViewId, WebPagerId, dstable.DataSet, keyName, pageSize, currentIndex);
            return newds;
        }
        /// <summary>
        /// 传入的DataTable返回DataSet的分页，不绑定控件
        /// </summary>
        /// <param name="dstable">传入的DataTable</param>
        /// <param name="keyName">主键名称</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="RecordCount">输出总记录数</param>
        /// <returns>返回分页后的DataSet</returns>
        public static DataSet Paging(DataTable dstable, string keyName, int pageSize, int currentIndex, out int RecordCount)
        {
            DataSet newds = Paging(dstable.DataSet, keyName, pageSize, currentIndex,out RecordCount);
            return newds;
        }
 
        /// <summary>
        ///传入拆分的表面，字段名，条件，绑定控件(默认访问alogerp)
        /// </summary>
        /// <param name="GridViewId">绑定控件</param>
        /// <param name="WebPagerId">翻页控件</param>
        /// <param name="tblName">表名</param>
        /// <param name="keyName">主键</param>
        /// <param name="OrderType">升序，降序</param>
        /// <param name="orderfld">排序字段</param>
        /// <param name="strWhere">where条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="currentIndex">默认页</param>
        /// <param name="colName">字段名</param>
        /// <returns>DataSet</returns>
        public static DataSet Paging(Control GridViewId, Control WebPagerId, string tblName, string keyName, int OrderType, string orderfld, string strWhere, int pageSize, int currentIndex, string colName)
        {
            DB_sql Ex_sql = new DB_sql();
            DataSet ds = Paging(GridViewId, WebPagerId, tblName, keyName, OrderType, orderfld, strWhere, pageSize, currentIndex, colName, Ex_sql.con1);
            return ds;
         }
        
        public static void AddDataAdapterParam(SqlDataAdapter myDataAdapter, SqlParameter[] mySqlParamter)
        {
            foreach (SqlParameter param in mySqlParamter)
            {
                myDataAdapter.SelectCommand.Parameters.Add(param);
            }
        }
    }
}