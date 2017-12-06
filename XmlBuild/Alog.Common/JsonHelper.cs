using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace Alog.Common
{
    public class JsonHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private static Lazy<JsonHelper> JsonHelperLazy;
        /// <summary>
        /// 
        /// </summary>
        static JsonHelper()
        {
            JsonHelperLazy = new Lazy<JsonHelper>(() => { return new JsonHelper(); }, true);
        }

        /// <summary>
        /// 
        /// </summary>
        public static JsonHelper HelperInstance
        {
            get
            {
                if (JsonHelperLazy != null)
                {
                    return JsonHelperLazy.Value;
                }

                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TModel"></param>
        /// <returns></returns>
        public string DataContractJsonSerializer<T>(T TModel, Encoding Format)
        {
            DataContractJsonSerializer JsonSerializer = new DataContractJsonSerializer(TModel.GetType());

            using (MemoryStream MStream = new MemoryStream())
            {
                JsonSerializer.WriteObject(MStream, TModel);

                return Format.GetString(MStream.ToArray());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonString"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        public T DataContractJsonDeSerializer<T>(String JsonString, Encoding Format)
        {
            using (MemoryStream MStream = new MemoryStream(Format.GetBytes(JsonString)))
            {
                DataContractJsonSerializer JsonSerializer = new DataContractJsonSerializer(typeof(T));

                return (T)JsonSerializer.ReadObject(MStream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonString"></param>
        /// <param name="Format"></param>
        /// <param name="List"></param>
        /// <returns></returns>
        public object DataContractJsonDeSerializer<T>(String JsonString, Encoding Format, List<T> List)
        {
            if (JsonString.StartsWith("[") && JsonString.EndsWith("]"))
            {
                JArray JsonArray = JArray.Parse(JsonString);

                IList<T> ResultList = new List<T>();

                using (MemoryStream MStream = new MemoryStream(Format.GetBytes(JsonString)))
                {
                    DataContractJsonSerializer JsonSerializer = new DataContractJsonSerializer(typeof(IList<T>));

                    return JsonSerializer.ReadObject(MStream);
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TModel"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        public string NewtonsoftJsonSerializer<T>(T TModel, Encoding Format)
        {
            JsonSerializer Serializer = new JsonSerializer();

            StringBuilder TextBuilder = new StringBuilder();

            using (StringWriter StringWriterBuilder = new StringWriter(TextBuilder))
            {
                Serializer.Serialize(StringWriterBuilder, TModel);
            }

            return TextBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonString"></param>
        /// <param name="Format"></param>
        /// <returns></returns>
        public object NewtonsoftJsonDeSerializer<T>(String JsonString, Encoding Format)
        {
            if (JsonString.StartsWith("[") && JsonString.EndsWith("]"))
            {
                JArray JsonArray = JArray.Parse(JsonString);

                IList<T> ResultList = new List<T>();

                for (int i = 0; i < JsonArray.Count; i++)
                {
                    JsonSerializer Serializer = new JsonSerializer();

                    object TModel = Serializer.Deserialize(JsonArray[i].CreateReader(), typeof(T));

                    ResultList.Add((T)TModel);
                }

                return ResultList;
            }
            else if (JsonString.StartsWith("{") && JsonString.EndsWith("}"))
            {
                JObject Json = JObject.Parse(JsonString);

                JsonSerializer Serializer = new JsonSerializer();

                T TModel = Serializer.Deserialize<T>(Json.CreateReader());

                return TModel;
            }

            return default(T);
        }

        /// <summary>
        /// 将json转换为DataTable
        /// </summary>
        /// <param name="strJson">得到的json</param>
        /// <returns></returns>
        public static DataTable JsonToDataTable(string strJson)
        {
            //转换json格式
            strJson = strJson.Replace(",\"", "*\"").Replace("\":", "\"#").ToString();
            //取出表名   
            var rg = new Regex(@"(?<={)[^:]+(?=:\[)", RegexOptions.IgnoreCase);
            string strName = rg.Match(strJson).Value;
            DataTable tb = null;
            //去除表名   
            strJson = strJson.Substring(strJson.IndexOf("[") + 1);
            strJson = strJson.Substring(0, strJson.IndexOf("]"));

            //获取数据   
            rg = new Regex(@"(?<={)[^}]+(?=})");
            MatchCollection mc = rg.Matches(strJson);
            for (int i = 0; i < mc.Count; i++)
            {
                string strRow = mc[i].Value;
                string[] strRows = strRow.Split('*');

                //创建表   
                if (tb == null)
                {
                    tb = new DataTable();
                    tb.TableName = strName;
                    foreach (string str in strRows)
                    {
                        var dc = new DataColumn();
                        string[] strCell = str.Split('#');

                        if (strCell[0].Substring(0, 1) == "\"")
                        {
                            int a = strCell[0].Length;
                            dc.ColumnName = strCell[0].Substring(1, a - 2);
                        }
                        else
                        {
                            dc.ColumnName = strCell[0];
                        }
                        tb.Columns.Add(dc);
                    }
                    tb.AcceptChanges();
                }

                //增加内容   
                DataRow dr = tb.NewRow();
                for (int r = 0; r < strRows.Length; r++)
                {
                    dr[r] = strRows[r].Split('#')[1].Trim().Replace("，", ",").Replace("：", ":").Replace("\"", "");
                }
                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }

            return tb;
        }



    }
}
