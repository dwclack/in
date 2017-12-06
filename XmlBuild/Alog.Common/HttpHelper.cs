using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Alog.Common
{
    public static class HttpHelper
    {
        public static string HttpPost(string baseUrl, Dictionary<string, string> urlParams, Dictionary<string, string> headerParams, Dictionary<string, string> postParams)
        {
            string result = string.Empty;

            string requestUrl = baseUrl + "?" + urlParams.ToUrlString();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(requestUrl);
            req.Method = "POST";
            req.KeepAlive = true;
            req.UserAgent = "SpaceTimeApp2.0";
            req.Timeout = 300000;
            req.ContentType = "application/x-www-form-urlencoded";

            if (headerParams != null)
            {
                foreach (var param in headerParams)
                {
                    req.Headers.Add(HttpUtility.UrlEncode(param.Key), HttpUtility.UrlEncode(param.Value));
                }
            }

            string postStr = string.Empty;
            if (postParams != null)
            {
                foreach (var param in postParams)
                {
                    postStr += HttpUtility.UrlEncode(param.Key) + "="
                  + HttpUtility.UrlEncode(param.Value) + "&";
                }
            }

            byte[] postData = Encoding.UTF8.GetBytes(postStr);
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(postData, 0, postData.Length);
            }

            using (HttpWebResponse rsp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream stream = rsp.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }

        public static string ToUrlString(this IDictionary<string, string> parameters)
        {
            StringBuilder result = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        result.Append("&");
                    }

                    result.Append(name);
                    result.Append("=");
                    result.Append(Uri.EscapeDataString(value));
                    hasParam = true;
                }
            }

            return result.ToString();
        }
    }
}
