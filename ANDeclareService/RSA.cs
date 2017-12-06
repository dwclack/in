using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Text;


using System.Management;

using Microsoft.Win32;

/// <summary>
/// RSA 的摘要说明
/// </summary>
public class RSAHelper
{

    #region RSA 加密解密


    #region RSA 的密钥产生
    /// <summary>
    /// RSA产生密钥
    /// </summary>
    /// <param name="xmlKeys">私钥</param>
    /// <param name="xmlPublicKey">公钥</param>
    public static void RSAKey(out string xmlKeys, out string xmlPublicKey)
    {
        try
        {
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            xmlKeys = rsa.ToXmlString(true);
            xmlPublicKey = rsa.ToXmlString(false);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    #endregion

    #region RSA加密函数
    //############################################################################## 
    //RSA 方式加密 
    //KEY必须是XML的形式,返回的是字符串 
    //该加密方式有长度限制的！
    //############################################################################## 

    /// <summary>
    /// RSA的加密函数(该加密方式有长度限制的！)
    /// </summary>
    /// <param name="xmlPublicKey">公钥</param>
    /// <param name="encryptString">待加密的字符串</param>
    /// <returns></returns>
    public static string RSAEncrypt(string xmlPublicKey, string encryptString)
    {
        try
        {
            byte[] PlainTextBArray;
            byte[] CypherTextBArray;
            string Result;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPublicKey);
            PlainTextBArray = (new UTF8Encoding()).GetBytes(encryptString);
            CypherTextBArray = rsa.Encrypt(PlainTextBArray, false);
            Result = Convert.ToBase64String(CypherTextBArray);
            return Result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// RSA的加密函数 
    /// </summary>
    /// <param name="xmlPublicKey">公钥</param>
    /// <param name="EncryptString">待加密的字节数组</param>
    /// <returns></returns>
    public static string RSAEncrypt(string xmlPublicKey, byte[] EncryptString)
    {
        try
        {
            byte[] CypherTextBArray;
            string Result;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPublicKey);
            CypherTextBArray = rsa.Encrypt(EncryptString, false);
            Result = Convert.ToBase64String(CypherTextBArray);
            return Result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    #endregion



    #region RSA的解密函数
    /// <summary>
    /// RSA的解密函数(该解密方式有长度限制的！)
    /// </summary>
    /// <param name="xmlPrivateKey">私钥</param>
    /// <param name="decryptString">待解密的字符串</param>
    /// <returns></returns>
    public static string RSADecrypt(string xmlPrivateKey, string decryptString)
    {
        try
        {
            byte[] PlainTextBArray;
            byte[] DypherTextBArray;
            string Result;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPrivateKey);
            PlainTextBArray = Convert.FromBase64String(decryptString);
            DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
            Result = (new UTF8Encoding()).GetString(DypherTextBArray);
            return Result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// RSA的解密函数 
    /// </summary>
    /// <param name="xmlPrivateKey">私钥</param>
    /// <param name="DecryptString">待解密的字节数组</param>
    /// <returns></returns>
    public static string RSADecrypt(string xmlPrivateKey, byte[] DecryptString)
    {
        try
        {
            byte[] DypherTextBArray;
            string Result;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPrivateKey);
            DypherTextBArray = rsa.Decrypt(DecryptString, false);
            Result = (new UTF8Encoding()).GetString(DypherTextBArray);
            return Result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// RSA加密
    /// </summary>
    /// <param name="publickey"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RSAEncryptLong(string publickey, string content)
    {
        byte[] result = null;
        try
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();


            rsa.FromXmlString(publickey);

            int keySize = rsa.KeySize / 8;

            int bufferSize = keySize - 11;

            byte[] buffer = new byte[bufferSize];

            MemoryStream msInput = new MemoryStream(Convert.FromBase64String(content));

            MemoryStream msOuput = new MemoryStream();

            int readLen = msInput.Read(buffer, 0, bufferSize);

            while (readLen > 0)
            {

                byte[] dataToEnc = new byte[readLen];

                Array.Copy(buffer, 0, dataToEnc, 0, readLen);

                byte[] encData = rsa.Encrypt(dataToEnc, false);

                msOuput.Write(encData, 0, encData.Length);

                readLen = msInput.Read(buffer, 0, bufferSize);

            }

            msInput.Close();

            result = msOuput.ToArray();    //得到加密结果

            msOuput.Close();

            rsa.Clear();

        }
        catch (Exception ex)
        {
            throw ex;
        }

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// RSA解密函数 
    /// </summary>
    /// <param name="privatekey"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RSADecryptLong(string privatekey, string content)
    {
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();


        string privateKey = System.Text.Encoding.UTF8.GetString(UTF8Encoding.UTF8.GetBytes((privatekey)));

        rsa.FromXmlString(privateKey);

        int keySize = rsa.KeySize / 8;

        byte[] buffer = new byte[keySize];

        MemoryStream msInput = new MemoryStream(Convert.FromBase64String(content));

        MemoryStream msOuput = new MemoryStream();

        int readLen = msInput.Read(buffer, 0, keySize);

        while (readLen > 0)
        {

            byte[] dataToDec = new byte[readLen];

            Array.Copy(buffer, 0, dataToDec, 0, readLen);

            byte[] decData = rsa.Decrypt(dataToDec, false);

            msOuput.Write(decData, 0, decData.Length);

            readLen = msInput.Read(buffer, 0, keySize);

        }

        msInput.Close();

        byte[] result = msOuput.ToArray();    //得到解密结果

        msOuput.Close();

        rsa.Clear();
        return Encoding.UTF8.GetString(result);

    }



    #endregion

    #endregion

    #region RSA数字签名

    #region 获取Hash描述表
    /// <summary>
    /// 获取Hash描述表
    /// </summary>
    /// <param name="strSource">待签名的字符串</param>
    /// <param name="HashData">Hash描述</param>
    /// <returns></returns>
    public static bool GetHash(string strSource, ref byte[] HashData)
    {
        try
        {
            byte[] Buffer;
            System.Security.Cryptography.HashAlgorithm SHA1 = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
            Buffer = Convert.FromBase64String(strSource);
            HashData = SHA1.ComputeHash(Buffer);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public static string EncodeBase64(Encoding encode, string source)
    {
        byte[] bytes = encode.GetBytes(source);
        string scode = "";
        try
        {
            scode = Convert.ToBase64String(bytes);
        }
        catch
        {
            scode = source;
        }
        return scode;
    }

     
    /// <summary>
    /// 获取Hash描述表
    /// </summary>
    /// <param name="strSource">待签名的字符串</param>
    /// <param name="strHashData">Hash描述</param>
    /// <returns></returns>
    public static bool GetHash(string strSource, ref string strHashData)
    {
        try
        {
            //从字符串中取得Hash描述 
            byte[] Buffer;
            byte[] HashData;
            System.Security.Cryptography.HashAlgorithm SHA1 = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
            Buffer = Convert.FromBase64String(strSource);
            HashData = SHA1.ComputeHash(Buffer);
            strHashData = Convert.ToBase64String(HashData);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>  
    /// 获取Hash描述表  
    /// </summary>  
    /// <param name="strSource">待签名的字符串</param>  
    /// <param name="strHashData">Hash描述</param>  
    /// <returns></returns>  
    public static bool GetHash33(string strSource, ref string strHashData)
    {
        try
        {
            //从字符串中取得Hash描述   
            byte[] Buffer;
            byte[] HashData;
            System.Security.Cryptography.HashAlgorithm SHA1 = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
            Buffer = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(strSource);
            HashData = SHA1.ComputeHash(Buffer);
            strHashData = Convert.ToBase64String(HashData);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }  



    /// <summary>
    /// 获取Hash描述表
    /// </summary>
    /// <param name="objFile">待签名的文件</param>
    /// <param name="HashData">Hash描述</param>
    /// <returns></returns>
    public static bool GetHash(System.IO.FileStream objFile, ref byte[] HashData)
    {
        try
        {
            //从文件中取得Hash描述 
            System.Security.Cryptography.HashAlgorithm SHA1 = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
            HashData = SHA1.ComputeHash(objFile);
            objFile.Close();
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// 获取Hash描述表
    /// </summary>
    /// <param name="objFile">待签名的文件</param>
    /// <param name="strHashData">Hash描述</param>
    /// <returns></returns>
    public static bool GetHash(System.IO.FileStream objFile, ref string strHashData)
    {
        try
        {
            //从文件中取得Hash描述 
            byte[] HashData;
            System.Security.Cryptography.HashAlgorithm SHA1 = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
            HashData = SHA1.ComputeHash(objFile);
            objFile.Close();
            strHashData = Convert.ToBase64String(HashData);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    #endregion

    #region RSA签名
    /// <summary>
    /// RSA签名
    /// </summary>
    /// <param name="strKeyPrivate">私钥</param>
    /// <param name="HashbyteSignature">待签名Hash描述</param>
    /// <param name="EncryptedSignatureData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureFormatter(string strKeyPrivate, byte[] HashbyteSignature, ref byte[] EncryptedSignatureData)
    {
        try
        {
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();

            RSA.FromXmlString(strKeyPrivate);
            System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
            //设置签名的算法为SHA1
            RSAFormatter.SetHashAlgorithm("SHA1");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// RSA签名
    /// </summary>
    /// <param name="strKeyPrivate">私钥</param>
    /// <param name="HashbyteSignature">待签名Hash描述</param>
    /// <param name="m_strEncryptedSignatureData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureFormatter(string strKeyPrivate, byte[] HashbyteSignature, ref string strEncryptedSignatureData)
    {
        try
        {
            byte[] EncryptedSignatureData;
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            RSA.FromXmlString(strKeyPrivate);
            System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
            //设置签名的算法为SHA1
            RSAFormatter.SetHashAlgorithm("SHA1");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
            strEncryptedSignatureData = Convert.ToBase64String(EncryptedSignatureData);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// RSA签名
    /// </summary>
    /// <param name="strKeyPrivate">私钥</param>
    /// <param name="strHashbyteSignature">待签名Hash描述</param>
    /// <param name="EncryptedSignatureData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureFormatter(string strKeyPrivate, string strHashbyteSignature, ref byte[] EncryptedSignatureData)
    {
        try
        {
            byte[] HashbyteSignature;

            HashbyteSignature = Convert.FromBase64String(strHashbyteSignature);
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();

            RSA.FromXmlString(strKeyPrivate);
            System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
            //设置签名的算法为SHA1
            RSAFormatter.SetHashAlgorithm("SHA1");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);

            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// RSA签名
    /// </summary>
    /// <param name="strKeyPrivate">私钥</param>
    /// <param name="strHashbyteSignature">待签名Hash描述</param>
    /// <param name="strEncryptedSignatureData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureFormatter(string strKeyPrivate, string strHashbyteSignature, ref string strEncryptedSignatureData)
    {
        try
        {
            byte[] HashbyteSignature;
            byte[] EncryptedSignatureData;
            HashbyteSignature = Convert.FromBase64String(strHashbyteSignature);
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            RSA.FromXmlString(strKeyPrivate);
            System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
            //设置签名的算法为SHA1
            RSAFormatter.SetHashAlgorithm("SHA1");
            //执行签名 
            EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
            strEncryptedSignatureData = Convert.ToBase64String(EncryptedSignatureData);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    #endregion

    #region RSA 签名验证
    /// <summary>
    /// RSA签名验证
    /// </summary>
    /// <param name="strKeyPublic">公钥</param>
    /// <param name="HashbyteDeformatter">Hash描述</param>
    /// <param name="DeformatterData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureDeformatter(string strKeyPublic, byte[] HashbyteDeformatter, byte[] DeformatterData)
    {
        try
        {
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            RSA.FromXmlString(strKeyPublic);
            System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
            //指定解密的时候HASH算法为SHA1
            RSADeformatter.SetHashAlgorithm("SHA1");
            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// RSA签名验证
    /// </summary>
    /// <param name="strKeyPublic">公钥</param>
    /// <param name="strHashbyteDeformatter">Hash描述</param>
    /// <param name="DeformatterData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureDeformatter(string strKeyPublic, string strHashbyteDeformatter, byte[] DeformatterData)
    {
        try
        {
            byte[] HashbyteDeformatter;
            HashbyteDeformatter = Convert.FromBase64String(strHashbyteDeformatter);
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            RSA.FromXmlString(strKeyPublic);
            System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
            //指定解密的时候HASH算法为SHA1
            RSADeformatter.SetHashAlgorithm("SHA1");
            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// RSA签名验证
    /// </summary>
    /// <param name="strKeyPublic">公钥</param>
    /// <param name="HashbyteDeformatter">Hash描述</param>
    /// <param name="strDeformatterData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureDeformatter(string strKeyPublic, byte[] HashbyteDeformatter, string strDeformatterData)
    {
        try
        {
            byte[] DeformatterData;
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            RSA.FromXmlString(strKeyPublic);
            System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
            //指定解密的时候HASH算法为SHA1
            RSADeformatter.SetHashAlgorithm("SHA1");
            DeformatterData = Convert.FromBase64String(strDeformatterData);
            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// RSA签名验证
    /// </summary>
    /// <param name="strKeyPublic">公钥</param>
    /// <param name="strHashbyteDeformatter">Hash描述</param>
    /// <param name="strDeformatterData">签名后的结果</param>
    /// <returns></returns>
    public static bool SignatureDeformatter(string strKeyPublic, string strHashbyteDeformatter, string strDeformatterData)
    {
        try
        {
            byte[] DeformatterData;
            byte[] HashbyteDeformatter;
            HashbyteDeformatter = Convert.FromBase64String(strHashbyteDeformatter);
            System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
            RSA.FromXmlString(strKeyPublic);
            System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
            //指定解密的时候HASH算法为SHA1
            RSADeformatter.SetHashAlgorithm("SHA1");
            DeformatterData = Convert.FromBase64String(strDeformatterData);
            if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    #endregion

    #endregion



}