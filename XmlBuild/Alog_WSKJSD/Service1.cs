using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using ASPNetPortal;
using System.Timers;
using Alog_WSKJSD;
using System.Configuration;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {

        int Flag = 0;
        int pCount = 7;   //报文类型数
        private readonly Thread[] threadHZ;
        static string BWReadType = ClsLog.GetAppSettings("BWReadType");
        static int HZThreadCount = Convert.ToInt32(ClsLog.GetAppSettings("HZThreadCount"));
        public Service1()
        {
            InitializeComponent();
            threadHZ = new Thread[HZThreadCount];
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                NLogger.WriteLog("================Service Runing:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                //======获取并设置定时间隔
                //int j = 0;
                int tmrBWInterval = 300000;
                int tmrHZInterval = 360000;

                Flag = 1;
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("BWInterval")))
                    tmrBWInterval = Convert.ToInt32(ClsLog.GetAppSettings("BWInterval"));
                if (!string.IsNullOrEmpty(ClsLog.GetAppSettings("HZInterval")))
                    tmrHZInterval = Convert.ToInt32(ClsLog.GetAppSettings("HZInterval"));

                //foreach (bool bOK in RepXml.AllOK) //初始启动所有线程
                //{
                //    RepXml.AllOK[j] = true;
                //    j++;
                //}
                DataRow[] drs = RepXml.dtRepXmlSet.Select("parentid= 0");
                foreach (DataRow dr in drs)  //处理每种报文类型
                {
                    RepXml.AllOK.Add(dr["RepTitle"].ToString(), true);
                }

                Flag = 2;
                //======获取报文类型并启动定时器
                if (tmrBWInterval > 0)
                {
                    timer1.Interval = tmrBWInterval;
                    timer1.Enabled = true;
                    timer1.Start();
                }
                if (tmrHZInterval > 0)
                {
                    timer2.Interval = tmrHZInterval;
                    timer2.Enabled = true;
                    timer2.Start();
                }
                Flag = 3;
                NLogger.WriteLog("============== Service Start:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
            }
            catch (Exception ex)
            {
                NLogger.WriteLog("============== Service Error:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
                NLogger.WriteLog("  Service Error Text: Flag=" + Flag.ToString() + ex.Message, "服务日志");
            }
        }



        protected override void OnStop()
        {
            timer1.Stop();
            timer1.Enabled = false;

            NLogger.WriteLog("****************STOP:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "服务日志");
        }


        /// <summary>
        /// 报文生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            //timer1.Enabled = false;
            try
            {
                timer1.Enabled = false;
                NLogger.WriteLog("报文生成开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "报文生成日志");
                int i = 0;
                Flag = 4;
                DataRow[] drs = RepXml.dtRepXmlSet.Select("parentid= 0");
                Thread[] t = new Thread[drs.Length];  //根据报文类型数定义线程数
                foreach (DataRow dr in drs)  //处理每种报文类型
                {
                    try
                    {
                        //NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + dr["RepTitle"].ToString() + "_线程开始", dr["RepTitle"].ToString());
                        if (RepXml.AllOK[dr["RepTitle"].ToString()] == false)
                        {
                            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + dr["RepTitle"].ToString() + "_线程阻塞", dr["RepTitle"].ToString());
                            continue;  //当报文是回执等类型或上次线程未结束，则跳至下个循环
                        }
                        if (!string.IsNullOrEmpty(BWReadType))
                        {
                            int isBw = 1;
                            for(int k=0;k<BWReadType.Split(',').Length;k++)
                            {
                                if (BWReadType.Split(',')[k].Trim() == dr["RepTitle"].ToString().Trim())
                                {
                                    isBw = 0;
                                }
                            }
                            ///如果不是指定的报文类型就不执行
                            if (isBw == 1)
                            {
                                continue;
                            }
                        }

                        ClsThreadParam ClsParam = new ClsThreadParam();
                        ClsParam.RepTitle = dr["RepTitle"].ToString();
                        ClsParam.dr = dr;
                        ClsParam.tCount = i;
                        ClsParam.AutoID = Convert.ToInt16(dr["AutoID"].ToString());
                        ClsParam.DSID = Convert.ToInt16(dr["DSID"].ToString());

                        RepXml rx = new RepXml();
                        t[i] = new Thread(new ParameterizedThreadStart(rx.ThreadHandle));
                        t[i].Start(ClsParam);

                    }
                    catch (Exception ex)
                    {
                        //当数据库服务器连接断开导致异常时，定时器状态需要开启
                        timer1.Enabled = true;
                        NLogger.WriteLog("Flag=" + Flag.ToString() + ex.Message, "错误日志");
                        i++;
                        continue;
                    }
                    i++;
                }
                NLogger.WriteLog("报文生成结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "报文生成日志");
                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer1.Enabled = true;
                NLogger.WriteLog("Flag=" + Flag.ToString() + ex.Message, "错误日志");
            }
        }

        private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            //======== 回执处理定时器
            #region 多线程读回执
            timer2.Enabled = false;
            NLogger.WriteLog("报文回执读取开始" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "报文回执日志");
            try
            {
                string HZPath = ClsLog.GetAppSettings("HZPath");
                string[] Files = System.IO.Directory.GetFiles(HZPath);
                int dataCount = Files.Length;
                var list = Files.Cast<string>().ToArray();
                int pageCount = HZThreadCount;//分页数默认1页
                int pageSize = dataCount % pageCount == 0 ? (dataCount / pageCount) : (dataCount / pageCount + 1);
                NLogger.WriteLog("报文回执读取分页开始 , Data Count : " + dataCount + ", Page Size:" + pageSize, "MultiPage");
                Parallel.For(0, pageCount, pageIndex =>
                {
                    var data = list.Skip(pageIndex * pageSize).Take(pageSize);
                    foreach (string strFile in data)
                    {
                        if (!string.IsNullOrEmpty(strFile))//一般来说strFiles的最后一个元素可能是空字符串
                        {
                            try
                            {
                                FileInfo fileInfo = new FileInfo(strFile.Trim());
                                if (fileInfo.Length <= 0)
                                {
                                    //如果文件小于等于0就执行跳过
                                    continue;
                                }
                                ReadHZXML(strFile);
                            }
                            catch (Exception ex)
                            {
                                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "读取回执文件失败：" + strFile + ex.Message, "错误日志");
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                //当数据库服务器连接断开导致异常时，定时器状态需要开启
                timer2.Enabled = true;
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "错误日志");
            }
            finally
            {
                timer2.Enabled = true;
            }
            NLogger.WriteLog("报文回执读取结束" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "报文回执日志");
            timer2.Enabled = true;
            #endregion
        }

        //private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    //======== 回执处理定时器
        //    #region 多线程读回执
        //    timer2.Enabled = false;
        //    try
        //    {
        //        if (!AllThreadIsAlive())
        //        {
        //            string HZPath = ClsLog.GetAppSettings("HZPath");
        //            string[] Files = System.IO.Directory.GetFiles(HZPath);
        //            for (int i = 0; i < threadHZ.Length; i++)
        //            {
        //                if (Files.Length > i)
        //                {
        //                    threadHZ[i] = new Thread(ReadHZXML);
        //                    threadHZ[i].Start(Files[i]);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //当数据库服务器连接断开导致异常时，定时器状态需要开启
        //        timer2.Enabled = true;
        //        NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
        //    }
        //    timer2.Enabled = true;
        //    #endregion

        //    #region 无多线程读回执
            
        //    //timer2.Enabled = false;
        //    //DataImport di = new DataImport();
        //    //try
        //    //{
        //    //    string HZPath = ClsLog.GetAppSettings("HZPath");
        //    //    string HZPathTempBak = ClsLog.GetAppSettings("HZPathTempBak");
        //    //    string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
        //    //    string[] Files = System.IO.Directory.GetFiles(HZPath);


        //    //    foreach (string file in System.IO.Directory.GetFiles(HZPath))
        //    //    {
        //    //        try
        //    //        {
        //    //            if (di.ReadFile(file) == 0)
        //    //            {
        //    //                ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
        //    //                            @"" + HZPathTempBak + @"\");
        //    //                ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
        //    //                                @"" + HZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
        //    //                di.ReadFile(file);
        //    //                ClsLog.DeleteFile(file);

        //    //            }
        //    //        }
        //    //        catch (Exception ex)
        //    //        {
        //    //            NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");

        //    //            continue;
        //    //        }
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    //当数据库服务器连接断开导致异常时，定时器状态需要开启
        //    //    timer2.Enabled = true;
        //    //    NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "服务日志");
        //    //}
        //    //timer2.Enabled = true;
            
        //    #endregion
        //}

        public void ReadHZXML(object fileS)
        {
            DataImport di = new DataImport();
            string HZPathTempBak = ClsLog.GetAppSettings("HZPathTempBak");
            string HZPathBak = ClsLog.GetAppSettings("HZPathBak");
            string file = fileS.ToString();
            try
            {
                if (di.ReadFile(file) == 0)
                {
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                @"" + HZPathTempBak + @"\");
                    ClsLog.CopyFile(Path.GetFileName(file), Path.GetDirectoryName(file) + @"\",
                                    @"" + HZPathBak + DateTime.Now.ToString("yyyyMM") + @"\");
                    //di.ReadFile(file);
                    ClsLog.DeleteFile(file);

                }
            }
            catch (Exception ex)
            {
                NLogger.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ex.Message, "错误日志");

            }
        }

        public bool AllThreadIsAlive()
        {
            bool IsAliv = false;
            for (int i = 0; i < threadHZ.Length; i++)
            {
                if (threadHZ[i] != null && threadHZ[i].IsAlive)
                {
                    IsAliv = true;
                    return IsAliv;
                }
            }
            return IsAliv;
        }

    }
}
