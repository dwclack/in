using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
namespace Alog.Common
{
    /// <summary>
    ///ApiSign 的摘要说明
    /// </summary>
    public class ApiSign
    {
        /// <summary>
        /// 给请求签名  
        /// </summary>
        /// <param name="parameters">所有字符型的TOP请求参数</param>
        /// <param name="secret">签名密钥</param>
        /// <returns>签名</returns>
        public static string CreateSign(string postData, string timestamp, string method, string secret)
        {
            string content = postData + timestamp + method + secret;
            return EncrypMD5(content);

        }

        public static string CreateSign(string postData, string secret)
        {
            string content = postData + secret;
            return EncrypMD5(content);

        }

        /// <summary>
        /// base64 MD5加密
        /// </summary>
        /// <param name="content">要加密的字串</param>
        /// <param name="key">约定的密钥</param>
        /// <returns>加密后的base64数字字串</returns>
        public static string EncrypMD5(string content)
        {
            // MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(content));
            //Byte[] bytes = md5.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(content));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 仓易宝 base64 MD5加密
        /// </summary>
        /// <param name="content">要加密的字串</param>
        /// <param name="key">和CYB约定的密钥</param>
        /// <returns>加密后的base64数字字串</returns>
        public static string EncrypMD5_CYB(string content, string key = "alogalog")
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] bytes = md5.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(content + key));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 给OMS请求签名  API v2.0
        /// </summary>
        /// <param name="parameters">所有字符型的TOP请求参数</param>
        /// <param name="secret">签名密钥</param>
        /// <returns>签名</returns>
        public static string CreateSign(IDictionary<string, string> parameters, string secret)
        {

            //md5:将secretcode同时拼接到参数字符串头、尾部进行md5加密，再转化成大写，格式是：uppercase(md5(secretkey1value1key2value2...secret)。例如:uppercase(md5（secretbar2baz3foo1secret)) 

            parameters.Remove("sign");
            parameters.Remove("secretKey");

            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            StringBuilder query = new StringBuilder(secret);
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append(value);
                }
            }
            query.Append(secret);// API 2.0 新签名方法            

            // 第三步：使用MD5加密
            MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query.ToString()));

            // 第四步：把二进制转化为大写的十六进制
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                string hex = bytes[i].ToString("X");
                if (hex.Length == 1)
                {
                    result.Append("0");
                }
                result.Append(hex);
            }

            return result.ToString();
        }

        /// <summary>
        /// base64 MD5加密
        /// </summary>
        /// <param name="content">要加密的字串</param>
        /// <param name="key">约定的密钥</param>
        /// <returns>加密后的base64数字字串</returns>
        public static string BSEncrypMD5(string content, string key = "alogalog")
        {
            // MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(content + key));
            //Byte[] bytes = md5.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(content));

            string pwd = BitConverter.ToString(bytes).Replace("-", "");

            return pwd;
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="content">待加密字符串</param>
        /// <param name="encode">编码方式</param>
        /// <returns>加密密文</returns>
        public static string Base64Encode(string content, Encoding encode)
        {
            byte[] bytes = encode.GetBytes(content);

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="content">待加密字符串</param>
        /// <param name="encode">编码方式</param>
        /// <returns>加密密文</returns>
        public static string MD5Encode(string content, Encoding encode)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(encode.GetBytes(content));

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                string hex = bytes[i].ToString("X");
                if (hex.Length == 1)
                {
                    result.Append("0");
                }
                result.Append(hex);
            }

            return result.ToString();
        }

    }
}