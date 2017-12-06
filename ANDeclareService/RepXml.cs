using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Threading;
using Alog.Common;
using ASPNetPortal.DB;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ANDeclareService
{
    public class RepXml
    {
        //需要仓易宝签名的
        string signCybownUserId = ClsLog.GetAppSettings("signCybownUserId");
        //需要发送给oms的
        string signOMSownUserId = ClsLog.GetAppSettings("signOMSownUserId");
        //全部内容post
        string allPost = ClsLog.GetAppSettings("allPost");
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
        /// 运行的版本号jupRepVersion
        /// </summary>
        private static string _jupRepVersion;
        public static string jupRepVersion
        {
            get
            {
                if (_jupRepVersion == null)
                {
                    _jupRepVersion = ClsLog.GetAppSettings("JupRepVersion");
                }
                return _jupRepVersion;
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
                                  ,[CompleteSQL],[ErrorSQL] from RepXmlSet where isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and ISNULL([parentid],0) = 0 and  RepVersion=" + repVersion;
                    return _dtRepXmlSet = DBAccess.GetDataTable(sql, 7);
                }

                return _dtRepXmlSet;
            }
            set
            {
                _dtRepXmlSet = value;
            }
        }


        #region 珊瑚云报文模板
        /// <summary>
        /// XML报文模板设置
        /// </summary>
        private static DataTable _dtJupXmlSet;
        public static DataTable dtJupXmlSet
        {
            get
            {
                if (_dtJupXmlSet == null)
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
                                  ,[CompleteSQL],ErrorSQL from RepXmlSet where  isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and  RepVersion=" + repVersion;
                    return _dtJupXmlSet = DBAccess.GetDataTable(sql, 7);
                }

                return _dtJupXmlSet;
            }
            set
            {
                _dtJupXmlSet = value;
            }
        }

        #endregion

        /// <summary>
        /// XML报文模板设置
        /// </summary>
        private static DataTable _dtJupRepXmlSet;
        public static DataTable dtJupRepXmlSet
        {
            get
            {
                if (_dtJupRepXmlSet == null)
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
                                  ,[CompleteSQL],[ErrorSQL] from RepXmlSet where isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1  AND parentid=0  and  RepVersion=" + jupRepVersion;
                    return _dtJupRepXmlSet = DBAccess.GetDataTable(sql, 7);
                }

                return _dtJupRepXmlSet;
            }
            set
            {
                _dtJupRepXmlSet = value;
            }
        }


        #endregion

        #region Post数据
        /// <summary>
        /// Post数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static string PostData(string url, string postData, out string err)
        {

            //lock (padlock)
            //{
            Thread.Sleep(50);
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            Encoding encoding = Encoding.GetEncoding("utf-8");
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
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

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
            //}
        }

        #endregion


        public void ThreadHandle(object ClsParam)
        {
            ClsThreadParam cls = (ClsThreadParam)ClsParam;
            ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + cls.RepTitle + "_线程启动", cls.RepTitle);

            string retStr = string.Empty;
            int ToDSID = int.Parse(cls.dr["DSID"].ToString());
            string QuerySql = cls.dr["QuerySql"].ToString();
            string MasterSQL = cls.dr["MasterSQL"].ToString();

            string CompleteSQL = cls.dr["CompleteSQL"].ToString();
            string ErrorSQL = cls.dr["ErrorSQL"].ToString();
            string tempErrorSQL = "";


            MasterSQL = MasterSQL.Replace("~", "'");
            QuerySql = QuerySql.Replace("~", "'");

            DataTable dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);
            foreach (DataRow dr in dtMaster.Rows)   //对主表数据逐条处理，每条生成一个报文
            {


                try
                {
                    string tempQuerySql = QuerySql;
                    string tempCompleteSQL = CompleteSQL;
                    tempErrorSQL = ErrorSQL;
                    RepXml rx = new RepXml();
                    ParentTempLabelHtml = rx.ReplaceMasterValue(cls.dr["XMLContent"].ToString(), dr);//替换XML模板的主表数据
                    tempQuerySql = rx.ReplaceMasterValue(tempQuerySql, dr);//替换表体SQL(循环用)查询的的主表数据
                    tempCompleteSQL = rx.ReplaceMasterValue(tempCompleteSQL, dr);
                    tempErrorSQL = rx.ReplaceMasterValue(tempErrorSQL, dr);

                    if (!string.IsNullOrEmpty(tempQuerySql))
                    {
                        DataTable dt = DBAccess.GetDataTable(tempQuerySql, ToDSID);
                        ParentTempLabelHtml = rx.ReplaceSlaveValue(ParentTempLabelHtml, dt);
                    }

                    string parentid = cls.dr["AutoId"].ToString();

                    InitLabelXML(parentid); //递归生成报文


                    // 2.4加密及签名机制
                    //采用AES对业务参数进行加密，RSA公钥对ASE对称密钥进行加密，RSA私钥对请求参数进行签名。
                    //AES对称密钥由合作方随机生成，长度为16位，可以用26个字母和数字组成，以下简称AESKey。
                    //爱农支付会提供给合作方 三串密钥：合作方RSA私钥（以下简称merPriKey），合作方RSA公钥（以下简称merPubKey），爱农支付RSA公钥（以下简称zpayPubKey）。

                    string url = ClsLog.GetAppSettings("ANUrl");
                    string merchantNo = ClsLog.GetAppSettings("merchantNo");   //爱农平台生成，每个商户唯一的标示代码
                    //string zpayPubKey = ClsLog.GetAppSettings("zpayPubKey");   //爱农支付RSA公钥
                    //string merPriKey = ClsLog.GetAppSettings("merPriKey");   //合作方RSA私钥
                    //RSA公钥（需要java方生成）
                    string publickey = "<RSAKeyValue><Modulus>r+cQloHNZClGF/l90sV98Ad1F8M4Oz4qCPJ78Imdr4se8UYC0NosbtP8WLTOA9boj+aU7AogKhVEMN2esL7HLvwPi5Z4b1Q2ADfIbqzCGzcPhLXuEuwgc4/GplqQpNmYUz5SfYzGiMmnc98iitc09YraKyTYczQ1r0Fv9ncrNpROUh6YkaCHFta8QzVnfAcGmE10iA2lUra8fy0UbLbbkDPUsjSMnlb9MweNZLeFtXnmjdmOuXYE4XYgt1x+9nwrPoG4ht5T+sXr4zAovQARnCGqYGjUpwnG7ZDooIfjXLX4J/oGCrVz8rnwTwww6rRhAEXKCXd3Sm1vEimAwMk8aw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";



                    //AES对称密钥由合作方随机生成，长度为16位，可以用26个字母和数字组成，以下简称AESKey。
                    string AESKey = "xy" + DateTime.Now.ToString("yyyyMMhhHHmmss");

                    //  ParentTempLabelHtml = "1";
                    //       string encryptedData = AESHelper.Encrypt(ParentTempLabelHtml, AESKey);

                    string encryptedData = AESHelper.Encrypt(ParentTempLabelHtml, AESKey);
                    string encryptedKey = RSAHelper.RSAEncrypt(publickey, AESKey);

                    IDictionary<string, string> parameters = new Dictionary<string, string>();

                    parameters.Add("version", "1.0");
                    parameters.Add("merchantNo", merchantNo);
                    parameters.Add("interfaceCode", cls.dr["LabelCode"].ToString());
                    parameters.Add("encryptedData", encryptedData);
                    parameters.Add("encryptedKey", encryptedKey);

                    // -3,-85,-5,-125,56,-92,-50,79
                    //     merPriKey = mctPrivateKeyData;

                    //RSA秘钥  java生成c#解密xml     
                    string privatekey = "<RSAKeyValue><Modulus>iKmvHtrnfmIH+MQ8mKGop99VKN98VFGhzgVIljJQKK5/ChEjFuqHIs3mhRCNSu9ymTUnyQYdvaVm0i8+ZrHreCJOqcucK80OBzhWDzIBzWKLgZ4CycsUgIW1dpnk17x3gmSONOwDG4ph+PFyNBRl3ie4+tw5QPZ0QZs5dK6Y4rSAm41QvPz1PwHIIxPYNDCGcqzhuUWFNveNayVgZZi6EJC1wvcPAk1o/S2zT625hfGDIhgOVsj+h3J3BzxNKWhqO6k+dsNQLGbq2N91o5FN28+geE6MqnER0ITlqH4PaMJiCMLGUMoA3YINUWBRBNZ6YXe0etzYn88l3CKG3eILLw==</Modulus><Exponent>AQAB</Exponent><P>2xMKezOmu52/FA90/WCz4eMOBbfHjm/U+mYdayKnHiM7xL49qfl6LJT2TqKEWXSKhVwflDUCHTeLWe1n7NoSbzw7oWyFW6IJT/G9XUVTfgjAXud5PRVpCjID4YoOjxsxNXn09fcvFYX5o7ZGpDdJN4bbXpsuZBcGdt+gWC0P1MU=</P><Q>n7KfW2fc/h/OMaOAt4kwiJgh2aErUvXS9Y6IWqW4locBjzASkNTLURLl7PBN1Sh81Fkq4WMNQ7Kd6fW5NN8h58GhDe8xB31Z70haZlTTvk+wRlzynccdT7Fg12v14BW9gYg1hFVo+uR0f2FKG/yqnsxojHCMjkKaMoW35jcB52M=</Q><DP>IeiwlqFIDYI9MT2zPgTZ0vzyDrCDkuh8bsWfiBW1CFUryygnI0gaQQxNk55UHgSL7Fh2CtqpmRwf1auJin+2msEX+cRyX9yU1Gr2hJlWcwunuwjZGztVJt7W+vIDjuMGmuBlqiy4fPxfx5dPF9v22UFmcx5R7+wgdAXpK7zN1oE=</DP><DQ>TT7FPh1bE8Cmp/QCSsSdKTUNCouevR4S0JwE8HPAeW8MHgqOsGd9gNW3SYhi3XwcBUqsJyEWRWycXZJx51UWvVc3Y9oSySCk480HjP/y+9bn2OEUqaSqVOXjhc++uolJhOIxoLT/dnwSrZqNkkQA85bD60p1ckpc85sifMSWsyk=</DQ><InverseQ>EeIDZUaXdJxL7Ysdat5W6rfZpBy4Mc3//FsXF7hZ3ZrGkrPBzCAE1T2x4K3ra1kxEr7uJapU3YSZ6GAcpyncUSU2bklTyrCma9OG2TpsnfiDCFj1a3d6iKf4IrkHr0mPN1EffSZLE2rh2iY+j65lClXb4iHMYk23IXc+2qx7iNs=</InverseQ><D>NKwJtb+zF0/bOLMRk9ZCtajrHpy8Q5dO4KPCrTjpEjTm+89NEekEepKXNVQC0Q3mwkvtcJot3kkgjwPsbhJG6f93CBRUtcELlhbNfH/OmAVSNXvcIUBnP33JSdPYkpmuChCNirIoJHY6eoM8e/wcovn2XqGAY5xZhGu6QlGE1WMgbaaGDN6345YvK91BxsH8yW9h+hP/1Y106BYYVl6CyUR74UUHZqJivnhuyNho+oo9uxSmuWO67sMmtdTO8ijv/ZKY2VkrCCgQ/lqDMQOzikmSUhYeZ3n+t6L99y/PY5TuRtVt/vPsc74KOYvZidjAWET3byDGqugBypr+QgzaWQ==</D></RSAKeyValue>";


                    //生成签名
                    string sign = "";

                    string querystr = GetSignStr(parameters);
                    //  querystr = System.Web.HttpUtility.UrlEncode("xy1234MMhhHHmmss", Encoding.UTF8); 
                    //   querystr = "1";
                    string HashData = null;  //Hash描述
                    //指定编码格式转换扯base64
                    querystr = RSAHelper.EncodeBase64(Encoding.UTF8, querystr);
                    //获取Hash描述
                    RSAHelper.GetHash(querystr, ref HashData);
                    //生成签名
                    RSAHelper.SignatureFormatter(privatekey, HashData, ref sign);

                    parameters.Add("signedData", sign);

                    string sb = "";
                    sb += "version=" + parameters["version"];
                    sb += "&merchantNo=" + parameters["merchantNo"];
                    sb += "&interfaceCode=" + parameters["interfaceCode"];
                    sb += "&encryptedData=" + System.Web.HttpUtility.UrlEncode(parameters["encryptedData"]);
                    sb += "&encryptedKey=" + System.Web.HttpUtility.UrlEncode(parameters["encryptedKey"]);
                    sb += "&signedData=" + System.Web.HttpUtility.UrlEncode(parameters["signedData"]);

                    string err = string.Empty;

                    if (cls.dr["LabelCode"].ToString() == "H1002")
                    {
                        url += "dataReportApply";
                    }
                    if (cls.dr["LabelCode"].ToString() == "H2002")
                    {
                        url += "singleOrderQuery";
                    }

                    retStr = PostData(url, sb, out err);
                    //返回值处理

                    JObject jo = new JObject();
                    jo = (JObject)JsonConvert.DeserializeObject(retStr);
                    string rtencryptedKey = jo["encryptedKey"].ToString();
                    string rtencryptedData = jo["encryptedData"].ToString();
                    string rtsignedData = jo["signedData"].ToString();

                    //使用RSA解密出AESKey 
                    string rtAESKey = RSAHelper.RSADecryptLong(privatekey, rtencryptedKey);
                    //使用 AES解密出encryptedData
                    rtencryptedData = AESHelper.AesDecrypt(rtencryptedData, rtAESKey);

                    #region 签名验证(暂时不进行验签)

                    //string HashData2 = null;  //Hash描述
                    //IDictionary<string, string> signParameters = new Dictionary<string, string>();
                    //signParameters.Add("version", "1.0");
                    //signParameters.Add("merchantNo", jo["merchantNo"].ToString());
                    //signParameters.Add("interfaceCode", jo["interfaceCode"].ToString());
                    //signParameters.Add("encryptedData", jo["encryptedData"].ToString());
                    //signParameters.Add("encryptedKey", rtencryptedKey);

                    ////指定编码格式转换扯base64
                    //string singquerystr = RSAHelper.EncodeBase64(Encoding.UTF8, GetSignStr(signParameters));
                    ////获取Hash描述
                    //RSAHelper.GetHash(singquerystr, ref HashData2);

                    //bool signbool = RSAHelper.SignatureDeformatter(publickey, HashData2, rtsignedData);


                    #endregion


                    string result = string.Empty;

                    //{"merchantNo":"M100000691","batchNo":"2017011300","customCode":"FZ001","transFlowNo":"D59F3265-FB54-4176-8BD4-C7D2A8289EDC","returnCode":"TRD0002","returnMsg":"接受成功"}
                    //                        TRD0002	受理成功		申报类交易
                    //TRD0003	受理失败		
                    //TRD3003	商户订单已存在		
                    //TRD30021	无法根据订单号找到对应文件		
                    //E3001	系统异常		
                    //TRD0000	交易成功		查询类交易时返回，仅表示当前查询返回成功，具体申报结果需要通过状态获取
                    //TRD30010	商户订单不存在		

                    JObject rtjo = new JObject();
                    rtjo = (JObject)JsonConvert.DeserializeObject(rtencryptedData);
                    string returnCode = rtjo["returnCode"].ToString();
                    string returnMsg = rtjo["returnMsg"].ToString();
                    string batchNo = "";
                    if (cls.dr["LabelCode"].ToString() == "H1002")
                    {
                        batchNo = rtjo["batchNo"].ToString();
                    }
                    if (cls.dr["LabelCode"].ToString() == "H2002")
                    {
                        batchNo = rtjo["detailList"][0]["batchNo"].ToString();
                    }



                    bool resbool = false;
                    //处理结果判断
                    switch (cls.dr["LabelCode"].ToString())
                    {
                        case "H1002": if (returnCode == "TRD0002")
                            {
                                resbool = true;
                            }
                            break;

                        //orderStatus                 WAIT	待申报
                        //PROCESSING	申报中
                        //SUCCESS	申报成功
                        //FAIL	申报失败                      

                        case "H2002": if (rtjo["detailList"][0]["orderStatus"].ToString() == "SUCCESS")
                            {
                                resbool = true;
                            }
                            break;
                        default:
                            break;
                    }

                    //如果成功运行
                    if (resbool)
                    {
                        if (!string.IsNullOrEmpty(tempCompleteSQL))
                        {
                            tempCompleteSQL = tempCompleteSQL.Replace("~", "'");

                            SqlParameter[] paras = new SqlParameter[1];
                            paras[0] = new SqlParameter("@SendReturnMessage", SqlDbType.NVarChar);
                            paras[0].Value = returnMsg;
                            DBAccess.GetDataTableByParam(tempCompleteSQL, paras, ToDSID);
                        }

                        //     ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 完成后sql执行成功", cls.RepTitle);
                    }
                    else
                    {
                        //保存报文
                        ApiZJLog(cls.dr["LabelCode"].ToString(), rtencryptedData, batchNo, "", "", "0", "", "json", "", 1, 0);

                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "post完整URL：" + url, cls.RepTitle);
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "post数据：" + sb.ToString() + ParentTempLabelHtml, cls.RepTitle);
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "接口返回结果：" + rtencryptedData, cls.RepTitle);


                        if (!string.IsNullOrEmpty(tempErrorSQL))
                        {
                            tempErrorSQL = tempErrorSQL.Replace("~", "'");

                            SqlParameter[] paras = new SqlParameter[1];
                            paras[0] = new SqlParameter("@SendReturnMessage", SqlDbType.NVarChar);
                            paras[0].Value = returnMsg;
                            DBAccess.GetDataTableByParam(tempErrorSQL, paras, ToDSID);
                        }
                        //  ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 失败运行sql执行成功", cls.RepTitle);

                    }
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(tempErrorSQL))
                    {
                        tempErrorSQL = tempErrorSQL.Replace("~", "'");
                        SqlParameter[] paras = new SqlParameter[1];
                        paras[0] = new SqlParameter("@SendReturnMessage", SqlDbType.NVarChar);
                        paras[0].Value = retStr;
                        DBAccess.GetDataTableByParam(tempErrorSQL, paras, ToDSID);
                    }
                    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message + ex.StackTrace, cls.RepTitle);
                    continue;
                }

                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + cls.dr["RepTitle"].ToString() + "_线程结束", cls.RepTitle);

            }

        }
        /// <summary>
        /// 递归读取标替换签模板
        /// </summary>
        /// <param name="parentid">父标签ID</param>
        void InitLabelXML(string parentid)
        {
            #region
            RepXml rx = new RepXml();
            if (dtJupXmlSet.Select("parentid=" + parentid).Count() > 0)
            {
                DataRow[] drs = dtJupXmlSet.Select("parentid=" + parentid);
                for (int i = 0; i < drs.Count(); i++)
                {
                    string temphtml = "";
                    string LabelCode = "";
                    string MasterSQL = "";
                    string SlaveSQL = "";
                    int ToDSID = 0;
                    LabelCode = drs[i]["LabelCode"].ToString();

                    string str = @"(" + LabelCode + @"(\((\w|,|\-)*\)))|(" + LabelCode + ")";//表达

                    MatchCollection macths = Regex.Matches(ParentTempLabelHtml, str, RegexOptions.RightToLeft);
                    foreach (Match macth in macths)
                    {
                        temphtml = drs[i]["XMLContent"].ToString();

                        string temptag = "";
                        temptag = macth.ToString();//把匹配的项付给变量
                        string[] Parameters = null;
                        if (temptag.IndexOf("(") > 0)
                        {
                            Parameters = temptag.Split('(')[1].Split(')')[0].Split(',');
                        }
                        MasterSQL = drs[i]["MasterSQL"].ToString();
                        MasterSQL = rx.ReplaceParameterValue(MasterSQL, Parameters);
                        SlaveSQL = drs[i]["QuerySQL"].ToString();
                        ToDSID = int.Parse(drs[i]["DSID"].ToString());
                        DataTable dtMaster = new DataTable();
                        if (!string.IsNullOrEmpty(MasterSQL))
                        {
                            MasterSQL = MasterSQL.Replace("~", "'");

                            dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);

                            temphtml = rx.ReplaceMasterValue(temphtml, dtMaster.Rows[0]); ;
                        }
                        if (!string.IsNullOrEmpty(SlaveSQL))
                        {
                            SlaveSQL = SlaveSQL.Replace("~", "'");
                            SlaveSQL = rx.ReplaceParameterValue(SlaveSQL, Parameters);
                            if (dtMaster.Rows.Count > 0)
                            {
                                SlaveSQL = rx.ReplaceMasterValue(SlaveSQL, dtMaster.Rows[0]); ;
                            }
                            DataTable dtSlave = DBAccess.GetDataTable(SlaveSQL, ToDSID);

                            temphtml = rx.ReplaceSlaveValue(temphtml, dtSlave);
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


        public static string GetSignStr(IDictionary<string, string> parameters)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            StringBuilder query = new StringBuilder();
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append("=").Append(value).Append("&");
                }
            }
            return query.ToString().Substring(0, query.ToString().Length - 1);
        }

        /// <summary>
        /// 替换副表数据
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
        /// 替换主表数据
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


        /// <summary>
        /// 模板
        /// </summary>
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





    }
}
