using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;


public class AESHelper
{
    
    /// <summary>
    /// DES加密 
    /// </summary>
    /// <param name="encryptString">要加密的字符串</param>
    /// <param name="btKeys">二进制秘钥</param>
    /// <param name="isBase64">是否进行Base64</param>
    /// <returns></returns>
    public static string DesEncrypt(string encryptString, byte[] btKeys, bool isBase64 = true)
    {
        byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
        string des = string.Empty;
        DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
        provider.Mode = CipherMode.ECB;
        MemoryStream mStream = new MemoryStream();
        CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(btKeys, btKeys), CryptoStreamMode.Write);
        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        cStream.FlushFinalBlock();

        des = Convert.ToBase64String(mStream.ToArray());
        if (isBase64)
        {
            des = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(des));
        }
        return des;
    }



    /// <summary>
    ///  AES 解密
    /// </summary>
    /// <param name="str"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string AesDecrypt(string str, string key)
    {
        if (string.IsNullOrEmpty(str)) return null;
        Byte[] toEncryptArray = Convert.FromBase64String(str);

        System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
        {
            Key = Encoding.UTF8.GetBytes(key),
            Mode = System.Security.Cryptography.CipherMode.ECB,
            Padding = System.Security.Cryptography.PaddingMode.PKCS7
        };

        System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
        Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Encoding.UTF8.GetString(resultArray);
    }









    /// <summary>
    /// 有密码的AES加密 
    /// </summary>
    /// <param name="toEncrypt">加密字符</param>
    /// <param name="key">加密的密码</param>
    /// <param name="isBase64">密钥是否转为64编码</param>
    /// <returns></returns>
    public static string Encrypt(string toEncrypt, string key, bool isBase64 = false)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
        if (isBase64)
        {
            keyArray = Convert.FromBase64String(key);
        }
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);


        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="toDecrypt">密文</param>
    /// <param name="key">密钥</param>
    /// <param name="isBase64">密钥是否为64编码</param>
    /// <returns></returns>
    public static string Decrypt(string toDecrypt, string key, bool isBase64 = false)
    {
        byte[] keyArray; byte[] toEncryptArray;
        if (isBase64)
        {
            keyArray = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(UTF8Encoding.UTF8.GetBytes(key)));
            toEncryptArray = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(UTF8Encoding.UTF8.GetBytes(toDecrypt)));
        }

        else
        {
            keyArray = (UTF8Encoding.UTF8.GetBytes(key));
            toEncryptArray = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(UTF8Encoding.UTF8.GetBytes(toDecrypt)));
        }

        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return UTF8Encoding.UTF8.GetString(resultArray);
    }





}

