using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Alog.Common
{
    public static class CommonUtil
    {
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// 从指定URL回传数据并返回结果
        /// </summary>
        /// <param name="url">WLB对应的URL</param>
        /// <param name="postData">回传的参数</param>
        /// <param name="encodeType">编码</param>
        /// <param name="err">返回的错误</param>
        /// <returns>结果字串</returns>
        public static string GetPage(string url, string postData, string encodeType, out string err)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            Encoding encoding = Encoding.GetEncoding(encodeType);

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
    }
}
