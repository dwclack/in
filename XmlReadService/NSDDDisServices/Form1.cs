using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Alog_WSKJSD;
using System.IO;

namespace NSDDDisServices
{
    public partial class Form1 : Form
    {
        static string FtpServiceUpPath = ClsLog.GetAppSettings("FtpServiceUpPath");
        static string FtpUpPath = ClsLog.GetAppSettings("FtpUpPath");
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ImportXMLData di = new ImportXMLData();
            //try
            //{
            //    foreach (string file in System.IO.Directory.GetFiles(FtpServiceUpPath))
            //    {
            //        string XMLDirName = di.OPNSXmlData(file);
            //        if (XMLDirName != "-1")
            //        {
            //            ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
            //                            @"" + FtpUpPath + @"\" + XMLDirName + @"\");

            //            ClsLog.DeleteFile(file);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //当数据库服务器连接断开导致异常时，定时器状态需要开启
            //    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            //}

            string HGZSFtpServiceUpPath = ClsLog.GetAppSettings("HGZSFtpServiceUpPath");
            string HGZSFtpUpPath = ClsLog.GetAppSettings("HGZSFtpUpPath");
            string HGZSDirNode = ClsLog.GetAppSettings("HGZSDirNode");
            ImportXMLData di = new ImportXMLData();
            try
            {
                foreach (string file in System.IO.Directory.GetFiles(HGZSFtpServiceUpPath))
                {
                    string XMLDirName = di.OPHGZSXmlData(file, HGZSDirNode);
                    if (XMLDirName != "-1")
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + HGZSFtpUpPath + @"\" + XMLDirName + @"\");

                        ClsLog.DeleteFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("好久没写代码了。。哎哎");
        }
    }
}
