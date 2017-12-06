using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net_tm.config", Watch = true)]
namespace Alog.Common.LogModel
{
    public static class TmLogHelper
    {
        private static ILog log = LogManager.GetLogger("LogHelper");

        public static void WriteLog(string msg)
        {
            log.Info(msg);
        }

        /// <summary>
        /// 记录登陆日志express_login
        /// </summary>
        /// <param name="operation_time">操作时间</param>
        /// <param name="company">公司名称</param>
        /// <param name="station_id">站点id</param>
        /// <param name="station">站点名称</param>
        /// <param name="province">站点所在省份</param>
        /// <param name="city">站点所在城市</param>
        /// <param name="user_id">操作员账号ID</param>
        /// <param name="user">操作员账号名</param>
        /// <param name="source_type">发起请求的终端应用类型:web/Client/ios/android/database</param>
        /// <param name="source_application">发起请求的应</param>
        /// <param name="source_public_ip">登陆操作源公网IP</param>
        /// <param name="source_private_ip">登陆操作源内网IP</param>
        /// <param name="source_vpn_ip">登陆操作源VPN IP</param>
        /// <param name="source_mac">源MAC地址</param>
        /// <param name="target_application">登录的目标应用</param>
        /// <param name="target_public_ip">登陆目标端公网IP</param>
        /// <param name="target_private_ip">登陆目标端内网IP</param>
        /// <param name="operation_type">操作类型:login/logout</param>
        /// <param name="result">操作结果:success/fail</param>
        /// <param name="reason">结果原因</param>
        /// <param name="feature">预留拓展字段：格式：(key1:value1;key2:value2)，
        /// 用括号来包括内容，具体内容以key-value的形式，用英文分号连接</param>
        public static void WriteExpressLogin(DateTime operation_time, string company, string station_id, string station,
            string province, string city, string user_id, string user, string source_type, string source_application,
            string source_public_ip, string source_private_ip, string source_vpn_ip, string source_mac,
            string target_application, string target_public_ip, string target_private_ip, string operation_type,
            string result, string reason, string feature
            )
        {
            StringBuilder postBuilder = new StringBuilder();
            postBuilder.Append("method=express_login,time=")
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append(",operation_time=").Append(operation_time.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append(",company=").Append(company.GetText())
                .Append(",station_id=").Append(station_id.GetText())
                .Append(",station=").Append(station.GetText())
                .Append(",province=").Append(province.GetText())
                .Append(",city=").Append(city.GetText())
                .Append(",user_id=").Append(user_id.GetText())
                .Append(",user=").Append(user.GetText())
                .Append(",source_type=").Append(source_type.GetText())
                .Append(",source_application=").Append(source_application.GetText())
                .Append(",source_public_ip=").Append(source_public_ip.GetText())
                .Append(",source_private_ip=").Append(source_private_ip.GetText())
                .Append(",source_vpn_ip=").Append(source_vpn_ip.GetText())
                .Append(",source_mac=").Append(source_mac.GetText())
                .Append(",target_application=").Append(target_application.GetText())
                .Append(",target_public_ip=").Append(target_public_ip.GetText())
                .Append(",target_private_ip=").Append(target_private_ip.GetText())
                .Append(",operation_type=").Append(operation_type.GetText())
                .Append(",result=").Append(result.GetText())
                .Append(",reason=").Append(reason.GetText())
                .Append(",feature").Append(feature.FeatureText());

            WriteLog(postBuilder.ToString());
        }

        /// <summary>
        /// 记录access日志，记录用户访问过程中的网络活动; 对于一次访问，SLB、中间件和应用服务器里分别有一条日志，都要收进来。
        /// </summary>
        /// <param name="application_name">所属应用系统名称</param>
        /// <param name="client_ip">客户端ip</param>
        /// <param name="client_port">客户端的端口</param>
        /// <param name="target_ip">目标ip</param>
        /// <param name="instance_ip">目标实例ip</param>
        /// <param name="instance_port">目标实例的端口</param>
        /// <param name="read_time">请求时间点</param>
        /// <param name="timezone">时区</param>
        /// <param name="process_time">请求响应时长:微秒</param>
        /// <param name="protocol">请求协议:http或https</param>
        /// <param name="http_method">请求方式:GET/POST/PUT/DELETE</param>
        /// <param name="uri">请求的URI地址</param>
        /// <param name="status">返回状态</param>
        /// <param name="receive_size">返回页面的大小,单位：byte</param>
        /// <param name="send_size">请求数据包大小,单位：byte</param>
        /// <param name="referrer">访问的来源链接</param>
        /// <param name="user_agent">操作系统（包括版本号）浏览器（包括版本号）和用户个人偏好</param>
        /// <param name="feature">预留拓展字段：格式：(key1:value1;key2:value2)，
        /// 用括号来包括内容，具体内容以key-value的形式，用英文分号连接</param>
        public static void WriteExpressAccessLog(string application_name, string client_ip, string client_port,
            string target_ip, string instance_ip, string instance_port, DateTime read_time, string timezone,
            string process_time, string protocol, string http_method, string uri,string status, int receive_size,
            int send_size, string referrer, string user_agent, string feature)
        {
            StringBuilder postBuilder = new StringBuilder();
            postBuilder.Append("method=express_accesslog,time=").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append(",application_name=").Append(application_name.GetText())
                .Append(",client_ip=").Append(client_ip.GetText())
                .Append(",client_port=").Append(client_port.GetText())
                .Append(",target_ip=").Append(target_ip.GetText())
                .Append(",instance_ip=").Append(instance_ip.GetText())
                .Append(",instance_port=").Append(instance_port.GetText())
                .Append(",read_time=").Append(read_time.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append(",timezone=").Append(timezone.GetText())
                .Append(",process_time=").Append(process_time.GetText())
                .Append(",protocol=").Append(protocol.GetText())
                .Append(",http_method=").Append(http_method.GetText())
                .Append(",uri=").Append(uri.GetText())
                .Append(",status=").Append(status.GetText())
                .Append(",receive_size=").Append(receive_size)
                .Append(",send_size=").Append(send_size)
                .Append(",referrer=").Append(referrer.GetText())
                .Append(",user_agent=").Append(user_agent.GetText())
                .Append(",user_agent=").Append(user_agent.FeatureText());

            WriteLog(postBuilder.ToString());
        }

        /// <summary>
        /// 记录查询日志
        /// </summary>
        /// <param name="operation_time">操作时间</param>
        /// <param name="company">公司名称</param>
        /// <param name="station_id">站点id</param>
        /// <param name="station">站点名称</param>
        /// <param name="province">站点所在省份</param>
        /// <param name="city">站点所在城市</param>
        /// <param name="user_id">操作员账号ID</param>
        /// <param name="user">操作员账号名</param>
        /// <param name="source_type">发起请求的终端应用类型:web/client/ios/android/database/interface</param>
        /// <param name="source_application">发起请求的应用</param>
        /// <param name="source_public_ip">查询操作源公网IP</param>
        /// <param name="source_private_ip">查询操作源内网IP</param>
        /// <param name="source_vpn_ip">查询操作源VPN IP</param>
        /// <param name="source_mac">源MAC地址</param>
        /// <param name="target_application">请求的目标应用</param>
        /// <param name="target_public_ip">查询目标端公网IP</param>
        /// <param name="target_private_ip">查询目标端内网IP</param>
        /// <param name="operation_type">操作类型（查询/导出）</param>
        /// <param name="result">操作结果:success/fail</param>
        /// <param name="reason">结果原因</param>
        /// <summary name="content">
        /// 查询\导出的信息内容
        /// 包含信息内容的标识，根据实际查询内容选择下面列出的英文标识进行组合，用英文分隔符进行分割：
        /// 运单号－mail_no ,物流订单号－order_id ,仓库号－wms_id ,收件人手机号－receiver_phone, 收件人姓名－receiver_name
        /// 收件详细地址－receiver_address , 收件人支付宝账号－alipay_no , 收件人邮箱－receiver_email , 收件人身份证号－identification_no
        /// 收件人护照号－passport_no , 收件人身份证照片—identification_photo , 收件人护照照片－passport_photo , 底单照片—mail_photo
        /// 单号绑定省－mail_province , 单号绑定市－mail_city , 如： receiver_phone;receiver_name;
        /// </summary>
        /// <param name="mail_no">运单号</param>
        /// <param name="order_id">物流订单号（LP号）</param>
        /// <param name="wms_id">仓库管理单号（如LBX号）</param>
        /// <param name="association_number">本次查询涉及单号数目</param>
        /// <param name="email">如果涉及对外发送客户信息，需要记录email地址</param>
        /// <param name="feature">预留拓展字段：格式：(key1:value1;key2:value2)，
        /// 用括号来包括内容，具体内容以key-value的形式，用英文分号连接</param>
        public static void WriteExpressQuery(DateTime operation_time, string company, string station_id, string station,
            string province, string city, string user_id, string user, string source_type, string source_application,
            string source_public_ip, string source_private_ip, string source_vpn_ip, string source_mac, 
            string target_application, string target_public_ip, string target_private_ip, string operation_type,
            string result, string reason, string content, string mail_no, string order_id, string wms_id, 
            string association_number, string email, string feature)
        {
            StringBuilder postBuilder = new StringBuilder();
            postBuilder.Append("method=express_query,time=").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append(",operation_time=").Append(operation_time.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append(",company=").Append(company.GetText())
                .Append(",station_id=").Append(station_id.GetText())
                .Append(",station=").Append(station.GetText())
                .Append(",province=").Append(province.GetText())
                .Append(",city=").Append(city.GetText())
                .Append(",user_id=").Append(user_id.GetText())
                .Append(",user=").Append(user.GetText())
                .Append(",source_type=").Append(source_type.GetText())
                .Append(",source_application=").Append(source_application.GetText())
                .Append(",source_public_ip=").Append(source_public_ip.GetText())
                .Append(",source_private_ip=").Append(source_private_ip.GetText())
                .Append(",source_vpn_ip=").Append(source_vpn_ip.GetText())
                .Append(",source_mac=").Append(source_mac.GetText())
                .Append(",target_application=").Append(target_application.GetText())
                .Append(",target_public_ip=").Append(target_public_ip.GetText())
                .Append(",target_private_ip=").Append(target_private_ip.GetText())
                .Append(",operation_type=").Append(operation_type.GetText())
                .Append(",result=").Append(result.GetText())
                .Append(",reason=").Append(reason.GetText())
                .Append(",content=").Append(content.GetText())
                .Append(",mail_no=").Append(mail_no.GetText())
                .Append(",order_id=").Append(order_id.GetText())
                .Append(",wms_id=").Append(wms_id.GetText())
                .Append(",association_number=").Append(association_number.GetText())
                .Append(",email=").Append(email.GetText())
                .Append(",feature=").Append(feature.FeatureText());

            WriteLog(postBuilder.ToString());
        }

        private static string FeatureText(this string featureStr)
        {
            if (featureStr == null)
                return "()";

            if (!featureStr.StartsWith("("))
                featureStr = "(" + featureStr;
            if (!featureStr.EndsWith(")"))
                featureStr += ")";

            return featureStr.Replace(',', ';');

        }

        private static string GetText(this string tetxt)
        {
            return tetxt ?? "";
        }
    }
}
