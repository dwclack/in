using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASPNetPortal;
using System.Text;
using System.Data;
using System.Collections.Specialized;
using System.Text.RegularExpressions;


namespace alogeip
{
    public class LabelMsg
    {
        string TempLabelHtml, LabelHtml, UserID, UrlPath, StringQuery, Page;

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="urlPath"></param>
        /// <param name="stringQuery"></param>
        /// <param name="page"></param>
        public LabelMsg()
        {
            this.UserID = HttpContext.Current.User.Identity.Name;
            this.UrlPath = HttpContext.Current.Request.Url.ToString();//((Page)HttpContext.Current.Handler).Request.Url.ToString();
            this.StringQuery = HttpContext.Current.Request.QueryString.ToString();//((Page)HttpContext.Current.Handler).Request.QueryString.ToString();
            this.Page = HttpContext.Current.Request["page"];//((Page)HttpContext.Current.Handler).Request["page"];

        }

        public LabelMsg(string userID, string urlPath, string stringQuery, string page)
        {
            this.UserID = userID;
            this.UrlPath = urlPath;
            this.StringQuery = stringQuery;
            this.Page = page;
        }
        #endregion

        #region 缓存方法
       
        private DataTable _htmlDT;
        public DataTable htmlDT
        {
            get
            {
                if (_htmlDT != null)
                {
                    return _htmlDT;
                }
                if (HttpRuntime.Cache["HtmlModel"] != null)
                {
                    _htmlDT = (DataTable)HttpRuntime.Cache["HtmlModel"];
                }
                else
                {
                    _htmlDT = DBAccess.GetDataTable("SELECT * FROM LabelModule", 7);
                    HttpRuntime.Cache["HtmlModel"] = _htmlDT;
                }
                return _htmlDT;
            } 
        }


        #endregion


        #region 根据标题获取标签数据

        /// <summary>
        /// 根据标题获取标签数据
        /// </summary>
        /// <param name="strTitle"></param>
        public string GetLabelContentByLabelHtmlAndLabelCode(string tempLabelHtml, string labelCode)
        {

            //if (htmlDT.Select("LabelTitle='" + strTitle + "'").Count() > 0)
            //{
            //    DataRow dr = htmlDT.Select("LabelTitle='" + strTitle + "'")[0];
            //    if (PortalSecurity.IsInRoles(dr["AuthorizedRoles"].ToString()) || string.IsNullOrEmpty(dr["AuthorizedRoles"].ToString()))
            //    {

                    TempLabelHtml = tempLabelHtml;
                    string LabelCode = labelCode;
                    LabelHtml = ReplaceLabelByTitle(TempLabelHtml, LabelCode);
            //    }
            //    else
            //    {
            //        TempLabelHtml = "";
            //    }
            //}
            return TempLabelHtml;

        }


        /// <summary>
        /// 根据标题获取标签数据
        /// </summary>
        /// <param name="strTitle"></param>
        public string GetLabelContentByTitle(string strTitle)
        {
  
            if (htmlDT.Select("LabelTitle='" + strTitle + "'").Count() > 0)
            {
                DataRow dr = htmlDT.Select("LabelTitle='" + strTitle + "'")[0];
                if (PortalSecurity.IsInRoles(dr["AuthorizedRoles"].ToString()) || string.IsNullOrEmpty(dr["AuthorizedRoles"].ToString()))
                {
                    TempLabelHtml = HttpUtility.HtmlDecode(dr["LabelContent"].ToString());
                    string LabelCode = dr["LabelCode"].ToString();
                    LabelHtml = ReplaceLabelByTitle(TempLabelHtml, LabelCode);
                }
                else
                {
                    TempLabelHtml = "";
                }
            }
            return TempLabelHtml;

            //DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT  AutoId,LabelCode,LabelContent,ToDSID,MasterSQL,QuerySQL,AuthorizedRoles  FROM LabelModule where LabelTitle = '" + strTitle + "'", "ConnectionString");
            //if (ds_LableSeting.Tables.Count > 0)
            //{
            //    if (ds_LableSeting.Tables[0].Rows.Count > 0)
            //    {
            //        if (PortalSecurity.IsInRoles(ds_LableSeting.Tables[0].Rows[0]["AuthorizedRoles"].ToString()) || string.IsNullOrEmpty(ds_LableSeting.Tables[0].Rows[0]["AuthorizedRoles"].ToString()))
            //        {
            //            TempLabelHtml = HttpUtility.HtmlDecode(ds_LableSeting.Tables[0].Rows[0]["LabelContent"].ToString());
            //            string LabelCode = ds_LableSeting.Tables[0].Rows[0]["LabelCode"].ToString();
            //            LabelHtml = ReplaceLabelByTitle(TempLabelHtml, LabelCode);
            //        }
            //        else
            //        {
            //            TempLabelHtml = "";
            //        }
            //    }
            //}
            //return TempLabelHtml;
        }

        //移动客户端根据标签获取数据
        public string GetMobileLabelContentByTitle(string AutoId)
        {
            //DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT  AutoId,LabelCode, MobileLabelContent as LabelContent,ToDSID,MasterSQL,QuerySQL,AuthorizedRoles  FROM LabelModule where AutoId = '" + AutoId + "'", "ConnectionString");
            //if (ds_LableSeting.Tables.Count > 0)
            //{
            //    if (ds_LableSeting.Tables[0].Rows.Count > 0)
            //    {
            //        if (PortalSecurity.IsInRoles(ds_LableSeting.Tables[0].Rows[0]["AuthorizedRoles"].ToString()) || string.IsNullOrEmpty(ds_LableSeting.Tables[0].Rows[0]["AuthorizedRoles"].ToString()))
            //        {
            //            TempLabelHtml = HttpUtility.HtmlDecode(ds_LableSeting.Tables[0].Rows[0]["LabelContent"].ToString());
            //            string LabelCode = ds_LableSeting.Tables[0].Rows[0]["LabelCode"].ToString();
            //            LabelHtml = ReplaceMobileLabelByTitle(TempLabelHtml, LabelCode);
            //        }
            //        else
            //        {
            //            TempLabelHtml = "";
            //        }
            //    }
            //}


            
            DataTable dt = htmlDT;
            if (dt.Select("AutoId=" + AutoId).Count() > 0)
            {
                DataRow dr = dt.Select("AutoId=" + AutoId)[0];
                if (PortalSecurity.IsInRoles(dr["AuthorizedRoles"].ToString()) || string.IsNullOrEmpty(dr["AuthorizedRoles"].ToString()))
                {
                    TempLabelHtml = HttpUtility.HtmlDecode(dr["MobileLabelContent"].ToString());
                    string LabelCode = dr["LabelCode"].ToString();
                    LabelHtml = ReplaceMobileLabelByTitle(TempLabelHtml, LabelCode);
                }
                else
                {
                    TempLabelHtml = "";
                }
            }
            return TempLabelHtml;
        }
        #endregion

        #region 根据标题获取标签数据
        /// <summary>
        /// 根据标题获取标签数据
        /// </summary>
        /// <param name="strTitle"></param>
        public string GetLabelContentByCode(string labelCode)
        {
            DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT  AutoId,LabelCode,LabelContent,ToDSID,MasterSQL,QuerySQL  FROM LabelModule where labelCode = '" + labelCode + "'", "ConnectionString");
            if (ds_LableSeting.Tables.Count > 0)
            {
                if (ds_LableSeting.Tables[0].Rows.Count > 0)
                {
                    TempLabelHtml = HttpUtility.HtmlDecode(ds_LableSeting.Tables[0].Rows[0]["LabelContent"].ToString());
                    string LabelCode = ds_LableSeting.Tables[0].Rows[0]["LabelCode"].ToString();
                    LabelHtml = ReplaceLabelByTitle(TempLabelHtml, LabelCode);
                }
            }
            return TempLabelHtml;
        }
        #endregion

        /// <summary>
        /// 读取标签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        public string ReplaceLabelByTitle(string temp, string LabelCode)
        {
            //DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT  AutoId,LabelCode,ToDSID,MasterSQL,QuerySQL  FROM LabelModule where LabelCode = '" + LabelCode + "'", "ConnectionString");
            //if (ds_LableSeting.Tables.Count > 0)
            //{
            //    if (ds_LableSeting.Tables[0].Rows.Count > 0)
            //    {
            //        if (!string.IsNullOrEmpty(ds_LableSeting.Tables[0].Rows[0]["MasterSQL"].ToString()))
            //        {
            //            TempLabelHtml = ReplaceMasterValue(TempLabelHtml, ds_LableSeting.Tables[0].Rows[0]["MasterSQL"].ToString(), int.Parse(ds_LableSeting.Tables[0].Rows[0]["ToDSID"].ToString()));
            //        }
            //        if (!string.IsNullOrEmpty(ds_LableSeting.Tables[0].Rows[0]["QuerySQL"].ToString()))
            //        {
            //            TempLabelHtml = ReplaceSlaveValue(TempLabelHtml, ds_LableSeting.Tables[0].Rows[0]["QuerySQL"].ToString(), int.Parse(ds_LableSeting.Tables[0].Rows[0]["ToDSID"].ToString()));
            //        }
            //        this.InitLabelHtml(ds_LableSeting.Tables[0].Rows[0]["AutoId"].ToString());
            //    }

            //}

            if (htmlDT.Select("LabelCode='" + LabelCode + "'").Count() > 0)
            {
                DataRow dr = htmlDT.Select("LabelCode='" + LabelCode + "'")[0];
                if (!string.IsNullOrEmpty(dr["MasterSQL"].ToString()))
                {
                    TempLabelHtml = ReplaceMasterValue(TempLabelHtml, dr["MasterSQL"].ToString(), int.Parse(dr["ToDSID"].ToString()));
                }
                if (!string.IsNullOrEmpty(dr["QuerySQL"].ToString()))
                {
                    TempLabelHtml = ReplaceSlaveValue(TempLabelHtml, dr["QuerySQL"].ToString(), int.Parse(dr["ToDSID"].ToString()));
                }
                this.InitLabelHtml(dr["AutoId"].ToString());
            }

            TempLabelHtml = TempLabelHtml.Replace("{*public*}", TempLabelHtml);
            TempLabelHtml = TempLabelHtml.Replace("~", "'");
            TempLabelHtml = ReplaceSingleUsers(TempLabelHtml);
            TempLabelHtml = ReplaceHtmlTag(TempLabelHtml);
            return TempLabelHtml;
        }

        //移动客户端
        public string ReplaceMobileLabelByTitle(string temp, string LabelCode)
        {
            //DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT  AutoId,LabelCode,ToDSID,MasterSQL,QuerySQL  FROM LabelModule where LabelCode = '" + LabelCode + "'", "ConnectionString");
            //if (ds_LableSeting.Tables.Count > 0)
            //{
            //    if (ds_LableSeting.Tables[0].Rows.Count > 0)
            //    {
            //        if (!string.IsNullOrEmpty(ds_LableSeting.Tables[0].Rows[0]["MasterSQL"].ToString()))
            //        {
            //            TempLabelHtml = ReplaceMasterValue(TempLabelHtml, ds_LableSeting.Tables[0].Rows[0]["MasterSQL"].ToString(), int.Parse(ds_LableSeting.Tables[0].Rows[0]["ToDSID"].ToString()));
            //        }
            //        if (!string.IsNullOrEmpty(ds_LableSeting.Tables[0].Rows[0]["QuerySQL"].ToString()))
            //        {
            //            TempLabelHtml = ReplaceSlaveValue(TempLabelHtml, ds_LableSeting.Tables[0].Rows[0]["QuerySQL"].ToString(), int.Parse(ds_LableSeting.Tables[0].Rows[0]["ToDSID"].ToString()));
            //        }
            //        this.InitMobileLabelHtml(ds_LableSeting.Tables[0].Rows[0]["AutoId"].ToString());
            //    }

            //}

            if (htmlDT.Select("LabelCode='" + LabelCode + "'").Count() > 0)
            {
                DataRow dr = htmlDT.Select("LabelCode='" + LabelCode + "'")[0];
                if (!string.IsNullOrEmpty(dr["MasterSQL"].ToString()))
                {
                    TempLabelHtml = ReplaceMasterValue(TempLabelHtml, dr["MasterSQL"].ToString(), int.Parse(dr["ToDSID"].ToString()));
                }
                if (!string.IsNullOrEmpty(dr["QuerySQL"].ToString()))
                {
                    TempLabelHtml = ReplaceSlaveValue(TempLabelHtml, dr["QuerySQL"].ToString(), int.Parse(dr["ToDSID"].ToString()));
                }
                this.InitMobileLabelHtml(dr["AutoId"].ToString());
            }

            TempLabelHtml = TempLabelHtml.Replace("{*public*}", TempLabelHtml);
            TempLabelHtml = TempLabelHtml.Replace("~", "'");
            TempLabelHtml = ReplaceSingleUsers(TempLabelHtml);
            TempLabelHtml = ReplaceMobileHtmlTag(TempLabelHtml);
            return TempLabelHtml;
        }

        string ReplaceSingleUsers(string temphtml)
        {
            if (HttpContext.Current.Items["SingleUsers"] != null)
            {
                try
                {
                    SingleUsers SUser = (SingleUsers)HttpContext.Current.Items["SingleUsers"];

                    // SingleUsers SUser = new SingleUsers(UserID);
                    temphtml = temphtml.Replace("{$部门ID$}", SUser.DeptID);
                    temphtml = temphtml.Replace("{$部门$}", SUser.DeptName);
                    temphtml = temphtml.Replace("{$工号$}", SUser.UserName);
                    temphtml = temphtml.Replace("{$姓名$}", SUser.Realname);
                }
                catch
                {
                    ;
                }
            }
            return temphtml;
        }

        /// <summary>
        /// 读取标签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        void InitLabelHtml(string parentid)
        {
            #region 老版2014-08-08
            //DataSet ds_ChildLableSeting = DBAccess.GetDataSet("SELECT AutoId,parentid,ToDSID,MasterSQL,QuerySQL,LabelTitle,LabelCode,LabelContent,AuthorizedRoles  FROM LabelModule where parentid = " + parentid + "", "ConnectionString");
            //if (ds_ChildLableSeting.Tables.Count > 0)
            //{
            //    for (int i = 0; i < ds_ChildLableSeting.Tables[0].Rows.Count; i++)
            //    {
            //        string temphtml = "";
            //        string LabelCode = "";
            //        string MasterSQL = "";
            //        string SlaveSQL = "";
            //        string AuthorizedRoles = "";
            //        int ToDSID = 0;
            //        AuthorizedRoles = ds_ChildLableSeting.Tables[0].Rows[i]["AuthorizedRoles"].ToString();
            //        LabelCode = ds_ChildLableSeting.Tables[0].Rows[i]["LabelCode"].ToString();
            //        if (PortalSecurity.IsInRoles(AuthorizedRoles) || string.IsNullOrEmpty(AuthorizedRoles))
            //        {
            //            string str = @"{#(.*)#}";//表达试
            //            Match macths = Regex.Match(LabelCode, str);
            //            if (macths.Length < 1)
            //            {
            //                if (TempLabelHtml.IndexOf(LabelCode) > -1)
            //                {
            //                    temphtml = HttpUtility.HtmlDecode(ds_ChildLableSeting.Tables[0].Rows[i]["LabelContent"].ToString());
            //                    //temphtml = ds_ChildLableSeting.Tables[0].Rows[i]["LabelContent"].ToString().Replace("'", "~");

            //                    MasterSQL = ds_ChildLableSeting.Tables[0].Rows[i]["MasterSQL"].ToString();
            //                    SlaveSQL = ds_ChildLableSeting.Tables[0].Rows[i]["QuerySQL"].ToString();
            //                    ToDSID = int.Parse(ds_ChildLableSeting.Tables[0].Rows[i]["ToDSID"].ToString());
            //                    if (!string.IsNullOrEmpty(MasterSQL))
            //                    {
            //                        temphtml = ReplaceMasterValue(temphtml, MasterSQL, ToDSID);
            //                    }
            //                    if (!string.IsNullOrEmpty(SlaveSQL))
            //                    {
            //                        temphtml = ReplaceSlaveValue(temphtml, SlaveSQL, ToDSID);
            //                    }
            //                    TempLabelHtml = TempLabelHtml.Replace(LabelCode, temphtml);
            //                    TempLabelHtml = ReplaceHtmlTag(TempLabelHtml);

            //                }
            //                parentid = ds_ChildLableSeting.Tables[0].Rows[i]["AutoId"].ToString();
            //                InitLabelHtml(parentid);
            //            }
            //        }
            //        else
            //        {
            //            TempLabelHtml = TempLabelHtml.Replace(LabelCode, "");
            //        }
            //    }
            //}
            #endregion

            //DataSet ds_ChildLableSeting = DBAccess.GetDataSet("SELECT AutoId,parentid,ToDSID,MasterSQL,QuerySQL,LabelTitle,LabelCode,LabelContent,AuthorizedRoles  FROM LabelModule where parentid = " + parentid + "", "ConnectionString");
            if (htmlDT.Select("parentid=" + parentid).Count() > 0)
            {
                DataRow[] drs = htmlDT.Select("parentid=" + parentid);
                for (int i = 0; i < drs.Count(); i++)
                {
                    string temphtml = "";
                    string LabelCode = "";
                    string MasterSQL = "";
                    string SlaveSQL = "";
                    string AuthorizedRoles = "";
                    int ToDSID = 0;
                    AuthorizedRoles = drs[i]["AuthorizedRoles"].ToString();
                    LabelCode = drs[i]["LabelCode"].ToString();
                    if (PortalSecurity.IsInRoles(AuthorizedRoles) || string.IsNullOrEmpty(AuthorizedRoles))
                    {
                        string str = @"{#(.*)#}";//表达试
                        Match macths = Regex.Match(LabelCode, str);
                        if (macths.Length < 1)
                        {
                            if (TempLabelHtml.IndexOf(LabelCode) > -1)
                            {
                                temphtml = HttpUtility.HtmlDecode(drs[i]["LabelContent"].ToString());
                                //temphtml = ds_ChildLableSeting.Tables[0].Rows[i]["LabelContent"].ToString().Replace("'", "~");

                                MasterSQL = drs[i]["MasterSQL"].ToString();
                                SlaveSQL = drs[i]["QuerySQL"].ToString();
                                ToDSID = int.Parse(drs[i]["ToDSID"].ToString());
                                if (!string.IsNullOrEmpty(MasterSQL))
                                {
                                    temphtml = ReplaceMasterValue(temphtml, MasterSQL, ToDSID);
                                }
                                if (!string.IsNullOrEmpty(SlaveSQL))
                                {
                                    temphtml = ReplaceSlaveValue(temphtml, SlaveSQL, ToDSID);
                                }
                                TempLabelHtml = TempLabelHtml.Replace(LabelCode, temphtml);
                                TempLabelHtml = ReplaceHtmlTag(TempLabelHtml);

                            }
                            parentid = drs[i]["AutoId"].ToString();
                            InitLabelHtml(parentid);
                        }
                    }
                    else
                    {
                        TempLabelHtml = TempLabelHtml.Replace(LabelCode, "");
                    }
                }
            }
        }
        //移动平台
        void InitMobileLabelHtml(string parentid)
        {
            #region 老版2014-08-08
            //DataSet ds_ChildLableSeting = DBAccess.GetDataSet("SELECT AutoId,parentid,ToDSID,MasterSQL,QuerySQL,LabelTitle,LabelCode,MobileLabelContent as LabelContent,AuthorizedRoles  FROM LabelModule where parentid = " + parentid + "", "ConnectionString");
            //if (ds_ChildLableSeting.Tables.Count > 0)
            //{
            //    for (int i = 0; i < ds_ChildLableSeting.Tables[0].Rows.Count; i++)
            //    {
            //        string temphtml = "";
            //        string LabelCode = "";
            //        string MasterSQL = "";
            //        string SlaveSQL = "";
            //        string AuthorizedRoles = "";
            //        int ToDSID = 0;
            //        AuthorizedRoles = ds_ChildLableSeting.Tables[0].Rows[i]["AuthorizedRoles"].ToString();
            //        LabelCode = ds_ChildLableSeting.Tables[0].Rows[i]["LabelCode"].ToString();
            //        if (PortalSecurity.IsInRoles(AuthorizedRoles) || string.IsNullOrEmpty(AuthorizedRoles))
            //        {
            //            string str = @"{#(.*)#}";//表达试
            //            Match macths = Regex.Match(LabelCode, str);
            //            if (macths.Length < 1)
            //            {
            //                if (TempLabelHtml.IndexOf(LabelCode) > -1)
            //                {
            //                    temphtml = HttpUtility.HtmlDecode(ds_ChildLableSeting.Tables[0].Rows[i]["LabelContent"].ToString());
            //                    //temphtml = ds_ChildLableSeting.Tables[0].Rows[i]["LabelContent"].ToString().Replace("'", "~");

            //                    MasterSQL = ds_ChildLableSeting.Tables[0].Rows[i]["MasterSQL"].ToString();
            //                    SlaveSQL = ds_ChildLableSeting.Tables[0].Rows[i]["QuerySQL"].ToString();
            //                    ToDSID = int.Parse(ds_ChildLableSeting.Tables[0].Rows[i]["ToDSID"].ToString());
            //                    if (!string.IsNullOrEmpty(MasterSQL))
            //                    {
            //                        temphtml = ReplaceMasterValue(temphtml, MasterSQL, ToDSID);
            //                    }
            //                    if (!string.IsNullOrEmpty(SlaveSQL))
            //                    {
            //                        temphtml = ReplaceSlaveValue(temphtml, SlaveSQL, ToDSID);
            //                    }
            //                    TempLabelHtml = TempLabelHtml.Replace(LabelCode, temphtml);
            //                    TempLabelHtml = ReplaceMobileHtmlTag(TempLabelHtml);

            //                }
            //                parentid = ds_ChildLableSeting.Tables[0].Rows[i]["AutoId"].ToString();
            //                InitMobileLabelHtml(parentid);
            //            }
            //        }
            //        else
            //        {
            //            TempLabelHtml = TempLabelHtml.Replace(LabelCode, "");
            //        }
            //    }
            //}
            #endregion

            //DataSet ds_ChildLableSeting = DBAccess.GetDataSet("SELECT AutoId,parentid,ToDSID,MasterSQL,QuerySQL,LabelTitle,LabelCode,MobileLabelContent as LabelContent,AuthorizedRoles  FROM LabelModule where parentid = " + parentid + "", "ConnectionString");

            if (htmlDT.Select("parentid=" + parentid).Count() > 0)
            {
                DataRow[] drs = htmlDT.Select("parentid=" + parentid);
                for (int i = 0; i < drs.Count(); i++)
                {
                    string temphtml = "";
                    string LabelCode = "";
                    string MasterSQL = "";
                    string SlaveSQL = "";
                    string AuthorizedRoles = "";
                    int ToDSID = 0;
                    AuthorizedRoles = drs[i]["AuthorizedRoles"].ToString();
                    LabelCode = drs[i]["LabelCode"].ToString();
                    if (PortalSecurity.IsInRoles(AuthorizedRoles) || string.IsNullOrEmpty(AuthorizedRoles))
                    {
                        string str = @"{#(.*)#}";//表达试
                        Match macths = Regex.Match(LabelCode, str);
                        if (macths.Length < 1)
                        {
                            if (TempLabelHtml.IndexOf(LabelCode) > -1)
                            {
                                temphtml = HttpUtility.HtmlDecode(drs[i]["MobileLabelContent"].ToString());
                                //temphtml = ds_ChildLableSeting.Tables[0].Rows[i]["LabelContent"].ToString().Replace("'", "~");

                                MasterSQL = drs[i]["MasterSQL"].ToString();
                                SlaveSQL = drs[i]["QuerySQL"].ToString();
                                ToDSID = int.Parse(drs[i]["ToDSID"].ToString());
                                if (!string.IsNullOrEmpty(MasterSQL))
                                {
                                    temphtml = ReplaceMasterValue(temphtml, MasterSQL, ToDSID);
                                }
                                if (!string.IsNullOrEmpty(SlaveSQL))
                                {
                                    temphtml = ReplaceSlaveValue(temphtml, SlaveSQL, ToDSID);
                                }
                                TempLabelHtml = TempLabelHtml.Replace(LabelCode, temphtml);
                                TempLabelHtml = ReplaceMobileHtmlTag(TempLabelHtml);

                            }
                            parentid = drs[i]["AutoId"].ToString();
                            InitMobileLabelHtml(parentid);
                        }
                    }
                    else
                    {
                        TempLabelHtml = TempLabelHtml.Replace(LabelCode, "");
                    }
                }
            }
        }
        string ReplacePage(string temphtml, string KeyName, int PageSize, int RowCount, int PageIndex)
        {
            string strUrl = UrlPath;

            if (Page != null)
            {
                string queryString = StringQuery;
                NameValueCollection col = ASPNetPortal.Components.common_function.GetQueryString(queryString);
                strUrl = strUrl.Replace("&page=" + col["page"], "");
                strUrl = strUrl.Replace("?page=" + col["page"], "?raa=0");
            }

            int PageCount = 0;

            //计算页数
            if (RowCount % PageSize == 0)
            {
                PageCount = RowCount / PageSize;
            }
            else
            {
                PageCount = RowCount / PageSize + 1;
            }
            string FirstHtml = "<a href='" + strUrl + "&page=1' class='disabled'>首页</a>";
            string PrevHtml = "";
            string NextHtml = "";
            string LastHtml = "<a href='" + strUrl + "&page=" + PageCount.ToString() + "' class='disabled'>尾页</a>";
            string pagehtml = "";
            if (RowCount < PageSize)
            {

                pagehtml = "<div class='badoo'><span class='disabled'> <  上一页</span><span class='current'>1</span><span class='disabled'>下一页  > </a></div>";
            }
            else
            {
                if (PageIndex == PageCount)
                {
                    NextHtml = "<span class='disabled'> 下一页  ></span>";
                }
                else
                {
                    NextHtml = "<a href='" + strUrl + "&page=" + (PageIndex + 1).ToString() + "' class='disabled'>下一页  ></a>";
                }
                if (PageIndex == 1)
                {
                    PrevHtml = "<span class='disabled'> <  上一页</span>";
                }
                else
                {
                    PrevHtml = "<a href='" + strUrl + "&page=" + (PageIndex - 1).ToString() + "' class='disabled'><  上一页 </a>";
                }
                //
                int starpage = 1;
                if (PageCount <= 8)
                {
                    starpage = 1;
                }
                else
                {
                    int pp = (PageCount / 8 + 1);
                    starpage = PageIndex / pp;

                    starpage = starpage * pp;
                }

                int npage = 0;
                for (int n = 1; n <= 8; n++)
                {
                    npage = starpage + n - 1;
                    if (npage > PageCount)
                    {
                        break;
                    }
                    if (npage == PageIndex)
                    {
                        pagehtml += "<span class='current'>" + npage.ToString() + "</span>";
                    }
                    else
                    {
                        pagehtml += "<a href='" + strUrl + "&page=" + npage.ToString() + "'>" + npage.ToString() + "</a>";
                    }
                }
                string endpage = "";
                if (PageCount - npage >= 2)
                {

                    endpage = " ...<a href='" + strUrl + "&page=" + (PageCount - 1).ToString() + "'>" + (PageCount - 1).ToString() + "</a><a href='#?page=" + PageCount.ToString() + "'>" + PageCount.ToString() + "</a>";
                }
                else if (PageCount - npage == 1)
                {

                    endpage = " ...<a href='" + strUrl + "&page=" + PageCount.ToString() + "'>" + PageCount.ToString() + "</a>";
                }
                pagehtml = "<div class='badoo'>" + FirstHtml + PrevHtml + pagehtml + endpage + NextHtml + LastHtml + "</div>";

            }
            temphtml = temphtml.Replace("{*page," + PageSize.ToString() + "," + KeyName + "}", pagehtml);
            return temphtml;
        }

        string ReplaceMasterValue(string temphtml, string MasterSQL, int ToDSID)
        {
            MasterSQL = MasterSQL.Replace("{*$UserName$}", UserID);
            MasterSQL = MasterSQL.Replace("{$UserName$}", UserID);
            MasterSQL = ASPNetPortal.Components.common_function.ReplaceQuery(MasterSQL);
            MasterSQL = MasterSQL.Replace("~", "'");
            DataSet ds = DBAccess.GetDataSet(MasterSQL, ToDSID);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    {
                        temphtml = temphtml.Replace("{*$" + ds.Tables[0].Columns[i].ColumnName + "$}", ds.Tables[0].Rows[0][ds.Tables[0].Columns[i].ColumnName].ToString());
                        temphtml = temphtml.Replace("{$" + ds.Tables[0].Columns[i].ColumnName + "$}", ds.Tables[0].Rows[0][ds.Tables[0].Columns[i].ColumnName].ToString());
                    }
                }
            }
            return temphtml;
        }

        string ReplaceSlaveValue(string temphtml, string SlaveSQL, int ToDSID)
        {
            //<loop></loop>
            SlaveSQL = SlaveSQL.Replace("{*$UserName$}", UserID);
            SlaveSQL = SlaveSQL.Replace("{$UserName$}", UserID);
            SlaveSQL = ASPNetPortal.Components.common_function.ReplaceQuery(SlaveSQL);
            DataSet ds = new DataSet();
            if (temphtml.IndexOf("{*page") > -1)
            {
                int PageSize = int.Parse(System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(temphtml, "{*page,")[1], ",")[0]);
                string KeyName = System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(temphtml, "{*page,")[1], ",")[1], "}")[0];
                int PageIndex = 1;
                if (Page != null)
                {
                    try
                    {
                        PageIndex = int.Parse(Page);
                    }
                    catch
                    {
                        PageIndex = 1;
                    }
                }
                else
                {
                    PageIndex = 1;
                }
                DB_sql dbsql = new DB_sql();

                string DSConnectionString = Components.EncUtil.DesDecrypt(DBAccess.DataSource.Select("dsid = " + ToDSID.ToString())[0]["ConnectionString"].ToString());
                int RowCount = 0;
                SlaveSQL = SlaveSQL.Replace("~", "'");
                if (SlaveSQL.ToLower().IndexOf("exec ") > -1)
                {
                    DataTable dt = DBAccess.GetDataTable(SlaveSQL, ToDSID);
                    ds = Pager.Paging(dt, KeyName, PageSize, PageIndex, out RowCount);
                }
                else
                {
                    ds = Pager.Paging(SlaveSQL, KeyName, PageSize, PageIndex, DSConnectionString, out RowCount);
                }
                temphtml = ReplacePage(temphtml, KeyName, PageSize, RowCount, PageIndex);
            }
            else
            {
                SlaveSQL = SlaveSQL.Replace("~", "'");
                ds = DBAccess.GetDataSet(SlaveSQL, ToDSID);
            }
            if (temphtml.IndexOf("<loop>") > -1 && temphtml.IndexOf("</loop>") > -1)
            {
                string temphtml_1 = System.Text.RegularExpressions.Regex.Split(temphtml, "<loop>")[0];
                string temphtml_2 = System.Text.RegularExpressions.Regex.Split(System.Text.RegularExpressions.Regex.Split(temphtml, "<loop>")[1], "</loop>")[0];
                string temphtml_3 = System.Text.RegularExpressions.Regex.Split(temphtml, "</loop>")[1];
                temphtml = temphtml_1;
                if (ds.Tables.Count > 0)
                {
                    for (int k = 0; k < ds.Tables[0].Rows.Count; k++)
                    {
                        string temphtml_2_temp = temphtml_2;
                        for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                        {
                            temphtml_2_temp = System.Text.RegularExpressions.Regex.Replace(temphtml_2_temp, "[{][*]#" + ds.Tables[0].Columns[i].ColumnName + "}", ds.Tables[0].Rows[k][ds.Tables[0].Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            temphtml_2_temp = System.Text.RegularExpressions.Regex.Replace(temphtml_2_temp, "[{]#" + ds.Tables[0].Columns[i].ColumnName + "}", ds.Tables[0].Rows[k][ds.Tables[0].Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        }
                        temphtml += temphtml_2_temp;
                    }
                }

                temphtml += temphtml_3;
            }
            else
            {//如果没有LOOP,就直接只返回一行数据
                if (ds.Tables.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                    {
                        temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{][*]#" + ds.Tables[0].Columns[i].ColumnName + "}", ds.Tables[0].Rows[0][ds.Tables[0].Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        temphtml = System.Text.RegularExpressions.Regex.Replace(temphtml, "[{]#" + ds.Tables[0].Columns[i].ColumnName + "}", ds.Tables[0].Rows[0][ds.Tables[0].Columns[i].ColumnName].ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                }
            }
            return temphtml;
        }

        /// <summary>
        /// 动态标签
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public string ReplaceHtmlTag(string temp)
        {
            //-----动态标签-----
            string str = @"{#(\w*)\*(\w|,)*#}";//表达试 
            StringBuilder GetTemp = new StringBuilder();
            GetTemp.Append(temp);
            MatchCollection macths = Regex.Matches(GetTemp.ToString(), str, RegexOptions.RightToLeft);
            foreach (Match macth in macths)
            {
                //{#mincalss*212,2342#}
                string temptag = "";
                temptag = macth.ToString();//把匹配的项付给变量
                int templength = temptag.ToString().IndexOf("{#") + 2; //获取标签头位置
                int templength2 = temptag.ToString().IndexOf("#}");//获取标签尾位置
                if (temptag.ToString().Substring(templength, templength2 - templength).ToString() != null)
                {
                    string Str_Tag = temptag.ToString().Substring(templength, templength2 - templength);//找到标签
                    //mincalss*212,2342
                    string[] tempString = Str_Tag.Split('*');//分类标签
                    string Tag_Name = tempString[0].ToString();  //获得标签名称,在之后做调用前的比较。
                    //mincalss
                    string Tag_ID = tempString[1].ToString();//分类ID,列,行,最大字符数
                    //212,2342
                    string[] tempid = GetTagID(Tag_ID);//获取个参数
                    //[212],[2342]
                    //开始替换标签
                    try
                    {

                        //DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT top 1  AutoId,LabelCode,LabelContent,ToDSID,MasterSQL,QuerySQL,AuthorizedRoles  FROM LabelModule where LabelCode like '%{#" + Tag_Name + "*%'", "ConnectionString");
                        if (htmlDT.Select("LabelCode like '%" + Tag_Name + "%'").Count() > 0)
                        {
                            //if (ds_LableSeting.Tables[0].Rows.Count > 0)
                            //{

                            DataRow dr = htmlDT.Select("LabelCode like '%{#" + Tag_Name + "*%'")[0];
                            string temphtml = "";
                            string LabelCode = "";
                            string MasterSQL = "";
                            string SlaveSQL = "";
                            string AuthorizedRoles = "";
                            int ToDSID = 0;
                            LabelCode = dr["LabelCode"].ToString();
                            if (PortalSecurity.IsInRoles(AuthorizedRoles) || string.IsNullOrEmpty(AuthorizedRoles))
                            {
                                temphtml = HttpUtility.HtmlDecode(dr["LabelContent"].ToString());
                                MasterSQL = dr["MasterSQL"].ToString().Replace("~", "'");
                                SlaveSQL = dr["QuerySQL"].ToString().Replace("~", "'");
                                ToDSID = int.Parse(dr["ToDSID"].ToString());
                                if (tempid.Length > 0)
                                {
                                    for (int i = 0; i < tempid.Length; i++)
                                    {

                                        MasterSQL = MasterSQL.Replace("'{" + (i + 1) + "}'", "'" + tempid[i] + "'");
                                        MasterSQL = MasterSQL.Replace("{" + (i + 1) + "}", "'" + tempid[i] + "'");
                                        SlaveSQL = SlaveSQL.Replace("'{" + (i + 1) + "}'", "'" + tempid[i] + "'");
                                        SlaveSQL = SlaveSQL.Replace("{" + (i + 1) + "}", "'" + tempid[i] + "'");
                                    }
                                }
                                if (!string.IsNullOrEmpty(MasterSQL))
                                {
                                    temphtml = ReplaceMasterValue(temphtml, MasterSQL, ToDSID);
                                }
                                if (!string.IsNullOrEmpty(SlaveSQL))
                                {
                                    temphtml = ReplaceSlaveValue(temphtml, SlaveSQL, ToDSID);
                                }
                                temp = temp.Replace(temptag, temphtml);

                            }
                            else
                            {
                                temp = temp.Replace(temptag, "");
                            }
                            //}
                        }
                    }
                    catch (Exception ee)
                    {
                        HttpContext.Current.Response.Write("可能是你的" + Tag_Name + "不存在或标签的参数个数不对，请仔细检查" + ee.Message);
                    }
                    finally
                    {

                    }
                }
            }
            return temp;
        }

        //移动客户端
        public string ReplaceMobileHtmlTag(string temp)
        {
            //-----动态标签-----
            string str = @"{#(\w*)\*(\w|,)*#}";//表达试 
            StringBuilder GetTemp = new StringBuilder();
            GetTemp.Append(temp);
            MatchCollection macths = Regex.Matches(GetTemp.ToString(), str, RegexOptions.RightToLeft);
            foreach (Match macth in macths)
            {
                //{#mincalss*212,2342#}
                string temptag = "";
                temptag = macth.ToString();//把匹配的项付给变量
                int templength = temptag.ToString().IndexOf("{#") + 2; //获取标签头位置
                int templength2 = temptag.ToString().IndexOf("#}");//获取标签尾位置
                if (temptag.ToString().Substring(templength, templength2 - templength).ToString() != null)
                {
                    string Str_Tag = temptag.ToString().Substring(templength, templength2 - templength);//找到标签
                    //mincalss*212,2342
                    string[] tempString = Str_Tag.Split('*');//分类标签
                    string Tag_Name = tempString[0].ToString();  //获得标签名称,在之后做调用前的比较。
                    //mincalss
                    string Tag_ID = tempString[1].ToString();//分类ID,列,行,最大字符数
                    //212,2342
                    string[] tempid = GetTagID(Tag_ID);//获取个参数
                    //[212],[2342]
                    //开始替换标签
                    try
                    {
                        //DataSet ds_LableSeting = DBAccess.GetDataSet("SELECT top 1  AutoId,LabelCode,MobileLabelContent as  LabelContent,ToDSID,MasterSQL,QuerySQL,AuthorizedRoles  FROM LabelModule where LabelCode like '%{#" + Tag_Name + "*%'", "ConnectionString");


                        if (htmlDT.Select("LabelCode like '%{#" + Tag_Name + "*%'").Count() > 0)
                        {
                            //if (ds_LableSeting.Tables[0].Rows.Count > 0)
                            //{
                            DataRow dr = htmlDT.Select("LabelCode like '%" + Tag_Name + "%'")[0];
                            string temphtml = "";
                            string LabelCode = "";
                            string MasterSQL = "";
                            string SlaveSQL = "";
                            string AuthorizedRoles = "";
                            int ToDSID = 0;
                            LabelCode = dr["LabelCode"].ToString();
                            if (PortalSecurity.IsInRoles(AuthorizedRoles) || string.IsNullOrEmpty(AuthorizedRoles))
                            {
                                temphtml = HttpUtility.HtmlDecode(dr["MobileLabelContent"].ToString());
                                MasterSQL = dr["MasterSQL"].ToString().Replace("~", "'");
                                SlaveSQL = dr["QuerySQL"].ToString().Replace("~", "'");
                                ToDSID = int.Parse(dr["ToDSID"].ToString());
                                if (tempid.Length > 0)
                                {
                                    for (int i = 0; i < tempid.Length; i++)
                                    {

                                        MasterSQL = MasterSQL.Replace("'{" + (i + 1) + "}'", "'" + tempid[i] + "'");
                                        MasterSQL = MasterSQL.Replace("{" + (i + 1) + "}", "'" + tempid[i] + "'");
                                        SlaveSQL = SlaveSQL.Replace("'{" + (i + 1) + "}'", "'" + tempid[i] + "'");
                                        SlaveSQL = SlaveSQL.Replace("{" + (i + 1) + "}", "'" + tempid[i] + "'");
                                    }
                                }
                                if (!string.IsNullOrEmpty(MasterSQL))
                                {
                                    temphtml = ReplaceMasterValue(temphtml, MasterSQL, ToDSID);
                                }
                                if (!string.IsNullOrEmpty(SlaveSQL))
                                {
                                    temphtml = ReplaceSlaveValue(temphtml, SlaveSQL, ToDSID);
                                }
                                temp = temp.Replace(temptag, temphtml);

                            }
                            else
                            {
                                temp = temp.Replace(temptag, "");
                            }
                        }
                        //}
                    }
                    catch (Exception ee)
                    {
                        HttpContext.Current.Response.Write("可能是你的" + Tag_Name + "不存在或标签的参数个数不对，请仔细检查" + ee.Message);
                    }
                    finally
                    {

                    }
                }
            }
            return temp;
        }
        #region-----------------返回字符串数组
        /// <summary>
        /// 返回字符串数组
        /// </summary>
        /// <param name="stemp"></param>
        /// <returns></returns>
        string[] GetTagID(string stemp)
        {
            string[] temp = null;
            try
            {

                if (stemp != "")
                {
                    temp = stemp.Split(',');

                }
            }
            catch
            {

            }
            finally
            {


            }
            return temp;
        }
        #endregion

    }
}