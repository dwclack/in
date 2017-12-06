using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alog.Common.LogModel
{
    /// <summary>
    /// 登陆日志
    /// </summary>
    public class express_login
    {
        /// <summary>
        /// 应用标识
        /// </summary>
        public string appId { get; set; }
        /// <summary>
        /// appKey值与appId值一样
        /// </summary>
        public string appKey { get; set; }
        /// <summary>
        /// 日志类型 = "express_login"
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// 上传日志的时间
        /// </summary>
        public DateTime time { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime operation_time { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string company { get; set; }
        /// <summary>
        /// 站点id
        /// </summary>
        public string station_id { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string station { get; set; }
        /// <summary>
        /// 站点所在省份
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 站点所在城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 操作员账号ID
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// 操作员账号名
        /// </summary>
        public string user { get; set; }
        /// <summary>
        /// 发起请求的终端应用类型
        /// </summary>
        public string source_type { get; set; }
        /// <summary>
        /// 发起请求的应用
        /// </summary>
        public string source_application { get; set; }
        /// <summary>
        /// 登陆操作源公网IP
        /// </summary>
        public string source_public_ip { get; set; }
        /// <summary>
        /// 登陆操作源内网IP
        /// </summary>
        public string source_private_ip { get; set; }
        /// <summary>
        /// 登陆操作源VPN IP
        /// </summary>
        public string source_vpn_ip { get; set; }
        /// <summary>
        /// 源MAC地址
        /// </summary>
        public string source_mac { get; set; }
        /// <summary>
        /// 登录的目标应用
        /// </summary>
        public string target_application { get; set; }
        /// <summary>
        /// 登陆目标端公网IP
        /// </summary>
        public string target_public_ip { get; set; }
        /// <summary>
        /// 登陆目标端内网IP
        /// </summary>
        public string target_private_ip { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public string operation_type { get; set; }
        /// <summary>
        /// 操作结果
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// 结果原因
        /// </summary>
        public string reason { get; set; }
        /// <summary>
        /// 预留拓展字段
        /// </summary>
        public string feature { get; set; }
    }
}
