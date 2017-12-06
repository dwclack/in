using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

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

    /// <summary>
    /// base64 MD5加密
    /// </summary>
    /// <param name="content">要加密的字串</param>
    /// <param name="key">约定的密钥</param>
    /// <returns>加密后的base64数字字串</returns>
    public static string EncrypMD5(string content)
    {
       // MD5 md5 = new MD5CryptoServiceProvider();
        Byte[] bytes =MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(content));
        //Byte[] bytes = md5.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(content));
        return Convert.ToBase64String(bytes);
    }
  
    /// <summary>
    /// 仓易宝 base64 MD5加密
    /// </summary>
    /// <param name="content">要加密的字串</param>
    /// <param name="key">和CYB约定的密钥</param>
    /// <returns>加密后的base64数字字串</returns>
    public static string EncrypMD5_CYB(string content, string key="alogalog")
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        Byte[] bytes = md5.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(content + key));
        return Convert.ToBase64String(bytes);
    }


	
}