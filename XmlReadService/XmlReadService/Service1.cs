using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Alog_WSKJSD;

namespace XmlReadService
{
    public partial class Service1 : ServiceBase
    {
        static string XYSFilePath = ClsLog.GetAppSettings("XYSFilePath");
        static string XYSSavePath = ClsLog.GetAppSettings("XYSSavePath");
        static string XYSBakPath = ClsLog.GetAppSettings("XYSBakPath");
        static string SearchKey = ClsLog.GetAppSettings("SearchKey");

        static string HGDDPath = ClsLog.GetAppSettings("HGDDPath");
        static string HGDDPathBak = ClsLog.GetAppSettings("HGDDPathBak");
        static string HGDDPathBakFtp = ClsLog.GetAppSettings("HGDDPathBakFtp");
        static string HGDDOwnerId = ClsLog.GetAppSettings("HGDDOwnerId");
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
           

            try
            {
                ClsLog.AppendLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                //======获取并设置定时间隔
                int tmrtimer1 = 300000;
                int tmrtimer2 = 300000;
                int tmrtimer3 = 300000;
                int tmrtimer4 = 300000;
                int tmrtimer5 = 300000;
                int tmrtimer6 = 300000;
                int tmrtimer7 = 300000;
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HZInterval")))
                {
                    tmrtimer1 = Convert.ToInt32(ClsLog.GetAppSettings("HZInterval"));
                }
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HZInterval")))
                {
                    tmrtimer2 = Convert.ToInt32(ClsLog.GetAppSettings("HZIntervalShare"));
                }
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("XYSIntervalShare")))
                {
                    tmrtimer3 = Convert.ToInt32(ClsLog.GetAppSettings("XYSIntervalShare"));
                }
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("NSInterval")))
                {
                    tmrtimer4 = Convert.ToInt32(ClsLog.GetAppSettings("NSInterval"));
                }

                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HGDDInterval")))
                {
                    tmrtimer5 = Convert.ToInt32(ClsLog.GetAppSettings("HGDDInterval"));
                }

                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("DYCKInterval")))
                {
                    tmrtimer6 = Convert.ToInt32(ClsLog.GetAppSettings("DYCKInterval"));
                }
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("NSGJInterval")))
                {
                    tmrtimer7 = Convert.ToInt32(ClsLog.GetAppSettings("NSGJInterval"));
                }
                //======获取报文类型并启动定时器
                if (tmrtimer1 > 0)
                {
                    timer1.Interval = tmrtimer1;
                    timer1.Enabled = true;
                    timer1.Start();
                }
                if (tmrtimer2 > 0)
                {
                    timer2.Interval = tmrtimer2;
                    timer2.Enabled = true;
                    timer2.Start();
                }
                if (tmrtimer3 > 0)
                {
                    timer3.Interval = tmrtimer3;
                    timer3.Enabled = true;
                    timer3.Start();
                }
                if (tmrtimer4 > 0)
                {
                    timer4.Interval = tmrtimer4;
                    timer4.Enabled = true;
                    timer4.Start();
                }
                if (tmrtimer5 > 0)
                {
                    timer5.Interval = tmrtimer5;
                    timer5.Enabled = true;
                    timer5.Start();
                }
                if (tmrtimer6 > 0)
                {
                    timer6.Interval = tmrtimer6;
                    timer6.Enabled = true;
                    timer6.Start();
                }
                if (tmrtimer7 > 0)
                {
                    timer7.Interval = tmrtimer7;
                    timer7.Enabled = true;
                    timer7.Start();
                }
                ClsLog.AppendLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            }
            catch (Exception ex)
            {
                ClsLog.AppendLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                ClsLog.AppendLog("  Service Error Text: Flag=" + ex.Message, "服务日志");
            }
        }

        protected override void OnStop()
        {
            timer1.Stop();
            timer1.Enabled = false;
            timer2.Stop();
            timer2.Enabled = false;
            timer3.Stop();
            timer3.Enabled = false;
            timer4.Stop();
            timer4.Enabled = false;
            timer5.Stop();
            timer5.Enabled = false;
            timer6.Stop();
            timer6.Enabled = false;
            timer7.Stop();
            timer7.Enabled = false;
            ClsLog.AppendLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }

       /// <summary>
       ///海关回执中小商家、天猫的自动放到share路径下面
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            //======== 回执处理定时器
            timer1.Enabled = false;
            ImportXMLData di = new ImportXMLData();
            try
            {
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
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer1.Enabled = true;
        }

        /// <summary>
        /// 报文回执格式异常进行处理重新读取出来，暂时不用，文件太多无法一个个查找
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 回执处理定时器
            timer2.Enabled = false;
            ImportXMLData di = new ImportXMLData();
            try
            {
                string HZPath = ClsLog.GetAppSettings("HZPath");
                string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
                //string[] Files = System.IO.Directory.GetFiles(HZPath);


                foreach (string file in System.IO.Directory.GetFiles(HZPath))
                {
                    if (di.ImportReadData(file) == 0)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + HZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
                        //di.ReadFile(file);
                        ClsLog.DeleteFile(file);

                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer2.Enabled = true;
        }

        /// <summary>
        /// 读取行邮税回执
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer3_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer3.Enabled = false;

            ImportXMLData di = new ImportXMLData();
            try
            {
                timer3AllDirectory(di, XYSFilePath);
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer3.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer3.Enabled = true;
        }

        public void timer3AllDirectory(ImportXMLData di,string dir)
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

        private void timer4_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 回执处理定时器
            timer4.Enabled = false;
            ImportXMLData di = new ImportXMLData();
            try
            {
                string NSHZPath = ClsLog.GetAppSettings("NSHZPath");
                string NSHZPathBak = ClsLog.GetAppSettings("NSHZPathBak");
                //string[] Files = System.IO.Directory.GetFiles(HZPath);


                foreach (string file in System.IO.Directory.GetFiles(NSHZPath))
                {
                    if (di.ImportNSReadData(file) == 0)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + NSHZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
                        //di.ReadFile(file);
                        ClsLog.DeleteFile(file);

                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer4.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer4.Enabled = true;
        }

        /// <summary>
        /// 海关订单报文分货主进行存放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer5_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 回执处理定时器
            timer5.Enabled = false;
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
                        //di.ReadFile(file);
                        ClsLog.DeleteFile(file);

                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer5.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer5.Enabled = true;
        }

        /// <summary>
        /// 读取机场单一窗口回执到指定目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer6_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            //======== 回执处理定时器
            timer6.Enabled = false;
            ImportXMLData di = new ImportXMLData();
            try
            {
                string DYCKHZPath = ClsLog.GetAppSettings("DYCKHZPath");
                string DYCKHZPathBak = ClsLog.GetAppSettings("DYCKHZPathBak");
                string DYCKHZPathShare = ClsLog.GetAppSettings("DYCKHZPathShare");
                string DYCKHZPathError = ClsLog.GetAppSettings("DYCKHZPathError");

                foreach (string file in System.IO.Directory.GetFiles(DYCKHZPath))
                {
                    int reval = di.CopyDYCKFileData(file);
                    if (reval == 0)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + DYCKHZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                        @"" + DYCKHZPathShare + @"\");

                        ClsLog.DeleteFile(file);
                    }
                    else if(reval==-2)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                       @"" + DYCKHZPathError  + @"\");

                        ClsLog.DeleteFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer6.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer6.Enabled = true;
        }

        /// <summary>
        /// 读取南沙回执到share目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer7_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //======== 回执处理定时器
            timer7.Enabled = false;
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
                    else if(retval==-2)
                    {
                        ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                       @"" + NSGJHZPathError  + @"\");
                        ClsLog.DeleteFile(file);
                    }
                }
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer7.Enabled = true;
                ClsLog.AppendLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
            }
            timer7.Enabled = true;
        }
    }
}
