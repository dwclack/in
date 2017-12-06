using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace TestFrom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
//            string tempStr = @"<kjsoDeclResponse><externalNo>234234</externalNo><lockFlag>1</lockFlag>
//                
//                <item><status>2</status><result>11</result><createTime>2016-08-31</createTime></item><item><status>2</status><result>11</result><createTime>2016-08-31</createTime></item></kjsoDeclResponse>";
//            DataSet importDS = new DataSet();
//            Stream stream = new MemoryStream(Encoding.Default.GetBytes(tempStr));
//            importDS.ReadXml(stream);
          string str=  EncrypMD5("nbtm001" + "alog001");
          string str2 = EncrypMD5("LDG0912" + "ed36e31d-0ff7-4430-9bde-1421ec40ef07");
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

            string ret = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                ret += Convert.ToString(bytes[i], 16).PadLeft(2, '0');
            }

            return ret.PadLeft(32, '0');
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /// <summary>
            /// 字节数组
            /// </summary>
            //string strFileName = "E:/SSTEMP/KJDOCREC_KJGGPT2016111510311515722.xml";
            //Byte[] buffer = new Byte[512];
            //FileStream input = new FileStream(strFileName, FileMode.Open);
            //int iBytes = 0;
            //int len=input.Read(buffer, 0, buffer.Length);

            //while ((iBytes = input.Read(buffer, 0, buffer.Length)) > 0)
            //{
            //    len = 99;
            //}
            string strFile = DateTime.Now.ToString("yyyyMMdd") + @".txt";
            NLoggerTest.NLogger.WriteLog("", "CS");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FileStream output = null;
            FileStream output1 = null;
            try
            {
               
                using (output = new FileStream("D:\\ZBB\\新建文本文档.txt", FileMode.Create))
                {

                    FileInfo fileInfo = new FileInfo("D:\\ZBB\\新建文本文档11.txt");
                    long len1 = fileInfo.Length;
                    output1 = new FileStream("D:\\ZBB\\新建文本文档11.txt", FileMode.Create);
                    long len=output1.Length;
                    string s ="1";
                    output.Close();
                }
            //output.Close();
            }
            catch (Exception ex)
            {
                //if (output != null)
                //{
                //    output.Close();
                //}
                //throw ex;
            }
        }
    }
}
