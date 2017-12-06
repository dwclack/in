using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using ASPNetPortal;
using System.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Alog_WSKJSD
{
    class ZtoPostDateFunc
    {


        #region 日志记录 public static void Log2(string contents,string path = "PDA_Log")
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="contents">内容</param>
        /// <param name="path">路径</param>
        public static void Log2(string contents, string path = "PDA_Log")
        {
            try
            {
                bool islog = true;
                if (islog)
                {
                    string logpath = "D:\\ZTOAPI_Log\\" + path + "\\";
                    Directory.CreateDirectory(logpath);
                    logpath = String.Format("{0}{1}.log", logpath, DateTime.Now.ToString("yyyyMMddHH"));
                    FileStream fs = null;
                    StreamWriter sw = null;
                    try
                    {
                        fs = new FileStream(logpath, FileMode.Append);
                        sw = new StreamWriter(fs, Encoding.UTF8);
                        sw.WriteLine("错误日期：" + DateTime.Now.ToString());
                        sw.WriteLine(contents);
                    }
                    catch (Exception e)
                    {
                        Log2("错误方法来源:DotNet.MvcWeb.Controllers.BaseController." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ; 错误信息: " + e.Message + "\r\n");
                        //throw e;
                    }
                    finally
                    {
                        if (sw != null) { sw.Close(); }
                        if (fs != null) { fs.Close(); }
                    }
                }
            }
            catch (Exception e)
            {
                // Log("错误方法来源:DotNet.MvcWeb.Controllers.BaseController." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ; 错误信息: " + e.Message + "\r\n");
                //throw e;
            }
        }
        #endregion

        #region 发送数据获取返回值 public static String PostDate(string url, string date)
        /// <summary>
        /// 发送数据获取返回值
        /// </summary>
        /// <param name="url">发送地址</param>
        /// <param name="date">发送的数据</param>
        /// <returns>URL的返回值</returns>
        public static String PostDate(string url, string date)
        {
            string returns = "";
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.KeepAlive = false;
                request.ContentType = "multipart/form-data";
                //"application/x-www-form-urlencoded;charset=utf-8"; ContentType.Text;
                //Encoding encoding = Encoding.UTF8();
                UTF8Encoding encoding = new UTF8Encoding(false);
                //Stream outStream = request.GetRequestStream();
                //outStream.Write(bytes, 0, bytes.Length);
                StreamWriter writer = new StreamWriter(request.GetRequestStream(), encoding);
                //byte[] b = System.Text.Encoding.Default.GetBytes(date);
                //writer.Write(Convert.ToBase64String(b));
                writer.Write(date);
                writer.Close();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();

                System.IO.StreamReader reader = new System.IO.StreamReader(responseStream, encoding);
                string ret = reader.ReadToEnd();

                reader.Close();
                responseStream.Close();
                returns = ret;
            }
            catch (Exception e)
            {
                Log2("错误方法来源:DotNet.MvcWeb.Controllers.BaseController." + System.Reflection.MethodBase.GetCurrentMethod().Name + " ; 错误信息: " + e.Message + "\r\n");
                returns = (e.Message);
            }
            return returns;
        }
        #endregion

        #region 发送数据获取返回值 public static String PostDate2(string url, NameValueCollection date)
        /// <summary>
        /// 发送数据获取返回值
        /// </summary>
        /// <param name="url">发送地址</param>
        /// <param name="date">发送的数据</param>
        /// <returns>URL的返回值</returns>
        public static String PostDate2(string url, NameValueCollection postValues)
        {
            string returns = "";
            try
            {
                WebClient webClient = new WebClient();
                // 向服务器发送POST数据
                byte[] responseArray = webClient.UploadValues(url, postValues);
                string response = Encoding.UTF8.GetString(responseArray);
                returns = response;
            }
            catch (Exception e)
            {
                Log2("错误方法来源:" + System.Reflection.MethodBase.GetCurrentMethod().Name + " ; 错误信息: " + e.Message + "\r\n");
                returns = (e.Message);
            }
            return returns;
        }
        #endregion

    

        public static string Strtobase(string str)
        {
            System.Text.Encoding encode = System.Text.Encoding.UTF8;
            byte[] bytedata = encode.GetBytes(str);
            string strPath = Convert.ToBase64String(bytedata, 0, bytedata.Length);
            return strPath;
        }
        #region MD5加密	public string GetMd5(string encypStr, string charset)
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="encypStr"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string GetMd5(string encypStr, string charset)
        {
            //BaseController.Log("加密的串：" + encypStr);
            string retStr;
            //retStr = MD5Encrypt(encypStr);
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();
            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;
            //使用GB2312编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch (Exception ex)
            {
                inputBye = Encoding.GetEncoding("UTF-8").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);


            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToLower();
            return retStr;
        }
        #endregion
        #region 发送报文接口 	public string PostDate3(string contxt, string func)
        public static string PostDate3(String contxt, String func)
        {
            string retStr;
            string name = ClsLog.GetAppSettings("ZtoInterfaceName");
            string pwd = ClsLog.GetAppSettings("ZtoInterfacePsw");
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string contentBase = Strtobase(contxt.Trim());
            string verify = GetMd5(name + date + contentBase + pwd, "urf-8");

            NameValueCollection postValues = new NameValueCollection();
            postValues.Add("style", "json"); //消息签名
            postValues.Add("func", func);
            postValues.Add("partner", name);
            postValues.Add("datetime", date);
            postValues.Add("content", contentBase);
            postValues.Add("verify", verify);

            //测试 http://testpartner.zto.cn/client/interface.php
            //正式 http://partner.zto.cn/client/interface.php 

            string posturl = ClsLog.GetAppSettings("ZtoInterface");
            retStr = PostDate2(posturl, postValues);//,500,Encoding.UTF8,null
            return retStr;
        }
        #endregion
        #region Post中通国际报文接口 	public string PostDate4(string contxt, string func)
        public static string PostDate4(String contxt, String func)
        {
            string retStr;
            string company_id = "alog";
            string key = ClsLog.GetAppSettings("ZtoGjInterfaceKey");
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string contentBase = Strtobase(contxt.Trim());
            string verify = GetMd5(contxt + key, "utf-8");

            //测试 http://116.228.70.118:9001/zto/api_utf8/exportController
            //正式 http://japi.zto.cn/zto/api_utf8/exportController
            string posturl = ClsLog.GetAppSettings("ZtoGjInterface");
            string err = "";
            retStr = GetPage(posturl, contxt, verify, func, company_id, out err);//,500,Encoding.UTF8,null
            return retStr;
        }

        /// <summary>
        /// 从指定URL回传数据并返回结果
        /// </summary>
        /// <param name="url">WLB对应的URL</param>
        /// <param name="postData">回传的参数</param>
        /// <param name="encodeType">编码</param>
        /// <param name="err">返回的错误</param>
        /// <returns>结果字串</returns>
        public static string GetPage(string url, string postData, string data_digest, string msg_type, string company_id,out string err)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            Encoding encoding = Encoding.GetEncoding("utf-8");
            postData = "data="+postData + "&data_digest=" + data_digest + "&company_id=" + company_id + "&msg_type=" + msg_type;
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

        #endregion
        #region 发送中通国际报文接口

        public static string ZtoIntlbillInsert()
        {
            string retStr = "";
            string ParentTempLabelHtml;
            DataRow drSet = dtZtoIntlbillInsertXmlSet.Rows[0];
            int ToDSID = int.Parse(drSet["DSID"].ToString());
            string QuerySql = drSet["QuerySql"].ToString();
            string MasterSQL = drSet["MasterSQL"].ToString();
            string CompleteSQL = drSet["CompleteSQL"].ToString();
            try
            {
                QuerySql = QuerySql.Replace("~", "'");
                MasterSQL = MasterSQL.Replace("~", "'");

                DataTable dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);
                foreach (DataRow dr in dtMaster.Rows)   //对主表数据逐条处理，每条生成一个报文
                {


                    string tempQuerySql = QuerySql;
                    RepXml rx = new RepXml();
                    ParentTempLabelHtml = rx.ReplaceMasterValue(drSet["XMLContent"].ToString(), dr);//替换XML模板的主表数据
                    tempQuerySql = rx.ReplaceMasterValue(tempQuerySql, dr);//替换表体SQL(循环用)查询的的主表数据
                    CompleteSQL = rx.ReplaceMasterValue(CompleteSQL, dr);
                    if (tempQuerySql != string.Empty)
                    {
                        DataTable dt = DBAccess.GetDataTable(tempQuerySql, ToDSID);
                        ParentTempLabelHtml = rx.ReplaceSlaveValue(ParentTempLabelHtml, dt); //替换XML模板的表体及表体明细数据
                    }

                    retStr = PostDate4(ParentTempLabelHtml, "zto.intlbill.insert");
                    JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                    string result = jo["status"].ToString();
                    //{"data":null,"msg":"成功","status":true}
                 
                    //如果成功运行
                    if (result.ToLower() == "true")
                    {
                        CompleteSQL = CompleteSQL.Replace("~", "'");
                        DataTable dtComplete = DBAccess.GetDataTable(CompleteSQL, ToDSID);
                    }
                    else
                    {
                        DBAccess.ExeuteSQL("update  t_OutOrder set ZtoIntlbillStatus=2 where OutOrderNo='" + dr["OutOrderNo"].ToString() + "'", 1);
                        ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "单号：" + dr["OutOrderNo"].ToString() + ".错误：" + retStr, "KJSDLogZt");
                    }
                }

            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, drSet["RepTitle"].ToString());

            }


            return retStr;
        }
        /// <summary>
        /// XML报文模板设置
        /// </summary>
        private static DataTable _dtZtoIntlbillInsertXmlSet;
        public static DataTable dtZtoIntlbillInsertXmlSet
        {
            get
            {
                if (_dtZtoIntlbillInsertXmlSet == null)
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
                                  ,[CompleteSQL] from RepXmlSet where  isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and  RepVersion=9 and AutoId =44";
                    return _dtZtoIntlbillInsertXmlSet = DBAccess.GetDataTable(sql, 7); ;
                }

                return _dtZtoIntlbillInsertXmlSet;
            }
            set
            {
                _dtZtoIntlbillInsertXmlSet = value;
            }
        }

        #endregion

        #region 订单取消接口

        public static string OrderCancel()
        {
            string retStr = "";
            string ParentTempLabelHtml;
            DataRow drSet = dtOrderCancelXmlSet.Rows[0];
            int ToDSID = int.Parse(drSet["DSID"].ToString());
            string QuerySql = drSet["QuerySql"].ToString();
            string MasterSQL = drSet["MasterSQL"].ToString();
            string CompleteSQL = drSet["CompleteSQL"].ToString();
            try
            {
                QuerySql = QuerySql.Replace("~", "'");
                MasterSQL = MasterSQL.Replace("~", "'");
                DataTable dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);
                foreach (DataRow dr in dtMaster.Rows)   //对主表数据逐条处理，每条生成一个报文
                {
                    string tempQuerySql = QuerySql;
                    RepXml rx = new RepXml();
                    ParentTempLabelHtml = rx.ReplaceMasterValue(drSet["XMLContent"].ToString(), dr);//替换XML模板的主表数据
                    tempQuerySql = rx.ReplaceMasterValue(tempQuerySql, dr);//替换表体SQL(循环用)查询的的主表数据
                    CompleteSQL = rx.ReplaceMasterValue(CompleteSQL, dr);
                    if (!string.IsNullOrEmpty(tempQuerySql))
                    {
                        DataTable dt = DBAccess.GetDataTable(tempQuerySql, ToDSID);
                        ParentTempLabelHtml = rx.ReplaceSlaveValue(ParentTempLabelHtml, dt); //替换XML模板的表体及表体明细数据
                    }
                    //{ "id": "ZTO-130520142013234","remark": "收货人出差了，过几天后再发"}
                    //ParentTempLabelHtml = "{ \"id\": \"2015070216291011005082508\",\"remark\": \"收货人出差了，过几天后再发\"}";
                    retStr = PostDate3(ParentTempLabelHtml, "order.cancel");
                    //{"result":"true","id":"2015070216291011005082508"}
                    JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                    string result = jo["result"].ToString();
                    //如果成功运行
                    if (result == "true")
                    {
                        string orderid = jo["id"].ToString();//true时才有ID
                        //CancelExpressNOStatus更新为1
                        CompleteSQL = CompleteSQL.Replace("{id}", orderid);
                        CompleteSQL = CompleteSQL.Replace("~", "'");
                        DataTable dtComplete = DBAccess.GetDataTable(CompleteSQL, ToDSID);
                    }

                }

            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, drSet["RepTitle"].ToString());

            }


            return retStr;
        }
        /// <summary>
        /// XML报文模板设置
        /// </summary>
        private static DataTable _dtOrderCancelXmlSet;
        public static DataTable dtOrderCancelXmlSet
        {
            get
            {
                if (_dtOrderCancelXmlSet == null)
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
                                  ,[CompleteSQL] from RepXmlSet where  isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and  RepVersion=9 and AutoId =43";
                    return _dtOrderCancelXmlSet = DBAccess.GetDataTable(sql, 7); ;
                }

                return _dtOrderCancelXmlSet;
            }
            set
            {
                _dtOrderCancelXmlSet = value;
            }
        }

        #endregion
        #region 中通新增或修改订单接口

        public static string OrderSubmit()
        {
            string retStr = "";
            string ParentTempLabelHtml;
            DataRow drSet = dtOrderSubmitXmlSet.Rows[0];
            int ToDSID = int.Parse(drSet["DSID"].ToString());
            string QuerySql = drSet["QuerySql"].ToString();
            string MasterSQL = drSet["MasterSQL"].ToString();
            string CompleteSQL = drSet["CompleteSQL"].ToString();
            try
            {
                QuerySql = QuerySql.Replace("~", "'");
                MasterSQL = MasterSQL.Replace("~", "'");


                DataTable dtMaster = DBAccess.GetDataTable(MasterSQL, ToDSID);
                foreach (DataRow dr in dtMaster.Rows)   //对主表数据逐条处理，每条生成一个报文
                {

                    string tempQuerySql = QuerySql;
                    RepXml rx = new RepXml();
                    ParentTempLabelHtml = rx.ReplaceMasterValue(drSet["XMLContent"].ToString(), dr);//替换XML模板的主表数据
                    tempQuerySql = rx.ReplaceMasterValue(tempQuerySql, dr);//替换表体SQL(循环用)查询的的主表数据
                    CompleteSQL = rx.ReplaceMasterValue(CompleteSQL, dr);
                    if (!string.IsNullOrEmpty(tempQuerySql))
                    {
                        DataTable dt = DBAccess.GetDataTable(tempQuerySql, ToDSID);
                        ParentTempLabelHtml = rx.ReplaceSlaveValue(ParentTempLabelHtml, dt); //替换XML模板的表体及表体明细数据
                    }
                    //ParentTempLabelHtml = "{\"id\":\"2015070216291011005082508\",\"sender\":{\"name\":\"神码测试仓库1\",\"mobile\":\"13212322345\",\"city\":\"河南省,郑州市,经济技术开发区\",\"address\":\"神码测试仓库1\"},\"receiver\":{\"name\":\"黄东东\",\"mobile\":\"15858112232\",\"city\":\"辽宁省,抚顺市,新抚区\",\"address\":\"辽宁省,抚顺市,新抚区,的第三方,黄东东\"}}";

                    retStr = PostDate3(ParentTempLabelHtml, "order.submit");

                    //添加读取快递单并更新快递单的方法
                    JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);

                    string result = jo["result"].ToString();

                    //如果成功运行
                    if (result == "true")
                    {
                        string orderid = jo["keys"]["orderid"].ToString();
                        string mailno = jo["keys"]["mailno"].ToString();

                        string mark = jo["keys"]["mark"].ToString();
                        string sitecode = jo["keys"]["sitecode"].ToString();
                        string sitename = jo["keys"]["sitename"].ToString();

                        if (string.IsNullOrEmpty(mark))
                        {
                            //string markjson1 = "{\"send_province\":\"上海\",\"send_city\":\"上海市\",\"send_district\":\"青浦区\",\"send_address\":\"华新镇1688号\",\"receive_province\":\"上海\",\"receive_city\":\"上海市\",\"receive_district\":\"青浦区\",\"receive_address\":\"华新镇1685号\" }";
                            string markjson = "{\"send_province\":\"{$SenderProvince$}\",\"send_city\":\"{$SenderCity$}\",\"send_district\":\"{$SenderArea$}\",\"send_address\":\"{$SenderAddress$}\",\"receive_province\":\"{$ReceiverProvince$}\",\"receive_city\":\"{$ReceiverCity$}\",\"receive_district\":\"{$ReceiverArea$}\",\"receive_address\":\"{$ReceiverAddress$}\" }";
                            markjson = rx.ReplaceMasterValue(markjson, dr);
                            mark = orderMarke(markjson);
                        }

                        CompleteSQL = CompleteSQL.Replace("{orderid}", orderid);
                        CompleteSQL = CompleteSQL.Replace("{mailno}", mailno);
                        CompleteSQL = CompleteSQL.Replace("{mark}", mark);

                        CompleteSQL = CompleteSQL.Replace("~", "'");

                        DataTable dtComplete = DBAccess.GetDataTable(CompleteSQL, ToDSID);
                    }
                }
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, drSet["RepTitle"].ToString());

            }


            return retStr;
        }


        public static string orderMarke(string contxt)
        {
            string retStr = "";
            try
            {
                string name = ClsLog.GetAppSettings("ZtoInterfaceName");
                string pwd = ClsLog.GetAppSettings("ZtoInterfacePsw");
                string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");



                NameValueCollection postValues = new NameValueCollection();
                postValues.Add("msg_type", "GETMARK"); //消息签名
                postValues.Add("data", contxt);


                string posturl = "http://japi.zto.cn/zto/api_utf8/mark";
                retStr = PostDate2(posturl, postValues);//,500,Encoding.UTF8,null

                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                string result = jo["result"]["print_mark"].ToString();

            }
            catch (Exception ex)
            {
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "大头笔");

            }


            return retStr;
        }


        /// <summary>
        /// XML报文模板设置
        /// </summary>
        private static DataTable _dtOrderSubmitXmlSet;
        public static DataTable dtOrderSubmitXmlSet
        {
            get
            {
                if (_dtOrderSubmitXmlSet == null)
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
                                  ,[CompleteSQL] from RepXmlSet where  isnull(IsDel,0) = 0 and isnull(IsValid,1) = 1 and  RepVersion=9 and AutoId =42";
                    return _dtOrderSubmitXmlSet = DBAccess.GetDataTable(sql, 7); ;
                }

                return _dtOrderSubmitXmlSet;
            }
            set
            {
                _dtOrderSubmitXmlSet = value;
            }
        }

        #endregion
    }
}
