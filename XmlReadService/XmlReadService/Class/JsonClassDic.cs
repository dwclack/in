using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ClassApIBILL.Model
{
    public class JsonClassDic
    {
        public struct JsonClass
        {
            //{"message":"","result":{"mark":"湖北仙桃","print_mark":"武汉转仙桃"},"status":true,"statusCode":""}
            public string message { get; set; }
            public Result result { get; set; }
            public bool status { get; set; }
            public string statusCode { get; set; }
        }
        public class Result
        {
            public string mark { get; set; }
            public string print_mark { get; set; }
        }

        #region 将json数据反序列化为Dictionary public static JsonClass DTBJsonToDictionary(string jsonData)
        /// <summary>
        /// 将json数据反序列化为Dictionary
        /// </summary>
        /// <param name="jsonData">json数据</param>
        /// <returns></returns>
        public static JsonClass DTBJsonToDictionary(string jsonData)
        {
            //实例化JavaScriptSerializer类的新实例
            JavaScriptSerializer jss = new JavaScriptSerializer();
            try
            {
                //将指定的 JSON 字符串转换为 Dictionary<string, object> 类型的对象
                return jss.Deserialize<JsonClass>(jsonData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}