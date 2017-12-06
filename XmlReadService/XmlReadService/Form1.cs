using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Alog_WSKJSD;
namespace XmlReadService
{
    public partial class Form1 : Form
    {
        static string XYSFilePath = ClsLog.GetAppSettings("XYSFilePath");
        static string XYSSavePath = ClsLog.GetAppSettings("XYSSavePath");
        static string XYSBakPath = ClsLog.GetAppSettings("XYSBakPath");
        static string SearchKey = ClsLog.GetAppSettings("SearchKey");
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s =
                "D:\\NEWCBL\\WS_KJSD_ProgramTest\\6_GZ_1.0_HG\\WS_KJSD_XML_BAK\\Bak_20151110\\880022_20151110101642T001.xml";
            string tempFileName = Path.GetFileName(s);
            //ImportXMLData di = new ImportXMLData();
            //try
            //{
            //    string HZPath = ClsLog.GetAppSettings("HZPath");
            //    string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
            //    string[] Files = System.IO.Directory.GetFiles(HZPath);


            //    foreach (string file in System.IO.Directory.GetFiles(HZPath))
            //    {
            //        if (di.ImportReadData(file) == 0)
            //        {
            //            ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
            //                            @"" + HZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
            //            //di.ReadFile(file);
            //            ClsLog.DeleteFile(file);

            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //当数据库服务器连接断开导致异常时，定时器状态需要开启
            //    ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ImportXMLData di = new ImportXMLData();
            string HGHZPath = ClsLog.GetAppSettings("HGHZPath");
            string HGHZPathBak = ClsLog.GetAppSettings("HGHZPathBak");
            string HGHZPathShare = ClsLog.GetAppSettings("HGHZPathShare");


            foreach (string file in System.IO.Directory.GetFiles(HGHZPath))
            {
                if (di.CopyFileData(file) == 0)
                {
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                    @"" + HGHZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                    @"" + HGHZPathShare + @"\");

                    ClsLog.DeleteFile(file);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImportXMLData di = new ImportXMLData();
            try
            {
                timer3AllDirectory(di, XYSFilePath);
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
        }

        public void timer3AllDirectory(ImportXMLData di, string dir)
        {
            foreach (string file in System.IO.Directory.GetFiles(dir, SearchKey))
            {
                if (di.CopyXYSFileData(file) == 0)
                {
                    DateTime dt = File.GetLastWriteTime(file);
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                    @"" + XYSSavePath + dt.ToString("yyyyMMdd") + @"\");
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                    @"" + XYSBakPath + dt.ToString("yyyyMMdd") + @"\");
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                   @"" + XYSBakPath + dt.ToString("yyyyMM") + @"\");

                    ClsLog.DeleteFile(file);
                }
            }
            string[] directorys = System.IO.Directory.GetDirectories(dir);
            if (directorys.Length <= 0) //如果该目录总没有其他文件夹
                return;
            else
            {
                for (int i = 0; i < directorys.Length; i++)
                {
                    timer3AllDirectory(di, directorys[i]);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //======== 回执处理定时器
            ImportXMLData di = new ImportXMLData();
            try
            {
                string HGDDPath = ClsLog.GetAppSettings("HGDDPath");
                string HGDDPathBak = ClsLog.GetAppSettings("HGDDPathBak");
                //string[] Files = System.IO.Directory.GetFiles(HZPath);


                foreach (string file in System.IO.Directory.GetFiles(HGDDPath, "880020*.xml"))
                {
                    if (di.CopyHGDDReadData(file) == 0)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + HGDDPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
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

        private void button5_Click(object sender, EventArgs e)
        {
            ImportXMLData di = new ImportXMLData();
            try
            {
                string NSGJHZPath = ClsLog.GetAppSettings("NSGJHZPath");
                string NSGJHZPathBak = ClsLog.GetAppSettings("NSGJHZPathBak");
                string NSGJHZPathShare = ClsLog.GetAppSettings("NSGJHZPathShare");
                string NSGJHZPathError = ClsLog.GetAppSettings("NSGJHZPathError");

                foreach (string file in System.IO.Directory.GetFiles(NSGJHZPath))
                {
                    int retval = di.ImportNSHZReadData(file);
                    if (retval == 0)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + NSGJHZPathBak + DateTime.Now.ToString("yyyyMMdd") + @"\");
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + NSGJHZPathShare + @"\");

                        ClsLog.DeleteFile(file);
                    }
                    else if (retval == -2)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                       @"" + NSGJHZPathError + @"\");
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
    }
}
