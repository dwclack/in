using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace Alog.Common
{
    /// <summary>
    ///ErrorCode 的摘要说明
    /// </summary>
    public class ErrorCode
    {


        #region 获取格式化的返回信息

        public enum ERROR_CODE
        {
            /// <summary>
            /// 空
            /// </summary>
            Empty = 0,
            /// <summary>
            /// 请求参数不正确
            /// </summary>
            S0001 = 1,
            /// <summary>
            /// 数据签名错误
            /// </summary>
            S0002 = 2,
            /// <summary>
            /// 非法的服务名称
            /// </summary>
            S0003 = 3,
            /// <summary>
            /// 非法的数据类型
            /// </summary>
            S0004 = 4,
            /// <summary>
            /// 非法的AppKey
            /// </summary>
            S0005 = 5,
            /// <summary>
            /// 非法的请求方法
            /// </summary>
            S0006 = 6,
            /// <summary>
            /// 非法的请求方法，只有用POST提交
            /// </summary>
            S9998 = 7,
            /// <summary>
            /// 系统异常，请重试
            /// </summary>
            S9999 = 8,
            /// <summary>
            /// 不正确的文本格式
            /// </summary>
            S0007 = 9,
            /// <summary>
            /// 错误的输出查询
            /// </summary>
            S0008 = 10,
            /// <summary>
            /// 数据验证错误
            /// </summary>
            S0009 = 11
        }

        private static string GetMessageByErrorCode(ERROR_CODE error_code)
        {
            switch (error_code)
            {
                case ERROR_CODE.Empty:
                    return string.Empty;
                case ERROR_CODE.S0001:
                    return "请求参数不正确";
                case ERROR_CODE.S0002:
                    return "数据签名错误";
                case ERROR_CODE.S0003:
                    return "非法的服务名称";
                case ERROR_CODE.S0004:
                    return "非法的数据类型";
                case ERROR_CODE.S0005:
                    return "非法的AppKey";
                case ERROR_CODE.S0006:
                    return "非法的请求方法";
                case ERROR_CODE.S0007:
                    return "不正确的文本格式";
                case ERROR_CODE.S0008:
                    return "错误的输出查询";
                case ERROR_CODE.S0009:
                    return "数据验证错误";
                case ERROR_CODE.S9998:
                    return "非法的请求方法，只有用POST提交";
                case ERROR_CODE.S9999:
                    return "系统异常，请重试";
                default:
                    return "系统异常，请重试";
            }
        }

        private static string GetMessageByErrorCodeForWms(ERROR_CODE error_code)
        {
            switch (error_code)
            {
                case ERROR_CODE.Empty:
                    return string.Empty;
                case ERROR_CODE.S0001:
                    return "请求参数不正确";
                case ERROR_CODE.S0002:
                    return "数据签名错误";
                case ERROR_CODE.S0003:
                    return "非法的服务名称";
                case ERROR_CODE.S0004:
                    return "非法的数据类型";
                case ERROR_CODE.S0005:
                    return "非法的AppKey";
                case ERROR_CODE.S0006:
                    return "非法的请求方法";
                case ERROR_CODE.S0007:
                    return "不正确的文本格式";
                case ERROR_CODE.S0008:
                    return "错误的输出查询";
                case ERROR_CODE.S0009:
                    return "数据验证错误";
                case ERROR_CODE.S9998:
                    return "非法的请求方法，只有用POST提交";
                case ERROR_CODE.S9999:
                    return "系统异常，请重试";
                default:
                    return "系统异常，请重试";
            }
        }

        public static string GetResult(string datatype, bool success, ERROR_CODE error_code)
        {
            if (datatype.Equals("json"))
            {
                return GetResultForJson(success, error_code, string.Empty);
            }
            else
            {
                return GetResultForXML(success, error_code, string.Empty);
            }
        }

        public static string GetResultForWms(string datatype, bool success, ERROR_CODE error_code)
        {
            if (datatype.Equals("json"))
            {
                return GetResultForJsonForWms(success, error_code, string.Empty);
            }
            else
            {
                return GetResultForXMLForWms(success, error_code, string.Empty);
            }
        }

        public static string GetRenewResult(string datatype, bool success, string message)
        {
            if (datatype == "json")
            {
                return GetResultForWms("json", success, ERROR_CODE.Empty, message);
            }
            else
            {
                return 
                    @"<response><success>" 
                    +  (success ? "true" : "false")
                    + @"</success><errorCode></errorCode><errorMsg>"
                    + message 
                    + @"</errorMsg></response>";

            }
        }



        public static string GetResult(string datatype, bool success, ERROR_CODE error_code, string message)
        {
            if (datatype.Equals("json"))
            {
                return GetResultForJson(success, error_code, message);
            }
            else
            {
                return GetResultForXML(success, error_code, message);
            }
        }

        public static string GetResultForWms(string datatype, bool success, ERROR_CODE error_code, string message)
        {
            if (datatype.Equals("json"))
            {
                return GetResultForJsonForWms(success, error_code, message);
            }
            else
            {
                return GetResultForXMLForWms(success, error_code, message);
            }
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForXML(bool success, string data)
        {
            return GetResultForXML(success, ERROR_CODE.Empty, string.Empty, data);
        }

        public static string GetResultForXMLForWms(bool success, string data)
        {
            return GetResultForXMLForWms(success, ERROR_CODE.Empty, string.Empty, data);
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForXML(bool success, ERROR_CODE error_code)
        {
            return GetResultForXML(success, error_code, string.Empty, string.Empty);
        }

        public static string GetResultForXMLForWms(bool success, ERROR_CODE error_code)
        {
            return GetResultForXMLForWms(success, error_code, string.Empty, string.Empty);
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForXML(bool success, ERROR_CODE error_code, string message)
        {
            return GetResultForXML(success, error_code, message, string.Empty);
        }

        public static string GetResultForXMLForWms(bool success, ERROR_CODE error_code, string message)
        {
            return GetResultForXMLForWms(success, error_code, message, string.Empty);
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForXML(bool success, ERROR_CODE error_code, string message, string data)
        {
            string msg = GetMessageByErrorCode(error_code);
            return @"<result>
                  <success>" + (success ? "1" : "0") + @"</success>
                  <data>" + data + @"</data>
                  <error_code>" + (error_code.Equals(ERROR_CODE.Empty) ? string.Empty : error_code.ToString()) + @"</error_code>
                  <message>" + msg + (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(msg) ? "，" : string.Empty) + message + @"</message>
                </result>";
        }

        public static string GetResultForXMLForWms(bool success, ERROR_CODE error_code, string message, string data)
        {
            string retstr = "";
            if (success)
            {
                retstr = "<response><success>true</success></response>";
            }
            else
            {
                string msg = GetMessageByErrorCodeForWms(error_code);
                msg = msg + message;
                retstr = "<response><success>false</success><errorCode>" + error_code + "</errorCode><errorMsg>" + msg + "</errorMsg></response>";
            }
            return retstr;
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForJson(string data_type, bool success, DataSet data)
        {
            return GetResultForJson(success, ERROR_CODE.Empty, string.Empty, data, string.Empty);
        }
        public static string GetResultForJsonForWms(string data_type, bool success, DataSet data)
        {
            return GetResultForJsonForWms(success, ERROR_CODE.Empty, string.Empty, data, string.Empty);
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForJson(bool success, ERROR_CODE error_code)
        {
            return GetResultForJson(success, error_code, string.Empty, null, string.Empty);
        }

        public static string GetResultForJsonForWms(bool success, ERROR_CODE error_code)
        {
            return GetResultForJsonForWms(success, error_code, string.Empty, null, string.Empty);
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForJson(bool success, ERROR_CODE error_code, string message)
        {
            return GetResultForJson(success, error_code, message, null, string.Empty);
        }

        public static string GetResultForJsonForWms(bool success, ERROR_CODE error_code, string message)
        {
            return GetResultForJsonForWms(success, error_code, message, null, string.Empty);
        }

        /// <summary>
        /// 获取格式化的返回信息
        /// </summary>
        /// <returns></returns>
        public static string GetResultForJson(bool success, ERROR_CODE error_code, string message, DataSet data, string groupby)
        {
            string msg = GetMessageByErrorCode(error_code);
            ResultInfo result = new ResultInfo() { success = (success ? "1" : "0"), data = DataSetGroupby(data, groupby), error_code = (error_code.Equals(ERROR_CODE.Empty) ? string.Empty : error_code.ToString()), message = msg + (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(msg) ? "，" : string.Empty) + message };

            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
            string returnStr = JsonConvert.SerializeObject(result, timeConverter);
            return returnStr;
        }

        public static string GetResultForJsonForWms(bool success, ERROR_CODE error_code, string message, DataSet data, string groupby)
        {
            string msg = GetMessageByErrorCodeForWms(error_code);
            ResultInfo result = new ResultInfo() { success = (success ? "1" : "0"), data = DataSetGroupby(data, groupby), error_code = (error_code.Equals(ERROR_CODE.Empty) ? string.Empty : error_code.ToString()), message = msg + (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(msg) ? "，" : string.Empty) + message };

            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";
            string returnStr = JsonConvert.SerializeObject(result, timeConverter);
            //ClsLog.AddDbLog( returnStr, "KJSDLogReturnStr");
            return returnStr;
        }

        /// <summary>
        /// 对某列进行分组，从而得到分组的json
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="groupby"></param>
        /// <returns></returns>
        public static DataSet DataSetGroupby(DataSet ds, string groupby)
        {
            if (!string.IsNullOrEmpty(groupby) && ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataSet returnDs = new DataSet();
                List<string> gpList = GetGroup(ds.Tables[0], groupby);
                int index = 1;
                foreach (string gp in gpList)
                {
                    DataTable dt = ds.Tables[0].Clone();
                    dt.TableName = "Table_" + index.ToString();
                    foreach (DataRow dr in ds.Tables[0].Select(groupby + "='" + gp + "'"))
                    {
                        dt.ImportRow(dr);
                    }
                    returnDs.Tables.Add(dt);
                    index++;
                }
                return returnDs;
            }
            else
            {
                return ds;
            }
        }
        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="groupby"></param>
        /// <returns></returns>
        private static List<string> GetGroup(DataTable dt, string groupby)
        {
            List<string> list = new List<string>();
            if (dt.Rows.Count > 0)
            {
                list.Add(dt.Rows[0][groupby] == null ? string.Empty : dt.Rows[0][groupby].ToString());
                foreach (DataRow dr in dt.Rows)
                {
                    if (!list.Contains(dr[groupby] == null ? string.Empty : dr[groupby].ToString()))
                    {
                        list.Add(dr[groupby].ToString());
                    }
                }
                return list;
            }
            return list;
        }


        /// <summary>
        /// JsonTreeInfo
        /// </summary>
        [DataContract]
        public class ResultInfo
        {
            /// <summary>
            /// 
            /// </summary>
            public ResultInfo()
            {

            }

            [DataMember]
            public string success { get; set; }

            [DataMember]
            public DataSet data { get; set; }

            [DataMember]
            public string error_code { get; set; }

            [DataMember]
            public string message { get; set; }

        }
        #endregion
    }
}