using System;
using System.Collections.Generic;

using System.Web;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace SQLExcService
{
    public class EncUtil
    {
        /// <summary>
        /// Des加密
        /// </summary>
        /// <param name="clearText"></param>
        /// <returns></returns>
        public static string DesEncrypt(string clearText)
        {
            byte[] byKey = System.Text.ASCIIEncoding.UTF8.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.UTF8.GetBytes(IV_64);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

            MemoryStream memStream = new MemoryStream();
            //以寫模式 把數據流和要加密的數據流建立連接
            CryptoStream cryStream = new CryptoStream(memStream, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            //將要加密的數據轉換為UTF8編碼的數組
            byte[] clearTextArray = Encoding.UTF8.GetBytes(clearText);

            //加密 並寫到 内存流memStream中
            cryStream.Write(clearTextArray, 0, clearTextArray.Length);
            //清空緩衝區
            cryStream.FlushFinalBlock();

            //將8位無符號整數數組 轉換為 等效的System.String 的形式.
            return Convert.ToBase64String(memStream.ToArray());
        }

        /// <summary>
        /// Des解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DesDecrypt(string encryptedText)
        {
            byte[] byKey = System.Text.ASCIIEncoding.UTF8.GetBytes(KEY_64);
            byte[] byIV = System.Text.ASCIIEncoding.UTF8.GetBytes(IV_64);

            //
            byte[] byteArray = Convert.FromBase64String(encryptedText);

            MemoryStream memStream = new MemoryStream();

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            CryptoStream cryStream = new CryptoStream(memStream, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Write);

            cryStream.Write(byteArray, 0, byteArray.Length);
            //清空緩衝區
            cryStream.FlushFinalBlock();

            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            //把字節數組轉換為 等效的System.String 的形式.
            return encoding.GetString(memStream.ToArray());
        }

        private const string KEY_64 = "Alog_Key";  //公鈅
        private const string IV_64 = "Alog_Key";   //私鈅,注意了:是8个字符，64位
    }
}