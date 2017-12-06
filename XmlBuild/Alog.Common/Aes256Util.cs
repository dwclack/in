using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace Alog.Common
{
    public class Aes256Util
    {
        /// <summary>
        /// AES256加密，并返回Base64字符串
        /// </summary>
        /// <param name="content">明文</param>
        /// <param name="key">32位字符的密钥</param>
        /// <param name="iv">16位的向量</param>
        /// <returns>明文密钥向量均不能为空否则返回空，密钥不是32位或向量不是16位也返回空，参数均满足条件后返回密文的base64字符串</returns>
        public static string Aes256Encode(string content, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(iv))
                return "";

            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(content);
            byte[] bIv = Encoding.UTF8.GetBytes(iv);

            if (keyArray.Length != 32 || bIv.Length != 16)
                return "";
            try
            {
                using (RijndaelManaged rDel = new RijndaelManaged())
                {
                    rDel.Key = keyArray;
                    rDel.IV = bIv;
                    rDel.Mode = CipherMode.ECB;
                    rDel.Padding = PaddingMode.PKCS7;
                    ICryptoTransform cTransform = rDel.CreateEncryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// AES256解密
        /// </summary>
        /// <param name="content">经过AES256加密并转换成BASE64的字符串</param>
        /// <param name="VersionId">加密版本ID</param>
        /// <returns>密文密钥向量均不能为空否则返回空，密钥不是32位或向量不是16位也返回空，参数均满足条件后返回解密后的字符串</returns>

        public static void DataTableAes256Decode(DataTable Dt, string EncodedColumns)
        {
            try
            {

                if (!string.IsNullOrEmpty(EncodedColumns))
                {
                    //判断是否要解密


                    if (Dt.Columns.Contains("EncodedVersion"))
                    {
                        DataRow[] drs = Dt.Select("EncodedVersion is not null");
                        if (drs.Length <= 0)
                        {
                            return;
                        }
                        else
                        {

                            foreach (DataRow dr in drs)
                            {
                                string EncodedVersion = "";
                                EncodedVersion = dr["EncodedVersion"].ToString();
                                //如果需要解密，把解密的字段更新一下
                                if (!string.IsNullOrEmpty(EncodedVersion))
                                {
                                    string[] EncodedColumnsM = EncodedColumns.Split(';');
                                    foreach (string EColumns in EncodedColumnsM)
                                    {
                                        if (dr.Table.Columns.Contains(EColumns))
                                        {
                                            string fdsf=Aes256Util.Aes256Decode(dr[EColumns].ToString(), EncodedVersion);
                                            dr[EColumns] = fdsf;
                                        }
                                    }
                                }
                            }

                        }
                    }

                }


            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// AES256解密
        /// </summary>
        /// <param name="content">经过AES256加密并转换成BASE64的字符串</param>
        /// <param name="VersionId">加密版本ID</param>
        /// <returns>密文密钥向量均不能为空否则返回空，密钥不是32位或向量不是16位也返回空，参数均满足条件后返回解密后的字符串</returns>

        public static void DataRowAes256Decode(DataRow dr, string EncodedColumns)
        {
            if (!string.IsNullOrEmpty(EncodedColumns))
            {
                //判断是否要解密
                string EncodedVersion = "";

                if (dr.Table.Columns.Contains("EncodedVersion"))
                {
                    EncodedVersion = dr["EncodedVersion"].ToString();
                }
                //如果需要解密，把解密的字段更新一下
                if (!string.IsNullOrEmpty(EncodedVersion))
                {
                    string[] EncodedColumnsM = EncodedColumns.Split(';');
                    foreach (string EColumns in EncodedColumnsM)
                    {
                        if (dr.Table.Columns.Contains(EColumns))
                        {
                            dr[EColumns] = Aes256Util.Aes256Decode(dr[EColumns].ToString(), "EncodedVersion");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// AES256解密
        /// </summary>
        /// <param name="content">经过AES256加密并转换成BASE64的字符串</param>
        /// <param name="VersionId">加密版本ID</param>
        /// <returns>密文密钥向量均不能为空否则返回空，密钥不是32位或向量不是16位也返回空，参数均满足条件后返回解密后的字符串</returns>

        public static string Aes256Decode(string content, string VersionId)
        {
            try
            {

                DataRow[] Dr = RepXml.RijnSet.Select("VersionId='" + VersionId + "'");
                if (Dr.Length <= 0)
                {
                    return content;
                }
                string key = Dr[0]["Key"].ToString();
                string iv = Dr[0]["IV"].ToString();
                return Aes256Decode(content, key, iv);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// AES256解密
        /// </summary>
        /// <param name="content">经过AES256加密并转换成BASE64的字符串</param>
        /// <param name="key">32位字符的密钥</param>
        /// <param name="iv">16位的向量</param>
        /// <returns>密文密钥向量均不能为空否则返回空，密钥不是32位或向量不是16位也返回空，参数均满足条件后返回解密后的字符串</returns>
        public static string Aes256Decode(string content, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(iv))
                return "";
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(content);
            byte[] bIv = Encoding.UTF8.GetBytes(iv);

            if (keyArray.Length != 32 || bIv.Length != 16)
                return "";
            try
            {
                using (RijndaelManaged rDel = new RijndaelManaged())
                {
                    rDel.Key = keyArray;
                    rDel.IV = bIv;
                    rDel.Mode = CipherMode.ECB;
                    rDel.Padding = PaddingMode.PKCS7;

                    ICryptoTransform cTransform = rDel.CreateDecryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                    return Encoding.UTF8.GetString(resultArray);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
