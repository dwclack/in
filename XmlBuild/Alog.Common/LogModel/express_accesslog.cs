using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alog.Common.LogModel
{
    /// <summary>
    /// 开启access日志，记录用户访问过程中的网络活动; 对于一次访问，SLB、中间件和应用服务器里分别有一条日志，都要收进来。
    /// </summary>
    public class express_accesslog
    {
        /// <summary>
        /// 应用标识 = appkey
        /// </summary>
        public string appId { get; set; }

        public string appKey { get; set; }
        /// <summary>
        /// 日志类型（标识） = "express_accesslog"
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// 上传日志的时间
        /// </summary>
        public DateTime time { get; set; }
        /// <summary>
        /// 所属应用系统名称
        /// </summary>
        public string application_name { get; set; }
        /// <summary>
        /// 客户端ip
        /// </summary>
        public string client_ip { get; set; }
        /// <summary>
        /// 客户端的端口
        /// </summary>
        public string client_port { get; set; }
        /// <summary>
        /// 目标ip
        /// </summary>
        public string target_ip { get; set; }
        /// <summary>
        /// 目标实例ip
        /// </summary>
        public string instance_ip { get; set; }
        /// <summary>
        /// 目标实例的端口
        /// </summary>
        public string instance_port { get; set; }
        /// <summary>
        /// 请求时间点
        /// </summary>
        public DateTime read_time { get; set; }
        /// <summary>
        /// 时区
        /// </summary>
        public string timezone { get; set; }
        /// <summary>
        /// 请求响应时长:微秒
        /// </summary>
        public double process_time { get; set; }
        /// <summary>
        /// 请求协议
        /// </summary>
        public string protocol { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string http_method { get; set; }
        /// <summary>
        /// 请求的URI地址
        /// </summary>
        public string uri { get; set; }
        /// <summary>
        /// 返回状态
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 返回页面的大小,单位：byte
        /// </summary>
        public int receive_size { get; set; }
        /// <summary>
        /// 请求数据包大小,单位：byte
        /// </summary>
        public int send_size { get; set; }
        /// <summary>
        /// 访问的来源链接
        /// </summary>
        public string referrer { get; set; }
        /// <summary>
        /// 操作系统（包括版本号）浏览器（包括版本号）和用户个人偏好
        /// </summary>
        public string user_agent { get; set; }
        /// <summary>
        /// 预留拓展字段
        /// </summary>
        public string feature { get; set; }

    }
}
