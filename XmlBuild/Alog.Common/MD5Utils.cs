using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Alog.Common
{
    public class MD5Utils
    {
        /// <summary>
        /// 将字符串MD5加密后返回
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string EncrypMD5(string content)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// 将文件流MD5加密后返回
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static string EncrypMD5(FileStream fs)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            Byte[] bytes = md5.ComputeHash(fs);
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// 将源文件MD5加密后并写到新的文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="targetFilePath"></param>
        public static void EncrypMD5File(string sourceFilePath, string targetFilePath)
        {
            using (FileStream fs = File.OpenRead(sourceFilePath))
            {
                string content = EncrypMD5(fs);
                FileStream fsw = new FileStream(targetFilePath, System.IO.FileMode.Create);
                StreamWriter sw = new StreamWriter(fsw, System.Text.Encoding.Default);
                sw.Write(content);
                sw.Close();
                fsw.Close();

            }
        }
    }
}
