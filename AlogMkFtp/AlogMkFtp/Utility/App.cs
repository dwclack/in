using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlogMkFtp.Utility
{
    public class App
    {
        public App()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }



        public  enum UpLoadType
        {
            /// <summary>
            /// 每日库存结算
            /// </summary>
            DailyStockSettlement=1,
            /// <summary>
            /// 进,出,盘点,调整
            /// </summary>
            Ordinaryorder=0
        }   





        /// <summary>
        /// WMS的数据库
        /// </summary>
        public static string g_strWMSDB = "";

        /// <summary>
        /// MK的FTP账户信息
        /// </summary>
        public static string g_strMKFTP = "";

        /// <summary>
        /// 货主代码
        /// </summary>
        public static string g_strOwnerCode = "";

        /// <summary>
        /// 当前应用程序的路径
        /// </summary>
        public static string g_strCurrentApplicationPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// RDC仓库的代码 ：6029505/6029529
        /// </summary>
        public static string g_strRDCCode = "";

        /// <summary>
        /// 公司代码 ：30602
        /// </summary>
        public static string g_strCompanyCode = "";

        /// <summary>
        /// 公司简称 ：ALOG
        /// </summary>
        public static string g_strCompanyShortName = "";

        /// <summary>
        /// 上传时间间隔
        /// </summary>
        public static string g_strFtpUploadInterVal = "";

        /// <summary>
        /// 下载时间间隔
        /// </summary>
        public static string g_strFtpDownloadInterVal = "";

        /// <summary>
        /// 压缩文件尾数
        /// </summary>
        public static string g_strZipNextNumber = "";
        
        /// <summary>
        /// ftp地址
        /// </summary>
        public static string g_strFtpHostReport = "";
        
        /// <summary>
        /// ftp用户名
        /// </summary>
        public static string g_strFtpUSerReport = "";
        
        /// <summary>
        /// ftp密码
        /// </summary>
        public static string g_strFtpPwdReport = "";


        /// <summary>
        /// ftp地址
        /// </summary>
        public static string g_strFtpHostReportSecond = "";

        /// <summary>
        /// ftp用户名
        /// </summary>
        public static string g_strFtpUSerReportSecond = "";

        /// <summary>
        /// ftp密码
        /// </summary>
        public static string g_strFtpPwdReportSecond = "";






        /// <summary>
        /// 接收上传回执的服务器ftp地址
        /// </summary>
        public static string g_strFtpHostHZ = "";

        /// <summary>
        /// 接收上传回执的服务器ftp用户名
        /// </summary>
        public static string g_strFtpUSerHZ = "";

        /// <summary>
        /// 接收上传回执的服务器ftp密码
        /// </summary>
        public static string g_strFtpPwdHZ = "";


        
            
        /// <summary>
        /// 读取回执目录第一级所有文件夹的xml文件（当为true时，节点“ReadHZBackwardMonths”“ReadHZDays”的值无效，反之则生效 ）
        /// </summary>
        public static string ReadAllFirstLeavlDirHZ = ""; 
        /// <summary>
        /// 读取回执往前推月数 （比如仅读取去当前月则为0或者为空，最近两个月为1，最近三个月为2；）
        /// </summary>
        public static string ReadHZBackwardMonths= "";   
        /// <summary>
        /// 当前月的第N天前（包含第N天）需要读取前一个月的回执文件夹的回执，默认第一天 
        /// </summary>
        public static string ReadHZDays = "";
        /// <summary>
        /// 下载完报文是否在FTP服务器上备份报文  value值为：true则备份  其他值一概不备份 默认为：false 不备份   备份目录为XML借点WS_XML_BAK的值  
        /// </summary>
        public static string DownLoadOverThenBackFTPXML = "";
        /// <summary>
        /// 是否启动FTP_HostReport  value值为：true则启动  其他值一概不备份 默认为：false 不启动
        /// </summary>
        public static string StartTimerFTP_HostReport = "";
        /// <summary>
        /// 是否启动FTP_HostReportSecond  value值为：true则启动  其他值一概不备份 默认为：false 不启动
        /// </summary>
        public static string StartTimerFTP_HostReportSecond = "";


        

        


        /// <summary>
        /// 每日上传库存结算小时时间
        /// </summary>
        public static string g_strDailyStockSettlementTime = "";

        /// <summary>
        /// 每日上传库存结算分钟时间
        /// </summary>
        public static string g_strDailyStockSettlementMinute = "";
        ///// <summary>
        ///// 国检报文XML
        ///// </summary>
        //public static string WS_XML = "";
        /// <summary>
        /// FTP服务器报文备份目录
        /// </summary>
        public static string WS_XML_BAK = "";
        /// <summary>
        /// FTP服务器报文目录
        /// </summary>
        public static string WS_XML_Ftp = "";
        /// <summary>
        /// FTP服务器回执目录
        /// </summary>
        public static string WS_HZ = "";
        /// <summary>
        /// FTP服务器回执临时目录
        /// </summary>
        public static string WS_HZTemp = "";
        /// <summary>
        /// 中转PC机回执目录
        /// </summary>
        public static string PCHZ = "";
        /// <summary>
        /// 中转PC机回执备份目录
        /// </summary>
        public static string PCBakHZ = "";
        /// <summary>
        /// 中转PC机保存FTP服务器报文的备份目录
        /// </summary>
        public static string BakXml = "";
        ///// <summary>
        ///// 发送XML
        ///// </summary>
        //public static string SendXMl = "";
        /// <summary>
        /// 中转PC机保存FTP服务器报文的临时目录
        /// </summary>
        public static string FtpXML = "";       
        /// <summary>
        /// 中转PC机保存FTP服务器报文的正式目录
        /// </summary>
        public static string FtpXMLTemp = "";


        
        ///// <summary>
        ///// 心怡供国检获取File的根目录
        ///// </summary>
        //public static string PCFileRoot = "";  
        /// <summary>
        /// FTP服务器下载根目录
        /// </summary>
        public static string DownloadFolder = ""; 
        /// <summary>
        /// 邮件服务器地址
        /// </summary>
        public static string SmtpHostAddress = "";
        /// <summary>
        /// 发件邮箱
        /// </summary>
        public static string SendMailAddress = "";
        /// <summary>
        /// 邮件服务器地址
        /// </summary>
        public static string SendMailPassword = "";
        /// <summary>
        /// 邮件主题
        /// </summary>
        public static string MailSubject = "";
        /// <summary>
        /// 异常邮件列表
        /// </summary>
        public static string ErrorReceiveMailAddress = "";
    }
}
